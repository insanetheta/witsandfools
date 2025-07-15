using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools.Demo
{
    /// <summary>
    /// Debug script to help diagnose rendering issues
    /// </summary>
    public class RenderingDebugger : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(DebugRenderingAfterDelay());
        }
        
        private IEnumerator DebugRenderingAfterDelay()
        {
            yield return new WaitForSeconds(2f);
            
            Debug.Log("=== RENDERING DEBUG INFO ===");
            
            // Check camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                Debug.Log($"Camera position: {cam.transform.position}");
                Debug.Log($"Camera rotation: {cam.transform.rotation.eulerAngles}");
                Debug.Log($"Camera orthographic: {cam.orthographic}");
                Debug.Log($"Camera size/FOV: {(cam.orthographic ? cam.orthographicSize.ToString() : cam.fieldOfView.ToString())}");
            }
            else
            {
                Debug.LogError("No main camera found!");
            }
            
            // Check canvas
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                Debug.Log($"Canvas: {canvas.name}, RenderMode: {canvas.renderMode}, SortingOrder: {canvas.sortingOrder}");
                if (canvas.worldCamera != null)
                {
                    Debug.Log($"Canvas WorldCamera: {canvas.worldCamera.name}");
                }
            }
            
            // Check cards
            Cards.Card[] cards = FindObjectsOfType<Cards.Card>();
            Debug.Log($"Found {cards.Length} card objects");
            
            foreach (Cards.Card card in cards)
            {
                Debug.Log($"Card: {card.name}");
                Debug.Log($"  Position: {card.transform.position}");
                Debug.Log($"  Local Position: {card.transform.localPosition}");
                Debug.Log($"  Active: {card.gameObject.activeInHierarchy}");
                
                // Check if card has visible components
                Image[] images = card.GetComponentsInChildren<Image>();
                Text[] texts = card.GetComponentsInChildren<Text>();
                
                Debug.Log($"  Images: {images.Length}, Texts: {texts.Length}");
                
                foreach (Image img in images)
                {
                    Debug.Log($"    Image {img.name}: Color={img.color}, Sprite={img.sprite?.name}, Active={img.gameObject.activeInHierarchy}");
                }
                
                foreach (Text txt in texts)
                {
                    Debug.Log($"    Text {txt.name}: '{txt.text}', Color={txt.color}, Active={txt.gameObject.activeInHierarchy}");
                }
            }
        }
    }
}