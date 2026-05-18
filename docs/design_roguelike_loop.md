# Wits and Fools: Roguelike Meta-Loop Design Document

---

## The Core Design Insight

Durak inverts the normal roguelike power fantasy. You win by **shedding cards**, not accumulating. "Getting stronger" can't mean "better deck" because both players share one 36-card deck. There is no HP, no scoring, no damage numbers. You either empty your hand first (win) or you don't (Fool).

Power expression in Wits and Fools comes from three axes **unique to each player**:

1. **Abilities** — which special effects you bring to the table (your "loadout")
2. **Trinkets** — persistent passive rules that bend Durak in your favor
3. **Preparation** — knowing your opponent and adapting before the match

**The metaphor is not "build a stronger deck." It's "become a better card player."** You're upgrading yourself — your tricks, your tools, your reputation, your knowledge of your opponents.

---

## 1. Run Structure — "The Renaissance Circuit"

You are a traveling card sharp making your way through a Renaissance city's hierarchy of taverns, guildhalls, and noble courts. Each run represents one evening's progression from the lowly dockside pub to the Duke's private salon. Win your way to the top and unseat the reigning champion. Lose, and you slink back to the docks.

### 5 Acts

| Act | Venue | Flavor | Difficulty |
|-----|-------|--------|-----------|
| 1 | The Bilge Rat Tavern | Dockworkers, sailors, petty thieves | Novice |
| 2 | The Merchant's Rest | Traders, artisans, foreign merchants | Journeyman |
| 3 | The Guildmaster's Hall | Guild officials, minor nobility, fixers | Adept |
| 4 | The Cardinal's Library | Clergy, scholars, spymasters | Master |
| 5 | The Duke's Salon | The Duke's inner circle, the Champion | Grandmaster |

Each Act presents a **branching map** of 3-4 columns with 2-3 nodes each. The player chooses one node per column, Slay-the-Spire style. A typical run hits ~12-15 encounters, of which 7-9 are actual Durak matches.

### Node Types

| Node | Description | Reward |
|------|-------------|--------|
| **Rival Match** | Standard Durak vs a named AI opponent with personality | 8-12 Florins + choose 1 of 3 abilities |
| **Elite Match** | Harder opponent with a House Rule modifier | Better ability pool (rare-weighted) + guaranteed Trinket |
| **The Fence** (shop) | Buy abilities, Trinkets, services with Florins | Spend resources |
| **Rumor** (event) | Narrative encounter with risk/reward choices | Variable (abilities, Florins, Trinkets, Burdens) |
| **The Hearth** (rest) | Choose: Mend, Study, or Eavesdrop | Recovery, growth, or intelligence |
| **Boss Match** | Act 5 finale — the reigning Champion | Run completion |

### Rest Site Actions (The Hearth)

- **Mend**: Remove one Burden (negative modifier)
- **Study**: Upgrade one ability to its enhanced version
- **Eavesdrop**: Reveal the next two opponents' personality archetype and ability loadout

---

## 2. Growth Axis 1: Ability Loadout

### How It Works

The player starts each run with **3 ability slots** (expandable to a maximum of 8 via Trinkets). Each slot holds one ability type. Before each match, the system builds the 36-card deck and assigns ability tags based on both players' loadouts. A card drawn by either player retains its ability tag, but **only the player whose loadout assigned it can activate it**.

This creates hidden information: you know the deck contains your Blocker somewhere, but you don't know which hand it will end up in. And the opponent might draw a card tagged with YOUR ability — they can see the tag but can't use it.

### Ability Pool (16 Abilities)

Expanded from the current 6 to 16, organized by role. Each ability has an **upgrade** available at Rest sites (Study action).

#### Attack Abilities (played during Attack phase)

| Ability | Effect | Upgrade | Rarity | Binding Count |
|---------|--------|---------|--------|---------------|
| **Double Trouble** | Next attack ignores rank-match rule | **Triple Threat**: ignores for 2 attacks | Common | 3 cards |
| **Extra Draw** | Force defender to draw 2 cards before defending | **Deluge**: force draw 3 | Common | 3 cards |
| **Pile On** | +2 max attacks this bout | **Avalanche**: +3 max attacks | Uncommon | 2 cards |
| **Feint** | Play deck's top card face-down as phantom attack; defender must beat it | **Grand Feint**: two phantom attacks | Rare | 1 card |

#### Defense Abilities (played during Defense phase)

| Ability | Effect | Upgrade | Rarity | Binding Count |
|---------|--------|---------|--------|---------------|
| **Blocker** | No more attacks can be added this bout | **Iron Wall**: also auto-defends the lowest undefended slot | Common | 3 cards |
| **Double Defense** | This card covers two undefended attack slots | **Triple Guard**: covers three | Uncommon | 2 cards |
| **Deflect** | Redirect bout — swap roles mid-bout; opponent must now defend | **Riposte**: swap + you add one attack | Rare | 1 card |
| **Slip Away** | Discard all undefended attacks (to discard pile, not your hand) | **Vanish**: discard ALL bout cards, even paired ones | Rare | 1 card |

#### Utility Abilities (either phase)

| Ability | Effect | Upgrade | Rarity | Binding Count |
|---------|--------|---------|--------|---------------|
| **Trump Changer** | Change trump suit to this card's suit (once per match) | **Trump Decree**: change trump AND draw 1 card | Common | 3 cards |
| **Seize Initiative** | You become attacker next bout | **Coup**: become attacker AND opponent discards random card | Uncommon | 2 cards |
| **Peek** | Reveal and rearrange top 3 cards of deck | **Oracle**: top 5 cards | Uncommon | 2 cards |
| **Gambit** | Discard entire hand, draw same count from deck | **Calculated Gambit**: draw +1 card | Rare | 1 card |

#### Passive Abilities (always active, no card binding, occupy a slot)

| Ability | Effect | Rarity |
|---------|--------|--------|
| **Trump Affinity** | When you draw a trump card, draw 1 additional card, then discard 1 of your choice | Uncommon |
| **Endgame Specialist** | When deck has 6 or fewer cards, play any card as defense regardless of suit (rank must still beat) | Uncommon |
| **Card Counter** | HUD shows count of each rank remaining in the deck | Uncommon |
| **Quick Hands** | After a successful all-defended bout, draw 1 extra during refill, then discard 1 of choice | Rare |

### Binding Count Design

Binding count determines how many cards in the 36-card deck carry each ability:

- **Common (3 cards)**: Reliable — you'll likely see at least one per match. Individually weaker effects.
- **Uncommon (2 cards)**: Moderate reliability. Stronger effects worth building around.
- **Rare (1 card)**: Powerful but unreliable. Might not draw the one card that carries it. Creates exciting variance.
- **Passive (0 cards)**: Always active, no draw luck involved. Occupies a slot without needing a card.

This creates tension: do you fill your loadout with reliable Commons, or gamble on Rares that might never fire?

### Acquiring Abilities

| Source | Selection |
|--------|-----------|
| **Run start** | Choose 3 from a random offering of 4 (weighted by rarity) |
| **Rival Match win** | Choose 1 of 3 randomly offered |
| **Elite Match win** | Choose 1 of 3 (rare-weighted pool) |
| **The Fence (shop)** | Buy specific abilities for Florins |
| **Rumor events** | Some grant or transform abilities |

If ability slots are full when gaining a new ability, the player must discard one to make room.

---

## 3. Growth Axis 2: Trinkets (Relic Equivalent)

Persistent passive modifiers that warp the rules of Durak itself. The player can hold up to **5 Trinkets** at once. Named "Tokens of Favor" in the fiction — gifts, bribes, and stolen goods collected on your way up the social ladder.

### Trinket Categories

#### Economy Trinkets

| Trinket | Effect |
|---------|--------|
| **The Merchant's Purse** | +3 Florins after each match |
| **The Miser's Ring** | +1 Florin per bout where you successfully defend |
| **Fool's Gold** | Start each match with a temporary "Gold Card" in hand (counts as 7 of any suit, dissolves after one use) |

#### Hand Management Trinkets

| Trinket | Effect |
|---------|--------|
| **The Tailor's Thimble** | Starting hand size is 5 instead of 6 (closer to winning!) |
| **The Juggler's Balls** | After the first bout, discard 1 card from hand for free |
| **Loaded Dice** | At match start, look at bottom 3 cards of deck and rearrange them |
| **The Courtier's Fan** | Once per match, when you would draw cards, draw 1 fewer (minimum 0) |

#### Combat Trinkets

| Trinket | Effect |
|---------|--------|
| **The Duelist's Glove** | First attack each bout ignores rank-match rule |
| **The Shield Brooch** | Once per match, auto-beat the first undefended attack card |
| **Poisoned Wine** | When opponent eats cards, they draw 2 extra from deck |
| **The Spy's Monocle** | See the top card of the deck at all times |
| **Marked Deck** | At match start, reveal 3 random cards in opponent's hand |

#### Trump Trinkets

| Trinket | Effect |
|---------|--------|
| **The Alchemist's Stone** | Choose the trump suit at match start |
| **Crown of Thorns** | Trump cards you play in defense also count as attacks for next bout's rank-match |
| **The Heretic's Brand** | Opponent's trump cards treated as 1 rank lower for defense |

#### Ability Trinkets

| Trinket | Effect |
|---------|--------|
| **The Scholar's Tome** | +1 ability slot |
| **The Forger's Kit** | Abilities bound to 1 card are now bound to 2 |
| **Quicksilver Vial** | Once per match, use an ability without discarding the card |
| **The Ventriloquist's Dummy** | Once per match, use one of the opponent's abilities as your own |

#### Risk/Reward Trinkets

| Trinket | Effect |
|---------|--------|
| **The Devil's Bargain** | At match start, choose: +3 Florins OR draw 1 fewer starting card |

---

## 4. Challenge Scaling — "Rivals of the Renaissance"

### 4A. AI Personality Archetypes

Each AI opponent is a named character with a personality archetype governing decision-making, a mechanical loadout, and (for Elites/Bosses) a House Rule.

| Archetype | Attack Style | Defense Style | Ability Usage | Weakness |
|-----------|-------------|---------------|---------------|----------|
| **The Brawler** | Plays highest cards first, piles aggressively | Eats readily rather than spend trumps | Uses attack abilities eagerly | Burns trumps early |
| **The Miser** | Only lowest non-trump cards, stops piling early | Cheapest possible defense, never eats voluntarily | Hoards abilities, uses only when losing | Predictable, never presses advantage |
| **The Fox** | Varies attack strength to probe | Strategically eats small bouts | Uses abilities at optimal moments | Overthinks simple situations |
| **The Noble** | Only attacks with face cards and aces | Defends with matching suits (aesthetic) | Uses abilities for "style" | Weak low-card game |
| **The Scholar** | Attacks with ranks matching discard (card counting) | Defends efficiently, tracks remaining cards | Information abilities first | Slow to adapt when plans fail |
| **The Assassin** | Holds back, then dumps 3+ attacks in one bout | Eats small attacks to maintain hand for big turns | Saves abilities for killing blows | Vulnerable to repeated defense pressure |

### 4B. Scaling Across Acts

| Dimension | Act 1 | Act 2 | Act 3 | Act 4 | Act 5 |
|-----------|-------|-------|-------|-------|-------|
| Opponent ability slots | 2 | 3 | 4 | 5 | 6 |
| Ability rarity access | Common | Common + Uncommon | Uncommon + Rare | Rare | Rare + Upgraded |
| Opponent Trinkets | 0 | 0-1 | 1-2 | 2-3 | 3-4 |
| AI archetypes available | Brawler, Miser | + Fox | + Noble, Scholar | + Assassin | Mixed/hybrid |
| AI quality | 10% random moves | 5% random | 0% random | 0% + 1-ply look-ahead | Perfect + look-ahead |

"AI quality" means the greedy heuristic occasionally makes deliberately suboptimal plays in early Acts. In Act 4+, the AI uses a simple 1-ply lookahead (evaluate all legal moves, pick best resulting state).

### 4C. House Rules (Elite and Boss Mechanics)

House Rules are temporary Durak rule modifications for a single match, announced before the match so the player can plan.

#### Elite House Rules (one per Elite, drawn from pool)

| House Rule | Effect |
|------------|--------|
| **"No Trumps Before Dusk"** | Trump cards cannot be played in the first 3 bouts |
| **"The Gauntlet"** | Starting attacker never rotates (same player attacks every bout) |
| **"Blindfolded"** | You cannot see the trump card (know suit but not rank) |
| **"Heavy Hands"** | Starting hand size is 8 instead of 6 for both players |
| **"The Taxman"** | After each bout, both players discard their lowest card to the discard pile |
| **"Cutthroat"** | Max 4 attacks per bout, but attacker can attack with any rank |
| **"Double or Nothing"** | If defender eats, they draw 2 extra from deck on top |
| **"The Mirror"** | Opponent copies your ability loadout |

#### Boss: The Champion

- **House Rule: "The Duke's Wager"** — The Champion starts with 5 cards (one card advantage). If the Champion successfully defends 3 consecutive bouts, they discard 2 cards for free.
- **Personality**: Hybrid Fox/Assassin — patient, reads patterns, then strikes
- **Loadout**: 6 ability slots (all Rare, 2 upgraded), 4 Trinkets

---

## 5. Burdens (Negative Modifiers)

The counterpart to Trinkets. Acquired from match losses and risky Rumor events. Removable at Rest sites (Mend) or bought off at shops.

| Burden | Effect |
|--------|--------|
| **Rattled Nerves** | First defense each bout must use highest-rank card in hand |
| **Heavy Purse** | Starting hand size is 7 instead of 6 |
| **Marked Cards** | Opponent sees 1 random card in your hand at all times |
| **Clumsy Fingers** | Once per match, a random ability activation fizzles |
| **Bad Reputation** | Shop prices +20% |

---

## 6. Economy

### Florins (Run Currency)

**Earned from:**

| Source | Amount |
|--------|--------|
| Match win | 8-12 (scales with Act) |
| Clean sweep bonus (no cards eaten) | +5 |
| Speed bonus (5 or fewer bouts) | +3 |
| Per-bout defense bonus | +1 per successful defense |
| Match loss (consolation) | 2 |

**Spent at The Fence (shop):**

| Item | Cost |
|------|------|
| Common ability | 8 Florins |
| Uncommon ability | 12 Florins |
| Rare ability | 18 Florins |
| Trinket | 12-25 Florins |
| Burden removal | 10 Florins |
| Ability upgrade | 15 Florins |
| Opponent intel (reveal next opponent's loadout) | 5 Florins |

**No interest system.** Durak is about shedding, not accumulating — the design philosophy extends to economy. Instead: **spending bonus** — after spending 20+ Florins at a single shop, gain a free Common ability.

### Prestige (Run HP)

The player starts with **3 Prestige** (visualized as ornate coins/crests). Losing a match costs 1 Prestige **and** applies a random Burden. At 0 Prestige, the run ends.

Prestige recovery is deliberately scarce:
- One Trinket ("The Phoenix Medal") restores 1 Prestige once per run
- One specific Rumor event can restore 1 Prestige

Each loss is doubly punishing (life + debuff) but the Burden is removable at Rest, creating meaningful Rest decisions.

### Reputation (Cross-Run Meta-Progression)

Accumulates forever, never spent. Unlocks permanent content.

| Source | Amount |
|--------|--------|
| Complete run (beat the Boss) | +50 |
| Reach Act 3 | +10 |
| Reach Act 5 | +25 |
| Fail a run | +5 |
| Achievements | +5-20 each |

**Unlock thresholds:**

| Rep | Unlock |
|-----|--------|
| 25 | The Brute (starting archetype) |
| 50 | 4 new abilities added to offering pool |
| 100 | The Diplomat (starting archetype) |
| 150 | 8 new Trinkets added to pool |
| 200 | Ascension Mode (stackable difficulty modifiers) |
| 300 | The Gambler (starting archetype) |
| 500 | The Masquerade (alternate final boss) |

---

## 7. Starting Archetypes

Each archetype has a different starting ability loadout and unique starting Trinket, encouraging a different playstyle. Unlocked via Reputation.

### The Rogue (Default — Always Available)
- **Starting abilities**: Blocker, Seize Initiative, Peek
- **Starting Trinket**: The Juggler's Balls (discard 1 after first bout)
- **Playstyle**: Reactive, defensive, information-gathering. Learns what the opponent has, then strikes.

### The Brute (25 Reputation)
- **Starting abilities**: Double Trouble, Pile On, Extra Draw
- **Starting Trinket**: The Duelist's Glove (first attack ignores rank-match)
- **Playstyle**: Aggressive, overwhelming attacks. Tries to make opponents eat every bout.

### The Diplomat (100 Reputation)
- **Starting abilities**: Trump Changer, Deflect, Slip Away
- **Starting Trinket**: The Alchemist's Stone (choose trump suit)
- **Playstyle**: Trump manipulation and redirection. Controls the flow of the game.

### The Gambler (300 Reputation)
- **Starting abilities**: Gambit, Feint, Card Counter
- **Starting Trinket**: Fool's Gold (gold card in starting hand)
- **Playstyle**: High risk/reward. Information advantage and dramatic hand resets.

---

## 8. Build Identity and Emergent Synergies

Different runs should feel different based on player choices. Build identity emerges from the intersection of starting archetype, ability offerings, Trinket combinations, and adaptation to opponents.

### Example Synergy Builds

**"The Trump Lord"**
- Trump Changer + Trump Affinity + Alchemist's Stone + Heretic's Brand
- Total trump suit domination. Choose a suit where you're strong, change trump to it, draw extra when hitting trumps, weaken opponent's trump defense.

**"The Wall"**
- Blocker + Double Defense + Shield Brooch + Tailor's Thimble
- Near-impenetrable defense. Start with fewer cards (closer to winning). Block everything. Win by never eating.

**"The Blitz"**
- Double Trouble + Pile On + Feint + Duelist's Glove + Poisoned Wine
- Overwhelming attacks. Ignore rank rules. Stack phantom attacks. When opponent eats, they draw even more dead weight.

**"The Ghost"**
- Slip Away + Gambit + Quick Hands + Courtier's Fan + Tailor's Thimble
- Dodge everything. Keep hand small. Shed fast. If hand gets bad, Gambit into a better one.

**"The Information Broker"**
- Peek + Card Counter + Marked Deck + Spy's Monocle + Endgame Specialist
- See everything. Know what's coming. Play perfect information Durak while opponent plays blind.

---

## 9. Between-Match Flow

After each match:

1. **Result Screen**: Win/Loss, bouts played, cards eaten by each player, abilities used, Florins earned. Brief animated summary.
2. **Reward Selection** (wins only): Choose 1 of 3 abilities. If slots are full, must discard one.
3. **Map View**: Branching path for the current Act. Upcoming opponents show portrait and archetype name (not loadout, unless Eavesdropped). Player selects next node.
4. **Pre-Match Screen** (before matches): Opponent name, portrait, archetype description, House Rule (if any). Player reviews their own loadout. Optional: one last-minute single-item purchase.

---

## 10. Replayability Drivers

1. **4 Starting Archetypes** with fundamentally different playstyles
2. **Randomized maps** with branching paths (different opponent sequences)
3. **Randomized ability offerings** (never the same build twice)
4. **16 abilities x upgrades = 32 variations** to combine in 3-8 slots
5. **20+ Trinkets** creating emergent synergies
6. **6 AI archetypes** presenting different puzzles
7. **8+ House Rules** demanding adaptation
8. **Rumor events** with narrative variety and risk/reward
9. **Ascension system** (at 200 Rep): stackable difficulty modifiers for mastery
10. **Achievements**: "Win without eating any cards." "Beat the Champion in 4 bouts." "Win with only passive abilities."

---

## 11. Rumor Events (Sample)

Events are narrative encounters at "?" nodes offering choices with mechanical consequences.

### "The Drunk Merchant"
*A wine-soaked trader slides a card across the table. "Found this in the Eastern Markets. Yours for a song... or a dare."*
- **Option A**: Pay 8 Florins for a random Uncommon ability
- **Option B**: Play a 1-bout mini-game against the merchant. Win = free Rare ability. Lose = gain the Burden "Heavy Purse."

### "The Forger's Offer"
*A hooded figure whispers: "I can upgrade any trick in your repertoire. But my work comes with... side effects."*
- **Option A**: Upgrade one ability to its enhanced version (free Study). Gain the Burden "Clumsy Fingers."
- **Option B**: Decline politely. Gain 5 Florins (the forger feels guilty for wasting your time).

### "The Cardinal's Confession"
*A nervous clergyman pulls you aside. "I know things about your next opponent. Their weaknesses. Their tells. But this knowledge comes at a price."*
- **Option A**: Pay 6 Florins. Reveal the next opponent's full loadout AND personality AND House Rule.
- **Option B**: The Cardinal owes you a favor. Gain the Trinket "The Cardinal's Seal" (+2 Florins after each match in The Cardinal's Library).

### "The Pickpocket"
*A child darts past, fingers grazing your pouch. You catch their wrist. They look up with defiant eyes.*
- **Option A**: Let them go. Lose 5 Florins. Gain 1 Prestige (word spreads of your mercy).
- **Option B**: Take their stolen goods. Gain a random Trinket. Gain the Burden "Bad Reputation."
- **Option C**: Recruit them. Gain the Trinket "The Spy's Monocle" (see top of deck). Lose 1 ability slot for the rest of the run (the kid takes up space at your table).

---

## 12. Implementation Architecture

### Relationship to Existing Codebase

The meta-loop wraps around the existing single-match engine without modifying its core logic. The boundary is clean:

- **Before a match**: `MatchSetup` takes `RunState` + `OpponentProfile` and produces a configured `GameEngine`
- **During a match**: `GameEngine` runs exactly as it does today (with `RulesConfig` overrides for House Rules)
- **After a match**: Read `WinnerIndex`/`FoolIndex` and match stats, update `RunState`

### New Runtime Types

```
RunState            — Serializable state for the full run
RunConfig           — Database of all content (abilities, trinkets, opponents, events, house rules)
RunManager          — MonoBehaviour state machine (Map → Shop/Event/Rest → Match → Result → Map)
MapGenerator        — Procedural branching map per Act
MapNode             — Data for a single map node
MatchSetup          — Configures GameEngine from RunState + OpponentProfile
OpponentProfile     — Named opponent: archetype, loadout, house rule, dialogue
AIArchetype         — Enum + parameters for personality-driven AI behavior
EnhancedAIPlayer    — Extended AIPlayer accepting archetype parameters
AbilityDefinition   — Data for each ability: rarity, binding count, upgrade, description
TrinketDefinition   — Data for each trinket
BurdenDefinition    — Data for each burden
HouseRule           — Enum + rule override data
RulesConfig         — Mutable replacement for Rules.cs constants
ShopState           — Shop inventory and purchase logic
EventDatabase       — Rumor events with choice trees
RunSaveSystem       — JSON persistence for mid-run saves
ReputationSystem    — Cross-run unlock tracking
```

### Modified Existing Types

| Type | Change |
|------|--------|
| `GameEngine` | Accept `RulesConfig` + `HouseRuleSet`, ability ownership check, Trinket hook events |
| `Rules` | Convert constants to configurable `RulesConfig` struct |
| `DeckConfig` | Dynamic builder combining both players' loadouts with ownership tracking |
| `Card` | Add `int? OwnerPlayerIndex` |
| `AIPlayer` | Accept `AIArchetype` parameters |
| `AbilityType` | Expand to 16 abilities |
| `GameManager` | Demote to match-only controller (RunManager owns the outer loop) |

### UI Architecture

Single scene with panel state machine (avoids scene-loading overhead):

```
RunManager controls:
  ├── MapPanel        — Branching map view, node selection
  ├── ShopPanel       — Ability/trinket shop
  ├── EventPanel      — Rumor event narrative + choices
  ├── RestPanel       — Mend/Study/Eavesdrop
  ├── MatchPanel      — The existing game view (GameManager)
  ├── ResultPanel     — Post-match results + reward selection
  └── LoadoutPanel    — Persistent sidebar: abilities, trinkets, burdens, florins, prestige
```

### Implementation Phases

**Phase 1 — Core Data** (no UI, smoke-testable)
- RunState, RunConfig, all definition types, MapGenerator, MatchSetup
- Automated: generate 100 runs, verify state consistency

**Phase 2 — Engine Refactoring**
- RulesConfig extraction, HouseRule support, ability ownership, 10 new abilities, EnhancedAIPlayer
- Automated: 50-game smoke test per HouseRule/AIArchetype combo

**Phase 3 — Run Flow**
- RunManager state machine, save system, shop logic
- Automated: full auto-play runs from Act 1 through Boss

**Phase 4 — UI**
- Map, shop, event, rest, loadout, result views
- Human playtest: 3 complete runs

**Phase 5 — Content**
- All abilities with upgrades, all trinkets, AI archetypes, events, dialogue
- Balance: 200 auto-play runs, verify win-rate spread

**Phase 6 — Meta-Progression**
- Reputation, archetype unlocks, Ascension, achievements
