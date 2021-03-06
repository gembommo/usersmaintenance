﻿using MyState.WebApplication.PushNotificationEngines;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Data.SqlTypes;
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
        private static readonly DateTime ONE_DAY_AGO = DateTime.UtcNow.AddDays(-1);//new DateTime(1753, 1, 1);
        static void Main(string[] args)
        {
            DB.Log(new Exception("UsersMaintenance"));
            //SendMePushReconnectCode();
            //return;
            try
            {
                DB.SyncDbAndRedisContacts();

                List<PhoneNumberAndSdkWithGcm> phoneNumbers = DB.GetStatesWhoDidntReportSince(ONE_DAY_AGO);

                //Canceled in order prevent Sdk users who doesn't have Gcm code from being removed
                DB.UninstallUsersWithoutGCM(phoneNumbers);

                phoneNumbers.RemoveAll(s => string.IsNullOrEmpty(s.GcmRegistrationId));

                DB.UninstallUsersWhoDidntReportForAWeek(phoneNumbers);

                SendPushToReconnectAndReportBack(phoneNumbers);

                DB.Log(new Exception("UsersMaintenance finished"));
            }
            catch (Exception ex)
            {
                DB.Log(ex);
            }
        }

        private static void SendMePushReconnectCode()
        {
            var pushDetails = PushSender.SendReconnectRequest(new List<string>() { "c41XYM5L_dY:APA91bGSFBvm3o7ESNsmz6V1_A9J6Ia43WObUsrFQnfDLSyJC0-rgDMH92LixkKNVhEI8yV4vdVPwSdtFttaAzLzIF_SCth2Cz-HLmzLGojem5dN_mF3WUiPKZrmNORmKB6VvI9J0mwH" });

            List<GoogleResult> gcmResults = pushDetails.RawResponse.Results;
            if (gcmResults[0].Error == "NotRegistered")
            {
                Console.WriteLine("NotRegistered");
            }
            else
            {
                Console.WriteLine("Registered");
            }
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
            options.ConnectTimeout = 240000;
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
