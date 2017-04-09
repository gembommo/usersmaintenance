using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonInterfaces
{
    public interface IDbCompleteDataStore
    {
        Task InsertLog(Log log);
        void SaveBadWords(HashSet<string> words);
        void SaveBadWord(string word);
        List<SearchableWord> LoadBadWords();
        Task<string> GetTempTableValue(string key);
        Task SetTempTableValue(int key, string value);
    }

    public class EmptyDb: IDbCompleteDataStore
    {
        public Task InsertLog(Log log)
        {
            return Task.CompletedTask;
        }

        public void SaveBadWords(HashSet<string> words)
        {
        }

        public void SaveBadWord(string word)
        {
        }

        public List<SearchableWord> LoadBadWords()
        {
            return new List<SearchableWord>();
        }

        public Task<string> GetTempTableValue(string key)
        {
            return Task.FromResult("");
        }

        public Task SetTempTableValue(int key, string value)
        {
            return Task.CompletedTask;
        }
    }
}