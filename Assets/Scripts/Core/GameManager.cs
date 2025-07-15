using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Main game controller that manages the overall game flow and state
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        public int maxPlayers = 5;
        public int minPlayers = 2;
        
        [Header("Game State")]
        public GameState currentState = GameState.Menu;
        
        [Header("Events")]
        public UnityEvent OnGameStart;
        public UnityEvent OnGameEnd;
        public UnityEvent<GameState> OnGameStateChanged;
        
        // Singleton pattern for easy access
        public static GameManager Instance { get; private set; }
        
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
            InitializeGame();
        }
        
        /// <summary>
        /// Initialize the game systems
        /// </summary>
        public void InitializeGame()
        {
            UnityEngine.Debug.Log("Game Manager initialized");
            ChangeState(GameState.Menu);
        }
        
        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartGame()
        {
            UnityEngine.Debug.Log("Starting new game");
            ChangeState(GameState.Playing);
            OnGameStart?.Invoke();
        }
        
        /// <summary>
        /// End the current game
        /// </summary>
        public void EndGame()
        {
            UnityEngine.Debug.Log("Ending game");
            ChangeState(GameState.GameOver);
            OnGameEnd?.Invoke();
        }
        
        /// <summary>
        /// Change the game state
        /// </summary>
        /// <param name="newState">New game state</param>
        public void ChangeState(GameState newState)
        {
            if (currentState != newState)
            {
                UnityEngine.Debug.Log($"Game state changed from {currentState} to {newState}");
                currentState = newState;
                OnGameStateChanged?.Invoke(newState);
            }
        }
        
        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            ChangeState(GameState.Menu);
        }
        
        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            UnityEngine.Debug.Log("Quitting game");
            Application.Quit();
        }
    }
}