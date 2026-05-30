using System;
using System.Collections.Generic;
using System.Linq;

namespace WitsAndFools
{
    public sealed class PlayerDeck
    {
        readonly List<CardDefinition> _templates;
        readonly List<Card> _drawPile = new();
        readonly List<Card> _discardPile = new();
        readonly List<Card> _removedFromGame = new();
        Random _rng;

        public int DeckSize => _templates.Count;
        public int DrawPileCount => _drawPile.Count;
        public int DiscardCount => _discardPile.Count;
        public int RemovedCount => _removedFromGame.Count;
        public bool DrawPileEmpty => _drawPile.Count == 0;
        public IReadOnlyList<CardDefinition> Templates => _templates;
        public IReadOnlyList<Card> DrawPile => _drawPile;
        public IReadOnlyList<Card> DiscardPile => _discardPile;

        public PlayerDeck(IEnumerable<CardDefinition> templates)
        {
            _templates = new List<CardDefinition>(templates);
        }

        public PlayerDeck(IEnumerable<string> cardIds)
        {
            _templates = new List<CardDefinition>();
            foreach (var id in cardIds)
                _templates.Add(CardCatalog.Get(id));
        }

        public void ApplyAbilityOverrides(Dictionary<string, AbilityType> overrides)
        {
            foreach (var t in _templates)
            {
                if (t.Id != null && overrides.TryGetValue(t.Id, out var upgraded))
                    t.Ability = upgraded;
            }
        }

        public void ApplyRankOverrides(Dictionary<string, Rank> overrides)
        {
            foreach (var t in _templates)
            {
                if (t.Id != null && overrides.TryGetValue(t.Id, out var newRank))
                    t.Rank = newRank;
            }
        }

        public void Build(int seed)
        {
            _rng = new Random(seed);
            _drawPile.Clear();
            _discardPile.Clear();
            _removedFromGame.Clear();
            foreach (var def in _templates)
                _drawPile.Add(def.ToRuntimeCard());
            Shuffle();
        }

        public void Shuffle()
        {
            for (int i = _drawPile.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
            }
        }

        public Card Draw()
        {
            if (_drawPile.Count == 0)
                throw new InvalidOperationException("Draw pile is empty.");
            var top = _drawPile[^1];
            _drawPile.RemoveAt(_drawPile.Count - 1);
            return top;
        }

        public bool TryDraw(out Card card)
        {
            if (_drawPile.Count == 0) { card = default; return false; }
            card = Draw();
            return true;
        }

        public Card[] PeekTop(int n)
        {
            int count = Math.Min(n, _drawPile.Count);
            var result = new Card[count];
            for (int i = 0; i < count; i++)
                result[i] = _drawPile[_drawPile.Count - 1 - i];
            return result;
        }

        public void ReplaceTop(Card[] cards)
        {
            int count = Math.Min(cards.Length, _drawPile.Count);
            for (int i = 0; i < count; i++)
                _drawPile[_drawPile.Count - 1 - i] = cards[i];
        }

        public void PutOnTop(Card card) => _drawPile.Add(card);

        public void PutOnBottom(Card card) => _drawPile.Insert(0, card);

        public void ShuffleIn(Card card)
        {
            _drawPile.Insert(_rng.Next(_drawPile.Count + 1), card);
        }

        public void ShuffleInMany(IEnumerable<Card> cards)
        {
            foreach (var c in cards)
                _drawPile.Insert(_rng.Next(_drawPile.Count + 1), c);
        }

        public bool RemoveCard(Card card) => _drawPile.Remove(card);

        public void Discard(Card card) => _discardPile.Add(card);

        public void RemoveFromGame(Card card) => _removedFromGame.Add(card);

        public void RemoveFromGame(IEnumerable<Card> cards)
        {
            foreach (var c in cards) _removedFromGame.Add(c);
        }

        public void AddTemplate(CardDefinition def) => _templates.Add(def);

        public void AddTemplate(string cardId) => _templates.Add(CardCatalog.Get(cardId));

        public bool RemoveTemplate(string cardId)
        {
            int idx = _templates.FindIndex(t => t.Id == cardId);
            if (idx < 0) return false;
            _templates.RemoveAt(idx);
            return true;
        }

        public IReadOnlyList<CardDefinition> GetPassiveCards() =>
            _templates.Where(t => t.Trigger == TriggerTiming.Passive).ToList();

        public IReadOnlyList<CardDefinition> GetDoctrineCards(DoctrineType doctrine) =>
            _templates.Where(t => t.Doctrine == doctrine).ToList();

        public int DoctrineCount(DoctrineType doctrine) =>
            _templates.Count(t => t.Doctrine == doctrine);

        public float SynergyDensity(DoctrineType primary)
        {
            if (_templates.Count == 0) return 0f;
            int doctrineCards = _templates.Count(t => t.Doctrine == primary);
            return (float)doctrineCards / _templates.Count;
        }

        public CardDefinition LookupDefinition(Card runtimeCard)
        {
            return _templates.FirstOrDefault(t => t.Suit == runtimeCard.Suit && t.Rank == runtimeCard.Rank);
        }

        public static PlayerDeck CreateStandard(IReadOnlyDictionary<(Suit, Rank), AbilityType> abilities = null)
        {
            var templates = new List<CardDefinition>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                for (Rank rank = Rank.Six; rank <= Rank.Ace; rank++)
                {
                    AbilityType? ability = null;
                    if (abilities != null && abilities.TryGetValue((suit, rank), out var a))
                        ability = a;
                    templates.Add(new CardDefinition
                    {
                        Id = $"std_{suit.ToString().ToLowerInvariant()}_{(int)rank}",
                        Name = $"{rank.Label()} of {suit}",
                        Suit = suit,
                        Rank = rank,
                        Doctrine = DoctrineType.Neutral,
                        Trigger = ability.HasValue ? TriggerTiming.OnAttack : TriggerTiming.None,
                        Ability = ability,
                        Rarity = CardRarity.Common
                    });
                }
            return new PlayerDeck(templates);
        }
    }
}
