using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOCATIONS
{
    public class LocationGoal
    {
        public int AvailibleMoves;
        public int PlayerMoves { get; private set; }
        public bool isAgile;
        public bool IsFinished => !isFailed && targetLocation == currentLocation;
        public bool isFailed => PlayerMoves > AvailibleMoves;

        private bool pause => LocationManager.instance.Status == LocationManager.ConversationStatus.Process || LocationManager.instance.Status == LocationManager.ConversationStatus.Top;

        public string targetLocation;
        public string currentLocation => LocationManager.instance.currentLocation;

        public LocationGoal()
        {
            VariableStore.TrySetValue("overmove", false);
            DIALOGUE.DialogueSystem.instance.DisableGlobalReading();
            LocationExpander.instance.Expander.interactable = true;
        }

        public void Move()
        {
            PlayerMoves++;
        }
        public void FailGoal()
        {
            if (PlayerMoves <= AvailibleMoves) return;
            
            VariableStore.TrySetValue("overmove", true);
            if (!isAgile || (isAgile && PlayerMoves >= AvailibleMoves && currentLocation == targetLocation))
            {
                LocationManager.instance.KillGoal();
                DIALOGUE.DialogueSystem.instance.DialogueContainer.show();
                DIALOGUE.DialogueSystem.instance.EnableGlobalReading();
                LocationExpander.instance.Expander.interactable = false;
            }
            
            
            Debug.Log("goal failed");
        }
        
        public IEnumerator Pause(bool evaluate)
        {
            while (pause && evaluate)
            {
                DIALOGUE.DialogueSystem.instance.DialogueContainer.show();
                DIALOGUE.DialogueSystem.instance.EnableGlobalReading();
                LocationExpander.instance.Expander.interactable = false;
                yield return null;
            }
            
            yield return Continue();
        }

        public void FinishGoal()
        {
            if (targetLocation != currentLocation) return;
            VariableStore.TrySetValue("overmove", false);
            LocationManager.instance.KillGoal();
            DIALOGUE.DialogueSystem.instance.DialogueContainer.show();
            DIALOGUE.DialogueSystem.instance.EnableGlobalReading();
            LocationExpander.instance.Expander.interactable = false;
           
            
            Debug.Log("goal finished");
        }

        public IEnumerator Continue()
        {
            LocationExpander.instance.Expander.interactable = true;
            LOCATIONS.LocationManager.instance.Status = LocationManager.ConversationStatus.Process;
            yield return null;
        }
    }
}