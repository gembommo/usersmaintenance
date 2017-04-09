using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonInterfaces;

namespace ContactDetailsCleenupTask.BadWordsFilters
{
    public class SearchWordsList : List<ISearchWords>
    {
        public void InsertWords(List<SearchableWord> wordsList)
        {
            ForEach(x=>x.InsertRange(wordsList));
        }

        public bool SearchWord(string item)
        {
            foreach (ISearchWords searchWords in this)
            {
                if (searchWords.SearchWord(item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
