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
        public RectTransform DeckSlot;       // legacy shared-deck slot (unused in dual-deck)
        public TMP_Text DeckCountLabel;
        public RectTransform TrumpSlot;
        public RectTransform DiscardSlot;    // removed-from-game pile (off the felt)
        public TMP_Text DiscardCountLabel;
        public RectTransform BoutArea;       // parent for attacker/defender card positions
        public RectTransform CardSpawnRoot;  // where transient cards (deal animations) start

        [Header("Dual-deck piles")]
        public RectTransform PlayerDeckSlot;     // your draw pile, on your side of the felt
        public TMP_Text PlayerDeckCountBadge;
        public RectTransform OpponentDeckSlot;   // foe's draw pile, on their side
        public TMP_Text OpponentDeckCountBadge;
        public TMP_Text TrumpRuleLabel;          // "Hearts beat any other suit"
        public TMP_Text TrumpGlyphLabel;         // big suit glyph on the trump card visual

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
