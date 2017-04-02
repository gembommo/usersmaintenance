using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonInterfaces
{
    public interface IDbCompleteDataStore
    {
        Task InsertLog(Log log);
        void SaveBadWords(HashSet<string> words);
        void SaveBadWord(string word);
        List<string> LoadBadWords();
        Task<string> GetTempTableValue(string key);
        Task SetTempTableValue(int key, string value);
    }
}