# Slay the Spire: Core Gameplay Loop Analysis

Research reference for the Wits and Fools roguelike meta-loop design.

---

## 1. Core Battle Loop

Each combat encounter is a turn-based card game against one or more enemies. Per turn:

1. **Draw phase**: Draw 5 cards from draw pile, receive 3 energy
2. **Play phase**: Play any number of cards (each costs 0-5 energy). Card types:
   - **Attacks**: Deal damage
   - **Skills**: Block, apply debuffs, draw cards
   - **Powers**: Persistent effects for the rest of combat
3. **Discard phase**: Unplayed cards discarded, unused energy lost (hand limit: 10)
4. **Enemy turn**: Enemies execute their telegraphed actions simultaneously

When the draw pile empties, the discard pile shuffles to form a new draw pile.

### The Intent System

The critical innovation: icons above each enemy **telegraph their next action** — attack (showing exact damage), block, buff, debuff, or special. This transforms combat into a solvable optimization problem with known constraints. The tension lives in the gap between what you want to play and what your energy and draw allow.

---

## 2. Run Structure

A standard run consists of **3 Acts** (optional Act 4 for keys). Each act has **15-17 floors** arranged as a branching map with 1-6 nodes per row. The player starts at the bottom and chooses a path upward.

### Node Types

| Node | Frequency | Reward |
|------|-----------|--------|
| **Monster** (standard fight) | Most common | Gold + potion chance + card reward |
| **Elite** (skull marker) | 1-3 per act | Relic + gold + card reward |
| **Rest Site** | 2-4 per act | Heal 30% HP OR upgrade one card |
| **Shop** | 1-2 per act | Buy cards/relics/potions, remove cards |
| **Unknown/Event** ("?") | Several per act | Narrative scenarios with tradeoffs |
| **Treasure** | Floor 9 guaranteed | Relic chest |
| **Boss** | Final floor | Boss relic choice (powerful + drawback) |

**Structural guarantees**: Floor 1 is always an easy fight. Floor 9 always has treasure rooms. Floor 15 always has rest sites (pre-boss healing).

---

## 3. Growth Mechanics

### Card Rewards (Deckbuilding)

After each combat, choose 1 card from 3 (or skip). Rarity tiers: Common, Uncommon, Rare.

**Critical insight: not taking a card is often correct.** Deck dilution is a real threat — a bloated deck reduces the chance of drawing key cards. The discipline to skip card rewards is one of the game's deepest strategic lessons.

### Card Removal

Shops allow removing a card for gold (price escalates). Trimming weak starter cards (Strikes, Defends) increases deck consistency. A 15-card deck with strong synergy often outperforms a 30-card deck with individually powerful cards.

### Card Upgrades

At rest sites, upgrade one card instead of healing. Upgrades improve damage, reduce cost, or add effects. Creates persistent **heal-vs-upgrade tension**: survive the immediate future, or invest in long-term power.

### Relics (~170 Total)

Passive items from elites, bosses, treasure, shops, and events. Tiers: Common, Uncommon, Rare, Boss, Shop, Event.

Range from simple stat boosts to game-warping effects:
- **Snecko Eye**: Randomizes all card costs but draws 2 extra cards
- **Dead Branch**: Creates a random card whenever you Exhaust a card
- **Runic Pyramid**: Retain your hand between turns (no discard)

Relics are the primary source of exponential power growth and the main reason to fight elites.

### Potions (Consumables)

2-3 slots. One-use burst effects: healing, temporary strength, block, enemy debuffs, card draw. Serve as a safety valve — banking them for elites/bosses is core resource management.

### Power Curve

A run's power grows through compounding interaction of a refined deck, accumulated relics, and upgraded cards. Early acts demand survival; later acts demand a cohesive engine.

---

## 4. Enemy Design and Challenge Curve

### Normal Enemies

- **Act 1**: Straightforward, teaching attack/block rhythms
- **Act 2**: Enemies with debuffs, multi-attacks, status card injection (Wounds, Burns, Dazed)
- **Act 3**: Artifact (debuff immunity), high damage, strategy-punishing mechanics

### Elites (Skill-Checks)

Each elite is designed as a puzzle testing a specific capability:

- **Gremlin Nob** (Act 1): Gains Strength every time player plays a Skill. Punishes defensive play, forces aggression.
- **Lagavulin** (Act 1): Debuffs Strength and Dexterity every 3 turns. Creates a damage race.
- **Slavers** (Act 2): Multi-enemy fight testing AoE capability.
- **Giant Head** (Act 3): Applies Slow (building counter toward massive damage). Demands fast resolution.

### Bosses (Act Capstones)

Signature gimmicks that demand specific deckbuilding solutions:

- **Hexaghost** (Act 1): Massive multi-hit opening attack. Tests early-game block capability.
- **The Champ** (Act 2): Cleanses all debuffs and executes devastating attack below half HP. Punishes debuff-reliant builds.
- **Awakened One** (Act 3): Gains Strength whenever player plays a Power card. Directly punishes an entire card type.

### Difficulty Scaling

- Act 1: Low HP, simple patterns
- Act 2: ~2x enemy HP, punishing debuff mechanics
- Act 3: 200+ HP, artifact charges, deck-disrupting mechanics
- **Ascension system** (20 levels): Stackable modifiers — more elites, reduced healing, stronger enemies, consecutive bosses

---

## 5. Strategic Depth

Every node presents a meaningful decision:

### Map Pathing
Do you take a path with 2 elites for relic rewards at HP cost, or a safer path with rest sites and events? Made with incomplete information (visible map, unknown specific encounters).

### Card Selection
Adding a synergistic card compounds power. Adding a generically "good" card may dilute the deck. Skipping is often correct.

### Rest Site Choice
Heal to survive vs. upgrade to scale. Escalates as the run progresses — more impactful upgrades but less HP margin.

### Shop Decisions
Powerful relic vs. specific card vs. card removal vs. saving gold. Every purchase has opportunity cost.

### Event Gambling
Many events offer powerful rewards paired with costs — accept a curse for a relic, lose max HP for a rare card, or play safe. Depends on deck size, removal access, and curse-synergy relics.

### Build Identity

Emerges organically. After finding a key relic and synergistic card offerings, the player commits to an archetype. The game rewards adaptation to what the run provides rather than forcing a predetermined build.

---

## 6. Character Diversity

Each of 4 characters has a unique starting deck, starter relic, and exclusive card pool:

### Ironclad
- **Starter relic**: Burning Blood (heal 6 HP after each combat)
- **Archetypes**: Strength/Heavy Blade, Corruption/Dead Branch (exhaust engine), Barricade/Body Slam (infinite block)
- **Identity**: Aggressive, self-damage tradeoffs, healing relic allows early aggression

### Silent
- **Starter relic**: Ring of the Snake (draw 2 extra at combat start)
- **Archetypes**: Poison (scaling DoT), Shivs (zero-cost attack quantity), Discard synergies
- **Identity**: Defensive, tempo-oriented, relies on Weak debuffs and Dexterity scaling

### Defect
- **Starter relic**: Cracked Core (channel 1 Lightning orb at start)
- **Unique mechanic**: Orbs (Lightning, Frost, Dark, Plasma) passively trigger each turn
- **Identity**: Focus scaling amplifies orb output. Managing orb slots is the core puzzle.

### Watcher
- **Starter relic**: Pure Water (Miracle card = 0-cost, gain 1 energy)
- **Unique mechanic**: Stances (Wrath = 2x damage dealt/received, Calm = 2 energy on exit)
- **Identity**: Highest damage ceiling, smallest margin for error. Stance-dancing is core skill expression.

Each starter relic shapes early-game strategy, while exclusive card pools create entirely different decision spaces.

---

## 7. Risk/Reward Tradeoffs

Risk/reward is the connective tissue of every system:

### Elite Pathing
Elites grant relics (strongest permanent power) but cost significant HP. Skipping elites means a safer but weaker run. Must judge if current deck can handle the elite and if the relic justifies the HP cost.

### Boss Relics
After each boss, choose 1 of 3 boss relics. Extremely powerful but severe drawbacks:
- **Ectoplasm**: +1 energy, but no gold gain ever
- **Runic Dome**: +1 energy, but enemy intents hidden
- **Coffee Dripper**: +1 energy, but can never rest

Each forces a fundamental playstyle change.

### Cursed Events
Many "?" events offer powerful rewards + Curses (unplayable deck-cloggers). Whether to accept depends on deck size, removal access, and curse-synergy relics.

### HP as Resource
Health is spendable. Self-damage cards (Offering: 6 HP for 2 energy + 5 draw), aggressive pathing, and risky events are calculated investments. The question is never "did I take damage?" but "was the damage worth what I gained?"

### Card Addition Risk
Every card added reduces the probability of drawing every other card. The hidden cost of deck dilution means mastery involves knowing when NOT to add cards.

---

## Key Design Takeaways for Wits and Fools

1. **The map IS the meta-game** — branching paths with different node types create the strategic layer between combats
2. **Opponents as puzzles** — each enemy tests a different capability, forcing build diversity
3. **Risk/reward at every decision point** — elite pathing, rest site heal-vs-upgrade, event choices
4. **Relics are the power fantasy** — passive items that warp rules create the most memorable moments
5. **Character diversity drives replayability** — different starting conditions force different strategies
6. **Less is more for decks** — the courage to skip/remove is as important as the courage to add
7. **Intent telegraphing** — letting the player see what's coming transforms random combat into solvable puzzles
8. **HP as resource, not just health** — having a "life total" that's spendable creates tension even when you're winning
9. **Ascension system** — stackable difficulty modifiers give mastered content infinite replayability
