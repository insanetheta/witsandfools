using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public enum Playstyle { Aggressive, Defensive, Greedy, Balanced }

    public static class ArchetypeBalanceTest
    {
        const int RunsPerCombo = 25;

        [MenuItem("Wits and Fools/Smoke Test/Archetype Balance (25 per combo)")]
        public static void Run25() => RunAll(25);

        [MenuItem("Wits and Fools/Smoke Test/Archetype Balance (50 per combo)")]
        public static void Run50() => RunAll(50);

        static void RunAll(int runsPerCombo)
        {
            var archetypes = ArchetypeDefinitions.AllArchetypes;
            var playstyles = (Playstyle[])Enum.GetValues(typeof(Playstyle));

            var rows = new List<ResultRow>();
            int baseSeed = Environment.TickCount;

            foreach (var arch in archetypes)
            {
                foreach (var style in playstyles)
                {
                    var row = RunBatch(arch, style, runsPerCombo, baseSeed);
                    rows.Add(row);
                    baseSeed += runsPerCombo;
                }
            }

            PrintReport(rows, runsPerCombo);
        }

        struct ResultRow
        {
            public ArchetypeType Archetype;
            public Playstyle Playstyle;
            public int Runs, Wins;
            public int TotalActsSurvived, TotalMatchesPlayed, TotalMatchesWon;
            public int TotalFlorins;
            public Dictionary<AbilityType, int> AbilityPicks;
        }

        static ResultRow RunBatch(ArchetypeType archetype, Playstyle style, int count, int baseSeed)
        {
            var row = new ResultRow
            {
                Archetype = archetype,
                Playstyle = style,
                Runs = count,
                AbilityPicks = new Dictionary<AbilityType, int>()
            };

            for (int i = 0; i < count; i++)
            {
                var result = SimulateRun(archetype, style, baseSeed + i);
                if (result.RunWon) row.Wins++;
                row.TotalActsSurvived += result.CurrentAct + (result.RunWon ? 0 : 0);
                row.TotalMatchesPlayed += result.MatchesPlayed;
                row.TotalMatchesWon += result.MatchesWon;
                row.TotalFlorins += result.Florins;

                foreach (var kv in result.AbilityPickCount)
                {
                    if (!row.AbilityPicks.ContainsKey(kv.Key)) row.AbilityPicks[kv.Key] = 0;
                    row.AbilityPicks[kv.Key] += kv.Value;
                }
            }

            return row;
        }

        static RunState SimulateRun(ArchetypeType archetype, Playstyle style, int seed)
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

            while (!run.RunComplete && run.CurrentAct < 5)
            {
                int col = 0;
                while (col < run.CurrentMap.Count && !run.RunComplete)
                {
                    var column = run.CurrentMap[col];
                    var node = PickNode(column, style);
                    col++;

                    switch (node.Type)
                    {
                        case MapNodeType.RivalMatch:
                        case MapNodeType.EliteMatch:
                        case MapNodeType.BossMatch:
                            SimulateMatch(run, node, style, rng);
                            if (run.RunComplete) break;
                            PickAbilityReward(run, node, archetype, style, rng);
                            break;
                        case MapNodeType.Shop:
                            SimulateShop(run, archetype, style, rng);
                            break;
                        case MapNodeType.Rumor:
                            SimulateRumor(run, archetype, rng);
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

        static MapNode PickNode(List<MapNode> column, Playstyle style)
        {
            MapNode best = column[0];
            int bestP = NodePriority(best.Type, style);
            for (int i = 1; i < column.Count; i++)
            {
                int p = NodePriority(column[i].Type, style);
                if (p > bestP) { best = column[i]; bestP = p; }
            }
            return best;
        }

        static int NodePriority(MapNodeType t, Playstyle style) => style switch
        {
            Playstyle.Aggressive => t switch
            {
                MapNodeType.BossMatch => 100,
                MapNodeType.EliteMatch => 95,
                MapNodeType.RivalMatch => 80,
                MapNodeType.Rest => 20,
                MapNodeType.Rumor => 15,
                MapNodeType.Shop => 10,
                _ => 0
            },
            Playstyle.Defensive => t switch
            {
                MapNodeType.BossMatch => 100,
                MapNodeType.Rest => 80,
                MapNodeType.Shop => 70,
                MapNodeType.Rumor => 50,
                MapNodeType.RivalMatch => 40,
                MapNodeType.EliteMatch => 30,
                _ => 0
            },
            Playstyle.Greedy => t switch
            {
                MapNodeType.BossMatch => 100,
                MapNodeType.Shop => 85,
                MapNodeType.Rumor => 75,
                MapNodeType.RivalMatch => 60,
                MapNodeType.Rest => 40,
                MapNodeType.EliteMatch => 35,
                _ => 0
            },
            _ => t switch // Balanced
            {
                MapNodeType.BossMatch => 100,
                MapNodeType.EliteMatch => 90,
                MapNodeType.RivalMatch => 80,
                MapNodeType.Rest => 30,
                MapNodeType.Rumor => 20,
                MapNodeType.Shop => 10,
                _ => 0
            }
        };

        static AIArchetypeName PlayerAiStyle(Playstyle style) => style switch
        {
            Playstyle.Aggressive => AIArchetypeName.Brawler,
            Playstyle.Defensive => AIArchetypeName.Miser,
            Playstyle.Greedy => AIArchetypeName.Fox,
            _ => AIArchetypeName.Scholar,
        };

        static void SimulateMatch(RunState run, MapNode node, Playstyle style, System.Random rng)
        {
            var config = MatchSetup.Build(run, node.Opponent, rng);
            var engine = new GameEngine(rng.Next(), config);
            engine.StartNewGame();

            var ai0 = new AIPlayer("Player", rng.Next());
            AIArchetypes.Apply(ai0, PlayerAiStyle(style), 4);
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

        static void PickAbilityReward(RunState run, MapNode node, ArchetypeType archetype, Playstyle style, System.Random rng)
        {
            if (run.RunComplete) return;
            bool isElite = node.Type == MapNodeType.EliteMatch || node.Type == MapNodeType.BossMatch;
            var offerings = PickAbilityOfferings(run, 3, isElite, archetype, rng);
            if (offerings.Count == 0) return;

            var pick = ChooseAbility(offerings, archetype, style, rng);
            if (run.PlayerAbilities.Count >= run.MaxAbilitySlots)
                run.PlayerAbilities.RemoveAt(0);
            run.PlayerAbilities.Add(pick);
            run.RecordAbilityPicked(pick);
        }

        static AbilityType ChooseAbility(List<AbilityType> offerings, ArchetypeType archetype, Playstyle style, System.Random rng)
        {
            if (style == Playstyle.Balanced)
                return offerings[rng.Next(offerings.Count)];

            var scored = new List<(AbilityType type, int score)>();
            foreach (var a in offerings)
            {
                int score = 10;
                if (archetype.IsSynergy(a)) score += 5;
                score += PlaystyleBonus(a, style);
                scored.Add((a, score));
            }

            int total = scored.Sum(s => s.score);
            int roll = rng.Next(total);
            int acc = 0;
            foreach (var s in scored)
            {
                acc += s.score;
                if (roll < acc) return s.type;
            }
            return scored[^1].type;
        }

        static int PlaystyleBonus(AbilityType a, Playstyle style) => style switch
        {
            Playstyle.Aggressive => a switch
            {
                AbilityType.DoubleTrouble or AbilityType.PileOn or AbilityType.Feint
                    or AbilityType.Gambit or AbilityType.ExtraDraw => 8,
                AbilityType.SeizeInitiative or AbilityType.TrumpAffinity => 4,
                _ => 0
            },
            Playstyle.Defensive => a switch
            {
                AbilityType.Blocker or AbilityType.DoubleDefense or AbilityType.SlipAway
                    or AbilityType.Deflect => 8,
                AbilityType.EndgameSpecialist or AbilityType.CardCounter => 4,
                _ => 0
            },
            Playstyle.Greedy => a switch
            {
                AbilityType.Peek or AbilityType.CardCounter or AbilityType.ExtraDraw => 6,
                AbilityType.TrumpAffinity or AbilityType.QuickHands => 4,
                _ => 0
            },
            _ => 0
        };

        static List<AbilityType> PickAbilityOfferings(RunState run, int count, bool isElite, ArchetypeType archetype, System.Random rng)
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

        static void SimulateShop(RunState run, ArchetypeType archetype, Playstyle style, System.Random rng)
        {
            bool preferTrinkets = style == Playstyle.Greedy;

            if (preferTrinkets && run.Florins >= 15 && run.PlayerTrinkets.Count < 5)
            {
                BuyTrinket(run, rng);
            }

            if (run.Florins >= 8 && run.PlayerAbilities.Count < run.MaxAbilitySlots)
            {
                var pool = new List<AbilityDefinition>();
                foreach (var def in AbilityPool.All)
                {
                    if (run.PlayerAbilities.Contains(def.Type)) continue;
                    if (!def.IsNeutral && def.Owner != archetype) continue;
                    pool.Add(def);
                }
                if (pool.Count > 0)
                {
                    int idx = rng.Next(pool.Count);
                    var pick = pool[idx];
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

            if (!preferTrinkets && run.Florins >= 15 && run.PlayerTrinkets.Count < 5)
            {
                BuyTrinket(run, rng);
            }

            if (run.PlayerBurdens.Count > 0 && run.Florins >= 6)
            {
                run.Florins -= 6;
                run.PlayerBurdens.RemoveAt(0);
            }
        }

        static void BuyTrinket(RunState run, System.Random rng)
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

        static void SimulateRumor(RunState run, ArchetypeType archetype, System.Random rng)
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
                    if (!def.IsNeutral && def.Owner != archetype) continue;
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

        static void PrintReport(List<ResultRow> rows, int runsPerCombo)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Archetype Balance Test: {runsPerCombo} runs per combo, {rows.Count} combos ===");
            sb.AppendLine();

            // Header
            sb.AppendLine($"{"Archetype",-14} {"Playstyle",-12} {"Win%",6} {"AvgAct",7} {"MtchW%",7} {"AvgFl",6} {"Runs",5}");
            sb.AppendLine(new string('-', 62));

            foreach (var r in rows)
            {
                double winPct = 100.0 * r.Wins / r.Runs;
                double avgAct = (double)r.TotalActsSurvived / r.Runs;
                double matchWinPct = r.TotalMatchesPlayed > 0
                    ? 100.0 * r.TotalMatchesWon / r.TotalMatchesPlayed : 0;
                double avgFlorins = (double)r.TotalFlorins / r.Runs;

                sb.AppendLine($"{r.Archetype.DisplayName(),-14} {r.Playstyle,-12} {winPct,5:0.0}% {avgAct,6:0.0} {matchWinPct,5:0.0}% {avgFlorins,5:0.0} {r.Runs,5}");
            }

            // Aggregate per archetype
            sb.AppendLine();
            sb.AppendLine("--- Per-Archetype Summary ---");
            sb.AppendLine($"{"Archetype",-14} {"Win%",6} {"AvgAct",7} {"MtchW%",7} {"BestStyle",-12}");
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

                sb.AppendLine($"{arch.DisplayName(),-14} {winPct,5:0.0}% {avgAct,6:0.0} {matchPct,5:0.0}% {best.Playstyle,-12}");
            }

            // Aggregate per playstyle
            sb.AppendLine();
            sb.AppendLine("--- Per-Playstyle Summary ---");
            sb.AppendLine($"{"Playstyle",-12} {"Win%",6} {"AvgAct",7} {"MtchW%",7}");
            sb.AppendLine(new string('-', 38));

            foreach (Playstyle style in Enum.GetValues(typeof(Playstyle)))
            {
                var group = rows.Where(r => r.Playstyle == style).ToList();
                int totalRuns = group.Sum(r => r.Runs);
                int totalWins = group.Sum(r => r.Wins);
                int totalActs = group.Sum(r => r.TotalActsSurvived);
                int totalMPlayed = group.Sum(r => r.TotalMatchesPlayed);
                int totalMWon = group.Sum(r => r.TotalMatchesWon);

                double winPct = 100.0 * totalWins / totalRuns;
                double avgAct = (double)totalActs / totalRuns;
                double matchPct = totalMPlayed > 0 ? 100.0 * totalMWon / totalMPlayed : 0;

                sb.AppendLine($"{style,-12} {winPct,5:0.0}% {avgAct,6:0.0} {matchPct,5:0.0}%");
            }

            // Top ability picks per archetype
            sb.AppendLine();
            sb.AppendLine("--- Top Ability Picks per Archetype ---");
            foreach (var arch in ArchetypeDefinitions.AllArchetypes)
            {
                var merged = new Dictionary<AbilityType, int>();
                foreach (var r in rows.Where(r => r.Archetype == arch))
                    foreach (var kv in r.AbilityPicks)
                    {
                        if (!merged.ContainsKey(kv.Key)) merged[kv.Key] = 0;
                        merged[kv.Key] += kv.Value;
                    }
                var top5 = merged.OrderByDescending(kv => kv.Value).Take(5);
                var names = string.Join(", ", top5.Select(kv => $"{kv.Key.DisplayName()}({kv.Value})"));
                sb.AppendLine($"  {arch.DisplayName()}: {names}");
            }

            Debug.Log(sb.ToString());
        }
    }
}
