using System.Collections.Generic;
using System.Data;
using CommonInterfaces;

namespace MyState.WebApplication.DataStore.UserDefinedSqlObjects
{
    internal sealed class DbPassedTempContactDetails
    {
        private readonly DataColumn[] _stringArrayScheme = {
            new DataColumn("PhoneNumber", typeof (string)),
            new DataColumn("SourcePhoneNumber", typeof (string)),
            new DataColumn("Name", typeof (string)),
        };
        
        public bool DissallowNullValues { get; set; }
        private HashSet<string> _innerHashSet;
        public DataTable ObjAdjustedToDb { get; private set; }
        public bool IsEmpty {
            get
            {
                return ObjAdjustedToDb == null || ObjAdjustedToDb.Columns.Count == 0 || ObjAdjustedToDb.Rows.Count == 0;
            }
        }

        public DbPassedTempContactDetails()
        {
            ObjAdjustedToDb = new DataTable();
            _innerHashSet = new HashSet<string>();
            ObjAdjustedToDb.Columns.AddRange(_stringArrayScheme);
            DissallowNullValues = true;
        }

        public DbPassedTempContactDetails(IEnumerable<IContactDetails> source)
            : this()
        {
            AddRange(source);
        }

        public void Add(IContactDetails value)
        {
            if (DissallowNullValues && value == null) return;
            if (_innerHashSet.Contains(value.PhoneNumber))
                return;

            _innerHashSet.Add(value.PhoneNumber);
            ObjAdjustedToDb.Rows.Add(value.PhoneNumber, value.SourcePhoneNumber, value.Name);
        }

        public void AddRange(IEnumerable<IContactDetails> source)
        {
            if (source == null) return;
            ObjAdjustedToDb.BeginLoadData();
            foreach (var value in source)
            {
                Add(value);
            }
            ObjAdjustedToDb.EndLoadData();
        }

        private void Clear()
        {
            ObjAdjustedToDb.Clear();
        }
    }
}
