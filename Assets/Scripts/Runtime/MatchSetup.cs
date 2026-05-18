using System;
using System.Collections.Generic;
using System.Linq;

namespace WitsAndFools
{
    public static class MatchSetup
    {
        public static MatchConfig Build(RunState run, OpponentProfile opponent, Random rng)
        {
            var config = new MatchConfig();
            var abilities = new Dictionary<(Suit, Rank), AbilityType>();
            var owners = new Dictionary<(Suit, Rank), int>();

            var availableCards = new List<(Suit, Rank)>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                for (Rank rank = Rank.Six; rank <= Rank.Ace; rank++)
                    availableCards.Add((suit, rank));

            Shuffle(availableCards, rng);

            bool hasForgers = run.PlayerTrinkets.Contains(TrinketType.ForgersKit);

            int idx = 0;
            foreach (var abilityType in run.PlayerAbilities)
            {
                var def = AbilityPool.Get(abilityType);
                if (def.IsPassive)
                {
                    ApplyPassive(config, 0, abilityType);
                    continue;
                }
                int binds = def.BindingCount;
                if (hasForgers && binds == 1) binds = 2;
                for (int b = 0; b < binds && idx < availableCards.Count; b++, idx++)
                {
                    abilities[availableCards[idx]] = abilityType;
                    owners[availableCards[idx]] = 0;
                }
            }

            var opponentAbilities = opponent.HouseRule == HouseRuleType.TheMirror
                ? run.PlayerAbilities
                : opponent.Abilities;

            foreach (var abilityType in opponentAbilities)
            {
                var def = AbilityPool.Get(abilityType);
                if (def.IsPassive)
                {
                    ApplyPassive(config, 1, abilityType);
                    continue;
                }
                for (int b = 0; b < def.BindingCount && idx < availableCards.Count; b++, idx++)
                {
                    if (!abilities.ContainsKey(availableCards[idx]))
                    {
                        abilities[availableCards[idx]] = abilityType;
                        owners[availableCards[idx]] = 1;
                    }
                    else idx++;
                }
            }

            config.Abilities = abilities;
            config.AbilityOwners = owners;

            foreach (var trinket in run.PlayerTrinkets)
                ApplyTrinket(config, 0, trinket);
            foreach (var trinket in opponent.Trinkets)
                ApplyTrinket(config, 1, trinket);

            foreach (var burden in run.PlayerBurdens)
                ApplyBurden(config, 0, burden);

            ApplyHouseRule(config, opponent.HouseRule);

            return config;
        }

        static void ApplyPassive(MatchConfig config, int player, AbilityType ability)
        {
            switch (ability)
            {
                case AbilityType.TrumpAffinity: config.TrumpAffinity[player] = true; break;
                case AbilityType.EndgameSpecialist: config.EndgameSpecialist[player] = true; break;
                case AbilityType.QuickHands: config.QuickHands[player] = true; break;
                case AbilityType.CardCounter: config.CardCounter[player] = true; break;
            }
        }

        static void ApplyTrinket(MatchConfig config, int player, TrinketType trinket)
        {
            switch (trinket)
            {
                case TrinketType.TailorsThimble:
                    if (player == 0) config.HandSize = Math.Min(config.HandSize, 5);
                    break;
                case TrinketType.DuelistsGlove:
                    config.DuelistGlove[player] = true;
                    break;
                case TrinketType.PoisonedWine:
                    config.PoisonedWine[player] = true;
                    break;
                case TrinketType.AlchemistsStone:
                    if (player == 0) config.ForcedTrumpSuit = 0;
                    break;
                case TrinketType.HereticsBrand:
                    config.HereticsBrand[player] = true;
                    break;
                case TrinketType.ShieldBrooch:
                    config.ShieldBrooch[player] = true;
                    break;
                case TrinketType.CourtiersFan:
                    config.CourtiersFan[player] = true;
                    break;
                case TrinketType.JugglersBalls:
                    config.JugglersBalls[player] = true;
                    break;
                case TrinketType.LoadedDice:
                    config.LoadedDice[player] = true;
                    break;
                case TrinketType.QuicksilverVial:
                    config.QuicksilverVial[player] = true;
                    break;
                case TrinketType.SpysMonocle:
                    config.SpysMonocle[player] = true;
                    break;
                case TrinketType.MarkedDeck:
                    config.MarkedDeck[player] = true;
                    break;
                case TrinketType.CrownOfThorns:
                    config.CrownOfThorns[player] = true;
                    break;
                case TrinketType.DevilsBargain:
                    config.HandSize = Math.Max(config.HandSize - 1, 4);
                    break;
                case TrinketType.FoolsGold:
                    config.FoolsGold[player] = true;
                    break;
                case TrinketType.VentriloquistsDummy:
                    config.VentriloquistsDummy[player] = true;
                    break;
            }
        }

        static void ApplyBurden(MatchConfig config, int player, BurdenType burden)
        {
            switch (burden)
            {
                case BurdenType.HeavyPurse:
                    if (player == 0) config.HandSize = Math.Max(config.HandSize, 7);
                    break;
                case BurdenType.RattledNerves:
                    config.RattledNerves[player] = true;
                    break;
                case BurdenType.ClumsyFingers:
                    config.ClumsyFingers[player] = true;
                    break;
            }
        }

        static void ApplyHouseRule(MatchConfig config, HouseRuleType rule)
        {
            switch (rule)
            {
                case HouseRuleType.NoTrumpsBeforeDusk:
                    config.NoTrumpsUntilBout = 3;
                    break;
                case HouseRuleType.TheGauntlet:
                    config.FixedAttacker = true;
                    break;
                case HouseRuleType.HeavyHands:
                    config.HandSize = 8;
                    break;
                case HouseRuleType.Cutthroat:
                    config.MaxAttacksPerBout = 4;
                    config.AnyRankAttack = true;
                    break;
                case HouseRuleType.DoubleOrNothing:
                    config.EatDrawsExtra = true;
                    break;
                case HouseRuleType.TheMirror:
                    config.MirrorAbilities = true;
                    break;
            }
        }

        static void Shuffle<T>(List<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
