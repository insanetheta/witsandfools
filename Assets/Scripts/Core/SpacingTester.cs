using UnityEngine;
using WitsAndFools.Core;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Simple script to test layout changes
    /// </summary>
    public class SpacingTester : MonoBehaviour
    {
        void Start()
        {
            // Apply layout refresh after a delay
            Invoke(nameof(RefreshLayout), 3f);
        }
        
        void RefreshLayout()
        {
            HandManager handManager = FindFirstObjectByType<HandManager>();
            if (handManager != null)
            {
                handManager.ArrangeCards();
                UnityEngine.Debug.Log("Layout refreshed!");
            }
        }
    }
}
