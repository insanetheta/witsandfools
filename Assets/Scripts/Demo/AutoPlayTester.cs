using UnityEngine;
using WitsAndFools.Demo;
using WitsAndFools.Core;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Simple tester script to verify AutoPlayDemo functionality
    /// This script will log detailed information about the demo state
    /// </summary>
    public class AutoPlayTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public bool runTestOnStart = true;
        public float testInterval = 3f;
        
        private AutoPlayDemo autoPlayDemo;
        private float lastTestTime;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                Debug.Log("=== AUTOPLAY TESTER STARTED ===");
                Debug.Log("This script will monitor and test AutoPlayDemo functionality");
                
                // Find or create AutoPlayDemo
                autoPlayDemo = FindObjectOfType<AutoPlayDemo>();
                if (autoPlayDemo == null)
                {
                    Debug.Log("AutoPlayDemo not found - creating one...");
                    GameObject autoPlayObj = new GameObject("AutoPlayDemo");
                    autoPlayDemo = autoPlayObj.AddComponent<AutoPlayDemo>();
                    Debug.Log("AutoPlayDemo created successfully");
                }
                else
                {
                    Debug.Log("AutoPlayDemo found in scene");
                }
                
                // Log initial state
                LogAutoPlayState();
                
                // Auto-activate AutoPlayDemo after a short delay
                Debug.Log("Auto-activating AutoPlayDemo in 2 seconds...");
                Invoke(nameof(AutoActivateAutoPlay), 2f);
            }
        }
        
        private void Update()
        {
            if (autoPlayDemo != null && Time.time - lastTestTime > testInterval)
            {
                LogAutoPlayState();
                LogGameState();
                lastTestTime = Time.time;
            }
            
            // Test controls
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestAutoPlayToggle();
            }
            
            if (Input.GetKeyDown(KeyCode.G))
            {
                LogGameState();
            }
        }
        
        /// <summary>
        /// Log the current state of AutoPlayDemo
        /// </summary>
        private void LogAutoPlayState()
        {
            if (autoPlayDemo == null) return;
            
            Debug.Log($"=== AUTOPLAY STATE CHECK ===");
            Debug.Log($"AutoPlay Enabled: {autoPlayDemo.autoPlayEnabled}");
            Debug.Log($"AutoPlay Delay: {autoPlayDemo.autoPlayDelay}s");
            Debug.Log($"Show Decisions: {autoPlayDemo.showAutoPlayDecisions}");
            Debug.Log($"Strategy - Random: {autoPlayDemo.playRandomCards}");
            Debug.Log($"Strategy - Low Value: {autoPlayDemo.preferLowValueCards}");
            Debug.Log($"Strategy - High Value: {autoPlayDemo.preferHighValueCards}");
        }
        
        /// <summary>
        /// Log the current game state
        /// </summary>
        private void LogGameState()
        {
            Debug.Log($"=== GAME STATE CHECK ===");
            
            if (TurnManager.Instance != null)
            {
                Player currentPlayer = TurnManager.Instance.GetCurrentPlayer();
                TurnPhase currentPhase = TurnManager.Instance.currentPhase;
                
                Debug.Log($"Current Phase: {currentPhase}");
                Debug.Log($"Current Player: {(currentPlayer != null ? currentPlayer.playerName : "None")}");
                Debug.Log($"Player Type: {(currentPlayer != null ? currentPlayer.playerType.ToString() : "None")}");
                
                if (currentPlayer != null)
                {
                    Debug.Log($"Is Attacking: {currentPlayer.isAttacking}");
                    Debug.Log($"Is Defending: {currentPlayer.isDefending}");
                    Debug.Log($"Hand Size: {currentPlayer.GetHandCards().Count}");
                }
            }
            else
            {
                Debug.Log("TurnManager not found");
            }
            
            if (AttackDefenseSystem.Instance != null)
            {
                var attackCards = AttackDefenseSystem.Instance.GetAttackCards();
                var defenseCards = AttackDefenseSystem.Instance.GetDefenseCards();
                
                Debug.Log($"Attack Cards: {attackCards.Count}");
                Debug.Log($"Defense Cards: {defenseCards.Count}");
            }
            else
            {
                Debug.Log("AttackDefenseSystem not found");
            }
        }
        
        /// <summary>
        /// Test the auto-play toggle functionality
        /// </summary>
        private void TestAutoPlayToggle()
        {
            if (autoPlayDemo != null)
            {
                Debug.Log("=== TESTING AUTOPLAY TOGGLE ===");
                bool wasEnabled = autoPlayDemo.autoPlayEnabled;
                autoPlayDemo.ToggleAutoPlay();
                bool isEnabled = autoPlayDemo.autoPlayEnabled;
                
                Debug.Log($"AutoPlay was: {wasEnabled}, now: {isEnabled}");
                Debug.Log("Toggle test completed");
            }
        }
        
        /// <summary>
        /// Auto-activate AutoPlayDemo
        /// </summary>
        private void AutoActivateAutoPlay()
        {
            if (autoPlayDemo != null && !autoPlayDemo.autoPlayEnabled)
            {
                Debug.Log("=== AUTO-ACTIVATING AUTOPLAY MODE ===");
                autoPlayDemo.ToggleAutoPlay();
                Debug.Log($"AutoPlay activated! Enabled: {autoPlayDemo.autoPlayEnabled}");
                Debug.Log("Game should now play automatically for human players!");
            }
        }
        
        /// <summary>
        /// Display test instructions
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(320, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("AUTOPLAY TESTER", GUI.skin.label);
            GUILayout.Label("Press T - Test Toggle");
            GUILayout.Label("Press G - Log Game State");
            GUILayout.Label("Press SPACE - Toggle AutoPlay");
            
            if (autoPlayDemo != null)
            {
                GUILayout.Label($"AutoPlay: {(autoPlayDemo.autoPlayEnabled ? "ON" : "OFF")}");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
