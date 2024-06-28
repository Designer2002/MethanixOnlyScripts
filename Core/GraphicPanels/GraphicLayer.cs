using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace GRAPHICS
{
    public class GraphicLayer
    {
        public const string LAYER_OBJECT_FORMAT = "Layer: [{0}]";
        public int layerDepth = 0;
        public Transform panel;
        public GraphicObject currentGraphic = null;
        public List<GraphicObject> oldGraphics = new List<GraphicObject>();
        public Dictionary<string, GraphicObject> createdBefore = new Dictionary<string, GraphicObject>();

        public Coroutine SetTexture(string path = "", float transitionSpeed = 1, Texture blend = null, bool immediate = false)
        {
            Texture tex = Resources.Load<Texture>(path);

            if (tex != null)  return SetTexture(tex, path, transitionSpeed, blend, immediate);
            else Debug.Log("TEXTURE NOT FOUND");
            return null;
        }

        public Coroutine SetTexture(Texture tex, string path, float transitionSpeed = 1, Texture blend = null, bool immediate = false, bool offset = false, bool change = true)
        {
            if(change)CheckVariable(tex.name);
            if (offset) panel.transform.position = new Vector3(panel.transform.position.x, panel.transform.position.y + 55, panel.transform.position.z);
            return createdBefore.ContainsKey(tex.name) ? GetGraphic(tex.name, transitionSpeed, blend, immediate) : CreateGraphic(tex, transitionSpeed, path, blend, immediate);
        }

        public Coroutine SetVideo(VideoClip video, string path, float transitionSpeed = 1, Texture blend = null, bool useAudio = false, float pspeed = 1, bool immediate = false, bool offset = false, bool change = true)
        {
            if(change)CheckVariable(video.name);
            if (offset) panel.transform.position = new Vector3(panel.transform.position.x, panel.transform.position.y + 55, panel.transform.position.z);
            return createdBefore.ContainsKey(video.name) ? GetGraphic(video.name, transitionSpeed, blend, immediate) : CreateGraphic (video, transitionSpeed, path, blend, useAudio, pspeed, immediate);
        }

        private void CheckVariable(string name)
        {
            VariableStore.TryGetValue("backgroundPanel", out object back);
            VariableStore.TrySetValue("backgroundPanel", name, change: false);
            
        }

        public Coroutine SetVideo(string path, float transitionSpeed = 1, Texture blend = null, bool useAudio = false, float pspeed = 1, bool immediate = false)
        {
            VideoClip video = Resources.Load<VideoClip>(path);
            if (video == null)
            {
                Debug.Log("VIDEO NOT FOUND");
                return null;
            }
            else
            {
                
                return SetVideo(video, path, transitionSpeed, blend, useAudio, pspeed, immediate);

            }

            }

            private Coroutine CreateGraphic<T>(T GraphicData, float transitionSpeed, string path, Texture blend, bool useAudioForVideo = false, float pspeed = 1, bool immediate = false)
        {
            GraphicObject newGraphic = null ;
            
            if (GraphicData is Texture) newGraphic = new GraphicObject(this, path, GraphicData as Texture, immediate);
            else if (GraphicData is VideoClip) newGraphic = new GraphicObject(this, path, GraphicData as VideoClip, useAudioForVideo, pspeed, immediate);
            if (currentGraphic != null && oldGraphics.Contains(currentGraphic)) oldGraphics.Add(currentGraphic);
            currentGraphic = newGraphic;
            if(!immediate) return currentGraphic.FadeIn(transitionSpeed, blend);
            DestroyOldGraphics();
            return null;

        }

        private Coroutine GetGraphic(string key, float transitionSpeed, Texture blend, bool immediate = false)
        {
            createdBefore[key].graphicGO.transform.SetSiblingIndex(createdBefore.Count);
            GraphicObject newGraphic = createdBefore[key];
            
            if (currentGraphic != null && oldGraphics.Contains(currentGraphic)) oldGraphics.Add(currentGraphic);
            currentGraphic = newGraphic;
            if (!immediate) return currentGraphic.FadeIn(transitionSpeed, blend);
            DestroyOldGraphics();
            return null;
        }

        public void DestroyOldGraphics()
        {
            foreach (var g in oldGraphics)
            {
                Object.Destroy(g.renderer.gameObject);
            }
            oldGraphics.Clear();
        }

        public void Clear(float trandSpeed = 1, Texture blend = null, bool immdiate = false)
        {
            if (currentGraphic != null)
                if (!immdiate) currentGraphic.FadeOut(trandSpeed, blend);
                else currentGraphic.Destroy();

            foreach(var g in oldGraphics)
            {
                if (!immdiate) g.FadeOut(trandSpeed, blend);
                else g.Destroy();
            }
        }
    }
}