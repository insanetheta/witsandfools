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

        static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("Wits and Fools/Build/Card Prefab")]
        public static void BuildCardPrefab()
        {
            var cardBackSprite = LoadSprite("Assets/Art/Cards/card_back.png");
            var go = new GameObject("CardView", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(CardWidth, CardHeight);

            // Outline (slightly larger background, sits behind the card)
            var outline = CreateChildImage(go.transform, "Outline", new Vector2(CardWidth + OutlineThickness, CardHeight + OutlineThickness));
            outline.color = ThemePalette.OutlineNone;

            // Background (the card face area)
            var background = CreateChildImage(go.transform, "Background", new Vector2(CardWidth, CardHeight));
            background.color = ThemePalette.CardCream;

            // Face
            var faceRoot = new GameObject("Face", typeof(RectTransform));
            faceRoot.transform.SetParent(go.transform, false);
            var faceRT = faceRoot.GetComponent<RectTransform>();
            faceRT.anchorMin = Vector2.zero;
            faceRT.anchorMax = Vector2.one;
            faceRT.offsetMin = Vector2.zero;
            faceRT.offsetMax = Vector2.zero;

            var rankTL = CreateChildText(faceRoot.transform, "RankTopLeft", "A♥",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(0, 1),
                pivot: new Vector2(0, 1),
                position: new Vector2(8, -6),
                size: new Vector2(60, 32),
                alignment: TextAlignmentOptions.TopLeft,
                fontSize: 22);

            // Skip a rotated bottom-right rank label (real cards have one but rotation causes
            // visible overflow when cards are fanned). Top-left rank + center pip is enough.
            TMP_Text rankBR = null;

            var nameLabel = CreateChildText(faceRoot.transform, "NameLabel", "",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(0.5f, 1),
                position: new Vector2(0, -34),
                size: new Vector2(0, 20),
                alignment: TextAlignmentOptions.Center,
                fontSize: 10);
            nameLabel.enableWordWrapping = true;
            nameLabel.fontStyle = FontStyles.Italic;
            nameLabel.color = ThemePalette.BlackSuit;
            if (BodyFont) nameLabel.font = BodyFont;

            var center = CreateChildText(faceRoot.transform, "CenterPip", "♥",
                anchorMin: new Vector2(0.5f, 0.5f), anchorMax: new Vector2(0.5f, 0.5f),
                pivot: new Vector2(0.5f, 0.5f),
                position: Vector2.zero,
                size: new Vector2(80, 80),
                alignment: TextAlignmentOptions.Center,
                fontSize: 56);

            // Ability strip at bottom of face: colored bg, white word, left-aligned so it
            // stays readable when a defense card overlaps the right side in the bout.
            var badgeBgGO = new GameObject("AbilityBadgeBg", typeof(RectTransform), typeof(Image));
            badgeBgGO.transform.SetParent(faceRoot.transform, false);
            var badgeBgRT = badgeBgGO.GetComponent<RectTransform>();
            badgeBgRT.anchorMin = new Vector2(0, 0);
            badgeBgRT.anchorMax = new Vector2(1, 0);
            badgeBgRT.pivot = new Vector2(0.5f, 0);
            badgeBgRT.sizeDelta = new Vector2(0, 22);
            badgeBgRT.anchoredPosition = Vector2.zero;
            var badgeBgImg = badgeBgGO.GetComponent<Image>();
            badgeBgImg.color = ThemePalette.UtilColor;
            badgeBgImg.raycastTarget = false;
            badgeBgGO.SetActive(false);

            var abilityBadge = CreateChildText(badgeBgGO.transform, "AbilityBadge", "",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f),
                position: Vector2.zero,
                size: Vector2.zero,
                alignment: TextAlignmentOptions.MidlineLeft,
                fontSize: 11);
            abilityBadge.fontStyle = FontStyles.Bold;
            if (BodyFont) abilityBadge.font = BodyFont;
            abilityBadge.color = Color.white;
            abilityBadge.enableAutoSizing = true;
            abilityBadge.fontSizeMin = 8;
            abilityBadge.fontSizeMax = 11;
            abilityBadge.margin = new Vector4(7, 0, 4, 0);
            abilityBadge.gameObject.SetActive(false);

            // Rank-bonus chip (top-right, e.g. "+2" from Conquer/Heavy Hand)
            var bonusChipGO = new GameObject("BonusChip", typeof(RectTransform), typeof(Image));
            bonusChipGO.transform.SetParent(go.transform, false);
            var bonusRT = bonusChipGO.GetComponent<RectTransform>();
            bonusRT.anchorMin = new Vector2(1, 1);
            bonusRT.anchorMax = new Vector2(1, 1);
            bonusRT.pivot = new Vector2(0.5f, 0.5f);
            bonusRT.sizeDelta = new Vector2(38, 38);
            bonusRT.anchoredPosition = new Vector2(-4, 4);
            var bonusImg = bonusChipGO.GetComponent<Image>();
            bonusImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            bonusImg.color = ThemePalette.AtkColor;
            bonusImg.raycastTarget = false;
            var bonusLabel = CreateChildText(bonusChipGO.transform, "BonusChipLabel", "+2",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f),
                position: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, fontSize: 15);
            bonusLabel.fontStyle = FontStyles.Bold;
            bonusLabel.color = Color.white;
            bonusLabel.outlineWidth = 0.25f;
            bonusLabel.outlineColor = new Color32(60, 10, 10, 255);
            bonusChipGO.SetActive(false);

            // Trump flag (top-center, shown when this card defends with trump)
            var trumpFlagGO = new GameObject("TrumpFlag", typeof(RectTransform), typeof(Image));
            trumpFlagGO.transform.SetParent(go.transform, false);
            var tfRT = trumpFlagGO.GetComponent<RectTransform>();
            tfRT.anchorMin = new Vector2(0.5f, 1);
            tfRT.anchorMax = new Vector2(0.5f, 1);
            tfRT.pivot = new Vector2(0.5f, 0.5f);
            tfRT.sizeDelta = new Vector2(86, 22);
            tfRT.anchoredPosition = new Vector2(0, 2);
            var tfImg = trumpFlagGO.GetComponent<Image>();
            tfImg.color = ThemePalette.VenetianRed;
            tfImg.raycastTarget = false;
            var tfLabel = CreateChildText(trumpFlagGO.transform, "TrumpFlagLabel", "♥ TRUMP",
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                pivot: new Vector2(0.5f, 0.5f),
                position: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, fontSize: 11);
            tfLabel.fontStyle = FontStyles.Bold;
            tfLabel.color = Color.white;
            trumpFlagGO.SetActive(false);

            // Back
            var backRoot = new GameObject("Back", typeof(RectTransform));
            backRoot.transform.SetParent(go.transform, false);
            var backRT = backRoot.GetComponent<RectTransform>();
            backRT.anchorMin = Vector2.zero;
            backRT.anchorMax = Vector2.one;
            backRT.offsetMin = new Vector2(4, 4);
            backRT.offsetMax = new Vector2(-4, -4);

            var backImage = backRoot.AddComponent<Image>();
            if (cardBackSprite)
            {
                backImage.sprite = cardBackSprite;
                backImage.color = Color.white;
                backImage.preserveAspect = true;
            }
            else
            {
                backImage.color = ThemePalette.CrimsonCard;
                var backLabel = CreateChildText(backRoot.transform, "BackPattern", "W&F",
                    anchorMin: new Vector2(0.5f, 0.5f), anchorMax: new Vector2(0.5f, 0.5f),
                    pivot: new Vector2(0.5f, 0.5f),
                    position: Vector2.zero,
                    size: new Vector2(80, 40),
                    alignment: TextAlignmentOptions.Center,
                    fontSize: 20);
                backLabel.color = ThemePalette.CardBackAccent;
                backLabel.fontStyle = FontStyles.Bold;
            }

            backRoot.SetActive(false);

            // Wire up CardView component
            var view = go.AddComponent<CardView>();
            view.Background = background;
            view.Outline = outline;
            view.FaceRoot = faceRT;
            view.RankTopLeft = rankTL;
            view.RankBottomRight = rankBR;
            view.CenterPip = center;
            view.NameLabel = nameLabel;
            view.BackRoot = backRT;
            view.BackImage = backImage;
            view.AbilityBadge = abilityBadge;
            view.AbilityBadgeBg = badgeBgImg;
            view.BonusChip = bonusChipGO;
            view.BonusChipLabel = bonusLabel;
            view.TrumpFlag = trumpFlagGO;
            view.TrumpFlagLabel = tfLabel;

            // Save as prefab
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Built {PrefabPath}");
        }

        // ---------- helpers ----------

        static Image CreateChildImage(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            return go.GetComponent<Image>();
        }

        static TMP_Text CreateChildText(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size,
            TextAlignmentOptions alignment, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = position;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (DefaultFont) tmp.font = DefaultFont;
            tmp.text = text;
            tmp.alignment = alignment;
            tmp.fontSize = fontSize;
            tmp.color = ThemePalette.BlackSuit;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
