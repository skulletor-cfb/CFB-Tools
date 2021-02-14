using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class FieldList : List<Field>
    {
        private Dictionary<string, Field> fieldNameLookup = new Dictionary<string, Field>();
        private Dictionary<string, Field> fieldAbbreviationLookup = new Dictionary<string, Field>();

        new public void Add(Field f)
        {
            var nameKey = string.IsNullOrEmpty(f.name) ? f.Name : f.name;
            nameKey = string.IsNullOrEmpty(nameKey) ? f.Abbreviation : nameKey;
            var abbvKey = string.IsNullOrEmpty(f.Abbreviation) ? nameKey : f.Abbreviation;
            this.fieldNameLookup[nameKey] = f;
            this.fieldAbbreviationLookup[abbvKey] = f;
            base.Add(f);
        }

        public  Field FindField( string Name)
        {
            Field f = this.GetFieldByName(Name);
            if (f != null)
                return f;
            return this.GetFieldByAbbreviation(Name);
        }

        public Field GetField(string name)
        {
            Field f = null;
            this.fieldNameLookup.TryGetValue(name, out f);
            return f;
        }

        public Field GetFieldByName(string name)
        {
            return this.GetField(name);
        }
        public Field GetFieldByAbbreviation(string name)
        {
            Field f = null;
            if (!this.fieldAbbreviationLookup.TryGetValue(name, out f))
            {
                f = this.GetFieldByName(name);
            }

            return f;
        }
    }
}