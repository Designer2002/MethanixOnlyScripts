using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchedObjectManager : MonoBehaviour
{
    public static SearchedObjectManager instance;
    [SerializeField]
    private GameObject ButtonPrefab;
    [SerializeField]
    private Canvas Main;
    public Dictionary<string, SearchedObjectButton> buttons = new Dictionary<string, SearchedObjectButton>();
    private int MIN_X;
    private int MAX_X;
    public bool IsAnythingToSearch => buttons.Count != 0;
    public bool FoundAll()
    {
        foreach (var b in buttons.Values)
        {
            if (!b.IsFound) return false;
        }
        return true;
    }
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        RectTransform rect = Main.GetComponent<RectTransform>();
        MIN_X = -1 * (int)(rect.rect.width / 2) + 20;
        MAX_X = (int)(rect.rect.width / 2) - 20;
        ButtonPrefab.GetComponent<RectTransform>().localPosition = new Vector3(0, 50, 0);
        ButtonPrefab.SetActive(false);
        VariableStore.CreateVariable("found", false);
    }
    public GameObject Create() => Instantiate(ButtonPrefab, GameObject.Find("SearchedObjects").transform);

    public SearchedObjectButton GetObject(string location) => buttons[location];

    public void CheckIfSomethingExistThere(string location)
    {
        if (!buttons.TryGetValue(location, out SearchedObjectButton b)) return;
        else buttons[location].Show();
    }
    public void Display(string textureName, string storingLocation, string currentLocation) //cmd general
    {
        SearchedObjectButton btn;
        if (buttons.TryGetValue(storingLocation, out btn))
            btn.Show();
        else
        {
            GameObject newObj = Create();
            newObj.SetActive(true);
            newObj.GetComponent<RectTransform>().localPosition += new Vector3(GetRandomPos(), 0, 0);
            btn = newObj.GetComponent<SearchedObjectButton>();
            btn.Spawn(textureName);
            buttons.Add(storingLocation, btn);
        }
        if (storingLocation == currentLocation)
        {
            btn.Show();
            
        }
        else
        {
            foreach (var b in buttons.Values)
                b.Hide();
        }
            
    }

    public int GetRandomPos()
    {
        System.Random r = new System.Random();
        return r.Next(MIN_X, MAX_X);
    }
}
