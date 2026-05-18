using TMPro;
using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class FontAssetBuilder
    {
        static readonly (string ttfPath, string assetPath, string label)[] Fonts =
        {
            ("Assets/Fonts/Cinzel-Variable.ttf", "Assets/Fonts/Cinzel SDF.asset", "Cinzel SDF"),
            ("Assets/Fonts/CrimsonPro-Variable.ttf", "Assets/Fonts/CrimsonPro SDF.asset", "CrimsonPro SDF"),
            ("Assets/Fonts/CrimsonPro-Italic-Variable.ttf", "Assets/Fonts/CrimsonPro-Italic SDF.asset", "CrimsonPro-Italic SDF"),
            ("Assets/Fonts/JetBrainsMono-Variable.ttf", "Assets/Fonts/JetBrainsMono SDF.asset", "JetBrainsMono SDF"),
        };

        [MenuItem("Wits and Fools/Build/Font Assets (SDF)")]
        public static void BuildFontAssets()
        {
            var fallback = FindFallbackFont();
            int created = 0;

            foreach (var (ttfPath, assetPath, label) in Fonts)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
                if (!font)
                {
                    Debug.LogWarning($"Font not found: {ttfPath}");
                    continue;
                }

                var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                if (existing)
                {
                    Debug.Log($"Already exists: {assetPath}");
                    continue;
                }

                var fontAsset = TMP_FontAsset.CreateFontAsset(font);
                fontAsset.name = label;

                if (fallback && fontAsset.fallbackFontAssetTable != null)
                    fontAsset.fallbackFontAssetTable.Add(fallback);
                else if (fallback)
                    fontAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { fallback };

                AssetDatabase.CreateAsset(fontAsset, assetPath);
                created++;
                Debug.Log($"Created {assetPath}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Font build complete: {created} new assets created");
        }

        static TMP_FontAsset FindFallbackFont()
        {
            var guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
            foreach (var g in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(g));
                if (asset) return asset;
            }
            return null;
        }
    }
}
