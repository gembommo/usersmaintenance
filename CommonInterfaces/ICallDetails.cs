using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonInterfaces
{
    public interface ICallDetails
    {
        string PhoneNumber { get; set; }
        string BusinessPhoneNumber { get; set; }
    }
}
