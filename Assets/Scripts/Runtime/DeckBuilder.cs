using System;
using System.Collections.Generic;
using System.Linq;

namespace WitsAndFools
{
    public static class DeckBuilder
    {
        public const int StartingDeckSize = 12;
        public const int RecommendedMin = 10;
        public const int RecommendedMax = 16;
        public const int WarningThreshold = 18;
        public const int HardCap = 25;

        public static List<CardDefinition> OfferCards(DoctrineType playerDoctrine, int count, Random rng,
            CardRarity maxRarity = CardRarity.Rare, HashSet<string> excludeIds = null)
        {
            var pool = CardCatalog.Draftable(playerDoctrine)
                .Where(c => c.Rarity <= maxRarity)
                .Where(c => excludeIds == null || !excludeIds.Contains(c.Id))
                .ToList();

            var weighted = new List<(CardDefinition card, float weight)>();
            foreach (var card in pool)
            {
                float w = card.Doctrine == playerDoctrine ? 2.0f : 1.0f;
                w *= card.Rarity switch
                {
                    CardRarity.Common => 3.0f,
                    CardRarity.Uncommon => 2.0f,
                    CardRarity.Rare => 1.0f,
                    _ => 1.0f
                };
                weighted.Add((card, w));
            }

            var result = new List<CardDefinition>();
            float totalWeight = weighted.Sum(w => w.weight);

            for (int i = 0; i < count && weighted.Count > 0; i++)
            {
                float roll = (float)(rng.NextDouble() * totalWeight);
                float cumulative = 0;
                int picked = 0;
                for (int j = 0; j < weighted.Count; j++)
                {
                    cumulative += weighted[j].weight;
                    if (cumulative >= roll) { picked = j; break; }
                }
                result.Add(weighted[picked].card);
                totalWeight -= weighted[picked].weight;
                weighted.RemoveAt(picked);
            }

            return result;
        }

        public static bool CanAddCard(List<string> deckCardIds) =>
            deckCardIds.Count < HardCap;

        public static bool IsBloated(List<string> deckCardIds) =>
            deckCardIds.Count >= WarningThreshold;

        public static DeckSizeInfo GetDeckSizeInfo(List<string> deckCardIds, DoctrineType doctrine)
        {
            int size = deckCardIds.Count;
            int doctrineCount = deckCardIds.Count(id =>
                CardCatalog.TryGet(id, out var c) && c.Doctrine == doctrine);
            float density = size > 0 ? (float)doctrineCount / size : 0;
            float cycleEstimate = size / 6.0f;

            return new DeckSizeInfo
            {
                TotalCards = size,
                DoctrineCards = doctrineCount,
                NeutralCards = size - doctrineCount,
                SynergyDensity = density,
                CycleEstimate = cycleEstimate,
                Status = size <= RecommendedMax ? DeckStatus.Healthy
                    : size <= WarningThreshold ? DeckStatus.Heavy
                    : DeckStatus.Bloated
            };
        }

        public static int CardRemovalCost(int removalsPurchased) =>
            12 + (removalsPurchased * 4);

        public static int CardPrice(CardRarity rarity) => rarity switch
        {
            CardRarity.Common => 7,
            CardRarity.Uncommon => 12,
            CardRarity.Rare => 21,
            _ => 10
        };
    }

    public enum DeckStatus { Healthy, Heavy, Bloated }

    public struct DeckSizeInfo
    {
        public int TotalCards;
        public int DoctrineCards;
        public int NeutralCards;
        public float SynergyDensity;
        public float CycleEstimate;
        public DeckStatus Status;
    }
}
