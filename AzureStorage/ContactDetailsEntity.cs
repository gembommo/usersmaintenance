using System;
using System.Threading;
using CommonInterfaces;

namespace AzureStorage
{
    public class ContactDetailsEntity : MyStateTableEntity, IBaseTableEntity
    {
        public const string TableName = "ContactDetailsEntity";

        //Additional secondary tables
        public static string SuspectedNamesVault = "ContactDetailsSuspectedNamesVault";
        //end

        private static long _safeInstanceCount = DateTime.UtcNow.Ticks;
        public ContactDetailsEntity()
        {
        }
        public ContactDetailsEntity(string contactPhoneNumber)
        {
            this.PartitionKey = contactPhoneNumber;
            this.RowKey = DateTime.UtcNow.Ticks.ToString();
        }

        public ContactDetailsEntity(IContactDetails contactsDetails)
        {
            this.PartitionKey = contactsDetails.PhoneNumber;
            {
                this.RowKey = Interlocked.Increment(ref _safeInstanceCount).ToString();
            }
            this.SourcePhoneNumber = contactsDetails.SourcePhoneNumber;
            this.Name = contactsDetails.Name;
        }

        public void ConvertTo(ref IContactDetails contactsDetails)
        {
            contactsDetails.PhoneNumber = this.PartitionKey;
            contactsDetails.RowKey = this.RowKey;
            contactsDetails.SourcePhoneNumber = this.SourcePhoneNumber;
            contactsDetails.Name = this.Name;
        }

        public string Name { get; set; }

        public string SourcePhoneNumber { get; set; }

        public string IconUrl { get; set; }
        public string BackgroundImageUrl { get; set; }

        public static string GetLast(string source, int last)
        {
            return last >= source.Length ? source : source.Substring(source.Length - last);
        }

        public override string GetTableName => TableName;
    }
}
