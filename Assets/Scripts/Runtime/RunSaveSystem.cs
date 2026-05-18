using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace WitsAndFools
{
    public sealed class RunSaveData
    {
        public RunState Run;
        public int CurrentColumn;
        public ArchetypeType? SelectedArchetype;
        public string RunPhase;
    }

    public static class RunSaveSystem
    {
        static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None
        };

        static string SavePath => Path.Combine(Application.persistentDataPath, "run_save.json");

        public static void Save(RunState run, int currentColumn, ArchetypeType? archetype, RunPhase phase)
        {
            var data = new RunSaveData
            {
                Run = run,
                CurrentColumn = currentColumn,
                SelectedArchetype = archetype,
                RunPhase = phase.ToString()
            };
            string json = JsonConvert.SerializeObject(data, Settings);
            File.WriteAllText(SavePath, json);
        }

        public static RunSaveData Load()
        {
            if (!File.Exists(SavePath)) return null;
            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonConvert.DeserializeObject<RunSaveData>(json, Settings);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RunSaveSystem] Failed to load save: {e.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }

        public static bool HasSave => File.Exists(SavePath);
    }
}
