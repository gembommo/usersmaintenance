using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorage
{
    public abstract class MyStateTableEntity : TableEntity
    {
        public static readonly Regex DisallowedCharsInTableKeys = new Regex(@"[\\\\#%+/?\u0000-\u001F\u007F-\u009F]");
        public string SystemKey { get; set; }
        public abstract string GetTableName {get;}
    }
}