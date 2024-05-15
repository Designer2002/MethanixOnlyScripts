namespace MISSIONS
{
    [System.Serializable]
    public class MissionConfigData
    {
        public string CodeWord;
        public string Name;
        public string Description;
        public Mission.MisStat Status;
        public int ProgressCurrent;
        public int ProgressMax;
        public UnityEngine.Sprite Icon;


        public MissionConfigData Copy()
        {
            MissionConfigData result = new MissionConfigData();
            result.Description = Description;
            result.CodeWord = CodeWord.ToLower();
            result.Status = Status;
            result.ProgressCurrent = ProgressCurrent;
            result.ProgressMax = ProgressMax;
            result.Name = Name;
            result.Icon = Icon;

            return result;
        }

        public static MissionConfigData Default => new MissionConfigData { Description = "", Name = "", CodeWord = "", ProgressCurrent = 0, ProgressMax = 0, Status = Mission.MisStat.Locked, Icon = null };
    }
}