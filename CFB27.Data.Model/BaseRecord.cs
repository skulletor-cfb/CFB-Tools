using MaddenFranchise;
using MaddenFranchise.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
namespace CFB27.Data.Model
{
    public static class SaveFileExtensions
    {
        public static T DefaultTransform<T>(this FranchiseFile dynasty, FranchiseFileRecord record) where T : BaseRecord
        {
            var result = record.Transform<T>();
            result.Row = record.Index;
            result.IsEmpty = record.IsEmpty;
            return result;
        }

        public static FranchiseFileRecord Follow(this FranchiseFile dynasty, RecordReference reference)
        {
            if (reference.TableId <= 0)
            {
                return null;
            }

            var table = dynasty.GetTableById(reference.TableId);
            table.ReadRecords();
            return reference.RowNumber < table.Records.Count ? table.Records[reference.RowNumber] : null;
        }

        public static CFBTable<T> GetTable<T>(this FranchiseFile dynasty, TableName name, Func<FranchiseFile, FranchiseFileRecord, T> transform = null) where T : BaseRecord
        {
            if (transform == null)
            {
                transform = DefaultTransform<T>;
            }

            var table = dynasty.GetPrimaryTable(name);
            var records = table.Records.Select(r => transform(dynasty, r)).ToList();
            return new CFBTable<T>
            {
                Records = records,
                RawTable = table
            };
        }
    }

    [JsonObject]
    public class CFBTable<T> where T : BaseRecord
    {
        [JsonProperty("header")]
        public CFBTableHeader Header { get; set; }

        [JsonProperty("fields")]
        public string[] Fields { get; set; }

        [JsonProperty("records")]
        public List<T> Records { get; set; }

        [JsonIgnore]
        public FranchiseFileTable RawTable { get; set; }
    }

    [JsonObject]
    public abstract class BaseRecord
    {
        public const string NoRefString = "00000000000000000000000000000000";
        private const string RowKey = "_row";
        private const string IsEmptyKey = "_isEmpty";

        [JsonProperty(RowKey)]
        public int Row { get; set; }

        [JsonProperty(IsEmptyKey)]
        public bool IsEmpty { get; set; }
    }
    public class CFBTableHeader
    {
        public string name { get; set; }
        public bool isArray { get; set; }
        public int tableId { get; set; }
        public long tablePad1 { get; set; }
        public long uniqueId { get; set; }
        public int tableUnknown1 { get; set; }
        public int tableUnknown2 { get; set; }
        public string data1Id { get; set; }
        public int data1Type { get; set; }
        public int data1Unknown1 { get; set; }
        public int data1Flag1 { get; set; }
        public int data1Flag2 { get; set; }
        public int data1Flag3 { get; set; }
        public int data1Flag4 { get; set; }
        public int tableStoreLength { get; set; }
        public object tableStoreName { get; set; }
        public int data1Offset { get; set; }
        public int data1TableId { get; set; }
        public int data1RecordCount { get; set; }
        public int data1Pad2 { get; set; }
        public int table1Length { get; set; }
        public int table2Length { get; set; }
        public int data1Pad3 { get; set; }
        public long data1Pad4 { get; set; }
        public int headerSize { get; set; }
        public int headerOffset { get; set; }
        public int record1SizeOffset { get; set; }
        public int record1SizeLength { get; set; }
        public int record1Size { get; set; }
        public int offsetStart { get; set; }
        public string data2Id { get; set; }
        public int table1Length2 { get; set; }
        public int tableTotalLength { get; set; }
        public bool hasSecondTable { get; set; }
        public int table1StartIndex { get; set; }
        public int table2StartIndex { get; set; }
        public int recordWords { get; set; }
        public int recordCapacity { get; set; }
        public int numMembers { get; set; }
        public int nextRecordToUse { get; set; }
        public bool hasThirdTable { get; set; }
        public int table3Length { get; set; }
        public int table3StartIndex { get; set; }
    }
}