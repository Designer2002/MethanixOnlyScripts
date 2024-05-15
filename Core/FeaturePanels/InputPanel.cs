using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputPanel : MonoBehaviour
{
    public static InputPanel instance { get; private set; }
    public string lastInput { get; set; } = string.Empty;
    [SerializeField]
    private UnityEngine.UI.InputField Input;
    [SerializeField]
    private CanvasGroup CanvasGroup;
    [SerializeField]
    private Button acceptButton;
    [SerializeField]
    private TMPro.TMP_Text titleText;

    private DIALOGUE.CanvasGroupController cg;

    public bool isWaitingOnUserInput { get; private set; }
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
        else DestroyImmediate(instance);
    }

    private void Start()
    {
        cg = new DIALOGUE.CanvasGroupController(this, CanvasGroup);
        cg.alpha = 0;
        cg.SetInteractableState(active: false);
        acceptButton.gameObject.SetActive(false);
        Input.onValueChanged.AddListener(OnInputChanged);
        acceptButton.onClick.AddListener(onAcceptInput);
    }

    private void OnInputChanged(string arg0)
    {
        acceptButton.gameObject.SetActive(HasValidText());
    }

    private void onAcceptInput()
    {
        if (Input.text == string.Empty) return;
        lastInput = Input.text;
        Hide();
    }

    public void Show(string title)
    {
        titleText.text = title;
        Input.text = string.Empty;
        cg.Show();
        cg.SetInteractableState(active: true);
        isWaitingOnUserInput = true;
    }

    public void Hide()
    {
        cg.Hide();
        cg.SetInteractableState(active: false);
        isWaitingOnUserInput = false;
    }

    private bool HasValidText()
    {
        return Input.text != string.Empty;
    }
}
