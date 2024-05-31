using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatsData
{
    public float Hope = 5;
    public float Logic = 5;
    public float Emotions = 5;
    public float Loyality = 5;
    public float Purpose = 5;

    public static StatsData Capture()
    {
        return new StatsData()
        {
            Hope = BALANCE.BalanceGraph.instance.HopeStat,
            Loyality = BALANCE.BalanceGraph.instance.LoyalityStat,
            Logic = BALANCE.BalanceGraph.instance.LogicStat,
            Purpose = BALANCE.BalanceGraph.instance.PurposeStat,
            Emotions = BALANCE.BalanceGraph.instance.EmotionStat
        };
    }
    public static void Apply(StatsData data)
    {
        BALANCE.BalanceGraph.instance.SetStats(new BALANCE.Stats(data.Hope, data.Logic, data.Emotions, data.Loyality, data.Purpose));
    }
}

