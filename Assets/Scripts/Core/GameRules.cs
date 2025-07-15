using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WitsAndFools.Cards;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Core game rules implementation for Wits and Fools
    /// Based on Durak-inspired mechanics with special abilities
    /// </summary>
    public class GameRules : MonoBehaviour
    {
        [Header("Game Settings")]
        public int initialHandSize = 5;
        public int maxPlayersPerRound = 5;
        public int minPlayersPerRound = 2;
        
        [Header("Trump System")]
        public CardSuit trumpSuit = CardSuit.Hearts;
        public bool trumpSuitSet = false;
        
        [Header("Attack/Defense")]
        public int maxAttackCards = 6;
        public bool multipleAttacksAllowed = true;
        
        // Singleton for easy access
        public static GameRules Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Check if a card can beat another card based on Durak rules
        /// </summary>
        /// <param name="attackCard">The attacking card</param>
        /// <param name="defenseCard">The defending card</param>
        /// <returns>True if defense card beats attack card</returns>
        public bool CanDefendWith(CardData attackCard, CardData defenseCard)
        {
            // Trump cards beat non-trump cards
            if (trumpSuitSet)
            {
                bool attackIsTrump = attackCard.suit == trumpSuit;
                bool defenseIsTrump = defenseCard.suit == trumpSuit;
                
                if (defenseIsTrump && !attackIsTrump)
                {
                    return true; // Trump beats any non-trump
                }
                
                if (attackIsTrump && !defenseIsTrump)
                {
                    return false; // Non-trump cannot beat trump
                }
            }
            
            // Same suit: higher value wins
            if (attackCard.suit == defenseCard.suit)
            {
                return defenseCard.value > attackCard.value;
            }
            
            // Different suits (both non-trump): defense fails
            return false;
        }
        
        /// <summary>
        /// Check if a card can be used to attack a defending player
        /// </summary>
        /// <param name="attackCard">Card being played for attack</param>
        /// <param name="currentAttackCards">Cards already in the attack</param>
        /// <returns>True if card can be added to attack</returns>
        public bool CanAttackWith(CardData attackCard, List<CardData> currentAttackCards)
        {
            // First attack card is always valid
            if (currentAttackCards.Count == 0)
            {
                return true;
            }
            
            // Additional attack cards must match value of existing cards in the bout
            foreach (CardData existingCard in currentAttackCards)
            {
                if (attackCard.value == existingCard.value)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Set the trump suit for the current game
        /// </summary>
        /// <param name="suit">Trump suit to set</param>
        public void SetTrumpSuit(CardSuit suit)
        {
            trumpSuit = suit;
            trumpSuitSet = true;
            UnityEngine.Debug.Log($"Trump suit set to: {suit}");
        }
        
        /// <summary>
        /// Check if the attack phase is complete
        /// </summary>
        /// <param name="attackCards">Cards used in attack</param>
        /// <param name="defenseCards">Cards used in defense</param>
        /// <returns>True if all attacks have been defended or attack limit reached</returns>
        public bool IsAttackPhaseComplete(List<CardData> attackCards, List<CardData> defenseCards)
        {
            // Attack complete if all attacks defended
            if (attackCards.Count > 0 && defenseCards.Count == attackCards.Count)
            {
                return true;
            }
            
            // Attack complete if maximum cards reached
            if (attackCards.Count >= maxAttackCards)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a player has won the game
        /// </summary>
        /// <param name="player">Player to check</param>
        /// <returns>True if player has won</returns>
        public bool HasPlayerWon(Player player)
        {
            // Player wins if they have no cards (simplified for demo)
            return player.GetHandSize() == 0;
        }
        
        /// <summary>
        /// Get the card values that can be used for additional attacks
        /// </summary>
        /// <param name="attackCards">Current attack cards</param>
        /// <param name="defenseCards">Current defense cards</param>
        /// <returns>List of values that can be used for additional attacks</returns>
        public List<int> GetValidAttackValues(List<CardData> attackCards, List<CardData> defenseCards)
        {
            HashSet<int> validValues = new HashSet<int>();
            
            // Add values from attack cards
            foreach (CardData card in attackCards)
            {
                validValues.Add(card.value);
            }
            
            // Add values from defense cards
            foreach (CardData card in defenseCards)
            {
                validValues.Add(card.value);
            }
            
            return new List<int>(validValues);
        }
    }
}
