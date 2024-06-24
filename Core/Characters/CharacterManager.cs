using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CHARACTERS
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; }
        private static Dictionary<string, Character> characters = new Dictionary<string, Character>();
        public Character[] allCharacters => characters.Values.ToArray();
        private CharacterConfigSO config => DIALOGUE.DialogueSystem.instance.characterConfigurationAsset;

        [SerializeField] 
        private RectTransform _characterPanel;
        public RectTransform characterPanel => _characterPanel;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            characters.Clear();
        }

        private const string CHARACTER_NAME_ID = "<charname>";
        public const string CHARACTER_CASTING_ID = " as ";
       
        private string RootPath => $"Characters/{CHARACTER_NAME_ID}";

        public string characterPrefabPath => $"{RootPath}/{CHARACTER_NAME_ID}";

        public string FormatCharacterPath(string characterName, string path) => path.Replace(CHARACTER_NAME_ID, characterName);

        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(characterName, characterPrefabPath);
            //Debug.Log($"PREFAB PATH - {prefabPath}");
            return Resources.Load<GameObject>(prefabPath);
        }

        public bool HasCharacter(string name)
        {
            return characters.ContainsKey(name.ToLower());
        }

        public Character CreateCharacter(string characterName, bool revealAfterCreation = false)
        {
            if (characters.ContainsKey(characterName))
            {
                //Debug.Log($"{characterName} is already here");
                return null;
            }
            CHARACTER_INFO infp = GetCharacterInfo(characterName);
            Character character = CreateCharacterFromInfo(infp);
            if(infp.castingName != infp.name)
            {
                character.castingName = infp.name;
            }
            characters.Add(infp.name.ToLower(), character);
            //Debug.Log($"Created character {infp.config.characterType.ToString()} - {infp.name}");
            if (revealAfterCreation) character.Show();
           // Debug.Log(character.IsVisible);
            return character;
            
        }

        public CharacterConfigData GetCharacterConfig(string characterName, bool getOriginal = false)
        {
            if (!getOriginal)
            {
                Character c = GetCharacter(characterName);
                if (c != null) return c.config;
                
            }
            return config.GetConfig(characterName);
        }

        public void UnhighlightAll()
        {
            foreach (var c in characters.Values)
                c.UnHighlight();
        }

        private CHARACTER_INFO GetCharacterInfo(string characterName)
        {
            CHARACTER_INFO result = new CHARACTER_INFO();
            //string[] nameData = characterName.Split(CHARACTER_CASTING_ID, System.StringSplitOptions.RemoveEmptyEntries);

            result.name = NameData(characterName)[0];
            result.castingName = NameData(characterName).Length > 1 ? NameData(characterName)[1] : result.name;
            //Debug.Log("cast - " + result.castingName);
            //Debug.Log(result.name);
            result.config = config.GetConfig(result.castingName);
            result.prefab = GetPrefabForCharacter(result.castingName);
            result.rootCharacterFolder = FormatCharacterPath(result.castingName, RootPath);
            return result;
        }

        private string[] NameData(string name)
        {
            if (name.Contains(CHARACTER_CASTING_ID))
            {
                int startAss = name.IndexOf(CHARACTER_CASTING_ID);
                string onlyName = name.Substring(0, startAss);
                string elseShit = name.Substring(name.LastIndexOf(CHARACTER_CASTING_ID), name.Length - 1 - (onlyName.Length - 1));
                elseShit = elseShit.Replace(CHARACTER_CASTING_ID, "");
                //Debug.Log($"1 - {elseShit}");
                //Debug.Log($"2 - {onlyName}");
                return new[] { onlyName, elseShit };
            }
            else return new[] { name };
        }

        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            CharacterConfigData config = info.config;
            switch (config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.name, config);
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, config, info.prefab, info.rootCharacterFolder);
                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name, config, info.prefab, info.rootCharacterFolder);
                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name, config, info.prefab, info.rootCharacterFolder);

                default:
                    return null;
            }
            
        }

        public Character GetCharacter(string characterName, bool createIfNotExist = false)
        {
            if (characters.ContainsKey(characterName.ToLower())) return characters[characterName.ToLower()];
            else if(createIfNotExist)
            {
                return CreateCharacter(characterName);
            }
            return null;
        }

        public string FindByAlias(string alias)
        {
            if (GetCharacterConfig(alias) != CharacterConfigData.Default)
            {
                var c = GetCharacterConfig(alias).name;
                return c;
            }
            else return string.Empty;
        }
        public void SortCharacters()
        {
            List<Character> activeCharacters = characters.Values.Where(c => c.root.gameObject.activeInHierarchy && c.IsVisible).ToList();
            List<Character> inactiveCharacters = characters.Values.Except(activeCharacters).ToList();

            activeCharacters.Sort((a, b) => a.priority.CompareTo(b.priority));
            activeCharacters.Concat(inactiveCharacters);
            SortCharacters(activeCharacters);
        }
        private void SortCharacters(List<Character> charactersSortingOrder)
        {
            int i = 0;
            foreach (Character character in charactersSortingOrder)
            {
                character.root.SetSiblingIndex(i++);
            }
        }
        public void SortCharacters(string[] CharacterNames)
        {
            List<Character> sortedCharacters = new List<Character>();
            sortedCharacters = CharacterNames.Select(name => GetCharacter(name)).Where(character => character != null).ToList();
            List<Character> remainingCharacters = characters.Values.Except(sortedCharacters).OrderBy(character => character.priority).ToList();
            sortedCharacters.Reverse();
            int startingPriority = remainingCharacters.Count > 0 ? remainingCharacters.Max(c => c.priority) : 0;
            for (int i = 0; i < sortedCharacters.Count; i++)
            {
                Character character = sortedCharacters[i];
                character.SetPriority(startingPriority + i + 1, autoSort: false);
            }
            List<Character> all = remainingCharacters.Concat(sortedCharacters).ToList();
            SortCharacters(all);
        }

        public void SelectSpeaker(Character character)
        {
            foreach(var remaining in characters.Values)
            {
                if (remaining.name == character.name)
                {
                    remaining.Highlight();
                    continue;
                }
                remaining.UnHighlight();
            }
        }
    }

    internal class CHARACTER_INFO
    {
        public string name = "";
        public string castingName = "";
        public CharacterConfigData config = null;
        public string rootCharacterFolder = "";
        public GameObject prefab = null;
    }

    
}