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
            }
        }

    }
}
