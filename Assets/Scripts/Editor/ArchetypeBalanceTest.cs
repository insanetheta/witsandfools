using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class ArchetypeBalanceTest
    {
        [MenuItem("Wits and Fools/Smoke Test/Archetype Balance (25 per combo)")]
        public static void Run25() => RunAll(25);

        [MenuItem("Wits and Fools/Smoke Test/Archetype Balance (50 per combo)")]
        public static void Run50() => RunAll(50);

        static void RunAll(int runsPerCombo)
        {
            var rows = new List<ResultRow>();
            int baseSeed = Environment.TickCount;

            foreach (var arch in ArchetypeDefinitions.AllArchetypes)
            {
                foreach (var path in arch.BuildPaths())
                {
                    var row = RunBatch(arch, path, runsPerCombo, baseSeed);
                    rows.Add(row);
                    baseSeed += runsPerCombo;
                }
            }

            PrintReport(rows, runsPerCombo);
        }

        struct ResultRow
        {
            public ArchetypeType Archetype;
            public string BuildPath;
            public int Runs, Wins;
            public int TotalActsSurvived, TotalMatchesPlayed, TotalMatchesWon;
            public int TotalFlorins;
            public int OnPathPicks, TotalPicks;
            public Dictionary<AbilityType, int> AbilityPicks;
        }

        static ResultRow RunBatch(ArchetypeType archetype, string buildPath, int count, int baseSeed)
        {
            var row = new ResultRow
            {
                Archetype = archetype,
                BuildPath = buildPath,
                Runs = count,
                AbilityPicks = new Dictionary<AbilityType, int>()
            };

            for (int i = 0; i < count; i++)
            {
                var result = SimulateRun(archetype, buildPath, baseSeed + i,
                    out int onPath, out int totalPicks);
                if (result.RunWon) row.Wins++;
                row.TotalActsSurvived += result.CurrentAct;
                row.TotalMatchesPlayed += result.MatchesPlayed;
                row.TotalMatchesWon += result.MatchesWon;
                row.TotalFlorins += result.Florins;
                row.OnPathPicks += onPath;
                row.TotalPicks += totalPicks;

                foreach (var kv in result.AbilityPickCount)
                {
                    if (!row.AbilityPicks.ContainsKey(kv.Key)) row.AbilityPicks[kv.Key] = 0;
                    row.AbilityPicks[kv.Key] += kv.Value;
                }
            }

            return row;
        }

        static AIArchetypeName AiStyleForPath(string path) => path switch
        {
            "Shadow" => AIArchetypeName.Miser,
            "Spy" => AIArchetypeName.Scholar,
            "Saboteur" => AIArchetypeName.Fox,
            "Berserker" => AIArchetypeName.Brawler,
            "Brawler" => AIArchetypeName.Brawler,
            "Warlord" => AIArchetypeName.Noble,
            "Courtier" => AIArchetypeName.Noble,
            "Puppeteer" => AIArchetypeName.Fox,
            "Peacemaker" => AIArchetypeName.Miser,
            "CardShark" => AIArchetypeName.Scholar,
            "HighRoller" => AIArchetypeName.Brawler,
            "Trickster" => AIArchetypeName.Fox,
            _ => AIArchetypeName.Scholar,
        };

        static RunState SimulateRun(ArchetypeType archetype, string buildPath, int seed,
            out int onPathPicks, out int totalPicks)
        {
            var rng = new System.Random(seed);
            var run = new RunState
            {
                Seed = seed,
                PlayerArchetype = archetype,
                CurrentAct = 0
            };

            run.PlayerAbilities.AddRange(archetype.StartingAbilities());
            var trinket = archetype.StartingTrinket();
            if (trinket.HasValue)
                run.PlayerTrinkets.Add(trinket.Value);

            run.CurrentMap = MapGenerator.Generate(0, rng);
            onPathPicks = 0;
            totalPicks = 0;

            while (!run.RunComplete && run.CurrentAct < 5)
            {
                int col = 0;
                while (col < run.CurrentMap.Count && !run.RunComplete)
                {
                    var column = run.CurrentMap[col];
                    var node = PickNode(column);
                    col++;

                    switch (node.Type)
                    {
                        case MapNodeType.RivalMatch:
                        case MapNodeType.EliteMatch:
                        case MapNodeType.BossMatch:
                            SimulateMatch(run, node, buildPath, rng);
                            if (run.RunComplete) break;
                            PickAbilityReward(run, node, archetype, buildPath, rng,
                                ref onPathPicks, ref totalPicks);
                            break;
                        case MapNodeType.Shop:
                            SimulateShop(run, archetype, buildPath, rng,
                                ref onPathPicks, ref totalPicks);
                            break;
                        case MapNodeType.Rumor:
                            SimulateRumor(run, archetype, buildPath, rng,
                                ref onPathPicks, ref totalPicks);
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

        static MapNode PickNode(List<MapNode> column)
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

        static void SimulateMatch(RunState run, MapNode node, string buildPath, System.Random rng)
        {
            var config = MatchSetup.Build(run, node.Opponent, rng);
            var engine = new GameEngine(rng.Next(), config);
            engine.StartNewGame();

            var ai0 = new AIPlayer("Player", rng.Next());
            AIArchetypes.Apply(ai0, AiStyleForPath(buildPath), 4);
            ai0.RandomMoveChance = 0f;

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

        static void PickAbilityReward(RunState run, MapNode node, ArchetypeType archetype,
            string buildPath, System.Random rng, ref int onPathPicks, ref int totalPicks)
        {
            if (run.RunComplete) return;
            bool isElite = node.Type == MapNodeType.EliteMatch || node.Type == MapNodeType.BossMatch;
            var offerings = PickAbilityOfferings(run, 3, isElite, archetype, rng);
            if (offerings.Count == 0) return;

            var pick = ArchetypeDefinitions.WeightedPick(offerings, buildPath, 10, 2, 1, rng);
            if (run.PlayerAbilities.Count >= run.MaxAbilitySlots)
                run.PlayerAbilities.RemoveAt(0);
            run.PlayerAbilities.Add(pick);
            run.RecordAbilityPicked(pick);

            totalPicks++;
            var def = AbilityPool.Get(pick);
            if (def.BuildPath == buildPath) onPathPicks++;
        }

        static List<AbilityType> PickAbilityOfferings(RunState run, int count, bool isElite,
            ArchetypeType archetype, System.Random rng)
        {
            var pool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (run.PlayerAbilities.Contains(def.Type)) continue;
                if (!def.IsNeutral && def.Owner != archetype) continue;
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

        static void SimulateShop(RunState run, ArchetypeType archetype, string buildPath,
            System.Random rng, ref int onPathPicks, ref int totalPicks)
        {
            if (run.Florins >= 8 && run.PlayerAbilities.Count < run.MaxAbilitySlots)
            {
                var pool = new List<AbilityType>();
                foreach (var def in AbilityPool.All)
                {
                    if (run.PlayerAbilities.Contains(def.Type)) continue;
                    if (!def.IsNeutral && def.Owner != archetype) continue;
                    pool.Add(def.Type);
                }
                if (pool.Count > 0)
                {
                    var pick = ArchetypeDefinitions.WeightedPick(pool, buildPath, 10, 2, 1, rng);
                    var def = AbilityPool.Get(pick);
                    int price = def.Rarity switch
                    {
                        AbilityRarity.Common => 8,
                        AbilityRarity.Uncommon => 12,
                        AbilityRarity.Rare => 18,
                        _ => 12
                    };
                    if (run.Florins >= price)
                    {
                        run.Florins -= price;
                        run.PlayerAbilities.Add(pick);
                        run.RecordAbilityPicked(pick);

                        totalPicks++;
                        if (def.BuildPath == buildPath) onPathPicks++;
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

        static void SimulateRumor(RunState run, ArchetypeType archetype, string buildPath,
            System.Random rng, ref int onPathPicks, ref int totalPicks)
        {
            int roll = rng.Next(100);
            if (roll < 40)
            {
                run.Florins += 5 + run.CurrentAct;
            }
            else if (roll < 70 && run.PlayerAbilities.Count < run.MaxAbilitySlots)
            {
                var pool = new List<AbilityType>();
                foreach (var def in AbilityPool.All)
                {
                    if (run.PlayerAbilities.Contains(def.Type)) continue;
                    if (!def.IsNeutral && def.Owner != archetype) continue;
                    pool.Add(def.Type);
                }
                if (pool.Count > 0)
                {
                    var pick = ArchetypeDefinitions.WeightedPick(pool, buildPath, 10, 2, 1, rng);
                    run.PlayerAbilities.Add(pick);
                    run.RecordAbilityPicked(pick);

                    totalPicks++;
                    var def = AbilityPool.Get(pick);
                    if (def.BuildPath == buildPath) onPathPicks++;
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

        static void PrintReport(List<ResultRow> rows, int runsPerCombo)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Build Path Balance: {runsPerCombo} runs per combo, {rows.Count} combos ===");
            sb.AppendLine();

            sb.AppendLine($"{"Archetype",-14} {"Build Path",-12} {"Win%",6} {"AvgAct",7} {"MtchW%",7} {"Path%",6} {"Runs",5}");
            sb.AppendLine(new string('-', 62));

            foreach (var r in rows)
            {
                double winPct = 100.0 * r.Wins / r.Runs;
                double avgAct = (double)r.TotalActsSurvived / r.Runs;
                double matchWinPct = r.TotalMatchesPlayed > 0
                    ? 100.0 * r.TotalMatchesWon / r.TotalMatchesPlayed : 0;
                double pathPct = r.TotalPicks > 0 ? 100.0 * r.OnPathPicks / r.TotalPicks : 0;

                sb.AppendLine($"{r.Archetype.DisplayName(),-14} {r.BuildPath,-12} {winPct,5:0.0}% {avgAct,6:0.0} {matchWinPct,5:0.0}% {pathPct,4:0.0}% {r.Runs,5}");
            }

            sb.AppendLine();
            sb.AppendLine("--- Per-Archetype Summary ---");
            sb.AppendLine($"{"Archetype",-14} {"Win%",6} {"AvgAct",7} {"MtchW%",7} {"BestPath",-12}");
            sb.AppendLine(new string('-', 52));

            foreach (var arch in ArchetypeDefinitions.AllArchetypes)
            {
                var group = rows.Where(r => r.Archetype == arch).ToList();
                int totalRuns = group.Sum(r => r.Runs);
                int totalWins = group.Sum(r => r.Wins);
                int totalActs = group.Sum(r => r.TotalActsSurvived);
                int totalMPlayed = group.Sum(r => r.TotalMatchesPlayed);
                int totalMWon = group.Sum(r => r.TotalMatchesWon);
                var best = group.OrderByDescending(r => (double)r.Wins / r.Runs).First();

                double winPct = 100.0 * totalWins / totalRuns;
                double avgAct = (double)totalActs / totalRuns;
                double matchPct = totalMPlayed > 0 ? 100.0 * totalMWon / totalMPlayed : 0;

                sb.AppendLine($"{arch.DisplayName(),-14} {winPct,5:0.0}% {avgAct,6:0.0} {matchPct,5:0.0}% {best.BuildPath,-12}");
            }

            sb.AppendLine();
            sb.AppendLine("--- Top Ability Picks per Build Path ---");
            foreach (var r in rows)
            {
                var top3 = r.AbilityPicks.OrderByDescending(kv => kv.Value).Take(3);
                var names = string.Join(", ", top3.Select(kv => $"{kv.Key.DisplayName()}({kv.Value})"));
                sb.AppendLine($"  {r.Archetype.DisplayName()}/{r.BuildPath}: {names}");
            }

            Debug.Log(sb.ToString());
        }
    }
}
