using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public enum Phase
    {
        Setup,
        Attack,
        Defense,
        Resolving,
        GameOver
    }

    public enum BoutOutcome
    {
        DefenderWonAllDiscarded,
        DefenderAteCards
    }

    public sealed class GameEngine
    {
        readonly Deck _deck;
        readonly Hand[] _hands;
        readonly List<Card> _discard = new();
        readonly Bout _bout = new();
        readonly MatchConfig _config;

        public Suit Trump { get; private set; }
        public Card TrumpCard { get; private set; }
        bool _trumpStillInDeck = true;
        bool _trumpChangerUsed;
        int? _seizeInitiativePlayer;
        bool _doubleTroubleActive;
        int _pileOnBonus;
        int _boutCount;
        bool[] _duelistGloveUsedThisBout = new bool[2];
        bool[] _shieldBroochUsed = new bool[2];
        bool[] _courtiersFanUsed = new bool[2];
        bool[] _clumsyFingersTriggered = new bool[2];
        bool[] _quicksilverUsed = new bool[2];
        bool[] _slipAwayUsed = new bool[2];
        bool[] _jugglersBallsUsed = new bool[2];
        readonly List<Rank> _pendingBonusRanks = new();

        public int AttackerIndex { get; private set; }
        public int DefenderIndex => 1 - AttackerIndex;
        public Phase Phase { get; private set; } = Phase.Setup;
        public int? WinnerIndex { get; private set; }
        public int? FoolIndex { get; private set; }

        public Bout Bout => _bout;
        public IReadOnlyList<Card> Discard => _discard;
        public int DeckCount => _deck.Count;
        public Card? DeckTopCard => _deck.Count > 0 ? _deck.PeekTop(1)[0] : null;

        public List<Card> GetMarkedCards(int playerIndex, int count)
        {
            int opponent = 1 - playerIndex;
            var hand = _hands[opponent];
            var result = new List<Card>();
            if (hand.Count == 0) return result;
            var indices = new List<int>();
            for (int i = 0; i < hand.Count; i++) indices.Add(i);
            int picks = System.Math.Min(count, hand.Count);
            for (int i = 0; i < picks; i++)
            {
                int idx = _deck.Count > 0 ? (_deck.Count + i) % indices.Count : i % indices.Count;
                result.Add(hand.Cards[indices[idx]]);
                indices.RemoveAt(idx);
            }
            return result;
        }
        public int PlayerCount => _hands.Length;
        public Hand HandOf(int playerIndex) => _hands[playerIndex];
        public MatchConfig Config => _config;
        public int BoutCount => _boutCount;

        public event Action OnSetupComplete;
        public event Action<int> OnTurnBegan;
        public event Action<int, Card> OnAttackPlayed;
        public event Action<int, int, Card> OnDefensePlayed;
        public event Action<BoutOutcome> OnBoutResolved;
        public event Action<int, int> OnDrew;
        public event Action<int> OnGameOver;
        public event Action<int, Card, AbilityType> OnAbilityUsed;
        public event Action<Suit> OnTrumpChanged;

        public bool TrumpChangerUsed => _trumpChangerUsed;
        public bool DoubleTroubleActive => _doubleTroubleActive;

        public GameEngine(int? seed = null, IReadOnlyDictionary<(Suit, Rank), AbilityType> abilities = null)
            : this(seed, new MatchConfig { Abilities = abilities != null ? new Dictionary<(Suit, Rank), AbilityType>(abilities) : DeckConfig.DefaultAbilities })
        { }

        public GameEngine(int? seed, MatchConfig config)
        {
            _config = config ?? MatchConfig.Default();
            _deck = new Deck(seed, _config.Abilities);
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
            _seizeInitiativePlayer = null;
            _doubleTroubleActive = false;
            _pileOnBonus = 0;
            _boutCount = 0;
            _duelistGloveUsedThisBout = new bool[2];
            _shieldBroochUsed = new bool[2];
            _courtiersFanUsed = new bool[2];
            _clumsyFingersTriggered = new bool[2];

            _deck.Shuffle();
            _quicksilverUsed = new bool[2];
            _slipAwayUsed = new bool[2];
            _jugglersBallsUsed = new bool[2];

            for (int p = 0; p < 2; p++)
            {
                if (!_config.LoadedDice[p]) continue;
                var topCards = _deck.PeekTop(3);
                if (topCards.Length > 1)
                {
                    Array.Sort(topCards, (a, b) =>
                    {
                        bool at = a.Suit == TrumpCard.Suit, bt = b.Suit == TrumpCard.Suit;
                        if (at != bt) return at ? 1 : -1;
                        return (int)a.Rank - (int)b.Rank;
                    });
                    _deck.ReplaceTop(topCards);
                }
                break;
            }

            int handSize = _config.HandSize;
            for (int i = 0; i < handSize; i++)
            {
                if (_deck.Count > 0) _hands[0].Add(_deck.Draw());
                if (_deck.Count > 0) _hands[1].Add(_deck.Draw());
            }

            TrumpCard = _deck.PeekBottom();
            if (_config.ForcedTrumpSuit.HasValue)
                Trump = (Suit)_config.ForcedTrumpSuit.Value;
            else
                Trump = TrumpCard.Suit;

            for (int p = 0; p < 2; p++)
            {
                if (_config.FoolsGold[p])
                    _hands[p].Add(new Card(Trump, Rank.Seven, null));
            }

            ApplyVentriloquistsDummy();

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
            return 0;
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

        void ApplyVentriloquistsDummy()
        {
            for (int p = 0; p < 2; p++)
            {
                if (!_config.VentriloquistsDummy[p]) continue;
                int opponent = 1 - p;
                var opponentAbilities = new List<AbilityType>();
                if (_config.Abilities == null || _config.AbilityOwners == null) continue;
                foreach (var kv in _config.AbilityOwners)
                {
                    if (kv.Value == opponent && _config.Abilities.ContainsKey(kv.Key))
                        opponentAbilities.Add(_config.Abilities[kv.Key]);
                }
                if (opponentAbilities.Count == 0) continue;

                var pick = opponentAbilities[_deck.Count % opponentAbilities.Count];
                for (int i = 0; i < _hands[p].Cards.Count; i++)
                {
                    var card = _hands[p].Cards[i];
                    if (card.HasAbility) continue;
                    var newCard = new Card(card.Suit, card.Rank, pick);
                    _hands[p].Remove(card);
                    _hands[p].Add(newCard);
                    _config.Abilities[(card.Suit, card.Rank)] = pick;
                    if (_config.AbilityOwners != null)
                        _config.AbilityOwners[(card.Suit, card.Rank)] = p;
                    break;
                }
            }
        }

        // ---------- Actions ----------

        public bool TryAttack(int playerIndex, Card card)
        {
            if (Phase != Phase.Attack) return false;
            if (playerIndex != AttackerIndex) return false;
            if (!_hands[playerIndex].Contains(card)) return false;
            if (_bout.AttacksCapped) return false;

            if (_config.NoTrumpsUntilBout > 0 && _boutCount < _config.NoTrumpsUntilBout && card.Suit == Trump)
                return false;

            bool rankBypass = _doubleTroubleActive;
            if (!rankBypass && _config.DuelistGlove[playerIndex] && !_duelistGloveUsedThisBout[playerIndex])
                rankBypass = true;
            if (!rankBypass && _config.AnyRankAttack)
                rankBypass = true;
            if (!rankBypass && !Rules.CanAttackWith(_bout, card)) return false;

            if (_bout.AttackCount - CountDefended() >= _hands[DefenderIndex].Count) return false;
            int maxAttacks = _config.MaxAttacksPerBout + _pileOnBonus;
            if (_bout.AttackCount >= maxAttacks) return false;

            _hands[playerIndex].Remove(card);
            _bout.AddAttack(card);

            if (_doubleTroubleActive) _doubleTroubleActive = false;
            else if (_config.DuelistGlove[playerIndex] && !_duelistGloveUsedThisBout[playerIndex] && rankBypass)
                _duelistGloveUsedThisBout[playerIndex] = true;

            Phase = Phase.Defense;
            OnAttackPlayed?.Invoke(playerIndex, card);
            TryShieldBrooch();
            return true;
        }

        void TryShieldBrooch()
        {
            int defender = DefenderIndex;
            if (!_config.ShieldBrooch[defender] || _shieldBroochUsed[defender]) return;
            int slot = _bout.FirstUndefendedSlot();
            if (slot < 0) return;
            _shieldBroochUsed[defender] = true;
            var attack = _bout.Attacks[slot];
            var ghost = new Card(attack.Suit, attack.Rank == Rank.Ace ? Rank.Ace : attack.Rank + 1, null);
            if (ghost.Suit != Trump && attack.Suit != Trump)
                ghost = new Card(Trump, Rank.Six, null);
            _bout.TryDefend(slot, ghost);
            if (_bout.FullyDefended) Phase = Phase.Attack;
            OnDefensePlayed?.Invoke(defender, slot, ghost);
        }

        public bool TryDefend(int playerIndex, int slot, Card card)
        {
            if (Phase != Phase.Defense) return false;
            if (playerIndex != DefenderIndex) return false;
            if (!_hands[playerIndex].Contains(card)) return false;

            bool canDefend;
            if (_config.EndgameSpecialist[playerIndex] && _deck.Count <= 6)
                canDefend = (int)card.Rank > (int)_bout.Attacks[slot].Rank || (card.Suit == Trump && _bout.Attacks[slot].Suit != Trump);
            else if (_config.HereticsBrand[playerIndex] && _bout.Attacks[slot].Suit == Trump)
                canDefend = Rules.CanDefendSlotWith(_bout, slot, card, Trump) && (int)card.Rank > (int)_bout.Attacks[slot].Rank;
            else
                canDefend = Rules.CanDefendSlotWith(_bout, slot, card, Trump);

            if (!canDefend) return false;
            if (_config.NoTrumpsUntilBout > 0 && _boutCount < _config.NoTrumpsUntilBout && card.Suit == Trump)
                return false;

            _hands[playerIndex].Remove(card);
            _bout.TryDefend(slot, card);
            Phase = Phase.Attack;
            OnDefensePlayed?.Invoke(playerIndex, slot, card);
            return true;
        }

        public bool TryEat(int playerIndex)
        {
            if (Phase != Phase.Defense && Phase != Phase.Attack) return false;
            if (playerIndex != DefenderIndex) return false;
            if (_bout.IsEmpty) return false;

            CollectCrownOfThornsRanks();
            foreach (var c in _bout.AllCards())
                _hands[playerIndex].Add(c);
            _bout.Clear();

            if (_config.PoisonedWine[1 - playerIndex])
            {
                int drawn = 0;
                while (drawn < 2 && _deck.Count > 0)
                {
                    _hands[playerIndex].Add(_deck.Draw());
                    drawn++;
                }
                if (drawn > 0) OnDrew?.Invoke(playerIndex, drawn);
            }

            if (_config.EatDrawsExtra)
            {
                int drawn = 0;
                while (drawn < 2 && _deck.Count > 0)
                {
                    _hands[playerIndex].Add(_deck.Draw());
                    drawn++;
                }
                if (drawn > 0) OnDrew?.Invoke(playerIndex, drawn);
            }

            ResolveBout(BoutOutcome.DefenderAteCards);
            return true;
        }

        public bool TryEndBout(int playerIndex)
        {
            if (Phase != Phase.Attack) return false;
            if (playerIndex != AttackerIndex) return false;
            if (_bout.IsEmpty) return false;
            if (!_bout.FullyDefended) return false;

            CollectCrownOfThornsRanks();
            foreach (var c in _bout.AllCards()) _discard.Add(c);
            _bout.Clear();

            ResolveBout(BoutOutcome.DefenderWonAllDiscarded);
            return true;
        }

        public bool TryUseAbility(int playerIndex, Card card, int defenseSlot = -1)
        {
            if (Phase != Phase.Attack && Phase != Phase.Defense) return false;
            int active = Phase == Phase.Defense ? DefenderIndex : AttackerIndex;
            if (playerIndex != active) return false;
            if (!_hands[playerIndex].Contains(card)) return false;
            if (!card.HasAbility) return false;

            if (_config.AbilityOwners != null &&
                _config.AbilityOwners.TryGetValue((card.Suit, card.Rank), out int owner) &&
                owner != playerIndex)
                return false;

            var ability = card.Ability.Value;
            if (ability.IsPassive()) return false;

            if (_config.ClumsyFingers[playerIndex] && !_clumsyFingersTriggered[playerIndex])
            {
                _clumsyFingersTriggered[playerIndex] = true;
                _hands[playerIndex].Remove(card);
                _discard.Add(card);
                OnAbilityUsed?.Invoke(playerIndex, card, ability);
                return true;
            }

            if (!ValidateAbility(ability, card, defenseSlot, playerIndex)) return false;

            bool keepCard = _config.QuicksilverVial[playerIndex] && !_quicksilverUsed[playerIndex];
            if (keepCard)
                _quicksilverUsed[playerIndex] = true;
            else
            {
                _hands[playerIndex].Remove(card);
                _discard.Add(card);
            }
            ApplyAbility(ability, playerIndex, card, defenseSlot);
            OnAbilityUsed?.Invoke(playerIndex, card, ability);
            return true;
        }

        bool ValidateAbility(AbilityType ability, Card card, int defenseSlot, int playerIndex)
        {
            switch (ability)
            {
                case AbilityType.TrumpChanger:
                    return !_trumpChangerUsed;
                case AbilityType.ExtraDraw:
                    return Phase == Phase.Attack && _deck.Count > 0;
                case AbilityType.DoubleTrouble:
                    return Phase == Phase.Attack;
                case AbilityType.Blocker:
                    return Phase == Phase.Defense;
                case AbilityType.DoubleDefense:
                    int ddSlot = defenseSlot >= 0 ? defenseSlot : _bout.FirstUndefendedSlot();
                    return Phase == Phase.Defense && ddSlot >= 0 && Rules.Beats(card, _bout.Attacks[ddSlot], Trump);
                case AbilityType.SeizeInitiative:
                    return true;
                case AbilityType.PileOn:
                    return Phase == Phase.Attack;
                case AbilityType.Feint:
                    return Phase == Phase.Attack && _deck.Count > 0 && _bout.AttackCount < (_config.MaxAttacksPerBout + _pileOnBonus);
                case AbilityType.Deflect:
                    return Phase == Phase.Defense && _bout.FirstUndefendedSlot() >= 0;
                case AbilityType.SlipAway:
                    return Phase == Phase.Defense && _bout.FirstUndefendedSlot() >= 0 && !_slipAwayUsed[playerIndex];
                case AbilityType.Peek:
                    return _deck.Count > 0;
                case AbilityType.Gambit:
                    return _deck.Count > 0;
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
                    int edTarget = _hands[DefenderIndex].Count + 2;
                    DrawTo(DefenderIndex, edTarget);
                    break;

                case AbilityType.Blocker:
                    _bout.AttacksCapped = true;
                    break;

                case AbilityType.SeizeInitiative:
                    _seizeInitiativePlayer = playerIndex;
                    break;

                case AbilityType.DoubleTrouble:
                    _doubleTroubleActive = true;
                    break;

                case AbilityType.DoubleDefense:
                    _discard.Remove(card);
                    int slot1 = defenseSlot >= 0 ? defenseSlot : _bout.FirstUndefendedSlot();
                    if (slot1 >= 0)
                    {
                        _bout.TryDefend(slot1, card);
                        Phase = Phase.Attack;
                        OnDefensePlayed?.Invoke(playerIndex, slot1, card);
                    }
                    int slot2 = _bout.FirstUndefendedSlot();
                    if (slot2 >= 0)
                        _bout.AutoDefend(slot2);
                    break;

                case AbilityType.PileOn:
                    _pileOnBonus += 2;
                    break;

                case AbilityType.Feint:
                    var feintCard = _deck.Draw();
                    _bout.AddAttack(feintCard);
                    Phase = Phase.Defense;
                    OnAttackPlayed?.Invoke(playerIndex, feintCard);
                    break;

                case AbilityType.Deflect:
                    AttackerIndex = 1 - AttackerIndex;
                    Phase = Phase.Defense;
                    break;

                case AbilityType.SlipAway:
                    _slipAwayUsed[playerIndex] = true;
                    for (int i = 0; i < _bout.Attacks.Count; i++)
                    {
                        if (_bout.Defenses[i] == null)
                            _discard.Add(_bout.Attacks[i]);
                        else
                        {
                            _discard.Add(_bout.Attacks[i]);
                            _discard.Add(_bout.Defenses[i].Value);
                        }
                    }
                    CollectCrownOfThornsRanks();
                    _bout.Clear();
                    ResolveBout(BoutOutcome.DefenderWonAllDiscarded);
                    break;

                case AbilityType.Peek:
                    var top = _deck.PeekTop(3);
                    if (top.Length > 1)
                    {
                        // Put easiest-to-shed cards on top: non-trump low rank first
                        Array.Sort(top, (a, b) =>
                        {
                            bool at = a.Suit == Trump, bt = b.Suit == Trump;
                            if (at != bt) return at ? 1 : -1;
                            return (int)a.Rank - (int)b.Rank;
                        });
                        _deck.ReplaceTop(top);
                    }
                    break;

                case AbilityType.Gambit:
                    int count = _hands[playerIndex].Count;
                    var toDiscard = new List<Card>(_hands[playerIndex].Cards);
                    foreach (var c in toDiscard)
                    {
                        _hands[playerIndex].Remove(c);
                        _discard.Add(c);
                    }
                    int toDraw = Math.Min(count, _deck.Count);
                    for (int i = 0; i < toDraw; i++)
                        _hands[playerIndex].Add(_deck.Draw());
                    if (toDraw > 0) OnDrew?.Invoke(playerIndex, toDraw);
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

        void CollectCrownOfThornsRanks()
        {
            _pendingBonusRanks.Clear();
            for (int p = 0; p < 2; p++)
            {
                if (!_config.CrownOfThorns[p]) continue;
                foreach (var d in _bout.Defenses)
                    if (d.HasValue && d.Value.Suit == Trump)
                        _pendingBonusRanks.Add(d.Value.Rank);
            }
        }

        void ResolveBout(BoutOutcome outcome)
        {
            Phase = Phase.Resolving;
            _boutCount++;
            OnBoutResolved?.Invoke(outcome);

            int attackerBefore = AttackerIndex;
            int defenderBefore = DefenderIndex;

            DrawTo(attackerBefore, _config.HandSize);
            DrawTo(defenderBefore, _config.HandSize);

            if (outcome == BoutOutcome.DefenderWonAllDiscarded)
            {
                for (int p = 0; p < 2; p++)
                {
                    if (_config.QuickHands[p] && _deck.Count > 0)
                    {
                        _hands[p].Add(_deck.Draw());
                        if (_hands[p].Count > 1)
                        {
                            var worst = FindWorstCard(_hands[p], Trump);
                            if (worst.HasValue)
                            {
                                _hands[p].Remove(worst.Value);
                                _discard.Add(worst.Value);
                            }
                        }
                    }
                }
            }

            for (int p = 0; p < 2; p++)
            {
                if (_config.JugglersBalls[p] && !_jugglersBallsUsed[p] && _boutCount == 1 && _hands[p].Count > 1)
                {
                    _jugglersBallsUsed[p] = true;
                    var worst = FindWorstCard(_hands[p], Trump);
                    if (worst.HasValue)
                    {
                        _hands[p].Remove(worst.Value);
                        _discard.Add(worst.Value);
                    }
                }
            }

            _doubleTroubleActive = false;
            _pileOnBonus = 0;
            _duelistGloveUsedThisBout = new bool[2];

            if (CheckGameOver(attackerBefore, defenderBefore, outcome)) return;

            if (_config.FixedAttacker)
            {
                // Attacker never rotates
            }
            else if (_seizeInitiativePlayer.HasValue)
            {
                AttackerIndex = _seizeInitiativePlayer.Value;
                _seizeInitiativePlayer = null;
            }
            else
            {
                AttackerIndex = outcome == BoutOutcome.DefenderWonAllDiscarded ? defenderBefore : attackerBefore;
            }

            foreach (var r in _pendingBonusRanks)
                _bout.AddBonusRank(r);
            _pendingBonusRanks.Clear();

            Phase = Phase.Attack;
            OnTurnBegan?.Invoke(AttackerIndex);
        }

        void DrawTo(int playerIndex, int target)
        {
            if (_config.CourtiersFan[playerIndex] && !_courtiersFanUsed[playerIndex] && _hands[playerIndex].Count < target)
            {
                _courtiersFanUsed[playerIndex] = true;
                target = Math.Max(target - 1, _hands[playerIndex].Count);
            }

            int drawn = 0;
            while (_hands[playerIndex].Count < target && _deck.Count > 0)
            {
                if (_trumpStillInDeck && _deck.Count == 1) _trumpStillInDeck = false;
                var drawnCard = _deck.Draw();
                _hands[playerIndex].Add(drawnCard);
                drawn++;

                if (_config.TrumpAffinity[playerIndex] && drawnCard.Suit == Trump && _deck.Count > 0)
                {
                    _hands[playerIndex].Add(_deck.Draw());
                    drawn++;
                    var worst = FindWorstCard(_hands[playerIndex], Trump);
                    if (worst.HasValue)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        _discard.Add(worst.Value);
                        drawn--;
                    }
                }
            }
            if (drawn > 0 && _config.CardCounter[playerIndex] && _deck.Count > 0)
            {
                var peeked = _deck.PeekTop(1);
                var worst = FindWorstCard(_hands[playerIndex], Trump);
                if (worst.HasValue && peeked.Length > 0)
                {
                    bool pBetter = IsBetterDraw(peeked[0], worst.Value, Trump);
                    if (pBetter)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        _hands[playerIndex].Add(_deck.Draw());
                        _discard.Add(worst.Value);
                    }
                }
            }
            if (drawn > 0) OnDrew?.Invoke(playerIndex, drawn);
        }

        static bool IsBetterDraw(Card candidate, Card current, Suit trump)
        {
            bool ct = candidate.Suit == trump, cu = current.Suit == trump;
            if (cu && !ct) return true;
            if (ct && !cu) return false;
            return (int)candidate.Rank < (int)current.Rank;
        }

        static Card? FindWorstCard(Hand hand, Suit trump)
        {
            Card? worst = null;
            foreach (var c in hand.Cards)
            {
                if (worst == null) { worst = c; continue; }
                bool cTrump = c.Suit == trump;
                bool wTrump = worst.Value.Suit == trump;
                if (wTrump && !cTrump) { worst = c; continue; }
                if (cTrump && !wTrump) continue;
                if ((int)c.Rank < (int)worst.Value.Rank) worst = c;
            }
            return worst;
        }

        bool CheckGameOver(int attackerBefore, int defenderBefore, BoutOutcome outcome)
        {
            bool deckEmpty = _deck.Count == 0;
            if (!deckEmpty) return false;

            bool empty0 = _hands[0].Count == 0;
            bool empty1 = _hands[1].Count == 0;

            if (empty0 && empty1)
            {
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
