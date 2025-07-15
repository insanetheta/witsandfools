using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Manages player hand UI and card layout
    /// </summary>
    public class HandManager : MonoBehaviour
    {
        [Header("Hand Settings")]
        public Transform handContainer;
        public GameObject cardPrefab;
        public int maxHandSize = 10;
        public float cardSpacing = 350f;
        
        [Header("Layout")]
        public bool arrangeInArc = true;
        public float arcAngle = 60f;
        
        [Header("Animation")]
        public float dealAnimationDuration = 0.5f;
        
        private List<GameObject> cardObjects = new List<GameObject>();
        private Player player;
        
        private void Awake()
        {
            player = GetComponent<Player>();
        }
        
        /// <summary>
        /// Add a card to the hand display
        /// </summary>
        /// <param name="cardData">Card data to add</param>
        public void AddCardToHand(Cards.CardData cardData)
        {
            if (cardObjects.Count >= maxHandSize)
            {
                Debug.LogWarning("Hand is full, cannot add more cards");
                return;
            }
            
            // Create card GameObject
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            cardObjects.Add(cardObj);
            
            // Set up card components
            Cards.Card cardScript = cardObj.GetComponent<Cards.Card>();
            if (cardScript != null)
            {
                // Ensure text references are properly assigned
                if (cardScript.cardNameText == null)
                {
                    Transform nameTransform = cardObj.transform.Find("CardName");
                    if (nameTransform != null)
                        cardScript.cardNameText = nameTransform.GetComponent<Text>();
                }
                
                if (cardScript.cardValueText == null)
                {
                    Transform valueTransform = cardObj.transform.Find("CardValue");
                    if (valueTransform != null)
                        cardScript.cardValueText = valueTransform.GetComponent<Text>();
                }
                
                if (cardScript.cardImage == null)
                {
                    cardScript.cardImage = cardObj.GetComponent<Image>();
                }
                
                cardScript.Initialize(cardData);
                cardScript.OnCardClicked += OnCardClicked;
            }
            
            // Set up card renderer
            Cards.CardRenderer renderer = cardObj.GetComponent<Cards.CardRenderer>();
            if (renderer != null)
            {
                renderer.SetCardData(cardData);
            }
            
            // Arrange cards in hand
            ArrangeCards();
            
            Debug.Log($"Added card to hand: {cardData.GetDisplayName()}");
        }
        
        /// <summary>
        /// Clear all cards from hand
        /// </summary>
        public void ClearHand()
        {
            foreach (GameObject cardObj in cardObjects)
            {
                Cards.Card cardScript = cardObj.GetComponent<Cards.Card>();
                if (cardScript != null)
                {
                    cardScript.OnCardClicked -= OnCardClicked;
                }
                Destroy(cardObj);
            }
            cardObjects.Clear();
            
            Debug.Log("Hand cleared");
        }
        
        /// <summary>
        /// Arrange cards in hand with proper spacing and layout
        /// </summary>
        public void ArrangeCards()
        {
            if (cardObjects.Count == 0) return;
            
            for (int i = 0; i < cardObjects.Count; i++)
            {
                Vector3 targetPosition = CalculateCardPosition(i);
                cardObjects[i].transform.localPosition = targetPosition;
            }
        }
        
        /// <summary>
        /// Calculate position for a card at given index
        /// </summary>
        /// <param name="index">Card index in hand</param>
        /// <returns>Target position</returns>
        private Vector3 CalculateCardPosition(int index)
        {
            if (!arrangeInArc)
            {
                // Simple horizontal layout
                float totalWidth = (cardObjects.Count - 1) * cardSpacing;
                float startX = -totalWidth / 2f;
                return new Vector3(startX + index * cardSpacing, 0, 0);
            }
            else
            {
                // Arc layout - adjust based on whether this is bottom hand (Player 0) or top hand (Player 1)
                bool isBottomHand = handContainer != null && handContainer.name == "PlayerHandArea";
                
                float angleStep = arcAngle / Mathf.Max(1, cardObjects.Count - 1);
                float currentAngle = -arcAngle / 2f + index * angleStep;
                
                float x = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * 350f;
                float y;
                
                if (isBottomHand)
                {
                    // Bottom hand - arc curves upward toward center
                    y = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * 120f - 120f;
                }
                else
                {
                    // Top hand (Player1HandArea) - arc curves downward toward center  
                    y = -Mathf.Cos(currentAngle * Mathf.Deg2Rad) * 120f + 120f;
                }
                
                return new Vector3(x, y, 0);
            }
        }
        
        /// <summary>
        /// Handle card click for attack/defense
        /// </summary>
        /// <param name="card">Clicked card</param>
        private void OnCardClicked(Cards.Card card)
        {
            if (card?.cardData == null) return;
            
            Player currentPlayer = player;
            if (currentPlayer == null) return;
            
            UnityEngine.Debug.Log($"Card clicked: {card.cardData.GetDisplayName()} by {currentPlayer.playerName}");
            
            // Check if it's an attack or defense action
            if (TurnManager.Instance != null)
            {
                if (TurnManager.Instance.currentPhase == TurnPhase.AttackPhase && currentPlayer.isAttacking)
                {
                    // Attempt attack
                    if (AttackDefenseSystem.Instance != null)
                    {
                        bool attackSuccess = AttackDefenseSystem.Instance.AttemptAttack(card.cardData, currentPlayer);
                        if (attackSuccess)
                        {
                            RemoveCardFromHand(card.cardData);
                        }
                    }
                }
                else if (TurnManager.Instance.currentPhase == TurnPhase.DefensePhase && currentPlayer.isDefending)
                {
                    // For defense, we need to know which attack card to defend against
                    // For demo purposes, defend against the first undefended attack card
                    if (AttackDefenseSystem.Instance != null)
                    {
                        var attackCards = AttackDefenseSystem.Instance.GetAttackCards();
                        var defenseCards = AttackDefenseSystem.Instance.GetDefenseCards();
                        
                        if (attackCards.Count > defenseCards.Count)
                        {
                            int attackIndex = defenseCards.Count; // Next attack card to defend
                            bool defenseSuccess = AttackDefenseSystem.Instance.AttemptDefense(card.cardData, attackIndex, currentPlayer);
                            if (defenseSuccess)
                            {
                                RemoveCardFromHand(card.cardData);
                            }
                        }
                    }
                }
            }
            
            // Notify player of card selection
            if (player != null)
            {
                UnityEngine.Debug.Log($"Player {player.playerName} selected card: {card.cardData.GetDisplayName()}");
            }
        }
        
        /// <summary>
        /// Remove a card from hand display
        /// </summary>
        /// <param name="cardData">Card data to remove</param>
        public void RemoveCardFromHand(Cards.CardData cardData)
        {
            for (int i = cardObjects.Count - 1; i >= 0; i--)
            {
                var cardScript = cardObjects[i].GetComponent<Cards.Card>();
                if (cardScript?.cardData == cardData)
                {
                    Destroy(cardObjects[i]);
                    cardObjects.RemoveAt(i);
                    break;
                }
            }
            
            // Rearrange remaining cards
            ArrangeCards();
            
            UnityEngine.Debug.Log($"Removed card from hand display: {cardData.GetDisplayName()}");
        }
        
        /// <summary>
        /// Get number of cards in hand
        /// </summary>
        /// <returns>Number of cards</returns>
        public int GetHandSize()
        {
            return cardObjects.Count;
        }
        
        /// <summary>
        /// Check if hand contains a specific card
        /// </summary>
        /// <param name="cardData">Card data to check for</param>
        /// <returns>True if card is in hand</returns>
        public bool HasCard(Cards.CardData cardData)
        {
            foreach (GameObject cardObj in cardObjects)
            {
                Cards.Card cardScript = cardObj.GetComponent<Cards.Card>();
                if (cardScript?.cardData == cardData)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get all cards in hand as CardData list
        /// </summary>
        /// <returns>List of card data</returns>
        public List<Cards.CardData> GetHandCards()
        {
            List<Cards.CardData> handCards = new List<Cards.CardData>();
            foreach (GameObject cardObj in cardObjects)
            {
                Cards.Card cardScript = cardObj.GetComponent<Cards.Card>();
                if (cardScript?.cardData != null)
                {
                    handCards.Add(cardScript.cardData);
                }
            }
            return handCards;
        }
    }
}
