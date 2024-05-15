using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTERS
{
    [System.Serializable]
    public class CharacterConfigData
    {
        public string name;
        public string RussianName;
        public Character.CharacterType characterType;
        public string alias;

        public Color nameColor;
        public Color dialogueColor;
        
        public TMPro.TMP_FontAsset nameFont;
        public TMPro.TMP_FontAsset dialogueFont;

        //public float namefontSize;
        //public float dialoguefontSize;

        public CharacterConfigData Copy()
        {
            CharacterConfigData result = new CharacterConfigData();
            result.name = name;
            result.RussianName = RussianName;
            result.alias = alias;
            result.characterType = characterType;
            result.dialogueFont = dialogueFont;
            result.nameFont = nameFont;
            result.nameColor = new Color(nameColor.r, nameColor.g, nameColor.b);
            result.dialogueColor = new Color(dialogueColor.r, dialogueColor.g, dialogueColor.b);
            //result.dialoguefontSize = dialoguefontSize;
            //result.namefontSize = namefontSize;

            return result;
        }
        private static Color defaultColor => DIALOGUE.DialogueSystem.instance.defultColor;
        private static TMPro.TMP_FontAsset defaultFont => DIALOGUE.DialogueSystem.instance.Font;

        public static CharacterConfigData Default
        {
            get
            {
                CharacterConfigData result = new CharacterConfigData();
                result.name = "";
                result.RussianName = "";
                result.alias = "";
                result.characterType = Character.CharacterType.Text;
                result.dialogueFont = defaultFont;
                result.nameFont = defaultFont;
                result.nameColor = defaultColor;
                result.dialogueColor = defaultColor;

                //result.dialoguefontSize = DIALOGUE.DialogueSystem.instance.config.defaultFontSize;
                //result.namefontSize = DIALOGUE.DialogueSystem.instance.config.defaultnameFontSize;

                return result;
            }
        }
    }
}