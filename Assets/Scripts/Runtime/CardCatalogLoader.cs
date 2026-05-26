using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace WitsAndFools
{
    public static class CardCatalogLoader
    {
        public static void LoadFromJson(string json)
        {
            var root = JObject.Parse(json);
            var cards = root["cards"] as JArray;
            if (cards == null) return;

            var defs = new List<CardDefinition>();
            foreach (var token in cards)
            {
                var c = token as JObject;
                if (c == null) continue;

                var def = new CardDefinition
                {
                    Id = (string)c["id"] ?? "unknown",
                    Name = (string)c["name"] ?? "Unknown",
                    Suit = ParseSuit((string)c["suit"]),
                    Rank = ParseRank((int)(c["rank"] ?? 6)),
                    Doctrine = ParseDoctrine((string)c["doctrine"]),
                    Trigger = ParseTrigger((string)c["trigger"]),
                    AbilityText = (string)c["ability"] ?? "",
                    FlavorText = (string)c["flavorText"] ?? "",
                    Rarity = ParseRarity((string)c["rarity"]),
                    InStartingDeck = (bool)(c["startingDeck"] ?? false),
                };

                def.Ability = MapAbility(def.Id, def.Trigger, def.AbilityText, def.Doctrine);
                defs.Add(def);
            }

            CardCatalog.RegisterAll(defs);
        }

        static Suit ParseSuit(string s) => s?.ToLowerInvariant() switch
        {
            "hearts" => Suit.Hearts,
            "diamonds" => Suit.Diamonds,
            "clubs" => Suit.Clubs,
            "spades" => Suit.Spades,
            _ => Suit.Spades,
        };

        static Rank ParseRank(int r) => (Rank)Math.Clamp(r, 6, 14);

        static DoctrineType ParseDoctrine(string s) => s?.ToLowerInvariant() switch
        {
            "schemer" => DoctrineType.Schemer,
            "brute" => DoctrineType.Brute,
            "trickster" => DoctrineType.Trickster,
            "hoarder" => DoctrineType.Hoarder,
            _ => DoctrineType.Neutral,
        };

        static TriggerTiming ParseTrigger(string s) => s?.ToLowerInvariant() switch
        {
            "onattack" => TriggerTiming.OnAttack,
            "ondefend" => TriggerTiming.OnDefend,
            "passive" => TriggerTiming.Passive,
            _ => TriggerTiming.None,
        };

        static CardRarity ParseRarity(string s) => s?.ToLowerInvariant() switch
        {
            "common" => CardRarity.Common,
            "uncommon" => CardRarity.Uncommon,
            "rare" => CardRarity.Rare,
            _ => CardRarity.Common,
        };

        static AbilityType? MapAbility(string id, TriggerTiming trigger, string text, DoctrineType doctrine)
        {
            if (string.IsNullOrEmpty(text) || text.Equals("None", StringComparison.OrdinalIgnoreCase))
                return null;

            var lower = text.ToLowerInvariant();

            if (lower.Contains("draw 2")) return AbilityType.ExtraDraw;
            if (lower.Contains("draw 3")) return AbilityType.Haymaker;
            if (lower.Contains("draw 1")) return AbilityType.ExtraDraw;
            if (lower.Contains("peek") || lower.Contains("look at the top")) return AbilityType.Peek;
            if (lower.Contains("trump") && lower.Contains("change")) return AbilityType.TrumpChanger;
            if (lower.Contains("block") || lower.Contains("cap attacks")) return AbilityType.Blocker;
            if (lower.Contains("discard") && lower.Contains("opponent")) return AbilityType.Riposte;
            if (lower.Contains("steal")) return AbilityType.DoubleAgent;
            if (lower.Contains("defend") && lower.Contains("two")) return AbilityType.DoubleDefense;
            if (lower.Contains("seize") && lower.Contains("initiative")) return AbilityType.SeizeInitiative;
            if (lower.Contains("any rank")) return AbilityType.DoubleTrouble;
            if (lower.Contains("+2 rank") || lower.Contains("bonus rank")) return AbilityType.Conquer;
            if (lower.Contains("resource") || lower.Contains("gain")) return AbilityType.BattleHardened;

            if (trigger == TriggerTiming.Passive)
            {
                if (lower.Contains("card counter") || lower.Contains("top card")) return AbilityType.CardCounter;
                if (lower.Contains("trump") && lower.Contains("draw")) return AbilityType.TrumpAffinity;
                if (lower.Contains("endgame") || lower.Contains("6 or fewer")) return AbilityType.EndgameSpecialist;
                if (lower.Contains("steady") || lower.Contains("3 or fewer")) return AbilityType.SteadyHand;
                if (lower.Contains("equilibrium") || lower.Contains("fewer cards")) return AbilityType.Equilibrium;
                if (lower.Contains("bloodlust") || lower.Contains("eat")) return AbilityType.Bloodlust;
                if (lower.Contains("quick hands") || lower.Contains("successful defense")) return AbilityType.QuickHands;
                return AbilityType.SteadyHand;
            }

            if (trigger == TriggerTiming.OnAttack) return AbilityType.ExtraDraw;
            if (trigger == TriggerTiming.OnDefend) return AbilityType.Brace;

            return null;
        }
    }
}
