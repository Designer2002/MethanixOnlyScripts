using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace DIALOGUE
{
    public class PlayerInputManager : MonoBehaviour
    {
        private PlayerInput input;
        private List<(InputAction action, Action<InputAction.CallbackContext> command)> actions = new List<(InputAction action, Action<InputAction.CallbackContext> command)>();
        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            InitaializeActions();
        }

        private void InitaializeActions()
        {
            actions.Add((input.actions["Next"], PromptAdvance));
        }

        private void OnEnable()
        {
            foreach (var InputAction in actions)
            {
                InputAction.action.performed += InputAction.command;
            }
        }

        private void OnDisable()
        {
            foreach (var InputAction in actions)
            {
                InputAction.action.performed -= InputAction.command;
            }
        }

        public void PromptAdvance(InputAction.CallbackContext c)
        {
            DialogueSystem.instance.OnUserPrompt_Next();
        }
    }
}