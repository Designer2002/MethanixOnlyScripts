using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class SavingsData
    {
        public List<CharacterData> characters;
        public List<AudioData> audio;
        public List<GraphicData> graphics;

        public static SavingsData Capture()
        {
            SavingsData data = new SavingsData();
            data.audio = AudioData.Capture();
            data.characters = CharacterData.Capture();
            data.graphics = GraphicData.Capture();
            return data;
        }

        public void Load()
        {
            CharacterData.Apply(characters);
            AudioData.Apply(audio);
            GraphicData.Apply(graphics);
        }
    }
}