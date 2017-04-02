using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using CommonInterfaces;
using CommonTools;
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


        public static SearchTree.SearchTree BadWords = new SearchTree.SearchTree();
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

            BadWords.InsertRange(Ioc.Get<IDbCompleteDataStore>().LoadBadWords());

            IContactDetailsLoader loader = Ioc.Get<IContactDetailsLoader>();

            loader.ForEach(batchCount: 1000, dellayInMilliSeconds: 100,
                operations: new Func<List<IContactDetails>, bool>[] {AddNewIndexes, MarkBadWords});

        }

        private static bool MarkBadWords(List<IContactDetails> contactDetailsList)
        {
            try
            {
                var contactsDb = Ioc.Get<IAzureStorage>();
                List<IContactDetails> contatsToRemove = new List<IContactDetails>();
                List<IContactDetails> contatsToUpdate = new List<IContactDetails>();
                foreach (var contectDetails in contactDetailsList)
                {
                    List<string> removedWords = new List<string>();
                    foreach (var item in contectDetails.Name.SplitBySpaces())
                    {
                        if (BadWords.SearchWithPossibleSkip(item, 1, false))
                        {
                            removedWords.Add(item);
                        }
                    }

                    if (removedWords.Count != 0)
                    {
                        contatsToRemove.Add(contectDetails.Clone());
                        removedWords.ForEach(x => contectDetails.Name = contectDetails.Name.Replace(x, ""));
                        if (contectDetails.Name.Length < 3)
                        {
                            contectDetails.Disabled = true;
                            contectDetails.ForbidenWord = true;
                        }
                        contatsToUpdate.Add(contectDetails);
                    }
                }

                contactsDb.SaveContactDetailsSuspectedNames(contatsToRemove);
                contactsDb.SaveContactDetails(contatsToUpdate);
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

            Ioc.Bind<IMyStateLogger>().To<MyStateLogger>();
            Ioc.Bind<IContactDetailsLoader>()
                .ToMethod(
                    context =>
                        new ContactDetailsLoader(context.Kernel.Get<IDbCompleteDataStore>(),
                            context.Kernel.Get<IAzureStorage>(), context.Kernel.Get<IMyStateLogger>()));
        }
    }
}
