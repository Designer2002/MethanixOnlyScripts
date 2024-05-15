using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LOCATIONS
{
    public class LocationExpander : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup Canvas;
        [SerializeField]
        public Button Expander;
        [SerializeField]
        private GameObject BottomPanel;

        private bool isExpanded = false;

        public DIALOGUE.CanvasGroupController cg = null;

        public static LocationExpander instance;
        // Start is called before the first frame update
        void Start()
        {
            this.Expander.interactable = LocationManager.instance.goal != null;
            cg = new DIALOGUE.CanvasGroupController(this, Canvas);
            cg.alpha = 0;
            cg.SetInteractableState(false);
        }

        public void Show()
        {
            isExpanded = true;
            cg.Show();
            cg.SetInteractableState(true);
            DIALOGUE.DialogueSystem.instance.DialogueContainer.hide();
            foreach (var b in BottomPanel.GetComponentsInChildren<Button>())
                b.enabled = false;
        }

        public void Hide()
        {
            isExpanded = false;
            cg.Hide();
            if (LocationManager.instance.goal == null) DIALOGUE.DialogueSystem.instance.DialogueContainer.show();
            cg.SetInteractableState(false);
            foreach (var b in BottomPanel.GetComponentsInChildren<Button>())
                b.enabled = true;
        }

        public void ToggleExpand()
        {
            if (!isExpanded) Show();
            else
            {
                Hide();

            }
        }

        // Update is called once per frame
        void Awake()
        {
            instance = this;
        }
    }
}