using System;
using CommonInterfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public class CallDetailsEntity : MyStateTableEntity, IBaseTableEntity
    {
        public const string TableName = "CallDetailsEntity";
        public CallDetailsEntity()
        {
        }
        public CallDetailsEntity(string callerPhoneNumber, string contactPhoneNumber)
        {
            this.PartitionKey = contactPhoneNumber;
            this.RowKey = DateTime.UtcNow.Ticks.ToString();
            this.SourcePhoneNumber = callerPhoneNumber; 
        }

        public CallDetailsEntity(ICallDetails contactsDetails)
        {
            this.PartitionKey = contactsDetails.BusinessPhoneNumber;
            this.RowKey = DateTime.UtcNow.Ticks.ToString();
            //this.RowKey = contactsDetails.SourcePhoneNumber;
            this.SourcePhoneNumber = contactsDetails.PhoneNumber;
        }

        public string SourcePhoneNumber { get; set; }

        public static string GetLast(string source, int last)
        {
            return last >= source.Length ? source : source.Substring(source.Length - last);
        }

        public override string GetTableName => TableName;
    }
}
