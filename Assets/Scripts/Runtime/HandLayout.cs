using System.Collections.Generic;
using UnityEngine;

namespace WitsAndFools
{
    // Lays out a list of CardViews in a horizontal fan. The owning GameManager
    // is responsible for parenting cards under this transform; HandLayout just
    // computes target positions and applies them.
    [RequireComponent(typeof(RectTransform))]
    public sealed class HandLayout : MonoBehaviour
    {
        public float Spacing = 92f;        // px between card centers
        public float MaxFanArc = 26f;      // total degrees of fan rotation across the hand (cleaner arc)
        public float MaxArcLift = 18f;     // px the center cards lift from the ends
        public bool FaceUp = true;
        public bool ReverseOrder = false;  // for the opponent's hand (laid out top-down)

        readonly List<CardView> _cards = new();

        public IReadOnlyList<CardView> Cards => _cards;

        public void Add(CardView view)
        {
            view.transform.SetParent(transform, false);
            _cards.Add(view);
            Apply();
        }

        public void Remove(CardView view)
        {
            _cards.Remove(view);
            Apply();
        }

        public void Clear()
        {
            foreach (var c in _cards) if (c) Destroy(c.gameObject);
            _cards.Clear();
        }

        public void Apply()
        {
            // Drop any destroyed views first.
            for (int i = _cards.Count - 1; i >= 0; i--)
                if (!_cards[i]) _cards.RemoveAt(i);

            int n = _cards.Count;
            if (n == 0) return;

            float totalWidth = (n - 1) * Spacing;
            float startX = -totalWidth * 0.5f;

            for (int i = 0; i < n; i++)
            {
                var rt = (RectTransform)_cards[i].transform;
                float t = n == 1 ? 0.5f : (float)i / (n - 1);

                float x = startX + i * Spacing;
                // Lift the middle: parabolic.
                float lift = MaxArcLift * (1f - Mathf.Abs((t - 0.5f) * 2f));
                // Rotate fan-style.
                float rotZ = Mathf.Lerp(MaxFanArc * 0.5f, -MaxFanArc * 0.5f, t);
                if (ReverseOrder) { rotZ = -rotZ; lift = -lift; }

                rt.anchoredPosition = new Vector2(x, lift);
                rt.localRotation = Quaternion.Euler(0, 0, rotZ);
                rt.SetSiblingIndex(i);
            }
        }
    }
}
