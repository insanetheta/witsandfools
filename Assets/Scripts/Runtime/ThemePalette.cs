using UnityEngine;

namespace WitsAndFools
{
    public static class ThemePalette
    {
        // Foundation palette (act-neutral)
        public static readonly Color Midnight     = Hex("#0A0A14");
        public static readonly Color DeepNavy     = Hex("#141C28");
        public static readonly Color DarkSlate    = Hex("#1E2832");
        public static readonly Color WarmSlate    = Hex("#2A3442");
        public static readonly Color BronzeEdge   = Hex("#5A4830");
        public static readonly Color Parchment    = Hex("#F0E6D2");
        public static readonly Color DustyTan     = Hex("#A89B8C");
        public static readonly Color Gold         = Hex("#D4A846");
        public static readonly Color GoldDark     = Hex("#B8922C");
        public static readonly Color VenetianRed  = Hex("#B84040");
        public static readonly Color RoyalBlue    = Hex("#4477AA");
        public static readonly Color Sage         = Hex("#66B866");
        public static readonly Color Amber        = Hex("#CC8833");

        // Match table flat fills (Act 1 defaults — replaced by per-act sprites)
        public static readonly Color TableBg         = new Color(0.10f, 0.18f, 0.10f);
        public static readonly Color TableFelt       = new Color(0.13f, 0.30f, 0.18f);
        public static readonly Color TableFeltInner  = new Color(0.10f, 0.24f, 0.14f);
        public static readonly Color DeckSlotDark    = new Color(0.20f, 0.06f, 0.06f);

        // Card colors
        public static readonly Color CrimsonCard  = Hex("#8C2020");
        public static readonly Color CardCream    = Hex("#F5F0E0");
        public static readonly Color CardBackAccent = Hex("#C8A040");
        public static readonly Color RedSuit      = Hex("#C02020");
        public static readonly Color BlackSuit    = Hex("#0D0D0D");

        // Ability type colors
        public static readonly Color AtkColor     = Hex("#CC4444");
        public static readonly Color DefColor     = Hex("#4488BB");
        public static readonly Color UtilColor    = Hex("#CC9933");
        public static readonly Color PassiveColor = Hex("#55AA55");

        // Rarity borders
        public static readonly Color RarityCommon   = Hex("#AA8855");
        public static readonly Color RarityUncommon = Hex("#AABBCC");
        public static readonly Color RarityRare     = Hex("#DDAA33");

        // Card highlight states
        public static readonly Color PlayableGlow   = Hex("#44AA44");
        public static readonly Color AbilityGlow    = Hex("#6699CC");
        public static readonly Color SelectedGlow   = Hex("#D4A846");
        public static readonly Color DisabledOutline = new Color(0.3f, 0.3f, 0.3f, 1f);
        public static readonly Color OutlineNone     = new Color(0, 0, 0, 0);

        // HUD element colors
        public static readonly Color PrestigeRed  = Hex("#FF6666");
        public static readonly Color AbilityBlue  = Hex("#99CCEE");
        public static readonly Color HudOverlay   = new Color(0, 0, 0, 0.45f);
        public static readonly Color RunHudOverlay = new Color(0, 0, 0, 0.60f);

        // Map node backgrounds
        public static readonly Color NodeMatch    = Hex("#7A3030");
        public static readonly Color NodeElite    = Hex("#8A6020");
        public static readonly Color NodeBoss     = Hex("#4A2040");
        public static readonly Color NodeShop     = Hex("#305830");
        public static readonly Color NodeRumor    = Hex("#3A3060");
        public static readonly Color NodeRest     = Hex("#4A3020");

        // Button states
        public static readonly Color ButtonGoldBg       = Hex("#D4A846");
        public static readonly Color ButtonGoldText     = Hex("#1A1408");
        public static readonly Color ButtonGoldHover    = Hex("#E0B850");
        public static readonly Color ButtonGoldDisabled = Hex("#555040");
        public static readonly Color ButtonDarkBg       = Hex("#2A3442");
        public static readonly Color ButtonDarkHover    = Hex("#3A4452");

        // Panel backgrounds
        public static readonly Color ModalOverlay  = new Color(0.03f, 0.03f, 0.07f, 0.92f);
        public static readonly Color ResultOverlay = new Color(0.04f, 0.04f, 0.06f, 0.95f);

        // Disabled alpha for cards
        public const float DisabledAlpha = 0.45f;

        // Per-act background tints (from design system)
        public static readonly Color[] ActBackgroundTint =
        {
            Hex("#14180A"), // Act 1 - Tavern (warm olive-dark)
            Hex("#181410"), // Act 2 - Merchant (warm dark)
            Hex("#141418"), // Act 3 - Guild Hall (cool dark)
            Hex("#18141C"), // Act 4 - Library (purple-dark)
            Hex("#0A1420"), // Act 5 - Salon (deep blue)
        };

        // Per-act table felt tint overlays
        public static readonly Color[] ActFeltTint =
        {
            new Color(0.13f, 0.30f, 0.18f, 0.7f), // Act 1 - green felt
            new Color(0.20f, 0.28f, 0.16f, 0.7f), // Act 2 - warm green
            new Color(0.12f, 0.22f, 0.18f, 0.7f), // Act 3 - cool green
            new Color(0.15f, 0.14f, 0.22f, 0.7f), // Act 4 - purple tint
            new Color(0.16f, 0.08f, 0.20f, 0.5f), // Act 5 - rich purple velvet
        };

        // Per-act frame accent
        public static readonly Color[] ActFrameColor =
        {
            new Color(0.40f, 0.20f, 0.10f), // Act 1 - rough wood brown
            new Color(0.50f, 0.35f, 0.18f), // Act 2 - polished oak
            new Color(0.35f, 0.30f, 0.25f), // Act 3 - dark iron
            new Color(0.25f, 0.20f, 0.30f), // Act 4 - ebony/pearl
            new Color(0.70f, 0.58f, 0.30f), // Act 5 - gold filigree
        };

        public static Color AbilityBadgeColor(AbilityType ability) => ability switch
        {
            AbilityType.TrumpChanger => UtilColor,
            AbilityType.ExtraDraw => UtilColor,
            AbilityType.Blocker => DefColor,
            AbilityType.DoubleTrouble => AtkColor,
            AbilityType.DoubleDefense => DefColor,
            AbilityType.SeizeInitiative => UtilColor,
            AbilityType.PileOn => AtkColor,
            AbilityType.Feint => AtkColor,
            AbilityType.Deflect => DefColor,
            AbilityType.SlipAway => DefColor,
            AbilityType.Peek => UtilColor,
            AbilityType.Gambit => UtilColor,
            AbilityType.TrumpAffinity => PassiveColor,
            AbilityType.EndgameSpecialist => PassiveColor,
            AbilityType.CardCounter => PassiveColor,
            AbilityType.QuickHands => PassiveColor,
            _ => DustyTan
        };

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
