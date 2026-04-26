using System.IO;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace WitsAndFools.EditorTools
{
    public static class TmpEssentialsImporter
    {
        [MenuItem("Wits and Fools/Setup/Import TMP Essentials (if missing)")]
        public static void EnsureImported()
        {
            if (TMP_Settings.instance != null && TMP_Settings.defaultFontAsset != null)
            {
                Debug.Log("TMP Essentials already imported.");
                return;
            }
            ImportPackage();
        }

        public static bool TryEnsureSync()
        {
            // Returns true if essentials are present (now or after import).
            try
            {
                if (TMP_Settings.instance != null && TMP_Settings.defaultFontAsset != null) return true;
            }
            catch { /* swallow null-ref from missing settings */ }
            ImportPackage();
            // Note: ImportPackage is async — caller may need to refresh and retry.
            return false;
        }

        static void ImportPackage()
        {
            string packageRoot = "Library/PackageCache";
            if (!Directory.Exists(packageRoot)) { Debug.LogError("PackageCache not found"); return; }

            string ugui = null;
            foreach (var d in Directory.GetDirectories(packageRoot))
                if (Path.GetFileName(d).StartsWith("com.unity.ugui")) { ugui = d; break; }
            if (ugui == null) { Debug.LogError("com.unity.ugui not found"); return; }

            string essentials = Path.Combine(ugui, "Package Resources", "TMP Essential Resources.unitypackage");
            if (!File.Exists(essentials)) { Debug.LogError("TMP Essentials package not found at " + essentials); return; }

            Debug.Log("Importing TMP Essentials from " + essentials);
            AssetDatabase.ImportPackage(essentials, false);
        }
    }
}
