using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools.Cards
{
    /// <summary>
    /// Runtime card representation that handles visual display and interaction
    /// </summary>
    public class Card : MonoBehaviour
    {
        [Header("Card Components")]
        public Image cardImage;
        public Image cardBack;
        public Text cardNameText;
        public Text cardValueText;
        public Text abilityText;
        public Button cardButton;
        
        [Header("Card Data")]
        public CardData cardData;
        
        [Header("Visual States")]
        public bool isRevealed = true;
        public bool isInteractable = true;
        public bool isSelected = false;
        
        // Events
        public System.Action<Card> OnCardClicked;
        public System.Action<Card> OnCardHover;
        
        private Vector3 originalScale;
        
        private void Awake()
        {
            originalScale = transform.localScale;
            
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(OnCardClick);
            }
        }
        
        /// <summary>
        /// Initialize the card with data
        /// </summary>
        /// <param name="data">Card data to display</param>
        public void Initialize(CardData data)
        {
            cardData = data;
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update the visual representation of the card
        /// </summary>
        public void UpdateVisuals()
        {
            if (cardData == null) return;
            
            // Update card art
            if (cardImage != null && cardData.cardArt != null)
            {
                cardImage.sprite = cardData.cardArt;
                cardImage.color = cardData.cardColor;
            }
            
            // Update text elements
            if (cardNameText != null)
            {
                cardNameText.text = cardData.GetDisplayName();
            }
            
            if (cardValueText != null)
            {
                cardValueText.text = cardData.value.ToString();
            }
            
            if (abilityText != null)
            {
                abilityText.text = cardData.abilityDescription;
                abilityText.gameObject.SetActive(cardData.HasAbility);
            }
            
            // Show/hide based on revealed state
            SetRevealed(isRevealed);
        }
        
        /// <summary>
        /// Set whether the card is revealed (face up) or hidden (face down)
        /// </summary>
        /// <param name="revealed">True to show card face, false to show back</param>
        public void SetRevealed(bool revealed)
        {
            isRevealed = revealed;
            
            if (cardImage != null)
                cardImage.gameObject.SetActive(revealed);
            
            if (cardBack != null)
                cardBack.gameObject.SetActive(!revealed);
            
            if (cardNameText != null)
                cardNameText.gameObject.SetActive(revealed);
            
            if (cardValueText != null)
                cardValueText.gameObject.SetActive(revealed);
            
            if (abilityText != null)
                abilityText.gameObject.SetActive(revealed && cardData != null && cardData.HasAbility);
        }
        
        /// <summary>
        /// Set whether the card can be interacted with
        /// </summary>
        /// <param name="interactable">True to allow interaction</param>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            
            if (cardButton != null)
                cardButton.interactable = interactable;
        }
        
        /// <summary>
        /// Set the selected state of the card
        /// </summary>
        /// <param name="selected">True if selected</param>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            // Visual feedback for selection can be added here
        }
        
        /// <summary>
        /// Handle card click
        /// </summary>
        private void OnCardClick()
        {
            if (!isInteractable) return;
            
            OnCardClicked?.Invoke(this);
        }
    }
}