using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class VNConversationData
    {
        public int progress;
        public List<string> conversation = new List<string>();
    }
}
