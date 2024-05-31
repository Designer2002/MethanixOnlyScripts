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
        public List<SearchingObjectsData> searchingObjects;
        public StatsData stats;
        public GoalData goal;

        public static SavingsData Capture()
        {
            SavingsData data = new SavingsData();
            data.audio = AudioData.Capture();
            data.characters = CharacterData.Capture();
            data.graphics = GraphicData.Capture();
            data.goal = GoalData.Capture();
            data.searchingObjects = SearchingObjectsData.Capture();
            data.stats = StatsData.Capture();
            return data;
        }

        public void Load()
        {
            CharacterData.Apply(characters);
            AudioData.Apply(audio);
            GraphicData.Apply(graphics);
            StatsData.Apply(stats);
            SearchingObjectsData.Apply(searchingObjects);
            GoalData.Apply(goal);
        }
    }
}