using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WitsAndFools
{
    public static class FontAssets
    {
        public enum Role { Heading, Body, BodyItalic, Mono }

        static TMP_FontAsset _heading;
        static TMP_FontAsset _body;
        static TMP_FontAsset _bodyItalic;
        static TMP_FontAsset _mono;
        static TMP_FontAsset _fallback;

        public static TMP_FontAsset Get(Role role) => role switch
        {
            Role.Heading => Load(ref _heading, "Cinzel SDF"),
            Role.Body => Load(ref _body, "CrimsonPro SDF"),
            Role.BodyItalic => Load(ref _bodyItalic, "CrimsonPro-Italic SDF"),
            Role.Mono => Load(ref _mono, "JetBrainsMono SDF"),
            _ => Fallback
        };

        public static TMP_FontAsset Heading => Get(Role.Heading);
        public static TMP_FontAsset Body => Get(Role.Body);
        public static TMP_FontAsset Mono => Get(Role.Mono);
        public static TMP_FontAsset Fallback => Load(ref _fallback, "LiberationSans SDF");

        static TMP_FontAsset Load(ref TMP_FontAsset cached, string name)
        {
            if (cached) return cached;

#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets($"{name} t:TMP_FontAsset");
            foreach (var g in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(g));
                if (asset && asset.name == name) { cached = asset; return cached; }
            }
#endif
            cached = Resources.Load<TMP_FontAsset>(name);
            if (cached) return cached;

            if (name != "LiberationSans SDF")
                return Fallback;
            return null;
        }
    }
}
