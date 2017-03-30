using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public class MessageForClient : TableEntity, IBaseTableEntity
    {
        public const string TableName = "MessageForClient";
        public string Message { get; set; }
        public string Messagetype { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Guid { get; set; }
        public string AdditionalInfo { get; set; }

        public MessageForClient()
        {
        }
        public MessageForClient(string phoneNumber, string rowKey)
        {
            this.PartitionKey = phoneNumber;
            this.RowKey = rowKey;
        }
    }
}
