using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonInterfaces
{
    public interface IDbCompleteDataStore
    {
        Task InsertLog(Log log);
        List<string> LoadBadWords();
        Task<string> GetTempTableValue(string key);
        Task SetTempTableValue(string value, string key);
    }
}