using UnityEngine;
using WitsAndFools.Core;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Forces hand layout refresh with improved spacing settings
    /// </summary>
    public class ImprovedHandLayout : MonoBehaviour
    {
        [Header("Layout Settings")]
        public bool useCustomSpacing = true;
        public float customCardSpacing = 350f;
        public float customArcAngle = 60f;
        
        void Start()
        {
            // Wait for cards to be dealt, then apply improved layout
            Invoke(nameof(ApplyImprovedLayout), 2.5f);
        }
        
        void ApplyImprovedLayout()
        {
            HandManager handManager = FindFirstObjectByType<HandManager>();
            if (handManager != null)
            {
                if (useCustomSpacing)
                {
                    handManager.cardSpacing = customCardSpacing;
                    handManager.arcAngle = customArcAngle;
                }
                
                handManager.ArrangeCards();
                UnityEngine.Debug.Log($"Applied improved layout - Spacing: {handManager.cardSpacing}, Arc: {handManager.arcAngle}");
            }
            else
            {
                UnityEngine.Debug.LogError("HandManager not found for layout improvement!");
            }
        }
        
        // Allow manual testing
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ApplyImprovedLayout();
            }
        }
    }
}
