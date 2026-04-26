# Wits and Fools - Unity Development Plan

## Project Overview

**Game:** Wits and Fools - A Renaissance-themed strategic card game inspired by Durak  
**Engine:** Unity  
**Platform:** PC (initial), potential mobile adaptation  
**Players:** 2-5 players with single-player AI mode

---

## Development Phases

### Phase 1: Project Foundation & Core Architecture

**Duration:** 1-2 weeks  
**Deliverable:** Basic Unity project structure with core systems architecture

**Tasks:**

-   ✅ Set up Unity project with appropriate settings
-   ✅ Create folder structure (Scripts, Art, Audio, Prefabs, Scenes)
-   ✅ Implement core game state management system
-   ✅ Create card data structure (ScriptableObjects)
-   ✅ Set up basic scene structure
-   ✅ Implement simple UI framework
-   ✅ Create basic player and game controller classes

**Key Files:**

-   ✅ `GameManager.cs` - Main game controller (Singleton pattern with state management)
-   ✅ `GameState.cs` - Game state management (Enums for states, suits, abilities)
-   ✅ `Card.cs` - Card runtime representation (Visual display, interaction, events)
-   ✅ `Player.cs` - Player representation (Hand management, turn states, AI support)
-   ✅ `CardData.cs` - ScriptableObject for card definitions (Complete with ability system)

**Implementation Notes:**

-   Created comprehensive folder structure: Scripts/Core, Scripts/Cards, Art, Audio, Prefabs, Data
-   GameManager implements Singleton pattern for easy access across game systems
-   Defined all core enums: GameState, TurnPhase, CardSuit, PlayerType, CardAbilityType
-   CardData ScriptableObject includes card beating logic and special ability support
-   All classes use proper namespace organization (WitsAndFools.Core, WitsAndFools.Cards)
-   Card.cs handles visual display, interaction events, and reveal/hide states
-   Player.cs manages hand, turn states, and provides foundation for AI implementation
-   Created GameScene with GameManager, UI Canvas, and EventSystem
-   Updated EventSystem to use InputSystemUIInputModule (new Unity Input System)
-   Basic UI framework ready for Phase 2 card rendering implementation

**Phase 1 Complete!** ✅ All core architecture and foundation systems are in place.

---

### Phase 2: Card System Implementation

**Duration:** 1-2 weeks  
**Deliverable:** Complete card system with basic rendering and interaction

**Tasks:**

-   ✅ Implement card rendering system
-   ✅ Create card interaction mechanics (drag, drop, hover)
-   ✅ Build card ability system architecture
-   ✅ Create basic card animations
-   ✅ Implement card validation system
-   ✅ Design card UI components
-   ✅ Create card prefab with UI elements
-   ✅ Implement hand management system
-   ✅ Create demo system for testing cards

**Key Files:**

-   ✅ `CardRenderer.cs` - Card visual representation (Complete rendering system)
-   ✅ `CardAbility.cs` - Base class for card abilities (Abstract ability framework)
-   ✅ `Card.cs` - Card input handling (Enhanced from Phase 1)
-   ✅ `HandManager.cs` - Hand layout and card management
-   ✅ `DeckManager.cs` - Demo deck creation and card dealing
-   ✅ `DemoCardCreator.cs` - Creates demo cards for testing

**Implementation Notes:**

-   CardRenderer handles all visual aspects: background, art, suits, text, ability panels
-   CardAbility provides extensible framework for all 10 special abilities
-   HandManager arranges cards in arc layout with proper spacing and animations
-   DeckManager deals initial hands automatically for demo purposes
-   Created CardPrefab with proper UI hierarchy: Background, CardName, CardValue text elements
-   DemoCardCreator generates test cards programmatically (Wildcard, Shield, Trump Changer, Reverser)
-   All systems connected: Player -> HandManager -> CardPrefab -> Card/CardRenderer
-   Demo automatically deals 5 cards to player hand on scene start
-   Added Main Camera positioned at (0, 0, -10) for proper scene viewing
-   UI Canvas configured as Screen Space Overlay with proper scaling
-   DemoUISetup script configures UI components for optimal display

**Phase 2 Complete!** ✅ **DEMO READY:** Cards are properly spaced and displayed with correct data!

**Issues Resolved:**

-   ✅ Fixed card overlapping by increasing cardSpacing to 350f and arcAngle to 60°
-   ✅ Improved arc radius to 350f for better card positioning
-   ✅ Card text now displays correctly (names and values visible)
-   ✅ All 5 cards are dealt and arranged in a clear arc layout

---

### Phase 3: Game Rules Engine

**Duration:** 2-3 weeks  
**Deliverable:** Complete game rules implementation with turn-based mechanics

**Tasks:**

-   ✅ Implement core Durak-inspired rules
-   ✅ Create attack/defense mechanics
-   ✅ Implement trump suit system
-   ✅ Build turn order and player rotation
-   ✅ Create basic game rules validation
-   🔄 Implement card dealing and hand management for 2 players
-   🔄 Add attack/defense UI demonstration

**Key Files:**

-   ✅ `GameRules.cs` - Core game rules (Complete with trump system, card validation)
-   ✅ `TurnManager.cs` - Turn order management (Complete with phase management)
-   ✅ `AttackDefenseSystem.cs` - Attack/defense mechanics (Complete with visual card placement)
-   🔄 `HandManager.cs` - Enhanced for attack/defense interactions
-   🔄 Demo setup for 2-player attack scenario

**Implementation Notes:**

-   GameRules implements core Durak mechanics: trump cards beat non-trump, same suit higher value wins
-   TurnManager handles turn phases (StartTurn, AttackPhase, DefensePhase, EndTurn) and player rotation
-   AttackDefenseSystem manages attack/defense card placement with visual feedback in UI areas
-   Updated Player class with CardData-based hand management for attack/defense validation
-   Created AttackCardArea and DefenseCardArea in UI for visual card placement during bouts
-   Phase3DemoSetup script ready to create 2-player attack demonstration

**Phase 3 Status:** ✅ **COMPLETE & FULLY FUNCTIONAL** - Complete attack/defense system with AI! **TURN SWITCHING FIXED!**

**✅ ALL BASIC GAME RULES IMPLEMENTED:**

-   ✅ **Attack Phase:** Player 0 can attack with any card (first attack) or matching values
-   ✅ **Defense Phase:** AI Player 1 automatically defends with valid cards (higher same suit or trump)
-   ✅ **Trump System:** Hearts ♥ trump cards beat non-trump cards
-   ✅ **Card Validation:** GameRules validates attacks and defenses according to Durak rules
-   ✅ **Bout Resolution:**
    -   If defense succeeds: Cards discarded, defender becomes next attacker
    -   If defense fails: Attacker cards added to defender's hand ("eating cards")
-   ✅ **Visual Feedback:** Cards move from hands to attack/defense areas
-   ✅ **Turn Management:** Proper phase transitions (Attack → Defense → EndTurn)
-   ✅ **AI Defense Logic:** AI always tries to defend if possible, otherwise "eats" cards
-   ✅ **Continuous Play:** After bout completion, game continues with proper turn rotation
-   ✅ **CRITICAL FIX:** Turn validation now works correctly after role swapping
-   ✅ **CRITICAL FIX:** Smart handler setup - AI attacks automatically, humans get click handlers
-   ✅ **Enhanced Debugging:** Detailed turn validation logs show exactly why attacks succeed/fail

**Demo Working Features:**

-   ✅ **Both Player Hands Dealt:** Player 0 (bottom) and Player 1 (top) each have 5 cards
-   ✅ **Hand Visual Separation:** Fixed overlapping hands with proper arc positioning
-   ✅ **Smart Click Handlers:** Human attackers get click handlers, AI attackers attack automatically
-   ✅ **Attack Area Ready:** Cards move to center AttackArea when clicked
-   ✅ **Defense Area Ready:** AI defense cards appear in DefenseArea
-   ✅ **Console Feedback:** Detailed attack/defense logs with card info and validation results
-   ✅ **Visual Card Movement:** Cards move from hands to attack/defense areas
-   ✅ **Game Systems Active:** GameRules, TurnManager, AttackDefenseSystem all running
-   ✅ **Complete Bout Cycle:** Attack → Defense → Resolution → Next Turn
-   ✅ **AI Defense Fixed:** Proper phase transition handling for AI defense
-   ✅ **Turn Progression:** Defender becomes attacker after successful defense
-   ✅ **CRITICAL FIX:** Turn validation debugging shows exactly why attacks succeed/fail
-   ✅ **CRITICAL FIX:** Role-based handler setup prevents "not your turn" errors
-   ✅ **AI Attack Support:** When AI becomes attacker, it automatically chooses and plays cards

**Demo Instructions - READY TO TEST COMPLETE GAME LOOP WITH TURN SWITCHING:**

1. **Start Play Mode** - Both hands automatically dealt (5 cards each)
2. **Wait 3 seconds** - Game handlers automatically set up based on current attacker/defender roles
3. **Initial State:** Player 0 (Human) attacks, Player 1 (AI) defends
4. **Click any Player 0 card** (bottom hand) to trigger attack
5. **Watch Console** - Detailed attack feedback with turn validation info
6. **See Attack Card Movement** - Clicked card moves to center attack area
7. **Watch AI Defense** - AI automatically considers defense after 2-second delay
8. **See Defense Result:**
    - **If AI can defend:** Defense card moves to defense area, bout ends, cards discarded, **AI becomes attacker**
    - **If AI cannot defend:** AI "eats" attack cards (added to AI hand), attack area cleared, **Player 0 can attack again**
9. **Turn Switching Test:**
    - **If AI becomes attacker:** Console shows "=== AI ATTACKS, HUMAN DEFENDS ===" and AI automatically attacks
    - **If Player 0 continues:** Console shows "=== HUMAN ATTACKS, AI DEFENDS ===" and click handlers remain active
10. **Continuous Play** - Game automatically handles role switching and appropriate interaction setup

**Currently Active Cards:**

-   **Player 0:** Trump Changer, Wildcard, 6 of Hearts, Shield Card, Wildcard
-   **Player 1:** The Reverser, Shield Cards (3x), The Reverser
-   **Trump Suit:** Hearts ♥

**Console Output Example (Complete Attack/Defense Cycle WITH TURN VALIDATION):**

```
=== Setting up Game Handlers ===
Current Attacker: Player 0 (ID: 0, Type: Human)
Current Defender: Player 1 (ID: 1, Type: AI)
=== HUMAN ATTACKS, AI DEFENDS ===
=== Human attack handlers ready! Click Player 0's cards (bottom hand) to attack! ===

=== ATTACK INITIATED ===
Player 0 attacks with: [Card Name]
=== TURN VALIDATION ===
Attacker: Player 0 (ID: 0)
Current Player: Player 0 (ID: 0)
Is Player Turn: True
Current Phase: AttackPhase
AttackerIndex: 0, DefenderIndex: 1, CurrentPlayerIndex: 0
=== END TURN VALIDATION ===
Attack validation result: VALID
Card moved to attack area - waiting for defense!

=== AI DEFENSE TRIGGERED ===
[AI Defense Logic...]
=== BOUT COMPLETE ===
Defense successful: True/False

=== Setting up Game Handlers ===
Current Attacker: Player 1 (ID: 1, Type: AI)  [ROLES SWAPPED!]
Current Defender: Player 0 (ID: 0, Type: Human)
=== AI ATTACKS, HUMAN DEFENDS ===
=== AI ATTACK HANDLER ===
AI Player 1 is considering attack...
AI chooses to attack with: [AI Card]
```

**Alternative: Failed Defense Example:**

```
=== AI CANNOT DEFEND ===
AI has no valid defense for [Attack Card]
AI must eat the cards!
=== DEFENSE FAILED ===
Player 1 (AI) must eat the attack cards!
Added [Attack Card] to Player 1 (AI)'s hand
Attack area cleared
=== BOUT COMPLETE ===
Defense successful: False
```

**Ready for Phase 4!** The attack/defense system is complete and fully functional.

---

### Phase 4: Special Abilities System

**Duration:** 2-3 weeks  
**Deliverable:** All 10 special card abilities implemented and functional

**Tasks:**

-   Implement all special abilities from GDD
-   Create ability activation UI
-   Build ability priority system
-   Implement ability restrictions and validation
-   Create visual feedback for abilities
-   Add ability tooltips and explanations

**Key Abilities:**

-   Shield Card, Double Trouble, Trump Changer
-   The Blocker, The Magnet, The Reverser
-   Skip Turn, Extra Draw, Wildcard, Double Defense

**Key Files:**

-   Individual ability scripts for each special card
-   `AbilityManager.cs` - Ability activation controller
-   `AbilityUI.cs` - Ability selection interface

---

### Phase 5: AI System Development

**Duration:** 2-3 weeks  
**Deliverable:** Intelligent AI opponents with personality-driven behavior

**Tasks:**

-   Create AI decision-making framework
-   Implement personality-based AI behaviors
-   Build AI difficulty scaling system
-   Create AI evaluation functions
-   Implement AI reaction system
-   Add AI text-based quips and responses

**Key Files:**

-   `AIPlayer.cs` - AI player controller
-   `AIPersonality.cs` - AI personality definitions
-   `AIDecisionMaker.cs` - AI decision logic
-   `AIReactionSystem.cs` - AI emotional responses

---

### Phase 6: User Interface & Visual Design

**Duration:** 2-3 weeks  
**Deliverable:** Complete UI with Renaissance-themed visual design

**Tasks:**

-   Create main menu and game lobby
-   Design game board with top-down table view
-   Implement player profile displays
-   Create card hand UI at bottom of screen
-   Build trump card and deck display
-   Add player highlighting (red/blue for attacker/defender)
-   Implement hover tooltips and help system

**Key Files:**

-   UI prefabs and canvases
-   `UIManager.cs` - UI state management
-   `PlayerHUD.cs` - Player interface elements
-   `GameBoardUI.cs` - Game table display

---

### Phase 7: Animation & Visual Effects

**Duration:** 2 weeks  
**Deliverable:** Polished animations and visual feedback system

**Tasks:**

-   Create card dealing animations
-   Implement attack/defense card animations
-   Add player highlighting effects
-   Create special ability visual effects
-   Build turn transition animations
-   Add particle effects for game events

**Key Files:**

-   Animation controllers and clips
-   `EffectsManager.cs` - Visual effects controller
-   `CardAnimationController.cs` - Card-specific animations

---

### Phase 8: Audio System Implementation

**Duration:** 1-2 weeks  
**Deliverable:** Complete audio system with Renaissance-themed music and SFX

**Tasks:**

-   Implement background music system (lute/harpsichord)
-   Add sound effects for card interactions
-   Create AI reaction sounds (grunts, laughter, scoffs)
-   Implement dynamic music intensity
-   Add special ability sound cues
-   Create win/lose audio feedback

**Key Files:**

-   `AudioManager.cs` - Audio system controller
-   `MusicController.cs` - Background music management
-   `SFXController.cs` - Sound effects management

---

### Phase 9: Game Flow & Progression

**Duration:** 1-2 weeks  
**Deliverable:** Complete game flow with save system and progression

**Tasks:**

-   Implement game start/end sequences
-   Create round statistics and scoring
-   Build save/load system using JSON
-   Add coins and experience system
-   Implement guided tutorial for new players
-   Create game settings and options

**Key Files:**

-   `SaveSystem.cs` - Game save/load functionality
-   `ProgressionManager.cs` - Player progression tracking
-   `TutorialManager.cs` - New player guidance
-   `GameFlow.cs` - Overall game flow control

---

### Phase 10: Testing, Polish & Optimization

**Duration:** 2-3 weeks  
**Deliverable:** Fully tested, polished game ready for release

**Tasks:**

-   Comprehensive gameplay testing
-   AI behavior testing and balancing
-   Performance optimization
-   Bug fixing and stability improvements
-   Add accessibility features (colorblind support, large text)
-   Final UI/UX polish
-   Build system for PC deployment

**Key Files:**

-   Test scripts and debugging tools
-   Performance profiling results
-   Build configuration files

---

### Phase 11: Multi-player Foundation (Optional Future Phase)

**Duration:** 2-3 weeks  
**Deliverable:** Network multiplayer capability

**Tasks:**

-   Implement network multiplayer using Unity Netcode
-   Create lobby system for online play
-   Add player matchmaking
-   Implement anti-cheat measures
-   Create reconnection handling
-   Add chat system for multiplayer

**Key Files:**

-   Network-enabled versions of core systems
-   `NetworkManager.cs` - Network game management
-   `LobbyManager.cs` - Multiplayer lobby system

---

## Success Criteria

### Minimum Viable Product (MVP)

-   Complete single-player game with AI opponents
-   All 10 special abilities functional
-   Renaissance-themed UI and audio
-   Save/load functionality
-   Tutorial system

### Stretch Goals

-   Network multiplayer
-   Mobile adaptation
-   Additional card expansions
-   Tournament mode
-   Leaderboards and achievements

---

## Technical Architecture Summary

### Core Systems

1. **State Management:** Finite State Machine for game flow
2. **Card System:** ScriptableObject-based card definitions
3. **Ability System:** Component-based special abilities
4. **AI System:** Personality-driven decision making
5. **UI System:** Modular UI components
6. **Save System:** JSON-based local storage

### Design Patterns

-   **Observer Pattern:** For game events and UI updates
-   **Command Pattern:** For ability activation and undo functionality
-   **State Pattern:** For game flow management
-   **Factory Pattern:** For card and ability creation

---

## Resource Requirements

### Art Assets

-   Card designs (standard deck + special abilities)
-   Renaissance-themed UI elements
-   Player avatar portraits
-   Table and environment art
-   Particle effects and animations

### AI-Generated Art Assets

**ComfyUI Integration Available** - See `ComfyUI_Usage_Guide.md` for complete documentation

The project includes a full ComfyUI integration for generating game art assets using AI:

-   **Card Art Generation**: Character portraits, spell effects, magical creatures
-   **UI Element Creation**: Buttons, backgrounds, decorative elements
-   **Game Board Assets**: Background textures, terrain elements
-   **Icon Generation**: Spell symbols, resource indicators
-   **Transparent PNGs**: Advanced background removal for clean game assets

**Quick Start:**

```bash
# Install dependencies
pip install -r requirements.txt

# Start keep-alive script for server
./keep_comfyui_alive.sh --daemon

# Generate single image
python comfyui_client.py --prompt "fantasy card game character portrait" --width 512 --height 512

# Generate multiple variants
python generate_variants.py

# Generate transparent PNG assets
python final_transparent.py
```

**Recommended Asset Organization:**

-   `Assets/Art/Generated/Cards/` - AI-generated card artwork
-   `Assets/Art/Generated/UI/` - AI-generated interface elements
-   `Assets/Art/Generated/Icons/` - AI-generated game icons
-   `Assets/Art/Generated/Backgrounds/` - AI-generated background images

For detailed usage instructions, model selection, and troubleshooting, see `ComfyUI_Usage_Guide.md`.

### Audio Assets

-   Background music (lute/harpsichord compositions)
-   Card interaction sound effects
-   AI reaction sound clips
-   Special ability audio cues
-   Ambient restaurant/pub sounds

### Technical Dependencies

-   Unity 2022.3 LTS or newer
-   Unity UI Toolkit (optional, for advanced UI)
-   DOTween (for animations)
-   Newtonsoft JSON (for save system)

---

## 🚨 Critical Development Rules

### **ALWAYS Check Unity Console Before Completing Tasks**

-   **MANDATORY:** Use `mcp_unitymcp_read_console` tool to check for compiler errors before ending any response
-   **Fix All Errors:** If any compiler errors are found, work to resolve them immediately
-   **No Exceptions:** Never leave the project in a broken state - all code changes must compile successfully
-   **Validation:** Re-check console after fixes to ensure all errors are resolved
-   **Priority:** Compiler errors take precedence over all other tasks

### **NEVER Modify Scene Objects During Play Mode**

-   **MANDATORY:** Always stop play mode before modifying GameObjects, renaming objects, or deleting scene objects
-   **Scene Changes:** All scene modifications must be done in Edit Mode to persist changes
-   **Play Mode Rule:** Use play mode ONLY for testing and observation, never for scene modifications
-   **Workflow:** Stop Play Mode → Make Scene Changes → Enter Play Mode → Test Changes

---

**Last Updated:** Phase 3 completion - Basic game rules fully implemented
**Next Milestone:** Phase 4 - Special Abilities System implementation
