using System;
using System.Collections.Generic;
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
        public GameObject MapNodeButtonPrefab;

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

        [Header("Opponent Portraits")]
        public string[] PortraitNames;
        public Sprite[] PortraitSprites;

        RunState _run;
        RunPhase _phase;
        public RunPhase CurrentPhase => _phase;
        int _currentColumn;
        System.Random _rng;
        MapNode _currentNode;
        ArchetypeType? _selectedArchetype;

        bool _autoRun;
        float _autoStepDelay = 0.15f;
        float _nextAutoStep;

        void Start()
        {
            if (GameManager) GameManager.AutoStartOnAwake = false;
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
            if (Enum.TryParse<RunPhase>(save.RunPhase, out var phase))
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
            if (!_autoRun || Time.time < _nextAutoStep) return;
            AutoStep();
        }

        void ToggleAutoRun()
        {
            _autoRun = !_autoRun;
            if (_autoRun)
            {
                _nextAutoStep = Time.time + _autoStepDelay;
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
            _autoRun = true;
            Time.timeScale = 20f;
            StartNewRun();
            _nextAutoStep = Time.time + _autoStepDelay;
            Debug.Log("[RunManager] Auto-run started (fresh run, timeScale=20)");
        }

        public void StartBatchRun(int count)
        {
            _batchRemaining = count;
            _batchWins = 0;
            _batchMatches = 0;
            _batchMatchWins = 0;
            _batchResults = new List<string>();
            _autoRun = true;
            Time.timeScale = 20f;
            StartNewRun();
            _nextAutoStep = Time.time + _autoStepDelay;
            Debug.Log($"[RunManager] Batch run started — {count} runs");
        }

        void AutoStep()
        {
            _nextAutoStep = Time.time + _autoStepDelay;
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
                case RunPhase.PostMatch:
                    if (_abilityPickOfferings != null && _abilityPickOfferings.Count > 0)
                        OnAbilityPicked(_abilityPickOfferings[_rng.Next(_abilityPickOfferings.Count)]);
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
                    }
                    break;
            }
        }

        void AutoSelectNode()
        {
            if (_currentColumn >= _run.CurrentMap.Count) return;
            var column = _run.CurrentMap[_currentColumn];
            MapNode best = column[0];
            int bestPriority = NodePriority(best.Type);
            for (int i = 1; i < column.Count; i++)
            {
                int p = NodePriority(column[i].Type);
                if (p > bestPriority) { best = column[i]; bestPriority = p; }
            }
            OnNodeSelected(best);
        }

        static int NodePriority(MapNodeType t) => t switch
        {
            MapNodeType.BossMatch => 100,
            MapNodeType.EliteMatch => 90,
            MapNodeType.RivalMatch => 80,
            MapNodeType.Rest => 30,
            MapNodeType.Rumor => 20,
            MapNodeType.Shop => 10,
            _ => 0
        };

        void AutoHandleShop()
        {
            if (_run.Florins >= 8 && _run.PlayerAbilities.Count < _run.MaxAbilitySlots)
            {
                var offerings = PickShopOfferings(1);
                if (offerings.Count > 0 && _run.Florins >= offerings[0].price)
                {
                    OnShopBuy(offerings[0].type, offerings[0].price);
                    return;
                }
            }
            if (_run.Florins >= 10 && _run.PlayerTrinkets.Count < 5)
            {
                var trinket = PickTrinketOffering();
                if (trinket.HasValue && _run.Florins >= trinket.Value.price)
                {
                    OnShopBuyTrinket(trinket.Value.type, trinket.Value.price);
                    return;
                }
            }
            OnResultContinue();
        }

        void AutoHandleEvent()
        {
            if (EventContinueButton && EventContinueButton.gameObject.activeSelf)
                OnResultContinue();
            else if (EventChoice1Button && EventChoice1Button.gameObject.activeSelf)
                _eventChoice1Action?.Invoke();
        }

        public void StartNewRun()
        {
            RunSaveSystem.Delete();
            int seed = Environment.TickCount;
            _rng = new System.Random(seed);
            _run = new RunState { Seed = seed };
            _selectedArchetype = null;

            _run.CurrentAct = 0;
            _run.CurrentMap = MapGenerator.Generate(0, _rng);
            _currentColumn = 0;
            ApplyActTheme(0);

            SetPhase(RunPhase.ArchetypeSelect);
        }

        void OnArchetypeSelected(ArchetypeType archetype)
        {
            _selectedArchetype = archetype;
            _run.PlayerAbilities.AddRange(archetype.StartingAbilities());
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
            if (EventPanel) EventPanel.SetActive(phase == RunPhase.Event || phase == RunPhase.Rest);
            if (RunHudPanel) RunHudPanel.SetActive(phase != RunPhase.Title && phase != RunPhase.RunOver && phase != RunPhase.ArchetypeSelect);

            UpdateRunHud();

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
            if (AbilitiesLabel) AbilitiesLabel.text = $"Abilities: {_run.PlayerAbilities.Count}/{_run.MaxAbilitySlots}";
        }

        // ---------- Archetype Select ----------

        void ShowArchetypeSelect()
        {
            var repData = ReputationSystem.Load();
            if (MapTitleLabel)
                MapTitleLabel.text = $"Choose Your Archetype  (Rep: {repData.TotalReputation})";
            ClearMapNodes();
            if (!MapNodeContainer) return;

            foreach (var archetype in ArchetypeDefinitions.AllArchetypes)
            {
                bool unlocked = repData.UnlockedArchetypes.Contains(archetype);
                CreateArchetypeButton(archetype, unlocked);
            }
        }

        void CreateArchetypeButton(ArchetypeType archetype, bool unlocked)
        {
            var abilities = archetype.StartingAbilities();
            string abilityList = string.Join(", ", abilities.ConvertAll(a => a.DisplayName()));

            int repNeeded = archetype switch
            {
                ArchetypeType.Brute => 25,
                ArchetypeType.Diplomat => 100,
                ArchetypeType.Gambler => 300,
                _ => 0
            };

            var btnGO = new GameObject($"Archetype_{archetype}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(MapNodeContainer, false);

            var img = btnGO.GetComponent<Image>();
            img.color = unlocked ? archetype switch
            {
                ArchetypeType.Rogue => ThemePalette.ArchRogue,
                ArchetypeType.Brute => ThemePalette.ArchBrute,
                ArchetypeType.Diplomat => ThemePalette.ArchDiplomat,
                ArchetypeType.Gambler => ThemePalette.ArchGambler,
                _ => Color.gray
            } : ThemePalette.LockedGray;

            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 100;
            le.preferredWidth = 550;

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
            descTMP.text = unlocked ? $"Starts with: {abilityList}" : "???";
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

        // ---------- Map ----------

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

            var column = _run.CurrentMap[_currentColumn];
            for (int i = 0; i < column.Count; i++)
            {
                var node = column[i];
                CreateMapNodeButton(node, i);
            }
        }

        void ClearMapNodes()
        {
            if (!MapNodeContainer) return;
            for (int i = MapNodeContainer.childCount - 1; i >= 0; i--)
                Destroy(MapNodeContainer.GetChild(i).gameObject);
        }

        void CreateMapNodeButton(MapNode node, int index)
        {
            if (!MapNodeContainer) return;

            GameObject btnGO;
            if (MapNodeButtonPrefab)
                btnGO = Instantiate(MapNodeButtonPrefab, MapNodeContainer);
            else
            {
                btnGO = new GameObject($"Node_{index}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                btnGO.transform.SetParent(MapNodeContainer, false);
                btnGO.GetComponent<Image>().color = NodeColor(node.Type);
                var le = btnGO.GetComponent<LayoutElement>();
                le.preferredHeight = 80;
                le.preferredWidth = 500;

                var lblGO = new GameObject("Label", typeof(RectTransform));
                lblGO.transform.SetParent(btnGO.transform, false);
                var lblRT = (RectTransform)lblGO.transform;
                lblRT.anchorMin = Vector2.zero;
                lblRT.anchorMax = Vector2.one;
                lblRT.offsetMin = Vector2.zero;
                lblRT.offsetMax = Vector2.zero;
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text = NodeLabel(node);
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.fontSize = 22;
                lbl.color = Color.white;
                lbl.raycastTarget = false;
            }

            var button = btnGO.GetComponent<Button>();
            var capturedNode = node;
            button.onClick.AddListener(() => OnNodeSelected(capturedNode));
        }

        string NodeLabel(MapNode node) => node.Type switch
        {
            MapNodeType.RivalMatch => $"[Match] {node.Opponent?.Name ?? "Rival"}",
            MapNodeType.EliteMatch => $"[ELITE] {node.Opponent?.Name ?? "Elite"}",
            MapNodeType.BossMatch => $"[BOSS] {node.Opponent?.Name ?? "Boss"}",
            MapNodeType.Shop => "[Shop] The Fence",
            MapNodeType.Rumor => "[Rumor] A whispered lead...",
            MapNodeType.Rest => "[Rest] The Hearth",
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

        void OnNodeSelected(MapNode node)
        {
            _currentNode = node;
            _currentColumn++;

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
                    HandleRumor();
                    break;
                case MapNodeType.Rest:
                    HandleRest();
                    break;
            }
        }

        // ---------- Match ----------

        int _boutsDefended;

        void StartMatch(MapNode node)
        {
            var config = MatchSetup.Build(_run, node.Opponent, _rng);
            SetPhase(RunPhase.InMatch);
            _boutsDefended = 0;

            GameManager.OnMatchComplete -= OnMatchComplete;
            GameManager.OnMatchComplete += OnMatchComplete;
            GameManager.BeginConfiguredGame(config, node.Opponent, _rng.Next());
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
                _run.Florins += florinsEarned;
            }
            else
            {
                _run.Prestige--;
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

            _abilityPickOfferings = null;
            if (won)
            {
                _abilityPickOfferings = PickAbilityOfferings(3);
                if (_abilityPickOfferings.Count > 0)
                    ShowAbilityPick();
                else
                    HideAbilityPick();
            }
            else
                HideAbilityPick();

            if (ResultContinueButton)
            {
                ResultContinueButton.onClick.RemoveAllListeners();
                ResultContinueButton.onClick.AddListener(OnResultContinue);
                ResultContinueButton.gameObject.SetActive(_abilityPickOfferings == null || _abilityPickOfferings.Count == 0);
            }
        }

        List<AbilityType> PickAbilityOfferings(int count)
        {
            var pool = new List<AbilityDefinition>();
            bool isElite = _currentNode?.Type == MapNodeType.EliteMatch || _currentNode?.Type == MapNodeType.BossMatch;
            foreach (var def in AbilityPool.All)
            {
                if (_run.PlayerAbilities.Contains(def.Type)) continue;
                if (!isElite && def.Rarity == AbilityRarity.Rare && _rng.Next(100) >= 30) continue;
                pool.Add(def);
            }
            var result = new List<AbilityType>();
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int idx = _rng.Next(pool.Count);
                result.Add(pool[idx].Type);
                pool.RemoveAt(idx);
            }
            return result;
        }

        void ShowAbilityPick()
        {
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
            bool slotsFull = _run.PlayerAbilities.Count >= _run.MaxAbilitySlots;

            var btnGO = new GameObject($"Pick_{abilityType}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(AbilityPickContainer, false);

            var img = btnGO.GetComponent<Image>();
            img.color = def.Rarity switch
            {
                AbilityRarity.Common => ThemePalette.RarityCommonBg,
                AbilityRarity.Uncommon => ThemePalette.RarityUncommonBg,
                AbilityRarity.Rare => ThemePalette.RarityRareBg,
                _ => ThemePalette.LockedGray
            };

            var le = btnGO.GetComponent<LayoutElement>();
            le.preferredHeight = 80;
            le.preferredWidth = 500;

            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(btnGO.transform, false);
            var nameRT = (RectTransform)nameGO.transform;
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(12, 0);
            nameRT.offsetMax = new Vector2(-12, -2);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            string rarityTag = def.Rarity != AbilityRarity.Common ? $"  [{def.Rarity}]" : "";
            string bindTag = def.IsPassive ? " (Passive)" : $" ({def.BindingCount} cards)";
            nameTMP.text = $"{abilityType.DisplayName()}{rarityTag}{bindTag}";
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.fontSize = 20;
            nameTMP.color = Color.white;
            nameTMP.raycastTarget = false;

            var descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(btnGO.transform, false);
            var descRT = (RectTransform)descGO.transform;
            descRT.anchorMin = Vector2.zero;
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.offsetMin = new Vector2(12, 2);
            descRT.offsetMax = new Vector2(-12, 0);
            var descTMP = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text = abilityType.Description();
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.fontSize = 15;
            descTMP.color = ThemePalette.DescGray;
            descTMP.raycastTarget = false;
            descTMP.enableWordWrapping = true;

            var btn = btnGO.GetComponent<Button>();
            var captured = abilityType;
            btn.onClick.AddListener(() => OnAbilityPicked(captured));
        }

        void OnAbilityPicked(AbilityType type)
        {
            if (_run.PlayerAbilities.Count >= _run.MaxAbilitySlots)
            {
                var worst = _run.PlayerAbilities[0];
                _run.PlayerAbilities.RemoveAt(0);
                Debug.Log($"[RunManager] Replaced {worst.DisplayName()} with {type.DisplayName()}");
            }
            _run.PlayerAbilities.Add(type);
            _run.RecordAbilityPicked(type);
            UpdateRunHud();
            FinishAbilityPick();
        }

        void OnAbilityPickSkip()
        {
            FinishAbilityPick();
        }

        void FinishAbilityPick()
        {
            _abilityPickOfferings = null;
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
            foreach (var offering in abilityOfferings)
                CreateShopItemButton(offering);

            var trinketOffering = PickTrinketOffering();
            if (trinketOffering.HasValue)
                CreateTrinketShopButton(trinketOffering.Value);

            if (_run.PlayerBurdens.Count > 0)
                CreateBurdenRemovalButton();
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
            var pool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (_run.PlayerAbilities.Contains(def.Type)) continue;
                pool.Add(def);
            }

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
                result.Add((def.Type, price));
            }
            return result;
        }

        void CreateShopItemButton(( AbilityType type, int price) offering)
        {
            var def = AbilityPool.Get(offering.type);
            bool canBuy = _run.Florins >= offering.price && _run.PlayerAbilities.Count < _run.MaxAbilitySlots;
            string label = $"{offering.type.DisplayName()}  —  {offering.price}f";
            string desc = offering.type.Description();

            var btnGO = new GameObject("ShopItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGO.transform.SetParent(ShopItemContainer, false);

            var img = btnGO.GetComponent<Image>();
            img.color = canBuy ? ThemePalette.ShopTrinketBg : ThemePalette.LockedGray;

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
            nameTMP.color = canBuy ? Color.white : ThemePalette.DisabledText;
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

            var rarityTag = def.Rarity switch
            {
                AbilityRarity.Common => "",
                AbilityRarity.Uncommon => "  [Uncommon]",
                AbilityRarity.Rare => "  [Rare]",
                _ => ""
            };
            if (rarityTag.Length > 0) nameTMP.text += rarityTag;

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = canBuy;
            var captured = offering;
            btn.onClick.AddListener(() => OnShopBuy(captured.type, captured.price));
        }

        void CreateBurdenRemovalButton()
        {
            int price = 6;
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
                case 0:
                    ShowEventChoices("A Whispered Lead...",
                        "You overhear a conversation in the shadows.\nA gambler's secret, or a pickpocket's trap?",
                        "Investigate", "Walk away");
                    _eventChoice1Action = DoRumorInvestigate;
                    _eventChoice2Action = DoRumorWalkAway;
                    break;
                case 1:
                    ShowEventChoices("The Stranger's Wager",
                        "A hooded figure offers you a bet:\n\"Double your coin, or lose it all.\"",
                        "Accept the wager", "Decline politely");
                    _eventChoice1Action = () =>
                    {
                        if (_rng.Next(100) < 50)
                        {
                            int gain = 8 + _run.CurrentAct * 2;
                            _run.Florins += gain;
                            ShowEventOutcome($"Lady luck smiles! +{gain} Florins.");
                        }
                        else
                        {
                            int loss = Math.Min(_run.Florins, 6);
                            _run.Florins -= loss;
                            ShowEventOutcome($"The stranger vanishes with your coin. -{loss} Florins.");
                        }
                    };
                    _eventChoice2Action = () => ShowEventOutcome("Wise choice. You move on.");
                    break;
                case 2:
                    ShowEventChoices("The Herbalist's Cart",
                        "A traveling herbalist offers a strange tonic.\n\"Sharpen the mind, but steady the hand,\" she warns.",
                        "Drink the tonic", "Pass");
                    _eventChoice1Action = () =>
                    {
                        if (_rng.Next(100) < 65)
                        {
                            if (_run.PlayerAbilities.Count < _run.MaxAbilitySlots)
                            {
                                var reward = PickAbilityReward(false);
                                if (reward.HasValue)
                                {
                                    _run.PlayerAbilities.Add(reward.Value);
                                    _run.RecordAbilityPicked(reward.Value);
                                    ShowEventOutcome($"Your mind sharpens! Learned {reward.Value.DisplayName()}.");
                                    return;
                                }
                            }
                            _run.Florins += 4;
                            ShowEventOutcome("The tonic refreshes you. +4 Florins.");
                        }
                        else
                        {
                            AddBurden(BurdenType.ClumsyFingers);
                            ShowEventOutcome("The tonic makes your hands tremble. Gained Clumsy Fingers.");
                        }
                    };
                    _eventChoice2Action = DoRumorWalkAway;
                    break;
                case 3:
                    ShowEventChoices("The Card Sharp's Challenge",
                        "A notorious card sharp blocks your path.\n\"Beat me in a trick, and I'll share my secret.\"",
                        "Accept", "Avoid");
                    _eventChoice1Action = () =>
                    {
                        if (_rng.Next(100) < 55)
                        {
                            int gain = 6 + _run.CurrentAct;
                            _run.Florins += gain;
                            ShowEventOutcome($"You outplay the sharp! +{gain} Florins and bragging rights.");
                        }
                        else
                        {
                            AddBurden(BurdenType.MarkedCards);
                            ShowEventOutcome("The sharp marks your deck. Gained Marked Cards.");
                        }
                    };
                    _eventChoice2Action = () =>
                    {
                        _run.Florins += 1;
                        ShowEventOutcome("You slip past. +1 Florin from a loose pocket.");
                    };
                    break;
                case 4:
                    ShowEventChoices("The Beggar's Plea",
                        "A ragged beggar clutches your sleeve.\n\"Spare some coin? I know things about your next rival...\"",
                        "Give 5 Florins", "Ignore");
                    _eventChoice1Action = () =>
                    {
                        if (_run.Florins >= 5)
                        {
                            _run.Florins -= 5;
                            int gain = 8 + _run.CurrentAct * 2;
                            _run.Florins += gain;
                            ShowEventOutcome($"Useful intel! The tip pays off. +{gain - 5} Florins net.");
                        }
                        else
                            ShowEventOutcome("You don't have enough coin. The beggar shuffles away.");
                    };
                    _eventChoice2Action = () => ShowEventOutcome("You walk on. The city has no mercy.");
                    break;
                case 5:
                    ShowEventChoices("The Cursed Trinket",
                        "A vendor displays a shimmering trinket.\n\"Powerful, but not without cost,\" she whispers.",
                        "Take it", "Leave it");
                    _eventChoice1Action = () =>
                    {
                        if (_run.PlayerTrinkets.Count < 5)
                        {
                            var allTrinkets = (TrinketType[])Enum.GetValues(typeof(TrinketType));
                            var trinket = allTrinkets[_rng.Next(allTrinkets.Length)];
                            _run.PlayerTrinkets.Add(trinket);
                            AddBurden(BurdenType.RattledNerves);
                            ShowEventOutcome($"You gain {trinket.DisplayName()}, but your nerves fray. Gained Rattled Nerves.");
                        }
                        else
                        {
                            _run.Florins += 3;
                            ShowEventOutcome("Your pockets are full. You take coin instead. +3 Florins.");
                        }
                    };
                    _eventChoice2Action = () => ShowEventOutcome("Probably wise. You move on.");
                    break;
                case 6:
                    ShowEventChoices("The Drunken Sailor",
                        "A sailor stumbles into you, scattering cards everywhere.\n\"Sorry, friend! Take one for your trouble.\"",
                        "Pick up a card", "Help him up");
                    _eventChoice1Action = () =>
                    {
                        if (_run.PlayerAbilities.Count < _run.MaxAbilitySlots)
                        {
                            var reward = PickAbilityReward(false);
                            if (reward.HasValue)
                            {
                                _run.PlayerAbilities.Add(reward.Value);
                                _run.RecordAbilityPicked(reward.Value);
                                ShowEventOutcome($"You pocket {reward.Value.DisplayName()}!");
                                return;
                            }
                        }
                        ShowEventOutcome("Nothing useful in the mess. You move on.");
                    };
                    _eventChoice2Action = () =>
                    {
                        _run.Florins += 4;
                        ShowEventOutcome("The grateful sailor presses coin into your hand. +4 Florins.");
                    };
                    break;
                default:
                    ShowEventChoices("The Fortune Teller",
                        "An old woman peers into a crystal ball.\n\"Your fate is not yet written. Choose your path.\"",
                        "Ask about rivals", "Ask about treasure");
                    _eventChoice1Action = () =>
                    {
                        _run.Florins += 3;
                        ShowEventOutcome("She reveals your next opponent's weakness. The knowledge is priceless. +3 Florins.");
                    };
                    _eventChoice2Action = () =>
                    {
                        int gain = 4 + _run.CurrentAct;
                        _run.Florins += gain;
                        ShowEventOutcome($"She reveals a hidden cache nearby. +{gain} Florins.");
                    };
                    break;
            }
        }

        void HandleRest()
        {
            SetPhase(RunPhase.Rest);

            bool hasBurdens = _run.PlayerBurdens.Count > 0;
            int restType = _rng.Next(3);

            if (restType == 0 && hasBurdens)
            {
                ShowEventChoices("The Hearth",
                    "The hearth crackles. You find a quiet moment of peace.",
                    "Mend (remove a burden)", "Rest quietly (+3 Florins)");
                _eventChoice1Action = DoRestMend;
                _eventChoice2Action = DoRestQuietly;
            }
            else if (restType == 1)
            {
                ShowEventChoices("The Study",
                    "You find a quiet corner with old game manuals.\nPerhaps you can learn something new.",
                    "Study (+ability if room)", "Rest quietly (+3 Florins)");
                _eventChoice1Action = () =>
                {
                    if (_run.PlayerAbilities.Count < _run.MaxAbilitySlots)
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
                };
                _eventChoice2Action = DoRestQuietly;
            }
            else
            {
                string desc = hasBurdens
                    ? "The hearth crackles. You find a quiet moment of peace."
                    : "A warm fire and a good meal. Simple comforts.";
                ShowEventChoices("The Hearth", desc,
                    hasBurdens ? "Mend (remove a burden)" : "Rest quietly (+3 Florins)", null);
                _eventChoice1Action = hasBurdens ? DoRestMend : DoRestQuietly;
                _eventChoice2Action = null;
            }
        }

        Action _eventChoice1Action;
        Action _eventChoice2Action;

        void ShowEventChoices(string title, string desc, string choice1, string choice2)
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
                }
            }

            if (EventContinueButton) EventContinueButton.gameObject.SetActive(false);
        }

        void ShowEventOutcome(string outcome)
        {
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

        void DoRumorInvestigate()
        {
            int roll = _rng.Next(100);
            if (roll < 45)
            {
                int bonus = 5 + _run.CurrentAct;
                _run.Florins += bonus;
                ShowEventOutcome($"A hidden purse! +{bonus} Florins.");
            }
            else if (roll < 75 && _run.PlayerAbilities.Count < _run.MaxAbilitySlots)
            {
                var reward = PickAbilityReward(false);
                if (reward.HasValue)
                {
                    _run.PlayerAbilities.Add(reward.Value);
                    _run.RecordAbilityPicked(reward.Value);
                    ShowEventOutcome($"An old gambler teaches you {reward.Value.DisplayName()}!");
                }
                else
                    ShowEventOutcome("The lead goes cold. Nothing gained.");
            }
            else if (roll < 85)
            {
                _run.Florins += 3;
                ShowEventOutcome("A minor tip. +3 Florins.");
            }
            else
            {
                ShowEventOutcome("It was a trap! But you escape unscathed.");
            }
        }

        void DoRumorWalkAway()
        {
            _run.Florins += 2;
            ShowEventOutcome("You keep your head down. +2 Florins for your trouble.");
        }

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
            _run.Florins += 3;
            ShowEventOutcome("A peaceful rest. +3 Florins.");
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
            _run.CurrentMap = MapGenerator.Generate(_run.CurrentAct, _rng);
            _currentColumn = 0;
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
                MapVenueBgImage.sprite = VenueBackgroundSprites[act];
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
                string stats = $"Archetype: {archName}\n" +
                    $"Acts completed: {_run.CurrentAct}/5\n" +
                    $"Matches won: {_run.MatchesWon}/{_run.MatchesPlayed}\n" +
                    $"Florins earned: {_run.Florins}\n" +
                    $"Abilities: {_run.PlayerAbilities.Count}\n" +
                    $"Trinkets: {_run.PlayerTrinkets.Count}\n" +
                    $"\n+{repEarned} Reputation  (Total: {repData.TotalReputation})";
                RunOverStatsLabel.text = stats;
            }
            if (RunOverRestartButton)
            {
                RunOverRestartButton.onClick.RemoveAllListeners();
                RunOverRestartButton.onClick.AddListener(StartNewRun);
            }
        }

        // ---------- Helpers ----------

        AbilityType? PickAbilityReward(bool eliteWeighted)
        {
            var pool = new List<AbilityDefinition>();
            foreach (var def in AbilityPool.All)
            {
                if (_run.PlayerAbilities.Contains(def.Type)) continue;
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
            return baseAmount;
        }
    }
}
