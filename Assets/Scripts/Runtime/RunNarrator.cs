using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WitsAndFools
{
    public sealed class RunNarrator : MonoBehaviour
    {
        const string OutputDir = "Screenshots/narrative";
        const float NarratorTimeScale = 3f;
        const float AiThinkDelay = 0.05f;
        const int SettleFrames = 8;

        RunManager _rm;
        GameManager _gm;
        GameEngine _engine;
        RunPhase _lastPhase;
        int _shotIndex;
        bool _capturing;
        bool _subscribed;
        int _matchNumber;
        int _boutNumber;
        string _opponentName;

        readonly List<NarrativeEntry> _entries = new();
        readonly StringBuilder _matchLog = new();

        struct NarrativeEntry
        {
            public int index;
            public string imagePath;
            public string title;
            public string body;
            public string category;
        }

        void Awake()
        {
            if (Directory.Exists(OutputDir))
                Directory.Delete(OutputDir, true);
            Directory.CreateDirectory(OutputDir);
        }

        void Start()
        {
            _rm = FindFirstObjectByType<RunManager>();
            _gm = _rm ? _rm.GameManager : FindFirstObjectByType<GameManager>();

            if (!_rm || !_gm)
            {
                Debug.LogError("[RunNarrator] RunManager or GameManager not found");
                Destroy(gameObject);
                return;
            }

            _rm.OnShopAction += OnShopActionFired;
            _rm.StartAutoRun();
            Time.timeScale = NarratorTimeScale;
            _gm.AiThinkSeconds = AiThinkDelay;
            _lastPhase = _rm.CurrentPhase;

            Log("run-start", "A New Run Begins",
                "The gambler steps into the smoke-filled parlor, ready to test fate...");
            Debug.Log("[RunNarrator] Started — narrating full run with screenshots");
        }

        void LateUpdate()
        {
            if (!_rm || _capturing) return;

            var phase = _rm.CurrentPhase;
            if (phase != _lastPhase)
            {
                OnPhaseChanged(_lastPhase, phase);
                _lastPhase = phase;
            }

            if (phase == RunPhase.InMatch && !_subscribed)
                TrySubscribeEngine();
        }

        void OnPhaseChanged(RunPhase from, RunPhase to)
        {
            switch (to)
            {
                case RunPhase.MapSelect:
                    StartCoroutine(CaptureAfterSettle("map", "The Map",
                        DescribeMapState()));
                    break;

                case RunPhase.InMatch:
                    _matchNumber++;
                    _boutNumber = 0;
                    _matchLog.Clear();
                    _opponentName = "opponent";
                    StartCoroutine(DelayedMatchCapture());
                    break;

                case RunPhase.PostMatch:
                    FlushMatchLog("match-end");
                    StartCoroutine(CaptureAfterSettle("post-match", "Match Result",
                        DescribePostMatch()));
                    break;

                case RunPhase.Shop:
                    StartCoroutine(CaptureAfterSettle("shop", "The Shop",
                        DescribeShop()));
                    break;

                case RunPhase.Event:
                    StartCoroutine(CaptureAfterSettle("event", "A Rumor...",
                        "An encounter on the road. What will the gambler choose?"));
                    break;

                case RunPhase.Rest:
                    StartCoroutine(CaptureAfterSettle("rest", "Rest Stop",
                        DescribeRest()));
                    break;

                case RunPhase.RunOver:
                    StartCoroutine(CaptureAndFinish());
                    break;
            }
        }

        IEnumerator DelayedMatchCapture()
        {
            for (int i = 0; i < 15; i++) yield return null;

            TrySubscribeEngine();
            if (_engine != null)
            {
                _opponentName = _gm.OpponentName ?? "opponent";
                string desc = DescribeMatchStart();
                _matchLog.AppendLine(desc);
                yield return StartCoroutine(CaptureAfterSettle("match-start",
                    $"Match {_matchNumber}: vs {_opponentName}", desc));
            }
        }

        void TrySubscribeEngine()
        {
            var eng = _gm.Engine;
            if (eng == null || eng == _engine) return;

            if (_engine != null) UnsubscribeEngine();

            _engine = eng;
            _engine.OnAttackPlayed += OnAttack;
            _engine.OnDefensePlayed += OnDefense;
            _engine.OnBoutResolved += OnBoutResolved;
            _engine.OnAbilityUsed += OnAbility;
            _engine.OnGameOver += OnGameOver;
            _engine.OnTrumpChanged += OnTrumpChanged;
            _engine.OnDesperationDiscard += OnDesperation;
            _subscribed = true;
        }

        void UnsubscribeEngine()
        {
            if (_engine == null) return;
            _engine.OnAttackPlayed -= OnAttack;
            _engine.OnDefensePlayed -= OnDefense;
            _engine.OnBoutResolved -= OnBoutResolved;
            _engine.OnAbilityUsed -= OnAbility;
            _engine.OnGameOver -= OnGameOver;
            _engine.OnTrumpChanged -= OnTrumpChanged;
            _engine.OnDesperationDiscard -= OnDesperation;
            _subscribed = false;
            _engine = null;
        }

        void OnAttack(int player, Card card)
        {
            string who = player == 0 ? "Player" : _opponentName;
            _matchLog.AppendLine($"  {who} attacks with {CardLabel(card)}");
        }

        void OnDefense(int player, int slot, Card card)
        {
            string who = player == 0 ? "Player" : _opponentName;
            var atkCard = _engine.Bout.Attacks[slot];
            _matchLog.AppendLine($"  {who} defends {CardLabel(atkCard)} with {CardLabel(card)}");
        }

        static string CardLabel(Card card)
        {
            if (!string.IsNullOrEmpty(card.DefinitionId) && CardCatalog.TryGet(card.DefinitionId, out var def))
                return $"{def.Name} ({card})";
            return card.ToString();
        }

        void OnBoutResolved(BoutOutcome outcome)
        {
            _boutNumber++;
            string result = outcome == BoutOutcome.DefenderWonAllDiscarded
                ? "Defender wins — cards discarded"
                : "Defender eats cards";
            int defIdx = _engine.DefenderIndex;
            string defender = defIdx == 0 ? "Player" : _opponentName;
            _matchLog.AppendLine($"  >> Bout {_boutNumber}: {result} ({defender} was defending)");
            _matchLog.AppendLine($"     Hands: Player={_engine.HandOf(0).Count} | {_opponentName}={_engine.HandOf(1).Count} | Deck={_engine.DeckCount}");

            if (_boutNumber == 1 || _boutNumber % 3 == 0)
            {
                string snap = _matchLog.ToString();
                StartCoroutine(CaptureAfterSettle("bout",
                    $"Match {_matchNumber} — Bout {_boutNumber}", snap));
            }
        }

        void OnAbility(int player, Card card, AbilityType ability)
        {
            string who = player == 0 ? "Player" : _opponentName;
            string effect = AbilityEffectSummary(ability);
            string costInfo = "";
            if (_engine != null)
            {
                var resType = _engine.GetResourceType(player);
                int resAmt = _engine.GetResource(player);
                if (resType.HasValue)
                    costInfo = $" [{resType.Value.DisplayName()} {resAmt}]";
            }
            _matchLog.AppendLine($"  ★ {who} triggers {ability.ShortName()} from {CardLabel(card)} — {effect}{costInfo}");
        }

        static string AbilityEffectSummary(AbilityType a) => a switch
        {
            AbilityType.ResourceGain => "gains 1 resource",
            AbilityType.ExtraDraw => "draws cards",
            AbilityType.Haymaker => "draws 2 cards",
            AbilityType.Peek => "scries deck",
            AbilityType.Conquer => "attacks with +2 rank",
            AbilityType.TrumpChanger => "changes trump suit",
            AbilityType.Blocker => "caps attacks this bout",
            AbilityType.DoubleAgent => "steals opponent's card",
            AbilityType.Riposte => "opponent discards",
            AbilityType.DoubleDefense => "covers two attacks",
            AbilityType.DoubleTrouble => "plays extra attack",
            AbilityType.Brace => "draws 2 cards",
            AbilityType.Fortify => "auto-defends one attack",
            AbilityType.Rampage => "plays 2 deck cards as attacks",
            AbilityType.IronGrip => "draws 3 cards",
            AbilityType.Intimidate => "opponent discards non-trump",
            AbilityType.SeizeInitiative => "seizes initiative",
            AbilityType.SlipAway => "discards undefended attacks",
            AbilityType.SmokeBomb => "ends bout, all cards discarded",
            AbilityType.BlindSwap => "swaps card with opponent",
            AbilityType.Masterstroke => "ULTIMATE: opponent loses 2 best cards, draws 1",
            AbilityType.Onslaught => "ULTIMATE: 3 deck cards become attacks",
            AbilityType.Masquerade => "ULTIMATE: swaps entire hand with opponent",
            AbilityType.Monopoly => "ULTIMATE: draws to 8, opponent discards to 5",
            _ => a.DisplayName(),
        };

        void OnTrumpChanged(Suit newTrump)
        {
            _matchLog.AppendLine($"  ♦ Trump changed to {newTrump}!");
        }

        void OnShopActionFired(string action)
        {
            Log("shop-action", "Shop Decision", action);
        }

        void OnDesperation(int player, int cardsDiscarded)
        {
            string who = player == 0 ? "Player" : _opponentName;
            _matchLog.AppendLine($"  ** {who} DESPERATION -- discards {cardsDiscarded} weak cards, gains resource!");
        }

        void OnGameOver(int foolIndex)
        {
            string winner = foolIndex == 1 ? "Player" : _opponentName;
            string loser = foolIndex == 0 ? "Player" : _opponentName;
            _matchLog.AppendLine($"\n  === {winner} WINS! {loser} is the Fool. ===");
            _matchLog.AppendLine($"  Final: {_boutNumber} bouts played");
            UnsubscribeEngine();
        }

        void FlushMatchLog(string category)
        {
            if (_matchLog.Length == 0) return;
            Log(category, $"Match {_matchNumber} — Battle Log", _matchLog.ToString());
        }

        // --- Descriptions ---

        string DescribeMapState()
        {
            var run = GetRunState();
            if (run == null) return "Choosing the next destination...";
            string deckInfo = run.PlayerDoctrine.HasValue
                ? $"Doctrine: {run.PlayerDoctrine.Value} | Deck: {run.PlayerDeckCardIds.Count} cards | Relics: {run.PlayerRelics.Count}"
                : $"Abilities: {run.PlayerAbilities.Count} | Trinkets: {run.PlayerTrinkets.Count}";
            return $"Act {run.CurrentAct + 1} — Prestige: {run.Prestige} | Florins: {run.Florins}\n" +
                   $"{deckInfo} | Burdens: {run.PlayerBurdens.Count}\n" +
                   "The auto-runner picks the best available node.";
        }

        string DescribeMatchStart()
        {
            if (_engine == null) return "Match begins...";
            string res = "";
            var rt = _engine.GetResourceType(0);
            if (rt.HasValue)
                res = $" | Resource: {_engine.GetResource(0)} {rt.Value}";
            return $"Trump: {_engine.Trump} | Player hand: {_engine.HandOf(0).Count} cards | " +
                   $"{_opponentName} hand: {_engine.HandOf(1).Count} cards | Deck: {_engine.DeckCount}{res}\n" +
                   $"First attacker: {(_engine.AttackerIndex == 0 ? "Player" : _opponentName)}";
        }

        string DescribePostMatch()
        {
            var run = GetRunState();
            if (run == null) return "Match complete.";
            string info = $"Record: {run.MatchesWon}W / {run.MatchesPlayed - run.MatchesWon}L | " +
                          $"Prestige: {run.Prestige} | Florins: {run.Florins}\n";
            if (run.PlayerDoctrine.HasValue)
                info += $"Doctrine: {run.PlayerDoctrine.Value} | Deck: {run.PlayerDeckCardIds.Count} cards | " +
                        $"Relics: [{string.Join(", ", run.PlayerRelics)}]\n";
            else
                info += $"Abilities: [{string.Join(", ", run.PlayerAbilities)}]\n";
            info += $"Burdens: {run.PlayerBurdens.Count}";
            return info;
        }

        string DescribeShop()
        {
            var run = GetRunState();
            if (run == null) return "Browsing wares...";
            return $"Florins: {run.Florins} | Deck: {run.PlayerDeckCardIds.Count} cards\n" +
                   $"Removals used: {run.CardRemovalsPurchased}/3\n" +
                   "The gambler surveys the wares...";
        }

        string DescribeRest()
        {
            var run = GetRunState();
            if (run == null) return "Taking a rest...";
            return $"Burdens: {run.PlayerBurdens.Count} | " +
                   (run.PlayerBurdens.Count > 0
                       ? "Mending a burden to restore fighting shape."
                       : "No burdens to mend — moving on.");
        }

        RunState GetRunState()
        {
            var field = typeof(RunManager).GetField("_run",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_rm) as RunState;
        }

        // --- Capture ---

        void Log(string category, string title, string body)
        {
            _entries.Add(new NarrativeEntry
            {
                index = _entries.Count,
                imagePath = null,
                title = title,
                body = body,
                category = category,
            });
        }

        IEnumerator CaptureAfterSettle(string category, string title, string body)
        {
            _capturing = true;
            float prev = Time.timeScale;
            Time.timeScale = 1f;

            for (int i = 0; i < SettleFrames; i++)
                yield return null;
            yield return new WaitForEndOfFrame();

            string filename = $"{_shotIndex:D3}_{category}.png";
            string path = $"{OutputDir}/{filename}";

            int w = Screen.width, h = Screen.height;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Destroy(tex);

            _entries.Add(new NarrativeEntry
            {
                index = _shotIndex,
                imagePath = filename,
                title = title,
                body = body,
                category = category,
            });
            _shotIndex++;
            Debug.Log($"[RunNarrator] #{_shotIndex} {category}: {title}");

            Time.timeScale = prev;
            _capturing = false;
        }

        IEnumerator CaptureAndFinish()
        {
            var run = GetRunState();
            string result = run != null && run.RunWon ? "VICTORY" : "DEFEAT";
            string buildInfo;
            {
                var cardNames = new System.Collections.Generic.List<string>();
                if (run != null)
                    foreach (var id in run.PlayerDeckCardIds)
                        cardNames.Add(CardCatalog.TryGet(id, out var def) ? def.Name : id);
                buildInfo = $"Doctrine: {run?.PlayerDoctrine}\n" +
                            $"Deck ({run?.PlayerDeckCardIds.Count ?? 0} cards): [{string.Join(", ", cardNames)}]\n" +
                            $"Relics: [{string.Join(", ", run?.PlayerRelics ?? new System.Collections.Generic.List<RelicType>())}]\n" +
                            $"Card removals purchased: {run?.CardRemovalsPurchased ?? 0}";
            }
            string body = run != null
                ? $"Result: {result}\n" +
                  $"Acts completed: {run.CurrentAct + (run.RunWon ? 1 : 0)}/5\n" +
                  $"Matches: {run.MatchesWon}W / {run.MatchesPlayed - run.MatchesWon}L\n" +
                  $"Florins: {run.Florins} | Prestige: {run.Prestige}\n" +
                  buildInfo
                : "Run complete.";

            yield return StartCoroutine(CaptureAfterSettle("run-over",
                $"Run Complete — {result}!", body));

            GenerateReport();
            Debug.Log($"[RunNarrator] Done! {_entries.Count} entries, {_shotIndex} screenshots. " +
                      $"Report: {Path.GetFullPath(OutputDir)}/report.html");

            Time.timeScale = 0f;
            Destroy(gameObject);
        }

        // --- HTML Report ---

        void GenerateReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
            sb.AppendLine("<title>Wits &amp; Fools — Run Narrative</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Georgia', serif; background: #1a1a2e; color: #e0d6c8; max-width: 900px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine("h1 { text-align: center; color: #d4a574; font-size: 2em; border-bottom: 2px solid #3a3a5e; padding-bottom: 10px; }");
            sb.AppendLine(".entry { margin: 30px 0; background: #16213e; border-radius: 8px; overflow: hidden; border: 1px solid #3a3a5e; }");
            sb.AppendLine(".entry img { width: 100%; display: block; }");
            sb.AppendLine(".entry-text { padding: 15px 20px; }");
            sb.AppendLine(".entry h2 { color: #d4a574; margin: 0 0 8px 0; font-size: 1.3em; }");
            sb.AppendLine(".entry .cat { display: inline-block; background: #3a3a5e; color: #a0a0c0; font-size: 0.75em; padding: 2px 8px; border-radius: 4px; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 1px; }");
            sb.AppendLine(".entry pre { background: #0f0f23; color: #c0b8a8; padding: 12px; border-radius: 4px; white-space: pre-wrap; font-size: 0.85em; line-height: 1.5; overflow-x: auto; }");
            sb.AppendLine(".entry p { margin: 0; line-height: 1.6; }");
            sb.AppendLine(".text-only { border-left: 3px solid #d4a574; }");
            sb.AppendLine("footer { text-align: center; color: #666; margin-top: 40px; font-size: 0.8em; }");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<h1>Wits &amp; Fools — Run Narrative</h1>");

            foreach (var e in _entries)
            {
                bool hasImage = !string.IsNullOrEmpty(e.imagePath);
                sb.AppendLine($"<div class='entry{(hasImage ? "" : " text-only")}'>");
                if (hasImage)
                    sb.AppendLine($"<img src='{e.imagePath}' alt='{Escape(e.title)}' loading='lazy'>");
                sb.AppendLine("<div class='entry-text'>");
                sb.AppendLine($"<span class='cat'>{Escape(e.category)}</span>");
                sb.AppendLine($"<h2>{Escape(e.title)}</h2>");

                if (e.body.Contains("\n") && (e.category.Contains("match") || e.category == "bout"))
                    sb.AppendLine($"<pre>{Escape(e.body)}</pre>");
                else
                    sb.AppendLine($"<p>{Escape(e.body).Replace("\n", "<br>")}</p>");

                sb.AppendLine("</div></div>");
            }

            sb.AppendLine("<footer>Generated by RunNarrator — Wits &amp; Fools</footer>");
            sb.AppendLine("</body></html>");

            File.WriteAllText($"{OutputDir}/report.html", sb.ToString());
        }

        static string Escape(string s) => s?
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;") ?? "";

        void OnDestroy()
        {
            UnsubscribeEngine();
            if (_rm) _rm.OnShopAction -= OnShopActionFired;
        }
    }
}
