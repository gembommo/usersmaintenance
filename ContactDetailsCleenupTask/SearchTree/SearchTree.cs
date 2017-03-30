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

        public Node PrefixWithPossibleSkip(string s, int skip)
        {
            var currentNode = _root;
            var result = currentNode;
            int missedDepthSteps = 0;

            foreach (var c in s)
            {
                for (int i = 0; i < skip; i++)
                {
                    currentNode = currentNode.FindChildNode(c);
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

        public bool SearchWithPossibleSkip(string s, int skip)
        {
            var prefix = PrefixWithPossibleSkip(s, skip);
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
