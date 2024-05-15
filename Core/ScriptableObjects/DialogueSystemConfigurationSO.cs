using UnityEngine;
using CHARACTERS;

namespace DIALOGUE
{
    [CreateAssetMenu(fileName = "Dialogue System Configuration", menuName = "Dialogue System/Dialogue Configuration Asset")]
    public class DialogueSystemConfigurationSO : ScriptableObject
    {
        public CharacterConfigSO characterConfig;

        public Color defaultColor = Color.white;
        public TMPro.TMP_FontAsset defaultFont;

        //public float defaultnameFontSize = 18;
        //public float defaultFontSize = 12;
    }
}