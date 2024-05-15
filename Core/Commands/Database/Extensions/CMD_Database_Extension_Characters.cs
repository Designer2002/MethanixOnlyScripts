using CHARACTERS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COMMANDS
{
    public class CMD_Database_Extension_Characters : CMD_DatabseExtensions
    {
        private static string[] IMMEDIATE = new[] { "-i", "-immediat" };
        private static string[] ENABLED = new[] { "-e", "-enabled" };
        private static string XPOS =>  "-x";
        private static string YPOS =>  "-y";
        private static string[] SPEED => new[] { "-speed", "-spd" };
        private static string[] SMOOTH => new[] { "-s", "-smooth" };
        private static string[] PAINT => new[] { "-p", "-paint", "paint" };
        private static string ON => "-power";

        private static string CHARACTER_TRAIT => "-trait";

        private static string LAYER => "-l";
        private static string[] ONLY => new[] { "-p", "-only" };
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));
            database.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));
            database.AddCommand("create", new Action<string[]>(CreateCharacter));
            database.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));
            database.AddCommand("sort", new Action<string[]>(Sort));
            database.AddCommand("lighten", new Func<string[], IEnumerator>(HighlightAll));
            database.AddCommand("darken", new Func<string[], IEnumerator>(UnHighlightAll));

            //CMD_Database_Extension_General.Extend(database);

            CommandDatabase baseCommands = CommandManager.instance.CreateDatabse(CommandManager.DATABASE_CHARACTER_BASE);
            baseCommands.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));
            baseCommands.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));
            baseCommands.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));
            baseCommands.AddCommand("create", new Action<string[]>(CreateCharacter));
            baseCommands.AddCommand("prioritize", new Action<string[]>(SetPriority));
            baseCommands.AddCommand("paint", new Func<string[], IEnumerator>(SetColor));
            baseCommands.AddCommand("lighten", new Func<string[], IEnumerator>(Highlight));
            baseCommands.AddCommand("darken", new Func<string[], IEnumerator>(UnHighlight));
            baseCommands.AddCommand("hologram", new Func<string[], IEnumerator>(BeHologram));
            baseCommands.AddCommand("say", new Func<string[], IEnumerator>(Say));
            baseCommands.AddCommand("hop", new Action<string>(Hop));
            baseCommands.AddCommand("shiver", new Action<string[]>(Shiver));
            baseCommands.AddCommand("faceleft", new Action<string>(FaceLeft));
            baseCommands.AddCommand("faceright", new Action<string>(FaceRight));
            baseCommands.AddCommand("setsprite", new Func<string[], IEnumerator>(SetSprite));

            baseCommands.AddCommand("decrease", new Func<string, IEnumerator>(SetNegativeAttitude));
            baseCommands.AddCommand("increase", new Func<string, IEnumerator>(SetPositiveAttitude));

            baseCommands.AddCommand("personality", new Func<string[], IEnumerator>(SetCharacterTraits));
        }

        private static IEnumerator SetCharacterTraits(string[] data)
        {
            yield return null;
            var c = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false);
            string trait;
            if (c == null || data.Length < 1 || c.name != "Dave") yield break;
            List<string> newdata = data.ToList();
            newdata.Insert(1, CHARACTER_TRAIT);
            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(CHARACTER_TRAIT, out trait);
            if (BALANCE.BalanceGraph.instance.HasTrait(trait)) yield return BALANCE.BalanceGraph.instance.Display(trait);
        }

        private static IEnumerator SetNegativeAttitude(string data)
        {
            var c = CharacterManager.instance.GetCharacter(data, createIfNotExist: false);
            if (c == null) yield break;
            object attitude;
            VariableStore.TryGetValue($"{c.name}.attitude", out attitude);
            Debug.Log(int.Parse(attitude.ToString()));
            AttitudePanel.instance.Value = int.Parse(attitude.ToString());
            VariableStore.TrySetValue($"{c.name}.attitude", int.Parse(attitude.ToString()) - 1);
            yield return DIALOGUE.NotificationPanel.instance.Display($"{ c.RussianName} ухудшил отношение к тебе.");
            yield return AttitudePanel.instance.Display(false);
        }

        private static IEnumerator SetPositiveAttitude(string data)
        {
            var c = CharacterManager.instance.GetCharacter(data, createIfNotExist: false);
            if (c == null) yield break;
            object attitude;
            VariableStore.TryGetValue($"{c.name}.attitude", out attitude);
            AttitudePanel.instance.Value = int.Parse(attitude.ToString()); 
            VariableStore.TrySetValue($"{c.name}.attitude", int.Parse(attitude.ToString()) + 1);
            yield return DIALOGUE.NotificationPanel.instance.Display($"{c.RussianName} улучшил отношение к тебе!");
            yield return AttitudePanel.instance.Display(true);
        }

        private static IEnumerator SetSprite(string[] data)
        {
            var c = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false) as Character_Sprite;
            string spriteName;
            float speed;
            int layer;
            bool immediate;
            if (c == null || data.Length < 2) yield break;
            List<string> newdata = data.ToList();
            newdata.Insert(1, "-p");

            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(PAINT, out spriteName);
            parameters.TryGetValue(LAYER, out layer);
            bool sspeed = parameters.TryGetValue(SPEED, out speed, defaultValue: 1f);
            if (!sspeed)
            {
                parameters.TryGetValue(IMMEDIATE, out immediate, defaultValue: true);
            }
            else
            {
                immediate = false;
            }
            Sprite sprite = c.GetSprite(spriteName);

            if (immediate) c.SetSprite(sprite, layer);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { c?.SetSprite(sprite); });
                c.TransitionSprite(sprite, (int)speed);
            }
            yield break;
        }

        private static void FaceRight(string data)
        {
            var c = CharacterManager.instance.GetCharacter(data, createIfNotExist: false);
            if (c == null) return;
            c.FaceLeft();
        }

        private static void FaceLeft(string data)
        {
            var c = CharacterManager.instance.GetCharacter(data, createIfNotExist: false);
            if (c == null) return;
            c.FaceRight();
        }

        private static IEnumerator HighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool handle_unspecified_characters = true;
            bool immediate = false;
            List<Character> unspecified_characters = new List<Character>();

            for (int i = 0; i < data.Length; i++)
            {
                Character c = CharacterManager.instance.GetCharacter(data[i], createIfNotExist: false);
                if (c != null) characters.Add(c);
            }
            if (characters.Count == 0) yield break;
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(ONLY, out handle_unspecified_characters, defaultValue: true);
            foreach(Character c in characters)
            {
                c.Highlight(immediate: immediate);
            }
            if(handle_unspecified_characters)
            {
                foreach(Character c in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(c)) continue;
                    unspecified_characters.Add(c);
                    c.UnHighlight(immediate: immediate);
                }
            }
            if(!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var c in characters) c.Highlight(immediate: true);
                    if (!handle_unspecified_characters) return;
                    foreach (var c in unspecified_characters) c.UnHighlight(immediate: true);
                });
            }
            while (characters.Any(c => c.is_highlight) && (handle_unspecified_characters && unspecified_characters.Any(us => us.is_unhighlight)))
            {
                yield return null;
            }

        }

        private static IEnumerator UnHighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool handle_unspecified_characters = true;
            bool immediate = false;
            List<Character> unspecified_characters = new List<Character>();

            for (int i = 0; i < data.Length; i++)
            {
                Character c = CharacterManager.instance.GetCharacter(data[i], createIfNotExist: false);
                if (c != null) characters.Add(c);
            }
            if (characters.Count == 0) yield break;
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(ONLY, out handle_unspecified_characters, defaultValue: true);
            foreach (Character c in characters)
            {
                c.UnHighlight(immediate: immediate);
            }
            if (handle_unspecified_characters)
            {
                foreach (Character c in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(c)) continue;
                    unspecified_characters.Add(c);
                    c.Highlight(immediate: immediate);
                }
            }
            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var c in characters) c.UnHighlight(immediate: true);
                    if (!handle_unspecified_characters) return;
                    foreach (var c in unspecified_characters) c.Highlight(immediate: true);
                });
            }
            while (characters.Any(c => c.is_unhighlight) && (handle_unspecified_characters && unspecified_characters.Any(us => us.is_highlight)))
            {
                yield return null;
            }
        }

        private static void SetPriority(string[] data)
        {
            var c = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false);
            int priority;
            if (c == null || data.Length > 2) return;
            if (!int.TryParse(data[1], out priority)) priority = 0;
            c.SetPriority(priority);
        }

        public static void Sort(string[] data)
        {
            CharacterManager.instance.SortCharacters(data);
        }

        public static IEnumerator ShowAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            foreach(string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, createIfNotExist: false);
                if (character != null) characters.Add(character);
            }
            if (characters.Count == 0)
            {
                yield break;
            }
            //convert
            else
            {
                var parameters = ConvertDataToParameters(data);
                parameters.TryGetValue(IMMEDIATE, out immediate, defaultValue: false);
                foreach (Character character in characters)
                {
                    if (immediate)
                    {
                        character.IsVisible = true;
                    }
                    else
                    {
                        character.Show();
                    }
                }
                if (!immediate)
                {
                    CommandManager.instance.AddTerminationActionToCurrentProcess(() => 
                    { 
                        foreach(var c in characters)
                        {
                            c.IsVisible = true;
                        }
                    });
                    while (characters.Any(c => c.is_revealing))
                    {

                        yield return null;
                    }
                }
            }
        }
        public static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, createIfNotExist: false);
                if (character != null) characters.Add(character);
            }
            if (characters.Count == 0) yield break;
            //convert
            else
            {
                var parameters = ConvertDataToParameters(data);
                parameters.TryGetValue(IMMEDIATE, out immediate, defaultValue: false);
                foreach (var character in characters)
                {
                    if (immediate)
                    {
                        character.IsVisible = false;
                    }
                    else
                    {
                        character.Hide();
                    }
                }
                if (!immediate)
                {
                    CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                    {
                        foreach (var c in characters)
                        {
                            c.IsVisible = false;
                        }
                    });
                    while (characters.Any(c => c.is_hiding))
                    {
                        yield return null;
                    }
                }
            }
        }
        public static void CreateCharacter(string[] data)
        {
            bool immediate = false;
            string characterName = data[0];
            bool enable = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(IMMEDIATE, out enable, defaultValue: false);
            parameters.TryGetValue(ENABLED, out immediate, defaultValue: false);
            var character = CharacterManager.instance.CreateCharacter(characterName);
            if (!enable) return;
            if (immediate)
            {
                character.IsVisible = true;
            }
            else character.Show();
        }

        private static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName, createIfNotExist: false);
            if (character == null) yield break;
            float x = 1;
            float y = 0.5f;
            float speed = 1;
            bool smooth = false;
            bool immediate = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(XPOS, out x, defaultValue: 1);
            parameters.TryGetValue(YPOS, out y, defaultValue: 0.5f);
            parameters.TryGetValue(SPEED, out speed, defaultValue: 1.5f);
            parameters.TryGetValue(SMOOTH, out smooth, defaultValue: true);
            parameters.TryGetValue(IMMEDIATE, out immediate, defaultValue: false);
            Vector2 position = new Vector2(x, y);
            if (immediate) character.SetPosition(position);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.SetPosition(position); });
                yield return character.MoveToPosition(position, speed, smooth);
            }
        }

        public static IEnumerator SetColor(string[] data)
        {
            var c = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false);
            string colorName;
            float speed;
            bool immediate;
            if (c == null || data.Length < 2) yield break;
            List<string> newdata = new List<string>();
            newdata.Add("-p");
            foreach(var d in data.Reverse())
            {
                newdata.Add(d);
            }
            
            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(PAINT, out colorName);
            bool sspeed = parameters.TryGetValue(SPEED, out speed, defaultValue: 1f);
            if(!sspeed)
            {
                parameters.TryGetValue(IMMEDIATE, out immediate, defaultValue: true);
            }
            else
            {
                immediate = false;
            }
            Color color = Color.white;
            color = ColorExtension.ColorFromName(colorName);
            if (immediate) c.SetColor(color);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { c?.SetColor(color); });
                c.TrasitionColor(color, speed);
            }
            yield break;
        }

        public static IEnumerator Highlight(string[] data)
        {
            var c = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false);
            bool immediate;
            if (c == null) yield break;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(IMMEDIATE, out immediate);
            if (immediate) c.Highlight(immediate: true) ;
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { c?.Highlight(); });
                yield return c.Highlight(); 
            }
            
        }

        public static IEnumerator BeHologram(string[] data)
        {
            var newdata = data.ToList();
            newdata.Insert(1, "-power");
            bool on = false;
            var c = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false);
            if (c == null) yield break;
            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(ON, out on, defaultValue: false);
            if (on)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { c?.Hologram(); });
                yield return c.Hologram();
            }
            else if (!on)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { c?.NotHologram(); });
                yield return c.NotHologram();
            }

        }

        public static IEnumerator UnHighlight(string[] data)
        {
            var c = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false);
            bool immediate;
            if (c == null) yield break;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(IMMEDIATE, out immediate);
            if (immediate) c.UnHighlight(immediate: true);
            else
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { c?.UnHighlight(); });
                yield return c.UnHighlight();
            }

        }

        public static IEnumerator Say(string[] data)
        {
            AudioClip sound = Resources.Load<AudioClip>(FilePaths.character_voice(data[0], data[1]));
            if (sound != null) yield return AUDIO.AudioManager.instance.PlayVoice(sound);
            else Debug.LogWarning("null sound");


        }

        public static void Hop(string data)
        {
            var c = CharacterManager.instance.GetCharacter(data, createIfNotExist: false);
            if (c == null) return;
            c.Animate("Hop");
        }
        public static void Shiver(string[] data)
        {
            bool shivering;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(IMMEDIATE, out shivering, defaultValue: true);
            var c = CharacterManager.instance.GetCharacter(data[0], createIfNotExist: false);
            if (c == null || data.Length > 2) return;
            c.Animate("Shiver", shivering);
        }
    }
}