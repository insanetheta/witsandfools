using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace WitsAndFools.EditorTools
{
    public static class RunSimulator
    {
        [MenuItem("Wits and Fools/Simulate/Run 100 Ladder Games")]
        public static void Run100() => SimulateRuns(100, 42);

        [MenuItem("Wits and Fools/Simulate/Run 1000 Ladder Games")]
        public static void Run1000() => SimulateRuns(1000, 42);

        [MenuItem("Wits and Fools/Simulate/Run 5000 Ladder Games")]
        public static void Run5000() => SimulateRuns(5000, 42);

        static void SimulateRuns(int runCount, int baseSeed)
        {
            var stats = new RunStats();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int r = 0; r < runCount; r++)
            {
                var run = PlayFullRun(baseSeed + r, stats);
                stats.TotalRuns++;
                if (run.RunWon) stats.RunsWon++;
                stats.TotalFlorins += run.Florins;

                foreach (var kv in run.AbilityPickCount)
                {
                    if (!stats.AbilityPicks.ContainsKey(kv.Key)) stats.AbilityPicks[kv.Key] = 0;
                    stats.AbilityPicks[kv.Key] += kv.Value;
                }
                foreach (var kv in run.AbilityUsageCount)
                {
                    if (!stats.AbilityUsage.ContainsKey(kv.Key)) stats.AbilityUsage[kv.Key] = 0;
                    stats.AbilityUsage[kv.Key] += kv.Value;
                }
            }

            sw.Stop();
            PrintReport(stats, runCount, sw.ElapsedMilliseconds);
        }

        static RunState PlayFullRun(int seed, RunStats stats)
        {
            var rng = new Random(seed);
            var run = new RunState { Seed = seed };

            var startingAbilities = PickStartingAbilities(rng);
            run.PlayerAbilities.AddRange(startingAbilities);
            foreach (var a in startingAbilities) run.RecordAbilityPicked(a);

            for (int act = 0; act < 5; act++)
            {
                run.CurrentAct = act;
                var map = MapGenerator.Generate(act, rng);
                run.CurrentMap = map;

                foreach (var column in map)
                {
                    var node = column[rng.Next(column.Count)];

                    switch (node.Type)
                    {
                        case MapNodeType.RivalMatch:
                        case MapNodeType.EliteMatch:
                        case MapNodeType.BossMatch:
                            bool won = PlayMatch(run, node.Opponent, rng, stats);
                            run.MatchesPlayed++;
                            if (won)
                            {
                                run.MatchesWon++;
                                int florins = CalculateFlorins(act, node.Type);
                                run.Florins += florins;

                                if (run.PlayerAbilities.Count < run.MaxAbilitySlots)
                                {
                                    var reward = PickAbilityReward(run, node.Type == MapNodeType.EliteMatch, rng);
                                    if (reward.HasValue)
                                    {
                                        run.PlayerAbilities.Add(reward.Value);
                                        run.RecordAbilityPicked(reward.Value);
                                    }
                                }

                                if (node.Type == MapNodeType.EliteMatch)
                                    TryAwardTrinket(run, rng);
                            }
                            else
                            {
                                run.Prestige--;
                                TryApplyBurden(run, rng);
                                if (run.Prestige <= 0)
                                {
                                    run.RunComplete = true;
                                    run.RunWon = false;
                                    if (!stats.ActReached.ContainsKey(act)) stats.ActReached[act] = 0;
                                    stats.ActReached[act]++;
                                    return run;
                                }
                            }
                            break;

                        case MapNodeType.Shop:
                            SimulateShop(run, rng);
                            break;

                        case MapNodeType.Rumor:
                            SimulateRumor(run, rng);
                            break;

                        case MapNodeType.Rest:
                            SimulateRest(run, rng);
                            break;
                    }
                }

                if (!stats.ActReached.ContainsKey(act)) stats.ActReached[act] = 0;
                stats.ActReached[act]++;

                if (!stats.ActWinRate.ContainsKey(act)) stats.ActWinRate[act] = new int[2];
            }

            run.RunComplete = true;
            run.RunWon = true;
            return run;
        }

        static bool PlayMatch(RunState run, OpponentProfile opponent, Random rng, RunStats stats)
        {
            int matchSeed = rng.Next();
            var (config, pDeck, eDeck) = MatchSetup.Build(run, opponent, rng);
            var engine = new GameEngine(matchSeed, config, pDeck, eDeck);

            var ai0 = new AIPlayer("Player", matchSeed);
            var ai1 = new AIPlayer(opponent.Name, matchSeed + 1);
            AIArchetypes.Apply(ai1, opponent.Archetype, opponent.ActIndex);

            engine.OnAbilityUsed += (playerIndex, card, ability) =>
            {
                if (playerIndex == 0) run.RecordAbilityUsed(ability);
            };

            int boutCount = 0;
            int boutsDefended = 0;
            engine.OnBoutResolved += outcome =>
            {
                boutCount++;
                if (outcome == BoutOutcome.DefenderWonAllDiscarded) boutsDefended++;
            };

            engine.StartNewGame();

            int safety = 0;
            while (engine.Phase != Phase.GameOver && safety++ < 5000)
            {
                if (safety > 3000)
                {
                    int p = engine.Phase == Phase.Defense ? engine.DefenderIndex : engine.AttackerIndex;
                    if (engine.Phase == Phase.Defense)
                        engine.TryEat(p);
                    else if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended)
                        engine.TryEndBout(p);
                    else if (engine.Bout.IsEmpty)
                    {
                        var hand = engine.HandOf(p);
                        if (hand.Count > 0) engine.TryAttack(p, hand.Cards[0]);
                    }
                    continue;
                }
                int active = engine.Phase == Phase.Defense ? engine.DefenderIndex : engine.AttackerIndex;
                if (active == 0)
                    ai0.RequestAction(engine, 0);
                else
                    ai1.RequestAction(engine, 1);
            }

            run.TotalBoutsPlayed += boutCount;
            run.TotalBoutsDefended += boutsDefended;

            bool playerWon = engine.WinnerIndex == 0;

            int act = opponent.ActIndex;
            if (!stats.ActWinRate.ContainsKey(act)) stats.ActWinRate[act] = new int[2];
            stats.ActWinRate[act][0]++;
            if (playerWon) stats.ActWinRate[act][1]++;

            if (safety >= 5000) stats.Stalls++;

            return playerWon;
        }

        static List<AbilityType> PickStartingAbilities(Random rng)
        {
            var pool = new List<AbilityType>(AbilityPool.ActiveAbilities);
            pool.AddRange(AbilityPool.PassiveAbilities);
            var picked = new List<AbilityType>();
            for (int i = 0; i < 4 && pool.Count > 0; i++)
            {
                int idx = rng.Next(pool.Count);
                picked.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
            return picked;
        }

        static AbilityType? PickAbilityReward(RunState run, bool eliteWeighted, Random rng)
        {
            var pool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (run.PlayerAbilities.Contains(def.Type)) continue;
                if (eliteWeighted || def.Rarity != AbilityRarity.Rare)
                    pool.Add(def);
            }
            if (pool.Count == 0) return null;

            if (eliteWeighted)
            {
                var rares = pool.Where(d => d.Rarity == AbilityRarity.Rare).ToList();
                if (rares.Count > 0 && rng.Next(100) < 50)
                    return rares[rng.Next(rares.Count)].Type;
            }

            return pool[rng.Next(pool.Count)].Type;
        }

        static void TryAwardTrinket(RunState run, Random rng)
        {
            if (run.PlayerTrinkets.Count >= 5) return;
            var all = (TrinketType[])Enum.GetValues(typeof(TrinketType));
            var available = all.Where(t => !run.PlayerTrinkets.Contains(t)).ToArray();
            if (available.Length == 0) return;
            run.PlayerTrinkets.Add(available[rng.Next(available.Length)]);
        }

        static void TryApplyBurden(RunState run, Random rng)
        {
            var all = (BurdenType[])Enum.GetValues(typeof(BurdenType));
            var available = all.Where(b => !run.PlayerBurdens.Contains(b)).ToArray();
            if (available.Length == 0) return;
            run.PlayerBurdens.Add(available[rng.Next(available.Length)]);
        }

        static int CalculateFlorins(int act, MapNodeType nodeType)
        {
            int baseAmount = 8 + act * 2;
            if (nodeType == MapNodeType.EliteMatch) baseAmount += 4;
            if (nodeType == MapNodeType.BossMatch) baseAmount += 8;
            return baseAmount;
        }

        static void SimulateShop(RunState run, Random rng)
        {
            if (run.Florins >= 12 && run.PlayerAbilities.Count < run.MaxAbilitySlots)
            {
                var reward = PickAbilityReward(run, false, rng);
                if (reward.HasValue)
                {
                    run.PlayerAbilities.Add(reward.Value);
                    run.RecordAbilityPicked(reward.Value);
                    run.Florins -= 12;
                }
            }
            else if (run.Florins >= 10 && run.PlayerBurdens.Count > 0)
            {
                run.PlayerBurdens.RemoveAt(rng.Next(run.PlayerBurdens.Count));
                run.Florins -= 10;
            }
        }

        static void SimulateRumor(RunState run, Random rng)
        {
            int roll = rng.Next(100);
            if (roll < 30)
            {
                run.Florins += 5;
            }
            else if (roll < 60 && run.PlayerAbilities.Count < run.MaxAbilitySlots)
            {
                var reward = PickAbilityReward(run, false, rng);
                if (reward.HasValue)
                {
                    run.PlayerAbilities.Add(reward.Value);
                    run.RecordAbilityPicked(reward.Value);
                }
            }
            else if (roll < 80)
            {
                TryApplyBurden(run, rng);
                run.Florins += 8;
            }
            else
            {
                run.Florins += 3;
            }
        }

        static void SimulateRest(RunState run, Random rng)
        {
            if (run.PlayerBurdens.Count > 0 && rng.Next(100) < 60)
                run.PlayerBurdens.RemoveAt(rng.Next(run.PlayerBurdens.Count));
        }

        // ---------- Reporting ----------

        static void PrintReport(RunStats stats, int runCount, long elapsedMs)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Roguelike Ladder Simulation: {runCount} runs ({elapsedMs}ms) ===");
            sb.AppendLine();

            sb.AppendLine($"Run success rate: {stats.RunsWon}/{stats.TotalRuns} ({100.0 * stats.RunsWon / stats.TotalRuns:0.0}%)");
            sb.AppendLine($"Stalled matches: {stats.Stalls}");
            sb.AppendLine($"Avg Florins/run: {stats.TotalFlorins / (double)stats.TotalRuns:0.0}");
            sb.AppendLine();

            sb.AppendLine("--- Win Rate by Act ---");
            for (int act = 0; act < 5; act++)
            {
                if (stats.ActWinRate.TryGetValue(act, out var wr) && wr[0] > 0)
                    sb.AppendLine($"  Act {act + 1}: {wr[1]}/{wr[0]} ({100.0 * wr[1] / wr[0]:0.0}%)");
            }
            sb.AppendLine();

            sb.AppendLine("--- Farthest Act Reached ---");
            for (int act = 0; act < 5; act++)
            {
                if (stats.ActReached.TryGetValue(act, out int count))
                    sb.AppendLine($"  Act {act + 1}: {count} runs");
            }
            sb.AppendLine();

            sb.AppendLine("--- Most Picked Abilities ---");
            foreach (var kv in stats.AbilityPicks.OrderByDescending(kv => kv.Value).Take(10))
                sb.AppendLine($"  {kv.Key.DisplayName()}: {kv.Value}");
            sb.AppendLine();

            sb.AppendLine("--- Most Used Abilities (in matches) ---");
            foreach (var kv in stats.AbilityUsage.OrderByDescending(kv => kv.Value).Take(10))
                sb.AppendLine($"  {kv.Key.DisplayName()}: {kv.Value}");

            Debug.Log(sb.ToString());
        }

        class RunStats
        {
            public int TotalRuns;
            public int RunsWon;
            public int Stalls;
            public int TotalFlorins;
            public Dictionary<int, int[]> ActWinRate = new();
            public Dictionary<int, int> ActReached = new();
            public Dictionary<AbilityType, int> AbilityPicks = new();
            public Dictionary<AbilityType, int> AbilityUsage = new();
        }
    }
}
