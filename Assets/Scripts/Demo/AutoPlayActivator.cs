using UnityEngine;
using WitsAndFools.Demo;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Simple script to programmatically activate AutoPlayDemo
    /// </summary>
    public class AutoPlayActivator : MonoBehaviour
    {
        [Header("Auto Activation")]
        public bool activateOnStart = true;
        public float activationDelay = 1f;
        
        private AutoPlayDemo autoPlayDemo;
        
        private void Start()
        {
            if (activateOnStart)
            {
                Debug.Log("=== AUTO-PLAY ACTIVATOR STARTED ===");
                
                // Find AutoPlayDemo
                autoPlayDemo = FindObjectOfType<AutoPlayDemo>();
                if (autoPlayDemo != null)
                {
                    Debug.Log("Found AutoPlayDemo - will activate in " + activationDelay + " seconds");
                    Invoke(nameof(ActivateAutoPlay), activationDelay);
                }
                else
                {
                    Debug.LogError("AutoPlayDemo not found!");
                }
            }
        }
        
        private void ActivateAutoPlay()
        {
            if (autoPlayDemo != null)
            {
                Debug.Log("=== ACTIVATING AUTO-PLAY MODE ===");
                autoPlayDemo.ToggleAutoPlay();
                Debug.Log("AutoPlay activated! Game should now play automatically.");
                
                // Log the new state
                Debug.Log($"AutoPlay Enabled: {autoPlayDemo.autoPlayEnabled}");
            }
        }
        
        /// <summary>
        /// Manual activation method for testing
        /// </summary>
        [ContextMenu("Activate AutoPlay")]
        public void ManualActivate()
        {
            ActivateAutoPlay();
        }
    }
}
