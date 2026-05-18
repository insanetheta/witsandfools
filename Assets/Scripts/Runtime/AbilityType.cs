namespace WitsAndFools
{
    public enum AbilityType
    {
        TrumpChanger,
        ExtraDraw,
        Blocker,
        DoubleTrouble,
        DoubleDefense,
        SeizeInitiative,
        PileOn,
        Feint,
        Deflect,
        SlipAway,
        Peek,
        Gambit,
        TrumpAffinity,
        EndgameSpecialist,
        CardCounter,
        QuickHands
    }

    public static class AbilityTypeExtensions
    {
        public static string DisplayName(this AbilityType a) => a switch
        {
            AbilityType.TrumpChanger => "Trump Changer",
            AbilityType.ExtraDraw => "Extra Draw",
            AbilityType.Blocker => "Blocker",
            AbilityType.DoubleTrouble => "Double Trouble",
            AbilityType.DoubleDefense => "Double Defense",
            AbilityType.SeizeInitiative => "Seize Initiative",
            AbilityType.PileOn => "Pile On",
            AbilityType.Feint => "Feint",
            AbilityType.Deflect => "Deflect",
            AbilityType.SlipAway => "Slip Away",
            AbilityType.Peek => "Peek",
            AbilityType.Gambit => "Gambit",
            AbilityType.TrumpAffinity => "Trump Affinity",
            AbilityType.EndgameSpecialist => "Endgame Specialist",
            AbilityType.CardCounter => "Card Counter",
            AbilityType.QuickHands => "Quick Hands",
            _ => a.ToString()
        };

        public static string ShortName(this AbilityType a) => a switch
        {
            AbilityType.TrumpChanger => "TRUMP",
            AbilityType.ExtraDraw => "DRAW",
            AbilityType.Blocker => "BLOCK",
            AbilityType.DoubleTrouble => "DBL ATK",
            AbilityType.DoubleDefense => "DBL DEF",
            AbilityType.SeizeInitiative => "SEIZE",
            AbilityType.PileOn => "PILE ON",
            AbilityType.Feint => "FEINT",
            AbilityType.Deflect => "DEFLECT",
            AbilityType.SlipAway => "SLIP",
            AbilityType.Peek => "PEEK",
            AbilityType.Gambit => "GAMBIT",
            AbilityType.TrumpAffinity => "AFFIN",
            AbilityType.EndgameSpecialist => "ENDGM",
            AbilityType.CardCounter => "COUNT",
            AbilityType.QuickHands => "QUICK",
            _ => "?"
        };

        public static string Description(this AbilityType a) => a switch
        {
            AbilityType.TrumpChanger => "Change the trump suit to this card's suit (once per game).",
            AbilityType.ExtraDraw => "Force the defender to draw 2 cards before defending.",
            AbilityType.Blocker => "No more attacks can be added this bout.",
            AbilityType.DoubleTrouble => "Play one extra attack ignoring the rank-match rule.",
            AbilityType.DoubleDefense => "This card covers two undefended attack slots.",
            AbilityType.SeizeInitiative => "You become the attacker after this bout resolves.",
            AbilityType.PileOn => "Raise the max attacks this bout by 2.",
            AbilityType.Feint => "Play the top deck card as a phantom attack.",
            AbilityType.Deflect => "Swap attacker/defender roles mid-bout.",
            AbilityType.SlipAway => "Once per match, discard undefended attacks instead of eating them.",
            AbilityType.Peek => "Reveal and rearrange the top 3 cards of the deck.",
            AbilityType.Gambit => "Discard your entire hand and draw the same number of cards.",
            AbilityType.TrumpAffinity => "When you draw a trump card, draw 1 extra then discard 1.",
            AbilityType.EndgameSpecialist => "When deck has 6 or fewer cards, defend with any suit.",
            AbilityType.CardCounter => "After drawing, peek at deck top and swap your worst card for it if better.",
            AbilityType.QuickHands => "After a successful defense, draw 1 extra then discard 1.",
            _ => ""
        };

        public static bool IsPassive(this AbilityType a) =>
            a == AbilityType.TrumpAffinity || a == AbilityType.EndgameSpecialist ||
            a == AbilityType.CardCounter || a == AbilityType.QuickHands;
    }
}
