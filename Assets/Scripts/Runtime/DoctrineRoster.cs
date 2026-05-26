using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WitsAndFools
{
    public static class DoctrineRoster
    {
        static readonly List<OpponentProfile> _all = new();
        static bool _initialized;

        public static void RegisterAll(IEnumerable<OpponentProfile> enemies)
        {
            _all.Clear();
            _all.AddRange(enemies);
            _initialized = true;
        }

        public static OpponentProfile Pick(int actIndex, bool isElite, bool isBoss, Random rng)
        {
            if (!_initialized || _all.Count == 0)
                return OpponentRoster.Pick(actIndex, isElite, isBoss, rng);

            int act = actIndex + 1;
            var pool = _all.Where(e => e.ActIndex == act).ToList();

            if (isBoss)
                pool = pool.Where(e => e.IsBoss).ToList();
            else if (isElite)
                pool = pool.Where(e => e.IsElite).ToList();
            else
                pool = pool.Where(e => !e.IsElite && !e.IsBoss).ToList();

            if (pool.Count == 0)
                return OpponentRoster.Pick(actIndex, isElite, isBoss, rng);

            return Clone(pool[rng.Next(pool.Count)]);
        }

        public static OpponentProfile[] AllForAct(int actIndex)
        {
            if (!_initialized) return OpponentRoster.AllForAct(actIndex);
            int act = actIndex + 1;
            return _all.Where(e => e.ActIndex == act).Select(Clone).ToArray();
        }

        public static int Count => _all.Count;
        public static bool IsInitialized => _initialized;

        public static void Clear()
        {
            _all.Clear();
            _initialized = false;
        }

        static DoctrineType ParseDoctrine(string s) => s?.ToLowerInvariant() switch
        {
            "schemer" => DoctrineType.Schemer,
            "brute" => DoctrineType.Brute,
            "trickster" => DoctrineType.Trickster,
            "hoarder" => DoctrineType.Hoarder,
            _ => DoctrineType.Neutral,
        };

        static AIArchetypeName ParseAI(string s) => s?.ToLowerInvariant() switch
        {
            "brawler" => AIArchetypeName.Brawler,
            "miser" => AIArchetypeName.Miser,
            "fox" => AIArchetypeName.Fox,
            "noble" => AIArchetypeName.Noble,
            "scholar" => AIArchetypeName.Scholar,
            "assassin" => AIArchetypeName.Assassin,
            _ => AIArchetypeName.Brawler,
        };

        public static List<OpponentProfile> ParseJson(string json)
        {
            var result = new List<OpponentProfile>();
            var root = JObject.Parse(json);
            var enemies = root["enemies"] as JArray;
            if (enemies == null) return result;

            foreach (var token in enemies)
            {
                var e = token as JObject;
                if (e == null) continue;

                var profile = new OpponentProfile
                {
                    Name = (string)e["name"] ?? "Unknown",
                    Archetype = ParseAI((string)e["aiPersonality"]),
                    ActIndex = (int)(e["act"] ?? 1),
                    Doctrine = ParseDoctrine((string)e["doctrine"]),
                };

                var tier = ((string)e["tier"] ?? "regular").ToLowerInvariant();
                profile.IsElite = tier == "elite";
                profile.IsBoss = tier == "boss";

                var comp = e["deckComposition"] as JObject;
                if (comp?["cardIds"] is JArray ids)
                {
                    foreach (var id in ids)
                        profile.DeckCardIds.Add((string)id);
                }

                var rule = (string)e["houseRules"];
                if (!string.IsNullOrEmpty(rule) && rule != "none")
                {
                    if (Enum.TryParse<HouseRuleType>(rule, true, out var parsed))
                        profile.HouseRule = parsed;
                }

                result.Add(profile);
            }

            return result;
        }

        static OpponentProfile Clone(OpponentProfile src) => new OpponentProfile
        {
            Name = src.Name,
            Archetype = src.Archetype,
            Abilities = new List<AbilityType>(src.Abilities),
            Trinkets = new List<TrinketType>(src.Trinkets),
            HouseRule = src.HouseRule,
            ActIndex = src.ActIndex,
            IsElite = src.IsElite,
            IsBoss = src.IsBoss,
            Doctrine = src.Doctrine,
            DeckCardIds = new List<string>(src.DeckCardIds),
            Relics = new List<RelicType>(src.Relics),
        };
    }
}
