using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class WebGLBuilder
    {
        [MenuItem("Wits and Fools/Build/WebGL (docs)")]
        public static void BuildWebGL()
        {
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
    }
}
