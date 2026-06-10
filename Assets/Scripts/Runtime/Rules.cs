namespace WitsAndFools
{
    public static class Rules
    {
        public const int HandSizeTwoPlayer = 6;
        public const int MaxAttacksPerBout = 6;

        public static bool Beats(Card defender, Card attacker, Suit trump)
        {
            if (defender.Suit == attacker.Suit)
                return (int)defender.Rank > (int)attacker.Rank;
            if (defender.Suit == trump && attacker.Suit != trump)
                return true;
            return false;
        }

        // First attack of a bout: any card legal. Follow-up attacks: rank must already be in play.
        public static bool CanAttackWith(Bout bout, Card card)
        {
            if (bout.IsEmpty) return true;
            foreach (var rank in bout.AttackRanks())
                if (rank == card.Rank) return true;
            return false;
        }

        public static bool CanDefendSlotWith(Bout bout, int slot, Card card, Suit trump)
        {
            if (slot < 0 || slot >= bout.Attacks.Count) return false;
            if (bout.Defenses[slot] != null) return false;
            var attack = bout.Attacks[slot];
            int bonus = bout.BonusAt(slot);
            if (card.Suit == attack.Suit)
                return (int)card.Rank > (int)attack.Rank + bonus;
            if (card.Suit == trump && attack.Suit != trump)
                return true;
            return false;
        }
    }
}
