using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MISSIONS
{
    public class MissionManager : MonoBehaviour
    {
        private Dictionary<string, Mission> missions = new Dictionary<string, Mission>();
        public static MissionManager instance;
        public MissionConfigurationSO config;
        public int count => missions.Count;

        public List<Mission> visible => missions.Values.Where(m => m.Stat != Mission.MisStat.Locked).ToList();
        public int unlocked => visible.Count;
        public List<string> names => visible.Select(v => v.codeWord).ToList();
        // Start is called before the first frame update
        public Mission GetMission(string codeWord, bool createIfNotExist = false)
        {
            if (missions.ContainsKey(codeWord.ToLower())) return missions[codeWord.ToLower()];
            else if (createIfNotExist)
            {
                return CreateMission(codeWord);
            }
            return null;
        }

        public void UpdateMissionStatus(string codeword, Mission.MisStat stat)
        {
            missions[codeword].Stat = stat;
        }

        public MissionConfigData GetMissionConfig(string codeword, bool getOriginal = false)
        {
            if (!getOriginal)
            {
                Mission m = GetMission(codeword);
                if (m != null) return m.config;

            }
            return config.GetConfig(codeword);
        }

        public Mission CreateMission(string codeWord, int max = 1)
        {
            if (missions.ContainsKey(codeWord))
            {
                return null;
            }
            MISSION_INFO info = GetMissionInfo(codeWord);
            Mission mission = CreateMissionFromInfo(info);
            mission.progressMax = max;
            missions.Add(info.CodeWord.ToLower(), mission);
            return mission;
        }

        private Mission CreateMissionFromInfo(MISSION_INFO info) => new Mission(info.config);

        public bool HasMission(string codeWord) => missions.ContainsKey(codeWord);

        private void Awake()
        {
            instance = this;
        }

        private MISSION_INFO GetMissionInfo(string codeWord)
        {
            MISSION_INFO result = new MISSION_INFO();
            //string[] nameData = characterName.Split(CHARACTER_CASTING_ID, System.StringSplitOptions.RemoveEmptyEntries);

            result.CodeWord = codeWord;
            result.config = config.GetConfig(result.CodeWord);
            return result;
        }

        internal class MISSION_INFO
        {
            public string CodeWord = "";
            public MissionConfigData config = null;
        }
    }
}