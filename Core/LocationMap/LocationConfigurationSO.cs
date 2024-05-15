using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOCATIONS
{
    [CreateAssetMenu(fileName = "Location Configuration Asset", menuName = "Locations/Location Configuration Asset")]
    public class LocationConfigurationSO : ScriptableObject
    {
        public LocationConfigData[] locations;
        public LocationConfigData GetConfig(string locationName)
        {
            locationName = locationName.ToLower();
            for (int i = 0; i < locations.Length; i++)
            {
                LocationConfigData data = locations[i];
                string lowercode = data.CodeWord.ToLower();
                if (string.Equals(locationName, lowercode))
                {
                    return data.Copy();
                }
            }
            return LocationConfigData.Default;
        }
    }
}