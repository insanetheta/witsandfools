using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Sets up the demo UI canvas and components for proper display
    /// </summary>
    public class DemoUISetup : MonoBehaviour
    {
        private void Start()
        {
            SetupCanvas();
            SetupCardPrefab();
        }
        
        /// <summary>
        /// Configure the main UI canvas
        /// </summary>
        private void SetupCanvas()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;
                
                CanvasScaler scaler = GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }
                
                Debug.Log("UI Canvas configured for screen overlay");
            }
        }
        
        /// <summary>
        /// Configure the card prefab components
        /// </summary>
        private void SetupCardPrefab()
        {
            GameObject cardPrefab = GameObject.Find("CardPrefab");
            if (cardPrefab != null)
            {
                // Set up card canvas
                Canvas cardCanvas = cardPrefab.GetComponent<Canvas>();
                if (cardCanvas != null)
                {
                    cardCanvas.overrideSorting = true;
                    cardCanvas.sortingOrder = 1;
                }
                
                // Set up card size
                RectTransform cardRect = cardPrefab.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    cardRect.sizeDelta = new Vector2(100, 140);
                }
                
                // Set up background
                GameObject background = cardPrefab.transform.Find("CardBackground")?.gameObject;
                if (background != null)
                {
                    RectTransform bgRect = background.GetComponent<RectTransform>();
                    if (bgRect != null)
                    {
                        bgRect.anchorMin = Vector2.zero;
                        bgRect.anchorMax = Vector2.one;
                        bgRect.sizeDelta = Vector2.zero;
                        bgRect.anchoredPosition = Vector2.zero;
                    }
                    
                    // Set background color
                    Image bgImage = background.GetComponent<Image>();
                    if (bgImage != null)
                    {
                        bgImage.color = new Color(0.9f, 0.85f, 0.7f, 1f); // Light parchment color
                    }
                }
                
                // Set up card name text
                GameObject cardName = cardPrefab.transform.Find("CardName")?.gameObject;
                if (cardName != null)
                {
                    RectTransform nameRect = cardName.GetComponent<RectTransform>();
                    if (nameRect != null)
                    {
                        nameRect.anchorMin = new Vector2(0, 0.7f);
                        nameRect.anchorMax = new Vector2(1, 0.9f);
                        nameRect.sizeDelta = Vector2.zero;
                        nameRect.anchoredPosition = Vector2.zero;
                    }
                    
                    Text nameText = cardName.GetComponent<Text>();
                    if (nameText != null)
                    {
                        nameText.text = "Card Name";
                        nameText.fontSize = 12;
                        nameText.alignment = TextAnchor.MiddleCenter;
                        nameText.color = Color.black;
                    }
                }
                
                // Set up card value text
                GameObject cardValue = cardPrefab.transform.Find("CardValue")?.gameObject;
                if (cardValue != null)
                {
                    RectTransform valueRect = cardValue.GetComponent<RectTransform>();
                    if (valueRect != null)
                    {
                        valueRect.anchorMin = new Vector2(0, 0.1f);
                        valueRect.anchorMax = new Vector2(0.3f, 0.3f);
                        valueRect.sizeDelta = Vector2.zero;
                        valueRect.anchoredPosition = Vector2.zero;
                    }
                    
                    Text valueText = cardValue.GetComponent<Text>();
                    if (valueText != null)
                    {
                        valueText.text = "1";
                        valueText.fontSize = 16;
                        valueText.fontStyle = FontStyle.Bold;
                        valueText.alignment = TextAnchor.MiddleCenter;
                        valueText.color = Color.black;
                    }
                }
                
                Debug.Log("Card prefab UI components configured");
            }
        }
    }
}