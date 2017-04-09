using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonInterfaces;

namespace ContactDetailsCleenupTask.BadWordsFilters
{
    class SearchTreeWithSkip : ISearchWords
    {
        SearchTree.SearchTree _searchTree = new SearchTree.SearchTree();
        public void InsertRange(List<SearchableWord> loadBadWords)
        {
            _searchTree.InsertRange(loadBadWords.Where(x => x.WordType == SearchableWord.SearchableWordType.Regular)
                    .Select(x => x.Word)
                    .ToList());
        }

        public bool SearchWord(string item)
        {
            return _searchTree.SearchWithPossibleSkip(item, 1, false);
        }
    }

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
            return _searchTree.Search(item);
        }
    }
}
