namespace WitsAndFools
{
    public enum AbilityType
    {
        TrumpChanger,
        ExtraDraw,
        Blocker,
        DoubleTrouble,
        DoubleDefense,
        SeizeInitiative
    }

    public static class AbilityTypeExtensions
    {
        public static string DisplayName(this AbilityType ability) => ability switch
        {
            AbilityType.TrumpChanger => "Trump Changer",
            AbilityType.ExtraDraw => "Extra Draw",
            AbilityType.Blocker => "Blocker",
            AbilityType.DoubleTrouble => "Double Trouble",
            AbilityType.DoubleDefense => "Double Defense",
            AbilityType.SeizeInitiative => "Seize Initiative",
            _ => ability.ToString()
        };

        public static string ShortName(this AbilityType ability) => ability switch
        {
            AbilityType.TrumpChanger => "TRUMP",
            AbilityType.ExtraDraw => "DRAW",
            AbilityType.Blocker => "BLOCK",
            AbilityType.DoubleTrouble => "DBL ATK",
            AbilityType.DoubleDefense => "DBL DEF",
            AbilityType.SeizeInitiative => "SEIZE",
            _ => "?"
        };

        public static string Description(this AbilityType ability) => ability switch
        {
            AbilityType.TrumpChanger => "Change the trump suit to this card's suit (once per game).",
            AbilityType.ExtraDraw => "Force the defender to draw 2 cards before defending.",
            AbilityType.Blocker => "No more attacks can be added this bout.",
            AbilityType.DoubleTrouble => "Play one extra attack ignoring the rank-match rule.",
            AbilityType.DoubleDefense => "This card covers two undefended attack slots.",
            AbilityType.SeizeInitiative => "You become the attacker after this bout resolves.",
            _ => ""
        };
    }
}
