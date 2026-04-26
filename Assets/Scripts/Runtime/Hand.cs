using System.Collections.Generic;

namespace WitsAndFools
{
    public sealed class Hand
    {
        readonly List<Card> _cards = new();

        public int Count => _cards.Count;
        public IReadOnlyList<Card> Cards => _cards;

        public void Add(Card card) => _cards.Add(card);

        public bool Remove(Card card) => _cards.Remove(card);

        public bool Contains(Card card) => _cards.Contains(card);

        public void Clear() => _cards.Clear();
    }
}
