using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

namespace DIALOGUE
{
    public class AutoReader : MonoBehaviour
    {
        private const int DEFAULT_CHARACTERS_READ_PER_SECOND = 18;
        private const float READ_TIME = 0.5f;
        private const float MIN_TIME = 1f;
        private const float MAX_TIME = 99f;
        [SerializeField]
        private Button autoButton;
        [SerializeField]
        private Button skipButton;
        private ConversationManager ConversationManager;
        private TextArchitect Architect => ConversationManager.architect;
        public bool skip { get; set; } = true;
        public float speed { get; set; } = 1;

        private Coroutine co_running = null;
        public bool is_on => co_running != null;

        public void Initialize(ConversationManager manager)
        {
            this.ConversationManager = manager;
        }

        private void Enable()
        {
            if (is_on)
            {
                Debug.Log("useless");
                return;
            }
            co_running = StartCoroutine(AutoRead());
            LOCATIONS.LocationExpander.instance.Expander.interactable = false;
        }

        public void Disable()
        {
            if (!is_on) return;
            StopCoroutine(co_running);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            co_running = null;
            skip = false;
            autoButton.enabled = true;
            skipButton.enabled = true;
            LOCATIONS.LocationExpander.instance.Expander.interactable = LOCATIONS.LocationManager.instance.goal != null; ;
        }

        private IEnumerator AutoRead()
        {
            if (!ConversationManager.isRunning)
            {
                Disable();
                yield break;
            }
            if (!Architect.isBuilding && Architect.current_text != string.Empty)
                DialogueSystem.instance.OnSystemPrompt_Next();
            while(ConversationManager.isRunning)
            {
                if(!skip)
                {
                    while(!Architect.isBuilding && !ConversationManager.isWaitingOnAutoTimer)
                    {
                        yield return null;
                    }
                    float timeStart = Time.time;
                    while(Architect.isBuilding || ConversationManager.isWaitingOnAutoTimer)
                    {
                        yield return null;
                    }
                    float timeToRead = Mathf.Clamp((float)Architect.tmpro.textInfo.characterCount/DEFAULT_CHARACTERS_READ_PER_SECOND, MIN_TIME, MAX_TIME);
                    timeToRead = Mathf.Clamp(timeToRead - (Time.time - timeStart), MIN_TIME, MAX_TIME);
                    timeToRead = timeToRead / speed + READ_TIME;
                    yield return new WaitForSeconds(timeToRead);
                }
                else
                {
                    Architect.ForceComplete();
                    yield return new WaitForSeconds(0.05f);
                }
                DialogueSystem.instance.OnSystemPrompt_Next();
            }
            Disable();
        }

        public void ToggleAuto()
        {
            skipButton.enabled = false;
            bool prevState = skip;
            skip = false;
            if (prevState) Enable();
            else
            {
                if (!is_on) Enable();
                else Disable();
            }
        }

        public void ToggleSkip()
        {
            autoButton.enabled = false;
            bool prevState = skip;
            skip = true;
            if (!prevState) Enable();
            else
            {
                if (!is_on) Enable();
                else Disable();
            }
            
        }

        public void DisableButtonsAndStopItsAction()
        {
            autoButton.enabled = false;
            skipButton.enabled = false;
            Disable();
        }

        public void EnableButtonsAndStopItsAction()
        {
            autoButton.enabled = true;
            skipButton.enabled = true;

        }
    }
}