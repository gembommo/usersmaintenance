using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage;
using CommonInterfaces;
using CommonTools;
using ContactDetailsCleenupTask.Logic;
using Microsoft.WindowsAzure.Storage.Table;
using MyState.WebApplication.DataStore.Account;

namespace ExcellLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            LoadExcelToBadWordsTable(args);
            //SendBadWordsTableViaEMail();

        }

        private static void SendBadWordsTableViaEMail()
        {
            string file = LoadBadWordsTableToExcel();
        }

        private static string LoadBadWordsTableToExcel()
        {
            //List<ContactDetailsEntity> badNamesList = LoadBadWords();


            // before your loop
            var csv = new StringBuilder();

            //in your loop
            //var first = reader[0].ToString();
            //var second = image.ToString();
            ////Suggestion made by KyleMit
            //var newLine = string.Format("{0},{1}", first, second);
            //csv.AppendLine(newLine);

            return csv.ToString();
            //after your loop
            //File.WriteAllText(filePath, csv.ToString());
        }

        //private static List<ContactDetailsEntity> LoadBadWords()
        //{
        //    AzureTableStorage clinet =
        //        new AzureTableStorage(
        //            "DefaultEndpointsProtocol=https;AccountName=phoneusehistory;AccountKey=nHyLOGDDDaNK957Mw5q+lsl6+CeuEYvFh9B7w4joXK059EQqksrXpbmCY43uH38ueJTpDdeYeDlr+JCEheql8Q==",
        //            new MyStateEmptyLogger(null));
        //    ContactDetailsSuspecetWordsLoader loader = new ContactDetailsSuspecetWordsLoader(new EmptyDb(), clinet,
        //        new MyStateEmptyLogger(null));

            
        //    loader.ForEach(batchCount: 1000, dellayInMilliSeconds: 100,
        //        operations: new Func<List<IContactDetails>, bool>[] { AddToFileList });
        //}

        private static void LoadExcelToBadWordsTable(string[] strings)
        {
            string[] args;
            args = new[]
            {
                "Curses3.txt"
            };
            if (args.IsNullOrEmpty())
            {
                return;
            }

            var items = new HashSet<string>();

            foreach (var fileName in args)
            {
                using (var stream = new StreamReader(new FileStream(fileName, FileMode.Open)))
                {
                    while (!stream.EndOfStream)
                    {
                        var collection = stream.ReadLine()?.Split(',', /*' ', */'\n').Where(x => x != "").ToList();
                        collection?.ForEach(x => items.Add(x.Trim()));
                    }
                }
            }

            IDbCompleteDataStore sql = new DapperDb("MyState_MainDB");
            //sql.SaveBadWords(items);

            foreach (var item in items)
            {
                sql.SaveBadWord(item);
            }
        }
    }
}

