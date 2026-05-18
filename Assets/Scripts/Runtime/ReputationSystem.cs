using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace WitsAndFools
{
    public sealed class ReputationData
    {
        public int TotalReputation;
        public int RunsCompleted;
        public int RunsWon;
        public int TotalMatchesWon;
        public List<ArchetypeType> UnlockedArchetypes = new() { ArchetypeType.Rogue };
        public Dictionary<ArchetypeType, int> ArchetypeWins = new();
    }

    public static class ReputationSystem
    {
        static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include
        };

        static string SavePath => Path.Combine(Application.persistentDataPath, "reputation.json");

        static ReputationData _cached;

        public static ReputationData Load()
        {
            if (_cached != null) return _cached;
            if (!File.Exists(SavePath))
            {
                _cached = new ReputationData();
                return _cached;
            }
            try
            {
                string json = File.ReadAllText(SavePath);
                _cached = JsonConvert.DeserializeObject<ReputationData>(json, Settings) ?? new ReputationData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ReputationSystem] Failed to load: {e.Message}");
                _cached = new ReputationData();
            }
            if (_cached.UnlockedArchetypes == null || _cached.UnlockedArchetypes.Count == 0)
                _cached.UnlockedArchetypes = new List<ArchetypeType> { ArchetypeType.Rogue };
            return _cached;
        }

        public static void Save()
        {
            if (_cached == null) return;
            string json = JsonConvert.SerializeObject(_cached, Settings);
            File.WriteAllText(SavePath, json);
        }

        public static int RecordRunEnd(RunState run, ArchetypeType? archetype)
        {
            var data = Load();
            data.RunsCompleted++;
            data.TotalMatchesWon += run.MatchesWon;
            if (run.RunWon) data.RunsWon++;

            int earned = CalculateReputation(run);
            data.TotalReputation += earned;

            if (archetype.HasValue)
            {
                if (!data.ArchetypeWins.ContainsKey(archetype.Value))
                    data.ArchetypeWins[archetype.Value] = 0;
                if (run.RunWon)
                    data.ArchetypeWins[archetype.Value]++;
            }

            CheckUnlocks(data);
            Save();
            return earned;
        }

        static int CalculateReputation(RunState run)
        {
            int rep = run.MatchesWon * 3;
            rep += run.CurrentAct * 5;
            if (run.RunWon) rep += 25;
            return rep;
        }

        static void CheckUnlocks(ReputationData data)
        {
            if (data.TotalReputation >= 25 && !data.UnlockedArchetypes.Contains(ArchetypeType.Brute))
            {
                data.UnlockedArchetypes.Add(ArchetypeType.Brute);
                Debug.Log("[ReputationSystem] Unlocked: The Brute (25 Rep)");
            }
            if (data.TotalReputation >= 100 && !data.UnlockedArchetypes.Contains(ArchetypeType.Diplomat))
            {
                data.UnlockedArchetypes.Add(ArchetypeType.Diplomat);
                Debug.Log("[ReputationSystem] Unlocked: The Diplomat (100 Rep)");
            }
            if (data.TotalReputation >= 300 && !data.UnlockedArchetypes.Contains(ArchetypeType.Gambler))
            {
                data.UnlockedArchetypes.Add(ArchetypeType.Gambler);
                Debug.Log("[ReputationSystem] Unlocked: The Gambler (300 Rep)");
            }
        }

        public static void ResetAll()
        {
            _cached = new ReputationData();
            Save();
        }
    }
}
