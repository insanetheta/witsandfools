using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public sealed class Deck
    {
        readonly List<Card> _cards = new(36);
        readonly Random _rng;

        public int Count => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        public Deck(int? seed = null, IReadOnlyDictionary<(Suit, Rank), AbilityType> abilities = null)
        {
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                for (Rank rank = Rank.Six; rank <= Rank.Ace; rank++)
                {
                    AbilityType? ability = null;
                    if (abilities != null && abilities.TryGetValue((suit, rank), out var a))
                        ability = a;
                    _cards.Add(new Card(suit, rank, ability));
                }
        }

        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public Card Draw()
        {
            if (_cards.Count == 0) throw new InvalidOperationException("Deck is empty.");
            var top = _cards[^1];
            _cards.RemoveAt(_cards.Count - 1);
            return top;
        }

        public Card PeekBottom() => _cards[0];

        public IReadOnlyList<Card> AsReadOnly() => _cards;
    }
}
