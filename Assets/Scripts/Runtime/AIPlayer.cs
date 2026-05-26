using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public sealed class AIPlayer : IPlayerController
    {
        public PlayerKind Kind => PlayerKind.AI;
        public string DisplayName { get; }

        public float RandomMoveChance { get; set; }
        public float AbilityEagerness { get; set; } = 1f;
        public AIArchetypeName Archetype { get; set; } = AIArchetypeName.Fox;
        Random _rng;
        int _boutsPlayed;

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

        public void NotifyBoutResolved() => _boutsPlayed++;
        public void ResetMatchState() => _boutsPlayed = 0;

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
                if (card.Trigger != TriggerTiming.None) continue;
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

                // --- Rogue: Shadow ---

                case AbilityType.Riposte:
                    return engine.Phase == Phase.Defense && engine.GetResource(playerIndex) >= 1
                        && engine.HandOf(1 - playerIndex).Count >= 2;

                case AbilityType.ShadowCloak:
                    return engine.Phase == Phase.Defense && engine.Bout.AttackCount >= 3;

                // --- Rogue: Spy ---

                case AbilityType.Wiretap:
                    return engine.DeckCount >= 5 && hand.Count >= 3;

                case AbilityType.DoubleAgent:
                    return engine.GetResource(playerIndex) >= 3
                        && engine.HandOf(1 - playerIndex).Count >= 3;

                case AbilityType.Blackmail:
                    return engine.GetResource(playerIndex) >= 2
                        && engine.HandOf(1 - playerIndex).Count >= 3;

                // --- Rogue: Saboteur ---

                case AbilityType.SleightOfHand:
                    return engine.DeckCount > 0 && hand.Count >= 3;

                case AbilityType.SmokeBomb:
                    return engine.Phase == Phase.Defense && engine.GetResource(playerIndex) >= 1
                        && !engine.Bout.IsEmpty && CountUndefended(engine.Bout) >= 2;

                case AbilityType.TrapCard:
                    return engine.Phase == Phase.Defense && engine.GetResource(playerIndex) >= 2
                        && engine.Bout.FirstUndefendedSlot() >= 0;

                // --- Brute: Berserker ---

                case AbilityType.Rampage:
                    return engine.Phase == Phase.Attack && engine.GetResource(playerIndex) >= 1
                        && engine.DeckCount >= 2;

                // --- Brute: Brawler ---

                case AbilityType.Haymaker:
                    return engine.Phase == Phase.Attack && engine.DeckCount >= 2 && hand.Count <= 5;

                case AbilityType.IronGrip:
                    return engine.GetResource(playerIndex) >= 1 && engine.DeckCount >= 3
                        && hand.Count <= 4;

                case AbilityType.Brawl:
                    return engine.GetResource(playerIndex) >= 2 && engine.DeckCount >= 6
                        && hand.Count <= 3 && engine.HandOf(1 - playerIndex).Count >= 5;

                // --- Brute: Warlord ---

                case AbilityType.Conquer:
                    return engine.Phase == Phase.Attack && engine.GetResource(playerIndex) >= 1
                        && engine.DeckCount > 0;

                case AbilityType.Intimidate:
                    return engine.Phase == Phase.Attack && engine.GetResource(playerIndex) >= 1
                        && engine.HandOf(1 - playerIndex).Count >= 3;

                case AbilityType.CrownSeize:
                    return engine.GetResource(playerIndex) >= 3 && engine.DeckCount >= 2
                        && CountSuit(hand, engine.Trump) <= 1;

                // --- Diplomat: Courtier ---

                case AbilityType.CourtIntrigue:
                    return engine.GetResource(playerIndex) >= 1 && engine.DeckCount >= 3;

                case AbilityType.RoyalDecree:
                    return engine.GetResource(playerIndex) >= 1 && engine.DeckCount >= 2
                        && engine.HandOf(1 - playerIndex).Count >= 2;

                case AbilityType.Patronage:
                    return engine.GetResource(playerIndex) >= 3 && engine.DeckCount >= 3;

                // --- Diplomat: Puppeteer ---

                case AbilityType.PullStrings:
                    return engine.GetResource(playerIndex) >= 1
                        && engine.HandOf(1 - playerIndex).Count >= 3;

                case AbilityType.Misdirection:
                    return engine.Phase == Phase.Defense && engine.GetResource(playerIndex) >= 1
                        && engine.Bout.AttackCount >= 2 && engine.HandOf(1 - playerIndex).Count >= 2;

                case AbilityType.ForcedHand:
                    return engine.GetResource(playerIndex) >= 2
                        && engine.HandOf(1 - playerIndex).Count >= 3;

                // --- Diplomat: Peacemaker ---

                case AbilityType.Diplomacy:
                    return engine.Phase == Phase.Defense && !engine.Bout.IsEmpty
                        && CountUndefended(engine.Bout) >= 2 && engine.DeckCount > 0;

                case AbilityType.SafePassage:
                    return engine.Phase == Phase.Defense && engine.GetResource(playerIndex) >= 2
                        && engine.Bout.FirstUndefendedSlot() >= 0 && CountUndefended(engine.Bout) >= 2;

                case AbilityType.Treaty:
                    return engine.GetResource(playerIndex) >= 3
                        && hand.Count <= 4 && engine.DeckCount >= 4;

                // --- Gambler: Card Shark ---

                case AbilityType.StackTheDeck:
                    return engine.DeckCount >= 2 && hand.Count >= 3;

                case AbilityType.SecondDeal:
                    return engine.GetResource(playerIndex) >= 1 && engine.DeckCount >= 2;

                case AbilityType.ColdRead:
                    return engine.GetResource(playerIndex) >= 2 && engine.DeckCount >= 3;

                // --- Gambler: High Roller ---

                case AbilityType.AllIn:
                    return engine.GetResource(playerIndex) >= 2
                        && engine.DeckCount >= engine.GetResource(playerIndex);

                case AbilityType.DoubleOrNothing:
                    return engine.GetResource(playerIndex) >= 1 && engine.DeckCount >= 2;

                case AbilityType.LuckyStreak:
                    return engine.Phase == Phase.Attack && engine.GetResource(playerIndex) >= 2
                        && engine.DeckCount >= 1;

                // --- Gambler: Trickster ---

                case AbilityType.BlindSwap:
                    return engine.HandOf(1 - playerIndex).Count >= 2 && hand.Count >= 2;

                case AbilityType.Misdeal:
                    return engine.Phase == Phase.Attack && engine.GetResource(playerIndex) >= 1
                        && engine.Bout.AttackCount >= 2;

                case AbilityType.WildCard:
                    return engine.GetResource(playerIndex) >= 2
                        && engine.Discard.Count > 0 && hand.Count <= 5;

                // --- Neutral ---

                case AbilityType.Fortify:
                    return engine.Phase == Phase.Defense && engine.Bout.FirstUndefendedSlot() >= 0
                        && !HasLegalDefense(engine, hand, engine.Bout.FirstUndefendedSlot());

                case AbilityType.SecondWind:
                    return hand.Count >= 4 && engine.DeckCount >= 3
                        && CountSuit(hand, engine.Trump) <= 1;

                case AbilityType.Brace:
                    return engine.DeckCount >= 2 && hand.Count <= 4;

                case AbilityType.Desperation:
                    return engine.Phase == Phase.Defense && CountUndefended(engine.Bout) >= 3
                        && hand.Count <= 3 && engine.DeckCount >= 4;

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

        // ---------- Attack step (archetype-specific) ----------

        void AttackStep(GameEngine engine, int playerIndex)
        {
            var hand = engine.HandOf(playerIndex);
            int oppIndex = 1 - playerIndex;
            var oppHand = engine.HandOf(oppIndex);

            Card? attack = SelectAttackCard(engine, hand, oppHand);

            if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended)
            {
                bool wantsToStop = attack == null || ShouldStopAttacking(engine, hand, attack.Value, oppHand);
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

        Card? SelectAttackCard(GameEngine engine, Hand hand, Hand oppHand)
        {
            return Archetype switch
            {
                AIArchetypeName.Brawler => HighestLegalAttack(engine, hand),
                AIArchetypeName.Miser => LowestLegalAttack(engine, hand),
                AIArchetypeName.Noble => NobleAttackPick(engine, hand),
                AIArchetypeName.Scholar => ScholarAttackPick(engine, hand),
                AIArchetypeName.Assassin => AssassinAttackPick(engine, hand),
                AIArchetypeName.Fox => FoxAttackPick(engine, hand, oppHand),
                _ => LowestLegalAttack(engine, hand),
            };
        }

        bool ShouldStopAttacking(GameEngine engine, Hand hand, Card next, Hand oppHand)
        {
            return Archetype switch
            {
                AIArchetypeName.Brawler => false, // never stops willingly
                AIArchetypeName.Miser => engine.Bout.AttackCount >= 2 || next.Suit == engine.Trump,
                AIArchetypeName.Noble => next.Suit == engine.Trump && engine.DeckCount >= 12,
                AIArchetypeName.Scholar => next.Suit == engine.Trump,
                AIArchetypeName.Assassin => _boutsPlayed < 3 && engine.Bout.AttackCount >= 1,
                AIArchetypeName.Fox => FoxShouldStop(engine, hand, next, oppHand),
                _ => next.Suit == engine.Trump,
            };
        }

        // --- Brawler: plays highest card to overwhelm ---

        Card? HighestLegalAttack(GameEngine engine, Hand hand)
        {
            Card? best = null;
            bool bypass = engine.DoubleTroubleActive;
            foreach (var c in hand.Cards)
            {
                if (!bypass && !Rules.CanAttackWith(engine.Bout, c)) continue;
                if (best == null) { best = c; continue; }
                if (PrefersHigherAttack(c, best.Value, engine.Trump)) best = c;
            }
            return best;
        }

        static bool PrefersHigherAttack(Card candidate, Card current, Suit trump)
        {
            bool ct = candidate.Suit == trump;
            bool cu = current.Suit == trump;
            if (cu && !ct) return true;  // prefer non-trump
            if (ct && !cu) return false;
            return (int)candidate.Rank > (int)current.Rank; // higher is better
        }

        // --- Miser: plays lowest, conserves hand ---
        // (uses LowestLegalAttack + stops after 2 cards via ShouldStopAttacking)

        // --- Noble: saves trumps for endgame ---

        Card? NobleAttackPick(GameEngine engine, Hand hand)
        {
            Card? best = null;
            bool bypass = engine.DoubleTroubleActive;
            foreach (var c in hand.Cards)
            {
                if (!bypass && !Rules.CanAttackWith(engine.Bout, c)) continue;
                if (c.Suit == engine.Trump && engine.DeckCount >= 12) continue; // save trumps early
                if (best == null) { best = c; continue; }
                if (PrefersAsAttack(c, best.Value, engine.Trump)) best = c;
            }
            // fall back to any legal card if all are trump
            return best ?? LowestLegalAttack(engine, hand);
        }

        // --- Scholar: prefers ranks already in the bout (rank-locks) ---

        Card? ScholarAttackPick(GameEngine engine, Hand hand)
        {
            Card? rankMatch = null;
            Card? fallback = null;
            bool bypass = engine.DoubleTroubleActive;

            foreach (var c in hand.Cards)
            {
                if (!bypass && !Rules.CanAttackWith(engine.Bout, c)) continue;
                bool matchesBoutRank = BoutContainsRank(engine.Bout, c.Rank);

                if (matchesBoutRank)
                {
                    if (rankMatch == null || PrefersAsAttack(c, rankMatch.Value, engine.Trump))
                        rankMatch = c;
                }
                else
                {
                    if (fallback == null || PrefersAsAttack(c, fallback.Value, engine.Trump))
                        fallback = c;
                }
            }
            return rankMatch ?? fallback;
        }

        static bool BoutContainsRank(Bout bout, Rank rank)
        {
            foreach (var a in bout.Attacks)
                if (a.Rank == rank) return true;
            for (int i = 0; i < bout.Defenses.Count; i++)
                if (bout.Defenses[i] != null && bout.Defenses[i].Value.Rank == rank) return true;
            return false;
        }

        // --- Assassin: holds back early, then plays highest ---

        Card? AssassinAttackPick(GameEngine engine, Hand hand)
        {
            if (_boutsPlayed < 3)
                return LowestLegalAttack(engine, hand);
            return HighestLegalAttack(engine, hand);
        }

        // --- Fox: adapts to opponent hand size ---

        Card? FoxAttackPick(GameEngine engine, Hand hand, Hand oppHand)
        {
            if (oppHand.Count <= 3)
                return HighestLegalAttack(engine, hand);
            return LowestLegalAttack(engine, hand);
        }

        bool FoxShouldStop(GameEngine engine, Hand hand, Card next, Hand oppHand)
        {
            if (oppHand.Count <= 3) return false; // press the advantage
            return next.Suit == engine.Trump;
        }

        // --- Shared: lowest legal attack (used by Miser, default) ---

        Card? LowestLegalAttack(GameEngine engine, Hand hand)
        {
            Card? best = null;
            int bestScore = int.MinValue;
            bool bypass = engine.DoubleTroubleActive;
            foreach (var c in hand.Cards)
            {
                if (!bypass && !Rules.CanAttackWith(engine.Bout, c)) continue;
                int bonus = AttackAbilityBonus(c);
                if (bonus > 0)
                {
                    int score = bonus + (int)c.Rank;
                    if (best == null || score > bestScore) { best = c; bestScore = score; continue; }
                }
                if (best == null) { best = c; continue; }
                if (bestScore <= 0 && PrefersAsAttack(c, best.Value, engine.Trump)) best = c;
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

        // ---------- Defense step (archetype-specific) ----------

        void DefenseStep(GameEngine engine, int playerIndex)
        {
            int slot = engine.Bout.FirstUndefendedSlot();
            if (slot < 0) return;

            var hand = engine.HandOf(playerIndex);

            // Archetype-specific eat decisions before trying to defend
            if (ShouldEatInstead(engine, playerIndex, hand, slot))
            {
                engine.TryEat(playerIndex);
                return;
            }

            Card? defense = SelectDefenseCard(engine, hand, slot);

            if (defense != null && engine.TryDefend(playerIndex, slot, defense.Value))
                return;

            engine.TryEat(playerIndex);
        }

        bool ShouldEatInstead(GameEngine engine, int playerIndex, Hand hand, int slot)
        {
            return Archetype switch
            {
                // Brawler eats freely if hand is big and cost is low
                AIArchetypeName.Brawler => hand.Count >= 5 && CountUndefended(engine.Bout) <= 2
                    && !HasLegalDefense(engine, hand, slot),
                // Assassin eats in early bouts to build hand for the kill
                AIArchetypeName.Assassin => _boutsPlayed < 3 && CountUndefended(engine.Bout) >= 3
                    && engine.DeckCount > 6,
                // Fox eats strategically when hand is small and defenses are bad
                AIArchetypeName.Fox => hand.Count <= 3 && !HasGoodDefense(engine, hand, slot),
                _ => false,
            };
        }

        Card? SelectDefenseCard(GameEngine engine, Hand hand, int slot)
        {
            return Archetype switch
            {
                AIArchetypeName.Scholar => ScholarDefensePick(engine, hand, slot),
                _ => DefaultDefensePick(engine, hand, slot),
            };
        }

        // --- Scholar: won't overspend (avoids using high cards to beat low attacks) ---

        Card? ScholarDefensePick(GameEngine engine, Hand hand, int slot)
        {
            var attack = engine.Bout.Attacks[slot];
            var candidates = new List<Card>();
            foreach (var c in hand.Cards)
            {
                if (!Rules.CanDefendSlotWith(engine.Bout, slot, c, engine.Trump)) continue;
                // Don't use a card more than 3 ranks above the attack (wasteful)
                int rankGap = (int)c.Rank - (int)attack.Rank;
                if (c.Suit == engine.Trump && attack.Suit != engine.Trump)
                    rankGap = 4; // trump vs non-trump is always "expensive"
                if (rankGap > 3 && candidates.Count > 0) continue;
                candidates.Add(c);
            }

            candidates.Sort((a, b) => PrefersAsDefense(a, b, engine.Trump) ? -1 : 1);
            return candidates.Count > 0 ? candidates[0] : null;
        }

        // --- Default defense: lowest legal card ---

        Card? DefaultDefensePick(GameEngine engine, Hand hand, int slot)
        {
            var candidates = new List<Card>();
            foreach (var c in hand.Cards)
            {
                if (!Rules.CanDefendSlotWith(engine.Bout, slot, c, engine.Trump)) continue;
                candidates.Add(c);
            }
            candidates.Sort((a, b) =>
            {
                int bonusA = DefenseAbilityBonus(a), bonusB = DefenseAbilityBonus(b);
                if (bonusA != bonusB) return bonusB.CompareTo(bonusA);
                return PrefersAsDefense(a, b, engine.Trump) ? -1 : 1;
            });
            return candidates.Count > 0 ? candidates[0] : null;
        }

        bool HasLegalDefense(GameEngine engine, Hand hand, int slot)
        {
            foreach (var c in hand.Cards)
                if (Rules.CanDefendSlotWith(engine.Bout, slot, c, engine.Trump)) return true;
            return false;
        }

        bool HasGoodDefense(GameEngine engine, Hand hand, int slot)
        {
            var attack = engine.Bout.Attacks[slot];
            foreach (var c in hand.Cards)
            {
                if (!Rules.CanDefendSlotWith(engine.Bout, slot, c, engine.Trump)) continue;
                if (c.Suit != engine.Trump) return true; // non-trump defense is "good"
                if ((int)c.Rank - (int)attack.Rank <= 2) return true; // close trump is ok
            }
            return false;
        }

        static bool PrefersAsDefense(Card candidate, Card current, Suit trump)
        {
            bool ct = candidate.Suit == trump;
            bool cu = current.Suit == trump;
            if (cu && !ct) return true;
            if (ct && !cu) return false;
            return (int)candidate.Rank < (int)current.Rank;
        }

        static int AttackAbilityBonus(Card card)
        {
            if (!card.HasAbility || card.Trigger != TriggerTiming.OnAttack) return 0;
            return card.Ability.Value switch
            {
                AbilityType.ExtraDraw => 3,
                AbilityType.DoubleTrouble => 4,
                AbilityType.PileOn => 3,
                AbilityType.Feint => 2,
                AbilityType.Rampage => 5,
                AbilityType.Haymaker => 3,
                AbilityType.Conquer => 3,
                AbilityType.Intimidate => 4,
                AbilityType.RoyalDecree => 3,
                _ => 2,
            };
        }

        static int DefenseAbilityBonus(Card card)
        {
            if (!card.HasAbility || card.Trigger != TriggerTiming.OnDefend) return 0;
            return card.Ability.Value switch
            {
                AbilityType.Blocker => 4,
                AbilityType.DoubleDefense => 5,
                AbilityType.Deflect => 4,
                AbilityType.SlipAway => 3,
                AbilityType.ShadowCloak => 3,
                AbilityType.Riposte => 4,
                AbilityType.IronGrip => 3,
                AbilityType.SmokeBomb => 3,
                AbilityType.Fortify => 2,
                AbilityType.Brace => 2,
                _ => 2,
            };
        }

        static int CardAttackScore(Card c, Suit trump)
        {
            int score = (int)c.Rank;
            if (c.Suit == trump) score -= 10;
            score += AttackAbilityBonus(c);
            return score;
        }

        static int CardDefenseScore(Card c, Suit trump)
        {
            int score = -(int)c.Rank;
            if (c.Suit == trump) score -= 5;
            score += DefenseAbilityBonus(c);
            return score;
        }
    }
}
