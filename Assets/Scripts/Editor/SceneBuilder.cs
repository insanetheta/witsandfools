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

            // Deck count overlay
            var deckCountLabel = AddText(deckSlot, "DeckCountLabel", "0",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 32, color: Color.white);
            var deckCountRT = (RectTransform)deckCountLabel.transform;
            deckCountRT.anchoredPosition = new Vector2(55, 0);
            deckCountLabel.outlineWidth = 0.3f;
            deckCountLabel.outlineColor = Color.black;

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
            table.DeckCountLabel = deckCountLabel;
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

            // ----- Ability choice panel -----
            var acPanel = NewChild(canvasRT, "AbilityChoicePanel");
            acPanel.anchorMin = new Vector2(0.5f, 0.5f);
            acPanel.anchorMax = new Vector2(0.5f, 0.5f);
            acPanel.sizeDelta = new Vector2(480, 220);
            var acBg = acPanel.gameObject.AddComponent<Image>();
            acBg.color = new Color(0.05f, 0.05f, 0.12f, 0.92f);
            var acLabel = AddText(acPanel, "AbilityChoiceLabel", "Ability",
                anchorMin: new Vector2(0.05f, 0.45f), anchorMax: new Vector2(0.95f, 0.95f),
                pivot: new Vector2(0.5f, 1f), alignment: TextAlignmentOptions.Center,
                fontSize: 24, color: Color.white);
            acLabel.enableWordWrapping = true;
            var normalBtn = AddButton(acPanel, "PlayNormallyButton", "Play normally");
            normalBtn.anchorMin = new Vector2(0.05f, 0);
            normalBtn.anchorMax = new Vector2(0.48f, 0);
            normalBtn.pivot = new Vector2(0.5f, 0);
            normalBtn.sizeDelta = new Vector2(0, 60);
            normalBtn.anchoredPosition = new Vector2(0, 20);
            var useBtn = AddButton(acPanel, "UseAbilityButton", "Use ability");
            useBtn.anchorMin = new Vector2(0.52f, 0);
            useBtn.anchorMax = new Vector2(0.95f, 0);
            useBtn.pivot = new Vector2(0.5f, 0);
            useBtn.sizeDelta = new Vector2(0, 60);
            useBtn.anchoredPosition = new Vector2(0, 20);
            hud.AbilityChoicePanel = acPanel.gameObject;
            hud.AbilityChoiceLabel = acLabel;
            hud.PlayNormallyButton = normalBtn.GetComponent<Button>();
            hud.UseAbilityButton = useBtn.GetComponent<Button>();
            acPanel.gameObject.SetActive(false);

            // ----- Auto-play button (bottom-left) -----
            var autoPlayBtn = AddButton(canvasRT, "AutoPlayButton", "Auto: OFF");
            autoPlayBtn.anchorMin = new Vector2(0, 0);
            autoPlayBtn.anchorMax = new Vector2(0, 0);
            autoPlayBtn.pivot = new Vector2(0, 0);
            autoPlayBtn.sizeDelta = new Vector2(180, 55);
            autoPlayBtn.anchoredPosition = new Vector2(30, 30);
            hud.AutoPlayButton = autoPlayBtn.GetComponent<Button>();

            // ----- Tooltip label (bottom-left, hidden by default) -----
            var tooltipLabel = AddText(canvasRT, "TooltipLabel", "",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.4f, 0),
                pivot: new Vector2(0, 0),
                alignment: TextAlignmentOptions.BottomLeft, fontSize: 18,
                color: new Color(1, 1, 1, 0.85f));
            var tooltipRT = (RectTransform)tooltipLabel.transform;
            tooltipRT.sizeDelta = new Vector2(0, 50);
            tooltipRT.anchoredPosition = new Vector2(30, 95);
            tooltipLabel.enableWordWrapping = true;
            tooltipLabel.gameObject.SetActive(false);
            hud.TooltipLabel = tooltipLabel;

            // ----- Deck top label for Spy's Monocle (right side, above deck) -----
            var deckTopLabel = AddText(canvasRT, "DeckTopLabel", "",
                anchorMin: new Vector2(0.7f, 0), anchorMax: new Vector2(1f, 0),
                pivot: new Vector2(1, 0),
                alignment: TextAlignmentOptions.BottomRight, fontSize: 18,
                color: new Color(0.9f, 0.8f, 0.4f));
            var deckTopRT = (RectTransform)deckTopLabel.transform;
            deckTopRT.sizeDelta = new Vector2(0, 40);
            deckTopRT.anchoredPosition = new Vector2(-20, 95);
            deckTopLabel.gameObject.SetActive(false);
            hud.DeckTopLabel = deckTopLabel;

            // ----- Info label for Marked Deck (top-right, below HUD bar) -----
            var infoLabel = AddText(canvasRT, "InfoLabel", "",
                anchorMin: new Vector2(0.4f, 1), anchorMax: new Vector2(1f, 1),
                pivot: new Vector2(1, 1),
                alignment: TextAlignmentOptions.TopRight, fontSize: 16,
                color: new Color(0.8f, 0.6f, 0.3f));
            var infoRT = (RectTransform)infoLabel.transform;
            infoRT.sizeDelta = new Vector2(0, 35);
            infoRT.anchoredPosition = new Vector2(-20, -70);
            infoLabel.enableWordWrapping = true;
            infoLabel.gameObject.SetActive(false);
            hud.InfoLabel = infoLabel;

            // ----- Wrap existing match UI in a MatchPanel group -----
            var matchPanel = NewChild(canvasRT, "MatchPanel");
            FillParent(matchPanel);
            // Reparent all match-specific UI under MatchPanel
            felt.SetParent(matchPanel, true);
            frame.SetParent(matchPanel, true);
            inner.SetParent(matchPanel, true);
            hudBar.SetParent(matchPanel, true);
            endBoutBtn.SetParent(matchPanel, true);
            deckSlot.SetParent(matchPanel, true);
            trumpSlot.SetParent(matchPanel, true);
            discardSlot.SetParent(matchPanel, true);
            boutArea.SetParent(matchPanel, true);
            playerHand.SetParent(matchPanel, true);
            opponentHand.SetParent(matchPanel, true);
            goPanel.SetParent(matchPanel, true);
            acPanel.SetParent(matchPanel, true);
            autoPlayBtn.SetParent(matchPanel, true);
            ((RectTransform)tooltipLabel.transform).SetParent(matchPanel, true);
            ((RectTransform)deckTopLabel.transform).SetParent(matchPanel, true);
            ((RectTransform)infoLabel.transform).SetParent(matchPanel, true);
            matchPanel.gameObject.SetActive(false);

            // ----- Map Panel -----
            var mapPanel = NewChild(canvasRT, "MapPanel");
            FillParent(mapPanel);
            var mapBg = mapPanel.gameObject.AddComponent<Image>();
            mapBg.color = new Color(0.08f, 0.12f, 0.18f);

            var mapTitle = AddText(mapPanel, "MapTitle", "Act 1 — The Bilge Rat Tavern",
                anchorMin: new Vector2(0.1f, 0.82f), anchorMax: new Vector2(0.9f, 0.95f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 36, color: new Color(0.95f, 0.85f, 0.55f));

            var mapSubtitle = AddText(mapPanel, "MapSubtitle", "Choose your path:",
                anchorMin: new Vector2(0.1f, 0.74f), anchorMax: new Vector2(0.9f, 0.82f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 24, color: new Color(0.7f, 0.7f, 0.7f));

            var mapNodeContainer = NewChild(mapPanel, "MapNodeContainer");
            mapNodeContainer.anchorMin = new Vector2(0.25f, 0.15f);
            mapNodeContainer.anchorMax = new Vector2(0.75f, 0.74f);
            mapNodeContainer.offsetMin = Vector2.zero;
            mapNodeContainer.offsetMax = Vector2.zero;
            var nodeLayout = mapNodeContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            nodeLayout.spacing = 16;
            nodeLayout.childAlignment = TextAnchor.UpperCenter;
            nodeLayout.childControlWidth = true;
            nodeLayout.childControlHeight = false;
            nodeLayout.childForceExpandWidth = false;
            nodeLayout.childForceExpandHeight = false;

            mapPanel.gameObject.SetActive(false);

            // ----- Result Panel -----
            var resultPanel = NewChild(canvasRT, "ResultPanel");
            FillParent(resultPanel);
            var resultBg = resultPanel.gameObject.AddComponent<Image>();
            resultBg.color = new Color(0.06f, 0.06f, 0.10f, 0.95f);

            var resultTitle = AddText(resultPanel, "ResultTitle", "Victory!",
                anchorMin: new Vector2(0.1f, 0.82f), anchorMax: new Vector2(0.9f, 0.95f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 42, color: new Color(0.95f, 0.85f, 0.55f));

            var resultDetails = AddText(resultPanel, "ResultDetails", "",
                anchorMin: new Vector2(0.15f, 0.72f), anchorMax: new Vector2(0.85f, 0.82f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: Color.white);
            resultDetails.enableWordWrapping = true;

            var resultReward = AddText(resultPanel, "ResultReward", "",
                anchorMin: new Vector2(0.15f, 0.64f), anchorMax: new Vector2(0.85f, 0.72f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 26, color: new Color(0.4f, 0.85f, 0.4f));

            // Ability pick area
            var abilityPickLabel = AddText(resultPanel, "AbilityPickLabel", "Choose an ability:",
                anchorMin: new Vector2(0.15f, 0.56f), anchorMax: new Vector2(0.85f, 0.64f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 24, color: new Color(0.85f, 0.75f, 0.45f));

            var abilityPickContainer = NewChild(resultPanel, "AbilityPickContainer");
            abilityPickContainer.anchorMin = new Vector2(0.15f, 0.18f);
            abilityPickContainer.anchorMax = new Vector2(0.85f, 0.56f);
            abilityPickContainer.offsetMin = Vector2.zero;
            abilityPickContainer.offsetMax = Vector2.zero;
            var pickLayout = abilityPickContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            pickLayout.spacing = 6;
            pickLayout.childAlignment = TextAnchor.UpperCenter;
            pickLayout.childControlWidth = true;
            pickLayout.childControlHeight = false;
            pickLayout.childForceExpandWidth = false;
            pickLayout.childForceExpandHeight = false;

            var abilityPickSkipBtn = AddButton(resultPanel, "AbilityPickSkipButton", "Skip");
            abilityPickSkipBtn.anchorMin = new Vector2(0.65f, 0.10f);
            abilityPickSkipBtn.anchorMax = new Vector2(0.65f, 0.10f);
            abilityPickSkipBtn.pivot = new Vector2(0.5f, 0);
            abilityPickSkipBtn.sizeDelta = new Vector2(180, 55);

            var resultContinueBtn = AddButton(resultPanel, "ResultContinueButton", "Continue");
            resultContinueBtn.anchorMin = new Vector2(0.35f, 0.10f);
            resultContinueBtn.anchorMax = new Vector2(0.35f, 0.10f);
            resultContinueBtn.pivot = new Vector2(0.5f, 0);
            resultContinueBtn.sizeDelta = new Vector2(200, 55);

            resultPanel.gameObject.SetActive(false);

            // ----- Run Over Panel -----
            var runOverPanel = NewChild(canvasRT, "RunOverPanel");
            FillParent(runOverPanel);
            var runOverBg = runOverPanel.gameObject.AddComponent<Image>();
            runOverBg.color = new Color(0.04f, 0.04f, 0.08f);

            var runOverTitle = AddText(runOverPanel, "RunOverTitle", "Run Over",
                anchorMin: new Vector2(0.1f, 0.60f), anchorMax: new Vector2(0.9f, 0.85f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 52, color: new Color(0.95f, 0.85f, 0.55f));

            var runOverStats = AddText(runOverPanel, "RunOverStats", "",
                anchorMin: new Vector2(0.2f, 0.30f), anchorMax: new Vector2(0.8f, 0.58f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 26, color: Color.white);
            runOverStats.enableWordWrapping = true;

            var runOverRestartBtn = AddButton(runOverPanel, "RunOverRestartButton", "New Run");
            runOverRestartBtn.anchorMin = new Vector2(0.5f, 0.10f);
            runOverRestartBtn.anchorMax = new Vector2(0.5f, 0.10f);
            runOverRestartBtn.pivot = new Vector2(0.5f, 0);
            runOverRestartBtn.sizeDelta = new Vector2(260, 70);

            runOverPanel.gameObject.SetActive(false);

            // ----- Shop Panel -----
            var shopPanel = NewChild(canvasRT, "ShopPanel");
            FillParent(shopPanel);
            var shopBg = shopPanel.gameObject.AddComponent<Image>();
            shopBg.color = new Color(0.08f, 0.14f, 0.20f);

            var shopTitle = AddText(shopPanel, "ShopTitle", "The Fence",
                anchorMin: new Vector2(0.1f, 0.82f), anchorMax: new Vector2(0.9f, 0.95f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 36, color: new Color(0.95f, 0.85f, 0.55f));

            var shopFlorins = AddText(shopPanel, "ShopFlorins", "Your purse: 0 Florins",
                anchorMin: new Vector2(0.1f, 0.74f), anchorMax: new Vector2(0.9f, 0.82f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: new Color(0.95f, 0.85f, 0.40f));

            var shopItemContainer = NewChild(shopPanel, "ShopItemContainer");
            shopItemContainer.anchorMin = new Vector2(0.20f, 0.15f);
            shopItemContainer.anchorMax = new Vector2(0.80f, 0.74f);
            shopItemContainer.offsetMin = Vector2.zero;
            shopItemContainer.offsetMax = Vector2.zero;
            var shopLayout = shopItemContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            shopLayout.spacing = 12;
            shopLayout.childAlignment = TextAnchor.UpperCenter;
            shopLayout.childControlWidth = true;
            shopLayout.childControlHeight = false;
            shopLayout.childForceExpandWidth = false;
            shopLayout.childForceExpandHeight = false;

            var shopLeaveBtn = AddButton(shopPanel, "ShopLeaveButton", "Leave");
            shopLeaveBtn.anchorMin = new Vector2(0.5f, 0.04f);
            shopLeaveBtn.anchorMax = new Vector2(0.5f, 0.04f);
            shopLeaveBtn.pivot = new Vector2(0.5f, 0);
            shopLeaveBtn.sizeDelta = new Vector2(260, 70);

            shopPanel.gameObject.SetActive(false);

            // ----- Event Panel -----
            var eventPanel = NewChild(canvasRT, "EventPanel");
            FillParent(eventPanel);
            var eventBg = eventPanel.gameObject.AddComponent<Image>();
            eventBg.color = new Color(0.10f, 0.08f, 0.14f);

            var eventTitle = AddText(eventPanel, "EventTitle", "Event",
                anchorMin: new Vector2(0.1f, 0.78f), anchorMax: new Vector2(0.9f, 0.92f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 36, color: new Color(0.95f, 0.85f, 0.55f));

            var eventDesc = AddText(eventPanel, "EventDesc", "",
                anchorMin: new Vector2(0.15f, 0.52f), anchorMax: new Vector2(0.85f, 0.76f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 24, color: Color.white);
            eventDesc.enableWordWrapping = true;

            var eventOutcome = AddText(eventPanel, "EventOutcome", "",
                anchorMin: new Vector2(0.15f, 0.35f), anchorMax: new Vector2(0.85f, 0.50f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 26, color: new Color(0.4f, 0.85f, 0.4f));
            eventOutcome.enableWordWrapping = true;
            eventOutcome.gameObject.SetActive(false);

            var eventChoice1Btn = AddButton(eventPanel, "EventChoice1Button", "Choice 1");
            eventChoice1Btn.anchorMin = new Vector2(0.5f, 0.22f);
            eventChoice1Btn.anchorMax = new Vector2(0.5f, 0.22f);
            eventChoice1Btn.pivot = new Vector2(0.5f, 0.5f);
            eventChoice1Btn.sizeDelta = new Vector2(420, 60);
            var eventChoice1Label = eventChoice1Btn.GetComponentInChildren<TMP_Text>();

            var eventChoice2Btn = AddButton(eventPanel, "EventChoice2Button", "Choice 2");
            eventChoice2Btn.anchorMin = new Vector2(0.5f, 0.12f);
            eventChoice2Btn.anchorMax = new Vector2(0.5f, 0.12f);
            eventChoice2Btn.pivot = new Vector2(0.5f, 0.5f);
            eventChoice2Btn.sizeDelta = new Vector2(420, 60);
            var eventChoice2Label = eventChoice2Btn.GetComponentInChildren<TMP_Text>();

            var eventContinueBtn = AddButton(eventPanel, "EventContinueButton", "Continue");
            eventContinueBtn.anchorMin = new Vector2(0.5f, 0.06f);
            eventContinueBtn.anchorMax = new Vector2(0.5f, 0.06f);
            eventContinueBtn.pivot = new Vector2(0.5f, 0);
            eventContinueBtn.sizeDelta = new Vector2(260, 70);
            eventContinueBtn.gameObject.SetActive(false);

            eventPanel.gameObject.SetActive(false);

            // ----- Run HUD (persistent bar) -----
            var runHudPanel = NewChild(canvasRT, "RunHudPanel");
            runHudPanel.anchorMin = new Vector2(0, 0);
            runHudPanel.anchorMax = new Vector2(1, 0);
            runHudPanel.pivot = new Vector2(0.5f, 0);
            runHudPanel.sizeDelta = new Vector2(0, 50);
            var runHudBg = runHudPanel.gameObject.AddComponent<Image>();
            runHudBg.color = new Color(0, 0, 0, 0.6f);

            var prestigeLabel = AddText(runHudPanel, "PrestigeLabel", "Prestige: ♥♥♥♥",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.25f, 1),
                pivot: new Vector2(0, 0.5f), alignment: TextAlignmentOptions.MidlineLeft,
                fontSize: 22, color: new Color(1f, 0.4f, 0.4f));
            ((RectTransform)prestigeLabel.transform).offsetMin = new Vector2(16, 0);

            var florinsLabel = AddText(runHudPanel, "FlorinsLabel", "Florins: 0",
                anchorMin: new Vector2(0.25f, 0), anchorMax: new Vector2(0.5f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: new Color(0.95f, 0.85f, 0.40f));

            var actLabel = AddText(runHudPanel, "ActLabel", "Act 1 of 5",
                anchorMin: new Vector2(0.5f, 0), anchorMax: new Vector2(0.75f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: Color.white);

            var abilitiesLabel = AddText(runHudPanel, "AbilitiesLabel", "Abilities: 4/5",
                anchorMin: new Vector2(0.75f, 0), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(1, 0.5f), alignment: TextAlignmentOptions.MidlineRight,
                fontSize: 22, color: new Color(0.6f, 0.8f, 1f));
            ((RectTransform)abilitiesLabel.transform).offsetMax = new Vector2(-16, 0);

            runHudPanel.gameObject.SetActive(false);

            // ----- GameManager wiring -----
            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.Table = table;
            gm.Hud = hud;
            gm.CardViewPrefab = cardPrefab;
            gm.AutoStartOnAwake = false;

            // ----- RunManager wiring -----
            var rmGO = new GameObject("RunManager");
            var rm = rmGO.AddComponent<RunManager>();
            rm.GameManager = gm;
            rm.MatchPanel = matchPanel.gameObject;
            rm.MapPanel = mapPanel.gameObject;
            rm.ResultPanel = resultPanel.gameObject;
            rm.RunOverPanel = runOverPanel.gameObject;
            rm.RunHudPanel = runHudPanel.gameObject;
            rm.MapTitleLabel = mapTitle;
            rm.MapNodeContainer = mapNodeContainer;
            rm.ResultTitleLabel = resultTitle;
            rm.ResultDetailsLabel = resultDetails;
            rm.ResultRewardLabel = resultReward;
            rm.ResultContinueButton = resultContinueBtn.GetComponent<Button>();
            rm.AbilityPickLabel = abilityPickLabel;
            rm.AbilityPickContainer = abilityPickContainer;
            rm.AbilityPickSkipButton = abilityPickSkipBtn.GetComponent<Button>();
            rm.RunOverTitleLabel = runOverTitle;
            rm.RunOverStatsLabel = runOverStats;
            rm.RunOverRestartButton = runOverRestartBtn.GetComponent<Button>();
            rm.PrestigeLabel = prestigeLabel;
            rm.FlorinsLabel = florinsLabel;
            rm.ActLabel = actLabel;
            rm.AbilitiesLabel = abilitiesLabel;
            rm.ShopPanel = shopPanel.gameObject;
            rm.ShopTitleLabel = shopTitle;
            rm.ShopFlorinsLabel = shopFlorins;
            rm.ShopItemContainer = shopItemContainer;
            rm.ShopLeaveButton = shopLeaveBtn.GetComponent<Button>();
            rm.EventPanel = eventPanel.gameObject;
            rm.EventTitleLabel = eventTitle;
            rm.EventDescLabel = eventDesc;
            rm.EventOutcomeLabel = eventOutcome;
            rm.EventChoice1Button = eventChoice1Btn.GetComponent<Button>();
            rm.EventChoice1Label = eventChoice1Label;
            rm.EventChoice2Button = eventChoice2Btn.GetComponent<Button>();
            rm.EventChoice2Label = eventChoice2Label;
            rm.EventContinueButton = eventContinueBtn.GetComponent<Button>();

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
