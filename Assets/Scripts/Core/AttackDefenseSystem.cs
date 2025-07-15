using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WitsAndFools.Cards;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Manages attack and defense mechanics for the card game
    /// </summary>
    public class AttackDefenseSystem : MonoBehaviour
    {
        [Header("Current Bout")]
        public List<CardData> attackCards = new List<CardData>();
        public List<CardData> defenseCards = new List<CardData>();
        public bool boutActive = false;
        
        [Header("Attack Settings")]
        public Transform attackCardArea;
        public Transform defenseCardArea;
        public float cardSpacing = 120f;
        
        [Header("Events")]
        public UnityEvent<CardData, Player> OnCardAttacked;
        public UnityEvent<CardData, Player> OnCardDefended;
        public UnityEvent<bool> OnBoutComplete; // true if defense successful
        public UnityEvent OnAttackAreaCleared;
        
        // Singleton pattern
        public static AttackDefenseSystem Instance { get; private set; }
        
        // Visual card objects for the attack/defense area
        private List<GameObject> attackCardObjects = new List<GameObject>();
        private List<GameObject> defenseCardObjects = new List<GameObject>();
        
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
        
        private void Start()
        {
            CleanupDuplicateAreas();
            SetupAttackDefenseAreas();
        }
        
        /// <summary>
        /// Clean up any duplicate attack/defense areas from previous sessions
        /// </summary>
        private void CleanupDuplicateAreas()
        {
            // Find all GameObjects and check for duplicates
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "AttackCardArea" && obj != gameObject)
                {
                    UnityEngine.Debug.Log($"Removing duplicate AttackCardArea: {obj.name}");
                    Destroy(obj);
                }
                else if (obj.name == "DefenseCardArea" && obj != gameObject)
                {
                    UnityEngine.Debug.Log($"Removing duplicate DefenseCardArea: {obj.name}");
                    Destroy(obj);
                }
            }
            
            UnityEngine.Debug.Log("Duplicate area cleanup complete");
        }
        
        /// <summary>
        /// Setup the visual areas for attack and defense cards
        /// </summary>
        private void SetupAttackDefenseAreas()
        {
            // Find existing attack and defense areas in the scene
            if (attackCardArea == null)
            {
                GameObject existingAttackArea = GameObject.Find("AttackArea");
                if (existingAttackArea != null)
                {
                    attackCardArea = existingAttackArea.transform;
                    UnityEngine.Debug.Log("Using existing AttackArea for attack cards");
                }
                else
                {
                    // Create new area only if existing one is not found
                    GameObject attackArea = new GameObject("AttackCardArea");
                    attackArea.transform.SetParent(GameObject.Find("UICanvas")?.transform);
                    attackCardArea = attackArea.transform;
                    attackCardArea.localPosition = new Vector3(0, 50, 0);
                    UnityEngine.Debug.Log("Created new AttackCardArea");
                }
            }
            
            if (defenseCardArea == null)
            {
                GameObject existingDefenseArea = GameObject.Find("DefenseArea");
                if (existingDefenseArea != null)
                {
                    defenseCardArea = existingDefenseArea.transform;
                    UnityEngine.Debug.Log("Using existing DefenseArea for defense cards");
                }
                else
                {
                    // Create new area only if existing one is not found
                    GameObject defenseArea = new GameObject("DefenseCardArea");
                    defenseArea.transform.SetParent(GameObject.Find("UICanvas")?.transform);
                    defenseCardArea = defenseArea.transform;
                    defenseCardArea.localPosition = new Vector3(0, 150, 0);
                    UnityEngine.Debug.Log("Created new DefenseCardArea");
                }
            }
            
            UnityEngine.Debug.Log("Attack/Defense areas setup complete");
        }
        
        /// <summary>
        /// Attempt to attack with a card
        /// </summary>
        /// <param name="card">Card to attack with</param>
        /// <param name="attacker">Player making the attack</param>
        /// <returns>True if attack was successful</returns>
        public bool AttemptAttack(CardData card, Player attacker)
        {
            // Check if this is a valid attack
            if (!GameRules.Instance.CanAttackWith(card, attackCards))
            {
                UnityEngine.Debug.Log($"Invalid attack: {card.GetDisplayName()} cannot be used to attack");
                return false;
            }
            
            // Check turn order with detailed debugging
            Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
            bool isPlayerTurn = TurnManager.Instance.IsPlayerTurn(attacker);
            TurnPhase currentPhase = TurnManager.Instance.currentPhase;
            
            UnityEngine.Debug.Log($"=== TURN VALIDATION ===");
            UnityEngine.Debug.Log($"Attacker: {attacker.playerName} (ID: {attacker.playerID})");
            UnityEngine.Debug.Log($"Current Player: {currentPlayer?.playerName ?? "NULL"} (ID: {currentPlayer?.playerID ?? -1})");
            UnityEngine.Debug.Log($"Is Player Turn: {isPlayerTurn}");
            UnityEngine.Debug.Log($"Current Phase: {currentPhase}");
            UnityEngine.Debug.Log($"AttackerIndex: {TurnManager.Instance.attackerIndex}");
            UnityEngine.Debug.Log($"DefenderIndex: {TurnManager.Instance.defenderIndex}");
            UnityEngine.Debug.Log($"CurrentPlayerIndex: {TurnManager.Instance.currentPlayerIndex}");
            UnityEngine.Debug.Log($"=== END TURN VALIDATION ===");
            
            if (!isPlayerTurn || currentPhase != TurnPhase.AttackPhase)
            {
                UnityEngine.Debug.Log($"Attack REJECTED - Not {attacker.playerName}'s turn to attack or wrong phase");
                return false;
            }
            
            // Add card to attack
            attackCards.Add(card);
            boutActive = true;
            
            // Create visual representation
            CreateAttackCardVisual(card, attackCards.Count - 1);
            
            // Remove card from attacker's hand
            attacker.RemoveCardFromHand(card);
            
            UnityEngine.Debug.Log($"{attacker.playerName} attacks with {card.GetDisplayName()}");
            OnCardAttacked?.Invoke(card, attacker);
            
            // Switch to defense phase
            TurnManager.Instance.StartDefensePhase();
            
            return true;
        }
        
        /// <summary>
        /// Attempt to defend against an attack
        /// </summary>
        /// <param name="defenseCard">Card to defend with</param>
        /// <param name="attackCardIndex">Index of attack card to defend against</param>
        /// <param name="defender">Player making the defense</param>
        /// <returns>True if defense was successful</returns>
        public bool AttemptDefense(CardData defenseCard, int attackCardIndex, Player defender)
        {
            // Validate defense attempt
            if (attackCardIndex < 0 || attackCardIndex >= attackCards.Count)
            {
                UnityEngine.Debug.Log("Invalid attack card index for defense");
                return false;
            }
            
            if (attackCardIndex < defenseCards.Count)
            {
                UnityEngine.Debug.Log("Attack card already defended");
                return false;
            }
            
            // Check if this is a valid defense
            CardData attackCard = attackCards[attackCardIndex];
            if (!GameRules.Instance.CanDefendWith(attackCard, defenseCard))
            {
                UnityEngine.Debug.Log($"Invalid defense: {defenseCard.GetDisplayName()} cannot beat {attackCard.GetDisplayName()}");
                return false;
            }
            
            // Check turn phase
            if (TurnManager.Instance.currentPhase != TurnPhase.DefensePhase)
            {
                UnityEngine.Debug.Log("Not in defense phase");
                return false;
            }
            
            // Add card to defense
            defenseCards.Add(defenseCard);
            
            // Create visual representation
            CreateDefenseCardVisual(defenseCard, attackCardIndex);
            
            UnityEngine.Debug.Log($"{defender.playerName} defends {attackCard.GetDisplayName()} with {defenseCard.GetDisplayName()}");
            OnCardDefended?.Invoke(defenseCard, defender);
            
            // Check if bout is complete
            if (GameRules.Instance.IsAttackPhaseComplete(attackCards, defenseCards))
            {
                CompleteBout(true);
            }
            
            return true;
        }
        
        /// <summary>
        /// Complete the current bout
        /// </summary>
        /// <param name="defenseSuccessful">Whether the defense was successful</param>
        public void CompleteBout(bool defenseSuccessful)
        {
            UnityEngine.Debug.Log($"Bout completed. Defense successful: {defenseSuccessful}");
            
            OnBoutComplete?.Invoke(defenseSuccessful);
            
            // Clear bout state but keep visuals for now (they'll be cleared by other systems)
            boutActive = false;
        }
        
        /// <summary>
        /// Clear the current bout and reset state
        /// </summary>
        public void ClearBout()
        {
            attackCards.Clear();
            defenseCards.Clear();
            boutActive = false;
            
            // Clear visual objects
            foreach (GameObject obj in attackCardObjects)
            {
                if (obj != null) Destroy(obj);
            }
            foreach (GameObject obj in defenseCardObjects)
            {
                if (obj != null) Destroy(obj);
            }
            
            attackCardObjects.Clear();
            defenseCardObjects.Clear();
            
            UnityEngine.Debug.Log("Bout state cleared");
        }
        
        /// <summary>
        /// Complete the current bout and handle post-bout logic
        /// </summary>
        /// <param name="defenseSuccessful">Whether defense was successful</param>
        public void CompleteBoutWithLogic(bool defenseSuccessful)
        {
            UnityEngine.Debug.Log($"Bout complete - Defense {(defenseSuccessful ? "successful" : "failed")}");
            
            OnBoutComplete?.Invoke(defenseSuccessful);
            
            if (defenseSuccessful)
            {
                // All cards go to discard pile
                UnityEngine.Debug.Log("All cards discarded - defense successful");
            }
            else
            {
                // Defender takes all cards
                Player defender = TurnManager.Instance.GetDefender();
                if (defender != null)
                {
                    foreach (CardData card in attackCards)
                    {
                        defender.AddCardToHand(card);
                    }
                    foreach (CardData card in defenseCards)
                    {
                        defender.AddCardToHand(card);
                    }
                    UnityEngine.Debug.Log($"{defender.playerName} takes all cards from failed defense");
                }
            }
            
            // Clear the bout
            ClearBout();
            
            // Continue to next round
            TurnManager.Instance.NextRound();
        }
        
        /// <summary>
        /// Create visual representation of an attack card
        /// </summary>
        /// <param name="card">Card data</param>
        /// <param name="index">Position index</param>
        private void CreateAttackCardVisual(CardData card, int index)
        {
            if (attackCardArea == null) return;
            
            // Find the card prefab
            GameObject cardPrefab = GameObject.Find("CardPrefab");
            if (cardPrefab == null)
            {
                UnityEngine.Debug.LogError("CardPrefab not found for attack visual");
                return;
            }
            
            // Create card instance
            GameObject cardObj = Instantiate(cardPrefab, attackCardArea);
            
            // Position the card
            float xPos = (index - (attackCards.Count - 1) / 2f) * cardSpacing;
            cardObj.transform.localPosition = new Vector3(xPos, 0, 0);
            
            // Set up the card
            Cards.Card cardScript = cardObj.GetComponent<Cards.Card>();
            if (cardScript != null)
            {
                cardScript.Initialize(card);
            }
            
            attackCardObjects.Add(cardObj);
            UnityEngine.Debug.Log($"Created attack card visual for {card.GetDisplayName()}");
        }
        
        /// <summary>
        /// Create visual representation of a defense card
        /// </summary>
        /// <param name="card">Card data</param>
        /// <param name="index">Position index (matches attack card)</param>
        private void CreateDefenseCardVisual(CardData card, int index)
        {
            if (defenseCardArea == null) return;
            
            // Find the card prefab
            GameObject cardPrefab = GameObject.Find("CardPrefab");
            if (cardPrefab == null)
            {
                UnityEngine.Debug.LogError("CardPrefab not found for defense visual");
                return;
            }
            
            // Create card instance
            GameObject cardObj = Instantiate(cardPrefab, defenseCardArea);
            
            // Position the card to match the attack card it's defending
            float xPos = (index - (attackCards.Count - 1) / 2f) * cardSpacing;
            cardObj.transform.localPosition = new Vector3(xPos, 0, 0);
            
            // Set up the card
            Cards.Card cardScript = cardObj.GetComponent<Cards.Card>();
            if (cardScript != null)
            {
                cardScript.Initialize(card);
            }
            
            defenseCardObjects.Add(cardObj);
            UnityEngine.Debug.Log($"Created defense card visual for {card.GetDisplayName()}");
        }
        
        /// <summary>
        /// Get the current attack cards
        /// </summary>
        /// <returns>List of attack cards</returns>
        public List<CardData> GetAttackCards()
        {
            return new List<CardData>(attackCards);
        }
        
        /// <summary>
        /// Get the current defense cards
        /// </summary>
        /// <returns>List of defense cards</returns>
        public List<CardData> GetDefenseCards()
        {
            return new List<CardData>(defenseCards);
        }
        
        /// <summary>
        /// Check if bout is currently active
        /// </summary>
        /// <returns>True if bout is active</returns>
        public bool IsBoutActive()
        {
            return boutActive;
        }
    }
}