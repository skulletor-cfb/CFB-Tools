using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace CFB27.Data.Model
{
    [JsonObject]
    public class CFBTable<T> where T : BaseRecord
    {
        [JsonProperty("header")]
        public CFBTableHeader Header { get; set; }

        [JsonProperty("fields")]
        public string[] Fields { get; set; }

        [JsonProperty("records")]
        public List<T> Records { get; set; }
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