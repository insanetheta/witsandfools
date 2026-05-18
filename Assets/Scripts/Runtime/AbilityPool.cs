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

        public AbilityDefinition(AbilityType type, AbilityRarity rarity, int bindingCount, bool isPassive = false)
        {
            Type = type;
            Rarity = rarity;
            BindingCount = bindingCount;
            IsPassive = isPassive;
        }
    }

    public static class AbilityPool
    {
        public static readonly AbilityDefinition[] All = new[]
        {
            new AbilityDefinition(AbilityType.TrumpChanger,     AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.ExtraDraw,        AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.Blocker,          AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.DoubleTrouble,    AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.DoubleDefense,    AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.SeizeInitiative,  AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.PileOn,           AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.Feint,            AbilityRarity.Rare,     1),
            new AbilityDefinition(AbilityType.Deflect,          AbilityRarity.Rare,     1),
            new AbilityDefinition(AbilityType.SlipAway,         AbilityRarity.Rare,     1),
            new AbilityDefinition(AbilityType.Peek,             AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.Gambit,           AbilityRarity.Rare,     1),
            new AbilityDefinition(AbilityType.TrumpAffinity,    AbilityRarity.Uncommon, 0, true),
            new AbilityDefinition(AbilityType.EndgameSpecialist,AbilityRarity.Uncommon, 0, true),
            new AbilityDefinition(AbilityType.CardCounter,      AbilityRarity.Uncommon, 0, true),
            new AbilityDefinition(AbilityType.QuickHands,       AbilityRarity.Rare,     0, true),
        };

        static readonly Dictionary<AbilityType, AbilityDefinition> _byType;

        static AbilityPool()
        {
            _byType = new Dictionary<AbilityType, AbilityDefinition>();
            foreach (var d in All) _byType[d.Type] = d;
        }

        public static AbilityDefinition Get(AbilityType type) => _byType[type];

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
