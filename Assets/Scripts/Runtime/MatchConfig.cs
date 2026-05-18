using System.Collections.Generic;

namespace WitsAndFools
{
    public sealed class MatchConfig
    {
        public int HandSize = Rules.HandSizeTwoPlayer;
        public int MaxAttacksPerBout = Rules.MaxAttacksPerBout;

        public Dictionary<(Suit, Rank), AbilityType> Abilities;
        public Dictionary<(Suit, Rank), int> AbilityOwners;

        public bool[] TrumpAffinity = new bool[2];
        public bool[] EndgameSpecialist = new bool[2];
        public bool[] QuickHands = new bool[2];
        public bool[] CardCounter = new bool[2];

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

        public bool[] RattledNerves = new bool[2];
        public bool[] ClumsyFingers = new bool[2];

        // Archetype passive perks
        public bool[] ShadowReflexes = new bool[2];  // Rogue: defend with matching rank = draw 1
        public bool[] BruteFury = new bool[2];        // Brute: when defender eats, attacker draws 1 extra
        public bool[] CourtFavor = new bool[2];       // Diplomat: at bout start, peek top 2, put 1 on bottom
        public bool[] LuckyDraw = new bool[2];        // Gambler: once per match after eating, discard worst card

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
