using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class EntryList : List<DBData>
    {
        private Dictionary<string, DBData> entryMap = new Dictionary<string, DBData>();

        new public void Add(DBData data)
        {
            entryMap[data.GetFieldName()] = data;
            base.Add(data);
        }

        public DBData GetEntry(string fieldName)
        {
            DBData data = null;
            this.entryMap.TryGetValue(fieldName, out data);
            return data;
        }
    }
}
