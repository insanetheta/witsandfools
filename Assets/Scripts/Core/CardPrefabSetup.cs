using UnityEngine;
using UnityEngine.UI;
using WitsAndFools.Cards;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Sets up card prefab component references properly
    /// </summary>
    public class CardPrefabSetup : MonoBehaviour
    {
        void Start()
        {
            SetupCardPrefab();
        }
        
        void SetupCardPrefab()
        {
            // Find the card prefab in the scene
            GameObject cardPrefab = GameObject.Find("CardPrefab");
            if (cardPrefab == null)
            {
                UnityEngine.Debug.LogWarning("CardPrefab not found in scene");
                return;
            }
            
            // Get the Card component
            Card cardComponent = cardPrefab.GetComponent<Card>();
            if (cardComponent == null)
            {
                UnityEngine.Debug.LogWarning("Card component not found on CardPrefab");
                return;
            }
            
            // Find and assign the text components
            Transform cardNameTransform = cardPrefab.transform.Find("CardName");
            if (cardNameTransform != null)
            {
                cardComponent.cardNameText = cardNameTransform.GetComponent<Text>();
                UnityEngine.Debug.Log("CardName text reference assigned");
            }
            
            Transform cardValueTransform = cardPrefab.transform.Find("CardValue");
            if (cardValueTransform != null)
            {
                cardComponent.cardValueText = cardValueTransform.GetComponent<Text>();
                UnityEngine.Debug.Log("CardValue text reference assigned");
            }
            
            // Assign the card image
            cardComponent.cardImage = cardPrefab.GetComponent<Image>();
            
            UnityEngine.Debug.Log("Card prefab component references setup complete");
        }
    }
}