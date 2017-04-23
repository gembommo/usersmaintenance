using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonInterfaces;
using Dapper;
using MyState.WebApplication.DataStore.Helpers;
using MyState.WebApplication.DataStore.UserDefinedSqlObjects;
using MyState.WebApplication.PushNotificationEngines;
using Newtonsoft.Json;

namespace MyState.WebApplication.DataStore.Account
{
    public class DapperDb : IDbCompleteDataStore
    {
        readonly string _connectionString;
        readonly string _multipleActiveResultSetsString;
        public const int DbTimeout = 240;


        public DapperDb(string stringConnectionName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings?[stringConnectionName]?.ConnectionString
                                    ?? stringConnectionName;

            SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(_connectionString)
            {
                MultipleActiveResultSets = true,
                ConnectTimeout = DbTimeout
            };
            _multipleActiveResultSetsString = scsb.ConnectionString;
        }

        private  SqlConnection GetOpenConnection(bool multipleActiveResultSets = false)
        {
            var cs = multipleActiveResultSets ? _multipleActiveResultSetsString : _connectionString;
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }

        private async Task<SqlConnection> GetOpenConnectionAsync(bool multipleActiveResultSets = false)
        {
            var cs = multipleActiveResultSets ? _multipleActiveResultSetsString : _connectionString;
            var connection = new SqlConnection(cs);
            await connection.OpenAsync();
            return connection;
        }

        public async Task SetTempTableValue(int key, string value)
        {
            using (var connection = GetOpenConnection())
            {
                var response = await connection.SafeExecuteAsync(
@"
begin tran
   update [dbo].[TempValues]
    
   set [value] = (@value)
   where [key] = @key

   if @@rowcount = 0
   begin
      insert into [dbo].[TempValues] 
    ([key], [value]) 
	VALUES (@key,@value)
   end
commit tran",
    new
    {
        key,
        value
    },
        commandType: CommandType.Text);
            }
        }

        public async Task<string> GetTempTableValue(string key)
        {
            using (var connection = GetOpenConnection())
            {
                var response = await connection.SafeQueryAsync<TempValues>(
                    @"
SELECT TOP 1 *
FROM [dbo].[TempValues]
where [key] = @key",
                    new { key = key },
                    commandType: CommandType.Text);
                return response.Content.GetComputedValueOrNull(x => x.FirstOrDefault()?.value);
            }
        }

        public void InsertLog(Log log)
        {
            using (var connection = GetOpenConnection())
            {
                connection.SafeExecute("InsertToLogs",
                    new
                    {
                        log.Title,
                        log.Message,
                        Type = log.Details,
                        log.Severity,
                        log.CallStack,
                        Extra =
                        JsonConvert.SerializeObject(log.Param,
                            new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate }),
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: DbTimeout);
            }
        }

        public async Task InsertLogAsync(Log log)
        {
            using (var connection = await GetOpenConnectionAsync())
            {
                connection.Execute("InsertToLogs",
                    new
                    {
                        log.Title,
                        log.Message,
                        Type = log.Details,
                        log.Severity,
                        log.CallStack,
                        Extra =
                        JsonConvert.SerializeObject(log.Param,
                            new JsonSerializerSettings() {DefaultValueHandling = DefaultValueHandling.Populate}),
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: DbTimeout);
            }
        }

        public List<SearchableWord> LoadBadWords()
        {
            using (var connection = GetOpenConnection())
            {
                var response = connection.SafeQuery<SearchableWord>(
                    @"
SELECT *
FROM [dbo].[ForbidenWords]
",
                    commandType: CommandType.Text);
                return response.Content?.ToList();
            }

        }

        public void SaveBadWords(HashSet<string> words)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var word in words)
            {
                sb.Append(string.Format("('{0}'),", word));
            }
            sb.Remove(sb.Length - 1, 1);
            

            using (var connection = GetOpenConnection())
            {
                string query = @"
insert into [dbo].[ForbidenWords]
(Word)
values
";
                var response = connection.SafeQuery<string>(
                    query + sb,
                    commandType: CommandType.Text);
            }

        }

        public void SaveBadWord(string word)
        {
            using (var connection = GetOpenConnection())
            {
                string query = @"
insert into [dbo].[ForbidenWords]
(Word)
values ('"+word+"')";
                var response = connection.SafeQuery<string>(
                    query,
                    commandType: CommandType.Text);
            }

        }
    }

    public class TempValues
    {
        public int key { get; set; }
        public string value { get; set; }
        public DateTime LastUpdated { get; set; }
           
    }

    public class SDKCompany
    {
        public string SDKKey { get; set; }
        public string AuthUrl { get; set; }
    }
}
