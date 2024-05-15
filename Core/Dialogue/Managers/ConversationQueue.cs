using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class ConversationQueue
    {
        private Queue<Conversation> convQueue = new Queue<Conversation>();
        public Conversation top => convQueue.Peek();

        public void Enqueue(Conversation conversation) => convQueue.Enqueue(conversation);
        public void EnqueuePriority(Conversation conversation)
        {
            Queue<Conversation> tmp = new Queue<Conversation>();
            tmp.Enqueue(conversation);
            while(convQueue.Count > 0)
            {
                tmp.Enqueue(convQueue.Dequeue());
            }
            convQueue = tmp;
        }
        public void Dequeue()
        {
            if (convQueue.Count > 0) convQueue.Dequeue();
        }
        public bool isEmpty() => convQueue.Count == 0;

        public void Clear()
        {
            convQueue.Clear();
        }

        public Conversation[] GetReadOnly() => convQueue.ToArray();
    }
}