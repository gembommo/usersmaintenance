using System.Collections.Generic;
using System.Data;

namespace MyState.WebApplication.DataStore.UserDefinedSqlObjects
{
    internal class DbPassedArray<T>
    {
        private readonly DataColumn[] _stringArrayScheme = {
            new DataColumn("Value", typeof (T))
        };


        public bool DissallowNullValues { get; set; }
        public bool IsEmpty
        {
            get
            {
                return _objAdjustedToDb == null || _objAdjustedToDb.Columns.Count == 0 || _objAdjustedToDb.Rows.Count == 0;
            }
        }

        private DataTable _objAdjustedToDb;
        public virtual DataTable ObjAdjustedToDb
        {
            get { return _objAdjustedToDb; }
            protected set { _objAdjustedToDb = value; }
        }

        public DbPassedArray()
        {
            _objAdjustedToDb = new DataTable();
            _objAdjustedToDb.Columns.AddRange(_stringArrayScheme);
            DissallowNullValues = true;
        }

        public virtual void Add(T value)
        {
            if (value == null) return;
            _objAdjustedToDb.Rows.Add(value);
        }

        public virtual void AddRange(IEnumerable<T> source)
        {
            if (source == null) return;
            _objAdjustedToDb.BeginLoadData();
            foreach (var value in source)
            {
                if (DissallowNullValues && value == null) continue;
                _objAdjustedToDb.Rows.Add(value);
            }
            _objAdjustedToDb.EndLoadData();
        }

        protected void Clear()
        {
            _objAdjustedToDb.Clear();
        }
    }
}
