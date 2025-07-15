using UnityEngine;
using WitsAndFools.Core;
using WitsAndFools.Cards;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Simple demo setup for Phase 3
    /// </summary>
    public class SimpleAttackDemo : MonoBehaviour
    {
        void Start()
        {
            UnityEngine.Debug.Log("=== Starting Simple Attack Demo ===");
            
            // Create basic players for testing
            CreateSimplePlayers();
        }
        
        void CreateSimplePlayers()
        {
            // Create Player 1
            GameObject p1 = new GameObject("AttackingPlayer");
            Player player1 = p1.AddComponent<Player>();
            player1.Initialize(0, "Player 1 (Attacker)", PlayerType.Human);
            player1.SetAsAttacker();
            
            // Create Player 2
            GameObject p2 = new GameObject("DefendingPlayer");
            Player player2 = p2.AddComponent<Player>();
            player2.Initialize(1, "Player 2 (Defender)", PlayerType.Human);
            player2.SetAsDefender();
            
            // Add some cards to their hands
            AddDemoCardsToPlayer(player1);
            AddDemoCardsToPlayer(player2);
            
            UnityEngine.Debug.Log("Simple demo players created:");
            UnityEngine.Debug.Log($"- {player1.playerName} has {player1.GetHandSize()} cards");
            UnityEngine.Debug.Log($"- {player2.playerName} has {player2.GetHandSize()} cards");
            UnityEngine.Debug.Log("Phase 3 core systems are ready for testing!");
        }
        
        void AddDemoCardsToPlayer(Player player)
        {
            // Create some basic cards
            for (int i = 1; i <= 3; i++)
            {
                CardData card = ScriptableObject.CreateInstance<CardData>();
                card.cardName = $"Demo Card {i}";
                card.suit = (CardSuit)(i % 4);
                card.value = 7 + i;
                card.abilityType = CardAbilityType.None;
                
                player.AddCardToHand(card);
            }
        }
    }
}
