using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools
{
    // Match HUD: identity panels, race meters, action zone, event log, overlays.
    public sealed class HudView : MonoBehaviour
    {
        public TMP_Text PhaseLabel;
        public TMP_Text TrumpLabel;
        public TMP_Text PlayerDeckCountLabel;
        public TMP_Text OpponentDeckCountLabel;
        public Button EndBoutButton;
        public Button RestartButton;
        public Button AutoPlayButton;
        public GameObject GameOverPanel;
        public TMP_Text GameOverLabel;

        [Header("Opponent Nameplate")]
        public Image OpponentPortrait;
        public TMP_Text OpponentNameLabel;
        public TMP_Text OpponentArchetypeLabel;

        [Header("Bout State")]
        public TMP_Text BoutStateBanner;
        public TMP_Text PlayerHandCount;
        public TMP_Text OpponentHandCount;

        [Header("Trinket Info")]
        public TMP_Text DeckTopLabel;
        public TMP_Text InfoLabel;

        [Header("Resources")]
        public TMP_Text PlayerResourceLabel;
        public TMP_Text OpponentResourceLabel;

        [Header("Ability Feedback")]
        public TMP_Text AbilityFeedbackLabel;

        [Header("Peek Overlay")]
        public GameObject PeekPanel;
        public RectTransform PeekCardContainer;
        public TMP_Text PeekNextDrawLabel;
        public Button PeekDismissButton;

        [Header("Tooltip")]
        public TMP_Text TooltipLabel;

        [Header("Ability Choice")]
        public GameObject AbilityChoicePanel;
        public TMP_Text AbilityChoiceLabel;
        public Button PlayNormallyButton;
        public Button UseAbilityButton;
        public Button CancelAbilityButton;

        [Header("Identity Panels (dual-deck board)")]
        public TMP_Text PlayerNameLabel;
        public TMP_Text PlayerTitleLabel;
        public TMP_Text PlayerPortraitLabel;     // monogram letter
        public TMP_Text OpponentPortraitMonogram;

        [Header("Race Meters")]
        public Image PlayerRaceFill;
        public TMP_Text PlayerRaceLabel;
        public Image OpponentRaceFill;
        public TMP_Text OpponentRaceLabel;

        [Header("Resource Pips")]
        public Image[] PlayerPips;
        public Image[] OpponentPips;

        [Header("Action Zone")]
        public TMP_Text ActionPhaseLine;
        public TMP_Text BoutChipLabel;

        [Header("Event Log")]
        public TMP_Text EventLogText;

        const int MaxLogLines = 5;
        readonly List<string> _logLines = new();

        public void SetDeckCounts(int playerDeck, int opponentDeck)
        {
            if (PlayerDeckCountLabel) PlayerDeckCountLabel.text = $"Deck: {playerDeck}";
            if (OpponentDeckCountLabel) OpponentDeckCountLabel.text = $"Deck: {opponentDeck}";
        }
        public void SetTrump(Suit suit) { if (TrumpLabel) { TrumpLabel.text = $"TRUMP {suit.Glyph()}"; TrumpLabel.color = ThemePalette.Gold; } }
        public void SetEndBoutEnabled(bool enabled) { if (EndBoutButton) EndBoutButton.interactable = enabled; }
        public void ShowGameOver(string message)
        {
            if (GameOverPanel) GameOverPanel.SetActive(true);
            if (GameOverLabel) GameOverLabel.text = message;
        }
        public void HideGameOver() { if (GameOverPanel) GameOverPanel.SetActive(false); }

        public void ShowAbilityChoice(string abilityName, string description, string useLabel,
            bool canPlayNormally = true, bool canUseAbility = true)
        {
            if (AbilityChoicePanel) AbilityChoicePanel.SetActive(true);
            if (AbilityChoiceLabel) AbilityChoiceLabel.text = $"<b>{abilityName}</b>\n{description}";
            if (UseAbilityButton)
            {
                var lbl = UseAbilityButton.GetComponentInChildren<TMP_Text>();
                if (lbl) lbl.text = useLabel;
                UseAbilityButton.interactable = canUseAbility;
            }
            if (PlayNormallyButton) PlayNormallyButton.interactable = canPlayNormally;
        }

        public void HideAbilityChoice() { if (AbilityChoicePanel) AbilityChoicePanel.SetActive(false); }
        public bool AbilityChoiceVisible => AbilityChoicePanel && AbilityChoicePanel.activeSelf;

        public void SetDeckTop(Card? card)
        {
            if (!DeckTopLabel) return;
            if (card.HasValue)
            {
                DeckTopLabel.gameObject.SetActive(true);
                DeckTopLabel.text = $"Top: {card.Value}";
            }
            else
                DeckTopLabel.gameObject.SetActive(false);
        }

        public void SetInfo(string text)
        {
            if (!InfoLabel) return;
            if (string.IsNullOrEmpty(text))
                InfoLabel.gameObject.SetActive(false);
            else
            {
                InfoLabel.gameObject.SetActive(true);
                InfoLabel.text = text;
            }
        }

        public void SetOpponent(string name, string archetype, Sprite portrait, Color archetypeColor)
        {
            if (OpponentNameLabel) OpponentNameLabel.text = name;
            if (OpponentArchetypeLabel) { OpponentArchetypeLabel.text = archetype; OpponentArchetypeLabel.color = archetypeColor; }
            if (OpponentPortrait && portrait)
            {
                OpponentPortrait.sprite = portrait;
                OpponentPortrait.color = Color.white;
                if (OpponentPortraitMonogram) OpponentPortraitMonogram.gameObject.SetActive(false);
            }
            else if (OpponentPortrait)
            {
                OpponentPortrait.color = ThemePalette.WarmSlate;
                if (OpponentPortraitMonogram && !string.IsNullOrEmpty(name))
                {
                    OpponentPortraitMonogram.gameObject.SetActive(true);
                    OpponentPortraitMonogram.text = name[0].ToString();
                }
            }
        }

        public void SetResource(int player, ResourceType type, int amount)
        {
            var label = player == 0 ? PlayerResourceLabel : OpponentResourceLabel;
            if (label)
            {
                label.gameObject.SetActive(true);
                var pips = player == 0 ? PlayerPips : OpponentPips;
                bool hasPips = pips != null && pips.Length > 0 && pips[0];
                label.text = hasPips ? type.DisplayName() : $"{type.DisplayName()}: {amount}";
                label.color = ResourceColor(type);
            }
            SetPips(player, type, amount);
        }

        void SetPips(int player, ResourceType type, int amount)
        {
            var pips = player == 0 ? PlayerPips : OpponentPips;
            if (pips == null) return;
            var fillColor = ResourceColor(type);
            for (int i = 0; i < pips.Length; i++)
            {
                if (!pips[i]) continue;
                bool full = i < amount;
                pips[i].color = full ? fillColor : new Color(0.35f, 0.28f, 0.19f, 0.9f);
            }
        }

        // Race-to-zero meter: fills toward WIN as cards are shed. Same data → same render.
        public void SetRace(int player, int remaining, int startTotal)
        {
            var fill = player == 0 ? PlayerRaceFill : OpponentRaceFill;
            var label = player == 0 ? PlayerRaceLabel : OpponentRaceLabel;
            if (label) label.text = $"{remaining} left";
            if (fill && startTotal > 0)
                fill.fillAmount = Mathf.Clamp01(1f - (float)remaining / startTotal);
        }

        public void SetPlayerIdentity(string name, string subtitle)
        {
            if (PlayerNameLabel) PlayerNameLabel.text = name;
            if (PlayerTitleLabel) PlayerTitleLabel.text = subtitle;
            if (PlayerPortraitLabel && !string.IsNullOrEmpty(name))
                PlayerPortraitLabel.text = name.StartsWith("The ") && name.Length > 4
                    ? name[4].ToString() : name[0].ToString();
        }

        public void SetActionZone(string phaseText, Color color)
        {
            if (!ActionPhaseLine) return;
            ActionPhaseLine.text = phaseText;
            ActionPhaseLine.color = color;
        }

        public void SetBoutChip(int bout, int cap)
        {
            if (!BoutChipLabel) return;
            BoutChipLabel.text = cap > 0 ? $"BOUT {bout}/{cap} · tie → prestige" : $"BOUT {bout}";
        }

        public void PushLog(string line, bool highlight = false)
        {
            if (!EventLogText) return;
            _logLines.Add(line);
            while (_logLines.Count > MaxLogLines) _logLines.RemoveAt(0);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < _logLines.Count; i++)
            {
                bool last = i == _logLines.Count - 1;
                if (last && highlight) sb.Append("<color=#D4A846>").Append(_logLines[i]).Append("</color>");
                else sb.Append(_logLines[i]);
                if (!last) sb.Append('\n');
            }
            EventLogText.text = sb.ToString();
        }

        public void ClearLog()
        {
            _logLines.Clear();
            if (EventLogText) EventLogText.text = "";
        }

        public void HideResource(int player)
        {
            var label = player == 0 ? PlayerResourceLabel : OpponentResourceLabel;
            if (label) label.gameObject.SetActive(false);
        }

        static readonly Color FuryWarm  = new(0.831f, 0.451f, 0.353f);
        static readonly Color IntelSlate = new(0.541f, 0.682f, 0.769f);
        static readonly Color LuckOlive  = new(0.639f, 0.722f, 0.424f);

        static Color ResourceColor(ResourceType r) => r switch
        {
            ResourceType.Intel => IntelSlate,
            ResourceType.Fury => FuryWarm,
            ResourceType.Favor => ThemePalette.Gold,
            ResourceType.Luck => LuckOlive,
            _ => Color.white
        };

        public void SetBoutState(string text, Color bgColor)
        {
            if (!BoutStateBanner) return;
            var panel = BoutStateBanner.transform.parent;
            if (panel != null) panel.gameObject.SetActive(true);
            else BoutStateBanner.gameObject.SetActive(true);
            BoutStateBanner.text = text;
            BoutStateBanner.color = bgColor;
        }
        public void HideBoutState()
        {
            if (!BoutStateBanner) return;
            var panel = BoutStateBanner.transform.parent;
            if (panel != null) panel.gameObject.SetActive(false);
            else BoutStateBanner.gameObject.SetActive(false);
        }

        public void SetHandCounts(int playerCards, int opponentCards)
        {
            if (PlayerHandCount) { PlayerHandCount.gameObject.SetActive(true); PlayerHandCount.text = $"Hand <b>{playerCards}</b>"; }
            if (OpponentHandCount) { OpponentHandCount.gameObject.SetActive(true); OpponentHandCount.text = $"Hand <b>{opponentCards}</b>"; }
        }

        public void ShowTooltip(string text) { if (TooltipLabel) { TooltipLabel.gameObject.SetActive(true); TooltipLabel.text = text; } }
        public void HideTooltip() { if (TooltipLabel) TooltipLabel.gameObject.SetActive(false); }

        public void ShowAbilityFeedback(string text, Color color)
        {
            if (!AbilityFeedbackLabel) return;
            var panel = AbilityFeedbackLabel.transform.parent;
            if (panel) panel.gameObject.SetActive(true);
            else AbilityFeedbackLabel.gameObject.SetActive(true);
            AbilityFeedbackLabel.text = text;
            AbilityFeedbackLabel.color = color;
        }
        public void HideAbilityFeedback()
        {
            if (!AbilityFeedbackLabel) return;
            var panel = AbilityFeedbackLabel.transform.parent;
            if (panel && panel.name == "AbilityFeedbackPanel") panel.gameObject.SetActive(false);
            else AbilityFeedbackLabel.gameObject.SetActive(false);
        }

        public void ShowPeekOverlay() { if (PeekPanel) PeekPanel.SetActive(true); }
        public void HidePeekOverlay()
        {
            if (!PeekPanel) return;
            PeekPanel.SetActive(false);
            if (PeekCardContainer)
                for (int i = PeekCardContainer.childCount - 1; i >= 0; i--)
                    Destroy(PeekCardContainer.GetChild(i).gameObject);
        }
    }
}
