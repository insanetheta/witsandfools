using System.Collections.Generic;
using TMPro;
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
        public TMP_Text DeckCountLabel;
        public RectTransform TrumpSlot;
        public RectTransform DiscardSlot;
        public TMP_Text DiscardCountLabel;
        public RectTransform BoutArea;       // parent for attacker/defender card positions
        public RectTransform CardSpawnRoot;  // where transient cards (deal animations) start

        [Header("Bout layout")]
        public float BoutSlotSpacing = 130f;
        public float DefenseOffset = 28f;     // defense card offsets down-right relative to its attack

        public float AttackRowY = 80f;
        public float DefenseRowY = -80f;

        public Vector2 BoutAttackSlotPos(int slot, int totalSlots)
        {
            int n = Mathf.Max(1, totalSlots);
            float startX = -(n - 1) * 0.5f * BoutSlotSpacing;
            return new Vector2(startX + slot * BoutSlotSpacing, AttackRowY);
        }

        public Vector2 BoutDefenseSlotPos(int slot, int totalSlots)
        {
            int n = Mathf.Max(1, totalSlots);
            float startX = -(n - 1) * 0.5f * BoutSlotSpacing;
            return new Vector2(startX + slot * BoutSlotSpacing, DefenseRowY);
        }
    }
}
