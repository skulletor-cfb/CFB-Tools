namespace CFB27.FileReader
{
    public class FranchiseFileTable
    {
        public int Index { get; set; } = -1;

        public byte[] Data { get; set; }

        public int LengthAtLastSave { get; set; }

        public int Offset { get; set; }

        public GameStrategy StrategyBase { get; }

        public TableStrategy Strategy { get; }

        public bool RecordsRead { get; set; }

        private readonly int _gameYear;

        public FranchiseFileTableHeader Header { get; set; }

        public string Name { get; set; }

        public bool IsArray { get; set; }

        public List<OffsetTableEntry> LoadedOffsets { get; set; }
            = new();

        public bool IsChanged { get; set; }

        public List<FranchiseFileRecord> Records { get; set; }
            = new();

        public List<FranchiseFileTable2Field> Table2Records { get; set; }
            = new();

        public List<FranchiseFileTable3Field> Table3Records { get; set; }
            = new();

        public List<int> ArraySizes { get; set; }
            = new();

        public Dictionary<int, EmptyRecordEntry> EmptyRecords { get; set; }
            = new();

        private readonly FranchiseFileSettings _settings;

        private TableSchema _schema;

        public event EventHandler? Changed;

        public FranchiseFileTable(
            byte[] data,
            int offset,
            int gameYear,
            GameStrategy strategy,
            FranchiseFileSettings settings)
        {
            Data = data;
            LengthAtLastSave = data.Length;
            Offset = offset;

            StrategyBase = strategy;
            Strategy = strategy.Table;

            RecordsRead = false;
            _gameYear = gameYear;

            Header = Strategy.ParseHeader(data);

            Name = Header.Name;
            IsArray = Header.IsArray;

            _settings = settings;
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}