using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class SavingManager : MonoBehaviour
    {
        public static SavingManager instance { get; private set; }
        public SavingsData state;
        private void Awake()
        {
            instance = this;
        }
        public void Log()
        {
            SavingsData state = SavingsData.Capture();
            this.state = state;
        }
        public void Load()
        {
            state.Load();
        }
        private void Start()
        {
            DIALOGUE.DialogueSystem.instance.onClear += Log;
        }
    }
}