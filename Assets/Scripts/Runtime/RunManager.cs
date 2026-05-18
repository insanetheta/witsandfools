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

        [Header("Map Panel Refs")]
        public TMP_Text MapTitleLabel;
        public Transform MapNodeContainer;
        public GameObject MapNodeButtonPrefab;

        [Header("Result Panel Refs")]
        public TMP_Text ResultTitleLabel;
        public TMP_Text ResultDetailsLabel;
        public TMP_Text ResultRewardLabel;
        public Button ResultContinueButton;

        [Header("Run Over Panel Refs")]
        public TMP_Text RunOverTitleLabel;
        public TMP_Text RunOverStatsLabel;
        public Button RunOverRestartButton;

        [Header("Run HUD Refs")]
        public TMP_Text PrestigeLabel;
        public TMP_Text FlorinsLabel;
        public TMP_Text ActLabel;
        public TMP_Text AbilitiesLabel;

        RunState _run;
        RunPhase _phase;
        int _currentColumn;
        System.Random _rng;
        MapNode _currentNode;

        void Start()
        {
            if (GameManager) GameManager.AutoStartOnAwake = false;
            StartNewRun();
        }

        public void StartNewRun()
        {
            int seed = Environment.TickCount;
            _rng = new System.Random(seed);
            _run = new RunState { Seed = seed };

            var startingAbilities = PickStartingAbilities();
            _run.PlayerAbilities.AddRange(startingAbilities);

            _run.CurrentAct = 0;
            _run.CurrentMap = MapGenerator.Generate(0, _rng);
            _currentColumn = 0;

            SetPhase(RunPhase.MapSelect);
        }

        void SetPhase(RunPhase phase)
        {
            _phase = phase;
            if (MatchPanel) MatchPanel.SetActive(phase == RunPhase.InMatch);
            if (MapPanel) MapPanel.SetActive(phase == RunPhase.MapSelect);
            if (ResultPanel) ResultPanel.SetActive(phase == RunPhase.PostMatch);
            if (RunOverPanel) RunOverPanel.SetActive(phase == RunPhase.RunOver);
            if (RunHudPanel) RunHudPanel.SetActive(phase != RunPhase.Title && phase != RunPhase.RunOver);

            UpdateRunHud();

            switch (phase)
            {
                case RunPhase.MapSelect:
                    ShowMap();
                    break;
                case RunPhase.RunOver:
                    ShowRunOver();
                    break;
            }
        }

        void UpdateRunHud()
        {
            if (_run == null) return;
            if (PrestigeLabel) PrestigeLabel.text = $"Prestige: {new string('♥', _run.Prestige)}";
            if (FlorinsLabel) FlorinsLabel.text = $"Florins: {_run.Florins}";
            if (ActLabel) ActLabel.text = $"Act {_run.CurrentAct + 1} of 5";
            if (AbilitiesLabel) AbilitiesLabel.text = $"Abilities: {_run.PlayerAbilities.Count}/{_run.MaxAbilitySlots}";
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
            MapNodeType.RivalMatch => new Color(0.55f, 0.20f, 0.20f),
            MapNodeType.EliteMatch => new Color(0.70f, 0.50f, 0.10f),
            MapNodeType.BossMatch => new Color(0.60f, 0.10f, 0.10f),
            MapNodeType.Shop => new Color(0.20f, 0.45f, 0.55f),
            MapNodeType.Rumor => new Color(0.40f, 0.35f, 0.55f),
            MapNodeType.Rest => new Color(0.20f, 0.50f, 0.30f),
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

        void StartMatch(MapNode node)
        {
            var config = MatchSetup.Build(_run, node.Opponent, _rng);
            SetPhase(RunPhase.InMatch);

            GameManager.OnMatchComplete -= OnMatchComplete;
            GameManager.OnMatchComplete += OnMatchComplete;
            GameManager.BeginConfiguredGame(config, node.Opponent.Name, _rng.Next());
        }

        void OnMatchComplete(int winnerIndex)
        {
            GameManager.OnMatchComplete -= OnMatchComplete;
            _run.MatchesPlayed++;

            bool won = winnerIndex == 0;
            int florinsEarned = 0;

            if (won)
            {
                _run.MatchesWon++;
                florinsEarned = CalculateFlorins(_run.CurrentAct, _currentNode.Type);
                _run.Florins += florinsEarned;
            }
            else
            {
                _run.Prestige--;
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

        void ShowResult(bool won, int florinsEarned)
        {
            SetPhase(RunPhase.PostMatch);

            if (ResultTitleLabel)
                ResultTitleLabel.text = won ? "Victory!" : "Defeat...";
            if (ResultDetailsLabel)
            {
                string details = won
                    ? $"You defeated {_currentNode.Opponent?.Name ?? "opponent"}!"
                    : $"{_currentNode.Opponent?.Name ?? "Opponent"} bested you.\nPrestige remaining: {_run.Prestige}";
                ResultDetailsLabel.text = details;
            }
            if (ResultRewardLabel)
            {
                if (won && florinsEarned > 0)
                    ResultRewardLabel.text = $"+{florinsEarned} Florins";
                else
                    ResultRewardLabel.text = "";
            }

            if (ResultContinueButton)
            {
                ResultContinueButton.onClick.RemoveAllListeners();
                ResultContinueButton.onClick.AddListener(OnResultContinue);
            }
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

        // ---------- Non-match encounters (simple for now) ----------

        void HandleShop()
        {
            int florinsEarned = 0;
            if (_run.Florins >= 12 && _run.PlayerAbilities.Count < _run.MaxAbilitySlots)
            {
                var reward = PickAbilityReward(false);
                if (reward.HasValue)
                {
                    _run.PlayerAbilities.Add(reward.Value);
                    _run.Florins -= 12;
                }
            }
            ShowSimpleResult("The Fence", _run.Florins >= 0 ? "You browse the wares..." : "Nothing catches your eye.", "");
        }

        void HandleRumor()
        {
            int roll = _rng.Next(100);
            string desc;
            if (roll < 40)
            {
                int bonus = 5;
                _run.Florins += bonus;
                desc = $"A whispered tip leads to a hidden purse. +{bonus} Florins.";
            }
            else if (roll < 70 && _run.PlayerAbilities.Count < _run.MaxAbilitySlots)
            {
                var reward = PickAbilityReward(false);
                if (reward.HasValue)
                {
                    _run.PlayerAbilities.Add(reward.Value);
                    desc = $"An old gambler teaches you {reward.Value.DisplayName()}.";
                }
                else desc = "The rumor leads nowhere.";
            }
            else
            {
                _run.Florins += 3;
                desc = "A minor lead. +3 Florins.";
            }
            ShowSimpleResult("Rumor", desc, "");
        }

        void HandleRest()
        {
            string desc;
            if (_run.PlayerBurdens.Count > 0 && _rng.Next(100) < 60)
            {
                var removed = _run.PlayerBurdens[_rng.Next(_run.PlayerBurdens.Count)];
                _run.PlayerBurdens.Remove(removed);
                desc = $"You rest by the hearth. {removed.DisplayName()} fades away.";
            }
            else
            {
                desc = "You rest, but nothing changes.";
            }
            ShowSimpleResult("The Hearth", desc, "");
        }

        void ShowSimpleResult(string title, string details, string reward)
        {
            SetPhase(RunPhase.PostMatch);
            if (ResultTitleLabel) ResultTitleLabel.text = title;
            if (ResultDetailsLabel) ResultDetailsLabel.text = details;
            if (ResultRewardLabel) ResultRewardLabel.text = reward;
            if (ResultContinueButton)
            {
                ResultContinueButton.onClick.RemoveAllListeners();
                ResultContinueButton.onClick.AddListener(OnResultContinue);
            }
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
            SetPhase(RunPhase.MapSelect);
        }

        // ---------- Run Over ----------

        void ShowRunOver()
        {
            if (RunOverTitleLabel)
                RunOverTitleLabel.text = _run.RunWon ? "The Circuit is Yours!" : "Your Reputation Crumbles...";
            if (RunOverStatsLabel)
            {
                string stats = $"Acts completed: {_run.CurrentAct}/5\n" +
                    $"Matches won: {_run.MatchesWon}/{_run.MatchesPlayed}\n" +
                    $"Florins earned: {_run.Florins}\n" +
                    $"Abilities: {_run.PlayerAbilities.Count}";
                RunOverStatsLabel.text = stats;
            }
            if (RunOverRestartButton)
            {
                RunOverRestartButton.onClick.RemoveAllListeners();
                RunOverRestartButton.onClick.AddListener(StartNewRun);
            }
        }

        // ---------- Helpers ----------

        List<AbilityType> PickStartingAbilities()
        {
            var pool = new List<AbilityType>(AbilityPool.ActiveAbilities);
            pool.AddRange(AbilityPool.PassiveAbilities);
            var picked = new List<AbilityType>();
            for (int i = 0; i < 4 && pool.Count > 0; i++)
            {
                int idx = _rng.Next(pool.Count);
                picked.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
            return picked;
        }

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
            int baseAmount = 8 + act * 2;
            if (nodeType == MapNodeType.EliteMatch) baseAmount += 4;
            if (nodeType == MapNodeType.BossMatch) baseAmount += 8;
            return baseAmount;
        }
    }
}
