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
        public GameObject GameOverPanel;
        public TMP_Text GameOverLabel;

        public void SetTurn(string text) { if (TurnLabel) TurnLabel.text = text; }
        public void SetDeckCount(int n) { if (DeckCountLabel) DeckCountLabel.text = $"Deck: {n}"; }
        public void SetTrump(Suit suit) { if (TrumpLabel) { TrumpLabel.text = $"Trump: {suit.Glyph()}"; TrumpLabel.color = suit.IsRed() ? new Color(0.85f, 0.20f, 0.20f) : Color.black; } }
        public void SetEndBoutEnabled(bool enabled) { if (EndBoutButton) EndBoutButton.interactable = enabled; }
        public void ShowGameOver(string message)
        {
            if (GameOverPanel) GameOverPanel.SetActive(true);
            if (GameOverLabel) GameOverLabel.text = message;
        }
        public void HideGameOver() { if (GameOverPanel) GameOverPanel.SetActive(false); }
    }
}
