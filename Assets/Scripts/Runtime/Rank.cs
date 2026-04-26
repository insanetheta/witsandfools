namespace WitsAndFools
{
    public enum Rank
    {
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public static class RankExtensions
    {
        public static string Label(this Rank rank) => rank switch
        {
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            _ => ((int)rank).ToString()
        };
    }
}
