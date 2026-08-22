# Session Lifecycle Disconnect Implementation Plan

> **For Codex:** Implement each task in order. Do not write production code before the
> named regression assertion has failed against the current server.

**Goal:** A player can disconnect, a replacement player can enter, move, use an item, and
remain connected without stopping the authoritative simulation loop.

**Architecture:** The simulation thread is the sole owner of player and replication maps.
Both socket termination and replication-write failure request removal through one idempotent
operation keyed by the exact `SessionReplicationState` instance. A stale request must not
remove a newly owned slot. An unexpected simulation-loop fault remains observable rather than
turning future session commands into indefinite waits.

**Tech Stack:** .NET 10, `DomeServer`, TCP loopback verification projects, Terraria V1456
packet codec.

---

### Task 1: Lock down disconnect and replacement-session behavior

**Files:**
- Modify: `Test/Terraria.Dome.SessionReplication.Verification/Program.cs`
- Test: `Test/Terraria.Dome.SessionReplication.Verification/Program.cs`

**Step 1: Write the failing behavior assertion**

After the first TCP client is disposed and its player count reaches zero, activate the
replacement client, submit rightward player controls followed by an item-use control, and
require all of the following:

- simulation ticks continue increasing;
- the replacement client receives the PVS section entered by movement;
- the replacement connection answers a Terraria `Ping` frame after the item-use control.

**Step 2: Run the focused verifier and record the RED result**

Run:

```powershell
dotnet run --project Test/Terraria.Dome.SessionReplication.Verification/Terraria.Dome.SessionReplication.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected before the fix: the replacement flow times out or fails to receive the expected
section or ping response.

### Task 2: Give session removal one owner and an identity check

**Files:**
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs` only if its
  teardown request needs to carry the existing replication-state identity.

**Step 1: Implement a single removal operation**

Replace direct removal logic in `DestroySessionPlayerCommand` and `RemoveFailedSessions` with
one private operation accepting `playerSlot` and `SessionReplicationState`. The operation must:

1. Return without side effects unless the slot currently maps to that exact state instance.
2. Remove replication ownership before disposal.
3. Remove and destroy the corresponding player once, closing its chest only when that player
   was actually owned.
4. Dispose the removed state once.

**Step 2: Keep the simulation loop diagnosable**

Retain cancellation handling. Capture any other loop exception in an internal/publicly
observable fault property and ensure queued command completions receive a prompt exception
instead of waiting forever. Do not restart the loop or silently swallow a fault.

### Task 3: Validate write-failure and explicit-disconnect ordering

**Files:**
- Modify: `Test/Terraria.Dome.Hardening.Loopback.Verification/Program.cs` only if its
  existing slow-reader assertion does not exercise the stale teardown ordering.
- Test: `Test/Terraria.Dome.Hardening.Loopback.Verification/Program.cs`

**Step 1: Run the isolation verifier**

Run:

```powershell
dotnet run --project Test/Terraria.Dome.Hardening.Loopback.Verification/Terraria.Dome.Hardening.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected after the fix: a slow reader is removed, the healthy client remains active, and ticks
continue.

### Task 4: Validate full protocol behavior

**Files:**
- Test: `Test/Terraria.Dome.FullClientBootstrap.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Run focused serial verification**

Run the session replication, hardening loopback, full-client bootstrap, and root verifier from
the repository root with `-p:UseSharedCompilation=false`.

**Step 2: Report boundaries precisely**

Do not claim the already-running server on port 7778 was changed in place. It is a stale,
faulted process. A fresh built process and the synthetic V1456 sequence prove the server-side
repair; a new external client session is still required to prove the existing game executable
uses the new binary.
