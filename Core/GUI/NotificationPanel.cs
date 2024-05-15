using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DIALOGUE
{
    public class NotificationPanel : MonoBehaviour
    {
        public static NotificationPanel instance;
        [SerializeField]
        private CanvasGroup CanvasGroup;
        [SerializeField]
        private Button CloseButton;
        [SerializeField]
        private GameObject panel;
        [SerializeField]
        private VerticalLayoutGroup panelLayout;
        System.Random r = new System.Random();
        [SerializeField]
        TextMeshProUGUI Message;

        private bool IsDisplaying => co_displaying != null;
        private Coroutine co_displaying;

        private const float WIDTH = 200;
        private const float MIN_HEIGHT = 150;
        private const float BUTTON_HEIGHT_PER_LINE = 20;

        private CanvasGroupController cg = null;
        // Start is called before the first frame update
        void Start()
        {
            cg = new CanvasGroupController(this, CanvasGroup);
            cg.alpha = 0;
            cg.SetInteractableState(false);
        }

        private void Awake()
        {
            instance = this;

        }


        private void DisplayMessage(string message)
        {
            Message.text = message;
            Message.ForceMeshUpdate();
            panel.GetComponent<Image>().color = PanelDesigner.instance.GetBrightColor();
            int lines = Message.textInfo.lineCount;
            panel.GetComponent<LayoutElement>().preferredHeight = BUTTON_HEIGHT_PER_LINE * lines + MIN_HEIGHT;
            
        }

        public Coroutine Display(string message)
        {
            if (co_displaying != null)
                StopCoroutine(Displaying(message));
            co_displaying = StartCoroutine(Displaying(message));
            return co_displaying;
        }

        public IEnumerator Displaying(string message)
        {
            DisplayMessage(message);
            yield return cg.Show();
            cg.SetInteractableState(true);
            
            Debug.Log(message);
            yield return new WaitForSeconds(3);
            cg.SetInteractableState(false);
            yield return cg.Hide();
            co_displaying = null;
        }

        public void ToggleClose()
        {
            cg.Hide();
        }
    }
}