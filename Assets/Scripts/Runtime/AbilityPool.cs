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
            // --- Neutral abilities ---
            new AbilityDefinition(AbilityType.Blocker,          AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.ExtraDraw,        AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.Peek,             AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.Fortify,          AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.SecondWind,       AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.Brace,            AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.Desperation,      AbilityRarity.Rare,     1),
            new AbilityDefinition(AbilityType.SteadyHand,       AbilityRarity.Common,   0, isPassive: true),

            // --- Rogue: Shadow ---
            new AbilityDefinition(AbilityType.SeizeInitiative,  AbilityRarity.Uncommon, 2, owner: ArchetypeType.Rogue, buildPath: "Shadow"),
            new AbilityDefinition(AbilityType.DoubleDefense,    AbilityRarity.Uncommon, 2, owner: ArchetypeType.Rogue, buildPath: "Shadow"),
            new AbilityDefinition(AbilityType.Riposte,          AbilityRarity.Common,   3, owner: ArchetypeType.Rogue, buildPath: "Shadow"),
            new AbilityDefinition(AbilityType.ShadowCloak,      AbilityRarity.Uncommon, 2, owner: ArchetypeType.Rogue, buildPath: "Shadow"),
            new AbilityDefinition(AbilityType.PatienceRewarded, AbilityRarity.Rare,     0, isPassive: true, owner: ArchetypeType.Rogue, buildPath: "Shadow"),

            // --- Rogue: Spy ---
            new AbilityDefinition(AbilityType.EndgameSpecialist,AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Rogue, buildPath: "Spy"),
            new AbilityDefinition(AbilityType.Wiretap,          AbilityRarity.Common,   3, owner: ArchetypeType.Rogue, buildPath: "Spy"),
            new AbilityDefinition(AbilityType.MarkedCards,       AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Rogue, buildPath: "Spy"),
            new AbilityDefinition(AbilityType.DoubleAgent,      AbilityRarity.Rare,     1, owner: ArchetypeType.Rogue, buildPath: "Spy"),
            new AbilityDefinition(AbilityType.Blackmail,        AbilityRarity.Uncommon, 2, owner: ArchetypeType.Rogue, buildPath: "Spy"),

            // --- Rogue: Saboteur ---
            new AbilityDefinition(AbilityType.SlipAway,         AbilityRarity.Rare,     1, owner: ArchetypeType.Rogue, buildPath: "Saboteur"),
            new AbilityDefinition(AbilityType.SleightOfHand,    AbilityRarity.Common,   3, owner: ArchetypeType.Rogue, buildPath: "Saboteur"),
            new AbilityDefinition(AbilityType.SmokeBomb,        AbilityRarity.Uncommon, 2, owner: ArchetypeType.Rogue, buildPath: "Saboteur"),
            new AbilityDefinition(AbilityType.TrapCard,         AbilityRarity.Uncommon, 2, owner: ArchetypeType.Rogue, buildPath: "Saboteur"),
            new AbilityDefinition(AbilityType.Undermine,        AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Rogue, buildPath: "Saboteur"),

            // --- Brute: Berserker ---
            new AbilityDefinition(AbilityType.DoubleTrouble,    AbilityRarity.Common,   3, owner: ArchetypeType.Brute, buildPath: "Berserker"),
            new AbilityDefinition(AbilityType.PileOn,           AbilityRarity.Uncommon, 2, owner: ArchetypeType.Brute, buildPath: "Berserker"),
            new AbilityDefinition(AbilityType.Feint,            AbilityRarity.Rare,     1, owner: ArchetypeType.Brute, buildPath: "Berserker"),
            new AbilityDefinition(AbilityType.Rampage,          AbilityRarity.Uncommon, 2, owner: ArchetypeType.Brute, buildPath: "Berserker"),
            new AbilityDefinition(AbilityType.Bloodlust,        AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Brute, buildPath: "Berserker"),

            // --- Brute: Brawler ---
            new AbilityDefinition(AbilityType.QuickHands,       AbilityRarity.Rare,     0, isPassive: true, owner: ArchetypeType.Brute, buildPath: "Brawler"),
            new AbilityDefinition(AbilityType.Haymaker,         AbilityRarity.Common,   3, owner: ArchetypeType.Brute, buildPath: "Brawler"),
            new AbilityDefinition(AbilityType.IronGrip,         AbilityRarity.Uncommon, 2, owner: ArchetypeType.Brute, buildPath: "Brawler"),
            new AbilityDefinition(AbilityType.Brawl,            AbilityRarity.Uncommon, 2, owner: ArchetypeType.Brute, buildPath: "Brawler"),
            new AbilityDefinition(AbilityType.ThickSkin,        AbilityRarity.Common,   0, isPassive: true, owner: ArchetypeType.Brute, buildPath: "Brawler"),

            // --- Brute: Warlord ---
            new AbilityDefinition(AbilityType.TrumpAffinity,    AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Brute, buildPath: "Warlord"),
            new AbilityDefinition(AbilityType.Conquer,          AbilityRarity.Common,   3, owner: ArchetypeType.Brute, buildPath: "Warlord"),
            new AbilityDefinition(AbilityType.Intimidate,       AbilityRarity.Uncommon, 2, owner: ArchetypeType.Brute, buildPath: "Warlord"),
            new AbilityDefinition(AbilityType.CrownSeize,       AbilityRarity.Rare,     1, owner: ArchetypeType.Brute, buildPath: "Warlord"),
            new AbilityDefinition(AbilityType.BattleHardened,   AbilityRarity.Common,   0, isPassive: true, owner: ArchetypeType.Brute, buildPath: "Warlord"),

            // --- Diplomat: Courtier ---
            new AbilityDefinition(AbilityType.TrumpChanger,     AbilityRarity.Common,   3, owner: ArchetypeType.Diplomat, buildPath: "Courtier"),
            new AbilityDefinition(AbilityType.CourtIntrigue,    AbilityRarity.Uncommon, 2, owner: ArchetypeType.Diplomat, buildPath: "Courtier"),
            new AbilityDefinition(AbilityType.RoyalDecree,      AbilityRarity.Uncommon, 2, owner: ArchetypeType.Diplomat, buildPath: "Courtier"),
            new AbilityDefinition(AbilityType.Patronage,        AbilityRarity.Rare,     1, owner: ArchetypeType.Diplomat, buildPath: "Courtier"),
            new AbilityDefinition(AbilityType.GracefulManners,  AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Diplomat, buildPath: "Courtier"),

            // --- Diplomat: Puppeteer ---
            new AbilityDefinition(AbilityType.Deflect,          AbilityRarity.Rare,     1, owner: ArchetypeType.Diplomat, buildPath: "Puppeteer"),
            new AbilityDefinition(AbilityType.PullStrings,      AbilityRarity.Common,   3, owner: ArchetypeType.Diplomat, buildPath: "Puppeteer"),
            new AbilityDefinition(AbilityType.Misdirection,     AbilityRarity.Uncommon, 2, owner: ArchetypeType.Diplomat, buildPath: "Puppeteer"),
            new AbilityDefinition(AbilityType.ForcedHand,       AbilityRarity.Uncommon, 2, owner: ArchetypeType.Diplomat, buildPath: "Puppeteer"),
            new AbilityDefinition(AbilityType.WebOfLies,        AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Diplomat, buildPath: "Puppeteer"),

            // --- Diplomat: Peacemaker ---
            new AbilityDefinition(AbilityType.Diplomacy,        AbilityRarity.Common,   3, owner: ArchetypeType.Diplomat, buildPath: "Peacemaker"),
            new AbilityDefinition(AbilityType.SafePassage,      AbilityRarity.Uncommon, 2, owner: ArchetypeType.Diplomat, buildPath: "Peacemaker"),
            new AbilityDefinition(AbilityType.Treaty,           AbilityRarity.Rare,     1, owner: ArchetypeType.Diplomat, buildPath: "Peacemaker"),
            new AbilityDefinition(AbilityType.GracefulRetreat,  AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Diplomat, buildPath: "Peacemaker"),
            new AbilityDefinition(AbilityType.Equilibrium,      AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Diplomat, buildPath: "Peacemaker"),

            // --- Gambler: Card Shark ---
            new AbilityDefinition(AbilityType.CardCounter,      AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Gambler, buildPath: "CardShark"),
            new AbilityDefinition(AbilityType.StackTheDeck,     AbilityRarity.Common,   3, owner: ArchetypeType.Gambler, buildPath: "CardShark"),
            new AbilityDefinition(AbilityType.SecondDeal,       AbilityRarity.Uncommon, 2, owner: ArchetypeType.Gambler, buildPath: "CardShark"),
            new AbilityDefinition(AbilityType.ColdRead,         AbilityRarity.Rare,     1, owner: ArchetypeType.Gambler, buildPath: "CardShark"),
            new AbilityDefinition(AbilityType.SharkInstinct,    AbilityRarity.Common,   0, isPassive: true, owner: ArchetypeType.Gambler, buildPath: "CardShark"),

            // --- Gambler: High Roller ---
            new AbilityDefinition(AbilityType.Gambit,           AbilityRarity.Rare,     1, owner: ArchetypeType.Gambler, buildPath: "HighRoller"),
            new AbilityDefinition(AbilityType.AllIn,            AbilityRarity.Common,   3, owner: ArchetypeType.Gambler, buildPath: "HighRoller"),
            new AbilityDefinition(AbilityType.DoubleOrNothing,  AbilityRarity.Uncommon, 2, owner: ArchetypeType.Gambler, buildPath: "HighRoller"),
            new AbilityDefinition(AbilityType.LuckyStreak,      AbilityRarity.Uncommon, 2, owner: ArchetypeType.Gambler, buildPath: "HighRoller"),
            new AbilityDefinition(AbilityType.Jackpot,          AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Gambler, buildPath: "HighRoller"),

            // --- Gambler: Trickster ---
            new AbilityDefinition(AbilityType.BlindSwap,        AbilityRarity.Common,   3, owner: ArchetypeType.Gambler, buildPath: "Trickster"),
            new AbilityDefinition(AbilityType.Misdeal,          AbilityRarity.Uncommon, 2, owner: ArchetypeType.Gambler, buildPath: "Trickster"),
            new AbilityDefinition(AbilityType.WildCard,         AbilityRarity.Uncommon, 2, owner: ArchetypeType.Gambler, buildPath: "Trickster"),
            new AbilityDefinition(AbilityType.SleightOfMind,    AbilityRarity.Uncommon, 0, isPassive: true, owner: ArchetypeType.Gambler, buildPath: "Trickster"),
            new AbilityDefinition(AbilityType.ChaoticNature,    AbilityRarity.Rare,     0, isPassive: true, owner: ArchetypeType.Gambler, buildPath: "Trickster"),

            // --- Upgraded (Plus) variants ---
            new AbilityDefinition(AbilityType.BlockerPlus,        AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.ExtraDrawPlus,      AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.PeekPlus,           AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.FortifyPlus,        AbilityRarity.Common,   3),
            new AbilityDefinition(AbilityType.SecondWindPlus,     AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.BracePlus,          AbilityRarity.Uncommon, 2),
            new AbilityDefinition(AbilityType.RipostePlus,        AbilityRarity.Common,   3, owner: ArchetypeType.Rogue),
            new AbilityDefinition(AbilityType.SleightOfHandPlus,  AbilityRarity.Common,   3, owner: ArchetypeType.Rogue),
            new AbilityDefinition(AbilityType.DoubleTroublePlus,  AbilityRarity.Common,   3, owner: ArchetypeType.Brute),
            new AbilityDefinition(AbilityType.PileOnPlus,         AbilityRarity.Uncommon, 2, owner: ArchetypeType.Brute),
            new AbilityDefinition(AbilityType.HaymakerPlus,       AbilityRarity.Common,   3, owner: ArchetypeType.Brute),
            new AbilityDefinition(AbilityType.DiplomacyPlus,      AbilityRarity.Common,   3, owner: ArchetypeType.Diplomat),
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
            AbilityType.SlipAway, AbilityType.Peek, AbilityType.Gambit,
            AbilityType.Riposte, AbilityType.ShadowCloak, AbilityType.Wiretap,
            AbilityType.DoubleAgent, AbilityType.Blackmail, AbilityType.SleightOfHand,
            AbilityType.SmokeBomb, AbilityType.TrapCard,
            AbilityType.Rampage, AbilityType.Haymaker, AbilityType.IronGrip, AbilityType.Brawl,
            AbilityType.Conquer, AbilityType.Intimidate, AbilityType.CrownSeize,
            AbilityType.CourtIntrigue, AbilityType.RoyalDecree, AbilityType.Patronage,
            AbilityType.PullStrings, AbilityType.Misdirection, AbilityType.ForcedHand,
            AbilityType.Diplomacy, AbilityType.SafePassage, AbilityType.Treaty,
            AbilityType.StackTheDeck, AbilityType.SecondDeal, AbilityType.ColdRead,
            AbilityType.AllIn, AbilityType.DoubleOrNothing, AbilityType.LuckyStreak,
            AbilityType.BlindSwap, AbilityType.Misdeal, AbilityType.WildCard,
            AbilityType.Fortify, AbilityType.SecondWind, AbilityType.Brace, AbilityType.Desperation,
        };

        public static readonly AbilityType[] PassiveAbilities = new[]
        {
            AbilityType.TrumpAffinity, AbilityType.EndgameSpecialist,
            AbilityType.CardCounter, AbilityType.QuickHands,
            AbilityType.PatienceRewarded, AbilityType.MarkedCards, AbilityType.Undermine,
            AbilityType.Bloodlust, AbilityType.ThickSkin, AbilityType.BattleHardened,
            AbilityType.GracefulManners, AbilityType.WebOfLies,
            AbilityType.GracefulRetreat, AbilityType.Equilibrium,
            AbilityType.SharkInstinct, AbilityType.Jackpot,
            AbilityType.SleightOfMind, AbilityType.ChaoticNature,
            AbilityType.SteadyHand,
        };
    }
}
