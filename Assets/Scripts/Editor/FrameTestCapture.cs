using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using TMPro;
using WitsAndFools;

namespace WitsAndFools.EditorTools
{
public static class FrameTestCapture
{
    [MenuItem("Wits and Fools/Test/Capture Framed Cards")]
    public static void Capture()
    {
        var catPath = "Assets/Data/card_catalog.json";
        CardCatalogLoader.LoadFromJson(File.ReadAllText(catPath));
        var allCards = CardCatalog.Draftable(DoctrineType.Schemer);

        var existing = GameObject.Find("FrameTestCanvas");
        if (existing != null) Object.DestroyImmediate(existing);

        int rtW = 1920, rtH = 1080;

        var camGO = new GameObject("CaptureCam");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = rtH / 2f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.06f, 0.04f);
        camGO.transform.position = new Vector3(rtW / 2f, rtH / 2f, -10f);

        var canvasGO = new GameObject("FrameTestCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 10f;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(rtW, rtH);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(canvasGO.transform, false);
        var bgRT = (RectTransform)bg.transform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.04f);

        var rm = Object.FindAnyObjectByType<RunManager>();
        if (rm == null)
        {
            Debug.LogError("FrameTestCapture: RunManager not found");
            Object.DestroyImmediate(canvasGO);
            Object.DestroyImmediate(camGO);
            return;
        }

        var createMethod = typeof(RunManager).GetMethod("CreateMiniCard",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (createMethod == null)
        {
            Debug.LogError("FrameTestCapture: CreateMiniCard not found");
            Object.DestroyImmediate(canvasGO);
            Object.DestroyImmediate(camGO);
            return;
        }

        int[][] sizes = { new[] { 88, 126 }, new[] { 130, 186 }, new[] { 220, 310 } };
        string[] labels = { "Small (88x126)", "Medium (130x186)", "Large (220x310)" };

        float startX = -750;
        for (int s = 0; s < sizes.Length; s++)
        {
            int w = sizes[s][0], h = sizes[s][1];

            var labelGO = new GameObject("Label_" + s, typeof(RectTransform));
            labelGO.transform.SetParent(canvasGO.transform, false);
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchoredPosition = new Vector2(startX + s * 560, 460);
            lrt.sizeDelta = new Vector2(500, 40);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = labels[s];
            tmp.fontSize = 22;
            tmp.color = new Color(0.83f, 0.66f, 0.27f);
            tmp.alignment = TextAlignmentOptions.Center;

            float colX = startX + s * 560;
            int count = s == 2 ? 2 : 3;
            for (int i = 0; i < count && i < allCards.Count; i++)
            {
                var cardGO = (GameObject)createMethod.Invoke(rm,
                    new object[] { allCards[i], canvasGO.transform, (float)w, (float)h });
                var crt = (RectTransform)cardGO.transform;
                float xOff = count == 2 ? (i - 0.5f) * (w + 16) : (i - 1) * (w + 12);
                crt.anchoredPosition = new Vector2(colX + xOff, 80);
            }
        }

        Canvas.ForceUpdateCanvases();

        var rt = new RenderTexture(rtW, rtH, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(rtW, rtH, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rtW, rtH), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        Directory.CreateDirectory("Screenshots");
        var pngData = tex.EncodeToPNG();
        File.WriteAllBytes("Screenshots/framed_cards_with_frame_overlay.png", pngData);
        Debug.Log($"FrameTestCapture: saved {pngData.Length / 1024}KB screenshot");

        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(camGO);
        Object.DestroyImmediate(canvasGO);
    }
}
}
