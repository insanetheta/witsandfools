using System;
using UnityEngine;

namespace WitsAndFools
{
    public enum InputMode { Pointer, Touch }

    /// <summary>
    /// Chooses the interaction layer at runtime by pointer capability — not by a separate build.
    /// Pointer (fine) = hover tooltips + lift, click to play. Touch (coarse) = tap-to-inspect.
    /// The same CardView handlers serve both; this only decides which affordances are live.
    /// A cheat override forces a mode for testing.
    /// </summary>
    public static class InputProfile
    {
        static InputMode _auto = Detect();
        public static InputMode? OverrideMode { get; private set; }   // null = auto
        public static InputMode Mode => OverrideMode ?? _auto;
        public static bool Hover => Mode == InputMode.Pointer;
        public static event Action OnChanged;

        static InputMode Detect()
        {
            if (Application.isMobilePlatform) return InputMode.Touch;
            if (Input.touchSupported && !Input.mousePresent) return InputMode.Touch;
            return InputMode.Pointer;
        }

        /// <summary>Re-detect (hybrid devices: a mouse appears/disappears). Only affects auto mode.</summary>
        public static void Reevaluate()
        {
            var m = Detect();
            if (OverrideMode == null && m != _auto) { _auto = m; OnChanged?.Invoke(); }
        }

        public static void SetOverride(InputMode? m) { OverrideMode = m; OnChanged?.Invoke(); }
    }
}
