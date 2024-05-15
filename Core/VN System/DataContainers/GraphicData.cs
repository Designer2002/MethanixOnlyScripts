using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class GraphicData
    {
        public string panelName;
        public List<LayerData> layers;

        [System.Serializable]
        public class LayerData
        {
            public int depth = 0;
            public string graphicName;
            public string graphicPath;
            public bool isVideo;
            public bool useAudio;

            public LayerData(GRAPHICS.GraphicLayer layer)
            {
                depth = layer.layerDepth;
                if (layer.currentGraphic == null)
                    return;
                var graphic = layer.currentGraphic;
                graphicName = graphic.graphicName;
                graphicPath = graphic.graphicPath;
                isVideo = graphic.isVideo;
                useAudio = false; //tmp!!!
            }

        }
        public static List<GraphicData> Capture()
        {
            List<GraphicData> graphics = new List<GraphicData>();
            foreach (var panel in GRAPHICS.GraphicPanelManager.instance.allPanels)
            {
                if (panel.isClear) continue;
                GraphicData data = new GraphicData();
                data.panelName = panel.panelName;
                data.layers = new List<LayerData>();
                foreach (var layer in panel.layers)
                {
                    LayerData entry = new LayerData(layer);
                    data.layers.Add(entry);
                }
                graphics.Add(data);
            }
            return graphics;
        }

        public static void Apply(List<GraphicData> data)
        {
            List<string> cache = new List<string>();
            foreach(var panelData in data)
            {
                var panel = GRAPHICS.GraphicPanelManager.instance.GetPanel(panelData.panelName);
                foreach(var layerData in panelData.layers)
                {
                    var layer = panel.GetLayer(layerData.depth, createIfDoesnNotExists: true);
                    if(layer.currentGraphic == null || layer.currentGraphic.graphicName != layerData.graphicName)
                    {
                        if(!layerData.isVideo)
                        {
                            Texture2D tex = SavingsCache.LoadImage(layerData.graphicPath);
                            if (tex != null) layer.SetTexture(tex, layerData.graphicPath, immediate: true);
                        }
                        else
                        {
                            UnityEngine.Video.VideoClip clip = SavingsCache.LoadVideo(layerData.graphicPath);
                            if (clip != null)
                                layer.SetVideo(clip, layerData.graphicPath, immediate: true);
                        }

                    }
                }
                cache.Add(panel.panelName);
            }
            foreach(var panel in GRAPHICS.GraphicPanelManager.instance.allPanels)
            {
                if (!cache.Contains(panel.panelName))
                    panel.Clear(im: true);
            }
        }
    }
}