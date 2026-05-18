# Wits and Fools: Gameplay Flow Bible

## Overview

A complete run of Wits and Fools is a ~15-minute journey through 5 acts of escalating Durak card matches, interleaved with shops, events, and rest stops. The player climbs the social ladder of Renaissance Italy, from dockside taverns to the Duke's private salon, building an ability loadout and collecting trinkets along the way.

This document maps every screen, transition, decision point, and emotional beat.

---

## Act Structure

| Act | Venue | Tone | Encounters |
|-----|-------|------|-----------|
| 1 | The Bilge Rat Tavern | Scrappy, casual, forgiving | 2-3 matches + 1-2 events |
| 2 | The Merchant's Rest | Stakes rising, economy matters | 2-3 matches + shop + event |
| 3 | The Guildmaster's Hall | Serious, elite opponents appear | 2-3 matches + elite + shop |
| 4 | The Cardinal's Library | Tense, mistakes punished | 2-3 matches + elite + events |
| 5 | The Duke's Salon | Final gauntlet, boss match | 2 matches + boss |

---

## Screen-by-Screen Flow

### 1. ARCHETYPE SELECT

**Purpose:** Define your starting identity and playstyle.

**Current state:** Four archetype buttons appear on a dark screen. Each archetype grants 3-4 starting abilities that shape early strategy.

| Archetype | Starting Abilities | Playstyle |
|-----------|-------------------|-----------|
| The Rogue | Blocker, Seize Initiative, Peek | Reactive, information-focused |
| The Brute | Double Trouble, Pile On, Extra Draw | Aggressive overwhelm |
| The Diplomat | Trump Changer, Deflect, Slip Away | Redirection, trump control |
| The Gambler | Gambit, Feint, Card Counter | High risk/reward |

**Decision weight:** HIGH. This is the seed of your build identity. The Rogue's defensive tools pair differently with shop offerings than The Brute's attack-heavy kit.

**Emotional beat:** Anticipation. "What kind of player am I going to be this run?"

**Art direction needs:**
- Each archetype needs a character portrait or emblem
- Visual preview of starting abilities with icons
- Atmospheric background suggesting the journey ahead
- This screen sets the tone for the entire run

---

### 2. MAP SELECT

**Purpose:** Choose your path through the current act.

**Current state:** Dark navy background. Gold act title ("Act 1 - The Bilge Rat Tavern"). Grey subtitle "Choose your path:". Vertical list of node buttons with red backgrounds showing node type and opponent name.

**Node types encountered:**
- **[Match] Opponent Name** - Standard Durak match vs a named AI
- **[ELITE] Opponent Name** - Harder match with house rule modifier, better rewards
- **[BOSS] The Champion** - Act 5 finale
- **Shop icon** - The Fence
- **Rumor icon** - Narrative event with choices
- **Rest icon** - The Hearth

**Decision weight:** MEDIUM. Path choice determines encounter type. Taking a match means risking prestige; taking rest means missing ability rewards.

**Emotional beat:** Strategic planning. "Do I take the elite for better rewards, or play safe with a rest stop?"

**Art direction needs:**
- This screen is the WORST offender visually. It's a text list on a flat background.
- Needs a proper branching map visualization (think Slay the Spire's node map)
- Each venue needs a background illustration or vignette
- Node types need distinctive icons (crossed swords for match, skull for elite, shop bag, campfire, scroll)
- Path connections should be visible as lines/roads
- Opponent previews should show archetype silhouettes
- The current act's venue should feel like a PLACE, not a menu

---

### 3. PRE-MATCH / MATCH TABLE

**Purpose:** The core gameplay. Play Durak against an opponent.

**Current state:** Green felt table background with wood-brown border frame. Top HUD bar shows phase ("Your move - attack" / "Defend!"), trump suit with colored glyph, and deck count. Opponent's hand at top (face-down, fanned). Player's hand at bottom (face-up, fanned). Deck stack at left with trump card rotated 90 degrees. Discard pile at right. Bout area in center for attack/defense pairs.

**Visual elements:**
- Cards: White/cream faces with suit symbols. Red backs with "W&F" text.
- Ability cards: Green dashed border with ability badge text (e.g., "DBL ATK", "BLOCK")
- Playable cards: Green solid border highlight
- Disabled cards: Greyed out
- HUD bar: Semi-transparent black, white text
- Action button: Bottom-right, tan/gold ("End bout" / "Take cards")
- Auto-play button: Bottom-left ("Auto: OFF")
- Run HUD: Bottom bar with Prestige hearts, Florins, Act counter, Abilities count

**Phases within a match:**
1. **Attack phase** - Player selects cards to play as attacks. Cards must match ranks already on the table.
2. **Defense phase** - Player covers attacks with higher cards of the same suit or any trump.
3. **Bout resolution** - Cards go to discard (successful defense) or defender takes them all (failed defense).
4. **Draw phase** - Both players draw back up to hand size from deck.
5. **Game over** - First player to empty their hand wins. Last player holding cards is "The Fool."

**Decision weight:** HIGHEST. Every card play matters. Using an ability card means spending it, bringing you closer to winning but burning a special power.

**Emotional beats:**
- Tension during defense ("Can I cover this?")
- Satisfaction on a clean defense ("All cards discarded!")
- Dread when forced to take cards
- Excitement when ability cards appear in hand
- Strategic agonizing ("Play this for the ability or save it for defense?")

**Art direction needs:**
- The table is the most visually complete screen but still programmer-art
- Cards need proper face designs (court cards especially)
- Card backs need a real pattern/logo design
- The felt texture should feel tactile, with subtle grain/noise
- Deck should show remaining card count prominently (now fixed)
- Opponent should have a portrait/nameplate above their hand
- Ability activation should have a visual flourish (glow, particle burst)
- The bout area needs clearer attack/defense pair visual grouping
- Trump suit indicator needs to be more prominent and thematic
- Sound design: card slides, shuffles, ability activation sounds

---

### 4. ABILITY CHOICE MODAL (during match)

**Purpose:** When clicking a card with an ability, choose to play it normally or activate the ability.

**Current state:** Dark blue/purple centered modal (480x220). Shows ability name in bold, description below. Two buttons: "Play normally" (left) and "Use [ABILITY]" (right). Buttons now grey out when that option isn't valid.

**Decision weight:** HIGH. This is the moment of maximum tension - do you use a powerful ability or play the card normally for a better tactical position?

**Emotional beat:** The "fork in the road" moment. Brief but intense.

**Art direction needs:**
- Modal should feel like an ornate card being presented
- Ability name should have its own icon/glyph
- Visual distinction between attack, defense, and utility abilities
- The card being played should be visually highlighted behind the modal
- Inactive buttons should feel clearly disabled, not just slightly dimmed

---

### 5. POST-MATCH RESULT

**Purpose:** Show match outcome, award florins, offer ability pick.

**Current state (Victory):** Dark background. Large gold "Victory!" title. White details ("You defeated Fishy Meg!"). Green reward text ("+8 Florins"). If won, shows "Choose an ability:" with 3 ability buttons plus "Skip" and "Continue".

**Current state (Defeat):** Gold "Defeat..." title. Details show opponent name and prestige remaining. No ability pick. Continue button.

**Sub-screen: Ability Pick (on victory):**
- 3 ability buttons in vertical layout, each showing name and description
- Rarity affects button color (not yet implemented in current screenshots)
- "Skip" button if player doesn't want any offering
- If at max ability slots, picking replaces the oldest ability

**Decision weight:** HIGH (ability pick). This is where builds are made. Choosing between a defensive ability that shores up weaknesses vs. an offensive one that doubles down on strengths.

**Emotional beats:**
- Victory: Triumph, excitement about rewards, anticipation of ability pick
- Defeat: Sting, worry about prestige loss, determination to recover
- Ability pick: Build-defining moment. "Endgame Specialist would complete my late-game build..."

**Art direction needs:**
- Victory should FEEL victorious - gold particles, fanfare, the opponent slinking away
- Defeat should feel solemn but not crushing - motivate the player to continue
- Ability pick cards should look like ornate tarot-style offerings
- Each ability needs an icon that's instantly recognizable at card size
- Rarity should have strong visual coding (bronze/silver/gold frames?)
- The reward summary should feel like counting coins on the table

---

### 6. SHOP (The Fence)

**Purpose:** Spend florins on abilities, trinkets, or burden removal.

**Current state:** Dark blue-teal background. Gold title "The Fence." Gold subtitle showing purse amount. Vertical list of items with grey backgrounds showing name, price, rarity tag, and description. "Leave" button at bottom.

**Available purchases:**
- Abilities: 8f (Common), 12f (Uncommon), 18f (Rare)
- Trinkets: ~15f
- Burden removal: 6f

**Decision weight:** HIGH. Resource allocation. Do you buy an ability that completes a combo, or save for a trinket that warps the rules of Durak in your favor?

**Emotional beat:** Window shopping with consequences. The shop should feel like a shady back-alley dealer presenting their wares.

**Art direction needs:**
- "The Fence" should feel like a character, not a menu
- Each item needs a visual representation (ability icon, trinket illustration)
- Price tags should look physical (dangling tags, chalk marks)
- Sold-out or unaffordable items should be visually distinct
- The background should suggest a cluttered, secretive market stall
- Trinkets especially need strong visual identity since they're persistent

---

### 7. EVENT / RUMOR

**Purpose:** Narrative encounters with risk/reward choices.

**Current state:** Dark purple background. Gold title. White narrative text in center. Two choice buttons with text descriptions. After choosing, outcome text appears in green/red and "Continue" button replaces choices.

**Event types:**
- Gambler's offer (risk florins for more)
- Mysterious stranger (ability offer with cost)
- Local gossip (free scouting info)
- Bar fight (lose prestige or ability)

**Decision weight:** MEDIUM. Events are the "spice" encounters. They break up the match-rest-shop rhythm with narrative flavor and meaningful gambles.

**Emotional beat:** Storytelling moment. The player should feel transported into a scene.

**Art direction needs:**
- Events are the game's main storytelling vehicle and currently the most barren screens
- Each event type needs an illustration (a shadowy figure, a card game in progress, a merchant's wagon)
- Choice buttons should feel consequential (ornate frames, risk indicators)
- Outcome reveal should have a dramatic beat (card flip animation?)
- The narrative text needs atmospheric framing (parchment background? speech scroll?)

---

### 8. REST (The Hearth)

**Purpose:** Recovery stop. Remove a burden or gain a small bonus.

**Current state:** Very dark background. Gold title "The Hearth." White narrative text ("The hearth crackles. You find a quiet moment of peace."). Single choice button or two options if player has burdens.

**Options:**
- Rest quietly (+3 Florins)
- Remove a burden (if applicable)

**Decision weight:** LOW-MEDIUM. Usually straightforward, but burden management can be critical.

**Emotional beat:** Relief. A breather between the tension of matches.

**Art direction needs:**
- Should feel warm and safe — crackling fire, amber lighting
- Strong contrast with the tension of match screens
- If burdens exist, show them as physical objects being discarded into the fire
- The hearth should be a visual anchor — same fireplace in every act but surroundings change

---

### 9. RUN OVER

**Purpose:** Final summary of the completed run.

**Current state:** Very dark background. Large gold title ("Run Over" / "Victory!" for act 5 completion). White stats text showing matches won/played, florins earned, abilities collected. "New Run" button.

**Displayed information:**
- Win/Loss status
- Acts completed
- Match record (W/L)
- Final florins
- Final ability loadout

**Decision weight:** None — this is reflection.

**Emotional beat:** 
- Win: Elation, pride in the build, desire to try again with a different archetype
- Loss: "What went wrong?", desire for revenge, "next time I'll take the Brute"

**Art direction needs:**
- Victory: Celebratory. The Duke's salon applauds. Your journey visualized.
- Loss: Reflective. Your character sits at the table, cards scattered.
- Show the journey: a visual timeline of acts completed, opponents faced
- Stats should feel like a score card or tournament bracket
- "New Run" should feel inviting, not punishing

---

### 10. RUN HUD (Persistent)

**Purpose:** Always-visible status bar during non-match screens.

**Current state:** Black semi-transparent bar at screen bottom. Shows:
- Prestige: Red heart symbols (e.g., "Prestige: ♥♥♥♥")
- Florins: Gold number
- Act counter: White text ("Act 1 of 5")
- Abilities: Blue text ("Abilities: 4/5")

**Art direction needs:**
- Prestige hearts should be more iconic (stylized, larger)
- Florins should show a coin icon
- Should integrate with each screen's aesthetic rather than floating on top
- Ability count should be interactive (click to see loadout?)
- During matches, integrates with the match HUD bar

---

## Transition Map

```
ARCHETYPE SELECT
      |
      v
MAP SELECT ---------> MATCH ---------> POST-MATCH RESULT
   |    ^                                    |
   |    |                                    v
   |    +------------ (continue) <------ ABILITY PICK
   |    |
   |    +------------ SHOP
   |    |
   |    +------------ EVENT / RUMOR
   |    |
   |    +------------ REST (The Hearth)
   |
   +--> (Act complete) --> MAP SELECT (next act)
   |
   +--> (Prestige = 0) --> RUN OVER (loss)
   |
   +--> (Act 5 boss defeated) --> RUN OVER (win)
```

---

## Pacing Analysis

A typical winning run (~15 minutes):
1. Archetype Select (10 seconds)
2. Act 1: 2 matches + 1 rest (3 minutes)
3. Act 2: 2 matches + 1 shop (3 minutes)
4. Act 3: 2 matches + 1 elite + 1 event (4 minutes)
5. Act 4: 2 matches + 1 elite + 1 shop (4 minutes)
6. Act 5: 1 match + boss (3 minutes)
7. Run Over (10 seconds)

**Rhythm:** Match (tension) -> Result (release) -> Map (planning) -> [Shop/Event/Rest] (investment) -> Match (tension)

The loop works well mechanically. The gap is that every non-match screen feels identical in tone and atmosphere — they're all "text on dark background." The visual progression from tavern to salon is entirely absent from the player experience.
