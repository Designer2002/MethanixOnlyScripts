using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchedObjectButton : MonoBehaviour
{
    
    [SerializeField]
    private CanvasGroup CanvasGroup;
    [SerializeField]
    private UnityEngine.UI.Button Button;
    public bool IsFound { get; set; }

    private DIALOGUE.CanvasGroupController cg = null;


    public void Show()
    {
        if (!IsFound)
        {
            cg.alpha = 1;
            cg.SetInteractableState(true);
        }
    }

    public void Spawn(string textureName)
    {
        if (!IsFound)
        {
            VariableStore.TrySetValue("found", false);
            Button.image.sprite = Resources.Load<Sprite>(FilePaths.SearchedObjectsPaths + textureName);
            cg = new DIALOGUE.CanvasGroupController(this, CanvasGroup);
            cg.alpha = 0;
            cg.SetInteractableState(false);
        }
    }

    

    public void Find()
    {
        if(!IsFound)
        {
            IsFound = true;
            cg.Hide();
        }
        if (SearchedObjectManager.instance.FoundAll())
        {
            DIALOGUE.NotificationPanel.instance.Display("Все предметы найдены, пора возращаться обратно!");
            VariableStore.TrySetValue("found", true);
        }
    }

    public void Hide()
    {
        cg.Hide();
        cg.SetInteractableState(false);
    }
}
