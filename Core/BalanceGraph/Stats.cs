using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BALANCE
{
    public class Stats
    {
        public event EventHandler OnStatsChanged;

        public static int StatMax = 15;
        public static int StatMin = 5;

        private SingleStat Hope;
        private SingleStat Logic;
        private SingleStat Emotions;
        private SingleStat Loyality;
        private SingleStat Purpose;

        public enum StatType
        {
            None,
            Hope,
            Logic,
            Emotions,
            Loyality,
            Purpose
        }

        public Stats(float hopeCount, float logicCount, float emotionsCount, float loyalityCount, float purposeCount)
        {
            Hope = new SingleStat(hopeCount);
            Logic = new SingleStat(logicCount);
            Emotions = new SingleStat(emotionsCount);
            Loyality = new SingleStat(loyalityCount);
            Purpose = new SingleStat(purposeCount);
        }

        private SingleStat GetSingleStat(StatType type)
        {
            switch(type)
            {
                case StatType.Hope:
                    return Hope;
                case StatType.Logic:
                    return Logic;
                case StatType.Emotions:
                    return Emotions;
                case StatType.Loyality:
                    return Loyality;
                case StatType.Purpose:
                    return Purpose;
                default:
                    return null;
            }
        }

        public StatType TryGetType(string name)
        {
            switch (name)
            {
                case "Hope":
                case "hope":
                    return StatType.Hope;
                case "Logic":
                case "logic":
                    return StatType.Logic;
                case "Loyality":
                case "loyality":
                    return StatType.Loyality;
                case "Purpose":
                case "purpose":
                    return StatType.Purpose;
                case "Emotions":
                case "emotions":
                    return StatType.Emotions;
                default:
                    return StatType.None;
            }
        }

        public void SetStatAmmount(StatType type, float value)
        {
            GetSingleStat(type).SetStatAmmount(value);
            if (OnStatsChanged != null) OnStatsChanged(this, EventArgs.Empty);
        }

        public float GetStatNormalized(StatType type) => (float)GetSingleStat(type).GetStat() / StatMax;

        public float GetStat(StatType type) => GetSingleStat(type).GetStat();

        public void Increase(StatType type) => GetSingleStat(type).Increase();
        public void Decrease(StatType type) => GetSingleStat(type).Decrease();

        private class SingleStat
        {
            public SingleStat(float stat) => SetStatAmmount(stat);
            private float stat;
            public void SetStatAmmount(float value)
            {
                stat = Mathf.Clamp(value, StatMin, StatMax);
            }

            public float GetHopeStatNormalized() => (float)stat / StatMax;

            public float GetStat()
            {
                return stat;
            }

            public void Increase() => stat += 1;
            public void Decrease() => stat += 1;
        }
    }
}