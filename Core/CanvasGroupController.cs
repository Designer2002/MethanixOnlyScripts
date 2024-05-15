using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class CanvasGroupController
    {
        private const float FADE_SPEED = 2f;
        private MonoBehaviour owner;
        private CanvasGroup canvas;

        private Coroutine co_showing = null;
        private Coroutine co_hiding = null;

        public bool is_showing => co_showing != null;
        public bool is_hiding => co_hiding != null;
        public bool is_fading => is_showing || is_hiding;

        public bool is_Visible => co_showing != null || canvas.alpha != 0;
        public float alpha {
                                get { return canvas.alpha; }
                                set { canvas.alpha = value; }
                            }

        public CanvasGroupController(MonoBehaviour owner, CanvasGroup canvas)
        {
            this.canvas = canvas;
            this.owner = owner;
        }

        public Coroutine MakeFaded()
        {
            if (is_showing) return co_showing;
            else if (is_hiding)
            {
                DialogueSystem.instance.StopCoroutine(co_hiding);
                co_hiding = null;
            }
            co_showing = DialogueSystem.instance.StartCoroutine(Fading(0.3f));
            return co_showing;
        }

        public Coroutine Show()
        {
            if (is_showing) return co_showing;
            else if (is_hiding)
            {
                DialogueSystem.instance.StopCoroutine(co_hiding);
                co_hiding = null;
            }
            co_showing = DialogueSystem.instance.StartCoroutine(Fading(1));
            return co_showing;
        }
        public Coroutine Hide()
        {
            if (is_hiding) return co_hiding;
            else if (is_showing)
            {
                DialogueSystem.instance.StopCoroutine(co_showing);
                co_showing = null;
            }
            co_hiding = DialogueSystem.instance.StartCoroutine(Fading(0));
            return co_hiding;
        }

        private IEnumerator Fading(float target)
        {
            CanvasGroup cg = canvas;
            while (cg.alpha != target)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, target, FADE_SPEED * Time.deltaTime);
                yield return null;
            }
            co_hiding = null;
            co_showing = null;
        }
        public void SetInteractableState(bool active)
        {
            canvas.interactable = active;
            canvas.blocksRaycasts = active;
        }
    }
}