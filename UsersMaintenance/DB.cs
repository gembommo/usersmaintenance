using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace UsersMaintenance
{
    public static class DB
    {
        private const int NUMBERS_TO_TAKE = 200;

        public const string USER_HASH_PREFIX = "User:";
        public const string INTERESTED_USERS_LIST = "InterestedUsersList";
        public const string INTERESTED_ON_OTHER_FIELD_PREFIX = "InterestedOnOther:";
        public const string INTERESTED_ON_USER_FIELD_PREFIX = "InterestedOnUser:";
        public const string SERVER_PUSH_CODE_FIELD_PREFIX = "ServerPushCode:";
        private const char DELIMITER = '|';

        public static void SyncDbAndRedisContacts()
        {
            Console.WriteLine("Syncing Database and Redis contacts");

            using (mystateApiDbEntities1 db = new mystateApiDbEntities1())
            {
                var skip = 0;

                while (true)
                {
                    Console.WriteLine("Taking From {0} to {1}", skip, skip + NUMBERS_TO_TAKE);

                    var states = (from s in db.States
                                  join u in db.Users on s.PhoneNumber equals u.PhoneNumber
                                  join sp in db.ServerPushCodes on s.PhoneNumber equals sp.PhoneNumber
                                  where u.DeviceType != "Apple" && sp.IsInstalled
                                  select s).OrderBy(s => s.PhoneNumber).ToList().Skip(skip).Take(NUMBERS_TO_TAKE).ToList();
                    if (states.Count == 0)
                        break;
                    skip += NUMBERS_TO_TAKE;

                    IDatabase redisDb = Program.Connection.GetDatabase();

                    List<string> phones = states.Select(s => s.PhoneNumber.Trim(' ')).ToList();

                    List<RedisValue[]> list = GetMultipleHashElements(phones);

                    int i = 0;
                     List<string> listOfBadDates = new List<string>();
                    foreach (State state in states)
                    {
                       
                        var redisState = list[i];
                        if (redisState[0].HasValue)
                        {
                            state.LastUpdated = new DateTime((long)redisState[0]);
                            if (redisState[1].HasValue)
                            {
                                state.FullState = (int)redisState[1];
                            } 
                            if (redisState[2].HasValue)
                            {
                                state.StateText = redisState[2];
                            } 
                            if (redisState[3].HasValue && (long)redisState[3] != 0)
                            {
                                state.StateTextDate = new DateTime((long)redisState[3]);
                            }
                            if (redisState[4].HasValue && (long)redisState[4] != 0)
                            {
                                DateTime dt = new DateTime((long)redisState[4]);
                                if (dt > DateTime.Now.AddYears(-100))//hackforbug
                                {
                                    state.StateAvailabilityDate = dt;
                                }
                                else
                                {
                                    listOfBadDates.Add(state.PhoneNumber);
                                }
                            }
                            if (redisState[5].HasValue && (long)redisState[5] != 0)
                            {
                                state.ReconnectPushDate = new DateTime((long)redisState[5]);
                            }
                        }
                        i++;
                    }
                    if (listOfBadDates.Count > 0)
                        Console.WriteLine("Found {0} wrong StateAvailabilityDates", listOfBadDates.Count);
                    foreach (var item in listOfBadDates)
                    {
                        Console.WriteLine("PhoneNumber: {0} has wrong StateAvailabilityDates", item);
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException duex)
                    {
                        Console.WriteLine("Error updating database: {0} for items" +duex.Entries.Count() );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error:" + ex);
                    }

                }
            }
        }


        public static List<RedisValue[]> GetMultipleHashElements(List<string> phones)
        {
            var redisDb = Program.Connection.GetDatabase();
            List<Task<RedisValue[]>> hashes = new List<Task<RedisValue[]>>();

            foreach (string item in phones)
            {
                hashes.Add(redisDb.HashGetAsync(USER_HASH_PREFIX + item.TrimEnd(' '),
                            new RedisValue[] 
                        { 
                            "LastUpdated", 
                            "FullState",
                            "StateText",
                            "StateTextDate",
                            "StateAvailabilityDate",
                            "ReconnectPushDate"
                        }));
            }

            Task.WaitAll(hashes.ToArray());
            List<RedisValue[]> list = new List<RedisValue[]>();
            foreach (Task<RedisValue[]> task in hashes)
            {
                list.Add(task.Result);
            }
            return list;
        }

        public static List<PhoneNumberAndSdkWithGcm> GetStatesWhoDidntReportSince(DateTime dt)
        {
            Console.WriteLine("Getting states who didnt report for a week");
            using (var db = new mystateApiDbEntities1())
            {
                var phoneNumbersWithGcm = (from s in db.States
                                           join u in db.Users on s.PhoneNumber equals u.PhoneNumber
                                           join spc in db.ServerPushCodes on s.PhoneNumber equals spc.PhoneNumber
                                           join c in db.Companies on spc.CompanyId equals c.Id
                                           where u.DeviceType != "Apple"
                                           && s.LastUpdated < dt
                                           && spc.IsInstalled
                                           select new PhoneNumberAndSdkWithGcm
                                           {
                                               PhoneNumber = u.PhoneNumber,
                                               CompanyId = spc.CompanyId,
                                               SkdKey = c.SDKKey,
                                               GcmRegistrationId = spc.PushCode,
                                               LastUpdated = s.LastUpdated
                                           }).OrderBy(s => s.PhoneNumber).ToList();
                Console.WriteLine("Got {0} states who didnt report for a week", phoneNumbersWithGcm.Count);
                return phoneNumbersWithGcm;
            }
        }

        public static void SaveResultsInDB(List<PhoneNumberAndSdkWithGcm> toUninstall, List<string> toSaveReconnect)
        {

            //"toSaveReconnect": They didn't report for full day!so probably they wouldn't report now too
            //and even if they do - we can reload them from db
            //the other option is to keep them, but then we would need to save "reconnect" for all of them 
            // and even those who are not in redis now. this seems stupid
            //other option is to save only those who already are in redis which seems lots of work
            //we have a racecondition here btw because after the push was sent - maybe the device already updated the db...
            var redisConnection = Program.Connection;
            var redisDb = redisConnection.GetDatabase();
            using (mystateApiDbEntities1 db = new mystateApiDbEntities1())
            {
                if (toUninstall.Count > 0)
                {
                    List<ServerPushCode> uninstalledPushCodes = GetServerPushCodes(db, toUninstall);

                    foreach (ServerPushCode serverPushCode in uninstalledPushCodes)
                    {
                        serverPushCode.IsInstalled = false;
                        serverPushCode.UninstallationReason = "NotRegistered";
                        //string sdkKey =
                        //    toUninstall.Find(
                        //        x => x.PhoneNumber == serverPushCode.PhoneNumber && x.CompanyId == serverPushCode.CompanyId)
                        //        .SkdKey;
                        //redisDb.HashDelete(USER_HASH_PREFIX + serverPushCode.PhoneNumber.TrimEnd(' '),
                        //    SERVER_PUSH_CODE_FIELD_PREFIX + sdkKey, CommandFlags.FireAndForget);
                        redisDb.KeyDelete(USER_HASH_PREFIX + serverPushCode.PhoneNumber.TrimEnd(' '), CommandFlags.FireAndForget);

                    }
                }

                //foreach (ServerPushCode uninstalledPushCode in uninstalledPushCodes)
                //{
                //    string sdkKey =
                //        toUninstall.Find(
                //            x => x.PhoneNumber == uninstalledPushCode.PhoneNumber && x.CompanyId == uninstalledPushCode.CompanyId)
                //            .SkdKey;
                //    db.InstallationsLogs.Add(new InstallationsLog { PhoneNumber = uninstalledPushCode.PhoneNumber + DELIMITER + sdkKey, Action = 0, Reason = "NotRegistered", CreateDate = DateTime.Now });
                //}


                List<State> statesToReconnect = db.States.Where(s => toSaveReconnect.Contains(s.PhoneNumber)).ToList();


                foreach (State state in statesToReconnect)
                {
                    state.ReconnectPushDate = DateTime.UtcNow;
                    redisDb.KeyDelete(USER_HASH_PREFIX + state.PhoneNumber.TrimEnd(' '), CommandFlags.FireAndForget);
                }

                db.SaveChanges();
            }
        }

        internal static void UninstallUsersWhoDidntReportForAWeek(List<PhoneNumberAndSdkWithGcm> phoneNumberAndSdkWithGcm)
        {

            using (mystateApiDbEntities1 db = new mystateApiDbEntities1())
            {
                phoneNumberAndSdkWithGcm = phoneNumberAndSdkWithGcm.Where(s => s.LastUpdated < DateTime.UtcNow.AddDays(-7)).
                                    ToList();


                List<ServerPushCode> uninstalledPushCodes = new List<ServerPushCode>();

                var list = phoneNumberAndSdkWithGcm.Chunk(400);
                int total = phoneNumberAndSdkWithGcm.Count;
                int i = 0;
                foreach (var item in list)
                {
                    uninstalledPushCodes.AddRange(GetServerPushCodes(db, item.ToList()));
                    i += 400;
                    Console.WriteLine("Got {0} Uninstalled PushCodes from {1}", i, total);
                }

                var redisConnection = Program.Connection;
                var redisDb = redisConnection.GetDatabase();

                foreach (ServerPushCode serverPushCode in uninstalledPushCodes)
                {
                    serverPushCode.IsInstalled = false;
                    serverPushCode.UninstallationReason = "LastUpdatedOld";

                    //string sdkKey =
                    //    phoneNumberAndSdkWithGcm.Find(
                    //        x => x.PhoneNumber == serverPushCode.PhoneNumber && x.CompanyId == serverPushCode.CompanyId).SkdKey;
                    //redisDb.HashDelete(USER_HASH_PREFIX + serverPushCode.PhoneNumber.TrimEnd(' '),
                    //    SERVER_PUSH_CODE_FIELD_PREFIX + sdkKey, CommandFlags.FireAndForget);
                    redisDb.KeyDelete(USER_HASH_PREFIX + serverPushCode.PhoneNumber.TrimEnd(' '), CommandFlags.FireAndForget);
                }

                //foreach (ServerPushCode uninstalledPushCode in uninstalledPushCodes)
                //{
                //    string sdkKey =
                //        phoneNumberAndSdkWithGcm.Find(
                //            x => x.PhoneNumber == uninstalledPushCode.PhoneNumber && x.CompanyId == uninstalledPushCode.CompanyId)
                //            .SkdKey;
                //    db.InstallationsLogs.Add(new InstallationsLog { PhoneNumber = uninstalledPushCode.PhoneNumber + DELIMITER + sdkKey, Action = 0, Reason = "LastUpdatedOld", CreateDate = DateTime.Now });
                //}

                db.SaveChanges();
            }
        }

        private static List<ServerPushCode> GetServerPushCodes(mystateApiDbEntities1 context, List<PhoneNumberAndSdkWithGcm> phoneNumberAndSdkWithGcm)
        {
            //List<SqlParameter> phoneNumberParameters = new List<SqlParameter>(phoneNumberAndSdkWithGcm.Count);
            List<SqlParameter> sqlParameters = new List<SqlParameter>(phoneNumberAndSdkWithGcm.Count);
            StringBuilder whereSB = new StringBuilder();

            for (int index = 0; index < phoneNumberAndSdkWithGcm.Count; index++)
            {
                if (index != 0)
                    whereSB.Append(" or ");
                sqlParameters.Add(new SqlParameter("companyId" + index, phoneNumberAndSdkWithGcm[index].CompanyId));
                sqlParameters.Add(new SqlParameter("phoneNumber" + index, phoneNumberAndSdkWithGcm[index].PhoneNumber));


                whereSB.Append(string.Format("(PhoneNumber = @phoneNumber{0} and CompanyId = @companyId{0})", index));
            }


            string query = @"
                SELECT [PhoneNumber]
                      ,[CompanyId]
                      ,[PushCode]
                      ,[CreatedDate]
                      ,[UpdatedDate]
                      ,[IsInstalled]
                      ,[UninstallationReason]
                  FROM [dbo].[ServerPushCodes]
                  where " + whereSB;

            var sqlQuery = context.Set<ServerPushCode>().SqlQuery(query, sqlParameters.ToArray());

            var result = sqlQuery.ToList();
            return result;



        }

        internal static void UninstallUsersWithoutGCM(List<PhoneNumberAndSdkWithGcm> phoneNumbersWithGcm)
        {
            List<string> noGcm = phoneNumbersWithGcm.Where(s => string.IsNullOrEmpty(s.GcmRegistrationId)).Select(s => s.PhoneNumber).ToList();

            if (noGcm.Count > 0)
            {
                using (var db = new mystateApiDbEntities1())
                {
                    var redisConnection = Program.Connection;
                    var redisDb = redisConnection.GetDatabase();
                    var statesToUninstall = db.States.Where(s => noGcm.Contains(s.PhoneNumber)).ToList();

                    foreach (var state in statesToUninstall)
                    {
                        state.IsUninstalled = true;
                        state.UninstallationReason = "GcmRegistrationIdEmpty";
                        redisDb.KeyDelete(USER_HASH_PREFIX + state.PhoneNumber.TrimEnd(' '), CommandFlags.FireAndForget);
                    }

                    foreach (string phone in noGcm)
                    {
                        db.InstallationsLogs.Add(new InstallationsLog { PhoneNumber = phone, Action = 0, Reason = "GcmRegistrationIdEmpty", CreateDate = DateTime.Now });
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}
