namespace CustomRegex
{
    public class BinaryTree<T>
    {
        public Node<T> Root { get; }

        public BinaryTree(Node<T> value)
        {
            Root = value;
        }
    }

    public class Node<T>
    {
        public T Value { get; }

        public Node<T> Left { get; set; }

        public Node<T> Right { get; set; }

        public Node(T value)
        {
            Value = value;
        }
    }
}
