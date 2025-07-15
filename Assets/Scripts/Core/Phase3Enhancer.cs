using UnityEngine;
using WitsAndFools.Core;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Adds Phase 3 attack/defense functionality to existing demo
    /// </summary>
    public class Phase3Enhancer : MonoBehaviour
    {
        void Start()
        {
            // Wait for the original demo to set up, then enhance it
            Invoke(nameof(EnhanceForPhase3), 3f);
        }
        
        void EnhanceForPhase3()
        {
            UnityEngine.Debug.Log("=== Enhancing Demo for Phase 3 Attack System ===");
            
            // Add game rules
            if (GameRules.Instance == null)
            {
                GameObject rulesObj = new GameObject("GameRules");
                rulesObj.AddComponent<GameRules>();
                GameRules.Instance.SetTrumpSuit(CardSuit.Hearts);
            }
            
            // Add turn manager
            if (TurnManager.Instance == null)
            {
                GameObject turnObj = new GameObject("TurnManager");
                turnObj.AddComponent<TurnManager>();
            }
            
            // Add attack/defense system
            if (AttackDefenseSystem.Instance == null)
            {
                GameObject attackObj = new GameObject("AttackDefenseSystem");
                attackObj.AddComponent<AttackDefenseSystem>();
            }
            
            // Setup the existing player as attacker
            Player humanPlayer = FindFirstObjectByType<Player>();
            if (humanPlayer != null)
            {
                humanPlayer.SetAsAttacker();
                
                // Add cards to the player's CardData hand for attack system
                HandManager handManager = humanPlayer.GetComponent<HandManager>();
                if (handManager != null)
                {
                    // Since cardObjects is private, we'll manually add some cards
                    for (int i = 0; i < 5; i++)
                    {
                        Cards.CardData card = ScriptableObject.CreateInstance<Cards.CardData>();
                        card.cardName = $"Attack Card {i + 1}";
                        card.suit = (CardSuit)(i % 4);
                        card.value = 7 + i;
                        humanPlayer.AddCardToHand(card);
                    }
                }
                
                UnityEngine.Debug.Log($"Enhanced {humanPlayer.playerName} as attacker with {humanPlayer.GetHandSize()} cards");
            }
            
            // Create a dummy defender
            GameObject defenderObj = new GameObject("DummyDefender");
            Player defender = defenderObj.AddComponent<Player>();
            defender.Initialize(1, "Dummy Defender", PlayerType.AI);
            defender.SetAsDefender();
            
            // Add some cards to defender
            for (int i = 0; i < 3; i++)
            {
                Cards.CardData card = ScriptableObject.CreateInstance<Cards.CardData>();
                card.cardName = $"Defense Card {i + 1}";
                card.suit = (CardSuit)(i % 4);
                card.value = 10 + i;
                defender.AddCardToHand(card);
            }
            
            // Initialize turn system
            if (TurnManager.Instance != null && humanPlayer != null)
            {
                TurnManager.Instance.players.Clear();
                TurnManager.Instance.players.Add(humanPlayer);
                TurnManager.Instance.players.Add(defender);
                TurnManager.Instance.attackerIndex = 0;
                TurnManager.Instance.defenderIndex = 1;
                TurnManager.Instance.StartAttackPhase();
            }
            
            ShowPhase3Instructions();
        }
        
        void ShowPhase3Instructions()
        {
            UnityEngine.Debug.Log("╔══════════════════════════════════════════════════════════════════════════════╗");
            UnityEngine.Debug.Log("║                          PHASE 3 ATTACK DEMO READY!                         ║");
            UnityEngine.Debug.Log("╠══════════════════════════════════════════════════════════════════════════════╣");
            UnityEngine.Debug.Log("║ INSTRUCTIONS:                                                                ║");
            UnityEngine.Debug.Log("║ 1. Click any card in the bottom hand to ATTACK                              ║");
            UnityEngine.Debug.Log("║ 2. The card will move to the center attack area                             ║");
            UnityEngine.Debug.Log("║ 3. Phase changes to Defense (no AI yet, but system is ready)               ║");
            UnityEngine.Debug.Log("║                                                                              ║");
            UnityEngine.Debug.Log("║ WHAT'S WORKING:                                                              ║");
            UnityEngine.Debug.Log("║ • ✅ Attack/Defense card validation                                          ║");
            UnityEngine.Debug.Log("║ • ✅ Turn phase management                                                   ║");
            UnityEngine.Debug.Log("║ • ✅ Trump suit system (Hearts)                                             ║");
            UnityEngine.Debug.Log("║ • ✅ Visual card placement in attack area                                   ║");
            UnityEngine.Debug.Log("║ • ✅ Player state management (attacker/defender)                            ║");
            UnityEngine.Debug.Log("╚══════════════════════════════════════════════════════════════════════════════╝");
        }
    }
}
