using UnityEngine;
using UnityEngine.UI;
using WitsAndFools.Cards;
using WitsAndFools.Core;
using System.Collections.Generic;

/// <summary>
/// Handles attack interactions and AI defense - adds click handlers to Player 1 cards and moves them to attack area
/// Also manages AI defense for Player 1
/// </summary>
public class AttackHandler : MonoBehaviour
{
    [Header("Attack Settings")]
    public float moveSpeed = 500f;
    public Transform attackArea;
    
    [Header("AI Defense Settings")]
    public float aiDecisionDelay = 2f;
    public bool autoDefend = true;
    public Transform defenseArea;
    
    // Store pending defense info
    private CardData pendingAttackCard;
    private Player pendingAttacker;
    private bool awaitingDefense = false;
    
    private void Start()
    {
        // Wait for cards to be dealt, then set up click handlers
        Invoke(nameof(SetupAttackHandlers), 3f);
        
        // Subscribe to attack events for AI defense
        if (AttackDefenseSystem.Instance != null)
        {
            AttackDefenseSystem.Instance.OnCardAttacked.AddListener(OnPlayerAttacked);
        }
        
        // Subscribe to phase changes for proper AI defense timing
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
        }
        
        // Find defense area
        if (defenseArea == null)
        {
            GameObject defenseAreaObj = GameObject.Find("DefenseArea");
            if (defenseAreaObj != null)
            {
                defenseArea = defenseAreaObj.transform;
                Debug.Log("Found DefenseArea for AI defense");
            }
        }
    }
    
    void SetupAttackHandlers()
    {
        Debug.Log("=== Setting up Game Handlers ===");
        
        // Find attack area
        if (attackArea == null)
        {
            GameObject attackAreaObj = GameObject.Find("AttackArea");
            if (attackAreaObj != null)
            {
                attackArea = attackAreaObj.transform;
                Debug.Log("Found AttackArea for card movement");
            }
            else
            {
                Debug.LogError("AttackArea not found!");
                return;
            }
        }
        
        // Get the current attacker and defender from TurnManager
        Player currentAttacker = TurnManager.Instance?.GetAttacker();
        Player currentDefender = TurnManager.Instance?.GetDefender();
        
        if (currentAttacker == null || currentDefender == null)
        {
            Debug.LogError("No current attacker or defender found!");
            return;
        }
        
        Debug.Log($"Current Attacker: {currentAttacker.playerName} (ID: {currentAttacker.playerID}, Type: {currentAttacker.playerType})");
        Debug.Log($"Current Defender: {currentDefender.playerName} (ID: {currentDefender.playerID}, Type: {currentDefender.playerType})");
        
        // Case 1: Human is attacker, AI is defender
        if (currentAttacker.playerType == PlayerType.Human && currentDefender.playerType == PlayerType.AI)
        {
            Debug.Log("=== HUMAN ATTACKS, AI DEFENDS ===");
            SetupHumanAttackHandlers();
        }
        // Case 2: AI is attacker, Human is defender  
        else if (currentAttacker.playerType == PlayerType.AI && currentDefender.playerType == PlayerType.Human)
        {
            Debug.Log("=== AI ATTACKS, HUMAN DEFENDS ===");
            Debug.Log("AI will attack automatically, then human can defend");
            StartCoroutine(HandleAIAttack());
        }
        // Case 3: Both human (shouldn't happen in this demo)
        else if (currentAttacker.playerType == PlayerType.Human && currentDefender.playerType == PlayerType.Human)
        {
            Debug.Log("=== BOTH PLAYERS HUMAN (NOT IMPLEMENTED) ===");
            SetupHumanAttackHandlers();
        }
        // Case 4: Both AI (shouldn't happen in this demo)
        else
        {
            Debug.Log("=== BOTH PLAYERS AI (NOT IMPLEMENTED) ===");
        }
    }
    
    void SetupHumanAttackHandlers()
    {
        // Find Player 0's hand area and add click handlers to all cards
        Transform playerHandArea = GameObject.Find("PlayerHandArea")?.transform;
        if (playerHandArea == null)
        {
            Debug.LogError("PlayerHandArea not found!");
            return;
        }
        
        Card[] player0Cards = playerHandArea.GetComponentsInChildren<Card>();
        Debug.Log($"Found {player0Cards.Length} cards in Player 0's hand (Human)");
        
        foreach (Card card in player0Cards)
        {
            SetupCardClickHandler(card);
        }
        
        Debug.Log("=== Human attack handlers ready! Click Player 0's cards (bottom hand) to attack! ===");
    }
    
    void SetupCardClickHandler(Card card)
    {
        // Add Button component if it doesn't exist
        Button button = card.GetComponent<Button>();
        if (button == null)
        {
            button = card.gameObject.AddComponent<Button>();
        }
        
        // Clear any existing listeners
        button.onClick.RemoveAllListeners();
        
        // Add attack handler
        button.onClick.AddListener(() => OnCardAttack(card));
        
        Debug.Log($"Added attack handler to card: {card.cardData?.cardName ?? "Unknown Card"}");
    }
    
    void OnCardAttack(Card attackCard)
    {
        // Move card to attack area
        MoveCardToAttackArea(attackCard);
        
        // Notify attack system
        if (AttackDefenseSystem.Instance != null)
        {
            // Find the correct attacking player by checking which hand area the card came from
            Player attacker = FindAttackingPlayerFromCard(attackCard);
            if (attacker != null)
            {
                Debug.Log($"=== ATTACK INITIATED ===");
                Debug.Log($"{attacker.playerName} (ID: {attacker.playerID}) attacks with: {attackCard.cardData?.GetDisplayName() ?? "Unknown Card"}");
                Debug.Log($"Card Value: {attackCard.cardData?.value ?? 0}");
                Debug.Log($"Card Suit: {attackCard.cardData?.suit ?? CardSuit.Clubs}");
                
                bool attackResult = AttackDefenseSystem.Instance.AttemptAttack(attackCard.cardData, attacker);
                Debug.Log($"Attack validation result: {(attackResult ? "VALID" : "INVALID")}");
                
                if (attackResult)
                {
                    Debug.Log("Card moved to attack area - waiting for defense!");
                }
                else
                {
                    Debug.Log("Attack failed validation - card returned to hand");
                    // Could add logic to return card to hand here
                }
            }
            else
            {
                Debug.LogWarning("No attacking player found!");
            }
        }
        else
        {
            Debug.LogWarning("AttackDefenseSystem not found!");
        }
        
        Debug.Log($"=== END ATTACK ===");
    }
    
    void MoveCardToAttackArea(Card card)
    {
        if (attackArea == null)
        {
            Debug.LogError("Attack area not found - cannot move card!");
            return;
        }
        
        Debug.Log($"Moving {card.cardData?.cardName ?? "card"} to attack area...");
        
        // Change parent to attack area
        card.transform.SetParent(attackArea);
        
        // Reset local position and scale
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        
        // Disable the button so it can't be clicked again
        Button button = card.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = false;
        }
        
        Debug.Log("Card successfully moved to attack area!");
    }
    
    /// <summary>
    /// Public method to manually trigger attack setup (for testing)
    /// </summary>
    public void ForceSetupAttackHandlers()
    {
        SetupAttackHandlers();
    }
    
    // AI Defense Methods
    void OnPlayerAttacked(CardData attackCard, Player attacker)
    {
        Debug.Log($"=== AI DEFENSE TRIGGERED ===");
        Debug.Log($"Attack card: {attackCard.GetDisplayName()}");
        Debug.Log($"Attacker: {attacker.playerName} (ID: {attacker.playerID})");
        Debug.Log($"Current TurnManager phase: {TurnManager.Instance?.currentPhase}");
        
        // Store the attack info and wait for phase change to DefensePhase
        pendingAttackCard = attackCard;
        pendingAttacker = attacker;
        awaitingDefense = true;
        
        Debug.Log("Stored attack info, waiting for DefensePhase...");
    }
    
    void OnPhaseChanged(TurnPhase newPhase)
    {
        Debug.Log($"Phase changed to: {newPhase}");
        
        // When phase changes to DefensePhase and we have a pending attack, process AI defense
        if (newPhase == TurnPhase.DefensePhase && awaitingDefense && pendingAttackCard != null)
        {
            Debug.Log("Processing AI defense for pending attack...");
            
            // Find Player 1 (the AI defender)
            Player defender = FindAIDefender();
            if (defender != null && autoDefend)
            {
                Debug.Log($"AI {defender.playerName} (ID: {defender.playerID}) considering defense...");
                StartCoroutine(ConsiderDefense(pendingAttackCard, defender));
            }
            else if (defender == null)
            {
                Debug.LogError("AI defender not found!");
            }
            else if (!autoDefend)
            {
                Debug.LogWarning("Auto defend is disabled!");
            }
            
            // Clear pending attack info
            awaitingDefense = false;
            pendingAttackCard = null;
            pendingAttacker = null;
        }
    }
    
    System.Collections.IEnumerator ConsiderDefense(CardData attackCard, Player defender)
    {
        // Add delay for realistic AI thinking
        yield return new WaitForSeconds(aiDecisionDelay);
        
        // Find a valid defense card
        CardData defenseCard = FindValidDefenseCard(attackCard, defender);
        
        if (defenseCard != null)
        {
            Debug.Log($"=== AI DEFENSE ATTEMPT ===");
            Debug.Log($"AI defends {attackCard.GetDisplayName()} with {defenseCard.GetDisplayName()}");
            
            // Attempt defense through the AttackDefenseSystem
            bool defenseSuccessful = AttackDefenseSystem.Instance.AttemptDefense(defenseCard, 0, defender);
            
            if (defenseSuccessful)
            {
                Debug.Log("AI defense successful!");
                
                // Remove the card from AI's hand and move to defense area
                RemoveCardFromHand(defenseCard, defender);
                MoveCardToDefenseArea(defenseCard);
                
                // Check if bout is complete
                CheckBoutCompletion();
            }
            else
            {
                Debug.Log("AI defense failed!");
                HandleFailedDefense(defender);
            }
        }
        else
        {
            Debug.Log("=== AI CANNOT DEFEND ===");
            Debug.Log($"AI has no valid defense for {attackCard.GetDisplayName()}");
            Debug.Log("AI must eat the cards!");
            
            // AI fails to defend - must eat the cards
            HandleFailedDefense(defender);
        }
    }
    
    CardData FindValidDefenseCard(CardData attackCard, Player defender)
    {
        List<CardData> hand = defender.GetHandCards();
        
        foreach (CardData card in hand)
        {
            if (GameRules.Instance.CanDefendWith(attackCard, card))
            {
                Debug.Log($"AI found valid defense: {card.GetDisplayName()} can beat {attackCard.GetDisplayName()}");
                return card;
            }
        }
        
        Debug.Log($"AI has no valid defense cards against {attackCard.GetDisplayName()}");
        return null;
    }
    
    void RemoveCardFromHand(CardData cardToRemove, Player player)
    {
        // Remove from player's hand data
        player.RemoveCardFromHand(cardToRemove);
        
        // Find and remove the visual card from Player 1's hand area
        Transform player1HandArea = GameObject.Find("Player1HandArea")?.transform;
        if (player1HandArea != null)
        {
            Card[] handCards = player1HandArea.GetComponentsInChildren<Card>();
            foreach (Card handCard in handCards)
            {
                if (handCard.cardData != null && 
                    handCard.cardData.cardName == cardToRemove.cardName &&
                    handCard.cardData.value == cardToRemove.value &&
                    handCard.cardData.suit == cardToRemove.suit)
                {
                    Debug.Log($"Removing {cardToRemove.GetDisplayName()} from AI hand visual");
                    Destroy(handCard.gameObject);
                    break;
                }
            }
        }
    }
    
    void MoveCardToDefenseArea(CardData defenseCard)
    {
        if (defenseArea == null)
        {
            Debug.LogError("Defense area not found!");
            return;
        }
        
        // Create visual representation in defense area
        GameObject cardPrefab = GameObject.Find("CardPrefab");
        if (cardPrefab != null)
        {
            GameObject defenseCardObj = Instantiate(cardPrefab, defenseArea);
            defenseCardObj.transform.localPosition = Vector3.zero;
            defenseCardObj.transform.localScale = Vector3.one;
            
            // Initialize the card
            Card cardScript = defenseCardObj.GetComponent<Card>();
            if (cardScript != null)
            {
                cardScript.Initialize(defenseCard);
            }
            
            Debug.Log($"AI defense card {defenseCard.GetDisplayName()} moved to defense area");
        }
    }
    
    void HandleFailedDefense(Player defender)
    {
        Debug.Log($"=== DEFENSE FAILED ===");
        Debug.Log($"{defender.playerName} must eat the attack cards!");
        
        // Get all cards from the attack
        List<CardData> attackCards = AttackDefenseSystem.Instance.GetAttackCards();
        
        // Add attack cards to defender's hand
        foreach (CardData card in attackCards)
        {
            defender.AddCardToHand(card);
            Debug.Log($"Added {card.GetDisplayName()} to {defender.playerName}'s hand");
        }
        
        // Clear the attack area
        ClearAttackArea();
        
        // End the bout
        CompleteBout(false);
    }
    
    void CheckBoutCompletion()
    {
        List<CardData> attackCards = AttackDefenseSystem.Instance.GetAttackCards();
        List<CardData> defenseCards = AttackDefenseSystem.Instance.GetDefenseCards();
        
        Debug.Log($"Bout status: {attackCards.Count} attacks, {defenseCards.Count} defenses");
        
        // If all attacks are defended, defense wins
        if (attackCards.Count > 0 && defenseCards.Count == attackCards.Count)
        {
            Debug.Log("=== DEFENSE SUCCESSFUL ===");
            Debug.Log("All attacks defended! Clearing the bout.");
            
            // Clear both areas
            ClearAttackArea();
            ClearDefenseArea();
            
            // Complete the bout successfully
            CompleteBout(true);
        }
    }
    
    void ClearAttackArea()
    {
        Transform attackAreaTransform = GameObject.Find("AttackArea")?.transform;
        if (attackAreaTransform != null)
        {
            for (int i = attackAreaTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(attackAreaTransform.GetChild(i).gameObject);
            }
            Debug.Log("Attack area cleared");
        }
    }
    
    void ClearDefenseArea()
    {
        if (defenseArea != null)
        {
            for (int i = defenseArea.childCount - 1; i >= 0; i--)
            {
                Destroy(defenseArea.GetChild(i).gameObject);
            }
            Debug.Log("Defense area cleared");
        }
    }
    
    void CompleteBout(bool defenseSuccessful)
    {
        Debug.Log($"=== BOUT COMPLETE ===");
        Debug.Log($"Defense successful: {defenseSuccessful}");
        
        // Clear the AttackDefenseSystem state
        AttackDefenseSystem.Instance.ClearBout();
        
        // Handle turn progression
        if (TurnManager.Instance != null)
        {
            if (defenseSuccessful)
            {
                // In Durak: Successful defense means defender becomes attacker
                Debug.Log("Defense successful! Defender becomes next attacker.");
                TurnManager.Instance.SwapAttackerDefender();
                TurnManager.Instance.StartAttackPhase();
                
                // Re-setup attack handlers for the new attacker
                Invoke(nameof(SetupAttackHandlers), 1f);
            }
            else
            {
                // Failed defense: Original attacker can attack again
                Debug.Log("Defense failed! Attacker can continue attacking.");
                TurnManager.Instance.StartAttackPhase();
                
                // Re-setup attack handlers
                Invoke(nameof(SetupAttackHandlers), 1f);
            }
        }
    }
    
    Player FindAIDefender()
    {
        // Find Player 1 (AI player)
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player player in players)
        {
            if (player.playerType == PlayerType.AI)
            {
                return player;
            }
        }
        return null;
    }
    
    Player FindAttackingPlayerFromCard(Card attackCard)
    {
        Debug.Log($"=== FINDING ATTACKER FOR CARD ===");
        Debug.Log($"Card: {attackCard.cardData?.GetDisplayName() ?? "Unknown"}");
        
        // Check if the card came from PlayerHandArea (Human Player)
        Transform playerHandArea = GameObject.Find("PlayerHandArea")?.transform;
        if (playerHandArea != null && attackCard.transform.IsChildOf(playerHandArea))
        {
            Debug.Log("Card came from PlayerHandArea (should be Player 0 - Human)");
            // Find the human player (ID 0)
            Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            Debug.Log($"Found {players.Length} total players");
            foreach (Player player in players)
            {
                Debug.Log($"Checking player: ID={player.playerID}, Name='{player.playerName}', Type={player.playerType}");
                if (player.playerType == PlayerType.Human && player.playerID == 0)
                {
                    Debug.Log($"Match found! Returning Player 0 (Human): {player.playerName}");
                    return player;
                }
            }
            Debug.LogWarning("No Human Player with ID 0 found!");
        }
        
        // Check if the card came from Player1HandArea (AI Player)
        Transform player1HandArea = GameObject.Find("Player1HandArea")?.transform;
        if (player1HandArea != null && attackCard.transform.IsChildOf(player1HandArea))
        {
            Debug.Log("Card came from Player1HandArea (should be Player 1 - AI)");
            // Find the AI player (ID 1)
            Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            foreach (Player player in players)
            {
                if (player.playerType == PlayerType.AI && player.playerID == 1)
                {
                    return player;
                }
            }
        }
        
        // Fallback: assume it's the human player for bottom hand clicks
        Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player player in allPlayers)
        {
            if (player.playerType == PlayerType.Human)
            {
                return player;
            }
        }
        
        return null;
    }
    
    System.Collections.IEnumerator HandleAIAttack()
    {
        Debug.Log("=== AI ATTACK HANDLER ===");
        
        // Give a brief delay for visual feedback
        yield return new WaitForSeconds(1f);
        
        Player aiAttacker = TurnManager.Instance?.GetAttacker();
        if (aiAttacker == null || aiAttacker.playerType != PlayerType.AI)
        {
            Debug.LogError("AI Attack called but current attacker is not AI!");
            yield break;
        }
        
        Debug.Log($"AI {aiAttacker.playerName} is considering attack...");
        
        // Get AI's hand from HandManager
        HandManager aiHandManager = aiAttacker.GetComponent<HandManager>();
        if (aiHandManager == null)
        {
            Debug.LogError("AI player has no HandManager component!");
            yield break;
        }
        
        // Find AI's hand area to get cards
        Transform aiHandArea = GameObject.Find("Player1HandArea")?.transform;
        if (aiHandArea == null)
        {
            Debug.LogError("Player1HandArea not found!");
            yield break;
        }
        
        Card[] aiCards = aiHandArea.GetComponentsInChildren<Card>();
        if (aiCards.Length == 0)
        {
            Debug.LogWarning("AI has no cards to attack with!");
            yield break;
        }
        
        // For now, AI attacks with the first available card
        Card attackCard = aiCards[0];
        Debug.Log($"AI chooses to attack with: {attackCard.cardData?.GetDisplayName() ?? "Unknown Card"}");
        
        // Move AI card to attack area and attempt the attack
        MoveCardToAttackArea(attackCard);
        
        // Attempt the attack
        bool attackResult = AttackDefenseSystem.Instance.AttemptAttack(attackCard.cardData, aiAttacker);
        Debug.Log($"AI attack result: {(attackResult ? "SUCCESS" : "FAILED")}");
        
        if (attackResult)
        {
            Debug.Log("AI attack successful - waiting for human defense");
        }
        else
        {
            Debug.LogWarning("AI attack failed - this shouldn't happen in normal gameplay");
        }
    }
}