using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GRAPHICS
{
    public class GraphicObject
    {
        public const string NAME_FORMAT = "Graphic - [{0}]";
        private const string MATERIAL_PATH = "Materials/layerTransitionMaterial";
        private const string MATERIAL_FIELD_COLOR = "_Color";
        private const string MATERIAL_MAIN_TEX = "_MainTex";
        private const string MATERIAL_BLEND_TEX = "_BlendTex";
        private const string MATERIAL_BLEND = "_Blend";
        private const string MATERIAL_ALPHA = "_Alpha";

        private GraphicLayer layer;

        public RawImage renderer;
        public VideoPlayer video = null;
        public bool isVideo => video != null;
        public bool useAudio => audio != null ? !audio.mute : false;
        public AudioSource audio = null;

        public string graphicPath = "";
        public string graphicName { get; private set; }

        private Coroutine co_fading_in = null;
        private Coroutine co_fading_out = null;

        public GameObject graphicGO { get; private set; } = null;
        public GraphicObject(GraphicLayer layer, string graphicPath, Texture tex, bool immediate)
        {
            this.graphicPath = graphicPath;
            this.layer = layer;
            graphicGO = new GameObject();

            graphicGO.transform.SetParent(layer.panel);
            
            renderer = graphicGO.AddComponent<RawImage>();

            var filter = graphicGO.AddComponent<AspectRatioFitter>();
            filter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            filter.aspectRatio = (float)tex.width / tex.height;
            graphicName = tex.name;
            InitGraphic(immediate);
            renderer.name = string.Format(NAME_FORMAT, graphicName);
            renderer.material.SetTexture(MATERIAL_MAIN_TEX, tex);
            this.layer.createdBefore.Add(graphicName, this);
        }

        public GraphicObject(GraphicLayer layer, string graphicPath, VideoClip clip, bool useAudio, float pspeed, bool immediate)
        {
            this.graphicPath = graphicPath;
            this.layer = layer;
            graphicGO = new GameObject();
            graphicGO.transform.SetParent(layer.panel);
            renderer = graphicGO.AddComponent<RawImage>();

            var filter = graphicGO.AddComponent<AspectRatioFitter>();
            filter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            filter.aspectRatio = (float)clip.width / clip.height;
            

            InitGraphic(immediate);

            RenderTexture tex = new RenderTexture(Mathf.RoundToInt(clip.width), Mathf.RoundToInt(clip.height), 0);
           
            renderer.material.SetTexture(MATERIAL_MAIN_TEX, tex);
            
            video = graphicGO.AddComponent<VideoPlayer>();
            graphicName = clip.name;
            renderer.name = string.Format(NAME_FORMAT, graphicName);
            video.playOnAwake = true;
            video.source = VideoSource.VideoClip;
            video.clip = clip;
            video.renderMode = VideoRenderMode.RenderTexture;
            video.targetTexture = tex;
            
            video.isLooping = true;
            video.playbackSpeed = pspeed;
             if(useAudio)
            {
                video.audioOutputMode = VideoAudioOutputMode.AudioSource;
                audio = graphicGO.AddComponent<AudioSource>();
                audio.volume = 0;
            }

            
            video.SetTargetAudioSource(0, audio);
            video.frame = 0;
            video.Prepare();
            video.Play();
            this.layer.createdBefore.Add(graphicName, this);
        }

        private void InitGraphic(bool immmeate)
        {
            renderer.transform.localPosition = Vector3.zero;
            renderer.transform.localScale = Vector3.one;

            RectTransform rect = renderer.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;

            float startOpacity = immmeate ? 1 : 0;
            renderer.material = GetTransitionMaterial();
            renderer.material.SetFloat(MATERIAL_BLEND, startOpacity);
            renderer.material.SetFloat(MATERIAL_ALPHA, startOpacity);
        }

        private Material GetTransitionMaterial()
        {
            var mat = Resources.Load<Material>(MATERIAL_PATH);
            if (mat != null) return new Material(mat);
            else return null;
        }

        GraphicPanelManager manager => GraphicPanelManager.instance;

        public Coroutine FadeIn(float speed = 1, Texture blend = null)
        {
            if (co_fading_out != null) manager.StopCoroutine(co_fading_out);
            if (co_fading_in != null) return co_fading_in;
            bool enabled = DIALOGUE.DialogueSystem.instance.reader != null && DIALOGUE.DialogueSystem.instance.reader.is_on;
            co_fading_in = manager.StartCoroutine(Fading(1f, speed, blend, enabled));
            return co_fading_in;
        }
        public Coroutine FadeOut(float speed = 1, Texture blend = null)
        {
            if (co_fading_in != null) manager.StopCoroutine(co_fading_in);
            if (co_fading_out != null) return co_fading_out;
            bool enabled = DIALOGUE.DialogueSystem.instance.reader.is_on;
            co_fading_out = manager.StartCoroutine(Fading(0f, speed, blend, enabled));
            return co_fading_out;
        }

        private IEnumerator Fading(float target, float speed, Texture blend, bool enabled = false)
        {
            bool is_blending = blend != null;
            bool fadingIn = target > 0;
            renderer.material.SetTexture(MATERIAL_BLEND_TEX, blend);
            renderer.material.SetFloat(MATERIAL_ALPHA, is_blending ? 1 : fadingIn ? 0 : 1);
            renderer.material.SetFloat(MATERIAL_BLEND, is_blending ? fadingIn ? 0 : 1 : 1);
            string opacityp = is_blending ? MATERIAL_BLEND : MATERIAL_ALPHA;
            while(renderer.material.GetFloat(opacityp) != target)
            {
                float opacity = Mathf.MoveTowards(renderer.material.GetFloat(opacityp), target, speed * Time.deltaTime);
                renderer.material.SetFloat(opacityp, opacity);
                if (isVideo && audio != null) audio.volume = opacity;
                if (!enabled && DIALOGUE.DialogueSystem.instance.reader != null) DIALOGUE.DialogueSystem.instance.reader.DisableButtonsAndStopItsAction();
                yield return null;

            }
            co_fading_in = null;
            co_fading_out = null;
            if (DIALOGUE.DialogueSystem.instance.reader != null) DIALOGUE.DialogueSystem.instance.reader.EnableButtonsAndStopItsAction();
            if (blend == null) Debug.Log("blend not found");

            if (target == 0) Destroy();
            else DestroyBGOnLayer();
        }
        public void Destroy()
        {
            if (layer.currentGraphic != null && layer.currentGraphic.renderer == renderer)
                layer.currentGraphic = null;
            if (layer.oldGraphics.Contains(this)) layer.oldGraphics.Remove(this);
            Object.Destroy(renderer.gameObject);
        }
        private void DestroyBGOnLayer()
        {
            layer.DestroyOldGraphics();
        }
    }
}