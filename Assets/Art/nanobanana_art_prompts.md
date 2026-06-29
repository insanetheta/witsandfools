# Wits and Fools — NanoBanana Art Prompt Guide

## Overview

This document contains AI image generation prompts for all game art assets in Wits and Fools, a Renaissance Italian card game roguelike. Each prompt is tuned for NanoBanana (Gemini Flash Image) with the correct aspect ratio for its Unity template slot.

**Visual Style (apply to ALL prompts):** Renaissance oil painting, Caravaggio chiaroscuro, warm candlelight, rich earth tones, gold accents. Dark moody backgrounds. Painterly brushwork visible. 16th-century Italian aesthetic. No modern elements, no fantasy magic glows, no anime. Think Rembrandt meets heist-film glamour.

---

## Asset Dimensions Reference

| Asset Type | Source Size | Aspect Ratio | Unity Display | Notes |
|---|---|---|---|---|
| Card character art | 512×768 px | 2:3 | 110×160 in hand | Portrait orientation, character fills upper 60% |
| Opponent portrait | 1408×768 px | ~16:9 | 66×66 (center crop) | Half-bust, face centered, dark BG with venue hints |
| Relic/trinket icon | 512×512 px | 1:1 | ~48×48 in UI | Single object on dark velvet, museum-lit |
| Card back | 512×768 px | 2:3 | 110×160 | Ornate repeating pattern, jester/fox emblem |
| Venue background | 1408×768 px | ~16:9 | Full screen | Atmospheric environment, no characters |
| Map node icon | 96×96 px | 1:1 | 48×48 on map | Simple iconic silhouette |

---

## SECTION 1: CARD CHARACTER ART (99 cards)

Each card depicts a single Renaissance character that embodies the card name and ability. Characters should look like they belong in a specific social stratum matching their doctrine:
- **Schemer**: Shadows, intelligence networks, coded letters, dark cloaks, monocles
- **Brute**: Armor, weapons, scars, military camps, siege equipment
- **Trickster**: Masks, theatrical costumes, carnival setting, sleight-of-hand gestures
- **Hoarder**: Coin purses, ledgers, scales, warehouses, merchant attire
- **Neutral**: Court finery, heraldic symbols, formal poses, generic noble bearing

**Rarity affects visual complexity:**
- Common: Simpler composition, one character, plain background
- Uncommon: More detail, secondary props, richer textures
- Rare: Most elaborate — dramatic lighting, ornate clothing, gilded props, maximum visual impact

### Format for each prompt:
> **[card_id]** — *Card Name* (Rank Suit, Doctrine, Rarity, Ability)
> `prompt text for nanobanana`

---

### SCHEMER DOCTRINE (22 cards)

**schemer_informant** — *The Informant* (7♠, Schemer, Common, Riposte)
`Renaissance oil painting, portrait of a lean sharp-featured man in dark leather vest whispering over a tavern table, one hand concealing a folded note, candlelight catching his knowing smirk, Caravaggio chiaroscuro, warm amber tones, dark wood-paneled tavern background, 16th century Italian, 2:3 aspect ratio`

**schemer_spyglass** — *The Spyglass* (6♦, Schemer, Common, SleightOfHand)
`Renaissance oil painting, portrait of a young woman peering through a small brass telescope from a shadowed balcony, one eye magnified, wearing a dark hood over merchant clothes, Caravaggio dramatic side-lighting, warm candlelight, Venetian rooftops barely visible in darkness, 2:3 aspect ratio`

**schemer_cipher** — *The Cipher* (8♣, Schemer, Common, Peek)
`Renaissance oil painting, portrait of a hooded scholar hunched over an encoded letter, quill in hand, cipher wheel beside them on the desk, candlelight illuminating the parchment and their intense focused eyes, dark library alcove, Caravaggio chiaroscuro, 2:3 aspect ratio`

**schemer_cartographer** — *The Cartographer* (6♥, Schemer, Uncommon, Peek)
`Renaissance oil painting, portrait of an older woman with spectacles bent over an intricate city map, compass and dividers in hand, scattered rolled maps and ink bottles, warm candlelight from brass candelabra, richly detailed study with dark wood shelving, Caravaggio lighting, 2:3 aspect ratio`

**schemer_forger** — *The Forger* (7♥, Schemer, Rare, Peek)
`Renaissance oil painting, portrait of a meticulous craftsman with magnifying loupe examining a freshly forged seal, wax sticks and blank documents spread before him, multiple candles casting overlapping warm shadows, ornate workshop with shelves of inks and stamps, dramatic Caravaggio lighting, elaborate detail, 2:3 aspect ratio`

**schemer_librarian** — *The Librarian* (6♣, Schemer, Uncommon, SteadyHand)
`Renaissance oil painting, portrait of a stern elderly woman in dark robes standing before towering bookshelves, one hand resting on an open tome, reading glasses on a chain, single candle casting dramatic shadows across leather-bound spines, cathedral library interior, Caravaggio chiaroscuro, 2:3 aspect ratio`

**schemer_interceptor** — *The Interceptor* (10♠, Schemer, Uncommon, SeizeInitiative)
`Renaissance oil painting, portrait of a swift dark-cloaked figure mid-stride catching a sealed letter from the air, rapier at hip, wind-blown cloak, moonlit cobblestone alley with a single lantern, dramatic motion blur in the cape, Caravaggio lighting, tense urgency, 2:3 aspect ratio`

**schemer_double_agent** — *The Double Agent* (8♥, Schemer, Common, DoubleAgent)
`Renaissance oil painting, portrait of a charming young man with two-toned doublet holding a card in each hand behind his back, sly half-smile, candlelit tavern booth with two empty chairs suggesting dual allegiance, warm Caravaggio lighting, 2:3 aspect ratio`

**schemer_poisoner** — *The Poisoner* (6♠, Schemer, Common, Riposte)
`Renaissance oil painting, portrait of a pale woman in dark velvet carefully tilting a tiny vial over a wine goblet, steady hands, poison-green liquid catching candlelight, dark dining room with a single taper candle, Caravaggio dramatic shadows, 2:3 aspect ratio`

**schemer_scribe** — *The Scribe* (7♦, Schemer, Common, SleightOfHand)
`Renaissance oil painting, portrait of a young clerk with ink-stained fingers writing rapidly with a feathered quill, stack of documents and sealed letters, candlelight on parchment, cramped candlelit office alcove, Caravaggio chiaroscuro, 2:3 aspect ratio`

**schemer_envoy** — *The Envoy* (10♥, Schemer, Common, ResourceGain)
`Renaissance oil painting, portrait of a well-dressed diplomat in a crimson doublet presenting a sealed scroll, confident posture, gold chain of office, warm torchlit corridor of a palace, Caravaggio lighting, polished marble floor reflecting light, 2:3 aspect ratio`

**schemer_inquisitor** — *The Inquisitor* (7♣, Schemer, Uncommon, Riposte)
`Renaissance oil painting, portrait of a stern man in black clerical robes with a silver cross, piercing gaze, one hand raised as if questioning, candlelit stone interrogation chamber with heavy wooden chair, Caravaggio dramatic chiaroscuro, oppressive shadows, 2:3 aspect ratio`

**schemer_mastermind** — *The Mastermind* (K♠, Schemer, Rare, SeizeInitiative)
`Renaissance oil painting, portrait of a commanding silver-haired man seated in a high-backed chair, chess pieces and a city map spread before him, steepled fingers, multiple candles casting complex shadows, opulent dark study with velvet curtains, Caravaggio lighting, maximum detail and grandeur, 2:3 aspect ratio`

**schemer_cryptanalyst** — *The Cryptanalyst* (Q♦, Schemer, Uncommon, ResourceGain)
`Renaissance oil painting, portrait of a woman surrounded by mathematical instruments and coded documents, astrolabe and cipher discs on desk, quill poised mid-calculation, intense concentration, candlelit scholar's study, brass instruments gleaming, Caravaggio lighting, 2:3 aspect ratio`

**schemer_puppeteer** — *The Puppeteer* (8♦, Schemer, Rare, Blackmail)
`Renaissance oil painting, portrait of an elegant figure in dark silk manipulating marionette strings from above, small puppet figures of nobles dangling below their fingers, theatrical stage curtain behind, dramatic Caravaggio side-lighting, sinister smile, elaborate costume detail, 2:3 aspect ratio`

**schemer_archivist** — *The Archivist* (9♣, Schemer, Rare, StackTheDeck)
`Renaissance oil painting, portrait of an ancient keeper surrounded by towering stacks of bound ledgers, one hand pulling a specific document from a perfectly organized shelf, knowing expression, dusty candlelit archive vault with iron-bound chests, Caravaggio chiaroscuro, rich textures, 2:3 aspect ratio`

**schemer_ambassador** — *The Ambassador* (A♥, Schemer, Uncommon, Blocker)
`Renaissance oil painting, portrait of a dignified diplomat in elaborate crimson and gold robes, one hand raised in a calming gesture, diplomatic seal visible on breast, grand candlelit embassy hall with flags of multiple city-states, Caravaggio lighting, 2:3 aspect ratio`

**schemer_shadow_courier** — *The Shadow Courier* (8♠, Schemer, Uncommon, Peek)
`Renaissance oil painting, portrait of a cloaked figure emerging from darkness clutching a leather satchel, face half-hidden by deep hood, moonlight through narrow alley, cobblestones wet with rain, Caravaggio extreme chiaroscuro, sense of urgency, 2:3 aspect ratio`

**schemer_venetian_mirror** — *The Venetian Mirror* (10♦, Schemer, Common, SleightOfHand)
`Renaissance oil painting, portrait of a masked figure in an ornate Venetian room, reflected differently in a gilded mirror behind them, the reflection showing their true calculating expression, warm candlelight, Caravaggio dramatic shadows, 2:3 aspect ratio`

**schemer_doge_spymaster** — *The Doge's Spymaster* (K♣, Schemer, Rare, Riposte)
`Renaissance oil painting, portrait of an older man in rich dark robes seated before a web of connected notes and red string on a wall, multiple sealed letters on desk, gold signet ring, dim candlelight illuminating only his face and the conspiracy board, Caravaggio lighting, elaborate detail, 2:3 aspect ratio`

**schemer_grand_inquisitor** — *The Grand Inquisitor* (A♣, Schemer, Rare, Masterstroke)
`Renaissance oil painting, portrait of a towering figure in black and gold ecclesiastical vestments, holding a massive leather-bound book of secrets, piercing eyes beneath a wide-brimmed hat, cathedral candlelight casting dramatic upward shadows, Caravaggio extreme chiaroscuro, maximum grandeur, 2:3 aspect ratio`

---

### BRUTE DOCTRINE (21 cards)

**brute_condottiero** — *The Condottiero* (8♣, Brute, Common, Conquer)
`Renaissance oil painting, portrait of a battle-scarred mercenary captain in dented plate armor, sword resting on shoulder, confident grin, military camp with tents in dark background, warm firelight, Caravaggio chiaroscuro, 2:3 aspect ratio`

**brute_siege_ram** — *The Battering Ram* (J♠, Brute, Common, ResourceGain)
`Renaissance oil painting, portrait of a massive bearded man gripping a heavy iron-capped battering ram, muscles straining, siege warfare background with fortress walls, firelight and torches, Caravaggio dramatic lighting, 2:3 aspect ratio`

**brute_berserker** — *The Berserker* (7♥, Brute, Common, DoubleTrouble)
`Renaissance oil painting, portrait of a wild-eyed warrior with matted hair and bare chest covered in scars, twin axes raised, blood-spattered, dark battlefield smoke behind, Caravaggio extreme chiaroscuro, raw intensity, 2:3 aspect ratio`

**brute_enforcer** — *The Enforcer* (8♦, Brute, Uncommon, Intimidate)
`Renaissance oil painting, portrait of a hulking man in leather armor cracking his knuckles, cold dead eyes, intimidating stance, dark alley with single torch, chain wrapped around one fist, Caravaggio dramatic shadows, menacing presence, 2:3 aspect ratio`

**brute_warmonger** — *The Warmonger* (Q♠, Brute, Common, Intimidate)
`Renaissance oil painting, portrait of a stern general in polished dark armor studying a battle map, sword at hip, military tent interior with campaign flags, warm lamplight, Caravaggio chiaroscuro, commanding authority, 2:3 aspect ratio`

**brute_pit_fighter** — *The Pit Fighter* (7♣, Brute, Uncommon, Conquer)
`Renaissance oil painting, portrait of a scarred gladiator in leather straps and iron bracers, blood on his hands, standing in a candlelit underground fighting pit, crowd shadows above, Caravaggio dramatic uplighting, visceral intensity, 2:3 aspect ratio`

**brute_cannon_master** — *The Cannon Master* (6♦, Brute, Rare, Blocker)
`Renaissance oil painting, portrait of a soot-covered artillery officer beside a bronze cannon, match cord smoldering in hand, cannonballs stacked nearby, fortress rampart at dusk, smoke and firelight, Caravaggio lighting, detailed military hardware, 2:3 aspect ratio`

**brute_mercenary_captain** — *The Mercenary Captain* (8♥, Brute, Uncommon, DoubleTrouble)
`Renaissance oil painting, portrait of a weathered mercenary leader in mismatched fine armor looted from kills, two swords crossed behind him, counting coins with one hand, torchlit camp, Caravaggio chiaroscuro, confident swagger, 2:3 aspect ratio`

**brute_sapper** — *The Sapper* (6♣, Brute, Common, Riposte)
`Renaissance oil painting, portrait of a dirt-covered soldier in a cramped tunnel holding a pickaxe and lantern, determined expression, earth walls close around him, single warm lantern glow, Caravaggio extreme chiaroscuro, claustrophobic, 2:3 aspect ratio`

**brute_gladiator** — *The Gladiator* (K♥, Brute, Uncommon, ResourceGain)
`Renaissance oil painting, portrait of a champion gladiator in polished bronze armor with laurel crown, trident and net in hand, victorious pose, colosseum torchlight, adoring crowd shadows, Caravaggio dramatic lighting, heroic grandeur, 2:3 aspect ratio`

**brute_pillager** — *The Pillager* (J♦, Brute, Common, Rampage)
`Renaissance oil painting, portrait of a rough raider carrying a sack of looted goods over one shoulder, torch in other hand, burning village reflected in his eyes, Caravaggio firelight chiaroscuro, destructive energy, 2:3 aspect ratio`

**brute_warlord** — *The Warlord* (A♠, Brute, Uncommon, Conquer)
`Renaissance oil painting, portrait of an imposing warlord in full battle plate seated on a campaign throne, conquered banners at his feet, dark tent with war trophies, multiple torch flames, Caravaggio chiaroscuro, dominating presence, 2:3 aspect ratio`

**brute_battering_shield** — *The Shield Breaker* (Q♣, Brute, Rare, ResourceGain)
`Renaissance oil painting, portrait of a massive warrior mid-swing smashing through a wooden shield with a war hammer, splinters flying, opponent's broken shield fragments, dramatic torchlight arena, Caravaggio extreme lighting, power and destruction, 2:3 aspect ratio`

**brute_vanguard** — *The Vanguard* (6♥, Brute, Common, Conquer)
`Renaissance oil painting, portrait of a young soldier in chain mail holding a tall pike, first in formation, determined forward gaze, misty battlefield dawn, warm torch from behind, Caravaggio chiaroscuro, youthful courage, 2:3 aspect ratio`

**brute_arsonist** — *The Arsonist* (7♦, Brute, Common, Intimidate)
`Renaissance oil painting, portrait of a grinning figure in charred leather holding a lit torch, face lit from below by flames, burning building reflected in dark eyes, Caravaggio extreme uplighting, unsettling warmth, 2:3 aspect ratio`

**brute_iron_duke** — *The Iron Duke* (J♠, Brute, Rare, BattleHardened)
`Renaissance oil painting, portrait of a grizzled veteran duke in blackened iron armor covered in dents and scratches, grey beard, battle-worn face showing decades of warfare, dark throne room with weapons mounted on walls, Caravaggio dramatic candlelight, maximum detail on armor damage, 2:3 aspect ratio`

**brute_siege_tower** — *The Siege Engineer* (8♠, Brute, Rare, Conquer)
`Renaissance oil painting, portrait of a brilliant military engineer with blueprints for siege engines spread before him, compass and ruler in hand, wooden siege tower model on desk, workshop with pulleys and gears in background, Caravaggio warm lamplight, intricate detail, 2:3 aspect ratio`

**brute_quartermaster** — *The Quartermaster* (9♣, Brute, Common, ResourceGain)
`Renaissance oil painting, portrait of a stout supply officer inventorying weapons and provisions, quill and ledger in hand, surrounded by crates and barrels in a torchlit supply depot, Caravaggio chiaroscuro, orderly chaos, 2:3 aspect ratio`

**brute_executioner** — *The Executioner* (9♥, Brute, Rare, Conquer)
`Renaissance oil painting, portrait of a hooded executioner in black, massive axe resting on the ground beside him, arms crossed, stone execution chamber with single high window beam of light, Caravaggio extreme chiaroscuro, ominous stillness, elaborate costume detail, 2:3 aspect ratio`

**brute_titan** — *The Titan* (A♦, Brute, Rare, Onslaught)
`Renaissance oil painting, portrait of an enormous warrior in masterwork gilded plate armor, great sword planted in ground, towering over the viewer, multiple torches and braziers casting upward shadows, battlefield smoke, Caravaggio dramatic lighting, maximum scale and grandeur, 2:3 aspect ratio`

**brute_war_elephant** — *The War Elephant* (K♦, Brute, Uncommon, DoubleTrouble)
`Renaissance oil painting, portrait of an armored war elephant handler standing beside his massive mount, elephant fitted with plate barding and howdah, military camp with fires, Caravaggio warm firelight, exotic and imposing, 2:3 aspect ratio`

---

### TRICKSTER DOCTRINE (21 cards)

**trickster_courtesan** — *The Courtesan* (7♥, Trickster, Common, TrumpChanger)
`Renaissance oil painting, portrait of an alluring woman in a low-cut velvet gown holding a fan that conceals half her face, knowing eyes above the fan, candlelit boudoir with silk drapes, Caravaggio warm chiaroscuro, seductive mystery, 2:3 aspect ratio`

**trickster_alchemist** — *The Alchemist* (6♦, Trickster, Uncommon, TrumpChanger)
`Renaissance oil painting, portrait of an eccentric alchemist surrounded by bubbling flasks and colored liquids, one hand pouring a transformation potion, wild hair, crowded laboratory with candles and retorts, Caravaggio warm lighting, curious intensity, 2:3 aspect ratio`

**trickster_jester** — *The Jester* (6♣, Trickster, Common, DoubleTrouble)
`Renaissance oil painting, portrait of a court jester in motley with bells on his cap, mischievous grin, juggling two cards, candlelit feast hall, Caravaggio chiaroscuro, playful chaos, 2:3 aspect ratio`

**trickster_mask_maker** — *The Mask Maker* (8♥, Trickster, Uncommon, TrumpChanger)
`Renaissance oil painting, portrait of a craftsman surrounded by Venetian carnival masks in various stages of completion, painting a golden mask, workshop with mask molds and feathers, warm candlelight reflecting off lacquered surfaces, Caravaggio lighting, rich detail, 2:3 aspect ratio`

**trickster_illusionist** — *The Illusionist* (7♠, Trickster, Common, Blocker)
`Renaissance oil painting, portrait of a mysterious figure producing smoke from empty hands, audience in shadow, theatrical stage with velvet curtain, single spotlight candle from above, Caravaggio extreme chiaroscuro, wonder and deception, 2:3 aspect ratio`

**trickster_charlatan** — *The Charlatan* (9♦, Trickster, Common, Riposte)
`Renaissance oil painting, portrait of a fast-talking street vendor in a feathered cap selling dubious miracle cures from a cart, animated hand gestures, gullible crowd shadows, market torch light, Caravaggio chiaroscuro, charming dishonesty, 2:3 aspect ratio`

**trickster_fortune_teller** — *The Fortune Teller* (8♣, Trickster, Uncommon, TrapCard)
`Renaissance oil painting, portrait of an enigmatic fortune teller gazing into a crystal ball, tarot cards spread on velvet table, brass incense burner smoking, dark tent interior with silk hangings, Caravaggio dramatic candlelight from below the crystal ball, mystic atmosphere, 2:3 aspect ratio`

**trickster_pickpocket** — *The Pickpocket* (6♠, Trickster, Rare, DoubleAgent)
`Renaissance oil painting, portrait of a nimble young thief with quick fingers lifting a coin purse, crowd of oblivious nobles in background, market setting, the thief looking directly at the viewer with a conspiratorial wink, Caravaggio lighting on the hands, elaborate street scene detail, 2:3 aspect ratio`

**trickster_card_sharp** — *The Card Sharp* (10♦, Trickster, Uncommon, ResourceGain)
`Renaissance oil painting, portrait of a card cheat at a gaming table palming an extra card, one hand on the table one hidden, cool confident expression, other players in shadow, candlelit gambling den, Caravaggio dramatic hand lighting, tension, 2:3 aspect ratio`

**trickster_smuggler** — *The Smuggler* (10♣, Trickster, Uncommon, Brace)
`Renaissance oil painting, portrait of a cloaked figure loading contraband into a hidden compartment of a trunk, dock warehouse at night, rope coils and cargo crates, single lantern, Caravaggio extreme chiaroscuro, secretive urgency, 2:3 aspect ratio`

**trickster_acrobat** — *The Acrobat* (6♥, Trickster, Uncommon, SeizeInitiative)
`Renaissance oil painting, portrait of a lithe performer in colorful tight costume balanced on one hand, carnival tent interior, audience torches below, dynamic pose defying gravity, Caravaggio dramatic uplighting, grace and danger, 2:3 aspect ratio`

**trickster_doppelganger** — *The Doppelganger* (8♠, Trickster, Rare, Fortify)
`Renaissance oil painting, portrait showing two identical figures facing each other across a candlelit mirror, one real one reflected but subtly different, dark chamber with a single ornate mirror, Caravaggio chiaroscuro, unsettling doubling, elaborate mirror frame detail, 2:3 aspect ratio`

**trickster_venetian_noble** — *The Venetian Noble* (Q♥, Trickster, Common, TrumpChanger)
`Renaissance oil painting, portrait of a Venetian noblewoman in elaborate carnival costume and half-mask, holding a peacock feather fan, grand ballroom with chandeliers in background, Caravaggio warm candlelight, elegant deception, 2:3 aspect ratio`

**trickster_con_artist** — *The Con Artist* (8♦, Trickster, Uncommon, Peek)
`Renaissance oil painting, portrait of a well-dressed swindler showing a rigged shell game, three cups and a hidden ball, charming smile, market square at dusk with torch light, Caravaggio focused lighting on the hands and cups, street-level cunning, 2:3 aspect ratio`

**trickster_escape_artist** — *The Escape Artist* (7♣, Trickster, Rare, SlipAway)
`Renaissance oil painting, portrait of a figure slipping free from heavy iron chains, one hand already free and reaching for the lock, prison cell with barred window, single beam of moonlight, Caravaggio extreme chiaroscuro, desperate ingenuity, elaborate chain and lock detail, 2:3 aspect ratio`

**trickster_puppet_master** — *The Carnival Master* (K♠, Trickster, Rare, Equilibrium)
`Renaissance oil painting, portrait of the ringmaster of a Venetian carnival in extravagant costume, top hat with peacock feathers, arms spread wide commanding a theatrical spectacle, carnival lights and performers in shadow behind, Caravaggio dramatic lighting, maximum showmanship and detail, 2:3 aspect ratio`

**trickster_mirror_mage** — *The Mirror Mage* (K♦, Trickster, Rare, ResourceGain)
`Renaissance oil painting, portrait of a mysterious figure surrounded by angled mirrors reflecting different playing card suits, light bouncing between mirrors creating prismatic effects, dark chamber, Caravaggio dramatic multi-directional lighting, elaborate mirror and glass detail, 2:3 aspect ratio`

**trickster_maestro** — *The Maestro* (A♠, Trickster, Uncommon, Blocker)
`Renaissance oil painting, portrait of an orchestra conductor in formal black coat directing invisible musicians, baton raised, intense focused expression, candlelit concert hall with balconies in shadow, Caravaggio side lighting, commanding presence, 2:3 aspect ratio`

**trickster_mountebank** — *The Mountebank* (Q♣, Trickster, Uncommon, BlindSwap)
`Renaissance oil painting, portrait of a theatrical quack doctor on a raised platform selling miracle elixirs, elaborate costume with cape, bottles of colored liquid, gullible crowd below in torchlight, Caravaggio dramatic stage lighting, bravado, 2:3 aspect ratio`

**trickster_grand_masquerader** — *The Grand Masquerader* (K♥, Trickster, Rare, Masquerade)
`Renaissance oil painting, portrait of a figure wearing the most elaborate Venetian carnival mask ever crafted, encrusted with jewels and gold leaf, full masked ball costume, grand Venetian palazzo ballroom with Murano glass chandeliers, Caravaggio warm golden lighting, peak opulence and mystery, 2:3 aspect ratio`

**trickster_contessa** — *The Contessa* (A♥, Trickster, Rare, Blocker)
`Renaissance oil painting, portrait of a powerful noblewoman in black and silver court dress, imperious gaze, one raised hand commanding silence, dark throne room with single candelabra, Caravaggio extreme chiaroscuro, absolute authority and elegance, maximum costume detail, 2:3 aspect ratio`

---

### HOARDER DOCTRINE (21 cards)

**hoarder_miser** — *The Miser* (7♦, Hoarder, Common, SteadyHand)
`Renaissance oil painting, portrait of a gaunt old man clutching a coin purse protectively to his chest, suspicious eyes, dim candlelit counting room with locked chest, Caravaggio chiaroscuro, Rembrandt-like miserly anxiety, 2:3 aspect ratio`

**hoarder_collector** — *The Collector* (6♥, Hoarder, Common, ThickSkin)
`Renaissance oil painting, portrait of a round-faced enthusiast examining a rare curio through a jeweler's loupe, cabinet of curiosities behind them with shells, minerals, dried specimens, warm candlelight, Caravaggio lighting, absorbed fascination, 2:3 aspect ratio`

**hoarder_merchant** — *The Merchant* (10♦, Hoarder, Uncommon, ExtraDraw)
`Renaissance oil painting, portrait of a prosperous merchant in a fur-trimmed robe weighing gold on a balance scale, ledger open beside him, richly appointed counting house with tapestries, Caravaggio warm candlelight, prosperity and calculation, 2:3 aspect ratio`

**hoarder_packrat** — *The Packrat* (6♣, Hoarder, Common, Patronage)
`Renaissance oil painting, portrait of a figure buried among tottering stacks of collected objects, barely visible among the hoard, one hand reaching to add another item, cramped attic storage room, single candle, Caravaggio chiaroscuro, compulsive accumulation, 2:3 aspect ratio`

**hoarder_warehouse_keeper** — *The Warehouse Keeper* (8♣, Hoarder, Uncommon, SteadyHand)
`Renaissance oil painting, portrait of a meticulous keeper with ring of iron keys at belt checking inventory against a ledger, vast warehouse of crates and barrels stretching into darkness behind, lantern in hand, Caravaggio dramatic lighting, orderly authority, 2:3 aspect ratio`

**hoarder_tax_collector** — *The Tax Collector* (8♦, Hoarder, Common, ResourceGain)
`Renaissance oil painting, portrait of a stern official in dark formal robes collecting coins into a strongbox, quill and tax ledger on desk, oppressive stone office, single taper candle, Caravaggio chiaroscuro, bureaucratic menace, 2:3 aspect ratio`

**hoarder_banker** — *The Banker* (A♦, Hoarder, Uncommon, DoubleOrNothing)
`Renaissance oil painting, portrait of a powerful Medici-style banker in rich crimson robes seated behind a massive oak desk, gold coins in neat stacks, bank vault visible behind, multiple candelabras, Caravaggio warm lighting, wealth and power, 2:3 aspect ratio`

**hoarder_antiquarian** — *The Antiquarian* (8♥, Hoarder, Common, SteadyHand)
`Renaissance oil painting, portrait of a scholarly collector examining an ancient Roman coin, surrounded by antique pottery and scrolls, dusty shop interior, warm lamplight catching patina on bronze objects, Caravaggio chiaroscuro, quiet expertise, 2:3 aspect ratio`

**hoarder_squirrel** — *The Provisioner* (7♠, Hoarder, Common, ExtraDraw)
`Renaissance oil painting, portrait of a resourceful stockkeeper loading supplies into hidden compartments of a cart, clever smile, market stall with hanging preserved meats and dried herbs, warm torchlight, Caravaggio chiaroscuro, practical cunning, 2:3 aspect ratio`

**hoarder_usurer** — *The Usurer* (7♥, Hoarder, Rare, ExtraDraw)
`Renaissance oil painting, portrait of a cold-eyed moneylender in rich but austere black robes, one hand on a stack of promissory notes, the other on a bag of gold, debtors visible as shadows at the door, dark private office with iron strongbox, Caravaggio extreme chiaroscuro, predatory patience, elaborate textile detail, 2:3 aspect ratio`

**hoarder_apothecary** — *The Apothecary* (7♣, Hoarder, Uncommon, ResourceGain)
`Renaissance oil painting, portrait of an apothecary measuring precise amounts of dried herbs on a small brass scale, shelves of labeled jars and bottles behind, mortar and pestle on counter, warm candlelight through colored glass bottles, Caravaggio lighting, careful precision, 2:3 aspect ratio`

**hoarder_auctioneer** — *The Auctioneer* (10♠, Hoarder, Uncommon, AllIn)
`Renaissance oil painting, portrait of a theatrical auctioneer with gavel raised, mouth open mid-bid, crowd of eager bidders in shadow below, grand auction hall with draped items, Caravaggio dramatic spotlight on the auctioneer, excitement and greed, 2:3 aspect ratio`

**hoarder_treasure_hunter** — *The Treasure Hunter* (6♠, Hoarder, Common, AllIn)
`Renaissance oil painting, portrait of a dusty adventurer holding up a glinting gold artifact just pulled from a stone chest, wide excited eyes, torch in other hand, underground vault or tomb, Caravaggio dramatic torchlight, discovery thrill, 2:3 aspect ratio`

**hoarder_guild_master** — *The Guild Master* (K♣, Hoarder, Uncommon, Conquer)
`Renaissance oil painting, portrait of a powerful trade guild leader in heavy chain of office, seated at head of a long table, guild seal prominent, dark wood-paneled guild hall, multiple candles in iron chandelier, Caravaggio warm lighting, institutional authority, 2:3 aspect ratio`

**hoarder_monopolist** — *The Monopolist* (A♠, Hoarder, Rare, Monopoly)
`Renaissance oil painting, portrait of a scheming merchant prince with one hand on a globe and the other signing exclusive trade contracts, piles of deeds and titles, opulent private office with world maps and trade route charts on walls, Caravaggio dramatic candlelight, absolute commercial power, maximum detail, 2:3 aspect ratio`

**hoarder_dynasty_heir** — *The Dynasty Heir* (K♥, Hoarder, Rare, Haymaker)
`Renaissance oil painting, portrait of a young noble in extravagant inherited finery looking bored while surrounded by inherited wealth, portrait of stern ancestors on walls behind, gold and jewels carelessly strewn, palatial candlelit chamber, Caravaggio lighting, decadent privilege, elaborate costume, 2:3 aspect ratio`

**hoarder_reliquary** — *The Reliquary* (Q♥, Hoarder, Rare, SecondDeal)
`Renaissance oil painting, portrait of a mysterious keeper of sacred relics opening an ornate gilded reliquary box, holy light emanating from within illuminating their face, dark cathedral crypt with stone arches, Caravaggio extreme chiaroscuro, reverent awe, elaborate goldwork detail, 2:3 aspect ratio`

**hoarder_fence** — *The Fence* (8♠, Hoarder, Uncommon, WildCard)
`Renaissance oil painting, portrait of a back-alley dealer examining stolen jewelry through a loupe, shadowy shop crammed with diverse goods of questionable origin, single hidden candle, Caravaggio extreme chiaroscuro, street-level commerce, 2:3 aspect ratio`

**hoarder_cartels_don** — *The Cartel's Don* (J♣, Hoarder, Rare, SharkInstinct)
`Renaissance oil painting, portrait of a ruthless trade baron in dark expensive clothing with gold rings on every finger, seated in a velvet chair in a private room, bodyguard shadow at the door, cigar smoke curling in candlelight, Caravaggio dramatic lighting, dangerous wealth, elaborate detail, 2:3 aspect ratio`

**hoarder_vault_keeper** — *The Vault Keeper* (Q♦, Hoarder, Rare, Brace)
`Renaissance oil painting, portrait of an armored guard standing before a massive iron vault door with complex locks, ring of ornate keys at belt, stone bank interior with iron bars, single torch, Caravaggio chiaroscuro, immovable resolve, elaborate lock mechanism detail, 2:3 aspect ratio`

**hoarder_speculator** — *The Speculator* (9♠, Hoarder, Uncommon, ExtraDraw)
`Renaissance oil painting, portrait of a nervous trader studying fluctuating commodity prices on a chalkboard, one hand on chin deliberating, busy trading floor behind with shouting merchants, Caravaggio warm lamplight, anxious calculation, 2:3 aspect ratio`

---

### NEUTRAL DOCTRINE (15 cards)

**neutral_knight** — *Knight of the Order* (A♠, Neutral, Common, Conquer)
`Renaissance oil painting, portrait of a noble knight in full polished plate armor with heraldic tabard, sword held upright before face in salute, dark castle interior with banner, Caravaggio candlelight on gleaming steel, chivalric honor, 2:3 aspect ratio`

**neutral_bishop** — *The Bishop* (A♥, Neutral, Common, Brace)
`Renaissance oil painting, portrait of a bishop in ornate ecclesiastical robes and mitre, pastoral staff in hand, benevolent but shrewd expression, cathedral interior with stained glass in background, Caravaggio warm candlelight, spiritual authority, 2:3 aspect ratio`

**neutral_guild_knight** — *Guild Champion* (A♣, Neutral, Common, DoubleTrouble)
`Renaissance oil painting, portrait of a tournament champion in guild-colored armor raising a victory lance, guild pennant behind, tournament grounds at torch-lit evening, Caravaggio warm lighting, competitive pride, 2:3 aspect ratio`

**neutral_crown_jewel** — *The Crown Jewel* (A♦, Neutral, Common, Fortify)
`Renaissance oil painting, portrait of a royal jeweler presenting the finest diamond in a velvet-lined case, gem catching and refracting candlelight, dark royal treasury with crown jewels on display, Caravaggio focused lighting on the jewel, precious radiance, 2:3 aspect ratio`

**neutral_duke** — *The Duke* (K♠, Neutral, Common, SeizeInitiative)
`Renaissance oil painting, portrait of a commanding duke in military dress with medal sash, decisive gesture, war room with maps, Caravaggio candlelight, aristocratic command, 2:3 aspect ratio`

**neutral_cardinal** — *The Cardinal* (K♥, Neutral, Common, DoubleDefense)
`Renaissance oil painting, portrait of a cardinal in scarlet robes and red cap, hands clasped before him, shrewd compassionate eyes, dark Vatican-style chamber, Caravaggio warm candlelight, political wisdom, 2:3 aspect ratio`

**neutral_magistrate** — *The Magistrate* (K♣, Neutral, Common, Blocker)
`Renaissance oil painting, portrait of a stern magistrate in black judicial robes holding scales of justice, courtroom with wooden bench, heavy law books, Caravaggio dramatic side-lighting, impartial authority, 2:3 aspect ratio`

**neutral_queen_regent** — *The Queen Regent* (Q♥, Neutral, Common, SeizeInitiative)
`Renaissance oil painting, portrait of a powerful queen regent in crown and royal purple robes, seated on a throne, scepter in hand, dark throne room with single shaft of light, Caravaggio chiaroscuro, regal command, 2:3 aspect ratio`

**neutral_castellan** — *The Castellan* (Q♠, Neutral, Common, Brace)
`Renaissance oil painting, portrait of a castle warden in chain mail and surcoat standing atop fortress battlements, keys at belt, overlooking dark countryside below, torchlit parapet, Caravaggio chiaroscuro, steadfast defense, 2:3 aspect ratio`

**neutral_treasurer** — *The Treasurer* (Q♦, Neutral, Common, ExtraDraw)
`Renaissance oil painting, portrait of a royal treasurer in formal robes counting the kingdom's gold, crown treasury ledger open, stacks of coins and ingots, candlelit stone vault, Caravaggio warm lighting, meticulous accounting, 2:3 aspect ratio`

**neutral_herald** — *The Herald* (J♣, Neutral, Common, Peek)
`Renaissance oil painting, portrait of a herald in tabard with royal coat of arms, unfurling a proclamation scroll, trumpet tucked under arm, castle courtyard at torch-lit dusk, Caravaggio chiaroscuro, official authority, 2:3 aspect ratio`

**neutral_squire** — *The Squire* (J♠, Neutral, Common, ExtraDraw)
`Renaissance oil painting, portrait of a young squire polishing a knight's sword, eager expression, armory room with hanging weapons and shields, warm firelight from forge, Caravaggio chiaroscuro, youthful aspiration, 2:3 aspect ratio`

**neutral_merchant_prince** — *The Merchant Prince* (J♦, Neutral, Common, ExtraDraw)
`Renaissance oil painting, portrait of a wealthy Venetian merchant in gold-threaded silk robes reviewing trade documents, ships visible through a window, counting house with exotic goods, Caravaggio warm candlelight, cosmopolitan wealth, 2:3 aspect ratio`

**neutral_footman** — *The Footman* (10♣, Neutral, Common, Brace)
`Renaissance oil painting, portrait of a common soldier in basic armor and helmet standing guard with pike, dutiful expression, castle gatehouse at night, single torch, Caravaggio chiaroscuro, humble reliability, 2:3 aspect ratio`

**neutral_courser** — *The Courser* (10♥, Neutral, Common, Peek)
`Renaissance oil painting, portrait of a swift mounted messenger dismounting with urgent sealed letter, road-dusty cloak, horse behind, inn courtyard at dusk with lantern, Caravaggio warm lighting, breathless speed, 2:3 aspect ratio`

---

## SECTION 2: RELIC & TRINKET ICONS (28 items)

Each relic is a single physical object painted as a Renaissance still-life study. Dark velvet or dark wood background. Dramatic museum-style lighting from one side. Object fills most of the frame. No characters, no hands — just the object.

**Aspect ratio: 1:1 (512×512)**

**spys_monocle** — *The Spy's Monocle* (Schemer starting relic)
`Renaissance still life painting, ornate brass monocle on a gold chain coiled on dark velvet, lens catching candlelight with a tiny reflection, Caravaggio chiaroscuro, warm golden tones, single dramatic light source, museum-quality detail, 1:1 aspect ratio`

**iron_gauntlet** — *The Iron Gauntlet* (Brute starting relic)
`Renaissance still life painting, heavy iron-plated gauntlet standing upright on dark oak table, scratched and dented from battle, knuckles reinforced with steel studs, Caravaggio dramatic side-lighting, dark background, menacing power, 1:1 aspect ratio`

**two_faced_coin** — *The Two-Faced Coin* (Trickster starting relic)
`Renaissance still life painting, a large gold coin balanced on its edge showing two different faces simultaneously, one face smiling one frowning, dark velvet background, Caravaggio candlelight catching gold surface, optical illusion quality, 1:1 aspect ratio`

**bottomless_purse** — *The Bottomless Purse* (Hoarder starting relic)
`Renaissance still life painting, an overflowing leather coin purse with gold coins spilling endlessly from its open mouth, dark wood table, Caravaggio warm candlelight on gold, impossibly deep interior visible, 1:1 aspect ratio`

**scholars_lens** — *Scholar's Lens*
`Renaissance still life painting, an elegant magnifying glass with brass frame and crystal lens resting on an open book, candlelight refracted through the lens creating a bright spot, dark scholarly desk, Caravaggio chiaroscuro, 1:1 aspect ratio`

**venetian_cipher** — *Venetian Cipher*
`Renaissance still life painting, a complex mechanical cipher device with rotating brass discs and letter engravings, dark velvet background, Caravaggio dramatic side-lighting catching the engraved letters, intricate clockwork detail, 1:1 aspect ratio`

**forgers_kit** — *The Forger's Kit*
`Renaissance still life painting, a compact leather case opened to reveal forger's tools: sealing wax, blank stamps, fine brushes, and ink bottles, dark wood table, Caravaggio warm candlelight, meticulous craft detail, 1:1 aspect ratio`

**duelists_glove** — *The Duelist's Glove*
`Renaissance still life painting, a fine white leather glove thrown down on dark wood as a challenge, embroidered cuff with gold thread, single dramatic candle light from above, Caravaggio chiaroscuro, aristocratic elegance, 1:1 aspect ratio`

**glass_eye** — *The Glass Eye*
`Renaissance still life painting, a remarkably lifelike glass eye on dark velvet cushion, iris painted in vivid blue with gold flecks, candlelight creating eerie reflection in the glass surface, Caravaggio chiaroscuro, unsettling realism, 1:1 aspect ratio`

**poisoned_wine** — *Poisoned Wine*
`Renaissance still life painting, an ornate Venetian glass wine goblet filled with dark wine that has a faint green shimmer, single candle behind casting light through the liquid, dark table, Caravaggio chiaroscuro, beautiful danger, 1:1 aspect ratio`

**warhammer_pommel** — *Warhammer Pommel*
`Renaissance still life painting, the ornate pommel of a warhammer with a snarling lion head cast in blackened iron, leather grip wrapping, dark background, Caravaggio dramatic side-lighting on the metalwork, brutal craftsmanship, 1:1 aspect ratio`

**heretics_brand** — *The Heretic's Brand*
`Renaissance still life painting, a glowing red-hot iron branding tool shaped like a heretic's mark, resting in dark coals, ember glow illuminating from below, Caravaggio extreme chiaroscuro, ominous heat, 1:1 aspect ratio`

**condottieros_sash** — *Condottiero's Sash*
`Renaissance still life painting, a crimson silk military sash with gold embroidered rank insignia and campaign medals pinned to it, draped over dark wood, Caravaggio candlelight on silk and gold, martial honor, 1:1 aspect ratio`

**alchemists_stone** — *The Alchemist's Stone*
`Renaissance still life painting, a glowing deep red philosopher's stone in an ornate brass and filigree setting, casting warm ruby light on surrounding dark velvet, Caravaggio chiaroscuro, mystical radiance, 1:1 aspect ratio`

**masquerade_mask** — *Masquerade Mask*
`Renaissance still life painting, an elaborate Venetian half-mask in white porcelain with gold leaf accents and black feathers, resting on dark silk, Caravaggio warm candlelight on the porcelain surface, carnival elegance, 1:1 aspect ratio`

**courtesans_fan** — *The Courtesan's Fan*
`Renaissance still life painting, an elaborate folding silk fan painted with a secret coded message among floral patterns, half-open on dark velvet, Caravaggio warm lighting catching silk sheen, hidden meaning, 1:1 aspect ratio`

**crown_of_thorns** — *Crown of Thorns*
`Renaissance still life painting, a twisted crown of dark thorns with drops of blood on the tips, resting on dark stone, single candle from above, Caravaggio extreme chiaroscuro, sacred suffering, 1:1 aspect ratio`

**merchants_purse** — *The Merchant's Purse*
`Renaissance still life painting, a fat leather merchant's purse with brass clasp, bulging with coins, a few gold pieces spilling out, dark oak table, Caravaggio warm candlelight on leather and gold, commercial prosperity, 1:1 aspect ratio`

**misers_ring** — *The Miser's Ring*
`Renaissance still life painting, a plain iron ring with a tiny hidden compartment popped open revealing a single gold coin inside, dark velvet background, Caravaggio focused candlelight on the secret compartment, clever concealment, 1:1 aspect ratio`

**shield_brooch** — *The Shield Brooch*
`Renaissance still life painting, an ornate shield-shaped brooch in silver with enamel heraldic design and gemstone center, pin clasp visible, dark velvet, Caravaggio dramatic lighting on the metalwork, protective talisman, 1:1 aspect ratio`

**tailors_thimble** — *The Tailor's Thimble*
`Renaissance still life painting, a fine silver thimble with decorative dimpling and engraved vine pattern, resting beside a needle and thread on dark fabric, Caravaggio warm candlelight, humble craftsmanship, 1:1 aspect ratio`

**quicksilver_vial** — *Quicksilver Vial*
`Renaissance still life painting, a small glass vial filled with liquid mercury reflecting everything around it, brass stopper, dark wood and velvet background, Caravaggio dramatic lighting through the glass, liquid metal shimmer, 1:1 aspect ratio`

**phoenix_medal** — *The Phoenix Medal*
`Renaissance still life painting, a large gold medal depicting a phoenix rising from flames in relief, heavy chain attached, dark velvet cushion, Caravaggio warm candlelight making the gold glow, rebirth symbolism, 1:1 aspect ratio`

**fools_gold** — *Fool's Gold*
`Renaissance still life painting, a chunk of iron pyrite (fool's gold) that gleams deceptively like real gold, resting beside a real gold coin for comparison, dark table, Caravaggio candlelight making both surfaces shimmer, deceptive beauty, 1:1 aspect ratio`

**devils_bargain** — *The Devil's Bargain*
`Renaissance still life painting, a contract scroll with elegant calligraphy and a red wax seal bearing a devil's face, quill pen dipped in blood-red ink beside it, dark desk, Caravaggio dramatic candlelight, infernal elegance, 1:1 aspect ratio`

**scholars_tome** — *The Scholar's Tome*
`Renaissance still life painting, a thick leather-bound tome with brass corner protectors and clasp lock, pages edged in gold leaf, dark oak reading stand, Caravaggio warm candlelight, ancient knowledge, 1:1 aspect ratio`

**ventriloquists_dummy** — *The Ventriloquist's Dummy*
`Renaissance still life painting, a small carved wooden puppet head with articulated jaw and glass eyes, painted features slightly unsettling, resting on dark velvet, Caravaggio dramatic side-lighting casting long shadow, uncanny presence, 1:1 aspect ratio`

**jugglers_balls** — *The Juggler's Balls*
`Renaissance still life painting, three polished leather juggling balls in red, gold, and blue, arranged in a triangle on dark velvet, Caravaggio warm candlelight on the leather surfaces, carnival craft, 1:1 aspect ratio`

---

## SECTION 3: OPPONENT PORTRAITS (17 characters)

Portraits are wide-format (16:9) oil paintings showing the character from chest up, seated or standing at a card table. The venue background should match their act. Face should be centered for square cropping in the UI. Gold ornate frame border included in the image.

**Aspect ratio: 16:9 (1408×768)**

### Act 1 — The Bilge Rat Tavern
Dark waterfront dive: warped planks, tallow candles, rope coils, barnacle-crusted portholes.

**portrait_barnacle_bill** — *Barnacle Bill* (Brawler)
`Renaissance oil painting portrait in ornate gold frame, grizzled old sailor with eye patch and weathered skin, gap-toothed grin, stained linen shirt, seated at rough wooden tavern table, tallow candle to his left, pewter tankard beside him, dark waterfront tavern with rope coils and barnacle-crusted porthole, Caravaggio chiaroscuro, warm amber candlelight, 16:9 aspect ratio`

**portrait_salty_pete** — *Salty Pete* (Miser)
`Renaissance oil painting portrait in ornate gold frame, thin suspicious dockworker in threadbare coat clutching a small coin purse to chest, narrow wary eyes, hunched shoulders, seated at rough tavern table, tallow candle, dark waterfront tavern with wooden planks and fishing nets, Caravaggio chiaroscuro, 16:9 aspect ratio`

**portrait_dock_rat** — *Dock Rat* (Fox)
`Renaissance oil painting portrait in ornate gold frame, wiry young street rat in patched clothes with quick clever eyes and a crooked smile, nimble fingers resting on cards, seated at rough tavern table, tallow candle, dark waterfront tavern with barrels and rope, Caravaggio chiaroscuro, 16:9 aspect ratio`

**portrait_fishy_meg** — *Fishy Meg* (Brawler, Elite)
`Renaissance oil painting portrait in ornate gold frame, tough older woman with weathered face and calculating expression, hair wrapped in kerchief, strong hands on the table, seated at tavern table, tallow candle illuminating her piercing stare, dark waterfront tavern with pottery jars and lanterns, Caravaggio chiaroscuro, 16:9 aspect ratio`

### Act 2 — The Merchant's Rest
Polished oak inn: brass fittings, maps, ledgers, fireplace, warm browns and copper.

**portrait_merchant_luca** — *Merchant Luca* (Fox)
`Renaissance oil painting portrait in ornate gold frame, sharp-eyed Italian merchant in fine wool coat with fur collar, quill tucked behind ear, seated at polished oak table in a trading inn, brass fittings and maps on walls, warm fireplace glow, Caravaggio chiaroscuro, prosperous cunning, 16:9 aspect ratio`

**portrait_trader_yun** — *Trader Yun* (Miser)
`Renaissance oil painting portrait in ornate gold frame, East Asian trader in silk merchant robes with careful guarded expression, hands folded protectively, seated at polished oak inn table, brass scales nearby, warm firelight, maps and ledgers on wall, Caravaggio chiaroscuro, cautious patience, 16:9 aspect ratio`

**portrait_silk_marco** — *Silk Marco* (Noble)
`Renaissance oil painting portrait in ornate gold frame, suave young merchant in expensive silk doublet with lace collar, confident charming smile, fan of cards held elegantly, seated at polished inn table, brass candelabra, warm trading inn interior with parchment maps, Caravaggio chiaroscuro, effortless style, 16:9 aspect ratio`

**portrait_coin_bianca** — *Coin Bianca* (Scholar, Elite)
`Renaissance oil painting portrait in ornate gold frame, brilliant woman in dark scholarly dress with gold coin brooch, sharp analytical gaze, mathematical instruments on table beside cards, seated at polished inn table, warm fire and brass fittings, trading inn with ledger shelves, Caravaggio chiaroscuro, dangerous intelligence, 16:9 aspect ratio`

### Act 3 — The Guildmaster's Hall
Dark wood guild hall: burgundy, gold leaf, oil portraits, stained glass, iron chandeliers.

**portrait_guildmaster_voss** — *Guildmaster Voss* (Noble)
`Renaissance oil painting portrait in ornate gold frame, imposing guild leader with grey temples and heavy gold chain of office, stern authoritative gaze, seated at ornate carved table in dark wood guild hall, oil portraits and stained glass behind, iron chandelier candles, Caravaggio chiaroscuro, institutional power, 16:9 aspect ratio`

**portrait_lady_ashton** — *Lady Ashton* (Scholar)
`Renaissance oil painting portrait in ornate gold frame, refined noblewoman in dark blue velvet gown with pearl earrings, intelligent measured expression, delicate hands on cards, seated at guild hall table, dark burgundy wood paneling with gold leaf accents, iron chandelier, Caravaggio chiaroscuro, graceful calculation, 16:9 aspect ratio`

**portrait_baron_kell** — *Baron Kell* (Fox)
`Renaissance oil painting portrait in ornate gold frame, roguish minor nobleman with sharp goatee and amused half-smile, fine but slightly rakish attire, seated at guild hall table, dark wood and burgundy interior with heraldic banners, iron chandelier candles, Caravaggio chiaroscuro, dangerous charm, 16:9 aspect ratio`

**portrait_fixer_tomas** — *Fixer Tomas* (Assassin, Elite)
`Renaissance oil painting portrait in ornate gold frame, cold-eyed operative in all black with leather gloves, scarred face perfectly still, seated at guild hall table with hands flat on surface, dark wood paneling with iron chandelier casting sharp shadows, Caravaggio extreme chiaroscuro, lethal calm, 16:9 aspect ratio`

### Act 4 — The Cardinal's Library
Cathedral library: purple, ecclesiastical gold, crimson, floor-to-ceiling bookshelves, astronomical instruments.

**portrait_cardinal_enzo** — *Cardinal Enzo* (Scholar)
`Renaissance oil painting portrait in ornate gold frame, elderly cardinal in crimson robes and red cap, wise penetrating eyes behind spectacles, seated at ebony table in cathedral library, floor-to-ceiling bookshelves and astronomical globe behind, candlelit alcove, Caravaggio chiaroscuro, ecclesiastical intellect, 16:9 aspect ratio`

**portrait_sister_agatha** — *Sister Agatha* (Noble)
`Renaissance oil painting portrait in ornate gold frame, composed nun in black and white habit with an unexpectedly steely gaze, rosary beads in one hand, seated at library table, illuminated manuscripts and candelabra, cathedral library with deep purple and gold, Caravaggio chiaroscuro, quiet ferocity, 16:9 aspect ratio`

**portrait_spymaster_grey** — *Spymaster Grey* (Assassin)
`Renaissance oil painting portrait in ornate gold frame, gaunt figure in dark grey robes with silver-streaked hair, eyes like a hawk, long fingers steepled, seated at library table, shadows deeper than they should be, cathedral library barely visible in darkness, Caravaggio extreme chiaroscuro, mastermind menace, 16:9 aspect ratio`

**portrait_scholar_ruiz** — *Scholar Ruiz* (Fox, Elite)
`Renaissance oil painting portrait in ornate gold frame, brilliant younger scholar in dark academic robes with unexpected humor in his eyes, surrounded by open books and astronomical instruments, seated at library table, cathedral bookshelves with brass telescope behind, Caravaggio warm candlelight, deceptive brilliance, 16:9 aspect ratio`

### Act 5 — The Duke's Salon
Venetian palace: royal purple, gold, midnight blue, gilt mirrors, Murano glass chandeliers, silk wallpaper.

**portrait_champion** — *The Champion* (Assassin, Boss)
`Renaissance oil painting portrait in ornate gold frame, mysterious figure in ornate Venetian carnival mask of white porcelain and gold filigree, dark purple velvet cloak with silver embroidery, impossible to read expression behind the mask, seated at masterwork mahogany table in Venetian palazzo, Murano glass chandelier with dozens of candles, gilt-framed mirrors, Caravaggio warm golden chiaroscuro, final boss legendary mystique, 16:9 aspect ratio`

---

## SECTION 4: VENUE BACKGROUNDS (5 acts)

Full-screen atmospheric environments. No characters. Wide composition showing the card-playing area as the focal point. Rich atmospheric detail.

**Aspect ratio: 16:9 (1408×768)**

**bg_tavern** — *Act 1: The Bilge Rat Tavern*
`Renaissance oil painting, interior of a dark waterfront tavern, rough wooden plank walls, tallow candles in bottles on a stained wooden card table, rope coils hung on walls, barnacle-crusted porthole windows, pewter tankards, empty chairs around the table, warm amber candlelight, Caravaggio chiaroscuro, atmospheric grime and character, no people, 16:9 aspect ratio`

**bg_merchant** — *Act 2: The Merchant's Rest*
`Renaissance oil painting, interior of a prosperous trading inn, polished oak furniture, brass fittings and candelabras, maps and ledgers pinned to walls, multi-paned glass windows, green baize card table with cleaner cards, warm fireplace glow, Caravaggio chiaroscuro, comfortable wealth, no people, 16:9 aspect ratio`

**bg_guild** — *Act 3: The Guildmaster's Hall*
`Renaissance oil painting, interior of a grand trade guild hall, high ceilings with dark wooden beams, oil portraits on burgundy walls, stained glass window, iron chandeliers with candles, ornate carved card table with thick green felt, silver card tray, Caravaggio chiaroscuro, institutional grandeur, no people, 16:9 aspect ratio`

**bg_library** — *Act 4: The Cardinal's Library*
`Renaissance oil painting, interior of a cathedral library, floor-to-ceiling bookshelves of leather-bound tomes, astronomical instruments and globe, illuminated manuscripts, candlelit alcoves, ebony card table with mother-of-pearl inlay, crystal wine decanter, deep purple and ecclesiastical gold, Caravaggio chiaroscuro, scholarly sanctum, no people, 16:9 aspect ratio`

**bg_salon** — *Act 5: The Duke's Salon*
`Renaissance oil painting, interior of a Venetian palazzo salon, gilt-framed mirrors, Murano glass chandelier with dozens of candles, silk wallpaper in midnight blue and gold, marble floors, moonlit canal visible through arched window, masterwork mahogany card table with gold filigree and velvet surface, crystal card holders, Caravaggio warm golden chiaroscuro, peak Renaissance opulence, no people, 16:9 aspect ratio`

---

## SECTION 5: TABLE SURFACES (5 acts)

Top-down view of the card-playing table surface. Used as the match background beneath cards.

**Aspect ratio: 16:9 (1408×768)**

**table_tavern** — *Act 1 Table*
`Top-down photograph style, rough wooden tavern table surface with knife marks, ale stains, and scattered crumbs, tallow candle wax drips, warm amber lighting from above, dark wood grain texture, no cards or objects, 16:9 aspect ratio`

**table_merchant** — *Act 2 Table*
`Top-down photograph style, green baize gaming table surface stretched over polished oak, brass corner protectors, cleaner and more refined than a tavern, warm firelight from side, no cards or objects, 16:9 aspect ratio`

**table_guild** — *Act 3 Table*
`Top-down photograph style, thick green felt card table surface with ornate carved dark wood border and silver card tray indent, tooled leather deck case area, iron chandelier light from above, no cards or objects, 16:9 aspect ratio`

**table_library** — *Act 4 Table*
`Top-down photograph style, ebony table surface with mother-of-pearl inlay border pattern, thick dark wood, crystal wine glass ring stain, candlelight reflection, scholarly refinement, no cards or objects, 16:9 aspect ratio`

**table_salon** — *Act 5 Table*
`Top-down photograph style, masterwork mahogany table with gold filigree inlay and deep velvet playing surface in royal purple, crystal card holder indents, Murano glass chandelier light from above, peak luxury, no cards or objects, 16:9 aspect ratio`

---

## SECTION 6: MAP NODE ICONS (6 types)

Small iconic images. Simple bold silhouette style on transparent or dark background. Must read clearly at 48×48 display size.

**Aspect ratio: 1:1 (96×96 target, generate at 512×512 and downscale)**

**map_node_match** — *Standard Match*
`Simple icon on dark background, two crossed swords in golden Renaissance style, clean bold silhouette, ornate but readable at small size, warm gold on dark brown, 1:1 aspect ratio`

**map_node_elite** — *Elite Match*
`Simple icon on dark background, crossed swords with a crown above in golden Renaissance style, clean bold silhouette, more ornate than standard match, warm gold with red accent on dark brown, 1:1 aspect ratio`

**map_node_boss** — *Boss Match*
`Simple icon on dark background, a Venetian carnival mask with crossed swords behind in golden Renaissance style, bold dramatic silhouette, gold and crimson on dark brown, 1:1 aspect ratio`

**map_node_shop** — *Shop*
`Simple icon on dark background, a merchant's balance scale in golden Renaissance style, clean bold silhouette, warm gold on dark brown, 1:1 aspect ratio`

**map_node_rest** — *Rest Stop*
`Simple icon on dark background, a hearth fire or campfire in golden Renaissance style, clean warm silhouette, warm gold and orange on dark brown, 1:1 aspect ratio`

**map_node_rumor** — *Rumor/Event*
`Simple icon on dark background, an ear or speech scroll in golden Renaissance style, clean bold silhouette, warm gold on dark brown, mysterious, 1:1 aspect ratio`

---

## Generation Tips for NanoBanana

1. **Consistency**: Use "Caravaggio chiaroscuro, Renaissance oil painting, warm candlelight" in every prompt to maintain style cohesion
2. **Character identity**: Name the character type first in the prompt (subject → action → setting → style)
3. **Aspect ratio**: Always specify at the end of each prompt
4. **Batch by type**: Generate all cards of one doctrine together, then move to the next, to maintain visual consistency
5. **Rarity scaling**: For Rare cards, add "maximum detail, elaborate" and describe more props/background elements
6. **Portrait cropping**: Keep face centered in the composition — the game crops to a 66×66 square from center
7. **Color temperature**: Act 1-2 should be warmer (amber/yellow), Act 3-4 cooler (crimson/purple), Act 5 golden
8. **No text**: Never include text, numbers, or suit symbols in generated art — the game overlays these programmatically

## File Naming Convention

Save generated files as:
- Cards: `Assets/Art/Generated/Cards/{card_id}.png`
- Relics: `Assets/Art/Generated/Relics/{relic_slug}.png`
- Portraits: `Assets/Art/Portraits/portrait_{slug}.png`
- Backgrounds: `Assets/Art/Backgrounds/bg_{venue}.png`
- Tables: `Assets/Art/Tables/table_{venue}.png`
- Map nodes: `Assets/Art/Map/map_node_{type}.png`

---

## SECTION 1b: PROMPTS AUTHORED FOR THE 10 PREVIOUSLY-MISSING CARDS

(Added when completing the card-art epic. Style prefix is applied programmatically in
generate_all_missing.py, so these are the bare scene descriptions.)

**schemer_expurgator** — *The Expurgator* (Schemer)
`portrait of a severe inquisitor-scholar blacking out lines of a forbidden book with heavy ink strokes, stacks of censored tomes, candlelit scriptorium with dark shelves, intense secrecy`

**schemer_censor** — *The Censor* (Schemer)
`portrait of a stern official pressing a black wax seal to suppress a sealed letter, red ribbon and confiscated documents, single taper candle, shadowed bureau office`

**brute_scorched_earth** — *Scorched Earth* (Brute)
`portrait of a grim armored soldier raising a flaming torch before a burning field at night, smoke and drifting embers, scarred face lit by firelight, war-torn landscape`

**brute_forge_master** — *The Forge Master* (Brute)
`portrait of a burly blacksmith hammering a glowing sword blade on an anvil, sparks flying, heavy leather apron, muscular scarred arms, fiery forge glow in a dark smithy`

**trickster_transmuter** — *The Transmuter* (Trickster)
`portrait of a theatrical conjurer transmuting a coin between gloved hands with a flourish, swirling colored smoke, carnival stage, Venetian masks in the shadowed background`

**trickster_identity_thief** — *The Identity Thief* (Trickster)
`portrait of a masked figure holding up a stolen face-shaped mask resembling another person, mid-disguise swapping cloaks, moonlit Venetian carnival alley, sly intrigue`

**hoarder_glutton** — *The Glutton* (Hoarder)
`portrait of a corpulent richly-dressed merchant feasting greedily at a laden table, one hand grabbing coins the other food, overflowing platters and goblets, candlelit dining hall`

**hoarder_opportunist** — *The Opportunist* (Hoarder)
`portrait of a sharp-eyed trader snatching a coin purse the instant a back is turned, calculating grin, market stall with brass scales and ledgers, warm candlelight`

**hoarder_grudge_keeper** — *The Grudge Keeper* (Hoarder)
`portrait of a bitter old moneylender clutching a thick ledger of debts, narrowed vengeful eyes, tally marks and unpaid notes, dim counting house with an iron-bound chest`

**hoarder_dragons_hoard** — *Dragon's Hoard* (Hoarder)
`portrait of a miser reclining atop a mountain of gold coins, jewelled goblets and treasure, greedy protective posture, candlelit vault glittering with riches`
