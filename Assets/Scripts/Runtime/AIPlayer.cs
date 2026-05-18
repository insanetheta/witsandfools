using System;

namespace WitsAndFools
{
    public sealed class AIPlayer : IPlayerController
    {
        public PlayerKind Kind => PlayerKind.AI;
        public string DisplayName { get; }

        public float RandomMoveChance { get; set; }
        public float AbilityEagerness { get; set; } = 1f;
        Random _rng;

        public AIPlayer(string name = "Knave", int? seed = null)
        {
            DisplayName = name;
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public void RequestAction(GameEngine engine, int playerIndex)
        {
            if (RandomMoveChance > 0 && _rng.NextDouble() < RandomMoveChance)
            {
                if (TryRandomAction(engine, playerIndex)) return;
            }

            if (TryActivateAbility(engine, playerIndex)) return;

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

        bool TryRandomAction(GameEngine engine, int playerIndex)
        {
            var hand = engine.HandOf(playerIndex);
            if (hand.Count == 0) return false;
            var card = hand.Cards[_rng.Next(hand.Count)];

            if (engine.Phase == Phase.Attack)
            {
                if (engine.TryAttack(playerIndex, card)) return true;
                if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended)
                    return engine.TryEndBout(playerIndex);
            }
            else if (engine.Phase == Phase.Defense)
            {
                int slot = engine.Bout.FirstUndefendedSlot();
                if (slot >= 0 && engine.TryDefend(playerIndex, slot, card)) return true;
                return engine.TryEat(playerIndex);
            }
            return false;
        }

        // ---------- Ability evaluation ----------

        bool TryActivateAbility(GameEngine engine, int playerIndex)
        {
            var hand = engine.HandOf(playerIndex);
            foreach (var card in hand.Cards)
            {
                if (!card.HasAbility) continue;
                if (!ShouldUseAbility(engine, playerIndex, card)) continue;
                int slot = engine.Phase == Phase.Defense ? engine.Bout.FirstUndefendedSlot() : -1;
                if (engine.TryUseAbility(playerIndex, card, slot))
                    return true;
            }
            return false;
        }

        bool ShouldUseAbility(GameEngine engine, int playerIndex, Card card)
        {
            var hand = engine.HandOf(playerIndex);
            bool wants = WantsToUseAbility(engine, playerIndex, card, hand);
            if (!wants) return false;

            if (AbilityEagerness < 1f && _rng.NextDouble() > AbilityEagerness)
                return false;

            return true;
        }

        bool WantsToUseAbility(GameEngine engine, int playerIndex, Card card, Hand hand)
        {
            switch (card.Ability.Value)
            {
                case AbilityType.TrumpChanger:
                    if (card.Suit == engine.Trump) return false;
                    int suitThreshold = AbilityEagerness >= 1.2f ? 2 : 3;
                    return CountSuit(hand, card.Suit) >= suitThreshold && CountSuit(hand, engine.Trump) <= 1;

                case AbilityType.ExtraDraw:
                    int deckThreshold = AbilityEagerness >= 1.2f ? 3 : 6;
                    return engine.Phase == Phase.Attack && engine.DeckCount >= deckThreshold;

                case AbilityType.Blocker:
                    int attackThreshold = AbilityEagerness >= 1.2f ? 1 : 2;
                    return engine.Phase == Phase.Defense && engine.Bout.AttackCount >= attackThreshold;

                case AbilityType.DoubleTrouble:
                    if (engine.Phase != Phase.Attack || engine.Bout.IsEmpty) return false;
                    return HasOffRankCard(hand, engine.Bout);

                case AbilityType.DoubleDefense:
                    return engine.Phase == Phase.Defense && CanCoverTwoSlots(engine, card);

                case AbilityType.SeizeInitiative:
                    int handReq = AbilityEagerness >= 1.2f ? 3 : 4;
                    return engine.Phase == Phase.Defense && hand.Count >= handReq;

                case AbilityType.PileOn:
                    return engine.Phase == Phase.Attack && engine.Bout.AttackCount >= 1 && hand.Count >= 4;

                case AbilityType.Feint:
                    return engine.Phase == Phase.Attack && engine.DeckCount > 0 && engine.Bout.AttackCount >= 1;

                case AbilityType.Deflect:
                    if (engine.Phase != Phase.Defense) return false;
                    int undefended = CountUndefended(engine.Bout);
                    int desperation = AbilityEagerness >= 1.2f ? 4 : 3;
                    return undefended >= 2 && hand.Count <= desperation;

                case AbilityType.SlipAway:
                    if (engine.Phase != Phase.Defense) return false;
                    return CountUndefended(engine.Bout) >= 2;

                case AbilityType.Peek:
                    return engine.DeckCount >= 3 && hand.Count >= 3;

                case AbilityType.Gambit:
                    int trumpCount = CountSuit(hand, engine.Trump);
                    int trumpReq = AbilityEagerness >= 1.2f ? 1 : 0;
                    return trumpCount <= trumpReq && hand.Count >= 4 && engine.DeckCount >= hand.Count;

                default:
                    return false;
            }
        }

        static int CountSuit(Hand hand, Suit suit)
        {
            int n = 0;
            foreach (var c in hand.Cards)
                if (c.Suit == suit) n++;
            return n;
        }

        static bool HasOffRankCard(Hand hand, Bout bout)
        {
            foreach (var c in hand.Cards)
                if (!Rules.CanAttackWith(bout, c)) return true;
            return false;
        }

        static int CountUndefended(Bout bout)
        {
            int n = 0;
            for (int i = 0; i < bout.Defenses.Count; i++)
                if (bout.Defenses[i] == null) n++;
            return n;
        }

        static bool CanCoverTwoSlots(GameEngine engine, Card card)
        {
            int slot1 = engine.Bout.FirstUndefendedSlot();
            if (slot1 < 0 || !Rules.Beats(card, engine.Bout.Attacks[slot1], engine.Trump)) return false;
            for (int i = slot1 + 1; i < engine.Bout.Defenses.Count; i++)
                if (engine.Bout.Defenses[i] == null && Rules.Beats(card, engine.Bout.Attacks[i], engine.Trump))
                    return true;
            return false;
        }

        // ---------- Normal attack ----------

        void AttackStep(GameEngine engine, int playerIndex)
        {
            var hand = engine.HandOf(playerIndex);
            Card? attack = LowestLegalAttack(engine, hand);

            if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended)
            {
                bool wantsToStop = attack == null || ShouldStopPilingOn(engine, attack.Value);
                if (wantsToStop || !engine.TryAttack(playerIndex, attack.Value))
                    engine.TryEndBout(playerIndex);
                return;
            }

            if (attack != null && engine.TryAttack(playerIndex, attack.Value))
                return;

            if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended)
                engine.TryEndBout(playerIndex);
            else if (!engine.Bout.IsEmpty)
                engine.TryEat(engine.DefenderIndex);
        }

        bool ShouldStopPilingOn(GameEngine engine, Card next)
        {
            return next.Suit == engine.Trump;
        }

        Card? LowestLegalAttack(GameEngine engine, Hand hand)
        {
            Card? best = null;
            bool bypass = engine.DoubleTroubleActive;
            foreach (var c in hand.Cards)
            {
                if (!bypass && !Rules.CanAttackWith(engine.Bout, c)) continue;
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

        // ---------- Normal defense ----------

        void DefenseStep(GameEngine engine, int playerIndex)
        {
            int slot = engine.Bout.FirstUndefendedSlot();
            if (slot < 0) return;

            var hand = engine.HandOf(playerIndex);
            var candidates = new System.Collections.Generic.List<Card>();
            foreach (var c in hand.Cards)
            {
                if (!Rules.CanDefendSlotWith(engine.Bout, slot, c, engine.Trump)) continue;
                candidates.Add(c);
            }

            candidates.Sort((a, b) => PrefersAsDefense(a, b, engine.Trump) ? -1 : 1);

            foreach (var c in candidates)
            {
                if (engine.TryDefend(playerIndex, slot, c)) return;
            }

            engine.TryEat(playerIndex);
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
