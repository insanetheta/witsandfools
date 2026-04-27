# Wits and Fools — Unity Development Plan

> **Status (2026-04-26):** Prototype rebuilt from scratch (commit `4e0e4f5` and onward). The previous build was unsalvageable per audit (36+ fixer scripts patching incomplete core systems, claimed phases didn't actually work). This plan reflects the rebuild, not the original phased plan.

## Goal

A polished 2-player vs AI prototype of core Durak rules. **No special abilities.** No multi-player. Programmer art, but readable and animated. Playable from start to finish: deal hands, attack, defend, eat-or-discard, refill, win.

## Architecture (current)

### Runtime (`Assets/Scripts/Runtime/`)

| File | Responsibility |
| --- | --- |
| `Suit.cs`, `Rank.cs`, `Card.cs` | Value types — card identity. |
| `Deck.cs`, `Hand.cs` | Collections backing the engine. |
| `Bout.cs` | Attack/defense pairs for the active bout. |
| `Rules.cs` | Pure functions: `Beats`, `CanAttackWith`, `CanDefendSlotWith`. |
| `GameEngine.cs` | Authoritative state + `TryAttack` / `TryDefend` / `TryEat` / `TryEndBout`. Fires events on every transition. |
| `PlayerController.cs` (`IPlayerController`) | Abstract player. `RequestAction` is called when it's that player's turn. |
| `HumanPlayer.cs` | No-op — UI drives input. |
| `AIPlayer.cs` | Simple greedy policy: cheapest legal defense, lowest non-trump attack, stops piling on with trumps. |
| `GameLoop.cs` | Wires engine events to a deferred `Tick()` so visuals run before AI acts. |
| `GameManager.cs` (`MonoBehaviour`) | Bootstraps everything in `Start`, ticks the loop in `Update`, owns the visual mapping (cards → CardViews). |
| `CardView.cs`, `HandLayout.cs` | Card prefab logic + horizontal fan layout. |
| `TableView.cs`, `HudView.cs` | Scene anchors and HUD references. |

### Editor (`Assets/Scripts/Editor/`)

- `PrefabBuilder.cs` — `Wits and Fools/Build/Card Prefab` rebuilds `Assets/Prefabs/CardView.prefab`.
- `SceneBuilder.cs` — `Wits and Fools/Build/Scene (GameScene)` rebuilds `Assets/Scenes/GameScene.unity` with all wiring (camera, canvas, hands, deck/trump/discard/bout slots, HUD, GameManager).
- `EngineSmokeTest.cs` — `Wits and Fools/Smoke Test/Play 50 Games (Greedy AIs)` runs 50 deterministic engine-only games. Healthy result: 0 stalls, balanced wins, ~13 turns/game.
- `TmpEssentialsImporter.cs` — Ensures TMP fonts are imported before scene build.

## Implementation status

| Area | State |
| --- | --- |
| Core data model (Card / Suit / Rank / Deck) | ✅ |
| Rules engine (events, attack/defense/eat/end) | ✅ — passes 50-game smoke. |
| Player abstraction + simple AI | ✅ |
| Card prefab + face/back rendering | ✅ |
| Hand fan layout, opponent face-down hand | ✅ |
| Trump card / deck stack / discard slot visuals | ✅ |
| Bout layout (attack + offset defense per slot) | ✅ |
| HUD (turn, deck count, trump, end-bout / take-cards button, game over) | ✅ |
| Card animations (move-to-bout, move-and-destroy on discard, absorb) | ✅ |
| Input: click-to-play, hover highlight, valid-target highlight, disabled-state visuals | ✅ |
| Scene wiring + GameManager bootstrap | ✅ |
| Human-in-the-loop smoke test | ⚠️ pending — open as `witsandfools-p63`. |

## Out of scope for this prototype

- 3+ players, networking, lobby, save/load.
- Special-ability cards (Shield, Wildcard, Trump Changer, etc.).
- Renaissance art, audio, music, tutorials, progression.
- Animations beyond functional readability (no particle effects, no SFX).

These are deferred until the core game proves fun to play.

## Conventions

- All gameplay logic lives in `WitsAndFools` namespace under `Assets/Scripts/Runtime/`. No singletons-from-Awake — `GameManager.Start()` is the single entry point.
- The engine never re-enters itself inside an event handler. UI-side responses to engine events should be visual only; subsequent engine actions (especially the AI's) happen in `GameManager.Update` via `GameLoop.Tick()`.
- `GameLoop.Tick()` respects `AiThinkSeconds` so play feels deliberate rather than instant.
- Build the scene/prefab via the editor menu, never hand-edit the .unity/.prefab YAML.

## Workflow

- Issues tracked in **beads** (`bd ready`, `bd close`, etc.). The epic is `witsandfools-6jh`.
- Engine validation: run the 50-game smoke test before merging engine changes.
- UI validation: enter Play mode in `GameScene.unity` and play 5+ rounds (covered by `witsandfools-p63`).

## Open issues

Track via `bd list --status=open`. Remaining for the prototype epic:

- `witsandfools-p63` (P1) — Human-in-the-loop smoke test.
- `witsandfools-15l` (P3) — Keep this plan in sync with reality (this doc).
