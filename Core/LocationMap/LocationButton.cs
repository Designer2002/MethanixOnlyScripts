using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOCATIONS
{
    public class LocationButton : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.UI.Button Button;
        public void OpenInfo()
        {
            //Debug.LogWarning(Button.name.ToLower());
            LocationInfo.instance.SetReceiver(Button.gameObject.name.ToLower());
            LocationInfo.instance.Show();
        }
    }
}