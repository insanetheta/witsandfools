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
            
            // Find all Players and assign to DeckManager
            Core.Player[] allPlayers = FindObjectsByType<Core.Player>(FindObjectsSortMode.None);
            if (allPlayers.Length > 0 && deckManager != null)
            {
                List<Core.Player> validPlayers = new List<Core.Player>();
                
                foreach (Core.Player player in allPlayers)
                {
                    // Always initialize/update player data to ensure correct assignment
                    int playerId = validPlayers.Count; // This will be 0 for first player, 1 for second
                    string playerName = $"Player {playerId}";
                    Core.PlayerType playerType = playerId == 0 ? Core.PlayerType.Human : Core.PlayerType.AI;
                    player.Initialize(playerId, playerName, playerType);
                    
                    // Set up HandManager for this player
                    Core.HandManager playerHandManager = player.GetComponent<Core.HandManager>();
                    if (playerHandManager != null)
                    {
                        // Assign appropriate hand area based on current validPlayers count
                        string handAreaName = validPlayers.Count == 0 ? "PlayerHandArea" : "Player1HandArea";
                        Transform handContainer = GameObject.Find(handAreaName)?.transform;
                        if (handContainer != null)
                        {
                            playerHandManager.handContainer = handContainer;
                            UnityEngine.Debug.Log($"Hand container {handAreaName} assigned to {player.playerName}");
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning($"Hand container {handAreaName} not found for {player.playerName}!");
                        }
                        
                        // Assign card prefab
                        GameObject cardPrefab = GameObject.Find("CardPrefab");
                        if (cardPrefab != null)
                        {
                            playerHandManager.cardPrefab = cardPrefab;
                        }
                    }
                    
                    validPlayers.Add(player);
                    UnityEngine.Debug.Log($"Player {player.playerName} (ID: {player.playerID}) initialized as {player.playerType}");
                }
                
                deckManager.players = validPlayers.ToArray();
                UnityEngine.Debug.Log($"All {validPlayers.Count} players assigned to DeckManager");
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