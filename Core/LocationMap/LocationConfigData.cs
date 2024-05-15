using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOCATIONS
{
    [System.Serializable]
    public class LocationConfigData
    {
        public string Name;
        public string CodeWord;
        public string Description;
        public List<string> Neighbours;

        public LocationConfigData Copy()
        {
            LocationConfigData result = new LocationConfigData();
            result.CodeWord = CodeWord;
            result.Description = Description;
            result.Name = Name;
            result.Neighbours = Neighbours;

            return result;
        }

        public static LocationConfigData Default => new LocationConfigData() { CodeWord = "", Description = "", Name = "", Neighbours = new List<string>() };
    }
}