using System.Collections.Generic;

namespace WitsAndFools
{
    public enum HouseRuleType
    {
        None,
        NoTrumpsBeforeDusk,
        TheGauntlet,
        HeavyHands,
        Cutthroat,
        DoubleOrNothing,
        TheMirror
    }

    public sealed class OpponentProfile
    {
        public string Name;
        public AIArchetypeName Archetype;
        public List<AbilityType> Abilities = new();
        public List<TrinketType> Trinkets = new();
        public HouseRuleType HouseRule = HouseRuleType.None;
        public int ActIndex;
        public bool IsElite;
        public bool IsBoss;
    }
}
