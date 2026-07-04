using Microsoft.VisualBasic.FileIO;
using System.IO.Compression;

namespace CFB27.FileReader
{
    public class DynastyFile
    {
        private const int CompressedDataOffset = 0x52;

        private readonly byte[] _rawContents;
        private readonly int? _gameYear;
        private readonly SchemaMetadata? _expectedSchemaVersion;

        public bool IsLoaded { get; private set; }

        public string FilePath { get; private set; }

        public byte[] UnpackedFileContents { get; private set; }

        public byte[]? PackedFileContents { get; private set; }

        public SaveFileType FileDetails { get; }

        public List<FranchiseFileTable> Tables { get; } = new();

        public List<AssetTable> AssetTable { get; } = new();

        public event EventHandler? Ready;
        public event EventHandler<Exception>? Error;
        public event EventHandler? Saving;
        public event EventHandler? Saved;

        public DynastyFile(string filePath)
        {
            FilePath = filePath;

            _rawContents = File.ReadAllBytes(filePath);
            FileDetails = GetFileType(_rawContents);

            if (FileDetails.Compressed)
            {
                PackedFileContents = _rawContents;
                UnpackedFileContents = UnpackFile(_rawContents, FileDetails);
            }
            else
            {
                UnpackedFileContents = _rawContents;
            }
        }

        public async Task ParseAsync()
        {
            try
            {
                await LoadAssetTableAsync();
                await LoadTablesAsync();

                IsLoaded = true;
                Ready?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                throw;
            }
        }

        public async Task SaveAsync(string? outputPath = null)
        {
            Saving?.Invoke(this, EventArgs.Empty);

            string destination = outputPath ?? FilePath;

            byte[] packed = await PackFileAsync(UnpackedFileContents);

            await File.WriteAllBytesAsync(destination, packed);

            Saved?.Invoke(this, EventArgs.Empty);
        }

        private async Task LoadTablesAsync()
        {
#if false
#else
            await Task.Run(() =>
            {
                byte[] spbf = { 0x53, 0x50, 0x42, 0x46 };
                byte[] asto = { 0x41, 0x53, 0x54, 0x4F };
                byte[] spex = { 0x53, 0x50, 0x45, 0x58 };

                var tableIndices = new List<int>();

                for (int i = 0; i < UnpackedFileContents.Length - 4; i++)
                {
                    if (Matches(i, spbf)
                        || Matches(i, asto)
                        || Matches(i, spex))
                    {
                        int start =
                            i - GetTableStartOffsetByGameYear(_gameYear ?? 20);

                        tableIndices.Add(start);
                    }
                }

                for (int i = 0; i < tableIndices.Count; i++)
                {
                    int current = tableIndices[i];
                    int next = i < tableIndices.Count - 1
                        ? tableIndices[i + 1]
                        : UnpackedFileContents.Length - 8;

                    byte[] tableData =
                        UnpackedFileContents[current..next];

                    var table = new FranchiseFileTable(tableData);

                    Tables.Add(table);
                }
            });

            bool Matches(int offset, byte[] pattern)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (UnpackedFileContents[offset + i] != pattern[i])
                        return false;
                }

                return true;
            }
#endif
        }

        private async Task LoadAssetTableAsync()
        {
            await Task.Run(() =>
            {
                uint offset =
                    ReadUInt32BE(UnpackedFileContents, 4);

                uint count =
                    ReadUInt32BE(UnpackedFileContents, 36);

                int currentOffset = (int)offset;

                for (int i = 0; i < count; i++)
                {
                    uint assetId =
                        ReadUInt32BE(UnpackedFileContents, currentOffset);

                    uint reference =
                        ReadUInt32BE(UnpackedFileContents,
                                     currentOffset + 4);

                    AssetTable.Add(new AssetTable
                    {
                        AssetId = assetId,
                        Reference = reference
                    });

                    currentOffset += 8;
                }
            });
        }

        private static byte[] UnpackFile(byte[] data, SaveFileType type)
        {
            int offset =
                type.FileType == FileType.Dynasty
                    ? CompressedDataOffset
                    : 0;

            using var input =
                new MemoryStream(data, offset, data.Length - offset);

            using var inflater =
                new ZLibStream(input, CompressionMode.Decompress);

            using var output = new MemoryStream();

            inflater.CopyTo(output);

            return output.ToArray();
        }

        private static async Task<byte[]> PackFileAsync(byte[] data)
        {
            using var output = new MemoryStream();

            await using (var compressor =
                new ZLibStream(output, CompressionLevel.Optimal, true))
            {
                await compressor.WriteAsync(data);
            }

            return output.ToArray();
        }

        
        public FranchiseFileTable? GetTableByName(string name)
            => Tables.FirstOrDefault(t => t.Name == name);

        public IEnumerable<FranchiseFileTable> GetAllTablesByName(string name)
            => Tables.Where(t => t.Name == name);

        public FranchiseFileTable? GetTableById(int id)
            => Tables.FirstOrDefault(
                t => t.Header?.TableId == id);

        public FranchiseFileTable? GetTableByUniqueId(int id)
            => Tables.FirstOrDefault(
                t => t.Header?.UniqueId == id);

        private static int GetTableStartOffsetByGameYear(int gameYear)
            => gameYear switch
            {
                19 => 0x90,
                _ => 0x94
            };

        private static bool IsCompressed(byte[] data)
        {
            return !(data[0] == 0x46 &&
                     data[1] == 0x72 &&
                     data[2] == 0x54 &&
                     data[3] == 0x6B);
        }

        private static uint ReadUInt32BE(byte[] buffer, int offset)
        {
            return ((uint)buffer[offset] << 24)
                 | ((uint)buffer[offset + 1] << 16)
                 | ((uint)buffer[offset + 2] << 8)
                 | buffer[offset + 3];
        }

        private static SaveFileType GetFileType(byte[] data)
        {
            return new SaveFileType
            {
                Compressed = IsCompressed(data),
                FileType = FileType.Dynasty,
            };
        }
    }
}
