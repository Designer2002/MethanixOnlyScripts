using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttitudePanel : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Slider slider;

    [SerializeField]
    UnityEngine.UI.Image fill;
    [SerializeField]
    UnityEngine.UI.Image border;
    [SerializeField]
    UnityEngine.UI.Image handle;

    public int Value { get; set; } = 0;

    private Color32 color;

    [SerializeField]
    private CanvasGroup canvas;

    [SerializeField]
    private Animator anim;

    public static AttitudePanel instance;
    private DIALOGUE.CanvasGroupController cg = null;

    private bool is_displaying => co_displaying != null;
    private bool is_blinking => co_blinking != null;
    private Coroutine co_displaying = null;
    private Coroutine co_blinking = null;
    private Color start;
    private float fillSpeed = 0.25f;
    private float divider = 10;

    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        start = new Color(handle.color.r, handle.color.g, handle.color.b, handle.color.a);
        cg = new DIALOGUE.CanvasGroupController(this, canvas);
        cg.SetInteractableState(false);
        cg.Hide();
        Value = 5;
        slider.value = Value;
        slider.maxValue = 1;
    }

    public Coroutine Display(bool positive)
    {
        if (is_displaying)
        {
            StopCoroutine(co_displaying);
            cg.Hide();
            return co_displaying;
        }
        co_displaying = StartCoroutine(Displaying(positive)); 

        return co_displaying;
    }

    public IEnumerator Displaying(bool positive)
    {
        color = PanelDesigner.instance.GetBrightColor();
        fill.color = color;
        border.color = color;
        handle.color = color;
        slider.value = Value / divider;
        yield return cg.Show();
        yield return MoveValue(positive);
        yield return new WaitForSeconds(2.5f);
        yield return cg.Hide();
        anim.enabled = false;
        co_displaying = null;
    }

    private IEnumerator MoveValue(bool positive)
    {
        float target = positive ? slider.value + 0.1f : slider.value - 0.1f;
        while (slider.value != target)
        {
            slider.value = Mathf.MoveTowards(slider.value, target, fillSpeed * Time.deltaTime);
            anim.enabled = true;
            yield return null;
        }
    }

}
