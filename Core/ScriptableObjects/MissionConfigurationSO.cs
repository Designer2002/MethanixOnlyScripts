using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MISSIONS
{
    [CreateAssetMenu(fileName = "Mission Configuration Asset", menuName = "Missions/Mission Configuration Asset")]
    public class MissionConfigurationSO : ScriptableObject
    {
        public MissionConfigData[] missions;
        public  MissionConfigData GetConfig(string missionName)
        {
            missionName = missionName.ToLower();
            for (int i = 0; i < missionName.Length; i++)
            {
                MissionConfigData data = missions[i];
                string lowercode = data.CodeWord.ToLower();
                if (string.Equals(missionName, lowercode))
                {
                    //Debug.Log("Found!!");
                    return data.Copy();
                }
            }
            return MissionConfigData.Default;
        }
    }
}