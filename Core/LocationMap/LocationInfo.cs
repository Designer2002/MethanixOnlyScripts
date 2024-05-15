using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LOCATIONS
{
    public class LocationInfo : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Image Panel;
        [SerializeField]
        private TextMeshProUGUI Name;
        [SerializeField]
        private TextMeshProUGUI Description;
        [SerializeField]
        private TextMeshProUGUI LocationStatus;
        [SerializeField]
        private UnityEngine.UI.Button Teleport;
        [SerializeField]
        private CanvasGroup Canvas;
        [SerializeField]
        private UnityEngine.UI.Button Close;

        [SerializeField]
        private UnityEngine.UI.Image Blackout;

        private DIALOGUE.CanvasGroupController cg = null;

        public static LocationInfo instance;

        private string ReceivedFrom;

        private Sprite here, opened, closed, unaccessable;

        public void SetReceiver(string receiver)
        {
            ReceivedFrom = receiver;
        }

        void Start()
        {
            cg = new DIALOGUE.CanvasGroupController(this, Canvas);
            cg.Hide();
            cg.SetInteractableState(false);
            here = Resources.Load<Sprite>($"{FilePaths.LocationStatus}Here");
            opened = Resources.Load<Sprite>($"{FilePaths.LocationStatus}Opened");
            closed = Resources.Load<Sprite>($"{FilePaths.LocationStatus}Closed");
            unaccessable = Resources.Load<Sprite>($"{FilePaths.LocationStatus}Unaccessable");
        }
        private void Awake()
        {
            instance = this;
        }
        public void Show()
        {
            Blackout.color = new Color32(0, 0, 0, 150);
            LocationExpander.instance.Expander.enabled = false;
            Panel.color = PanelDesigner.instance.GetBrightColor();

            Location location = LocationManager.instance.GetLocation(ReceivedFrom);
            if (location == null) return;

            Name.text = location.Name;
            Description.text = location.Description;
            switch(location.Status)
            {
                case Location.OpenStatus.Here:
                    Teleport.GetComponent<UnityEngine.UI.Image>().sprite = here;
                    LocationStatus.color = new Color32(66, 66, 66, 255);
                    LocationStatus.text = "ты уже здесь";
                    break;
                case Location.OpenStatus.Opened:
                    Teleport.GetComponent<UnityEngine.UI.Image>().sprite = opened;
                    LocationStatus.color = new Color32(213, 255, 182, 255);
                    LocationStatus.text = "сюда можно попасть прямым путём";
                    break;
                case Location.OpenStatus.Closed:
                    Teleport.GetComponent<UnityEngine.UI.Image>().sprite = closed;
                    LocationStatus.color = new Color32(255, 182, 191, 255);
                    LocationStatus.text = "эта локация не соседняя, пройти нельзя";
                    break;
                case Location.OpenStatus.Unaccessable:
                    Teleport.GetComponent<UnityEngine.UI.Image>().sprite = unaccessable;
                    LocationStatus.color = new Color32(66, 66, 66, 255);
                    LocationStatus.text = "здесь тебе делать нечего.";
                    break;
            }
            Teleport.enabled = location.Status == Location.OpenStatus.Opened;

            cg.Show();
            cg.SetInteractableState(true);

            LocationExpander.instance.cg.MakeFaded();


        }

        public void ToggleClose()
        {
            Hide();
            
        }

        public void Hide()
        {
            Blackout.color = new Color32(0, 0, 0, 0);
            cg.Hide();
            cg.SetInteractableState(false);
            LocationExpander.instance.cg.Show();
            LocationExpander.instance.Expander.enabled = true;
        }

        public void ToggleTeleport()
        {
            Debug.Log(ReceivedFrom);
            Debug.Log(LocationManager.instance.currentLocation);
            StartCoroutine(LocationManager.instance.Teleport(LocationManager.instance.GetLocation(ReceivedFrom).CodeWord.ToLower(), is_teleporting_by_button: true));
            Debug.Log(LocationManager.instance.currentLocation);
        }
    }
}