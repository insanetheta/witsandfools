using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WitsAndFools.Cards;

namespace WitsAndFools.Core
{
    /// <summary>
    /// Simple demo manager to set up 2-player attack demonstration
    /// </summary>
    public class SimpleDemoManager : MonoBehaviour
    {
        [Header("Demo Settings")]
        public int cardsPerPlayer = 5;
        
        void Start()
        {
            StartCoroutine(SetupDemo());
        }
        
        IEnumerator SetupDemo()
        {
            UnityEngine.Debug.Log("SimpleDemoManager: Starting demo setup...");
            
            // Wait for systems to initialize
            yield return new WaitForSeconds(1f);
            
            // Initialize trump suit
            if (GameRules.Instance != null)
            {
                GameRules.Instance.SetTrumpSuit(CardSuit.Hearts);
                UnityEngine.Debug.Log("Trump suit set to Hearts");
            }
            
            UnityEngine.Debug.Log("SimpleDemoManager: Demo setup complete!");
            UnityEngine.Debug.Log("Instructions:");
            UnityEngine.Debug.Log("- Use existing card display");
            UnityEngine.Debug.Log("- Click cards to attack/defend");
            UnityEngine.Debug.Log("- Watch console for game events");
        }
    }
}