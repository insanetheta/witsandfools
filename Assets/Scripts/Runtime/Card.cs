using System;

namespace WitsAndFools
{
    public readonly struct Card : IEquatable<Card>
    {
        public readonly Suit Suit;
        public readonly Rank Rank;

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public bool Equals(Card other) => Suit == other.Suit && Rank == other.Rank;
        public override bool Equals(object obj) => obj is Card other && Equals(other);
        public override int GetHashCode() => ((int)Suit * 16) ^ (int)Rank;
        public static bool operator ==(Card a, Card b) => a.Equals(b);
        public static bool operator !=(Card a, Card b) => !a.Equals(b);

        public override string ToString() => $"{Rank.Label()}{Suit.Glyph()}";
    }
}
