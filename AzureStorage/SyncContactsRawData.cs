using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public class SyncContactsRawData : TableEntity, IBaseTableEntity
    {
        public const string TableName = "SyncContactsRawData";
        public string RawData { get; set; }

        public SyncContactsRawData()
        {
        }
        public SyncContactsRawData(string phoneNumber)
        {
            this.PartitionKey = DateTime.UtcNow.ToString("yy-MM-dd");
            this.RowKey = phoneNumber;
        }
    }
}
