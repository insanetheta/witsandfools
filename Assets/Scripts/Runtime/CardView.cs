using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WitsAndFools
{
    // Visual representation of a single Card.
    // Knows how to render its face (suit/rank), or show its back, and signals click events.
    [RequireComponent(typeof(RectTransform))]
    public sealed class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References (set by CardPrefabBuilder)")]
        public Image Background;
        public Image Outline;          // a slightly larger Image behind Background, for highlight color
        public RectTransform FaceRoot;
        public TMP_Text RankTopLeft;
        public TMP_Text RankBottomRight;
        public TMP_Text CenterPip;
        public RectTransform BackRoot;
        public TMP_Text AbilityBadge;

        [Header("Visual settings")]
        public Color FaceColor = new Color(0.97f, 0.94f, 0.86f);
        public Color BackColor = new Color(0.30f, 0.10f, 0.10f);
        public Color RedSuitColor = new Color(0.75f, 0.10f, 0.10f);
        public Color BlackSuitColor = new Color(0.05f, 0.05f, 0.05f);
        public Color OutlineDefault = new Color(0, 0, 0, 0);
        public Color OutlinePlayable = new Color(0.20f, 0.85f, 0.30f, 1f);
        public Color OutlineHover = new Color(1f, 1f, 1f, 1f);
        public Color OutlineDisabled = new Color(0.3f, 0.3f, 0.3f, 1f);
        public float DisabledAlpha = 0.45f;

        Card _card;
        bool _faceUp = true;
        bool _hover;
        Highlight _highlight = Highlight.None;
        CanvasGroup _canvasGroup;

        public Card Card => _card;
        public bool FaceUp => _faceUp;

        public enum Highlight { None, Playable, Disabled }

        // Click delegate so HUD/Hand wiring can route clicks through one funnel.
        public System.Action<CardView> OnClicked;
        public static System.Action<Card?> OnHoverChanged;

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (!_canvasGroup) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Bind(Card card, bool faceUp)
        {
            _card = card;
            SetFaceUp(faceUp);
        }

        public void SetFaceUp(bool faceUp)
        {
            _faceUp = faceUp;
            if (FaceRoot) FaceRoot.gameObject.SetActive(faceUp);
            if (BackRoot) BackRoot.gameObject.SetActive(!faceUp);
            if (faceUp) RenderFace();
            else RenderBack();
        }

        public void SetHighlight(Highlight h)
        {
            _highlight = h;
            ApplyOutline();
        }

        void RenderFace()
        {
            if (Background) Background.color = FaceColor;
            var color = _card.Suit.IsRed() ? RedSuitColor : BlackSuitColor;
            string label = _card.Rank.Label() + _card.Suit.Glyph();
            if (RankTopLeft) { RankTopLeft.text = label; RankTopLeft.color = color; }
            if (RankBottomRight) { RankBottomRight.text = label; RankBottomRight.color = color; }
            if (CenterPip) { CenterPip.text = _card.Suit.Glyph(); CenterPip.color = color; }
            if (AbilityBadge)
            {
                if (_card.HasAbility)
                {
                    AbilityBadge.gameObject.SetActive(true);
                    AbilityBadge.text = _card.Ability.Value.ShortName();
                    AbilityBadge.color = AbilityBadgeColor(_card.Ability.Value);
                }
                else AbilityBadge.gameObject.SetActive(false);
            }
            ApplyOutline();
        }

        static Color AbilityBadgeColor(AbilityType ability) => ability switch
        {
            AbilityType.TrumpChanger => new Color(0.55f, 0.20f, 0.70f),
            AbilityType.ExtraDraw => new Color(0.15f, 0.40f, 0.75f),
            AbilityType.Blocker => new Color(0.85f, 0.50f, 0.10f),
            AbilityType.DoubleTrouble => new Color(0.80f, 0.15f, 0.15f),
            AbilityType.DoubleDefense => new Color(0.15f, 0.60f, 0.25f),
            AbilityType.SeizeInitiative => new Color(0.75f, 0.65f, 0.10f),
            _ => Color.gray
        };

        void RenderBack()
        {
            if (Background) Background.color = BackColor;
            ApplyOutline();
        }

        void ApplyOutline()
        {
            if (Outline)
            {
                if (_hover && _highlight == Highlight.Playable) Outline.color = OutlineHover;
                else switch (_highlight)
                {
                    case Highlight.Playable: Outline.color = OutlinePlayable; break;
                    case Highlight.Disabled: Outline.color = OutlineDisabled; break;
                    default: Outline.color = OutlineDefault; break;
                }
            }
            if (_canvasGroup)
                _canvasGroup.alpha = _highlight == Highlight.Disabled ? DisabledAlpha : 1f;
        }

        public void OnPointerClick(PointerEventData eventData) => OnClicked?.Invoke(this);
        public void OnPointerEnter(PointerEventData eventData) { _hover = true; ApplyOutline(); OnHoverChanged?.Invoke(_faceUp ? (Card?)_card : null); }
        public void OnPointerExit(PointerEventData eventData) { _hover = false; ApplyOutline(); OnHoverChanged?.Invoke(null); }
    }
}
