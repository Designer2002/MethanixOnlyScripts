using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MISSIONS
{
    public class MissionExpander : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup Canvas;
        [SerializeField]
        public Button Expander;
        [SerializeField]
        TMPro.TextMeshProUGUI Counter;
        [SerializeField]
        GameObject No;
        [SerializeField]
        GameObject Panel;

        private bool isExpanded = false;

        private DIALOGUE.CanvasGroupController cg = null;
        public static MissionExpander instance;
        // Start is called before the first frame update
        void Start()
        {
            cg = new DIALOGUE.CanvasGroupController(this, Canvas);
            cg.alpha = 0;
            cg.SetInteractableState(false);
            No.SetActive(false);
            Panel.SetActive(false);
        }
        public void Show()
        {
            isExpanded = true;
            cg.Show();
            cg.SetInteractableState(true);
            No.SetActive(MissionManager.instance.unlocked == 0);
            Panel.SetActive(MissionPanel.instance.count < int.Parse(Counter.text));
            //Debug.Log(MissionManager.instance.unlocked);
            if (MissionManager.instance.unlocked > 0)
            {
                MissionPanel.instance.Show();
            }
        }

        public void UpdateCounter() => Counter.text = MissionManager.instance.unlocked.ToString();

        public void ToggleExpand()
        {
            if (!isExpanded) Show();
            else
            {
                Hide();
                
            }
        }

        public void HandleCounter()
        {
            Counter.text = MissionManager.instance.count.ToString();
        }

        public void Hide()
        {
            isExpanded = false;
            cg.Hide();
            cg.SetInteractableState(false);
        }
        private void Awake()
        {
            instance = this;
        }

    }
}