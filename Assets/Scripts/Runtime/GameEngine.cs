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
        readonly PlayerDeck[] _playerDecks;
        readonly List<Card> _discard = new();
        readonly int _seed;
        readonly Hand[] _hands;
        readonly List<Card>[] _playerDiscards;
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
        bool[] _luckyDrawUsed = new bool[2];
        readonly int[] _resource = new int[2];
        bool[] _attackedThisBout = new bool[2];
        bool[] _usedAbilityThisBout = new bool[2];
        bool[] _shadowReflexesUsedThisBout = new bool[2];
        readonly List<Rank> _pendingBonusRanks = new();

        public int AttackerIndex { get; private set; }
        public int DefenderIndex => 1 - AttackerIndex;
        public Phase Phase { get; private set; } = Phase.Setup;
        public int? WinnerIndex { get; private set; }
        public int? FoolIndex { get; private set; }

        public Bout Bout => _bout;
        public int DeckCount => AnyDeckCount();
        public bool IsDualDeck => true;
        public int DeckCountOf(int playerIndex) => DeckCountFor(playerIndex);
        public Card? DeckTopCard => AnyDeckCount() > 0 ? PeekTopDeck(AttackerIndex, 1)[0] : null;
        public Card[] PeekTopCards(int playerIndex, int count) => PeekTopDeck(playerIndex, count);

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
                int idx = AnyDeckCount() > 0 ? (PseudoRandomSeed() + i) % indices.Count : i % indices.Count;
                result.Add(hand.Cards[indices[idx]]);
                indices.RemoveAt(idx);
            }
            return result;
        }
        public int PlayerCount => _hands.Length;
        public Hand HandOf(int playerIndex) => _hands[playerIndex];
        public MatchConfig Config => _config;
        public int BoutCount => _boutCount;
        public int HandCount(int playerIndex) => _hands[playerIndex].Count;
        public int UndefendedCount => _bout.AttackCount - CountDefended();

        public event Action OnSetupComplete;
        public event Action<int> OnTurnBegan;
        public event Action<int, Card> OnAttackPlayed;
        public event Action<int, int, Card> OnDefensePlayed;
        public event Action<BoutOutcome> OnBoutResolved;
        public event Action<int, int> OnDrew;
        public event Action<int> OnGameOver;
        public event Action<int, Card, AbilityType> OnAbilityUsed;
        public event Action<Suit> OnTrumpChanged;
        public event Action<int, ResourceType, int> OnResourceChanged;
        public event Action<int, int> OnDesperationDiscard;

        public bool TrumpChangerUsed => _trumpChangerUsed;
        public bool DoubleTroubleActive => _doubleTroubleActive;

        public int GetResource(int player) => _resource[player];
        public ResourceType? GetResourceType(int player) => _config.ArchetypeResource[player];

        const int MaxResource = 5;

        public void GainResource(int player, int amount)
        {
            if (_config.ArchetypeResource[player] == null || amount <= 0) return;
            _resource[player] = Math.Min(_resource[player] + amount, MaxResource);
            OnResourceChanged?.Invoke(player, _config.ArchetypeResource[player].Value, _resource[player]);
        }

        public bool SpendResource(int player, int cost)
        {
            if (_config.ArchetypeResource[player] == null || cost <= 0) return false;
            if (_resource[player] < cost) return false;
            _resource[player] -= cost;
            OnResourceChanged?.Invoke(player, _config.ArchetypeResource[player].Value, _resource[player]);
            return true;
        }

        public GameEngine(int? seed, MatchConfig config, PlayerDeck deck0, PlayerDeck deck1)
        {
            _config = config ?? MatchConfig.Default();
            _seed = seed ?? Environment.TickCount;
            _playerDecks = new[] { deck0, deck1 };
            _playerDiscards = new[] { new List<Card>(), new List<Card>() };
            _hands = new[] { new Hand(), new Hand() };

            deck0.Build(_seed);
            deck1.Build(_seed + 1);
        }

        // --- Deck helpers ---

        Card DrawFromDeck(int playerIndex) => _playerDecks[playerIndex].Draw();

        bool CanDrawFromDeck(int playerIndex) => _playerDecks[playerIndex].DrawPileCount > 0;

        int AnyDeckCount() => _playerDecks[0].DrawPileCount + _playerDecks[1].DrawPileCount;

        int DeckCountFor(int playerIndex) => _playerDecks[playerIndex].DrawPileCount;

        void AddToDiscard(Card card, int ownerIndex = -1)
        {
            if (ownerIndex >= 0) _playerDiscards[ownerIndex].Add(card);
        }

        void RecycleCard(Card card, int ownerIndex) => _playerDecks[ownerIndex].PutOnBottom(card);

        Card[] PeekTopDeck(int playerIndex, int count) => _playerDecks[playerIndex].PeekTop(count);

        void ReplaceTopDeck(int playerIndex, Card[] cards) => _playerDecks[playerIndex].ReplaceTop(cards);

        void PutOnTopOfDeck(int playerIndex, Card card) => _playerDecks[playerIndex].PutOnTop(card);

        void ShuffleIntoDeck(int playerIndex, Card card) => _playerDecks[playerIndex].ShuffleIn(card);

        int PseudoRandomSeed() => _playerDecks[0].DrawPileCount + _playerDecks[1].DrawPileCount + _boutCount;

        void PutOnBottomOfDeck(int playerIndex, Card card) => _playerDecks[playerIndex].PutOnBottom(card);

        void ShuffleInManyToDeck(int playerIndex, IEnumerable<Card> cards) => _playerDecks[playerIndex].ShuffleInMany(cards);

        public void StartNewGame()
        {
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
            _quicksilverUsed = new bool[2];
            _slipAwayUsed = new bool[2];
            _jugglersBallsUsed = new bool[2];
            _luckyDrawUsed = new bool[2];
            _resource[0] = _config.StartingResource;
            _resource[1] = _config.StartingResource;
            _attackedThisBout[0] = false;
            _attackedThisBout[1] = false;

            if (_config.ForcedTrumpSuit.HasValue)
                Trump = (Suit)_config.ForcedTrumpSuit.Value;
            else
                Trump = (Suit)(new Random(_seed).Next(4));
            TrumpCard = new Card(Trump, Rank.Six, null);

            int handSize = _config.HandSize;
            for (int i = 0; i < handSize; i++)
            {
                if (CanDrawFromDeck(0)) _hands[0].Add(DrawFromDeck(0));
                if (CanDrawFromDeck(1)) _hands[1].Add(DrawFromDeck(1));
            }

            for (int p = 0; p < 2; p++)
            {
                if (!_config.LoadedDice[p]) continue;
                var topCards = PeekTopDeck(p, 3);
                if (topCards.Length > 1)
                {
                    Array.Sort(topCards, (a, b) =>
                    {
                        bool at = a.Suit == Trump, bt = b.Suit == Trump;
                        if (at != bt) return at ? 1 : -1;
                        return (int)a.Rank - (int)b.Rank;
                    });
                    ReplaceTopDeck(p, topCards);
                }
                break;
            }

            for (int p = 0; p < 2; p++)
            {
                if (_config.FoolsGold[p])
                    _hands[p].Add(new Card(Trump, Rank.Seven, null));
            }

            ApplyDeckPassives();
            ApplyVentriloquistsDummy();

            AttackerIndex = ChooseFirstAttacker();
            ApplyBoutStartPassives();
            ApplyCourtFavor(AttackerIndex);
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

                var pick = opponentAbilities[PseudoRandomSeed() % opponentAbilities.Count];
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
            _attackedThisBout[playerIndex] = true;

            if (_doubleTroubleActive) _doubleTroubleActive = false;
            else if (_config.DuelistGlove[playerIndex] && !_duelistGloveUsedThisBout[playerIndex] && rankBypass)
                _duelistGloveUsedThisBout[playerIndex] = true;

            if (_config.BattleHardened[playerIndex] && card.Suit == Trump)
                GainResource(playerIndex, 1);

            Phase = Phase.Defense;
            OnAttackPlayed?.Invoke(playerIndex, card);

            if (card.Trigger == TriggerTiming.OnAttack && card.HasAbility)
            {
                int cost = _config.AbilitiesCostResource ? card.Ability.Value.TriggerCost() : 0;
                bool canFire = cost == 0 || SpendResource(playerIndex, cost);
                if (canFire)
                {
                    ApplyAbility(card.Ability.Value, playerIndex, card, -1);
                    _usedAbilityThisBout[playerIndex] = true;
                    OnAbilityUsed?.Invoke(playerIndex, card, card.Ability.Value);
                }
            }

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
            if (_config.EndgameSpecialist[playerIndex] && AnyDeckCount() <= 6)
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

            if (card.Trigger == TriggerTiming.OnDefend && card.HasAbility)
            {
                int cost = _config.AbilitiesCostResource ? card.Ability.Value.TriggerCost() : 0;
                bool canFire = cost == 0 || SpendResource(playerIndex, cost);
                if (canFire)
                {
                    ApplyAbility(card.Ability.Value, playerIndex, card, slot);
                    _usedAbilityThisBout[playerIndex] = true;
                    OnAbilityUsed?.Invoke(playerIndex, card, card.Ability.Value);
                }
            }

            if (_config.ShadowReflexes[playerIndex] && !_shadowReflexesUsedThisBout[playerIndex] && CanDrawFromDeck(playerIndex))
            {
                _shadowReflexesUsedThisBout[playerIndex] = true;
                _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                OnDrew?.Invoke(playerIndex, 1);
                GainResource(playerIndex, 1);
            }

            return true;
        }

        public bool TryEat(int playerIndex)
        {
            if (Phase != Phase.Defense && Phase != Phase.Attack) return false;
            if (playerIndex != DefenderIndex) return false;
            if (_bout.IsEmpty) return false;

            CollectCrownOfThornsRanks();

            int cardsEaten = 0;
            foreach (var c in _bout.AllCards())
            {
                if (_config.GracefulRetreat[playerIndex] && cardsEaten == 0 && _bout.Defenses[0] == null)
                {
                    RecycleCard(c, playerIndex);
                    cardsEaten++;
                    continue;
                }
                _hands[playerIndex].Add(c);
                cardsEaten++;
            }
            _bout.Clear();

            if (_config.PoisonedWine[1 - playerIndex])
            {
                int drawn = 0;
                while (drawn < 2 && CanDrawFromDeck(playerIndex))
                {
                    _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                    drawn++;
                }
                if (drawn > 0) OnDrew?.Invoke(playerIndex, drawn);
            }

            if (_config.EatDrawsExtra)
            {
                int drawn = 0;
                while (drawn < 2 && CanDrawFromDeck(playerIndex))
                {
                    _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                    drawn++;
                }
                if (drawn > 0) OnDrew?.Invoke(playerIndex, drawn);
            }

            if (_config.ThickSkin[playerIndex] && CanDrawFromDeck(playerIndex))
            {
                _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                OnDrew?.Invoke(playerIndex, 1);
            }

            int attacker = 1 - playerIndex;
            if (_config.BruteFury[attacker] && CanDrawFromDeck(attacker))
            {
                _hands[attacker].Add(DrawFromDeck(attacker));
                OnDrew?.Invoke(attacker, 1);
                GainResource(attacker, 1);
            }

            if (_config.Bloodlust[attacker])
            {
                int eaten = cardsEaten;
                GainResource(attacker, eaten > 0 ? eaten : 1);
            }

            if (_config.LuckyDraw[playerIndex] && !_luckyDrawUsed[playerIndex] && _hands[playerIndex].Count > 1)
            {
                _luckyDrawUsed[playerIndex] = true;
                var worst = FindWorstCard(_hands[playerIndex], Trump);
                if (worst.HasValue)
                {
                    _hands[playerIndex].Remove(worst.Value);
                    RecycleCard(worst.Value, playerIndex);
                }
                GainResource(playerIndex, 1);
            }

            if (_config.DesperationDiscard && _hands[playerIndex].Count >= 10)
            {
                int discarded = 0;
                for (int d = 0; d < 2 && _hands[playerIndex].Count > 6; d++)
                {
                    var worst = FindWorstCard(_hands[playerIndex], Trump);
                    if (worst.HasValue)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        AddToDiscard(worst.Value, playerIndex);
                        discarded++;
                    }
                }
                if (discarded > 0)
                {
                    GainResource(playerIndex, 1);
                    OnDesperationDiscard?.Invoke(playerIndex, discarded);
                }
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
            if (card.Trigger != TriggerTiming.None) return false;

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
                RecycleCard(card, playerIndex);
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
                RecycleCard(card, playerIndex);
            }

            // ChaoticNature: 50% chance to return the ability card to hand
            if (!keepCard && _config.ChaoticNature[playerIndex])
            {
                int roll = (PseudoRandomSeed() + playerIndex) % 2;
                if (roll == 0)
                {
                    _playerDecks[playerIndex].RemoveCard(card);
                    _hands[playerIndex].Add(card);
                }
            }

            // Undermine: opponent discards 1 extra card when using an ability
            int opponent = 1 - playerIndex;
            if (_config.Undermine[opponent] && _hands[playerIndex].Count > 0)
            {
                var worst = FindWorstCard(_hands[playerIndex], Trump);
                if (worst.HasValue)
                {
                    _hands[playerIndex].Remove(worst.Value);
                    RecycleCard(worst.Value, playerIndex);
                }
            }

            // WebOfLies: gain 1 Favor when opponent uses an ability
            if (_config.WebOfLies[opponent])
                GainResource(opponent, 1);

            ApplyAbility(ability, playerIndex, card, defenseSlot);

            // SleightOfMind: draw 1 after using an active ability
            if (_config.SleightOfMind[playerIndex] && CanDrawFromDeck(playerIndex) && Phase != Phase.GameOver)
            {
                _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                OnDrew?.Invoke(playerIndex, 1);
            }

            _usedAbilityThisBout[playerIndex] = true;
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
                    return Phase == Phase.Attack && DeckCountFor(playerIndex) > 0;
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
                    return Phase == Phase.Attack && DeckCountFor(playerIndex) > 0 && _bout.AttackCount < (_config.MaxAttacksPerBout + _pileOnBonus);
                case AbilityType.Deflect:
                    return Phase == Phase.Defense && _bout.FirstUndefendedSlot() >= 0 && _resource[playerIndex] >= 1;
                case AbilityType.SlipAway:
                    return Phase == Phase.Defense && _bout.FirstUndefendedSlot() >= 0 && !_slipAwayUsed[playerIndex];
                case AbilityType.Peek:
                    return DeckCountFor(playerIndex) > 0;
                case AbilityType.Gambit:
                    return DeckCountFor(playerIndex) > 0;

                // Rogue: Shadow
                case AbilityType.Riposte:
                    return Phase == Phase.Defense && _resource[playerIndex] >= 1;
                case AbilityType.ShadowCloak:
                    return Phase == Phase.Defense;

                // Rogue: Spy
                case AbilityType.Wiretap:
                    return DeckCountFor(playerIndex) > 0;
                case AbilityType.DoubleAgent:
                    return _resource[playerIndex] >= 3 && _hands[1 - playerIndex].Count > 0;
                case AbilityType.Blackmail:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 2 && _hands[1 - playerIndex].Count > 0;

                // Rogue: Saboteur
                case AbilityType.SleightOfHand:
                    return DeckCountFor(playerIndex) > 0 && _hands[playerIndex].Count > 0;
                case AbilityType.SmokeBomb:
                    return Phase == Phase.Defense && _resource[playerIndex] >= 1 && !_bout.IsEmpty;
                case AbilityType.TrapCard:
                    return true;

                // Brute: Berserker
                case AbilityType.Rampage:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 1 && DeckCountFor(playerIndex) >= 2;

                // Brute: Brawler
                case AbilityType.Haymaker:
                    return Phase == Phase.Attack && DeckCountFor(playerIndex) > 0;
                case AbilityType.IronGrip:
                    return Phase == Phase.Defense && _resource[playerIndex] >= 1 && DeckCountFor(playerIndex) > 0;
                case AbilityType.Brawl:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 2 && DeckCountFor(playerIndex) >= 6;

                // Brute: Warlord
                case AbilityType.Conquer:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 1 && DeckCountFor(playerIndex) > 0;
                case AbilityType.Intimidate:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 1 && _hands[1 - playerIndex].Count > 0;
                case AbilityType.CrownSeize:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 3 && DeckCountFor(playerIndex) > 0;

                // Diplomat: Courtier
                case AbilityType.CourtIntrigue:
                    return _resource[playerIndex] >= 1 && DeckCountFor(playerIndex) > 0;
                case AbilityType.RoyalDecree:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 1 && DeckCountFor(playerIndex) > 0;
                case AbilityType.Patronage:
                    return _resource[playerIndex] >= 3 && DeckCountFor(playerIndex) > 0;

                // Diplomat: Puppeteer
                case AbilityType.PullStrings:
                    return Phase == Phase.Defense && _resource[playerIndex] >= 1 && _hands[1 - playerIndex].Count > 0;
                case AbilityType.Misdirection:
                    return Phase == Phase.Defense && _resource[playerIndex] >= 1;
                case AbilityType.ForcedHand:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 2 && _hands[1 - playerIndex].Count > 0;

                // Diplomat: Peacemaker
                case AbilityType.Diplomacy:
                    return Phase == Phase.Defense && !_bout.IsEmpty;
                case AbilityType.SafePassage:
                    return Phase == Phase.Defense && _resource[playerIndex] >= 2 && _bout.FirstUndefendedSlot() >= 0;
                case AbilityType.Treaty:
                    return _resource[playerIndex] >= 3 && DeckCountFor(playerIndex) > 0;

                // Gambler: Card Shark
                case AbilityType.StackTheDeck:
                    return DeckCountFor(playerIndex) > 0 && _hands[playerIndex].Count > 0;
                case AbilityType.SecondDeal:
                    return _resource[playerIndex] >= 1 && DeckCountFor(playerIndex) > 0;
                case AbilityType.ColdRead:
                    return _resource[playerIndex] >= 2 && DeckCountFor(playerIndex) > 0;

                // Gambler: High Roller
                case AbilityType.AllIn:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 1 && DeckCountFor(playerIndex) > 0;
                case AbilityType.DoubleOrNothing:
                    return _resource[playerIndex] >= 1 && DeckCountFor(playerIndex) >= 2;
                case AbilityType.LuckyStreak:
                    return Phase == Phase.Attack && _resource[playerIndex] >= 2 && DeckCountFor(playerIndex) > 0;

                // Gambler: Trickster
                case AbilityType.BlindSwap:
                    return _hands[1 - playerIndex].Count > 0 && _hands[playerIndex].Count > 0;
                case AbilityType.Misdeal:
                    return Phase == Phase.Defense && _resource[playerIndex] >= 1 && !_bout.IsEmpty;
                case AbilityType.WildCard:
                    return _resource[playerIndex] >= 2 && _discard.Count > 0;

                // Neutral
                case AbilityType.Fortify:
                    return Phase == Phase.Defense && _bout.FirstUndefendedSlot() >= 0;
                case AbilityType.SecondWind:
                    return _hands[playerIndex].Count >= 2 && DeckCountFor(playerIndex) > 0;
                case AbilityType.Brace:
                    return Phase == Phase.Defense && DeckCountFor(playerIndex) > 0;
                case AbilityType.Desperation:
                    return Phase == Phase.Defense && _bout.FirstUndefendedSlot() >= 0;
                case AbilityType.ResourceGain:
                    return true;

                case AbilityType.Masterstroke:
                    return _hands[1 - playerIndex].Count > 0;
                case AbilityType.Onslaught:
                    return DeckCountFor(playerIndex) >= 3;
                case AbilityType.Masquerade:
                    return _hands[1 - playerIndex].Count > 0;
                case AbilityType.Monopoly:
                    return DeckCountFor(playerIndex) > 0;

                default:
                    return true;
            }
        }

        void ApplyAbility(AbilityType ability, int playerIndex, Card card, int defenseSlot)
        {
            int opponent = 1 - playerIndex;

            switch (ability)
            {
                case AbilityType.TrumpChanger:
                    Trump = card.Suit;
                    _trumpChangerUsed = true;
                    OnTrumpChanged?.Invoke(Trump);
                    ApplyGracefulManners();
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
                    var feintCard = DrawFromDeck(playerIndex);
                    _bout.AddAttack(feintCard);
                    Phase = Phase.Defense;
                    OnAttackPlayed?.Invoke(playerIndex, feintCard);
                    break;

                case AbilityType.Deflect:
                    SpendResource(playerIndex, 1);
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
                    SortTopDeck(playerIndex, 3);
                    break;

                case AbilityType.Gambit:
                {
                    int count = _hands[playerIndex].Count;
                    var toDiscard = new List<Card>(_hands[playerIndex].Cards);
                    foreach (var c in toDiscard)
                    {
                        _hands[playerIndex].Remove(c);
                        RecycleCard(c, playerIndex);
                    }
                    int toDraw = Math.Min(count, DeckCountFor(playerIndex));
                    for (int i = 0; i < toDraw; i++)
                        _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                    if (toDraw > 0) OnDrew?.Invoke(playerIndex, toDraw);
                    break;
                }

                // --- Rogue: Shadow ---

                case AbilityType.Riposte:
                    SpendResource(playerIndex, 1);
                    DiscardRandomCards(opponent, 1);
                    break;

                case AbilityType.ShadowCloak:
                    _pileOnBonus -= 2;
                    break;

                // --- Rogue: Spy ---

                case AbilityType.Wiretap:
                    SortTopDeck(playerIndex, 5);
                    break;

                case AbilityType.DoubleAgent:
                    SpendResource(playerIndex, 3);
                    StealRandomCard(playerIndex, opponent);
                    break;

                case AbilityType.Blackmail:
                    SpendResource(playerIndex, 2);
                    DiscardHighestCards(opponent, 2);
                    break;

                // --- Rogue: Saboteur ---

                case AbilityType.SleightOfHand:
                {
                    if (CanDrawFromDeck(playerIndex))
                        _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                    var worst = FindWorstCard(_hands[playerIndex], Trump);
                    if (worst.HasValue)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        PutOnTopOfDeck(playerIndex, worst.Value);
                    }
                    break;
                }

                case AbilityType.SmokeBomb:
                {
                    SpendResource(playerIndex, 1);
                    for (int i = 0; i < _bout.Attacks.Count; i++)
                    {
                        _discard.Add(_bout.Attacks[i]);
                        if (_bout.Defenses[i] != null)
                            _discard.Add(_bout.Defenses[i].Value);
                    }
                    CollectCrownOfThornsRanks();
                    _bout.Clear();
                    ResolveBout(BoutOutcome.DefenderWonAllDiscarded);
                    break;
                }

                case AbilityType.TrapCard:
                {
                    DrawCards(playerIndex, 1);
                    if (_hands[opponent].Count > 0)
                    {
                        var worst = FindWorstCard(_hands[opponent], Trump);
                        if (worst.HasValue)
                        {
                            _hands[opponent].Remove(worst.Value);
                            PutOnBottomOfDeck(opponent, worst.Value);
                        }
                    }
                    break;
                }

                // --- Brute: Berserker ---

                case AbilityType.Rampage:
                {
                    SpendResource(playerIndex, 1);
                    int played = 0;
                    while (played < 2 && CanDrawFromDeck(playerIndex))
                    {
                        var rampCard = DrawFromDeck(playerIndex);
                        _bout.AddAttack(rampCard);
                        Phase = Phase.Defense;
                        OnAttackPlayed?.Invoke(playerIndex, rampCard);
                        played++;
                    }
                    break;
                }

                // --- Brute: Brawler ---

                case AbilityType.Haymaker:
                    DrawCards(playerIndex, 2);
                    break;

                case AbilityType.IronGrip:
                    SpendResource(playerIndex, 1);
                    DrawCards(playerIndex, 3);
                    break;

                case AbilityType.Brawl:
                {
                    SpendResource(playerIndex, 2);
                    for (int p = 0; p < 2; p++)
                    {
                        var toDiscard = new List<Card>(_hands[p].Cards);
                        foreach (var c in toDiscard)
                        {
                            _hands[p].Remove(c);
                            RecycleCard(c, p);
                        }
                    }
                    for (int p = 0; p < 2; p++)
                        DrawCards(p, 6);
                    break;
                }

                // --- Brute: Warlord ---

                case AbilityType.Conquer:
                {
                    SpendResource(playerIndex, 1);
                    if (CanDrawFromDeck(playerIndex))
                    {
                        var drawn = DrawFromDeck(playerIndex);
                        _hands[playerIndex].Add(drawn);
                        int d = 1;
                        if (drawn.Suit == Trump && CanDrawFromDeck(playerIndex))
                        {
                            _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                            d++;
                        }
                        OnDrew?.Invoke(playerIndex, d);
                    }
                    break;
                }

                case AbilityType.Intimidate:
                    SpendResource(playerIndex, 1);
                    DiscardRandomNonTrump(opponent, 1);
                    break;

                case AbilityType.CrownSeize:
                    SpendResource(playerIndex, 3);
                    Trump = card.Suit;
                    OnTrumpChanged?.Invoke(Trump);
                    ApplyGracefulManners();
                    DrawCards(playerIndex, 2);
                    break;

                // --- Diplomat: Courtier ---

                case AbilityType.CourtIntrigue:
                    SpendResource(playerIndex, 1);
                    SortTopDeck(playerIndex, 3);
                    DrawCards(playerIndex, 1);
                    break;

                case AbilityType.RoyalDecree:
                    SpendResource(playerIndex, 1);
                    DrawCards(playerIndex, 2);
                    DiscardRandomCards(opponent, 1);
                    break;

                case AbilityType.Patronage:
                    SpendResource(playerIndex, 3);
                    DrawCards(playerIndex, 3);
                    break;

                // --- Diplomat: Puppeteer ---

                case AbilityType.PullStrings:
                    SpendResource(playerIndex, 1);
                    DiscardRandomCards(opponent, 2);
                    break;

                case AbilityType.Misdirection:
                    SpendResource(playerIndex, 1);
                    _bout.AttacksCapped = true;
                    DiscardRandomCards(opponent, 1);
                    break;

                case AbilityType.ForcedHand:
                    SpendResource(playerIndex, 2);
                    DiscardHighestCards(opponent, 1);
                    break;

                // --- Diplomat: Peacemaker ---

                case AbilityType.Diplomacy:
                {
                    for (int i = 0; i < _bout.Attacks.Count; i++)
                    {
                        _discard.Add(_bout.Attacks[i]);
                        if (_bout.Defenses[i] != null)
                            _discard.Add(_bout.Defenses[i].Value);
                    }
                    CollectCrownOfThornsRanks();
                    _bout.Clear();
                    DrawCards(playerIndex, 1);
                    ResolveBout(BoutOutcome.DefenderWonAllDiscarded);
                    break;
                }

                case AbilityType.SafePassage:
                {
                    SpendResource(playerIndex, 2);
                    for (int i = 0; i < _bout.Defenses.Count; i++)
                    {
                        if (_bout.Defenses[i] == null)
                            _bout.AutoDefend(i);
                    }
                    Phase = Phase.Attack;
                    break;
                }

                case AbilityType.Treaty:
                {
                    SpendResource(playerIndex, 3);
                    for (int p = 0; p < 2; p++)
                    {
                        int need = 6 - _hands[p].Count;
                        if (need > 0) DrawCards(p, need);
                    }
                    break;
                }

                // --- Gambler: Card Shark ---

                case AbilityType.StackTheDeck:
                {
                    var worst = FindWorstCard(_hands[playerIndex], Trump);
                    if (worst.HasValue)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        PutOnTopOfDeck(playerIndex, worst.Value);
                    }
                    DrawCards(playerIndex, 2);
                    break;
                }

                case AbilityType.SecondDeal:
                {
                    SpendResource(playerIndex, 1);
                    DrawCards(playerIndex, 2);
                    var worst = FindWorstCard(_hands[playerIndex], Trump);
                    if (worst.HasValue)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        PutOnBottomOfDeck(playerIndex, worst.Value);
                    }
                    break;
                }

                case AbilityType.ColdRead:
                {
                    SpendResource(playerIndex, 2);
                    DrawCards(playerIndex, 3);
                    var worst = FindWorstCard(_hands[playerIndex], Trump);
                    if (worst.HasValue)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        PutOnTopOfDeck(playerIndex, worst.Value);
                    }
                    break;
                }

                // --- Gambler: High Roller ---

                case AbilityType.AllIn:
                {
                    int luck = _resource[playerIndex];
                    SpendResource(playerIndex, luck);
                    DrawCards(playerIndex, luck);
                    break;
                }

                case AbilityType.DoubleOrNothing:
                {
                    SpendResource(playerIndex, 1);
                    DrawCards(playerIndex, 2);
                    var worst = FindWorstCard(_hands[playerIndex], Trump);
                    if (worst.HasValue)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        RecycleCard(worst.Value, playerIndex);
                    }
                    break;
                }

                case AbilityType.LuckyStreak:
                {
                    SpendResource(playerIndex, 2);
                    if (CanDrawFromDeck(playerIndex))
                    {
                        var ls1 = DrawFromDeck(playerIndex);
                        _bout.AddAttack(ls1);
                        Phase = Phase.Defense;
                        OnAttackPlayed?.Invoke(playerIndex, ls1);
                        if (ls1.Suit == Trump && CanDrawFromDeck(playerIndex))
                        {
                            var ls2 = DrawFromDeck(playerIndex);
                            _bout.AddAttack(ls2);
                            OnAttackPlayed?.Invoke(playerIndex, ls2);
                        }
                    }
                    break;
                }

                // --- Gambler: Trickster ---

                case AbilityType.BlindSwap:
                {
                    if (_hands[playerIndex].Count > 0 && _hands[opponent].Count > 0)
                    {
                        int myIdx = PseudoRandomSeed() % _hands[playerIndex].Count;
                        int theirIdx = (PseudoRandomSeed() + 1) % _hands[opponent].Count;
                        var myCard = _hands[playerIndex].Cards[myIdx];
                        var theirCard = _hands[opponent].Cards[theirIdx];
                        _hands[playerIndex].Remove(myCard);
                        _hands[opponent].Remove(theirCard);
                        _hands[playerIndex].Add(theirCard);
                        _hands[opponent].Add(myCard);
                    }
                    break;
                }

                case AbilityType.Misdeal:
                {
                    SpendResource(playerIndex, 1);
                    var attacks = new List<Card>();
                    for (int i = 0; i < _bout.Attacks.Count; i++)
                    {
                        attacks.Add(_bout.Attacks[i]);
                        if (_bout.Defenses[i] != null)
                            _discard.Add(_bout.Defenses[i].Value);
                    }
                    ShuffleInManyToDeck(1 - playerIndex, attacks);
                    CollectCrownOfThornsRanks();
                    _bout.Clear();
                    ResolveBout(BoutOutcome.DefenderWonAllDiscarded);
                    break;
                }

                case AbilityType.WildCard:
                {
                    SpendResource(playerIndex, 2);
                    if (_discard.Count > 0)
                    {
                        var retrieved = _discard[_discard.Count - 1];
                        _discard.RemoveAt(_discard.Count - 1);
                        _hands[playerIndex].Add(retrieved);
                    }
                    break;
                }

                // --- Neutral ---

                case AbilityType.Fortify:
                {
                    int fSlot = _bout.FirstUndefendedSlot();
                    if (fSlot >= 0)
                        _bout.AutoDefend(fSlot);
                    if (_bout.FullyDefended) Phase = Phase.Attack;
                    break;
                }

                case AbilityType.SecondWind:
                {
                    int discarded = 0;
                    while (discarded < 2 && _hands[playerIndex].Count > 0)
                    {
                        var worst = FindWorstCard(_hands[playerIndex], Trump);
                        if (!worst.HasValue) break;
                        _hands[playerIndex].Remove(worst.Value);
                        RecycleCard(worst.Value, playerIndex);
                        discarded++;
                    }
                    DrawCards(playerIndex, 3);
                    break;
                }

                case AbilityType.Brace:
                    DrawCards(playerIndex, 2);
                    break;

                case AbilityType.Desperation:
                {
                    var toDiscard = new List<Card>(_hands[playerIndex].Cards);
                    foreach (var c in toDiscard)
                    {
                        _hands[playerIndex].Remove(c);
                        RecycleCard(c, playerIndex);
                    }
                    for (int i = 0; i < _bout.Defenses.Count; i++)
                    {
                        if (_bout.Defenses[i] == null)
                            _bout.AutoDefend(i);
                    }
                    Phase = Phase.Attack;
                    DrawCards(playerIndex, 4);
                    break;
                }

                case AbilityType.ResourceGain:
                    if (_resource[playerIndex] >= MaxResource)
                        DrawCards(playerIndex, 1);
                    else
                        GainResource(playerIndex, 1);
                    break;

                case AbilityType.Masterstroke:
                    DiscardHighestCards(opponent, 2);
                    DrawCards(playerIndex, 1);
                    break;

                case AbilityType.Onslaught:
                {
                    int onsPlayed = 0;
                    while (onsPlayed < 3 && CanDrawFromDeck(playerIndex))
                    {
                        var onsCard = DrawFromDeck(playerIndex);
                        _bout.AddAttack(onsCard);
                        Phase = Phase.Defense;
                        OnAttackPlayed?.Invoke(playerIndex, onsCard);
                        onsPlayed++;
                    }
                    break;
                }

                case AbilityType.Masquerade:
                {
                    var myCards = new List<Card>(_hands[playerIndex].Cards);
                    var theirCards = new List<Card>(_hands[opponent].Cards);
                    _hands[playerIndex].Clear();
                    _hands[opponent].Clear();
                    foreach (var c in theirCards) _hands[playerIndex].Add(c);
                    foreach (var c in myCards) _hands[opponent].Add(c);
                    break;
                }

                case AbilityType.Monopoly:
                {
                    int need = 7 - _hands[playerIndex].Count;
                    if (need > 0) DrawCards(playerIndex, need);
                    while (_hands[opponent].Count > 5)
                    {
                        var worst = FindWorstCard(_hands[opponent], Trump);
                        if (!worst.HasValue) break;
                        _hands[opponent].Remove(worst.Value);
                        AddToDiscard(worst.Value, opponent);
                    }
                    break;
                }
            }
        }

        // ---------- Ability helpers ----------

        void DrawCards(int playerIndex, int count)
        {
            int drawn = 0;
            while (drawn < count && CanDrawFromDeck(playerIndex))
            {
                _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                drawn++;
            }
            if (drawn > 0) OnDrew?.Invoke(playerIndex, drawn);
        }

        void DiscardRandomCards(int playerIndex, int count)
        {
            for (int i = 0; i < count && _hands[playerIndex].Count > 0; i++)
            {
                int idx = (PseudoRandomSeed() + i) % _hands[playerIndex].Count;
                var c = _hands[playerIndex].Cards[idx];
                _hands[playerIndex].Remove(c);
                RecycleCard(c, playerIndex);
            }
        }

        void DiscardRandomNonTrump(int playerIndex, int count)
        {
            var candidates = new List<Card>();
            foreach (var c in _hands[playerIndex].Cards)
                if (c.Suit != Trump) candidates.Add(c);
            if (candidates.Count == 0) return;
            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                int idx = (PseudoRandomSeed() + i) % candidates.Count;
                var c = candidates[idx];
                _hands[playerIndex].Remove(c);
                RecycleCard(c, playerIndex);
                candidates.RemoveAt(idx);
            }
        }

        void DiscardHighestCards(int playerIndex, int count)
        {
            for (int i = 0; i < count && _hands[playerIndex].Count > 0; i++)
            {
                Card? highest = null;
                foreach (var c in _hands[playerIndex].Cards)
                {
                    if (highest == null) { highest = c; continue; }
                    if ((int)c.Rank > (int)highest.Value.Rank) highest = c;
                    else if ((int)c.Rank == (int)highest.Value.Rank && c.Suit == Trump && highest.Value.Suit != Trump)
                        highest = c;
                }
                if (highest.HasValue)
                {
                    _hands[playerIndex].Remove(highest.Value);
                    RecycleCard(highest.Value, playerIndex);
                }
            }
        }

        void StealRandomCard(int playerIndex, int fromPlayer)
        {
            if (_hands[fromPlayer].Count == 0) return;
            int idx = PseudoRandomSeed() % _hands[fromPlayer].Count;
            var c = _hands[fromPlayer].Cards[idx];
            _hands[fromPlayer].Remove(c);
            _hands[playerIndex].Add(c);
        }

        void SortTopDeck(int playerIndex, int count)
        {
            var top = PeekTopDeck(playerIndex, count);
            if (top.Length > 1)
            {
                Array.Sort(top, (a, b) =>
                {
                    bool at = a.Suit == Trump, bt = b.Suit == Trump;
                    if (at != bt) return at ? 1 : -1;
                    return (int)a.Rank - (int)b.Rank;
                });
                ReplaceTopDeck(playerIndex, top);
            }
        }

        void ApplyGracefulManners()
        {
            for (int p = 0; p < 2; p++)
            {
                if (_config.GracefulManners[p])
                    GainResource(p, 2);
            }
        }

        // ---------- Bout-start passives ----------

        void ApplyBoutStartPassives()
        {
            for (int p = 0; p < 2; p++)
            {
                if (_config.AbilitiesCostResource && _boutCount % 2 == 0)
                    GainResource(p, 1);
                if (_config.MarkedCards[p])
                    GainResource(p, 1);
                if (_config.SharkInstinct[p])
                    GainResource(p, 1);
                if (_config.Equilibrium[p] && _hands[1 - p].Count > _hands[p].Count && CanDrawFromDeck(p))
                {
                    _hands[p].Add(DrawFromDeck(p));
                    OnDrew?.Invoke(p, 1);
                }
                if (_config.SteadyHand[p] && _hands[p].Count <= 3 && CanDrawFromDeck(p))
                {
                    _hands[p].Add(DrawFromDeck(p));
                    OnDrew?.Invoke(p, 1);
                }
            }
        }

        void ApplyDeckPassives()
        {
            for (int p = 0; p < 2; p++)
            {
                if (_playerDecks[p] == null) continue;
                foreach (var def in _playerDecks[p].Templates)
                {
                    if (def.Trigger != TriggerTiming.Passive || !def.HasAbility) continue;
                    ApplyPassiveFromAbility(def.Ability.Value, p);
                }
            }
        }

        void ApplyPassiveFromAbility(AbilityType ability, int player)
        {
            switch (ability)
            {
                case AbilityType.Equilibrium: _config.Equilibrium[player] = true; break;
                case AbilityType.SteadyHand: _config.SteadyHand[player] = true; break;
                case AbilityType.MarkedCards: _config.MarkedCards[player] = true; break;
                case AbilityType.SharkInstinct: _config.SharkInstinct[player] = true; break;
                case AbilityType.BattleHardened: _config.BattleHardened[player] = true; break;
                case AbilityType.ThickSkin: _config.ThickSkin[player] = true; break;
                case AbilityType.Bloodlust: _config.Bloodlust[player] = true; break;
                case AbilityType.EndgameSpecialist: _config.EndgameSpecialist[player] = true; break;
                case AbilityType.SleightOfMind: _config.SleightOfMind[player] = true; break;
                case AbilityType.CardCounter: _config.CardCounter[player] = true; break;
                case AbilityType.TrumpAffinity: _config.TrumpAffinity[player] = true; break;
                case AbilityType.QuickHands: _config.QuickHands[player] = true; break;
                case AbilityType.GracefulManners: _config.GracefulManners[player] = true; break;
                case AbilityType.PatienceRewarded: _config.PatienceRewarded[player] = true; break;
                case AbilityType.Jackpot: _config.Jackpot[player] = true; break;
            }
        }

        // ---------- Core engine ----------

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
                    if (_config.QuickHands[p] && CanDrawFromDeck(p))
                    {
                        _hands[p].Add(DrawFromDeck(p));
                        if (_hands[p].Count > 1)
                        {
                            var worst = FindWorstCard(_hands[p], Trump);
                            if (worst.HasValue)
                            {
                                _hands[p].Remove(worst.Value);
                                RecycleCard(worst.Value, p);
                            }
                        }
                    }
                }

                // PatienceRewarded: defender gains 2 Intel on successful defense
                if (_config.PatienceRewarded[defenderBefore])
                    GainResource(defenderBefore, 2);

                // Jackpot: defender gains 2 Luck on successful defense
                if (_config.Jackpot[defenderBefore])
                    GainResource(defenderBefore, 2);
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
                        RecycleCard(worst.Value, p);
                    }
                }
            }

            _doubleTroubleActive = false;
            _pileOnBonus = 0;
            _duelistGloveUsedThisBout = new bool[2];
            _shadowReflexesUsedThisBout = new bool[2];

            ApplyResourceDecay();

            if (CheckGameOver(attackerBefore, defenderBefore, outcome)) return;

            if (_config.MaxBouts > 0 && _boutCount >= _config.MaxBouts)
            {
                int cards0 = _hands[0].Count + DeckCountFor(0);
                int cards1 = _hands[1].Count + DeckCountFor(1);
                int loser = cards0 >= cards1 ? 0 : 1;
                EndGame(fool: loser);
                return;
            }

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

            ApplyBoutStartPassives();
            ApplyCourtFavor(AttackerIndex);

            Phase = Phase.Attack;
            OnTurnBegan?.Invoke(AttackerIndex);
        }

        void ApplyResourceDecay()
        {
            for (int p = 0; p < 2; p++)
            {
                var rt = _config.ArchetypeResource[p];
                if (rt == null) continue;

                switch (rt.Value)
                {
                    case ResourceType.Luck:
                        if (!_usedAbilityThisBout[p] && _resource[p] > 0)
                        {
                            _resource[p]--;
                            OnResourceChanged?.Invoke(p, rt.Value, _resource[p]);
                        }
                        break;
                    case ResourceType.Fury:
                        // Fury no longer decays per bout — balanced by fewer generators
                        break;
                    case ResourceType.Favor:
                        if (!_usedAbilityThisBout[p] && _resource[p] > 0)
                        {
                            _resource[p]--;
                            OnResourceChanged?.Invoke(p, rt.Value, _resource[p]);
                        }
                        break;
                }
                _attackedThisBout[p] = false;
                _usedAbilityThisBout[p] = false;
            }
        }

        void ApplyCourtFavor(int player)
        {
            if (!_config.CourtFavor[player] || DeckCountFor(player) < 4) return;
            var top2 = _playerDecks[player].PeekTop(2);
            if (top2.Length < 2) return;
            int s0 = CardValue(top2[0], Trump), s1 = CardValue(top2[1], Trump);
            var topCard = _playerDecks[player].Draw();
            var secondCard = _playerDecks[player].Draw();
            if (s0 <= s1)
                _playerDecks[player].PutOnBottom(topCard);
            else
                _playerDecks[player].PutOnBottom(secondCard);
            if (s0 <= s1)
                _playerDecks[player].PutOnTop(secondCard);
            else
                _playerDecks[player].PutOnTop(topCard);
        }

        static int CardValue(Card c, Suit trump)
        {
            int v = (int)c.Rank;
            if (c.Suit == trump) v += 20;
            return v;
        }

        void DrawTo(int playerIndex, int target)
        {
            if (_config.CourtiersFan[playerIndex] && !_courtiersFanUsed[playerIndex] && _hands[playerIndex].Count < target)
            {
                _courtiersFanUsed[playerIndex] = true;
                target = Math.Max(target - 1, _hands[playerIndex].Count);
            }

            int drawn = 0;
            while (_hands[playerIndex].Count < target && CanDrawFromDeck(playerIndex))
            {
                if (_trumpStillInDeck && DeckCountFor(playerIndex) == 1) _trumpStillInDeck = false;
                var drawnCard = DrawFromDeck(playerIndex);
                _hands[playerIndex].Add(drawnCard);
                drawn++;

                if (_config.TrumpAffinity[playerIndex] && drawnCard.Suit == Trump && CanDrawFromDeck(playerIndex))
                {
                    _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                    drawn++;
                    var worst = FindWorstCard(_hands[playerIndex], Trump);
                    if (worst.HasValue)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        RecycleCard(worst.Value, playerIndex);
                        drawn--;
                    }
                }
            }
            if (drawn > 0 && _config.CardCounter[playerIndex] && CanDrawFromDeck(playerIndex))
            {
                var peeked = PeekTopDeck(playerIndex, 1);
                var worst = FindWorstCard(_hands[playerIndex], Trump);
                if (worst.HasValue && peeked.Length > 0)
                {
                    bool pBetter = IsBetterDraw(peeked[0], worst.Value, Trump);
                    if (pBetter)
                    {
                        _hands[playerIndex].Remove(worst.Value);
                        _hands[playerIndex].Add(DrawFromDeck(playerIndex));
                        RecycleCard(worst.Value, playerIndex);
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
            bool empty0 = _hands[0].Count == 0 && DeckCountFor(0) == 0;
            bool empty1 = _hands[1].Count == 0 && DeckCountFor(1) == 0;

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

        public void ForceEndGame(int fool) => EndGame(fool);

        void EndGame(int fool)
        {
            FoolIndex = fool;
            WinnerIndex = 1 - fool;
            Phase = Phase.GameOver;
            OnGameOver?.Invoke(fool);
        }
    }
}
