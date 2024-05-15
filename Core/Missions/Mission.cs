using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MISSIONS
{
    public class Mission
    {
        public string Name = "";
        public string codeWord = "";
        public MisStat Stat 
        {
            get
            {
                return _stat;
            }
            set
            {
                _stat = value;
                if (progressCurrent == progressMax)
                {
                    GiveReward();
                }
            }
        }
        private MisStat _stat;
        public bool isActive => Stat != MisStat.Locked || Stat != MisStat.Failed || Stat != MisStat.Finished;
        public bool isVisible => Stat != MisStat.Locked;
        public int progressCurrent;
        public int progressMax;
        public string description;
        public Sprite icon;
        public MissionConfigData config;

        public Mission(MissionConfigData config)
        {
            this.codeWord = config.CodeWord.ToLower();
            this.Name = config.Name;
            this.progressMax = config.ProgressMax;
            this.progressCurrent = config.ProgressCurrent;
            this.description = config.Description;
            this.Stat = config.Status;
            this.icon = config.Icon;
        }

        public enum MisStat
        {
            Locked,
            InProcess,
            Failed,
            Finished
        }

        public IEnumerator GiveReward()
        {
            if (Stat == MisStat.Failed)
                yield break;
            VariableStore.TrySetValue(codeWord, true);
            Stat = MisStat.Finished;
            yield return DIALOGUE.NotificationPanel.instance.Display($"квест {Name} пройден! Это может изменить ход истории.");
        }

        public void Fail()
        {
            Stat = MisStat.Failed;
        }

    }
}