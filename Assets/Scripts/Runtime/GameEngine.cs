using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public enum Phase
    {
        Setup,
        Attack,         // attacker may add an attack card (or end the bout if at least one is defended)
        Defense,        // defender must beat the next undefended attack, or eat
        Resolving,      // brief moment between actions while UI animates
        GameOver
    }

    public enum BoutOutcome
    {
        DefenderWonAllDiscarded,
        DefenderAteCards
    }

    public sealed class GameEngine
    {
        // ----- State -----
        readonly Deck _deck;
        readonly Hand[] _hands;
        readonly List<Card> _discard = new();
        readonly Bout _bout = new();
        public Suit Trump { get; private set; }
        public Card TrumpCard { get; private set; }     // the visible bottom card
        bool _trumpStillInDeck = true;
        bool _trumpChangerUsed;
        public int AttackerIndex { get; private set; }
        public int DefenderIndex => 1 - AttackerIndex;  // 2-player only
        public Phase Phase { get; private set; } = Phase.Setup;
        public int? WinnerIndex { get; private set; }   // index of the player who emptied first; loser is the Fool
        public int? FoolIndex { get; private set; }

        // ----- Read-only accessors -----
        public Bout Bout => _bout;
        public IReadOnlyList<Card> Discard => _discard;
        public int DeckCount => _deck.Count;
        public int PlayerCount => _hands.Length;
        public Hand HandOf(int playerIndex) => _hands[playerIndex];

        // ----- Events (UI/AI subscribe) -----
        public event Action OnSetupComplete;
        public event Action<int> OnTurnBegan;             // playerIndex now attacking
        public event Action<int, Card> OnAttackPlayed;    // attackerIndex, card
        public event Action<int, int, Card> OnDefensePlayed; // defenderIndex, slot, card
        public event Action<BoutOutcome> OnBoutResolved;
        public event Action<int, int> OnDrew;             // playerIndex, drawnCount
        public event Action<int> OnGameOver;              // foolIndex
        public event Action<int, Card, AbilityType> OnAbilityUsed; // playerIndex, card, ability
        public event Action<Suit> OnTrumpChanged;                    // newTrumpSuit
        public bool TrumpChangerUsed => _trumpChangerUsed;

        public GameEngine(int? seed = null, IReadOnlyDictionary<(Suit, Rank), AbilityType> abilities = null)
        {
            _deck = new Deck(seed, abilities ?? DeckConfig.DefaultAbilities);
            _hands = new[] { new Hand(), new Hand() };
        }

        public void StartNewGame()
        {
            _discard.Clear();
            _bout.Clear();
            _hands[0].Clear();
            _hands[1].Clear();
            WinnerIndex = null;
            FoolIndex = null;
            _trumpStillInDeck = true;
            _trumpChangerUsed = false;

            _deck.Shuffle();

            for (int i = 0; i < Rules.HandSizeTwoPlayer; i++)
            {
                _hands[0].Add(_deck.Draw());
                _hands[1].Add(_deck.Draw());
            }

            // Trump = bottom card of remaining deck (visible to both players).
            TrumpCard = _deck.PeekBottom();
            Trump = TrumpCard.Suit;

            AttackerIndex = ChooseFirstAttacker();
            Phase = Phase.Attack;

            OnSetupComplete?.Invoke();
            OnTurnBegan?.Invoke(AttackerIndex);
        }

        int ChooseFirstAttacker()
        {
            int? lowest0 = LowestTrumpRank(_hands[0]);
            int? lowest1 = LowestTrumpRank(_hands[1]);
            if (lowest0.HasValue && lowest1.HasValue)
                return lowest0.Value <= lowest1.Value ? 0 : 1;
            if (lowest0.HasValue) return 0;
            if (lowest1.HasValue) return 1;
            return 0; // neither has trump (rare with 6-card hands and 36-card deck); default to player 0
        }

        int? LowestTrumpRank(Hand h)
        {
            int? best = null;
            foreach (var c in h.Cards)
            {
                if (c.Suit != Trump) continue;
                if (!best.HasValue || (int)c.Rank < best.Value) best = (int)c.Rank;
            }
            return best;
        }

        // ---------- Actions ----------

        public bool TryAttack(int playerIndex, Card card)
        {
            if (Phase != Phase.Attack) return false;
            if (playerIndex != AttackerIndex) return false;
            if (!_hands[playerIndex].Contains(card)) return false;
            if (!Rules.CanAttackWith(_bout, card)) return false;
            // Can't add an attack the defender has no cards to cover.
            if (_bout.AttackCount - CountDefended() >= _hands[DefenderIndex].Count) return false;
            // Hard cap: max attacks per bout (rules say defender's starting hand size).
            if (_bout.AttackCount >= Rules.MaxAttacksPerBout) return false;

            _hands[playerIndex].Remove(card);
            _bout.AddAttack(card);
            // Phase must flip BEFORE firing the event: handlers (UpdateHud,
            // ApplyHighlightForPhase) read Engine.Phase synchronously and would
            // otherwise see the pre-attack Phase, leaving the UI stuck.
            Phase = Phase.Defense;
            OnAttackPlayed?.Invoke(playerIndex, card);
            return true;
        }

        public bool TryDefend(int playerIndex, int slot, Card card)
        {
            if (Phase != Phase.Defense) return false;
            if (playerIndex != DefenderIndex) return false;
            if (!_hands[playerIndex].Contains(card)) return false;
            if (!Rules.CanDefendSlotWith(_bout, slot, card, Trump)) return false;

            _hands[playerIndex].Remove(card);
            _bout.TryDefend(slot, card);
            // After defense, control returns to attacker who may add another card or end bout.
            // Set Phase before the event so handlers read the new state.
            Phase = Phase.Attack;
            OnDefensePlayed?.Invoke(playerIndex, slot, card);
            return true;
        }

        // Defender chooses to take all bout cards into their hand instead of defending.
        public bool TryEat(int playerIndex)
        {
            if (Phase != Phase.Defense && Phase != Phase.Attack) return false;
            if (playerIndex != DefenderIndex) return false;
            if (_bout.IsEmpty) return false;

            foreach (var c in _bout.AllCards())
                _hands[playerIndex].Add(c);
            _bout.Clear();

            ResolveBout(BoutOutcome.DefenderAteCards);
            return true;
        }

        // Attacker stops adding cards. Only valid if at least one attack exists and all are defended.
        public bool TryEndBout(int playerIndex)
        {
            if (Phase != Phase.Attack) return false;
            if (playerIndex != AttackerIndex) return false;
            if (_bout.IsEmpty) return false;
            if (!_bout.FullyDefended) return false;

            foreach (var c in _bout.AllCards()) _discard.Add(c);
            _bout.Clear();

            ResolveBout(BoutOutcome.DefenderWonAllDiscarded);
            return true;
        }

        // Activate a card's ability instead of playing it normally.
        // The card is consumed (removed from hand, discarded).
        // defenseSlot is used by defense-phase abilities (Double Defense, Blocker).
        public bool TryUseAbility(int playerIndex, Card card, int defenseSlot = -1)
        {
            if (Phase != Phase.Attack && Phase != Phase.Defense) return false;
            int active = Phase == Phase.Defense ? DefenderIndex : AttackerIndex;
            if (playerIndex != active) return false;
            if (!_hands[playerIndex].Contains(card)) return false;
            if (!card.HasAbility) return false;

            var ability = card.Ability.Value;

            if (!ValidateAbility(ability)) return false;

            _hands[playerIndex].Remove(card);
            _discard.Add(card);
            ApplyAbility(ability, playerIndex, card, defenseSlot);
            OnAbilityUsed?.Invoke(playerIndex, card, ability);
            return true;
        }

        bool ValidateAbility(AbilityType ability)
        {
            switch (ability)
            {
                case AbilityType.TrumpChanger:
                    return !_trumpChangerUsed;
                case AbilityType.ExtraDraw:
                    return Phase == Phase.Attack && _deck.Count > 0;
                default:
                    return true;
            }
        }

        void ApplyAbility(AbilityType ability, int playerIndex, Card card, int defenseSlot)
        {
            switch (ability)
            {
                case AbilityType.TrumpChanger:
                    Trump = card.Suit;
                    _trumpChangerUsed = true;
                    OnTrumpChanged?.Invoke(Trump);
                    break;
                case AbilityType.ExtraDraw:
                    int target = _hands[DefenderIndex].Count + 2;
                    DrawTo(DefenderIndex, target);
                    break;
            }
        }

        int CountDefended()
        {
            int n = 0;
            for (int i = 0; i < _bout.Defenses.Count; i++)
                if (_bout.Defenses[i] != null) n++;
            return n;
        }

        void ResolveBout(BoutOutcome outcome)
        {
            Phase = Phase.Resolving;
            OnBoutResolved?.Invoke(outcome);

            // Refill draw order: attacker first, then defender.
            int attackerBefore = AttackerIndex;
            int defenderBefore = DefenderIndex;

            DrawTo(attackerBefore, Rules.HandSizeTwoPlayer);
            DrawTo(defenderBefore, Rules.HandSizeTwoPlayer);

            if (CheckGameOver(attackerBefore, defenderBefore, outcome)) return;

            // Switch attacker on a successful defense; same attacker continues if defender ate.
            AttackerIndex = outcome == BoutOutcome.DefenderWonAllDiscarded ? defenderBefore : attackerBefore;

            Phase = Phase.Attack;
            OnTurnBegan?.Invoke(AttackerIndex);
        }

        void DrawTo(int playerIndex, int target)
        {
            int drawn = 0;
            while (_hands[playerIndex].Count < target && _deck.Count > 0)
            {
                if (_trumpStillInDeck && _deck.Count == 1) _trumpStillInDeck = false;
                _hands[playerIndex].Add(_deck.Draw());
                drawn++;
            }
            if (drawn > 0) OnDrew?.Invoke(playerIndex, drawn);
        }

        bool CheckGameOver(int attackerBefore, int defenderBefore, BoutOutcome outcome)
        {
            // After draws, if the deck is empty and a player has zero cards, they're out.
            // The remaining player with cards is the Fool.
            bool deckEmpty = _deck.Count == 0;
            if (!deckEmpty) return false;

            bool empty0 = _hands[0].Count == 0;
            bool empty1 = _hands[1].Count == 0;

            if (empty0 && empty1)
            {
                // Both empty on the same beat — the player who *just* succeeded wins.
                // On a successful defense (DefenderWon), the defender finished clean.
                // On Eat, the defender took cards so they can't be empty here, but cover anyway.
                int winner = outcome == BoutOutcome.DefenderWonAllDiscarded ? defenderBefore : attackerBefore;
                EndGame(fool: 1 - winner);
                return true;
            }
            if (empty0) { EndGame(fool: 1); return true; }
            if (empty1) { EndGame(fool: 0); return true; }
            return false;
        }

        void EndGame(int fool)
        {
            FoolIndex = fool;
            WinnerIndex = 1 - fool;
            Phase = Phase.GameOver;
            OnGameOver?.Invoke(fool);
        }
    }
}
