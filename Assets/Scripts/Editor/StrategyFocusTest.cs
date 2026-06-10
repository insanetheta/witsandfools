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
    // Plays focused-strategy decks (starting deck + themed draft picks) against the
    // standard starting decks of all four doctrines. Verifies each doctrine has more
    // than one viable line of play and that no ability package is an auto-win.
    public static class StrategyFocusTest
    {
        const int GamesPerOpponent = 250;

        class Strategy
        {
            public string Name;
            public DoctrineType Doctrine;
            public AbilityType[] Preferred;
        }

        static readonly Strategy[] Strategies =
        {
            new Strategy { Name = "Disruption", Doctrine = DoctrineType.Schemer,
                Preferred = new[] { AbilityType.Riposte, AbilityType.Blackmail, AbilityType.DoubleAgent, AbilityType.Masterstroke } },
            new Strategy { Name = "Tempo", Doctrine = DoctrineType.Schemer,
                Preferred = new[] { AbilityType.SeizeInitiative, AbilityType.SleightOfHand, AbilityType.StackTheDeck, AbilityType.Peek } },
            new Strategy { Name = "Overwhelm", Doctrine = DoctrineType.Brute,
                Preferred = new[] { AbilityType.Conquer, AbilityType.HeavyHand, AbilityType.Onslaught } },
            new Strategy { Name = "Pressure", Doctrine = DoctrineType.Brute,
                Preferred = new[] { AbilityType.DoubleTrouble, AbilityType.Rampage, AbilityType.Intimidate } },
            new Strategy { Name = "Control", Doctrine = DoctrineType.Trickster,
                Preferred = new[] { AbilityType.Blocker, AbilityType.TrumpChanger, AbilityType.Fortify, AbilityType.SlipAway } },
            new Strategy { Name = "Chaos", Doctrine = DoctrineType.Trickster,
                Preferred = new[] { AbilityType.BlindSwap, AbilityType.DoubleAgent, AbilityType.Masquerade, AbilityType.SeizeInitiative } },
            new Strategy { Name = "Race", Doctrine = DoctrineType.Hoarder,
                Preferred = new[] { AbilityType.ExtraDraw, AbilityType.Brace, AbilityType.AllIn, AbilityType.Patronage } },
            new Strategy { Name = "BigHand", Doctrine = DoctrineType.Hoarder,
                Preferred = new[] { AbilityType.HeavyHand, AbilityType.Monopoly, AbilityType.SteadyHand, AbilityType.ThickSkin } },
        };

        static readonly DoctrineType[] Field =
        {
            DoctrineType.Schemer, DoctrineType.Brute,
            DoctrineType.Trickster, DoctrineType.Hoarder
        };

        [MenuItem("Wits and Fools/Playtest/Strategy Focus Matrix (250 per opponent)")]
        public static void Run()
        {
            CardCatalog.Clear();
            CardCatalogLoader.LoadFromJson(File.ReadAllText("Assets/Data/card_catalog.json"));

            var sb = new StringBuilder();
            sb.AppendLine("=== Strategy Focus Matrix ===");
            sb.AppendLine($"Each strategy: doctrine starting deck + up to 4 themed picks, vs all 4 standard decks, {GamesPerOpponent} games each.");
            sb.AppendLine();

            foreach (var strat in Strategies)
            {
                var deckTemplates = BuildStrategyDeck(strat, out var picks);
                sb.Append($"{strat.Doctrine}/{strat.Name,-10} picks: [{string.Join(", ", picks)}]  ");

                float totalWins = 0; int totalGames = 0; long totalTurns = 0; int stalls = 0;
                var perOpp = new List<string>();
                foreach (var opp in Field)
                {
                    int wins = 0; int turnsSum = 0;
                    var oppTemplates = CardCatalog.StartingDeck(opp);
                    for (int g = 0; g < GamesPerOpponent; g++)
                    {
                        int seed = g * 31 + (int)strat.Doctrine * 7919 + (int)opp * 104729 + strat.Name.Length * 17;
                        var deck0 = new PlayerDeck(deckTemplates);
                        var deck1 = new PlayerDeck(oppTemplates);
                        var config = new MatchConfig();
                        config.ArchetypeResource[0] = strat.Doctrine.Resource();
                        config.ArchetypeResource[1] = opp.Resource();
                        config.AbilitiesCostResource = true;

                        var engine = new GameEngine(seed, config, deck0, deck1);
                        var ai0 = new AIPlayer("P0", seed);
                        var ai1 = new AIPlayer("P1", seed + 1);
                        int turns = 0;
                        engine.OnTurnBegan += _ => turns++;
                        engine.StartNewGame();
                        int safety = 0;
                        while (engine.Phase != Phase.GameOver && safety++ < 5000)
                        {
                            if (engine.AwaitingStackPutBack(0)) { ai0.RequestAction(engine, 0); continue; }
                            if (engine.AwaitingStackPutBack(1)) { ai1.RequestAction(engine, 1); continue; }
                            int active = engine.Phase == Phase.Defense ? engine.DefenderIndex : engine.AttackerIndex;
                            if (active == 0) ai0.RequestAction(engine, 0); else ai1.RequestAction(engine, 1);
                        }
                        if (safety >= 5000) { stalls++; continue; }
                        if (engine.WinnerIndex == 0) wins++;
                        turnsSum += turns;
                    }
                    totalWins += wins; totalGames += GamesPerOpponent; totalTurns += turnsSum;
                    perOpp.Add($"vs {opp}: {wins * 100 / GamesPerOpponent}%");
                }

                float overall = totalWins * 100f / totalGames;
                sb.AppendLine();
                sb.AppendLine($"  overall {overall:F1}%  avg turns {(float)totalTurns / totalGames:F1}  stalls {stalls}  |  {string.Join("  ", perOpp)}");
                sb.AppendLine();
            }

            Debug.Log(sb.ToString());
        }

        // Starting deck plus up to 4 themed picks from the doctrine pool (non-starting cards
        // whose abilityType is in the strategy's preferred set; padded with neutrals if thin).
        static List<CardDefinition> BuildStrategyDeck(Strategy strat, out List<string> pickNames)
        {
            var deck = new List<CardDefinition>(CardCatalog.StartingDeck(strat.Doctrine));
            var inDeck = new HashSet<string>(deck.Select(c => c.Id));
            var pool = CardCatalog.All()
                .Where(c => !inDeck.Contains(c.Id))
                .Where(c => c.Doctrine == strat.Doctrine || c.IsNeutral)
                .Where(c => c.Ability.HasValue && strat.Preferred.Contains(c.Ability.Value))
                .OrderByDescending(c => (int)c.Rarity)
                .ThenBy(c => c.Id, StringComparer.Ordinal)
                .Take(4)
                .ToList();
            deck.AddRange(pool);
            pickNames = pool.Select(c => c.Name).ToList();
            return deck;
        }
    }
}
