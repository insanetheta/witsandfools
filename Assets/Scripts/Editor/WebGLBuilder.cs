using System.IO;
using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class WebGLBuilder
    {
        [MenuItem("Wits and Fools/Build/WebGL (docs)")]
        public static void BuildWebGL()
        {
            // Assets/Data is the canonical source but is not bundled into the build,
            // so mirror the runtime JSON into Resources/Data (which WebGL can load).
            SyncDataToResources();

            PlayerSettings.productName = "Wits and Fools";
            PlayerSettings.WebGL.template = "PROJECT:WitsAndFools";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.nameFilesAsHashes = false;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.runInBackground = true;

            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.WebGL, BuildTarget.WebGL);

            string scenePath = "Assets/Scenes/GameScene.unity";
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            AssetDatabase.Refresh();

            string outputPath = "docs";
            var options = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log($"[WebGL] Build succeeded: {outputPath} ({report.summary.totalSize / (1024*1024)}MB)");
            else
                Debug.LogError($"[WebGL] Build failed: {report.summary.result}");
        }

        // Mirrors the runtime data JSON from the canonical Assets/Data into
        // Resources/Data so it gets bundled and is loadable on WebGL.
        static void SyncDataToResources()
        {
            string srcDir = Path.Combine(Application.dataPath, "Data");
            string dstDir = Path.Combine(Application.dataPath, "Resources", "Data");
            Directory.CreateDirectory(dstDir);
            foreach (var name in new[] { "card_catalog.json", "enemy_roster.json" })
            {
                string src = Path.Combine(srcDir, name);
                if (File.Exists(src)) File.Copy(src, Path.Combine(dstDir, name), true);
            }
            AssetDatabase.Refresh();
        }
    }
}
