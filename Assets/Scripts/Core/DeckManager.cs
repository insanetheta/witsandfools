using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Manages deck creation and card dealing for demo purposes
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        [Header("Demo Settings")]
        public Cards.CardData[] demoCards;
        public int cardsPerHand = 5;
        
        [Header("Players")]
        public Player[] players;
        
        private void Start()
        {
            // Wait a frame for other systems to initialize
            StartCoroutine(DealInitialHands());
        }
        
        /// <summary>
        /// Deal initial hands to all players for demo
        /// </summary>
        private IEnumerator DealInitialHands()
        {
            yield return new WaitForSeconds(1f);
            
            if (demoCards == null || demoCards.Length == 0)
            {
                UnityEngine.Debug.LogWarning("No demo cards available");
                yield break;
            }
            
            UnityEngine.Debug.Log("Dealing initial hands...");
            
            foreach (Player player in players)
            {
                if (player != null)
                {
                    HandManager handManager = player.GetComponent<HandManager>();
                    if (handManager != null)
                    {
                        // Deal cards to this player
                        for (int i = 0; i < cardsPerHand; i++)
                        {
                            // Get random card from demo cards
                            Cards.CardData cardData = demoCards[Random.Range(0, demoCards.Length)];
                            handManager.AddCardToHand(cardData);
                            
                            // Add small delay between cards for visual effect
                            yield return new WaitForSeconds(0.2f);
                        }
                        
                        UnityEngine.Debug.Log($"Dealt {cardsPerHand} cards to {player.playerName}");
                    }
                }
            }
            
            UnityEngine.Debug.Log("Initial dealing complete!");
        }
        
        /// <summary>
        /// Create demo card data if none exists
        /// </summary>
        public void CreateDemoCards()
        {
            if (demoCards == null || demoCards.Length == 0)
            {
                UnityEngine.Debug.Log("Creating demo cards...");
                // This would normally create ScriptableObject instances
                // For now, we'll just log that demo cards need to be created
                UnityEngine.Debug.LogWarning("Demo cards need to be created as ScriptableObjects");
            }
        }
    }
}