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
        public float UltrawideAspect = 2.5f;

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

            if (Scaler)
            {
                // Bias scaling to height so the vertical card stack always fits and never clips.
                Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                Scaler.matchWidthOrHeight = 1f;
            }
            ApplyUltrawideClamp(ar, h);

            LayoutTier t = Override ?? ComputeTier(w, h);
            if (force || !_applied || t != Tier)
            {
                Tier = t;
                _applied = true;
                Apply(t);
                OnTierChanged?.Invoke(t);
            }
        }

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

        // Collapsed log button toggles the docked panel as a transient overlay.
        public void ToggleLog()
        {
            if (EventLogPanel) EventLogPanel.SetActive(!EventLogPanel.activeSelf);
        }

        void ApplyUltrawideClamp(float ar, int h)
        {
            if (!PlayRoot) return;
            if (ar >= UltrawideAspect)
            {
                // cap width to ~the mock's 240vh and center; sides pillarbox to the canvas bg.
                PlayRoot.anchorMin = new Vector2(0.5f, 0f);
                PlayRoot.anchorMax = new Vector2(0.5f, 1f);
                PlayRoot.pivot = new Vector2(0.5f, 0.5f);
                // sizeDelta.x is the explicit width when anchors are a vertical line; use reference height.
                float refH = Scaler ? Scaler.referenceResolution.y : 1080f;
                PlayRoot.sizeDelta = new Vector2(refH * 2.4f, 0f);
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
