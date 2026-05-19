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

Each Act presents a **branching map** of 3-4 columns with 2-3 nodes each (Act 5 is shorter: 2 columns + boss). The player chooses one node per column, Slay-the-Spire style.

### Node Types

| Node | Description | Reward |
|------|-------------|--------|
| **Rival Match** | Standard Durak vs a named AI opponent with personality | Florins + choose 1 of 3 abilities |
| **Elite Match** | Harder opponent with a House Rule modifier | Better ability pool (rare-weighted) + guaranteed Trinket |
| **The Fence** (shop) | Buy abilities, Trinkets, services with Florins | Spend resources |
| **Rumor** (event) | Narrative encounter with risk/reward choices | Variable (abilities, Florins, Trinkets, Burdens) |
| **The Hearth** (rest) | Choose: Mend, Study, or Eavesdrop | Recovery, growth, or intelligence |
| **Boss Match** | Act 5 finale — the reigning Champion | Run completion |

### Map Generation

First column of each act is always a Rival Match. Subsequent columns use weighted random selection (Rival 40%, Shop 25%, Rumor 20%, Rest 15%), with no duplicate node types per column. Elites are placed at column 1, row 0 for acts 2+. Act 5 has a fixed boss node appended after the last column.

Named opponents per act:
- **Act 1**: Barnacle Bill, Salty Pete, Dock Rat, Fishy Meg
- **Act 2**: Merchant Luca, Trader Yun, Silk Marco, Coin Bianca
- **Act 3**: Guildmaster Voss, Lady Ashton, Baron Kell, Fixer Tomas
- **Act 4**: Cardinal Enzo, Sister Agatha, Spymaster Grey, Scholar Ruiz
- **Act 5**: The Champion

### Rest Site Actions (The Hearth)

- **Mend**: Remove one Burden (negative modifier)
- **Study**: Upgrade one ability to its enhanced version *(not yet implemented)*
- **Eavesdrop**: Reveal the next two opponents' personality archetype and ability loadout *(not yet implemented)*

---

## 2. Growth Axis 1: Ability Loadout

### How It Works

The player starts each run with **3 ability slots** from their chosen archetype (expandable to a maximum of 8 via Trinkets like Scholar's Tome). Each slot holds one ability type. Before each match, `MatchSetup` builds the 36-card deck and assigns ability tags based on both players' loadouts. A card drawn by either player retains its ability tag, but **only the player whose loadout assigned it can activate it** (tracked via `AbilityOwners` dictionary in `MatchConfig`).

### Ability Pool (16 Abilities)

12 active abilities + 4 passives, organized by role.

#### Attack Abilities (played during Attack phase)

| Ability | Effect | Rarity | Binding Count |
|---------|--------|--------|---------------|
| **Double Trouble** | Next attack ignores rank-match rule | Common | 3 cards |
| **Extra Draw** | Force defender to draw 2 cards before defending | Common | 3 cards |
| **Pile On** | +2 max attacks this bout | Uncommon | 2 cards |
| **Feint** | Play deck's top card as phantom attack; defender must beat it | Rare | 1 card |

#### Defense Abilities (played during Defense phase)

| Ability | Effect | Rarity | Binding Count |
|---------|--------|--------|---------------|
| **Blocker** | No more attacks can be added this bout | Common | 3 cards |
| **Double Defense** | This card covers two undefended attack slots | Uncommon | 2 cards |
| **Deflect** | Swap attacker/defender roles mid-bout | Rare | 1 card |
| **Slip Away** | Discard undefended attacks instead of eating them | Rare | 1 card |

#### Utility Abilities (either phase)

| Ability | Effect | Rarity | Binding Count |
|---------|--------|--------|---------------|
| **Trump Changer** | Change trump suit to this card's suit (once per match) | Common | 3 cards |
| **Seize Initiative** | You become attacker next bout | Uncommon | 2 cards |
| **Peek** | Reveal and rearrange top 3 cards of deck | Uncommon | 2 cards |
| **Gambit** | Discard entire hand, draw same count from deck | Rare | 1 card |

#### Passive Abilities (always active, no card binding, occupy a slot)

| Ability | Effect | Rarity |
|---------|--------|--------|
| **Trump Affinity** | When you draw a trump card, draw 1 additional card, then discard 1 | Uncommon |
| **Endgame Specialist** | When deck has 6 or fewer cards, defend with any suit (rank must still beat) | Uncommon |
| **Card Counter** | HUD shows count of each rank remaining in the deck | Uncommon |
| **Quick Hands** | After a successful all-defended bout, draw 1 extra during refill, then discard 1 | Rare |

### Ability Upgrades (Future)

Each ability has a planned upgrade available at Rest sites (Study action). Upgrades are not yet implemented but are designed:

| Ability | Upgrade Name | Upgraded Effect |
|---------|-------------|----------------|
| Double Trouble | Triple Threat | Ignores rank-match for 2 attacks |
| Extra Draw | Deluge | Force draw 3 |
| Pile On | Avalanche | +3 max attacks |
| Feint | Grand Feint | Two phantom attacks |
| Blocker | Iron Wall | Also auto-defends lowest undefended slot |
| Double Defense | Triple Guard | Covers three slots |
| Deflect | Riposte | Swap + you add one attack |
| Slip Away | Vanish | Discard ALL bout cards, even paired |
| Trump Changer | Trump Decree | Change trump AND draw 1 |
| Seize Initiative | Coup | Become attacker + opponent discards random card |
| Peek | Oracle | Top 5 cards instead of 3 |
| Gambit | Calculated Gambit | Draw +1 card |

### Binding Count Design

Binding count determines how many cards in the 36-card deck carry each ability:

- **Common (3 cards)**: Reliable — you'll likely see at least one per match. Individually weaker effects.
- **Uncommon (2 cards)**: Moderate reliability. Stronger effects worth building around.
- **Rare (1 card)**: Powerful but unreliable. Might not draw the one card that carries it.
- **Passive (0 cards)**: Always active, no draw luck. Occupies a slot without needing a card.

The Forger's Kit trinket upgrades Rare bindings from 1 to 2 cards, improving reliability of powerful abilities.

### Acquiring Abilities

| Source | Selection |
|--------|-----------|
| **Run start** | Archetype determines 3 starting abilities |
| **Match win** | Choose 1 of 3 randomly offered |
| **The Fence (shop)** | Buy specific abilities for Florins |
| **Rumor events** | Some grant or transform abilities |

If ability slots are full when gaining a new ability, the player must discard one to make room.

---

## 3. Growth Axis 2: Trinkets (Relic Equivalent)

Persistent passive modifiers that warp the rules of Durak itself. The player can hold up to **5 Trinkets** at once. All 21 trinkets are defined and integrated into the engine via `MatchSetup.ApplyTrinket` and `MatchConfig` flags.

### Economy Trinkets

| Trinket | Effect | Engine-Affecting |
|---------|--------|:---:|
| **The Merchant's Purse** | +3 Florins after each match | No |
| **The Miser's Ring** | +1 Florin per bout where you successfully defend | No |
| **Fool's Gold** | Start match with a temporary Gold Card (7 of trump suit) in hand | Yes |

### Hand Management Trinkets

| Trinket | Effect | Engine-Affecting |
|---------|--------|:---:|
| **The Tailor's Thimble** | Starting hand size is 5 instead of 6 | Yes |
| **The Juggler's Balls** | After the first bout, discard 1 card from hand for free | Yes |
| **Loaded Dice** | At match start, look at bottom 3 cards of deck and rearrange them | Yes |
| **The Courtier's Fan** | Once per match, draw 1 fewer during refill | Yes |

### Combat Trinkets

| Trinket | Effect | Engine-Affecting |
|---------|--------|:---:|
| **The Duelist's Glove** | First attack each bout ignores rank-match rule | Yes |
| **The Shield Brooch** | Once per match, auto-beat the first undefended attack | Yes |
| **Poisoned Wine** | When opponent eats, they draw 2 extra from deck | Yes |
| **The Spy's Monocle** | See the top card of the deck at all times (HUD display) | Yes |
| **Marked Deck** | At match start, reveal 3 random cards in opponent's hand (HUD display) | Yes |

### Trump Trinkets

| Trinket | Effect | Engine-Affecting |
|---------|--------|:---:|
| **The Alchemist's Stone** | Choose the trump suit at match start | Yes |
| **Crown of Thorns** | Trump cards played in defense count for next bout's rank-match | Yes |
| **The Heretic's Brand** | Opponent's trump cards treated as 1 rank lower for defense | Yes |

### Ability Trinkets

| Trinket | Effect | Engine-Affecting |
|---------|--------|:---:|
| **The Scholar's Tome** | +1 ability slot | No (meta-loop only) |
| **The Forger's Kit** | Abilities bound to 1 card are now bound to 2 | No (binding-time only) |
| **Quicksilver Vial** | Once per match, use an ability without discarding the card | Yes |
| **The Ventriloquist's Dummy** | Once per match, copy one opponent ability onto your unbound card | Yes |

### Risk/Reward Trinkets

| Trinket | Effect | Engine-Affecting |
|---------|--------|:---:|
| **The Devil's Bargain** | Draw 1 fewer starting card | Yes |
| **The Phoenix Medal** | Restore 1 Prestige once per run | No (meta-loop only) |

---

## 4. Challenge Scaling — "Rivals of the Renaissance"

### 4A. AI Personality Archetypes

Each AI opponent has a personality archetype that governs decision-making via `AIArchetypes.Apply()`. The archetype sets `RandomMoveChance` (base value per act) and `AbilityEagerness` (multiplier on ability usage probability).

| Archetype | Ability Eagerness | Behavior |
|-----------|:-:|----------|
| **The Brawler** | 1.5x | Aggressive — uses attack abilities eagerly, piles high |
| **The Miser** | 0.3x | Conservative — hoards abilities, only uses when losing |
| **The Fox** | 1.0x | Balanced — varies strategy, optimal ability timing |
| **The Noble** | 0.8x | Stylistic — moderate ability usage |
| **The Scholar** | 1.2x | Analytical — information abilities first, efficient defense |
| **The Assassin** | 0.5x | Patient — saves abilities for killing blows |

### 4B. Act Gating

Archetypes are introduced progressively:
- **Act 1**: Brawler, Miser
- **Act 2**: + Fox
- **Act 3**: Fox, Noble, Scholar
- **Act 4**: Scholar, Assassin, Fox
- **Act 5**: Fox, Assassin, Scholar (boss pool)

### 4C. Scaling Across Acts

| Dimension | Act 1 | Act 2 | Act 3 | Act 4 | Act 5 |
|-----------|:-----:|:-----:|:-----:|:-----:|:-----:|
| Opponent ability slots | 0 | 1 | 2 | 3 | 5 |
| Opponent Trinkets | 0 | 0-1 | 1-2 | 2-3 | 3-4 |
| AI random move chance | 35% | 20% | 10% | 4% | 0% |
| Elite bonus slots | +1 | +1 | +1 | +1 | +1 |

Ability rarity access scales with act: Commons only at Act 1, Uncommons from Act 2, Rares and Passives from Act 3+.

### 4D. House Rules (Elite and Boss Mechanics)

House Rules are temporary Durak rule modifications for a single match, set in `MatchConfig` via `MatchSetup.ApplyHouseRule`.

| House Rule | Effect | Config Field |
|------------|--------|-------------|
| **No Trumps Before Dusk** | Trump cards cannot be played in the first 3 bouts | `NoTrumpsUntilBout = 3` |
| **The Gauntlet** | Starting attacker never rotates | `FixedAttacker = true` |
| **Heavy Hands** | Starting hand size is 8 for both players | `HandSize = 8` |
| **Cutthroat** | Max 4 attacks, but any rank allowed | `MaxAttacksPerBout = 4, AnyRankAttack = true` |
| **Double or Nothing** | Defender draws extra when eating | `EatDrawsExtra = true` |
| **The Mirror** | Opponent copies your ability loadout | `MirrorAbilities = true` |

Boss matches use randomly-selected house rules from this pool.

---

## 5. Burdens (Negative Modifiers)

Acquired from match losses (random burden applied) and risky Rumor events. Removable at Rest sites (Mend) or bought off at shops (10 Florins).

| Burden | Effect | Engine-Affecting |
|--------|--------|:---:|
| **Rattled Nerves** | First defense each bout must use highest-rank card | Yes |
| **Heavy Purse** | Starting hand size is 7 instead of 6 | Yes |
| **Marked Cards** | Opponent can see 1 random card in your hand | No |
| **Clumsy Fingers** | Once per match, a random ability activation fizzles | Yes |
| **Bad Reputation** | Shop prices are 20% higher | No |

---

## 6. Economy

### Florins (Run Currency)

**Earned from matches:**

| Source | Amount |
|--------|--------|
| Base match win | 10 + (act * 2) |
| Elite bonus | +5 |
| Boss bonus | +10 |
| Match loss (consolation) | 2 |

**Spent at The Fence (shop):**

The shop offers 2 random abilities (from the full pool minus what the player already has), 1 random trinket, and burden removal if the player has burdens. Trinket offerings are weighted toward engine-affecting trinkets (15 weight vs 10 for non-engine).

| Item | Cost |
|------|------|
| Common ability | 8 Florins |
| Uncommon ability | 12 Florins |
| Rare ability | 18 Florins |
| Trinket | 15 Florins |
| Burden removal | 10 Florins |

### Prestige (Run HP)

The player starts with **7 Prestige**. Losing a match costs 1 Prestige **and** applies a random Burden. At 0 Prestige, the run ends.

Prestige recovery is deliberately scarce:
- The Phoenix Medal trinket restores 1 Prestige once per run

Each loss is doubly punishing (life + debuff) but the Burden is removable at Rest, creating meaningful Rest decisions.

### Reputation (Cross-Run Meta-Progression)

Accumulates forever, never spent. Persisted to `reputation.json` via Newtonsoft.Json.

**Earned per run:**

| Source | Amount |
|--------|--------|
| Per match won | +3 |
| Per act reached | +5 |
| Win bonus (beat the Boss) | +25 |

**Unlock thresholds:**

| Rep | Unlock |
|-----|--------|
| 25 | The Brute (starting archetype) |
| 100 | The Diplomat (starting archetype) |
| 300 | The Gambler (starting archetype) |

Additional unlock tiers (ability pool expansions, Ascension Mode, alternate bosses) are planned but not yet implemented.

---

## 7. Starting Archetypes

Each archetype defines a different starting ability loadout, encouraging a different playstyle. Unlocked via Reputation. Selected at run start via the ArchetypeSelect phase.

### The Rogue (Default — Always Available)
- **Starting abilities**: Blocker, Seize Initiative, Peek
- **Playstyle**: Reactive, information-focused. Defends well, then seizes the moment.

### The Brute (25 Reputation)
- **Starting abilities**: Double Trouble, Pile On, Extra Draw
- **Playstyle**: Aggressive overwhelm. Pile attacks high and draw deep.

### The Diplomat (100 Reputation)
- **Starting abilities**: Trump Changer, Deflect, Slip Away
- **Playstyle**: Redirection and trump control. Bend the rules in your favor.

### The Gambler (300 Reputation)
- **Starting abilities**: Gambit, Feint, Card Counter
- **Playstyle**: High risk, high reward. Bluff, feint, and read the table.

Note: Starting trinkets per archetype are designed (Juggler's Balls for Rogue, Duelist's Glove for Brute, Alchemist's Stone for Diplomat, Fool's Gold for Gambler) but not yet implemented.

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

1. **Result Screen**: Win/Loss, Florins earned. Ability pick (choose 1 of 3).
2. **Map View**: Branching path for the current act. Player selects next node.
3. **Shop/Event/Rest**: Context-appropriate panel based on selected node.
4. **Act Transition**: When all columns in an act are complete, advance to next act and generate new map.

---

## 10. Rumor Events (Sample — Not Yet Implemented)

Events are narrative encounters at Rumor nodes offering choices with mechanical consequences.

### "The Drunk Merchant"
*A wine-soaked trader slides a card across the table.*
- **Option A**: Pay 8 Florins for a random Uncommon ability
- **Option B**: Play a 1-bout mini-game. Win = free Rare ability. Lose = gain Burden "Heavy Purse."

### "The Forger's Offer"
*A hooded figure whispers about upgrading your tricks.*
- **Option A**: Upgrade one ability (free Study). Gain Burden "Clumsy Fingers."
- **Option B**: Decline. Gain 5 Florins.

### "The Cardinal's Confession"
*A nervous clergyman offers intelligence about your next opponent.*
- **Option A**: Pay 6 Florins. Reveal next opponent's full loadout + personality + House Rule.
- **Option B**: The Cardinal owes you a favor. Gain +2 Florins after each match in The Cardinal's Library.

### "The Pickpocket"
*A child darts past, fingers grazing your pouch.*
- **Option A**: Let them go. Lose 5 Florins. Gain 1 Prestige.
- **Option B**: Take their stolen goods. Gain random Trinket. Gain Burden "Bad Reputation."
- **Option C**: Recruit them. Gain Spy's Monocle. Lose 1 ability slot.

---

## 11. Implementation Architecture

### Relationship to Existing Codebase

The meta-loop wraps around the single-match engine without modifying its core logic. The boundary is clean:

- **Before a match**: `MatchSetup.Build()` takes `RunState` + `OpponentProfile` and produces a configured `MatchConfig`
- **During a match**: `GameEngine` runs with config-driven rules (abilities, trinkets, burdens, house rules)
- **After a match**: Read `WinnerIndex`/`FoolIndex`, update `RunState`, award rewards

### Runtime Types

| Type | Purpose | Status |
|------|---------|--------|
| `RunState` | Serializable run state (act, map, loadout, trinkets, burdens, florins, prestige) | Done |
| `RunManager` | MonoBehaviour state machine (ArchetypeSelect → MapSelect → InMatch → PostMatch → Shop/Event/Rest → RunOver) | Done |
| `MapGenerator` | Procedural branching map per act with weighted node types | Done |
| `MapNode` | Data for a single map node (type, opponent, position) | Done |
| `MatchSetup` | Configures MatchConfig from RunState + OpponentProfile + Random | Done |
| `MatchConfig` | All per-match configuration: abilities, trinkets, burdens, house rules | Done |
| `OpponentProfile` | Named opponent: archetype, abilities, trinkets, house rule, act/elite/boss flags | Done |
| `AIArchetype` | Enum + Apply() for personality-driven AI behavior (random chance, ability eagerness) | Done |
| `AbilityPool` | Static registry of all 16 ability definitions with rarity and binding counts | Done |
| `AbilityType` | Enum for all 16 abilities + extension methods (DisplayName, Description, IsPassive) | Done |
| `TrinketType` | Enum for all 21 trinkets + extension methods (DisplayName, Description, AffectsEngine) | Done |
| `BurdenType` | Enum for all 5 burdens + extension methods | Done |
| `HouseRuleType` | Enum for 6 house rules (None + 6 types) | Done |
| `Archetype` | 4 player archetypes with starting ability loadouts | Done |
| `RunSaveSystem` | JSON persistence for mid-run saves (auto-saves on phase transitions) | Done |
| `ReputationSystem` | Cross-run meta-progression (reputation tracking, archetype unlocks) | Done |
| `HudView` | Match HUD with SpysMonocle deck-top display and MarkedDeck info display | Done |

### UI Architecture

Single scene (`GameScene`) with panel state machine managed by `RunManager`. Built via `SceneBuilder` editor menu.

```
RunManager controls:
  ├── ArchetypePanel   — Starting archetype selection (rep-gated)
  ├── MapPanel         — Branching map view, node selection
  ├── ShopPanel        — Ability/trinket shop with burden removal
  ├── EventPanel       — Rumor events (placeholder — random rewards)
  ├── RestPanel        — Mend only (Study/Eavesdrop not yet implemented)
  ├── MatchPanel       — GameManager controls the match
  ├── ResultPanel      — Post-match results + ability reward selection
  └── RunOverPanel     — Run end screen with reputation earned
```

### Testing Infrastructure

- **Smoke Test**: Menu item `Wits and Fools/Smoke Test/Play 50 Games` — runs 50 deterministic games with greedy AIs. Healthy: 0 stalls, balanced wins, ~13 turns/game.
- **Batch Run**: `RunManager.StartBatchRun(n)` — runs N full roguelike loops at `Time.timeScale=20` with `AiThinkSeconds=0.02f`. Logs win rates, archetype distribution, ability/trinket stats per run.
- **Auto-Play**: `RunManager.StartAutoRun()` — single run without UI interaction. Press R to toggle. Auto-selects map nodes (Match > Elite > Rest > Rumor > Shop priority).

### Balance Tuning (Current State)

Based on batch testing with 50+ auto-play runs:
- **Run win rate (greedy AI)**: ~35%. Estimated 50-60% for human players.
- **Prestige**: 7 (gives enough runway for early-act losses)
- **Act 1 opponents**: 0 abilities, 35% random moves (very beatable)
- **Act 5 boss**: 5 abilities, 3-4 trinkets, 0% random (requires strong build)

### Implementation Status

| Phase | Description | Status |
|-------|-------------|--------|
| **Phase 1 — Core Data** | RunState, MapGenerator, MatchSetup, all enums/definitions | Complete |
| **Phase 2 — Engine Integration** | MatchConfig-driven rules, ability ownership, all 16 abilities, 6 AI archetypes, trinket/burden engine hooks | Complete |
| **Phase 3 — Run Flow** | RunManager state machine, save system, shop logic, archetype selection, auto-run | Complete |
| **Phase 4 — UI** | Map, shop, event, rest panels, HUD (SpysMonocle/MarkedDeck displays), archetype select screen | Complete (basic) |
| **Phase 5 — Content** | 16 abilities, 21 trinkets, 6 AI archetypes, 5 burdens, 6 house rules, named opponents | Complete (no upgrades, events are placeholder) |
| **Phase 6 — Meta-Progression** | Reputation system, archetype unlocks | Complete (Ascension/achievements not started) |

### Not Yet Implemented

- Ability upgrades (Study at rest sites)
- Eavesdrop rest action (scout future opponents)
- Narrative Rumor events (currently gives random rewards)
- Starting trinkets per archetype
- Ascension Mode (stackable difficulty modifiers)
- Achievements
- Alternate final boss (The Masquerade)
- Additional reputation unlock tiers (ability pool expansions, extra trinkets)
- Pre-match loadout review screen
- Spending bonus (free Common after 20+ Florins at shop)
- Opponent intel purchase at shop
