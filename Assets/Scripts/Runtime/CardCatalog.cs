using System;
using System.Collections.Generic;
using System.Linq;

namespace WitsAndFools
{
    public static class CardCatalog
    {
        static readonly Dictionary<string, CardDefinition> _byId = new();
        static bool _initialized;

        public static void Register(CardDefinition card)
        {
            _byId[card.Id] = card;
        }

        public static void RegisterAll(IEnumerable<CardDefinition> cards)
        {
            foreach (var card in cards)
                _byId[card.Id] = card;
            _initialized = true;
        }

        public static CardDefinition Get(string id)
        {
            if (_byId.TryGetValue(id, out var card)) return card;
            throw new KeyNotFoundException($"Card not found in catalog: {id}");
        }

        public static bool TryGet(string id, out CardDefinition card) =>
            _byId.TryGetValue(id, out card);

        public static IReadOnlyList<CardDefinition> All() =>
            _byId.Values.ToList();

        public static IReadOnlyList<CardDefinition> ForDoctrine(DoctrineType doctrine) =>
            _byId.Values.Where(c => c.Doctrine == doctrine).ToList();

        public static IReadOnlyList<CardDefinition> NeutralCards() =>
            ForDoctrine(DoctrineType.Neutral);

        public static IReadOnlyList<CardDefinition> ForDoctrine(DoctrineType doctrine, CardRarity rarity) =>
            _byId.Values.Where(c => c.Doctrine == doctrine && c.Rarity == rarity).ToList();

        public static IReadOnlyList<CardDefinition> StartingDeck(DoctrineType doctrine) =>
            _byId.Values.Where(c => c.InStartingDeck && (c.Doctrine == doctrine || c.IsNeutral)).ToList();

        public static IReadOnlyList<CardDefinition> ByRarity(CardRarity rarity) =>
            _byId.Values.Where(c => c.Rarity == rarity).ToList();

        public static IReadOnlyList<CardDefinition> Draftable(DoctrineType playerDoctrine) =>
            _byId.Values.Where(c => c.Doctrine == playerDoctrine || c.IsNeutral).ToList();

        public static int Count => _byId.Count;
        public static bool IsInitialized => _initialized;

        public static void Clear()
        {
            _byId.Clear();
            _initialized = false;
        }
    }
}
