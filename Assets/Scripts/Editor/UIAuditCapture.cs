using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class UIAuditCapture
    {
        [MenuItem("Wits and Fools/UI Audit/Capture All Screens (Auto-Run)")]
        static void CaptureAllScreens()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[UIAudit] Enter Play mode first.");
                return;
            }

            var rm = Object.FindFirstObjectByType<RunManager>();
            if (rm == null)
            {
                Debug.LogError("[UIAudit] No RunManager in scene.");
                return;
            }

            var capture = Object.FindFirstObjectByType<UIScreenCapture>();
            if (capture == null)
            {
                var go = new GameObject("UIScreenCapture");
                capture = go.AddComponent<UIScreenCapture>();
            }

            capture.BeginCapture();
            rm.StartAutoRun();
            Debug.Log("[UIAudit] Capture session started with auto-run. Will generate report when run completes.");
        }

        [MenuItem("Wits and Fools/UI Audit/Stop Capture & Generate Report")]
        static void StopAndReport()
        {
            var capture = Object.FindFirstObjectByType<UIScreenCapture>();
            if (capture == null || !capture.IsCapturing)
            {
                Debug.LogWarning("[UIAudit] No active capture session.");
                return;
            }

            capture.EndCapture();
        }

        [MenuItem("Wits and Fools/UI Audit/Open Latest Report")]
        static void OpenLatestReport()
        {
            var dir = System.IO.Path.Combine(Application.dataPath, "..", "Screenshots", "ui_audit");
            if (!System.IO.Directory.Exists(dir))
            {
                Debug.LogWarning("[UIAudit] No ui_audit directory found.");
                return;
            }

            var dirs = System.IO.Directory.GetDirectories(dir);
            if (dirs.Length == 0)
            {
                Debug.LogWarning("[UIAudit] No capture sessions found.");
                return;
            }

            System.Array.Sort(dirs);
            var latest = dirs[dirs.Length - 1];
            var report = System.IO.Path.Combine(latest, "report.html");
            if (System.IO.File.Exists(report))
            {
                Application.OpenURL("file://" + report);
                Debug.Log($"[UIAudit] Opened {report}");
            }
            else
                Debug.LogWarning($"[UIAudit] No report.html in {latest}");
        }
    }
}
