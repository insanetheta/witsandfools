using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public static class MatchSetup
    {
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

        public static (MatchConfig config, PlayerDeck playerDeck, PlayerDeck enemyDeck) Build(
            RunState run, OpponentProfile opponent, Random rng)
        {
            var config = new MatchConfig();

            var playerDeck = new PlayerDeck(run.PlayerDeckCardIds);
            if (run.CardAbilityOverrides.Count > 0)
                playerDeck.ApplyAbilityOverrides(run.CardAbilityOverrides);
            if (run.CardRankOverrides.Count > 0)
                playerDeck.ApplyRankOverrides(run.CardRankOverrides);
            var enemyDeck = new PlayerDeck(opponent.DeckCardIds);

            if (run.PlayerDoctrine.HasValue)
            {
                var doctrine = run.PlayerDoctrine.Value;
                config.ArchetypeResource[0] = doctrine.Resource();
            }

            if (opponent.Doctrine.HasValue)
            {
                var doctrine = opponent.Doctrine.Value;
                config.ArchetypeResource[1] = doctrine.Resource();
            }

            foreach (var relic in run.PlayerRelics)
                ApplyRelic(config, 0, relic);
            foreach (var relic in opponent.Relics)
                ApplyRelic(config, 1, relic);

            foreach (var burden in run.PlayerBurdens)
                ApplyBurden(config, 0, burden);

            foreach (var trinket in run.PlayerTrinkets)
                ApplyTrinket(config, 0, trinket);

            config.AnyRankAttack = true;
            config.AbilitiesCostResource = true;
            config.StartingResource = 2;
            config.DesperationDiscard = true;
            ApplyHouseRule(config, opponent.HouseRule);

            return (config, playerDeck, enemyDeck);
        }

        static void ApplyRelic(MatchConfig config, int player, RelicType relic)
        {
            if (!RelicPool.TryGet(relic, out var def)) return;

            switch (relic)
            {
                case RelicType.SpysMonocle: config.SpysMonocle[player] = true; break;
                case RelicType.IronGauntlet: config.BruteFury[player] = true; break;
                case RelicType.TwoFacedCoin: config.LuckyDraw[player] = true; break;
                case RelicType.BottomlessPurse: config.SteadyHand[player] = true; break;
                case RelicType.CandleStub: config.CardCounter[player] = true; break;
                case RelicType.MerchantsPurse: break;
                case RelicType.PhoenixMedal: break;
                case RelicType.GamblersDie: config.LoadedDice[player] = true; break;
                case RelicType.VenetianGlass: config.CourtiersFan[player] = true; break;
                case RelicType.ThiefsLantern: config.QuicksilverVial[player] = true; break;
                case RelicType.PilgrimsCompass: config.QuickHands[player] = true; break;
                case RelicType.TitansCrown: config.HandSize += 2; break;
                case RelicType.SovereignsDecree: config.MaxAttacksPerBout += 1; break;
                case RelicType.HeraldsHorn: config.StartingResource += 3; break;
                case RelicType.WanderersBoots: break;
                case RelicType.MisersHoard: break;
                case RelicType.PhoenixFeather: break;
            }
        }

        static void ApplyTrinket(MatchConfig config, int player, TrinketType trinket)
        {
            switch (trinket)
            {
                case TrinketType.DuelistsGlove: config.DuelistGlove[player] = true; break;
                case TrinketType.ShieldBrooch: config.ShieldBrooch[player] = true; break;
                case TrinketType.PoisonedWine: config.PoisonedWine[player] = true; break;
                case TrinketType.CourtiersFan: config.CourtiersFan[player] = true; break;
                case TrinketType.JugglersBalls: config.JugglersBalls[player] = true; break;
                case TrinketType.LoadedDice: config.LoadedDice[player] = true; break;
                case TrinketType.QuicksilverVial: config.QuicksilverVial[player] = true; break;
                case TrinketType.SpysMonocle: config.SpysMonocle[player] = true; break;
                case TrinketType.MarkedDeck: config.MarkedDeck[player] = true; break;
                case TrinketType.CrownOfThorns: config.CrownOfThorns[player] = true; break;
                case TrinketType.HereticsBrand: config.HereticsBrand[player] = true; break;
                case TrinketType.FoolsGold: config.FoolsGold[player] = true; break;
                case TrinketType.VentriloquistsDummy: config.VentriloquistsDummy[player] = true; break;
                case TrinketType.CatalystGem: config.CatalystGem[player] = true; break;
                case TrinketType.EchoStone: config.EchoStone[player] = true; break;
                case TrinketType.RazorsEdge: config.RazorsEdge[player] = true; break;
                case TrinketType.Bloodstone: config.Bloodstone[player] = true; break;
            }
        }

    }
}
