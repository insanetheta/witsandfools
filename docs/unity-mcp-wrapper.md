# Unity MCP Wrapper (v2) — Why It Exists and How to Work With It

**TL;DR:** Unity MCP "disconnects" were never the relay dropping on its own — they were the
client wedging permanently every time the relay process died. The wrapper
(`scripts/unity-mcp-wrapper.py`, wired up in `.mcp.json`) makes relay deaths heal in ~1
second, invisibly. A few calling-discipline rules stop the relay from being killed in the
first place.

This is the same wrapper proven in the GameEngineStandalone project (see
`GameEngineStandalone/docs/unity_mcp_stability_v2.md` for the full deep-dive with defect
analysis and verification scenarios). This copy differs only in its pinned project path.

---

## What breaks with a direct relay connection

Connecting MCP straight to the relay binary
(`Library/PackageCache/com.unity.ai.assistant@*/RelayApp~/relay_mac_arm64.app/...`) has three
failure modes:

1. **In-flight calls hang forever.** When the relay process dies mid-call, nothing ever
   answers the pending JSON-RPC ids. The MCP client sits on a dead promise until its own
   timeout — surfacing as "MCP disconnected" and requiring a manual `/mcp` reconnect.
2. **Requests sent while the relay is down are silently dropped** (written to a dead pipe),
   so the next call after a crash also hangs.
3. **A hung (not dead) relay wedges everything indefinitely** — nothing detects it.

The most common relay killer: the Unity AI Assistant 2.9.0-pre.2 **namespace validator**
crashes *all* MCP connections whenever a `Unity_RunCommand` script touches
`System.Reflection` — including indirectly (`Enum.GetValues()`, `Type.GetMethod()`). With a
direct connection, that's a manual reconnect every time. Historically this meant a
disconnect every 10–20 minutes of active use.

## What the wrapper does

| Direct-relay failure | Wrapper v2 behavior |
|----------------------|---------------------|
| In-flight calls hang on relay death | **Pending-id tracking**: each in-flight call immediately gets a synthesized *retryable* JSON-RPC error ("Unity relay restarted… please retry"). Retry once ~2s later and it works. |
| Requests dropped during restart | **Never-drop inbox queue**: requests arriving mid-restart are queued and replayed once the new relay is up. |
| Hung relay wedges forever | **45s watchdog** kills hung relays; restart takes ~1s. |
| Attaches to whichever editor it finds | **Project-path pinning** — the wrapper always launches the relay against THIS project. |

**Why restarts are invisible:** a freshly restarted relay serves requests *without* needing
the MCP `initialize` handshake (it returns all tools immediately). So a relay restart is
protocol-transparent — the AI session never notices it happened.

## Configuration

`.mcp.json` (project root):

```json
{
  "mcpServers": {
    "unity-mcp": {
      "command": "/opt/homebrew/bin/python3",
      "args": ["/Users/greg/projects/witsandfools/scripts/unity-mcp-wrapper.py"]
    }
  }
}
```

The wrapper resolves the Unity project root relative to its own location (`UNITY_PROJECT`
constant near the top of the script). If you copy this wrapper to another project, update
that constant — everything else is project-agnostic.

**Do not** replace this with a direct relay command. If the relay binary path changes after
a package update, the wrapper finds it automatically; there is nothing to fix in the config.

## Rules for AI agents (and humans) using Unity MCP here

1. **"Unity relay restarted … please retry"** is normal self-healing — retry the call once
   after ~2 seconds. Only if calls *keep* failing (or the Unity tools vanish entirely) is a
   manual `/mcp` reconnect warranted.
2. **Never use `System.Reflection` in `Unity_RunCommand`** — not as a `using`, not fully
   qualified, not via reflective helpers. Each occurrence crashes every MCP connection.
   Use direct type access, `GetType().Name` string comparison, `[MenuItem]` methods via
   `Unity_ManageMenuItem`, or compiled Editor scripts instead.
3. **Never edit C# while Play Mode is active.** Auto-recompile triggers a domain reload that
   destroys coroutines and bridge state. Always: Stop → Edit → Verify compilation → Clear
   console → Play.
4. **Never hold an MCP call open for minutes.** Long waits (test suites, builds) should poll
   a result file from a background shell loop; keep MCP calls short.
