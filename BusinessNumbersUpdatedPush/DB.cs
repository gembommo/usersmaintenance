using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessNumbersUpdatedPush
{
    class DB
    {
        public static List<string> GetNumbers()
        {
            using (mystateApiDbEntities db = new mystateApiDbEntities())
            {

                var states = (from s in db.vw_user
                              where s.devicetype != "Apple"
                              && !s.isuninstalled
                              && s.appversioncode>=141
                              select s).Select(s=>s.gcmregistrationid).ToList();
                //List<string> phones = states.Select(s => s.phonenumber.Trim(' ')).ToList();

                return states;

            }
        }
    }
}
