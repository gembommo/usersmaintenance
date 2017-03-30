using System.Collections.Generic;
using System.Data;
using MyState.WebApplication.DataStore.DataObjects;

namespace MyState.WebApplication.DataStore.UserDefinedSqlObjects
{
    internal sealed class DbPassedMarketingObjectArray
    {
        private readonly DataColumn[] _stringArrayScheme = {
            new DataColumn("PhoneNumber", typeof (string)),
            new DataColumn("DisplayName", typeof (string)),
            new DataColumn("TimesContacted", typeof (int)),
            new DataColumn("Starred", typeof (bool)),
            new DataColumn("Favorite", typeof (bool)),
            new DataColumn("Recents", typeof (bool)),
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

        public DbPassedMarketingObjectArray()
        {
            ObjAdjustedToDb = new DataTable();
            _innerHashSet = new HashSet<string>();
            ObjAdjustedToDb.Columns.AddRange(_stringArrayScheme);
            DissallowNullValues = true;
        }

        public DbPassedMarketingObjectArray(IEnumerable<NonMyStateContactsForMarketing> source)
            : this()
        {
            AddRange(source);
        }

        public void Add(NonMyStateContactsForMarketing value)
        {
            if (DissallowNullValues && value == null) return;
            if (_innerHashSet.Contains(value.PhoneNumber))
                return;

            _innerHashSet.Add(value.PhoneNumber);
            ObjAdjustedToDb.Rows.Add(value.PhoneNumber, value.DisplayName, value.TimesContacted, value.Starred, value.Favorite, value.Recents);
        }

        public void AddRange(IEnumerable<NonMyStateContactsForMarketing> source)
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
