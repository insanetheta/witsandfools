using System.Collections.Generic;

namespace WitsAndFools
{
    public sealed class MatchConfig
    {
        public int HandSize = Rules.HandSizeTwoPlayer;
        public int MaxAttacksPerBout = Rules.MaxAttacksPerBout;

        public Dictionary<(Suit, Rank), AbilityType> Abilities;
        public Dictionary<(Suit, Rank), int> AbilityOwners;

        // Original passive abilities
        public bool[] TrumpAffinity = new bool[2];
        public bool[] EndgameSpecialist = new bool[2];
        public bool[] QuickHands = new bool[2];
        public bool[] CardCounter = new bool[2];

        // Trinkets
        public bool[] DuelistGlove = new bool[2];
        public bool[] PoisonedWine = new bool[2];
        public bool[] HereticsBrand = new bool[2];
        public bool[] ShieldBrooch = new bool[2];
        public bool[] CourtiersFan = new bool[2];
        public int? ForcedTrumpSuit;
        public bool[] JugglersBalls = new bool[2];
        public bool[] LoadedDice = new bool[2];
        public bool[] QuicksilverVial = new bool[2];
        public bool[] SpysMonocle = new bool[2];
        public bool[] MarkedDeck = new bool[2];
        public bool[] CrownOfThorns = new bool[2];

        public bool[] FoolsGold = new bool[2];
        public bool[] VentriloquistsDummy = new bool[2];

        // Burdens
        public bool[] RattledNerves = new bool[2];
        public bool[] ClumsyFingers = new bool[2];

        // Archetype resources
        public ResourceType?[] ArchetypeResource = new ResourceType?[2];

        // Archetype passive perks
        public bool[] ShadowReflexes = new bool[2];
        public bool[] BruteFury = new bool[2];
        public bool[] CourtFavor = new bool[2];
        public bool[] LuckyDraw = new bool[2];

        // New passive abilities
        public bool[] PatienceRewarded = new bool[2];
        public bool[] MarkedCards = new bool[2];
        public bool[] Undermine = new bool[2];
        public bool[] Bloodlust = new bool[2];
        public bool[] ThickSkin = new bool[2];
        public bool[] BattleHardened = new bool[2];
        public bool[] GracefulManners = new bool[2];
        public bool[] WebOfLies = new bool[2];
        public bool[] GracefulRetreat = new bool[2];
        public bool[] Equilibrium = new bool[2];
        public bool[] SharkInstinct = new bool[2];
        public bool[] Jackpot = new bool[2];
        public bool[] SleightOfMind = new bool[2];
        public bool[] ChaoticNature = new bool[2];
        public bool[] SteadyHand = new bool[2];

        // House rules
        public int NoTrumpsUntilBout;
        public bool FixedAttacker;
        public bool AnyRankAttack;
        public bool EatDrawsExtra;
        public bool MirrorAbilities;

        public static MatchConfig Default() => new()
        {
            Abilities = DeckConfig.DefaultAbilities,
            AbilityOwners = null
        };
    }
}
