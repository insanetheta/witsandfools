# Wits and Fools - Copilot Instructions

## Project Overview

**Wits and Fools** is a Renaissance-themed strategic card game inspired by Durak, built in Unity. This is a turn-based card game featuring special abilities, AI opponents, and political intrigue themes.

### Key Game Mechanics
- **Attack/Defense System:** Players attack with cards, defenders must beat with higher same suit or trump cards
- **Trump System:** One suit beats all non-trump cards (currently Hearts ♥)
- **Bout Resolution:** Failed defense = defender "eats" attack cards; successful defense = cards discarded
- **Special Abilities:** 10 unique card abilities (Shield, Wildcard, Trump Changer, etc.)
- **Win Condition:** First to empty hand wins; last player with cards is the "Fool"

## Current Development Status

### ✅ **Phase 3 COMPLETE** - Basic Game Rules
- **Core Attack/Defense Loop:** Fully functional with AI opponent
- **Trump System:** Hearts trump beats non-trump cards
- **Turn Management:** Attack → Defense → Resolution phases
- **Visual Feedback:** Cards move between hand areas and attack/defense zones
- **AI Defense:** Automatic AI player that defends when possible or "eats" cards
- **Game Rules Validation:** All Durak-inspired rules implemented

### 🔄 **Next: Phase 4** - Special Abilities System
The next major milestone is implementing the 10 special card abilities from the GDD.

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Main game systems
│   │   ├── GameManager.cs
│   │   ├── GameRules.cs        # Durak rule validation
│   │   ├── TurnManager.cs      # Phase management
│   │   ├── AttackDefenseSystem.cs  # Attack/defense mechanics
│   │   ├── Player.cs           # Player data & hand management
│   │   ├── HandManager.cs      # Visual hand layout
│   │   └── DeckManager.cs      # Card dealing
│   ├── Cards/          # Card system
│   │   ├── Card.cs             # Card runtime behavior
│   │   ├── CardData.cs         # ScriptableObject definitions
│   │   └── CardRenderer.cs     # Visual rendering
│   └── Demo/           # Demo & testing scripts
│       ├── AttackHandler.cs    # Attack/defense demo controller
│       ├── DemoCardCreator.cs  # Demo card generation
│       └── DemoUISetup.cs      # UI configuration
├── Scenes/
│   └── GameScene.unity         # Main game scene
└── Data/               # ScriptableObject assets
```

## Development Guidelines

### Code Style & Architecture
- **Namespace Organization:** Use `WitsAndFools.Core`, `WitsAndFools.Cards`, etc.
- **Singleton Pattern:** Used for managers (GameManager, GameRules, TurnManager, AttackDefenseSystem)
- **ScriptableObjects:** Card definitions use CardData ScriptableObjects
- **Event System:** UnityEvents for decoupled communication between systems
- **Debug Logging:** Extensive console output for gameplay debugging

### Card System Architecture
- **CardData (ScriptableObject):** Static card definitions with abilities
- **Card (MonoBehaviour):** Runtime card behavior and UI interaction
- **CardRenderer:** Visual representation and display logic
- **Special Abilities:** Component-based system for card effects

### Game Flow Patterns
```csharp
// Typical attack flow:
1. Player clicks card → AttackHandler.OnCardAttack()
2. AttackDefenseSystem.AttemptAttack() → Validates & processes
3. TurnManager.StartDefensePhase() → Switches to defense
4. AI considers defense → AttackHandler.ConsiderDefense()
5. Resolution → CompleteBout() → Clear areas → Next turn
```

### UI & Visual Guidelines
- **Renaissance Theme:** Inspired by Queen's Blood and Tetra Master
- **Hand Layout:** Arc formation at bottom (Player 0) and top (Player 1)
- **Attack/Defense Areas:** Center screen for active cards
- **Card Movement:** Smooth transitions between areas
- **Visual Hierarchy:** Clear distinction between player areas

## Key Classes & Responsibilities

### Core Systems
- **GameManager:** Main game controller, singleton, scene setup
- **GameRules:** Validates attacks/defenses, trump logic, bout completion
- **TurnManager:** Phase management (StartTurn → AttackPhase → DefensePhase → EndTurn)
- **AttackDefenseSystem:** Manages active bout, card placement, visual areas
- **Player:** Hand management, card storage, player state

### Demo Systems
- **AttackHandler:** Handles both human attacks and AI defense logic
- **DemoCardCreator:** Generates test cards for development
- **DeckManager:** Deals initial hands to players

## Special Abilities (Phase 4 Ready)

From GDD Section 3, implement these 10 abilities:
1. **Shield Card:** Pass attack to next player
2. **Double Trouble:** Play two additional cards
3. **Trump Changer:** Change trump suit
4. **The Blocker:** Prevent additional attack cards
5. **The Magnet:** Attract same-rank cards from deck
6. **The Reverser:** Reverse turn order
7. **Skip Turn:** Skip next player
8. **Extra Draw:** Force defender to draw extra card
9. **Wildcard:** Play as any rank/suit
10. **Double Defense:** Counter two attacks with one card

## Testing & Demo Instructions

### Current Demo (Phase 3)
1. Open GameScene in Unity
2. Enter Play Mode
3. Wait 3 seconds for card dealing and setup
4. Click any Player 1 card (bottom hand) to attack
5. Watch AI defend automatically after 2-second delay
6. Observe complete bout cycle in console logs

### Expected Behavior
- **Attack:** Card moves to attack area, detailed logging
- **Defense:** AI attempts defense or "eats" cards
- **Resolution:** Areas clear, bout completes, ready for next attack

## Console Logging Patterns

The game uses extensive debug logging for development:
```
=== ATTACK INITIATED ===
=== AI DEFENSE TRIGGERED ===
=== DEFENSE SUCCESSFUL/FAILED ===
=== BOUT COMPLETE ===
```

## Common Development Tasks

### Adding New Cards
1. Create CardData ScriptableObject in Data folder
2. Set card properties (name, value, suit, ability type)
3. Add to DemoCardCreator for testing
4. Implement ability logic if special card

### Debugging Game Flow
- Check console logs for detailed bout information
- Verify TurnManager.currentPhase for proper phase transitions
- Inspect Player.GetHand() for current card states
- Use AttackDefenseSystem.GetAttackCards/GetDefenseCards() for bout state

### UI Updates
- Hand areas: PlayerHandArea (bottom), Player1HandArea (top)
- Attack/Defense: AttackArea, DefenseArea (center)
- Cards auto-arrange in arc formation via HandManager

## Performance Considerations
- Card instantiation uses object pooling patterns
- Visual updates only when necessary
- Coroutines for AI thinking delays and animations
- Efficient card matching algorithms in GameRules

## Future Phases (Post-Phase 4)
- **Phase 5:** AI Personality System
- **Phase 6:** Renaissance UI/UX Polish
- **Phase 7:** Animation & Visual Effects
- **Phase 8:** Audio System (Lute/Harpsichord)
- **Phase 9:** Save System & Progression
- **Phase 10:** Polish & Optimization

## Important Files to Reference
- `wits_and_fools_gdd.md` - Complete game design document
- `WitsAndFools_Development_Plan.md` - Detailed development progress
- `Assets/Scripts/Core/GameRules.cs` - Core game logic reference
- Current scene: `Assets/Scenes/GameScene.unity`

## Development Philosophy
- **Iterative Development:** Build and test incrementally
- **Extensive Logging:** Debug information for all game events
- **Modular Design:** Systems can be developed and tested independently
- **Demo-Driven:** Always maintain a playable demo state
- **GDD Compliance:** All features align with game design document

## 🚨 Critical Development Rules

### **ALWAYS Check Unity Console Before Completing Tasks**
- **MANDATORY:** Use `mcp_unitymcp_read_console` tool to check for compiler errors before ending any response
- **Fix All Errors:** If any compiler errors are found, work to resolve them immediately
- **No Exceptions:** Never leave the project in a broken state - all code changes must compile successfully
- **Validation:** Re-check console after fixes to ensure all errors are resolved
- **Priority:** Compiler errors take precedence over all other tasks

### **Unity Editor State Management**
- **Exit Play Mode:** Always ensure Unity is out of play mode before editing GameObjects, components, or scene assets using Unity MCP tools
- **Use `mcp_unitymcp_manage_editor` with action 'get_state'** to check if Unity is in play mode before making scene changes
- **Use `mcp_unitymcp_manage_editor` with action 'stop'** to exit play mode if currently playing
- **Script Compilation:** Wait for scripts to recompile after code changes before making additional Unity editor operations
- **Validation:** Check editor state before any GameObject, scene, or asset management operations

---

**Last Updated:** Phase 3 completion - Basic game rules fully implemented
**Next Milestone:** Phase 4 - Special Abilities System implementation