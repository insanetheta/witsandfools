namespace WitsAndFools
{
    // A simple greedy AI: defend with the cheapest legal card, otherwise eat.
    // Attack with the lowest non-trump that's legal; piles on follow-up attacks while it has them.
    public sealed class AIPlayer : IPlayerController
    {
        public PlayerKind Kind => PlayerKind.AI;
        public string DisplayName { get; }

        public AIPlayer(string name = "Knave") { DisplayName = name; }

        public void RequestAction(GameEngine engine, int playerIndex)
        {
            switch (engine.Phase)
            {
                case Phase.Attack:
                    AttackStep(engine, playerIndex);
                    return;
                case Phase.Defense:
                    DefenseStep(engine, playerIndex);
                    return;
            }
        }

        void AttackStep(GameEngine engine, int playerIndex)
        {
            var hand = engine.HandOf(playerIndex);
            Card? attack = LowestLegalAttack(engine, hand);

            // If the bout is fully defended, decide whether to pile on or end.
            if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended)
            {
                bool wantsToStop = attack == null || ShouldStopPilingOn(engine, attack.Value);
                if (wantsToStop || !engine.TryAttack(playerIndex, attack.Value))
                    engine.TryEndBout(playerIndex);
                return;
            }

            if (attack == null) return; // first attack of bout but hand has no legal play (rare; trust caller)
            if (!engine.TryAttack(playerIndex, attack.Value))
            {
                // Engine refused — likely defender's hand is too small. End the bout if we can.
                if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended)
                    engine.TryEndBout(playerIndex);
            }
        }

        bool ShouldStopPilingOn(GameEngine engine, Card next)
        {
            // Don't waste good trumps if all current attacks already landed.
            return next.Suit == engine.Trump;
        }

        Card? LowestLegalAttack(GameEngine engine, Hand hand)
        {
            Card? best = null;
            foreach (var c in hand.Cards)
            {
                if (!Rules.CanAttackWith(engine.Bout, c)) continue;
                if (best == null) { best = c; continue; }
                if (PrefersAsAttack(c, best.Value, engine.Trump)) best = c;
            }
            return best;
        }

        static bool PrefersAsAttack(Card candidate, Card current, Suit trump)
        {
            bool ct = candidate.Suit == trump;
            bool cu = current.Suit == trump;
            if (cu && !ct) return true;
            if (ct && !cu) return false;
            return (int)candidate.Rank < (int)current.Rank;
        }

        void DefenseStep(GameEngine engine, int playerIndex)
        {
            int slot = engine.Bout.FirstUndefendedSlot();
            if (slot < 0) return;

            var hand = engine.HandOf(playerIndex);
            Card? best = null;
            foreach (var c in hand.Cards)
            {
                if (!Rules.CanDefendSlotWith(engine.Bout, slot, c, engine.Trump)) continue;
                if (best == null) { best = c; continue; }
                if (PrefersAsDefense(c, best.Value, engine.Trump)) best = c;
            }

            if (best == null) { engine.TryEat(playerIndex); return; }
            engine.TryDefend(playerIndex, slot, best.Value);
        }

        static bool PrefersAsDefense(Card candidate, Card current, Suit trump)
        {
            bool ct = candidate.Suit == trump;
            bool cu = current.Suit == trump;
            if (cu && !ct) return true;
            if (ct && !cu) return false;
            return (int)candidate.Rank < (int)current.Rank;
        }
    }
}
