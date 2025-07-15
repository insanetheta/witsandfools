using UnityEngine;
using UnityEngine.UI;
using WitsAndFools.Cards;

namespace WitsAndFools.Diagnostics
{
    public class CardVisibilityChecker : MonoBehaviour
    {
        void Start()
        {
            InvokeRepeating(nameof(CheckCardVisibility), 2f, 3f);
        }

        void CheckCardVisibility()
        {
            Card[] cards = FindObjectsOfType<Card>();
            UnityEngine.Debug.Log($"Found {cards.Length} cards in scene");
            
            foreach (Card card in cards)
            {
                Image cardImage = card.GetComponent<Image>();
                RectTransform rectTransform = card.GetComponent<RectTransform>();
                
                UnityEngine.Debug.Log($"Card: {card.name}");
                UnityEngine.Debug.Log($"  Position: {rectTransform.position}");
                UnityEngine.Debug.Log($"  Size: {rectTransform.sizeDelta}");
                UnityEngine.Debug.Log($"  Active: {card.gameObject.activeInHierarchy}");
                UnityEngine.Debug.Log($"  Image Color: {cardImage.color}");
                UnityEngine.Debug.Log($"  Card Data: {(card.cardData != null ? card.cardData.cardName : "NULL")}");
                
                // Check child text components
                Text[] texts = card.GetComponentsInChildren<Text>();
                foreach (Text text in texts)
                {
                    UnityEngine.Debug.Log($"  Text '{text.name}': '{text.text}' (Color: {text.color})");
                }
            }
        }
    }
}