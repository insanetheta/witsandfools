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
        public Image BackImage;
        public TMP_Text AbilityBadge;

        [Header("Visual settings")]
        public Color FaceColor = default;
        public Color BackColor = default;
        public Color RedSuitColor = default;
        public Color BlackSuitColor = default;

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
            if (FaceColor == default) FaceColor = ThemePalette.CardCream;
            if (BackColor == default) BackColor = ThemePalette.CrimsonCard;
            if (RedSuitColor == default) RedSuitColor = ThemePalette.RedSuit;
            if (BlackSuitColor == default) BlackSuitColor = ThemePalette.BlackSuit;
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

        static Color AbilityBadgeColor(AbilityType ability) => ThemePalette.AbilityBadgeColor(ability);

        void RenderBack()
        {
            if (Background)
            {
                if (BackImage && BackImage.sprite)
                    Background.color = Color.white;
                else
                    Background.color = BackColor;
            }
            ApplyOutline();
        }

        void ApplyOutline()
        {
            if (Outline)
            {
                if (_hover && _highlight == Highlight.Playable) Outline.color = ThemePalette.SelectedGlow;
                else switch (_highlight)
                {
                    case Highlight.Playable: Outline.color = ThemePalette.PlayableGlow; break;
                    case Highlight.Disabled: Outline.color = ThemePalette.DisabledOutline; break;
                    default: Outline.color = ThemePalette.OutlineNone; break;
                }
            }
            if (_canvasGroup)
                _canvasGroup.alpha = _highlight == Highlight.Disabled ? ThemePalette.DisabledAlpha : 1f;
        }

        public void OnPointerClick(PointerEventData eventData) => OnClicked?.Invoke(this);
        public void OnPointerEnter(PointerEventData eventData) { _hover = true; ApplyOutline(); OnHoverChanged?.Invoke(_faceUp ? (Card?)_card : null); }
        public void OnPointerExit(PointerEventData eventData) { _hover = false; ApplyOutline(); OnHoverChanged?.Invoke(null); }
    }
}
