using System.Collections.Generic;

namespace WitsAndFools
{
    public sealed class Bout
    {
        readonly List<Card> _attacks = new();
        readonly List<Card?> _defenses = new();
        readonly HashSet<int> _autoDefended = new();
        public bool AttacksCapped { get; set; }

        public IReadOnlyList<Card> Attacks => _attacks;
        public IReadOnlyList<Card?> Defenses => _defenses;

        public int AttackCount => _attacks.Count;
        public bool IsEmpty => _attacks.Count == 0;
        public bool FullyDefended
        {
            get
            {
                if (_attacks.Count == 0) return false;
                for (int i = 0; i < _defenses.Count; i++)
                    if (_defenses[i] == null && !_autoDefended.Contains(i)) return false;
                return true;
            }
        }

        public void AddAttack(Card card)
        {
            _attacks.Add(card);
            _defenses.Add(null);
        }

        public bool TryDefend(int slot, Card defenseCard)
        {
            if (slot < 0 || slot >= _defenses.Count) return false;
            if (_defenses[slot] != null) return false;
            _defenses[slot] = defenseCard;
            return true;
        }

        public int FirstUndefendedSlot()
        {
            for (int i = 0; i < _defenses.Count; i++)
                if (_defenses[i] == null && !_autoDefended.Contains(i)) return i;
            return -1;
        }

        public void AutoDefend(int slot)
        {
            if (slot >= 0 && slot < _defenses.Count) _autoDefended.Add(slot);
        }

        public IEnumerable<Card> AllCards()
        {
            foreach (var a in _attacks) yield return a;
            foreach (var d in _defenses) if (d.HasValue) yield return d.Value;
        }

        public IEnumerable<Rank> AttackRanks()
        {
            var seen = new HashSet<Rank>();
            foreach (var a in _attacks) if (seen.Add(a.Rank)) yield return a.Rank;
            foreach (var d in _defenses)
                if (d.HasValue && seen.Add(d.Value.Rank)) yield return d.Value.Rank;
        }

        public void Clear()
        {
            _attacks.Clear();
            _defenses.Clear();
            _autoDefended.Clear();
            AttacksCapped = false;
        }
    }
}
