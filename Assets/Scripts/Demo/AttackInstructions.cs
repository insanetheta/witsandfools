using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Creates an instruction panel showing how to attack in the demo
    /// </summary>
    public class AttackInstructions : MonoBehaviour
    {
        void Start()
        {
            CreateInstructionPanel();
        }
        
        void CreateInstructionPanel()
        {
            // Find or create the game info panel
            GameObject infoPanel = GameObject.Find("GameInfoPanel");
            if (infoPanel == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    infoPanel = new GameObject("GameInfoPanel");
                    infoPanel.transform.SetParent(canvas.transform);
                    RectTransform rect = infoPanel.AddComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(400, 200);
                    rect.sizeDelta = new Vector2(300, 200);
                }
            }
            
            if (infoPanel != null)
            {
                // Create background
                Image bg = infoPanel.GetComponent<Image>();
                if (bg == null)
                {
                    bg = infoPanel.AddComponent<Image>();
                    bg.color = new Color(0, 0, 0, 0.8f);
                }
                
                // Create instruction text
                GameObject textObj = new GameObject("InstructionText");
                textObj.transform.SetParent(infoPanel.transform);
                
                Text instructionText = textObj.AddComponent<Text>();
                instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                instructionText.fontSize = 14;
                instructionText.color = Color.white;
                instructionText.alignment = TextAnchor.MiddleLeft;
                
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(10, 10);
                textRect.offsetMax = new Vector2(-10, -10);
                
                instructionText.text = "=== ATTACK DEMO ===\n\n" +
                                     "HOW TO ATTACK:\n" +
                                     "1. Click cards in bottom hand\n" +
                                     "2. Watch console for results\n" +
                                     "3. Cards appear in center\n\n" +
                                     "TRUMP: Hearts ♥\n\n" +
                                     "Player 1: Attacker (bottom)\n" +
                                     "Player 2: Defender (top)";
                
                UnityEngine.Debug.Log("Attack instructions panel created");
            }
        }
    }
}