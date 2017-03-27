using MyState.WebApplication.PushNotificationEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessNumbersUpdatedPush
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Getting Push Codes from DB");
            //get the gcm codes for relevant numbers:
            //3. they are from version x(?) or more?
            var pushCodes = DB.GetNumbers();
            pushCodes.RemoveAll(s => s == "GcmRegistrationId");
            pushCodes.RemoveAll(s => s == null);

            Console.WriteLine("Got {0} Push Codes from DB",pushCodes.Count);
            var listOfLists = Chunk(pushCodes, 800);

            foreach (var list in listOfLists)
            {
                Console.WriteLine("Sending {0} Pushes ", list.Count());
                PushDetails details = PushSender.SendResyncBusinessNumbersRequest(list);

                Thread.Sleep(1000);
            }
          

        }

        private static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}
