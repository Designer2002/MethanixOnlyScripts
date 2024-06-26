using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class Page : MonoBehaviour
    {
        public enum PageType { SAVE_AND_LOAD, CONFIG, RULES, BAD_SAVE };
        public PageType pageType;

        private const string OPEN = "Open";
        private const string CLOSE = "Close";
        [SerializeField]
        public Animator anim;
        public virtual void Open()
        {
            anim.SetTrigger(OPEN);
        }

        public virtual void Close(bool closeAllMenus = false)
        {
            anim.SetTrigger(CLOSE);
            if (closeAllMenus)
            {
                VNMenuManager.instance.CloseRoot();
            }
        }
    }
}