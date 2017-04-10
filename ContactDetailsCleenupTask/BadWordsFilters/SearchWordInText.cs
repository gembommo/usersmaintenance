using System.Collections.Generic;
using System.Linq;
using CommonInterfaces;

namespace ContactDetailsCleenupTask.BadWordsFilters
{
    class SearchWordInText : ISearchWords
    {
        SearchTree.SearchTree _searchTree = new SearchTree.SearchTree();
        public void InsertRange(List<SearchableWord> loadBadWords)
        {
            _searchTree.InsertRange(
                loadBadWords.Where(x => x.WordType == SearchableWord.SearchableWordType.Regular)
                    .Select(x => x.Word)
                    .ToList());
        }

        public bool SearchWord(string item)
        {
            return _searchTree.SearchInText(item);
        }
    }
}