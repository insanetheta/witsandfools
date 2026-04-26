using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WitsAndFools.EditorTools
{
    public static class SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/GameScene.unity";
        const string CardPrefabPath = "Assets/Prefabs/CardView.prefab";

        static TMP_FontAsset s_font;
        static TMP_FontAsset DefaultFont
        {
            get
            {
                if (s_font) return s_font;
                try { s_font = TMP_Settings.defaultFontAsset; } catch { s_font = null; }
                if (!s_font)
                {
                    var guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
                    foreach (var g in guids)
                    {
                        var p = AssetDatabase.GUIDToAssetPath(g);
                        s_font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
                        if (s_font) break;
                    }
                }
                return s_font;
            }
        }

        [MenuItem("Wits and Fools/Build/Scene (GameScene)")]
        public static void BuildScene()
        {
            // Ensure TMP Essentials are imported (so text renders)
            if (DefaultFont == null)
            {
                Debug.LogWarning("TMP Essentials not yet imported — importing now. You may need to re-run 'Build/Scene' after the import completes.");
                TmpEssentialsImporter.EnsureImported();
                AssetDatabase.Refresh();
            }

            // Make sure card prefab exists
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            if (!cardPrefab)
            {
                PrefabBuilder.BuildCardPrefab();
                cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ----- Camera -----
            var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0, 0, -10);
            var c = cam.GetComponent<Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.10f, 0.18f, 0.10f); // pub-table green
            c.orthographic = true;

            // ----- EventSystem -----
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            // ----- Canvas -----
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var canvasRT = (RectTransform)canvasGO.transform;

            // ----- Felt background (full-screen image) -----
            var felt = NewChild(canvasRT, "TableFelt");
            var feltImg = felt.gameObject.AddComponent<Image>();
            feltImg.color = new Color(0.13f, 0.30f, 0.18f);
            feltImg.raycastTarget = false;
            FillParent(felt);

            // Decorative inner frame
            var frame = NewChild(canvasRT, "TableFrame");
            var frameImg = frame.gameObject.AddComponent<Image>();
            frameImg.color = new Color(0.40f, 0.20f, 0.10f, 1f);
            frameImg.raycastTarget = false;
            frame.anchorMin = new Vector2(0, 0);
            frame.anchorMax = new Vector2(1, 1);
            frame.offsetMin = new Vector2(20, 20);
            frame.offsetMax = new Vector2(-20, -20);
            // Hollow it out by adding a slightly smaller felt panel on top
            var inner = NewChild(canvasRT, "TableInner");
            var innerImg = inner.gameObject.AddComponent<Image>();
            innerImg.color = new Color(0.10f, 0.24f, 0.14f);
            innerImg.raycastTarget = false;
            inner.anchorMin = new Vector2(0, 0);
            inner.anchorMax = new Vector2(1, 1);
            inner.offsetMin = new Vector2(36, 36);
            inner.offsetMax = new Vector2(-36, -36);

            // ----- HUD bar (top) -----
            var hudBar = NewChild(canvasRT, "HudBar");
            var hudBarImg = hudBar.gameObject.AddComponent<Image>();
            hudBarImg.color = new Color(0, 0, 0, 0.45f);
            hudBar.anchorMin = new Vector2(0, 1);
            hudBar.anchorMax = new Vector2(1, 1);
            hudBar.pivot = new Vector2(0.5f, 1);
            hudBar.sizeDelta = new Vector2(0, 70);
            hudBar.anchoredPosition = new Vector2(0, -10);

            var turnLabel = AddText(hudBar, "TurnLabel", "—", anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.4f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.MidlineLeft, fontSize: 28, color: Color.white);
            turnLabel.margin = new Vector4(20, 0, 0, 0);

            var trumpLabel = AddText(hudBar, "TrumpLabel", "Trump: ♥", anchorMin: new Vector2(0.4f, 0), anchorMax: new Vector2(0.7f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center, fontSize: 28, color: Color.white);

            var deckLabel = AddText(hudBar, "DeckCountLabel", "Deck: 0", anchorMin: new Vector2(0.7f, 0), anchorMax: new Vector2(1f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.MidlineRight, fontSize: 28, color: Color.white);
            deckLabel.margin = new Vector4(0, 0, 20, 0);

            // ----- End-bout button (bottom-right) -----
            var endBoutBtn = AddButton(canvasRT, "EndBoutButton", "End bout");
            endBoutBtn.anchorMin = new Vector2(1, 0);
            endBoutBtn.anchorMax = new Vector2(1, 0);
            endBoutBtn.pivot = new Vector2(1, 0);
            endBoutBtn.sizeDelta = new Vector2(220, 70);
            endBoutBtn.anchoredPosition = new Vector2(-30, 30);

            // ----- Center band: deck/trump anchored to LEFT edge, discard anchored to RIGHT edge,
            // bout area in the middle. Anchoring to edges keeps everything visible at narrow aspect ratios.
            var deckSlot = NewChild(canvasRT, "DeckSlot");
            deckSlot.anchorMin = new Vector2(0, 0.5f);
            deckSlot.anchorMax = new Vector2(0, 0.5f);
            deckSlot.pivot = new Vector2(0, 0.5f);
            deckSlot.sizeDelta = new Vector2(110, 160);
            deckSlot.anchoredPosition = new Vector2(120, 0);
            var deckImg = deckSlot.gameObject.AddComponent<Image>();
            deckImg.color = new Color(0.20f, 0.06f, 0.06f, 1f);
            // Deck "stack" visual using offset rectangles
            for (int i = 0; i < 6; i++)
            {
                var stack = NewChild(deckSlot, $"DeckStack{i}");
                stack.sizeDelta = new Vector2(110, 160);
                stack.anchoredPosition = new Vector2(55 - i * 0.5f, i * 0.6f); // pivot is left so center is +55
                var img = stack.gameObject.AddComponent<Image>();
                img.color = new Color(0.55f, 0.10f, 0.10f);
                img.raycastTarget = false;
            }

            // Trump card sits to the right of the deck, rotated 90° (handled by GameManager) so it peeks out.
            var trumpSlot = NewChild(canvasRT, "TrumpSlot");
            trumpSlot.anchorMin = new Vector2(0, 0.5f);
            trumpSlot.anchorMax = new Vector2(0, 0.5f);
            trumpSlot.pivot = new Vector2(0, 0.5f);
            trumpSlot.sizeDelta = new Vector2(160, 110);
            trumpSlot.anchoredPosition = new Vector2(180, 0);

            // Discard pile on the right edge.
            var discardSlot = NewChild(canvasRT, "DiscardSlot");
            discardSlot.anchorMin = new Vector2(1, 0.5f);
            discardSlot.anchorMax = new Vector2(1, 0.5f);
            discardSlot.pivot = new Vector2(1, 0.5f);
            discardSlot.sizeDelta = new Vector2(110, 160);
            discardSlot.anchoredPosition = new Vector2(-120, 0);
            var discardImg = discardSlot.gameObject.AddComponent<Image>();
            discardImg.color = new Color(1, 1, 1, 0.05f);
            // Add a "Discard" label
            AddText(discardSlot, "DiscardLabel", "Discard",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 0),
                alignment: TextAlignmentOptions.Center, fontSize: 18,
                color: new Color(1, 1, 1, 0.5f));

            var boutArea = NewChild(canvasRT, "BoutArea");
            boutArea.anchorMin = new Vector2(0.5f, 0.5f);
            boutArea.anchorMax = new Vector2(0.5f, 0.5f);
            boutArea.sizeDelta = new Vector2(900, 240);
            boutArea.anchoredPosition = new Vector2(0, 0);

            // ----- Hands -----
            var playerHand = NewChild(canvasRT, "PlayerHand");
            playerHand.sizeDelta = new Vector2(900, 200);
            playerHand.anchorMin = new Vector2(0.5f, 0);
            playerHand.anchorMax = new Vector2(0.5f, 0);
            playerHand.pivot = new Vector2(0.5f, 0);
            playerHand.anchoredPosition = new Vector2(0, 130);
            var playerHandLayout = playerHand.gameObject.AddComponent<HandLayout>();
            playerHandLayout.FaceUp = true;
            playerHandLayout.ReverseOrder = false;

            var opponentHand = NewChild(canvasRT, "OpponentHand");
            opponentHand.sizeDelta = new Vector2(900, 200);
            opponentHand.anchorMin = new Vector2(0.5f, 1);
            opponentHand.anchorMax = new Vector2(0.5f, 1);
            opponentHand.pivot = new Vector2(0.5f, 1);
            opponentHand.anchoredPosition = new Vector2(0, -100);
            var opponentHandLayout = opponentHand.gameObject.AddComponent<HandLayout>();
            opponentHandLayout.FaceUp = false;
            opponentHandLayout.ReverseOrder = true;

            // ----- TableView wiring -----
            var tableGO = new GameObject("TableView");
            var table = tableGO.AddComponent<TableView>();
            table.PlayerHand = playerHandLayout;
            table.OpponentHand = opponentHandLayout;
            table.DeckSlot = deckSlot;
            table.TrumpSlot = trumpSlot;
            table.DiscardSlot = discardSlot;
            table.BoutArea = boutArea;
            table.CardSpawnRoot = canvasRT;

            // ----- HUD wiring -----
            var hudGO = new GameObject("HudView");
            var hud = hudGO.AddComponent<HudView>();
            hud.TurnLabel = turnLabel;
            hud.DeckCountLabel = deckLabel;
            hud.TrumpLabel = trumpLabel;
            hud.EndBoutButton = endBoutBtn.GetComponent<Button>();

            // ----- Game-over panel -----
            var goPanel = NewChild(canvasRT, "GameOverPanel");
            goPanel.anchorMin = new Vector2(0.5f, 0.5f);
            goPanel.anchorMax = new Vector2(0.5f, 0.5f);
            goPanel.sizeDelta = new Vector2(600, 280);
            var goBg = goPanel.gameObject.AddComponent<Image>();
            goBg.color = new Color(0, 0, 0, 0.85f);
            var goLabel = AddText(goPanel, "GameOverLabel", "Game over",
                anchorMin: new Vector2(0, 0.5f), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 44, color: Color.white);
            var restartBtn = AddButton(goPanel, "RestartButton", "Play again");
            restartBtn.anchorMin = new Vector2(0.5f, 0);
            restartBtn.anchorMax = new Vector2(0.5f, 0);
            restartBtn.pivot = new Vector2(0.5f, 0);
            restartBtn.sizeDelta = new Vector2(240, 70);
            restartBtn.anchoredPosition = new Vector2(0, 30);
            hud.RestartButton = restartBtn.GetComponent<Button>();
            hud.GameOverPanel = goPanel.gameObject;
            hud.GameOverLabel = goLabel;
            goPanel.gameObject.SetActive(false);

            // ----- GameManager wiring -----
            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.Table = table;
            gm.Hud = hud;
            gm.CardViewPrefab = cardPrefab;

            // Save scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath);

            // Make sure GameScene is in build settings
            var bs = EditorBuildSettings.scenes;
            bool present = false;
            foreach (var b in bs) if (b.path == ScenePath) { present = true; break; }
            if (!present)
            {
                var newBs = new EditorBuildSettingsScene[bs.Length + 1];
                System.Array.Copy(bs, newBs, bs.Length);
                newBs[^1] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettings.scenes = newBs;
            }

            Debug.Log($"Built {ScenePath}");
        }

        // ---------- helpers ----------

        static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void FillParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static TMP_Text AddText(RectTransform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            TextAlignmentOptions alignment, float fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<TextMeshProUGUI>();
            if (DefaultFont) t.font = DefaultFont;
            t.text = text;
            t.alignment = alignment;
            t.fontSize = fontSize;
            t.color = color;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            return t;
        }

        static RectTransform AddButton(RectTransform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            var img = go.GetComponent<Image>();
            img.color = new Color(0.85f, 0.78f, 0.55f);
            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 0.95f, 0.7f);
            colors.disabledColor = new Color(0.55f, 0.50f, 0.40f, 1f);
            btn.colors = colors;

            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(go.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            FillParent(lblRT);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            if (DefaultFont) lbl.font = DefaultFont;
            lbl.text = label;
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 28;
            lbl.color = new Color(0.15f, 0.10f, 0.05f);
            lbl.raycastTarget = false;
            return rt;
        }
    }
}
