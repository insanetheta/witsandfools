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

        public Card[] PeekTop(int n)
        {
            int count = System.Math.Min(n, _cards.Count);
            var result = new Card[count];
            for (int i = 0; i < count; i++)
                result[i] = _cards[_cards.Count - 1 - i];
            return result;
        }

        public void ReplaceTop(Card[] cards)
        {
            int count = System.Math.Min(cards.Length, _cards.Count);
            for (int i = 0; i < count; i++)
                _cards[_cards.Count - 1 - i] = cards[i];
        }

        public void SwapTopTwo()
        {
            if (_cards.Count < 2) return;
            int top = _cards.Count - 1;
            (_cards[top], _cards[top - 1]) = (_cards[top - 1], _cards[top]);
        }

        public void PutTopOnBottom()
        {
            if (_cards.Count < 2) return;
            var top = _cards[_cards.Count - 1];
            _cards.RemoveAt(_cards.Count - 1);
            _cards.Insert(0, top);
        }

        public void PutOnTop(Card card) => _cards.Add(card);

        public void PutOnBottom(Card card) => _cards.Insert(0, card);

        public void ShuffleIn(Card card)
        {
            _cards.Insert(_rng.Next(_cards.Count + 1), card);
        }

        public void ShuffleInMany(IEnumerable<Card> cards)
        {
            foreach (var c in cards)
                _cards.Insert(_rng.Next(_cards.Count + 1), c);
        }
    }
}

