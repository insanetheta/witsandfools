# Wits and Fools - Standard Playing Cards Art Generation Plan

## Overview

This plan creates a modular card art system for standard playing cards (2-A in all four suits) following the Renaissance theme established in the GDD. The system maximizes efficiency by separating reusable components.

## Art Style Requirements

-   **Theme**: Renaissance era, political intrigue, tavern/court setting
-   **Inspiration**: Queen's Blood (FF7 Rebirth), Tetra Master (FF9)
-   **Aesthetic**: Ornate, detailed fantasy illustrations with rich textures
-   **Color Palette**: Warm, ambient lighting suitable for medieval tavern settings
-   **Resolution**: 512x768 pixels (2:3 aspect ratio)

## Modular Asset System

### 1. Card Background Templates (4 Assets)

**Purpose**: Reusable background/border for each suit with suit-specific theming

**Assets Needed**:

-   `Card_Background_Hearts.png` - Hearts suit themed border
-   `Card_Background_Diamonds.png` - Diamonds suit themed border
-   `Card_Background_Clubs.png` - Clubs suit themed border
-   `Card_Background_Spades.png` - Spades suit themed border

**Design Specifications**:

-   Size: 512x768 pixels
-   Contains ornate Renaissance border with suit-specific decorative elements
-   Center area (50% of card) left transparent for center art overlay
-   Corner areas include small suit symbols and space for rank text
-   Consistent ornate frame style across all suits with color variations

**Suit-Specific Theming**:

-   **Hearts**: Red/gold coloring, romantic/noble imagery, love/passion themes
-   **Diamonds**: Blue/silver coloring, wealth/merchant imagery, commerce/prosperity themes
-   **Clubs**: Green/brown coloring, nature/forest imagery, growth/war themes
-   **Spades**: Black/purple coloring, military/death imagery, power/nobility themes

### 2. Center Art - Face Cards (16 Assets)

**Purpose**: Character portraits for Jack, Queen, King, Ace

**Size**: Designed to fill 50% of card center area

**Assets Needed**:

**Hearts Suit (Royal/Noble Theme)**:

-   `Center_Jack_Hearts.png` - Young noble courtier, romantic figure
-   `Center_Queen_Hearts.png` - Elegant noble lady, regal bearing
-   `Center_King_Hearts.png` - Benevolent monarch, crown and scepter
-   `Center_Ace_Hearts.png` - Ornate heart symbol with magical aura

**Diamonds Suit (Merchant/Wealth Theme)**:

-   `Center_Jack_Diamonds.png` - Wealthy young merchant, fine clothes
-   `Center_Queen_Diamonds.png` - Rich merchant lady, jeweled attire
-   `Center_King_Diamonds.png` - Merchant prince, gold and gems
-   `Center_Ace_Diamonds.png` - Brilliant diamond with magical sparkles

**Clubs Suit (Nature/War Theme)**:

-   `Center_Jack_Clubs.png` - Young knight/warrior, club weapon
-   `Center_Queen_Clubs.png` - Warrior queen, natural crown
-   `Center_King_Clubs.png` - Forest king, nature and strength
-   `Center_Ace_Clubs.png` - Ornate club symbol with nature magic

**Spades Suit (Military/Power Theme)**:

-   `Center_Jack_Spades.png` - Dark knight, armor and spade weapon
-   `Center_Queen_Spades.png` - Mysterious noble lady, dark elegance
-   `Center_King_Spades.png` - Powerful king, dark crown and authority
-   `Center_Ace_Spades.png` - Ornate spade symbol with shadow magic

### 3. Center Art - Number Cards (36 Assets)

**Purpose**: Thematic imagery for number cards 2-10

**Size**: Designed to fill 25% of card center area

**Assets Needed** (9 per suit × 4 suits = 36 total):

**Design Approach**: Each number has thematic meaning tied to its suit:

**Hearts (Love/Passion/Nobility)**:

-   `Center_2_Hearts.png` - Two intertwined roses
-   `Center_3_Hearts.png` - Three dancing couples
-   `Center_4_Hearts.png` - Four-poster noble bed
-   `Center_5_Hearts.png` - Five musicians playing
-   `Center_6_Hearts.png` - Six wine goblets
-   `Center_7_Hearts.png` - Seven love letters
-   `Center_8_Hearts.png` - Eight wedding rings
-   `Center_9_Hearts.png` - Nine romantic candles
-   `Center_10_Hearts.png` - Ten heart-shaped gems

**Diamonds (Wealth/Commerce/Prosperity)**:

-   `Center_2_Diamonds.png` - Two scales of justice/trade
-   `Center_3_Diamonds.png` - Three bags of gold
-   `Center_4_Diamonds.png` - Four precious gemstones
-   `Center_5_Diamonds.png` - Five merchant ships
-   `Center_6_Diamonds.png` - Six gold coins
-   `Center_7_Diamonds.png` - Seven treasure chests
-   `Center_8_Diamonds.png` - Eight jeweled chalices
-   `Center_9_Diamonds.png` - Nine diamond necklaces
-   `Center_10_Diamonds.png` - Ten golden crowns

**Clubs (Nature/Growth/War)**:

-   `Center_2_Clubs.png` - Two crossed war hammers
-   `Center_3_Clubs.png` - Three growing saplings
-   `Center_4_Clubs.png` - Four seasons symbols
-   `Center_5_Clubs.png` - Five war banners
-   `Center_6_Clubs.png` - Six acorns
-   `Center_7_Clubs.png` - Seven forest creatures
-   `Center_8_Clubs.png` - Eight battle shields
-   `Center_9_Clubs.png` - Nine tree branches
-   `Center_10_Clubs.png` - Ten warrior weapons

**Spades (Power/Death/Mystery)**:

-   `Center_2_Spades.png` - Two crossed swords
-   `Center_3_Spades.png` - Three dark towers
-   `Center_4_Spades.png` - Four skull chalices
-   `Center_5_Spades.png` - Five black roses
-   `Center_6_Spades.png` - Six dark crystals
-   `Center_7_Spades.png` - Seven shadow figures
-   `Center_8_Spades.png` - Eight dark spears
-   `Center_9_Spades.png` - Nine midnight stars
-   `Center_10_Spades.png` - Ten dark thrones

### 4. Corner Rank & Suit Symbols (56 Assets)

**Purpose**: Small rank numbers/letters and suit symbols for card corners

**Size**: No more than 10% of card size (approximately 51x77 pixels)

**Assets Needed**:

**Rank Symbols (13 assets)**:

-   `Rank_2.png` - Stylized number "2" in Renaissance font
-   `Rank_3.png` - Stylized number "3" in Renaissance font
-   `Rank_4.png` - Stylized number "4" in Renaissance font
-   `Rank_5.png` - Stylized number "5" in Renaissance font
-   `Rank_6.png` - Stylized number "6" in Renaissance font
-   `Rank_7.png` - Stylized number "7" in Renaissance font
-   `Rank_8.png` - Stylized number "8" in Renaissance font
-   `Rank_9.png` - Stylized number "9" in Renaissance font
-   `Rank_10.png` - Stylized number "10" in Renaissance font
-   `Rank_J.png` - Stylized letter "J" in Renaissance font
-   `Rank_Q.png` - Stylized letter "Q" in Renaissance font
-   `Rank_K.png` - Stylized letter "K" in Renaissance font
-   `Rank_A.png` - Stylized letter "A" in Renaissance font

**Suit Symbols (4 assets)**:

-   `Suit_Hearts.png` - Small heart symbol with Renaissance styling
-   `Suit_Diamonds.png` - Small diamond symbol with Renaissance styling
-   `Suit_Clubs.png` - Small club symbol with Renaissance styling
-   `Suit_Spades.png` - Small spade symbol with Renaissance styling

**Design Specifications**:

-   Small size: Maximum 10% of card area (51x77 pixels or smaller)
-   Clean, readable Renaissance-style typography for ranks
-   Ornate but recognizable suit symbols
-   High contrast for readability
-   Transparent backgrounds
-   Suitable for both upper-left and lower-right corners (lower-right will be rotated 180°)

## Generation Prompts Template

### Card Backgrounds

```
"Renaissance era playing card border for [SUIT], ornate medieval frame with [SUIT_THEME] decorative elements, [SUIT_COLORS], detailed fantasy border design, card game artwork style like Queen's Blood, rich textures, political intrigue theme, tavern setting, transparent center area for artwork overlay"
```

### Face Cards

```
"Renaissance era [RANK] of [SUIT], [CHARACTER_DESCRIPTION], [SUIT_THEME] styling, portrait artwork for card center, detailed character design, card game artwork style like Queen's Blood, rich textures, political intrigue theme, medieval fantasy, isolated on transparent background"
```

### Number Cards

```
"Renaissance era [NUMBER] of [SUIT], [THEMATIC_OBJECTS], [SUIT_THEME] styling, small decorative artwork for card center, detailed fantasy illustration, card game artwork style like Queen's Blood, rich textures, political intrigue theme, medieval tavern setting, isolated on transparent background"
```

### Rank Symbols

```
"Renaissance era playing card rank symbol '[RANK]', ornate medieval typography, stylized [NUMBER/LETTER], clean readable design, card game artwork style like Queen's Blood, high contrast, political intrigue theme, isolated on transparent background, small corner symbol"
```

### Suit Symbols

```
"Renaissance era playing card suit symbol '[SUIT]', ornate medieval [SUIT_SHAPE], detailed fantasy design, card game artwork style like Queen's Blood, high contrast, political intrigue theme, isolated on transparent background, small corner symbol"
```

## Unity Integration System

### Prefab Structure

```
CardPrefab
├── Background (Image - Card_Background_[Suit].png)
├── CenterArt (Image - Center_[Rank]_[Suit].png)
├── RankSymbol_TopLeft (Image - Rank_[Rank].png)
├── RankSymbol_BottomRight (Image - Rank_[Rank].png, rotated 180°)
├── SuitSymbol_TopLeft (Image - Suit_[Suit].png)
└── SuitSymbol_BottomRight (Image - Suit_[Suit].png, rotated 180°)
```

### Asset Organization

```
Assets/Art/Cards/
├── Backgrounds/
│   ├── Card_Background_Hearts.png
│   ├── Card_Background_Diamonds.png
│   ├── Card_Background_Clubs.png
│   └── Card_Background_Spades.png
├── CenterArt/
│   ├── FaceCards/
│   │   ├── Center_Jack_Hearts.png
│   │   ├── Center_Queen_Hearts.png
│   │   └── [...all 16 face cards]
│   └── NumberCards/
│       ├── Center_2_Hearts.png
│       ├── Center_3_Hearts.png
│       └── [...all 36 number cards]
├── RankSymbols/
│   ├── Rank_2.png
│   ├── Rank_3.png
│   ├── [...all 13 rank symbols]
│   └── Rank_A.png
└── SuitSymbols/
    ├── Suit_Hearts.png
    ├── Suit_Diamonds.png
    ├── Suit_Clubs.png
    └── Suit_Spades.png
```

## Efficiency Benefits

1. **Memory Usage**: 56 total assets instead of 52 unique full cards
2. **Disk Space**: Reusable backgrounds save significant space
3. **Maintainability**: Easy to swap out individual elements
4. **Flexibility**: Can create card variants by mixing/matching components
5. **Scalability**: Easy to add new card types or special variants

## Generation Schedule

### Phase 1: Card Backgrounds (4 assets)

-   Generate one background per suit
-   Test Unity integration with temporary center art

### Phase 2: Face Cards Center Art (16 assets)

-   Generate all Jack, Queen, King, Ace center artwork
-   Integrate and test in Unity

### Phase 3: Number Cards Center Art (36 assets)

-   Generate 2-10 center artwork for all suits
-   Complete integration and testing

### Phase 4: Corner Rank Symbols (13 assets)

-   Generate all rank symbols (2, 3, 4, 5, 6, 7, 8, 9, 10, J, Q, K, A)
-   Test corner positioning and readability

### Phase 5: Corner Suit Symbols (4 assets)

-   Generate small suit symbols for each suit
-   Final integration and polish

## Technical Specifications

-   **AI Model**: Stable Diffusion XL (sd_xl_base_1.0.safetensors)
-   **Resolution**: 512x768 pixels for backgrounds, appropriate sizing for center art
-   **Format**: PNG with alpha transparency
-   **Color Space**: sRGB
-   **Transparency**: All center art and symbols must have transparent backgrounds
-   **Naming Convention**: Descriptive names as specified above
-   **Unity Settings**: Sprite (2D and UI), appropriate compression

## Quality Assurance

1. All artwork maintains consistent Renaissance aesthetic
2. Color schemes are appropriate for each suit theme
3. Center art sizing is correct (50% for face, 25% for numbers)
4. Transparent backgrounds are clean with no artifacts
5. All assets integrate properly in Unity prefab system

---

**Total Assets**: 73 individual art pieces

-   4 Card Backgrounds
-   16 Face Card Center Art
-   36 Number Card Center Art
-   13 Rank Symbols
-   4 Suit Symbols

This modular approach provides maximum efficiency while maintaining the high-quality Renaissance aesthetic required for Wits and Fools.
