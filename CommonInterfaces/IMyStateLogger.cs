using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonInterfaces
{
    public interface IMyStateLogger
    {
        void Write(Log log);
        void Write(Exception ex);
    }
}
