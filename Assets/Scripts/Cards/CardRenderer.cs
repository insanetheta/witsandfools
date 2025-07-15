using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools.Cards
{
    /// <summary>
    /// Handles rendering and visual representation of cards
    /// </summary>
    public class CardRenderer : MonoBehaviour
    {
        [Header("Card Visual Components")]
        public Image cardBackground;
        public Image cardArt;
        public Image cardBack;
        public Image suitIcon;
        public Text cardNameText;
        public Text cardValueText;
        public Text abilityDescriptionText;
        public GameObject abilityPanel;
        
        [Header("Visual Settings")]
        public Color[] suitColors = new Color[4];
        public Sprite[] suitSprites = new Sprite[4];
        public Sprite defaultCardBack;
        public Color defaultBackgroundColor = Color.white;
        
        private CardData cardData;
        private bool isRevealed = true;
        
        /// <summary>
        /// Initialize the renderer with card data
        /// </summary>
        /// <param name="data">Card data to render</param>
        public void SetCardData(CardData data)
        {
            cardData = data;
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update all visual elements based on card data
        /// </summary>
        public void UpdateVisuals()
        {
            if (cardData == null) return;
            
            // Set card background color
            if (cardBackground != null)
            {
                cardBackground.color = cardData.cardColor != Color.white ? cardData.cardColor : defaultBackgroundColor;
            }
            
            // Set card art
            if (cardArt != null)
            {
                cardArt.sprite = cardData.cardArt;
                cardArt.gameObject.SetActive(cardData.cardArt != null && isRevealed);
            }
            
            // Set card back
            if (cardBack != null)
            {
                cardBack.sprite = cardData.cardBack != null ? cardData.cardBack : defaultCardBack;
                cardBack.gameObject.SetActive(!isRevealed);
            }
            
            // Set suit icon
            if (suitIcon != null && isRevealed)
            {
                int suitIndex = (int)cardData.suit;
                if (suitIndex >= 0 && suitIndex < suitSprites.Length)
                {
                    suitIcon.sprite = suitSprites[suitIndex];
                    suitIcon.color = suitIndex < suitColors.Length ? suitColors[suitIndex] : Color.white;
                }
                suitIcon.gameObject.SetActive(true);
            }
            else if (suitIcon != null)
            {
                suitIcon.gameObject.SetActive(false);
            }
            
            // Set card name
            if (cardNameText != null)
            {
                cardNameText.text = cardData.GetDisplayName();
                cardNameText.gameObject.SetActive(isRevealed);
            }
            
            // Set card value
            if (cardValueText != null)
            {
                cardValueText.text = cardData.value.ToString();
                cardValueText.gameObject.SetActive(isRevealed);
            }
            
            // Set ability description
            if (abilityDescriptionText != null)
            {
                abilityDescriptionText.text = cardData.abilityDescription;
                abilityDescriptionText.gameObject.SetActive(isRevealed && cardData.HasAbility);
            }
            
            // Show/hide ability panel
            if (abilityPanel != null)
            {
                abilityPanel.SetActive(isRevealed && cardData.HasAbility);
            }
        }
        
        /// <summary>
        /// Set whether the card is revealed (face up) or hidden (face down)
        /// </summary>
        /// <param name="revealed">True to show card face</param>
        public void SetRevealed(bool revealed)
        {
            isRevealed = revealed;
            UpdateVisuals();
        }
        
        /// <summary>
        /// Get the current card data
        /// </summary>
        /// <returns>Current card data</returns>
        public CardData GetCardData()
        {
            return cardData;
        }
        
        /// <summary>
        /// Check if card is currently revealed
        /// </summary>
        /// <returns>True if card face is showing</returns>
        public bool IsRevealed()
        {
            return isRevealed;
        }
    }
}