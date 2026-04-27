using System.Collections.Generic;

namespace WitsAndFools
{
    public static class DeckConfig
    {
        // 10 of 36 cards carry abilities (~1 in 3.6).
        // Spread across ranks and suits to avoid predictability.
        // Tuned during balance playtest (witsandfools-zqm.7).
        public static readonly Dictionary<(Suit, Rank), AbilityType> DefaultAbilities = new()
        {
            // Trump Changer x2 — court + low card for interesting tension
            { (Suit.Hearts, Rank.Jack),  AbilityType.TrumpChanger },
            { (Suit.Diamonds, Rank.Seven), AbilityType.TrumpChanger },

            // Double Trouble x2 — high + mid card
            { (Suit.Spades, Rank.King),  AbilityType.DoubleTrouble },
            { (Suit.Clubs, Rank.Nine),   AbilityType.DoubleTrouble },

            // The Blocker x2
            { (Suit.Clubs, Rank.Queen),  AbilityType.Blocker },
            { (Suit.Diamonds, Rank.Ten), AbilityType.Blocker },

            // Extra Draw x2
            { (Suit.Hearts, Rank.King),  AbilityType.ExtraDraw },
            { (Suit.Clubs, Rank.Eight),  AbilityType.ExtraDraw },

            // Double Defense x1
            { (Suit.Diamonds, Rank.Queen), AbilityType.DoubleDefense },

            // Seize Initiative x1
            { (Suit.Spades, Rank.Jack),  AbilityType.SeizeInitiative },
        };
    }
}
