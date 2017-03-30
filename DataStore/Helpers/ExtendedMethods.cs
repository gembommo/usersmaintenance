using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using Dapper;
using MyState.WebApplication.DataStore.Account;
using Newtonsoft.Json;

namespace MyState.WebApplication.DataStore.Helpers
{
    internal static class ExtendedMethods
    {
        public static BaseSqlResponse<IEnumerable<T>> SafeQuery<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var response = new BaseSqlResponse<IEnumerable<T>>();
            try
            {
                commandTimeout = commandTimeout ?? DapperDb.DbTimeout;
                response.Content = cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
            }
            catch (Exception e)
            {
                LogToSql(cnn, sql, param, e);
            }
            return response;
        }

        public static BaseSqlResponse SafeExecute(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var response = new BaseSqlResponse();
            try
            {
                commandTimeout = commandTimeout ?? DapperDb.DbTimeout;
                cnn.Execute(sql, param, commandType: commandType, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                try
                {
                    LogToSql(cnn, sql, param, e);
                }
                catch (Exception ex)
                {
                    //Add to other log target
                }
                response.Exception = e;
            }
            return response;
        }

        private static void LogToSql(IDbConnection cnn, string sql, object param, Exception e)
        {
            cnn.Execute("InsertToLogs",
                                    new
                                    {
                                        Title = "DB Exception",
                                        Message = e.Message,
                                        Type = sql,
                                        Severity = 10,
                                        CallStack = e.StackTrace,
                                        Extra = JsonConvert.SerializeObject(param, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate }),
                                    },
                                    commandType: CommandType.StoredProcedure);
        }

        public static async Task<BaseSqlResponse> SafeExecuteAsync(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null,
          bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var response = new BaseSqlResponse();
            try
            {
                commandTimeout = commandTimeout ?? DapperDb.DbTimeout;
                await cnn.ExecuteAsync(sql, param, commandType: commandType, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                try
                {
                    LogToSql(cnn, sql, param, e);
                }
                catch (Exception)
                {
                    //Add to other log target
                }
                response.Exception = e;
            }
            return response;
        }


        public static async Task< BaseSqlResponse<IEnumerable<T>>> SafeQueryAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var response = new BaseSqlResponse<IEnumerable<T>>();
            try
            {
                commandTimeout = commandTimeout ?? DapperDb.DbTimeout;
                response.Content = await cnn.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
            }
            catch(Exception e)
            {
                LogToSql(cnn, sql, param, e);
            }
            return response;
        }

        public static S GetComputedValueOrNull<T, S>(this IEnumerable<T> content, Func<IEnumerable<T>, S> func)
        {
            return content == null ? default(S) : func(content);
        }
    
        public static DateTime GetSqlDateTime(this DateTime source)
        {
            return source == DateTime.MinValue
                ? SqlDateTime.MinValue.Value
                : source;
        }

        public static DateTime GetSqlDateTime(this long source)
        {
            return source == 0
                ? SqlDateTime.MinValue.Value
                : new DateTime(source);
        }
    
    }

    internal class BaseSqlResponse
    {
        public Exception Exception { get; set; }
    }

    internal class BaseSqlResponse<T> : BaseSqlResponse
    {
        public T Content { get; set; }
    }
    
}
