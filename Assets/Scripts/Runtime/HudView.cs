using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools
{
    // Top-bar HUD: turn label, deck count, trump suit, plus end-bout button and game-over panel.
    public sealed class HudView : MonoBehaviour
    {
        public TMP_Text TurnLabel;
        public TMP_Text DeckCountLabel; // legacy single deck label (unused in dual-deck)
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

        public void SetTurn(string text) { if (TurnLabel) TurnLabel.text = text; }
        public void SetDeckCount(int n) { if (DeckCountLabel) DeckCountLabel.text = $"Deck: {n}"; }
        public void SetDeckCounts(int playerDeck, int opponentDeck)
        {
            if (PlayerDeckCountLabel) PlayerDeckCountLabel.text = $"Deck: {playerDeck}";
            if (OpponentDeckCountLabel) OpponentDeckCountLabel.text = $"Deck: {opponentDeck}";
            if (DeckCountLabel) DeckCountLabel.gameObject.SetActive(false);
        }
        public void SetTrump(Suit suit) { if (TrumpLabel) { TrumpLabel.text = $"Trump: {suit.Glyph()}"; TrumpLabel.color = suit.IsRed() ? ThemePalette.PrestigeRed : Color.white; } }
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
            if (OpponentPortrait && portrait) { OpponentPortrait.sprite = portrait; OpponentPortrait.color = Color.white; }
            else if (OpponentPortrait) OpponentPortrait.color = ThemePalette.WarmSlate;
        }

        public void SetResource(int player, ResourceType type, int amount)
        {
            var label = player == 0 ? PlayerResourceLabel : OpponentResourceLabel;
            if (!label) return;
            label.gameObject.SetActive(true);
            label.text = $"{type.DisplayName()}: {amount}";
            label.color = ResourceColor(type);
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
            if (PlayerHandCount) { PlayerHandCount.gameObject.SetActive(true); PlayerHandCount.text = $"Your Hand: {playerCards}"; }
            if (OpponentHandCount) { OpponentHandCount.gameObject.SetActive(true); OpponentHandCount.text = $"Foe Hand: {opponentCards}"; }
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
