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
            var archetypeMatchWins = new Dictionary<ArchetypeType, int>();
            var archetypeMatchPlayed = new Dictionary<ArchetypeType, int>();
            var archetypeFlorins = new Dictionary<ArchetypeType, int>();
            var abilityPicks = new Dictionary<AbilityType, int>();
            var buildPathPicks = new Dictionary<string, int>();
            var buildPathWins = new Dictionary<string, int>();
            var buildPathRuns = new Dictionary<string, int>();
            var results = new List<string>();

            for (int r = 0; r < runCount; r++)
            {
                var result = SimulateRun(Environment.TickCount + r, out var archetype, out var buildPath);
                totalMatches += result.MatchesPlayed;
                totalMatchWins += result.MatchesWon;

                if (!archetypeRuns.ContainsKey(archetype)) archetypeRuns[archetype] = 0;
                archetypeRuns[archetype]++;
                if (!archetypeMatchWins.ContainsKey(archetype)) archetypeMatchWins[archetype] = 0;
                archetypeMatchWins[archetype] += result.MatchesWon;
                if (!archetypeMatchPlayed.ContainsKey(archetype)) archetypeMatchPlayed[archetype] = 0;
                archetypeMatchPlayed[archetype] += result.MatchesPlayed;
                if (!archetypeFlorins.ContainsKey(archetype)) archetypeFlorins[archetype] = 0;
                archetypeFlorins[archetype] += result.Florins;

                string pathKey = buildPath ?? "None";
                string fullKey = $"{archetype.DisplayName()}/{pathKey}";
                if (!buildPathRuns.ContainsKey(fullKey)) buildPathRuns[fullKey] = 0;
                buildPathRuns[fullKey]++;

                if (result.RunWon)
                {
                    wins++;
                    if (!archetypeWins.ContainsKey(archetype)) archetypeWins[archetype] = 0;
                    archetypeWins[archetype]++;
                    if (!buildPathWins.ContainsKey(fullKey)) buildPathWins[fullKey] = 0;
                    buildPathWins[fullKey]++;
                }

                foreach (var kv in result.AbilityPickCount)
                {
                    if (!abilityPicks.ContainsKey(kv.Key)) abilityPicks[kv.Key] = 0;
                    abilityPicks[kv.Key] += kv.Value;
                }

                string tag = result.RunWon ? "WON" : "LOST";
                results.Add($"  {tag} | {archetype.DisplayName()} [{pathKey}] | Acts: {result.CurrentAct + 1}/5 | W/L: {result.MatchesWon}/{result.MatchesPlayed} | Florins: {result.Florins} | Burdens: {result.PlayerBurdens.Count}");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== Meta-Loop Batch: {runCount} runs ===");
            sb.AppendLine($"Run win rate: {wins}/{runCount} ({100.0 * wins / runCount:0.0}%)");
            sb.AppendLine($"Match win rate: {totalMatchWins}/{totalMatches} ({100.0 * totalMatchWins / totalMatches:0.0}%)");
            sb.AppendLine();

            sb.AppendLine("=== ARCHETYPE BREAKDOWN ===");
            foreach (var arch in ArchetypeDefinitions.AllArchetypes)
            {
                int played = archetypeRuns.GetValueOrDefault(arch, 0);
                int won = archetypeWins.GetValueOrDefault(arch, 0);
                int mw = archetypeMatchWins.GetValueOrDefault(arch, 0);
                int mp = archetypeMatchPlayed.GetValueOrDefault(arch, 0);
                int fl = archetypeFlorins.GetValueOrDefault(arch, 0);
                if (played > 0)
                    sb.AppendLine($"  {arch.DisplayName()}: {won}/{played} runs won ({100.0 * won / played:0.0}%)  |  Matches: {mw}/{mp} ({100.0 * mw / mp:0.0}%)  |  Avg Florins: {fl / played}");
            }
            sb.AppendLine();

            sb.AppendLine("=== BUILD PATH BREAKDOWN ===");
            foreach (var kv in buildPathRuns.OrderByDescending(kv => kv.Value))
            {
                int bpWon = buildPathWins.GetValueOrDefault(kv.Key, 0);
                sb.AppendLine($"  {kv.Key}: {bpWon}/{kv.Value} runs won ({100.0 * bpWon / kv.Value:0.0}%)");
            }
            sb.AppendLine();

            sb.AppendLine("=== TOP ABILITY PICKS ===");
            var sorted = abilityPicks.OrderByDescending(kv => kv.Value).Take(15);
            foreach (var kv in sorted)
            {
                var def = AbilityPool.Get(kv.Key);
                string path = def.BuildPath ?? "Neutral";
                sb.AppendLine($"  {kv.Key.DisplayName()} [{path}] ({def.Rarity}): {kv.Value}");
            }
            sb.AppendLine();

            foreach (var line in results)
                sb.AppendLine(line);

            Debug.Log(sb.ToString());
        }

        static RunState SimulateRun(int seed, out ArchetypeType archetype, out string adoptedBuildPath)
        {
            var rng = new System.Random(seed);
            var run = new RunState { Seed = seed };

            var allArch = ArchetypeDefinitions.AllArchetypes;
            archetype = allArch[rng.Next(allArch.Length)];
            run.PlayerArchetype = archetype;
            run.PlayerAbilities.AddRange(archetype.StartingAbilities());
            var trinket = archetype.StartingTrinket();
            if (trinket.HasValue) run.PlayerTrinkets.Add(trinket.Value);

            string adoptedPath = null;

            run.CurrentAct = 0;
            run.CurrentMap = MapGenerator.Generate(0, rng);

            while (!run.RunComplete && run.CurrentAct < 5)
            {
                int col = 0;
                while (col < run.CurrentMap.Count && !run.RunComplete)
                {
                    var column = run.CurrentMap[col];
                    var node = PickBestNode(column, run);
                    col++;

                    switch (node.Type)
                    {
                        case MapNodeType.RivalMatch:
                        case MapNodeType.EliteMatch:
                        case MapNodeType.BossMatch:
                            SimulateMatch(run, node, rng);
                            if (run.RunComplete) break;
                            PickAbilityReward(run, node, rng, ref adoptedPath);
                            break;
                        case MapNodeType.Shop:
                            SimulateShop(run, rng, ref adoptedPath);
                            break;
                        case MapNodeType.Rumor:
                            SimulateRumor(run, rng, ref adoptedPath);
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

            adoptedBuildPath = adoptedPath;
            return run;
        }

        static MapNode PickBestNode(List<MapNode> column, RunState run)
        {
            MapNode best = column[0];
            int bestP = NodePriority(best.Type, run);
            for (int i = 1; i < column.Count; i++)
            {
                int p = NodePriority(column[i].Type, run);
                if (p > bestP) { best = column[i]; bestP = p; }
            }
            return best;
        }

        static int NodePriority(MapNodeType t, RunState run)
        {
            switch (t)
            {
                case MapNodeType.BossMatch: return 100;
                case MapNodeType.EliteMatch: return 90;
                case MapNodeType.RivalMatch: return 70;
                case MapNodeType.Rest:
                    if (run.PlayerBurdens.Count >= 3) return 95;
                    if (run.PlayerBurdens.Count >= 2) return 80;
                    if (run.PlayerBurdens.Count >= 1) return 60;
                    return 25;
                case MapNodeType.Shop:
                    if (run.Florins >= 15 && run.PlayerAbilities.Count < run.MaxAbilitySlots) return 75;
                    if (run.Florins >= 10) return 40;
                    return 15;
                case MapNodeType.Rumor: return 20;
                default: return 0;
            }
        }

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

        static void PickAbilityReward(RunState run, MapNode node, System.Random rng, ref string adoptedPath)
        {
            if (run.RunComplete) return;
            bool isElite = node.Type == MapNodeType.EliteMatch || node.Type == MapNodeType.BossMatch;
            var offerings = PickAbilityOfferings(run, 3, isElite, rng);
            if (offerings.Count == 0) return;

            AbilityType pick;
            if (adoptedPath != null)
                pick = ArchetypeDefinitions.WeightedPick(offerings, adoptedPath, 5, 2, 1, rng);
            else
                pick = offerings[rng.Next(offerings.Count)];

            if (run.PlayerAbilities.Count >= run.MaxAbilitySlots)
            {
                int worstIdx = FindWorstAbility(run);
                run.PlayerAbilities.RemoveAt(worstIdx);
            }
            run.PlayerAbilities.Add(pick);
            run.RecordAbilityPicked(pick);

            if (adoptedPath == null)
            {
                var def = AbilityPool.Get(pick);
                if (def.BuildPath != null) adoptedPath = def.BuildPath;
            }
        }

        static List<AbilityType> PickAbilityOfferings(RunState run, int count, bool isElite, System.Random rng)
        {
            var archPool = new List<AbilityDefinition>();
            var neutralPool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (run.PlayerAbilities.Contains(def.Type)) continue;
                if (!def.IsNeutral && def.Owner != run.PlayerArchetype) continue;
                if (!isElite && def.Rarity == AbilityRarity.Rare && rng.Next(100) >= 30) continue;
                if (def.IsNeutral) neutralPool.Add(def);
                else archPool.Add(def);
            }

            var result = new List<AbilityType>();
            if (archPool.Count > 0)
            {
                int idx = rng.Next(archPool.Count);
                result.Add(archPool[idx].Type);
                archPool.RemoveAt(idx);
            }

            var combined = new List<AbilityDefinition>();
            combined.AddRange(archPool);
            combined.AddRange(neutralPool);
            while (result.Count < count && combined.Count > 0)
            {
                int idx = rng.Next(combined.Count);
                result.Add(combined[idx].Type);
                combined.RemoveAt(idx);
            }
            return result;
        }

        static void SimulateShop(RunState run, System.Random rng, ref string adoptedPath)
        {
            if (run.Florins >= 8 && run.PlayerAbilities.Count < run.MaxAbilitySlots)
            {
                var pool = new List<AbilityType>();
                foreach (var def in AbilityPool.All)
                {
                    if (run.PlayerAbilities.Contains(def.Type)) continue;
                    if (!def.IsNeutral && def.Owner != run.PlayerArchetype) continue;
                    pool.Add(def.Type);
                }
                if (pool.Count > 0)
                {
                    AbilityType pickType;
                    if (adoptedPath != null)
                        pickType = ArchetypeDefinitions.WeightedPick(pool, adoptedPath, 5, 2, 1, rng);
                    else
                        pickType = pool[rng.Next(pool.Count)];

                    var pick = AbilityPool.Get(pickType);
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
                        run.PlayerAbilities.Add(pickType);
                        run.RecordAbilityPicked(pickType);

                        if (adoptedPath == null && pick.BuildPath != null)
                            adoptedPath = pick.BuildPath;
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

        static void SimulateRumor(RunState run, System.Random rng, ref string adoptedPath)
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
                    if (!def.IsNeutral && def.Owner != run.PlayerArchetype) continue;
                    pool.Add(def.Type);
                }
                if (pool.Count > 0)
                {
                    AbilityType pick;
                    if (adoptedPath != null)
                        pick = ArchetypeDefinitions.WeightedPick(pool, adoptedPath, 5, 2, 1, rng);
                    else
                        pick = pool[rng.Next(pool.Count)];

                    run.PlayerAbilities.Add(pick);
                    run.RecordAbilityPicked(pick);

                    if (adoptedPath == null)
                    {
                        var def = AbilityPool.Get(pick);
                        if (def.BuildPath != null) adoptedPath = def.BuildPath;
                    }
                }
            }
            else
            {
                run.Florins += 3;
            }
        }

        static void SimulateRest(RunState run, System.Random rng)
        {
            if (run.PlayerBurdens.Count > 0)
                run.PlayerBurdens.RemoveAt(rng.Next(run.PlayerBurdens.Count));
            else
                run.Florins += 3;
        }

        static int FindWorstAbility(RunState run)
        {
            int worst = 0;
            int worstScore = AbilityKeepScore(run.PlayerAbilities[0], run.PlayerArchetype);
            for (int i = 1; i < run.PlayerAbilities.Count; i++)
            {
                int score = AbilityKeepScore(run.PlayerAbilities[i], run.PlayerArchetype);
                if (score < worstScore) { worst = i; worstScore = score; }
            }
            return worst;
        }

        static int AbilityKeepScore(AbilityType type, ArchetypeType? archetype)
        {
            var def = AbilityPool.Get(type);
            int score = def.Rarity switch
            {
                AbilityRarity.Common => 0,
                AbilityRarity.Uncommon => 100,
                AbilityRarity.Rare => 200,
                _ => 0
            };
            if (!def.IsNeutral && def.Owner == archetype) score += 50;
            return score;
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
