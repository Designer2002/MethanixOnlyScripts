using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    public class DialogueContinuationPrompt : MonoBehaviour
    {
        private RectTransform root;
        [SerializeField]
        private Animator anim;
        [SerializeField]
        private TextMeshProUGUI tmpro;
        private Sprite sprite;
        private Sprite[] sprites;

        public bool is_showing => anim.gameObject.activeSelf;
        void Start()
        {
            sprites = Resources.LoadAll<Sprite>("GUI/DialogueContinuationPrompt");
            root = GetComponent<RectTransform>();
        }


        public void Show()
        {
            if (tmpro.text == string.Empty)
            {
                if (is_showing) Hide();
                return;
            }
            tmpro.ForceMeshUpdate();
            anim.gameObject.SetActive(true);
            root.transform.SetParent(tmpro.transform);

            TMP_CharacterInfo finalCharacter = tmpro.textInfo.characterInfo[tmpro.textInfo.characterCount - 1];
            Vector3 targetPose = finalCharacter.bottomRight;
            float characterWidth = finalCharacter.pointSize * 0.5f;
            targetPose = new Vector3(targetPose.x + characterWidth, targetPose.y + root.rect.height, 0);
            root.localPosition = targetPose;

        }

        public void Hide()
        {
            anim.gameObject.SetActive(false);
        }

        public void Change()
        {
            System.Random r = new System.Random();
            sprite = sprites[r.Next(0, sprites.Length - 1)];
            root.GetComponentInChildren<UnityEngine.UI.Image>().sprite = sprite;
        }

    }
}