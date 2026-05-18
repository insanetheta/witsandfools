using System.Collections.Generic;

namespace WitsAndFools
{
    public enum ArchetypeType
    {
        Rogue,
        Brute,
        Diplomat,
        Gambler
    }

    public static class ArchetypeDefinitions
    {
        public static string DisplayName(this ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => "The Rogue",
            ArchetypeType.Brute => "The Brute",
            ArchetypeType.Diplomat => "The Diplomat",
            ArchetypeType.Gambler => "The Gambler",
            _ => a.ToString()
        };

        public static string Description(this ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => "Reactive and information-focused. Defends well, then seizes the moment.",
            ArchetypeType.Brute => "Aggressive overwhelm. Pile attacks high and draw deep.",
            ArchetypeType.Diplomat => "Redirection and trump control. Bend the rules in your favor.",
            ArchetypeType.Gambler => "High risk, high reward. Bluff, feint, and read the table.",
            _ => ""
        };

        public static List<AbilityType> StartingAbilities(this ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => new List<AbilityType>
            {
                AbilityType.Blocker, AbilityType.SeizeInitiative, AbilityType.Peek
            },
            ArchetypeType.Brute => new List<AbilityType>
            {
                AbilityType.DoubleTrouble, AbilityType.PileOn, AbilityType.ExtraDraw
            },
            ArchetypeType.Diplomat => new List<AbilityType>
            {
                AbilityType.TrumpChanger, AbilityType.Deflect, AbilityType.SlipAway
            },
            ArchetypeType.Gambler => new List<AbilityType>
            {
                AbilityType.Gambit, AbilityType.Feint, AbilityType.CardCounter
            },
            _ => new List<AbilityType>()
        };

        public static readonly ArchetypeType[] AllArchetypes = new[]
        {
            ArchetypeType.Rogue,
            ArchetypeType.Brute,
            ArchetypeType.Diplomat,
            ArchetypeType.Gambler
        };
    }
}
