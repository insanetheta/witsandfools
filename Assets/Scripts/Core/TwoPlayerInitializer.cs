using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WitsAndFools.Cards;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Automatically initialize the 2-player demo once both players are set up
    /// </summary>
    public class TwoPlayerInitializer : MonoBehaviour
    {
        [Header("Settings")]
        public float checkInterval = 1f;
        public int cardsPerPlayer = 3;
        
        private bool initialized = false;
        
        void Start()
        {
            StartCoroutine(CheckForTwoPlayers());
        }
        
        IEnumerator CheckForTwoPlayers()
        {
            while (!initialized)
            {
                yield return new WaitForSeconds(checkInterval);
                
                // Find all players in the scene
                Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
                
                if (players.Length >= 2)
                {
                    InitializeTwoPlayerDemo(players);
                    initialized = true;
                }
            }
        }
        
        void InitializeTwoPlayerDemo(Player[] players)
        {
            UnityEngine.Debug.Log("=== Two Player Demo Initialization ===");
            
            // Initialize TurnManager with the players
            if (TurnManager.Instance != null)
            {
                // Set the players directly in the TurnManager
                TurnManager.Instance.players.Clear();
                for (int i = 0; i < Mathf.Min(2, players.Length); i++)
                {
                    TurnManager.Instance.players.Add(players[i]);
                    UnityEngine.Debug.Log($"Added {players[i].playerName} to turn order");
                }
                
                // Initialize the turn order
                TurnManager.Instance.InitializeTurnOrder();
                
                UnityEngine.Debug.Log("Turn manager initialized with 2 players");
            }
            
            // Give Player 2 some cards for testing
            if (players.Length >= 2)
            {
                Player player2 = players[1];
                HandManager player2Hand = player2.GetComponent<HandManager>();
                
                if (player2Hand != null)
                {
                    // Create some demo cards for Player 2
                    for (int i = 0; i < cardsPerPlayer; i++)
                    {
                        CardData demoCard = ScriptableObject.CreateInstance<CardData>();
                        demoCard.cardName = $"Defense Card {i + 1}";
                        demoCard.value = 6 + i;
                        demoCard.suit = (CardSuit)(i % 4);
                        
                        player2Hand.AddCardToHand(demoCard);
                    }
                    
                    UnityEngine.Debug.Log($"Gave {cardsPerPlayer} cards to Player 2");
                }
            }
            
            // Set initial game state
            if (GameRules.Instance != null)
            {
                GameRules.Instance.SetTrumpSuit(CardSuit.Hearts);
                UnityEngine.Debug.Log("Trump suit set to Hearts for demo");
            }
            
            UnityEngine.Debug.Log("=== Demo Ready! ===");
            UnityEngine.Debug.Log("Player 1: Click cards at bottom to attack");
            UnityEngine.Debug.Log("Player 2: Click cards at top to defend");
            UnityEngine.Debug.Log("Watch center area for played cards");
        }
    }
}