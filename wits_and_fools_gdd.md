**Game Design Document**  
**Wits and Fools**

---

## **1. Introduction & Summary**

**Game Title:** Wits and Fools  
**Genre:** Card Strategy Game  
**Platform:** PC (initial), potential mobile adaptation  
**Engine:** Unity  

**Overview:**  
Wits and Fools is a turn-based strategic card game inspired by Durak but with unique mechanics involving special abilities. Players engage in political maneuvering in a Renaissance-inspired setting, attacking and defending using numbered cards while utilizing special card effects to disrupt opponents. The game supports **2-5 players**, with a **single-player mode against AI bots**. The objective is to outlast opponents and avoid becoming the Fool—the last player left with cards.  

**Theme & Setting:**  
Set in a **Renaissance-era pub or restaurant**, the game exudes an atmosphere of **political intrigue and cunning**. The art style draws heavily from the visual language of card mini-games in classic RPGs such as **Queen's Blood** from Final Fantasy VII Rebirth and **Tetra Master** from Final Fantasy IX. The aesthetic features stylized character portraits, ornate board decorations, rich texture detail, and an immersive medieval fantasy tone.

---

## **2. Gameplay Rules & Flow**

### **2.1. Game Flow & Round Structure**
1. **Game Start:**
   - Players are dealt hands based on player count (2 players: 7 cards, 3 players: 7 cards, 4 players: 6 cards, 5 players: 5 cards).  
   - A **trump suit** is determined by flipping a card from the deck.
   - The player with the **lowest trump card** goes first.

2. **Turn Structure:**
   - The active player challenges the player to their left with a card of any suit.
   - The defender must **rebut** with a higher card of the same suit or a trump card.
   - Other players may add **challenges** clockwise, using cards matching the attack.
   - The defender may **slide** the attack to the next player if they have a card of the same value.
   - If the defender successfully defends all challenges, they become the next challenger.
   - If the defender fails, they **eat the cards**, adding them to their hand, and the next turn passes left.

3. **Endgame Condition:**
   - When the deck is empty, players no longer draw to minimum hand size.
   - Players cannot be given more cards than they have in hand.
   - The game ends when all but one player has emptied their hand. The last remaining player is the **Fool**.

---

## **3. Special Card Abilities & Effects**

**Abilities are activated when the card is played, with an option to use the ability or play the card normally.**

- **Shield Card:** The defender can skip their turn, passing the attack to the next player.
- **Double Trouble:** Allows the attacker to play two additional cards of any rank.
- **Trump Changer:** Changes the trump suit to the suit of the played card.
- **The Blocker:** Prevents the defender from adding any more cards to the attack.
- **The Magnet:** Attracts all cards of the same rank from the deck.
- **The Reverser:** Reverses turn order.
- **Skip Turn:** The next player is skipped.
- **Extra Draw:** Forces the defender to draw an extra card before defending.
- **Wildcard:** Can be played as any rank or suit.
- **Double Defense:** Allows the defender to counter two attacking cards with one if they match rank or suit.

**Restrictions & Priority:**
- Abilities must be activated during the appropriate phase (attack, defense, or pre-round).
- Ability priority follows turn order, ensuring no simultaneous activations.
- AI and UI will enforce ability play restrictions dynamically.

---

## **4. UI/UX & Visual Design**

**Board Layout:**
- The game table is presented **top-down**, showing players seated around a stylized, medieval-inspired table similar to card games in **Queen's Blood** and **Tetra Master**.
- The **player’s hand** is displayed at the bottom.
- The **deck, discard pile, and trump card** are in the center.
- Opponents’ **hand sizes are visible** but their cards are hidden.

**Animations & Feedback:**
- Attackers and defenders are **highlighted in red and blue** respectively.
- A **pale glow** highlights active players.
- **Distinct animations** for attack/defense (card slaps, swipe effects, impact effects).

**Player Indicators:**
- Each player has a **profile image** with a unique avatar and personality.
- AI opponents **react with text-based quips** and **non-verbal sounds** (grunts, laughter, scoffs).
- UI displays **turn status** (e.g., “Attacking,” “Defending”).

**Accessibility:**
- Hover/tap **tooltips** for ability explanations.
- Future-proofing for **colorblind and large-text options**.

---

## **5. AI Design**

**AI Behavior**
- **Personality-Driven AI:** Rule-based decision-making with varied tendencies:
  - Some AIs prefer aggressive play, others are defensive or opportunistic.
- **No Cheating:** AI does not access hidden information.
- **AI Difficulty Scaling:** Affects both decision-making and card pool difficulty.

---

## **6. Technical Architecture**

**Game State Management:**
- **State Machine** controls game flow.
- **JSON file storage** for single-player saves.

**Card & Ability System:**
- **Component-based architecture** (scriptable abilities attach to cards).
- **Data-driven effects** allow for easy expansion.

**Save & Persistence:**
- Single-player progress is stored **locally** in a JSON file.

---

## **7. Sound & Player Experience**

**Music:**
- **Lute & harpsichord** tracks with occasional percussion.
- Lighthearted background music, intensifying near the endgame.
- Sound cues for **winning, losing, and key gameplay moments**.

**Sound Effects:**
- **Card slap:** Attack.
- **Swipe effect:** Defense.
- **Angry/happy noises** when players react.
- Unique **sound cues for special abilities** (bells, harp strings, drum thumps).

**Game Flow & Retention:**
- **Short breaks** between rounds with a stats screen.
- Players earn **coins & experience** per round.
- **Guided first match** with tooltips for new players.

---

## **Conclusion**
Wits and Fools is a **tactically deep yet accessible** card game built around **engaging AI, balanced mechanics, and a richly themed Renaissance setting**. With art and UI direction inspired by **Queen's Blood** and **Tetra Master**, it offers a nostalgic yet fresh experience. This document provides all the core **design, technical, and artistic guidelines** needed for prototype development.

**Next Steps:** Prototype development, UI/UX wireframing, and initial AI behavior testing.

