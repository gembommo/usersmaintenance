using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonInterfaces;

namespace ContactDetailsCleenupTask
{
    public class SearchWordsByRegex: ISearchWords
    {
        private List<Regex> _regexList;
        public void InsertRange(List<SearchableWord> badWords)
        {
            var loadBadWords = badWords.Where(x => x.WordType == SearchableWord.SearchableWordType.Expandable)
                .Select(x => x.Word)
                .ToList();
            _regexList = new List<Regex>(badWords.Count);
            string seperatorString = "(.){0,1}";
            foreach (var badWord in loadBadWords)
            {
                var regexString = String.Join(seperatorString, badWord.ToCharArray());
                _regexList.Add(new Regex(regexString, RegexOptions.IgnoreCase));
            }
        }

        public bool SearchWord(string item)
        {
            if (string.IsNullOrEmpty(item))
                return false;
            foreach (var regex in _regexList)
            {
                var match = regex.Match(item);
                if (match.Success)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class SearchWordsThatStartsWith : ISearchWords
    {
        private string v;

        public SearchWordsThatStartsWith(string v)
        {
            this.v = v;
        }

        public bool SearchWord(string item)
        {
            return !string.IsNullOrEmpty(item) && item.StartsWith(v);
        }

        public void InsertRange(List<SearchableWord> loadBadWords)
        {
            
        }
    }

    public class SearchPossiblePasswords : ISearchWords
    {
        private Regex regex;
        public void InsertRange(List<SearchableWord> loadBadWords)
        {
            var pattern = "[0-9]{3}";
            regex = new Regex(pattern, RegexOptions.IgnoreCase);

        }

        public bool SearchWord(string item)
        {
            MatchCollection matches = regex.Matches(item);
            return matches.Count > 0;
        }

        
    }

}
