using MyState.WebApplication.PushNotificationEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToClients
{
    class Program
    {
        static void Main(string[] args)
        {
            var oldVersionList = DB.GetIsraelisWithOldVersionForPush();
            var listOfLists = oldVersionList.Chunk(800);
            foreach (var item in listOfLists)
            {
                PushSender.SendSimpleMessageWithUrl("חג שמח הכי מהיר בעולם", "בלחיצה אחת במייסטייט 🚀 - שגרו ברכה בלי להקליד. הקליקו לעדכון", "http://bitly.com/pesachpush", item.Select(s => s.gcmregistrationid));
            }

            var newVersionList = DB.GetIsraelisWithNewVersionForPush();
            var newlistOfLists = newVersionList.Chunk(800);
            foreach (var item in newlistOfLists)
            {
                PushSender.SendSimpleMessageWithUrl("חג שמח הכי מהיר בעולם", "בלחיצה אחת במייסטייט 🚀 - שגרו ברכה בלי להקליד. נסו עכשיו!", "http://bit.ly/22Ogh8M", item.Select(s => s.gcmregistrationid));
            }

             //string hagai = "APA91bGHPDa_EiFn8p9OWESH48mDis4fenU3eZcFQpLaAtNT6MPYadro-DHlNpb3IIyLIyyoU54OiGTXHFFPEOdqFN43PeVh9VteueK6nEc5nwsiOh5DAzqN_w_0-L-gu_5nxTITYZ3z";
             //           string eido = "APA91bEOvSVOkzwvkG_ymCmiN7Tp3iFQgnyYYrIND1VGM_EjyQGQphV7FIr4On_jXfBdM085BQ1DCZWuFY1Dnz4opPqULkXR6tzQ4j1bD0BdH8mgumlPLsyQkIx0zHSu7ZaeYmQ6C2Xi";
             //           string assaf = "APA91bF-AQ0E3jaJu3MlAiByAVlNueQV-KVx4Dg8wNe4xXeCFiVTRF_aUQBSW_bmxl__iTFoMrbgN9ANF-pKojd3JGoGi329Eoos_J7ICufcPiGB1Oadaxr-fJV-OSolFDOyrn-usDdN";
             //           string michael = "APA91bFGdXbMaNMBsfw8awmQXgOPsFqFguIIBZw0abdc3kmkfS7qm3zHqm8fLwMVoXUcZHHd2pvEbCxk1vaaDBg0tIZ5pKoWgxk19NE5FspGhSKxQvYmcxtVEQ32HNPLTvSGA2biT0vP";
             //           string sveta = "APA91bG8ichrQRx3QIX2pRrwASy9gJOay9NQ-HUuCDQNG9y1Ynhv_F52ud5qrS1oLCUHyD4wPrK-uQgrX1o67tsVjDzU32nJEhuasOe8uVFQ9KJSKZmr9tgA9E40OMwat0qApv4Lon_N";



             //           PushSender.SendSimpleMessageWithUrl("חג שמח הכי מהיר בעולם", "בלחיצה אחת במייסטייט 🚀 - שגרו ברכה בלי להקליד. הקליקו לעדכון", "http://bitly.com/pesachpush", new List<string> { hagai,eido,assaf,michael,sveta});
             //           PushSender.SendSimpleMessageWithUrl("חג שמח הכי מהיר בעולם", "בלחיצה אחת במייסטייט 🚀 - שגרו ברכה בלי להקליד. נסו עכשיו!", "http://bit.ly/22Ogh8M", new List<string> { hagai, eido, assaf, michael, sveta });
        }
    }

    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

    }
}
