using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WitsAndFools.Core;
using WitsAndFools.Cards;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Auto-play system for demonstrating the game without manual input
    /// Toggleable feature that makes human players play automatically
    /// </summary>
    public class AutoPlayDemo : MonoBehaviour
    {
        [Header("Auto-Play Settings")]
        public bool autoPlayEnabled = false;
        public float autoPlayDelay = 2f;
        public bool showAutoPlayDecisions = true;
        
        [Header("Auto-Play Strategy")]
        public bool playRandomCards = false;
        public bool preferLowValueCards = true;
        public bool preferHighValueCards = false;
        
        [Header("UI Controls")]
        public KeyCode toggleAutoPlayKey = KeyCode.Space;
        public bool showInstructions = true;
        
        // Internal state
        private bool isAutoPlaying = false;
        private Coroutine autoPlayCoroutine;
        
        private void Start()
        {
            if (showInstructions)
            {
                Debug.Log("=== AUTO-PLAY DEMO CONTROLS ===");
                Debug.Log($"Press {toggleAutoPlayKey} to toggle auto-play mode");
                Debug.Log("Auto-play will make human players play automatically");
                Debug.Log("================================");
            }
            
            // FORCE AUTO-ACTIVATION FOR TESTING
            Debug.Log("=== FORCING AUTO-PLAY ACTIVATION IN 3 SECONDS ===");
            Invoke(nameof(ForceActivateAutoPlay), 3f);
            
            // Start aggressive game state monitoring
            StartCoroutine(AggressiveGameStateMonitoring());
        }
        
        private void ForceActivateAutoPlay()
        {
            if (!autoPlayEnabled)
            {
                Debug.Log("=== FORCE ACTIVATING AUTO-PLAY ===");
                ToggleAutoPlay();
            }
            
            // Fix attack system first
            StartCoroutine(FixAttackSystem());
            
            // Start aggressive card playing immediately
            StartCoroutine(AggressiveCardPlaying());
        }
        
        private IEnumerator FixAttackSystem()
        {
            Debug.Log("=== FIXING ATTACK SYSTEM ===");
            
            // Wait for systems to initialize
            yield return new WaitForSeconds(2f);
            
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
            if (TurnManager.Instance.players.Count >= 2)
            {
                Player attacker = TurnManager.Instance.GetAttacker();
                Player defender = TurnManager.Instance.GetDefender();
                
                if (attacker != null && defender != null)
                {
                    // Force correct player states
                    foreach (Player player in TurnManager.Instance.players)
                    {
                        player.isAttacking = false;
                        player.isDefending = false;
                    }
                    
                    attacker.isAttacking = true;
                    defender.isDefending = true;
                    
                    Debug.Log($"Fixed player states - Attacker: {attacker.playerName} (isAttacking: {attacker.isAttacking})");
                    Debug.Log($"Fixed player states - Defender: {defender.playerName} (isDefending: {defender.isDefending})");
                }
            }
            
            // Force start attack phase if not already started
            if (TurnManager.Instance.currentPhase != TurnPhase.AttackPhase)
            {
                Debug.Log("Forcing attack phase start...");
                TurnManager.Instance.StartAttackPhase();
            }
            
            Debug.Log("=== ATTACK SYSTEM FIX COMPLETE ===");
        }
        
        private IEnumerator AggressiveGameStateMonitoring()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                
                Debug.Log("=== AUTO-PLAY GAME STATE CHECK ===");
                
                if (TurnManager.Instance != null)
                {
                    Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
                    TurnPhase currentPhase = TurnManager.Instance.currentPhase;
                    
                    Debug.Log($"Current Phase: {currentPhase}");
                    Debug.Log($"Current Player: {(currentPlayer != null ? currentPlayer.playerName : "None")}");
                    
                    if (currentPlayer != null)
                    {
                        Debug.Log($"Player Type: {currentPlayer.playerType}");
                        Debug.Log($"Is Attacking: {currentPlayer.isAttacking}");
                        Debug.Log($"Is Defending: {currentPlayer.isDefending}");
                        Debug.Log($"Hand Size: {currentPlayer.GetHandCards().Count}");
                    }
                }
                else
                {
                    Debug.Log("TurnManager.Instance is NULL");
                }
                
                if (AttackDefenseSystem.Instance != null)
                {
                    var attackCards = AttackDefenseSystem.Instance.GetAttackCards();
                    var defenseCards = AttackDefenseSystem.Instance.GetDefenseCards();
                    Debug.Log($"Attack Cards: {attackCards.Count}, Defense Cards: {defenseCards.Count}");
                }
                else
                {
                    Debug.Log("AttackDefenseSystem.Instance is NULL");
                }
                
                Debug.Log($"AutoPlay Enabled: {autoPlayEnabled}, Is Auto Playing: {isAutoPlaying}");
                Debug.Log("=== END GAME STATE CHECK ===");
            }
        }
        
        private IEnumerator AggressiveCardPlaying()
        {
            yield return new WaitForSeconds(5f); // Wait for game to initialize
            
            while (true)
            {
                yield return new WaitForSeconds(3f);
                
                Debug.Log("=== AGGRESSIVE CARD PLAYING ATTEMPT ===");
                
                // Check if TurnManager and AttackDefenseSystem are ready
                if (TurnManager.Instance == null || AttackDefenseSystem.Instance == null)
                {
                    Debug.Log("TurnManager or AttackDefenseSystem not ready yet");
                    continue;
                }
                
                // Get current player and phase
                Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
                TurnPhase currentPhase = TurnManager.Instance.currentPhase;
                
                Debug.Log($"Current Player: {(currentPlayer != null ? currentPlayer.playerName : "NULL")}");
                Debug.Log($"Current Phase: {currentPhase}");
                Debug.Log($"Player Type: {(currentPlayer != null ? currentPlayer.playerType.ToString() : "NULL")}");
                
                // Only proceed if current player is human and we're in a valid phase
                if (currentPlayer != null && currentPlayer.playerType == PlayerType.Human)
                {
                    var handCards = currentPlayer.GetHandCards();
                    Debug.Log($"Current human player {currentPlayer.playerName} has {handCards.Count} cards in hand");
                    
                    if (handCards.Count > 0)
                    {
                        CardData cardToPlay = handCards[0];
                        bool played = false;
                        
                        // Try to play based on current phase
                        if (currentPhase == TurnPhase.AttackPhase && currentPlayer.isAttacking)
                        {
                            Debug.Log($"=== ATTEMPTING ATTACK: {currentPlayer.playerName} with {cardToPlay.GetDisplayName()} ===");
                            Debug.Log($"PRE-ATTACK CHECK: Phase={currentPhase}, IsAttacking={currentPlayer.isAttacking}");
                            Debug.Log($"PRE-ATTACK CHECK: AttackDefenseSystem.Instance null? {AttackDefenseSystem.Instance == null}");
                            Debug.Log($"PRE-ATTACK CHECK: About to call AttemptAttack...");
                            
                            played = AttackDefenseSystem.Instance.AttemptAttack(cardToPlay, currentPlayer);
                            
                            Debug.Log($"POST-ATTACK CHECK: AttemptAttack returned {played}");
                            
                            if (played)
                            {
                                Debug.Log($"SUCCESS: Attack with {cardToPlay.GetDisplayName()}");
                            }
                            else
                            {
                                Debug.Log($"FAILED: Attack with {cardToPlay.GetDisplayName()} - checking game rules");
                                
                                // Check why it failed
                                var currentAttackCards = AttackDefenseSystem.Instance.GetAttackCards();
                                bool canAttack = GameRules.Instance.CanAttackWith(cardToPlay, currentAttackCards);
                                Debug.Log($"Can attack with card: {canAttack}");
                                Debug.Log($"Current attack cards count: {currentAttackCards.Count}");
                            }
                        }
                        else if (currentPhase == TurnPhase.AttackPhase)
                        {
                            Debug.Log($"ATTACK PHASE BUT PLAYER NOT ATTACKING: {currentPlayer.playerName}, isAttacking={currentPlayer.isAttacking}");
                            
                            // Force the player to be attacking
                            Debug.Log($"FORCING {currentPlayer.playerName} to be attacking...");
                            currentPlayer.isAttacking = true;
                            
                            // Try the attack again
                            Debug.Log($"=== FORCE ATTEMPTING ATTACK: {currentPlayer.playerName} with {cardToPlay.GetDisplayName()} ===");
                            played = AttackDefenseSystem.Instance.AttemptAttack(cardToPlay, currentPlayer);
                            Debug.Log($"FORCE ATTACK RESULT: {played}");
                        }
                        else if (currentPhase == TurnPhase.DefensePhase && currentPlayer.isDefending)
                        {
                            Debug.Log($"=== ATTEMPTING DEFENSE: {currentPlayer.playerName} with {cardToPlay.GetDisplayName()} ===");
                            
                            var attackCards = AttackDefenseSystem.Instance.GetAttackCards();
                            var defenseCards = AttackDefenseSystem.Instance.GetDefenseCards();
                            
                            if (attackCards.Count > defenseCards.Count)
                            {
                                int attackIndex = defenseCards.Count;
                                CardData attackCard = attackCards[attackIndex];
                                
                                // Check if we can defend with this card
                                bool canDefend = GameRules.Instance.CanDefendWith(attackCard, cardToPlay);
                                Debug.Log($"Can defend {attackCard.GetDisplayName()} with {cardToPlay.GetDisplayName()}: {canDefend}");
                                
                                if (canDefend)
                                {
                                    played = AttackDefenseSystem.Instance.AttemptDefense(cardToPlay, attackIndex, currentPlayer);
                                    
                                    if (played)
                                    {
                                        Debug.Log($"SUCCESS: Defense with {cardToPlay.GetDisplayName()}");
                                    }
                                    else
                                    {
                                        Debug.Log($"FAILED: Defense with {cardToPlay.GetDisplayName()}");
                                    }
                                }
                                else
                                {
                                    Debug.Log($"Cannot defend {attackCard.GetDisplayName()} with {cardToPlay.GetDisplayName()} - trying next card");
                                    
                                    // Try other cards in hand
                                    bool foundDefense = false;
                                    for (int i = 1; i < handCards.Count && !foundDefense; i++)
                                    {
                                        CardData altCard = handCards[i];
                                        if (GameRules.Instance.CanDefendWith(attackCard, altCard))
                                        {
                                            Debug.Log($"Found valid defense: {altCard.GetDisplayName()}");
                                            played = AttackDefenseSystem.Instance.AttemptDefense(altCard, attackIndex, currentPlayer);
                                            foundDefense = played;
                                            break;
                                        }
                                    }
                                    
                                    if (!foundDefense)
                                    {
                                        Debug.Log($"No valid defense found - {currentPlayer.playerName} must take cards");
                                        // Trigger failed defense
                                        AttackDefenseSystem.Instance.CompleteBoutWithLogic(false);
                                        played = true; // Mark as handled
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.Log($"NOT IN ATTACK PHASE: Phase={currentPhase}, IsAttacking={currentPlayer.isAttacking}");
                            
                            // Force attack phase and try
                            Debug.Log($"FORCING ATTACK PHASE...");
                            TurnManager.Instance.ChangePhase(TurnPhase.AttackPhase);
                            TurnManager.Instance.currentPlayerIndex = TurnManager.Instance.attackerIndex;
                            currentPlayer.isAttacking = true;
                            
                            Debug.Log($"=== FORCE PHASE ATTACK: {currentPlayer.playerName} with {cardToPlay.GetDisplayName()} ===");
                            played = AttackDefenseSystem.Instance.AttemptAttack(cardToPlay, currentPlayer);
                            Debug.Log($"FORCE PHASE ATTACK RESULT: {played}");
                        }
                        
                        if (played)
                        {
                            // Wait a bit before next action
                            yield return new WaitForSeconds(2f);
                        }
                    }
                    else
                    {
                        Debug.Log($"Current player {currentPlayer.playerName} has no cards in hand");
                    }
                }
                else
                {
                    Debug.Log("Current player is not human or is null");
                }
            }
        }
        
        private void Update()
        {
            // Toggle auto-play with key press
            if (Input.GetKeyDown(toggleAutoPlayKey))
            {
                ToggleAutoPlay();
            }
            
            // Check if we should auto-play for current human player
            if (autoPlayEnabled && !isAutoPlaying)
            {
                CheckForAutoPlayOpportunity();
            }
        }
        
        /// <summary>
        /// Toggle auto-play mode on/off
        /// </summary>
        public void ToggleAutoPlay()
        {
            autoPlayEnabled = !autoPlayEnabled;
            
            if (autoPlayEnabled)
            {
                Debug.Log("=== AUTO-PLAY ENABLED ===");
                Debug.Log("Human players will now play automatically");
            }
            else
            {
                Debug.Log("=== AUTO-PLAY DISABLED ===");
                Debug.Log("Manual play mode restored");
                
                // Stop any ongoing auto-play
                if (autoPlayCoroutine != null)
                {
                    StopCoroutine(autoPlayCoroutine);
                    autoPlayCoroutine = null;
                    isAutoPlaying = false;
                }
            }
        }
        
        /// <summary>
        /// Check if current situation requires auto-play intervention
        /// </summary>
        private void CheckForAutoPlayOpportunity()
        {
            if (TurnManager.Instance == null || AttackDefenseSystem.Instance == null)
                return;
            
            Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
            if (currentPlayer == null || currentPlayer.playerType != PlayerType.Human)
                return;
            
            TurnPhase currentPhase = TurnManager.Instance.currentPhase;
            
            // Auto-play during attack phase for human attackers
            if (currentPhase == TurnPhase.AttackPhase && currentPlayer.isAttacking)
            {
                if (showAutoPlayDecisions)
                {
                    Debug.Log($"=== AUTO-PLAY: {currentPlayer.playerName} will attack automatically ===");
                }
                autoPlayCoroutine = StartCoroutine(AutoPlayAttack(currentPlayer));
            }
            // Auto-play during defense phase for human defenders
            else if (currentPhase == TurnPhase.DefensePhase && currentPlayer.isDefending)
            {
                if (showAutoPlayDecisions)
                {
                    Debug.Log($"=== AUTO-PLAY: {currentPlayer.playerName} will defend automatically ===");
                }
                autoPlayCoroutine = StartCoroutine(AutoPlayDefense(currentPlayer));
            }
        }
        
        /// <summary>
        /// Auto-play an attack for the human player
        /// </summary>
        /// <param name="attacker">Human player to attack for</param>
        private IEnumerator AutoPlayAttack(Player attacker)
        {
            isAutoPlaying = true;
            
            yield return new WaitForSeconds(autoPlayDelay);
            
            // Get available cards for attack
            List<CardData> availableCards = attacker.GetHandCards();
            List<CardData> currentAttack = AttackDefenseSystem.Instance.GetAttackCards();
            
            CardData chosenCard = null;
            
            // Find a valid attack card
            foreach (CardData card in availableCards)
            {
                if (GameRules.Instance.CanAttackWith(card, currentAttack))
                {
                    chosenCard = card;
                    break;
                }
            }
            
            if (chosenCard != null)
            {
                if (showAutoPlayDecisions)
                {
                    Debug.Log($"AUTO-PLAY: {attacker.playerName} chooses to attack with {chosenCard.GetDisplayName()}");
                }
                
                // Attempt the attack
                bool attackSuccess = AttackDefenseSystem.Instance.AttemptAttack(chosenCard, attacker);
                
                if (attackSuccess)
                {
                    Debug.Log($"AUTO-PLAY: Attack successful with {chosenCard.GetDisplayName()}");
                }
                else
                {
                    Debug.Log($"AUTO-PLAY: Attack failed with {chosenCard.GetDisplayName()}");
                }
            }
            else
            {
                if (showAutoPlayDecisions)
                {
                    Debug.Log($"AUTO-PLAY: {attacker.playerName} has no valid attack cards");
                }
            }
            
            isAutoPlaying = false;
        }
        
        /// <summary>
        /// Auto-play a defense for the human player
        /// </summary>
        /// <param name="defender">Human player to defend for</param>
        private IEnumerator AutoPlayDefense(Player defender)
        {
            isAutoPlaying = true;
            
            yield return new WaitForSeconds(autoPlayDelay);
            
            // Get attack cards that need defending
            List<CardData> attackCards = AttackDefenseSystem.Instance.GetAttackCards();
            List<CardData> defenseCards = AttackDefenseSystem.Instance.GetDefenseCards();
            List<CardData> availableCards = defender.GetHandCards();
            
            // Find the first undefended attack card
            int attackIndex = defenseCards.Count;
            if (attackIndex < attackCards.Count)
            {
                CardData attackCard = attackCards[attackIndex];
                CardData chosenDefense = null;
                
                // Find a valid defense card
                foreach (CardData card in availableCards)
                {
                    if (GameRules.Instance.CanDefendWith(attackCard, card))
                    {
                        chosenDefense = card;
                        break;
                    }
                }
                
                if (chosenDefense != null)
                {
                    if (showAutoPlayDecisions)
                    {
                        Debug.Log($"AUTO-PLAY: {defender.playerName} defends {attackCard.GetDisplayName()} with {chosenDefense.GetDisplayName()}");
                    }
                    
                    // Attempt the defense
                    bool defenseSuccess = AttackDefenseSystem.Instance.AttemptDefense(chosenDefense, attackIndex, defender);
                    
                    if (defenseSuccess)
                    {
                        Debug.Log($"AUTO-PLAY: Defense successful with {chosenDefense.GetDisplayName()}");
                    }
                    else
                    {
                        Debug.Log($"AUTO-PLAY: Defense failed with {chosenDefense.GetDisplayName()}");
                    }
                }
                else
                {
                    if (showAutoPlayDecisions)
                    {
                        Debug.Log($"AUTO-PLAY: {defender.playerName} cannot defend {attackCard.GetDisplayName()} - must eat cards");
                    }
                    
                    // Trigger failed defense (defender eats cards)
                    AttackDefenseSystem.Instance.CompleteBoutWithLogic(false);
                }
            }
            
            isAutoPlaying = false;
        }
        
        /// <summary>
        /// Choose the best card based on strategy settings
        /// </summary>
        /// <param name="availableCards">Cards to choose from</param>
        /// <returns>Best card according to strategy</returns>
        private CardData ChooseBestCard(List<CardData> availableCards)
        {
            if (availableCards.Count == 0) return null;
            
            if (playRandomCards)
            {
                return availableCards[Random.Range(0, availableCards.Count)];
            }
            
            if (preferLowValueCards)
            {
                CardData lowestCard = availableCards[0];
                foreach (CardData card in availableCards)
                {
                    if (card.value < lowestCard.value)
                        lowestCard = card;
                }
                return lowestCard;
            }
            
            if (preferHighValueCards)
            {
                CardData highestCard = availableCards[0];
                foreach (CardData card in availableCards)
                {
                    if (card.value > highestCard.value)
                        highestCard = card;
                }
                return highestCard;
            }
            
            // Default: return first available card
            return availableCards[0];
        }
        
        /// <summary>
        /// Display current auto-play status
        /// </summary>
        private void OnGUI()
        {
            if (!showInstructions) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("AUTO-PLAY DEMO", GUI.skin.label);
            GUILayout.Label($"Status: {(autoPlayEnabled ? "ENABLED" : "DISABLED")}");
            GUILayout.Label($"Press {toggleAutoPlayKey} to toggle");
            
            if (autoPlayEnabled)
            {
                GUILayout.Label($"Delay: {autoPlayDelay}s");
                GUILayout.Label($"Strategy: {GetStrategyName()}");
                
                if (isAutoPlaying)
                {
                    GUILayout.Label("Currently auto-playing...");
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Get the current strategy name for display
        /// </summary>
        /// <returns>Strategy name</returns>
        private string GetStrategyName()
        {
            if (playRandomCards) return "Random";
            if (preferLowValueCards) return "Low Value First";
            if (preferHighValueCards) return "High Value First";
            return "First Available";
        }
    }
}
