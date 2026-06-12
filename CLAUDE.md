# Project Instructions for AI Agents

This file provides instructions and context for AI coding agents working on this project.

<!-- BEGIN BEADS INTEGRATION v:1 profile:minimal hash:ca08a54f -->
## Beads Issue Tracker

This project uses **bd (beads)** for issue tracking. Run `bd prime` to see full workflow context and commands.

### Quick Reference

```bash
bd ready              # Find available work
bd show <id>          # View issue details
bd update <id> --claim  # Claim work
bd close <id>         # Complete work
```

### Rules

- Use `bd` for ALL task tracking — do NOT use TodoWrite, TaskCreate, or markdown TODO lists
- Run `bd prime` for detailed command reference and session close protocol
- Use `bd remember` for persistent knowledge — do NOT use MEMORY.md files

## Session Completion

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:
   ```bash
   git pull --rebase
   bd dolt push
   git push
   git status  # MUST show "up to date with origin"
   ```
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**
- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds
<!-- END BEADS INTEGRATION -->


## Build & Test

_Add your build and test commands here_

```bash
# Example:
# npm install
# npm test
```

## Architecture Overview

_Add a brief overview of your project architecture_

## Unity MCP Rules

### Connection: self-healing wrapper v2 (NEVER connect to the relay directly)
This project's `.mcp.json` runs Unity MCP through **`scripts/unity-mcp-wrapper.py`** (wrapper v2), NOT the raw relay binary from the Unity package cache. The wrapper makes relay crashes self-healing:

- If the relay dies, in-flight calls return a fast **retryable error** ("Unity relay restarted … please retry") instead of hanging — **retry the call once after ~2s** before involving the user.
- Requests sent during a restart are queued and replayed, never dropped. Restart takes ~1s.
- A 45s watchdog kills hung relays. Manual `/mcp` should almost never be needed — only if calls KEEP failing or the tools vanish entirely.
- The wrapper pins the relay to THIS project (`--project-path`), so it cannot attach to another open Unity editor.

Do NOT "fix" the config by pointing it back at `Library/PackageCache/com.unity.ai.assistant@*/RelayApp~/...` — that is the old direct-relay setup, and every relay crash then becomes a permanent hang requiring manual reconnection. Full background: `docs/unity-mcp-wrapper.md`.

### NEVER use System.Reflection in Unity_RunCommand
When executing C# via `Unity_RunCommand`, **never** call methods that use `System.Reflection` internally (e.g., `Enum.GetValues()`, `Type.GetMethod()`, assembly scanning) — the package's namespace validator crashes ALL MCP connections on every occurrence. With wrapper v2 this recovers in ~1s (retry once), but avoid triggering it: call existing `[MenuItem]` methods via `Unity_ManageMenuItem`, or write non-reflective code in `RunCommand`.

### Never edit C# while Play Mode is active
Unity auto-recompiles on save; the domain reload kills coroutines and the MCP bridge state. Always: Stop → Edit → Verify compilation → Clear console → Play.

### Never hold an MCP call open for minutes
For long waits (test runs, builds), poll an output/result file from a background shell loop instead of blocking inside an MCP call.

## Conventions & Patterns

### UI Screen Registry Rule
When adding a new screen or modal to the game:
1. Add an entry to `UIScreen` enum in `Assets/Scripts/Runtime/UIScreenCapture.cs`
2. Add a `UIScreenCapture.Instance?.NotifyModal(UIScreen.YourScreen)` or `NotifyPhaseChange` call at the point the screen becomes visible
3. The UI audit report (menu: Wits and Fools > UI Audit > Capture All Screens) will flag missing screens
4. Design targets live in `docs/design/walkthrough/index.html` and `docs/design/ui_screens_mockup.html`
