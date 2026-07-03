using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WitsAndFools
{
    // Visual representation of a single Card — Design B frame: full-bleed character art with a
    // top scrim, an upper-left rank over a subtle engraved cartouche, a doctrine gem, and a bottom
    // "info drawer" that is collapsed (name + ability label) in hand and slides up to reveal the
    // full ability body text for reward / deck / detail / hover-inspect.
    [RequireComponent(typeof(RectTransform))]
    public sealed class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References (set by PrefabBuilder)")]
        public Image Background;
        public Image Outline;          // larger Image behind the card, for the highlight glow
        public Image ArtImage;         // full-bleed character portrait (Resources/CardArt)
        public Image TopScrim;         // dark gradient at the top for rank legibility
        public RectTransform FaceRoot;
        public TMP_Text RankTopLeft;   // "7♥"
        public TMP_Text RankBottomRight;
        public TMP_Text CenterPip;     // legacy fallback (hidden when art present)
        public Image EngraveImage;     // subtle gold cartouche behind the rank
        public Image DoctrineGem;      // small gem top-right, tinted by doctrine
        public RectTransform BackRoot;
        public Image BackImage;
        public TMP_Text NameLabel;
        public TMP_Text DoctrineSubLabel;
        public TMP_Text AbilityBadge;
        public Image AbilityBadgeBg;   // colored strip behind the ability word
        public GameObject BonusChip;   // "+2" rank-bonus chip (top-right)
        public TMP_Text BonusChipLabel;
        public GameObject TrumpFlag;   // "♥ TRUMP" flag shown on trump defenses
        public TMP_Text TrumpFlagLabel;

        [Header("Info drawer (slide-up)")]
        public RectTransform Drawer;   // bottom drawer (stencil Mask); height animates
        public Image DrawerScrim;      // gradient that darkens the art behind the drawer
        public CanvasGroup BodyGroup;  // body-text panel (faded in on expand)
        public TMP_Text BodyText;      // full ability description + flavour
        public float DrawerCollapsedH = 38f;
        public float DrawerOpenH = 108f;
        public float ExpandLift = 1.16f;   // slight scale-up while inspecting

        [Header("Visual settings")]
        public Color FaceColor = default;
        public Color BackColor = default;
        public Color RedSuitColor = default;
        public Color BlackSuitColor = default;

        Card _card;
        bool _faceUp = true;
        bool _hover;
        bool _expanded;
        bool _expandLocked;            // reward/deck/detail keep the drawer open and ignore hover
        Highlight _highlight = Highlight.None;
        CanvasGroup _canvasGroup;
        Coroutine _anim;
        Vector3 _baseScale = Vector3.one;

        public Card Card => _card;
        public bool FaceUp => _faceUp;

        public enum Highlight { None, Playable, Disabled, Threat }

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
            ApplyDrawer(_expanded ? 1f : 0f);
        }

        public void Bind(Card card, bool faceUp)
        {
            _card = card;
            SetBonus(0);
            SetTrumpFlag(false, default);
            SetFaceUp(faceUp);
        }

        public void SetBonus(int bonus)
        {
            if (!BonusChip) return;
            BonusChip.SetActive(bonus > 0);
            if (bonus > 0 && BonusChipLabel) BonusChipLabel.text = $"+{bonus}";
        }

        public void SetTrumpFlag(bool show, Suit trump)
        {
            if (!TrumpFlag) return;
            TrumpFlag.SetActive(show);
            if (show && TrumpFlagLabel) TrumpFlagLabel.text = $"{trump.Glyph()} TRUMP";
        }

        public void SetFaceUp(bool faceUp)
        {
            _faceUp = faceUp;
            if (FaceRoot) FaceRoot.gameObject.SetActive(faceUp);
            if (BackRoot) BackRoot.gameObject.SetActive(!faceUp);
            if (faceUp) RenderFace();
            else RenderBack();
        }

        public void SetHighlight(Highlight h) { _highlight = h; ApplyOutline(); }

        // Touch tap-to-inspect lifts/expands the card; reused as the selected-glow path too.
        public void SetSelected(bool on)
        {
            _hover = on;
            if (!_expandLocked) SetExpanded(on);
            ApplyOutline();
        }

        // Reward / deck / detail call this so the card shows open by default and ignores hover.
        public void SetDetail(bool open)
        {
            _expandLocked = open;
            SetExpanded(open, animate:false);
        }

        public void SetExpanded(bool on, bool animate = true)
        {
            if (_expanded == on) return;
            _expanded = on;
            if (!isActiveAndEnabled || !animate) { ApplyDrawer(on ? 1f : 0f); return; }
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(AnimateDrawer(on ? 1f : 0f));
        }

        IEnumerator AnimateDrawer(float target)
        {
            float start = Drawer ? Mathf.InverseLerp(DrawerCollapsedH, DrawerOpenH, Drawer.sizeDelta.y) : 0f;
            const float dur = 0.26f;
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                ApplyDrawer(Mathf.Lerp(start, target, k));
                yield return null;
            }
            ApplyDrawer(target);
            _anim = null;
        }

        // k: 0 = collapsed, 1 = open. Drives drawer height, body fade, and the inspect lift.
        void ApplyDrawer(float k)
        {
            if (Drawer) Drawer.sizeDelta = new Vector2(Drawer.sizeDelta.x, Mathf.Lerp(DrawerCollapsedH, DrawerOpenH, k));
            if (BodyGroup) { BodyGroup.alpha = Mathf.Clamp01(k * 1.4f - 0.2f); BodyGroup.gameObject.SetActive(k > 0.01f); }
            // Hover-lift only in hand; a locked detail card keeps whatever scale its spawner set.
            if (!_expandLocked) transform.localScale = _baseScale * Mathf.Lerp(1f, ExpandLift, k);
        }

        void RenderFace()
        {
            var color = _card.Suit.IsRed() ? RedSuitColor : BlackSuitColor;
            string label = _card.Rank.Label() + _card.Suit.Glyph();
            if (RankTopLeft) { RankTopLeft.text = label; RankTopLeft.color = Color.white; }
            if (CenterPip) { CenterPip.text = _card.Suit.Glyph(); CenterPip.color = color; }

            CardDefinition def = null;
            bool hasDef = _card.DefinitionId != null && CardCatalog.TryGet(_card.DefinitionId, out def);

            // Full-bleed art (fallback to a dark face + center pip if a card has no art).
            Sprite art = _card.DefinitionId != null ? Resources.Load<Sprite>("CardArt/" + _card.DefinitionId) : null;
            if (ArtImage)
            {
                if (art != null) { ArtImage.sprite = art; ArtImage.color = Color.white; ArtImage.gameObject.SetActive(true); }
                else ArtImage.gameObject.SetActive(false);
            }
            if (Background) Background.color = art != null ? new Color(0.06f, 0.05f, 0.08f, 1f) : FaceColor;
            if (CenterPip) CenterPip.gameObject.SetActive(art == null);

            if (DoctrineGem) DoctrineGem.color = hasDef ? DoctrineColor(def.Doctrine) : ThemePalette.DustyTan;

            if (NameLabel) { NameLabel.text = hasDef ? def.Name : label; }
            if (DoctrineSubLabel) DoctrineSubLabel.text = hasDef ? def.Doctrine.ToString() : "";

            if (AbilityBadge)
            {
                if (_card.HasAbility)
                {
                    AbilityBadge.gameObject.SetActive(true);
                    AbilityBadge.text = _card.Ability.Value.DisplayName();
                    AbilityBadge.color = Color.white;
                    if (AbilityBadgeBg) { AbilityBadgeBg.gameObject.SetActive(true); AbilityBadgeBg.color = AbilityBadgeColor(_card.Ability.Value); }
                }
                else { AbilityBadge.gameObject.SetActive(false); if (AbilityBadgeBg) AbilityBadgeBg.gameObject.SetActive(false); }
            }

            if (BodyText)
            {
                string body = hasDef && !string.IsNullOrEmpty(def.AbilityText) ? def.AbilityText
                            : (_card.HasAbility ? _card.Ability.Value.DisplayName() : "");
                string flav = hasDef && !string.IsNullOrEmpty(def.FlavorText)
                            ? $"\n<size=88%><i><color=#A89B8C>{def.FlavorText}</color></i></size>" : "";
                BodyText.text = body + flav;
            }
            ApplyOutline();
        }

        static Color AbilityBadgeColor(AbilityType ability) => ThemePalette.AbilityBadgeColor(ability);

        static Color DoctrineColor(DoctrineType d) => d switch
        {
            DoctrineType.Schemer   => new Color(0.18f, 0.55f, 0.62f),
            DoctrineType.Brute     => new Color(0.66f, 0.20f, 0.20f),
            DoctrineType.Trickster => new Color(0.54f, 0.31f, 0.63f),
            DoctrineType.Hoarder   => new Color(0.75f, 0.54f, 0.16f),
            _                      => new Color(0.66f, 0.61f, 0.55f),
        };

        void RenderBack()
        {
            // Dark edge behind the back sprite (matches the face's dark border); white here
            // reads as a glitchy white slab around every face-down card.
            if (Background) Background.color = (BackImage && BackImage.sprite) ? new Color(0.06f, 0.05f, 0.08f, 1f) : BackColor;
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
                    case Highlight.Threat:   Outline.color = ThemePalette.VenetianRed; break;
                    default:                 Outline.color = ThemePalette.OutlineNone; break;
                }
            }
            if (_canvasGroup) _canvasGroup.alpha = _highlight == Highlight.Disabled ? ThemePalette.DisabledAlpha : 1f;
        }

        public void OnPointerClick(PointerEventData eventData) => OnClicked?.Invoke(this);

        // Pointer hover (desktop) expands the inspect drawer; touch drives it via SetSelected.
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!InputProfile.Hover) return;
            _hover = true; ApplyOutline();
            if (!_expandLocked && _faceUp) SetExpanded(true);
            OnHoverChanged?.Invoke(_faceUp ? (Card?)_card : null);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!InputProfile.Hover) return;
            _hover = false; ApplyOutline();
            if (!_expandLocked) SetExpanded(false);
            OnHoverChanged?.Invoke(null);
        }
    }
}
