using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public GameEngine Engine => _loop?.Engine;
        readonly Dictionary<Card, CardView> _humanCardViews = new();
        readonly List<CardView> _opponentCardViews = new();
        readonly List<CardView> _attackViews = new();   // bout slot index -> view
        readonly List<CardView> _defenseViews = new();
        CardView _trumpView;
        readonly int[] _matchStartTotals = new int[2];
        bool _humanIsPlayerZero;

        const int HumanPlayerIndex = 0;

        void Start()
        {
            if (AutoStartOnAwake) BeginNewGame();
        }

        float _earliestNextAiAct;
        bool _autoPlay;
        int _autoPlaySpeed; // 0=off, 1=1x, 2=2x
        AIPlayer _autoPlayAI;
        float _baseAiThinkSeconds;
        float _baseMoveSeconds;
        float _baseDealSeconds;

        int _stallFrames;
        int _matchFrames;


        void Update()
        {
            if (_loop == null) return;

            if (Input.GetKeyDown(KeyCode.A) && Engine.Phase != Phase.GameOver)
                CycleAutoPlay();

            if (Engine.Phase == Phase.GameOver) return;

            if (_awaitingStackPutBack || Engine.AwaitingStackPutBack(HumanPlayerIndex))
            {
                if (_autoPlay && Engine.AwaitingStackPutBack(HumanPlayerIndex))
                {
                    _awaitingStackPutBack = false;
                    if (_stackPutBackCoroutine != null) { StopCoroutine(_stackPutBackCoroutine); _stackPutBackCoroutine = null; }
                    Hud?.HideAbilityFeedback();
                    AutoCompleteStackPutBack();
                }
                return;
            }

            int aiIndex = 1 - HumanPlayerIndex;
            if (Engine.AwaitingStackPutBack(aiIndex) && _loop.Controllers[aiIndex].Kind == PlayerKind.AI)
            {
                _loop.Controllers[aiIndex].RequestAction(Engine, aiIndex);
                ReconcileHumanCardViews();
                UpdateHud();
            }

            _matchFrames++;

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
            if (!on) _autoPlaySpeed = 0;
            else if (_autoPlaySpeed == 0) _autoPlaySpeed = 1;
            if (_autoPlay && Hud && Hud.AbilityChoiceVisible)
                Hud.HideAbilityChoice();
            UpdateAutoPlayLabel();
            if (_autoPlay) ApplyHighlightForPhase();
        }

        void CycleAutoPlay()
        {
            _autoPlaySpeed = (_autoPlaySpeed + 1) % 3;
            _autoPlay = _autoPlaySpeed > 0;
            if (_autoPlay && Hud && Hud.AbilityChoiceVisible)
                Hud.HideAbilityChoice();
            ApplyAutoPlaySpeed();
            UpdateAutoPlayLabel();
            if (_autoPlay) ApplyHighlightForPhase();
        }

        void ApplyAutoPlaySpeed()
        {
            if (_baseAiThinkSeconds == 0f) _baseAiThinkSeconds = AiThinkSeconds;
            if (_baseMoveSeconds == 0f) _baseMoveSeconds = MoveSeconds;
            if (_baseDealSeconds == 0f) _baseDealSeconds = DealSeconds;

            switch (_autoPlaySpeed)
            {
                case 0:
                    AiThinkSeconds = _baseAiThinkSeconds;
                    MoveSeconds = _baseMoveSeconds;
                    DealSeconds = _baseDealSeconds;
                    Time.timeScale = 1f;
                    break;
                case 1:
                    AiThinkSeconds = _baseAiThinkSeconds;
                    MoveSeconds = _baseMoveSeconds;
                    DealSeconds = _baseDealSeconds;
                    Time.timeScale = 1f;
                    break;
                case 2:
                    AiThinkSeconds = _baseAiThinkSeconds * 0.5f;
                    MoveSeconds = _baseMoveSeconds * 0.5f;
                    DealSeconds = _baseDealSeconds * 0.5f;
                    Time.timeScale = 2f;
                    break;
            }
        }

        void UpdateAutoPlayLabel()
        {
            if (!Hud || !Hud.AutoPlayButton) return;
            var lbl = Hud.AutoPlayButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (lbl) lbl.text = _autoPlaySpeed switch
            {
                1 => "Auto 1x",
                2 => "Auto 2x",
                _ => "Auto Off"
            };
        }

        public void BeginGame(MatchConfig config, PlayerDeck playerDeck, PlayerDeck enemyDeck,
            OpponentProfile opponent, int? seed = null)
        {
            OpponentName = opponent.Name;
            ClearAllVisuals();
            Hud?.HideGameOver();

            var engine = new GameEngine(seed, config, playerDeck, enemyDeck);
            var ai = new AIPlayer(opponent.Name, seed ?? 0);
            AIArchetypes.Apply(ai, opponent.Archetype, opponent.ActIndex);
            _loop = new GameLoop(
                p0: new HumanPlayer(PlayerName),
                p1: ai,
                engine: engine);
            _humanIsPlayerZero = true;
            _matchFrames = 0;

            WireEngineEvents();
            WireHudButtons();
            _loop.Start();
            Debug.Log($"[GameManager] Match started: autoPlay={_autoPlay} autoPlaySpeed={_autoPlaySpeed} timeScale={Time.timeScale} " +
                $"p0Hand={engine.HandOf(0).Count} p1Hand={engine.HandOf(1).Count} deckTotal={engine.DeckCount} " +
                $"attacker={engine.AttackerIndex} controller0={_loop.Controllers[0].Kind} controller1={_loop.Controllers[1].Kind}");
            StartCoroutine(AutoHideInfo(3f));
        }

        IEnumerator AutoHideInfo(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Hud?.SetInfo(null);
        }

        public void BeginNewGame()
        {
            ClearAllVisuals();
            Hud?.HideGameOver();

            var config = MatchConfig.Default();
            var deck0 = PlayerDeck.CreateStandard(config.Abilities);
            var deck1 = PlayerDeck.CreateStandard(config.Abilities);
            var engine = new GameEngine(null, config, deck0, deck1);
            _loop = new GameLoop(
                p0: new HumanPlayer(PlayerName),
                p1: new AIPlayer(OpponentName),
                engine: engine);
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
            Engine.OnResourceChanged += OnResourceChanged;
            Engine.OnStackPutBackRequired += OnStackPutBackRequired;
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
                if (Hud.CancelAbilityButton) Hud.CancelAbilityButton.onClick.RemoveAllListeners();
                if (Hud.CancelAbilityButton) Hud.CancelAbilityButton.onClick.AddListener(OnAbilityChoiceCancel);
                if (Hud.PeekDismissButton) Hud.PeekDismissButton.onClick.RemoveAllListeners();
                if (Hud.PeekDismissButton) Hud.PeekDismissButton.onClick.AddListener(DismissPeekOverlay);
                Hud.SetEndBoutEnabled(false);
                Hud.HideAbilityChoice();
                Hud.HidePeekOverlay();
            }

            if (Hud && Hud.AutoPlayButton)
            {
                Hud.AutoPlayButton.onClick.RemoveAllListeners();
                Hud.AutoPlayButton.onClick.AddListener(CycleAutoPlay);
            }
            _autoPlay = false;
            _autoPlaySpeed = 0;
            UpdateAutoPlayLabel();

            CardView.OnHoverChanged = OnCardHover;
        }

        void OnSetupComplete()
        {
            // Trump card sits upright in the trump panel (the panel itself is tilted).
            _trumpView = SpawnCardView(Engine.TrumpCard, faceUp: true, parent: Table.TrumpSlot);
            var trumpRT = (RectTransform)_trumpView.transform;
            trumpRT.anchorMin = trumpRT.anchorMax = new Vector2(0.5f, 0.5f);
            trumpRT.anchoredPosition = Vector2.zero;
            trumpRT.localRotation = Quaternion.identity;

            // Win-condition baseline for the race meters.
            for (int p = 0; p < 2; p++)
                _matchStartTotals[p] = Engine.HandCount(p) + Engine.DeckCountOf(p);
            Hud?.ClearLog();

            // Build hands
            foreach (var c in Engine.HandOf(HumanPlayerIndex).Cards)
                AddHumanCardView(c);
            foreach (var _ in Engine.HandOf(1 - HumanPlayerIndex).Cards)
                AddOpponentCardView();

            UpdateHud();
            ApplyHighlightForPhase();

            for (int p = 0; p < 2; p++)
            {
                var rt = Engine.GetResourceType(p);
                if (rt.HasValue)
                    Hud.SetResource(p, rt.Value, 0);
                else
                    Hud.HideResource(p);
            }

            if (Engine.Config.MarkedDeck[HumanPlayerIndex])
            {
                var marked = Engine.GetMarkedCards(HumanPlayerIndex, 3);
                if (marked.Count > 0)
                {
                    var names = new System.Collections.Generic.List<string>();
                    foreach (var c in marked) names.Add(c.ToString());
                    Hud.SetInfo($"Marked: {string.Join(", ", names)}");
                }
            }
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
                if (_opponentCardViews.Count > 0)
                {
                    view = _opponentCardViews[0];
                    _opponentCardViews.RemoveAt(0);
                    Table.OpponentHand.Remove(view);
                    view.Bind(card, faceUp: true);
                }
                else
                    view = SpawnCardView(card, faceUp: true, parent: Table.BoutArea);
            }

            view.transform.SetParent(Table.BoutArea, false);
            view.SetHighlight(CardView.Highlight.None);
            EnsureSlotList(_attackViews, slot + 1);
            _attackViews[slot] = view;
            view.OnClicked = null;
            view.transform.localRotation = Quaternion.identity;
            ((RectTransform)view.transform).anchoredPosition = Vector2.zero; // start at center, then animate
            RelayoutBout();

            PushLog(attackerIndex, $"attacks: {CardLogName(card)}");
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
                if (_opponentCardViews.Count > 0)
                {
                    view = _opponentCardViews[0];
                    _opponentCardViews.RemoveAt(0);
                    Table.OpponentHand.Remove(view);
                    view.Bind(card, faceUp: true);
                }
                else
                    view = SpawnCardView(card, faceUp: true, parent: Table.BoutArea);
            }

            view.transform.SetParent(Table.BoutArea, false);
            view.SetHighlight(CardView.Highlight.None);
            EnsureSlotList(_defenseViews, slot + 1);
            _defenseViews[slot] = view;
            view.OnClicked = null;
            view.transform.localRotation = Quaternion.Euler(0, 0, -8f);
            ((RectTransform)view.transform).anchoredPosition = Vector2.zero;
            RelayoutBout();

            // Trump plays are self-explaining: flag the defense that used trump on a non-trump attack.
            bool trumped = card.Suit == Engine.Trump
                && slot < Engine.Bout.AttackCount && Engine.Bout.Attacks[slot].Suit != Engine.Trump;
            view.SetTrumpFlag(trumped, Engine.Trump);
            PushLog(defenderIndex, trumped
                ? $"defends with trump: {CardLogName(card)}"
                : $"defends: {CardLogName(card)}", highlight: trumped);

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
            foreach (var c in _loop.Controllers)
                if (c is AIPlayer ai) ai.NotifyBoutResolved();

            // Move all bout cards to the removed pile or to the eater's hand area.
            // Engine has already updated hand state; visuals follow the data.
            if (outcome == BoutOutcome.DefenderWonAllDiscarded)
            {
                int eaten = 0;
                foreach (var v in _attackViews) if (v) { StartCoroutine(MoveAndDestroy(v, Table.DiscardSlot.position, MoveSeconds)); eaten++; }
                foreach (var v in _defenseViews) if (v) { StartCoroutine(MoveAndDestroy(v, Table.DiscardSlot.position, MoveSeconds)); eaten++; }
                if (eaten > 0) Hud?.PushLog($"Bout ends — {eaten} cards removed from the game");
            }
            else
            {
                // Defender ate. Convert visuals into hand-cards for the defender.
                int defenderBefore = Engine.AttackerIndex == HumanPlayerIndex ? 1 - HumanPlayerIndex : HumanPlayerIndex;
                bool defenderIsHuman = defenderBefore == HumanPlayerIndex;
                int eaten = 0;
                foreach (var v in _attackViews) if (v) { AbsorbIntoHand(v, defenderIsHuman, faceUp: defenderIsHuman); eaten++; }
                foreach (var v in _defenseViews) if (v) { AbsorbIntoHand(v, defenderIsHuman, faceUp: defenderIsHuman); eaten++; }
                PushLog(defenderBefore, $"eats {eaten} cards", highlight: true);
            }
            _attackViews.Clear();
            _defenseViews.Clear();
            UpdateHud();
        }

        // ---------- Event log helpers ----------

        void PushLog(int playerIndex, string action, bool highlight = false)
        {
            if (!Hud) return;
            string who = playerIndex == HumanPlayerIndex ? "You" : OpponentName;
            Hud.PushLog($"<b>{who}</b> {action}", highlight);
        }

        string CardLogName(Card card)
        {
            string rank = $"{card.Rank.Label()}{card.Suit.Glyph()}";
            if (card.DefinitionId != null && CardCatalog.TryGet(card.DefinitionId, out var def))
                return $"{rank} {def.Name}";
            return rank;
        }

        void OnDrew(int playerIndex, int drawnCount)
        {
            UpdateHud();
        }

        void OnGameOver(int foolIndex)
        {
            Debug.Log($"[GameManager] GAME OVER: fool={foolIndex} boutCount={Engine?.BoutCount} matchFrames={_matchFrames} " +
                $"stack={StackTraceUtility.ExtractStackTrace()}");
            int winnerIndex = 1 - foolIndex;
            string msg = foolIndex == HumanPlayerIndex
                ? "You are the Fool."
                : $"You won! {OpponentName} is the Fool.";
            Hud?.ShowGameOver(msg);
            OnMatchComplete?.Invoke(winnerIndex);
        }

        // ---------- Input ----------

        CardView _pendingAbilityView;

        CardView _touchSelected;

        void OnHumanCardClicked(CardView view)
        {
            if (Hud && Hud.AbilityChoiceVisible) return;

            // Touch: first tap selects + inspects (no commit); a second tap on the same card commits.
            // Pointer: hover already inspects, so a click commits immediately.
            if (!InputProfile.Hover)
            {
                if (_touchSelected != view)
                {
                    SelectForTouch(view);
                    return;
                }
                DeselectTouch();   // second tap on the same card falls through to commit
            }

            CommitCardAction(view);
        }

        void SelectForTouch(CardView view)
        {
            if (_touchSelected && _touchSelected != view) _touchSelected.SetSelected(false);
            _touchSelected = view;
            view.SetSelected(true);
            if (Hud)
            {
                if (view.Card.HasAbility)
                {
                    var a = view.Card.Ability.Value;
                    Hud.ShowTooltip($"{a.DisplayName()} — {a.Description()}");
                }
                else Hud.ShowTooltip(view.Card.ToString());
            }
        }

        void DeselectTouch()
        {
            if (_touchSelected) _touchSelected.SetSelected(false);
            _touchSelected = null;
            Hud?.HideTooltip();
        }

        void CommitCardAction(CardView view)
        {
            if (view.Card.HasAbility && view.Card.Trigger == TriggerTiming.None)
            {
                bool canPlay = CanPlayCardNormally(view.Card);
                bool canUse = AbilityValidForPhase(view.Card.Ability.Value);
                if (canPlay || canUse)
                {
                    _pendingAbilityView = view;
                    var ability = view.Card.Ability.Value;
                    Hud?.ShowAbilityChoice(ability.DisplayName(), ability.Description(),
                        $"Use {ability.ShortName()}", canPlay, canUse);
                    UIScreenCapture.Instance?.NotifyModal(UIScreen.AbilityChoiceModal);
                    return;
                }
            }

            PlayCardNormally(view);
        }

        bool CanPlayCardNormally(Card card)
        {
            if (Engine.Phase == Phase.Attack && Engine.AttackerIndex == HumanPlayerIndex)
                return Rules.CanAttackWith(Engine.Bout, card);
            if (Engine.Phase == Phase.Defense && Engine.DefenderIndex == HumanPlayerIndex)
            {
                int slot = Engine.Bout.FirstUndefendedSlot();
                return slot >= 0 && Rules.CanDefendSlotWith(Engine.Bout, slot, card, Engine.Trump);
            }
            return false;
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

        void OnAbilityChoiceCancel()
        {
            Hud?.HideAbilityChoice();
            _pendingAbilityView = null;
        }

        void OnAbilityUsed(int playerIndex, Card card, AbilityType ability)
        {
            bool autoTriggered = card.Trigger == TriggerTiming.OnAttack || card.Trigger == TriggerTiming.OnDefend;
            if (!autoTriggered)
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
            }
            ShowAbilityFeedback(playerIndex, card, ability);
            UpdateHud();
            ApplyHighlightForPhase();
        }

        Coroutine _feedbackCoroutine;
        bool _peekDeckTopActive;
        void ShowAbilityFeedback(int playerIndex, Card card, AbilityType ability)
        {
            if (Hud == null) return;
            string who = playerIndex == HumanPlayerIndex ? "You" : OpponentName;
            string name = card.DefinitionId != null && CardCatalog.TryGet(card.DefinitionId, out var def) ? def.Name : card.ToString();
            string text = $"{who}: {name} → {ability.DisplayName()}";
            PushLog(playerIndex, $"{name} → {ability.DisplayName()}", highlight: true);

            if (ability == AbilityType.Peek && playerIndex == HumanPlayerIndex)
            {
                ShowPeekOverlay(playerIndex);
                return;
            }

            var color = playerIndex == HumanPlayerIndex ? ThemePalette.Gold : ThemePalette.VenetianRed;
            Hud.ShowAbilityFeedback(text, color);
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(HideFeedbackAfter(1.2f));
        }

        void ShowPeekOverlay(int playerIndex)
        {
            var top = Engine.PeekTopCards(playerIndex, 3);
            if (top.Length == 0 || Hud == null || Hud.PeekCardContainer == null) return;

            Hud.SetDeckTop(top[0]);
            _peekDeckTopActive = true;

            // Activate panel BEFORE spawning cards so CardView.Awake() runs
            Hud.ShowPeekOverlay();
            UIScreenCapture.Instance?.NotifyModal(UIScreen.PeekOverlay);

            for (int i = 0; i < top.Length; i++)
            {
                var view = SpawnCardView(top[i], faceUp: true, Hud.PeekCardContainer);
                var rt = view.GetComponent<RectTransform>();
                float spacing = 150f;
                float totalWidth = (top.Length - 1) * spacing;
                rt.anchoredPosition = new Vector2(-totalWidth / 2f + i * spacing, 0);
            }

            if (Hud.PeekNextDrawLabel)
            {
                Hud.PeekNextDrawLabel.gameObject.SetActive(true);
                Hud.PeekNextDrawLabel.text = $"Next draw: {top[0]}";
            }

            if (Hud.PeekDismissButton)
            {
                Hud.PeekDismissButton.onClick.RemoveAllListeners();
                Hud.PeekDismissButton.onClick.AddListener(DismissPeekOverlay);
            }

            if (_autoPlay)
                StartCoroutine(AutoDismissPeek());
        }

        IEnumerator AutoDismissPeek()
        {
            yield return new WaitForSeconds(0.3f);
            DismissPeekOverlay();
        }

        void DismissPeekOverlay()
        {
            Hud?.HidePeekOverlay();
            if (_peekDeckTopActive)
            {
                _peekDeckTopActive = false;
                if (!Engine.Config.SpysMonocle[HumanPlayerIndex])
                    Hud?.SetDeckTop(null);
            }
        }

        IEnumerator HideFeedbackAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Hud?.HideAbilityFeedback();
            if (_peekDeckTopActive)
            {
                _peekDeckTopActive = false;
                if (!Engine.Config.SpysMonocle[HumanPlayerIndex])
                    Hud?.SetDeckTop(null);
            }
            _feedbackCoroutine = null;
        }

        // ---------- StackTheDeck: draw 2 then choose 1 to put back ----------

        bool _awaitingStackPutBack;
        Coroutine _stackPutBackCoroutine;

        void OnStackPutBackRequired(int playerIndex)
        {
            if (playerIndex != HumanPlayerIndex) return;
            if (_autoPlay)
            {
                AutoCompleteStackPutBack();
                return;
            }
            _stackPutBackCoroutine = StartCoroutine(StackPutBackSequence());
        }

        void AutoCompleteStackPutBack()
        {
            var hand = Engine.HandOf(HumanPlayerIndex).Cards;
            Card? worst = null;
            int worstRank = int.MaxValue;
            foreach (var c in hand)
            {
                int rank = (int)c.Rank;
                if (c.Suit != Engine.Trump && rank < worstRank) { worst = c; worstRank = rank; }
            }
            worst ??= hand.Count > 0 ? hand[0] : (Card?)null;
            if (worst.HasValue)
            {
                Engine.CompleteStackPutBack(HumanPlayerIndex, worst.Value);
                ReconcileHumanCardViews();
                UpdateHud();
            }
        }

        IEnumerator StackPutBackSequence()
        {
            var oldCards = new HashSet<Card>(_humanCardViews.Keys);
            ReconcileHumanCardViews();
            UpdateHud();

            var newViews = new List<CardView>();
            foreach (var kv in _humanCardViews)
                if (!oldCards.Contains(kv.Key))
                    newViews.Add(kv.Value);

            var playerDeckOrigin = Table.PlayerDeckSlot ? Table.PlayerDeckSlot : Table.DeckSlot;
            if (newViews.Count > 0 && playerDeckOrigin)
            {
                var handRT = (RectTransform)Table.PlayerHand.transform;
                var targets = new Vector2[newViews.Count];
                for (int i = 0; i < newViews.Count; i++)
                    targets[i] = ((RectTransform)newViews[i].transform).anchoredPosition;

                Vector2 deckLocal;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    handRT, RectTransformUtility.WorldToScreenPoint(null, playerDeckOrigin.position),
                    null, out deckLocal);

                foreach (var v in newViews)
                {
                    ((RectTransform)v.transform).anchoredPosition = deckLocal;
                    v.SetFaceUp(false);
                }
                yield return null;

                foreach (var v in newViews)
                    v.SetFaceUp(true);

                float elapsed = 0f;
                float duration = MoveSeconds * 1.5f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float e = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                    for (int i = 0; i < newViews.Count; i++)
                    {
                        if (!newViews[i]) continue;
                        ((RectTransform)newViews[i].transform).anchoredPosition =
                            Vector2.LerpUnclamped(deckLocal, targets[i], e);
                    }
                    yield return null;
                }

                for (int i = 0; i < newViews.Count; i++)
                    if (newViews[i])
                        ((RectTransform)newViews[i].transform).anchoredPosition = targets[i];
            }

            foreach (var kv in _humanCardViews)
                StartCoroutine(JiggleCard((RectTransform)kv.Value.transform));
            yield return new WaitForSeconds(0.25f);

            _awaitingStackPutBack = true;
            foreach (var kv in _humanCardViews)
            {
                kv.Value.SetHighlight(CardView.Highlight.Playable);
                kv.Value.OnClicked = OnStackPutBackCardClicked;
            }

            Hud?.ShowAbilityFeedback("Choose a card to put on top of your deck", ThemePalette.Gold);
        }

        IEnumerator JiggleCard(RectTransform rt)
        {
            if (!rt) yield break;
            var original = rt.anchoredPosition;
            float amplitude = 6f;
            float duration = 0.2f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float offset = Mathf.Sin(t * Mathf.PI * 3f) * amplitude * (1f - t);
                if (rt) rt.anchoredPosition = original + new Vector2(0, offset);
                yield return null;
            }
            if (rt) rt.anchoredPosition = original;
        }

        void OnStackPutBackCardClicked(CardView view)
        {
            if (!_awaitingStackPutBack) return;
            _awaitingStackPutBack = false;

            bool ok = Engine.CompleteStackPutBack(HumanPlayerIndex, view.Card);
            if (!ok)
            {
                _awaitingStackPutBack = true;
                return;
            }

            _humanCardViews.Remove(view.Card);
            Table.PlayerHand.Remove(view);
            view.SetFaceUp(false);
            var deckTarget = Table.PlayerDeckSlot ? Table.PlayerDeckSlot : Table.DeckSlot;
            var deckPos = deckTarget ? deckTarget.position : Table.DiscardSlot.position;
            StartCoroutine(MoveAndDestroy(view, deckPos, MoveSeconds));

            Hud?.HideAbilityFeedback();
            UpdateHud();
            ApplyHighlightForPhase();
        }

        bool AbilityValidForPhase(AbilityType ability) => ability switch
        {
            AbilityType.TrumpChanger => !Engine.TrumpChangerUsedBy(HumanPlayerIndex),
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
            // Flip the trump card visual to the new suit.
            if (_trumpView)
                _trumpView.Bind(new Card(newSuit, Engine.TrumpCard.Rank, null), faceUp: true);
            Hud?.PushLog($"Trump changes to {newSuit.Glyph()} {newSuit}", highlight: true);
            UpdateHud();
            ApplyHighlightForPhase();
        }

        void OnResourceChanged(int playerIndex, ResourceType type, int amount)
        {
            if (Hud) Hud.SetResource(playerIndex, type, amount);
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

            // Only drop a pending touch selection if it's gone stale — never mid-turn between the two
            // taps (B1): clear when it's no longer the human's turn or the selected card left the hand.
            if (_touchSelected == null || !humanActive || !_humanCardViews.ContainsValue(_touchSelected))
                DeselectTouch();

            int defendSlot = humanDefense ? Engine.Bout.FirstUndefendedSlot() : -1;

            foreach (var kv in _humanCardViews)
            {
                var view = kv.Value;
                bool playable =
                    (humanAttack && Rules.CanAttackWith(Engine.Bout, view.Card)) ||
                    (humanDefense && defendSlot >= 0 && Rules.CanDefendSlotWith(Engine.Bout, defendSlot, view.Card, Engine.Trump));
                bool abilityUsable = humanActive && view.Card.HasAbility
                    && view.Card.Trigger == TriggerTiming.None
                    && AbilityValidForPhase(view.Card.Ability.Value);

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
            int playerDeck = Engine.DeckCountOf(HumanPlayerIndex);
            int oppDeck = Engine.DeckCountOf(1 - HumanPlayerIndex);
            Hud.SetDeckCounts(playerDeck, oppDeck);
            if (Table)
            {
                if (Table.PlayerDeckCountBadge)
                {
                    Table.PlayerDeckCountBadge.text = playerDeck.ToString();
                    Table.PlayerDeckCountBadge.color = playerDeck == 0 ? ThemePalette.VenetianRed : ThemePalette.Gold;
                }
                if (Table.OpponentDeckCountBadge)
                {
                    Table.OpponentDeckCountBadge.text = oppDeck.ToString();
                    Table.OpponentDeckCountBadge.color = oppDeck == 0 ? ThemePalette.VenetianRed : ThemePalette.Gold;
                }
                if (Table.TrumpRuleLabel)
                    Table.TrumpRuleLabel.text = $"{Engine.Trump} beat any other suit";
            }
            Hud.SetTrump(Engine.Trump);
            Hud.SetBoutChip(Engine.BoutCount + 1, Engine.Config.MaxBouts);

            string phase;
            Color boutColor;
            if (Engine.Phase == Phase.GameOver) { phase = "GAME OVER"; boutColor = ThemePalette.WarmSlate; }
            else if (Engine.Phase == Phase.Attack)
            {
                bool yourTurn = Engine.AttackerIndex == HumanPlayerIndex;
                int undefended = Engine.UndefendedCount;
                string sub = undefended == 0 ? "all attacks parried" : $"{undefended} open";
                phase = yourTurn ? $"YOUR ATTACK · {sub}" : $"{OpponentName.ToUpper()} ATTACKS";
                boutColor = yourTurn ? ThemePalette.Sage : ThemePalette.VenetianRed;
            }
            else if (Engine.Phase == Phase.Defense)
            {
                bool yourTurn = Engine.DefenderIndex == HumanPlayerIndex;
                int undefended = Engine.UndefendedCount;
                phase = yourTurn ? $"DEFEND! · {undefended} open" : $"{OpponentName.ToUpper()} DEFENDS";
                boutColor = yourTurn ? ThemePalette.VenetianRed : ThemePalette.Sage;
            }
            else { phase = "..."; boutColor = ThemePalette.WarmSlate; }
            Hud.SetBoutState(phase, boutColor);
            Hud.SetActionZone(phase, boutColor);

            int playerCards = Engine.HandCount(HumanPlayerIndex);
            int oppCards = Engine.HandCount(1 - HumanPlayerIndex);
            Hud.SetHandCounts(playerCards, oppCards);

            // Race to zero: remaining = hand + draw pile; first to shed everything wins.
            Hud.SetRace(0, playerCards + playerDeck, _matchStartTotals[HumanPlayerIndex]);
            Hud.SetRace(1, oppCards + oppDeck, _matchStartTotals[1 - HumanPlayerIndex]);

            ReconcileHumanCardViews();
            ReconcileOpponentCardViews(oppCards);

            // Rank-bonus chips on bout attacks (Conquer / Heavy Hand)
            for (int i = 0; i < _attackViews.Count; i++)
                if (_attackViews[i])
                    _attackViews[i].SetBonus(i < Engine.Bout.AttackCount ? Engine.Bout.BonusAt(i) : 0);

            if (Table && Table.DiscardCountLabel)
            {
                Table.DiscardCountLabel.gameObject.SetActive(true);
                int removed = Engine.DiscardCountOf(0) + Engine.DiscardCountOf(1);
                Table.DiscardCountLabel.text = removed.ToString();
            }

            if (Engine.Config.SpysMonocle[HumanPlayerIndex])
                Hud.SetDeckTop(Engine.DeckTopCard);
            else if (!_peekDeckTopActive)
                Hud.SetDeckTop(null);
        }

        // ---------- View reconciliation ----------

        void ReconcileHumanCardViews()
        {
            var engineCards = Engine.HandOf(HumanPlayerIndex).Cards;
            var stale = new List<Card>();
            foreach (var kvp in _humanCardViews)
                if (!engineCards.Contains(kvp.Key))
                    stale.Add(kvp.Key);
            foreach (var c in stale)
            {
                var v = _humanCardViews[c];
                _humanCardViews.Remove(c);
                Table.PlayerHand.Remove(v);
                Destroy(v.gameObject);
            }
            foreach (var c in engineCards)
                if (!_humanCardViews.ContainsKey(c))
                    AddHumanCardView(c);
        }

        void ReconcileOpponentCardViews(int target)
        {
            while (_opponentCardViews.Count > target)
            {
                var v = _opponentCardViews[^1];
                _opponentCardViews.RemoveAt(_opponentCardViews.Count - 1);
                Table.OpponentHand.Remove(v);
                Destroy(v.gameObject);
            }
            while (_opponentCardViews.Count < target)
                AddOpponentCardView();
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
