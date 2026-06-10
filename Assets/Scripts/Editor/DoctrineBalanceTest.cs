using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class DoctrineBalanceTest
    {
        [MenuItem("Wits and Fools/Balance/Doctrine Matchup Matrix (100 games)")]
        public static void RunSmall() => RunMatrix(100);

        [MenuItem("Wits and Fools/Balance/Doctrine Matchup Matrix (1000 games)")]
        public static void RunFull() => RunMatrix(1000);

        static readonly DoctrineType[] Doctrines =
        {
            DoctrineType.Schemer, DoctrineType.Brute,
            DoctrineType.Trickster, DoctrineType.Hoarder
        };

        static void RunMatrix(int gamesPerMatchup)
        {
            if (!CardCatalog.IsInitialized)
            {
                string path = Path.Combine(Application.dataPath, "Data", "card_catalog.json");
                if (!File.Exists(path))
                {
                    Debug.LogError($"Card catalog not found at: {path}");
                    return;
                }
                CardCatalogLoader.LoadFromJson(File.ReadAllText(path));
                Debug.Log($"Loaded {CardCatalog.Count} cards from catalog.");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== Doctrine Balance Matrix: {gamesPerMatchup} games per matchup ===");
            sb.AppendLine();

            var winRates = new float[4, 4];
            var avgTurns = new float[4, 4];
            var stalls = new int[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var result = RunMatchup(Doctrines[i], Doctrines[j], gamesPerMatchup);
                    winRates[i, j] = result.p0WinRate;
                    avgTurns[i, j] = result.avgTurns;
                    stalls[i, j] = result.stalls;
                }
            }

            sb.AppendLine("Win rates (row = P0, column = P1):");
            sb.Append("             ");
            for (int j = 0; j < 4; j++)
                sb.Append($"{Doctrines[j].DisplayName(),-12}");
            sb.AppendLine();

            for (int i = 0; i < 4; i++)
            {
                sb.Append($"{Doctrines[i].DisplayName(),-13}");
                for (int j = 0; j < 4; j++)
                    sb.Append($"{winRates[i, j]:P0}        ");
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("Average turns per game:");
            sb.Append("             ");
            for (int j = 0; j < 4; j++)
                sb.Append($"{Doctrines[j].DisplayName(),-12}");
            sb.AppendLine();

            for (int i = 0; i < 4; i++)
            {
                sb.Append($"{Doctrines[i].DisplayName(),-13}");
                for (int j = 0; j < 4; j++)
                    sb.Append($"{avgTurns[i, j]:F1}        ");
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("Per-doctrine overall win rate:");
            for (int i = 0; i < 4; i++)
            {
                float totalWins = 0, totalGames = 0;
                for (int j = 0; j < 4; j++)
                {
                    totalWins += winRates[i, j] * gamesPerMatchup;
                    totalGames += gamesPerMatchup;
                }
                float overall = totalGames > 0 ? totalWins / totalGames : 0;
                string status = overall >= 0.45f && overall <= 0.55f ? "BALANCED" :
                    overall < 0.45f ? "WEAK" : "STRONG";
                sb.AppendLine($"  {Doctrines[i].DisplayName()}: {overall:P1} [{status}]");
            }

            int totalStalls = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    totalStalls += stalls[i, j];

            sb.AppendLine();
            sb.AppendLine($"Total stalls: {totalStalls} / {gamesPerMatchup * 16}");

            Debug.Log(sb.ToString());
        }

        struct MatchupResult
        {
            public float p0WinRate;
            public float avgTurns;
            public int stalls;
        }

        static MatchupResult RunMatchup(DoctrineType d0, DoctrineType d1, int games)
        {
            int p0wins = 0;
            int totalTurns = 0;
            int stallCount = 0;

            var startingDeck0 = CardCatalog.StartingDeck(d0);
            var startingDeck1 = CardCatalog.StartingDeck(d1);

            for (int g = 0; g < games; g++)
            {
                int seed = g * 17 + (int)d0 * 1000 + (int)d1 * 100;

                var deck0 = new PlayerDeck(startingDeck0);
                var deck1 = new PlayerDeck(startingDeck1);

                var config = new MatchConfig();
                config.ArchetypeResource[0] = d0.Resource();
                config.ArchetypeResource[1] = d1.Resource();
                config.AbilitiesCostResource = true; // match production (MatchSetup)

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
                    if (active == 0)
                        ai0.RequestAction(engine, 0);
                    else
                        ai1.RequestAction(engine, 1);
                }

                if (safety >= 5000) stallCount++;
                totalTurns += turns;
                if (engine.WinnerIndex == 0) p0wins++;
            }

            return new MatchupResult
            {
                p0WinRate = games > 0 ? (float)p0wins / games : 0,
                avgTurns = games > 0 ? (float)totalTurns / games : 0,
                stalls = stallCount,
            };
        }
    }
}
