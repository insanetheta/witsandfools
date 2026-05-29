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

            // Ability badge at bottom of face
            var abilityBadge = CreateChildText(faceRoot.transform, "AbilityBadge", "",
                anchorMin: new Vector2(0, 0), anchorMax: new Vector2(1, 0),
                pivot: new Vector2(0.5f, 0),
                position: new Vector2(0, 4),
                size: new Vector2(0, 20),
                alignment: TextAlignmentOptions.Center,
                fontSize: 11);
            abilityBadge.fontStyle = FontStyles.Bold;
            if (BodyFont) abilityBadge.font = BodyFont;
            abilityBadge.outlineWidth = 0.2f;
            abilityBadge.outlineColor = new Color32(0, 0, 0, 180);
            abilityBadge.gameObject.SetActive(false);

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
