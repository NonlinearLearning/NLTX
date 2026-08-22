# Dome Full Server Packet Parity Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make the Dome server reproduce the complete server's full-client `join-stable`
packet exchange under the recorded 2026-08-16 scenario, including exact directional counts.

**Architecture:** Retain the existing separation of responsibilities: the protocol session host
owns handshake state and initial world streaming, the simulation loop owns authoritative state,
and `SessionReplicationState` serializes all socket writes. First obtain a fresh raw trace from
the current source build, then repair the earliest divergent state transition or write path.
Packet-count parity is an observable result of real bootstrap and replication work, never a
counter-driven frame emitter.

**Tech Stack:** .NET 10, C#, Terraria V1456 packet codec, loopback TCP, Python standard-library
trace proxy and summarizer.

---

### Task 1: Establish a current-source baseline

**Files:**
- Read: `docs/server-completion/2026-08-16-dome-full-client-packet-trace.md`
- Read: `docs/server-completion/2026-08-16-full-client-server-packet-statistics.md`
- Read: `Build/diagnostics/full_session_trace.py`
- Create: `Build/diagnostics/dome-parity-<timestamp>/trace.jsonl`
- Create: `Build/diagnostics/dome-parity-<timestamp>/summary.json`
- Create: `Build/diagnostics/dome-parity-<timestamp>/summary.md`

**Step 1: Build the current server and full-client bootstrap verifier.**

Run from the repository root:

```powershell
dotnet build Terraria.Dome.sln -p:UseSharedCompilation=false
```

Expected: exit code `0`; record every warning and the actual `Build/bin` paths.

**Step 2: Run the in-process full-client bootstrap verifier.**

```powershell
dotnet run --project Test/Terraria.Dome.FullClientBootstrap.Verification \
  -p:UseSharedCompilation=false
```

Expected: either a protocol failure identifying the first missing state transition, or a pass
which proves the synthetic flow but not the executable client's real ordering.

**Step 3: Start the built Dome server behind the transparent frame recorder.**

Use separate locally allocated ports, a fresh timestamped `Build/diagnostics/dome-parity-*`
directory, and only terminate processes started for this capture.

**Step 4: Run the full Terraria client `join-stable` scenario through the recorder.**

Preserve client result JSON and server stdout/stderr beside the raw trace.

**Step 5: Summarize and compare.**

Run `summarize_full_session_trace.py` and compare C2S/S2C totals, ID counts, and chronological
first divergence against the complete-server summary. A synthetic verifier pass does not close
this task.

### Task 2: Make the earliest handshake divergence executable-client compatible

**Files:**
- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Session/TerrariaSession.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Test: `Test/Terraria.Dome.FullClientBootstrap.Verification/Program.cs`

**Step 1: Add a failing verifier assertion for the first baseline-only handshake frame.**

Use the captured `S2C messageId=82` after `SetUserSlot` as the initial assertion only if the
fresh trace confirms it remains the earliest difference. Assert both order and exact payload
shape rather than only the ID.

**Step 2: Run the narrowed verifier and confirm the expected failure.**

```powershell
dotnet run --project Test/Terraria.Dome.FullClientBootstrap.Verification \
  -p:UseSharedCompilation=false
```

**Step 3: Implement the minimal protocol-state transition and writer call.**

Emit the exact V1456 frame from the session host at the same state boundary used by the baseline.
Do not mark a session active, create a simulation player, or send world data before the client
has completed the corresponding bootstrap input.

**Step 4: Run the verifier again.**

Expected: the new assertion passes and pre-existing bootstrap ordering assertions remain valid.

### Task 3: Repair initial-world flow and state visibility

**Files:**
- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`
- Modify: `src/Terraria.Dome.Server/Replication/SessionReplicationState.cs`
- Modify: `src/Terraria.Dome.Server/Replication/WorldSectionReplication.cs`
- Test: `Test/Terraria.Dome.FullClientBootstrap.Verification/Program.cs`

**Step 1: Add a failing end-to-end assertion for the observed first missing response.**

After the full 350-item bootstrap payload, assert `RequestWorldData(6)` receives `WorldData(7)`
within the bounded read timeout. Then assert `SpawnTileData` receives status text, 15 tile
sections, and initial spawn in order.

**Step 2: Run the verifier to capture the failure.**

**Step 3: Correct the causative state or write-queue failure.**

The repair must preserve one serialized socket writer and surface asynchronous writer failures
to the session host. It must not swallow failed writes and continue accepting the client as
active.

**Step 4: Run the verifier to green.**

Expected: world data, all initial sections, initial spawn, connection completion, and authority
projection pass under an actual TCP session.

### Task 4: Align ongoing replication with the complete session

**Files:**
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Modify: one narrowly identified `src/Terraria.Dome.Server/Replication/*.cs` cursor or assembler
- Test: the relevant existing `Test/Terraria.Dome.*.Verification/Program.cs`

**Step 1: Derive the first post-bootstrap count mismatch from the fresh trace.**

Use a chronological comparison, not only aggregate IDs. The baseline's dominant steady-state
frames are `TileSquare(20)`, `SyncNPC(23)`, `PlayerControls(13)`, and `TileSection(10)`.

**Step 2: Add a focused failing loopback assertion for the missing replication trigger.**

Ensure it drives authentic world or simulation state, such as a tile change or NPC movement.

**Step 3: Implement only the missing trigger/cursor behavior.**

Do not increase send frequency globally to match a count. Preserve queue limits and 16ms tick
cadence.

**Step 4: Run affected verifier projects and the full bootstrap verifier.**

### Task 5: Prove full-client packet parity

**Files:**
- Create: `Build/diagnostics/dome-parity-<timestamp>/trace.jsonl`
- Create: `Build/diagnostics/dome-parity-<timestamp>/summary.json`
- Create: `Build/diagnostics/dome-parity-<timestamp>/summary.md`
- Modify: `docs/server-completion/<timestamp>-dome-full-client-packet-parity.md`

**Step 1: Build all changed projects serially.**

```powershell
dotnet build Terraria.Dome.sln -p:UseSharedCompilation=false
```

**Step 2: Run the selected verification projects serially.**

Include bootstrap, completion, loopback, world, and replication projects that cover changed
paths. Record each exit code.

**Step 3: Capture a clean full executable-client session.**

Use the same client automation, client build, proxy, timing window, and summary program as the
2026-08-16 baseline. Keep the server process, proxy process, and client process IDs in the
evidence folder.

**Step 4: Check exact parity.**

Require `C2S=360`, `S2C=1,197`, 11 C2S types, 19 S2C types, 30 total types, zero invalid frames,
and a successful `join-stable` result. If world contents or timer drift change any total, diagnose
and eliminate that nondeterminism before claiming completion.

**Step 5: Write the evidence report.**

Link the fresh raw trace and generated summary. Include the old versus new counts, first
divergence, source files changed, every verification command's exit status, and residual
warnings. Do not report parity based on a synthetic test alone.
