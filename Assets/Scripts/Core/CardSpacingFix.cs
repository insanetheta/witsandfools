using UnityEngine;
using WitsAndFools.Core;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Updates card spacing to fix overlapping issue
    /// </summary>
    public class CardSpacingFix : MonoBehaviour
    {
        void Start()
        {
            // Wait a moment for cards to be dealt, then fix spacing
            Invoke(nameof(FixCardSpacing), 2f);
        }
        
        void FixCardSpacing()
        {
            HandManager handManager = FindFirstObjectByType<HandManager>();
            if (handManager != null)
            {
                // Increase spacing significantly to prevent overlap
                handManager.cardSpacing = 300f;
                handManager.ArrangeCards();
                UnityEngine.Debug.Log($"Fixed card spacing to: {handManager.cardSpacing}");
            }
            else
            {
                UnityEngine.Debug.LogError("HandManager not found!");
            }
        }
    }
}
