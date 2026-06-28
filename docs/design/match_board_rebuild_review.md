# Match Board Rebuild — Unified Design vs Current Build

**Purpose:** Before rebuilding the match board, compare the approved `unified_board.html`
against the shipped Unity board, and decide — element by element — what to **Adopt**,
**Adapt**, **Keep**, or **Drop**, judged against our direction: **less busy, responsive,
readable, good UX**. This is *not* a blind port of the mockup; several mockup choices are
flagged for adaptation.

## Method / evidence
- Rendered `unified_board.html` at all three tiers via headless browser:
  - `unified_spacious.png` (1280×760), `unified_comfortable.png` (960×600), `unified_compact.png` (820×420)
- Current build, latest full-run audit set: `Screenshots/review_set/screen_MatchInProgress.png`,
  `responsive_1_compact.png` / `_2_comfortable.png` / `_3_spacious.png`.
- Current construction code: `Assets/Scripts/Editor/SceneBuilder.cs` (lines ~60–490) and
  `Assets/Scripts/Runtime/ResponsiveLayout.cs`.

---

## Headline finding

**The unified design IS our less-busy direction, and it is internally consistent with the
responsive system we already shipped.** Its tier breakpoints (height-first: ≥521 comfortable,
≥700 & ≥1100 spacious) match `ResponsiveLayout` almost exactly, and its chip→panel growth,
collapsed-log→docked-rail, and subtitle/race-label reveals mirror our `SpaciousOnly` toggles.

**The single biggest win is the background.** The current board's busy-ness comes almost
entirely from the `table_tavern.png` ornate carved-wood venue sprite + per-act felt tint +
decorative frame. The unified design replaces all of that with a **clean felt radial gradient
+ a soft candlelight glow**. Adopting that one change removes the dominant source of clutter
*and* fixes most of the text-contrast problems QA/UX flagged (cream/gold text was fighting the
wood grain, not the felt).

**The one thing the mockup gets "wrong" for us:** it draws cards as flat schematic pips
(rank + italic name + colored ability bar). Our real game renders **framed card art**
(`CardView`, fed by the card-art epic). The rebuild must adopt the unified **layout**
(slots, fan, panels, felt) while **keeping our richer card rendering** — do not regress cards
to schematic pips.

---

## Element-by-element

Verdict legend: **ADOPT** (take from mockup), **ADAPT** (take, but change for our direction),
**KEEP** (current is fine / better), **DROP** (remove from current).

| # | Element | Unified design | Current build | Verdict | Notes (less-busy / readable / UX) |
|---|---------|----------------|---------------|---------|-----------------------------------|
| 1 | **Background** | Felt radial gradient + candlelight `felt-glow`; no frame | `table_tavern.png` carved-wood sprite + felt tint + 3px frame edges | **ADOPT (felt) + ADAPT** | The big win. Replace wood sprite with felt gradient. *Adapt:* keep a **subtle** vignette + candlelight glow so we don't lose all Venetian mood — clean ≠ sterile. Drop the per-act wood venue *on the board* (venues already shine on map/event/rest screens). |
| 2 | **Bout zone** | Two `.slot` wells: rounded, inset shadow, `ATK`/`DEF` tab, attacker seated, defender overlapped ~5° | Flat grey banner; played card floats above it (our recessed "bout zone" is one big rect, not per-card slots) | **ADOPT** | This is bead `9n3c` (P0). Build real per-card slots with role tabs. Remove the grey status-text-as-backdrop. Biggest readability + "where's the action" UX win after the felt. |
| 3 | **Player hand** | Even symmetric arc fan (rotations −7°…+7°), dimmed unplayable + gold-glow playable, hover-lift | Scattered uneven overlap pile | **ADOPT** | Bead `9dr0`. Symmetric fan + single clear playable highlight. Our `HandLayout` likely needs fixed per-index rotation/offset instead of whatever produces the scatter. |
| 4 | **Identity (chip→panel)** | One element scaled by `--chipscale` (1 → 1.18 → 1.5); subtitle + "RACE TO ZERO" label appear only spacious | Fixed `BuildIdentityPanel`; we hide subtitle/race-label at non-spacious via `SpaciousOnly` | **ADAPT / KEEP** | Behaviour already matches. *Caution:* the mockup's `--chipscale` would **double-scale** under Unity's CanvasScaler (which already scales by tier via `referenceResolution.y`). KEEP our toggle approach; do **not** add a second per-tier scale multiplier. |
| 5 | **Event log** | Collapsed 📜 round button → docked `TABLE TALK` rail (spacious only) | `EventLogPanel` docked / `EventLogButton` swap — already wired in `ResponsiveLayout` | **KEEP** | Already implemented and matches. Bead `jndf` is really just "make the spacious rail look like the mockup's panel," not new behaviour. |
| 6 | **Trump + rule** | Right-edge mini card, "TRUMP ♥", rule text spelled out spacious only | Right-edge; we just added a dark backing plate; rule in `SpaciousOnly` | **KEEP** | Matches after round-1 fix. Minor: align plate styling to mockup's. |
| 7 | **Removed pile** | Small pill "✦ REMOVED 4" top-right | `RemovedPile` deck (dimmed) top-right | **ADAPT** | Current shows a full dimmed deck; mockup's compact pill is lighter / less busy. Consider the pill. |
| 8 | **Action button** | Green gradient, **dark ink** text, sublabel "bout 3/12 · tie→prestige" | `End Bout` gold button + separate bout-chip + phase line | **ADAPT** | Consolidate phase line / bout chip into the button's sublabel like the mockup → fewer floating labels = less busy. Our `AddButton` already uses dark ink. |
| 9 | **Phase ribbon** | Sage-bordered pill top-center, "YOUR ATTACK · all parried" | `BoutStatePanel` plated, top-center | **KEEP** | Matches. (Bead `olik`/U2 ghost-duplicate is a *bug* to fix, not a design change.) |
| 10 | **Cards themselves** | Schematic: pip + italic name + colored ability bar | **Framed art** via `CardView` (art epic) | **KEEP CURRENT** | Do **not** regress to pips. Keep art; borrow only the **ability-bar colour coding** (atk/def/util/pas) and bonus-chip / trump-flag affordances, which we already have. |
| 11 | **Decks (dual)** | `foe-deck` top-left, `you-deck` near action | `OpponentDeckPile` / `PlayerDeckPile` per-side | **KEEP** | Already dual-deck per side. Match label placement to mockup so labels never tuck under buttons. |

---

## Critical flags — where the mockup must be ADAPTED for our direction

1. **Card-text legibility at Compact (READABILITY RISK).** The mockup's card name is 9px italic
   brown-on-cream; the ability bar text ~9.5px. At Compact that is near the floor of legibility,
   and on a real display with our art behind it could be worse. **Recommendation:** at Compact,
   drop the card *name* and keep **rank + suit pip + ability bar** only — fewer glyphs, larger,
   and *less busy*. Names return at Comfortable+. (Our CardView already conditionally shows the
   name label; gate it on tier.)

2. **Empty felt / dead space on wide screens (the recurring UX-agent flag).** Both the mockup
   *and* current strand a large empty felt region center-right at 16:9 because the table elements
   anchor to edges. The mockup only clamps at AR ≥ 2.5; we already clamp tighter at **1.95**
   (`ResponsiveLayout.UltrawideAspect`) — **keep ours**. For 16:9 specifically, recommend pulling
   the trump, removed pill, and player-deck *inward* (anchored to the play-field clamp, not the
   raw screen edge) so they frame the felt instead of hugging far corners. Treat remaining center
   space as intentional breathing room, not a defect.

3. **Keep Venetian mood while going clean (less-busy ≠ sterile).** Pure flat felt risks losing
   the game's candlelit-palazzo identity that the meta screens nail. **Recommendation:** felt
   gradient + a *subtle* vignette + the `felt-glow` warm highlight + thin gold hairline edge —
   richness from light, not from a busy wood texture.

4. **Do not double-scale (TECH PITFALL).** The mockup expresses tier sizing via CSS `--chipscale`
   and card `--cw/--ch` vars. In Unity, the CanvasScaler *already* rescales the whole canvas per
   tier (we set `referenceResolution.y` to 760/920/1080). Re-applying a per-tier multiplier on top
   would compound. Implement tier differences as **layout/visibility toggles** (what shows, where it
   anchors), letting CanvasScaler own the size — which is what we already do.

5. **Ability-chip row on the run-end screen** (carried from review) and **missing common-card art**
   (bead `jyrw` / art epic `2x84`) are adjacent but out of match-board scope; don't fold them in.

---

## Recommended rebuild sequence (maps to existing beads under epic `witsandfools-l46r`)

1. **Felt swap** (new, highest impact, lowest risk): replace `table_tavern.png` + tint + frame on
   the board with a felt radial gradient + subtle vignette + candle glow. *Fixes most contrast
   findings as a side effect.* — *file as new child*
2. **Bout slots** `9n3c` (P0): per-card ATK/DEF wells with role tabs; remove grey banner backdrop.
3. **Hand fan** `9dr0`: symmetric arc + single gold playable highlight; name hidden at Compact (flag #1).
4. **Edge-element inset on 16:9** (flag #2): anchor trump / removed / player-deck to the clamped
   play field. — *file as new child*
5. **Action consolidation** `8` above: fold phase line + bout chip into the button sublabel.
6. **Ghost-ribbon bugfix** `olik` (P0): ensure one opaque phase-ribbon instance (this is a bug, do early).
7. Polish: spacious log-rail / trump-plate styling to match mockup (`jndf`, `qh49` follow-ups).

**Explicitly NOT doing:** regressing cards to schematic pips (#10); adding a second per-tier scale
multiplier (#4); per-act wood venue on the board (#1).

---

## Questions for UX review
- Q1: Agree the felt-swap (drop wood venue on the board) is the right call, with a subtle
  vignette/candle-glow to retain mood — or is the per-act venue identity worth keeping on the board?
- Q2: Agree with dropping the card **name** at Compact (rank+suit+ability only) for legibility, or
  keep names always and accept smaller text?
- Q3: For 16:9 dead-space: inset edge elements to frame the felt (recommended), accept the empty
  felt as breathing room, or something else?
- Q4: Any unified element I marked KEEP/ADAPT that you'd actually rank as a must-ADOPT-as-drawn?

---

## UX review resolutions (independent UX agent — verdict: SOUND TO PROCEED)

The independent UX reviewer agreed with the direction, sequence, and nearly every verdict, and
credited the plan for *improving on* the mockup in two places (tighter 1.95 ultrawide clamp +
edge-inset; keeping framed-art cards). Confirmed answers and the adjustments now folded in:

- **Q1 → Felt-swap, confirmed.** Per-act wood venue is the dominant clutter source and the meta
  screens already carry venue identity. Retain mood via vignette + candle-glow + thin gold hairline.
  Optional whisper of per-act flavour: tint the felt/glow warmth slightly per act (won't fight text).
- **Q2 → Drop the name at Compact, confirmed.** Rank + suit + ability only; names return at
  Comfortable+. **Adjustment:** when the name is dropped, the **ability-bar text must scale UP**
  (don't leave it at ~9.5px) — otherwise it has the same legibility problem.
- **Q3 → Inset edge elements to FRAME the felt, confirmed.** Keep our 1.95 clamp (do NOT inherit
  the mockup's 2.5). "Inset to frame, not to fill" — residual center felt is intentional breathing
  room; don't drag widgets inward to fill it.
- **Q4 → Must-ADOPT-as-drawn:** the **bout slots (#2)** must be two distinct per-card wells with
  role tabs — do not let it soften back into a single recessed "zone."

**Three pre-build adjustments (now part of the spec):**
1. **Reclassify Trump (#6) KEEP → ADAPT** and handle it in the edge-inset pass — it still strands
   far-right on bare felt at 16:9 even with the round-1 backing plate (`responsive_3_spacious.png`).
2. **Ability-bar text scales up** when the card name is dropped at Compact (extends flag #1).
3. **Neutralize the ribbon's sage-green border** (off-palette) to gold/neutral during the
   `olik` ribbon bugfix — keeps board chrome in one palette.

**Caveats to honour while building:**
- **Removed pile (#7):** confirm it is non-interactive before dropping to a bare count pill; if
  removed cards are ever inspectable, use a small labelled stack instead of a count.
- **Collapsed-log unread dot:** make it reflect real unread state, not a decorative always-on dot.

**Mockup elements explicitly NOT to carry over** (UX-confirmed): sage ribbon border; the mockup's
own 2.5 dead-space handling; 9px name / 9.5px ability text; schematic pip cards; `--chipscale`/
`--cw/--ch` multipliers (double-scale under CanvasScaler).
