using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Manages turn order, player rotation, and game flow
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("Turn Settings")]
        public float turnTimeLimit = 30f;
        public bool enableTurnTimer = false;
        
        [Header("Player Management")]
        public List<Player> players = new List<Player>();
        public int currentPlayerIndex = 0;
        public int attackerIndex = 0;
        public int defenderIndex = 1;
        
        [Header("Turn State")]
        public TurnPhase currentPhase = TurnPhase.StartTurn;
        public bool waitingForDefense = false;
        
        [Header("Events")]
        public UnityEvent<Player> OnTurnStart;
        public UnityEvent<Player> OnTurnEnd;
        public UnityEvent<TurnPhase> OnPhaseChanged;
        public UnityEvent<Player, Player> OnAttackPhaseStart; // attacker, defender
        
        // Singleton pattern
        public static TurnManager Instance { get; private set; }
        
        private Coroutine turnTimerCoroutine;
        
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
            // Delay initialization to allow other systems to set up first
            StartCoroutine(DelayedInitialization());
        }
        
        private IEnumerator DelayedInitialization()
        {
            // Wait for other systems to initialize (DemoCardCreator runs first)
            yield return new WaitForSeconds(2f);
            InitializeTurnOrder();
        }
        
    /// <summary>
    /// Initialize turn order and start first turn
    /// </summary>
    public void InitializeTurnOrder()
    {
        if (players.Count < 2)
        {
            Debug.LogError("Need at least 2 players to start game");
            return;
        }
        
        // Set initial attacker and defender
        attackerIndex = 0;
        defenderIndex = 1;
        currentPlayerIndex = attackerIndex;
        
        // Set player states
        Player attacker = GetAttacker();
        Player defender = GetDefender();
        
        // Reset all player states first
        foreach (Player player in players)
        {
            player.isAttacking = false;
            player.isDefending = false;
        }
        
        // Set correct states
        attacker.isAttacking = true;
        defender.isDefending = true;
        
        Debug.Log($"Attacker: {attacker.playerName} (ID: {attacker.playerID}), Defender: {defender.playerName} (ID: {defender.playerID})");
        Debug.Log($"Attacker isAttacking: {attacker.isAttacking}, Defender isDefending: {defender.isDefending}");
        
        // Start with attack phase
        ChangePhase(TurnPhase.AttackPhase);
    }
        
        /// <summary>
        /// Start the attack phase
        /// </summary>
        public void StartAttackPhase()
        {
            if (players.Count < 2) return;
            
            Player attacker = players[attackerIndex];
            Player defender = players[defenderIndex];
            
            ChangePhase(TurnPhase.AttackPhase);
            currentPlayerIndex = attackerIndex;
            waitingForDefense = false;
            
            // Update player states
            foreach (Player player in players)
            {
                player.isAttacking = false;
                player.isDefending = false;
            }
            
            attacker.isAttacking = true;
            defender.isDefending = true;
            
            UnityEngine.Debug.Log($"Attack phase started - {attacker.playerName} attacking {defender.playerName}");
            
            OnAttackPhaseStart?.Invoke(attacker, defender);
            OnTurnStart?.Invoke(attacker);
            
            if (enableTurnTimer)
            {
                StartTurnTimer();
            }
        }
        
        /// <summary>
        /// Start the defense phase
        /// </summary>
        public void StartDefensePhase()
        {
            if (players.Count < 2) return;
            
            Player defender = players[defenderIndex];
            
            ChangePhase(TurnPhase.DefensePhase);
            currentPlayerIndex = defenderIndex;
            waitingForDefense = true;
            
            UnityEngine.Debug.Log($"Defense phase started - {defender.playerName} must defend");
            
            OnTurnStart?.Invoke(defender);
            
            if (enableTurnTimer)
            {
                StartTurnTimer();
            }
        }
        
        /// <summary>
        /// End the current turn and move to next player
        /// </summary>
        public void EndTurn()
        {
            if (players.Count == 0) return;
            
            Player currentPlayer = GetCurrentPlayer();
            OnTurnEnd?.Invoke(currentPlayer);
            
            StopTurnTimer();
            
            // Move to next phase or next round
            if (currentPhase == TurnPhase.AttackPhase && waitingForDefense)
            {
                StartDefensePhase();
            }
            else
            {
                NextRound();
            }
        }
        
        /// <summary>
        /// Move to the next round (new attacker/defender pair)
        /// </summary>
        public void NextRound()
        {
            // Rotate attacker and defender
            attackerIndex = (attackerIndex + 1) % players.Count;
            defenderIndex = (defenderIndex + 1) % players.Count;
            
            // Make sure attacker and defender are different
            if (attackerIndex == defenderIndex)
            {
                defenderIndex = (defenderIndex + 1) % players.Count;
            }
            
            UnityEngine.Debug.Log($"New round - Attacker: {players[attackerIndex].playerName}, Defender: {players[defenderIndex].playerName}");
            
            StartAttackPhase();
        }
        
        /// <summary>
        /// Change the current game phase
        /// </summary>
        /// <param name="newPhase">New phase to enter</param>
        public void ChangePhase(TurnPhase newPhase)
        {
            if (currentPhase != newPhase)
            {
                UnityEngine.Debug.Log($"Phase changed from {currentPhase} to {newPhase}");
                currentPhase = newPhase;
                OnPhaseChanged?.Invoke(newPhase);
            }
        }
        
        /// <summary>
        /// Get the currently active player
        /// </summary>
        /// <returns>Current player</returns>
        public Player GetCurrentPlayer()
        {
            if (players.Count == 0 || currentPlayerIndex >= players.Count)
                return null;
            
            return players[currentPlayerIndex];
        }
        
        /// <summary>
        /// Get the attacking player
        /// </summary>
        /// <returns>Attacking player</returns>
        public Player GetAttacker()
        {
            if (players.Count == 0 || attackerIndex >= players.Count)
                return null;
            
            return players[attackerIndex];
        }
        
        /// <summary>
        /// Get the defending player
        /// </summary>
        /// <returns>Defending player</returns>
        public Player GetDefender()
        {
            if (players.Count == 0 || defenderIndex >= players.Count)
                return null;
            
            return players[defenderIndex];
        }
        
        /// <summary>
        /// Start the turn timer
        /// </summary>
        private void StartTurnTimer()
        {
            StopTurnTimer();
            if (enableTurnTimer)
            {
                turnTimerCoroutine = StartCoroutine(TurnTimerCoroutine());
            }
        }
        
        /// <summary>
        /// Stop the turn timer
        /// </summary>
        private void StopTurnTimer()
        {
            if (turnTimerCoroutine != null)
            {
                StopCoroutine(turnTimerCoroutine);
                turnTimerCoroutine = null;
            }
        }
        
        /// <summary>
        /// Turn timer coroutine
        /// </summary>
        private IEnumerator TurnTimerCoroutine()
        {
            yield return new WaitForSeconds(turnTimeLimit);
            
            UnityEngine.Debug.Log($"Turn timer expired for {GetCurrentPlayer()?.playerName}");
            EndTurn();
        }
        
        /// <summary>
        /// Check if it's the specified player's turn
        /// </summary>
        /// <param name="player">Player to check</param>
        /// <returns>True if it's the player's turn</returns>
        public bool IsPlayerTurn(Player player)
        {
            return GetCurrentPlayer() == player;
        }
        
        /// <summary>
        /// Force a specific player to be the attacker
        /// </summary>
        /// <param name="playerIndex">Index of player to make attacker</param>
        public void SetAttacker(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < players.Count)
            {
                attackerIndex = playerIndex;
                defenderIndex = (playerIndex + 1) % players.Count;
                
                // Ensure different players
                if (attackerIndex == defenderIndex && players.Count > 1)
                {
                    defenderIndex = (defenderIndex + 1) % players.Count;
                }
                
                UnityEngine.Debug.Log($"Forced attacker change - Attacker: {players[attackerIndex].playerName}, Defender: {players[defenderIndex].playerName}");
            }
        }
        
        /// <summary>
        /// Swap attacker and defender roles (for successful defense in Durak)
        /// </summary>
        public void SwapAttackerDefender()
        {
            int tempIndex = attackerIndex;
            attackerIndex = defenderIndex;
            defenderIndex = tempIndex;
            
            // Update current player to be the new attacker
            currentPlayerIndex = attackerIndex;
            
            // Update player states
            foreach (Player player in players)
            {
                player.isAttacking = false;
                player.isDefending = false;
            }
            
            players[attackerIndex].isAttacking = true;
            players[defenderIndex].isDefending = true;
            
            UnityEngine.Debug.Log($"Roles swapped - New Attacker: {players[attackerIndex].playerName}, New Defender: {players[defenderIndex].playerName}");
        }
    }
}
