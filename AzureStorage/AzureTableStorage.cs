using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureStorage;
using CommonInterfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using Newtonsoft.Json;

namespace AzureStorage
{
    public class AzureTableStorage : IAzureStorage
    {
        private readonly IMyStateLogger _logger;
        readonly CloudTableClient _tableClient;
        private Dictionary<string, string> tableTypeDictionary;
        //todo: consider using IgnoreResourceNotFoundException

        public AzureTableStorage(string connectionString, IMyStateLogger logger)
        {
            try
            {
                _logger = logger;
                // Retrieve the storage account from the connection string.
                var storageAccount = CloudStorageAccount.Parse(connectionString);

                // Create the table client.
                _tableClient = storageAccount.CreateCloudTableClient();
                // Create the table if it doesn't exist.
                CreateTables();
            }
            catch (Exception ex)
            {
                _logger.Write(new Log(ex, "AzureTableStorage", Log.MessageType.Exception, "Constractor"));
            }

        }

        private void CreateTables()
        {
            try
            {
                tableTypeDictionary = new Dictionary<string, string>(2);

                CreateTable<StateHistoryEntity>();
                CreateTable<MessageForClient>();
                CreateTable<LogRecord>();
                CreateTable<SyncContactsRawData>();
                CreateTable<ContactDetailsEntity>();
                CreateTable<ContactDetailsEntity>(ContactDetailsEntity.SuspectedNamesVault);
                CreateTable<ContactDetailsEntity>(ContactDetailsEntity.DuplicateBackup);
                CreateTable<CallDetailsEntity>();

            }
            catch (Exception ex)
            {
                _logger.Write(new Log(ex, "AzureTableStorage", Log.MessageType.Exception, "CreateTables"));
            }
        }

        private void CreateTable<T>(string additionalSecondaryTableName)
        {
            Type tableType = typeof(T);
            string tableInitials = tableType.FullName.Substring(tableType.FullName.LastIndexOf('.')+1);
            tableTypeDictionary.Add(tableInitials + '.' + additionalSecondaryTableName, additionalSecondaryTableName);
            CloudTable table = _tableClient.GetTableReference(additionalSecondaryTableName);
            table.CreateIfNotExists();
            
        }

        private void CreateTable<T>()
        {
            Type tableType = typeof(T);

            string tableName = tableType.FullName.Substring(tableType.FullName.LastIndexOf('.') + 1);
            tableTypeDictionary.Add(tableType.FullName, tableName);
            CloudTable table = _tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();
        }

        public void Save<T>(T objToSave) where T : TableEntity, new()
        {
            try
            {
                // Create the CloudTable object that represents the "people" table.
                CloudTable table = _tableClient.GetTableReference(tableTypeDictionary[typeof(T).FullName]);

                // Create a new customer entity.
                // Create the TableOperation that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(objToSave);

                // Execute the insert operation.
                table.Execute(insertOperation);
            }
            catch (Exception ex)
            {
                _logger.Write(new Log(ex, "AzureTableStorage", Log.MessageType.Exception, "Save"));
            }
        }

        public void SaveBatch<T>(List<T> objToSaveList) where T : TableEntity, new()
        {
            try
            {
                // Create the CloudTable object that represents the "people" table.
                CloudTable table = _tableClient.GetTableReference(tableTypeDictionary[typeof(T).FullName]);

                // Create the batch operation.
                TableBatchOperation batchOperation = new TableBatchOperation();

                objToSaveList.ForEach(batchOperation.Insert);

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);
            }
            catch (Exception ex)
            {
                _logger.Write(new Log(ex, "AzureTableStorage", Log.MessageType.Exception, "Save"));
            }
        }

        public List<T> Get<T>(string partitionKey) where T : TableEntity, new()
        {
            try
            {
                string tableName = typeof(T).FullName;
                if (!tableTypeDictionary.Keys.Contains(tableName))
                    return null;
                string tableRef = tableTypeDictionary[tableName];
                //Create the CloudTable object that represents the "people" table.
                CloudTable table = _tableClient.GetTableReference(tableRef);

                // Create the table query.
                TableQuery<T> rangeQuery = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

                // Loop through the results, displaying information about the entity.
                var result = table.ExecuteQuery(rangeQuery);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.Write(new Log(ex, "AzureTableStorage", Log.MessageType.Exception, "Load"));
            }
            return null;
        }

        public List<T> Get<T>(string partitionKey, int take) where T : TableEntity, new()
        {
            try
            {
                string tableName = typeof(T).FullName;
                if (!tableTypeDictionary.Keys.Contains(tableName))
                    return null;
                string tableRef = tableTypeDictionary[tableName];
                //Create the CloudTable object that represents the "people" table.
                CloudTable table = _tableClient.GetTableReference(tableRef);

                // Create the table query.
                TableQuery<T> rangeQuery = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey))
                    .Take(take);

                // Loop through the results, displaying information about the entity.
                var result = table.ExecuteQuery(rangeQuery);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.Write(new Log(ex, "AzureTableStorage", Log.MessageType.Exception, "Load"));
            }
            return null;
        }

        public List<T> Get<T>(string partitionKey, DateTime @from, DateTime to) where T : TableEntity, new()
        {
            try
            {
                string tableName = typeof(T).FullName;
                if (!tableTypeDictionary.Keys.Contains(tableName))
                    return null;
                string tableRef = tableTypeDictionary[tableName];
                //Create the CloudTable object that represents the "people" table.
                CloudTable table = _tableClient.GetTableReference(tableRef);

                // Create the table query.
                TableQuery<T> rangeQuery = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey))
                    .Where(TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, new DateTimeOffset(from)))
                    .Where(TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, new DateTimeOffset(to)));

                // Loop through the results, displaying information about the entity.
                var result = table.ExecuteQuery(rangeQuery);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.Write(new Log(ex, "AzureTableStorage", Log.MessageType.Exception, "Load"));
            }
            return null;
        }

        public T Get<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            try
            {
                string tableName = typeof(T).FullName;
                if (!tableTypeDictionary.Keys.Contains(tableName))
                    return null;
                string tableRef = tableTypeDictionary[tableName];
                //Create the CloudTable object that represents the "people" table.
                CloudTable table = _tableClient.GetTableReference(tableRef);

                TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
                TableResult retrievedResult = table.Execute(retrieveOperation);
                return retrievedResult.Result as T;
            }
            catch (Exception ex)
            {
                _logger.Write(new Log(ex, "AzureTableStorage", Log.MessageType.Exception, "Load"));
            }
            return null;
        }

        public void DeleteBatch<T>(string partitionKey, IEnumerable<string> rowKeys) where T : TableEntity, new()
        {
            if (string.IsNullOrEmpty(partitionKey) || rowKeys == null || !rowKeys.Any())
                return;
            CloudTable table = _tableClient.GetTableReference(tableTypeDictionary[typeof(T).FullName]);
            ExecuteBatch(rowKeys.Select(x => new DynamicTableEntity(partitionKey, x)), table, TableOperation.Delete);
        }

        public void DeleteBatch<T>(List<T> entities) where T : TableEntity, new()
        {
            if (entities == null || !entities.Any())
                return;
            CloudTable table = _tableClient.GetTableReference(tableTypeDictionary[typeof(T).FullName]);
            ExecuteBatch(entities.Select(x => new DynamicTableEntity(x.PartitionKey, x.RowKey)), table, TableOperation.Delete);
        }

        public void DeleteBatch(IEnumerable<DynamicTableEntity> entities, CloudTable table)
        {
            ExecuteBatch(entities, table, TableOperation.Delete);
        }

        public void ExecuteBatch(IEnumerable<DynamicTableEntity> entities, CloudTable table, Func<ITableEntity, TableOperation> operation)
        {
            var batches = new Dictionary<string, TableBatchOperation>();

            foreach (var entity in entities)
            {
                entity.ETag = "*";
                TableBatchOperation batch = null;

                if (batches.TryGetValue(entity.PartitionKey, out batch) == false)
                {
                    batches[entity.PartitionKey] = batch = new TableBatchOperation();
                }

                batch.Add(operation(entity));

                if (batch.Count == 100)
                {
                    ExecuteBatchSafely(table, batch);
                    batches[entity.PartitionKey] = new TableBatchOperation();
                }
            }

            foreach (var batch in batches.Values)
            {
                if (batch.Count > 0)
                {
                    ExecuteBatchSafely(table, batch);
                }
            }

        }

        /// <summary>
        /// Comment: ignore duplicates (takes the first)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="operation"></param>
        public void ExecuteBatch<T>(IEnumerable<T> entities, Func<ITableEntity, TableOperation> operation, string altTableName = null) where T : class, ITableEntity
        {
            if (entities == null)
                return;

            var batches = new Dictionary<string, TableBatchOperation>();
            HashSet<string> duplicateScreen = new HashSet<string>();
            CloudTable table = _tableClient.GetTableReference(altTableName ?? tableTypeDictionary[typeof(T).FullName]);

            foreach (var entity in entities)
            {
                if (string.IsNullOrEmpty(entity?.PartitionKey))
                    continue;

                entity.ETag = "*";
                string hashedKey = entity.PartitionKey + entity.RowKey;
                if (duplicateScreen.Contains(hashedKey))
                {
                    continue;
                }
                duplicateScreen.Add(hashedKey);

                if (!batches.ContainsKey(entity.PartitionKey))
                {
                    batches.Add(entity.PartitionKey, new TableBatchOperation());
                }

                batches[entity.PartitionKey].Add(operation(entity));

                if (batches[entity.PartitionKey].Count == 100)
                {
                    ExecuteBatchSafely(table, batches[entity.PartitionKey]);
                    batches[entity.PartitionKey] = new TableBatchOperation();
                }
            }

            foreach (var batch in batches.Values)
            {
                if (batch.Count > 0)
                {
                    ExecuteBatchSafely(table, batch);
                }
            }
        }

        /// <summary>
        /// Warning, not recommended.
        /// If you're not sure it means you dont need this.
        /// These Are Not the Droids You Are Looking For.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="operation"></param>
        public List<T> GetAll<T>() where T : class, ITableEntity, new()
        {
            string tableName = typeof(T).FullName;
            if (!tableTypeDictionary.Keys.Contains(tableName))
                return null;
            string tableRef = tableTypeDictionary[tableName];
            //Create the CloudTable object that represents the "people" table.
            CloudTable table = _tableClient.GetTableReference(tableRef);

            // Create the table query.
            TableQuery<T> rangeQuery = new TableQuery<T>();
            rangeQuery = rangeQuery.Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, ""))
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual,
                    "+9999999999999"))
                    .Select(new List<string>() { "PartitionKey" })
                    ;
            var result = table.ExecuteQuery(rangeQuery);
            return result.ToList();

        }


        private void ExecuteBatchSafely(CloudTable table, TableBatchOperation batch)
        {
            if (batch == null || !batch.Any())
                return;
            try
            {
                table.ExecuteBatch(batch);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "ResourceNotFound")
                {
                    if (batch.Count == 1)
                        return;
                    TableBatchOperation firstBatch = new TableBatchOperation();
                    TableBatchOperation secondBatch = new TableBatchOperation();
                    for (int i = 0; i < batch.Count; i++)
                    {
                        if (i % 2 == 0)
                            firstBatch.Add(batch[i]);
                        else
                            secondBatch.Add(batch[i]);
                    }

                    ExecuteBatchSafely(table, firstBatch);
                    ExecuteBatchSafely(table, secondBatch);
                }
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }

        }

        private void DeleteAllEntitiesInBatches(CloudTable table, Expression<Func<DynamicTableEntity, bool>> filter)
        {
            ProcessEntities(table, DeleteBatch, filter);
        }

        private void ProcessEntities(CloudTable table,
                                    Action<IEnumerable<DynamicTableEntity>, CloudTable> processor,
                                    Expression<Func<DynamicTableEntity, bool>> filter)
        {
            TableQuerySegment<DynamicTableEntity> segment = null;

            while (segment == null || segment.ContinuationToken != null)
            {
                if (filter == null)
                {
                    segment = table.ExecuteQuerySegmented(new TableQuery().Take(100), segment == null ? null : segment.ContinuationToken);
                }
                else
                {
                    var query = table.CreateQuery<DynamicTableEntity>().Where(filter).Take(100).AsTableQuery();
                    segment = query.ExecuteSegmented(segment == null ? null : segment.ContinuationToken);
                }

                processor(segment.Results, table);
            }
        }

        public void SaveStateUpdate(string phoneNumber, string stateName, string stateValueStringed)
        {
            StateHistoryEntity stateHistoryEntity = new StateHistoryEntity(phoneNumber, stateName, stateValueStringed);
            Save(stateHistoryEntity);
        }

        public void SaveStateUpdateBatch(List</*BaseStateUpdateRequest*/ object> updatedUserStateList)
        {
            //long now = DateTime.UtcNow.Ticks;
            //List<StateHistoryEntity> listToSave =
            //updatedUserStateList.Select(
            //    x =>
            //        new StateHistoryEntity(x.PhoneNumber, (now++).ToString())
            //        {
            //            StateName = x.StateName,
            //            StateValue = x.StateValueStringed,
            //            UpdatedDate = x.UpdatedDate.ToString(CultureInfo.InvariantCulture)
            //        }).ToList();
            //SaveBatch(listToSave);
        }

        public void SaveContactDetails(List<IContactDetails> contactsList)
        {
            if (contactsList == null || !contactsList.Any())
                return;

            List<ContactDetailsEntity> contactDetailsEntity =
                contactsList.
                    Where(x => x.PhoneNumber != null).Select(
                        x => new ContactDetailsEntity(x))
                    .ToList();
            AddSystemRow(contactDetailsEntity);
            ExecuteBatch(contactDetailsEntity, TableOperation.InsertOrReplace);
        }

        public void SaveContactDetailsSuspectedNames(List<IContactDetails> contactsList)
        {
            if (contactsList == null || !contactsList.Any())
                return;

            List<ContactDetailsEntity> contactDetailsEntity =
                contactsList.
                    Where(x => x.PhoneNumber != null).Select(
                        x => new ContactDetailsEntity(x))
                    .ToList();
            AddSystemRow(contactDetailsEntity);
            ExecuteBatch(contactDetailsEntity, TableOperation.InsertOrReplace, ContactDetailsEntity.SuspectedNamesVault);
        }

        public void SaveContactDetailsDuplicateBackup(List<IContactDetails> contactsList)
        {
            if (contactsList == null || !contactsList.Any())
                return;

            List<ContactDetailsEntity> contactDetailsEntity =
                contactsList.
                    Where(x => x.PhoneNumber != null).Select(
                        x => new ContactDetailsEntity(x))
                    .ToList();
            AddSystemRow(contactDetailsEntity);
            ExecuteBatch(contactDetailsEntity, TableOperation.InsertOrReplace, ContactDetailsEntity.DuplicateBackup);
        }

        public void SaveCalledContact(CallDetailsEntity callDetailsEntity)
        {
            var callDetailsEntityList = new List<CallDetailsEntity> { callDetailsEntity };
            AddSystemRow(callDetailsEntityList);
            ExecuteBatch(callDetailsEntityList, TableOperation.InsertOrReplace);
        }

        public Tuple<List<T>, TableContinuationToken> GetAllEntitiesByBatchs<T>(int maxNumOfElements, TableContinuationToken continuationToken) where T : MyStateTableEntity, new()
        {
            // Create the CloudTable object that represents the "people" table.
            var table = _tableClient.GetTableReference(tableTypeDictionary[typeof(T).FullName]);
            var tableQuery = new TableQuery<T>();
            tableQuery.Take(maxNumOfElements);
            var query = table.ExecuteQuerySegmented(
                tableQuery, continuationToken);
            return new Tuple<List<T>, TableContinuationToken>(query.Results, query.ContinuationToken);
        }

        public void SaveLogRecord(LogRecord logRecord)
        {
            logRecord.PartitionKey = "defualt";
            logRecord.RowKey = DateTime.UtcNow.Ticks.ToString();
            Save(logRecord);
        }

        private void AddSystemRow<T>(List<T> entityList) where T : MyStateTableEntity, new()
        {
            if (entityList == null)
                return;
            HashSet<string> uniquePartitions = new HashSet<string>(entityList.Select(x => x.PartitionKey));
            foreach (var uniquePartition in uniquePartitions)
            {
                if (uniquePartition == null)
                    continue;
                T sysItem = new T()
                {
                    PartitionKey = uniquePartition,
                };
                sysItem.RowKey = sysItem.GetTableName;
                entityList.Add(sysItem);
            }
        }
    }
}

public interface IAzureStorage
{
    void Save<T>(T customerEntity) where T : TableEntity, new();
    List<T> Get<T>(string lastName) where T : TableEntity, new();
    List<T> Get<T>(string partitionKey, int take) where T : TableEntity, new();
    List<T> Get<T>(string partitionKey, DateTime @from, DateTime to) where T : TableEntity, new();
    void SaveStateUpdateBatch(List</*BaseStateUpdateRequest*/ object> updatedUserState);
    void SaveBatch<T>(List<T> objToSaveList) where T : TableEntity, new();
    void DeleteBatch<T>(string partitionKey, IEnumerable<string> rowKeys) where T : TableEntity, new();

    void DeleteBatch<T>(List<T> entities) where T : TableEntity, new();
    void DeleteBatch(IEnumerable<DynamicTableEntity> entities, CloudTable table);
    void SaveContactDetails(List<IContactDetails> contactsList);
    void SaveContactDetailsSuspectedNames(List<IContactDetails> contactsList);
    void SaveContactDetailsDuplicateBackup(List<IContactDetails> contactsList);
    List<T> GetAll<T>() where T : class, ITableEntity, new();

    void SaveCalledContact(CallDetailsEntity callDetailsEntity);

    Tuple<List<T>, TableContinuationToken> GetAllEntitiesByBatchs<T>(int maxNumOfElements,
        TableContinuationToken continuationToken) where T : MyStateTableEntity, new();
    void SaveLogRecord(LogRecord logRecord);
}

