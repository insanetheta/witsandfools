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

            // ----- Table background: clean green felt (replaces the busy per-act wood venue sprite) -----
            // Venue identity lives on the map/event/rest screens now; the board reads as a card table.
            // Radial focus comes from the vignette overlay; subtle per-act warmth from the felt tint below.
            var tableBg = NewChild(canvasRT, "TableBackground");
            var tableBgImg = tableBg.gameObject.AddComponent<Image>();
            var feltSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Textures/felt_smooth.png");
            if (feltSprite) { tableBgImg.sprite = feltSprite; tableBgImg.color = Color.white; }
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

            // Thin gold hairline frame (subtle edge so the clean felt isn't a bare rectangle —
            // richness from a fine line, not a busy carved border). Per-act accent kept faint.
            var frame = NewChild(canvasRT, "TableFrame");
            frame.anchorMin = new Vector2(0, 0);
            frame.anchorMax = new Vector2(1, 1);
            frame.offsetMin = new Vector2(12, 12);
            frame.offsetMax = new Vector2(-12, -12);
            var frameColor = new Color(
                ThemePalette.ActFrameColor[0].r,
                ThemePalette.ActFrameColor[0].g,
                ThemePalette.ActFrameColor[0].b, 0.25f);
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

            // ----- Opponent identity panel (top-left): portrait, name, hand count, pips, race meter -----
            var oppPanel = BuildIdentityPanel(canvasRT, "OpponentPanel",
                anchorTop: true, keyline: ThemePalette.VenetianRed,
                out var portraitImg, out var oppMonogram, out var oppNameLabel, out var oppArchLabel,
                out var oppHandCountP, out var oppResLabelP, out var oppPips,
                out var oppRaceFill, out var oppRaceLabel);

            // ----- Action zone (bottom-right): phase line + bout chip + commit button -----
            var actionZone = NewChild(canvasRT, "ActionZone");
            actionZone.anchorMin = new Vector2(1, 0);
            actionZone.anchorMax = new Vector2(1, 0);
            actionZone.pivot = new Vector2(1, 0);
            actionZone.sizeDelta = new Vector2(330, 175);
            actionZone.anchoredPosition = new Vector2(-20, 84);

            var phaseLine = AddText(actionZone, "ActionPhaseLine", "",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 1), alignment: TextAlignmentOptions.Center,
                fontSize: 15, color: ThemePalette.Sage, font: HeadingFont);
            ((RectTransform)phaseLine.transform).sizeDelta = new Vector2(0, 26);
            phaseLine.fontStyle = FontStyles.Bold;

            var boutChip = AddText(actionZone, "BoutChipLabel", "BOUT 1/12",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 1), alignment: TextAlignmentOptions.Center,
                fontSize: 13, color: ThemePalette.DustyTan, font: HeadingFont);
            var boutChipRT = (RectTransform)boutChip.transform;
            boutChipRT.sizeDelta = new Vector2(0, 22);
            boutChipRT.anchoredPosition = new Vector2(0, -30);

            var endBoutBtn = AddButton(actionZone, "EndBoutButton", "End Bout");
            endBoutBtn.anchorMin = new Vector2(0, 0);
            endBoutBtn.anchorMax = new Vector2(1, 0);
            endBoutBtn.pivot = new Vector2(0.5f, 0);
            endBoutBtn.sizeDelta = new Vector2(0, 66);
            endBoutBtn.anchoredPosition = Vector2.zero;

            // ----- Dual-deck piles: each player's draw pile on their side of the felt -----
            var oppDeckSlot = BuildDeckPile(canvasRT, "OpponentDeckPile",
                anchor: new Vector2(0, 1), pos: new Vector2(190, -290), rotation: -8f,
                edgeColor: ThemePalette.VenetianRed, label: "FOE DECK", out var oppDeckBadge);

            var playerDeckSlot = BuildDeckPile(canvasRT, "PlayerDeckPile",
                anchor: new Vector2(1, 0), pos: new Vector2(-420, 250), rotation: 7f,
                edgeColor: ThemePalette.Gold, label: "YOUR DECK", out var playerDeckBadge);

            // Removed pile: off the felt (out of the game), dimmed
            var discardSlot = BuildDeckPile(canvasRT, "RemovedPile",
                anchor: new Vector2(1, 1), pos: new Vector2(-110, -160), rotation: 4f,
                edgeColor: new Color(0.42f, 0.40f, 0.36f), label: "REMOVED", out var discardCountLabel,
                dimmed: true);

            // Legacy shared-deck anchor: kept for draw animations' origin in legacy mode.
            var deckSlot = NewChild(canvasRT, "DeckSlot");
            deckSlot.anchorMin = new Vector2(1, 0);
            deckSlot.anchorMax = new Vector2(1, 0);
            deckSlot.pivot = new Vector2(0.5f, 0.5f);
            deckSlot.sizeDelta = new Vector2(10, 10);
            deckSlot.anchoredPosition = new Vector2(-420, 250);
            var deckCountLabel = AddText(deckSlot, "DeckCountLabel", "",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 1, color: Color.clear, font: MonoFont);

            // ----- Trump panel: on the felt, mid right edge, with its rule spelled out -----
            var trumpPanel = NewChild(canvasRT, "TrumpPanel");
            trumpPanel.anchorMin = new Vector2(1, 0.5f);
            trumpPanel.anchorMax = new Vector2(1, 0.5f);
            trumpPanel.pivot = new Vector2(0.5f, 0.5f);
            trumpPanel.sizeDelta = new Vector2(200, 240);
            trumpPanel.anchoredPosition = new Vector2(-200, 30);

            // Dark backing plate behind the trump label + rule so the gold/tan text reads against the
            // busy wood frame instead of floating low-contrast on the felt.
            var trumpPlate = NewChild(trumpPanel, "TrumpPlate");
            trumpPlate.anchorMin = new Vector2(0.5f, 0);
            trumpPlate.anchorMax = new Vector2(0.5f, 0);
            trumpPlate.pivot = new Vector2(0.5f, 0);
            trumpPlate.sizeDelta = new Vector2(190, 74);
            trumpPlate.anchoredPosition = new Vector2(0, -4);
            var trumpPlateImg = trumpPlate.gameObject.AddComponent<Image>();
            trumpPlateImg.color = new Color(0.04f, 0.04f, 0.08f, 0.82f);
            trumpPlateImg.raycastTarget = false;

            var trumpSlot = NewChild(trumpPanel, "TrumpSlot");
            trumpSlot.anchorMin = new Vector2(0.5f, 1);
            trumpSlot.anchorMax = new Vector2(0.5f, 1);
            trumpSlot.pivot = new Vector2(0.5f, 1);
            trumpSlot.sizeDelta = new Vector2(110, 160);
            trumpSlot.anchoredPosition = new Vector2(0, 0);
            trumpSlot.localRotation = Quaternion.Euler(0, 0, 12f);

            var trumpLabel = AddText(trumpPanel, "TrumpLabel", "TRUMP ♥",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 0),
                pivot: new Vector2(0.5f, 0), alignment: TextAlignmentOptions.Center,
                fontSize: 18, color: ThemePalette.Gold, font: HeadingFont);
            var trumpLabelRT = (RectTransform)trumpLabel.transform;
            trumpLabelRT.sizeDelta = new Vector2(0, 26);
            trumpLabelRT.anchoredPosition = new Vector2(0, 36);
            trumpLabel.fontStyle = FontStyles.Bold;

            var trumpRule = AddText(trumpPanel, "TrumpRuleLabel", "",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 0),
                pivot: new Vector2(0.5f, 0), alignment: TextAlignmentOptions.Center,
                fontSize: 14, color: ThemePalette.DustyTan, font: DefaultFont);
            var trumpRuleRT = (RectTransform)trumpRule.transform;
            trumpRuleRT.sizeDelta = new Vector2(0, 36);
            trumpRuleRT.anchoredPosition = new Vector2(0, 0);
            trumpRule.fontStyle = FontStyles.Italic;
            trumpRule.enableWordWrapping = true;

            var boutArea = NewChild(canvasRT, "BoutArea");
            boutArea.anchorMin = new Vector2(0.5f, 0.5f);
            boutArea.anchorMax = new Vector2(0.5f, 0.5f);
            boutArea.sizeDelta = new Vector2(900, 280);
            boutArea.anchoredPosition = new Vector2(0, 40);

            // Bout slot wells: a pool of recessed rounded wells, one per attacker/defender pair, so a
            // played card seats in a defined well instead of floating on bare felt (per unified_board.html).
            // GameManager positions + toggles them per bout. Each carries an "ATK" role tab (the slot is
            // an attack being answered). Children of BoutArea and built first, so cards render on top.
            var roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            var atkRed = new Color(0.66f, 0.20f, 0.20f, 0.95f);
            var boutWells = new RectTransform[6];
            for (int wi = 0; wi < boutWells.Length; wi++)
            {
                var well = NewChild(boutArea, "BoutWell" + wi);
                well.anchorMin = well.anchorMax = new Vector2(0.5f, 0.5f);
                well.pivot = new Vector2(0.5f, 0.5f);
                well.sizeDelta = new Vector2(152, 214);
                var wellImg = well.gameObject.AddComponent<Image>();
                if (roundedSprite) { wellImg.sprite = roundedSprite; wellImg.type = Image.Type.Sliced; }
                wellImg.color = new Color(0f, 0f, 0f, 0.26f);
                wellImg.raycastTarget = false;
                var wellEdge = well.gameObject.AddComponent<Outline>();
                wellEdge.effectColor = new Color(0.94f, 0.90f, 0.82f, 0.10f);
                wellEdge.effectDistance = new Vector2(1.5f, -1.5f);

                var tab = NewChild(well, "RoleTab");
                tab.anchorMin = tab.anchorMax = new Vector2(0, 1);
                tab.pivot = new Vector2(0, 1);
                tab.sizeDelta = new Vector2(42, 19);
                tab.anchoredPosition = new Vector2(8, 7);
                var tabImg = tab.gameObject.AddComponent<Image>();
                tabImg.color = atkRed;
                tabImg.raycastTarget = false;
                var tabLbl = AddText(tab, "RoleTabLabel", "ATK",
                    anchorMin: Vector2.zero, anchorMax: Vector2.one, pivot: new Vector2(0.5f, 0.5f),
                    alignment: TextAlignmentOptions.Center, fontSize: 11, color: Color.white, font: HeadingFont);
                tabLbl.fontStyle = FontStyles.Bold;

                well.gameObject.SetActive(false);
                boutWells[wi] = well;
            }

            // ----- Hands -----
            var playerHand = NewChild(canvasRT, "PlayerHand");
            playerHand.sizeDelta = new Vector2(900, 200);
            playerHand.anchorMin = new Vector2(0.5f, 0);
            playerHand.anchorMax = new Vector2(0.5f, 0);
            playerHand.pivot = new Vector2(0.5f, 0);
            playerHand.anchoredPosition = new Vector2(0, 100);
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
            // Flatter fan for the face-down foe hand — it carries no readable info, so a shallow arc
            // keeps its vertical footprint small and leaves clean space between it and the bout well.
            opponentHandLayout.MaxFanArc = 16f;
            opponentHandLayout.MaxArcLift = 8f;

            // ----- TableView wiring -----
            var tableGO = new GameObject("TableView");
            var table = tableGO.AddComponent<TableView>();
            table.PlayerHand = playerHandLayout;
            table.OpponentHand = opponentHandLayout;
            table.DeckSlot = deckSlot;
            table.DeckCountLabel = deckCountLabel;
            table.TrumpSlot = trumpSlot;
            table.DiscardSlot = discardSlot;
            table.DiscardCountLabel = discardCountLabel;
            table.BoutArea = boutArea;
            table.BoutWells = boutWells;
            table.CardSpawnRoot = canvasRT;
            table.PlayerDeckSlot = playerDeckSlot;
            table.PlayerDeckCountBadge = playerDeckBadge;
            table.OpponentDeckSlot = oppDeckSlot;
            table.OpponentDeckCountBadge = oppDeckBadge;
            table.TrumpRuleLabel = trumpRule;

            // ----- HUD wiring -----
            var hudGO = new GameObject("HudView");
            var hud = hudGO.AddComponent<HudView>();
            hud.TrumpLabel = trumpLabel;
            hud.EndBoutButton = endBoutBtn.GetComponent<Button>();
            hud.OpponentPortrait = portraitImg;
            hud.OpponentPortraitMonogram = oppMonogram;
            hud.OpponentNameLabel = oppNameLabel;
            hud.OpponentArchetypeLabel = oppArchLabel;
            hud.OpponentHandCount = oppHandCountP;
            hud.OpponentResourceLabel = oppResLabelP;
            hud.OpponentPips = oppPips;
            hud.OpponentRaceFill = oppRaceFill;
            hud.OpponentRaceLabel = oppRaceLabel;
            hud.ActionPhaseLine = phaseLine;
            hud.BoutChipLabel = boutChip;

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
            acPanel.sizeDelta = new Vector2(480, 260);
            var acBg = acPanel.gameObject.AddComponent<Image>();
            acBg.color = ThemePalette.ModalOverlay;
            var acLabel = AddText(acPanel, "AbilityChoiceLabel", "Ability",
                anchorMin: new Vector2(0.05f, 0.40f), anchorMax: new Vector2(0.95f, 0.92f),
                pivot: new Vector2(0.5f, 1f), alignment: TextAlignmentOptions.Center,
                fontSize: 24, color: Color.white);
            acLabel.enableWordWrapping = true;
            var normalBtn = AddButton(acPanel, "PlayNormallyButton", "Play normally");
            normalBtn.anchorMin = new Vector2(0.05f, 0);
            normalBtn.anchorMax = new Vector2(0.48f, 0);
            normalBtn.pivot = new Vector2(0.5f, 0);
            normalBtn.sizeDelta = new Vector2(0, 55);
            normalBtn.anchoredPosition = new Vector2(0, 65);
            var useBtn = AddButton(acPanel, "UseAbilityButton", "Use ability");
            useBtn.anchorMin = new Vector2(0.52f, 0);
            useBtn.anchorMax = new Vector2(0.95f, 0);
            useBtn.pivot = new Vector2(0.5f, 0);
            useBtn.sizeDelta = new Vector2(0, 55);
            useBtn.anchoredPosition = new Vector2(0, 65);
            var cancelBtn = AddButton(acPanel, "CancelAbilityButton", "Cancel", secondary: true);
            cancelBtn.anchorMin = new Vector2(0.25f, 0);
            cancelBtn.anchorMax = new Vector2(0.75f, 0);
            cancelBtn.pivot = new Vector2(0.5f, 0);
            cancelBtn.sizeDelta = new Vector2(0, 45);
            cancelBtn.anchoredPosition = new Vector2(0, 10);
            hud.AbilityChoicePanel = acPanel.gameObject;
            hud.AbilityChoiceLabel = acLabel;
            hud.PlayNormallyButton = normalBtn.GetComponent<Button>();
            hud.UseAbilityButton = useBtn.GetComponent<Button>();
            hud.CancelAbilityButton = cancelBtn.GetComponent<Button>();
            acPanel.gameObject.SetActive(false);

            // ----- Auto-play button (bottom-left, above run HUD) -----
            var autoPlayBtn = AddButton(canvasRT, "AutoPlayButton", "Auto: OFF", secondary: true);
            autoPlayBtn.anchorMin = new Vector2(0, 0);
            autoPlayBtn.anchorMax = new Vector2(0, 0);
            autoPlayBtn.pivot = new Vector2(0, 0);
            autoPlayBtn.sizeDelta = new Vector2(140, 46);
            autoPlayBtn.anchoredPosition = new Vector2(16, 80);
            hud.AutoPlayButton = autoPlayBtn.GetComponent<Button>();

            // ----- Tooltip label (bottom-left, above resource label) -----
            var tooltipLabel = AddText(canvasRT, "TooltipLabel", "",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.4f, 0),
                pivot: new Vector2(0, 0),
                alignment: TextAlignmentOptions.BottomLeft, fontSize: 18,
                color: new Color(1, 1, 1, 0.85f));
            var tooltipRT = (RectTransform)tooltipLabel.transform;
            tooltipRT.sizeDelta = new Vector2(0, 50);
            tooltipRT.anchoredPosition = new Vector2(16, 250);
            tooltipLabel.enableWordWrapping = true;
            tooltipLabel.gameObject.SetActive(false);
            hud.TooltipLabel = tooltipLabel;

            // ----- Deck top label for Spy's Monocle (right side, above run HUD) -----
            var deckTopLabel = AddText(canvasRT, "DeckTopLabel", "",
                anchorMin: new Vector2(0.7f, 0), anchorMax: new Vector2(1f, 0),
                pivot: new Vector2(1, 0),
                alignment: TextAlignmentOptions.BottomRight, fontSize: 18,
                color: ThemePalette.Gold, font: DefaultFont);
            var deckTopRT = (RectTransform)deckTopLabel.transform;
            deckTopRT.sizeDelta = new Vector2(0, 40);
            deckTopRT.anchoredPosition = new Vector2(-16, 200);
            deckTopLabel.gameObject.SetActive(false);
            hud.DeckTopLabel = deckTopLabel;

            // ----- Info label for Marked Deck (top-right, below HUD bar) -----
            var infoLabel = AddText(canvasRT, "InfoLabel", "",
                anchorMin: new Vector2(0.4f, 1), anchorMax: new Vector2(1f, 1),
                pivot: new Vector2(1, 1),
                alignment: TextAlignmentOptions.TopRight, fontSize: 20,
                color: ThemePalette.Amber, font: DefaultFont);
            var infoRT = (RectTransform)infoLabel.transform;
            infoRT.sizeDelta = new Vector2(0, 35);
            infoRT.anchoredPosition = new Vector2(-16, -76);
            infoLabel.enableWordWrapping = true;
            infoLabel.gameObject.SetActive(false);
            hud.InfoLabel = infoLabel;

            // ----- Player identity panel (bottom-left): mirrors the opponent panel -----
            var playerPanel = BuildIdentityPanel(canvasRT, "PlayerPanel",
                anchorTop: false, keyline: ThemePalette.Gold,
                out _, out var playerMonogram, out var playerNameLabel, out var playerTitleLabel,
                out var playerHandCountP, out var playerResLabelP, out var playerPips,
                out var playerRaceFill, out var playerRaceLabel);
            hud.PlayerNameLabel = playerNameLabel;
            hud.PlayerTitleLabel = playerTitleLabel;
            hud.PlayerPortraitLabel = playerMonogram;
            hud.PlayerHandCount = playerHandCountP;
            hud.PlayerResourceLabel = playerResLabelP;
            hud.PlayerPips = playerPips;
            hud.PlayerRaceFill = playerRaceFill;
            hud.PlayerRaceLabel = playerRaceLabel;
            playerNameLabel.text = "YOU";
            playerTitleLabel.text = "";

            // ----- Event log ("Table Talk", left edge): the record of plays and triggers -----
            var logPanel = NewChild(canvasRT, "EventLogPanel");
            logPanel.anchorMin = new Vector2(0, 0.5f);
            logPanel.anchorMax = new Vector2(0, 0.5f);
            logPanel.pivot = new Vector2(0, 0.5f);
            logPanel.sizeDelta = new Vector2(350, 168);
            logPanel.anchoredPosition = new Vector2(16, -30);
            var logBg = logPanel.gameObject.AddComponent<Image>();
            logBg.color = new Color(0.04f, 0.04f, 0.08f, 0.82f);
            logBg.raycastTarget = false;

            var logTitle = AddText(logPanel, "LogTitle", "TABLE TALK",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 1), alignment: TextAlignmentOptions.MidlineLeft,
                fontSize: 13, color: ThemePalette.DustyTan, font: HeadingFont);
            var logTitleRT = (RectTransform)logTitle.transform;
            logTitleRT.sizeDelta = new Vector2(0, 26);
            logTitle.margin = new Vector4(12, 0, 0, 0);

            var logText = AddText(logPanel, "EventLogText", "",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.BottomLeft,
                fontSize: 17, color: ThemePalette.DustyTan, font: DefaultFont);
            var logTextRT = (RectTransform)logText.transform;
            logTextRT.offsetMin = new Vector2(12, 8);
            logTextRT.offsetMax = new Vector2(-10, -26);
            logText.enableWordWrapping = false;
            logText.overflowMode = TextOverflowModes.Ellipsis;
            logText.richText = true;
            hud.EventLogText = logText;

            // Collapsed log button (shown at Compact/Comfortable; ResponsiveLayout swaps it with the
            // docked panel and wires its click to toggle the panel as a transient overlay).
            var logButton = AddButton(canvasRT, "EventLogButton", "LOG", secondary: true);
            // Left edge, vertically centered — the spot the docked log occupies at Spacious (free at
            // Compact), clear of the opponent panel (top-left) and player panel (bottom-left).
            logButton.anchorMin = new Vector2(0, 0.5f); logButton.anchorMax = new Vector2(0, 0.5f); logButton.pivot = new Vector2(0, 0.5f);
            logButton.sizeDelta = new Vector2(70, 50);
            logButton.anchoredPosition = new Vector2(16, 0);
            logButton.gameObject.SetActive(false);

            // ----- Phase ribbon (single phase indicator, pinned to the top strip per unified_board.html
            // so it never collides with the foe fan or the bout well — at Compact the fan+bout fill the
            // mid-band, so the ribbon must sit above them, not between). -----
            var boutPanel = NewChild(canvasRT, "BoutStatePanel");
            boutPanel.anchorMin = new Vector2(0.5f, 1);
            boutPanel.anchorMax = new Vector2(0.5f, 1);
            boutPanel.pivot = new Vector2(0.5f, 1);
            boutPanel.sizeDelta = new Vector2(520, 30);
            boutPanel.anchoredPosition = new Vector2(0, -6);
            var boutPanelBg = boutPanel.gameObject.AddComponent<Image>();
            boutPanelBg.color = new Color(0.04f, 0.04f, 0.08f, 0.82f);
            boutPanelBg.raycastTarget = false;
            boutPanel.gameObject.SetActive(false);

            var boutBanner = AddText(boutPanel, "BoutStateBanner", "",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f),
                alignment: TextAlignmentOptions.Center, fontSize: 19,
                color: Color.white, font: HeadingFont);
            boutBanner.fontStyle = FontStyles.Bold;
            hud.BoutStateBanner = boutBanner;

            // ----- Ability trigger feedback banner (center screen, with background panel) -----
            var feedbackPanel = NewChild(canvasRT, "AbilityFeedbackPanel");
            feedbackPanel.anchorMin = new Vector2(0.15f, 0.55f);
            feedbackPanel.anchorMax = new Vector2(0.85f, 0.63f);
            feedbackPanel.offsetMin = Vector2.zero;
            feedbackPanel.offsetMax = Vector2.zero;
            var feedbackBg = feedbackPanel.gameObject.AddComponent<Image>();
            feedbackBg.color = new Color(0, 0, 0, 0.7f);
            feedbackBg.raycastTarget = false;
            feedbackPanel.gameObject.SetActive(false);

            var feedbackLabel = AddText(feedbackPanel, "AbilityFeedbackLabel", "",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f),
                alignment: TextAlignmentOptions.Center, fontSize: 28,
                color: ThemePalette.Gold, font: DefaultFont);
            feedbackLabel.fontStyle = FontStyles.Bold;
            feedbackLabel.enableWordWrapping = true;
            feedbackLabel.raycastTarget = false;
            hud.AbilityFeedbackLabel = feedbackLabel;

            // ----- Peek overlay (shows actual card visuals when Peek fires) -----
            var peekPanel = NewChild(canvasRT, "PeekPanel");
            FillParent(peekPanel);
            var peekScrim = peekPanel.gameObject.AddComponent<Image>();
            peekScrim.color = new Color(0, 0, 0, 0.78f);
            peekScrim.raycastTarget = true;

            var peekTitle = AddText(peekPanel, "PeekTitle", "Deck Peek",
                anchorMin: new Vector2(0.2f, 0.74f), anchorMax: new Vector2(0.8f, 0.84f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 34, color: ThemePalette.Gold, font: HeadingFont);
            peekTitle.fontStyle = FontStyles.Bold;

            var peekSubtitle = AddText(peekPanel, "PeekSubtitle", "Sorted by trump suit & rank",
                anchorMin: new Vector2(0.2f, 0.68f), anchorMax: new Vector2(0.8f, 0.74f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 16, color: ThemePalette.Parchment, font: DefaultFont);
            peekSubtitle.fontStyle = FontStyles.Italic;

            var peekCardContainer = NewChild(peekPanel, "PeekCardContainer");
            peekCardContainer.anchorMin = new Vector2(0.5f, 0.5f);
            peekCardContainer.anchorMax = new Vector2(0.5f, 0.5f);
            peekCardContainer.sizeDelta = new Vector2(520, 240);
            peekCardContainer.anchoredPosition = new Vector2(0, 10);

            var peekNextDraw = AddText(peekPanel, "PeekNextDrawLabel", "",
                anchorMin: new Vector2(0.2f, 0.28f), anchorMax: new Vector2(0.8f, 0.36f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: ThemePalette.Gold, font: DefaultFont);
            peekNextDraw.fontStyle = FontStyles.Italic;

            var peekDismissBtn = AddButton(peekPanel, "PeekDismissButton", "Very Well");
            peekDismissBtn.anchorMin = new Vector2(0.5f, 0.22f);
            peekDismissBtn.anchorMax = new Vector2(0.5f, 0.22f);
            peekDismissBtn.pivot = new Vector2(0.5f, 0.5f);
            peekDismissBtn.sizeDelta = new Vector2(200, 55);

            hud.PeekPanel = peekPanel.gameObject;
            hud.PeekCardContainer = peekCardContainer;
            hud.PeekNextDrawLabel = peekNextDraw;
            hud.PeekDismissButton = peekDismissBtn.GetComponent<Button>();
            peekPanel.gameObject.SetActive(false);

            // ----- Vignette overlay (on top of all game elements) -----
            var vignette = NewChild(canvasRT, "VignetteOverlay");
            var vignetteImg = vignette.gameObject.AddComponent<Image>();
            var vignetteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Textures/vignette_overlay.png");
            if (vignetteSprite) { vignetteImg.sprite = vignetteSprite; vignetteImg.color = Color.white; }
            else vignetteImg.color = new Color(0, 0, 0, 0);
            vignetteImg.raycastTarget = false;
            FillParent(vignette);

            // ----- Wrap match UI in a MatchPanel group -----
            // The felt/vignette/frame are a full-screen backdrop that always fills the viewport. All
            // interactive/info UI lives in a ContentRoot that ResponsiveLayout clamps to a framed width
            // on wide screens — so the widgets frame the felt instead of stranding in far corners, while
            // the felt itself stays edge-to-edge (no pillarboxing).
            var matchPanel = NewChild(canvasRT, "MatchPanel");
            FillParent(matchPanel);
            tableBg.SetParent(matchPanel, true);
            felt.SetParent(matchPanel, true);
            frame.SetParent(matchPanel, true);
            vignette.SetParent(matchPanel, true); // behind content so it doesn't obscure UI

            var contentRoot = NewChild(matchPanel, "ContentRoot");
            FillParent(contentRoot);
            oppPanel.SetParent(contentRoot, true);
            actionZone.SetParent(contentRoot, true);
            oppDeckSlot.SetParent(contentRoot, true);
            playerDeckSlot.SetParent(contentRoot, true);
            deckSlot.SetParent(contentRoot, true);
            trumpPanel.SetParent(contentRoot, true);
            discardSlot.SetParent(contentRoot, true);
            boutArea.SetParent(contentRoot, true);   // bout wells are children of boutArea, move with it
            playerHand.SetParent(contentRoot, true);
            opponentHand.SetParent(contentRoot, true);
            goPanel.SetParent(contentRoot, true);
            acPanel.SetParent(contentRoot, true);
            autoPlayBtn.SetParent(contentRoot, true);
            ((RectTransform)tooltipLabel.transform).SetParent(contentRoot, true);
            ((RectTransform)deckTopLabel.transform).SetParent(contentRoot, true);
            ((RectTransform)infoLabel.transform).SetParent(contentRoot, true);
            boutPanel.SetParent(contentRoot, true);
            playerPanel.SetParent(contentRoot, true);
            logPanel.SetParent(contentRoot, true);
            logButton.SetParent(contentRoot, true);
            feedbackPanel.SetParent(contentRoot, true);
            peekPanel.SetParent(contentRoot, true);
            matchPanel.gameObject.SetActive(false);

            // ----- Map Panel (branching path layout) -----
            var mapPanel = NewChild(canvasRT, "MapPanel");
            FillParent(mapPanel);
            // Parchment background
            var mapBgImg = mapPanel.gameObject.AddComponent<Image>();
            var parchmentSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Map/map_parchment_bg.png");
            if (parchmentSprite) { mapBgImg.sprite = parchmentSprite; mapBgImg.color = Color.white; }
            else mapBgImg.color = ThemePalette.DeepNavy;
            // Venue background (dimmed overlay, set at runtime by RunManager)
            var mapVenueBg = NewChild(mapPanel.GetComponent<RectTransform>(), "MapVenueBg");
            var mapVenueBgImg = mapVenueBg.gameObject.AddComponent<Image>();
            mapVenueBgImg.color = new Color(1, 1, 1, 0f);
            mapVenueBgImg.raycastTarget = false;
            FillParent(mapVenueBg);
            // Vignette overlay
            var mapVignette = NewChild(mapPanel, "MapVignette");
            var mapVignetteImg = mapVignette.gameObject.AddComponent<Image>();
            if (vignetteSprite) { mapVignetteImg.sprite = vignetteSprite; mapVignetteImg.color = new Color(1, 1, 1, 0.4f); }
            else mapVignetteImg.color = new Color(0, 0, 0, 0);
            mapVignetteImg.raycastTarget = false;
            FillParent(mapVignette);

            // Dark gradient strip behind map title for legibility
            var mapTitleBg = NewChild(mapPanel, "MapTitleBg");
            var mapTitleBgImg = mapTitleBg.gameObject.AddComponent<Image>();
            mapTitleBgImg.color = new Color(0.05f, 0.03f, 0.02f, 0.6f);
            mapTitleBgImg.raycastTarget = false;
            mapTitleBg.anchorMin = new Vector2(0f, 0.86f);
            mapTitleBg.anchorMax = new Vector2(1f, 1f);
            mapTitleBg.offsetMin = Vector2.zero;
            mapTitleBg.offsetMax = Vector2.zero;

            var mapTitle = AddText(mapPanel, "MapTitle", "Act 1 — The Bilge Rat Tavern",
                anchorMin: new Vector2(0.05f, 0.88f), anchorMax: new Vector2(0.95f, 0.97f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 32, color: ThemePalette.Gold, font: HeadingFont);

            // Container for all map columns — nodes created dynamically by RunManager
            var mapNodeContainer = NewChild(mapPanel, "MapNodeContainer");
            mapNodeContainer.anchorMin = new Vector2(0.08f, 0.10f);
            mapNodeContainer.anchorMax = new Vector2(0.92f, 0.85f);
            mapNodeContainer.offsetMin = Vector2.zero;
            mapNodeContainer.offsetMax = Vector2.zero;

            // Path line container (behind nodes, drawn by RunManager)
            var mapPathContainer = NewChild(mapNodeContainer, "MapPathContainer");
            FillParent(mapPathContainer);
            mapPathContainer.SetAsFirstSibling();

            mapPanel.gameObject.SetActive(false);

            // ----- Result Panel -----
            var resultPanel = NewChild(canvasRT, "ResultPanel");
            FillParent(resultPanel);
            var resultBg = resultPanel.gameObject.AddComponent<Image>();
            resultBg.color = ThemePalette.ResultOverlay;
            // Venue background behind overlay
            var resultVenueBg = NewChild(resultPanel.GetComponent<RectTransform>(), "ResultVenueBg");
            var resultVenueBgImg = resultVenueBg.gameObject.AddComponent<Image>();
            resultVenueBgImg.color = new Color(1, 1, 1, 0f);
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
                fontSize: 55, color: ThemePalette.Gold, font: HeadingFont);

            var resultDetails = AddText(resultPanel, "ResultDetails", "",
                anchorMin: new Vector2(0.15f, 0.72f), anchorMax: new Vector2(0.85f, 0.82f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 29, color: Color.white);
            resultDetails.enableWordWrapping = true;

            var resultReward = AddText(resultPanel, "ResultReward", "",
                anchorMin: new Vector2(0.15f, 0.64f), anchorMax: new Vector2(0.85f, 0.72f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 34, color: ThemePalette.Sage);

            // Card reward area (shares vertical band with ability pick — never shown simultaneously)
            var cardRewardLabel = AddText(resultPanel, "CardRewardLabel", "Choose a card for your deck:",
                anchorMin: new Vector2(0.15f, 0.56f), anchorMax: new Vector2(0.85f, 0.64f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 31, color: ThemePalette.Gold, font: HeadingFont);
            cardRewardLabel.gameObject.SetActive(false);

            var cardRewardContainer = NewChild(resultPanel, "CardRewardContainer");
            cardRewardContainer.anchorMin = new Vector2(0.1f, 0.14f);
            cardRewardContainer.anchorMax = new Vector2(0.9f, 0.56f);
            cardRewardContainer.offsetMin = Vector2.zero;
            cardRewardContainer.offsetMax = Vector2.zero;
            var cardRewardLayout = cardRewardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            cardRewardLayout.spacing = 30;
            cardRewardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardRewardLayout.childControlWidth = false;
            cardRewardLayout.childControlHeight = false;
            cardRewardLayout.childForceExpandWidth = false;
            cardRewardLayout.childForceExpandHeight = false;
            cardRewardContainer.gameObject.SetActive(false);

            var cardRewardSkipBtn = AddButton(resultPanel, "CardRewardSkipButton", "Skip", secondary: true);
            cardRewardSkipBtn.anchorMin = new Vector2(0.5f, 0.06f);
            cardRewardSkipBtn.anchorMax = new Vector2(0.5f, 0.06f);
            cardRewardSkipBtn.pivot = new Vector2(0.5f, 0.5f);
            cardRewardSkipBtn.sizeDelta = new Vector2(200, 60);
            cardRewardSkipBtn.gameObject.SetActive(false);

            // Ability pick area
            var abilityPickLabel = AddText(resultPanel, "AbilityPickLabel", "Choose an ability:",
                anchorMin: new Vector2(0.15f, 0.56f), anchorMax: new Vector2(0.85f, 0.64f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 31, color: ThemePalette.Gold);

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
            abilityPickSkipBtn.anchorMin = new Vector2(0.65f, 0.08f);
            abilityPickSkipBtn.anchorMax = new Vector2(0.65f, 0.08f);
            abilityPickSkipBtn.pivot = new Vector2(0.5f, 0);
            abilityPickSkipBtn.sizeDelta = new Vector2(230, 70);

            var resultContinueBtn = AddButton(resultPanel, "ResultContinueButton", "Continue");
            resultContinueBtn.anchorMin = new Vector2(0.35f, 0.08f);
            resultContinueBtn.anchorMax = new Vector2(0.35f, 0.08f);
            resultContinueBtn.pivot = new Vector2(0.5f, 0);
            resultContinueBtn.sizeDelta = new Vector2(260, 70);

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
                anchorMin: new Vector2(0.1f, 0.78f), anchorMax: new Vector2(0.9f, 0.95f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 52, color: ThemePalette.Gold, font: HeadingFont);

            // Decorative divider under title
            var divider = NewChild(runOverPanel, "Divider");
            divider.anchorMin = new Vector2(0.35f, 0.76f);
            divider.anchorMax = new Vector2(0.65f, 0.76f);
            divider.sizeDelta = new Vector2(0, 2);
            var divImg = divider.gameObject.AddComponent<Image>();
            divImg.color = new Color(ThemePalette.Gold.r, ThemePalette.Gold.g, ThemePalette.Gold.b, 0.4f);
            divImg.raycastTarget = false;

            // Journey visualization strip (5 venue thumbnails)
            var journeyContainer = NewChild(runOverPanel, "JourneyContainer");
            journeyContainer.anchorMin = new Vector2(0.1f, 0.60f);
            journeyContainer.anchorMax = new Vector2(0.9f, 0.75f);
            journeyContainer.offsetMin = Vector2.zero;
            journeyContainer.offsetMax = Vector2.zero;
            var journeyLayout = journeyContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            journeyLayout.spacing = 12;
            journeyLayout.childAlignment = TextAnchor.MiddleCenter;
            journeyLayout.childControlWidth = false;
            journeyLayout.childControlHeight = false;
            journeyLayout.childForceExpandWidth = false;
            journeyLayout.childForceExpandHeight = false;

            // Backing plate so the run summary reads as a card, not faint text floating on the lit venue.
            var runOverStatsPlate = NewChild(runOverPanel, "RunOverStatsPlate");
            runOverStatsPlate.anchorMin = new Vector2(0.30f, 0.30f);
            runOverStatsPlate.anchorMax = new Vector2(0.70f, 0.59f);
            runOverStatsPlate.offsetMin = Vector2.zero;
            runOverStatsPlate.offsetMax = Vector2.zero;
            var runOverStatsPlateImg = runOverStatsPlate.gameObject.AddComponent<Image>();
            runOverStatsPlateImg.color = new Color(0.04f, 0.04f, 0.08f, 0.86f);
            runOverStatsPlateImg.raycastTarget = false;
            var runOverStatsPlateEdge = runOverStatsPlate.gameObject.AddComponent<Outline>();
            runOverStatsPlateEdge.effectColor = new Color(ThemePalette.Gold.r, ThemePalette.Gold.g, ThemePalette.Gold.b, 0.35f);
            runOverStatsPlateEdge.effectDistance = new Vector2(1.5f, -1.5f);

            var runOverStats = AddText(runOverPanel, "RunOverStats", "",
                anchorMin: new Vector2(0.2f, 0.32f), anchorMax: new Vector2(0.8f, 0.58f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 22, color: ThemePalette.Parchment);
            runOverStats.enableWordWrapping = true;

            // Ability chips container
            var abilityChipsContainer = NewChild(runOverPanel, "AbilityChipsContainer");
            abilityChipsContainer.anchorMin = new Vector2(0.1f, 0.20f);
            abilityChipsContainer.anchorMax = new Vector2(0.9f, 0.30f);
            abilityChipsContainer.offsetMin = Vector2.zero;
            abilityChipsContainer.offsetMax = Vector2.zero;
            var chipsLayout = abilityChipsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            chipsLayout.spacing = 8;
            chipsLayout.childAlignment = TextAnchor.MiddleCenter;
            chipsLayout.childControlWidth = false;
            chipsLayout.childControlHeight = false;
            chipsLayout.childForceExpandWidth = false;
            chipsLayout.childForceExpandHeight = false;

            var runOverRestartBtn = AddButton(runOverPanel, "RunOverRestartButton", "New Run");
            runOverRestartBtn.anchorMin = new Vector2(0.5f, 0.06f);
            runOverRestartBtn.anchorMax = new Vector2(0.5f, 0.06f);
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
                fontSize: 22, color: ThemePalette.Gold, font: DefaultFont);

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
                fontSize: 47, color: ThemePalette.Gold, font: HeadingFont);

            var eventDesc = AddText(eventPanel, "EventDesc", "",
                anchorMin: new Vector2(0.15f, 0.52f), anchorMax: new Vector2(0.85f, 0.76f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 31, color: Color.white);
            eventDesc.enableWordWrapping = true;

            var eventOutcome = AddText(eventPanel, "EventOutcome", "",
                anchorMin: new Vector2(0.15f, 0.35f), anchorMax: new Vector2(0.85f, 0.50f),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 34, color: ThemePalette.Sage);
            eventOutcome.enableWordWrapping = true;
            eventOutcome.gameObject.SetActive(false);

            var eventChoice1Btn = AddButton(eventPanel, "EventChoice1Button", "Choice 1");
            eventChoice1Btn.anchorMin = new Vector2(0.5f, 0.22f);
            eventChoice1Btn.anchorMax = new Vector2(0.5f, 0.22f);
            eventChoice1Btn.pivot = new Vector2(0.5f, 0.5f);
            eventChoice1Btn.sizeDelta = new Vector2(540, 78);
            var eventChoice1Label = eventChoice1Btn.GetComponentInChildren<TMP_Text>();

            var eventChoice2Btn = AddButton(eventPanel, "EventChoice2Button", "Choice 2");
            eventChoice2Btn.anchorMin = new Vector2(0.5f, 0.12f);
            eventChoice2Btn.anchorMax = new Vector2(0.5f, 0.12f);
            eventChoice2Btn.pivot = new Vector2(0.5f, 0.5f);
            eventChoice2Btn.sizeDelta = new Vector2(540, 78);
            var eventChoice2Label = eventChoice2Btn.GetComponentInChildren<TMP_Text>();

            var eventContinueBtn = AddButton(eventPanel, "EventContinueButton", "Continue");
            eventContinueBtn.anchorMin = new Vector2(0.5f, 0.06f);
            eventContinueBtn.anchorMax = new Vector2(0.5f, 0.06f);
            eventContinueBtn.pivot = new Vector2(0.5f, 0);
            eventContinueBtn.sizeDelta = new Vector2(340, 80);
            eventContinueBtn.gameObject.SetActive(false);

            eventPanel.gameObject.SetActive(false);

            // ----- Run HUD (persistent bar) -----
            var runHudPanel = NewChild(canvasRT, "RunHudPanel");
            runHudPanel.anchorMin = new Vector2(0, 0);
            runHudPanel.anchorMax = new Vector2(1, 0);
            runHudPanel.pivot = new Vector2(0.5f, 0);
            runHudPanel.sizeDelta = new Vector2(0, 70);
            var runHudBg = runHudPanel.gameObject.AddComponent<Image>();
            runHudBg.color = new Color(0, 0, 0, 0.75f);

            var prestigeLabel = AddText(runHudPanel, "PrestigeLabel", "Prestige: ♥♥♥♥",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0.25f, 1),
                pivot: new Vector2(0, 0.5f), alignment: TextAlignmentOptions.MidlineLeft,
                fontSize: 26, color: ThemePalette.PrestigeRed, font: DefaultFont);
            ((RectTransform)prestigeLabel.transform).offsetMin = new Vector2(24, 0);

            var florinsLabel = AddText(runHudPanel, "FlorinsLabel", "Florins: 0",
                anchorMin: new Vector2(0.25f, 0), anchorMax: new Vector2(0.5f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 26, color: ThemePalette.Gold, font: DefaultFont);

            var actLabel = AddText(runHudPanel, "ActLabel", "Act 1 of 5",
                anchorMin: new Vector2(0.5f, 0), anchorMax: new Vector2(0.75f, 1),
                pivot: new Vector2(0.5f, 0.5f), alignment: TextAlignmentOptions.Center,
                fontSize: 26, color: Color.white, font: DefaultFont);

            var abilitiesLabel = AddText(runHudPanel, "AbilitiesLabel", "Abilities: 4/5",
                anchorMin: new Vector2(0.75f, 0), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(1, 0.5f), alignment: TextAlignmentOptions.MidlineRight,
                fontSize: 26, color: ThemePalette.AbilityBlue, font: DefaultFont);
            ((RectTransform)abilitiesLabel.transform).offsetMax = new Vector2(-24, 0);

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
            rm.MapPathContainer = mapPathContainer;

            // Load map node sprites — indexed by MapNodeType enum order
            rm.MapNodeSprites = new[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Map/map_node_match.png"),  // RivalMatch=0
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Map/map_node_elite.png"),  // EliteMatch=1
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Map/map_node_shop.png"),   // Shop=2
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Map/map_node_rumor.png"),  // Rumor=3
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Map/map_node_rest.png"),   // Rest=4
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Map/map_node_boss.png"),   // BossMatch=5
            };
            rm.ResultTitleLabel = resultTitle;
            rm.ResultDetailsLabel = resultDetails;
            rm.ResultRewardLabel = resultReward;
            rm.ResultContinueButton = resultContinueBtn.GetComponent<Button>();
            rm.CardRewardLabel = cardRewardLabel;
            rm.CardRewardContainer = cardRewardContainer;
            rm.CardRewardSkipButton = cardRewardSkipBtn.GetComponent<Button>();
            rm.AbilityPickLabel = abilityPickLabel;
            rm.AbilityPickContainer = abilityPickContainer;
            rm.AbilityPickSkipButton = abilityPickSkipBtn.GetComponent<Button>();
            rm.RunOverTitleLabel = runOverTitle;
            rm.RunOverStatsLabel = runOverStats;
            rm.RunOverRestartButton = runOverRestartBtn.GetComponent<Button>();
            rm.RunOverJourneyContainer = journeyContainer;
            rm.RunOverAbilityChipsContainer = abilityChipsContainer;
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
                if (sprite == null)
                {
                    var altSlug = slug.Replace("the_", "");
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Portraits/portrait_{altSlug}.png");
                }
                portraitNames.Add(opp.Name);
                portraitSprites.Add(sprite);
            }
            rm.PortraitNames = portraitNames.ToArray();
            rm.PortraitSprites = portraitSprites.ToArray();

            // ----- Portrait landscape-lock overlay (top-level, on top of everything) -----
            var portraitOverlay = NewChild(canvasRT, "PortraitOverlay");
            FillParent(portraitOverlay);
            var poBg = portraitOverlay.gameObject.AddComponent<Image>();
            poBg.color = new Color(0.04f, 0.04f, 0.08f, 1f);
            var poIcon = AddText(portraitOverlay, "PoIcon", "↻",
                anchorMin: new Vector2(0, 0.5f), anchorMax: new Vector2(1, 0.5f), pivot: new Vector2(0.5f, 0),
                alignment: TextAlignmentOptions.Center, fontSize: 64, color: ThemePalette.Gold, font: HeadingFont);
            ((RectTransform)poIcon.transform).anchoredPosition = new Vector2(0, 20);
            var poText = AddText(portraitOverlay, "PoText", "ROTATE TO LANDSCAPE",
                anchorMin: new Vector2(0, 0.5f), anchorMax: new Vector2(1, 0.5f), pivot: new Vector2(0.5f, 1),
                alignment: TextAlignmentOptions.Center, fontSize: 26, color: ThemePalette.Gold, font: HeadingFont);
            ((RectTransform)poText.transform).anchoredPosition = new Vector2(0, -6);
            var poSub = AddText(portraitOverlay, "PoSub", "Wits & Fools is played at the table — turn your device sideways.",
                anchorMin: new Vector2(0.1f, 0.5f), anchorMax: new Vector2(0.9f, 0.5f), pivot: new Vector2(0.5f, 1),
                alignment: TextAlignmentOptions.Center, fontSize: 16, color: ThemePalette.DustyTan);
            ((RectTransform)poSub.transform).anchoredPosition = new Vector2(0, -44);
            poSub.enableWordWrapping = true;
            portraitOverlay.gameObject.SetActive(false);

            // ----- Responsive + cheat components on the Canvas (persist across the whole run) -----
            var responsive = canvasGO.AddComponent<ResponsiveLayout>();
            responsive.Scaler = scaler;
            responsive.PlayRoot = contentRoot;   // clamp the UI content; felt backdrop stays full-screen
            responsive.PortraitOverlay = portraitOverlay.gameObject;
            responsive.EventLogPanel = logPanel.gameObject;
            responsive.EventLogButton = logButton.gameObject;
            // Spacious-only chrome: identity subtitles, trump rule, and the "RACE TO ZERO" labels
            // (Compact keeps just the "N LEFT" value).
            var spaciousOnly = new System.Collections.Generic.List<GameObject>
            {
                oppArchLabel.gameObject, playerTitleLabel.gameObject, trumpRule.gameObject
            };
            var oppRaceTitle = oppPanel.Find("RaceTitle");
            if (oppRaceTitle) spaciousOnly.Add(oppRaceTitle.gameObject);
            var playerRaceTitle = playerPanel.Find("RaceTitle");
            if (playerRaceTitle) spaciousOnly.Add(playerRaceTitle.gameObject);
            responsive.SpaciousOnly = spaciousOnly.ToArray();
            logButton.GetComponent<Button>().onClick.AddListener(responsive.ToggleLog);
            canvasGO.AddComponent<CheatMenu>();

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

        // Identity panel: portrait + monogram, name/title, hand count + resource pips, race meter.
        static RectTransform BuildIdentityPanel(RectTransform canvasRT, string name, bool anchorTop, Color keyline,
            out Image portraitImg, out TMP_Text monogram, out TMP_Text nameLabel, out TMP_Text titleLabel,
            out TMP_Text handCount, out TMP_Text resLabel, out Image[] pips,
            out Image raceFill, out TMP_Text raceLabel)
        {
            var panel = NewChild(canvasRT, name);
            var a = anchorTop ? new Vector2(0, 1) : new Vector2(0, 0);
            panel.anchorMin = a; panel.anchorMax = a; panel.pivot = a;
            panel.sizeDelta = new Vector2(400, 150);
            panel.anchoredPosition = anchorTop ? new Vector2(16, -16) : new Vector2(16, 86);
            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.12f, 0.93f);
            bg.raycastTarget = false;

            var key = NewChild(panel, "Keyline");
            key.anchorMin = new Vector2(0, 0); key.anchorMax = new Vector2(0, 1); key.pivot = new Vector2(0, 0.5f);
            key.sizeDelta = new Vector2(4, 0);
            var keyImg = key.gameObject.AddComponent<Image>();
            keyImg.color = keyline;
            keyImg.raycastTarget = false;

            var portraitRT = NewChild(panel, "Portrait");
            portraitRT.anchorMin = new Vector2(0, 1); portraitRT.anchorMax = new Vector2(0, 1);
            portraitRT.pivot = new Vector2(0, 1);
            portraitRT.sizeDelta = new Vector2(60, 60);
            portraitRT.anchoredPosition = new Vector2(14, -12);
            portraitImg = portraitRT.gameObject.AddComponent<Image>();
            portraitImg.color = ThemePalette.WarmSlate;
            portraitImg.preserveAspect = true;
            portraitImg.raycastTarget = false;
            var portraitOutline = portraitRT.gameObject.AddComponent<Outline>();
            portraitOutline.effectColor = new Color(0.75f, 0.65f, 0.45f, 0.7f);
            portraitOutline.effectDistance = new Vector2(2, -2);

            monogram = AddText(portraitRT, "Monogram", "?",
                anchorMin: Vector2.zero, anchorMax: Vector2.one, pivot: new Vector2(0.5f, 0.5f),
                alignment: TextAlignmentOptions.Center, fontSize: 30, color: ThemePalette.Gold, font: HeadingFont);
            monogram.fontStyle = FontStyles.Bold;

            nameLabel = AddText(panel, "NameLabel", "—",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1), pivot: new Vector2(0, 1),
                alignment: TextAlignmentOptions.MidlineLeft, fontSize: 21, color: ThemePalette.Parchment, font: HeadingFont);
            var nameRT = (RectTransform)nameLabel.transform;
            nameRT.sizeDelta = new Vector2(-96, 32); nameRT.anchoredPosition = new Vector2(84, -10);

            titleLabel = AddText(panel, "TitleLabel", "",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1), pivot: new Vector2(0, 1),
                alignment: TextAlignmentOptions.MidlineLeft, fontSize: 15, color: ThemePalette.DustyTan);
            var titleRT = (RectTransform)titleLabel.transform;
            titleRT.sizeDelta = new Vector2(-96, 22); titleRT.anchoredPosition = new Vector2(84, -42);
            titleLabel.fontStyle = FontStyles.Italic;

            // statline: hand count + resource label + pips
            handCount = AddText(panel, "HandCount", "Hand 0",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(0, 1), pivot: new Vector2(0, 1),
                alignment: TextAlignmentOptions.MidlineLeft, fontSize: 17, color: ThemePalette.Parchment);
            var hcRT = (RectTransform)handCount.transform;
            hcRT.sizeDelta = new Vector2(100, 26); hcRT.anchoredPosition = new Vector2(16, -82);
            handCount.richText = true;

            resLabel = AddText(panel, "ResourceLabel", "",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(0, 1), pivot: new Vector2(0, 1),
                alignment: TextAlignmentOptions.MidlineLeft, fontSize: 15, color: ThemePalette.Gold);
            var resRT = (RectTransform)resLabel.transform;
            resRT.sizeDelta = new Vector2(70, 26); resRT.anchoredPosition = new Vector2(140, -82);

            pips = new Image[5];
            var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            for (int i = 0; i < 5; i++)
            {
                var pip = NewChild(panel, $"Pip{i}");
                pip.anchorMin = new Vector2(0, 1); pip.anchorMax = new Vector2(0, 1); pip.pivot = new Vector2(0, 1);
                pip.sizeDelta = new Vector2(16, 16);
                pip.anchoredPosition = new Vector2(216 + i * 22, -86);
                var pipImg = pip.gameObject.AddComponent<Image>();
                pipImg.sprite = knob;
                pipImg.color = new Color(0.35f, 0.28f, 0.19f, 0.9f);
                pipImg.raycastTarget = false;
                pips[i] = pipImg;
            }

            // race meter: label row + fill bar + WIN cap
            var raceTitle = AddText(panel, "RaceTitle", "RACE TO ZERO",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(0, 0), pivot: new Vector2(0, 0),
                alignment: TextAlignmentOptions.MidlineLeft, fontSize: 12, color: ThemePalette.DustyTan, font: HeadingFont);
            var rtTitleRT = (RectTransform)raceTitle.transform;
            rtTitleRT.sizeDelta = new Vector2(160, 20); rtTitleRT.anchoredPosition = new Vector2(16, 28);

            raceLabel = AddText(panel, "RaceLabel", "",
                anchorMin: new Vector2(1, 0), anchorMax: new Vector2(1, 0), pivot: new Vector2(1, 0),
                alignment: TextAlignmentOptions.MidlineRight, fontSize: 15, color: ThemePalette.Gold, font: HeadingFont);
            var raceLblRT = (RectTransform)raceLabel.transform;
            raceLblRT.sizeDelta = new Vector2(130, 20); raceLblRT.anchoredPosition = new Vector2(-48, 28);

            var barBg = NewChild(panel, "RaceBarBg");
            barBg.anchorMin = new Vector2(0, 0); barBg.anchorMax = new Vector2(1, 0); barBg.pivot = new Vector2(0, 0);
            barBg.offsetMin = new Vector2(16, 12); barBg.offsetMax = new Vector2(-48, 0);
            barBg.sizeDelta = new Vector2(barBg.sizeDelta.x, 11);
            var barBgImg = barBg.gameObject.AddComponent<Image>();
            barBgImg.color = new Color(0.04f, 0.06f, 0.09f, 1f);
            barBgImg.raycastTarget = false;

            var barFill = NewChild(barBg, "RaceBarFill");
            FillParent(barFill);
            barFill.offsetMin = new Vector2(1, 1); barFill.offsetMax = new Vector2(-1, -1);
            raceFill = barFill.gameObject.AddComponent<Image>();
            raceFill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            raceFill.type = Image.Type.Filled;
            raceFill.fillMethod = Image.FillMethod.Horizontal;
            raceFill.fillAmount = 0f;
            raceFill.color = ThemePalette.Gold;
            raceFill.raycastTarget = false;

            var winCap = AddText(panel, "WinCap", "WIN",
                anchorMin: new Vector2(1, 0), anchorMax: new Vector2(1, 0), pivot: new Vector2(1, 0),
                alignment: TextAlignmentOptions.MidlineLeft, fontSize: 11, color: ThemePalette.DustyTan, font: HeadingFont);
            var winRT = (RectTransform)winCap.transform;
            winRT.sizeDelta = new Vector2(34, 16); winRT.anchoredPosition = new Vector2(-12, 10);

            return panel;
        }

        // A physical draw/removed pile: 3 offset card backs, count badge, label.
        static RectTransform BuildDeckPile(RectTransform canvasRT, string name, Vector2 anchor, Vector2 pos,
            float rotation, Color edgeColor, string label, out TMP_Text countBadge, bool dimmed = false)
        {
            var pile = NewChild(canvasRT, name);
            pile.anchorMin = anchor; pile.anchorMax = anchor; pile.pivot = new Vector2(0.5f, 0.5f);
            pile.sizeDelta = new Vector2(110, 160);
            pile.anchoredPosition = pos;
            pile.localRotation = Quaternion.Euler(0, 0, rotation);

            var backSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Cards/card_back.png");
            float[] rots = { -3f, 2f, -0.5f };
            Vector2[] offs = { new(2, 3), new(-1, 1), Vector2.zero };
            for (int i = 0; i < 3; i++)
            {
                var cardRT = NewChild(pile, $"Stack{i}");
                cardRT.sizeDelta = new Vector2(100, 144);
                cardRT.anchoredPosition = offs[i];
                cardRT.localRotation = Quaternion.Euler(0, 0, rots[i]);
                var img = cardRT.gameObject.AddComponent<Image>();
                if (backSprite) { img.sprite = backSprite; img.color = dimmed ? new Color(0.5f, 0.48f, 0.46f) : Color.white; }
                else img.color = dimmed ? new Color(0.32f, 0.28f, 0.26f) : ThemePalette.CrimsonCard;
                img.raycastTarget = false;
                var edge = cardRT.gameObject.AddComponent<Outline>();
                edge.effectColor = edgeColor;
                edge.effectDistance = new Vector2(2, -2);
            }

            var knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            var badgeOuter = NewChild(pile, "CountBadgeOuter");
            badgeOuter.anchorMin = new Vector2(1, 0); badgeOuter.anchorMax = new Vector2(1, 0);
            badgeOuter.pivot = new Vector2(0.5f, 0.5f);
            badgeOuter.sizeDelta = new Vector2(46, 46);
            badgeOuter.anchoredPosition = new Vector2(6, 2);
            badgeOuter.localRotation = Quaternion.Euler(0, 0, -rotation); // keep badge upright
            var outerImg = badgeOuter.gameObject.AddComponent<Image>();
            outerImg.sprite = knobSprite;
            outerImg.color = dimmed ? ThemePalette.DustyTan : ThemePalette.Gold;
            outerImg.raycastTarget = false;

            var badgeInner = NewChild(badgeOuter, "CountBadgeInner");
            badgeInner.anchorMin = new Vector2(0.5f, 0.5f); badgeInner.anchorMax = new Vector2(0.5f, 0.5f);
            badgeInner.pivot = new Vector2(0.5f, 0.5f);
            badgeInner.sizeDelta = new Vector2(40, 40);
            var innerImg = badgeInner.gameObject.AddComponent<Image>();
            innerImg.sprite = knobSprite;
            innerImg.color = ThemePalette.DarkSlate;
            innerImg.raycastTarget = false;

            countBadge = AddText(badgeInner, "Count", "0",
                anchorMin: Vector2.zero, anchorMax: Vector2.one, pivot: new Vector2(0.5f, 0.5f),
                alignment: TextAlignmentOptions.Center, fontSize: 19,
                color: dimmed ? ThemePalette.DustyTan : ThemePalette.Gold, font: HeadingFont);
            countBadge.fontStyle = FontStyles.Bold;

            var pileLabel = AddText(pile, "PileLabel", label,
                anchorMin: new Vector2(0.5f, 0), anchorMax: new Vector2(0.5f, 0), pivot: new Vector2(0.5f, 1),
                alignment: TextAlignmentOptions.Center, fontSize: 14,
                color: ThemePalette.Parchment, font: HeadingFont);
            var pileLabelRT = (RectTransform)pileLabel.transform;
            pileLabelRT.sizeDelta = new Vector2(160, 22);
            pileLabelRT.anchoredPosition = new Vector2(0, -8);
            pileLabelRT.localRotation = Quaternion.Euler(0, 0, -rotation);

            return pile;
        }

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
            lbl.fontSize = 20;
            lbl.color = secondary ? Color.white : new Color(0.08f, 0.06f, 0.02f, 1f);
            lbl.raycastTarget = false;
            return rt;
        }
    }
}
