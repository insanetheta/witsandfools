using UnityEngine;
using WitsAndFools.Demo;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Simple setup script to add AutoPlayDemo to the scene
    /// Attach this to any GameObject or run once to set up auto-play
    /// </summary>
    public class AutoPlaySetup : MonoBehaviour
    {
        [Header("Setup")]
        public bool setupOnStart = true;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupAutoPlay();
            }
        }
        
        /// <summary>
        /// Set up the auto-play demo system
        /// </summary>
        public void SetupAutoPlay()
        {
            // Check if AutoPlayDemo already exists
            AutoPlayDemo existingAutoPlay = FindObjectOfType<AutoPlayDemo>();
            
            if (existingAutoPlay == null)
            {
                // Create new GameObject with AutoPlayDemo
                GameObject autoPlayObj = new GameObject("AutoPlayDemo");
                autoPlayObj.AddComponent<AutoPlayDemo>();
                
                Debug.Log("AutoPlayDemo system added to scene");
                Debug.Log("Press SPACE to toggle auto-play mode");
            }
            else
            {
                Debug.Log("AutoPlayDemo already exists in scene");
            }
        }
    }
}
