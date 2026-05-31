using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WitsAndFools
{
    public enum UIScreen
    {
        // Run phases
        ArchetypeSelect,
        Map,
        MatchInProgress,
        MatchVictory,
        MatchDefeat,
        Shop,
        Event,
        Rest,
        RunComplete,
        RunFailed,

        // Post-match modals
        CardReward,
        AbilityPick,
        RelicPick,

        // Overlays
        DeckBrowser,
        CardDetail,
        EventOutcome,

        // In-match overlays
        AbilityChoiceModal,
        PeekOverlay,
    }

    public sealed class UIScreenCapture : MonoBehaviour
    {
        public static UIScreenCapture Instance { get; private set; }

        public bool IsCapturing { get; private set; }

        string _outputDir;
        readonly HashSet<UIScreen> _captured = new();
        readonly List<(UIScreen screen, string path, DateTime time)> _manifest = new();
        int _captureIndex;
        RunManager _runManager;

        static readonly UIScreen[] AllScreens = (UIScreen[])Enum.GetValues(typeof(UIScreen));

        void Awake()
        {
            Instance = this;
        }

        public void BeginCapture(string outputDir = null)
        {
            _outputDir = outputDir ?? Path.Combine(Application.dataPath, "..", "Screenshots", "ui_audit",
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(_outputDir);
            _captured.Clear();
            _manifest.Clear();
            _captureIndex = 0;
            IsCapturing = true;

            _runManager = FindFirstObjectByType<RunManager>();
            Debug.Log($"[UIScreenCapture] Capture session started → {_outputDir}");
        }

        public void EndCapture()
        {
            IsCapturing = false;
            GenerateReport();
            Debug.Log($"[UIScreenCapture] Capture session ended — {_captured.Count}/{AllScreens.Length} screens captured");

            var missing = new List<UIScreen>();
            foreach (var s in AllScreens)
                if (!_captured.Contains(s)) missing.Add(s);
            if (missing.Count > 0)
                Debug.LogWarning($"[UIScreenCapture] Missing screens: {string.Join(", ", missing)}");
        }

        public void CaptureScreen(UIScreen screen)
        {
            if (!IsCapturing) return;
            StartCoroutine(CaptureEndOfFrame(screen));
        }

        IEnumerator CaptureEndOfFrame(UIScreen screen)
        {
            yield return new WaitForEndOfFrame();

            var filename = $"{_captureIndex:D3}_{screen}.png";
            var path = Path.Combine(_outputDir, filename);

            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Destroy(tex);

            bool isNew = _captured.Add(screen);
            _manifest.Add((screen, filename, DateTime.Now));
            _captureIndex++;

            Debug.Log($"[UIScreenCapture] {(isNew ? "NEW" : "repeat")} — {screen} → {filename}");
        }

        public void NotifyPhaseChange(RunPhase phase, bool won = false)
        {
            if (!IsCapturing) return;

            switch (phase)
            {
                case RunPhase.ArchetypeSelect:
                    CaptureScreen(UIScreen.ArchetypeSelect);
                    break;
                case RunPhase.MapSelect:
                    CaptureScreen(UIScreen.Map);
                    break;
                case RunPhase.InMatch:
                    CaptureScreen(UIScreen.MatchInProgress);
                    break;
                case RunPhase.Shop:
                    CaptureScreen(UIScreen.Shop);
                    break;
                case RunPhase.Event:
                    CaptureScreen(UIScreen.Event);
                    break;
                case RunPhase.Rest:
                    CaptureScreen(UIScreen.Rest);
                    break;
                case RunPhase.RunOver:
                    if (won) CaptureScreen(UIScreen.RunComplete);
                    else CaptureScreen(UIScreen.RunFailed);
                    break;
            }
        }

        public void NotifyModal(UIScreen screen)
        {
            if (!IsCapturing) return;
            CaptureScreen(screen);
        }

        void GenerateReport()
        {
            var html = new System.Text.StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='en'><head><meta charset='UTF-8'>");
            html.AppendLine("<title>Wits &amp; Fools — UI Audit Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("@import url('https://fonts.googleapis.com/css2?family=Cinzel:wght@400;700&family=Crimson+Pro&display=swap');");
            html.AppendLine(":root { --midnight: #0A0A14; --parchment: #F0E6D2; --gold: #D4A846; --bronze-edge: #5A4830; --sage: #66B866; --venetian-red: #B84040; }");
            html.AppendLine("* { margin: 0; padding: 0; box-sizing: border-box; }");
            html.AppendLine("body { background: var(--midnight); color: var(--parchment); font-family: 'Crimson Pro', serif; padding: 2rem; }");
            html.AppendLine("h1 { font-family: 'Cinzel', serif; color: var(--gold); text-align: center; margin-bottom: 0.5rem; }");
            html.AppendLine(".meta { text-align: center; color: #A89B8C; margin-bottom: 2rem; }");
            html.AppendLine(".summary { display: flex; justify-content: center; gap: 2rem; margin-bottom: 2rem; }");
            html.AppendLine(".stat { text-align: center; padding: 1rem 2rem; border: 1px solid var(--bronze-edge); border-radius: 8px; }");
            html.AppendLine(".stat .num { font-size: 2rem; font-family: 'Cinzel', serif; color: var(--gold); }");
            html.AppendLine(".stat .label { color: #A89B8C; font-size: 0.9rem; }");
            html.AppendLine(".grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(480px, 1fr)); gap: 1.5rem; }");
            html.AppendLine(".card { border: 1px solid var(--bronze-edge); border-radius: 8px; overflow: hidden; background: #141C28; }");
            html.AppendLine(".card img { width: 100%; display: block; }");
            html.AppendLine(".card .info { padding: 0.75rem 1rem; }");
            html.AppendLine(".card .screen-name { font-family: 'Cinzel', serif; color: var(--gold); font-size: 1.1rem; }");
            html.AppendLine(".card .time { color: #A89B8C; font-size: 0.85rem; }");
            html.AppendLine(".missing { border: 1px dashed var(--venetian-red); border-radius: 8px; padding: 2rem; text-align: center; }");
            html.AppendLine(".missing .screen-name { font-family: 'Cinzel', serif; color: var(--venetian-red); }");
            html.AppendLine(".captured { color: var(--sage); } .not-captured { color: var(--venetian-red); }");
            html.AppendLine("</style></head><body>");

            html.AppendLine("<h1>UI Audit Report</h1>");
            html.AppendLine($"<p class='meta'>Generated {DateTime.Now:yyyy-MM-dd HH:mm:ss} — {_captured.Count}/{AllScreens.Length} screens captured</p>");

            html.AppendLine("<div class='summary'>");
            html.AppendLine($"<div class='stat'><div class='num'>{_captured.Count}</div><div class='label'>Captured</div></div>");
            html.AppendLine($"<div class='stat'><div class='num'>{AllScreens.Length - _captured.Count}</div><div class='label'>Missing</div></div>");
            html.AppendLine($"<div class='stat'><div class='num'>{_manifest.Count}</div><div class='label'>Total Shots</div></div>");
            html.AppendLine("</div>");

            html.AppendLine("<div class='grid'>");

            foreach (var screen in AllScreens)
            {
                var entries = _manifest.FindAll(e => e.screen == screen);
                if (entries.Count > 0)
                {
                    var entry = entries[entries.Count - 1];
                    html.AppendLine("<div class='card'>");
                    html.AppendLine($"<img src='{entry.path}' alt='{screen}' loading='lazy'>");
                    html.AppendLine($"<div class='info'><div class='screen-name captured'>{screen}</div>");
                    html.AppendLine($"<div class='time'>{entry.time:HH:mm:ss} — {entries.Count} capture(s)</div></div></div>");
                }
                else
                {
                    html.AppendLine("<div class='missing'>");
                    html.AppendLine($"<div class='screen-name'>{screen}</div>");
                    html.AppendLine("<div class='time'>Not captured this session</div></div>");
                }
            }

            html.AppendLine("</div></body></html>");

            var reportPath = Path.Combine(_outputDir, "report.html");
            File.WriteAllText(reportPath, html.ToString());
            Debug.Log($"[UIScreenCapture] Report → {reportPath}");
        }
    }
}
