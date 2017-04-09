using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using CommonInterfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace CommonTools
{
    public class ModelConverter
    {
        public static List<IContactDetails> GetContactDetailsesList(List<ContactDetailsEntity> nextBatch)
        {
            return nextBatch.Select(x =>
            {
                IContactDetails item = new ContactDetails();
                x.ConvertTo(ref item);
                return item;
            }).ToList();
        }
    }
}
