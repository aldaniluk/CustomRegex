using System.Collections.Generic;
using System.Linq;

namespace CustomRegex
{
    public class BsuRegex
    {
        public string RegularExpression { get; }

        private static char AND = '.';
        private static char OR = '|';
        private static char STAR = '*';
        private static char OPEN = '(';
        private static char CLOSE = ')';

        public BsuRegex(string regularExpression)
        {
            RegularExpression = regularExpression;
        }

        public bool IsMatch(string str)
        {
            return false;
        }

        private BinaryTree<char> GetTree()
        {
            string normalizedRegularExpression = NormalizeRegularExpression(RegularExpression);
            Node<char> root = GetTreeInternal(normalizedRegularExpression);
            var result = new BinaryTree<char>(root);

            return result;
        }

        public string NormalizeRegularExpression(string regularExpression)
        {
            var normalizedRegularExpression = new List<char>();
            char[] elements = regularExpression.ToCharArray();
            bool isSearchingForLetter = false;
            for (int i = 0; i < elements.Length; i++)
            {
                if (char.IsLetter(elements[i]) && !isSearchingForLetter && i + 1 < elements.Length &&
                    char.IsLetter(elements[i + 1]))
                {
                    normalizedRegularExpression.Add('(');
                    isSearchingForLetter = true;
                }

                if (isSearchingForLetter && !char.IsLetter(elements[i]))
                {
                    normalizedRegularExpression.Add(')');
                    isSearchingForLetter = false;
                }

                normalizedRegularExpression.Add(elements[i]);

                if (char.IsLetter(elements[i]) && i + 1 < elements.Length && char.IsLetter(elements[i + 1]))
                {
                    normalizedRegularExpression.Add('.');
                }

                if (elements[i] == STAR)
                {
                    normalizedRegularExpression.Add(')');
                    int counter = 0;

                    for (int j = i - 1; j > 0; j--)
                    {
                        if (elements[j] == OPEN)
                        {
                            counter++;
                        }
                        if (elements[j] == CLOSE)
                        {
                            counter--;
                        }
                        if (char.IsLetter(elements[j]) && counter == 0)
                        {
                            normalizedRegularExpression.Add('(');
                        }
                    }
                }
            }

            return string.Join("", normalizedRegularExpression).Replace(")(", ").(");
        }

        private Node<char> GetTreeInternal(string str)
        {
            Node<char> root = null;
            char[] elements = str.ToCharArray();
            Node<char> current = null;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] == OPEN)
                {
                    string subString = GetSubString(str, i, out int endIndex);
                    i = endIndex;
                    current = GetTreeInternal(subString);
                    LinkWithRoot(current, root);
                }

                if (elements[i] == AND || elements[i] == OR)
                {
                    var newRoot = new Node<char>(elements[i]);
                    newRoot.Left = root;
                    root = newRoot;
                }

                if (elements[i] == STAR)
                {
                    current = new Node<char>(STAR);
                    current.Left = root;
                    root = current;
                }

                if (char.IsLetter(elements[i]))
                {
                    current = new Node<char>(elements[i]);
                    LinkWithRoot(current, root);
                }
            }

            return root;
        }

        private void LinkWithRoot(Node<char> current, Node<char> root)
        {
            if (root == null)
            {
                root = current;
            }
            else
            {
                root.Right = current;
            }
        }

        private string GetSubString(string str, int startIndex, out int endIndex)
        {
            int counter = 0;
            endIndex = 0;
            char[] elements = str.ToCharArray();
            for (int i = startIndex; i < elements.Length; i++)
            {
                if (elements[i] == OPEN)
                {
                    counter++;
                }
                if (elements[i] == CLOSE)
                {
                    counter--;
                }
                if (counter == 0)
                {
                    endIndex = i;
                    break;
                }
            }

            return str.Substring(startIndex + 1, endIndex - startIndex - 1);
        }
    }
}
