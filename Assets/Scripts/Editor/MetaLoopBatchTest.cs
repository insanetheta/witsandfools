using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class MetaLoopBatchTest
    {
        [MenuItem("Wits and Fools/Smoke Test/Meta-Loop 30 Runs")]
        public static void Run30() => Run(30);

        [MenuItem("Wits and Fools/Smoke Test/Meta-Loop 100 Runs")]
        public static void Run100() => Run(100);

        static void Run(int runCount)
        {
            int wins = 0, totalMatches = 0, totalMatchWins = 0;
            var archetypeWins = new Dictionary<ArchetypeType, int>();
            var archetypeRuns = new Dictionary<ArchetypeType, int>();
            var abilityPicks = new Dictionary<AbilityType, int>();
            var results = new List<string>();

            for (int r = 0; r < runCount; r++)
            {
                var result = SimulateRun(Environment.TickCount + r, out var archetype);
                totalMatches += result.MatchesPlayed;
                totalMatchWins += result.MatchesWon;

                if (!archetypeRuns.ContainsKey(archetype)) archetypeRuns[archetype] = 0;
                archetypeRuns[archetype]++;

                if (result.RunWon)
                {
                    wins++;
                    if (!archetypeWins.ContainsKey(archetype)) archetypeWins[archetype] = 0;
                    archetypeWins[archetype]++;
                }

                foreach (var kv in result.AbilityPickCount)
                {
                    if (!abilityPicks.ContainsKey(kv.Key)) abilityPicks[kv.Key] = 0;
                    abilityPicks[kv.Key] += kv.Value;
                }

                string tag = result.RunWon ? "WON" : "LOST";
                results.Add($"  {tag} | {archetype.DisplayName()} | Acts: {result.CurrentAct + 1}/5 | W/L: {result.MatchesWon}/{result.MatchesPlayed} | Florins: {result.Florins} | Abilities: {result.PlayerAbilities.Count}");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== Meta-Loop Batch: {runCount} runs ===");
            sb.AppendLine($"Run win rate: {wins}/{runCount} ({100.0 * wins / runCount:0.0}%)");
            sb.AppendLine($"Match win rate: {totalMatchWins}/{totalMatches} ({100.0 * totalMatchWins / totalMatches:0.0}%)");
            sb.AppendLine();

            sb.AppendLine("Archetype breakdown:");
            foreach (var arch in ArchetypeDefinitions.AllArchetypes)
            {
                int played = archetypeRuns.GetValueOrDefault(arch, 0);
                int won = archetypeWins.GetValueOrDefault(arch, 0);
                if (played > 0)
                    sb.AppendLine($"  {arch.DisplayName()}: {won}/{played} wins ({100.0 * won / played:0.0}%)");
            }
            sb.AppendLine();

            sb.AppendLine("Top ability picks:");
            var sorted = abilityPicks.OrderByDescending(kv => kv.Value).Take(10);
            foreach (var kv in sorted)
                sb.AppendLine($"  {kv.Key.DisplayName()}: {kv.Value}");
            sb.AppendLine();

            foreach (var line in results)
                sb.AppendLine(line);

            Debug.Log(sb.ToString());
        }

        static RunState SimulateRun(int seed, out ArchetypeType archetype)
        {
            var rng = new System.Random(seed);
            var run = new RunState { Seed = seed };

            var allArch = ArchetypeDefinitions.AllArchetypes;
            archetype = allArch[rng.Next(allArch.Length)];
            run.PlayerArchetype = archetype;
            run.PlayerAbilities.AddRange(archetype.StartingAbilities());
            var trinket = archetype.StartingTrinket();
            if (trinket.HasValue) run.PlayerTrinkets.Add(trinket.Value);

            run.CurrentAct = 0;
            run.CurrentMap = MapGenerator.Generate(0, rng);

            while (!run.RunComplete && run.CurrentAct < 5)
            {
                int col = 0;
                while (col < run.CurrentMap.Count && !run.RunComplete)
                {
                    var column = run.CurrentMap[col];
                    var node = PickBestNode(column);
                    col++;

                    switch (node.Type)
                    {
                        case MapNodeType.RivalMatch:
                        case MapNodeType.EliteMatch:
                        case MapNodeType.BossMatch:
                            SimulateMatch(run, node, rng);
                            if (run.RunComplete) break;
                            PickAbilityReward(run, node, rng);
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

                if (run.RunComplete) break;

                run.CurrentAct++;
                if (run.CurrentAct < 5)
                    run.CurrentMap = MapGenerator.Generate(run.CurrentAct, rng);
                else
                {
                    run.RunComplete = true;
                    run.RunWon = true;
                }
            }

            return run;
        }

        static MapNode PickBestNode(List<MapNode> column)
        {
            MapNode best = column[0];
            int bestP = NodePriority(best.Type);
            for (int i = 1; i < column.Count; i++)
            {
                int p = NodePriority(column[i].Type);
                if (p > bestP) { best = column[i]; bestP = p; }
            }
            return best;
        }

        static int NodePriority(MapNodeType t) => t switch
        {
            MapNodeType.BossMatch => 100,
            MapNodeType.EliteMatch => 90,
            MapNodeType.RivalMatch => 80,
            MapNodeType.Rest => 30,
            MapNodeType.Rumor => 20,
            MapNodeType.Shop => 10,
            _ => 0
        };

        static void SimulateMatch(RunState run, MapNode node, System.Random rng)
        {
            var config = MatchSetup.Build(run, node.Opponent, rng);
            var engine = new GameEngine(rng.Next(), config);
            engine.StartNewGame();

            var ai0 = new AIPlayer("Player", rng.Next());
            var ai1 = new AIPlayer(node.Opponent.Name, rng.Next());
            AIArchetypes.Apply(ai1, node.Opponent.Archetype, node.Opponent.ActIndex);

            int safety = 0;
            while (engine.Phase != Phase.GameOver && safety++ < 5000)
            {
                int active = engine.Phase == Phase.Defense ? engine.DefenderIndex : engine.AttackerIndex;
                if (active == 0)
                    ai0.RequestAction(engine, 0);
                else
                    ai1.RequestAction(engine, 1);
            }

            run.MatchesPlayed++;
            bool won = engine.WinnerIndex == 0;

            if (won)
            {
                run.MatchesWon++;
                int florins = 10 + run.CurrentAct * 2;
                if (node.Type == MapNodeType.EliteMatch) florins += 5;
                if (node.Type == MapNodeType.BossMatch) florins += 10;
                if (run.PlayerTrinkets.Contains(TrinketType.MerchantsPurse)) florins += 3;
                run.Florins += florins;
            }
            else
            {
                run.Prestige--;
                if (run.Prestige <= 0 && run.PlayerTrinkets.Contains(TrinketType.PhoenixMedal) && !run.PhoenixMedalUsed)
                {
                    run.PhoenixMedalUsed = true;
                    run.Prestige = 1;
                }
                AssignBurden(run, rng);
                if (run.Prestige <= 0)
                {
                    run.RunComplete = true;
                    run.RunWon = false;
                }
            }
        }

        static void PickAbilityReward(RunState run, MapNode node, System.Random rng)
        {
            if (run.RunComplete) return;
            bool isElite = node.Type == MapNodeType.EliteMatch || node.Type == MapNodeType.BossMatch;
            var offerings = PickAbilityOfferings(run, 3, isElite, rng);
            if (offerings.Count == 0) return;

            var pick = offerings[rng.Next(offerings.Count)];
            if (run.PlayerAbilities.Count >= run.MaxAbilitySlots)
                run.PlayerAbilities.RemoveAt(0);
            run.PlayerAbilities.Add(pick);
            run.RecordAbilityPicked(pick);
        }

        static List<AbilityType> PickAbilityOfferings(RunState run, int count, bool isElite, System.Random rng)
        {
            var pool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (run.PlayerAbilities.Contains(def.Type)) continue;
                if (!def.IsNeutral && def.Owner != run.PlayerArchetype) continue;
                if (!isElite && def.Rarity == AbilityRarity.Rare && rng.Next(100) >= 30) continue;
                pool.Add(def);
            }
            var result = new List<AbilityType>();
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int idx = rng.Next(pool.Count);
                result.Add(pool[idx].Type);
                pool.RemoveAt(idx);
            }
            return result;
        }

        static void SimulateShop(RunState run, System.Random rng)
        {
            if (run.Florins >= 8 && run.PlayerAbilities.Count < run.MaxAbilitySlots)
            {
                var pool = new List<AbilityDefinition>();
                foreach (var def in AbilityPool.All)
                {
                    if (run.PlayerAbilities.Contains(def.Type)) continue;
                    if (!def.IsNeutral && def.Owner != run.PlayerArchetype) continue;
                    pool.Add(def);
                }
                if (pool.Count > 0)
                {
                    var pick = pool[rng.Next(pool.Count)];
                    int price = pick.Rarity switch
                    {
                        AbilityRarity.Common => 8,
                        AbilityRarity.Uncommon => 12,
                        AbilityRarity.Rare => 18,
                        _ => 12
                    };
                    if (run.Florins >= price)
                    {
                        run.Florins -= price;
                        run.PlayerAbilities.Add(pick.Type);
                        run.RecordAbilityPicked(pick.Type);
                    }
                }
            }

            if (run.Florins >= 15 && run.PlayerTrinkets.Count < 5)
            {
                var allTrinkets = (TrinketType[])Enum.GetValues(typeof(TrinketType));
                var available = allTrinkets.Where(t => !run.PlayerTrinkets.Contains(t)).ToArray();
                if (available.Length > 0)
                {
                    run.Florins -= 15;
                    var trinket = available[rng.Next(available.Length)];
                    run.PlayerTrinkets.Add(trinket);
                    if (trinket == TrinketType.ScholarsTome) run.MaxAbilitySlots++;
                }
            }

            if (run.PlayerBurdens.Count > 0 && run.Florins >= 6)
            {
                run.Florins -= 6;
                run.PlayerBurdens.RemoveAt(0);
            }
        }

        static void SimulateRumor(RunState run, System.Random rng)
        {
            int roll = rng.Next(100);
            if (roll < 40)
            {
                run.Florins += 5 + run.CurrentAct;
            }
            else if (roll < 70 && run.PlayerAbilities.Count < run.MaxAbilitySlots)
            {
                var pool = new List<AbilityDefinition>();
                foreach (var def in AbilityPool.All)
                {
                    if (run.PlayerAbilities.Contains(def.Type)) continue;
                    if (!def.IsNeutral && def.Owner != run.PlayerArchetype) continue;
                    pool.Add(def);
                }
                if (pool.Count > 0)
                {
                    var pick = pool[rng.Next(pool.Count)];
                    run.PlayerAbilities.Add(pick.Type);
                    run.RecordAbilityPicked(pick.Type);
                }
            }
            else
            {
                run.Florins += 3;
            }
        }

        static void SimulateRest(RunState run, System.Random rng)
        {
            if (run.PlayerBurdens.Count > 0 && rng.Next(100) < 60)
                run.PlayerBurdens.RemoveAt(0);
            else
                run.Florins += 3;
        }

        static void AssignBurden(RunState run, System.Random rng)
        {
            var all = (BurdenType[])Enum.GetValues(typeof(BurdenType));
            var available = all.Where(b => !run.PlayerBurdens.Contains(b)).ToArray();
            if (available.Length > 0)
                run.PlayerBurdens.Add(available[rng.Next(available.Length)]);
        }
    }
}
