using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoicePanel : MonoBehaviour
{
    public static ChoicePanel instance;
    [SerializeField]
    private CanvasGroup CanvasGroup;
    [SerializeField]
    private TextMeshProUGUI title;
    [SerializeField]
    private GameObject ChoiceButtonPrefab;
    [SerializeField]
    private VerticalLayoutGroup buttonLayout;
    public ChoicePanelDesicion lastDecision { get; set; } = null;
    public bool isWaitingOnUserChoice { get; private set; } = false;

    private const float MIN_WIDTH = 80;
    private const float MAX_WIDTH = 240;
    private const float WIDTH_PADDING = 25;

    private const float BUTTON_HEIGHT_PER_LINE = 50;
    private const float BUTTON_HEIGHT_PADDING = 20;

    private List<ChoiceButton> buttons = new List<ChoiceButton>();

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

    // Update is called once per frame
    public void Show(string question, string[] choices)
    {
        
        
        lastDecision = new ChoicePanelDesicion(question, choices);
        isWaitingOnUserChoice = true;
        cg.Show();
        cg.SetInteractableState(true);
        title.text = question;
        StartCoroutine(GenerateChoices(choices));
    }

    private IEnumerator GenerateChoices(string[] choices)
    {
        float maxWidth = 0;
        for(int i = 0; i < choices.Length; i++)
        {
            ChoiceButton choiceButton;
            if(i < buttons.Count)
            {
                choiceButton = buttons[i];
            }
            else
            {
                GameObject button = Instantiate(ChoiceButtonPrefab, buttonLayout.transform);
                ChoiceButtonPrefab.SetActive(false);
                button.SetActive(true);
                Button _newbtn = button.GetComponent<Button>();
                TextMeshProUGUI ugui = button.GetComponentInChildren<TextMeshProUGUI>();
                LayoutElement newlayout = button.GetComponent<LayoutElement>();
                button.GetComponent<Image>().color = PanelDesigner.instance.GetBrightColor();
                choiceButton = new ChoiceButton { button = _newbtn, layout = newlayout, title = ugui };
                buttons.Add(choiceButton);
            }
            choiceButton.button.onClick.RemoveAllListeners();
            int buttonIdx = i;
            choiceButton.button.onClick.AddListener(() => AcceptAnswer(buttonIdx));
            choiceButton.title.text = choices[i];

            float buttonWWidth = Mathf.Clamp(WIDTH_PADDING + choiceButton.title.preferredWidth, MIN_WIDTH, MAX_WIDTH);
            maxWidth = Mathf.Max(maxWidth, buttonWWidth);
        }
        foreach (var button in buttons)
        {
            button.layout.preferredWidth = maxWidth;
        }
        for(int i =0; i < buttons.Count;i++)
        {
            bool show = i < choices.Length;
            buttons[i].button.gameObject.SetActive(show);
            yield return new WaitForEndOfFrame();
        }

        foreach (var button in buttons)
        {

            button.title.ForceMeshUpdate();
            int lines = button.title.textInfo.lineCount;
            button.layout.preferredHeight = BUTTON_HEIGHT_PER_LINE * lines + BUTTON_HEIGHT_PADDING;

        }
    }

    public void Hide()
    {
        cg.SetInteractableState(false);
        cg.Hide();
    }

    private void AcceptAnswer(int idx)
    {
        if (idx < 0 || idx > lastDecision.choices.Length - 1) return;
        lastDecision.answerIdx = idx;
        isWaitingOnUserChoice = false;
        Hide();
    }

    public class ChoicePanelDesicion
    {
        public string question = string.Empty;
        public int answerIdx = -1;
        public string[] choices = new string[0];

        public ChoicePanelDesicion(string q, string[] ch)
        {
            this.answerIdx = -1;
            this.choices = ch;
            this.question = q;
        }
    }

    private struct ChoiceButton
    {
        public Button button;
        public TextMeshProUGUI title;
        public LayoutElement layout;
    }

}
