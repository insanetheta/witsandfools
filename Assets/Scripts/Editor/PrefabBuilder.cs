using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WitsAndFools.EditorTools
{
    public static class PrefabBuilder
    {
        const string PrefabPath = "Assets/Prefabs/CardView.prefab";
        const float CardWidth = 110f;
        const float CardHeight = 160f;
        const float OutlineThickness = 4f;

        static TMP_FontAsset DefaultFont => FontAssets.Mono;
        static TMP_FontAsset BodyFont => FontAssets.Body;
        static TMP_FontAsset HeadingFont => FontAssets.Heading;

        static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        // Design B card: full-bleed art, top scrim, upper-left rank over a subtle gold engraving,
        // doctrine gem, and a bottom info drawer (name + ability label) that slides up to reveal the
        // full ability body text for reward/deck/detail/hover.
        [MenuItem("Wits and Fools/Build/Card Prefab")]
        public static void BuildCardPrefab()
        {
            var cardBackSprite = LoadSprite("Assets/Art/Cards/card_back.png");
            var scrimTop       = LoadSprite("Assets/Art/Textures/card_scrim_top.png");
            var scrimBottom    = LoadSprite("Assets/Art/Textures/card_scrim_bottom.png");
            var engraveSprite  = LoadSprite("Assets/Art/Textures/card_engrave.png");
            var trigAttack     = LoadSprite("Assets/Art/Icons/trigger_attack.png");
            var trigDefend     = LoadSprite("Assets/Art/Icons/trigger_defend.png");
            var trigPassive    = LoadSprite("Assets/Art/Icons/trigger_passive.png");
            var rounded        = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

            var go = new GameObject("CardView", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(CardWidth, CardHeight);

            // outer glow (highlight states)
            var outline = CreateChildImage(go.transform, "Outline", new Vector2(CardWidth + OutlineThickness, CardHeight + OutlineThickness));
            outline.color = ThemePalette.OutlineNone;

            // shared background (dark behind the art; cream/back when no art / face-down)
            var background = CreateChildImage(go.transform, "Background", new Vector2(CardWidth, CardHeight));
            background.color = new Color(0.06f, 0.05f, 0.08f, 1f);

            // ---- Face ----
            var faceRoot = new GameObject("Face", typeof(RectTransform));
            faceRoot.transform.SetParent(go.transform, false);
            var faceRT = faceRoot.GetComponent<RectTransform>();
            Fill(faceRT);

            // full-bleed art
            var artGO = new GameObject("ArtImage", typeof(RectTransform), typeof(Image));
            artGO.transform.SetParent(faceRoot.transform, false);
            var artRT = (RectTransform)artGO.transform; Fill(artRT); artRT.offsetMin = new Vector2(2,2); artRT.offsetMax = new Vector2(-2,-2);
            var artImg = artGO.GetComponent<Image>(); artImg.raycastTarget = false; artImg.preserveAspect = false;
            artImg.color = Color.white;

            // top scrim (rank legibility)
            var topScrimGO = new GameObject("TopScrim", typeof(RectTransform), typeof(Image));
            topScrimGO.transform.SetParent(faceRoot.transform, false);
            var tsRT = (RectTransform)topScrimGO.transform;
            tsRT.anchorMin = new Vector2(0,1); tsRT.anchorMax = new Vector2(1,1); tsRT.pivot = new Vector2(0.5f,1);
            tsRT.sizeDelta = new Vector2(-4, 58); tsRT.anchoredPosition = new Vector2(0,-2);
            var tsImg = topScrimGO.GetComponent<Image>(); tsImg.sprite = scrimTop; tsImg.type = Image.Type.Simple; tsImg.raycastTarget = false;

            // engraved cartouche behind the rank (subtle)
            var engGO = new GameObject("Engrave", typeof(RectTransform), typeof(Image));
            engGO.transform.SetParent(faceRoot.transform, false);
            var engRT = (RectTransform)engGO.transform;
            engRT.anchorMin = new Vector2(0,1); engRT.anchorMax = new Vector2(0,1); engRT.pivot = new Vector2(0,1);
            engRT.sizeDelta = new Vector2(40,48); engRT.anchoredPosition = new Vector2(6,-5);
            var engImg = engGO.GetComponent<Image>(); engImg.sprite = engraveSprite; engImg.raycastTarget = false;
            engImg.color = new Color(0.91f, 0.78f, 0.40f, 0.42f);

            var rankTL = CreateChildText(faceRoot.transform, "RankTopLeft", "A♥",
                anchorMin:new Vector2(0,1), anchorMax:new Vector2(0,1), pivot:new Vector2(0,1),
                position:new Vector2(6,-5), size:new Vector2(40,48), alignment:TextAlignmentOptions.Center, fontSize:19);
            rankTL.fontStyle = FontStyles.Bold; rankTL.color = Color.white; if (HeadingFont) rankTL.font = HeadingFont;

            // doctrine gem (top-right, rotated 45°)
            var gemGO = new GameObject("DoctrineGem", typeof(RectTransform), typeof(Image));
            gemGO.transform.SetParent(faceRoot.transform, false);
            var gemRT = (RectTransform)gemGO.transform;
            gemRT.anchorMin = new Vector2(1,1); gemRT.anchorMax = new Vector2(1,1); gemRT.pivot = new Vector2(1,1);
            gemRT.sizeDelta = new Vector2(13,13); gemRT.anchoredPosition = new Vector2(-8,-8); gemRT.localRotation = Quaternion.Euler(0,0,45);
            var gemImg = gemGO.GetComponent<Image>(); gemImg.color = ThemePalette.Gold; gemImg.raycastTarget = false;

            // legacy center pip (fallback when a card has no art)
            var center = CreateChildText(faceRoot.transform, "CenterPip", "♥",
                anchorMin:new Vector2(0.5f,0.6f), anchorMax:new Vector2(0.5f,0.6f), pivot:new Vector2(0.5f,0.5f),
                position:Vector2.zero, size:new Vector2(80,80), alignment:TextAlignmentOptions.Center, fontSize:52);
            center.gameObject.SetActive(false);

            // ---- info drawer (clipped, slides up) ----
            // Stencil Mask, NOT RectMask2D: hand cards are rotated by the fan layout, and
            // RectMask2D clips in axis-aligned canvas space, slicing drawer content diagonally
            // on rotated cards (and leaking the body panel outside the drawer).
            var drawerGO = new GameObject("Drawer", typeof(RectTransform), typeof(Image), typeof(Mask));
            drawerGO.transform.SetParent(faceRoot.transform, false);
            var drawerRT = (RectTransform)drawerGO.transform;
            drawerRT.anchorMin = new Vector2(0,0); drawerRT.anchorMax = new Vector2(1,0); drawerRT.pivot = new Vector2(0.5f,0);
            drawerRT.sizeDelta = new Vector2(0, 38); drawerRT.anchoredPosition = Vector2.zero;
            drawerGO.GetComponent<Image>().raycastTarget = false;
            drawerGO.GetComponent<Mask>().showMaskGraphic = false;

            var dScrimGO = new GameObject("DrawerScrim", typeof(RectTransform), typeof(Image));
            dScrimGO.transform.SetParent(drawerGO.transform, false);
            Fill((RectTransform)dScrimGO.transform);
            var dScrim = dScrimGO.GetComponent<Image>(); dScrim.sprite = scrimBottom; dScrim.type = Image.Type.Simple; dScrim.raycastTarget = false;

            // ability strip (bottom of drawer)
            var badgeBgGO = new GameObject("AbilityBadgeBg", typeof(RectTransform), typeof(Image));
            badgeBgGO.transform.SetParent(drawerGO.transform, false);
            var badgeBgRT = (RectTransform)badgeBgGO.transform;
            badgeBgRT.anchorMin = new Vector2(0,0); badgeBgRT.anchorMax = new Vector2(1,0); badgeBgRT.pivot = new Vector2(0.5f,0);
            badgeBgRT.sizeDelta = new Vector2(0,17); badgeBgRT.anchoredPosition = Vector2.zero;
            var badgeBgImg = badgeBgGO.GetComponent<Image>(); badgeBgImg.color = ThemePalette.UtilColor; badgeBgImg.raycastTarget = false;
            var abilityBadge = CreateChildText(badgeBgGO.transform, "AbilityBadge", "",
                anchorMin:Vector2.zero, anchorMax:Vector2.one, pivot:new Vector2(0.5f,0.5f),
                position:Vector2.zero, size:Vector2.zero, alignment:TextAlignmentOptions.Center, fontSize:9);
            abilityBadge.fontStyle = FontStyles.Bold; abilityBadge.color = Color.white; if (HeadingFont) abilityBadge.font = HeadingFont;
            abilityBadge.enableAutoSizing = true; abilityBadge.fontSizeMin = 7; abilityBadge.fontSizeMax = 10;
            // keep the centered label clear of the timing glyph on the left
            var abRT = (RectTransform)abilityBadge.transform;
            abRT.offsetMin = new Vector2(16, 0); abRT.offsetMax = new Vector2(-16, 0);

            // trigger-timing glyph (sword = on attack, shield = on defend, sunburst = passive)
            var trigGO = new GameObject("TriggerIcon", typeof(RectTransform), typeof(Image));
            trigGO.transform.SetParent(badgeBgGO.transform, false);
            var trigRT = (RectTransform)trigGO.transform;
            trigRT.anchorMin = new Vector2(0, 0.5f); trigRT.anchorMax = new Vector2(0, 0.5f); trigRT.pivot = new Vector2(0, 0.5f);
            trigRT.sizeDelta = new Vector2(12, 12); trigRT.anchoredPosition = new Vector2(3, 0);
            var trigImg = trigGO.GetComponent<Image>(); trigImg.color = Color.white; trigImg.raycastTarget = false;
            trigGO.SetActive(false);

            // name (above the ability strip)
            var nameLabel = CreateChildText(drawerGO.transform, "NameLabel", "",
                anchorMin:new Vector2(0,0), anchorMax:new Vector2(1,0), pivot:new Vector2(0.5f,0),
                position:new Vector2(0,19), size:new Vector2(0,17), alignment:TextAlignmentOptions.Center, fontSize:11);
            nameLabel.color = ThemePalette.Gold; if (HeadingFont) nameLabel.font = HeadingFont;
            nameLabel.enableAutoSizing = true; nameLabel.fontSizeMin = 8; nameLabel.fontSizeMax = 12;

            // body text panel (revealed on open)
            var bodyGO = new GameObject("BodyPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            bodyGO.transform.SetParent(drawerGO.transform, false);
            var bodyRT = (RectTransform)bodyGO.transform;
            bodyRT.anchorMin = new Vector2(0,0); bodyRT.anchorMax = new Vector2(1,0); bodyRT.pivot = new Vector2(0.5f,0);
            bodyRT.sizeDelta = new Vector2(-12, 66); bodyRT.anchoredPosition = new Vector2(0, 39);
            var bodyImg = bodyGO.GetComponent<Image>();
            if (rounded) { bodyImg.sprite = rounded; bodyImg.type = Image.Type.Sliced; }
            bodyImg.color = new Color(0.047f, 0.039f, 0.063f, 0.62f); bodyImg.raycastTarget = false;
            var bodyEdge = bodyGO.AddComponent<Outline>();
            bodyEdge.effectColor = new Color(ThemePalette.Gold.r, ThemePalette.Gold.g, ThemePalette.Gold.b, 0.34f);
            bodyEdge.effectDistance = new Vector2(1f,-1f);
            var bodyGroup = bodyGO.GetComponent<CanvasGroup>();
            var bodyText = CreateChildText(bodyGO.transform, "BodyText", "",
                anchorMin:Vector2.zero, anchorMax:Vector2.one, pivot:new Vector2(0.5f,0.5f),
                position:Vector2.zero, size:new Vector2(-16,-12), alignment:TextAlignmentOptions.Top, fontSize:9);
            bodyText.color = new Color(0.93f, 0.89f, 0.82f); if (BodyFont) bodyText.font = BodyFont;
            bodyText.enableWordWrapping = true; bodyText.richText = true;
            ((RectTransform)bodyText.transform).offsetMin = new Vector2(8,6); ((RectTransform)bodyText.transform).offsetMax = new Vector2(-8,-6);

            // ---- rank-bonus chip (overflow, top-right) ----
            var bonusChipGO = new GameObject("BonusChip", typeof(RectTransform), typeof(Image));
            bonusChipGO.transform.SetParent(go.transform, false);
            var bonusRT = (RectTransform)bonusChipGO.transform;
            bonusRT.anchorMin = new Vector2(1,1); bonusRT.anchorMax = new Vector2(1,1); bonusRT.pivot = new Vector2(0.5f,0.5f);
            bonusRT.sizeDelta = new Vector2(34,34); bonusRT.anchoredPosition = new Vector2(-3,3);
            var bonusImg = bonusChipGO.GetComponent<Image>();
            bonusImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            bonusImg.color = ThemePalette.AtkColor; bonusImg.raycastTarget = false;
            var bonusLabel = CreateChildText(bonusChipGO.transform, "BonusChipLabel", "+2",
                anchorMin:Vector2.zero, anchorMax:Vector2.one, pivot:new Vector2(0.5f,0.5f),
                position:Vector2.zero, size:Vector2.zero, alignment:TextAlignmentOptions.Center, fontSize:14);
            bonusLabel.fontStyle = FontStyles.Bold; bonusLabel.color = Color.white;
            bonusChipGO.SetActive(false);

            // ---- trump flag (overflow, bottom) ----
            var trumpFlagGO = new GameObject("TrumpFlag", typeof(RectTransform), typeof(Image));
            trumpFlagGO.transform.SetParent(go.transform, false);
            var tfRT = (RectTransform)trumpFlagGO.transform;
            tfRT.anchorMin = new Vector2(0.5f,0); tfRT.anchorMax = new Vector2(0.5f,0); tfRT.pivot = new Vector2(0.5f,0.5f);
            tfRT.sizeDelta = new Vector2(86,22); tfRT.anchoredPosition = new Vector2(0,-2);
            var tfImg = trumpFlagGO.GetComponent<Image>(); tfImg.color = ThemePalette.VenetianRed; tfImg.raycastTarget = false;
            var tfLabel = CreateChildText(trumpFlagGO.transform, "TrumpFlagLabel", "♥ TRUMP",
                anchorMin:Vector2.zero, anchorMax:Vector2.one, pivot:new Vector2(0.5f,0.5f),
                position:Vector2.zero, size:Vector2.zero, alignment:TextAlignmentOptions.Center, fontSize:11);
            tfLabel.fontStyle = FontStyles.Bold; tfLabel.color = Color.white; if (HeadingFont) tfLabel.font = HeadingFont;
            trumpFlagGO.SetActive(false);

            // ---- back ----
            var backRoot = new GameObject("Back", typeof(RectTransform));
            backRoot.transform.SetParent(go.transform, false);
            var backRT = backRoot.GetComponent<RectTransform>(); Fill(backRT); backRT.offsetMin = new Vector2(4,4); backRT.offsetMax = new Vector2(-4,-4);
            var backImage = backRoot.AddComponent<Image>();
            if (cardBackSprite) { backImage.sprite = cardBackSprite; backImage.color = Color.white; backImage.preserveAspect = true; }
            else backImage.color = ThemePalette.CrimsonCard;
            backRoot.SetActive(false);

            // ---- wire CardView ----
            var view = go.AddComponent<CardView>();
            view.Background = background;
            view.Outline = outline;
            view.ArtImage = artImg;
            view.TopScrim = tsImg;
            view.FaceRoot = faceRT;
            view.RankTopLeft = rankTL;
            view.RankBottomRight = null;
            view.CenterPip = center;
            view.EngraveImage = engImg;
            view.DoctrineGem = gemImg;
            view.BackRoot = backRT;
            view.BackImage = backImage;
            view.NameLabel = nameLabel;
            view.DoctrineSubLabel = null;
            view.AbilityBadge = abilityBadge;
            view.AbilityBadgeBg = badgeBgImg;
            view.TriggerIcon = trigImg;
            view.TriggerAttackSprite = trigAttack;
            view.TriggerDefendSprite = trigDefend;
            view.TriggerPassiveSprite = trigPassive;
            view.Drawer = drawerRT;
            view.DrawerScrim = dScrim;
            view.BodyGroup = bodyGroup;
            view.BodyText = bodyText;
            view.BonusChip = bonusChipGO;
            view.BonusChipLabel = bonusLabel;
            view.TrumpFlag = trumpFlagGO;
            view.TrumpFlagLabel = tfLabel;

            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Built {PrefabPath} (Design B frame)");
        }

        // ---------- helpers ----------
        static void Fill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static Image CreateChildImage(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
            return go.GetComponent<Image>();
        }

        static TMP_Text CreateChildText(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size,
            TextAlignmentOptions alignment, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot; rt.sizeDelta = size; rt.anchoredPosition = position;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (DefaultFont) tmp.font = DefaultFont;
            tmp.text = text; tmp.alignment = alignment; tmp.fontSize = fontSize; tmp.color = ThemePalette.BlackSuit;
            tmp.enableWordWrapping = false; tmp.raycastTarget = false;
            return tmp;
        }
    }
}
