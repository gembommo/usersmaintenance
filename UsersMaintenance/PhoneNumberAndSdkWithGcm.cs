using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersMaintenance
{
    public class PhoneNumberAndSdkWithGcm
    {
        public string PhoneNumber { get; set; }
        public int CompanyId { get; set; }
        public string GcmRegistrationId { get; set; }

        public DateTime? LastUpdated { get; set; }
        public string SkdKey { get; set; }

        public PhoneNumberAndSdkWithGcm()
        {

        }
        public PhoneNumberAndSdkWithGcm(string phoneNumber, string gcmRegistrationId, DateTime? lastUpdated)
        {
            PhoneNumber = phoneNumber;
            GcmRegistrationId = gcmRegistrationId;
            LastUpdated = lastUpdated;
        }
    }
}
