using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator anim;
    private static ButtonBehaviour selectedButton = null;

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.Play("Exit");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(selectedButton != null && selectedButton != this)
        {
            selectedButton.OnPointerExit(null);
        }
        anim.Play("Enter");
        selectedButton = this;
    }

    
}
