using System;
using System.Collections.Generic;
using System.Text;

namespace CFB27.FileReader
{
    public enum FileType
    {
        Unknown,
        Dynasty,
        Roster,
    }
    public sealed class AssetTable
    {
        public uint AssetId { get; set; }
        public uint Reference { get; set; }
    }

    public sealed class RecordReference
    {
        public int TableId { get; set; }
        public int RowNumber { get; set; }
    }

    public sealed class SaveFileType
    {
        public bool Compressed { get; set; }

        public FileType FileType{ get; set; }
    }

    public sealed class SchemaMetadata
    {
        public int? GameYear { get; set; }
        public int? Major { get; set; }
        public int? Minor { get; set; }
        public string? Path { get; set; }
    }
}
