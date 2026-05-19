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

        static TMP_FontAsset HeadingFont => FontAssets.Heading;
        static TMP_FontAsset MonoFont => FontAssets.Mono;
        static TMP_FontAsset DefaultFont => FontAssets.Body;

        [MenuItem("Wits and Fools/Build/Scene (GameScene)")]
        public static void BuildScene()
        {
            // Ensure TMP Essentials are imported (so text renders)
            if (FontAssets.Fallback == null)
            {
                Debug.LogWarning("TMP Essentials not yet imported — importing now. You may need to re-run 'Build/Scene' after the import completes.");
                TmpEssentialsImporter.EnsureImported();
                AssetDatabase.Refresh();
            }

            // Generate SDF font assets if not yet created
            if (FontAssets.Heading == FontAssets.Fallback)
            {
                Debug.Log("Custom font assets not found — building SDF assets from TTFs...");
                FontAssetBuilder.BuildFontAssets();
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
            c.backgroundColor = ThemePalette.TableBg;
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

            // ----- Table background (venue surface sprite, per-act) -----
            var tableBg = NewChild(canvasRT, "TableBackground");
            var tableBgImg = tableBg.gameObject.AddComponent<Image>();
            var tableBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tables/table_tavern.png");
            if (tableBgSprite) { tableBgImg.sprite = tableBgSprite; tableBgImg.color = Color.white; }
            else tableBgImg.color = ThemePalette.TableBg;
            tableBgImg.raycastTarget = false;
            tableBgImg.preserveAspect = false;
            FillParent(tableBg);

            // ----- Felt overlay (tinted per-act, semi-transparent) -----
            var felt = NewChild(canvasRT, "TableFelt");
            var feltImg = felt.gameObject.AddComponent<Image>();
            feltImg.color = ThemePalette.ActFeltTint[0];
            feltImg.raycastTarget = false;
            FillParent(felt);

            // Thin decorative border frame (3px edges at 12px inset)
            var frame = NewChild(canvasRT, "TableFrame");
            frame.anchorMin = new Vector2(0, 0);
            frame.anchorMax = new Vector2(1, 1);
            frame.offsetMin = new Vector2(12, 12);
            frame.offsetMax = new Vector2(-12, -12);
            var frameColor = new Color(
                ThemePalette.ActFrameColor[0].r,
                ThemePalette.ActFrameColor[0].g,
                ThemePalette.ActFrameColor[0].b, 0.6f);
            foreach (var (aMin, aMax, sd) in new[] {
                (new Vector2(0,1), new Vector2(1,1), new Vector2(0,3)),   // top
                (new Vector2(0,0), new Vector2(1,0), new Vector2(0,3)),   // bottom
                (new Vector2(0,0), new Vector2(0,1), new Vector2(3,0)),   // left
                (new Vector2(1,0), new Vector2(1,1), new Vector2(3,0)),   // right
            })
            {
                var edge = NewChild(frame, "Edge");
                var edgeImg = edge.gameObject.AddComponent<Image>();
                edgeImg.color = frameColor;
                edgeImg.raycastTarget = false;
                edge.anchorMin = aMin;
                edge.anchorMax = aMax;
                edge.sizeDelta = sd;
                edge.pivot = new Vector2(0.5f, 0.5f);
            }
            // Store first edge image for per-act color updates
            var frameEdgeImg = frame.GetChild(0).GetComponent<Image>();

            // ----- HUD bar (top) -----
            var hudBar = NewChild(canvasRT, "HudBar");
            var hudBarImg = hudBar.gameObject.AddComponent<Image>();
            hudBarImg.color = new Color(0, 0, 0, 0.55f);
            hudBar.anchorMin = new Vector2(0, 1);
            hudBar.anchorMax = new Vector2(1, 1);
            hudBar.pivot = new Vector2(0.5f, 1);
            hudBar.sizeDelta = new Vector2(0, 42);
            hudBar.anchoredPosition = Vector2.zero;

            var turnLabel = AddText(hudBar, "TurnLabel", "—", anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.4f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.MidlineLeft, fontSize: 16, color: Color.white);
            turnLabel.margin = new Vector4(16, 0, 0, 0);

            var trumpLabel = AddText(hudBar, "TrumpLabel", "Trump: ♥", anchorMin: new Vector2(0.4f, 0), anchorMax: new Vector2(0.7f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center, fontSize: 16, color: Color.white,
                font: MonoFont);

            var deckLabel = AddText(hudBar, "DeckCountLabel", "Deck: 0", anchorMin: new Vector2(0.7f, 0), anchorMax: new Vector2(1f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.MidlineRight, fontSize: 16, color: Color.white,
                font: MonoFont);
            deckLabel.margin = new Vector4(0, 0, 16, 0);

            // ----- Opponent Nameplate (top-left, below HUD bar) -----
            var nameplate = NewChild(canvasRT, "OpponentNameplate");
            nameplate.anchorMin = new Vector2(0, 1);
            nameplate.anchorMax = new Vector2(0, 1);
            nameplate.pivot = new Vector2(0, 1);
            nameplate.sizeDelta = new Vector2(220, 48);
            nameplate.anchoredPosition = new Vector2(16, -48);
            var nameplateBg = nameplate.gameObject.AddComponent<Image>();
            nameplateBg.color = new Color(0, 0, 0, 0.5f);
            nameplateBg.raycastTarget = false;

            var portraitRT = NewChild(nameplate, "Portrait");
            portraitRT.anchorMin = new Vector2(0, 0);
            portraitRT.anchorMax = new Vector2(0, 1);
            portraitRT.pivot = new Vector2(0, 0.5f);
            portraitRT.sizeDelta = new Vector2(40, 40);
            portraitRT.anchoredPosition = new Vector2(4, 0);
            var portraitImg = portraitRT.gameObject.AddComponent<Image>();
            portraitImg.color = ThemePalette.WarmSlate;
            portraitImg.preserveAspect = true;
            portraitImg.raycastTarget = false;

            var oppNameLabel = AddText(nameplate, "OpponentName", "Opponent",
                anchorMin: new Vector2(0, 0.5f), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0, 1), alignment: TextAlignmentOptions.MidlineLeft,
                fontSize: 14, color: ThemePalette.Parchment, font: HeadingFont);
            var oppNameRT = (RectTransform)oppNameLabel.transform;
            oppNameRT.offsetMin = new Vector2(50, 0);
            oppNameRT.offsetMax = new Vector2(-4, -2);

            var oppArchLabel = AddText(nameplate, "OpponentArchetype", "The Fox",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 0.5f),
                pivot: new Vector2(0, 0), alignment: TextAlignmentOptions.MidlineLeft,
                fontSize: 11, color: ThemePalette.DustyTan);
            var oppArchRT = (RectTransform)oppArchLabel.transform;
            oppArchRT.offsetMin = new Vector2(50, 2);
            oppArchRT.offsetMax = new Vector2(-4, 0);

            // ----- End-bout button (bottom-right) -----
            var endBoutBtn = AddButton(canvasRT, "EndBoutButton", "End Bout");
            endBoutBtn.anchorMin = new Vector2(1, 0);
            endBoutBtn.anchorMax = new Vector2(1, 0);
            endBoutBtn.pivot = new Vector2(1, 0);
            endBoutBtn.sizeDelta = new Vector2(140, 40);
            endBoutBtn.anchoredPosition = new Vector2(-16, 52);

            // ----- Center band: deck/trump anchored to LEFT edge, discard anchored to RIGHT edge,
            // bout area in the middle. Anchoring to edges keeps everything visible at narrow aspect ratios.
            var deckSlot = NewChild(canvasRT, "DeckSlot");
            deckSlot.anchorMin = new Vector2(0, 0.5f);
            deckSlot.anchorMax = new Vector2(0, 0.5f);
            deckSlot.pivot = new Vector2(0, 0.5f);
            deckSlot.sizeDelta = new Vector2(110, 160);
            deckSlot.anchoredPosition = new Vector2(30, 0);
            var deckImg = deckSlot.gameObject.AddComponent<Image>();
            deckImg.color = ThemePalette.DeckSlotDark;
            // Deck "stack" visual using offset rectangles
            for (int i = 0; i < 6; i++)
            {
                var stack = NewChild(deckSlot, $"DeckStack{i}");
                stack.sizeDelta = new Vector2(110, 160);
                stack.anchoredPosition = new Vector2(55 - i * 0.5f, i * 0.6f); // pivot is left so center is +55
                var img = stack.gameObject.AddComponent<Image>();
                img.color = ThemePalette.CrimsonCard;
                img.raycastTarget = false;
            }

            // Deck count overlay
            var deckCountLabel = AddText(deckSlot, "DeckCountLabel", "0",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 32, color: Color.white, font: MonoFont);
            var deckCountRT = (RectTransform)deckCountLabel.transform;
            deckCountRT.anchoredPosition = new Vector2(55, 0);
            deckCountLabel.fontSize = 24;
            deckCountLabel.outlineWidth = 0.3f;
            deckCountLabel.outlineColor = Color.black;

            // Trump card sits to the right of the deck, rotated 90° (handled by GameManager) so it peeks out.
            var trumpSlot = NewChild(canvasRT, "TrumpSlot");
            trumpSlot.anchorMin = new Vector2(0, 0.5f);
            trumpSlot.anchorMax = new Vector2(0, 0.5f);
            trumpSlot.pivot = new Vector2(0, 0.5f);
            trumpSlot.sizeDelta = new Vector2(160, 110);
            trumpSlot.anchoredPosition = new Vector2(100, 0);

            // Discard pile on the right edge.
            var discardSlot = NewChild(canvasRT, "DiscardSlot");
            discardSlot.anchorMin = new Vector2(1, 0.5f);
            discardSlot.anchorMax = new Vector2(1, 0.5f);
            discardSlot.pivot = new Vector2(1, 0.5f);
            discardSlot.sizeDelta = new Vector2(110, 160);
            discardSlot.anchoredPosition = new Vector2(-30, 0);
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
            playerHand.anchoredPosition = new Vector2(0, 52);
            var playerHandLayout = playerHand.gameObject.AddComponent<HandLayout>();
            playerHandLayout.FaceUp = true;
            playerHandLayout.ReverseOrder = false;

            var opponentHand = NewChild(canvasRT, "OpponentHand");
            opponentHand.sizeDelta = new Vector2(900, 200);
            opponentHand.anchorMin = new Vector2(0.5f, 1);
            opponentHand.anchorMax = new Vector2(0.5f, 1);
            opponentHand.pivot = new Vector2(0.5f, 1);
            opponentHand.anchoredPosition = new Vector2(0, -50);
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
            hud.OpponentPortrait = portraitImg;
            hud.OpponentNameLabel = oppNameLabel;
            hud.OpponentArchetypeLabel = oppArchLabel;

            // ----- Game-over panel -----
            var goPanel = NewChild(canvasRT, "GameOverPanel");
            goPanel.anchorMin = new Vector2(0.5f, 0.5f);
            goPanel.anchorMax = new Vector2(0.5f, 0.5f);
            goPanel.sizeDelta = new Vector2(600, 280);
            var goBg = goPanel.gameObject.AddComponent<Image>();
            goBg.color = ThemePalette.ModalOverlay;
            var goLabel = AddText(goPanel, "GameOverLabel", "Game over",
                anchorMin: new Vector2(0, 0.5f), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 44, color: Color.white, font: HeadingFont);
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
            acBg.color = ThemePalette.ModalOverlay;
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
            var autoPlayBtn = AddButton(canvasRT, "AutoPlayButton", "Auto: OFF", secondary: true);
            autoPlayBtn.anchorMin = new Vector2(0, 0);
            autoPlayBtn.anchorMax = new Vector2(0, 0);
            autoPlayBtn.pivot = new Vector2(0, 0);
            autoPlayBtn.sizeDelta = new Vector2(120, 36);
            autoPlayBtn.anchoredPosition = new Vector2(16, 52);
            hud.AutoPlayButton = autoPlayBtn.GetComponent<Button>();

            // ----- Tooltip label (bottom-left, hidden by default) -----
            var tooltipLabel = AddText(canvasRT, "TooltipLabel", "",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.4f, 0),
                pivot: new Vector2(0, 0),
                alignment: TextAlignmentOptions.BottomLeft, fontSize: 18,
                color: new Color(1, 1, 1, 0.85f));
            var tooltipRT = (RectTransform)tooltipLabel.transform;
            tooltipRT.sizeDelta = new Vector2(0, 50);
            tooltipRT.anchoredPosition = new Vector2(16, 52);
            tooltipLabel.enableWordWrapping = true;
            tooltipLabel.gameObject.SetActive(false);
            hud.TooltipLabel = tooltipLabel;

            // ----- Deck top label for Spy's Monocle (right side, above deck) -----
            var deckTopLabel = AddText(canvasRT, "DeckTopLabel", "",
                anchorMin: new Vector2(0.7f, 0), anchorMax: new Vector2(1f, 0),
                pivot: new Vector2(1, 0),
                alignment: TextAlignmentOptions.BottomRight, fontSize: 18,
                color: ThemePalette.Gold, font: MonoFont);
            var deckTopRT = (RectTransform)deckTopLabel.transform;
            deckTopRT.sizeDelta = new Vector2(0, 40);
            deckTopRT.anchoredPosition = new Vector2(-16, 52);
            deckTopLabel.gameObject.SetActive(false);
            hud.DeckTopLabel = deckTopLabel;

            // ----- Info label for Marked Deck (top-right, below HUD bar) -----
            var infoLabel = AddText(canvasRT, "InfoLabel", "",
                anchorMin: new Vector2(0.4f, 1), anchorMax: new Vector2(1f, 1),
                pivot: new Vector2(1, 1),
                alignment: TextAlignmentOptions.TopRight, fontSize: 16,
                color: ThemePalette.Amber, font: MonoFont);
            var infoRT = (RectTransform)infoLabel.transform;
            infoRT.sizeDelta = new Vector2(0, 35);
            infoRT.anchoredPosition = new Vector2(-16, -48);
            infoLabel.enableWordWrapping = true;
            infoLabel.gameObject.SetActive(false);
            hud.InfoLabel = infoLabel;

            // ----- Player resource label (bottom-left, near hand) -----
            var playerResLabel = AddText(canvasRT, "PlayerResourceLabel", "",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.3f, 0),
                pivot: new Vector2(0, 0),
                alignment: TextAlignmentOptions.BottomLeft, fontSize: 18,
                color: ThemePalette.Gold, font: MonoFont);
            var playerResRT = (RectTransform)playerResLabel.transform;
            playerResRT.sizeDelta = new Vector2(0, 30);
            playerResRT.anchoredPosition = new Vector2(16, 170);
            playerResLabel.gameObject.SetActive(false);
            hud.PlayerResourceLabel = playerResLabel;

            // ----- Opponent resource label (top-left, near opponent area) -----
            var oppResLabel = AddText(canvasRT, "OpponentResourceLabel", "",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(0.3f, 1),
                pivot: new Vector2(0, 1),
                alignment: TextAlignmentOptions.TopLeft, fontSize: 16,
                color: ThemePalette.Gold, font: MonoFont);
            var oppResRT = (RectTransform)oppResLabel.transform;
            oppResRT.sizeDelta = new Vector2(0, 26);
            oppResRT.anchoredPosition = new Vector2(16, -48);
            oppResLabel.gameObject.SetActive(false);
            hud.OpponentResourceLabel = oppResLabel;

            // ----- Vignette overlay (on top of all game elements) -----
            var vignette = NewChild(canvasRT, "VignetteOverlay");
            var vignetteImg = vignette.gameObject.AddComponent<Image>();
            var vignetteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Textures/vignette_overlay.png");
            if (vignetteSprite) { vignetteImg.sprite = vignetteSprite; vignetteImg.color = Color.white; }
            else vignetteImg.color = new Color(0, 0, 0, 0);
            vignetteImg.raycastTarget = false;
            FillParent(vignette);

            // ----- Wrap existing match UI in a MatchPanel group -----
            var matchPanel = NewChild(canvasRT, "MatchPanel");
            FillParent(matchPanel);
            // Reparent all match-specific UI under MatchPanel
            tableBg.SetParent(matchPanel, true);
            felt.SetParent(matchPanel, true);
            frame.SetParent(matchPanel, true);
            vignette.SetParent(matchPanel, true); // behind HUD/cards so it doesn't obscure UI
            hudBar.SetParent(matchPanel, true);
            nameplate.SetParent(matchPanel, true);
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
            var mapBgImg = mapPanel.gameObject.AddComponent<Image>();
            mapBgImg.color = ThemePalette.DeepNavy;
            // Venue background (dimmed, set at runtime by RunManager)
            var mapVenueBg = NewChild(mapPanel.GetComponent<RectTransform>(), "MapVenueBg");
            var mapVenueBgImg = mapVenueBg.gameObject.AddComponent<Image>();
            mapVenueBgImg.color = new Color(1, 1, 1, 0.4f);
            mapVenueBgImg.raycastTarget = false;
            FillParent(mapVenueBg);
            mapVenueBg.SetAsFirstSibling();

            var mapTitle = AddText(mapPanel, "MapTitle", "Act 1 — The Bilge Rat Tavern",
                anchorMin: new Vector2(0.1f, 0.82f), anchorMax: new Vector2(0.9f, 0.95f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 36, color: ThemePalette.Gold, font: HeadingFont);

            var mapSubtitle = AddText(mapPanel, "MapSubtitle", "Choose your path:",
                anchorMin: new Vector2(0.1f, 0.74f), anchorMax: new Vector2(0.9f, 0.82f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 24, color: ThemePalette.DustyTan);

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

            // Vignette overlay for map
            var mapVignette = NewChild(mapPanel, "MapVignette");
            var mapVignetteImg = mapVignette.gameObject.AddComponent<Image>();
            if (vignetteSprite) { mapVignetteImg.sprite = vignetteSprite; mapVignetteImg.color = new Color(1, 1, 1, 0.7f); }
            else mapVignetteImg.color = new Color(0, 0, 0, 0);
            mapVignetteImg.raycastTarget = false;
            FillParent(mapVignette);
            mapVignette.SetSiblingIndex(1); // after venue bg, before UI elements

            mapPanel.gameObject.SetActive(false);

            // ----- Result Panel -----
            var resultPanel = NewChild(canvasRT, "ResultPanel");
            FillParent(resultPanel);
            var resultBg = resultPanel.gameObject.AddComponent<Image>();
            resultBg.color = ThemePalette.ResultOverlay;
            // Venue background behind overlay
            var resultVenueBg = NewChild(resultPanel.GetComponent<RectTransform>(), "ResultVenueBg");
            var resultVenueBgImg = resultVenueBg.gameObject.AddComponent<Image>();
            resultVenueBgImg.color = new Color(1, 1, 1, 0.25f);
            resultVenueBgImg.raycastTarget = false;
            FillParent(resultVenueBg);
            resultVenueBg.SetAsFirstSibling();
            // Vignette
            var resultVignette = NewChild(resultPanel.GetComponent<RectTransform>(), "ResultVignette");
            var resultVignetteImg = resultVignette.gameObject.AddComponent<Image>();
            if (vignetteSprite) { resultVignetteImg.sprite = vignetteSprite; resultVignetteImg.color = new Color(1, 1, 1, 0.8f); }
            else resultVignetteImg.color = new Color(0, 0, 0, 0);
            resultVignetteImg.raycastTarget = false;
            FillParent(resultVignette);
            resultVignette.SetSiblingIndex(1);

            var resultTitle = AddText(resultPanel, "ResultTitle", "Victory!",
                anchorMin: new Vector2(0.1f, 0.82f), anchorMax: new Vector2(0.9f, 0.95f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 42, color: ThemePalette.Gold, font: HeadingFont);

            var resultDetails = AddText(resultPanel, "ResultDetails", "",
                anchorMin: new Vector2(0.15f, 0.72f), anchorMax: new Vector2(0.85f, 0.82f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: Color.white);
            resultDetails.enableWordWrapping = true;

            var resultReward = AddText(resultPanel, "ResultReward", "",
                anchorMin: new Vector2(0.15f, 0.64f), anchorMax: new Vector2(0.85f, 0.72f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 26, color: ThemePalette.Sage);

            // Ability pick area
            var abilityPickLabel = AddText(resultPanel, "AbilityPickLabel", "Choose an ability:",
                anchorMin: new Vector2(0.15f, 0.56f), anchorMax: new Vector2(0.85f, 0.64f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 24, color: ThemePalette.Gold);

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
            runOverBg.color = ThemePalette.Midnight;
            // Venue background
            var runOverVenueBg = NewChild(runOverPanel.GetComponent<RectTransform>(), "RunOverVenueBg");
            var runOverVenueBgImg = runOverVenueBg.gameObject.AddComponent<Image>();
            runOverVenueBgImg.color = new Color(1, 1, 1, 0.2f);
            runOverVenueBgImg.raycastTarget = false;
            FillParent(runOverVenueBg);
            runOverVenueBg.SetAsFirstSibling();
            // Vignette
            var runOverVignette = NewChild(runOverPanel.GetComponent<RectTransform>(), "RunOverVignette");
            var runOverVignetteImg = runOverVignette.gameObject.AddComponent<Image>();
            if (vignetteSprite) { runOverVignetteImg.sprite = vignetteSprite; runOverVignetteImg.color = Color.white; }
            else runOverVignetteImg.color = new Color(0, 0, 0, 0);
            runOverVignetteImg.raycastTarget = false;
            FillParent(runOverVignette);
            runOverVignette.SetSiblingIndex(1);

            var runOverTitle = AddText(runOverPanel, "RunOverTitle", "Run Over",
                anchorMin: new Vector2(0.1f, 0.65f), anchorMax: new Vector2(0.9f, 0.88f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 52, color: ThemePalette.Gold, font: HeadingFont);

            // Decorative divider under title
            var divider = NewChild(runOverPanel, "Divider");
            divider.anchorMin = new Vector2(0.35f, 0.63f);
            divider.anchorMax = new Vector2(0.65f, 0.63f);
            divider.sizeDelta = new Vector2(0, 2);
            var divImg = divider.gameObject.AddComponent<Image>();
            divImg.color = new Color(ThemePalette.Gold.r, ThemePalette.Gold.g, ThemePalette.Gold.b, 0.4f);
            divImg.raycastTarget = false;

            var runOverStats = AddText(runOverPanel, "RunOverStats", "",
                anchorMin: new Vector2(0.2f, 0.28f), anchorMax: new Vector2(0.8f, 0.60f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: ThemePalette.Parchment);
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
            shopBg.color = ThemePalette.DeepNavy;
            // Scene background
            var shopSceneBg = NewChild(shopPanel.GetComponent<RectTransform>(), "ShopSceneBg");
            var shopSceneBgImg = shopSceneBg.gameObject.AddComponent<Image>();
            var shopSceneSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Scenes/scene_shop.png");
            if (shopSceneSprite) { shopSceneBgImg.sprite = shopSceneSprite; shopSceneBgImg.color = new Color(1, 1, 1, 0.4f); }
            else shopSceneBgImg.color = new Color(0, 0, 0, 0);
            shopSceneBgImg.raycastTarget = false;
            FillParent(shopSceneBg);
            shopSceneBg.SetAsFirstSibling();
            // Vignette
            var shopVignette = NewChild(shopPanel.GetComponent<RectTransform>(), "ShopVignette");
            var shopVignetteImg = shopVignette.gameObject.AddComponent<Image>();
            if (vignetteSprite) { shopVignetteImg.sprite = vignetteSprite; shopVignetteImg.color = new Color(1, 1, 1, 0.7f); }
            else shopVignetteImg.color = new Color(0, 0, 0, 0);
            shopVignetteImg.raycastTarget = false;
            FillParent(shopVignette);
            shopVignette.SetSiblingIndex(1);

            var shopTitle = AddText(shopPanel, "ShopTitle", "The Fence",
                anchorMin: new Vector2(0.1f, 0.82f), anchorMax: new Vector2(0.9f, 0.95f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 36, color: ThemePalette.Gold, font: HeadingFont);

            var shopFlorins = AddText(shopPanel, "ShopFlorins", "Your purse: 0 Florins",
                anchorMin: new Vector2(0.1f, 0.74f), anchorMax: new Vector2(0.9f, 0.82f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: ThemePalette.Gold, font: MonoFont);

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
            eventBg.color = ThemePalette.Midnight;
            // Scene background
            var eventSceneBg = NewChild(eventPanel.GetComponent<RectTransform>(), "EventSceneBg");
            var eventSceneBgImg = eventSceneBg.gameObject.AddComponent<Image>();
            var eventSceneSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Scenes/scene_rumor.png");
            if (eventSceneSprite) { eventSceneBgImg.sprite = eventSceneSprite; eventSceneBgImg.color = new Color(1, 1, 1, 0.4f); }
            else eventSceneBgImg.color = new Color(0, 0, 0, 0);
            eventSceneBgImg.raycastTarget = false;
            FillParent(eventSceneBg);
            eventSceneBg.SetAsFirstSibling();
            // Vignette
            var eventVignette = NewChild(eventPanel.GetComponent<RectTransform>(), "EventVignette");
            var eventVignetteImg = eventVignette.gameObject.AddComponent<Image>();
            if (vignetteSprite) { eventVignetteImg.sprite = vignetteSprite; eventVignetteImg.color = new Color(1, 1, 1, 0.7f); }
            else eventVignetteImg.color = new Color(0, 0, 0, 0);
            eventVignetteImg.raycastTarget = false;
            FillParent(eventVignette);
            eventVignette.SetSiblingIndex(1);

            var eventTitle = AddText(eventPanel, "EventTitle", "Event",
                anchorMin: new Vector2(0.1f, 0.78f), anchorMax: new Vector2(0.9f, 0.92f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 36, color: ThemePalette.Gold, font: HeadingFont);

            var eventDesc = AddText(eventPanel, "EventDesc", "",
                anchorMin: new Vector2(0.15f, 0.52f), anchorMax: new Vector2(0.85f, 0.76f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 24, color: Color.white);
            eventDesc.enableWordWrapping = true;

            var eventOutcome = AddText(eventPanel, "EventOutcome", "",
                anchorMin: new Vector2(0.15f, 0.35f), anchorMax: new Vector2(0.85f, 0.50f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 26, color: ThemePalette.Sage);
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
            runHudPanel.sizeDelta = new Vector2(0, 36);
            var runHudBg = runHudPanel.gameObject.AddComponent<Image>();
            runHudBg.color = new Color(0, 0, 0, 0.65f);

            var prestigeLabel = AddText(runHudPanel, "PrestigeLabel", "Prestige: ♥♥♥♥",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.25f, 1),
                pivot: new Vector2(0, 0.5f), alignment: TextAlignmentOptions.MidlineLeft,
                fontSize: 13, color: ThemePalette.PrestigeRed, font: MonoFont);
            ((RectTransform)prestigeLabel.transform).offsetMin = new Vector2(16, 0);

            var florinsLabel = AddText(runHudPanel, "FlorinsLabel", "Florins: 0",
                anchorMin: new Vector2(0.25f, 0), anchorMax: new Vector2(0.5f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 13, color: ThemePalette.Gold, font: MonoFont);

            var actLabel = AddText(runHudPanel, "ActLabel", "Act 1 of 5",
                anchorMin: new Vector2(0.5f, 0), anchorMax: new Vector2(0.75f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 13, color: Color.white);

            var abilitiesLabel = AddText(runHudPanel, "AbilitiesLabel", "Abilities: 4/5",
                anchorMin: new Vector2(0.75f, 0), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(1, 0.5f), alignment: TextAlignmentOptions.MidlineRight,
                fontSize: 13, color: ThemePalette.AbilityBlue, font: MonoFont);
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

            // Match table theme
            rm.TableBackgroundImage = tableBgImg;
            rm.TableFeltImage = feltImg;
            rm.TableFrameImage = frameEdgeImg;
            rm.VignetteImage = vignetteImg;
            rm.VignetteSprite = vignetteSprite;

            // Load table surface sprites for per-act theming
            var tableSurfaces = new[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tables/table_tavern.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tables/table_merchant.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tables/table_guild.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tables/table_library.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tables/table_salon.png"),
            };
            rm.TableSurfaceSprites = tableSurfaces;
            rm.MapVenueBgImage = mapVenueBgImg;
            rm.ResultVenueBgImage = resultVenueBgImg;
            rm.RunOverVenueBgImage = runOverVenueBgImg;

            // Load venue background sprites for per-act theming
            var venueBackgrounds = new[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/bg_tavern.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/bg_merchant.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/bg_guild.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/bg_library.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/bg_salon.png"),
            };
            rm.VenueBackgroundSprites = venueBackgrounds;

            // Load opponent portrait sprites
            var allOpponents = new System.Collections.Generic.List<OpponentProfile>();
            for (int act = 0; act < 5; act++)
                allOpponents.AddRange(OpponentRoster.AllForAct(act));
            var portraitNames = new System.Collections.Generic.List<string>();
            var portraitSprites = new System.Collections.Generic.List<Sprite>();
            foreach (var opp in allOpponents)
            {
                var slug = opp.Name.ToLower().Replace(" ", "_");
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Portraits/portrait_{slug}.png");
                portraitNames.Add(opp.Name);
                portraitSprites.Add(sprite);
            }
            rm.PortraitNames = portraitNames.ToArray();
            rm.PortraitSprites = portraitSprites.ToArray();

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
            TextAlignmentOptions alignment, float fontSize, Color color,
            TMP_FontAsset font = null)
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
            var f = font ? font : DefaultFont;
            if (f) t.font = f;
            t.text = text;
            t.alignment = alignment;
            t.fontSize = fontSize;
            t.color = color;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            return t;
        }

        static RectTransform AddButton(RectTransform parent, string name, string label, bool secondary = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            var img = go.GetComponent<Image>();
            img.color = secondary ? ThemePalette.DarkSlate : ThemePalette.ButtonGoldBg;
            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = secondary ? ThemePalette.WarmSlate : ThemePalette.ButtonGoldHover;
            colors.disabledColor = ThemePalette.ButtonGoldDisabled;
            btn.colors = colors;

            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(go.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            FillParent(lblRT);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            var btnFont = HeadingFont ? HeadingFont : DefaultFont;
            if (btnFont) lbl.font = btnFont;
            lbl.text = label;
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 15;
            lbl.color = secondary ? ThemePalette.Parchment : ThemePalette.ButtonGoldText;
            lbl.raycastTarget = false;
            return rt;
        }
    }
}
