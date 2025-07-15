using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Represents a player in the game
    /// </summary>
    public class Player : MonoBehaviour
    {
        [Header("Player Information")]
        public string playerName;
        public int playerID;
        public PlayerType playerType;
        public Sprite playerAvatar;
        
        [Header("Game State")]
        public bool isActive = false;
        public bool isDefending = false;
        public bool isAttacking = false;
        
        [Header("AI Settings")]
        public int difficultyLevel = 1;
        public string personalityType = "Balanced";
        
        // Components
        private HandManager handManager;
        
        // Events
        public System.Action<Player> OnPlayerTurnStart;
        public System.Action<Player> OnPlayerTurnEnd;
        public System.Action<Player, Cards.CardData> OnCardPlayed;
        
        // Properties - delegate to HandManager
        public int HandSize => handManager?.GetHandSize() ?? 0;
        public bool IsHuman => playerType == PlayerType.Human;
        public bool IsAI => playerType == PlayerType.AI;
        public bool HasCards => HandSize > 0;
        
        private void Awake()
        {
            // Get HandManager component
            handManager = GetComponent<HandManager>();
            if (handManager == null)
            {
                Debug.LogError($"Player {gameObject.name} is missing HandManager component!");
            }
            
            // Initialize player with default values
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = $"Player {playerID}";
            }
        }
        
        /// <summary>
        /// Initialize the player with specific data
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <param name="name">Player name</param>
        /// <param name="type">Player type (Human/AI)</param>
        public void Initialize(int id, string name, PlayerType type)
        {
            playerID = id;
            playerName = name;
            playerType = type;
            
            Debug.Log($"Player {playerName} (ID: {playerID}) initialized as {playerType}");
        }
        
        /// <summary>
        /// Add a card to the player's hand - delegates to HandManager
        /// </summary>
        /// <param name="cardData">Card data to add</param>
        public void AddCardToHand(Cards.CardData cardData)
        {
            if (handManager != null && cardData != null)
            {
                handManager.AddCardToHand(cardData);
                Debug.Log($"{playerName} received card: {cardData.GetDisplayName()}");
            }
        }
        
        /// <summary>
        /// Remove a card from the player's hand - delegates to HandManager
        /// </summary>
        /// <param name="cardData">Card data to remove</param>
        /// <returns>True if card was removed successfully</returns>
        public bool RemoveCardFromHand(Cards.CardData cardData)
        {
            if (handManager != null && handManager.HasCard(cardData))
            {
                handManager.RemoveCardFromHand(cardData);
                Debug.Log($"{playerName} played card: {cardData.GetDisplayName()}");
                OnCardPlayed?.Invoke(this, cardData);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Clear all cards from the player's hand - delegates to HandManager
        /// </summary>
        public void ClearHand()
        {
            if (handManager != null)
            {
                handManager.ClearHand();
                Debug.Log($"{playerName}'s hand cleared");
            }
        }
        
        /// <summary>
        /// Check if player has a specific card - delegates to HandManager
        /// </summary>
        /// <param name="cardData">Card data to check for</param>
        /// <returns>True if player has the card</returns>
        public bool HasCard(Cards.CardData cardData)
        {
            return handManager?.HasCard(cardData) ?? false;
        }
        
        /// <summary>
        /// Get all cards in hand - delegates to HandManager
        /// </summary>
        /// <returns>List of card data</returns>
        public List<Cards.CardData> GetHandCards()
        {
            return handManager?.GetHandCards() ?? new List<Cards.CardData>();
        }
        
        /// <summary>
        /// Get the number of cards in hand - delegates to HandManager
        /// </summary>
        /// <returns>Number of cards</returns>
        public int GetHandSize()
        {
            return handManager?.GetHandSize() ?? 0;
        }
        
        /// <summary>
        /// Check if player can attack with a specific card
        /// </summary>
        /// <param name="cardData">Card to check</param>
        /// <param name="currentAttack">Current attack cards</param>
        /// <returns>True if card can be used for attack</returns>
        public bool CanAttackWith(Cards.CardData cardData, List<Cards.CardData> currentAttack)
        {
            if (!HasCard(cardData)) return false;
            if (!isAttacking) return false;
            
            // Basic validation - detailed rules handled in AttackDefenseSystem
            return true;
        }
        
        /// <summary>
        /// Check if player can defend with a specific card
        /// </summary>
        /// <param name="defenseCard">Card to defend with</param>
        /// <param name="attackCard">Card being defended against</param>
        /// <returns>True if card can be used for defense</returns>
        public bool CanDefendWith(Cards.CardData defenseCard, Cards.CardData attackCard)
        {
            if (!HasCard(defenseCard)) return false;
            if (!isDefending) return false;
            
            // Basic validation - detailed rules handled in AttackDefenseSystem
            return true;
        }
        
        /// <summary>
        /// Start the player's turn
        /// </summary>
        public void StartTurn()
        {
            isActive = true;
            Debug.Log($"{playerName}'s turn started");
            OnPlayerTurnStart?.Invoke(this);
        }
        
        /// <summary>
        /// End the player's turn
        /// </summary>
        public void EndTurn()
        {
            isActive = false;
            isAttacking = false;
            isDefending = false;
            Debug.Log($"{playerName}'s turn ended");
            OnPlayerTurnEnd?.Invoke(this);
        }
        
        /// <summary>
        /// Set the player as the attacker
        /// </summary>
        public void SetAsAttacker()
        {
            isAttacking = true;
            isDefending = false;
            Debug.Log($"{playerName} is now attacking");
        }
        
        /// <summary>
        /// Set the player as the defender
        /// </summary>
        public void SetAsDefender()
        {
            isDefending = true;
            isAttacking = false;
            Debug.Log($"{playerName} is now defending");
        }
        
        /// <summary>
        /// Check if the player has won (no cards left)
        /// </summary>
        /// <returns>True if player has no cards</returns>
        public bool CheckWinCondition()
        {
            bool hasWon = HandSize == 0;
            if (hasWon)
            {
                Debug.Log($"{playerName} has won!");
            }
            return hasWon;
        }
        
        /// <summary>
        /// Get player information as a string
        /// </summary>
        /// <returns>Player info string</returns>
        public override string ToString()
        {
            return $"Player: {playerName} (ID: {playerID}, Type: {playerType}, Cards: {HandSize})";
        }
    }
}
