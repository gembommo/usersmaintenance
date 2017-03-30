using System;
using System.Globalization;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public class StateHistoryEntity : TableEntity, IBaseTableEntity
    {
        public const string TableName = "StateHistoryEntity";
        public StateHistoryEntity()
        {
        }
        public StateHistoryEntity(string phoneNumber, string rowKey)
        {
            this.PartitionKey = phoneNumber;
            this.RowKey = rowKey;
        }
        public StateHistoryEntity(string phoneNumber, string stateName, string stateValueStringed)
        {
            DateTime now = DateTime.UtcNow;
            this.PartitionKey = phoneNumber;
            this.RowKey = now.Ticks.ToString(CultureInfo.InvariantCulture);
            this.StateName = stateName;
            StateValue = stateValueStringed;
            UpdatedDate = now.ToString(CultureInfo.InvariantCulture);
        }

        
        public string UpdatedDate { get; set; }
        public string StateValue { get; set; }
        public string StateName { get; set; }
    }
}
