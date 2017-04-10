using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using CommonInterfaces;
using CommonTools;
using ContactDetailsCleenupTask.BadWordsFilters;
using ContactDetailsCleenupTask.Interfaces;
using ContactDetailsCleenupTask.Logic;
using Microsoft.Azure.WebJobs;
using MyState.WebApplication.DataStore.Account;
using Ninject;

namespace ContactDetailsCleenupTask
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage

        public static IKernel Ioc;


        public static SearchWordsList BadWordsFilters = new SearchWordsList(){
            new SearchWordsByRegex(),
            new SearchWordsThatStartsWith("ה"),
            new SearchPossiblePasswords(),
            new SearchWordInText()
        };
        //public static SearchTree.SearchTree BadWords = new SearchTree.SearchTree();
        static void Main()
        {
            //var config = new JobHostConfiguration();

            //if (config.IsDevelopment)
            //{
            //    config.UseDevelopmentSettings();
            //}

            //var host = new JobHost();
            //// The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            LoadIoc();

            BadWordsFilters.InsertWords(Ioc.Get<IDbCompleteDataStore>().LoadBadWords());

            IContactDetailsLoader loader = Ioc.Get<IContactDetailsLoader>();

            loader.ForEach(batchCount: 1000, dellayInMilliSeconds: 100,
                operations: new Func<List<IContactDetails>, bool>[] { RemoveDuplicateRecords, AddNewIndexes, MarkBadWords});

        }

        private static bool RemoveDuplicateRecords(List<IContactDetails> contactDetailsList)
        {
            var logger = Ioc.Get<IMyStateLogger>();

            try
            {
                Dictionary<string, List<IContactDetails>> duplicatesToRemove = new Dictionary<string, List<IContactDetails>>();
                foreach (var contactDetails in contactDetailsList)
                {
                    if (contactDetails.RowKey == "ContactDetailsEntity")//Ignore control row
                        continue;
                    string localKey = string.Format("{0}#{1}#{2}", contactDetails.PhoneNumber,
                        contactDetails.SourcePhoneNumber, contactDetails.Name);
                    if (duplicatesToRemove.ContainsKey(localKey))
                    {
                        duplicatesToRemove[localKey].Add(contactDetails);
                    }
                    else
                    {
                        duplicatesToRemove.Add(localKey, new List<IContactDetails>());
                    }
                }


                var contactsDb = Ioc.Get<IAzureStorage>();
                foreach (var itemToRemove in duplicatesToRemove)
                {
                    if (itemToRemove.Value.IsNullOrEmpty())
                        continue;
                    contactsDb.DeleteBatch<ContactDetailsEntity>(itemToRemove.Key.Split('#').First(),
                        itemToRemove.Value.Select(x => x.RowKey));
                    contactsDb.SaveContactDetailsDuplicateBackup(itemToRemove.Value);

                    itemToRemove.Value.ForEach(x => contactDetailsList.Remove(x));
                }
            }
            catch (Exception ex)
            {
                logger.Write(ex);
                return false;
            }
            return true;
        }

        private static bool MarkBadWords(List<IContactDetails> contactDetailsList)
        {
            try
            {
                if (contactDetailsList.IsNullOrEmpty())
                    return true;
                var contactsDb = Ioc.Get<IAzureStorage>();
                var logger = Ioc.Get<IMyStateLogger>();
                List<IContactDetails> contactsToRemove = new List<IContactDetails>();
                
                foreach (var contectDetails in contactDetailsList)
                {
                    try
                    {
                        if (BadWordsFilters.SearchWord(contectDetails.Name))
                        {
                            contactsToRemove.Add(contectDetails.Clone());
                            contectDetails.Disabled = true;
                            contectDetails.ForbidenWord = true;
                        }
                    }
                    catch (Exception ex)
                    {

                        logger.Write(ex);
                    }
                }

                contactsDb.SaveContactDetailsSuspectedNames(contactsToRemove);

                contactsDb.DeleteBatch(ModelConverter.GetContactDetailsEntityList(contactsToRemove));
                return true;
            }
            catch (Exception ex)
            {
                Ioc.Get<IMyStateLogger>().Write(ex);
                return false;
            }
        }

        private static bool AddNewIndexes(List<IContactDetails> contectDetailsList)
        {
            //Add indexing by Name, source number and (?)searchable index.

            //throw new NotImplementedException();
            return true;
        }

        private static void LoadIoc()
        {
            Ioc = new StandardKernel();

            Ioc.Bind<IDbCompleteDataStore>().ToMethod(contex => new DapperDb("MyState_MainDB"));


            string storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            Ioc.Bind<IAzureStorage>()
                .ToMethod(
                    context => new AzureTableStorage(storageConnectionString, context.Kernel.Get<IMyStateLogger>()));

            Ioc.Bind<IMyStateLogger>()
                .ToMethod(
                    context =>
                        new MyStateLogger(context.Kernel.Get<IDbCompleteDataStore>()));
            Ioc.Bind<IContactDetailsLoader>()
                .ToMethod(
                    context =>
                        new ContactDetailsLoader(context.Kernel.Get<IDbCompleteDataStore>(),
                            context.Kernel.Get<IAzureStorage>(), context.Kernel.Get<IMyStateLogger>()));
        }
    }
}
