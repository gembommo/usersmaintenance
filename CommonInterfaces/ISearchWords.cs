using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonInterfaces
{
    public interface ISearchWords
    {
        void InsertRange(List<SearchableWord> loadBadWords);
        bool SearchWord(string item);
    }

    public class SearchableWord
    {
        public string Word { get; set; }
        public SearchableWordType WordType { get; set; }

        public enum SearchableWordType
        {
            Regular,
            Expandable,
        }
    }
}
