using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class DialogueSystem : MonoBehaviour
    {
        public CHARACTERS.CharacterConfigSO characterConfigurationAsset;
        public Color defultColor = Color.white;
        public TMPro.TMP_FontAsset Font;

        [SerializeField]
        private CanvasGroup MainCanvas;

        [SerializeField]
        private DialogueSystemConfigurationSO _config;

        [SerializeField]
        private UnityEngine.UI.Button PanelButtonReader;
        public DialogueSystemConfigurationSO config => _config;

        public delegate void DialogueSystemEvent();
        public event DialogueSystemEvent onUserPrompt_Next;
        public event DialogueSystemEvent onClear;
        public DialogueContainer DialogueContainer;
        public ConversationManager conversationManager { get; private set; }
        public bool isRunningConversation => conversationManager.isRunning;
        public TextArchitect architect { get; private set; }
        public AutoReader reader { get; private set; }
        public static DialogueSystem instance { get; private set; }
        public DialogueContinuationPrompt DialogueContinuationPrompt;
        private CanvasGroupController controller;

        public bool isVisible => controller.is_Visible;

        private bool is_lifted;
        // Start is called before the first frame update

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else
            {
                DestroyImmediate(gameObject);
            }
            
        }

        bool _initialized = false;
        public void OnUserPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();
            if (reader != null && reader.is_on) reader.Disable();
        }
        public void OnSystemPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();
        }

        public void OnSystemPrompt_Clear()
        {
            onClear?.Invoke();
        }

        public void ApplySpeakerDataToDialogueContainer(string speakerName)
        {
            CHARACTERS.Character character = CHARACTERS.CharacterManager.instance.GetCharacter(speakerName);
            CHARACTERS.CharacterConfigData config = character != null ? character.config : CHARACTERS.CharacterManager.instance.GetCharacterConfig(speakerName);

            ApplySpeakerDataToDialogueContainer(config);
        }
        public void ApplySpeakerDataToDialogueContainer(CHARACTERS.CharacterConfigData config)
        {
            DialogueContainer.SetDialogueColor(config.dialogueColor);
            DialogueContainer.SetDialogueFont(config.dialogueFont);
            //DialogueContainer.SetDialogueFontSize(config.dialoguefontSize);

            DialogueContainer.nameContainer.SetNameColor(config.nameColor);
            DialogueContainer.nameContainer.SetNameFont(config.nameFont);
            //DialogueContainer.nameContainer.SetNameFontSize(config.namefontSize);
        }

        private void Initialize()
        {
            if (_initialized) return;
            
            architect = new TextArchitect(DialogueContainer.dialogueText);
            conversationManager = new ConversationManager(architect);
            DialogueContainer.Initialize();
            controller = new CanvasGroupController(this, MainCanvas);
            reader = GetComponent<AutoReader>();
            if (reader != null) reader.Initialize(conversationManager);
            _initialized = true;
        }

        public void ShowName(string speakerName = "")
        {
            if (speakerName == "narrator" || speakerName == "n" || speakerName == "")
            {
                HideSpeakerName();
                LiftText();
            }

            else
            {
                DialogueContainer.nameContainer.Show(speakerName);
                DropText();
                
            }
        }
        public void ShowImage(string speakerName = "")
        {
            if (speakerName == "narrator" || speakerName == "n" || speakerName == "" || speakerName == " ")
            {
                HideSideImage();
            }

            else
            {
                DialogueContainer.SideImage.Show(speakerName);
            }
        }

        public void LiftText()
        {
            if (!is_lifted)
            {
                DialogueContainer.dialogueText.transform.position = new Vector3(DialogueContainer.dialogueText.transform.position.x, DialogueContainer.dialogueText.transform.position.y + 12, DialogueContainer.dialogueText.transform.position.z);
                is_lifted = true;
            }
        }

        public void DropText()
        {
            if (is_lifted)
            {
                DialogueContainer.dialogueText.transform.position = new Vector3(DialogueContainer.dialogueText.transform.position.x, DialogueContainer.dialogueText.transform.position.y - 12, DialogueContainer.dialogueText.transform.position.z);
                is_lifted = false;
            }
        }

        public void HideSpeakerName()
        {
            DialogueContainer.nameContainer.Hide();
            
        }
        public void HideSideImage() => DialogueContainer.SideImage.Hide();
        public Coroutine Say(string speaker, string dialogue)
        {
            List<string> conversation = new List<string> { $"{speaker} \"{dialogue}\"" };
            return Say(conversation);
        }
        public Coroutine Say(List<string> conv, string filePath = "")
        {
            Conversation conversation = new Conversation(conv, file:filePath);
            return conversationManager.StartConverstaion(conversation);
        }

        public Coroutine Say(Conversation conversation)
        {
            return conversationManager.StartConverstaion(conversation);
        }

        public Coroutine show()
        {
            return controller.Show();
        }

        public Coroutine hide()
        {
            return controller.Hide();
        }

        public void ShowNotification(string message)
        {
            NotificationPanel.instance.Display(message);
        }

        public void EnableGlobalReading()
        {
            PanelButtonReader.enabled = true;
            reader.EnableButtonsAndStopItsAction();
        }
        public void DisableGlobalReading()
        {
            PanelButtonReader.enabled = false;
            reader.DisableButtonsAndStopItsAction();
            
        }
    }
}