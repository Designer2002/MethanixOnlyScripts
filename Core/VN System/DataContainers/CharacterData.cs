using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public string castingName;
        public string displayName;
        public Color color;
        public int priority;
        public bool isHighlighted;
        public bool isFacingLeft;
        public Vector2 position;
        public bool enabled;
        public string dataJSON;

        public static List<CharacterData> Capture()
        {
            List<CharacterData> characters = new List<CharacterData>();
            foreach (var ch in CharacterManager.instance.allCharacters)
            {
                CharacterData entry = new CharacterData();
                entry.characterName = ch.name;
                entry.castingName = ch.name;
                entry.displayName = ch.displayName;
                entry.color = ch.color;
                entry.isFacingLeft = ch.is_facing_left;
                entry.priority = ch.priority;
                entry.enabled = ch.IsVisible;
                entry.isHighlighted = ch.highlighted;
                entry.position = ch.targetPosition;

                switch (ch.config.characterType)
                {
                    case Character.CharacterType.Text:
                        break;
                    case Character.CharacterType.Sprite:
                    case Character.CharacterType.SpriteSheet:
                        SpriteData sd = new SpriteData();
                        sd.layers = new List<SpriteData.LayerData>();
                        Character_Sprite sc = ch as Character_Sprite;
                        foreach(var layer in sc.layers[0])
                        {
                            var layerData = new SpriteData.LayerData();
                            layerData.color = layer.renderer.color;
                            layerData.spriteName = layer.renderer.sprite.name;
                            sd.layers.Add(layerData);
                        }

                        entry.dataJSON = JsonUtility.ToJson(sd);
                        break;
                    case Character.CharacterType.Live2D:
                        break;
                    case Character.CharacterType.Model3D:
                        break;
                }
                characters.Add(entry);
            }
            return characters;
        }

        public static void Apply(List<CharacterData> data)
        {
            List<string> cache = new List<string>();
            foreach(var characterData in data)
            {
                Character character = null;
                if(characterData.castingName == string.Empty)
                {
                    character = CharacterManager.instance.GetCharacter(characterData.characterName, createIfNotExist: true);
                }
                else
                {
                    character = CharacterManager.instance.GetCharacter(characterData.characterName, createIfNotExist: false);
                    if(character == null)
                    {
                        string castingName = $"{characterData.characterName}{CharacterManager.CHARACTER_CASTING_ID}{characterData.castingName}";
                        character = CharacterManager.instance.CreateCharacter(castingName);
                    }
                }
                
                character.displayName = characterData.displayName;
                character.SetColor(characterData.color);
                if (characterData.isHighlighted)
                {
                    character.Highlight(immediate: true);
                }
                else character.UnHighlight(immediate: true);
                character.SetPriority(characterData.priority);
                if (characterData.isFacingLeft)
                {
                    character.FaceLeft(immediate: true);
                }
                else character.FaceRight(immediate: true);
                
                
                character.MoveToPosition(characterData.position);
                character.SetPosition(character.targetPosition);
                character.IsVisible = characterData.enabled;

                switch(character.config.characterType)
                {
                    case Character.CharacterType.Sprite:
                    case Character.CharacterType.SpriteSheet:
                        SpriteData sd = JsonUtility.FromJson<SpriteData>(characterData.dataJSON);
                        Character_Sprite cs = character as Character_Sprite;
                        for(int i = 0; i < sd.layers.Count; i++)
                        {
                            var layer = sd.layers[i];
                            if(cs.layers[0][i].renderer.sprite != null && cs.layers[0][i].renderer.sprite.name != layer.spriteName)
                            {
                                Sprite sprite = cs.GetSprite(layer.spriteName);
                                if (sprite != null) cs.SetSprite(sprite);
                            }
                        }
                        break;
                }
                cache.Add(character.name);
            }
            foreach(Character c in CharacterManager.instance.allCharacters)
            {
                if (!cache.Contains(c.name))
                    c.IsVisible = false;
            }
        }

        [System.Serializable]
        public class SpriteData
        {
            public List<LayerData> layers = new List<LayerData>();
            [System.Serializable]
            public class LayerData
            {
                public string spriteName;
                public Color color;
            }
        }
    }
}