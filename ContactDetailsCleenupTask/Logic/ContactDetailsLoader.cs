using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage;
using CommonInterfaces;
using CommonTools;
using ContactDetailsCleenupTask.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace ContactDetailsCleenupTask.Logic
{
    public class ContactDetailsLoader : IContactDetailsLoader
    {
        private readonly IDbCompleteDataStore _monitorDb;
        private readonly IAzureStorage _elementSource;
        private readonly IMyStateLogger _logger;
        private int _contactDetailsLoaderCounter;

        public ContactDetailsLoader(IDbCompleteDataStore monitorDb, IAzureStorage elementSource, IMyStateLogger logger)
        {
            _monitorDb = monitorDb;
            _elementSource = elementSource;
            _logger = logger;
        }
        public void ForEach(int batchCount, int dellayInMilliSeconds, Func<List<IContactDetails>, bool>[] operations)
        {
            ResetMonitoringParameters();
            Tuple<List<ContactDetailsEntity>, TableContinuationToken> nextBatch =
                new Tuple<List<ContactDetailsEntity>, TableContinuationToken>(null, null);
            do
            {
                nextBatch = _elementSource.GetAllEntitiesByBatchs<ContactDetailsEntity>(batchCount, nextBatch.Item2);
                //nextBatch = new Tuple<List<ContactDetailsEntity>, TableContinuationToken>(_elementSource.Get<ContactDetailsEntity>("+972524645991") , null);
                
                List<IContactDetails> items = ModelConverter.GetContactDetailsList(nextBatch.Item1);
                foreach (var operation in operations)
                {
                    bool result;
                    try
                    {
                        result = operation.Invoke(items);
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        _logger.Write(ex);
                    } 
                    if (!result)
                    {
                        _logger.Write(new Log()
                        {
                            Details = operation.Method.Name,
                            Title = "Operation Failed",
                        });
                    }
                }

                UpdateMonitor(nextBatch.Item1.Count, nextBatch.Item1.Last().PartitionKey);

                Thread.Sleep(dellayInMilliSeconds);

            } while (nextBatch.Item2 != null);
        }

        private void UpdateMonitor(int itemsCount, string partitionKey)
        {
            _contactDetailsLoaderCounter += itemsCount;
            _monitorDb.SetTempTableValue(/*"ContactDetailsLoaderCounter"*/3, _contactDetailsLoaderCounter.ToString());
            _monitorDb.SetTempTableValue(/*"ContactDetailsLoaderProgress"*/4, partitionKey);
        }

        //private static List<IContactDetails> GetContactDetailsesList(Tuple<List<ContactDetailsEntity>, TableContinuationToken> nextBatch)
        //{
        //    return nextBatch.Item1.Select(x =>
        //    {
        //        IContactDetails item = new ContactDetails();
        //        x.ConvertTo(ref item);
        //        return item;
        //    }).ToList();
        //}

        private void ResetMonitoringParameters()
        {
            _contactDetailsLoaderCounter = 0;
            UpdateMonitor(0, "");
        }
    }
}
