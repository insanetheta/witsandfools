using System;

namespace WitsAndFools
{
    public readonly struct Card : IEquatable<Card>
    {
        public readonly Suit Suit;
        public readonly Rank Rank;
        public readonly AbilityType? Ability;

        public Card(Suit suit, Rank rank, AbilityType? ability = null)
        {
            Suit = suit;
            Rank = rank;
            Ability = ability;
        }

        public bool HasAbility => Ability.HasValue;

        // Equality is Suit+Rank only — ability is metadata, not identity.
        // Every Suit+Rank combo is unique in a 36-card deck.
        public bool Equals(Card other) => Suit == other.Suit && Rank == other.Rank;
        public override bool Equals(object obj) => obj is Card other && Equals(other);
        public override int GetHashCode() => ((int)Suit * 16) ^ (int)Rank;
        public static bool operator ==(Card a, Card b) => a.Equals(b);
        public static bool operator !=(Card a, Card b) => !a.Equals(b);

        public override string ToString() => Ability.HasValue
            ? $"{Rank.Label()}{Suit.Glyph()} [{Ability.Value.ShortName()}]"
            : $"{Rank.Label()}{Suit.Glyph()}";
    }
}
