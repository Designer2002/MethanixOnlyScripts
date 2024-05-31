using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SearchingObjectsData
{
    public bool found;

    public static List<SearchingObjectsData> Capture()
    {
        List<SearchingObjectsData> searchingObjects = new List<SearchingObjectsData>();
        if (!SearchedObjectManager.instance.IsAnythingToSearch) return searchingObjects;
        foreach(var button in SearchedObjectManager.instance.buttons)
        {
            var obj = new SearchingObjectsData();
            obj.found = button.Value.IsFound;
            searchingObjects.Add(obj);
        }
        return searchingObjects;
    }
    public static void Apply(List<SearchingObjectsData> data)
    {
        int idx = 0;
        foreach (var button in SearchedObjectManager.instance.buttons)
        {
            button.Value.IsFound = data[idx].found;
            idx++;
        }
    }
}
