using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GRAPHICS
{
    public class GraphicPanelManager : MonoBehaviour
    {
        public static GraphicPanelManager instance { get; private set; }
        [field: SerializeField]
        public GraphicPanel[] allPanels { get; private set; }
        public const float DEFAULT_TRANSITION_SPEED = 3f;
        private void Awake()
        {
            instance = this;
        }
        public GraphicPanel GetPanel(string name)
        {
            foreach(var panel in allPanels)
            {
                if (panel.panelName.ToLower() == name.ToLower())
                {
                    Debug.Log($"panel {name} found");
                    return panel;
                }
            }
            Debug.Log($"panel {name} not found");
            return null;
        }
    }
}