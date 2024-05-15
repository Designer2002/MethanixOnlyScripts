using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GRAPHICS
{
    [System.Serializable]
    public class GraphicPanel
    {
        public string panelName;
        public GameObject rootPanel;
        public List<GraphicLayer> layers { get; private set; } = new List<GraphicLayer>();

        public bool isClear => layers.Count == 0 || layers == null || layers.All(layer => layer.currentGraphic == null);
        public GraphicLayer GetLayer(int layerDepth, bool createIfDoesnNotExists = false)
        {
           // Debug.Log(createIfDoesnNotExists);
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].layerDepth == layerDepth)
                    return layers[i];
            }
            if (createIfDoesnNotExists) return CreateLayer(layerDepth);
            return null;
        }

        public GraphicLayer CreateLayer(int layerDepth)
        {
            GraphicLayer layer = new GraphicLayer();
            GameObject panel = new GameObject(string.Format(GraphicLayer.LAYER_OBJECT_FORMAT, layerDepth));
            RectTransform rect = panel.AddComponent<RectTransform>();
            panel.GetComponent<CanvasGroup>();
            panel.transform.SetParent(rootPanel.transform, false);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;
            layer.panel = panel.transform;
            layer.layerDepth = layerDepth;

            int index = layers.FindIndex(l => l.layerDepth > layerDepth);
            if (index == -1) layers.Add(layer);
            else layers.Insert(index, layer);
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].panel.SetSiblingIndex(layers[i].layerDepth);
            }
            return layer;

        }
        public void Clear(float trandSpeed = 1, Texture blend = null, bool im = false)
        {
            foreach (var l in layers) l.Clear(trandSpeed, blend, im);
        }
    }
}