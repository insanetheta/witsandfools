# Balatro: Core Gameplay Loop Analysis

Research reference for the Wits and Fools roguelike meta-loop design.

---

## 1. Core Battle Loop

A single "hand" in Balatro works as follows: the player is dealt 8 cards (default hand size) from a standard 52-card deck. They select up to 5 cards to play as a poker hand. The scoring formula is always:

**Score = Chips x Mult**

Each poker hand type has a base Chip value and a base Mult value. A level-1 Pair gives 10 base chips and 2 mult, while a level-1 Straight gives 30 chips and 4 mult. Harder-to-assemble hands yield higher base values on both axes.

After the base values are set, each individual scoring card adds its rank value in chips (e.g., a King adds 10 chips, an Ace adds 11). Then Joker effects, card enhancements, editions, and seals trigger in left-to-right order, adding +Chips, +Mult, or xMult modifiers. **Order matters enormously**: additive mult is applied first, then multiplicative mult multiplies the running total, so the sequence of Jokers in your slots directly affects the final number.

Within a single blind, the player has a limited number of **hands** (default 4) to meet the required score, and a limited number of **discards** (default 3) to cycle through their deck looking for better combinations. Each played hand's score accumulates toward the blind's target. If the cumulative score meets or exceeds the target before hands run out, the blind is defeated.

---

## 2. Run Structure

A full run spans **8 Antes**. Each Ante contains three blinds played in sequence:

- **Small Blind**: Requires 1x the Ante's base score. Can be skipped for a Tag reward.
- **Big Blind**: Requires 1.5x the base score. Can also be skipped for a Tag.
- **Boss Blind**: Requires 2x the base score and imposes a unique debuff. Cannot be skipped.

Base score values scale steeply:

| Ante | Base Score | Boss Blind Target |
|------|-----------|-------------------|
| 1 | 300 | 600 |
| 2 | 800 | 1,600 |
| 3 | 2,000 | 4,000 |
| 4 | 5,000 | 10,000 |
| 5 | 11,000 | 22,000 |
| 6 | 20,000 | 40,000 |
| 7 | 35,000 | 70,000 |
| 8 | 50,000 | 100,000 |

Defeating the Ante 8 Boss Blind wins the run. After Ante 8, the player may optionally continue into **Endless Mode** with exponential scaling.

After each blind (except the Boss), the player visits **The Shop**: up to 2 Joker/consumable cards for purchase, 2 Booster Packs, and 1 Voucher. Players can reroll for $5 (increasing by $1 per reroll within a single visit).

---

## 3. Growth Mechanics

### Jokers (Primary Build-Defining Axis)

The backbone of every build. The player has **5 Joker slots** (modifiable via vouchers). There are **150 Jokers** spanning categories:

- **Additive Mult**: Flat mult bonuses for meeting conditions
- **Multiplicative Mult**: xMult bonuses (exponentially powerful)
- **Chip Generation**: Flat chip bonuses
- **Economy**: Generate gold, increase interest caps
- **Retrigger**: Cause scoring cards to trigger multiple times
- **Conditional**: Activate based on specific hand types, suits, or game states

Because slots are scarce, choosing which 5 Jokers to hold (and in what left-to-right order) is the central strategic decision. Selling a Joker to make room for a better one is a recurring tension.

### Planet Cards (Hand Type Specialization)

Consumables that permanently level up a specific poker hand type for the rest of the run. Each level adds base chips and base mult. No level cap. A focused strategy might level Flush 8-9 times, making it vastly more powerful than other hand types. This is how the player **specializes their "damage type."**

### Tarot Cards (Deck Sculpting)

Consumables that modify individual playing cards: change suit (enabling flushes), change rank, or apply **Enhancements** (Mult Card, Glass Card, Steel Card, Gold Card, Wild Card, Stone Card, Lucky Card). Enhanced cards gain additional scoring properties. Tarots are the primary tool for sculpting the deck toward a specific strategy.

### Spectral Cards (High-Risk Modifications)

Rare consumables making dramatic changes: destroying cards, adding powerful editions (Polychrome, Holographic, Foil), creating copies. Often carry a cost (destroy a card, reduce hand size) attached to a powerful effect.

### Vouchers (Permanent Rule Changes)

32 total (16 base at $10, each unlocking an upgraded version at $10). Modify systemic rules: extra Joker slots, extra hand size, cheaper rerolls, more shop offerings, increased interest caps. Bought from the shop, refreshed after each Boss Blind.

### Booster Packs

5 types: Arcana (Tarots), Celestial (Planets), Spectral, Buffoon (Jokers), Standard (playing cards with enhancements). Each has Normal, Jumbo, and Mega variants.

---

## 4. Challenge Scaling

The score curve is exponential: Ante 1's 300 base to Ante 8's 50,000 represents a ~167x increase. Higher Stake difficulties further inflate requirements.

### Boss Blinds

28 total (23 regular, 5 Finishers). Debuff categories:

- **Card debuffs**: The Plant debuffs all face cards; The Window debuffs all Diamonds; The Pillar debuffs cards already played this ante
- **Hand restrictions**: The Mouth forces one poker hand type for the entire round; The Manacle reduces hand size by 1
- **Information denial**: The House draws initial cards face-down; The Mark marks face cards face-down
- **Scoring interference**: The Needle gives only 1 hand to play; The Water gives 0 discards

Boss debuffs force adaptability. A build relying on face cards crumbles against The Plant. A flush build struggles against suit-debuffing bosses. **No single strategy guarantees a win.**

---

## 5. Economy

Three income sources:
1. **Base payout** for winning each blind (scales with difficulty)
2. **Bonus dollars** for remaining hands
3. **Interest**: $1 per $5 held, up to $5 cap (i.e., $25+ earns max interest)

Vouchers can raise the interest cap: Seed Money to $10 ($50 held), Money Tree to $20 ($100 held).

**Core tension: spending vs. saving.** Every dollar spent drops you below an interest threshold. The optimal play is often to buy only what you need to survive the next blind and protect your interest breakpoint.

Shop rerolls cost $5+ (incrementing per reroll), making fishing for specific cards expensive. Economy Jokers (like To The Moon) can generate $13-40/round when stacked, enabling aggressive late-game shopping.

---

## 6. Build Identity and Replayability

Build identity emerges from the intersection of:

1. **Which Jokers you find** (random offerings + limited slots = hard choices)
2. **Which hand types you invest in** (Planet cards specialize your "damage type")
3. **How you sculpt your deck** (Tarots modify suits/ranks, Spectrals add editions)
4. **Skip vs. play decisions** (skipping Small/Big Blinds sacrifices money for Tags)

Example builds that play completely differently:
- "Steel Flush" (Steel-enhanced cards + Flush-focused Jokers)
- "Mult Pair" (pair-focused with additive mult stacking)
- "Five of a Kind with Polychrome" (duplicate cards + multiplicative Jokers)

Since offerings are randomized, every run forces adaptation. The player must read what the run offers and commit to a strategy rather than forcing a predetermined plan.

---

## Key Design Takeaways for Wits and Fools

1. **Exponential scoring creates satisfying power expression** — even without HP, finding ways to make the player feel exponentially stronger is important
2. **Scarce slots force meaningful choices** — 5 Joker slots out of 150 options is the sweet spot
3. **Multiple growth axes compound** — Jokers, Planets, Tarots, and Vouchers each serve a different role but multiply together
4. **Boss mechanics force adaptation** — prevents any single strategy from dominating
5. **Economy tension (save vs spend)** adds a parallel strategic layer
6. **Left-to-right ordering** (Joker sequence matters) adds surprising depth from a simple mechanic
7. **The shop is where strategy happens** — the between-combat decision space is as important as combat itself
