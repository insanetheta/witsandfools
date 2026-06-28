using UnityEngine;

namespace WitsAndFools
{
    /// <summary>
    /// Dev cheat menu, opened/closed with the ~ (backquote) or \ (backslash) key.
    /// Forces the responsive tier and input mode independent of viewport, plus a few
    /// run cheats. IMGUI so it needs no prefab wiring and never ships in the play path.
    /// </summary>
    public sealed class CheatMenu : MonoBehaviour
    {
        public bool EnabledInBuild = true;   // set false to compile out of release builds if desired
        bool _open;

        void Update()
        {
            if (!EnabledInBuild && !Application.isEditor) return;
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Backslash))
                _open = !_open;
        }

        void OnGUI()
        {
            if (!_open) return;
            if (!EnabledInBuild && !Application.isEditor) return;

            const float w = 320f, h = 250f;
            var rect = new Rect(14, 14, w, h);
            GUI.color = new Color(0.04f, 0.04f, 0.08f, 0.92f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(rect.x + 12, rect.y + 10, w - 24, h - 20));
            GUILayout.Label("CHEAT MENU   (~ or \\ to close)");
            GUILayout.Space(4);

            var rl = ResponsiveLayout.Instance;
            if (rl != null)
            {
                string ov = rl.Override.HasValue ? rl.Override.ToString() : "Auto";
                GUILayout.Label($"Tier: {rl.Tier}   {Screen.width}x{Screen.height}   [{ov}]"
                    + (rl.IsPortrait ? "  PORTRAIT-LOCK" : ""));
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Auto")) rl.SetOverride(null);
                if (GUILayout.Button("Compact")) rl.SetOverride(LayoutTier.Compact);
                if (GUILayout.Button("Comfort")) rl.SetOverride(LayoutTier.Comfortable);
                if (GUILayout.Button("Spacious")) rl.SetOverride(LayoutTier.Spacious);
                GUILayout.EndHorizontal();
            }
            else GUILayout.Label("ResponsiveLayout not in scene.");

            GUILayout.Space(8);
            string iov = InputProfile.OverrideMode.HasValue ? InputProfile.OverrideMode.ToString() : "Auto";
            GUILayout.Label($"Input: {InputProfile.Mode}   [{iov}]");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Auto")) InputProfile.SetOverride(null);
            if (GUILayout.Button("Touch (tap)")) InputProfile.SetOverride(InputMode.Touch);
            if (GUILayout.Button("Pointer (hover)")) InputProfile.SetOverride(InputMode.Pointer);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("► JUMP INTO MATCH"))
            {
                var rm = FindFirstObjectByType<RunManager>(FindObjectsInactive.Include);
                if (rm != null) rm.DebugJumpToMatch();
            }
            GUILayout.Label("Match: press A to cycle auto-play.");
            GUILayout.EndArea();
        }
    }
}
