using System.Collections.Generic;

namespace WitsAndFools
{
    public enum AbilityRarity { Common, Uncommon, Rare }

    public readonly struct AbilityDefinition
    {
        public readonly AbilityType Type;
        public readonly AbilityRarity Rarity;
        public readonly int BindingCount;
        public readonly bool IsPassive;
        public readonly ArchetypeType? Owner;
        public readonly string BuildPath;

        public AbilityDefinition(AbilityType type, AbilityRarity rarity, int bindingCount,
            bool isPassive = false, ArchetypeType? owner = null, string buildPath = null)
        {
            Type = type;
            Rarity = rarity;
            BindingCount = bindingCount;
            IsPassive = isPassive;
            Owner = owner;
            BuildPath = buildPath;
        }

        public bool IsNeutral => Owner == null;
    }

    public static class AbilityPool
    {
        public static readonly AbilityDefinition[] All = new[]
        {
            // --- Neutral abilities (available to all archetypes) ---
            new AbilityDefinition(AbilityType.Blocker,          AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.ExtraDraw,        AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.Peek,             AbilityRarity.Uncommon, 2),

            // --- Rogue abilities ---
            new AbilityDefinition(AbilityType.SeizeInitiative,  AbilityRarity.Uncommon, 2, owner: ArchetypeType.Rogue, buildPath: "Shadow"),
            new AbilityDefinition(AbilityType.DoubleDefense,    AbilityRarity.Uncommon, 2, owner: ArchetypeType.Rogue, buildPath: "Shadow"),
            new AbilityDefinition(AbilityType.SlipAway,         AbilityRarity.Rare,     1, owner: ArchetypeType.Rogue, buildPath: "Saboteur"),
            new AbilityDefinition(AbilityType.EndgameSpecialist,AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Rogue, buildPath: "Spy"),

            // --- Brute abilities ---
            new AbilityDefinition(AbilityType.DoubleTrouble,    AbilityRarity.Common,   3, owner: ArchetypeType.Brute, buildPath: "Berserker"),
            new AbilityDefinition(AbilityType.PileOn,           AbilityRarity.Uncommon, 2, owner: ArchetypeType.Brute, buildPath: "Berserker"),
            new AbilityDefinition(AbilityType.Feint,            AbilityRarity.Rare,     1, owner: ArchetypeType.Brute, buildPath: "Berserker"),
            new AbilityDefinition(AbilityType.TrumpAffinity,    AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Brute, buildPath: "Warlord"),
            new AbilityDefinition(AbilityType.QuickHands,       AbilityRarity.Rare,     0, isPassive: true, owner: ArchetypeType.Brute, buildPath: "Brawler"),

            // --- Diplomat abilities ---
            new AbilityDefinition(AbilityType.TrumpChanger,     AbilityRarity.Common,   3, owner: ArchetypeType.Diplomat, buildPath: "Courtier"),
            new AbilityDefinition(AbilityType.Deflect,          AbilityRarity.Rare,     1, owner: ArchetypeType.Diplomat, buildPath: "Puppeteer"),

            // --- Gambler abilities ---
            new AbilityDefinition(AbilityType.Gambit,           AbilityRarity.Rare,     1, owner: ArchetypeType.Gambler, buildPath: "HighRoller"),
            new AbilityDefinition(AbilityType.CardCounter,      AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Gambler, buildPath: "CardShark"),
        };

        static readonly Dictionary<AbilityType, AbilityDefinition> _byType;

        static AbilityPool()
        {
            _byType = new Dictionary<AbilityType, AbilityDefinition>();
            foreach (var d in All) _byType[d.Type] = d;
        }

        public static AbilityDefinition Get(AbilityType type) => _byType[type];

        public static List<AbilityDefinition> ForArchetype(ArchetypeType? archetype)
        {
            var result = new List<AbilityDefinition>();
            foreach (var d in All)
            {
                if (d.IsNeutral || d.Owner == archetype)
                    result.Add(d);
            }
            return result;
        }

        public static readonly AbilityType[] ActiveAbilities = new[]
        {
            AbilityType.TrumpChanger, AbilityType.ExtraDraw, AbilityType.Blocker,
            AbilityType.DoubleTrouble, AbilityType.DoubleDefense, AbilityType.SeizeInitiative,
            AbilityType.PileOn, AbilityType.Feint, AbilityType.Deflect,
            AbilityType.SlipAway, AbilityType.Peek, AbilityType.Gambit
        };

        public static readonly AbilityType[] PassiveAbilities = new[]
        {
            AbilityType.TrumpAffinity, AbilityType.EndgameSpecialist,
            AbilityType.CardCounter, AbilityType.QuickHands
        };
    }
}
