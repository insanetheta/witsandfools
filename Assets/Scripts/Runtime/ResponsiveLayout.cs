using System;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools
{
    public enum LayoutTier { Compact, Comfortable, Spacious }

    /// <summary>
    /// One layout, reflowed by viewport. Tier is keyed on HEIGHT first (a card game must stack
    /// opponent + bout + hand + action vertically); width only gates the desktop expansions, so a
    /// wide-but-short window stays Compact instead of cramming desktop chrome into a short screen.
    /// Portrait is landscape-locked; ultrawide clamps & centers the play field. Recomputes live on
    /// any viewport change. A cheat override forces a tier regardless of viewport.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResponsiveLayout : MonoBehaviour
    {
        public static ResponsiveLayout Instance { get; private set; }

        [Header("Driven references (assigned by SceneBuilder)")]
        public CanvasScaler Scaler;
        public RectTransform PlayRoot;       // MatchPanel — clamped & centered when ultrawide
        public GameObject PortraitOverlay;   // "rotate to landscape" cover
        public GameObject EventLogPanel;     // docked rail (Spacious)
        public GameObject EventLogButton;    // collapsed log button (Compact/Comfortable)
        public GameObject[] SpaciousOnly;    // panel extras (subtitles, race labels, trump rule) shown only when Spacious

        // Breakpoints (logical px). Height-first.
        public int CompactMaxHeight = 520;
        public int SpaciousMinHeight = 700;
        public int SpaciousMinWidth = 1100;
        // Above this aspect the play field is clamped & centered (16:9 desktop ~1.78 is unaffected;
        // very wide / multi-monitor views re-center instead of stranding panels in far corners).
        public float UltrawideAspect = 1.95f;
        public float ClampWidthFactor = 1.85f;   // max play-field width = refHeight * this

        public LayoutTier Tier { get; private set; } = LayoutTier.Spacious;
        public bool IsPortrait { get; private set; }
        public LayoutTier? Override { get; private set; }   // null = auto (aspect-driven)
        public event Action<LayoutTier> OnTierChanged;

        int _lastW = -1, _lastH = -1;
        bool _applied;

        void Awake() => Instance = this;
        void OnDestroy() { if (Instance == this) Instance = null; }
        void Start() => Recompute(true);

        void Update()
        {
            if (Screen.width != _lastW || Screen.height != _lastH)
                Recompute(false);   // live switch the instant the Game view (Free Aspect) changes
        }

        /// <summary>Cheat hook: pass a tier to force it, or null to return to aspect-driven Auto.</summary>
        public void SetOverride(LayoutTier? tier) { Override = tier; Recompute(true); }

        // Per-tier reference height (CanvasScaler is height-matched): a smaller reference => larger
        // UI, so Compact gets bigger touch targets and the three tiers are genuinely distinct in size,
        // not just in which chrome is shown.
        public float CompactRefHeight = 760f;
        public float ComfortableRefHeight = 920f;
        public float SpaciousRefHeight = 1080f;

        public void Recompute(bool force)
        {
            int w = _lastW = Screen.width;
            int h = _lastH = Screen.height;
            if (h <= 0) return;
            float ar = (float)w / h;

            bool portrait = w < h;
            if (portrait != IsPortrait || force)
            {
                IsPortrait = portrait;
                if (PortraitOverlay) PortraitOverlay.SetActive(portrait);
            }

            LayoutTier t = Override ?? ComputeTier(w, h);

            if (Scaler)
            {
                // Bias scaling to height so the vertical card stack always fits and never clips,
                // and set the per-tier reference height so element sizes differ by tier.
                Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                Scaler.matchWidthOrHeight = 1f;
                Scaler.referenceResolution = new Vector2(Scaler.referenceResolution.x, RefHeightFor(t));
            }
            ApplyUltrawideClamp(ar);

            if (force || !_applied || t != Tier)
            {
                Tier = t;
                _applied = true;
                Apply(t);
                OnTierChanged?.Invoke(t);
            }
        }

        float RefHeightFor(LayoutTier t) => t switch
        {
            LayoutTier.Compact => CompactRefHeight,
            LayoutTier.Comfortable => ComfortableRefHeight,
            _ => SpaciousRefHeight,
        };

        LayoutTier ComputeTier(int w, int h)
        {
            if (h <= CompactMaxHeight) return LayoutTier.Compact;
            if (h >= SpaciousMinHeight && w >= SpaciousMinWidth) return LayoutTier.Spacious;
            return LayoutTier.Comfortable;
        }

        void Apply(LayoutTier t)
        {
            bool spacious = t == LayoutTier.Spacious;
            if (EventLogPanel) EventLogPanel.SetActive(spacious);
            if (EventLogButton) EventLogButton.SetActive(!spacious);
            if (SpaciousOnly != null)
                foreach (var go in SpaciousOnly) if (go) go.SetActive(spacious);
        }

        // Deterministic evaluation for QA (no Screen dependency): tier/portrait/ultrawide for any size.
        public struct LayoutEval { public LayoutTier Tier; public bool Portrait; public bool Ultrawide; }
        public LayoutEval EvaluateFor(int w, int h)
        {
            float ar = h > 0 ? (float)w / h : 1f;
            return new LayoutEval { Tier = ComputeTier(w, h), Portrait = w < h, Ultrawide = ar >= UltrawideAspect };
        }

        // Collapsed log button toggles the docked panel as a transient overlay.
        public void ToggleLog()
        {
            if (EventLogPanel) EventLogPanel.SetActive(!EventLogPanel.activeSelf);
        }

        void ApplyUltrawideClamp(float ar)
        {
            if (!PlayRoot) return;
            if (ar >= UltrawideAspect)
            {
                // cap the play-field width and center it; sides pillarbox to the canvas bg so panels
                // anchored to its edges pull inward instead of stranding across a huge felt void.
                PlayRoot.anchorMin = new Vector2(0.5f, 0f);
                PlayRoot.anchorMax = new Vector2(0.5f, 1f);
                PlayRoot.pivot = new Vector2(0.5f, 0.5f);
                float refH = Scaler ? Scaler.referenceResolution.y : 1080f;
                PlayRoot.sizeDelta = new Vector2(refH * ClampWidthFactor, 0f);
                PlayRoot.anchoredPosition = Vector2.zero;
            }
            else
            {
                PlayRoot.anchorMin = Vector2.zero;
                PlayRoot.anchorMax = Vector2.one;
                PlayRoot.offsetMin = Vector2.zero;
                PlayRoot.offsetMax = Vector2.zero;
            }
        }
    }
}
