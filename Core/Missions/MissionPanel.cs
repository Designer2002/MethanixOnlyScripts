using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MISSIONS
{
    public class MissionPanel : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup Canvas;
        [SerializeField]
        GameObject PanelPrefab;
        [SerializeField]
        VerticalLayoutGroup Layout;

        private static List<Panel> panels = new List<Panel>();
        private const float MIN_HEIGHT = 100;
        private List<string> names => FillNames();
        private DIALOGUE.CanvasGroupController cg = null;

        public static MissionPanel instance;

        private float HeightPerLine = 50;
        private float padding = 20;
        public int count => panels.Count;

        // Start is called before the first frame update

        void Start()
        {
            cg = new DIALOGUE.CanvasGroupController(this, Canvas);
            
        }
        private void Awake()
        {
            instance = this;
        }
        public void Show()
        {
            cg.Show();
            cg.SetInteractableState(true);
            Debug.Log(panels.Count);
            if(panels.Count < names.Count) StartCoroutine(GeneratePanel());
        }

        public void Hide()
        {
            cg.Hide();
            cg.SetInteractableState(false);
        }

        private List<string> FillNames()
        {
            List<string> names = new List<string>();
            foreach(var m in MissionManager.instance.names)
            {
                names.Add(m);
            }
            return names;
        }


        public IEnumerator GeneratePanel()
        {
            for (int i = 0; i < names.Count; i++)
            {
                Mission mission = MissionManager.instance.GetMission(names[i]);
                if (mission == null) yield break;
                Panel panel;
                if (i < panels.Count)
                {
                    panel = panels[i];
                }
                else
                {
                    GameObject new_panel = Instantiate(PanelPrefab, Layout.transform);
                    PanelPrefab.SetActive(false);
                    new_panel.SetActive(true);
                    GameObject field = new_panel;
                    TextMeshProUGUI status = Component.FindObjectsOfType<TextMeshProUGUI>().ToList().Find(x => x.name == "Status");
                    TextMeshProUGUI progress = Component.FindObjectsOfType<TextMeshProUGUI>().ToList().Find(x => x.name == "Progress");
                    TextMeshProUGUI description = Component.FindObjectsOfType<TextMeshProUGUI>().ToList().Find(x => x.name == "Description");
                    TextMeshProUGUI name = Component.FindObjectsOfType<TextMeshProUGUI>().ToList().Find(x => x.name == "QuestName");
                    LayoutElement layout = new_panel.GetComponent<LayoutElement>();
                    Image image = Component.FindObjectsOfType<Image>().ToList().Find(x => x.name == "Icon");
                    new_panel.GetComponent<Image>().color = PanelDesigner.instance.GetBrightColor((byte)((Color32)new_panel.GetComponent<Image>().color).a);
                    image.color = PanelDesigner.instance.GetBrightColor();
                    panel = new Panel { Field = field, Description = description, Icon = image, Layout = layout, Name = name, Progress = progress, Status = status};
                    panels.Add(panel);
                }
                panel.Description.text = mission.description;
                panel.Progress.text = $"{mission.progressCurrent}/{mission.progressMax}";
                panel.Name.text = mission.Name;
                panel.Icon.sprite = mission.icon;
                switch (mission.Stat)
                {
                    case Mission.MisStat.InProcess:
                        panel.Status.text = "Активен";
                        panel.Status.color = Color.green;
                        break;
                    case Mission.MisStat.Failed:
                        panel.Status.text = "Провален";
                        panel.Status.color = Color.red;
                        break;
                    case Mission.MisStat.Finished:
                        panel.Status.text = "Выполнен";
                        panel.Status.color = Color.blue;
                        break;
                }
            }
            for (int i = 0; i < panels.Count; i++)
            {
                bool show = i < panels.Count;
                panels[i].Field.gameObject.SetActive(show);
                yield return new WaitForEndOfFrame();
            }

            foreach (var p in panels)
            {
                p.Description.ForceMeshUpdate();
                p.Name.ForceMeshUpdate();
                p.Progress.ForceMeshUpdate();
                p.Status.ForceMeshUpdate();
                int lines = p.Description.textInfo.lineCount + p.Name.textInfo.lineCount + p.Progress.textInfo.lineCount + p.Status.textInfo.lineCount;
                p.Layout.preferredHeight = HeightPerLine * lines + padding;
                //LayoutObj.GetComponent<RectTransform>().sizeDelta += new Vector2(0, p.Field.transform.localScale.y);
                //LayoutRebuilder.ForceRebuildLayoutImmediate(LayoutObj.GetComponent<RectTransform>());
            }

        }

        private struct Panel
        {
            public GameObject Field;
            public Image Icon;
            public TextMeshProUGUI Status;
            public TextMeshProUGUI Progress;
            public TextMeshProUGUI Name;
            public TextMeshProUGUI Description;
            public LayoutElement Layout;
        }
    }
}