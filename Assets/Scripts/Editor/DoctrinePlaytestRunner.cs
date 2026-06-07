using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace WitsAndFools.EditorTools
{
    public static class DoctrinePlaytestRunner
    {
        const int RUNS_PER_DOCTRINE = 50;

        [MenuItem("Wits and Fools/Playtest/All Doctrines Act Breakdown (50 each)")]
        public static void RunAll()
        {
            CardCatalogLoader.LoadFromJson(File.ReadAllText("Assets/Data/card_catalog.json"));
            var rosterPath = "Assets/Data/enemy_roster.json";
            if (File.Exists(rosterPath))
                DoctrineRoster.RegisterAll(DoctrineRoster.ParseJson(File.ReadAllText(rosterPath)));
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║     DOCTRINE PLAYTEST: Act-by-Act Gameplay Analysis         ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            var allResults = new Dictionary<DoctrineType, List<RunLog>>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (var doctrine in new[] { DoctrineType.Schemer, DoctrineType.Brute, DoctrineType.Trickster, DoctrineType.Hoarder })
            {
                var runs = new List<RunLog>();
                for (int i = 0; i < RUNS_PER_DOCTRINE; i++)
                    runs.Add(PlayFullRun(doctrine, 1000 + (int)doctrine * 1000 + i));
                allResults[doctrine] = runs;
            }

            sw.Stop();
            sb.AppendLine($"Simulated {RUNS_PER_DOCTRINE * 4} runs in {sw.ElapsedMilliseconds}ms\n");

            foreach (var kv in allResults)
                AppendDoctrineReport(sb, kv.Key, kv.Value);

            AppendCrossDoctrineComparison(sb, allResults);

            Debug.Log(sb.ToString());
        }

        static RunLog PlayFullRun(DoctrineType doctrine, int seed)
        {
            var rng = new Random(seed);
            var run = new RunState { Seed = seed };
            run.InitDoctrineDeck(doctrine);
            if (doctrine == DoctrineType.Hoarder) run.Prestige = 8;
            if (doctrine == DoctrineType.Schemer)
                TryRemoveWeakestCard(run, rng);

            var log = new RunLog { Doctrine = doctrine, Seed = seed };

            for (int act = 0; act < 5; act++)
            {
                run.CurrentAct = act;
                var map = MapGenerator.Generate(act, rng);
                run.CurrentMap = map;

                var actLog = new ActLog { ActIndex = act };
                actLog.DeckSizeStart = run.PlayerDeckCardIds.Count;
                actLog.AbilityCountStart = run.PlayerAbilities.Count;
                actLog.RelicCount = run.PlayerRelics.Count;
                actLog.TrinketCount = run.PlayerTrinkets.Count;
                actLog.FlorinsStart = run.Florins;
                actLog.BurdenCount = run.PlayerBurdens.Count;

                foreach (var column in map)
                {
                    var node = PickNode(column, rng);
                    actLog.NodesVisited++;

                    switch (node.Type)
                    {
                        case MapNodeType.RivalMatch:
                        case MapNodeType.EliteMatch:
                        case MapNodeType.BossMatch:
                            var matchResult = PlayMatch(run, node, rng);
                            actLog.Matches.Add(matchResult);
                            run.MatchesPlayed++;

                            if (matchResult.Won)
                            {
                                run.MatchesWon++;
                                int florins = CalculateFlorins(act, node.Type);
                                if (run.PlayerDoctrine == DoctrineType.Hoarder)
                                    florins += Math.Max(0, (run.PlayerDeckCardIds.Count - 10) / 3);
                                run.Florins += florins;

                                bool skipReward = ShouldSkipCardReward(run, rng);
                                if (!skipReward)
                                    actLog.CardsAdded += AwardDoctrineCards(run, node.Type == MapNodeType.EliteMatch, rng);

                                ApplyPostMatchDeckManagement(run, node.Type == MapNodeType.EliteMatch, rng);

                                if (node.Type == MapNodeType.EliteMatch)
                                    TryAwardRelic(run, rng);
                            }
                            else
                            {
                                run.Prestige--;
                                if (run.Prestige <= 0)
                                {
                                    actLog.DeckSizeEnd = run.PlayerDeckCardIds.Count;
                                    actLog.FlorinsEnd = run.Florins;
                                    log.Acts.Add(actLog);
                                    log.FarthestAct = act;
                                    log.Won = false;
                                    return log;
                                }
                            }
                            break;

                        case MapNodeType.Shop:
                            SimulateShop(run, rng, actLog);
                            break;

                        case MapNodeType.Rumor:
                            SimulateRumor(run, rng, actLog);
                            break;

                        case MapNodeType.Rest:
                            SimulateRest(run, rng, actLog);
                            break;
                    }
                }

                actLog.DeckSizeEnd = run.PlayerDeckCardIds.Count;
                actLog.FlorinsEnd = run.Florins;
                log.Acts.Add(actLog);
            }

            log.FarthestAct = 4;
            log.Won = true;
            return log;
        }

        static MapNode PickNode(List<MapNode> column, Random rng)
        {
            var matches = column.Where(n => n.Type == MapNodeType.RivalMatch || n.Type == MapNodeType.EliteMatch || n.Type == MapNodeType.BossMatch).ToList();
            if (matches.Count > 0) return matches[rng.Next(matches.Count)];
            return column[rng.Next(column.Count)];
        }

        static MatchLog PlayMatch(RunState run, MapNode node, Random rng)
        {
            var opponent = node.Opponent;
            var matchLog = new MatchLog
            {
                NodeType = node.Type,
                OpponentName = opponent.Name,
                ActIndex = run.CurrentAct,
                PlayerDeckSize = run.PlayerDeckCardIds.Count,
            };

            int matchSeed = rng.Next();
            var (config, pDeck, eDeck) = MatchSetup.Build(run, opponent, rng);
            var engine = new GameEngine(matchSeed, config, pDeck, eDeck);

            var ai0 = new AIPlayer("Player", matchSeed);
            var ai1 = new AIPlayer(opponent.Name, matchSeed + 1);
            AIArchetypes.Apply(ai1, opponent.Archetype, opponent.ActIndex);

            int boutCount = 0;
            int abilitiesUsed = 0;
            int cardsPlayed = 0;

            engine.OnBoutResolved += _ => boutCount++;
            engine.OnAbilityUsed += (pi, card, ability) => { if (pi == 0) abilitiesUsed++; };
            engine.OnAttackPlayed += (pi, card) => { if (pi == 0) cardsPlayed++; };

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
                if (active == 0) ai0.RequestAction(engine, 0);
                else ai1.RequestAction(engine, 1);
            }

            matchLog.Won = engine.WinnerIndex == 0;
            matchLog.BoutCount = boutCount;
            matchLog.AbilitiesUsed = abilitiesUsed;
            matchLog.CardsPlayed = cardsPlayed;
            matchLog.Stalled = safety >= 5000;
            matchLog.TurnsPlayed = safety;

            return matchLog;
        }

        static int AwardDoctrineCards(RunState run, bool isElite, Random rng)
        {
            if (!run.PlayerDoctrine.HasValue) return 0;
            var pool = CardCatalog.Draftable(run.PlayerDoctrine.Value);
            var candidates = pool.Where(c => !run.PlayerDeckCardIds.Contains(c.Id)).ToList();
            if (candidates.Count == 0) return 0;

            switch (run.PlayerDoctrine.Value)
            {
                case DoctrineType.Schemer:
                {
                    int picks = Math.Min(3, candidates.Count);
                    var options = new List<CardDefinition>();
                    var temp = new List<CardDefinition>(candidates);
                    for (int i = 0; i < picks; i++)
                    {
                        int idx = rng.Next(temp.Count);
                        options.Add(temp[idx]);
                        temp.RemoveAt(idx);
                    }
                    var best = options.OrderByDescending(c => (int)c.Rarity).First();
                    run.PlayerDeckCardIds.Add(best.Id);
                    return 1;
                }
                case DoctrineType.Hoarder:
                {
                    int picks = Math.Min(3, candidates.Count);
                    var options = new List<CardDefinition>();
                    var temp = new List<CardDefinition>(candidates);
                    for (int i = 0; i < picks; i++)
                    {
                        int idx = rng.Next(temp.Count);
                        options.Add(temp[idx]);
                        temp.RemoveAt(idx);
                    }
                    var best = options.OrderByDescending(c => (int)c.Rarity).First();
                    run.PlayerDeckCardIds.Add(best.Id);
                    return 1;
                }
                case DoctrineType.Trickster:
                {
                    if (run.CurrentAct <= 2)
                    {
                        var nonRare = candidates.Where(c => c.Rarity != CardRarity.Rare).ToList();
                        if (nonRare.Count > 0) candidates = nonRare;
                    }
                    var uncommonPlus = candidates.Where(c => c.Rarity >= CardRarity.Uncommon).ToList();
                    if (uncommonPlus.Count > 0 && rng.Next(100) < 40)
                    {
                        run.PlayerDeckCardIds.Add(uncommonPlus[rng.Next(uncommonPlus.Count)].Id);
                        return 1;
                    }
                    var commons = candidates.Where(c => c.Rarity == CardRarity.Common).ToList();
                    if (commons.Count > 0)
                    {
                        run.PlayerDeckCardIds.Add(commons[rng.Next(commons.Count)].Id);
                        return 1;
                    }
                    run.PlayerDeckCardIds.Add(candidates[rng.Next(candidates.Count)].Id);
                    return 1;
                }
                default:
                {
                    if (isElite)
                    {
                        var rares = candidates.Where(c => c.Rarity == CardRarity.Rare).ToList();
                        if (rares.Count > 0 && rng.Next(100) < 40)
                        {
                            run.PlayerDeckCardIds.Add(rares[rng.Next(rares.Count)].Id);
                            return 1;
                        }
                    }
                    run.PlayerDeckCardIds.Add(candidates[rng.Next(candidates.Count)].Id);
                    return 1;
                }
            }
        }

        static void TryRemoveWeakestCard(RunState run, Random rng)
        {
            if (run.PlayerDeckCardIds.Count <= 8) return;
            var commons = run.PlayerDeckCardIds
                .Select(id => CardCatalog.Get(id))
                .Where(c => c != null && c.Rarity == CardRarity.Common)
                .ToList();
            if (commons.Count == 0) return;
            var weakest = commons[rng.Next(commons.Count)];
            run.PlayerDeckCardIds.Remove(weakest.Id);
        }

        static bool ShouldSkipCardReward(RunState run, Random rng)
        {
            if (!run.PlayerDoctrine.HasValue) return false;
            switch (run.PlayerDoctrine.Value)
            {
                case DoctrineType.Schemer:
                    if (run.PlayerDeckCardIds.Count > 14) return rng.Next(100) < 60;
                    if (run.PlayerDeckCardIds.Count > 12) return rng.Next(100) < 30;
                    return false;
                case DoctrineType.Brute:
                    if (run.PlayerDeckCardIds.Count > 16) return rng.Next(100) < 30;
                    return false;
                case DoctrineType.Trickster:
                    if (run.PlayerDeckCardIds.Count > 18) return rng.Next(100) < 40;
                    if (run.PlayerDeckCardIds.Count > 15) return rng.Next(100) < 20;
                    return false;
                case DoctrineType.Hoarder:
                    if (run.PlayerDeckCardIds.Count > 16) return rng.Next(100) < 30;
                    if (run.PlayerDeckCardIds.Count > 14) return rng.Next(100) < 15;
                    return false;
                default:
                    return false;
            }
        }

        static void ApplyPostMatchDeckManagement(RunState run, bool isElite, Random rng)
        {
            if (!run.PlayerDoctrine.HasValue) return;
            switch (run.PlayerDoctrine.Value)
            {
                case DoctrineType.Schemer:
                    // Expurgator/Censor: if owned, chance to exile a card post-match
                    if (run.PlayerDeckCardIds.Contains("schemer_expurgator") && rng.Next(100) < 40)
                        TryRemoveWeakestCard(run, rng);
                    if (run.PlayerDeckCardIds.Contains("schemer_censor") && rng.Next(100) < 25)
                        TryRemoveWeakestCard(run, rng);
                    break;
                case DoctrineType.Brute:
                    if (rng.Next(100) < 15)
                        TryRemoveWeakestCard(run, rng);
                    if (run.PlayerDeckCardIds.Contains("brute_scorched_earth") && rng.Next(100) < 20)
                        TryRemoveWeakestCard(run, rng);
                    if (run.PlayerDeckCardIds.Contains("brute_forge_master") && isElite && rng.Next(100) < 40)
                        TryRemoveWeakestCard(run, rng);
                    break;
                case DoctrineType.Trickster:
                    // Transform only at shops — no free post-match transforms
                    break;
                case DoctrineType.Hoarder:
                    if (run.PlayerDeckCardIds.Count > 12 && rng.Next(100) < 15)
                        TryRemoveWeakestCard(run, rng);
                    break;
            }
        }

        static void TryTransformCard(RunState run, Random rng)
        {
            if (!run.PlayerDoctrine.HasValue) return;
            var pool = CardCatalog.Draftable(run.PlayerDoctrine.Value);
            var ownedCommons = run.PlayerDeckCardIds
                .Select(id => CardCatalog.Get(id))
                .Where(c => c != null && c.Rarity == CardRarity.Common)
                .ToList();
            if (ownedCommons.Count == 0) return;
            var uncommons = pool
                .Where(c => c.Rarity == CardRarity.Uncommon && !run.PlayerDeckCardIds.Contains(c.Id))
                .ToList();
            if (uncommons.Count == 0) return;
            var oldCard = ownedCommons[rng.Next(ownedCommons.Count)];
            var newCard = uncommons[rng.Next(uncommons.Count)];
            run.PlayerDeckCardIds.Remove(oldCard.Id);
            run.PlayerDeckCardIds.Add(newCard.Id);
        }

        static void TryAwardRelic(RunState run, Random rng)
        {
            if (run.PlayerRelics.Count >= 5) return;
            var all = (RelicType[])Enum.GetValues(typeof(RelicType));
            var available = all.Where(r => !run.PlayerRelics.Contains(r)).ToArray();
            if (available.Length == 0) return;
            run.PlayerRelics.Add(available[rng.Next(available.Length)]);
        }

        static int CalculateFlorins(int act, MapNodeType nodeType)
        {
            int baseAmount = 8 + act * 2;
            if (nodeType == MapNodeType.EliteMatch) baseAmount += 4;
            if (nodeType == MapNodeType.BossMatch) baseAmount += 8;
            return baseAmount;
        }

        static void SimulateShop(RunState run, Random rng, ActLog actLog)
        {
            if (!run.PlayerDoctrine.HasValue) return;
            switch (run.PlayerDoctrine.Value)
            {
                case DoctrineType.Schemer:
                    // Schemer prioritizes removal at half price (4 florins)
                    if (run.Florins >= 4 && run.PlayerDeckCardIds.Count > 9)
                    {
                        TryRemoveWeakestCard(run, rng);
                        run.Florins -= 4;
                        actLog.ShopPurchases++;
                    }
                    else if (run.Florins >= 6)
                    {
                        actLog.CardsAdded += AwardDoctrineCards(run, false, rng);
                        run.Florins -= 6;
                        actLog.ShopPurchases++;
                    }
                    break;
                case DoctrineType.Hoarder:
                    if (run.Florins >= 6)
                    {
                        actLog.CardsAdded += AwardDoctrineCards(run, false, rng);
                        run.Florins -= 6;
                        actLog.ShopPurchases++;
                    }
                    if (run.Florins >= 8 && run.PlayerDeckCardIds.Count > 14)
                    {
                        TryRemoveWeakestCard(run, rng);
                        run.Florins -= 8;
                        actLog.ShopPurchases++;
                    }
                    break;
                case DoctrineType.Brute:
                    if (run.Florins >= 6)
                    {
                        actLog.CardsAdded += AwardDoctrineCards(run, false, rng);
                        run.Florins -= 6;
                        actLog.ShopPurchases++;
                    }
                    if (run.Florins >= 5 && run.PlayerDeckCardIds.Count > 8)
                    {
                        TryRemoveWeakestCard(run, rng);
                        run.Florins -= 5;
                        actLog.ShopPurchases++;
                    }
                    break;
                case DoctrineType.Trickster:
                    // Trickster buys a card, then can pay 10 to transform a common
                    if (run.Florins >= 6)
                    {
                        actLog.CardsAdded += AwardDoctrineCards(run, false, rng);
                        run.Florins -= 6;
                        actLog.ShopPurchases++;
                    }
                    if (run.Florins >= 10)
                    {
                        TryTransformCard(run, rng);
                        run.Florins -= 10;
                        actLog.ShopPurchases++;
                    }
                    break;
                default:
                    if (run.Florins >= 6)
                    {
                        actLog.CardsAdded += AwardDoctrineCards(run, false, rng);
                        run.Florins -= 6;
                        actLog.ShopPurchases++;
                    }
                    break;
            }
        }

        static void SimulateRumor(RunState run, Random rng, ActLog actLog)
        {
            int roll = rng.Next(100);
            switch (run.PlayerDoctrine ?? DoctrineType.Neutral)
            {
                case DoctrineType.Schemer:
                    if (roll < 40) run.Florins += 6;
                    else if (roll < 70) { run.Florins += 4; if (run.PlayerBurdens.Count > 0) run.PlayerBurdens.RemoveAt(0); }
                    else if (roll < 85) { run.Florins += 10; run.PlayerBurdens.Add(BurdenType.MarkedCards); }
                    else run.Florins += 4;
                    break;
                case DoctrineType.Hoarder:
                    int bonus = Math.Min(run.PlayerBurdens.Count * 3, 9);
                    if (roll < 35) run.Florins += 7 + bonus;
                    else if (roll < 65) run.Florins += 4 + bonus;
                    else if (roll < 80) { run.Florins += 12; run.PlayerBurdens.Add(BurdenType.HeavyPurse); }
                    else run.Florins += 3;
                    break;
                default:
                    if (roll < 30) run.Florins += 5;
                    else if (roll < 60) run.Florins += 3;
                    else if (roll < 80) { run.Florins += 8; run.PlayerBurdens.Add(BurdenType.MarkedCards); }
                    else run.Florins += 3;
                    break;
            }
        }

        static void SimulateRest(RunState run, Random rng, ActLog actLog)
        {
            switch (run.PlayerDoctrine ?? DoctrineType.Neutral)
            {
                case DoctrineType.Brute:
                    if (run.PlayerBurdens.Count > 0 && rng.Next(100) < 60)
                        run.PlayerBurdens.RemoveAt(rng.Next(run.PlayerBurdens.Count));
                    if (run.Prestige < 7 && rng.Next(100) < 40)
                        run.Prestige++;
                    break;
                case DoctrineType.Trickster:
                    if (run.PlayerBurdens.Count > 0 && rng.Next(100) < 70)
                        run.PlayerBurdens.RemoveAt(rng.Next(run.PlayerBurdens.Count));
                    break;
                default:
                    if (run.PlayerBurdens.Count > 0 && rng.Next(100) < 60)
                        run.PlayerBurdens.RemoveAt(rng.Next(run.PlayerBurdens.Count));
                    break;
            }
        }

        // ---------- Reporting ----------

        static void AppendDoctrineReport(StringBuilder sb, DoctrineType doctrine, List<RunLog> runs)
        {
            sb.AppendLine($"┌────────────────────────────────────────────┐");
            sb.AppendLine($"│  {doctrine.ToString().ToUpper(),-40}  │");
            sb.AppendLine($"└────────────────────────────────────────────┘");

            int won = runs.Count(r => r.Won);
            sb.AppendLine($"  Win Rate: {won}/{runs.Count} ({100.0 * won / runs.Count:0.0}%)");
            sb.AppendLine();

            sb.AppendLine("  Act │ WinRate │ AvgBouts │ AvgAbility │ DeckSize │ Florins │ Matches │ CardsAdd");
            sb.AppendLine("  ────┼─────────┼──────────┼────────────┼──────────┼─────────┼─────────┼─────────");

            for (int act = 0; act < 5; act++)
            {
                var actLogs = runs.SelectMany(r => r.Acts).Where(a => a.ActIndex == act).ToList();
                if (actLogs.Count == 0) continue;

                var matches = actLogs.SelectMany(a => a.Matches).ToList();
                int totalMatches = matches.Count;
                int matchesWon = matches.Count(m => m.Won);
                double winRate = totalMatches > 0 ? 100.0 * matchesWon / totalMatches : 0;
                double avgBouts = matches.Count > 0 ? matches.Average(m => m.BoutCount) : 0;
                double avgAbilities = matches.Count > 0 ? matches.Average(m => m.AbilitiesUsed) : 0;
                double avgDeckSize = actLogs.Average(a => a.DeckSizeStart);
                double avgFlorins = actLogs.Average(a => a.FlorinsStart);
                double avgCardsAdded = actLogs.Average(a => a.CardsAdded);

                sb.AppendLine($"  {act + 1,3} │ {winRate,6:0.0}% │ {avgBouts,8:0.1} │ {avgAbilities,10:0.1} │ {avgDeckSize,8:0.1} │ {avgFlorins,7:0.0} │ {totalMatches,7} │ {avgCardsAdded,7:0.1}");
            }

            sb.AppendLine();

            // Match length by act (turns)
            sb.AppendLine("  Act │ AvgTurns │ MinTurns │ MaxTurns │ Stalls");
            sb.AppendLine("  ────┼──────────┼──────────┼──────────┼───────");
            for (int act = 0; act < 5; act++)
            {
                var matches = runs.SelectMany(r => r.Acts).Where(a => a.ActIndex == act).SelectMany(a => a.Matches).ToList();
                if (matches.Count == 0) continue;
                double avg = matches.Average(m => m.TurnsPlayed);
                int min = matches.Min(m => m.TurnsPlayed);
                int max = matches.Max(m => m.TurnsPlayed);
                int stalls = matches.Count(m => m.Stalled);
                sb.AppendLine($"  {act + 1,3} │ {avg,8:0.0} │ {min,8} │ {max,8} │ {stalls,5}");
            }

            sb.AppendLine();

            // Node type distribution per act
            sb.AppendLine("  Act │ Rival │ Elite │ Boss │ Shop │ Rest │ Rumor");
            sb.AppendLine("  ────┼───────┼───────┼──────┼──────┼──────┼──────");
            for (int act = 0; act < 5; act++)
            {
                var matches = runs.SelectMany(r => r.Acts).Where(a => a.ActIndex == act).SelectMany(a => a.Matches).ToList();
                if (matches.Count == 0) continue;
                int rival = matches.Count(m => m.NodeType == MapNodeType.RivalMatch);
                int elite = matches.Count(m => m.NodeType == MapNodeType.EliteMatch);
                int boss = matches.Count(m => m.NodeType == MapNodeType.BossMatch);
                var actLogs = runs.SelectMany(r => r.Acts).Where(a => a.ActIndex == act).ToList();
                int shopVisits = actLogs.Sum(a => a.ShopPurchases > 0 ? 1 : 0);
                sb.AppendLine($"  {act + 1,3} │ {rival,5} │ {elite,5} │ {boss,4} │ {shopVisits,4} │   -  │   -  ");
            }

            sb.AppendLine();

            // Cards played per match by act
            sb.AppendLine("  Act │ AvgCardsPlayed │ WinCardsPlayed │ LossCardsPlayed");
            sb.AppendLine("  ────┼────────────────┼────────────────┼────────────────");
            for (int act = 0; act < 5; act++)
            {
                var matches = runs.SelectMany(r => r.Acts).Where(a => a.ActIndex == act).SelectMany(a => a.Matches).ToList();
                if (matches.Count == 0) continue;
                double avgAll = matches.Average(m => m.CardsPlayed);
                var wins = matches.Where(m => m.Won).ToList();
                var losses = matches.Where(m => !m.Won).ToList();
                double avgWin = wins.Count > 0 ? wins.Average(m => m.CardsPlayed) : 0;
                double avgLoss = losses.Count > 0 ? losses.Average(m => m.CardsPlayed) : 0;
                sb.AppendLine($"  {act + 1,3} │ {avgAll,14:0.1} │ {avgWin,14:0.1} │ {avgLoss,14:0.1}");
            }

            sb.AppendLine();

            // Where runs end
            sb.AppendLine("  Death distribution:");
            var deadRuns = runs.Where(r => !r.Won).ToList();
            for (int act = 0; act < 5; act++)
            {
                int died = deadRuns.Count(r => r.FarthestAct == act);
                if (died > 0)
                    sb.AppendLine($"    Died in Act {act + 1}: {died}");
            }
            sb.AppendLine();
        }

        static void AppendCrossDoctrineComparison(StringBuilder sb, Dictionary<DoctrineType, List<RunLog>> all)
        {
            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║  CROSS-DOCTRINE COMPARISON: Act 1 vs Act 5 Gameplay Delta   ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            sb.AppendLine("  Doctrine    │ Act1 WR │ Act5 WR │ Δ WR   │ Act1 Bouts │ Act5 Bouts │ Δ Bouts");
            sb.AppendLine("  ────────────┼─────────┼─────────┼────────┼────────────┼────────────┼────────");

            foreach (var kv in all)
            {
                var act1m = kv.Value.SelectMany(r => r.Acts).Where(a => a.ActIndex == 0).SelectMany(a => a.Matches).ToList();
                var act5m = kv.Value.SelectMany(r => r.Acts).Where(a => a.ActIndex == 4).SelectMany(a => a.Matches).ToList();

                double wr1 = act1m.Count > 0 ? 100.0 * act1m.Count(m => m.Won) / act1m.Count : 0;
                double wr5 = act5m.Count > 0 ? 100.0 * act5m.Count(m => m.Won) / act5m.Count : 0;
                double b1 = act1m.Count > 0 ? act1m.Average(m => m.BoutCount) : 0;
                double b5 = act5m.Count > 0 ? act5m.Average(m => m.BoutCount) : 0;

                sb.AppendLine($"  {kv.Key,-12}│ {wr1,6:0.0}% │ {wr5,6:0.0}% │ {wr5 - wr1,5:+0.0;-0.0}% │ {b1,10:0.1} │ {b5,10:0.1} │ {b5 - b1,6:+0.1;-0.1}");
            }

            sb.AppendLine();

            sb.AppendLine("  Doctrine    │ Act1 Deck │ Act5 Deck │ Δ Deck │ Act1 Abil │ Act5 Abil │ Δ Abil");
            sb.AppendLine("  ────────────┼───────────┼───────────┼────────┼───────────┼───────────┼────────");

            foreach (var kv in all)
            {
                var act1 = kv.Value.SelectMany(r => r.Acts).Where(a => a.ActIndex == 0).ToList();
                var act5 = kv.Value.SelectMany(r => r.Acts).Where(a => a.ActIndex == 4).ToList();

                double d1 = act1.Count > 0 ? act1.Average(a => a.DeckSizeStart) : 0;
                double d5 = act5.Count > 0 ? act5.Average(a => a.DeckSizeStart) : 0;

                var act1m = act1.SelectMany(a => a.Matches).ToList();
                var act5m = act5.SelectMany(a => a.Matches).ToList();
                double a1 = act1m.Count > 0 ? act1m.Average(m => m.AbilitiesUsed) : 0;
                double a5 = act5m.Count > 0 ? act5m.Average(m => m.AbilitiesUsed) : 0;

                sb.AppendLine($"  {kv.Key,-12}│ {d1,9:0.1} │ {d5,9:0.1} │ {d5 - d1,5:+0.1;-0.1} │ {a1,9:0.1} │ {a5,9:0.1} │ {a5 - a1,6:+0.1;-0.1}");
            }

            sb.AppendLine();

            sb.AppendLine("  Doctrine    │ Act1 Cards │ Act5 Cards │ Δ Cards │ Act1 Turns │ Act5 Turns │ Δ Turns");
            sb.AppendLine("  ────────────┼────────────┼────────────┼─────────┼────────────┼────────────┼────────");

            foreach (var kv in all)
            {
                var act1m = kv.Value.SelectMany(r => r.Acts).Where(a => a.ActIndex == 0).SelectMany(a => a.Matches).ToList();
                var act5m = kv.Value.SelectMany(r => r.Acts).Where(a => a.ActIndex == 4).SelectMany(a => a.Matches).ToList();

                double c1 = act1m.Count > 0 ? act1m.Average(m => m.CardsPlayed) : 0;
                double c5 = act5m.Count > 0 ? act5m.Average(m => m.CardsPlayed) : 0;
                double t1 = act1m.Count > 0 ? act1m.Average(m => m.TurnsPlayed) : 0;
                double t5 = act5m.Count > 0 ? act5m.Average(m => m.TurnsPlayed) : 0;

                sb.AppendLine($"  {kv.Key,-12}│ {c1,10:0.1} │ {c5,10:0.1} │ {c5 - c1,6:+0.1;-0.1} │ {t1,10:0.0} │ {t5,10:0.0} │ {t5 - t1,6:+0.0;-0.0}");
            }

            sb.AppendLine();
        }

        // ---------- Data classes ----------

        class RunLog
        {
            public DoctrineType Doctrine;
            public int Seed;
            public bool Won;
            public int FarthestAct;
            public List<ActLog> Acts = new();
        }

        class ActLog
        {
            public int ActIndex;
            public int DeckSizeStart;
            public int DeckSizeEnd;
            public int AbilityCountStart;
            public int RelicCount;
            public int TrinketCount;
            public int BurdenCount;
            public int FlorinsStart;
            public int FlorinsEnd;
            public int NodesVisited;
            public int CardsAdded;
            public int ShopPurchases;
            public List<MatchLog> Matches = new();
        }

        class MatchLog
        {
            public MapNodeType NodeType;
            public string OpponentName;
            public int ActIndex;
            public int PlayerDeckSize;
            public bool Won;
            public int BoutCount;
            public int AbilitiesUsed;
            public int CardsPlayed;
            public int TurnsPlayed;
            public bool Stalled;
        }
    }
}
