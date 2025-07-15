using UnityEngine;
using WitsAndFools.Core;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Forces hand layout refresh for testing spacing adjustments
    /// </summary>
    public class HandLayoutRefresh : MonoBehaviour
    {
        void Start()
        {
            // Wait a moment after cards are dealt, then refresh layout
            Invoke(nameof(RefreshHandLayout), 3f);
        }
        
        void RefreshHandLayout()
        {
            HandManager handManager = FindFirstObjectByType<HandManager>();
            if (handManager != null)
            {
                handManager.ArrangeCards();
                UnityEngine.Debug.Log("Hand layout refreshed with new spacing");
            }
        }
    }
}