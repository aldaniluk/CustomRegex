using Logic;
using System;
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
        private static char EPS = 'э';

        public BsuRegex(string regularExpression)
        {
            RegularExpression = regularExpression;
        }

        public bool IsMatch(string str)
        {
            var tree = GetTree();
            var nfa = GetNfa(tree);
            var dfa = GetDfa(nfa);

            return IsPassedStringThroughDfa(str, dfa);
        }

        #region Get tree
        public BinaryTree<char> GetTree()
        {
            string normalizedRegularExpression = NormalizeRegularExpression(RegularExpression);
            Node<char> root = GetTreeInternal(normalizedRegularExpression);

            return new BinaryTree<char>(root);
        }

        #region RE Normalization
        private string NormalizeRegularExpression(string regularExpression)
        {
            List<char> reWithGroupedStars = GroupBySTAR(regularExpression.ToList());
            List<char> reSeparatedByAND = SeparateByAND(reWithGroupedStars);

            return string.Join("", reSeparatedByAND).Replace(")(", ").(");
        }

        private List<char> GroupBySTAR(List<char> elements)
        {
            var reWithGroupedStars = new List<char>();

            for (int i = 0; i < elements.Count; i++)
            {
                reWithGroupedStars.Add(elements[i]);

                if (elements[i] == STAR)
                {
                    reWithGroupedStars.Add(')');
                    int counter = 0;

                    for (int j = reWithGroupedStars.Count - 3; j >= 0; j--)
                    {
                        if (reWithGroupedStars[j] == OPEN)
                        {
                            counter++;
                        }
                        if (reWithGroupedStars[j] == CLOSE)
                        {
                            counter--;
                        }
                        if (counter == 0)
                        {
                            reWithGroupedStars.Insert(j, '(');
                            break;
                        }
                    }
                }
            }

            return reWithGroupedStars;
        }

        private List<char> SeparateByAND(List<char> elements)
        {
            var reSeparatedByAND = new List<char>();

            bool isSearchingForLetter = false;
            for (int i = 0; i < elements.Count; i++)
            {
                if (char.IsLetter(elements[i]) && !isSearchingForLetter)
                {
                    reSeparatedByAND.Add('(');
                    isSearchingForLetter = true;
                }

                if (isSearchingForLetter && !char.IsLetter(elements[i]))
                {
                    reSeparatedByAND.Add(')');
                    isSearchingForLetter = false;
                }

                reSeparatedByAND.Add(elements[i]);

                if (char.IsLetter(elements[i]) && i + 1 < elements.Count && char.IsLetter(elements[i + 1]))
                {
                    reSeparatedByAND.Add('.');
                }
            }

            if (isSearchingForLetter)
            {
                reSeparatedByAND.Add(')');
            }

            return reSeparatedByAND;
        }
        #endregion

        private Node<char> GetTreeInternal(string regularExpression)
        {
            Node<char> root = null;
            char[] elements = regularExpression.ToCharArray();

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] == OPEN)
                {
                    string subString = GetSubString(elements, i, out int endIndex);
                    i = endIndex;
                    Node<char> current = GetTreeInternal(subString);
                    LinkWithRoot(current, ref root);
                }
                else if (elements[i] == AND || elements[i] == OR)
                {
                    var newRoot = new Node<char>(elements[i]);
                    newRoot.Left = root;
                    root = newRoot;
                }
                else if (elements[i] == STAR)
                {
                    Node<char> current = new Node<char>(STAR);
                    current.Left = root;
                    root = current;
                }
                else
                {
                    Node<char> current = new Node<char>(elements[i]);
                    LinkWithRoot(current, ref root);
                }
            }

            return root;
        }

        private void LinkWithRoot(Node<char> current, ref Node<char> root)
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

        private string GetSubString(char[] elements, int startIndex, out int endIndex)
        {
            int counter = 0;
            endIndex = 0;
            for (int i = startIndex; i < elements.Length; i++)
            {
                if (elements[i] == OPEN)
                {
                    counter++;
                }
                else if (elements[i] == CLOSE)
                {
                    counter--;
                }

                if (counter == 0)
                {
                    endIndex = i;
                    break;
                }
            }

            return string.Join("", elements).Substring(startIndex + 1, endIndex - startIndex - 1);
        }
        #endregion

        #region Get NFA
        public NfaNode GetNfa(BinaryTree<char> tree)
        {
            return GetNfaInternal(tree.Root);
        }

        private NfaNode GetNfaInternal(Node<char> node)
        {
            NfaNode left = node.Left != null ? GetNfaInternal(node.Left) : null;
            NfaNode right = node.Right != null ? GetNfaInternal(node.Right) : null;

            if (node.Value == AND)
            {
                return GetNfaNodesWithAnd(left, right);
            }
            else if (node.Value == OR)
            {
                return GetNfaNodesWithOr(left, right);
            }
            else if (node.Value == STAR)
            {
                return GetNfaNodeWithStar(left);
            }
            else
            {
                var first = new NfaNode();
                first.Links.Add(new NfaNodeLink(new NfaNode(), node.Value));

                return first;
            }
        }

        private NfaNode GetNfaNodesWithAnd(NfaNode left, NfaNode right)
        {
            GetLastNfaNode(left).Links.Add(new NfaNodeLink(right, EPS));

            return left;
        }

        private NfaNode GetNfaNodesWithOr(NfaNode left, NfaNode right)
        {
            var firstNfaNode = new NfaNode();
            firstNfaNode.Links.Add(new NfaNodeLink(left, EPS));
            firstNfaNode.Links.Add(new NfaNodeLink(right, EPS));

            var lastNfaNode = new NfaNode();
            GetLastNfaNode(left).Links.Add(new NfaNodeLink(lastNfaNode, EPS));
            GetLastNfaNode(right).Links.Add(new NfaNodeLink(lastNfaNode, EPS));

            return firstNfaNode;
        }

        private NfaNode GetNfaNodeWithStar(NfaNode left)
        {
            NfaNode leftLastNfaNode = GetLastNfaNode(left);
            left.Links.Add(new NfaNodeLink(leftLastNfaNode, EPS));
            leftLastNfaNode.Links.Add(new NfaNodeLink(left, EPS));

            return left;
        }

        private NfaNode GetLastNfaNode(NfaNode node, List<NfaNode> previous = null)
        {
            if (previous == null)
            {
                previous = new List<NfaNode>();
            }

            previous.Add(node);

            foreach (NfaNodeLink childNode in node.Links)
            {
                if (previous.Contains(childNode.NfaNode))
                {
                    continue;
                }

                return GetLastNfaNode(childNode.NfaNode, previous);
            }

            return node;
        }
        #endregion

        #region Get DFA
        public DfaNode GetDfa(NfaNode nfaNode)
        {
            DfaNode firstDfaNode = GetFirstDfaNode(nfaNode);
            NfaNode lastNfaNode = GetLastNfaNode(nfaNode);
            BuildDfaInternal(firstDfaNode, lastNfaNode, new List<DfaNode> { firstDfaNode });
            MarkIsFinal(firstDfaNode, nfaNode);

            return firstDfaNode;
        }

        private DfaNode GetFirstDfaNode(NfaNode nfaNode)
        {
            var dfaNode = new DfaNode();
            dfaNode.NfaNodes = GetValuesFromNfaNode(nfaNode, c => c == EPS, nfaL => nfaL.NfaNode,
                values: new List<NfaNode> { nfaNode });

            return dfaNode;
        }

        private void BuildDfaInternal(DfaNode dfaNode, NfaNode lastNfaNode, List<DfaNode> previous)
        {
            List<char> values = GetValuesFromNfaNodes(dfaNode.NfaNodes, c => c != EPS, nfaL => nfaL.LinkValue);

            foreach (char value in values)
            {
                var newDfaNode = new DfaNode();
                previous.Add(newDfaNode);
                newDfaNode.NfaNodes = GetValuesFromNfaNodes(dfaNode.NfaNodes, c => c == value,
                    nfaL => nfaL.NfaNode, addEpsCondition: true).Distinct().ToList();

                dfaNode.Links.Add(new DfaNodeLink(newDfaNode, value));

                DfaNode possibleSameDfaNode = previous
                    .FirstOrDefault(pr => pr.NfaNodes.Intersect(newDfaNode.NfaNodes).Any());
                if (possibleSameDfaNode != newDfaNode)
                {
                    dfaNode.Links.Add(new DfaNodeLink(possibleSameDfaNode, value));

                    bool containLastNfaNode = newDfaNode.NfaNodes.Contains(lastNfaNode);
                    newDfaNode.NfaNodes = newDfaNode.NfaNodes.Except(possibleSameDfaNode.NfaNodes).ToList();
                    if (containLastNfaNode)
                    {
                        newDfaNode.NfaNodes.Add(lastNfaNode);
                    }
                }

                BuildDfaInternal(newDfaNode, lastNfaNode, previous);
            }
        }

        private List<T> GetValuesFromNfaNodes<T>(List<NfaNode> nodes, Func<char, bool> matchСondition,
            Func<NfaNodeLink, T> getValue, bool addEpsCondition = false)
        {
            var values = new List<T>();
            foreach (NfaNode node in nodes)
            {
                values.AddRange(GetValuesFromNfaNode<T>(node, matchСondition, getValue, addEpsCondition));
            }

            return values;
        }

        private List<T> GetValuesFromNfaNode<T>(NfaNode node, Func<char, bool> matchСondition,
            Func<NfaNodeLink, T> getValue, bool addEpsCondition = false, List<T> values = null,
            List<NfaNode> previous = null)
        {
            if (previous == null)
            {
                previous = new List<NfaNode>();
            }
            if (values == null)
            {
                values = new List<T>();
            }

            previous.Add(node);

            foreach (NfaNodeLink childNode in node.Links)
            {
                if (matchСondition(childNode.LinkValue))
                {
                    values.Add(getValue(childNode));
                    if (addEpsCondition)
                    {
                        matchСondition += c => c == EPS;
                    }
                }
                else
                {
                    continue;
                }

                if (previous.Contains(childNode.NfaNode))
                {
                    continue;
                }

                GetValuesFromNfaNode<T>(childNode.NfaNode, matchСondition, getValue, addEpsCondition, values, previous);
            }

            return values;
        }

        private void MarkIsFinal(DfaNode firstDfaNode, NfaNode nfaNode)
        {
            NfaNode lastNfaNode = GetLastNfaNode(nfaNode);
            MarkIsFinalInternal(firstDfaNode, lastNfaNode);
        }

        private void MarkIsFinalInternal(DfaNode node, NfaNode nfaNode, List<DfaNode> previous = null)
        {
            if (previous == null)
            {
                previous = new List<DfaNode>();
            }

            previous.Add(node);

            foreach (DfaNodeLink childNode in node.Links)
            {
                if (childNode.DfaNode.NfaNodes.Contains(nfaNode))
                {
                    childNode.DfaNode.IsFinal = true;
                }

                if (previous.Contains(childNode.DfaNode))
                {
                    continue;
                }

                MarkIsFinalInternal(childNode.DfaNode, nfaNode, previous);
            }
        }
        #endregion

        private bool IsPassedStringThroughDfa(string str, DfaNode dfaNode)
        {
            if (str.Length == 0)
            {
                return dfaNode.IsFinal;
            }

            foreach (DfaNodeLink childNode in dfaNode.Links)
            {
                if (childNode.LinkValue == str.First())
                {
                    bool isMatched = IsPassedStringThroughDfa(str.Substring(1, str.Length - 1), childNode.DfaNode);
                    if (isMatched)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
