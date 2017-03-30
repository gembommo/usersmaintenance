using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public class LogRecord : TableEntity, IBaseTableEntity
    {
        public const string TableName = "LogRecord";
        public string Messagetype { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }


        public LogRecord()
        {
        }
        public LogRecord(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }
    }
}
