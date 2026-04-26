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
            BeginNewGame();
        }

        public void BeginNewGame()
        {
            ClearAllVisuals();
            Hud?.HideGameOver();

            _loop = new GameLoop(
                p0: new HumanPlayer(PlayerName),
                p1: new AIPlayer(OpponentName));
            _humanIsPlayerZero = true;

            Engine.OnSetupComplete += OnSetupComplete;
            Engine.OnTurnBegan += OnTurnBegan;
            Engine.OnAttackPlayed += OnAttackPlayed;
            Engine.OnDefensePlayed += OnDefensePlayed;
            Engine.OnBoutResolved += OnBoutResolved;
            Engine.OnDrew += OnDrew;
            Engine.OnGameOver += OnGameOver;

            if (Hud)
            {
                if (Hud.EndBoutButton) Hud.EndBoutButton.onClick.RemoveAllListeners();
                if (Hud.EndBoutButton) Hud.EndBoutButton.onClick.AddListener(OnEndBoutClicked);
                if (Hud.RestartButton) Hud.RestartButton.onClick.RemoveAllListeners();
                if (Hud.RestartButton) Hud.RestartButton.onClick.AddListener(BeginNewGame);
                Hud.SetEndBoutEnabled(false);
            }

            _loop.Start();
        }

        void OnSetupComplete()
        {
            // Build trump card visual (face-up, under the deck)
            _trumpView = SpawnCardView(Engine.TrumpCard, faceUp: true, parent: Table.TrumpSlot);
            _trumpView.transform.localPosition = Vector3.zero;
            _trumpView.transform.localRotation = Quaternion.Euler(0, 0, 90);

            // Build hands
            foreach (var c in Engine.HandOf(HumanPlayerIndex).Cards)
                AddHumanCardView(c);
            foreach (var _ in Engine.HandOf(1 - HumanPlayerIndex).Cards)
                AddOpponentCardView();

            UpdateHud();
        }

        void OnTurnBegan(int playerIndex)
        {
            UpdateHud();
            ApplyHighlightForPhase();
            // AI thinks briefly before its action.
            if (Engine.Phase != Phase.GameOver && _loop.CurrentController.Kind == PlayerKind.AI)
                StartCoroutine(AiThink());
        }

        IEnumerator AiThink()
        {
            yield return new WaitForSeconds(AiThinkSeconds);
            // GameLoop already pumped on OnTurnBegan; the AI controller acted synchronously.
            // But because we're using events for everything, an immediate AI act has already happened.
            // This delay gives the player time to read the table.
            // We still want UI updates to keep flowing — re-apply highlights after delay.
            ApplyHighlightForPhase();
            UpdateHud();
        }

        void OnAttackPlayed(int attackerIndex, Card card)
        {
            int slot = Engine.Bout.AttackCount - 1;
            CardView view;
            if (attackerIndex == HumanPlayerIndex)
            {
                view = _humanCardViews[card];
                _humanCardViews.Remove(card);
                Table.PlayerHand.Remove(view);
            }
            else
            {
                view = _opponentCardViews[0];
                _opponentCardViews.RemoveAt(0);
                Table.OpponentHand.Remove(view);
                view.Bind(card, faceUp: true);
            }

            view.transform.SetParent(Table.BoutArea, true);
            view.SetHighlight(CardView.Highlight.None);
            EnsureSlotList(_attackViews, slot + 1);
            _attackViews[slot] = view;
            view.OnClicked = null;
            view.transform.localRotation = Quaternion.identity;
            StartCoroutine(MoveTo(view, Table.BoutAttackSlotPos(slot), MoveSeconds));

            UpdateHud();
            ApplyHighlightForPhase();
            if (Engine.Phase != Phase.GameOver && _loop.CurrentController.Kind == PlayerKind.AI)
                StartCoroutine(AiThink());
        }

        void OnDefensePlayed(int defenderIndex, int slot, Card card)
        {
            CardView view;
            if (defenderIndex == HumanPlayerIndex)
            {
                view = _humanCardViews[card];
                _humanCardViews.Remove(card);
                Table.PlayerHand.Remove(view);
            }
            else
            {
                view = _opponentCardViews[0];
                _opponentCardViews.RemoveAt(0);
                Table.OpponentHand.Remove(view);
                view.Bind(card, faceUp: true);
            }

            view.transform.SetParent(Table.BoutArea, true);
            view.SetHighlight(CardView.Highlight.None);
            EnsureSlotList(_defenseViews, slot + 1);
            _defenseViews[slot] = view;
            view.OnClicked = null;
            view.transform.localRotation = Quaternion.Euler(0, 0, -8f);
            StartCoroutine(MoveTo(view, Table.BoutDefenseSlotPos(slot), MoveSeconds));

            UpdateHud();
            ApplyHighlightForPhase();
            if (Engine.Phase != Phase.GameOver && _loop.CurrentController.Kind == PlayerKind.AI)
                StartCoroutine(AiThink());
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
            string msg = foolIndex == HumanPlayerIndex
                ? "You are the Fool."
                : $"You won! {OpponentName} is the Fool.";
            Hud?.ShowGameOver(msg);
        }

        // ---------- Input ----------

        void OnHumanCardClicked(CardView view)
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

        void OnEndBoutClicked()
        {
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

            int defendSlot = humanDefense ? Engine.Bout.FirstUndefendedSlot() : -1;

            foreach (var kv in _humanCardViews)
            {
                var view = kv.Value;
                view.OnClicked = OnHumanCardClicked;
                if (humanAttack && Rules.CanAttackWith(Engine.Bout, view.Card))
                    view.SetHighlight(CardView.Highlight.Playable);
                else if (humanDefense && defendSlot >= 0 && Rules.CanDefendSlotWith(Engine.Bout, defendSlot, view.Card, Engine.Trump))
                    view.SetHighlight(CardView.Highlight.Playable);
                else
                    view.SetHighlight(CardView.Highlight.None);
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
            foreach (var kv in _humanCardViews) if (kv.Value) Destroy(kv.Value.gameObject);
            _humanCardViews.Clear();
            foreach (var v in _opponentCardViews) if (v) Destroy(v.gameObject);
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
