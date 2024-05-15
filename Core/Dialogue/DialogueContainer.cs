using System.Collections;
using TMPro;
using UnityEngine;

namespace DIALOGUE

{
    [System.Serializable]
    public class DialogueContainer
    {
      
        public GameObject root;
        public NameContainer nameContainer;
        public TextMeshProUGUI dialogueText;
        public SideImageContainer SideImage;

        private CanvasGroupController controller;

        private bool imitialized = false;

        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;

        public void SetDialogueFontSize(float size) => dialogueText.fontSize = size;

        public void Initialize()
        {
            if (imitialized) return;
            controller = new CanvasGroupController(DialogueSystem.instance, root.GetComponent<CanvasGroup>());
        }

        public bool isVisible => controller.is_Visible;

        public Coroutine show() => controller.Show();
        public Coroutine hide() => controller.Hide();
    }
}