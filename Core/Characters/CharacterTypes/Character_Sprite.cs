using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CHARACTERS
{
    public class Character_Sprite : Character
    {
        
        private const string SPRITE_RENDERER_PARENT_NAME = "Renderer";
        private const string DEFAULT_SPRITE_SHEET_TEXTURE_NAME = "Default";
        private const char SPRITE_SHEET_DELIMITTER = '/';
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>();
        private string artAssetsDirectory = "";
        public override bool IsVisible
        {
            get
            {
                return is_revealing || rootCG.alpha == 1;
            }
            set
            {
                rootCG.alpha = value ? 1 : 0;
            }
        }
        public Character_Sprite(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder) : base(name, config, prefab)
        {

            rootCG.alpha = ENABLED_ON_START ? 1 : 0;
            GetLayers();
            artAssetsDirectory = rootAssetsFolder + "/Images";
            Debug.Log($"Created character '{name}'");
        }

        public List<List<CharacterSpriteLayer>> layers = new List<List<CharacterSpriteLayer>>();
        public List<CharacterSpriteLayer> sublayersBody = new List<CharacterSpriteLayer>();
        public List<CharacterSpriteLayer> sublayersFace = new List<CharacterSpriteLayer>();

        private void GetLayers()
        {
            Transform rendererRoot = animator.transform.Find(SPRITE_RENDERER_PARENT_NAME);
            if (rendererRoot == null)
            {
                return;
            }
            //1st do the BODY
            Transform layer0 = rendererRoot.transform.Find("Layer 0");
            for (int i = 0; i < layer0.childCount; i++)
            {
                Transform child = layer0.transform.GetChild(i);
                Image rendererImage = child.GetComponentInChildren<Image>();
                if (rendererImage != null)
                {
                    CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                    sublayersBody.Add(layer);
                    child.name = $"layer: {i}";

                }
            }
            Transform layer1 = rendererRoot.transform.Find("Layer 1");
            if(layer1 != null)
            {
                for (int i = 0; i < layer1.childCount; i++)
                {
                    Transform child = layer1.transform.GetChild(i);
                    Image rendererImage = child.GetComponentInChildren<Image>();
                    if (rendererImage != null)
                    {
                        CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                        sublayersFace.Add(layer);
                        child.name = $"layer: {i}";

                    }
                }
            }
            layers.Add(sublayersBody);
            if(sublayersFace.Count != 0) layers.Add(sublayersFace);


        }


        public void SetSprite(Sprite sprite, int layer = 0, int sublayer = 0)
        {
            layers[layer][sublayer].SetSprite(sprite);
        }
        public Sprite GetSprite(string SpriteName)
        {
            if(config.characterType == CharacterType.SpriteSheet)
            {
                string[] data = SpriteName.Split(SPRITE_SHEET_DELIMITTER);
                Sprite[] sa = new Sprite[0];
                if (data.Length == 2)
                {
                    string textureName = data[0];
                    SpriteName = data[1];
                    sa = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{textureName}");
                }
                else
                {
                    sa= Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{DEFAULT_SPRITE_SHEET_TEXTURE_NAME}");
                }
                //Debug.Log($"{artAssetsDirectory}/{DEFAULT_SPRITE_SHEET_TEXTURE_NAME}");
                //Debug.Log("sprite - " + SpriteName);
                return Array.Find(sa, sprite => SpriteName == sprite.name);
            }
            else
            {
                //Debug.Log($"{artAssetsDirectory}/{SpriteName.ToLower()}");
                return Resources.Load<Sprite>($"{artAssetsDirectory}/{SpriteName.ToLower()}");
            }
        }

        public Coroutine TransitionSprite(Sprite sprite, int layer = 1, int sublayer = 0, float speed = 1)
        {
            CharacterSpriteLayer spriteLayer = layers[layer][sublayer];
            return spriteLayer.TransitionSprite(sprite, speed);
        }
        public override IEnumerator ShowingOrHiding(bool show)
        {
            float targetAlpha = show ? 1f : 0;
            yield return Alphing(targetAlpha);
            
            
            co_hiding = null;
            co_revealing = null;
        }

        private IEnumerator Alphing(float targetAlpha)
        {
            CanvasGroup self = rootCG;
            while (self.alpha != targetAlpha)
            {
                self.alpha = Mathf.MoveTowards(self.alpha, targetAlpha, 3f * Time.deltaTime);
                yield return null;
            }
        }
        public override void SetColor(Color color)
        {
            
            base.SetColor(color);
            color = DisplayColor;
            foreach (var list in layers)
            {
                foreach (var layer in list)
                {
                    layer.StopChangingColor();
                    layer.SetColor(color);
                }
            }
        }

        public override IEnumerator ChangingColor(Color color, float speed, int idx = 0)
        {
            foreach(var list in layers)
            {
                foreach(var layer in list)
                {
                    layer.TransitionColor(color, speed);
                }
            }
            yield return null;
            while(layers[idx].Any(l => l.is_coloring))
            {
                yield return null;
            }
            co_changing_color = null;
        }

        public override IEnumerator Highlighting(bool highlight, float speedMultiplier, int idx = 0, bool immediate = false)
        {
            
            Color targetColor = DisplayColor;
            foreach (var list in layers)
            {
                foreach (var layer in list)
                {
                    if (!immediate) layer.TransitionColor(targetColor, speedMultiplier);
                    else layer.SetColor(DisplayColor);
                }
                yield return null;
            }
            while (layers[idx].Any(l => l.is_coloring))
            {
                yield return null;
            }
            co_highlighting = null;
        }
        public override IEnumerator FaceDirection(bool faceleft, float speedMultiplier, bool immediate, int idx = 0)
        {
            foreach(var list in layers)
            {
                foreach (var layer in list)
                {
                    if (faceleft)
                        layer.FaceLeft(speedMultiplier, immediate);
                    else layer.FaceRight(speedMultiplier, immediate);
                }
            }
            yield return null;
            while (layers[idx].Any(l => l.is_flipping))
            {
                yield return null;
            }
            co_flipping = null;
        }
        public override void OnReceiveExpressions(string expression, int layer = 1)
        {
            Sprite sprite = GetSprite(expression);
            if(sprite == null)
            {
                Debug.Log("sprite expression not found");
                return;
            }
            TransitionSprite(sprite, layer);
        }

        public override IEnumerator BeHologram(int idx = 0)
        {
            System.Random r = new System.Random();
            this.HoloColor = new Color32(0, 245, 157, 255);
            if(!is_hologram)
            {

                while (true)
                {
                    byte val = (byte)r.Next(122, 255);
                    var targetfloat = r.Next((int)HoloColor.b, 220);
                    foreach (var list in layers)
                    {
                        foreach (var layer in list)
                        {

                            layer.SetColor(Hologramming(HoloColor, HoloColor.r, HoloColor.b, targetfloat, val));

                        }

                        yield return null;
                    }

                    while (layers[idx].Any(l => l.is_coloring))
                    {
                        yield return null;
                    }
                    is_hologram = true;
                    yield return null;
                    
                }
            }
            co_hologramming = null;


          
        }

        public override IEnumerator StopBeingHologram(int idx = 0)
        {
            if (is_hologram)
            {
                foreach (var list in layers)
                {
                    foreach (var layer in list)
                    {

                        layer.TransitionColor(Color.white, 1);

                    }

                    yield return null;
                }

                while (layers[idx].Any(l => l.is_coloring))
                {
                    yield return null;
                }
                is_hologram = false;
                yield return null;


            }
           
            co_stopping_being_hologram = null;
        }

        private Color32 Hologramming(Color32 c, byte r, byte b, float target, byte a)
        {
            if(b < target)
            {
                while(b != target)
                {
                    b++;
                    r += 3;
                }
            }
            return new Color32(r, c.g, b, a);
        }
    }
}