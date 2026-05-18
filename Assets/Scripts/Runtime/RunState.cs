using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public enum MapNodeType
    {
        RivalMatch,
        EliteMatch,
        Shop,
        Rumor,
        Rest,
        BossMatch
    }

    public sealed class MapNode
    {
        public MapNodeType Type;
        public OpponentProfile Opponent;
        public int Column;
        public int Row;
    }

    public sealed class RunState
    {
        public int Seed;
        public int CurrentAct;
        public int CurrentColumn;
        public int Prestige = 5;
        public int Florins;
        public int Reputation;
        public int MatchesPlayed;
        public int MatchesWon;
        public int TotalBoutsPlayed;
        public int TotalBoutsDefended;

        public List<AbilityType> PlayerAbilities = new();
        public int MaxAbilitySlots = 5;
        public List<TrinketType> PlayerTrinkets = new();
        public List<BurdenType> PlayerBurdens = new();

        public List<List<MapNode>> CurrentMap;
        public bool RunComplete;
        public bool RunWon;
        public bool PhoenixMedalUsed;

        public Dictionary<AbilityType, int> AbilityUsageCount = new();
        public Dictionary<AbilityType, int> AbilityPickCount = new();

        public void RecordAbilityUsed(AbilityType a)
        {
            if (!AbilityUsageCount.ContainsKey(a)) AbilityUsageCount[a] = 0;
            AbilityUsageCount[a]++;
        }

        public void RecordAbilityPicked(AbilityType a)
        {
            if (!AbilityPickCount.ContainsKey(a)) AbilityPickCount[a] = 0;
            AbilityPickCount[a]++;
        }
    }
}
