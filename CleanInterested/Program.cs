using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanInterested
{
    class Program
    {

        public const string USER_HASH_PREFIX = "User:";
        public const string INTERESTED_USERS_LIST = "InterestedUsersList";
        public const string INTERESTED_ON_OTHER_FIELD_PREFIX = "InterestedOnOther:";
        public const string INTERESTED_ON_USER_FIELD_PREFIX = "InterestedOnUser:";
        public const string SERVER_PUSH_CODE_FIELD_PREFIX = "ServerPushCode:";
        private const char DELIMITER = '|';

        static void Main(string[] args)
        {
            var db = Connection.GetDatabase();
            RedisValue[] interestedUsersPhones = db.SortedSetRangeByScore(INTERESTED_USERS_LIST, 0, DateTime.UtcNow.AddMinutes(-15).Ticks);

            foreach (string userPhoneAndSdk in interestedUsersPhones)
            {
                string[] parts = userPhoneAndSdk.Split(DELIMITER);
                string userPhone = parts[0];
                string userSdkKey = parts[1];

                IEnumerable<HashEntry> interestedOnOthers = db.HashScan(USER_HASH_PREFIX + userPhone,
                    INTERESTED_ON_OTHER_FIELD_PREFIX + "*", 100);

                foreach (HashEntry entry in interestedOnOthers)
                {
                    db.HashDelete(USER_HASH_PREFIX + userPhone, CreateInterestedOnOtherKey(entry.Value, userSdkKey), CommandFlags.FireAndForget);
                    db.HashDelete(USER_HASH_PREFIX + entry.Value, CreateInterestedKey(userPhone, userSdkKey), CommandFlags.FireAndForget);
                }
                db.SortedSetRemove(INTERESTED_USERS_LIST, CreateInterestedUsersListKey(userPhone, userSdkKey), CommandFlags.FireAndForget);
            }
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

        private static string CreateInterestedKey(string userPhone, string userSdkKey)
        {
            return INTERESTED_ON_USER_FIELD_PREFIX + userPhone + DELIMITER + userSdkKey;
        }

        private static string CreateInterestedUsersListKey(string userPhone, string userSdkKey)
        {
            return userPhone + DELIMITER + userSdkKey;
        }

        private static string CreateInterestedOnOtherKey(string phoneNumber, string sdkKey)
        {
            return INTERESTED_ON_OTHER_FIELD_PREFIX + phoneNumber + DELIMITER + sdkKey;
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
