using System.Collections;
using UnityEngine;
using WitsAndFools.Core;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Fixes issues with the attack system not working during auto play demo
    /// Ensures proper initialization and state management
    /// </summary>
    public class AttackSystemFixer : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool enableDebugLogs = true;
        public float initializationDelay = 3f;
        
        private void Start()
        {
            if (enableDebugLogs)
            {
                Debug.Log("=== ATTACK SYSTEM FIXER STARTED ===");
            }
            
            // Start the initialization process
            StartCoroutine(InitializeAttackSystem());
        }
        
        private IEnumerator InitializeAttackSystem()
        {
            // Wait for other systems to initialize
            yield return new WaitForSeconds(initializationDelay);
            
            if (enableDebugLogs)
            {
                Debug.Log("=== FIXING ATTACK SYSTEM INITIALIZATION ===");
            }
            
            // Ensure TurnManager is properly set up
            if (TurnManager.Instance == null)
            {
                Debug.LogError("TurnManager.Instance is null! Cannot fix attack system.");
                yield break;
            }
            
            // Ensure AttackDefenseSystem is properly set up
            if (AttackDefenseSystem.Instance == null)
            {
                Debug.LogError("AttackDefenseSystem.Instance is null! Cannot fix attack system.");
                yield break;
            }
            
            // Force proper initialization of turn order
            TurnManager.Instance.InitializeTurnOrder();
            
            // Wait a moment for initialization
            yield return new WaitForSeconds(1f);
            
            // Verify and fix player states
            VerifyAndFixPlayerStates();
            
            // Force start attack phase if not already started
            if (TurnManager.Instance.currentPhase != TurnPhase.AttackPhase)
            {
                Debug.Log("Forcing attack phase start...");
                TurnManager.Instance.StartAttackPhase();
            }
            
            // Start continuous monitoring
            StartCoroutine(ContinuousMonitoring());
            
            if (enableDebugLogs)
            {
                Debug.Log("=== ATTACK SYSTEM FIXER COMPLETE ===");
            }
        }
        
        private void VerifyAndFixPlayerStates()
        {
            if (TurnManager.Instance.players.Count < 2)
            {
                Debug.LogError("Not enough players for attack system!");
                return;
            }
            
            Player attacker = TurnManager.Instance.GetAttacker();
            Player defender = TurnManager.Instance.GetDefender();
            
            if (attacker == null || defender == null)
            {
                Debug.LogError("Attacker or defender is null!");
                return;
            }
            
            // Force correct player states
            foreach (Player player in TurnManager.Instance.players)
            {
                player.isAttacking = false;
                player.isDefending = false;
            }
            
            attacker.isAttacking = true;
            defender.isDefending = true;
            
            if (enableDebugLogs)
            {
                Debug.Log($"Fixed player states - Attacker: {attacker.playerName} (isAttacking: {attacker.isAttacking})");
                Debug.Log($"Fixed player states - Defender: {defender.playerName} (isDefending: {defender.isDefending})");
            }
        }
        
        private IEnumerator ContinuousMonitoring()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);
                
                if (enableDebugLogs)
                {
                    Debug.Log("=== ATTACK SYSTEM STATUS CHECK ===");
                    
                    if (TurnManager.Instance != null)
                    {
                        Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
                        TurnPhase currentPhase = TurnManager.Instance.currentPhase;
                        
                        Debug.Log($"Current Phase: {currentPhase}");
                        Debug.Log($"Current Player: {(currentPlayer != null ? currentPlayer.playerName : "NULL")}");
                        
                        if (currentPlayer != null)
                        {
                            Debug.Log($"Player Type: {currentPlayer.playerType}");
                            Debug.Log($"Is Attacking: {currentPlayer.isAttacking}");
                            Debug.Log($"Is Defending: {currentPlayer.isDefending}");
                            Debug.Log($"Hand Size: {currentPlayer.GetHandCards().Count}");
                        }
                        
                        // Check if we need to fix states
                        if (currentPhase == TurnPhase.AttackPhase)
                        {
                            Player attacker = TurnManager.Instance.GetAttacker();
                            if (attacker != null && !attacker.isAttacking)
                            {
                                Debug.Log("FIXING: Attacker state was incorrect, fixing...");
                                attacker.isAttacking = true;
                            }
                        }
                        else if (currentPhase == TurnPhase.DefensePhase)
                        {
                            Player defender = TurnManager.Instance.GetDefender();
                            if (defender != null && !defender.isDefending)
                            {
                                Debug.Log("FIXING: Defender state was incorrect, fixing...");
                                defender.isDefending = true;
                            }
                        }
                    }
                    
                    if (AttackDefenseSystem.Instance != null)
                    {
                        var attackCards = AttackDefenseSystem.Instance.GetAttackCards();
                        var defenseCards = AttackDefenseSystem.Instance.GetDefenseCards();
                        Debug.Log($"Attack Cards: {attackCards.Count}, Defense Cards: {defenseCards.Count}");
                        Debug.Log($"Bout Active: {AttackDefenseSystem.Instance.IsBoutActive()}");
                    }
                    
                    Debug.Log("=== END STATUS CHECK ===");
                }
            }
        }
        
        /// <summary>
        /// Force an attack to happen for testing purposes
        /// </summary>
        [ContextMenu("Force Test Attack")]
        public void ForceTestAttack()
        {
            if (TurnManager.Instance == null || AttackDefenseSystem.Instance == null)
            {
                Debug.LogError("Cannot force attack - systems not ready");
                return;
            }
            
            Player attacker = TurnManager.Instance.GetAttacker();
            if (attacker == null)
            {
                Debug.LogError("No attacker found");
                return;
            }
            
            var handCards = attacker.GetHandCards();
            if (handCards.Count == 0)
            {
                Debug.LogError("Attacker has no cards");
                return;
            }
            
            // Force the attack
            var cardToPlay = handCards[0];
            Debug.Log($"=== FORCING TEST ATTACK: {attacker.playerName} with {cardToPlay.GetDisplayName()} ===");
            
            bool success = AttackDefenseSystem.Instance.AttemptAttack(cardToPlay, attacker);
            Debug.Log($"Force attack result: {success}");
        }
    }
}
