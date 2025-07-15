using System.Collections;
using UnityEngine;
using WitsAndFools.Cards;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Simple demo setup for attack functionality
    /// </summary>
    public class AttackDemo : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(SetupDemo());
        }
        
        IEnumerator SetupDemo()
        {
            UnityEngine.Debug.Log("=== Wits and Fools Attack Demo ===");
            UnityEngine.Debug.Log("Demo starting immediately...");
            
            // Wait for card dealing to complete
            yield return new WaitForSeconds(3f);
            
            // Initialize trump suit
            if (GameRules.Instance != null)
            {
                GameRules.Instance.SetTrumpSuit(CardSuit.Hearts);
                UnityEngine.Debug.Log("Trump suit set to Hearts");
            }
            else
            {
                UnityEngine.Debug.Log("GameRules not found");
            }
            
            // Set up Player 2 cards
            SetupPlayer2Cards();
            
            // Set up click handlers for Player 1
            SetupPlayer1ClickHandlers();
            
            UnityEngine.Debug.Log("=== Demo Instructions ===");
            UnityEngine.Debug.Log("1. Cards are displayed in Player 1's hand at bottom");
            UnityEngine.Debug.Log("2. Player 2's cards are displayed at top");
            UnityEngine.Debug.Log("3. Click Player 1's cards to ATTACK!");
            UnityEngine.Debug.Log("4. Check console for attack results");
            UnityEngine.Debug.Log("5. Cards will appear in center when played");
            
            yield return new WaitForSeconds(1f);
            
            UnityEngine.Debug.Log("Demo ready! Click Player 1's cards to attack Player 2!");
        }
        
        void SetupPlayer2()
        {
            // Find or create Player2 GameObject
            GameObject player2Obj = GameObject.Find("Player2");
            if (player2Obj == null)
            {
                // Create Player 2 if it doesn't exist
                player2Obj = new GameObject("Player2");
                player2Obj.AddComponent<Player>();
                player2Obj.AddComponent<HandManager>();
                UnityEngine.Debug.Log("Created Player2 GameObject with Player and HandManager components");
            }
            
            // Get Player component and initialize
            Player player2 = player2Obj.GetComponent<Player>();
            if (player2 != null && (string.IsNullOrEmpty(player2.playerName) || player2.playerName.Contains("Player ")))
            {
                player2.Initialize(1, "Player 2 (Defender)", PlayerType.Human);
                player2.isDefending = true;
                UnityEngine.Debug.Log("Player 2 initialized as Defender");
            }
            
            // Get HandManager and set up
            HandManager handManager = player2Obj.GetComponent<HandManager>();
            if (handManager != null)
            {
                // Set up hand container
                if (handManager.handContainer == null)
                {
                    Transform handArea = GameObject.Find("Player2HandArea")?.transform;
                    if (handArea != null)
                    {
                        handManager.handContainer = handArea;
                    }
                }
                
                // Set up card prefab
                if (handManager.cardPrefab == null)
                {
                    GameObject cardPrefab = GameObject.Find("CardPrefab");
                    if (cardPrefab != null)
                    {
                        handManager.cardPrefab = cardPrefab;
                    }
                }
                
                // Give Player 2 some defense cards
                GivePlayer2DefenseCards(handManager);
            }
            
            // Update TurnManager
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.InitializeTurnOrder();
                UnityEngine.Debug.Log("TurnManager updated with both players");
            }
        }
        
        void GivePlayer2DefenseCards(HandManager handManager)
        {
            // Create defense cards
            CardData[] defenseCards = new CardData[]
            {
                CreateCard("Royal Guard", CardSuit.Hearts, 10), // Trump defender
                CreateCard("Knight Shield", CardSuit.Spades, 9),
                CreateCard("Court Defender", CardSuit.Diamonds, 8),
                CreateCard("Trump Ace", CardSuit.Hearts, 14), // Strong trump
                CreateCard("Palace Guard", CardSuit.Clubs, 7)
            };
            
            foreach (CardData card in defenseCards)
            {
                handManager.AddCardToHand(card);
                UnityEngine.Debug.Log($"Gave Player 2: {card.GetDisplayName()}");
            }
            
            UnityEngine.Debug.Log($"Player 2 ready with {defenseCards.Length} defense cards!");
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
        
        void SetupPlayer2Cards()
        {
            UnityEngine.Debug.Log("Setting up Player 2 cards...");
            
            // Find the Player2HandArea
            Transform player2HandArea = GameObject.Find("Player2HandArea")?.transform;
            if (player2HandArea == null)
            {
                UnityEngine.Debug.LogError("Player2HandArea not found!");
                return;
            }
            
            // Find the CardPrefab
            GameObject cardPrefab = GameObject.Find("CardPrefab");
            if (cardPrefab == null)
            {
                UnityEngine.Debug.LogError("CardPrefab not found!");
                return;
            }
            
            // Create Player 2 cards
            string[] cardNames = {"Royal Guard", "Shield Bearer", "Court Defender", "Trump Ace", "Palace Guard"};
            CardSuit[] suits = {CardSuit.Hearts, CardSuit.Spades, CardSuit.Diamonds, CardSuit.Hearts, CardSuit.Clubs};
            int[] values = {10, 9, 8, 14, 7};
            
            for (int i = 0; i < cardNames.Length; i++)
            {
                // Create card instance
                GameObject cardObj = Instantiate(cardPrefab, player2HandArea);
                cardObj.name = $"Player2Card_{cardNames[i]}";
                
                // Position the card
                float spacing = 100f;
                float startX = -(cardNames.Length - 1) * spacing / 2f;
                cardObj.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
                
                // Set up card data
                Card cardComponent = cardObj.GetComponent<Card>();
                if (cardComponent != null)
                {
                    CardData cardData = CreateCard(cardNames[i], suits[i], values[i]);
                    cardComponent.cardData = cardData;
                    // Update card display manually
                    if (cardComponent.cardNameText != null)
                        cardComponent.cardNameText.text = cardData.cardName;
                    if (cardComponent.cardValueText != null)
                        cardComponent.cardValueText.text = cardData.value.ToString();
                }
                
                UnityEngine.Debug.Log($"Created Player 2 card: {cardNames[i]}");
            }
        }
        
        void SetupPlayer1ClickHandlers()
        {
            UnityEngine.Debug.Log("Setting up Player 1 click handlers...");
            
            // Find all cards in PlayerHandArea
            Transform player1HandArea = GameObject.Find("PlayerHandArea")?.transform;
            if (player1HandArea == null) 
            {
                UnityEngine.Debug.LogError("PlayerHandArea not found!");
                return;
            }
            
            Card[] player1Cards = player1HandArea.GetComponentsInChildren<Card>();
            foreach (Card card in player1Cards)
            {
                // Add click detection
                var button = card.GetComponent<UnityEngine.UI.Button>();
                if (button == null)
                {
                    button = card.gameObject.AddComponent<UnityEngine.UI.Button>();
                }
                
                // Remove old listeners and add new one
                button.onClick.RemoveAllListeners();
                Card cardRef = card; // Capture for closure
                button.onClick.AddListener(() => OnCardClicked(cardRef));
                
                UnityEngine.Debug.Log($"Added click handler to card: {card.cardData?.cardName ?? "Unknown"}");
            }
        }
        
        void OnCardClicked(Card card)
        {
            UnityEngine.Debug.Log($"=== CARD CLICKED ===");
            UnityEngine.Debug.Log($"Card: {card.cardData?.GetDisplayName() ?? "Unknown Card"}");
            UnityEngine.Debug.Log($"Player 1 is attacking with this card!");
            
            // Simulate attack
            if (AttackDefenseSystem.Instance != null)
            {
                UnityEngine.Debug.Log("Attempting attack through AttackDefenseSystem...");
                // Find the attacking player
                Player attacker = FindFirstObjectByType<Player>();
                if (attacker != null)
                {
                    bool attackSuccess = AttackDefenseSystem.Instance.AttemptAttack(card.cardData, attacker);
                    UnityEngine.Debug.Log($"Attack result: {(attackSuccess ? "SUCCESS" : "FAILED")}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("AttackDefenseSystem not found!");
            }
        }
    }
}