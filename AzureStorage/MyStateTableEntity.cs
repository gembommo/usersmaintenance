using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public abstract class MyStateTableEntity : TableEntity
    {
        public string SystemKey { get; set; }

        public abstract string GetTableName {get;}
    }
}