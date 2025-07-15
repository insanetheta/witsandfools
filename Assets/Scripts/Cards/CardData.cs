using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WitsAndFools.Cards
{
    /// <summary>
    /// ScriptableObject that defines card data
    /// </summary>
    [CreateAssetMenu(fileName = "New Card", menuName = "Wits and Fools/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Basic Card Information")]
        public string cardName;
        public int value;
        public Core.CardSuit suit;
        public Sprite cardArt;
        
        [Header("Special Abilities")]
        public Core.CardAbilityType abilityType = Core.CardAbilityType.None;
        public string abilityDescription;
        
        [Header("Visual")]
        public Color cardColor = Color.white;
        public Sprite cardBack;
        
        [Header("Audio")]
        public AudioClip playSound;
        public AudioClip abilitySound;
        
        /// <summary>
        /// Check if this card has a special ability
        /// </summary>
        public bool HasAbility => abilityType != Core.CardAbilityType.None;
        
        /// <summary>
        /// Get the display name for this card
        /// </summary>
        public string GetDisplayName()
        {
            if (HasAbility)
                return cardName;
            else
                return $"{value} of {suit}";
        }
        
        /// <summary>
        /// Check if this card can beat another card
        /// </summary>
        /// <param name="otherCard">The card to compare against</param>
        /// <param name="trumpSuit">The current trump suit</param>
        /// <returns>True if this card beats the other card</returns>
        public bool CanBeat(CardData otherCard, Core.CardSuit trumpSuit)
        {
            // Special case for Wildcard
            if (abilityType == Core.CardAbilityType.Wildcard)
                return true;
                
            // Trump cards beat non-trump cards
            if (suit == trumpSuit && otherCard.suit != trumpSuit)
                return true;
                
            // Non-trump cannot beat trump
            if (suit != trumpSuit && otherCard.suit == trumpSuit)
                return false;
                
            // Same suit comparison
            if (suit == otherCard.suit)
                return value > otherCard.value;
                
            // Different suits (neither trump) - cannot beat
            return false;
        }
    }
}