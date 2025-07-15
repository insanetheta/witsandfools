using UnityEngine;
using WitsAndFools.Core;
using WitsAndFools.Cards;
using System.Collections.Generic;
using System.Collections;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Aggressive auto-start system that forces complete game initialization and auto-play
    /// </summary>
    public class ManualGameTrigger : MonoBehaviour
    {
        [Header("Auto-Start Settings")]
        public bool autoStartOnPlay = true;
        public float initializationDelay = 2f;
        public float gameStartDelay = 4f;
        public float autoPlayActivationDelay = 6f;
        
        [Header("Debug")]
        public bool verboseLogging = true;
        
        private bool gameInitialized = false;
        private bool autoPlayActivated = false;
        
        private void Start()
        {
            if (autoStartOnPlay)
            {
                Debug.Log("=== AGGRESSIVE AUTO-START SYSTEM ACTIVATED ===");
                Debug.Log("This system will force complete game initialization and auto-play");
                StartCoroutine(ForceGameInitialization());
            }
        }
        
        private IEnumerator ForceGameInitialization()
        {
            // Step 1: Wait for basic initialization
            yield return new WaitForSeconds(initializationDelay);
            Debug.Log("=== STEP 1: FORCING GAME MANAGER START ===");
            ForceGameManagerStart();
            
            // Step 2: Force game start
            yield return new WaitForSeconds(gameStartDelay - initializationDelay);
            Debug.Log("=== STEP 2: FORCING GAME START ===");
            ForceGameStart();
            
            // Step 3: Activate auto-play
            yield return new WaitForSeconds(autoPlayActivationDelay - gameStartDelay);
            Debug.Log("=== STEP 3: ACTIVATING AUTO-PLAY ===");
            ForceAutoPlayActivation();
            
            // Step 4: Start monitoring and forcing moves
            yield return new WaitForSeconds(2f);
            Debug.Log("=== STEP 4: STARTING AGGRESSIVE MOVE FORCING ===");
            StartCoroutine(AggressivelyForceMoves());
        }
        
        private void ForceGameManagerStart()
        {
            if (GameManager.Instance != null)
            {
                Debug.Log("Forcing GameManager to start game...");
                GameManager.Instance.StartGame();
                Debug.Log($"GameManager state: {GameManager.Instance.currentState}");
            }
            else
            {
                Debug.LogError("GameManager.Instance is null!");
            }
        }
        
        private void ForceGameStart()
        {
            Debug.Log("=== FORCING COMPLETE GAME INITIALIZATION ===");
            
            // Force TurnManager initialization
            if (TurnManager.Instance != null)
            {
                Debug.Log("TurnManager found - checking initialization...");
                
                // Find all players
                Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
                Debug.Log($"Found {allPlayers.Length} players in scene");
                
                if (allPlayers.Length >= 2)
                {
                    // Clear and set players
                    TurnManager.Instance.players.Clear();
                    for (int i = 0; i < Mathf.Min(2, allPlayers.Length); i++)
                    {
                        TurnManager.Instance.players.Add(allPlayers[i]);
                        Debug.Log($"Added {allPlayers[i].playerName} to TurnManager");
                    }
                    
                    // Force turn order initialization
                    TurnManager.Instance.InitializeTurnOrder();
                    Debug.Log("TurnManager initialization forced!");
                    
                    gameInitialized = true;
                }
                else
                {
                    Debug.LogError("Not enough players found for initialization!");
                }
            }
            else
            {
                Debug.LogError("TurnManager.Instance is null!");
            }
        }
        
        private void ForceAutoPlayActivation()
        {
            Debug.Log("=== FORCING AUTO-PLAY ACTIVATION ===");
            
            // Find AutoPlayDemo
            AutoPlayDemo autoPlayDemo = FindObjectOfType<AutoPlayDemo>();
            if (autoPlayDemo != null)
            {
                if (!autoPlayDemo.autoPlayEnabled)
                {
                    Debug.Log("Activating AutoPlayDemo...");
                    autoPlayDemo.ToggleAutoPlay();
                    autoPlayActivated = true;
                    Debug.Log($"AutoPlay activated! Status: {autoPlayDemo.autoPlayEnabled}");
                }
                else
                {
                    Debug.Log("AutoPlayDemo already enabled");
                    autoPlayActivated = true;
                }
            }
            else
            {
                Debug.LogError("AutoPlayDemo not found!");
            }
        }
        
        private IEnumerator AggressivelyForceMoves()
        {
            Debug.Log("=== STARTING AGGRESSIVE MOVE FORCING ===");
            
            while (true)
            {
                yield return new WaitForSeconds(3f);
                
                if (verboseLogging)
                {
                    LogCurrentGameState();
                }
                
                // Try to force a move if the game seems stuck
                if (gameInitialized && autoPlayActivated)
                {
                    TryForceMove();
                }
            }
        }
        
        private void LogCurrentGameState()
        {
            Debug.Log("=== CURRENT GAME STATE ===");
            
            if (TurnManager.Instance != null)
            {
                Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
                Debug.Log($"Current Phase: {TurnManager.Instance.currentPhase}");
                Debug.Log($"Current Player: {(currentPlayer != null ? currentPlayer.playerName : "None")}");
                
                if (currentPlayer != null)
                {
                    Debug.Log($"Player Type: {currentPlayer.playerType}");
                    Debug.Log($"Is Attacking: {currentPlayer.isAttacking}");
                    Debug.Log($"Is Defending: {currentPlayer.isDefending}");
                    Debug.Log($"Hand Size: {currentPlayer.GetHandCards().Count}");
                }
            }
            
            if (AttackDefenseSystem.Instance != null)
            {
                var attackCards = AttackDefenseSystem.Instance.GetAttackCards();
                var defenseCards = AttackDefenseSystem.Instance.GetDefenseCards();
                Debug.Log($"Attack Cards: {attackCards.Count}, Defense Cards: {defenseCards.Count}");
            }
        }
        
        private void TryForceMove()
        {
            if (TurnManager.Instance == null || AttackDefenseSystem.Instance == null)
                return;
            
            Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
            if (currentPlayer == null) return;
            
            TurnPhase currentPhase = TurnManager.Instance.currentPhase;
            
            // If it's a human player's turn to attack, force an attack
            if (currentPlayer.playerType == PlayerType.Human && 
                currentPhase == TurnPhase.AttackPhase && 
                currentPlayer.isAttacking)
            {
                Debug.Log("=== FORCING HUMAN ATTACK ===");
                ForceHumanAttack(currentPlayer);
            }
        }
        
        private void ForceHumanAttack(Player attacker)
        {
            List<CardData> handCards = attacker.GetHandCards();
            List<CardData> currentAttack = AttackDefenseSystem.Instance.GetAttackCards();
            
            Debug.Log($"Forcing attack for {attacker.playerName}");
            Debug.Log($"Hand cards: {handCards.Count}, Current attack: {currentAttack.Count}");
            
            // Find the first valid attack card
            CardData chosenCard = null;
            foreach (CardData card in handCards)
            {
                if (GameRules.Instance.CanAttackWith(card, currentAttack))
                {
                    chosenCard = card;
                    break;
                }
            }
            
            if (chosenCard != null)
            {
                Debug.Log($"*** FORCING ATTACK WITH: {chosenCard.GetDisplayName()} ***");
                
                bool attackSuccess = AttackDefenseSystem.Instance.AttemptAttack(chosenCard, attacker);
                
                if (attackSuccess)
                {
                    Debug.Log($"✓ FORCED ATTACK SUCCESSFUL with {chosenCard.GetDisplayName()}!");
                }
                else
                {
                    Debug.LogError($"✗ FORCED ATTACK FAILED with {chosenCard.GetDisplayName()}");
                }
            }
            else
            {
                Debug.Log("No valid attack cards found for forced attack");
                
                // List all cards for debugging
                Debug.Log("Available cards:");
                foreach (CardData card in handCards)
                {
                    Debug.Log($"  - {card.GetDisplayName()} (Value: {card.value})");
                }
            }
        }
        
        private void TriggerGameAction()
        {
            Debug.Log("=== TRIGGERING MANUAL GAME ACTION ===");
            
            if (TurnManager.Instance == null)
            {
                Debug.LogError("TurnManager not found!");
                return;
            }
            
            if (AttackDefenseSystem.Instance == null)
            {
                Debug.LogError("AttackDefenseSystem not found!");
                return;
            }
            
            Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
            if (currentPlayer == null)
            {
                Debug.LogError("No current player found!");
                return;
            }
            
            Debug.Log($"Current Player: {currentPlayer.playerName} (Type: {currentPlayer.playerType})");
            Debug.Log($"Current Phase: {TurnManager.Instance.currentPhase}");
            Debug.Log($"Is Attacking: {currentPlayer.isAttacking}");
            Debug.Log($"Is Defending: {currentPlayer.isDefending}");
            
            // If it's a human player's turn to attack, make an attack
            if (currentPlayer.playerType == PlayerType.Human && 
                TurnManager.Instance.currentPhase == TurnPhase.AttackPhase && 
                currentPlayer.isAttacking)
            {
                MakeAttack(currentPlayer);
            }
            else
            {
                Debug.Log("Conditions not met for manual attack trigger");
                Debug.Log("Scheduling another check in 2 seconds...");
                Invoke(nameof(TriggerGameAction), 2f);
            }
        }
        
        private void MakeAttack(Player attacker)
        {
            Debug.Log($"=== MAKING MANUAL ATTACK FOR {attacker.playerName} ===");
            
            List<CardData> handCards = attacker.GetHandCards();
            List<CardData> currentAttack = AttackDefenseSystem.Instance.GetAttackCards();
            
            Debug.Log($"Hand cards count: {handCards.Count}");
            Debug.Log($"Current attack cards: {currentAttack.Count}");
            
            // Find the first valid attack card
            CardData chosenCard = null;
            foreach (CardData card in handCards)
            {
                if (GameRules.Instance.CanAttackWith(card, currentAttack))
                {
                    chosenCard = card;
                    break;
                }
            }
            
            if (chosenCard != null)
            {
                Debug.Log($"Chosen attack card: {chosenCard.GetDisplayName()}");
                
                bool attackSuccess = AttackDefenseSystem.Instance.AttemptAttack(chosenCard, attacker);
                
                if (attackSuccess)
                {
                    Debug.Log($"✓ Attack successful with {chosenCard.GetDisplayName()}!");
                    Debug.Log("Scheduling next action check in 3 seconds...");
                    Invoke(nameof(TriggerGameAction), 3f);
                }
                else
                {
                    Debug.LogError($"✗ Attack failed with {chosenCard.GetDisplayName()}");
                }
            }
            else
            {
                Debug.Log("No valid attack cards found");
                
                // List all cards for debugging
                Debug.Log("Available cards:");
                foreach (CardData card in handCards)
                {
                    Debug.Log($"  - {card.GetDisplayName()} (Value: {card.value})");
                }
            }
        }
        
        [ContextMenu("Trigger Action Now")]
        public void TriggerActionNow()
        {
            TriggerGameAction();
        }
    }
}
