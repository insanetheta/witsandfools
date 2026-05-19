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
                AbilityType.TrumpChanger, AbilityType.Deflect, AbilityType.Diplomacy
            },
            ArchetypeType.Gambler => new List<AbilityType>
            {
                AbilityType.Gambit, AbilityType.Feint, AbilityType.CardCounter
            },
            _ => new List<AbilityType>()
        };

        public static TrinketType? StartingTrinket(this ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => TrinketType.SpysMonocle,
            ArchetypeType.Brute => TrinketType.DuelistsGlove,
            ArchetypeType.Diplomat => TrinketType.CourtiersFan,
            ArchetypeType.Gambler => TrinketType.LoadedDice,
            _ => null
        };

        public static string PerkName(this ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => "Shadow Reflexes",
            ArchetypeType.Brute => "Fury",
            ArchetypeType.Diplomat => "Court Favor",
            ArchetypeType.Gambler => "Lucky Draw",
            _ => ""
        };

        public static string PerkDescription(this ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => "First successful defense each bout draws 1 card and gains 1 Intel.",
            ArchetypeType.Brute => "When opponent eats your attacks, draw 1 extra.",
            ArchetypeType.Diplomat => "At each bout (deck ≥ 4), the better of the top 2 deck cards rises to the top.",
            ArchetypeType.Gambler => "Once per match after eating, discard your worst card.",
            _ => ""
        };

        public static readonly ArchetypeType[] AllArchetypes = new[]
        {
            ArchetypeType.Rogue,
            ArchetypeType.Brute,
            ArchetypeType.Diplomat,
            ArchetypeType.Gambler
        };

        static readonly HashSet<AbilityType> RogueSynergy = new()
        {
            AbilityType.Blocker, AbilityType.DoubleDefense, AbilityType.SlipAway,
            AbilityType.EndgameSpecialist, AbilityType.SeizeInitiative, AbilityType.Peek
        };

        static readonly HashSet<AbilityType> BruteSynergy = new()
        {
            AbilityType.DoubleTrouble, AbilityType.PileOn, AbilityType.ExtraDraw,
            AbilityType.Feint, AbilityType.TrumpAffinity, AbilityType.QuickHands
        };

        static readonly HashSet<AbilityType> DiplomatSynergy = new()
        {
            AbilityType.TrumpChanger, AbilityType.Deflect, AbilityType.SeizeInitiative,
            AbilityType.Peek, AbilityType.Diplomacy, AbilityType.EndgameSpecialist
        };

        static readonly HashSet<AbilityType> GamblerSynergy = new()
        {
            AbilityType.Gambit, AbilityType.Feint, AbilityType.CardCounter,
            AbilityType.QuickHands, AbilityType.ExtraDraw, AbilityType.Deflect
        };

        public static bool IsSynergy(this ArchetypeType a, AbilityType ability) => a switch
        {
            ArchetypeType.Rogue => RogueSynergy.Contains(ability),
            ArchetypeType.Brute => BruteSynergy.Contains(ability),
            ArchetypeType.Diplomat => DiplomatSynergy.Contains(ability),
            ArchetypeType.Gambler => GamblerSynergy.Contains(ability),
            _ => false
        };

        public static string[] BuildPaths(this ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => new[] { "Shadow", "Spy", "Saboteur" },
            ArchetypeType.Brute => new[] { "Berserker", "Brawler", "Warlord" },
            ArchetypeType.Diplomat => new[] { "Courtier", "Puppeteer", "Peacemaker" },
            ArchetypeType.Gambler => new[] { "CardShark", "HighRoller", "Trickster" },
            _ => System.Array.Empty<string>()
        };

        public static AbilityType WeightedPick(List<AbilityType> candidates, string targetPath,
            int pathWeight, int archWeight, int neutralWeight, System.Random rng)
        {
            if (candidates.Count == 0) return default;
            if (candidates.Count == 1) return candidates[0];

            int totalWeight = 0;
            var weights = new int[candidates.Count];
            for (int i = 0; i < candidates.Count; i++)
            {
                var def = AbilityPool.Get(candidates[i]);
                int w;
                if (def.BuildPath == targetPath) w = pathWeight;
                else if (def.Owner.HasValue) w = archWeight;
                else w = neutralWeight;
                weights[i] = w;
                totalWeight += w;
            }

            int roll = rng.Next(totalWeight);
            int acc = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (roll < acc) return candidates[i];
            }
            return candidates[^1];
        }
    }
}
