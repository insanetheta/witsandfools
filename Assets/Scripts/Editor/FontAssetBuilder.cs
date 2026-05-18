using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace WitsAndFools.EditorTools
{
    public static class FontAssetBuilder
    {
        const int SamplingPointSize = 44;
        const int AtlasPadding = 5;
        const int AtlasWidth = 512;
        const int AtlasHeight = 512;

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
                    AssetDatabase.DeleteAsset(assetPath);
                    Debug.Log($"Deleted stale: {assetPath}");
                }

                var fontAsset = TMP_FontAsset.CreateFontAsset(
                    font, SamplingPointSize, AtlasPadding,
                    GlyphRenderMode.SDFAA, AtlasWidth, AtlasHeight);
                fontAsset.name = label;

                if (fallback)
                {
                    fontAsset.fallbackFontAssetTable ??=
                        new System.Collections.Generic.List<TMP_FontAsset>();
                    fontAsset.fallbackFontAssetTable.Add(fallback);
                }

                AssetDatabase.CreateAsset(fontAsset, assetPath);

                if (fontAsset.atlasTexture)
                {
                    fontAsset.atlasTexture.name = $"{label} Atlas";
                    AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
                }
                if (fontAsset.material)
                {
                    fontAsset.material.name = $"{label} Material";
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }

                EditorUtility.SetDirty(fontAsset);
                created++;
                Debug.Log($"Created {assetPath} (atlas {AtlasWidth}x{AtlasHeight}, {fontAsset.atlasTexture != null})");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Font build complete: {created} assets created");
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
