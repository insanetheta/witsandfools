using System.Collections;
using UnityEngine;
using WitsAndFools.Cards;
using WitsAndFools.Core;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Fixes AI defense by ensuring it triggers after attacks
    /// </summary>
    public class AIDefenseFix : MonoBehaviour
    {
        [Header("AI Defense Settings")]
        public float defenseDelay = 2f;
        
        void Start()
        {
            // Subscribe to attack events after a delay to ensure other systems are ready
            StartCoroutine(SetupDefenseListener());
        }
        
        IEnumerator SetupDefenseListener()
        {
            yield return new WaitForSeconds(1f);
            
            if (AttackDefenseSystem.Instance != null)
            {
                AttackDefenseSystem.Instance.OnCardAttacked.AddListener(OnPlayerAttacked);
                Debug.Log("AI Defense Fix: Subscribed to attack events");
            }
            else
            {
                Debug.LogError("AI Defense Fix: AttackDefenseSystem not found!");
            }
        }
        
        void OnPlayerAttacked(CardData attackCard, Player attacker)
        {
            Debug.Log($"=== AI DEFENSE FIX TRIGGERED ===");
            Debug.Log($"Attack detected: {attackCard.GetDisplayName()} by {attacker.playerName}");
            
            // Find the AI defender
            Player aiDefender = FindAIPlayer();
            if (aiDefender != null)
            {
                Debug.Log($"Found AI defender: {aiDefender.playerName} (ID: {aiDefender.playerID})");
                StartCoroutine(TriggerAIDefense(attackCard, aiDefender));
            }
            else
            {
                Debug.LogError("AI Defense Fix: No AI player found!");
            }
        }
        
        IEnumerator TriggerAIDefense(CardData attackCard, Player aiDefender)
        {
            yield return new WaitForSeconds(defenseDelay);
            
            Debug.Log($"=== AI ATTEMPTING DEFENSE ===");
            
            // Find a valid defense card from AI's hand
            CardData defenseCard = FindValidDefenseCard(attackCard, aiDefender);
            
            if (defenseCard != null)
            {
                Debug.Log($"AI defends with: {defenseCard.GetDisplayName()}");
                
                // Attempt defense through the system
                bool success = AttackDefenseSystem.Instance.AttemptDefense(defenseCard, 0, aiDefender);
                
                if (success)
                {
                    Debug.Log("AI defense successful!");
                }
                else
                {
                    Debug.Log("AI defense failed - eating cards");
                    HandleFailedDefense(aiDefender);
                }
            }
            else
            {
                Debug.Log("AI has no valid defense - eating cards");
                HandleFailedDefense(aiDefender);
            }
        }
        
        Player FindAIPlayer()
        {
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
        
        CardData FindValidDefenseCard(CardData attackCard, Player defender)
        {
            foreach (CardData card in defender.GetHandCards())
            {
                if (GameRules.Instance.CanDefendWith(attackCard, card))
                {
                    Debug.Log($"Found valid defense: {card.GetDisplayName()} can beat {attackCard.GetDisplayName()}");
                    return card;
                }
            }
            return null;
        }
        
        void HandleFailedDefense(Player defender)
        {
            // The AttackDefenseSystem should handle adding cards to defender's hand
            // Just complete the bout
            if (AttackDefenseSystem.Instance != null)
            {
                AttackDefenseSystem.Instance.CompleteBout(false); // false = defense failed
            }
        }
    }
}
