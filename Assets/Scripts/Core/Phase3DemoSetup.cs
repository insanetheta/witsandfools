using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WitsAndFools.Cards;
using WitsAndFools.Core;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Sets up Phase 3 demo: 2 players with attack mechanics
    /// </summary>
    public class Phase3DemoSetup : MonoBehaviour
    {
        [Header("Demo Settings")]
        public int cardsPerPlayer = 5;
        public bool autoStartDemo = true;
        
        private Player player1;
        private Player player2;
        
        void Start()
        {
            if (autoStartDemo)
            {
                StartCoroutine(SetupPhase3Demo());
            }
        }
        
        IEnumerator SetupPhase3Demo()
        {
            UnityEngine.Debug.Log("=== Phase 3 Demo Setup Starting ===");
            
            // Wait a moment for other systems to initialize
            yield return new WaitForSeconds(1f);
            
            // Setup game rules and trump suit
            SetupGameRules();
            
            // Setup two players
            SetupPlayers();
            
            // Deal cards to both players
            yield return StartCoroutine(DealCardsToPlayers());
            
            // Initialize turn manager
            SetupTurnSystem();
            
            // Setup attack/defense system
            SetupAttackDefenseSystem();
            
            UnityEngine.Debug.Log("=== Phase 3 Demo Ready ===");
            UnityEngine.Debug.Log($"Player 1 ({player1.playerName}) can attack Player 2 ({player2.playerName})");
            UnityEngine.Debug.Log("Click on Player 1's cards to attack!");
        }
        
        void SetupGameRules()
        {
            // Create GameRules if it doesn't exist
            if (GameRules.Instance == null)
            {
                GameObject rulesObj = new GameObject("GameRules");
                rulesObj.AddComponent<GameRules>();
            }
            
            // Set trump suit for demo
            GameRules.Instance.SetTrumpSuit(CardSuit.Hearts);
            UnityEngine.Debug.Log("Game rules initialized with Hearts as trump suit");
        }
        
        void SetupPlayers()
        {
            // Find existing players or create them
            Player[] existingPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
            
            if (existingPlayers.Length >= 2)
            {
                player1 = existingPlayers[0];
                player2 = existingPlayers[1];
            }
            else
            {
                // Create Player 1
                if (existingPlayers.Length >= 1)
                {
                    player1 = existingPlayers[0];
                }
                else
                {
                    GameObject p1Obj = new GameObject("Player1");
                    player1 = p1Obj.AddComponent<Player>();
                }
                
                // Create Player 2
                GameObject p2Obj = new GameObject("Player2");
                player2 = p2Obj.AddComponent<Player>();
            }
            
            // Initialize players
            player1.Initialize(0, "Player 1", PlayerType.Human);
            player2.Initialize(1, "Player 2", PlayerType.Human);
            
            // Set initial states
            player1.SetAsAttacker();
            player2.SetAsDefender();
            
            UnityEngine.Debug.Log($"Players setup: {player1.playerName} (Attacker) vs {player2.playerName} (Defender)");
        }
        
        IEnumerator DealCardsToPlayers()
        {
            // Create demo cards
            List<CardData> demoCards = CreateDemoCards();
            
            // Deal cards to each player
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                if (i < demoCards.Count)
                {
                    player1.AddCardToHand(demoCards[i]);
                }
                
                if (i + cardsPerPlayer < demoCards.Count)
                {
                    player2.AddCardToHand(demoCards[i + cardsPerPlayer]);
                }
                
                yield return new WaitForSeconds(0.1f); // Small delay for visual effect
            }
            
            UnityEngine.Debug.Log($"Dealt {cardsPerPlayer} cards to each player");
        }
        
        List<CardData> CreateDemoCards()
        {
            List<CardData> cards = new List<CardData>();
            
            // Create a variety of cards for demo
            // Player 1 cards (attacking)
            cards.Add(CreateCard("7 of Hearts", CardSuit.Hearts, 7, CardAbilityType.None));
            cards.Add(CreateCard("9 of Spades", CardSuit.Spades, 9, CardAbilityType.None));
            cards.Add(CreateCard("Jack of Clubs", CardSuit.Clubs, 11, CardAbilityType.None));
            cards.Add(CreateCard("Queen of Diamonds", CardSuit.Diamonds, 12, CardAbilityType.None));
            cards.Add(CreateCard("King of Hearts", CardSuit.Hearts, 13, CardAbilityType.None));
            
            // Player 2 cards (defending)
            cards.Add(CreateCard("8 of Hearts", CardSuit.Hearts, 8, CardAbilityType.None));
            cards.Add(CreateCard("10 of Spades", CardSuit.Spades, 10, CardAbilityType.None));
            cards.Add(CreateCard("Ace of Clubs", CardSuit.Clubs, 14, CardAbilityType.None));
            cards.Add(CreateCard("King of Diamonds", CardSuit.Diamonds, 13, CardAbilityType.None));
            cards.Add(CreateCard("Ace of Hearts", CardSuit.Hearts, 14, CardAbilityType.None));
            
            return cards;
        }
        
        CardData CreateCard(string name, CardSuit suit, int value, CardAbilityType ability)
        {
            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = name;
            card.suit = suit;
            card.value = value;
            card.abilityType = ability;
            return card;
        }
        
        void SetupTurnSystem()
        {
            // Create TurnManager if it doesn't exist
            if (TurnManager.Instance == null)
            {
                GameObject turnObj = new GameObject("TurnManager");
                turnObj.AddComponent<TurnManager>();
            }
            
            // Initialize turn system with our players
            TurnManager.Instance.players.Clear();
            TurnManager.Instance.players.Add(player1);
            TurnManager.Instance.players.Add(player2);
            TurnManager.Instance.attackerIndex = 0;  // Player 1 attacks
            TurnManager.Instance.defenderIndex = 1; // Player 2 defends
            TurnManager.Instance.currentPlayerIndex = 0;
            
            TurnManager.Instance.StartAttackPhase();
            
            UnityEngine.Debug.Log("Turn system initialized - Player 1's turn to attack");
        }
        
        void SetupAttackDefenseSystem()
        {
            // Create AttackDefenseSystem if it doesn't exist
            if (AttackDefenseSystem.Instance == null)
            {
                GameObject attackObj = new GameObject("AttackDefenseSystem");
                attackObj.AddComponent<AttackDefenseSystem>();
            }
            
            UnityEngine.Debug.Log("Attack/Defense system initialized");
        }
    }
}
