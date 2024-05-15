using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTERS
{
    public abstract class Character
    {

        public string name = "";
        public string displayName = "";
        public string RussianName = "";
        public RectTransform root = null;
        public DIALOGUE.DialogueSystem DialogueSystem => DIALOGUE.DialogueSystem.instance;
        public const bool ENABLED_ON_START = false;
        public const float UNHIGHLIGHTED_COLOR_STRENGTH = 0.65f;
        public const bool DEFAULT_ORIENTATION_IS_LEFT = true;
        protected bool facing_left = DEFAULT_ORIENTATION_IS_LEFT;
        public const string ANIMATION_REFRESH_TRIGGER = "Refresh";
        public CharacterConfigData config;

        public Vector2 targetPosition { get; private set; }
        public Color32 color { get; protected set; } = Color.white;
        public Color32 HighlightedColor => color;
        public Color32 DisplayColor => highlighted ? HighlightedColor : UnHighlightedColor;
        //public Color32 UnHighlightedColor => new Color32((byte)(color.r * UNHIGHLIGHTED_COLOR_STRENGTH), (byte)(color.g * UNHIGHLIGHTED_COLOR_STRENGTH), (byte)(color.b * UNHIGHLIGHTED_COLOR_STRENGTH), color.a);
        public Color32 UnHighlightedColor => new Color32((byte)(color.r * UNHIGHLIGHTED_COLOR_STRENGTH), (byte)(color.g * UNHIGHLIGHTED_COLOR_STRENGTH), (byte)(color.b * UNHIGHLIGHTED_COLOR_STRENGTH), color.a);

        public Color32 HoloColor { get; set; }

        public bool highlighted { get; protected set; } = true;

        protected CharacterManager manager => CharacterManager.instance;

        public Animator animator;

        protected Coroutine co_revealing, co_hiding;
        protected Coroutine co_moving;
        protected Coroutine co_changing_color;
        protected Coroutine co_highlighting;
        protected Coroutine co_unhighlighting;
        protected Coroutine co_flipping;
        protected Coroutine co_hologramming;
        protected Coroutine co_stopping_being_hologram;
        public bool is_highlight => highlighted && co_highlighting != null;
        public bool is_unhighlight => !highlighted && co_unhighlighting != null;
        public bool is_moving => co_moving != null;
        public bool is_revealing => co_revealing != null;
        public bool is_hiding => co_hiding != null;
        public bool is_coloring => co_changing_color != null;
        public bool is_flipping => co_flipping != null;
        public bool is_facing_left => facing_left;
        public bool is_facing_right => !facing_left;
        public bool is_hologramming => co_hologramming != null;
        public bool is_stopping_being_hologram => co_stopping_being_hologram != null;

        public bool is_hologram = false;

        public virtual bool IsVisible { get; set; }

        public int priority;
        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet,
            Live2D,
            Model3D
        }

        public Character(string _name, CharacterConfigData config, GameObject prefab)
        {
            this.config = config;
            this.config.name = name;
            this.name = _name;
            displayName = _name;
            this.RussianName = config.RussianName;
            
            if (prefab != null)
            {
                GameObject ob = Object.Instantiate(prefab, manager.characterPanel);
                ob.name = config.name;
                ob.SetActive(true);
                root = ob.GetComponent<RectTransform>();
                animator = root.GetComponentInChildren<Animator>();
            }
        }

        public void SetNameColor(Color color) => config.nameColor = color;
        public void SetNameFont(TMPro.TMP_FontAsset font) => config.nameFont = font;
        public void SetDialogueColor(Color color) => config.dialogueColor = color;
        public void SetDialogueFont(TMPro.TMP_FontAsset font) => config.dialogueFont = font;

        public void ResetConfigurationData() => config = CharacterManager.instance.GetCharacterConfig(name, getOriginal: true);

        public void UpdateTextCustomizationOnScreen() => DialogueSystem.ApplySpeakerDataToDialogueContainer(config);

        public Coroutine Say(string dialogue) => Say(new List<string> { dialogue });

        public Coroutine Say(List<string> dialogue)
        {
            DialogueSystem.ShowName(displayName);
            DialogueSystem.ShowImage(name);
            UpdateTextCustomizationOnScreen();
            return DialogueSystem.Say(dialogue);
        }

        public virtual Coroutine Show()
        {
            if (is_revealing) manager.StopCoroutine(co_revealing);
            if (is_hiding) manager.StopCoroutine(co_hiding);
            co_revealing = manager.StartCoroutine(ShowingOrHiding(true));
            return co_revealing;
        }

        public virtual Coroutine Hide()
        {
            if (is_hiding)  manager.StopCoroutine(co_hiding);
            if (is_revealing) manager.StopCoroutine(co_revealing);
            co_hiding = manager.StartCoroutine(ShowingOrHiding(false));
            
            return co_hiding;
        }

        public virtual void SetPosition(Vector2 pos)
        {
            if (root == null) return;
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUiTargetPositionToRelativeCharacterTargetAnchors(pos);
            root.anchorMin = minAnchorTarget;
            root.anchorMax = maxAnchorTarget;
            targetPosition = pos;
        }

        public virtual Coroutine MoveToPosition(Vector2 position, float speed = 2f, bool smooth = false)
        {
            if (root == null) return null;
            if (is_moving) manager.StopCoroutine(co_moving);
            co_moving = manager.StartCoroutine(MovingToPosition(position, speed, smooth));
            targetPosition = position;
            return co_moving;
        }

        public IEnumerator MovingToPosition(Vector2 position, float speed, bool smooth)
        {
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUiTargetPositionToRelativeCharacterTargetAnchors(position);
            Vector2 padding = root.anchorMax - root.anchorMin;
            while (root.anchorMin != minAnchorTarget || root.anchorMax != maxAnchorTarget)
            {
                root.anchorMin = smooth ?
                    Vector2.Lerp(root.anchorMin, minAnchorTarget, speed * Time.deltaTime)
                    : Vector2.MoveTowards(root.anchorMin, minAnchorTarget, speed * Time.deltaTime * 0.35f);
                root.anchorMax = root.anchorMin + padding;
                if (smooth && Vector2.Distance(root.anchorMin, minAnchorTarget) <= 0.001f)
                {
                    root.anchorMin = minAnchorTarget;
                    root.anchorMax = maxAnchorTarget;
                    break;
                }
                yield return null;
            }
            //Debug.Log("Done moving");

        }

        public void LightUp()
        {
            manager.SelectSpeaker(this);
        }

        protected (Vector2, Vector2) ConvertUiTargetPositionToRelativeCharacterTargetAnchors(Vector2 position)
        {
            Vector2 padding = root.anchorMax - root.anchorMin;
            float maxX = 1f - padding.x;
            float maxY = 1f - padding.y;
            Vector2 minAnchorTarget = new Vector2(maxX * position.x, maxY * position.y);
            Vector2 maxAnchorTarget = minAnchorTarget + padding;
            return (minAnchorTarget, maxAnchorTarget);
        }

        public virtual IEnumerator BeHologram(int idx = 0)
        {
            Debug.Log("can't be virtual");
            return null;
        }

        public virtual IEnumerator StopBeingHologram(int idx = 0)
        {
            Debug.Log("can't be virtual");
            return null;
        }

        public virtual void SetColor(Color color)
        {
            this.color = color;
        }

        public virtual IEnumerator ShowingOrHiding(bool show)
        {
            yield return null;
        }

        public Coroutine TrasitionColor(Color color, float speed = 1)
        {
            this.color = color;
            if (is_coloring) manager.StopCoroutine(co_changing_color);

            co_changing_color = manager.StartCoroutine(ChangingColor(DisplayColor, speed));

            return co_changing_color;
        }

        public Coroutine Hologram()
        {
            this.color = HoloColor;
            if (is_hologramming) return co_hologramming;
            if (is_stopping_being_hologram) manager.StopCoroutine(co_stopping_being_hologram); 
            co_hologramming = manager.StartCoroutine(BeHologram());
            return co_hologramming;

        }

        public Coroutine NotHologram()
        {
            this.color = DisplayColor;
            if (!is_hologramming) return co_stopping_being_hologram;
            if (is_stopping_being_hologram) manager.StopCoroutine(co_hologramming);
            
            co_stopping_being_hologram = manager.StartCoroutine(StopBeingHologram());

            return co_stopping_being_hologram;

        }

        public virtual IEnumerator ChangingColor(Color color, float speed, int idx = 0)
        {
            Debug.Log("CAN'T BE ON TEXT");
            yield return null;
        }

        public Coroutine Highlight(float speed = 1f, int idx = 0, bool immediate = false)
        {
            if (is_highlight) manager.StopCoroutine(co_highlighting);
            if (is_unhighlight) manager.StopCoroutine(co_unhighlighting);
            highlighted = true;
            co_highlighting = manager.StartCoroutine(Highlighting(highlighted, speed, idx, immediate));
            return co_highlighting;
        }
        public Coroutine UnHighlight(float speed = 1f, int idx = 0, bool immediate = false)
        {
            if (is_unhighlight) manager.StopCoroutine(co_unhighlighting);
            if (is_highlight) manager.StopCoroutine(co_highlighting);
            highlighted = false;
            co_unhighlighting = manager.StartCoroutine(Highlighting(highlighted, speed, idx, immediate));
            return co_unhighlighting;
        }

        public Coroutine Flip(float speed = 1, bool immadeiat = false)
        {
            if (is_facing_left) return FaceRight();
            else return FaceLeft();
        }
        public Coroutine FaceLeft(float speed = 3, bool immediate = false)
        {
            if (is_flipping) manager.StopCoroutine(co_flipping);
            facing_left = true;
            co_flipping = manager.StartCoroutine(FaceDirection(facing_left, speed, immediate));
            return co_flipping;
        }
        public Coroutine FaceRight(float speed = 3, bool immediate = false)
        {
            if (is_flipping) manager.StopCoroutine(co_flipping);
            facing_left = false ;
            co_flipping = manager.StartCoroutine(FaceDirection(facing_left, speed, immediate));
            return co_flipping;
        }

        public virtual IEnumerator FaceDirection(bool faceleft, float speedMultiplier, bool immediate, int idx = 0)
        {
            Debug.Log("VIRTUAL");
            yield return null;
        }
        public virtual IEnumerator Highlighting(bool highlight, float speedMultiplier, int idx = 0, bool immediate = false)
        {
            Debug.Log("isn't availible for text");
            yield return null;
        }

        public void SetPriority(int priority, bool autoSort = true)
        {
            this.priority = priority;
            if (autoSort) manager.SortCharacters();
            root.SetSiblingIndex(priority);
        }

        public void Animate(string animationName)
        {
            animator.SetTrigger(animationName);
        }

        public void Animate(string animationName, bool state)
        {
            animator.SetBool(animationName, state);
            animator.SetTrigger(ANIMATION_REFRESH_TRIGGER);
        }

        public virtual void OnReceiveExpressions(string expression, int layer = 1)
        {
            return;
        }
    }
}