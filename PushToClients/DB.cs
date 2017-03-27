using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToClients
{
    public static class  DB
    {
        public static List<vw_user> GetIsraelisWithOldVersionForPush()
        {
            using (var db = new mystateApiDbEntities())
            {
                var res = db.vw_user.SqlQuery(@"select * from vw_user 
            where phonenumber like '972%'
            and isuninstalled = 0
            and (devicetype is null or devicetype= 'Android')
            and statetext  LIKE '%[א-ת]%'
            and appversioncode >= 130 and appversioncode < 149").ToList();
                return res;
            }
        }

        public static List<vw_user> GetIsraelisWithNewVersionForPush()
        {
            using (var db = new mystateApiDbEntities())
            {
                var res = db.vw_user.SqlQuery(@"select * from vw_user 
            where phonenumber like '972%'
            and isuninstalled = 0
            and (devicetype is null or devicetype= 'Android')
            and statetext  LIKE '%[א-ת]%'
            and appversioncode >= 149").ToList();
                return res;
            }
        }
    }

    public class PhoneDetails
    {
        public string PhoneNumber { get; set; }
        public string DeviceType { get; set; }
        public string GCM { get; set; }

    }
}
