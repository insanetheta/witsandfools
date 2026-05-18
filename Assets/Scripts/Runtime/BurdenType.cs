namespace WitsAndFools
{
    public enum BurdenType
    {
        RattledNerves,
        HeavyPurse,
        MarkedCards,
        ClumsyFingers,
        BadReputation
    }

    public static class BurdenTypeExtensions
    {
        public static string DisplayName(this BurdenType b) => b switch
        {
            BurdenType.RattledNerves => "Rattled Nerves",
            BurdenType.HeavyPurse => "Heavy Purse",
            BurdenType.MarkedCards => "Marked Cards",
            BurdenType.ClumsyFingers => "Clumsy Fingers",
            BurdenType.BadReputation => "Bad Reputation",
            _ => b.ToString()
        };

        public static string Description(this BurdenType b) => b switch
        {
            BurdenType.RattledNerves => "First defense each bout must use your highest-rank card.",
            BurdenType.HeavyPurse => "Starting hand size is 7 instead of 6.",
            BurdenType.MarkedCards => "Opponent can see 1 random card in your hand.",
            BurdenType.ClumsyFingers => "Once per match, a random ability activation fizzles.",
            BurdenType.BadReputation => "Shop prices are 20% higher.",
            _ => ""
        };

        public static bool AffectsEngine(this BurdenType b) => b switch
        {
            BurdenType.RattledNerves => true,
            BurdenType.HeavyPurse => true,
            BurdenType.ClumsyFingers => true,
            _ => false
        };
    }
}
