using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Creates demo cards for testing the card system
    /// </summary>
    public class DemoCardCreator : MonoBehaviour
    {
        [Header("Demo Card Creation")]
        public bool createDemoCards = true;
        
        private void Start()
        {
            if (createDemoCards)
            {
                CreateDemoCards();
            }
        }
        
        /// <summary>
        /// Create demo cards programmatically
        /// </summary>
        private void CreateDemoCards()
        {
            // Create demo cards
            var wildcardData = CreateCardData("Wildcard", 1, Core.CardSuit.Clubs, Core.CardAbilityType.Wildcard, "Can be played as any card");
            var shieldData = CreateCardData("Shield Card", 8, Core.CardSuit.Diamonds, Core.CardAbilityType.Shield, "Skip your defense turn");
            var trumpChangerData = CreateCardData("Trump Changer", 4, Core.CardSuit.Hearts, Core.CardAbilityType.TrumpChanger, "Changes the trump suit");
            var reverserData = CreateCardData("The Reverser", 9, Core.CardSuit.Spades, Core.CardAbilityType.Reverser, "Reverses the turn order");
            var normalCard = CreateCardData("Normal Card", 6, Core.CardSuit.Hearts, Core.CardAbilityType.None, "");
            
            // Find DeckManager and assign cards
            Core.DeckManager deckManager = FindFirstObjectByType<Core.DeckManager>();
            if (deckManager != null)
            {
                deckManager.demoCards = new Cards.CardData[] { wildcardData, shieldData, trumpChangerData, reverserData, normalCard };
                UnityEngine.Debug.Log("Demo cards created and assigned to DeckManager");
            }
            
            // Find HandManager and assign container
            Core.HandManager handManager = FindFirstObjectByType<Core.HandManager>();
            if (handManager != null)
            {
                Transform handContainer = GameObject.Find("PlayerHandArea")?.transform;
                if (handContainer != null)
                {
                    handManager.handContainer = handContainer;
                    UnityEngine.Debug.Log("Hand container assigned to HandManager");
                }
                
                // Create a basic card prefab reference
                GameObject cardPrefab = GameObject.Find("CardPrefab");
                if (cardPrefab != null)
                {
                    handManager.cardPrefab = cardPrefab;
                    UnityEngine.Debug.Log("Card prefab assigned to HandManager");
                }
            }
            
            // Find Players by specific GameObject names to ensure correct ID assignment
            List<Core.Player> validPlayers = new List<Core.Player>();
            
            // Find Player0 GameObject and assign ID 0
            GameObject player0GameObject = GameObject.Find("Player0");
            if (player0GameObject != null)
            {
                Core.Player player0 = player0GameObject.GetComponent<Core.Player>();
                if (player0 != null)
                {
                    player0.Initialize(0, "Player 0", Core.PlayerType.Human);
                    
                    // Set up HandManager for Player 0
                    Core.HandManager player0HandManager = player0.GetComponent<Core.HandManager>();
                    if (player0HandManager != null)
                    {
                        Transform handContainer = GameObject.Find("PlayerHandArea")?.transform;
                        if (handContainer != null)
                        {
                            player0HandManager.handContainer = handContainer;
                            UnityEngine.Debug.Log("Hand container PlayerHandArea assigned to Player 0");
                        }
                        
                        GameObject cardPrefab = GameObject.Find("CardPrefab");
                        if (cardPrefab != null)
                        {
                            player0HandManager.cardPrefab = cardPrefab;
                        }
                    }
                    
                    validPlayers.Add(player0);
                    UnityEngine.Debug.Log($"Player {player0.playerName} (ID: {player0.playerID}) initialized as {player0.playerType}");
                }
            }
            
            // Find Player1 GameObject and assign ID 1
            GameObject player1GameObject = GameObject.Find("Player1");
            if (player1GameObject != null)
            {
                Core.Player player1 = player1GameObject.GetComponent<Core.Player>();
                if (player1 != null)
                {
                    player1.Initialize(1, "Player 1", Core.PlayerType.AI);
                    
                    // Set up HandManager for Player 1
                    Core.HandManager player1HandManager = player1.GetComponent<Core.HandManager>();
                    if (player1HandManager != null)
                    {
                        Transform handContainer = GameObject.Find("Player1HandArea")?.transform;
                        if (handContainer != null)
                        {
                            player1HandManager.handContainer = handContainer;
                            UnityEngine.Debug.Log("Hand container Player1HandArea assigned to Player 1");
                        }
                        
                        GameObject cardPrefab = GameObject.Find("CardPrefab");
                        if (cardPrefab != null)
                        {
                            player1HandManager.cardPrefab = cardPrefab;
                        }
                    }
                    
                    validPlayers.Add(player1);
                    UnityEngine.Debug.Log($"Player {player1.playerName} (ID: {player1.playerID}) initialized as {player1.playerType}");
                }
            }
            
            // Assign players to DeckManager
            if (validPlayers.Count > 0 && deckManager != null)
            {
                deckManager.players = validPlayers.ToArray();
                UnityEngine.Debug.Log($"All {validPlayers.Count} players assigned to DeckManager");
            }
            
            // Assign players to TurnManager
            Core.TurnManager turnManager = FindFirstObjectByType<Core.TurnManager>();
            if (turnManager != null && validPlayers.Count > 0)
            {
                turnManager.players = validPlayers;
                UnityEngine.Debug.Log($"All {validPlayers.Count} players assigned to TurnManager");
                
                // Initialize the turn order immediately
                turnManager.InitializeTurnOrder();
            }
            else if (turnManager == null)
            {
                UnityEngine.Debug.LogError("TurnManager not found! Cannot assign players.");
            }
        }
        
        /// <summary>
        /// Create a card data instance
        /// </summary>
        private Cards.CardData CreateCardData(string name, int value, Core.CardSuit suit, Core.CardAbilityType ability, string description)
        {
            Cards.CardData cardData = ScriptableObject.CreateInstance<Cards.CardData>();
            cardData.cardName = name;
            cardData.value = value;
            cardData.suit = suit;
            cardData.abilityType = ability;
            cardData.abilityDescription = description;
            cardData.cardColor = Color.white;
            
            return cardData;
        }
    }
}
