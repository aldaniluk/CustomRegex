using System.Collections.Generic;

namespace Logic
{
    public class NfaNode
    {
        public List<NfaNodeLink> Links { get; set; }

        public NfaNode()
        {
            Links = new List<NfaNodeLink>();
        }
    }

    public class NfaNodeLink
    {
        public NfaNode NfaNode { get; set; }

        public char LinkValue { get; set; }

        public NfaNodeLink(NfaNode node, char value)
        {
            NfaNode = node;
            LinkValue = value;
        }
    }
}
