using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools
{
    // The visual table. Holds named anchor RectTransforms wired by SceneBuilder.
    // Provides positioning utilities for the GameManager.
    public sealed class TableView : MonoBehaviour
    {
        [Header("Hands")]
        public HandLayout PlayerHand;
        public HandLayout OpponentHand;

        [Header("Center")]
        public RectTransform DeckSlot;
        public RectTransform TrumpSlot;
        public RectTransform DiscardSlot;
        public RectTransform BoutArea;       // parent for attacker/defender card positions
        public RectTransform CardSpawnRoot;  // where transient cards (deal animations) start

        [Header("Bout layout")]
        public float BoutSlotSpacing = 130f;
        public float DefenseOffset = 28f;     // defense card offsets down-right relative to its attack

        public Vector2 BoutAttackSlotPos(int slot)
        {
            int n = Mathf.Max(1, slot + 1);
            float baseX = (slot - 2.5f) * BoutSlotSpacing; // up to 6 slots; centered roughly
            return new Vector2(baseX, 0);
        }

        public Vector2 BoutDefenseSlotPos(int slot)
        {
            var atk = BoutAttackSlotPos(slot);
            return new Vector2(atk.x + DefenseOffset, atk.y - DefenseOffset);
        }
    }
}
