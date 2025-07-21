using UnityEngine;
using UnityEngine.UI;
using WitsAndFools.Demo;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Controls the AutoPlay UI and connects it to the AutoPlayDemo system
    /// </summary>
    public class AutoPlayUIController : MonoBehaviour
    {
        [Header("UI References")]
        public Button autoPlayToggleButton;
        public Text buttonText;
        public Text statusText;
        
        [Header("AutoPlay Reference")]
        public AutoPlayDemo autoPlayDemo;
        
        private bool lastKnownState = false;
        
        private void Start()
        {
            // Find AutoPlayDemo if not assigned
            if (autoPlayDemo == null)
            {
                autoPlayDemo = FindObjectOfType<AutoPlayDemo>();
            }
            
            // Find UI components if not assigned
            if (autoPlayToggleButton == null)
            {
                autoPlayToggleButton = GameObject.Find("AutoPlayToggle")?.GetComponent<Button>();
            }
            
            if (buttonText == null)
            {
                buttonText = GameObject.Find("ButtonText")?.GetComponent<Text>();
            }
            
            // Set up button click listener
            if (autoPlayToggleButton != null)
            {
                autoPlayToggleButton.onClick.AddListener(ToggleAutoPlay);
                Debug.Log("AutoPlay UI Controller: Button listener added");
            }
            else
            {
                Debug.LogError("AutoPlay UI Controller: Toggle button not found!");
            }
            
            // Update initial UI state
            UpdateUI();
        }
        
        /// <summary>
        /// Toggle autoplay and update UI
        /// </summary>
        public void ToggleAutoPlay()
        {
            if (autoPlayDemo != null)
            {
                Debug.Log("=== AUTOPLAY UI: Toggling AutoPlay ===");
                autoPlayDemo.ToggleAutoPlay();
                UpdateUI();
                
                // Log the state change
                Debug.Log($"AutoPlay is now: {(autoPlayDemo.autoPlayEnabled ? "ENABLED" : "DISABLED")}");
            }
            else
            {
                Debug.LogError("AutoPlay UI Controller: AutoPlayDemo reference is null!");
            }
        }
        
        /// <summary>
        /// Update UI elements to reflect current autoplay state
        /// </summary>
        private void UpdateUI()
        {
            if (autoPlayDemo == null) return;
            
            bool isEnabled = autoPlayDemo.autoPlayEnabled;
            
            // Only update and log if state has changed
            if (isEnabled != lastKnownState)
            {
                lastKnownState = isEnabled;
                
                // Update button text
                if (buttonText != null)
                {
                    buttonText.text = isEnabled ? "Disable AutoPlay" : "Enable AutoPlay";
                }
                
                // Update button color
                if (autoPlayToggleButton != null)
                {
                    Image buttonImage = autoPlayToggleButton.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = isEnabled ? new Color(0.2f, 0.8f, 0.2f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f);
                    }
                }
                
                Debug.Log($"AutoPlay UI updated - Enabled: {isEnabled}");
            }
        }
        
        /// <summary>
        /// Update UI periodically to reflect current state
        /// </summary>
        private void Update()
        {
            // Update UI periodically to reflect any state changes
            if (Time.frameCount % 60 == 0) // Update once per second at 60fps
            {
                UpdateUI();
            }
        }
    }
}