using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonInterfaces;
using CommonTools;
using MyState.WebApplication.DataStore.Account;

namespace ExcellLoader
{
    class Program
    {
        static void Main(string[] args)
        {
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
                        collection?.ForEach(x=> items.Add(x.Trim()));
                    }
                }
            }

            IDbCompleteDataStore  sql = new DapperDb("MyState_MainDB");
            //sql.SaveBadWords(items);

            foreach (var item in items)
            {
                sql.SaveBadWord(item);
            }
        }
    }
}

