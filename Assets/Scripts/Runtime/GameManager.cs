using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools
{
    public sealed class GameManager : MonoBehaviour
    {
        [Header("Refs")]
        public TableView Table;
        public HudView Hud;
        public GameObject CardViewPrefab;

        [Header("Players")]
        public string PlayerName = "You";
        public string OpponentName = "Knave";

        [Header("Tuning")]
        public float AiThinkSeconds = 0.5f;
        public float DealSeconds = 0.18f;
        public float MoveSeconds = 0.22f;

        public bool AutoStartOnAwake = true;
        public event Action<int> OnMatchComplete;

        GameLoop _loop;
        GameEngine Engine => _loop.Engine;
        readonly Dictionary<Card, CardView> _humanCardViews = new();
        readonly List<CardView> _opponentCardViews = new();
        readonly List<CardView> _attackViews = new();   // bout slot index -> view
        readonly List<CardView> _defenseViews = new();
        CardView _trumpView;
        bool _humanIsPlayerZero;

        const int HumanPlayerIndex = 0;

        void Start()
        {
            if (AutoStartOnAwake) BeginNewGame();
        }

        float _earliestNextAiAct;
        bool _autoPlay;
        AIPlayer _autoPlayAI;

        int _stallFrames;

        void Update()
        {
            if (_loop == null) return;

            if (Input.GetKeyDown(KeyCode.A) && Engine.Phase != Phase.GameOver)
                ToggleAutoPlay();

            if (Engine.Phase == Phase.GameOver) return;

            int active = Engine.Phase == Phase.Defense ? Engine.DefenderIndex : Engine.AttackerIndex;
            bool aiTurn = _loop.Controllers[active].Kind == PlayerKind.AI;
            bool autoPlayTurn = _autoPlay && active == HumanPlayerIndex;

            if ((aiTurn || autoPlayTurn) && Time.time < _earliestNextAiAct) return;

            Phase prevPhase = Engine.Phase;
            int prevDeck = Engine.DeckCount;
            int prevH0 = Engine.HandOf(0).Count;
            int prevH1 = Engine.HandOf(1).Count;

            if (autoPlayTurn)
            {
                if (_autoPlayAI == null) _autoPlayAI = new AIPlayer("AutoPlay");
                _autoPlayAI.RequestAction(Engine, active);
                _earliestNextAiAct = Time.time + AiThinkSeconds;
            }
            else
            {
                _loop.Tick();
                if (aiTurn) _earliestNextAiAct = Time.time + AiThinkSeconds;
            }

            bool changed = Engine.Phase != prevPhase || Engine.DeckCount != prevDeck
                || Engine.HandOf(0).Count != prevH0 || Engine.HandOf(1).Count != prevH1;
            if (changed)
                _stallFrames = 0;
            else if (aiTurn || autoPlayTurn)
            {
                _stallFrames++;
                if (_stallFrames > 100)
                {
                    Debug.LogWarning($"[GameManager] Match stalled for {_stallFrames} frames — forcing eat. Phase={Engine.Phase} active={active}");
                    Engine.TryEat(Engine.DefenderIndex);
                    _stallFrames = 0;
                }
            }
        }

        public void SetAutoPlay(bool on)
        {
            _autoPlay = on;
            if (_autoPlay && Hud && Hud.AbilityChoiceVisible)
                Hud.HideAbilityChoice();
            UpdateAutoPlayLabel();
            if (_autoPlay) ApplyHighlightForPhase();
        }

        void ToggleAutoPlay()
        {
            SetAutoPlay(!_autoPlay);
        }

        void UpdateAutoPlayLabel()
        {
            if (!Hud || !Hud.AutoPlayButton) return;
            var lbl = Hud.AutoPlayButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (lbl) lbl.text = _autoPlay ? "Auto: ON" : "Auto: OFF";
        }

        public void BeginConfiguredGame(MatchConfig config, string opponentName, int? seed = null)
        {
            OpponentName = opponentName;
            ClearAllVisuals();
            Hud?.HideGameOver();

            var engine = new GameEngine(seed, config);
            _loop = new GameLoop(
                p0: new HumanPlayer(PlayerName),
                p1: new AIPlayer(opponentName),
                engine: engine);
            _humanIsPlayerZero = true;
            WireEngineEvents();
            WireHudButtons();
            _loop.Start();
        }

        public void BeginNewGame()
        {
            ClearAllVisuals();
            Hud?.HideGameOver();

            _loop = new GameLoop(
                p0: new HumanPlayer(PlayerName),
                p1: new AIPlayer(OpponentName));
            _humanIsPlayerZero = true;
            WireEngineEvents();
            WireHudButtons();
            _loop.Start();
        }

        void WireEngineEvents()
        {
            Engine.OnSetupComplete += OnSetupComplete;
            Engine.OnTurnBegan += OnTurnBegan;
            Engine.OnAttackPlayed += OnAttackPlayed;
            Engine.OnDefensePlayed += OnDefensePlayed;
            Engine.OnBoutResolved += OnBoutResolved;
            Engine.OnDrew += OnDrew;
            Engine.OnGameOver += OnGameOver;
            Engine.OnAbilityUsed += OnAbilityUsed;
            Engine.OnTrumpChanged += OnTrumpChanged;
        }

        void WireHudButtons()
        {
            if (Hud)
            {
                if (Hud.EndBoutButton) Hud.EndBoutButton.onClick.RemoveAllListeners();
                if (Hud.EndBoutButton) Hud.EndBoutButton.onClick.AddListener(OnEndBoutClicked);
                if (Hud.RestartButton) Hud.RestartButton.onClick.RemoveAllListeners();
                if (Hud.RestartButton) Hud.RestartButton.onClick.AddListener(BeginNewGame);
                if (Hud.PlayNormallyButton) Hud.PlayNormallyButton.onClick.RemoveAllListeners();
                if (Hud.PlayNormallyButton) Hud.PlayNormallyButton.onClick.AddListener(OnAbilityChoiceNormal);
                if (Hud.UseAbilityButton) Hud.UseAbilityButton.onClick.RemoveAllListeners();
                if (Hud.UseAbilityButton) Hud.UseAbilityButton.onClick.AddListener(OnAbilityChoiceUse);
                Hud.SetEndBoutEnabled(false);
                Hud.HideAbilityChoice();
            }

            if (Hud && Hud.AutoPlayButton)
            {
                Hud.AutoPlayButton.onClick.RemoveAllListeners();
                Hud.AutoPlayButton.onClick.AddListener(ToggleAutoPlay);
            }
            _autoPlay = false;
            UpdateAutoPlayLabel();

            CardView.OnHoverChanged = OnCardHover;
        }

        void OnSetupComplete()
        {
            // Build trump card visual (face-up, peeking out from under the deck, rotated 90°)
            _trumpView = SpawnCardView(Engine.TrumpCard, faceUp: true, parent: Table.TrumpSlot);
            var trumpRT = (RectTransform)_trumpView.transform;
            trumpRT.anchoredPosition = Vector2.zero;
            trumpRT.localRotation = Quaternion.Euler(0, 0, 90);

            // Build hands
            foreach (var c in Engine.HandOf(HumanPlayerIndex).Cards)
                AddHumanCardView(c);
            foreach (var _ in Engine.HandOf(1 - HumanPlayerIndex).Cards)
                AddOpponentCardView();

            UpdateHud();
            ApplyHighlightForPhase();
        }

        void OnTurnBegan(int playerIndex)
        {
            UpdateHud();
            ApplyHighlightForPhase();
        }

        void OnAttackPlayed(int attackerIndex, Card card)
        {
            int slot = Engine.Bout.AttackCount - 1;
            CardView view;
            if (attackerIndex == HumanPlayerIndex)
            {
                if (_humanCardViews.TryGetValue(card, out view))
                {
                    _humanCardViews.Remove(card);
                    Table.PlayerHand.Remove(view);
                }
                else
                {
                    view = SpawnCardView(card, faceUp: true, parent: Table.BoutArea);
                }
            }
            else
            {
                view = _opponentCardViews[0];
                _opponentCardViews.RemoveAt(0);
                Table.OpponentHand.Remove(view);
                view.Bind(card, faceUp: true);
            }

            view.transform.SetParent(Table.BoutArea, false);
            view.SetHighlight(CardView.Highlight.None);
            EnsureSlotList(_attackViews, slot + 1);
            _attackViews[slot] = view;
            view.OnClicked = null;
            view.transform.localRotation = Quaternion.identity;
            ((RectTransform)view.transform).anchoredPosition = Vector2.zero; // start at center, then animate
            RelayoutBout();

            UpdateHud();
            ApplyHighlightForPhase();
        }

        void OnDefensePlayed(int defenderIndex, int slot, Card card)
        {
            CardView view;
            if (defenderIndex == HumanPlayerIndex)
            {
                if (_humanCardViews.TryGetValue(card, out view))
                {
                    _humanCardViews.Remove(card);
                    Table.PlayerHand.Remove(view);
                }
                else
                {
                    view = SpawnCardView(card, faceUp: true, parent: Table.BoutArea);
                }
            }
            else
            {
                view = _opponentCardViews[0];
                _opponentCardViews.RemoveAt(0);
                Table.OpponentHand.Remove(view);
                view.Bind(card, faceUp: true);
            }

            view.transform.SetParent(Table.BoutArea, false);
            view.SetHighlight(CardView.Highlight.None);
            EnsureSlotList(_defenseViews, slot + 1);
            _defenseViews[slot] = view;
            view.OnClicked = null;
            view.transform.localRotation = Quaternion.Euler(0, 0, -8f);
            ((RectTransform)view.transform).anchoredPosition = Vector2.zero;
            RelayoutBout();

            UpdateHud();
            ApplyHighlightForPhase();
        }

        void RelayoutBout()
        {
            int n = _attackViews.Count;
            for (int i = 0; i < n; i++)
            {
                if (_attackViews[i])
                    StartCoroutine(MoveTo(_attackViews[i], Table.BoutAttackSlotPos(i, n), MoveSeconds));
                if (i < _defenseViews.Count && _defenseViews[i])
                    StartCoroutine(MoveTo(_defenseViews[i], Table.BoutDefenseSlotPos(i, n), MoveSeconds));
            }
        }

        void OnBoutResolved(BoutOutcome outcome)
        {
            // Move all bout cards to discard or to the eater's hand area.
            // Engine has already updated hand state; visuals follow the data.
            if (outcome == BoutOutcome.DefenderWonAllDiscarded)
            {
                foreach (var v in _attackViews) if (v) StartCoroutine(MoveAndDestroy(v, Table.DiscardSlot.position, MoveSeconds));
                foreach (var v in _defenseViews) if (v) StartCoroutine(MoveAndDestroy(v, Table.DiscardSlot.position, MoveSeconds));
            }
            else
            {
                // Defender ate. Convert visuals into hand-cards for the defender.
                int defenderBefore = Engine.AttackerIndex == HumanPlayerIndex ? 1 - HumanPlayerIndex : HumanPlayerIndex;
                bool defenderIsHuman = defenderBefore == HumanPlayerIndex;
                foreach (var v in _attackViews) if (v) AbsorbIntoHand(v, defenderIsHuman, faceUp: defenderIsHuman);
                foreach (var v in _defenseViews) if (v) AbsorbIntoHand(v, defenderIsHuman, faceUp: defenderIsHuman);
            }
            _attackViews.Clear();
            _defenseViews.Clear();
            UpdateHud();
        }

        void OnDrew(int playerIndex, int drawnCount)
        {
            if (playerIndex == HumanPlayerIndex)
            {
                // Add views for any cards in hand we don't have a view for yet.
                var data = Engine.HandOf(playerIndex).Cards;
                foreach (var c in data)
                    if (!_humanCardViews.ContainsKey(c))
                        AddHumanCardView(c);
            }
            else
            {
                int target = Engine.HandOf(playerIndex).Count;
                while (_opponentCardViews.Count < target) AddOpponentCardView();
            }
            UpdateHud();
        }

        void OnGameOver(int foolIndex)
        {
            int winnerIndex = 1 - foolIndex;
            string msg = foolIndex == HumanPlayerIndex
                ? "You are the Fool."
                : $"You won! {OpponentName} is the Fool.";
            Hud?.ShowGameOver(msg);
            OnMatchComplete?.Invoke(winnerIndex);
        }

        // ---------- Input ----------

        CardView _pendingAbilityView;

        void OnHumanCardClicked(CardView view)
        {
            if (Hud && Hud.AbilityChoiceVisible) return;

            if (view.Card.HasAbility && AbilityValidForPhase(view.Card.Ability.Value))
            {
                _pendingAbilityView = view;
                var ability = view.Card.Ability.Value;
                Hud?.ShowAbilityChoice(ability.DisplayName(), ability.Description(), $"Use {ability.ShortName()}");
                return;
            }

            PlayCardNormally(view);
        }

        void PlayCardNormally(CardView view)
        {
            if (Engine.Phase == Phase.Attack && Engine.AttackerIndex == HumanPlayerIndex)
            {
                Engine.TryAttack(HumanPlayerIndex, view.Card);
            }
            else if (Engine.Phase == Phase.Defense && Engine.DefenderIndex == HumanPlayerIndex)
            {
                int slot = Engine.Bout.FirstUndefendedSlot();
                if (slot >= 0) Engine.TryDefend(HumanPlayerIndex, slot, view.Card);
            }
        }

        void OnAbilityChoiceNormal()
        {
            Hud?.HideAbilityChoice();
            if (_pendingAbilityView)
            {
                PlayCardNormally(_pendingAbilityView);
                _pendingAbilityView = null;
            }
        }

        void OnAbilityChoiceUse()
        {
            Hud?.HideAbilityChoice();
            if (_pendingAbilityView)
            {
                int slot = Engine.Phase == Phase.Defense ? Engine.Bout.FirstUndefendedSlot() : -1;
                Engine.TryUseAbility(HumanPlayerIndex, _pendingAbilityView.Card, slot);
                _pendingAbilityView = null;
            }
        }

        void OnAbilityUsed(int playerIndex, Card card, AbilityType ability)
        {
            if (playerIndex == HumanPlayerIndex && _humanCardViews.TryGetValue(card, out var view))
            {
                _humanCardViews.Remove(card);
                Table.PlayerHand.Remove(view);
                StartCoroutine(MoveAndDestroy(view, Table.DiscardSlot.position, MoveSeconds));
            }
            else if (playerIndex != HumanPlayerIndex && _opponentCardViews.Count > 0)
            {
                var view2 = _opponentCardViews[0];
                _opponentCardViews.RemoveAt(0);
                Table.OpponentHand.Remove(view2);
                view2.Bind(card, faceUp: true);
                StartCoroutine(MoveAndDestroy(view2, Table.DiscardSlot.position, MoveSeconds));
            }
            UpdateHud();
            ApplyHighlightForPhase();
        }

        bool AbilityValidForPhase(AbilityType ability) => ability switch
        {
            AbilityType.TrumpChanger => !Engine.TrumpChangerUsed,
            AbilityType.ExtraDraw => Engine.Phase == Phase.Attack && Engine.DeckCount > 0,
            AbilityType.DoubleTrouble => Engine.Phase == Phase.Attack,
            AbilityType.Blocker => Engine.Phase == Phase.Defense,
            AbilityType.DoubleDefense => Engine.Phase == Phase.Defense,
            AbilityType.SeizeInitiative => true,
            _ => false
        };

        void OnCardHover(Card? card)
        {
            if (card.HasValue && card.Value.HasAbility)
            {
                var a = card.Value.Ability.Value;
                Hud?.ShowTooltip($"{a.DisplayName()} — {a.Description()}");
            }
            else Hud?.HideTooltip();
        }

        void OnTrumpChanged(Suit newSuit)
        {
            UpdateHud();
            ApplyHighlightForPhase();
        }

        void OnEndBoutClicked()
        {
            if (Hud && Hud.AbilityChoiceVisible) return;
            if (Engine.Phase == Phase.Attack && Engine.AttackerIndex == HumanPlayerIndex)
                Engine.TryEndBout(HumanPlayerIndex);
            else if (Engine.Phase == Phase.Defense && Engine.DefenderIndex == HumanPlayerIndex)
                Engine.TryEat(HumanPlayerIndex);
        }

        // ---------- Highlights & HUD ----------

        void ApplyHighlightForPhase()
        {
            bool humanAttack = Engine.Phase == Phase.Attack && Engine.AttackerIndex == HumanPlayerIndex;
            bool humanDefense = Engine.Phase == Phase.Defense && Engine.DefenderIndex == HumanPlayerIndex;
            bool humanActive = humanAttack || humanDefense;

            int defendSlot = humanDefense ? Engine.Bout.FirstUndefendedSlot() : -1;

            foreach (var kv in _humanCardViews)
            {
                var view = kv.Value;
                bool playable =
                    (humanAttack && Rules.CanAttackWith(Engine.Bout, view.Card)) ||
                    (humanDefense && defendSlot >= 0 && Rules.CanDefendSlotWith(Engine.Bout, defendSlot, view.Card, Engine.Trump));
                bool abilityUsable = humanActive && view.Card.HasAbility && AbilityValidForPhase(view.Card.Ability.Value);

                if (playable || abilityUsable)
                {
                    view.SetHighlight(CardView.Highlight.Playable);
                    view.OnClicked = OnHumanCardClicked;
                }
                else if (humanActive)
                {
                    view.SetHighlight(CardView.Highlight.Disabled);
                    view.OnClicked = null;
                }
                else
                {
                    view.SetHighlight(CardView.Highlight.None);
                    view.OnClicked = null;
                }
            }

            // End-bout / Eat button enable
            if (Hud)
            {
                if (humanAttack && !Engine.Bout.IsEmpty && Engine.Bout.FullyDefended)
                {
                    Hud.SetEndBoutEnabled(true);
                    SetEndBoutLabel("End bout");
                }
                else if (humanDefense)
                {
                    Hud.SetEndBoutEnabled(true);
                    SetEndBoutLabel("Take cards");
                }
                else
                {
                    Hud.SetEndBoutEnabled(false);
                    SetEndBoutLabel("End bout");
                }
            }
        }

        void SetEndBoutLabel(string text)
        {
            if (!Hud || !Hud.EndBoutButton) return;
            var lbl = Hud.EndBoutButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (lbl) lbl.text = text;
        }

        void UpdateHud()
        {
            if (!Hud) return;
            Hud.SetDeckCount(Engine.DeckCount);
            Hud.SetTrump(Engine.Trump);
            string phase;
            if (Engine.Phase == Phase.GameOver) phase = "Game over";
            else if (Engine.Phase == Phase.Attack)
                phase = Engine.AttackerIndex == HumanPlayerIndex ? "Your move — attack" : $"{OpponentName} attacks";
            else if (Engine.Phase == Phase.Defense)
                phase = Engine.DefenderIndex == HumanPlayerIndex ? "Defend!" : $"{OpponentName} defends";
            else phase = "...";
            Hud.SetTurn(phase);
        }

        // ---------- Visual helpers ----------

        void AddHumanCardView(Card card)
        {
            var view = SpawnCardView(card, faceUp: true, parent: Table.PlayerHand.transform);
            _humanCardViews[card] = view;
            Table.PlayerHand.Add(view);
            view.OnClicked = OnHumanCardClicked;
        }

        void AddOpponentCardView()
        {
            var view = SpawnCardView(default, faceUp: false, parent: Table.OpponentHand.transform);
            _opponentCardViews.Add(view);
            Table.OpponentHand.Add(view);
        }

        CardView SpawnCardView(Card card, bool faceUp, Transform parent)
        {
            var go = Instantiate(CardViewPrefab, parent);
            var view = go.GetComponent<CardView>();
            view.Bind(card, faceUp);
            return view;
        }

        void AbsorbIntoHand(CardView view, bool toHumanHand, bool faceUp)
        {
            view.SetHighlight(CardView.Highlight.None);
            view.OnClicked = null;
            view.SetFaceUp(faceUp);
            if (toHumanHand)
            {
                _humanCardViews[view.Card] = view;
                Table.PlayerHand.Add(view);
                view.OnClicked = OnHumanCardClicked;
            }
            else
            {
                _opponentCardViews.Add(view);
                Table.OpponentHand.Add(view);
            }
        }

        IEnumerator MoveTo(CardView view, Vector2 localTarget, float duration)
        {
            var rt = (RectTransform)view.transform;
            Vector2 from = rt.anchoredPosition;
            float t = 0;
            while (t < duration)
            {
                if (!view) yield break;
                t += Time.deltaTime;
                float e = EaseOutCubic(Mathf.Clamp01(t / duration));
                rt.anchoredPosition = Vector2.LerpUnclamped(from, localTarget, e);
                yield return null;
            }
            if (view) rt.anchoredPosition = localTarget;
        }

        IEnumerator MoveAndDestroy(CardView view, Vector3 worldTarget, float duration)
        {
            float t = 0;
            Vector3 from = view.transform.position;
            while (t < duration)
            {
                if (!view) yield break;
                t += Time.deltaTime;
                float e = EaseOutCubic(Mathf.Clamp01(t / duration));
                view.transform.position = Vector3.LerpUnclamped(from, worldTarget, e);
                yield return null;
            }
            if (view) Destroy(view.gameObject);
        }

        static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        void ClearAllVisuals()
        {
            if (Table)
            {
                if (Table.PlayerHand) Table.PlayerHand.Clear();
                if (Table.OpponentHand) Table.OpponentHand.Clear();
            }
            _humanCardViews.Clear();
            _opponentCardViews.Clear();
            foreach (var v in _attackViews) if (v) Destroy(v.gameObject);
            _attackViews.Clear();
            foreach (var v in _defenseViews) if (v) Destroy(v.gameObject);
            _defenseViews.Clear();
            if (_trumpView) Destroy(_trumpView.gameObject);
            _trumpView = null;
        }

        static void EnsureSlotList(List<CardView> list, int size)
        {
            while (list.Count < size) list.Add(null);
        }
    }
}
