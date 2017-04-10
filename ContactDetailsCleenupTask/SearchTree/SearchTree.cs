using System.Collections.Generic;

namespace ContactDetailsCleenupTask.SearchTree
{
    public class SearchTree
    {
        private readonly Node _root;

        public SearchTree()
        {
            _root = new Node('^', 0, null);
        }

        public Node Prefix(string s)
        {
            var currentNode = _root;
            var result = currentNode;

            foreach (var c in s)
            {
                currentNode = currentNode.FindChildNode(c);
                if (currentNode == null)
                    break;
                result = currentNode;
            }

            return result;
        }

        public Node Prefix(char[] cArray, int startAt)
        {
            var currentNode = _root;
            var result = currentNode;

            for (var index = startAt; index < cArray.Length; index++)
            {
                var c = cArray[index];
                currentNode = currentNode.FindChildNode(c);
                if (currentNode == null)
                    break;
                result = currentNode;
            }

            return result;
        }

        public Node PrefixWithPossibleSkip(string s, int skip, bool isCaseSensitive)
        {
            var currentNode = _root;
            var result = currentNode;
            int missedDepthSteps = 0;

            foreach (var c in s)
            {
                for (int i = 0; i < skip; i++)
                {
                    currentNode = currentNode.FindChildNode(c, isCaseSensitive);
                    if (currentNode != null)
                        break;
                    missedDepthSteps++;
                }
                if (currentNode == null)
                    break;

                result = currentNode;

            }

            return result;
        }

        public bool Search(string s)
        {
            var prefix = Prefix(s);
            return prefix.Depth == s.Length && prefix.FindChildNode('$') != null;
        }

        public bool SearchInText(string s)
        {
            var cArray = s.ToCharArray();
            for (int i = 0; i < cArray.Length; i++)
            {
                var prefix = Prefix(cArray, i);
                var result = prefix.FindChildNode('$') != null;
                if (result)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// This function doesn't work properly. 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="skip"></param>
        /// <param name="isCaseSensitive"></param>
        /// <returns></returns>
        [System.Obsolete("This function doesn't work properly yet")]
        public bool SearchWithPossibleSkip(string s, int skip, bool isCaseSensitive)
        {
            var prefix = PrefixWithPossibleSkip(s, skip, isCaseSensitive);
            return prefix.Depth == s.Length && prefix.FindChildNode('$') != null;
        }

        public void InsertRange(List<string> items)
        {
            for (int i = 0; i < items.Count; i++)
                Insert(items[i]);
        }

        public void Insert(string s)
        {
            var commonPrefix = Prefix(s);
            var current = commonPrefix;

            for (var i = current.Depth; i < s.Length; i++)
            {
                var newNode = new Node(s[i], current.Depth + 1, current);
                current.Children.Add(newNode);
                current = newNode;
            }

            current.Children.Add(new Node('$', current.Depth + 1, current));
        }

        public void Delete(string s)
        {
            if (Search(s))
            {
                var node = Prefix(s).FindChildNode('$');

                while (node.IsLeaf())
                {
                    var parent = node.Parent;
                    parent.DeleteChildNode(node.Value);
                    node = parent;
                }
            }
        }

    }
}
