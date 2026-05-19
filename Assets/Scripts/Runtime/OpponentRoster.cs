using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public static class OpponentRoster
    {
        static readonly OpponentProfile[][] ActOpponents =
        {
            // Act 1 — Tavern (simple, no abilities/trinkets)
            new[]
            {
                new OpponentProfile
                {
                    Name = "Barnacle Bill", Archetype = AIArchetypeName.Brawler, ActIndex = 0,
                },
                new OpponentProfile
                {
                    Name = "Salty Pete", Archetype = AIArchetypeName.Miser, ActIndex = 0,
                },
                new OpponentProfile
                {
                    Name = "Dock Rat", Archetype = AIArchetypeName.Fox, ActIndex = 0,
                },
            },
            // Act 2 — Merchant Quarter (1 ability, 0-1 trinkets)
            new[]
            {
                new OpponentProfile
                {
                    Name = "Merchant Luca", Archetype = AIArchetypeName.Fox, ActIndex = 1,
                    Abilities = { AbilityType.ExtraDraw },
                },
                new OpponentProfile
                {
                    Name = "Trader Yun", Archetype = AIArchetypeName.Miser, ActIndex = 1,
                    Abilities = { AbilityType.Blocker },
                    Trinkets = { TrinketType.JugglersBalls },
                },
                new OpponentProfile
                {
                    Name = "Silk Marco", Archetype = AIArchetypeName.Noble, ActIndex = 1,
                    Abilities = { AbilityType.TrumpChanger },
                    Trinkets = { TrinketType.CourtiersFan },
                },
            },
            // Act 3 — Guild Hall (2 abilities, 1-2 trinkets)
            new[]
            {
                new OpponentProfile
                {
                    Name = "Guildmaster Voss", Archetype = AIArchetypeName.Noble, ActIndex = 2,
                    Abilities = { AbilityType.SeizeInitiative, AbilityType.Blocker },
                    Trinkets = { TrinketType.DuelistsGlove },
                },
                new OpponentProfile
                {
                    Name = "Lady Ashton", Archetype = AIArchetypeName.Scholar, ActIndex = 2,
                    Abilities = { AbilityType.Peek, AbilityType.DoubleDefense },
                    Trinkets = { TrinketType.HereticsBrand, TrinketType.ShieldBrooch },
                },
                new OpponentProfile
                {
                    Name = "Baron Kell", Archetype = AIArchetypeName.Fox, ActIndex = 2,
                    Abilities = { AbilityType.Feint, AbilityType.SlipAway },
                    Trinkets = { TrinketType.LoadedDice },
                },
            },
            // Act 4 — Library/Cathedral (3 abilities, 2-3 trinkets)
            new[]
            {
                new OpponentProfile
                {
                    Name = "Cardinal Enzo", Archetype = AIArchetypeName.Scholar, ActIndex = 3,
                    Abilities = { AbilityType.Peek, AbilityType.Gambit, AbilityType.CardCounter },
                    Trinkets = { TrinketType.LoadedDice, TrinketType.CrownOfThorns },
                },
                new OpponentProfile
                {
                    Name = "Sister Agatha", Archetype = AIArchetypeName.Noble, ActIndex = 3,
                    Abilities = { AbilityType.Blocker, AbilityType.DoubleDefense, AbilityType.EndgameSpecialist },
                    Trinkets = { TrinketType.ShieldBrooch, TrinketType.CrownOfThorns },
                },
                new OpponentProfile
                {
                    Name = "Spymaster Grey", Archetype = AIArchetypeName.Assassin, ActIndex = 3,
                    Abilities = { AbilityType.Feint, AbilityType.Deflect, AbilityType.SlipAway },
                    Trinkets = { TrinketType.PoisonedWine, TrinketType.VentriloquistsDummy, TrinketType.QuicksilverVial },
                },
            },
            // Act 5 — Salon (boss act)
            new OpponentProfile[0],
        };

        static readonly OpponentProfile[][] ActElites =
        {
            // Act 1
            new[]
            {
                new OpponentProfile
                {
                    Name = "Fishy Meg", Archetype = AIArchetypeName.Brawler, ActIndex = 0,
                    IsElite = true, HouseRule = HouseRuleType.HeavyHands,
                    Abilities = { AbilityType.DoubleTrouble },
                },
            },
            // Act 2
            new[]
            {
                new OpponentProfile
                {
                    Name = "Coin Bianca", Archetype = AIArchetypeName.Scholar, ActIndex = 1,
                    IsElite = true, HouseRule = HouseRuleType.NoTrumpsBeforeDusk,
                    Abilities = { AbilityType.Peek, AbilityType.ExtraDraw },
                    Trinkets = { TrinketType.JugglersBalls },
                },
            },
            // Act 3
            new[]
            {
                new OpponentProfile
                {
                    Name = "Fixer Tomas", Archetype = AIArchetypeName.Assassin, ActIndex = 2,
                    IsElite = true, HouseRule = HouseRuleType.Cutthroat,
                    Abilities = { AbilityType.DoubleTrouble, AbilityType.Feint, AbilityType.PileOn },
                    Trinkets = { TrinketType.DuelistsGlove, TrinketType.PoisonedWine },
                },
            },
            // Act 4
            new[]
            {
                new OpponentProfile
                {
                    Name = "Scholar Ruiz", Archetype = AIArchetypeName.Fox, ActIndex = 3,
                    IsElite = true, HouseRule = HouseRuleType.TheMirror,
                    Abilities = { AbilityType.Gambit, AbilityType.TrumpChanger, AbilityType.Deflect, AbilityType.QuickHands },
                    Trinkets = { TrinketType.FoolsGold, TrinketType.CrownOfThorns, TrinketType.QuicksilverVial },
                },
            },
            // Act 5 (no separate elite — boss is the elite)
            new OpponentProfile[0],
        };

        static readonly OpponentProfile Boss = new OpponentProfile
        {
            Name = "The Champion", Archetype = AIArchetypeName.Assassin, ActIndex = 4,
            IsBoss = true, HouseRule = HouseRuleType.DoubleOrNothing,
            Abilities =
            {
                AbilityType.DoubleTrouble, AbilityType.Deflect,
                AbilityType.SeizeInitiative, AbilityType.Gambit, AbilityType.EndgameSpecialist
            },
            Trinkets =
            {
                TrinketType.FoolsGold, TrinketType.DuelistsGlove,
                TrinketType.PoisonedWine, TrinketType.HereticsBrand
            },
        };

        public static OpponentProfile Pick(int actIndex, bool isElite, bool isBoss, Random rng)
        {
            if (isBoss) return Clone(Boss);

            var source = isElite
                ? ActElites[Math.Min(actIndex, ActElites.Length - 1)]
                : ActOpponents[Math.Min(actIndex, ActOpponents.Length - 1)];

            if (source.Length == 0) return Clone(Boss);

            var template = source[rng.Next(source.Length)];
            var result = Clone(template);
            if (!isElite && !isBoss)
                ApplyVariation(result, actIndex, rng);
            return result;
        }

        static readonly HouseRuleType[] MinorHouseRules =
        {
            HouseRuleType.HeavyHands, HouseRuleType.NoTrumpsBeforeDusk,
        };

        static readonly AbilityType[] BonusAbilities =
        {
            AbilityType.Blocker, AbilityType.ExtraDraw, AbilityType.Fortify,
            AbilityType.Peek, AbilityType.SteadyHand, AbilityType.Brace,
        };

        static void ApplyVariation(OpponentProfile opp, int actIndex, Random rng)
        {
            int roll = rng.Next(100);

            // 20% chance: gain a bonus ability
            if (roll < 20 && actIndex >= 1)
            {
                var bonus = BonusAbilities[rng.Next(BonusAbilities.Length)];
                if (!opp.Abilities.Contains(bonus))
                    opp.Abilities.Add(bonus);
            }
            // 10% chance: gain a minor house rule (acts 2+)
            else if (roll < 30 && actIndex >= 2 && opp.HouseRule == HouseRuleType.None)
            {
                opp.HouseRule = MinorHouseRules[rng.Next(MinorHouseRules.Length)];
            }
            // 15% chance: gain a trinket (acts 1+)
            else if (roll < 45 && actIndex >= 1)
            {
                var allTrinkets = (TrinketType[])Enum.GetValues(typeof(TrinketType));
                var trinket = allTrinkets[rng.Next(allTrinkets.Length)];
                if (!opp.Trinkets.Contains(trinket))
                    opp.Trinkets.Add(trinket);
            }
        }

        public static OpponentProfile[] AllForAct(int actIndex)
        {
            var list = new List<OpponentProfile>();
            if (actIndex < ActOpponents.Length)
                list.AddRange(ActOpponents[actIndex]);
            if (actIndex < ActElites.Length)
                list.AddRange(ActElites[actIndex]);
            if (actIndex == 4) list.Add(Boss);
            return list.ToArray();
        }

        static OpponentProfile Clone(OpponentProfile src) => new OpponentProfile
        {
            Name = src.Name,
            Archetype = src.Archetype,
            Abilities = new List<AbilityType>(src.Abilities),
            Trinkets = new List<TrinketType>(src.Trinkets),
            HouseRule = src.HouseRule,
            ActIndex = src.ActIndex,
            IsElite = src.IsElite,
            IsBoss = src.IsBoss,
        };
    }
}
