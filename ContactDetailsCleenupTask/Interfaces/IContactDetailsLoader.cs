using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonInterfaces;

namespace ContactDetailsCleenupTask.Interfaces
{
    public interface IContactDetailsLoader
    {
        void ForEach(int batchCount, int dellayInMilliSeconds, Func<List<IContactDetails>, bool>[] operations);
    }
}
