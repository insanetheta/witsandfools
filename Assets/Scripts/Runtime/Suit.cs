namespace WitsAndFools
{
    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public static class SuitExtensions
    {
        public static string Glyph(this Suit suit) => suit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => "?"
        };

        public static bool IsRed(this Suit suit) => suit == Suit.Hearts || suit == Suit.Diamonds;
    }
}
