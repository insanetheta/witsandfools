using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WitsAndFools
{
    public enum RunPhase
    {
        Title,
        ArchetypeSelect,
        MapSelect,
        PreMatch,
        InMatch,
        PostMatch,
        Shop,
        Event,
        Rest,
        RunOver
    }

    public sealed class RunManager : MonoBehaviour
    {
        [Header("Scene Refs")]
        public GameManager GameManager;
        public GameObject MatchPanel;
        public GameObject MapPanel;
        public GameObject ResultPanel;
        public GameObject RunOverPanel;
        public GameObject RunHudPanel;
        public GameObject ShopPanel;
        public GameObject EventPanel;

        [Header("Map Panel Refs")]
        public TMP_Text MapTitleLabel;
        public Transform MapNodeContainer;
        public Transform MapPathContainer;
        public GameObject MapNodeButtonPrefab;
        public Sprite[] MapNodeSprites;

        [Header("Result Panel Refs")]
        public TMP_Text ResultTitleLabel;
        public TMP_Text ResultDetailsLabel;
        public TMP_Text ResultRewardLabel;
        public Button ResultContinueButton;
        public TMP_Text AbilityPickLabel;
        public Transform AbilityPickContainer;
        public Button AbilityPickSkipButton;

        [Header("Run Over Panel Refs")]
        public TMP_Text RunOverTitleLabel;
        public TMP_Text RunOverStatsLabel;
        public Button RunOverRestartButton;
        public RectTransform RunOverJourneyContainer;
        public RectTransform RunOverAbilityChipsContainer;

        [Header("Run HUD Refs")]
        public TMP_Text PrestigeLabel;
        public TMP_Text FlorinsLabel;
        public TMP_Text ActLabel;
        public TMP_Text AbilitiesLabel;

        [Header("Shop Panel Refs")]
        public TMP_Text ShopTitleLabel;
        public TMP_Text ShopFlorinsLabel;
        public Transform ShopItemContainer;
        public Button ShopLeaveButton;

        [Header("Event Panel Refs")]
        public TMP_Text EventTitleLabel;
        public TMP_Text EventDescLabel;
        public TMP_Text EventOutcomeLabel;
        public Button EventChoice1Button;
        public TMP_Text EventChoice1Label;
        public Button EventChoice2Button;
        public TMP_Text EventChoice2Label;
        public Button EventContinueButton;

        [Header("Match Table Theme")]
        public Image TableBackgroundImage;
        public Image TableFeltImage;
        public Image TableFrameImage;
        public Image VignetteImage;
        public Sprite[] TableSurfaceSprites;
        public Sprite[] VenueBackgroundSprites;
        public Sprite VignetteSprite;
        public Image MapVenueBgImage;
        public Image ResultVenueBgImage;
        public Image RunOverVenueBgImage;

        [Header("Opponent Portraits")]
        public string[] PortraitNames;
        public Sprite[] PortraitSprites;

        [Header("Card Detail Modal")]
        public GameObject CardDetailPanel;

        [Header("Deck Browser")]
        public GameObject DeckBrowserPanel;
        public Transform DeckBrowserContainer;
        public TMP_Text DeckStatsLabel;
        public Button DeckBrowserCloseButton;
        public Button ViewDeckButton;

        [Header("Card Reward")]
        public TMP_Text CardRewardLabel;
        public Transform CardRewardContainer;
        public Button CardRewardSkipButton;

        RunState _run;
        RunPhase _phase;
        public RunPhase CurrentPhase => _phase;
        public event Action<string> OnShopAction;
        int _currentColumn;
        System.Random _rng;
        MapNode _currentNode;
        ArchetypeType? _selectedArchetype;

        bool _autoRun;
        int _pendingAscension;
        float _autoStepDelay = 0.15f;
        float _nextAutoStep;

        void Start()
        {
            Time.timeScale = 1f;
            _autoRun = false;
            if (GameManager) GameManager.AutoStartOnAwake = false;
            if (ViewDeckButton)
            {
                ViewDeckButton.onClick.RemoveAllListeners();
                ViewDeckButton.onClick.AddListener(ShowDeckBrowser);
            }
            if (!TryLoadSave())
                StartNewRun();
        }

        bool TryLoadSave()
        {
            var save = RunSaveSystem.Load();
            if (save?.Run == null) return false;
            _run = save.Run;
            _currentColumn = save.CurrentColumn;
            _selectedArchetype = save.SelectedArchetype;
            _rng = new System.Random(_run.Seed + _run.MatchesPlayed);
            EnsureCatalogsLoaded();
            if (Enum.TryParse<RunPhase>(save.RunPhase, out var phase)
                && (phase == RunPhase.MapSelect || phase == RunPhase.ArchetypeSelect))
                SetPhase(phase);
            else
                SetPhase(RunPhase.MapSelect);
            Debug.Log($"[RunManager] Loaded saved run — Act {_run.CurrentAct + 1}, {_run.MatchesPlayed} matches played");
            return true;
        }

        void AutoSave()
        {
            if (_run == null || _autoRun) return;
            RunSaveSystem.Save(_run, _currentColumn, _selectedArchetype, _phase);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && _phase != RunPhase.InMatch)
                ToggleAutoRun();
            if (!_autoRun || Time.unscaledTime < _nextAutoStep) return;
            AutoStep();
        }

        void ToggleAutoRun()
        {
            _autoRun = !_autoRun;
            if (_autoRun)
            {
                _nextAutoStep = Time.unscaledTime + _autoStepDelay;
                Debug.Log("[RunManager] Auto-run ON");
            }
            else
                Debug.Log("[RunManager] Auto-run OFF");
        }

        int _batchRemaining;
        int _batchWins;
        int _batchMatches;
        int _batchMatchWins;
        List<string> _batchResults = new();

        public void StartAutoRun()
        {
            _batchRemaining = 0;
            StartNewRun();
            _autoRun = true;
            Time.timeScale = 20f;
            _nextAutoStep = Time.unscaledTime + _autoStepDelay;
            Debug.Log("[RunManager] Auto-run started (fresh run, timeScale=20)");
        }

        public void StartBatchRun(int count)
        {
            _batchRemaining = count;
            _batchWins = 0;
            _batchMatches = 0;
            _batchMatchWins = 0;
            _batchResults = new List<string>();
            StartNewRun();
            _autoRun = true;
            Time.timeScale = 20f;
            _nextAutoStep = Time.unscaledTime + _autoStepDelay;
            Debug.Log($"[RunManager] Batch run started — {count} runs");
        }

        void AutoStep()
        {
            _nextAutoStep = Time.unscaledTime + _autoStepDelay;
            if (_run == null || _run.CurrentMap == null) return;

            switch (_phase)
            {
                case RunPhase.ArchetypeSelect:
                    var allArch = ArchetypeDefinitions.AllArchetypes;
                    OnArchetypeSelected(allArch[_rng.Next(allArch.Length)]);
                    break;
                case RunPhase.MapSelect:
                    AutoSelectNode();
                    break;
                case RunPhase.PreMatch:
                    _eventChoice1Action?.Invoke();
                    break;
                case RunPhase.PostMatch:
                    if (_cardRewardOfferings != null && _cardRewardOfferings.Count > 0)
                        OnCardRewardPicked(_cardRewardOfferings[_rng.Next(_cardRewardOfferings.Count)]);
                    else if (_abilityPickOfferings != null && _abilityPickOfferings.Count > 0)
                        OnAbilityPicked(_abilityPickOfferings[_rng.Next(_abilityPickOfferings.Count)]);
                    else if (_relicPickOfferings != null && _relicPickOfferings.Count > 0)
                        OnRelicPicked(_relicPickOfferings[_rng.Next(_relicPickOfferings.Count)]);
                    else
                        OnResultContinue();
                    break;
                case RunPhase.Shop:
                    AutoHandleShop();
                    break;
                case RunPhase.Event:
                case RunPhase.Rest:
                    AutoHandleEvent();
                    break;
                case RunPhase.RunOver:
                    string archTag = _selectedArchetype.HasValue ? _selectedArchetype.Value.DisplayName() : "?";
                    string runResult = $"{(_run.RunWon ? "WON" : "LOST")} | {archTag} | Acts: {_run.CurrentAct}/5 | W/L: {_run.MatchesWon}/{_run.MatchesPlayed} | Florins: {_run.Florins}";
                    if (_batchRemaining > 0)
                    {
                        _batchResults.Add(runResult);
                        if (_run.RunWon) _batchWins++;
                        _batchMatches += _run.MatchesPlayed;
                        _batchMatchWins += _run.MatchesWon;
                        _batchRemaining--;
                        if (_batchRemaining > 0)
                        {
                            StartNewRun();
                            _autoRun = true;
                            Time.timeScale = 20f;
                            break;
                        }
                        _autoRun = false;
                        Time.timeScale = 1f;
                        Debug.Log($"[RunManager] Batch complete — {_batchWins}/{_batchResults.Count} runs won, {_batchMatchWins}/{_batchMatches} matches won");
                        foreach (var r in _batchResults) Debug.Log($"  {r}");
                    }
                    else
                    {
                        _autoRun = false;
                        Time.timeScale = 1f;
                        Debug.Log($"[RunManager] Auto-run complete — {runResult}");
                        if (UIScreenCapture.Instance != null && UIScreenCapture.Instance.IsCapturing)
                            UIScreenCapture.Instance.EndCapture();
                    }
                    break;
            }
        }

        void AutoSelectNode()
        {
            if (_currentColumn >= _run.CurrentMap.Count) return;
            var column = _run.CurrentMap[_currentColumn];
            int bestRow = 0;
            int bestPriority = NodePriority(column[0].Type);
            for (int i = 1; i < column.Count; i++)
            {
                int p = NodePriority(column[i].Type);
                if (p > bestPriority) { bestRow = i; bestPriority = p; }
            }
            OnNodeSelected(column[bestRow], bestRow);
        }

        int NodePriority(MapNodeType t)
        {
            switch (t)
            {
                case MapNodeType.BossMatch: return 100;
                case MapNodeType.EliteMatch: return 90;
                case MapNodeType.RivalMatch: return 70;
                case MapNodeType.Rest:
                    int restBase = 25;
                    if (_run.PlayerBurdens.Count >= 3) restBase = 95;
                    else if (_run.PlayerBurdens.Count >= 2) restBase = 80;
                    else if (_run.PlayerBurdens.Count >= 1) restBase = 60;
                    return restBase;
                case MapNodeType.Shop:
                    int shopBase = 55;
                    if (_run.Florins >= 8) shopBase = 80;
                    return shopBase;
                case MapNodeType.Rumor:
                    int rumorBase = 50;
                    if (_run.NodesSinceLastRumor >= 3) rumorBase = 85;
                    return rumorBase;
                default: return 0;
            }
        }

        void AutoHandleShop()
        {
            int removalPrice = GetCardRemovalPrice();
            if (_run.Florins >= removalPrice && _run.PlayerDeckCardIds.Count > 8 && _run.CardRemovalsPurchased < 3)
            {
                string weakest = FindWeakestDeckCard();
                if (weakest != null)
                {
                    string removedName = CardCatalog.TryGet(weakest, out var wDef) ? wDef.Name : weakest;
                    _run.PlayerDeckCardIds.Remove(weakest);
                    _run.Florins -= removalPrice;
                    _run.CardRemovalsPurchased++;
                    OnShopAction?.Invoke($"Removed '{removedName}' from deck (-{removalPrice} florins, {_run.Florins} remaining)");
                    OnResultContinue();
                    return;
                }
            }
            if (_run.Florins >= 6)
            {
                var upgradeCandidates = GetUpgradeableCards();
                if (upgradeCandidates.Count > 0)
                {
                    var pick = upgradeCandidates[_rng.Next(upgradeCandidates.Count)];
                    Rank newRank = pick.rank + 1;
                    int price = (int)pick.rank >= 10 ? 10 : 6;
                    if (_run.Florins >= price)
                    {
                        _run.CardRankOverrides[pick.id] = newRank;
                        _run.Florins -= price;
                        OnShopAction?.Invoke($"Honed '{pick.name}' {pick.rank.Label()}→{newRank.Label()} (-{price} florins, {_run.Florins} remaining)");
                        OnResultContinue();
                        return;
                    }
                }
            }
            if (_run.Florins >= 6 && _run.PlayerDeckCardIds.Count < 16)
            {
                var card = PickDraftableCard();
                if (card != null)
                {
                    _run.PlayerDeckCardIds.Add(card.Id);
                    _run.Florins -= 6;
                    OnShopAction?.Invoke($"Added '{card.Name}' to deck (-6 florins, {_run.Florins} remaining)");
                    OnResultContinue();
                    return;
                }
            }
            OnShopAction?.Invoke("Browsed but bought nothing.");
            OnResultContinue();
        }

        string FindWeakestDeckCard()
        {
            string weakest = null;
            int weakestScore = int.MaxValue;
            foreach (var id in _run.PlayerDeckCardIds)
            {
                if (!CardCatalog.TryGet(id, out var def)) continue;
                int score = (int)def.Rank;
                if (def.HasAbility) score += 10;
                if (def.Doctrine != DoctrineType.Neutral) score += 5;
                if (score < weakestScore) { weakestScore = score; weakest = id; }
            }
            return weakest;
        }

        CardDefinition PickDraftableCard()
        {
            if (!_run.PlayerDoctrine.HasValue) return null;
            var pool = CardCatalog.Draftable(_run.PlayerDoctrine.Value);
            var candidates = new List<CardDefinition>();
            foreach (var card in pool)
            {
                if (_run.PlayerDeckCardIds.Contains(card.Id)) continue;
                if (card.InStartingDeck) continue;
                candidates.Add(card);
            }
            if (candidates.Count == 0) return null;
            return candidates[_rng.Next(candidates.Count)];
        }

        void AutoHandleEvent()
        {
            if (EventContinueButton && EventContinueButton.gameObject.activeSelf)
            {
                OnResultContinue();
                return;
            }
            if (_phase == RunPhase.Rest && _run.PlayerBurdens.Count > 0)
            {
                if (_eventChoice1Action == DoRestMend)
                    _eventChoice1Action.Invoke();
                else if (_eventChoice2Action == DoRestMend)
                    _eventChoice2Action.Invoke();
                else
                {
                    DoRestMend();
                    ShowEventOutcome($"Mended a burden. {_run.PlayerBurdens.Count} remaining.");
                }
                return;
            }
            if (EventChoice1Button && EventChoice1Button.gameObject.activeSelf)
            {
                bool choice1Disabled = EventChoice1Label &&
                    (EventChoice1Label.text.Contains("Can't afford") ||
                     EventChoice1Label.text.Contains("Not enough") ||
                     EventChoice1Label.text.Contains("No suitable"));
                if (choice1Disabled && _eventChoice2Action != null
                    && EventChoice2Button && EventChoice2Button.gameObject.activeSelf)
                    _eventChoice2Action.Invoke();
                else
                    _eventChoice1Action?.Invoke();
            }
        }

        public void SetAscensionLevel(int level) => _pendingAscension = Math.Clamp(level, 0, Ascension.MaxLevel);

        public void StartNewRun()
        {
            RunSaveSystem.Delete();
            _autoRun = false;
            Time.timeScale = 1f;
            int seed = Environment.TickCount;
            _rng = new System.Random(seed);
            _run = new RunState { Seed = seed };
            _selectedArchetype = null;

            EnsureCatalogsLoaded();

            _run.AscensionLevel = _pendingAscension;
            _run.CurrentAct = 0;
            _run.CurrentMap = MapGenerator.Generate(0, _rng, Ascension.ShorterRoads(_run.AscensionLevel) ? 1 : 0);
            _currentColumn = 0;
            _visitedNodeRows.Clear();
            _selectedNodeRow = -1;
            ApplyActTheme(0);

            if (Ascension.RattledStart(_run.AscensionLevel))
                AddBurden(BurdenType.RattledNerves);
            if (Ascension.NarrowMind(_run.AscensionLevel))
                _run.MaxAbilitySlots = 3;

            SetPhase(RunPhase.ArchetypeSelect);
        }

        void EnsureCatalogsLoaded()
        {
            if (!CardCatalog.IsInitialized)
            {
                string path = Path.Combine(Application.dataPath, "Data", "card_catalog.json");
                if (File.Exists(path))
                {
                    CardCatalogLoader.LoadFromJson(File.ReadAllText(path));
                    Debug.Log($"[RunManager] Loaded {CardCatalog.Count} cards from catalog.");
                }
            }
            if (!DoctrineRoster.IsInitialized)
            {
                string path = Path.Combine(Application.dataPath, "Data", "enemy_roster.json");
                if (File.Exists(path))
                {
                    var enemies = DoctrineRoster.ParseJson(File.ReadAllText(path));
                    DoctrineRoster.RegisterAll(enemies);
                    Debug.Log($"[RunManager] Loaded {DoctrineRoster.Count} enemies from roster.");
                }
            }
            if (!RelicPool.IsInitialized)
            {
                RelicPool.RegisterAll(RelicDefinitions.All());
                Debug.Log($"[RunManager] Registered {RelicPool.Count} relic definitions.");
            }
        }

        void OnArchetypeSelected(ArchetypeType archetype)
        {
            _selectedArchetype = archetype;
            _run.PlayerArchetype = archetype;
            _run.PlayerAbilities.AddRange(archetype.StartingAbilities());
            var trinket = archetype.StartingTrinket();
            if (trinket.HasValue)
                _run.PlayerTrinkets.Add(trinket.Value);

            var doctrine = DoctrineExtensions.FromArchetype(archetype);
            if (doctrine.HasValue && CardCatalog.IsInitialized)
            {
                _run.InitDoctrineDeck(doctrine.Value);
                Debug.Log($"[RunManager] Initialized {doctrine.Value} deck with {_run.PlayerDeckCardIds.Count} cards.");
            }

            SetPhase(RunPhase.MapSelect);
        }

        void SetPhase(RunPhase phase)
        {
            _phase = phase;
            if (MatchPanel) MatchPanel.SetActive(phase == RunPhase.InMatch);
            if (MapPanel) MapPanel.SetActive(phase == RunPhase.MapSelect || phase == RunPhase.ArchetypeSelect);
            if (ResultPanel) ResultPanel.SetActive(phase == RunPhase.PostMatch);
            if (RunOverPanel) RunOverPanel.SetActive(phase == RunPhase.RunOver);
            if (ShopPanel) ShopPanel.SetActive(phase == RunPhase.Shop);
            if (EventPanel) EventPanel.SetActive(phase == RunPhase.Event || phase == RunPhase.Rest || phase == RunPhase.PreMatch);
            if (RunHudPanel) RunHudPanel.SetActive(phase != RunPhase.Title && phase != RunPhase.RunOver && phase != RunPhase.ArchetypeSelect);

            UpdateRunHud();

            if (UIScreenCapture.Instance != null)
            {
                UIScreenCapture.Instance.NotifyPhaseChange(phase, _run?.RunWon ?? false);
                if (UIScreenCapture.Instance.IsCapturing && _autoRun)
                    _nextAutoStep = Time.unscaledTime + 0.2f;
            }

            switch (phase)
            {
                case RunPhase.ArchetypeSelect:
                    ShowArchetypeSelect();
                    break;
                case RunPhase.MapSelect:
                    ShowMap();
                    break;
                case RunPhase.RunOver:
                    ShowRunOver();
                    RunSaveSystem.Delete();
                    return;
            }

            if (phase == RunPhase.MapSelect || phase == RunPhase.Shop
                || phase == RunPhase.Event || phase == RunPhase.Rest)
                AutoSave();
        }

        void UpdateRunHud()
        {
            if (_run == null) return;
            if (PrestigeLabel) PrestigeLabel.text = $"Prestige: {new string('♥', _run.Prestige)}";
            if (FlorinsLabel) FlorinsLabel.text = $"Florins: {_run.Florins}";
            if (ActLabel) ActLabel.text = $"Act {_run.CurrentAct + 1} of 5";
            if (AbilitiesLabel && _run.PlayerDoctrine.HasValue)
            {
                var doc = _run.PlayerDoctrine.Value;
                AbilitiesLabel.text = $"{doc.DisplayName()} | Deck: {_run.PlayerDeckCardIds.Count}";
            }
            if (GameManager.Hud && GameManager.Hud.PhaseLabel && _phase != RunPhase.InMatch)
            {
                string phaseText = _phase switch
                {
                    RunPhase.MapSelect => $"Act {_run.CurrentAct + 1}",
                    RunPhase.Shop => "Shop",
                    RunPhase.Event => "Event",
                    RunPhase.Rest => "Rest",
                    RunPhase.PreMatch => "Preparing...",
                    RunPhase.PostMatch => "Victory",
                    _ => ""
                };
                GameManager.Hud.PhaseLabel.text = phaseText;
            }
        }

        // ---------- Archetype Select ----------

        void ShowArchetypeSelect()
        {
            var repData = ReputationSystem.Load();
            if (MapTitleLabel)
                MapTitleLabel.text = $"Choose Your Archetype  (Rep: {repData.TotalReputation})";
            ClearMapNodes();
            if (!MapNodeContainer) return;

            var allArch = ArchetypeDefinitions.AllArchetypes;
            var containerRT = (RectTransform)MapNodeContainer;
            Canvas.ForceUpdateCanvases();
            float containerW = containerRT.rect.width;
            float containerH = containerRT.rect.height;
            if (containerW <= 0) containerW = 1612f;
            if (containerH <= 0) containerH = 810f;

            float btnW = 550f;
            float btnH = 100f;
            float gap = 16f;
            float totalH = allArch.Length * btnH + (allArch.Length - 1) * gap;
            float startY = containerH / 2f + totalH / 2f;

            for (int i = 0; i < allArch.Length; i++)
            {
                float y = startY - i * (btnH + gap) - btnH / 2f;
                bool unlocked = repData.UnlockedArchetypes.Contains(allArch[i]);
                CreateArchetypeButton(allArch[i], unlocked, new Vector2(containerW / 2f, y), btnW, btnH);
            }
        }

        void CreateArchetypeButton(ArchetypeType archetype, bool unlocked, Vector2 pos, float w, float h)
        {
            var abilities = archetype.StartingAbilities();
            string abilityList = string.Join(", ", abilities.ConvertAll(a => a.DisplayName()));
            var trinket = archetype.StartingTrinket();
            string trinketStr = trinket.HasValue ? $"  |  Trinket: {trinket.Value.DisplayName()}" : "";

            int repNeeded = archetype switch
            {
                ArchetypeType.Brute => 25,
                ArchetypeType.Diplomat => 100,
                ArchetypeType.Gambler => 300,
                _ => 0
            };

            var btnGO = new GameObject($"Archetype_{archetype}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(MapNodeContainer, false);

            var btnRT = (RectTransform)btnGO.transform;
            btnRT.anchorMin = btnRT.anchorMax = Vector2.zero;
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.anchoredPosition = pos;
            btnRT.sizeDelta = new Vector2(w, h);

            var img = btnGO.GetComponent<Image>();
            img.color = unlocked ? archetype switch
            {
                ArchetypeType.Rogue => ThemePalette.ArchRogue,
                ArchetypeType.Brute => ThemePalette.ArchBrute,
                ArchetypeType.Diplomat => ThemePalette.ArchDiplomat,
                ArchetypeType.Gambler => ThemePalette.ArchGambler,
                _ => Color.gray
            } : ThemePalette.LockedGray;

            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(btnGO.transform, false);
            var nameRT = (RectTransform)nameGO.transform;
            nameRT.anchorMin = new Vector2(0, 0.55f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(16, 0);
            nameRT.offsetMax = new Vector2(-16, -4);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text = unlocked
                ? $"{archetype.DisplayName()} — {archetype.Description()}"
                : $"{archetype.DisplayName()} — Locked (requires {repNeeded} Rep)";
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.fontSize = 20;
            nameTMP.color = unlocked ? Color.white : ThemePalette.DisabledText;
            nameTMP.raycastTarget = false;

            var descGO = new GameObject("Abilities", typeof(RectTransform));
            descGO.transform.SetParent(btnGO.transform, false);
            var descRT = (RectTransform)descGO.transform;
            descRT.anchorMin = Vector2.zero;
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.offsetMin = new Vector2(16, 4);
            descRT.offsetMax = new Vector2(-16, 0);
            var descTMP = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text = unlocked ? $"Starts with: {abilityList}{trinketStr}" : "???";
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.fontSize = 16;
            descTMP.color = ThemePalette.DescGray;
            descTMP.raycastTarget = false;
            descTMP.enableWordWrapping = true;

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = unlocked;
            var captured = archetype;
            btn.onClick.AddListener(() => OnArchetypeSelected(captured));
        }

        // ---------- Map (branching path layout) ----------

        int _selectedNodeRow = -1; // which row was picked in the current column (for path lines)
        readonly List<int> _visitedNodeRows = new(); // row picked per visited column

        void ShowMap()
        {
            if (MapTitleLabel)
            {
                string[] venueNames = { "The Bilge Rat Tavern", "The Merchant's Rest", "The Guildmaster's Hall", "The Cardinal's Library", "The Duke's Salon" };
                string venue = _run.CurrentAct < venueNames.Length ? venueNames[_run.CurrentAct] : "???";
                MapTitleLabel.text = $"Act {_run.CurrentAct + 1} — {venue}";
            }

            ClearMapNodes();

            if (_currentColumn >= _run.CurrentMap.Count)
            {
                AdvanceAct();
                return;
            }

            RenderFullMap();
        }

        void ClearMapNodes()
        {
            if (!MapNodeContainer) return;
            for (int i = MapNodeContainer.childCount - 1; i >= 0; i--)
            {
                var child = MapNodeContainer.GetChild(i);
                if (child.name != "MapPathContainer")
                    Destroy(child.gameObject);
            }
            if (MapPathContainer)
                for (int i = MapPathContainer.childCount - 1; i >= 0; i--)
                    Destroy(MapPathContainer.GetChild(i).gameObject);
        }

        void RenderFullMap()
        {
            if (!MapNodeContainer) return;
            var containerRT = (RectTransform)MapNodeContainer;
            var map = _run.CurrentMap;
            int totalCols = map.Count;
            float containerW = containerRT.rect.width;
            float containerH = containerRT.rect.height;
            if (containerW <= 0) containerW = 1200f;
            if (containerH <= 0) containerH = 600f;

            float colSpacing = containerW / (totalCols + 1);
            float nodeW = 200f;
            float nodeH = 140f;
            float nodeGap = 14f;
            var nodePositions = new List<List<Vector2>>();

            for (int col = 0; col < totalCols; col++)
            {
                var column = map[col];
                float x = colSpacing * (col + 1);
                float totalH = column.Count * nodeH + (column.Count - 1) * nodeGap;
                float startY = containerH / 2f + totalH / 2f;

                var colPositions = new List<Vector2>();
                for (int row = 0; row < column.Count; row++)
                {
                    float y = startY - row * (nodeH + nodeGap) - nodeH / 2f;
                    colPositions.Add(new Vector2(x, y));
                }
                nodePositions.Add(colPositions);
            }

            // Draw path lines between columns
            DrawPathLines(nodePositions, containerRT);

            // Create node cards
            for (int col = 0; col < totalCols; col++)
            {
                var column = map[col];
                bool isVisited = col < _currentColumn;
                bool isCurrent = col == _currentColumn;
                bool isFuture = col > _currentColumn;

                for (int row = 0; row < column.Count; row++)
                {
                    var node = column[row];
                    var pos = nodePositions[col][row];
                    CreateMapNode(node, pos, nodeW, nodeH, isVisited, isCurrent, isFuture, col, row);
                }

                // Column numeral header
                string numeral = (col + 1) switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", _ => (col+1).ToString() };
                var headerGO = new GameObject($"ColHeader_{col}", typeof(RectTransform));
                headerGO.transform.SetParent(MapNodeContainer, false);
                var headerRT = (RectTransform)headerGO.transform;
                headerRT.anchorMin = headerRT.anchorMax = new Vector2(0, 0);
                headerRT.pivot = new Vector2(0.5f, 0);
                float headerX = nodePositions[col][0].x;
                float topNodeY = nodePositions[col][0].y + nodeH / 2f;
                headerRT.anchoredPosition = new Vector2(headerX, topNodeY + 12f);
                headerRT.sizeDelta = new Vector2(80, 36);
                var headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
                var hFont = FontAssets.Heading;
                if (hFont) headerTMP.font = hFont;
                headerTMP.text = numeral;
                headerTMP.alignment = TextAlignmentOptions.Center;
                headerTMP.fontSize = 22;
                headerTMP.color = new Color(ThemePalette.Gold.r, ThemePalette.Gold.g, ThemePalette.Gold.b, 0.6f);
                headerTMP.raycastTarget = false;
            }
        }

        void CreateMapNode(MapNode node, Vector2 pos, float w, float h, bool visited, bool current, bool future, int col, int row)
        {
            var go = new GameObject($"Node_{col}_{row}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(MapNodeContainer, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;

            var bgImg = go.GetComponent<Image>();
            var typeColor = NodeColor(node.Type);
            bool wasChosen = visited && col < _visitedNodeRows.Count && _visitedNodeRows[col] == row;

            if (visited)
                bgImg.color = wasChosen
                    ? new Color(typeColor.r * 0.35f, typeColor.g * 0.35f, typeColor.b * 0.35f, 0.8f)
                    : new Color(0.12f, 0.12f, 0.12f, 0.45f);
            else if (current)
                bgImg.color = new Color(typeColor.r * 0.25f, typeColor.g * 0.25f, typeColor.b * 0.25f, 0.92f);
            else
                bgImg.color = new Color(0.1f, 0.08f, 0.06f, 0.65f);

            // Colored top stripe (type indicator)
            var stripeGO = new GameObject("Stripe", typeof(RectTransform), typeof(Image));
            stripeGO.transform.SetParent(go.transform, false);
            var stripeRT = (RectTransform)stripeGO.transform;
            stripeRT.anchorMin = new Vector2(0, 1);
            stripeRT.anchorMax = Vector2.one;
            stripeRT.pivot = new Vector2(0.5f, 1);
            stripeRT.sizeDelta = new Vector2(0, 6);
            stripeRT.anchoredPosition = Vector2.zero;
            var brightType = new Color(
                Mathf.Min(typeColor.r * 1.6f, 1f),
                Mathf.Min(typeColor.g * 1.6f, 1f),
                Mathf.Min(typeColor.b * 1.6f, 1f));
            var stripeColor = current ? brightType : (visited && wasChosen ? typeColor * 0.6f : typeColor * 0.4f);
            stripeColor.a = visited && !wasChosen ? 0.3f : 1f;
            stripeGO.GetComponent<Image>().color = stripeColor;
            stripeGO.GetComponent<Image>().raycastTarget = false;

            // Type label (e.g. "MATCH", "SHOP", "REST")
            string typeName = NodeTypeName(node.Type);
            var typeGO = new GameObject("TypeLabel", typeof(RectTransform));
            typeGO.transform.SetParent(go.transform, false);
            var typeRT = (RectTransform)typeGO.transform;
            typeRT.anchorMin = new Vector2(0, 0.68f);
            typeRT.anchorMax = new Vector2(1, 0.95f);
            typeRT.offsetMin = new Vector2(4, 0);
            typeRT.offsetMax = new Vector2(-4, -6);
            var typeTMP = typeGO.AddComponent<TextMeshProUGUI>();
            var headFont = FontAssets.Heading;
            if (headFont) typeTMP.font = headFont;
            typeTMP.text = typeName;
            typeTMP.alignment = TextAlignmentOptions.Center;
            typeTMP.fontSize = 20;
            typeTMP.characterSpacing = 4;
            typeTMP.raycastTarget = false;
            if (future) typeTMP.color = new Color(
                Mathf.Min(typeColor.r * 2.2f, 1f),
                Mathf.Min(typeColor.g * 2.2f, 1f),
                Mathf.Min(typeColor.b * 2.2f, 1f), 0.5f);
            else if (visited && !wasChosen) typeTMP.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            else typeTMP.color = new Color(
                Mathf.Min(typeColor.r * 2.5f, 1f),
                Mathf.Min(typeColor.g * 2.5f, 1f),
                Mathf.Min(typeColor.b * 2.5f, 1f), 1f);

            // Icon sprite (smaller, between type and name)
            int spriteIdx = (int)node.Type;
            if (MapNodeSprites != null && spriteIdx < MapNodeSprites.Length && MapNodeSprites[spriteIdx])
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(go.transform, false);
                var iconRT = (RectTransform)iconGO.transform;
                iconRT.anchorMin = new Vector2(0.5f, 0.3f);
                iconRT.anchorMax = new Vector2(0.5f, 0.7f);
                iconRT.pivot = new Vector2(0.5f, 0.5f);
                iconRT.sizeDelta = new Vector2(44, 0);
                iconRT.anchoredPosition = new Vector2(0, -2);
                var iconImg = iconGO.GetComponent<Image>();
                iconImg.sprite = MapNodeSprites[spriteIdx];
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                if (future) iconImg.color = new Color(0.7f, 0.65f, 0.6f, 0.4f);
                else if (visited && !wasChosen) iconImg.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
                else if (visited) iconImg.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
                else iconImg.color = new Color(1, 1, 1, current ? 0.9f : 0.5f);
            }

            // Name label at bottom
            string title = NodeTitle(node);
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = new Vector2(1, 0.3f);
            labelRT.offsetMin = new Vector2(4, 2);
            labelRT.offsetMax = new Vector2(-4, 0);
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = title;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.fontSize = 18;
            labelTMP.enableWordWrapping = false;
            labelTMP.overflowMode = TextOverflowModes.Ellipsis;
            labelTMP.raycastTarget = false;
            if (future) labelTMP.color = new Color(ThemePalette.Parchment.r, ThemePalette.Parchment.g, ThemePalette.Parchment.b, 0.35f);
            else if (visited && !wasChosen) labelTMP.color = new Color(0.5f, 0.5f, 0.5f, 0.45f);
            else if (visited) labelTMP.color = new Color(ThemePalette.Parchment.r, ThemePalette.Parchment.g, ThemePalette.Parchment.b, 0.6f);
            else labelTMP.color = current ? ThemePalette.Parchment : new Color(ThemePalette.Parchment.r, ThemePalette.Parchment.g, ThemePalette.Parchment.b, 0.55f);

            // Gold border glow for current column
            if (current)
            {
                var glowGO = new GameObject("Glow", typeof(RectTransform), typeof(Image));
                glowGO.transform.SetParent(go.transform, false);
                var glowRT = (RectTransform)glowGO.transform;
                glowRT.anchorMin = Vector2.zero;
                glowRT.anchorMax = Vector2.one;
                glowRT.offsetMin = new Vector2(-3, -3);
                glowRT.offsetMax = new Vector2(3, 3);
                glowGO.GetComponent<Image>().color = new Color(ThemePalette.Gold.r, ThemePalette.Gold.g, ThemePalette.Gold.b, 0.6f);
                glowGO.GetComponent<Image>().raycastTarget = false;
                glowGO.transform.SetAsFirstSibling();

                var btn = go.AddComponent<Button>();
                var btnColors = btn.colors;
                btnColors.highlightedColor = new Color(typeColor.r * 0.45f, typeColor.g * 0.45f, typeColor.b * 0.45f, 0.95f);
                btn.colors = btnColors;
                var capturedNode = node;
                var capturedRow = row;
                btn.onClick.AddListener(() => OnNodeSelected(capturedNode, capturedRow));
            }

            // Visited marker: a gold disc (Image, not a text glyph — the default font
            // lacks U+2713 and rendered as a missing-glyph box).
            if (wasChosen)
            {
                var checkGO = new GameObject("VisitedDot", typeof(RectTransform), typeof(Image));
                checkGO.transform.SetParent(go.transform, false);
                var checkRT = (RectTransform)checkGO.transform;
                checkRT.anchorMin = new Vector2(1, 1);
                checkRT.anchorMax = new Vector2(1, 1);
                checkRT.pivot = new Vector2(1, 1);
                checkRT.sizeDelta = new Vector2(16, 16);
                checkRT.anchoredPosition = new Vector2(-4, -4);
                checkRT.localRotation = Quaternion.Euler(0, 0, 45f); // diamond
                var checkImg = checkGO.GetComponent<Image>();
                checkImg.color = ThemePalette.Gold;
                checkImg.raycastTarget = false;
            }
        }

        void DrawPathLines(List<List<Vector2>> nodePositions, RectTransform container)
        {
            if (!MapPathContainer) return;

            for (int col = 0; col < nodePositions.Count - 1; col++)
            {
                bool isVisitedCol = col < _currentColumn;
                bool isCurrentCol = col == _currentColumn - 1;

                for (int srcRow = 0; srcRow < nodePositions[col].Count; srcRow++)
                {
                    bool srcChosen = col < _visitedNodeRows.Count && _visitedNodeRows[col] == srcRow;
                    if (isVisitedCol && !srcChosen) continue;

                    var srcPos = nodePositions[col][srcRow];

                    for (int dstRow = 0; dstRow < nodePositions[col + 1].Count; dstRow++)
                    {
                        bool dstChosen = (col + 1) < _visitedNodeRows.Count && _visitedNodeRows[col + 1] == dstRow;
                        var dstPos = nodePositions[col + 1][dstRow];

                        float alpha;
                        Color lineColor;
                        if (isVisitedCol && srcChosen && dstChosen)
                        {
                            lineColor = ThemePalette.Gold;
                            alpha = 0.8f;
                        }
                        else if (isVisitedCol && srcChosen && (col + 1) == _currentColumn)
                        {
                            lineColor = ThemePalette.Gold;
                            alpha = 0.5f;
                        }
                        else if (!isVisitedCol && col >= _currentColumn)
                        {
                            lineColor = ThemePalette.DustyTan;
                            alpha = 0.25f;
                        }
                        else continue;

                        DrawLine(srcPos, dstPos, new Color(lineColor.r, lineColor.g, lineColor.b, alpha), 5f);
                    }
                }
            }
        }

        void DrawLine(Vector2 from, Vector2 to, Color color, float thickness)
        {
            var dir = to - from;
            float dist = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            var strokeGO = new GameObject("PathStroke", typeof(RectTransform), typeof(Image));
            strokeGO.transform.SetParent(MapPathContainer, false);
            var strokeRT = (RectTransform)strokeGO.transform;
            strokeRT.anchorMin = strokeRT.anchorMax = new Vector2(0, 0);
            strokeRT.pivot = new Vector2(0, 0.5f);
            strokeRT.sizeDelta = new Vector2(dist, thickness + 4f);
            strokeRT.anchoredPosition = from;
            strokeRT.localRotation = Quaternion.Euler(0, 0, angle);
            var strokeImg = strokeGO.GetComponent<Image>();
            strokeImg.color = new Color(0, 0, 0, color.a * 0.8f);
            strokeImg.raycastTarget = false;

            var lineGO = new GameObject("PathLine", typeof(RectTransform), typeof(Image));
            lineGO.transform.SetParent(MapPathContainer, false);
            var rt = (RectTransform)lineGO.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(dist, thickness);
            rt.anchoredPosition = from;
            rt.localRotation = Quaternion.Euler(0, 0, angle);
            var img = lineGO.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }

        string NodeTypeName(MapNodeType type) => type switch
        {
            MapNodeType.RivalMatch => "MATCH",
            MapNodeType.EliteMatch => "ELITE",
            MapNodeType.BossMatch => "BOSS",
            MapNodeType.Shop => "SHOP",
            MapNodeType.Rumor => "RUMOR",
            MapNodeType.Rest => "REST",
            _ => "???"
        };

        string NodeTitle(MapNode node) => node.Type switch
        {
            MapNodeType.RivalMatch => node.Opponent?.Name ?? "Rival",
            MapNodeType.EliteMatch => node.Opponent?.Name ?? "Elite",
            MapNodeType.BossMatch => node.Opponent?.Name ?? "Boss",
            MapNodeType.Shop => "The Fence",
            MapNodeType.Rumor => "Whispered Lead",
            MapNodeType.Rest => "The Hearth",
            _ => "???"
        };

        Color NodeColor(MapNodeType type) => type switch
        {
            MapNodeType.RivalMatch => ThemePalette.NodeMatch,
            MapNodeType.EliteMatch => ThemePalette.NodeElite,
            MapNodeType.BossMatch => ThemePalette.NodeBoss,
            MapNodeType.Shop => ThemePalette.NodeShop,
            MapNodeType.Rumor => ThemePalette.NodeRumor,
            MapNodeType.Rest => ThemePalette.NodeRest,
            _ => Color.gray
        };

        void OnNodeSelected(MapNode node, int row = -1)
        {
            if (row >= 0)
            {
                _selectedNodeRow = row;
                _visitedNodeRows.Add(row);
            }
            _currentNode = node;
            _currentColumn++;
            _run.NodesSinceLastRumor++;

            switch (node.Type)
            {
                case MapNodeType.RivalMatch:
                case MapNodeType.EliteMatch:
                case MapNodeType.BossMatch:
                    StartMatch(node);
                    break;
                case MapNodeType.Shop:
                    HandleShop();
                    break;
                case MapNodeType.Rumor:
                    _run.NodesSinceLastRumor = 0;
                    HandleRumor();
                    break;
                case MapNodeType.Rest:
                    HandleRest();
                    break;
            }
        }

        // ---------- Match ----------

        int _boutsDefended;

        MatchConfig _pendingConfig;
        PlayerDeck _pendingPlayerDeck;
        PlayerDeck _pendingEnemyDeck;
        MapNode _pendingMatchNode;

        void StartMatch(MapNode node)
        {
            _boutsDefended = 0;

            GameManager.OnMatchComplete -= OnMatchComplete;
            GameManager.OnMatchComplete += OnMatchComplete;

            if (Ascension.ArmedElites(_run.AscensionLevel)
                && (node.Type == MapNodeType.EliteMatch || node.Type == MapNodeType.BossMatch))
            {
                var available = new List<RelicType>();
                foreach (var r in new[] { RelicType.CandleStub, RelicType.GamblersDie, RelicType.PilgrimsCompass })
                    if (!node.Opponent.Relics.Contains(r)) available.Add(r);
                if (available.Count > 0)
                    node.Opponent.Relics.Add(available[_rng.Next(available.Count)]);
            }

            var (config, pDeck, eDeck) = MatchSetup.Build(_run, node.Opponent, _rng);
            config.MaxBouts = 12;

            if (_run.PlayerTrinkets.Contains(TrinketType.DevilsBargain))
            {
                _pendingConfig = config;
                _pendingPlayerDeck = pDeck;
                _pendingEnemyDeck = eDeck;
                _pendingMatchNode = node;
                SetPhase(RunPhase.PreMatch);
                ShowEventChoices("The Devil's Bargain",
                    $"Before facing <b>{node.Opponent.Name}</b>, the devil offers a deal.\nChoose your terms:",
                    "Take +3 Florins", "Draw 1 fewer card",
                    ChoiceCost, ChoiceRisky);
                _eventChoice1Action = () =>
                {
                    _run.Florins += 3;
                    UpdateRunHud();
                    LaunchPendingMatch();
                };
                _eventChoice2Action = () =>
                {
                    _pendingConfig.HandSize = Math.Max(1, _pendingConfig.HandSize - 1);
                    LaunchPendingMatch();
                };
                return;
            }

            LaunchMatch(config, pDeck, eDeck, node);
        }

        void LaunchPendingMatch()
        {
            LaunchMatch(_pendingConfig, _pendingPlayerDeck, _pendingEnemyDeck, _pendingMatchNode);
            _pendingConfig = null;
            _pendingPlayerDeck = null;
            _pendingEnemyDeck = null;
            _pendingMatchNode = null;
        }

        void LaunchMatch(MatchConfig config, PlayerDeck pDeck, PlayerDeck eDeck, MapNode node)
        {
            SetPhase(RunPhase.InMatch);
            GameManager.BeginGame(config, pDeck, eDeck, node.Opponent, _rng.Next());
            Debug.Log($"[RunManager] Match vs {node.Opponent.Name} (bout cap: 12)");

            if (_autoRun)
            {
                GameManager.SetAutoPlay(true);
                GameManager.AiThinkSeconds = 0.02f;
            }

            if (GameManager.Hud && node.Opponent != null)
            {
                var portrait = FindPortrait(node.Opponent.Name);
                var archName = AIArchetypes.DisplayName(node.Opponent.Archetype);
                var archColor = ArchetypeColor(node.Opponent.Archetype);
                GameManager.Hud.SetOpponent(node.Opponent.Name, archName, portrait, archColor);

                if (_run.PlayerDoctrine.HasValue)
                    GameManager.Hud.SetPlayerIdentity($"The {_run.PlayerDoctrine.Value}",
                        $"You · Act {_run.CurrentAct + 1} of 5");

                if (!string.IsNullOrEmpty(node.Opponent.GimmickDescription))
                    GameManager.Hud.SetInfo(node.Opponent.GimmickDescription);
            }

            var engine = GameManager.Engine;
            if (engine != null)
                engine.OnBoutResolved += OnBoutResolved;
        }

        Sprite FindPortrait(string opponentName)
        {
            if (PortraitNames == null || PortraitSprites == null) return null;
            for (int i = 0; i < PortraitNames.Length && i < PortraitSprites.Length; i++)
                if (PortraitNames[i] == opponentName) return PortraitSprites[i];
            return null;
        }

        static Color ArchetypeColor(AIArchetypeName arch) => arch switch
        {
            AIArchetypeName.Brawler => ThemePalette.AtkColor,
            AIArchetypeName.Miser => ThemePalette.DefColor,
            AIArchetypeName.Fox => ThemePalette.UtilColor,
            AIArchetypeName.Noble => ThemePalette.Gold,
            AIArchetypeName.Scholar => ThemePalette.AbilityBlue,
            AIArchetypeName.Assassin => ThemePalette.VenetianRed,
            _ => ThemePalette.DustyTan,
        };

        void OnBoutResolved(BoutOutcome outcome)
        {
            _run.TotalBoutsPlayed++;
            if (outcome == BoutOutcome.DefenderWonAllDiscarded)
            {
                var engine = GameManager.Engine;
                if (engine != null && engine.DefenderIndex == 0)
                    _boutsDefended++;
            }
        }

        void OnMatchComplete(int winnerIndex)
        {
            GameManager.OnMatchComplete -= OnMatchComplete;
            var engine = GameManager.Engine;
            if (engine != null) engine.OnBoutResolved -= OnBoutResolved;
            _run.MatchesPlayed++;
            _run.TotalBoutsDefended += _boutsDefended;

            bool won = winnerIndex == 0;
            int florinsEarned = 0;

            if (won)
            {
                _run.MatchesWon++;
                florinsEarned = CalculateFlorins(_run.CurrentAct, _currentNode.Type);
                if (_run.PlayerTrinkets.Contains(TrinketType.MerchantsPurse))
                    florinsEarned += 3;
                if (_run.PlayerTrinkets.Contains(TrinketType.MisersRing))
                    florinsEarned += _boutsDefended;
                if (_run.PlayerRelics.Contains(RelicType.MisersHoard))
                    florinsEarned = (int)(florinsEarned * 1.5f);
                _run.Florins += florinsEarned;
            }
            else
            {
                if (_run.PlayerRelics.Contains(RelicType.PhoenixFeather))
                    _run.PlayerRelics.Remove(RelicType.PhoenixFeather);
                else
                {
                    int loss = Ascension.BrutalDefeats(_run.AscensionLevel) ? 2 : 1;
                    _run.Prestige -= loss;
                }
                AssignRandomBurden();
                if (_run.Prestige <= 0 && _run.PlayerTrinkets.Contains(TrinketType.PhoenixMedal) && !_run.PhoenixMedalUsed)
                {
                    _run.PhoenixMedalUsed = true;
                    _run.Prestige = 1;
                }
                if (_run.Prestige <= 0)
                {
                    _run.RunComplete = true;
                    _run.RunWon = false;
                    SetPhase(RunPhase.RunOver);
                    return;
                }
            }

            ShowResult(won, florinsEarned);
        }

        List<AbilityType> _abilityPickOfferings;

        void ShowResult(bool won, int florinsEarned)
        {
            SetPhase(RunPhase.PostMatch);
            UIScreenCapture.Instance?.NotifyModal(won ? UIScreen.MatchVictory : UIScreen.MatchDefeat);

            if (ResultTitleLabel)
                ResultTitleLabel.text = won ? "Victory!" : "Defeat...";
            if (ResultDetailsLabel)
            {
                string details;
                if (won)
                    details = $"You defeated {_currentNode.Opponent?.Name ?? "opponent"}!";
                else
                {
                    details = $"{_currentNode.Opponent?.Name ?? "Opponent"} bested you.\nPrestige remaining: {_run.Prestige}";
                    if (_run.PlayerBurdens.Count > 0)
                        details += $"\nGained: {_run.PlayerBurdens[^1].DisplayName()}";
                }
                ResultDetailsLabel.text = details;
            }
            if (ResultRewardLabel)
            {
                if (won && florinsEarned > 0)
                    ResultRewardLabel.text = $"+{florinsEarned} Florins";
                else
                    ResultRewardLabel.text = "";
            }

            _cardRewardOfferings = null;
            _abilityPickOfferings = null;
            _relicPickOfferings = null;
            HideCardReward();
            HideAbilityPick();

            if (won)
            {
                if (ShouldSkipCardReward())
                    FinishCardReward();
                else
                    ShowCardReward();
            }

            if (ResultContinueButton)
            {
                ResultContinueButton.onClick.RemoveAllListeners();
                ResultContinueButton.onClick.AddListener(OnResultContinue);
                bool showContinue = !won || (_cardRewardOfferings == null || _cardRewardOfferings.Count == 0);
                ResultContinueButton.gameObject.SetActive(showContinue);
            }
        }

        List<AbilityType> PickAbilityOfferings(int count)
        {
            bool isElite = _currentNode?.Type == MapNodeType.EliteMatch || _currentNode?.Type == MapNodeType.BossMatch;
            return PickAbilityOfferingsWeighted(count, isElite, _rng);
        }

        static List<AbilityDefinition> BuildAbilityPool(ArchetypeType? archetype,
            List<AbilityType> owned, bool isElite, System.Random rng)
        {
            var pool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (owned.Contains(def.Type)) continue;
                if (AbilityUpgrades.TryGetUpgrade(def.Type, out var upgraded)
                    && owned.Contains(upgraded)) continue;
                if (!def.IsNeutral && def.Owner != archetype) continue;
                if (!isElite && def.Rarity == AbilityRarity.Rare && rng.Next(100) >= 30) continue;
                pool.Add(def);
            }
            return pool;
        }

        List<AbilityType> PickAbilityOfferingsWeighted(int count, bool isElite, System.Random rng)
        {
            var archPool = new List<AbilityDefinition>();
            var neutralPool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (_run.PlayerAbilities.Contains(def.Type)) continue;
                if (AbilityUpgrades.TryGetUpgrade(def.Type, out var upg)
                    && _run.PlayerAbilities.Contains(upg)) continue;
                if (!def.IsNeutral && def.Owner != _run.PlayerArchetype) continue;
                if (!isElite && def.Rarity == AbilityRarity.Rare && rng.Next(100) >= 30) continue;
                if (def.IsNeutral) neutralPool.Add(def);
                else archPool.Add(def);
            }

            var result = new List<AbilityType>();
            if (archPool.Count > 0)
            {
                int idx = rng.Next(archPool.Count);
                result.Add(archPool[idx].Type);
                archPool.RemoveAt(idx);
            }

            var combined = new List<AbilityDefinition>();
            combined.AddRange(archPool);
            combined.AddRange(neutralPool);
            while (result.Count < count && combined.Count > 0)
            {
                int idx = rng.Next(combined.Count);
                result.Add(combined[idx].Type);
                combined.RemoveAt(idx);
            }
            return result;
        }

        void ShowAbilityPick()
        {
            UIScreenCapture.Instance?.NotifyModal(UIScreen.AbilityPick);
            if (AbilityPickLabel) AbilityPickLabel.text = "Choose an ability:";
            if (AbilityPickLabel) AbilityPickLabel.gameObject.SetActive(true);
            ClearAbilityPickButtons();

            if (AbilityPickContainer)
            {
                foreach (var abilityType in _abilityPickOfferings)
                    CreateAbilityPickButton(abilityType);
            }

            if (AbilityPickSkipButton)
            {
                AbilityPickSkipButton.gameObject.SetActive(true);
                AbilityPickSkipButton.onClick.RemoveAllListeners();
                AbilityPickSkipButton.onClick.AddListener(OnAbilityPickSkip);
            }
        }

        void HideAbilityPick()
        {
            if (AbilityPickLabel) AbilityPickLabel.gameObject.SetActive(false);
            ClearAbilityPickButtons();
            if (AbilityPickSkipButton) AbilityPickSkipButton.gameObject.SetActive(false);
        }

        void ClearAbilityPickButtons()
        {
            if (!AbilityPickContainer) return;
            for (int i = AbilityPickContainer.childCount - 1; i >= 0; i--)
                Destroy(AbilityPickContainer.GetChild(i).gameObject);
        }

        void CreateAbilityPickButton(AbilityType abilityType)
        {
            var def = AbilityPool.Get(abilityType);

            var btnGO = new GameObject($"Pick_{abilityType}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(AbilityPickContainer, false);

            var bgColor = def.Rarity switch
            {
                AbilityRarity.Common => ThemePalette.RarityCommonBg,
                AbilityRarity.Uncommon => ThemePalette.RarityUncommonBg,
                AbilityRarity.Rare => ThemePalette.RarityRareBg,
                _ => ThemePalette.LockedGray
            };
            var img = btnGO.GetComponent<Image>();
            img.color = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 0.9f);

            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 104;
            le.preferredWidth = 650;

            // Rarity accent bar (left edge)
            var rarityColor = def.Rarity switch
            {
                AbilityRarity.Common => ThemePalette.RarityCommon,
                AbilityRarity.Uncommon => ThemePalette.RarityUncommon,
                AbilityRarity.Rare => ThemePalette.RarityRare,
                _ => ThemePalette.DustyTan
            };
            var accentGO = new GameObject("RarityAccent", typeof(RectTransform), typeof(Image));
            accentGO.transform.SetParent(btnGO.transform, false);
            var accentRT = (RectTransform)accentGO.transform;
            accentRT.anchorMin = new Vector2(0, 0);
            accentRT.anchorMax = new Vector2(0, 1);
            accentRT.pivot = new Vector2(0, 0.5f);
            accentRT.sizeDelta = new Vector2(4, 0);
            accentRT.anchoredPosition = Vector2.zero;
            accentGO.GetComponent<Image>().color = rarityColor;
            accentGO.GetComponent<Image>().raycastTarget = false;

            // Ability type indicator dot
            var typeColor = ThemePalette.AbilityBadgeColor(abilityType);
            var dotGO = new GameObject("TypeDot", typeof(RectTransform), typeof(Image));
            dotGO.transform.SetParent(btnGO.transform, false);
            var dotRT = (RectTransform)dotGO.transform;
            dotRT.anchorMin = new Vector2(0, 0.5f);
            dotRT.anchorMax = new Vector2(0, 0.5f);
            dotRT.pivot = new Vector2(0.5f, 0.5f);
            dotRT.sizeDelta = new Vector2(12, 12);
            dotRT.anchoredPosition = new Vector2(16, 0);
            dotGO.GetComponent<Image>().color = typeColor;
            dotGO.GetComponent<Image>().raycastTarget = false;

            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(btnGO.transform, false);
            var nameRT = (RectTransform)nameGO.transform;
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(32, 0);
            nameRT.offsetMax = new Vector2(-12, -2);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            var headingFont = FontAssets.Heading;
            if (headingFont) nameTMP.font = headingFont;
            string rarityTag = def.Rarity != AbilityRarity.Common ? $"  [{def.Rarity}]" : "";
            string bindTag = def.IsPassive ? " (Passive)" : $" ({def.BindingCount} cards)";
            string pathTag = !string.IsNullOrEmpty(def.BuildPath) ? $"  <color=#9988AA>{def.BuildPath}</color>" : "";
            nameTMP.text = $"{abilityType.DisplayName()}{rarityTag}{bindTag}{pathTag}";
            nameTMP.richText = true;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.fontSize = 23;
            nameTMP.color = ThemePalette.Parchment;
            nameTMP.raycastTarget = false;

            var descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(btnGO.transform, false);
            var descRT = (RectTransform)descGO.transform;
            descRT.anchorMin = Vector2.zero;
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.offsetMin = new Vector2(32, 2);
            descRT.offsetMax = new Vector2(-12, 0);
            var descTMP = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text = abilityType.Description();
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.fontSize = 18;
            descTMP.color = ThemePalette.DescGray;
            descTMP.raycastTarget = false;
            descTMP.enableWordWrapping = true;

            var btn = btnGO.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.95f);
            btn.colors = colors;
            var captured = abilityType;
            btn.onClick.AddListener(() => OnAbilityPicked(captured));
        }

        void OnAbilityPicked(AbilityType type)
        {
            if (_run.PlayerAbilities.Count >= _run.MaxAbilitySlots)
            {
                int worstIdx = FindWorstAbilityIndex();
                var worst = _run.PlayerAbilities[worstIdx];
                _run.PlayerAbilities.RemoveAt(worstIdx);
                Debug.Log($"[RunManager] Replaced {worst.DisplayName()} with {type.DisplayName()}");
            }
            _run.PlayerAbilities.Add(type);
            _run.RecordAbilityPicked(type);
            UpdateRunHud();
            FinishAbilityPick();
        }

        int FindWorstAbilityIndex()
        {
            int worst = 0;
            int worstScore = AbilityKeepScore(_run.PlayerAbilities[0]);
            for (int i = 1; i < _run.PlayerAbilities.Count; i++)
            {
                int score = AbilityKeepScore(_run.PlayerAbilities[i]);
                if (score < worstScore)
                {
                    worst = i;
                    worstScore = score;
                }
            }
            return worst;
        }

        int AbilityKeepScore(AbilityType type)
        {
            var def = AbilityPool.Get(type);
            int score = def.Rarity switch
            {
                AbilityRarity.Common => 0,
                AbilityRarity.Uncommon => 100,
                AbilityRarity.Rare => 200,
                _ => 0
            };
            if (!def.IsNeutral && def.Owner == _run.PlayerArchetype)
                score += 50;
            return score;
        }

        void OnAbilityPickSkip()
        {
            FinishAbilityPick();
        }

        void FinishAbilityPick()
        {
            _abilityPickOfferings = null;
            HideAbilityPick();

            bool isBossOrElite = _currentNode?.Type == MapNodeType.EliteMatch
                              || _currentNode?.Type == MapNodeType.BossMatch;
            if (isBossOrElite && _relicPickOfferings == null)
            {
                _relicPickOfferings = PickRelicOfferings(3);
                if (_relicPickOfferings.Count > 0)
                {
                    ShowRelicPick();
                    return;
                }
            }

            _relicPickOfferings = null;
            if (ResultContinueButton) ResultContinueButton.gameObject.SetActive(true);
        }

        List<RelicType> _relicPickOfferings;

        static readonly RelicType[] BossRelicTypes =
        {
            RelicType.TitansCrown, RelicType.SovereignsDecree, RelicType.HeraldsHorn,
            RelicType.WanderersBoots, RelicType.MisersHoard, RelicType.PhoenixFeather
        };

        static readonly Dictionary<RelicType, string> BossRelicNames = new()
        {
            { RelicType.TitansCrown, "Titan's Crown" },
            { RelicType.SovereignsDecree, "Sovereign's Decree" },
            { RelicType.HeraldsHorn, "Herald's Horn" },
            { RelicType.WanderersBoots, "Wanderer's Boots" },
            { RelicType.MisersHoard, "Miser's Hoard" },
            { RelicType.PhoenixFeather, "Phoenix Feather" },
        };

        static readonly Dictionary<RelicType, string> BossRelicDescs = new()
        {
            { RelicType.TitansCrown, "+2 hand size in all matches" },
            { RelicType.SovereignsDecree, "+1 max attacks per bout" },
            { RelicType.HeraldsHorn, "+3 starting resource each match" },
            { RelicType.WanderersBoots, "+1 ability slot" },
            { RelicType.MisersHoard, "+50% Florins from matches" },
            { RelicType.PhoenixFeather, "Prevents one prestige loss per match" },
        };

        List<RelicType> PickRelicOfferings(int count)
        {
            var available = new List<RelicType>();
            foreach (var r in BossRelicTypes)
                if (!_run.PlayerRelics.Contains(r)) available.Add(r);
            var result = new List<RelicType>();
            int picks = Math.Min(count, available.Count);
            for (int i = 0; i < picks; i++)
            {
                int idx = _rng.Next(available.Count);
                result.Add(available[idx]);
                available.RemoveAt(idx);
            }
            return result;
        }

        void ShowRelicPick()
        {
            UIScreenCapture.Instance?.NotifyModal(UIScreen.RelicPick);
            if (AbilityPickLabel)
            {
                AbilityPickLabel.text = "Choose a boss relic:";
                AbilityPickLabel.gameObject.SetActive(true);
            }
            ClearAbilityPickButtons();
            if (AbilityPickContainer)
                foreach (var relic in _relicPickOfferings)
                    CreateRelicPickButton(relic);
            if (AbilityPickSkipButton)
            {
                AbilityPickSkipButton.gameObject.SetActive(true);
                AbilityPickSkipButton.onClick.RemoveAllListeners();
                AbilityPickSkipButton.onClick.AddListener(OnRelicPickSkip);
            }
        }

        void CreateRelicPickButton(RelicType relicType)
        {
            string name = BossRelicNames.TryGetValue(relicType, out var n) ? n : relicType.ToString();
            string desc = BossRelicDescs.TryGetValue(relicType, out var d) ? d : "";

            var btnGO = new GameObject($"Relic_{relicType}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(AbilityPickContainer, false);

            var img = btnGO.GetComponent<Image>();
            img.color = new Color(0.25f, 0.18f, 0.12f, 0.9f);

            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 104;
            le.preferredWidth = 650;

            var accentGO = new GameObject("RelicAccent", typeof(RectTransform), typeof(Image));
            accentGO.transform.SetParent(btnGO.transform, false);
            var accentRT = (RectTransform)accentGO.transform;
            accentRT.anchorMin = new Vector2(0, 0);
            accentRT.anchorMax = new Vector2(0, 1);
            accentRT.pivot = new Vector2(0, 0.5f);
            accentRT.sizeDelta = new Vector2(4, 0);
            accentRT.anchoredPosition = Vector2.zero;
            accentGO.GetComponent<Image>().color = ThemePalette.Gold;
            accentGO.GetComponent<Image>().raycastTarget = false;

            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(btnGO.transform, false);
            var nameRT = (RectTransform)nameGO.transform;
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(16, 0);
            nameRT.offsetMax = new Vector2(-12, -2);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            var headingFont = FontAssets.Heading;
            if (headingFont) nameTMP.font = headingFont;
            nameTMP.text = name;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.fontSize = 23;
            nameTMP.color = ThemePalette.Gold;
            nameTMP.raycastTarget = false;

            var descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(btnGO.transform, false);
            var descRT = (RectTransform)descGO.transform;
            descRT.anchorMin = Vector2.zero;
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.offsetMin = new Vector2(16, 2);
            descRT.offsetMax = new Vector2(-12, 0);
            var descTMP = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text = desc;
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.fontSize = 18;
            descTMP.color = ThemePalette.DustyTan;
            descTMP.raycastTarget = false;
            descTMP.enableWordWrapping = true;

            var btn = btnGO.GetComponent<Button>();
            var captured = relicType;
            btn.onClick.AddListener(() => OnRelicPicked(captured));
        }

        void OnRelicPicked(RelicType type)
        {
            _run.PlayerRelics.Add(type);
            if (type == RelicType.WanderersBoots)
                _run.MaxAbilitySlots++;
            FinishRelicPick();
        }

        void OnRelicPickSkip() => FinishRelicPick();

        void FinishRelicPick()
        {
            _relicPickOfferings = null;
            HideAbilityPick();
            if (ResultContinueButton) ResultContinueButton.gameObject.SetActive(true);
        }

        void OnResultContinue()
        {
            if (_currentColumn >= _run.CurrentMap.Count)
            {
                AdvanceAct();
                return;
            }
            SetPhase(RunPhase.MapSelect);
        }

        // ---------- Shop ----------

        void HandleShop()
        {
            SetPhase(RunPhase.Shop);

            if (ShopTitleLabel) ShopTitleLabel.text = "The Fence";
            UpdateShopFlorins();
            PopulateShopItems();

            if (ShopLeaveButton)
            {
                ShopLeaveButton.onClick.RemoveAllListeners();
                ShopLeaveButton.onClick.AddListener(OnResultContinue);
            }
        }

        void UpdateShopFlorins()
        {
            if (ShopFlorinsLabel) ShopFlorinsLabel.text = $"Your purse: {_run.Florins} Florins";
        }

        void PopulateShopItems()
        {
            ClearShopItems();
            if (!ShopItemContainer) return;

            var abilityOfferings = PickShopOfferings(2);
            if (abilityOfferings.Count > 0)
            {
                CreateShopSectionHeader("Abilities", ThemePalette.AbilityBadgeColor(AbilityType.ExtraDraw));
                foreach (var offering in abilityOfferings)
                    CreateShopItemButton(offering);
            }

            CreateCardShopItems();

            var trinketOffering = PickTrinketOffering();
            if (trinketOffering.HasValue)
            {
                CreateShopSectionHeader("Trinkets", ThemePalette.Gold);
                CreateTrinketShopButton(trinketOffering.Value);
            }

            bool hasServices = false;
            var upgradeOffering = PickCardUpgradeOffering();
            bool hasBurden = _run.PlayerBurdens.Count > 0;
            bool hasRemoval = _run.PlayerDeckCardIds.Count > 8 && _run.CardRemovalsPurchased < 3;
            bool hasTransform = _run.PlayerDoctrine == DoctrineType.Trickster && HasTransformableCommon();
            if (upgradeOffering.HasValue || hasBurden || hasRemoval || hasTransform)
            {
                CreateShopSectionHeader("Services", new Color(0.5f, 0.65f, 0.5f));
                hasServices = true;
            }

            if (upgradeOffering.HasValue)
                CreateCardUpgradeButton(upgradeOffering.Value);

            if (hasBurden)
                CreateBurdenRemovalButton();

            if (hasRemoval)
                CreateCardRemovalButton();

            if (_run.PlayerDoctrine == DoctrineType.Trickster && HasTransformableCommon())
                CreateCardTransformButton();
        }

        void CreateShopSectionHeader(string title, Color accentColor)
        {
            var headerGO = new GameObject($"Section_{title}", typeof(RectTransform), typeof(LayoutElement));
            headerGO.transform.SetParent(ShopItemContainer, false);
            headerGO.GetComponent<LayoutElement>().preferredHeight = 32;
            headerGO.GetComponent<LayoutElement>().preferredWidth = 550;

            var lineGO = new GameObject("Line", typeof(RectTransform), typeof(Image));
            lineGO.transform.SetParent(headerGO.transform, false);
            var lineRT = (RectTransform)lineGO.transform;
            lineRT.anchorMin = new Vector2(0, 0);
            lineRT.anchorMax = new Vector2(1, 0);
            lineRT.pivot = new Vector2(0.5f, 0);
            lineRT.sizeDelta = new Vector2(0, 1);
            lineGO.GetComponent<Image>().color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.3f);
            lineGO.GetComponent<Image>().raycastTarget = false;

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(headerGO.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(8, 0);
            labelRT.offsetMax = Vector2.zero;
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Heading) tmp.font = FontAssets.Heading;
            tmp.text = title.ToUpper();
            tmp.fontSize = 14;
            tmp.color = accentColor;
            tmp.alignment = TextAlignmentOptions.BottomLeft;
            tmp.raycastTarget = false;
        }

        void ClearShopItems()
        {
            if (!ShopItemContainer) return;
            for (int i = ShopItemContainer.childCount - 1; i >= 0; i--)
                Destroy(ShopItemContainer.GetChild(i).gameObject);
        }

        List<(AbilityType type, int price)> PickShopOfferings(int count)
        {
            var result = new List<(AbilityType, int)>();
            var pool = BuildAbilityPool(_run.PlayerArchetype, _run.PlayerAbilities, true, _rng);

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int idx = _rng.Next(pool.Count);
                var def = pool[idx];
                pool.RemoveAt(idx);
                int price = def.Rarity switch
                {
                    AbilityRarity.Common => 8,
                    AbilityRarity.Uncommon => 12,
                    AbilityRarity.Rare => 18,
                    _ => 12
                };
                if (Ascension.InflatedPrices(_run.AscensionLevel))
                    price = (int)(price * 1.25f);
                if (_run.PlayerBurdens.Contains(BurdenType.BadReputation))
                    price = (int)(price * 1.20f);
                result.Add((def.Type, price));
            }
            return result;
        }

        void CreateShopItemButton(( AbilityType type, int price) offering)
        {
            var def = AbilityPool.Get(offering.type);
            bool canBuy = _run.Florins >= offering.price && _run.PlayerAbilities.Count < _run.MaxAbilitySlots;
            string desc = offering.type.Description();

            var btnGO = new GameObject("ShopItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(ShopItemContainer, false);

            var img = btnGO.GetComponent<Image>();
            img.color = canBuy ? new Color(0.12f, 0.22f, 0.30f, 0.85f) : ThemePalette.LockedGray;

            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 85;
            le.preferredWidth = 550;

            // Rarity accent bar
            var rarityColor = def.Rarity switch
            {
                AbilityRarity.Common => ThemePalette.RarityCommon,
                AbilityRarity.Uncommon => ThemePalette.RarityUncommon,
                AbilityRarity.Rare => ThemePalette.RarityRare,
                _ => ThemePalette.DustyTan
            };
            var accentGO = new GameObject("RarityAccent", typeof(RectTransform), typeof(Image));
            accentGO.transform.SetParent(btnGO.transform, false);
            var accentRT = (RectTransform)accentGO.transform;
            accentRT.anchorMin = new Vector2(0, 0);
            accentRT.anchorMax = new Vector2(0, 1);
            accentRT.pivot = new Vector2(0, 0.5f);
            accentRT.sizeDelta = new Vector2(4, 0);
            accentRT.anchoredPosition = Vector2.zero;
            accentGO.GetComponent<Image>().color = canBuy ? rarityColor : ThemePalette.DisabledOutline;
            accentGO.GetComponent<Image>().raycastTarget = false;

            // Ability type dot
            var typeColor = ThemePalette.AbilityBadgeColor(offering.type);
            var dotGO = new GameObject("TypeDot", typeof(RectTransform), typeof(Image));
            dotGO.transform.SetParent(btnGO.transform, false);
            var dotRT = (RectTransform)dotGO.transform;
            dotRT.anchorMin = new Vector2(0, 0.5f);
            dotRT.anchorMax = new Vector2(0, 0.5f);
            dotRT.pivot = new Vector2(0.5f, 0.5f);
            dotRT.sizeDelta = new Vector2(12, 12);
            dotRT.anchoredPosition = new Vector2(18, 0);
            dotGO.GetComponent<Image>().color = canBuy ? typeColor : ThemePalette.DisabledOutline;
            dotGO.GetComponent<Image>().raycastTarget = false;

            // Price tag (right side)
            var priceGO = new GameObject("Price", typeof(RectTransform));
            priceGO.transform.SetParent(btnGO.transform, false);
            var priceRT = (RectTransform)priceGO.transform;
            priceRT.anchorMin = new Vector2(1, 0);
            priceRT.anchorMax = new Vector2(1, 1);
            priceRT.pivot = new Vector2(1, 0.5f);
            priceRT.sizeDelta = new Vector2(70, 0);
            priceRT.anchoredPosition = new Vector2(-12, 0);
            var priceTMP = priceGO.AddComponent<TextMeshProUGUI>();
            var monoFont = FontAssets.Mono;
            if (monoFont) priceTMP.font = monoFont;
            priceTMP.text = $"{offering.price}f";
            priceTMP.alignment = TextAlignmentOptions.Center;
            priceTMP.fontSize = 22;
            priceTMP.color = canBuy ? ThemePalette.Gold : ThemePalette.DisabledText;
            priceTMP.raycastTarget = false;

            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(btnGO.transform, false);
            var nameRT = (RectTransform)nameGO.transform;
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(36, 0);
            nameRT.offsetMax = new Vector2(-85, -4);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            var headingFont = FontAssets.Heading;
            if (headingFont) nameTMP.font = headingFont;
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            string rarityTag = def.Rarity != AbilityRarity.Common ? $"  <color=#{rarityHex}><size=80%>{def.Rarity}</size></color>" : "";
            nameTMP.text = $"{offering.type.DisplayName()}{rarityTag}";
            nameTMP.richText = true;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.fontSize = 18;
            nameTMP.color = canBuy ? ThemePalette.Parchment : ThemePalette.DisabledText;
            nameTMP.raycastTarget = false;

            var descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(btnGO.transform, false);
            var descRT = (RectTransform)descGO.transform;
            descRT.anchorMin = Vector2.zero;
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.offsetMin = new Vector2(36, 4);
            descRT.offsetMax = new Vector2(-85, 0);
            var descTMP = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text = desc;
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.fontSize = 14;
            descTMP.color = ThemePalette.DescGray;
            descTMP.raycastTarget = false;
            descTMP.enableWordWrapping = true;

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = canBuy;
            var captured = offering;
            btn.onClick.AddListener(() => OnShopBuy(captured.type, captured.price));
        }

        void CreateBurdenRemovalButton()
        {
            int price = 6;
            if (Ascension.InflatedPrices(_run.AscensionLevel))
                price = (int)(price * 1.25f);
            if (_run.PlayerBurdens.Contains(BurdenType.BadReputation))
                price = (int)(price * 1.20f);
            bool canBuy = _run.Florins >= price;
            var burden = _run.PlayerBurdens[0];

            var btnGO = new GameObject("BurdenRemoval", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(ShopItemContainer, false);

            var img = btnGO.GetComponent<Image>();
            img.color = canBuy ? ThemePalette.ShopConsumableBg : ThemePalette.LockedGray;

            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.preferredWidth = 550;

            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(btnGO.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = new Vector2(16, 0);
            lblRT.offsetMax = new Vector2(-16, 0);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text = $"Remove {burden.DisplayName()}  —  {price}f";
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 20;
            lbl.color = canBuy ? Color.white : ThemePalette.DisabledText;
            lbl.raycastTarget = false;

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = canBuy;
            btn.onClick.AddListener(() => OnShopRemoveBurden(burden, price));
        }

        int GetCardRemovalPrice()
        {
            int price = _run.PlayerDoctrine switch
            {
                DoctrineType.Schemer => 4,
                DoctrineType.Brute => 5,
                DoctrineType.Hoarder => 8,
                _ => 8
            };
            if (Ascension.InflatedPrices(_run.AscensionLevel))
                price = (int)(price * 1.25f);
            if (_run.PlayerBurdens.Contains(BurdenType.BadReputation))
                price = (int)(price * 1.20f);
            return price;
        }

        void CreateCardRemovalButton()
        {
            string weakest = FindWeakestDeckCard();
            if (weakest == null) return;
            string cardName = CardCatalog.TryGet(weakest, out var wDef) ? wDef.Name : weakest;
            int price = GetCardRemovalPrice();
            bool canBuy = _run.Florins >= price;

            var btnGO = new GameObject("CardRemoval", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(ShopItemContainer, false);
            btnGO.GetComponent<Image>().color = canBuy ? new Color(0.35f, 0.12f, 0.12f, 0.85f) : ThemePalette.LockedGray;
            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.preferredWidth = 550;

            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(btnGO.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = new Vector2(16, 0);
            lblRT.offsetMax = new Vector2(-16, 0);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text = $"Remove \"{cardName}\"  —  {price}f";
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 20;
            lbl.color = canBuy ? ThemePalette.Parchment : ThemePalette.DisabledText;
            lbl.raycastTarget = false;

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = canBuy;
            string capturedId = weakest;
            btn.onClick.AddListener(() => OnShopRemoveCard(capturedId, price));
        }

        void OnShopRemoveCard(string cardId, int price)
        {
            if (_run.Florins < price) return;
            _run.Florins -= price;
            _run.PlayerDeckCardIds.Remove(cardId);
            _run.CardAbilityOverrides.Remove(cardId);
            _run.CardRankOverrides.Remove(cardId);
            _run.CardRemovalsPurchased++;
            UpdateRunHud();
            UpdateShopFlorins();
            PopulateShopItems();
        }

        bool HasTransformableCommon()
        {
            if (!_run.PlayerDoctrine.HasValue) return false;
            foreach (var id in _run.PlayerDeckCardIds)
            {
                if (CardCatalog.TryGet(id, out var def) && def.Rarity == CardRarity.Common)
                    return true;
            }
            return false;
        }

        void CreateCardTransformButton()
        {
            int price = 10;
            if (Ascension.InflatedPrices(_run.AscensionLevel))
                price = (int)(price * 1.25f);
            if (_run.PlayerBurdens.Contains(BurdenType.BadReputation))
                price = (int)(price * 1.20f);
            bool canBuy = _run.Florins >= price;

            var btnGO = new GameObject("CardTransform", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(ShopItemContainer, false);
            btnGO.GetComponent<Image>().color = canBuy ? new Color(0.18f, 0.12f, 0.30f, 0.85f) : ThemePalette.LockedGray;
            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.preferredWidth = 550;

            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(btnGO.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = new Vector2(16, 0);
            lblRT.offsetMax = new Vector2(-16, 0);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text = $"Transform a Common → Uncommon  —  {price}f";
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 20;
            lbl.color = canBuy ? new Color(0.7f, 0.55f, 0.9f) : ThemePalette.DisabledText;
            lbl.raycastTarget = false;

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = canBuy;
            int capturedPrice = price;
            btn.onClick.AddListener(() => OnShopTransformCard(capturedPrice));
        }

        void OnShopTransformCard(int price)
        {
            if (_run.Florins < price || !_run.PlayerDoctrine.HasValue) return;
            var commons = new List<string>();
            foreach (var id in _run.PlayerDeckCardIds)
            {
                if (CardCatalog.TryGet(id, out var def) && def.Rarity == CardRarity.Common)
                    commons.Add(id);
            }
            if (commons.Count == 0) return;

            var pool = CardCatalog.Draftable(_run.PlayerDoctrine.Value);
            var uncommons = new List<CardDefinition>();
            foreach (var c in pool)
            {
                if (c.Rarity == CardRarity.Uncommon && !_run.PlayerDeckCardIds.Contains(c.Id))
                    uncommons.Add(c);
            }
            if (uncommons.Count == 0) return;

            string oldId = commons[_rng.Next(commons.Count)];
            var newCard = uncommons[_rng.Next(uncommons.Count)];
            _run.PlayerDeckCardIds.Remove(oldId);
            _run.PlayerDeckCardIds.Add(newCard.Id);
            _run.Florins -= price;

            string oldName = CardCatalog.TryGet(oldId, out var oldDef) ? oldDef.Name : oldId;
            Debug.Log($"[RunManager] Transformed '{oldName}' → '{newCard.Name}'. Deck size: {_run.PlayerDeckCardIds.Count}");
            UpdateRunHud();
            UpdateShopFlorins();
            PopulateShopItems();
        }

        (TrinketType type, int price)? PickTrinketOffering()
        {
            if (_run.PlayerTrinkets.Count >= 5) return null;
            var pool = new List<TrinketType>();
            foreach (TrinketType t in Enum.GetValues(typeof(TrinketType)))
            {
                if (_run.PlayerTrinkets.Contains(t)) continue;
                if (t == TrinketType.PhoenixMedal && _run.PhoenixMedalUsed) continue;
                pool.Add(t);
            }
            if (pool.Count == 0) return null;
            var trinket = pool[_rng.Next(pool.Count)];
            int price = trinket.AffectsEngine() ? 15 : 10;
            if (Ascension.InflatedPrices(_run.AscensionLevel))
                price = (int)(price * 1.25f);
            if (_run.PlayerBurdens.Contains(BurdenType.BadReputation))
                price = (int)(price * 1.20f);
            return (trinket, price);
        }

        void CreateTrinketShopButton((TrinketType type, int price) offering)
        {
            bool canBuy = _run.Florins >= offering.price && _run.PlayerTrinkets.Count < 5;
            string label = $"{offering.type.DisplayName()}  —  {offering.price}f  [Trinket]";
            string desc = offering.type.Description();

            var btnGO = new GameObject("TrinketItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(ShopItemContainer, false);
            btnGO.GetComponent<Image>().color = canBuy ? ThemePalette.ShopAbilityBg : ThemePalette.LockedGray;
            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 90;
            le.preferredWidth = 550;

            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(btnGO.transform, false);
            var nameRT = (RectTransform)nameGO.transform;
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(16, 0);
            nameRT.offsetMax = new Vector2(-16, -4);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text = label;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.fontSize = 22;
            nameTMP.color = canBuy ? ThemePalette.Gold : ThemePalette.DisabledText;
            nameTMP.raycastTarget = false;

            var descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(btnGO.transform, false);
            var descRT = (RectTransform)descGO.transform;
            descRT.anchorMin = Vector2.zero;
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.offsetMin = new Vector2(16, 4);
            descRT.offsetMax = new Vector2(-16, 0);
            var descTMP = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text = desc;
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.fontSize = 16;
            descTMP.color = ThemePalette.DescGray;
            descTMP.raycastTarget = false;
            descTMP.enableWordWrapping = true;

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = canBuy;
            var captured = offering;
            btn.onClick.AddListener(() => OnShopBuyTrinket(captured.type, captured.price));
        }

        void OnShopBuyTrinket(TrinketType type, int price)
        {
            if (_run.Florins < price || _run.PlayerTrinkets.Count >= 5) return;
            _run.Florins -= price;
            _run.PlayerTrinkets.Add(type);
            if (type == TrinketType.ScholarsTome) _run.MaxAbilitySlots++;
            UpdateRunHud();
            UpdateShopFlorins();
            PopulateShopItems();
        }

        (string cardId, string cardName, Rank currentRank, Rank newRank, int price)? PickCardUpgradeOffering()
        {
            var candidates = new List<(string id, string name, Rank rank)>();
            foreach (var cardId in _run.PlayerDeckCardIds)
            {
                var def = CardCatalog.Get(cardId);
                Rank effective = _run.CardRankOverrides.TryGetValue(cardId, out var ov) ? ov : def.Rank;
                if (effective >= Rank.Ace) continue;
                candidates.Add((cardId, def.Name, effective));
            }
            if (candidates.Count == 0) return null;
            var pick = candidates[_rng.Next(candidates.Count)];
            Rank newRank = pick.rank + 1;
            int price = (int)pick.rank >= 10 ? 10 : 6;
            if (Ascension.InflatedPrices(_run.AscensionLevel))
                price = (int)(price * 1.25f);
            if (_run.PlayerBurdens.Contains(BurdenType.BadReputation))
                price = (int)(price * 1.20f);
            return (pick.id, pick.name, pick.rank, newRank, price);
        }

        void CreateCardUpgradeButton((string cardId, string cardName, Rank currentRank, Rank newRank, int price) offering)
        {
            bool canBuy = _run.Florins >= offering.price;
            string label = $"Hone: {offering.cardName}  {offering.currentRank.Label()} → {offering.newRank.Label()}  —  {offering.price}f";

            var btnGO = new GameObject("CardUpgrade", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(ShopItemContainer, false);
            btnGO.GetComponent<Image>().color = canBuy ? new Color(0.18f, 0.14f, 0.10f, 0.85f) : ThemePalette.LockedGray;
            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.preferredWidth = 550;

            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(btnGO.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = new Vector2(16, 0);
            lblRT.offsetMax = new Vector2(-16, 0);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text = label;
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 20;
            lbl.color = canBuy ? ThemePalette.Parchment : ThemePalette.DisabledText;
            lbl.raycastTarget = false;

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = canBuy;
            var captured = offering;
            btn.onClick.AddListener(() => OnShopUpgradeCard(captured.cardId, captured.newRank, captured.price));
        }

        void OnShopUpgradeCard(string cardId, Rank newRank, int price)
        {
            if (_run.Florins < price) return;
            _run.Florins -= price;
            _run.CardRankOverrides[cardId] = newRank;
            UpdateRunHud();
            UpdateShopFlorins();
            PopulateShopItems();
        }

        void OnShopBuy(AbilityType type, int price)
        {
            if (_run.Florins < price || _run.PlayerAbilities.Count >= _run.MaxAbilitySlots) return;
            _run.Florins -= price;
            _run.PlayerAbilities.Add(type);
            _run.RecordAbilityPicked(type);
            UpdateRunHud();
            UpdateShopFlorins();
            PopulateShopItems();
        }

        void OnShopRemoveBurden(BurdenType burden, int price)
        {
            if (_run.Florins < price) return;
            _run.Florins -= price;
            _run.PlayerBurdens.Remove(burden);
            UpdateRunHud();
            UpdateShopFlorins();
            PopulateShopItems();
        }

        // ---------- Event / Rumor / Rest ----------

        void HandleRumor()
        {
            SetPhase(RunPhase.Event);
            int eventId = _rng.Next(8);
            switch (eventId)
            {
                case 0: RumorDrunkMerchant(); break;
                case 1: RumorForgersOffer(); break;
                case 2: RumorCardinalsConfession(); break;
                case 3: RumorPickpocket(); break;
                case 4: RumorAlchemistsBargain(); break;
                case 5: RumorDuelistsGhost(); break;
                case 6: RumorTheFence(); break;
                default: RumorWhisperingWalls(); break;
            }
        }

        void RumorDrunkMerchant()
        {
            int cost = 8 + _run.CurrentAct * 3;
            bool canAfford = _run.Florins >= cost;
            bool hasRoom = _run.PlayerAbilities.Count < _run.MaxAbilitySlots;
            var ability = hasRoom ? PickAbilityReward(true) : null;
            string abilityName = ability?.DisplayName() ?? "a rare technique";

            ShowEventChoices("The Drunk Merchant",
                $"A bleary-eyed merchant waves you over.\n\"I know secrets worth more than gold. {abilityName}... for {cost} Florins.\"",
                canAfford && ability.HasValue ? $"Pay {cost} Florins" : $"Can't afford ({cost}F)",
                "Walk away (+2 Florins)",
                ChoiceCost, ChoiceSafe);

            if (canAfford && ability.HasValue)
            {
                var captured = ability.Value;
                _eventChoice1Action = () =>
                {
                    _run.Florins -= cost;
                    _run.PlayerAbilities.Add(captured);
                    _run.RecordAbilityPicked(captured);
                    ShowEventOutcome($"Learned {captured.DisplayName()}! (-{cost} Florins)");
                };
            }
            else
                _eventChoice1Action = () => ShowEventOutcome("You can't make the deal.");

            _eventChoice2Action = () =>
            {
                _run.Florins += 2;
                ShowEventOutcome("You leave the drunk to his cups. +2 Florins.");
            };
        }

        void RumorForgersOffer()
        {
            var upgradeable = AbilityUpgrades.GetUpgradeableAbilities(_run.PlayerAbilities);
            if (upgradeable.Count == 0)
            {
                RumorDrunkMerchant();
                return;
            }
            var pick = upgradeable[_rng.Next(upgradeable.Count)];
            AbilityUpgrades.TryGetUpgrade(pick, out var upgraded);

            ShowEventChoices("The Forger's Offer",
                $"A scarred forger leans close.\n\"I can refine your {pick.DisplayName()} into {upgraded.DisplayName()}. But my methods leave a mark.\"",
                $"Accept (upgrade + burden)", "Decline",
                ChoiceRisky, ChoiceSafe);

            _eventChoice1Action = () =>
            {
                int idx = _run.PlayerAbilities.IndexOf(pick);
                if (idx >= 0) _run.PlayerAbilities[idx] = upgraded;
                UpgradeDeckAbility(pick, upgraded);
                AssignRandomBurden();
                var burden = _run.PlayerBurdens[^1];
                ShowEventOutcome($"{pick.DisplayName()} forged into {upgraded.DisplayName()}! Gained {burden.DisplayName()}.");
            };
            _eventChoice2Action = () => ShowEventOutcome("You keep your cards as they are.");
        }

        void RumorCardinalsConfession()
        {
            int upgradeCost = 6 + _run.CurrentAct * 2;
            var candidates = GetUpgradeableCards();
            bool canUpgrade = candidates.Count > 0 && _run.Florins >= upgradeCost;
            int freeGain = 4 + _run.CurrentAct;

            string upgradeDesc;
            string cardId = null;
            string cardName = null;
            Rank newRank = Rank.Six;
            if (candidates.Count > 0)
            {
                var pick = candidates[_rng.Next(candidates.Count)];
                cardId = pick.id;
                cardName = pick.name;
                newRank = pick.rank + 1;
                upgradeDesc = $"\"I can bless your {cardName}\" ({pick.rank.Label()} → {newRank.Label()}) for {upgradeCost} Florins.";
            }
            else
                upgradeDesc = "He has nothing to offer your cards.";

            ShowEventChoices("The Cardinal's Confession",
                $"A cardinal beckons from a shadowed alcove.\n{upgradeDesc}\nOr he can share a blessing of coin.",
                canUpgrade ? $"Bless card (-{upgradeCost}F)" : $"No suitable cards",
                $"Take the blessing (+{freeGain} Florins)",
                ChoiceCost, ChoiceSafe);

            if (canUpgrade)
            {
                string capturedId = cardId;
                string capturedName = cardName;
                Rank capturedRank = newRank;
                _eventChoice1Action = () =>
                {
                    _run.Florins -= upgradeCost;
                    _run.CardRankOverrides[capturedId] = capturedRank;
                    ShowEventOutcome($"{capturedName} blessed to {capturedRank.Label()}! (-{upgradeCost} Florins)");
                };
            }
            else
                _eventChoice1Action = () => ShowEventOutcome("The cardinal shrugs apologetically.");

            _eventChoice2Action = () =>
            {
                _run.Florins += freeGain;
                ShowEventOutcome($"The cardinal presses coins into your palm. +{freeGain} Florins.");
            };
        }

        void RumorPickpocket()
        {
            int stealAmount = 6 + _run.CurrentAct * 2;
            ShowEventChoices("The Pickpocket's Dilemma",
                $"You spot a fat purse dangling from a merchant's belt.\nYou could take {stealAmount} Florins... but thieves get marked.",
                $"Steal ({stealAmount}F + burden)", "Return the wallet (+ability slot)",
                ChoiceRisky, ChoiceSafe);

            _eventChoice1Action = () =>
            {
                _run.Florins += stealAmount;
                AddBurden(BurdenType.BadReputation);
                ShowEventOutcome($"Quick fingers! +{stealAmount} Florins. Gained Bad Reputation.");
            };
            _eventChoice2Action = () =>
            {
                _run.MaxAbilitySlots++;
                ShowEventOutcome($"The merchant is grateful. Your reputation opens doors. +1 ability slot! (Now {_run.MaxAbilitySlots})");
            };
        }

        void RumorAlchemistsBargain()
        {
            if (_run.PlayerDeckCardIds.Count <= 8)
            {
                RumorCardinalsConfession();
                return;
            }
            string weakest = FindWeakestDeckCard();
            var candidates = GetUpgradeableCards();
            if (weakest == null || candidates.Count == 0)
            {
                RumorCardinalsConfession();
                return;
            }
            var upgradeTarget = candidates[_rng.Next(candidates.Count)];
            Rank boosted = (Rank)Math.Min((int)upgradeTarget.rank + 2, (int)Rank.Ace);
            CardCatalog.TryGet(weakest, out var weakDef);

            ShowEventChoices("The Alchemist's Bargain",
                $"An alchemist gestures at your deck.\n\"Sacrifice '{weakDef.Name}' and I will transmute '{upgradeTarget.name}' to {boosted.Label()}.\"",
                $"Transmute (remove + upgrade)", "Decline",
                ChoiceRisky, ChoiceSafe);

            string capturedWeak = weakest;
            _eventChoice1Action = () =>
            {
                _run.PlayerDeckCardIds.Remove(capturedWeak);
                _run.CardRankOverrides.Remove(capturedWeak);
                _run.CardAbilityOverrides.Remove(capturedWeak);
                _run.CardRankOverrides[upgradeTarget.id] = boosted;
                ShowEventOutcome($"{weakDef.Name} dissolved. {upgradeTarget.name} transmuted to {boosted.Label()}!");
            };
            _eventChoice2Action = () =>
            {
                _run.Florins += 2;
                ShowEventOutcome("You keep your cards intact. +2 Florins.");
            };
        }

        void RumorDuelistsGhost()
        {
            int prestigeCost = 2;
            bool canPay = _run.Prestige > prestigeCost;
            ShowEventChoices("The Duelist's Ghost",
                $"A spectral duelist materializes before you.\n\"Prove your worth. Sacrifice {prestigeCost} Prestige and I will expand your mind.\"",
                canPay ? $"Accept (-{prestigeCost} Prestige, +1 slot)" : "Not enough Prestige",
                "Refuse (+4 Florins)",
                ChoiceCost, ChoiceSafe);

            if (canPay)
            {
                _eventChoice1Action = () =>
                {
                    _run.Prestige -= prestigeCost;
                    _run.MaxAbilitySlots++;
                    ShowEventOutcome($"Your mind expands! +1 ability slot (now {_run.MaxAbilitySlots}). -{prestigeCost} Prestige.");
                };
            }
            else
                _eventChoice1Action = () => ShowEventOutcome("The ghost fades, unimpressed.");

            _eventChoice2Action = () =>
            {
                _run.Florins += 4;
                ShowEventOutcome("The ghost vanishes. You find coins where it stood. +4 Florins.");
            };
        }

        void RumorTheFence()
        {
            bool hasBurdens = _run.PlayerBurdens.Count > 0;
            int removeCost = 10 + _run.CurrentAct * 2;
            bool canAfford = _run.Florins >= removeCost;

            if (hasBurdens && canAfford)
            {
                var burden = _run.PlayerBurdens[_rng.Next(_run.PlayerBurdens.Count)];
                ShowEventChoices("The Fence",
                    $"A shadowy fence offers a deal.\n\"I can make your '{burden.DisplayName()}' disappear... for {removeCost} Florins.\"",
                    $"Pay {removeCost}F (remove burden)", "Decline (+3 Florins)",
                    ChoiceCost, ChoiceSafe);

                _eventChoice1Action = () =>
                {
                    _run.Florins -= removeCost;
                    _run.PlayerBurdens.Remove(burden);
                    ShowEventOutcome($"{burden.DisplayName()} lifted! (-{removeCost} Florins)");
                };
                _eventChoice2Action = () =>
                {
                    _run.Florins += 3;
                    ShowEventOutcome("You bear your burden a while longer. +3 Florins.");
                };
            }
            else
            {
                int sellAmount = 6 + _run.CurrentAct;
                ShowEventChoices("The Fence",
                    "A shadowy fence eyes your belongings.\n\"Nothing to fix, but I can offer you coin for information.\"",
                    $"Share intel (+{sellAmount} Florins)", "Move on (+2 Florins)",
                    ChoiceIntel, ChoiceSafe);

                _eventChoice1Action = () =>
                {
                    _run.Florins += sellAmount;
                    ShowEventOutcome($"The fence pays well for your knowledge. +{sellAmount} Florins.");
                };
                _eventChoice2Action = () =>
                {
                    _run.Florins += 2;
                    ShowEventOutcome("You keep your secrets. +2 Florins.");
                };
            }
        }

        void RumorWhisperingWalls()
        {
            if (_run.PlayerDeckCardIds.Count > 8)
            {
                int cost = 5 + _run.CurrentAct;
                bool canAfford = _run.Florins >= cost;
                string weakest = FindWeakestDeckCard();
                string cardName = "a weak card";
                if (weakest != null && CardCatalog.TryGet(weakest, out var def))
                    cardName = def.Name;

                ShowEventChoices("The Whispering Walls",
                    $"Ancient walls whisper secrets of refinement.\n\"Shed the unnecessary. Let '{cardName}' go for {cost} Florins.\"",
                    canAfford ? $"Thin deck (-{cost}F, remove card)" : $"Can't afford ({cost}F)",
                    "Listen quietly (+3 Florins)",
                    ChoiceCost, ChoiceSafe);

                if (canAfford && weakest != null)
                {
                    string capturedId = weakest;
                    _eventChoice1Action = () =>
                    {
                        _run.Florins -= cost;
                        _run.PlayerDeckCardIds.Remove(capturedId);
                        _run.CardRankOverrides.Remove(capturedId);
                        _run.CardAbilityOverrides.Remove(capturedId);
                        ShowEventOutcome($"{cardName} forgotten. Your deck feels focused. (-{cost} Florins)");
                    };
                }
                else
                    _eventChoice1Action = () => ShowEventOutcome("The walls fall silent.");
            }
            else
            {
                int gain = 5 + _run.CurrentAct;
                ShowEventChoices("The Whispering Walls",
                    "Ancient walls murmur faintly.\nYour deck is already lean. They offer what they can.",
                    $"Listen (+{gain} Florins)", null,
                    ChoiceSafe);

                _eventChoice1Action = () =>
                {
                    _run.Florins += gain;
                    ShowEventOutcome($"The whispers reveal hidden treasure. +{gain} Florins.");
                };
                _eventChoice2Action = null;
            }

            if (_eventChoice2Action == null)
            {
                _eventChoice2Action = () =>
                {
                    _run.Florins += 3;
                    ShowEventOutcome("You rest against the old stones. +3 Florins.");
                };
            }
        }

        void HandleRest()
        {
            SetPhase(RunPhase.Rest);

            bool hasBurdens = _run.PlayerBurdens.Count > 0;
            int restType = _rng.Next(5);

            if (restType == 0 && hasBurdens)
            {
                ShowEventChoices("The Hearth",
                    "The hearth crackles. You find a quiet moment of peace.",
                    "Mend (remove a burden)", "Rest quietly (+3 Florins)",
                    ChoiceSafe, ChoiceSafe);
                _eventChoice1Action = DoRestMend;
                _eventChoice2Action = DoRestQuietly;
            }
            else if (restType == 1)
            {
                var upgradeable = AbilityUpgrades.GetUpgradeableAbilities(_run.PlayerAbilities);
                bool canUpgrade = upgradeable.Count > 0;
                bool canLearn = _run.PlayerAbilities.Count < _run.MaxAbilitySlots;

                string studyLabel = canUpgrade ? "Study (upgrade an ability)" : "Study (+ability if room)";
                ShowEventChoices("The Study",
                    canUpgrade
                        ? "You find a quiet corner with old game manuals.\nYour experience lets you refine what you already know."
                        : "You find a quiet corner with old game manuals.\nPerhaps you can learn something new.",
                    studyLabel, "Rest quietly (+3 Florins)",
                    ChoiceSafe, ChoiceSafe);
                _eventChoice1Action = () => DoRestStudy(upgradeable, canLearn);
                _eventChoice2Action = DoRestQuietly;
            }
            else if (restType == 2)
            {
                var candidates = GetUpgradeableCards();
                if (candidates.Count > 0)
                {
                    var pick = candidates[_rng.Next(candidates.Count)];
                    Rank newRank = pick.rank + 1;
                    ShowEventChoices("The Smithy",
                        $"A traveling smith offers to hone one of your cards.\n\"{pick.name}\" can be sharpened from {pick.rank.Label()} to {newRank.Label()}.",
                        $"Hone ({pick.rank.Label()} → {newRank.Label()})", "Rest quietly (+3 Florins)",
                        ChoiceSafe, ChoiceSafe);
                    _eventChoice1Action = () =>
                    {
                        _run.CardRankOverrides[pick.id] = newRank;
                        ShowEventOutcome($"{pick.name} honed to {newRank.Label()}!");
                    };
                    _eventChoice2Action = DoRestQuietly;
                }
                else
                {
                    ShowEventChoices("The Hearth",
                        "A warm fire and a good meal. Simple comforts.",
                        "Rest quietly (+3 Florins)", null,
                        ChoiceSafe);
                    _eventChoice1Action = DoRestQuietly;
                    _eventChoice2Action = null;
                }
            }
            else if (restType == 3 && _run.PlayerDeckCardIds.Count > 8)
            {
                string weakest = FindWeakestDeckCard();
                if (weakest != null && CardCatalog.TryGet(weakest, out var wDef))
                {
                    ShowEventChoices("The Bonfire",
                        $"Flames lick the night air. You could burn away the dead weight.\n\"{wDef.Name}\" ({wDef.Rank.Label()}{wDef.Suit.Glyph()}) looks expendable.",
                        $"Burn it (remove from deck)", "Rest quietly (+3 Florins)",
                        ChoiceRisky, ChoiceSafe);
                    string capturedId = weakest;
                    _eventChoice1Action = () =>
                    {
                        _run.PlayerDeckCardIds.Remove(capturedId);
                        _run.CardRankOverrides.Remove(capturedId);
                        _run.CardAbilityOverrides.Remove(capturedId);
                        ShowEventOutcome($"{wDef.Name} crumbles to ash. Your deck feels lighter.");
                    };
                    _eventChoice2Action = DoRestQuietly;
                }
                else
                {
                    ShowEventChoices("The Hearth",
                        "A warm fire and a good meal. Simple comforts.",
                        "Rest quietly (+3 Florins)", null,
                        ChoiceSafe);
                    _eventChoice1Action = DoRestQuietly;
                    _eventChoice2Action = null;
                }
            }
            else
            {
                string desc = hasBurdens
                    ? "The hearth crackles. You find a quiet moment of peace."
                    : "A warm fire and a good meal. Simple comforts.";
                ShowEventChoices("The Hearth", desc,
                    hasBurdens ? "Mend (remove a burden)" : "Rest quietly (+3 Florins)", null,
                    ChoiceSafe);
                _eventChoice1Action = hasBurdens ? DoRestMend : DoRestQuietly;
                _eventChoice2Action = null;
            }
        }

        Action _eventChoice1Action;
        Action _eventChoice2Action;

        static readonly Color ChoiceSafe  = new(0.35f, 0.65f, 0.35f);
        static readonly Color ChoiceRisky = new(0.72f, 0.28f, 0.28f);
        static readonly Color ChoiceCost  = new(0.78f, 0.62f, 0.25f);
        static readonly Color ChoiceIntel = new(0.35f, 0.55f, 0.75f);

        void ShowEventChoices(string title, string desc, string choice1, string choice2,
            Color? accent1 = null, Color? accent2 = null)
        {
            if (EventTitleLabel) EventTitleLabel.text = title;
            if (EventDescLabel) EventDescLabel.text = desc;
            if (EventOutcomeLabel) { EventOutcomeLabel.text = ""; EventOutcomeLabel.gameObject.SetActive(false); }

            if (EventChoice1Button)
            {
                EventChoice1Button.gameObject.SetActive(true);
                EventChoice1Button.onClick.RemoveAllListeners();
                EventChoice1Button.onClick.AddListener(OnEventChoice1);
                if (EventChoice1Label) EventChoice1Label.text = choice1 ?? "";
                SetChoiceAccent(EventChoice1Button, accent1 ?? ChoiceCost);
            }

            if (EventChoice2Button)
            {
                bool show2 = choice2 != null;
                EventChoice2Button.gameObject.SetActive(show2);
                if (show2)
                {
                    EventChoice2Button.onClick.RemoveAllListeners();
                    EventChoice2Button.onClick.AddListener(OnEventChoice2);
                    if (EventChoice2Label) EventChoice2Label.text = choice2;
                    SetChoiceAccent(EventChoice2Button, accent2 ?? ChoiceSafe);
                }
            }

            if (EventContinueButton) EventContinueButton.gameObject.SetActive(false);
        }

        void SetChoiceAccent(Button btn, Color color)
        {
            var rt = (RectTransform)btn.transform;
            var existing = rt.Find("ChoiceAccent");
            if (existing) Destroy(existing.gameObject);

            var accent = new GameObject("ChoiceAccent", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(rt, false);
            accent.transform.SetAsFirstSibling();
            var aRT = (RectTransform)accent.transform;
            aRT.anchorMin = Vector2.zero;
            aRT.anchorMax = new Vector2(0, 1);
            aRT.pivot = new Vector2(0, 0.5f);
            aRT.sizeDelta = new Vector2(5, 0);
            aRT.anchoredPosition = Vector2.zero;
            accent.GetComponent<Image>().color = color;
            accent.GetComponent<Image>().raycastTarget = false;
        }

        void ShowEventOutcome(string outcome)
        {
            UIScreenCapture.Instance?.NotifyModal(UIScreen.EventOutcome);
            if (EventOutcomeLabel)
            {
                EventOutcomeLabel.text = outcome;
                EventOutcomeLabel.gameObject.SetActive(true);
            }
            if (EventChoice1Button) EventChoice1Button.gameObject.SetActive(false);
            if (EventChoice2Button) EventChoice2Button.gameObject.SetActive(false);
            if (EventContinueButton)
            {
                EventContinueButton.gameObject.SetActive(true);
                EventContinueButton.onClick.RemoveAllListeners();
                EventContinueButton.onClick.AddListener(OnResultContinue);
            }
            UpdateRunHud();
        }

        void OnEventChoice1() => _eventChoice1Action?.Invoke();
        void OnEventChoice2() => _eventChoice2Action?.Invoke();

        void AddBurden(BurdenType burden)
        {
            if (!_run.PlayerBurdens.Contains(burden))
                _run.PlayerBurdens.Add(burden);
        }

        void AssignRandomBurden()
        {
            var allBurdens = (BurdenType[])Enum.GetValues(typeof(BurdenType));
            var available = new List<BurdenType>();
            foreach (var b in allBurdens)
                if (!_run.PlayerBurdens.Contains(b)) available.Add(b);
            if (available.Count > 0)
                _run.PlayerBurdens.Add(available[_rng.Next(available.Count)]);
        }

        List<(string id, string name, Rank rank)> GetUpgradeableCards()
        {
            var result = new List<(string, string, Rank)>();
            foreach (var cardId in _run.PlayerDeckCardIds)
            {
                var def = CardCatalog.Get(cardId);
                Rank effective = _run.CardRankOverrides.TryGetValue(cardId, out var ov) ? ov : def.Rank;
                if (effective < Rank.Ace)
                    result.Add((cardId, def.Name, effective));
            }
            return result;
        }

        void DoRestStudy(List<AbilityType> upgradeable, bool canLearn)
        {
            if (upgradeable.Count > 0)
            {
                var pick = upgradeable[_rng.Next(upgradeable.Count)];
                if (AbilityUpgrades.TryGetUpgrade(pick, out var upgraded))
                {
                    int idx = _run.PlayerAbilities.IndexOf(pick);
                    if (idx >= 0) _run.PlayerAbilities[idx] = upgraded;
                    UpgradeDeckAbility(pick, upgraded);
                    ShowEventOutcome($"{pick.DisplayName()} upgraded to {upgraded.DisplayName()}!");
                    return;
                }
            }
            if (canLearn)
            {
                var reward = PickAbilityReward(false);
                if (reward.HasValue)
                {
                    _run.PlayerAbilities.Add(reward.Value);
                    _run.RecordAbilityPicked(reward.Value);
                    ShowEventOutcome($"Studied and learned {reward.Value.DisplayName()}!");
                    return;
                }
            }
            _run.Florins += 3;
            ShowEventOutcome("Nothing new in the books. +3 Florins from a tip jar.");
        }

        void UpgradeDeckAbility(AbilityType from, AbilityType to)
        {
            foreach (var cardId in _run.PlayerDeckCardIds)
            {
                var def = CardCatalog.Get(cardId);
                AbilityType? current = _run.CardAbilityOverrides.TryGetValue(cardId, out var ov) ? ov : def.Ability;
                if (current == from)
                {
                    _run.CardAbilityOverrides[cardId] = to;
                    break;
                }
            }
        }

        void DoRestMend()
        {
            if (_run.PlayerBurdens.Count > 0)
            {
                var removed = _run.PlayerBurdens[_rng.Next(_run.PlayerBurdens.Count)];
                _run.PlayerBurdens.Remove(removed);
                ShowEventOutcome($"{removed.DisplayName()} fades away. You feel lighter.");
            }
            else
                ShowEventOutcome("You rest, but nothing changes.");
        }

        void DoRestQuietly()
        {
            int gain = Ascension.SpartanRest(_run.AscensionLevel) ? 2 : 3;
            _run.Florins += gain;
            ShowEventOutcome($"A peaceful rest. +{gain} Florins.");
        }

        // ---------- Act progression ----------

        void AdvanceAct()
        {
            _run.CurrentAct++;
            if (_run.CurrentAct >= 5)
            {
                _run.RunComplete = true;
                _run.RunWon = true;
                SetPhase(RunPhase.RunOver);
                return;
            }
            _run.MaxAbilitySlots++;
            _run.CurrentMap = MapGenerator.Generate(_run.CurrentAct, _rng, Ascension.ShorterRoads(_run.AscensionLevel) ? 1 : 0);
            _currentColumn = 0;
            _visitedNodeRows.Clear();
            _selectedNodeRow = -1;
            ApplyActTheme(_run.CurrentAct);
            SetPhase(RunPhase.MapSelect);
        }

        void ApplyActTheme(int act)
        {
            act = Mathf.Clamp(act, 0, 4);
            if (TableBackgroundImage && TableSurfaceSprites != null && act < TableSurfaceSprites.Length)
            {
                TableBackgroundImage.sprite = TableSurfaceSprites[act];
                TableBackgroundImage.color = Color.white;
            }
            if (TableFeltImage)
                TableFeltImage.color = ThemePalette.ActFeltTint[act];
            if (TableFrameImage)
            {
                var fc = ThemePalette.ActFrameColor[act];
                fc.a = 0.6f;
                var parent = TableFrameImage.transform.parent;
                for (int i = 0; i < parent.childCount; i++)
                {
                    var edge = parent.GetChild(i).GetComponent<Image>();
                    if (edge) edge.color = fc;
                }
            }
            if (VignetteImage && VignetteSprite)
            {
                VignetteImage.sprite = VignetteSprite;
                VignetteImage.color = Color.white;
            }
            if (MapVenueBgImage && VenueBackgroundSprites != null && act < VenueBackgroundSprites.Length)
            {
                MapVenueBgImage.sprite = VenueBackgroundSprites[act];
                MapVenueBgImage.color = new Color(1, 1, 1, 0.2f);
            }
            if (ResultVenueBgImage && VenueBackgroundSprites != null && act < VenueBackgroundSprites.Length)
            {
                ResultVenueBgImage.sprite = VenueBackgroundSprites[act];
                ResultVenueBgImage.color = new Color(1, 1, 1, 0.2f);
            }
            if (RunOverVenueBgImage && VenueBackgroundSprites != null && act < VenueBackgroundSprites.Length)
                RunOverVenueBgImage.sprite = VenueBackgroundSprites[act];
            if (Camera.main)
                Camera.main.backgroundColor = ThemePalette.ActBackgroundTint[act];
        }

        // ---------- Run Over ----------

        void ShowRunOver()
        {
            int repEarned = 0;
            ReputationData repData;
            if (!_autoRun)
            {
                repEarned = ReputationSystem.RecordRunEnd(_run, _selectedArchetype);
                repData = ReputationSystem.Load();
            }
            else
                repData = ReputationSystem.Load();

            if (RunOverTitleLabel)
                RunOverTitleLabel.text = _run.RunWon ? "The Circuit is Yours!" : "Your Reputation Crumbles...";
            if (RunOverStatsLabel)
            {
                string archName = _selectedArchetype.HasValue ? _selectedArchetype.Value.DisplayName() : "Unknown";
                string docLine = _run.PlayerDoctrine.HasValue
                    ? $"<color=#D4A846>{_run.PlayerDoctrine.Value.DisplayName()}</color> ({archName})"
                    : $"<color=#D4A846>{archName}</color>";
                string deckLine = $"Deck  <color=#99CCEE>{_run.PlayerDeckCardIds.Count} cards</color>    Relics  <color=#99CCEE>{_run.PlayerTrinkets.Count}</color>";
                string stats = $"{docLine}\n\n" +
                    $"Acts completed  <color=#99CCEE>{_run.CurrentAct}/5</color>\n" +
                    $"Matches won  <color=#99CCEE>{_run.MatchesWon}/{_run.MatchesPlayed}</color>\n" +
                    $"Florins earned  <color=#D4A846>{_run.Florins}</color>\n" +
                    $"{deckLine}\n\n" +
                    $"<color=#66B866>+{repEarned} Reputation</color>  (Total: {repData.TotalReputation})";
                RunOverStatsLabel.text = stats;
                RunOverStatsLabel.richText = true;
            }
            PopulateRunOverJourney();
            PopulateRunOverAbilityChips();

            if (RunOverRestartButton)
            {
                RunOverRestartButton.onClick.RemoveAllListeners();
                RunOverRestartButton.onClick.AddListener(StartNewRun);
            }
        }

        void PopulateRunOverJourney()
        {
            if (!RunOverJourneyContainer) return;
            for (int i = RunOverJourneyContainer.childCount - 1; i >= 0; i--)
                Destroy(RunOverJourneyContainer.GetChild(i).gameObject);

            int acts = Mathf.Max(_run.CurrentAct, 1);
            for (int a = 0; a < acts; a++)
            {
                var go = new GameObject($"Act{a}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(RunOverJourneyContainer, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(120, 80);
                var img = go.GetComponent<Image>();
                if (VenueBackgroundSprites != null && a < VenueBackgroundSprites.Length)
                    img.sprite = VenueBackgroundSprites[a];
                img.color = a < _run.CurrentAct ? Color.white : new Color(1, 1, 1, 0.4f);
                img.preserveAspect = true;

                var labelGO = new GameObject("Label", typeof(RectTransform));
                labelGO.transform.SetParent(go.transform, false);
                var lrt = labelGO.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
                var lbl = labelGO.AddComponent<TextMeshProUGUI>();
                lbl.text = $"Act {a + 1}";
                lbl.fontSize = 14;
                lbl.alignment = TextAlignmentOptions.Bottom;
                lbl.color = Color.white;
                lbl.enableAutoSizing = false;
            }
        }

        void PopulateRunOverAbilityChips()
        {
            if (!RunOverAbilityChipsContainer) return;
            for (int i = RunOverAbilityChipsContainer.childCount - 1; i >= 0; i--)
                Destroy(RunOverAbilityChipsContainer.GetChild(i).gameObject);

            if (_run.PlayerAbilities.Count == 0) return;

            foreach (var ability in _run.PlayerAbilities)
            {
                var def = AbilityPool.Get(ability);
                var color = ThemePalette.AbilityBadgeColor(ability);

                var pill = new GameObject($"Chip_{ability}", typeof(RectTransform), typeof(Image));
                pill.transform.SetParent(RunOverAbilityChipsContainer, false);
                var rt = pill.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(130, 32);
                var bg = pill.GetComponent<Image>();
                bg.color = new Color(color.r, color.g, color.b, 0.25f);

                var textGO = new GameObject("Text", typeof(RectTransform));
                textGO.transform.SetParent(pill.transform, false);
                var trt = textGO.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = new Vector2(6, 0);
                trt.offsetMax = new Vector2(-6, 0);
                var txt = textGO.AddComponent<TextMeshProUGUI>();
                txt.text = ability.DisplayName();
                txt.fontSize = 16;
                txt.alignment = TextAlignmentOptions.Center;
                txt.color = color;
                txt.enableAutoSizing = false;
                txt.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        // ---------- Helpers ----------

        AbilityType? PickAbilityReward(bool eliteWeighted)
        {
            var pool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (_run.PlayerAbilities.Contains(def.Type)) continue;
                if (AbilityUpgrades.TryGetUpgrade(def.Type, out var upgraded)
                    && _run.PlayerAbilities.Contains(upgraded)) continue;
                if (eliteWeighted || def.Rarity != AbilityRarity.Rare)
                    pool.Add(def);
            }
            if (pool.Count == 0) return null;
            return pool[_rng.Next(pool.Count)].Type;
        }

        int CalculateFlorins(int act, MapNodeType nodeType)
        {
            int baseAmount = 10 + act * 2;
            if (nodeType == MapNodeType.EliteMatch) baseAmount += 5;
            if (nodeType == MapNodeType.BossMatch) baseAmount += 10;
            if (Ascension.LeanPurse(_run.AscensionLevel))
                baseAmount = Math.Max(1, baseAmount - 3);
            return baseAmount;
        }

        // ---------- Card Detail Modal ----------

        void ShowCardDetail(CardDefinition def)
        {
            if (!CardDetailPanel) BuildCardDetailPanel();
            if (!CardDetailPanel) return;

            CardDetailPanel.SetActive(true);
            UIScreenCapture.Instance?.NotifyModal(UIScreen.CardDetail);

            for (int i = CardDetailPanel.transform.childCount - 1; i >= 0; i--)
                Destroy(CardDetailPanel.transform.GetChild(i).gameObject);

            var container = new GameObject("DetailContent", typeof(RectTransform), typeof(Image));
            container.transform.SetParent(CardDetailPanel.transform, false);
            var containerRT = (RectTransform)container.transform;
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.sizeDelta = new Vector2(500, 280);
            container.GetComponent<Image>().color = ThemePalette.DeepNavy;

            var borderGO = container.AddComponent<Outline>();
            if (borderGO) { borderGO.effectColor = new Color(0.35f, 0.28f, 0.19f); borderGO.effectDistance = new Vector2(1, -1); }

            // Card visual (left side)
            var cardGO = CreateMiniCard(def, container.transform, 130, 186);
            var cardRT = (RectTransform)cardGO.transform;
            cardRT.anchorMin = new Vector2(0, 0);
            cardRT.anchorMax = new Vector2(0, 1);
            cardRT.pivot = new Vector2(0, 0.5f);
            cardRT.anchoredPosition = new Vector2(16, 0);
            cardRT.sizeDelta = new Vector2(130, -32);

            // Info panel (right side)
            float infoLeft = 160;
            float infoRight = -16;

            // Card name
            var nameGO = new GameObject("DetailName", typeof(RectTransform));
            nameGO.transform.SetParent(container.transform, false);
            var nameRT = (RectTransform)nameGO.transform;
            nameRT.anchorMin = new Vector2(0, 1);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.pivot = new Vector2(0, 1);
            nameRT.anchoredPosition = new Vector2(infoLeft, -16);
            nameRT.sizeDelta = new Vector2(infoRight - infoLeft, 30);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Heading) nameTMP.font = FontAssets.Heading;
            nameTMP.text = def.Name;
            nameTMP.fontSize = 24;
            nameTMP.color = ThemePalette.Parchment;
            nameTMP.alignment = TextAlignmentOptions.TopLeft;
            nameTMP.raycastTarget = false;

            // Meta tags (doctrine, rarity, suit+rank)
            var metaGO = new GameObject("DetailMeta", typeof(RectTransform));
            metaGO.transform.SetParent(container.transform, false);
            var metaRT = (RectTransform)metaGO.transform;
            metaRT.anchorMin = new Vector2(0, 1);
            metaRT.anchorMax = new Vector2(1, 1);
            metaRT.pivot = new Vector2(0, 1);
            metaRT.anchoredPosition = new Vector2(infoLeft, -50);
            metaRT.sizeDelta = new Vector2(infoRight - infoLeft, 22);
            var metaTMP = metaGO.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Mono) metaTMP.font = FontAssets.Mono;
            string docColor = DoctrineHexColor(def.Doctrine);
            metaTMP.text = $"<color={docColor}>{def.Doctrine}</color>  {def.Rarity}  {def.Rank.Label()}{def.Suit.Glyph()}";
            metaTMP.fontSize = 14;
            metaTMP.color = ThemePalette.DustyTan;
            metaTMP.richText = true;
            metaTMP.alignment = TextAlignmentOptions.TopLeft;
            metaTMP.raycastTarget = false;

            // Ability section
            if (def.HasAbility)
            {
                var abilityBg = new GameObject("AbilityBg", typeof(RectTransform), typeof(Image));
                abilityBg.transform.SetParent(container.transform, false);
                var abRT = (RectTransform)abilityBg.transform;
                abRT.anchorMin = new Vector2(0, 1);
                abRT.anchorMax = new Vector2(1, 1);
                abRT.pivot = new Vector2(0, 1);
                abRT.anchoredPosition = new Vector2(infoLeft, -80);
                abRT.sizeDelta = new Vector2(infoRight - infoLeft, 90);
                abilityBg.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);

                var abilityColor = ThemePalette.AbilityBadgeColor(def.Ability.Value);
                var accentBar = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
                accentBar.transform.SetParent(abilityBg.transform, false);
                var abcRT = (RectTransform)accentBar.transform;
                abcRT.anchorMin = Vector2.zero;
                abcRT.anchorMax = new Vector2(0, 1);
                abcRT.pivot = new Vector2(0, 0.5f);
                abcRT.sizeDelta = new Vector2(3, 0);
                accentBar.GetComponent<Image>().color = abilityColor;
                accentBar.GetComponent<Image>().raycastTarget = false;

                var abNameGO = new GameObject("AbName", typeof(RectTransform));
                abNameGO.transform.SetParent(abilityBg.transform, false);
                var abNameRT = (RectTransform)abNameGO.transform;
                abNameRT.anchorMin = new Vector2(0, 1);
                abNameRT.anchorMax = new Vector2(1, 1);
                abNameRT.pivot = new Vector2(0, 1);
                abNameRT.anchoredPosition = new Vector2(10, -6);
                abNameRT.sizeDelta = new Vector2(-16, 20);
                var abNameTMP = abNameGO.AddComponent<TextMeshProUGUI>();
                if (FontAssets.Heading) abNameTMP.font = FontAssets.Heading;
                string triggerTag = def.Trigger != TriggerTiming.Passive ? $"  <size=11><color=#888>{def.Trigger}</color></size>" : "  <size=11><color=#888>Passive</color></size>";
                abNameTMP.text = $"{def.Ability.Value.DisplayName()}{triggerTag}";
                abNameTMP.fontSize = 16;
                abNameTMP.color = ThemePalette.Parchment;
                abNameTMP.richText = true;
                abNameTMP.alignment = TextAlignmentOptions.MidlineLeft;
                abNameTMP.raycastTarget = false;

                var abDescGO = new GameObject("AbDesc", typeof(RectTransform));
                abDescGO.transform.SetParent(abilityBg.transform, false);
                var abDescRT = (RectTransform)abDescGO.transform;
                abDescRT.anchorMin = Vector2.zero;
                abDescRT.anchorMax = new Vector2(1, 0.65f);
                abDescRT.offsetMin = new Vector2(10, 4);
                abDescRT.offsetMax = new Vector2(-6, 0);
                var abDescTMP = abDescGO.AddComponent<TextMeshProUGUI>();
                abDescTMP.text = def.AbilityText ?? def.Ability.Value.Description();
                abDescTMP.fontSize = 14;
                abDescTMP.color = ThemePalette.DescGray;
                abDescTMP.enableWordWrapping = true;
                abDescTMP.alignment = TextAlignmentOptions.TopLeft;
                abDescTMP.raycastTarget = false;
            }

            // Flavor text
            if (!string.IsNullOrEmpty(def.FlavorText))
            {
                var flavorGO = new GameObject("Flavor", typeof(RectTransform));
                flavorGO.transform.SetParent(container.transform, false);
                var flavorRT = (RectTransform)flavorGO.transform;
                flavorRT.anchorMin = new Vector2(0, 0);
                flavorRT.anchorMax = new Vector2(1, 0);
                flavorRT.pivot = new Vector2(0, 0);
                flavorRT.anchoredPosition = new Vector2(infoLeft, 12);
                flavorRT.sizeDelta = new Vector2(infoRight - infoLeft, 40);
                var flavorTMP = flavorGO.AddComponent<TextMeshProUGUI>();
                flavorTMP.text = $"<i>“{def.FlavorText}”</i>";
                flavorTMP.fontSize = 13;
                flavorTMP.color = ThemePalette.DustyTan;
                flavorTMP.richText = true;
                flavorTMP.enableWordWrapping = true;
                flavorTMP.alignment = TextAlignmentOptions.BottomLeft;
                flavorTMP.raycastTarget = false;
            }

            // Close button
            var closeGO = new GameObject("CloseBtn", typeof(RectTransform), typeof(Button));
            closeGO.transform.SetParent(container.transform, false);
            var closeRT = (RectTransform)closeGO.transform;
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-6, -4);
            closeRT.sizeDelta = new Vector2(30, 30);
            var closeTMP = closeGO.AddComponent<TextMeshProUGUI>();
            closeTMP.text = "×";
            closeTMP.fontSize = 22;
            closeTMP.color = ThemePalette.DustyTan;
            closeTMP.alignment = TextAlignmentOptions.Center;
            closeGO.GetComponent<Button>().onClick.AddListener(HideCardDetail);
        }

        void HideCardDetail()
        {
            if (CardDetailPanel) CardDetailPanel.SetActive(false);
        }

        void BuildCardDetailPanel()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas) canvas = FindFirstObjectByType<Canvas>();
            if (!canvas) return;

            var panelGO = new GameObject("CardDetailPanel", typeof(RectTransform), typeof(Image), typeof(Button));
            panelGO.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)panelGO.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            panelGO.GetComponent<Image>().color = ThemePalette.ModalOverlay;
            panelGO.GetComponent<Button>().onClick.AddListener(HideCardDetail);
            CardDetailPanel = panelGO;
            panelGO.SetActive(false);
        }

        static string DoctrineHexColor(DoctrineType d) => d switch
        {
            DoctrineType.Schemer => "#1A8A7A",
            DoctrineType.Brute => "#A83232",
            DoctrineType.Trickster => "#7C3AED",
            DoctrineType.Hoarder => "#B45309",
            _ => "#6B7280"
        };

        GameObject CreateMiniCard(CardDefinition def, Transform parent, float width, float height)
        {
            bool isSmall = width <= 100;
            bool isLarge = width > 150;
            float topBandH = isSmall ? 20 : isLarge ? 30 : 24;
            float bottomBandH = isSmall ? 16 : isLarge ? 24 : 20;
            float borderW = def.Rarity == CardRarity.Rare ? 3 : 2;

            var cardGO = new GameObject("Card_" + def.Id, typeof(RectTransform), typeof(Image));
            cardGO.transform.SetParent(parent, false);
            var rt = (RectTransform)cardGO.transform;
            rt.sizeDelta = new Vector2(width, height);

            // Dark card frame background
            cardGO.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.03f);

            var rarityBorder = def.Rarity switch
            {
                CardRarity.Common => ThemePalette.RarityCommon,
                CardRarity.Uncommon => ThemePalette.RarityUncommon,
                CardRarity.Rare => ThemePalette.RarityRare,
                _ => ThemePalette.DustyTan
            };
            var outline = cardGO.AddComponent<Outline>();
            outline.effectColor = rarityBorder;
            outline.effectDistance = new Vector2(borderW, -borderW);

            // Inner gold filigree border
            var filigree = cardGO.AddComponent<Outline>();
            filigree.effectColor = def.Rarity == CardRarity.Rare
                ? new Color(0.87f, 0.67f, 0.2f, 0.35f)
                : new Color(0.83f, 0.66f, 0.27f, 0.2f);
            filigree.effectDistance = new Vector2(1, -1);

            Rank displayRank = def.Rank;
            if (_run != null && _run.CardRankOverrides.TryGetValue(def.Id, out var overrideRank))
                displayRank = overrideRank;

            var docColor = def.Doctrine switch
            {
                DoctrineType.Schemer => new Color(0.1f, 0.54f, 0.48f),
                DoctrineType.Brute => new Color(0.66f, 0.2f, 0.2f),
                DoctrineType.Trickster => new Color(0.49f, 0.23f, 0.93f),
                DoctrineType.Hoarder => new Color(0.71f, 0.33f, 0.04f),
                _ => new Color(0.42f, 0.45f, 0.5f)
            };

            // === TOP BAND ===
            var topBand = new GameObject("TopBand", typeof(RectTransform), typeof(Image));
            topBand.transform.SetParent(cardGO.transform, false);
            var topRT = (RectTransform)topBand.transform;
            topRT.anchorMin = new Vector2(0, 1);
            topRT.anchorMax = new Vector2(1, 1);
            topRT.pivot = new Vector2(0.5f, 1);
            topRT.anchoredPosition = Vector2.zero;
            topRT.sizeDelta = new Vector2(0, topBandH);
            topBand.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.06f, 0.95f);
            topBand.GetComponent<Image>().raycastTarget = false;

            // Gold separator at bottom of top band
            var topSep = new GameObject("TopSep", typeof(RectTransform), typeof(Image));
            topSep.transform.SetParent(topBand.transform, false);
            var tsRT = (RectTransform)topSep.transform;
            tsRT.anchorMin = new Vector2(0, 0);
            tsRT.anchorMax = new Vector2(1, 0);
            tsRT.pivot = new Vector2(0.5f, 0);
            tsRT.anchoredPosition = Vector2.zero;
            tsRT.sizeDelta = new Vector2(0, 1);
            topSep.GetComponent<Image>().color = new Color(0.83f, 0.66f, 0.27f, 0.25f);
            topSep.GetComponent<Image>().raycastTarget = false;

            // Rank + Suit (top band left)
            var rankGO = new GameObject("RankSuit", typeof(RectTransform));
            rankGO.transform.SetParent(topBand.transform, false);
            var rankRT = (RectTransform)rankGO.transform;
            rankRT.anchorMin = new Vector2(0, 0);
            rankRT.anchorMax = new Vector2(0.3f, 1);
            rankRT.offsetMin = new Vector2(4, 0);
            rankRT.offsetMax = Vector2.zero;
            var rankTMP = rankGO.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Mono) rankTMP.font = FontAssets.Mono;
            rankTMP.text = $"{displayRank.Label()}{def.Suit.Glyph()}";
            rankTMP.fontSize = isSmall ? 10 : isLarge ? 15 : 11;
            rankTMP.color = def.Suit.IsRed() ? ThemePalette.RedSuit : ThemePalette.DustyTan;
            rankTMP.alignment = TextAlignmentOptions.MidlineLeft;
            rankTMP.raycastTarget = false;

            // Card name (top band center)
            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(topBand.transform, false);
            var nameRT = (RectTransform)nameGO.transform;
            nameRT.anchorMin = new Vector2(0.25f, 0);
            nameRT.anchorMax = new Vector2(0.75f, 1);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Heading) nameTMP.font = FontAssets.Heading;
            nameTMP.text = def.Name;
            nameTMP.fontSize = isSmall ? 8 : isLarge ? 13 : 9;
            nameTMP.color = ThemePalette.Gold;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.enableWordWrapping = false;
            nameTMP.overflowMode = TextOverflowModes.Ellipsis;
            nameTMP.raycastTarget = false;

            // Doctrine badge (top band right)
            float badgeW = isSmall ? 22 : isLarge ? 32 : 28;
            float badgeH = isSmall ? 10 : isLarge ? 14 : 12;
            var docGO = new GameObject("Doc", typeof(RectTransform), typeof(Image));
            docGO.transform.SetParent(topBand.transform, false);
            var docRT = (RectTransform)docGO.transform;
            docRT.anchorMin = new Vector2(1, 0.5f);
            docRT.anchorMax = new Vector2(1, 0.5f);
            docRT.pivot = new Vector2(1, 0.5f);
            docRT.anchoredPosition = new Vector2(-3, 0);
            docRT.sizeDelta = new Vector2(badgeW, badgeH);
            docGO.GetComponent<Image>().color = docColor;
            docGO.GetComponent<Image>().raycastTarget = false;
            var docLabel = new GameObject("DocLabel", typeof(RectTransform));
            docLabel.transform.SetParent(docGO.transform, false);
            var dlRT = (RectTransform)docLabel.transform;
            dlRT.anchorMin = Vector2.zero;
            dlRT.anchorMax = Vector2.one;
            dlRT.offsetMin = Vector2.zero;
            dlRT.offsetMax = Vector2.zero;
            var dlTMP = docLabel.AddComponent<TextMeshProUGUI>();
            dlTMP.text = def.Doctrine.ToString().Substring(0, 3).ToUpper();
            dlTMP.fontSize = isSmall ? 6 : isLarge ? 8 : 7;
            dlTMP.color = Color.white;
            dlTMP.alignment = TextAlignmentOptions.Center;
            dlTMP.raycastTarget = false;

            // === ART WINDOW (center, fills between bands) ===
            var artGO = new GameObject("ArtWindow", typeof(RectTransform), typeof(Image));
            artGO.transform.SetParent(cardGO.transform, false);
            var artRT = (RectTransform)artGO.transform;
            artRT.anchorMin = Vector2.zero;
            artRT.anchorMax = Vector2.one;
            artRT.offsetMin = new Vector2(0, bottomBandH);
            artRT.offsetMax = new Vector2(0, -topBandH);
            var artImg = artGO.GetComponent<Image>();
            var artSprite = Resources.Load<Sprite>("CardArt/" + def.Id);
            if (artSprite != null)
            {
                artImg.sprite = artSprite;
                artImg.type = Image.Type.Simple;
                artImg.preserveAspect = false;
                artImg.color = Color.white;
            }
            else
            {
                // No art for this card (e.g. neutral/trickster/hoarder): paint a parchment
                // surface with a large translucent suit glyph so the card is never blank.
                var parchment = Color.Lerp(ThemePalette.CardCream, docColor, 0.18f);
                artImg.color = parchment;

                var glyphGO = new GameObject("ArtGlyph", typeof(RectTransform));
                glyphGO.transform.SetParent(artGO.transform, false);
                var glyphRT = (RectTransform)glyphGO.transform;
                glyphRT.anchorMin = Vector2.zero;
                glyphRT.anchorMax = Vector2.one;
                glyphRT.offsetMin = Vector2.zero;
                glyphRT.offsetMax = Vector2.zero;
                var glyphTMP = glyphGO.AddComponent<TextMeshProUGUI>();
                if (FontAssets.Mono) glyphTMP.font = FontAssets.Mono;
                glyphTMP.text = def.Suit.Glyph();
                glyphTMP.fontSize = isSmall ? 36 : isLarge ? 120 : 64;
                var glyphColor = def.Suit.IsRed() ? ThemePalette.RedSuit : ThemePalette.BlackSuit;
                glyphColor.a = 0.22f;
                glyphTMP.color = glyphColor;
                glyphTMP.alignment = TextAlignmentOptions.Center;
                glyphTMP.raycastTarget = false;
            }
            artImg.raycastTarget = false;

            // === BOTTOM BAND ===
            var bottomBand = new GameObject("BottomBand", typeof(RectTransform), typeof(Image));
            bottomBand.transform.SetParent(cardGO.transform, false);
            var botRT = (RectTransform)bottomBand.transform;
            botRT.anchorMin = Vector2.zero;
            botRT.anchorMax = new Vector2(1, 0);
            botRT.pivot = new Vector2(0.5f, 0);
            botRT.anchoredPosition = Vector2.zero;
            botRT.sizeDelta = new Vector2(0, bottomBandH);
            bottomBand.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.06f, 0.95f);
            bottomBand.GetComponent<Image>().raycastTarget = false;

            // Gold separator at top of bottom band
            var botSep = new GameObject("BotSep", typeof(RectTransform), typeof(Image));
            botSep.transform.SetParent(bottomBand.transform, false);
            var bsRT = (RectTransform)botSep.transform;
            bsRT.anchorMin = new Vector2(0, 1);
            bsRT.anchorMax = new Vector2(1, 1);
            bsRT.pivot = new Vector2(0.5f, 1);
            bsRT.anchoredPosition = Vector2.zero;
            bsRT.sizeDelta = new Vector2(0, 1);
            botSep.GetComponent<Image>().color = new Color(0.83f, 0.66f, 0.27f, 0.25f);
            botSep.GetComponent<Image>().raycastTarget = false;

            if (def.HasAbility)
            {
                float dotSize = isSmall ? 5 : isLarge ? 8 : 6;
                var dotGO = new GameObject("Dot", typeof(RectTransform), typeof(Image));
                dotGO.transform.SetParent(bottomBand.transform, false);
                var dotRT = (RectTransform)dotGO.transform;
                dotRT.anchorMin = new Vector2(0, 0.5f);
                dotRT.anchorMax = new Vector2(0, 0.5f);
                dotRT.pivot = new Vector2(0.5f, 0.5f);
                dotRT.anchoredPosition = new Vector2(dotSize + 2, 0);
                dotRT.sizeDelta = new Vector2(dotSize, dotSize);
                dotGO.GetComponent<Image>().color = ThemePalette.AbilityBadgeColor(def.Ability.Value);
                dotGO.GetComponent<Image>().raycastTarget = false;

                var abLabelGO = new GameObject("AbLabel", typeof(RectTransform));
                abLabelGO.transform.SetParent(bottomBand.transform, false);
                var abLabelRT = (RectTransform)abLabelGO.transform;
                abLabelRT.anchorMin = Vector2.zero;
                abLabelRT.anchorMax = new Vector2(0.8f, 1);
                abLabelRT.offsetMin = new Vector2(dotSize * 2 + 4, 0);
                abLabelRT.offsetMax = Vector2.zero;
                var abLabelTMP = abLabelGO.AddComponent<TextMeshProUGUI>();
                if (FontAssets.Body) abLabelTMP.font = FontAssets.Body;
                abLabelTMP.text = def.Ability.Value.DisplayName();
                abLabelTMP.fontSize = isSmall ? 8 : isLarge ? 11 : 9;
                abLabelTMP.color = ThemePalette.Parchment;
                abLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;
                abLabelTMP.enableWordWrapping = false;
                abLabelTMP.overflowMode = TextOverflowModes.Ellipsis;
                abLabelTMP.raycastTarget = false;
            }

            // Rarity pip (bottom band right)
            var rarityPipGlyph = def.Rarity switch
            {
                CardRarity.Uncommon => "◆",
                CardRarity.Rare => "★",
                _ => "●"
            };
            var rpGO = new GameObject("RarityPip", typeof(RectTransform));
            rpGO.transform.SetParent(bottomBand.transform, false);
            var rpRT = (RectTransform)rpGO.transform;
            rpRT.anchorMin = new Vector2(1, 0);
            rpRT.anchorMax = new Vector2(1, 1);
            rpRT.pivot = new Vector2(1, 0.5f);
            rpRT.anchoredPosition = new Vector2(-4, 0);
            rpRT.sizeDelta = new Vector2(20, 0);
            var rpTMP = rpGO.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Mono) rpTMP.font = FontAssets.Mono;
            rpTMP.text = rarityPipGlyph;
            rpTMP.fontSize = isSmall ? 7 : isLarge ? 10 : 8;
            rpTMP.color = rarityBorder;
            rpTMP.alignment = TextAlignmentOptions.MidlineRight;
            rpTMP.raycastTarget = false;

            // === FRAME OVERLAY (9-sliced ornate border on top of everything) ===
            var frameSprite = Resources.Load<Sprite>("CardFrameGold");
            if (frameSprite != null)
            {
                var frameGO = new GameObject("FrameOverlay", typeof(RectTransform), typeof(Image));
                frameGO.transform.SetParent(cardGO.transform, false);
                var frameRT = (RectTransform)frameGO.transform;
                frameRT.anchorMin = Vector2.zero;
                frameRT.anchorMax = Vector2.one;
                frameRT.offsetMin = Vector2.zero;
                frameRT.offsetMax = Vector2.zero;
                var frameImg = frameGO.GetComponent<Image>();
                frameImg.sprite = frameSprite;
                frameImg.type = Image.Type.Sliced;
                frameImg.fillCenter = false;
                frameImg.pixelsPerUnitMultiplier = isSmall ? 4.5f : isLarge ? 1.8f : 3f;
                frameImg.color = new Color(1f, 1f, 1f, isSmall ? 0.4f : 0.55f);
                frameImg.raycastTarget = false;
            }

            return cardGO;
        }

        // ---------- Card Reward ----------

        List<CardDefinition> _cardRewardOfferings;

        List<CardDefinition> PickCardRewardOfferings(int count)
        {
            if (!_run.PlayerDoctrine.HasValue || !CardCatalog.IsInitialized) return new List<CardDefinition>();

            var pool = CardCatalog.Draftable(_run.PlayerDoctrine.Value);
            var candidates = new List<CardDefinition>();
            foreach (var card in pool)
            {
                if (_run.PlayerDeckCardIds.Contains(card.Id)) continue;
                if (card.InStartingDeck && card.Rarity == CardRarity.Common) continue;
                candidates.Add(card);
            }

            bool isElite = _currentNode?.Type == MapNodeType.EliteMatch || _currentNode?.Type == MapNodeType.BossMatch;
            var result = new List<CardDefinition>();
            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                CardRarity targetRarity;
                int roll = _rng.Next(100);
                if (isElite)
                    targetRarity = roll < 25 ? CardRarity.Rare : roll < 60 ? CardRarity.Uncommon : CardRarity.Common;
                else
                    targetRarity = roll < 10 ? CardRarity.Rare : roll < 35 ? CardRarity.Uncommon : CardRarity.Common;

                var rarityPool = candidates.FindAll(c => c.Rarity == targetRarity);
                if (rarityPool.Count == 0) rarityPool = candidates;

                int idx = _rng.Next(rarityPool.Count);
                var pick = rarityPool[idx];
                result.Add(pick);
                candidates.Remove(pick);
            }
            return result;
        }

        void ShowCardReward()
        {
            _cardRewardOfferings = PickCardRewardOfferings(3);
            if (_cardRewardOfferings.Count == 0) { FinishCardReward(); return; }
            UIScreenCapture.Instance?.NotifyModal(UIScreen.CardReward);

            if (CardRewardLabel) { CardRewardLabel.text = "Choose a card for your deck:"; CardRewardLabel.gameObject.SetActive(true); }
            ClearCardRewardButtons();

            if (CardRewardContainer)
            {
                CardRewardContainer.gameObject.SetActive(true);
                foreach (var cardDef in _cardRewardOfferings)
                    CreateCardRewardButton(cardDef);
            }

            if (CardRewardSkipButton)
            {
                CardRewardSkipButton.gameObject.SetActive(true);
                CardRewardSkipButton.onClick.RemoveAllListeners();
                CardRewardSkipButton.onClick.AddListener(OnCardRewardSkip);
            }
        }

        void CreateCardRewardButton(CardDefinition def)
        {
            const float cardW = 220, cardH = 310, labelH = 24;
            var wrapper = new GameObject("RewardSlot_" + def.Id, typeof(RectTransform));
            wrapper.transform.SetParent(CardRewardContainer, false);
            var wrapperRT = (RectTransform)wrapper.transform;
            wrapperRT.sizeDelta = new Vector2(cardW, cardH + labelH);

            var le = wrapper.AddComponent<LayoutElement>();
            le.preferredWidth = cardW;
            le.preferredHeight = cardH + labelH;

            var miniCard = CreateMiniCard(def, wrapper.transform, cardW, cardH);
            var miniRT = (RectTransform)miniCard.transform;
            miniRT.anchorMin = new Vector2(0.5f, 1);
            miniRT.anchorMax = new Vector2(0.5f, 1);
            miniRT.pivot = new Vector2(0.5f, 1);
            miniRT.anchoredPosition = Vector2.zero;

            var rarityColor = def.Rarity switch
            {
                CardRarity.Common => ThemePalette.RarityCommon,
                CardRarity.Uncommon => ThemePalette.RarityUncommon,
                CardRarity.Rare => ThemePalette.RarityRare,
                _ => ThemePalette.DustyTan
            };
            var rarityLabelGO = new GameObject("RarityLabel", typeof(RectTransform));
            rarityLabelGO.transform.SetParent(wrapper.transform, false);
            var rlRT = (RectTransform)rarityLabelGO.transform;
            rlRT.anchorMin = new Vector2(0, 0);
            rlRT.anchorMax = new Vector2(1, 0);
            rlRT.pivot = new Vector2(0.5f, 0);
            rlRT.anchoredPosition = Vector2.zero;
            rlRT.sizeDelta = new Vector2(0, labelH);
            var rlTMP = rarityLabelGO.AddComponent<TextMeshProUGUI>();
            rlTMP.text = def.Rarity.ToString().ToUpper();
            rlTMP.fontSize = 17;
            rlTMP.fontStyle = FontStyles.Bold;
            rlTMP.characterSpacing = 4f;
            // Brighten the rarity label so it reads against the dark venue background.
            rlTMP.color = Color.Lerp(rarityColor, Color.white, 0.35f);
            rlTMP.alignment = TextAlignmentOptions.Center;
            rlTMP.raycastTarget = false;
            if (FontAssets.Heading) rlTMP.font = FontAssets.Heading;

            var btn = wrapper.AddComponent<Button>();
            var captured = def;
            btn.onClick.AddListener(() => OnCardRewardPicked(captured));
        }

        void OnCardRewardPicked(CardDefinition def)
        {
            _run.PlayerDeckCardIds.Add(def.Id);
            Debug.Log($"[RunManager] Added card '{def.Name}' to deck. Deck size: {_run.PlayerDeckCardIds.Count}");
            UpdateRunHud();
            FinishCardReward();
        }

        void OnCardRewardSkip()
        {
            Debug.Log("[RunManager] Skipped card reward.");
            FinishCardReward();
        }

        bool ShouldSkipCardReward()
        {
            if (_run?.PlayerDoctrine == null) return false;
            int deckSize = _run.PlayerDeckCardIds.Count;
            switch (_run.PlayerDoctrine.Value)
            {
                case DoctrineType.Schemer:
                    if (deckSize > 14) return _rng.Next(100) < 60;
                    if (deckSize > 12) return _rng.Next(100) < 30;
                    return false;
                case DoctrineType.Brute:
                    if (deckSize > 16) return _rng.Next(100) < 30;
                    return false;
                case DoctrineType.Trickster:
                    if (deckSize > 18) return _rng.Next(100) < 40;
                    if (deckSize > 15) return _rng.Next(100) < 20;
                    return false;
                case DoctrineType.Hoarder:
                    if (deckSize > 16) return _rng.Next(100) < 30;
                    if (deckSize > 14) return _rng.Next(100) < 15;
                    return false;
                default:
                    return false;
            }
        }

        void ApplyPostMatchDeckManagement()
        {
            if (_run?.PlayerDoctrine == null) return;
            bool isElite = _currentNode?.Type == MapNodeType.EliteMatch || _currentNode?.Type == MapNodeType.BossMatch;
            string removed = null;
            switch (_run.PlayerDoctrine.Value)
            {
                case DoctrineType.Schemer:
                    if (_run.PlayerDeckCardIds.Contains("schemer_expurgator") && _rng.Next(100) < 40)
                        removed = RemoveWeakestCommon();
                    if (removed == null && _run.PlayerDeckCardIds.Contains("schemer_censor") && _rng.Next(100) < 25)
                        removed = RemoveWeakestCommon();
                    break;
                case DoctrineType.Brute:
                    if (_rng.Next(100) < 15)
                        removed = RemoveWeakestCommon();
                    if (removed == null && _run.PlayerDeckCardIds.Contains("brute_scorched_earth") && _rng.Next(100) < 20)
                        removed = RemoveWeakestCommon();
                    if (removed == null && _run.PlayerDeckCardIds.Contains("brute_forge_master") && isElite && _rng.Next(100) < 40)
                        removed = RemoveWeakestCommon();
                    break;
                case DoctrineType.Hoarder:
                    if (_run.PlayerDeckCardIds.Count > 12 && _rng.Next(100) < 15)
                        removed = RemoveWeakestCommon();
                    break;
            }
            if (removed != null)
            {
                Debug.Log($"[RunManager] Post-match deck management: removed '{removed}'. Deck size: {_run.PlayerDeckCardIds.Count}");
                UpdateRunHud();
            }
        }

        string RemoveWeakestCommon()
        {
            if (_run.PlayerDeckCardIds.Count <= 8) return null;
            string weakest = null;
            int weakestRank = int.MaxValue;
            foreach (var id in _run.PlayerDeckCardIds)
            {
                if (!CardCatalog.TryGet(id, out var def)) continue;
                if (def.Rarity != CardRarity.Common) continue;
                int rank = (int)def.Rank;
                if (rank < weakestRank) { weakestRank = rank; weakest = id; }
            }
            if (weakest == null) return null;
            _run.PlayerDeckCardIds.Remove(weakest);
            string name = CardCatalog.TryGet(weakest, out var wDef) ? wDef.Name : weakest;
            return name;
        }

        void FinishCardReward()
        {
            _cardRewardOfferings = null;
            HideCardReward();

            ApplyPostMatchDeckManagement();

            _abilityPickOfferings = PickAbilityOfferings(3);
            if (_abilityPickOfferings.Count > 0)
                ShowAbilityPick();
            else
            {
                HideAbilityPick();
                if (ResultContinueButton) ResultContinueButton.gameObject.SetActive(true);
            }
        }

        void HideCardReward()
        {
            if (CardRewardLabel) CardRewardLabel.gameObject.SetActive(false);
            ClearCardRewardButtons();
            if (CardRewardContainer) CardRewardContainer.gameObject.SetActive(false);
            if (CardRewardSkipButton) CardRewardSkipButton.gameObject.SetActive(false);
        }

        void ClearCardRewardButtons()
        {
            if (!CardRewardContainer) return;
            for (int i = CardRewardContainer.childCount - 1; i >= 0; i--)
                Destroy(CardRewardContainer.GetChild(i).gameObject);
        }

        // ---------- Deck Browser ----------

        void ShowDeckBrowser()
        {
            if (!DeckBrowserPanel) BuildDeckBrowserPanel();
            if (!DeckBrowserPanel) return;

            DeckBrowserPanel.SetActive(true);
            PopulateDeckBrowser();
            UIScreenCapture.Instance?.NotifyModal(UIScreen.DeckBrowser);
        }

        void HideDeckBrowser()
        {
            if (DeckBrowserPanel) DeckBrowserPanel.SetActive(false);
        }

        void BuildDeckBrowserPanel()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas) canvas = FindFirstObjectByType<Canvas>();
            if (!canvas) return;

            var panelGO = new GameObject("DeckBrowserPanel", typeof(RectTransform), typeof(Image));
            panelGO.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)panelGO.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            panelGO.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.1f, 0.97f);

            // Title
            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(panelGO.transform, false);
            var titleRT = (RectTransform)titleGO.transform;
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(0.7f, 1);
            titleRT.pivot = new Vector2(0, 1);
            titleRT.anchoredPosition = new Vector2(24, -16);
            titleRT.sizeDelta = new Vector2(0, 36);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Heading) titleTMP.font = FontAssets.Heading;
            titleTMP.text = "Your Deck";
            titleTMP.fontSize = 28;
            titleTMP.color = ThemePalette.Gold;
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            titleTMP.raycastTarget = false;

            // Stats
            var statsGO = new GameObject("Stats", typeof(RectTransform));
            statsGO.transform.SetParent(panelGO.transform, false);
            var statsRT = (RectTransform)statsGO.transform;
            statsRT.anchorMin = new Vector2(0, 1);
            statsRT.anchorMax = new Vector2(1, 1);
            statsRT.pivot = new Vector2(0, 1);
            statsRT.anchoredPosition = new Vector2(24, -52);
            statsRT.sizeDelta = new Vector2(-48, 20);
            DeckStatsLabel = statsGO.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Mono) DeckStatsLabel.font = FontAssets.Mono;
            DeckStatsLabel.fontSize = 14;
            DeckStatsLabel.color = ThemePalette.DustyTan;
            DeckStatsLabel.alignment = TextAlignmentOptions.MidlineLeft;
            DeckStatsLabel.raycastTarget = false;

            // Close button
            var closeGO = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGO.transform.SetParent(panelGO.transform, false);
            var closeRT = (RectTransform)closeGO.transform;
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-16, -12);
            closeRT.sizeDelta = new Vector2(90, 32);
            closeGO.GetComponent<Image>().color = ThemePalette.DarkSlate;
            var closeBtnLabel = new GameObject("Label", typeof(RectTransform));
            closeBtnLabel.transform.SetParent(closeGO.transform, false);
            var clRT = (RectTransform)closeBtnLabel.transform;
            clRT.anchorMin = Vector2.zero; clRT.anchorMax = Vector2.one;
            clRT.offsetMin = Vector2.zero; clRT.offsetMax = Vector2.zero;
            var clTMP = closeBtnLabel.AddComponent<TextMeshProUGUI>();
            if (FontAssets.Heading) clTMP.font = FontAssets.Heading;
            clTMP.text = "Close";
            clTMP.fontSize = 14;
            clTMP.color = ThemePalette.Parchment;
            clTMP.alignment = TextAlignmentOptions.Center;
            clTMP.raycastTarget = false;
            DeckBrowserCloseButton = closeGO.GetComponent<Button>();
            DeckBrowserCloseButton.onClick.AddListener(HideDeckBrowser);

            // Scroll content area
            var scrollGO = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollGO.transform.SetParent(panelGO.transform, false);
            var scrollRT = (RectTransform)scrollGO.transform;
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(16, 8);
            scrollRT.offsetMax = new Vector2(-16, -76);

            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(scrollGO.transform, false);
            var contentRT = (RectTransform)contentGO.transform;
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(8, 8, 4, 4);

            var csf = contentGO.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.content = contentRT;
            scroll.horizontal = false;
            scroll.vertical = true;

            DeckBrowserContainer = contentRT;
            DeckBrowserPanel = panelGO;
            panelGO.SetActive(false);
        }

        void PopulateDeckBrowser()
        {
            if (!DeckBrowserContainer) return;

            for (int i = DeckBrowserContainer.childCount - 1; i >= 0; i--)
                Destroy(DeckBrowserContainer.GetChild(i).gameObject);

            var cards = new List<CardDefinition>();
            int totalRank = 0;
            int abilityCount = 0;
            foreach (var id in _run.PlayerDeckCardIds)
            {
                if (!CardCatalog.TryGet(id, out var def)) continue;
                cards.Add(def);
                Rank r = def.Rank;
                if (_run.CardRankOverrides.TryGetValue(id, out var oRank)) r = oRank;
                totalRank += (int)r;
                if (def.HasAbility) abilityCount++;
            }

            float avgRank = cards.Count > 0 ? (float)totalRank / cards.Count : 0;
            string doctrine = _run.PlayerDoctrine.HasValue ? _run.PlayerDoctrine.Value.ToString() : "None";
            if (DeckStatsLabel)
                DeckStatsLabel.text = $"Cards: {cards.Count}    Avg Rank: {avgRank:F1}    Abilities: {abilityCount}    Doctrine: {doctrine}";

            var suitOrder = new[] { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
            foreach (var suit in suitOrder)
            {
                var suitCards = cards.FindAll(c => c.Suit == suit);
                if (suitCards.Count == 0) continue;

                suitCards.Sort((a, b) =>
                {
                    Rank ra = a.Rank, rb = b.Rank;
                    if (_run.CardRankOverrides.TryGetValue(a.Id, out var oa)) ra = oa;
                    if (_run.CardRankOverrides.TryGetValue(b.Id, out var ob)) rb = ob;
                    return ra.CompareTo(rb);
                });

                // Suit group container
                var groupGO = new GameObject($"SuitGroup_{suit}", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
                groupGO.transform.SetParent(DeckBrowserContainer, false);
                var gVlg = groupGO.GetComponent<VerticalLayoutGroup>();
                gVlg.spacing = 6;
                gVlg.childForceExpandWidth = true;
                gVlg.childForceExpandHeight = false;

                // Suit header
                var headerGO = new GameObject("Header", typeof(RectTransform), typeof(LayoutElement));
                headerGO.transform.SetParent(groupGO.transform, false);
                headerGO.GetComponent<LayoutElement>().preferredHeight = 24;
                var headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
                if (FontAssets.Heading) headerTMP.font = FontAssets.Heading;
                string suitColorTag = suit.IsRed() ? "<color=#C02020>" : "<color=#CCCCCC>";
                headerTMP.text = $"{suitColorTag}{suit.Glyph()}</color>  {suit}  ({suitCards.Count})";
                headerTMP.richText = true;
                headerTMP.fontSize = 16;
                headerTMP.color = ThemePalette.DustyTan;
                headerTMP.alignment = TextAlignmentOptions.MidlineLeft;
                headerTMP.raycastTarget = false;

                // Card row
                var rowGO = new GameObject("CardRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                rowGO.transform.SetParent(groupGO.transform, false);
                var rowHlg = rowGO.GetComponent<HorizontalLayoutGroup>();
                rowHlg.spacing = 8;
                rowHlg.childForceExpandWidth = false;
                rowHlg.childForceExpandHeight = false;
                rowHlg.childAlignment = TextAnchor.MiddleLeft;
                rowGO.GetComponent<LayoutElement>().preferredHeight = 130;

                foreach (var cardDef in suitCards)
                {
                    var miniCard = CreateMiniCard(cardDef, rowGO.transform, 88, 126);
                    var mcLE = miniCard.AddComponent<LayoutElement>();
                    mcLE.preferredWidth = 88;
                    mcLE.preferredHeight = 126;

                    var btn = miniCard.AddComponent<Button>();
                    var captured = cardDef;
                    btn.onClick.AddListener(() => ShowCardDetail(captured));
                }
            }
        }

        // ---------- Shop Card Purchase ----------

        void CreateCardShopItems()
        {
            if (!ShopItemContainer || !CardCatalog.IsInitialized || !_run.PlayerDoctrine.HasValue) return;
            if (_run.PlayerDeckCardIds.Count >= 20) return;

            var offerings = PickCardRewardOfferings(2);
            if (offerings.Count > 0)
                CreateShopSectionHeader("Cards for Sale", new Color(0.1f, 0.54f, 0.48f));
            foreach (var cardDef in offerings)
            {
                int price = cardDef.Rarity switch
                {
                    CardRarity.Common => 6,
                    CardRarity.Uncommon => 10,
                    CardRarity.Rare => 16,
                    _ => 8
                };
                if (Ascension.InflatedPrices(_run.AscensionLevel))
                    price = (int)(price * 1.25f);
                if (_run.PlayerBurdens.Contains(BurdenType.BadReputation))
                    price = (int)(price * 1.20f);

                bool canBuy = _run.Florins >= price && _run.PlayerDeckCardIds.Count < 20;

                var btnGO = new GameObject($"ShopCard_{cardDef.Id}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                btnGO.transform.SetParent(ShopItemContainer, false);
                btnGO.GetComponent<Image>().color = canBuy ? new Color(0.07f, 0.22f, 0.18f, 0.85f) : ThemePalette.LockedGray;
                btnGO.GetComponent<LayoutElement>().preferredHeight = 90;
                btnGO.GetComponent<LayoutElement>().preferredWidth = 550;

                // Rarity accent
                var rarityColor = cardDef.Rarity switch
                {
                    CardRarity.Common => ThemePalette.RarityCommon,
                    CardRarity.Uncommon => ThemePalette.RarityUncommon,
                    CardRarity.Rare => ThemePalette.RarityRare,
                    _ => ThemePalette.DustyTan
                };
                var accentGO = new GameObject("Accent", typeof(RectTransform), typeof(Image));
                accentGO.transform.SetParent(btnGO.transform, false);
                var aRT = (RectTransform)accentGO.transform;
                aRT.anchorMin = Vector2.zero; aRT.anchorMax = new Vector2(0, 1);
                aRT.pivot = new Vector2(0, 0.5f); aRT.sizeDelta = new Vector2(4, 0);
                accentGO.GetComponent<Image>().color = canBuy ? rarityColor : ThemePalette.DisabledOutline;
                accentGO.GetComponent<Image>().raycastTarget = false;

                // Suit+Rank
                Rank displayRank = cardDef.Rank;
                if (_run.CardRankOverrides.TryGetValue(cardDef.Id, out var oRank)) displayRank = oRank;

                var suitGO = new GameObject("SuitRank", typeof(RectTransform));
                suitGO.transform.SetParent(btnGO.transform, false);
                var sRT = (RectTransform)suitGO.transform;
                sRT.anchorMin = new Vector2(0, 0); sRT.anchorMax = new Vector2(0, 1);
                sRT.pivot = new Vector2(0, 0.5f); sRT.anchoredPosition = new Vector2(14, 0); sRT.sizeDelta = new Vector2(36, 0);
                var suitTMP = suitGO.AddComponent<TextMeshProUGUI>();
                if (FontAssets.Mono) suitTMP.font = FontAssets.Mono;
                suitTMP.text = $"{displayRank.Label()}{cardDef.Suit.Glyph()}";
                suitTMP.fontSize = 20;
                suitTMP.color = canBuy ? (cardDef.Suit.IsRed() ? ThemePalette.VenetianRed : ThemePalette.Parchment) : ThemePalette.DisabledText;
                suitTMP.alignment = TextAlignmentOptions.Center;
                suitTMP.raycastTarget = false;

                // Name
                var nameGO = new GameObject("Name", typeof(RectTransform));
                nameGO.transform.SetParent(btnGO.transform, false);
                var nRT = (RectTransform)nameGO.transform;
                nRT.anchorMin = new Vector2(0, 0.5f); nRT.anchorMax = new Vector2(1, 1);
                nRT.offsetMin = new Vector2(54, 0); nRT.offsetMax = new Vector2(-80, -4);
                var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
                if (FontAssets.Heading) nameTMP.font = FontAssets.Heading;
                string docTag = $"<color={DoctrineHexColor(cardDef.Doctrine)}>{cardDef.Doctrine}</color>";
                nameTMP.text = $"{cardDef.Name}  {docTag}";
                nameTMP.richText = true;
                nameTMP.fontSize = 17;
                nameTMP.color = canBuy ? ThemePalette.Parchment : ThemePalette.DisabledText;
                nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
                nameTMP.raycastTarget = false;

                // Ability desc
                var descGO = new GameObject("Desc", typeof(RectTransform));
                descGO.transform.SetParent(btnGO.transform, false);
                var dRT = (RectTransform)descGO.transform;
                dRT.anchorMin = Vector2.zero; dRT.anchorMax = new Vector2(1, 0.5f);
                dRT.offsetMin = new Vector2(54, 4); dRT.offsetMax = new Vector2(-80, 0);
                var descTMP = descGO.AddComponent<TextMeshProUGUI>();
                if (cardDef.HasAbility)
                    descTMP.text = $"{cardDef.Ability.Value.DisplayName()} · {cardDef.AbilityText ?? cardDef.Ability.Value.Description()}";
                descTMP.fontSize = 13;
                descTMP.color = ThemePalette.DescGray;
                descTMP.enableWordWrapping = true;
                descTMP.alignment = TextAlignmentOptions.MidlineLeft;
                descTMP.raycastTarget = false;

                // Price
                var priceGO = new GameObject("Price", typeof(RectTransform));
                priceGO.transform.SetParent(btnGO.transform, false);
                var pRT = (RectTransform)priceGO.transform;
                pRT.anchorMin = new Vector2(1, 0); pRT.anchorMax = new Vector2(1, 1);
                pRT.pivot = new Vector2(1, 0.5f); pRT.sizeDelta = new Vector2(65, 0); pRT.anchoredPosition = new Vector2(-10, 0);
                var priceTMP = priceGO.AddComponent<TextMeshProUGUI>();
                if (FontAssets.Mono) priceTMP.font = FontAssets.Mono;
                priceTMP.text = $"{price}f";
                priceTMP.fontSize = 20;
                priceTMP.color = canBuy ? ThemePalette.Gold : ThemePalette.DisabledText;
                priceTMP.alignment = TextAlignmentOptions.Center;
                priceTMP.raycastTarget = false;

                var btn = btnGO.GetComponent<Button>();
                btn.interactable = canBuy;
                var capturedDef = cardDef;
                int capturedPrice = price;
                btn.onClick.AddListener(() => OnShopCardBuy(capturedDef, capturedPrice));
            }
        }

        void OnShopCardBuy(CardDefinition def, int price)
        {
            if (_run.Florins < price) return;
            _run.PlayerDeckCardIds.Add(def.Id);
            _run.Florins -= price;
            Debug.Log($"[RunManager] Bought card '{def.Name}' for {price}f. Deck: {_run.PlayerDeckCardIds.Count}");
            OnShopAction?.Invoke($"Bought '{def.Name}' (-{price} Florins)");
            UpdateShopFlorins();
            UpdateRunHud();
            PopulateShopItems();
        }
    }
}
