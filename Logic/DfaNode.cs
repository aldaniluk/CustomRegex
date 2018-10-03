using System.Collections.Generic;

namespace Logic
{
    public class DfaNode
    {
        public List<NfaNode> NfaNodes { get; set; }

        public List<DfaNodeLink> Links { get; set; }

        public bool IsFinal { get; set; }

        public DfaNode()
        {
            NfaNodes = new List<NfaNode>();
            Links = new List<DfaNodeLink>();
            IsFinal = false;
        }
    }

    public class DfaNodeLink
    {
        public DfaNode DfaNode { get; set; }

        public char LinkValue { get; set; }

        public DfaNodeLink(DfaNode node, char value)
        {
            DfaNode = node;
            LinkValue = value;
        }
    }
}
