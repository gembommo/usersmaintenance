using MyState.WebApplication.PushNotificationEngines;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UsersMaintenance
{
    class Program
    {
        private static readonly DateTime ONE_DAY_AGO = DateTime.UtcNow.AddDays(-1);
        static void Main(string[] args)
        {
            DB.SyncDbAndRedisContacts();

            List<PhoneNumberAndSdkWithGcm> phoneNumbers = DB.GetStatesWhoDidntReportSince(ONE_DAY_AGO);

            //Canceled in order prevent Sdk users who doesn't have Gcm code from being removed
            //DB.UninstallUsersWithoutGCM(phoneNumbers);

            phoneNumbers.RemoveAll(s => string.IsNullOrEmpty(s.GcmRegistrationId));

            DB.UninstallUsersWhoDidntReportForAWeek(phoneNumbers);

            SendPushToReconnectAndReportBack(phoneNumbers);
        }

        private static List<PhoneNumberAndSdkWithGcm> SendPushToReconnectAndReportBack(List<PhoneNumberAndSdkWithGcm> phoneNumbersAndSdksWithGcm)
        {
            phoneNumbersAndSdksWithGcm = phoneNumbersAndSdksWithGcm.Where(s => s.LastUpdated >= DateTime.UtcNow.AddDays(-7)).ToList();

            var listOfPhonesLists = phoneNumbersAndSdksWithGcm.Chunk(200).ToList();

            var count = listOfPhonesLists.Count;
            var j = 0;

            foreach (IEnumerable<PhoneNumberAndSdkWithGcm> phoneNumberAndSdkWithGcm in listOfPhonesLists)
            {
                Console.WriteLine("Round " + j++ + " from " + count);
                var partialList = phoneNumberAndSdkWithGcm.ToList();
                string[] usersCodes = partialList.Select(s => s.GcmRegistrationId).ToArray();
                var pushDetails = PushSender.SendReconnectRequest(usersCodes);

                List<GoogleResult> gcmResults = pushDetails.RawResponse.Results;

                List<PhoneNumberAndSdkWithGcm> toUninstall = new List<PhoneNumberAndSdkWithGcm>();
                List<string> toSaveReconnect = new List<string>();

                for (int i = 0; i < gcmResults.Count; i++)
                {
                    if (gcmResults[i].Error == "NotRegistered")
                    {
                        var uninstalledPushCode = partialList.Find(x => x.GcmRegistrationId == usersCodes[i]);
                        toUninstall.Add(uninstalledPushCode);
                    }
                    else
                    {
                        toSaveReconnect.Add(partialList[i].PhoneNumber);
                    }
                }

                DB.SaveResultsInDB(toUninstall, toSaveReconnect);
            }
            return phoneNumbersAndSdksWithGcm;
        }
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            ConfigurationOptions options = new ConfigurationOptions();
            options.EndPoints.Add(RedisConsts.RedisUrl);
            options.Ssl = true;
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 20000;
            options.Password = RedisConsts.RedisPassword;
            return ConnectionMultiplexer.Connect(options);
        });
        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }

    public static class RedisConsts
    {
        public static string RedisUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["RedisEndpoint"];
            }
        }

        public static string RedisPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["RedisPassword"];
            }
        }
    }
}
