using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CHARACTERS
{
    public class CharacterSpriteLayer
    {
        private CharacterManager Manager = CharacterManager.instance;
        public const float DEFAULT_TRANSITION_SPEED = 3;
        public float DEFAULT_TRANSITION_SPEED_MULTIPLIER = 1;
        public int layer { get; private set; } = 0;
        public Image renderer { get; private set; } = null;
        public CanvasGroup rendererCG => renderer.GetComponent<CanvasGroup>();
        private List<CanvasGroup> oldRenderes = new List<CanvasGroup>();
        private Coroutine co_transitioning = null;
        private Coroutine co_leveling_alpha = null;
        private Coroutine co_changing_color = null;
        private Coroutine co_flipping = null;

        private bool is_facing_left = Character.DEFAULT_ORIENTATION_IS_LEFT;
        public bool is_flipping => co_flipping != null;
        public bool is_coloring => co_changing_color != null;
        public bool isLeveling => co_leveling_alpha != null;
        public bool is_transitioning => co_transitioning != null;
        public CharacterSpriteLayer(Image defaultRenderer, int layer = 0)
        {
            renderer = defaultRenderer;
            this.layer = layer;
        }

        public void SetSprite(Sprite sprite, int layer = 0)
        {
            renderer.sprite = sprite;
        }

        public Coroutine TransitionSprite(Sprite sprite, float speed)
        {
            if (sprite == renderer.sprite)
                return null;

            if(is_transitioning)
            {
                Manager.StopCoroutine(co_transitioning);
            }
            co_transitioning = Manager.StartCoroutine(TransitioningSprite(sprite, speed));
            return co_transitioning;
        }
        private IEnumerator TransitioningSprite(Sprite sprite, float speedMultiplier)
        {
            DEFAULT_TRANSITION_SPEED_MULTIPLIER = speedMultiplier;
            Image newRenderer = CreateRenderer(renderer.transform.parent);
            newRenderer.sprite = sprite;

            yield return TryLevelingAlphas();
            co_transitioning = null;
        }
        
        private Image CreateRenderer(Transform parent)
        {
            Image newRenderer = Object.Instantiate(renderer, parent);
            oldRenderes.Add(rendererCG);
            newRenderer.name = renderer.name;
            renderer = newRenderer;
            renderer.gameObject.SetActive(true);
            rendererCG.alpha = 0;
            return newRenderer;
        }
        private Coroutine TryLevelingAlphas()
        {
            if (isLeveling)
                Manager.StopCoroutine(co_leveling_alpha);
            co_leveling_alpha = Manager.StartCoroutine(AlphaLeveling());
            return co_leveling_alpha;
        }
        private IEnumerator AlphaLeveling()
        {
            while (rendererCG.alpha < 1 || oldRenderes.Any(oldCG => oldCG.alpha > 0))
            {
                float speed = DEFAULT_TRANSITION_SPEED * DEFAULT_TRANSITION_SPEED_MULTIPLIER * Time.deltaTime;
                rendererCG.alpha = Mathf.MoveTowards(rendererCG.alpha, 1, speed);
                for (int i = oldRenderes.Count - 1; i >= 0; i--)
                {
                    CanvasGroup oldCG = oldRenderes[i];
                    oldCG.alpha = Mathf.MoveTowards(oldCG.alpha, 0, speed);
                    if(oldCG.alpha <= 0)
                    {
                        oldRenderes.RemoveAt(i);
                        Object.Destroy(oldCG.gameObject);
                    }
                }
                yield return null;
            }

            co_leveling_alpha = null;
        }
        public void StopChangingColor()
        {
            if (!is_coloring) return;
            Manager.StopCoroutine(co_changing_color);
            co_changing_color = null;
        }
        public void SetColor(Color32 color)
        {
            renderer.color = color;
            foreach(CanvasGroup oldCG in oldRenderes)
            {
                oldCG.GetComponent<Image>().color = color;
            }
        }

        public Coroutine TransitionColor(Color32 color, float speed)
        {
            if(is_coloring)
            {
                Manager.StopCoroutine(co_changing_color);

            }
            co_changing_color = Manager.StartCoroutine(ChangingColor(color, speed));
            return co_changing_color;
        }
        private IEnumerator ChangingColor(Color32 color, float speed)
        {
            Color32 old_color = renderer.color;
            List<Image> oldImages = new List<Image>();

            foreach(var oldCG in oldRenderes)
            {
                oldImages.Add(oldCG.GetComponent<Image>());
            }
            float colorPercent = 0;
            while(colorPercent < 1)
            {
                colorPercent += DEFAULT_TRANSITION_SPEED * DEFAULT_TRANSITION_SPEED_MULTIPLIER * Time.deltaTime;
                renderer.color = Color32.Lerp(old_color, color, colorPercent);

                for (int i = 0; i < oldImages.Count; i++)
                {
                    Image image = oldImages[i];
                    if (image != null) image.color = renderer.color;
                    else oldImages.RemoveAt(i);
                }
                yield return null;
            }
            co_changing_color = null;
        }

        public Coroutine Flip(float speed = 1, bool immadeiat = false)
        {
            if (is_facing_left) return FaceRight(speed, immadeiat);
            else return FaceLeft(speed, immadeiat);
        }

        public Coroutine FaceLeft(float speed, bool immediately)
        {
            if (is_flipping) Manager.StopCoroutine(co_flipping);
            is_facing_left = true;
            Manager.StartCoroutine(FaceDirection(is_facing_left, speed, immediately));
            return co_flipping;
        }
        public Coroutine FaceRight(float speed, bool immediately)
        {
            if (is_flipping) Manager.StopCoroutine(co_flipping);
            is_facing_left = false;
            Manager.StartCoroutine(FaceDirection(is_facing_left, speed, immediately));
            return co_flipping;
        }
        private IEnumerator FaceDirection(bool faceleft, float speedMultiplier, bool immediately)
        {
            float xScale = faceleft ? 1 : -1;
            Vector3 newScale = new Vector3(xScale, 1, 1); ;
            if (!immediately)
            {
                Image newRenderer = CreateRenderer(renderer.transform.parent);
                if (newRenderer == null) Debug.LogWarning("NULL RENDERER");
                newRenderer.transform.localScale = newScale;
                DEFAULT_TRANSITION_SPEED_MULTIPLIER = speedMultiplier;
                TryLevelingAlphas();
                while (isLeveling) yield return null;
            }
            else
            {
                renderer.transform.localScale = newScale;
            }
            co_flipping = null;
        }
    }
}