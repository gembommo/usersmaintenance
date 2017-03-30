using System.Collections.Generic;

namespace MyState.WebApplication.DataStore.UserDefinedSqlObjects
{
    internal class DbPassedDistinctArray<T> : DbPassedArray<T>
    {
        private HashSet<T> _innerHashSet;

        
        public DbPassedDistinctArray()
        {
            _innerHashSet = new HashSet<T>();
        }

        public override void Add(T value)
        {
            if (value == null) return;
            _innerHashSet.Add(value);
        }

        public override void AddRange(IEnumerable<T> source)
        {
            _innerHashSet = new HashSet<T>(source);
            base.AddRange(_innerHashSet);
        }

    }
}
