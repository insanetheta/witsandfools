# Wits and Fools: UI/UX Design System

## Design Principles

1. **The table is real.** Every UI element should feel like a physical object on or near a card table. Buttons are embossed cards, labels are engraved brass plates, menus are leather-bound ledgers.

2. **Clarity over decoration.** During gameplay, readability is sacred. A player should know their options within 0.5 seconds of glancing at their hand. Decoration serves the theme but never obscures information.

3. **Progressive refinement.** The UI itself should feel like it improves as you climb acts. Act 1 HUD feels rough and makeshift. Act 5 HUD feels polished and gilded. Same components, different skins.

4. **Consistent visual language.** Colors, icons, and spatial relationships have fixed meanings across all screens. Red always means danger/attack. Blue always means defense. Gold always means reward/currency.

---

## Typography

### Font Stack

| Role | Current | Recommended | Rationale |
|------|---------|-------------|-----------|
| **Headings** | Liberation Sans | **Cinzel** (Google Fonts, free) | Renaissance-appropriate serif with authority. Used for act titles, screen headers, opponent names. |
| **Body** | Liberation Sans | **Crimson Pro** (Google Fonts, free) | Highly readable serif with period-appropriate character. Used for descriptions, narrative text, ability descriptions. |
| **HUD/Numbers** | Liberation Sans | **JetBrains Mono** or **Fira Code** (free) | Tabular figures, monospace for aligned numbers. Used for deck count, florins, prestige, ability badges. |
| **Flavor/Accent** | None | **IM Fell English** (Google Fonts, free) | Rough-hewn Renaissance typeface for event titles, venue names, dramatic moments. |

### Type Scale

| Level | Size | Weight | Usage |
|-------|------|--------|-------|
| Display | 48-52px | Bold | Run Over title, Act transition titles |
| H1 | 36-42px | Bold | Screen titles ("The Fence", "Victory!") |
| H2 | 24-28px | SemiBold | Subtitles, section headers |
| Body | 20-22px | Regular | Descriptions, narrative text |
| Caption | 16-18px | Regular | Ability descriptions, tooltips, secondary info |
| Badge | 14-16px | Bold | Card ability badges, rarity tags |
| HUD | 22-24px | SemiBold | Persistent status bar values |

### Type Colors

| Context | Color | Hex |
|---------|-------|-----|
| Primary text | Warm white | `#F0E6D2` |
| Secondary text | Muted parchment | `#A89B8C` |
| Gold accent (titles, rewards) | Renaissance gold | `#D4A846` |
| Danger/loss | Muted crimson | `#C04040` |
| Success/gain | Sage green | `#66B866` |
| Ability text | Pale blue | `#99CCEE` |
| Disabled | Dark grey | `#555555` |

---

## Color System

### Foundation Palette

**The palette shifts per act**, warming and enriching as you climb. These are the Act-neutral foundation colors:

| Role | Name | Hex | Usage |
|------|------|-----|-------|
| Background (darkest) | Midnight | `#0A0A14` | Screen backgrounds, overlays |
| Background (dark) | Deep Navy | `#141C28` | Panels, cards, containers |
| Surface | Dark Slate | `#1E2832` | Button backgrounds, input fields |
| Surface Elevated | Warm Slate | `#2A3442` | Hover states, modal backgrounds |
| Border | Bronze Edge | `#5A4830` | Card borders, panel edges, dividers |
| Text Primary | Parchment | `#F0E6D2` | All primary text |
| Text Secondary | Dusty Tan | `#A89B8C` | Descriptions, secondary info |
| Accent Primary | Renaissance Gold | `#D4A846` | Titles, currency, primary actions |
| Accent Secondary | Venetian Red | `#B84040` | Attacks, danger, prestige loss |
| Accent Tertiary | Royal Blue | `#4477AA` | Defense, abilities, information |
| Success | Sage | `#66B866` | Rewards, successful defense, positive outcomes |
| Warning | Amber | `#CC8833` | Low prestige, risky choices |

### Per-Act Color Tinting

Each act applies a subtle hue shift to the foundation:

| Act | Hue Shift | Background Tint | Accent Warmth |
|-----|-----------|-----------------|---------------|
| 1 - Tavern | Warm yellow | `#14180A` (olive-dark) | Amber/orange |
| 2 - Merchant | Warm brown | `#181410` (warm dark) | Copper/brass |
| 3 - Guild Hall | Neutral | `#141418` (cool dark) | Silver/steel |
| 4 - Library | Cool purple | `#18141C` (purple-dark) | Amethyst/gold |
| 5 - Salon | Rich blue | `#0A1420` (deep blue) | Pure gold/silver |

### Card Colors

| Element | Color | Hex |
|---------|-------|-----|
| Card face (default) | Warm cream | `#F5F0E0` |
| Card back | Deep crimson | `#8C2020` |
| Card back accent | Gold thread | `#C8A040` |
| Playable highlight | Bright green border | `#44AA44` |
| Ability card glow | Pale blue border | `#6699CC` |
| Disabled card | Desaturated | 50% saturation + 80% brightness |
| Selected card | Gold border + slight raise | `#D4A846` |

### Ability Type Colors

| Type | Color | Hex | Usage |
|------|-------|-----|-------|
| Attack | Crimson | `#CC4444` | Attack ability badges, borders |
| Defense | Steel Blue | `#4488BB` | Defense ability badges, borders |
| Utility | Amber Gold | `#CC9933` | Utility ability badges, borders |
| Passive | Sage Green | `#55AA55` | Passive ability indicators |

### Rarity Colors

| Rarity | Color | Hex | Border Treatment |
|--------|-------|-----|-----------------|
| Common | Bronze | `#AA8855` | Single thin border |
| Uncommon | Silver | `#AABBCC` | Double border with slight glow |
| Rare | Gold | `#DDAA33` | Triple border with particle shimmer |

---

## Component Library

### Buttons

**Primary Button (Call to Action)**
```
Background: #D4A846 (gold)
Text: #1A1408 (near-black)
Border: 2px #B8922C (darker gold)
Border-radius: 6px
Padding: 12px 32px
Shadow: 0 2px 4px rgba(0,0,0,0.4)
Hover: Background lightens to #E0B850, shadow expands
Active: Background darkens to #C09838, shadow contracts
Disabled: Background #555040, text #888070, no shadow
```
Used for: "Continue", "New Run", "Leave", primary actions

**Secondary Button (Alternative Action)**
```
Background: #2A3442 (dark slate)
Text: #F0E6D2 (parchment)
Border: 1px #5A4830 (bronze)
Border-radius: 6px
Padding: 10px 24px
Shadow: 0 1px 2px rgba(0,0,0,0.3)
Hover: Border lightens to #8A7050
Disabled: Background #1E2228, text #555, border #333
```
Used for: "Skip", "Play normally", secondary actions

**Danger Button**
```
Background: #882020 (dark red)
Text: #F0E6D2
Border: 1px #AA3030
Hover: Background #993030
```
Used for: "Take cards" (eating), risky event choices

**Map Node Button**
```
Background: Varies by node type (see below)
Text: #F0E6D2
Border-radius: 8px
Padding: 16px 24px
Min-height: 70px
```
Node type backgrounds:
- Match: `#7A3030` (muted red)
- Elite: `#8A6020` (dark gold) with skull icon
- Boss: `#4A2040` (dark purple) with crown icon
- Shop: `#305830` (dark green) with purse icon
- Rumor: `#3A3060` (dark purple) with scroll icon  
- Rest: `#4A3020` (warm brown) with fire icon

### Cards

**Standard Card (in hand)**
```
Width: 110px
Height: 160px
Background: #F5F0E0 (cream)
Border: 2px #8A7A60 (tan)
Border-radius: 8px
Shadow: 0 2px 6px rgba(0,0,0,0.4)
```

**Card Back**
```
Background: #8C2020 (crimson)
Border: 2px #6A1818
Pattern: Repeating diamond/scrollwork overlay at 10% opacity
Center emblem: W&F crest at 40% opacity
```

**Card States**
- Default: Standard border
- Playable: 3px `#44AA44` border + subtle green glow (`box-shadow: 0 0 8px #44AA4440`)
- Ability playable: 3px `#6699CC` dashed border + blue glow
- Selected: 3px `#D4A846` border + 8px upward translate
- Disabled: 60% opacity, desaturated
- Hover (playable): 12px upward translate, shadow intensifies

### Panels

**Screen Panel (full-screen backgrounds)**
```
Background: Gradient from act-tinted dark to midnight
Overlay: Subtle noise texture at 3% opacity for grain
Vignette: Radial gradient darkening edges by 20%
```

**Modal Panel (ability choice, popups)**
```
Background: #0A0A14 at 92% opacity
Border: 1px #5A4830
Border-radius: 12px
Shadow: 0 8px 32px rgba(0,0,0,0.6)
Padding: 24px
Backdrop: Full-screen #000 at 50% opacity
```

**Info Card (shop items, ability offerings)**
```
Background: #1E2832
Border: 1px #3A3020
Border-radius: 8px
Padding: 16px 20px
Hover: Border #5A4830, background #2A3442
Shadow: 0 2px 4px rgba(0,0,0,0.3)
```

### HUD Bar

**Match HUD (top bar)**
```
Background: #000000 at 45% opacity
Height: 70px
Blur: 8px backdrop blur
Text: #F0E6D2 at 24px
Layout: Phase (left 40%) | Trump (center 30%) | Deck (right 30%)
```

**Run HUD (bottom bar)**
```
Background: #000000 at 60% opacity
Height: 50px
Text: 22px
Layout: Prestige (left) | Florins (center-left) | Act (center-right) | Abilities (right)
Prestige color: #FF6666
Florins color: #D4A846
Act color: #F0E6D2
Abilities color: #99CCEE
```

---

## Iconography

### Style

All icons should be **line-art style** with 2px stroke weight, rendered in a single color. Think Renaissance engravings simplified to icon scale.

### Standard Icons (24x24 base, scalable)

| Icon | Description | Usage |
|------|-------------|-------|
| Crossed Swords | Two rapiers crossed at center | Match node, attack phase |
| Shield | Heraldic shield shape | Defense phase, Blocker ability |
| Skull + Crown | Skull wearing a small crown | Elite match node |
| Crown | Simple three-point crown | Boss match node |
| Purse | Drawstring coin purse | Shop node, florins |
| Scroll | Rolled parchment with seal | Rumor/event node |
| Flame | Campfire flame | Rest node |
| Heart | Heraldic heart shape | Prestige |
| Eye | Single open eye | Peek, Spy's Monocle |
| Lightning | Simple bolt | Quick Hands, Seize Initiative |
| Card Fan | Three cards fanned | Hand size, card draw |
| Hourglass | Classic hourglass | Endgame Specialist, timers |
| Arrow Bounce | Arrow hitting surface and deflecting | Deflect ability |
| Smoke Puff | Wispy smoke curl | Slip Away ability |
| Double Arrow | Two arrows pointing same direction | Double Trouble, Double Defense |
| Question Card | Card with ? on face | Feint, unknown information |

---

## Spacing System

Base unit: **8px**

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | Tight internal padding, icon-to-text gap |
| sm | 8px | Compact element spacing |
| md | 16px | Standard element spacing, card gap in hand |
| lg | 24px | Section spacing, panel padding |
| xl | 32px | Major section breaks |
| 2xl | 48px | Screen-level spacing (title to content) |
| 3xl | 64px | Top/bottom screen margins |

### Layout Grid

- **Screen safe area:** 80px inset from edges (accounts for varying display sizes)
- **Content max-width:** 960px centered (prevents ultra-wide stretching)
- **Card hand area:** Bottom 25% of screen
- **Opponent area:** Top 20% of screen
- **Play area:** Middle 55% of screen
- **HUD bars:** Fixed top/bottom, overlaying content

---

## Texture & Material

### Surface Treatments

| Surface | Treatment |
|---------|-----------|
| Card table felt | Subtle woven texture, slight color variation (noise), warm directional lighting |
| Card faces | Linen paper texture at 5% opacity over cream base |
| Card backs | Tooled leather texture with pressed pattern |
| Wood surfaces | Visible grain, slight gloss variation |
| Metal elements (HUD, buttons) | Brushed brass texture with subtle highlight |
| Parchment (events, shop) | Aged paper with slight brown edge tinting |
| Glass (Act 5) | Subtle reflection, transparency with color tint |

### Lighting Model

Every screen should feel lit by a specific light source:
- **Act 1:** Single candle on table, warm orange, strong shadows
- **Act 2:** Fireplace + window light, warm but brighter
- **Act 3:** Overhead chandelier, even but dramatic
- **Act 4:** Multiple candles + moonlight through stained glass, purple tinted
- **Act 5:** Grand chandelier, brilliant and clear, golden

Lighting is achieved through:
1. Background gradient (dark edges, lighter center)
2. Vignette overlay (25% edge darkening)
3. Color temperature of text and UI elements
4. Highlight placement on card surfaces

---

## Animation Specifications

### Timing

| Type | Duration | Easing |
|------|----------|--------|
| Card play (hand to table) | 200ms | ease-out-cubic |
| Card draw (deck to hand) | 180ms | ease-out-cubic |
| Card hover (raise) | 120ms | ease-out-quad |
| Screen transition | 400ms | ease-in-out-cubic |
| Modal appear | 250ms | ease-out-back (slight overshoot) |
| Modal dismiss | 200ms | ease-in-quad |
| Button hover | 100ms | ease-out-quad |
| Bout discard sweep | 300ms | ease-in-cubic |
| Florins counter | 600ms | ease-out-quad (counting up) |
| Prestige heart break | 400ms | ease-out-quad + shake |

### Feedback Animations

| Event | Animation |
|-------|-----------|
| Playable card hover | Card rises 12px, shadow deepens |
| Card selected | Gold border appears, 8px rise |
| Successful defense | Brief green flash on table center, cards sweep to discard |
| Failed defense (eating) | Brief red flash on table edges, cards drag to hand |
| Ability activation | Card glows ability-type color, brief particle burst upward |
| Trump change | All four suit symbols cycle rapidly, landing on new trump with a flash |
| Prestige lost | Heart icon cracks and fades, screen edge flashes red 200ms |
| Florins gained | Small coin icons cascade from top, number counts up |
| Victory | Gold particles rise from table, text scales in with ease-out-back |
| Defeat | Cards scatter on table, text fades in slowly |

---

## Responsive Considerations

### Target Resolutions

| Platform | Resolution | Scale Factor |
|----------|-----------|--------------|
| Desktop (primary) | 1920x1080 | 1.0x |
| Desktop (high-DPI) | 2560x1440 | 1.0x (larger assets) |
| Ultrawide | 2560x1080 | Content centered, extra table visible |
| Steam Deck | 1280x800 | 0.75x, larger touch targets |
| Mobile (future) | 1080x1920 (portrait) | Complete layout rethink |

### Scaling Rules

- Card size: Minimum 90px wide for readability of suit/rank
- Font size: Minimum 16px for any readable text
- Touch targets: Minimum 44px for any interactive element
- Hand fan: Overlaps increase when more than 6 cards to fit width
- Bout area: Compresses horizontally when 4+ attack/defense pairs

---

## Accessibility

### Color Blind Support

- Never rely on red/green distinction alone
- Card suits already have distinct shapes (good)
- Ability types should use shape coding in addition to color:
  - Attack: Circle icon frame
  - Defense: Shield icon frame
  - Utility: Diamond icon frame
  - Passive: Square icon frame
- Playable/disabled states use border style (solid/dashed) in addition to color

### Readability

- All body text minimum 20px
- High contrast ratio: text on dark backgrounds > 7:1 contrast
- Important numbers (deck count, florins, prestige) > 10:1 contrast
- Card rank/suit readable at 60% zoom level

---

## Implementation Priority

### Phase 1: Foundation (Highest Impact, Lowest Effort)
1. Replace Liberation Sans with Cinzel (headings) + Crimson Pro (body)
2. Apply the color system (replace hardcoded colors in SceneBuilder)
3. Add vignette overlay to all screen panels
4. Add subtle noise texture to table felt
5. Implement button hover/press states with the spec above

### Phase 2: Cards & Table (Core Experience)
6. Design proper card faces with suit illustrations
7. Design card back pattern
8. Implement card hover animation (rise + shadow)
9. Add ability activation glow effect
10. Per-act table surface color variation

### Phase 3: Screens & Atmosphere
11. Map screen redesign (node graph with icons instead of text list)
12. Opponent portraits (can start with silhouettes)
13. Per-act background illustrations (even simple atmospheric gradients help)
14. Shop item icons
15. Event screen illustrations

### Phase 4: Polish & Juice
16. Screen transitions
17. Particle effects (florins, victory, ability activation)
18. Sound design integration
19. Opponent voice lines / flavor text
20. Per-act HUD skin variations
