using UnityEngine;
using UnityEngine.UI;
using WitsAndFools.Cards;
using WitsAndFools.Core;
using System.Collections.Generic;

/// <summary>
/// Handles AI defense decisions and automates the defense phase for Player 1
/// </summary>
public class AIDefenseManager : MonoBehaviour
{
    [Header("AI Settings")]
    public float aiDecisionDelay = 2f;
    public bool autoDefend = true;
    
    [Header("Defense Logic")]
    public Transform defenseArea;
    
    private void Start()
    {
        // Subscribe to attack events
        if (AttackDefenseSystem.Instance != null)
        {
            AttackDefenseSystem.Instance.OnCardAttacked.AddListener(OnPlayerAttacked);
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
    
    void OnPlayerAttacked(CardData attackCard, Player attacker)
    {
        Debug.Log($"=== AI DEFENSE TRIGGERED ===");
        Debug.Log($"Attack card: {attackCard.GetDisplayName()}");
        
        // Check if it's the defense phase and Player 1's turn to defend
        if (TurnManager.Instance != null && TurnManager.Instance.currentPhase == TurnPhase.DefensePhase)
        {
            // Find Player 1 (the AI defender)
            Player defender = FindAIDefender();
            if (defender != null && autoDefend)
            {
                Debug.Log($"AI {defender.playerName} considering defense...");
                StartCoroutine(ConsiderDefense(attackCard, defender));
            }
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
        Transform attackArea = GameObject.Find("AttackArea")?.transform;
        if (attackArea != null)
        {
            for (int i = attackArea.childCount - 1; i >= 0; i--)
            {
                Destroy(attackArea.GetChild(i).gameObject);
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
                // Defender becomes the next attacker
                Debug.Log("Defender wins! Switching to next turn.");
                TurnManager.Instance.ChangePhase(TurnPhase.EndTurn);
            }
            else
            {
                // Attacker continues, defender ate cards
                Debug.Log("Attacker wins! Continuing with next player.");
                TurnManager.Instance.ChangePhase(TurnPhase.EndTurn);
            }
        }
    }
    
    Player FindAIDefender()
    {
        // Find Player 1 (AI player)
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            if (player.playerType == PlayerType.AI)
            {
                return player;
            }
        }
        return null;
    }
}