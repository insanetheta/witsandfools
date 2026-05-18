using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools
{
    // Top-bar HUD: turn label, deck count, trump suit, plus end-bout button and game-over panel.
    public sealed class HudView : MonoBehaviour
    {
        public TMP_Text TurnLabel;
        public TMP_Text DeckCountLabel;
        public TMP_Text TrumpLabel;
        public Button EndBoutButton;
        public Button RestartButton;
        public Button AutoPlayButton;
        public GameObject GameOverPanel;
        public TMP_Text GameOverLabel;

        [Header("Trinket Info")]
        public TMP_Text DeckTopLabel;
        public TMP_Text InfoLabel;

        [Header("Tooltip")]
        public TMP_Text TooltipLabel;

        [Header("Ability Choice")]
        public GameObject AbilityChoicePanel;
        public TMP_Text AbilityChoiceLabel;
        public Button PlayNormallyButton;
        public Button UseAbilityButton;

        public void SetTurn(string text) { if (TurnLabel) TurnLabel.text = text; }
        public void SetDeckCount(int n) { if (DeckCountLabel) DeckCountLabel.text = $"Deck: {n}"; }
        public void SetTrump(Suit suit) { if (TrumpLabel) { TrumpLabel.text = $"Trump: {suit.Glyph()}"; TrumpLabel.color = suit.IsRed() ? ThemePalette.RedSuit : ThemePalette.BlackSuit; } }
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

        public void ShowTooltip(string text) { if (TooltipLabel) { TooltipLabel.gameObject.SetActive(true); TooltipLabel.text = text; } }
        public void HideTooltip() { if (TooltipLabel) TooltipLabel.gameObject.SetActive(false); }
    }
}
