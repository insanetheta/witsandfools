using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WitsAndFools.Core;
using WitsAndFools.Cards;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Complete Phase 3 demo: 2 players with attack mechanics
    /// </summary>
    public class TwoPlayerAttackDemo : MonoBehaviour
    {
        [Header("Demo Players")]
        public GameObject player1Object;
        public GameObject player2Object;
        
        [Header("Hand Areas")]
        public Transform player1HandArea;
        public Transform player2HandArea;
        
        [Header("Demo Settings")]
        public int cardsPerPlayer = 5;
        
        private Player player1;
        private Player player2;
        private HandManager player1HandManager;
        private HandManager player2HandManager;
        
        void Start()
        {
            StartCoroutine(SetupTwoPlayerDemo());
        }
        
        IEnumerator SetupTwoPlayerDemo()
        {
            UnityEngine.Debug.Log("=== Setting up 2-Player Attack Demo ===");
            
            yield return new WaitForSeconds(0.5f);
            
            // Step 1: Setup game systems
            SetupGameSystems();
            
            // Step 2: Create or find players
            SetupPlayers();
            
            // Step 3: Setup hand managers
            SetupHandManagers();
            
            // Step 4: Deal cards to both players
            yield return StartCoroutine(DealDemoCards());
            
            // Step 5: Initialize turn system
            SetupTurnSystem();
            
            // Step 6: Display instructions
            ShowDemoInstructions();
            
            UnityEngine.Debug.Log("=== 2-Player Attack Demo Ready! ===");
        }
        
        void SetupGameSystems()
        {
            // Create GameRules
            if (GameRules.Instance == null)
            {
                GameObject rulesObj = new GameObject("GameRules");
                rulesObj.AddComponent<GameRules>();
            }
            GameRules.Instance.SetTrumpSuit(CardSuit.Hearts);
            
            // Create TurnManager
            if (TurnManager.Instance == null)
            {
                GameObject turnObj = new GameObject("TurnManager");
                turnObj.AddComponent<TurnManager>();
            }
            
            // Create AttackDefenseSystem
            if (AttackDefenseSystem.Instance == null)
            {
                GameObject attackObj = new GameObject("AttackDefenseSystem");
                attackObj.AddComponent<AttackDefenseSystem>();
            }
            
            UnityEngine.Debug.Log("Game systems initialized");
        }
        
        void SetupPlayers()
        {
            // Find hand areas
            if (player1HandArea == null)
            {
                player1HandArea = GameObject.Find("PlayerHandArea")?.transform;
            }
            if (player2HandArea == null)
            {
                player2HandArea = GameObject.Find("Player2HandArea")?.transform;
            }
            
            // Create Player 1
            if (player1Object == null)
            {
                player1Object = new GameObject("Player1");
            }
            player1 = player1Object.GetComponent<Player>();
            if (player1 == null)
            {
                player1 = player1Object.AddComponent<Player>();
            }
            player1.Initialize(0, "Player 1 (You)", PlayerType.Human);
            
            // Create Player 2
            if (player2Object == null)
            {
                player2Object = new GameObject("Player2");
            }
            player2 = player2Object.GetComponent<Player>();
            if (player2 == null)
            {
                player2 = player2Object.AddComponent<Player>();
            }
            player2.Initialize(1, "Player 2 (Opponent)", PlayerType.Human);
            
            UnityEngine.Debug.Log("Players created: " + player1.playerName + " vs " + player2.playerName);
        }
        
        void SetupHandManagers()
        {
            // Setup Player 1 Hand Manager
            player1HandManager = player1Object.GetComponent<HandManager>();
            if (player1HandManager == null)
            {
                player1HandManager = player1Object.AddComponent<HandManager>();
            }
            
            if (player1HandArea != null)
            {
                player1HandManager.handContainer = player1HandArea;
                GameObject cardPrefab = GameObject.Find("CardPrefab");
                if (cardPrefab != null)
                {
                    player1HandManager.cardPrefab = cardPrefab;
                }
            }
            
            // Setup Player 2 Hand Manager (simplified - no interaction needed for demo)
            player2HandManager = player2Object.GetComponent<HandManager>();
            if (player2HandManager == null)
            {
                player2HandManager = player2Object.AddComponent<HandManager>();
            }
            
            if (player2HandArea != null)
            {
                player2HandManager.handContainer = player2HandArea;
                GameObject cardPrefab = GameObject.Find("CardPrefab");
                if (cardPrefab != null)
                {
                    player2HandManager.cardPrefab = cardPrefab;
                }
            }
            
            UnityEngine.Debug.Log("Hand managers setup complete");
        }
        
        IEnumerator DealDemoCards()
        {
            List<CardData> demoCards = CreateDemoCards();
            
            // Deal cards to Player 1
            for (int i = 0; i < cardsPerPlayer && i < demoCards.Count; i++)
            {
                CardData card = demoCards[i];
                player1.AddCardToHand(card);
                if (player1HandManager != null)
                {
                    player1HandManager.AddCardToHand(card);
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            // Deal cards to Player 2
            for (int i = cardsPerPlayer; i < cardsPerPlayer * 2 && i < demoCards.Count; i++)
            {
                CardData card = demoCards[i];
                player2.AddCardToHand(card);
                if (player2HandManager != null)
                {
                    player2HandManager.AddCardToHand(card);
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            UnityEngine.Debug.Log($"Dealt {cardsPerPlayer} cards to each player");
        }
        
        List<CardData> CreateDemoCards()
        {
            List<CardData> cards = new List<CardData>();
            
            // Player 1 cards (can attack with these)
            cards.Add(CreateCard("7 of Hearts", CardSuit.Hearts, 7));
            cards.Add(CreateCard("9 of Spades", CardSuit.Spades, 9));
            cards.Add(CreateCard("Jack of Clubs", CardSuit.Clubs, 11));
            cards.Add(CreateCard("Queen of Diamonds", CardSuit.Diamonds, 12));
            cards.Add(CreateCard("King of Hearts", CardSuit.Hearts, 13));
            
            // Player 2 cards (can defend with these)
            cards.Add(CreateCard("8 of Hearts", CardSuit.Hearts, 8));
            cards.Add(CreateCard("10 of Spades", CardSuit.Spades, 10));
            cards.Add(CreateCard("Ace of Clubs", CardSuit.Clubs, 14));
            cards.Add(CreateCard("King of Diamonds", CardSuit.Diamonds, 13));
            cards.Add(CreateCard("Ace of Hearts", CardSuit.Hearts, 14));
            
            return cards;
        }
        
        CardData CreateCard(string name, CardSuit suit, int value)
        {
            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = name;
            card.suit = suit;
            card.value = value;
            card.abilityType = CardAbilityType.None;
            return card;
        }
        
        void SetupTurnSystem()
        {
            // Initialize turn system
            TurnManager.Instance.players.Clear();
            TurnManager.Instance.players.Add(player1);
            TurnManager.Instance.players.Add(player2);
            
            // Set Player 1 as attacker, Player 2 as defender
            TurnManager.Instance.attackerIndex = 0;
            TurnManager.Instance.defenderIndex = 1;
            TurnManager.Instance.currentPlayerIndex = 0;
            
            // Set player states
            player1.SetAsAttacker();
            player2.SetAsDefender();
            
            // Start the attack phase
            TurnManager.Instance.StartAttackPhase();
            
            UnityEngine.Debug.Log("Turn system initialized - Player 1 can attack Player 2");
        }
        
        void ShowDemoInstructions()
        {
            UnityEngine.Debug.Log("╔══════════════════════════════════════════════════════════════════════════════╗");
            UnityEngine.Debug.Log("║                           PHASE 3 DEMO INSTRUCTIONS                         ║");
            UnityEngine.Debug.Log("╠══════════════════════════════════════════════════════════════════════════════╣");
            UnityEngine.Debug.Log("║ • Player 1 (bottom) is the ATTACKER                                         ║");
            UnityEngine.Debug.Log("║ • Player 2 (top) is the DEFENDER                                            ║");
            UnityEngine.Debug.Log("║ • Trump suit is HEARTS ♥                                                    ║");
            UnityEngine.Debug.Log("║                                                                              ║");
            UnityEngine.Debug.Log("║ TO ATTACK:                                                                   ║");
            UnityEngine.Debug.Log("║ 1. Click any card in Player 1's hand (bottom)                               ║");
            UnityEngine.Debug.Log("║ 2. The card will appear in the center attack area                           ║");
            UnityEngine.Debug.Log("║ 3. Game will switch to defense phase for Player 2                           ║");
            UnityEngine.Debug.Log("║                                                                              ║");
            UnityEngine.Debug.Log("║ DEFENSE RULES:                                                               ║");
            UnityEngine.Debug.Log("║ • Same suit + higher value beats attack card                                 ║");
            UnityEngine.Debug.Log("║ • Trump card (Hearts) beats any non-trump card                              ║");
            UnityEngine.Debug.Log("║ • Higher trump beats lower trump                                             ║");
            UnityEngine.Debug.Log("╚══════════════════════════════════════════════════════════════════════════════╝");
        }
    }
}
