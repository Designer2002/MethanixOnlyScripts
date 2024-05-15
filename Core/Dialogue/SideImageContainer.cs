using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SideImageContainer : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject sideImage;
    private UnityEngine.UI.Image img => GameObject.Find("Thumb") != null ? GameObject.Find("Thumb").GetComponent<UnityEngine.UI.Image>() : null;
    // Start is called before the first frame update
    public void Show(string nameToShow = "")
    {
        root.SetActive(true);
        if (nameToShow != string.Empty)
        {
            sideImage.SetActive(true);
            string name = nameToShow.ToLower() + "_thumb";
            //var temporary_path = "tmp/";
            var namePath = $"Characters/{nameToShow}/Images/";
            string path =namePath + name;
            //string path = @"_TESTING_/tmp/";
            //Debug.Log(path);
            img.color = new Color32(125, 120, 188, 255);
            img.sprite = Resources.Load<Sprite>(path);
            if (!img.sprite)
            {
                Hide();
                Debug.Log("NO SPRITE!");
                return;
            }
            sideImage.GetComponentInChildren<UnityEngine.UI.Image>().sprite = img.sprite;
            //Debug.Log($"{sideImage.GetComponentInChildren<UnityEngine.UI.Image>().sprite.bounds.center}");
        }
    }

    // Update is called once per frame
    public void Hide()
    {
        if (img != null)
        {
            img.color = new Color32(0, 0, 0, 0);
            sideImage.SetActive(false);
        }
        

    }
}
