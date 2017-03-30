using System;
using System.Globalization;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public class ExceptionLogEntity : TableEntity, IBaseTableEntity
    {
        public const string TableName = "ExceptionLogEntity";

        public ExceptionLogEntity()
        {
        }

        public ExceptionLogEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public ExceptionLogEntity(Exception ex)
        {
            DateTime now = DateTime.UtcNow;
            this.PartitionKey = now.Date.ToString(CultureInfo.InvariantCulture);
            this.RowKey = now.Ticks.ToString(CultureInfo.InvariantCulture);
            StackTrace = ex.StackTrace;
            Message = ex.Message;
        }

        public string Message { get; set; }

        public string StackTrace { get; set; }
    }
}