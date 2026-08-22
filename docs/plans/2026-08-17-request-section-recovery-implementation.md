# Request Section Recovery Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Restore V1456 client-requested map-section delivery after a client has entered a Dome world.

**Architecture:** Model message 159 as a typed active-session request. The protocol dispatcher
validates and routes it, while `WorldSectionReplication` creates one authoritative section
snapshot and the session host writes it only to the requesting connection. The session cursor
prevents duplicate delivery until the section leaves visibility or its version changes.

**Tech Stack:** .NET 10, C#, Terraria V1456 TCP frames, existing loopback verification project.

---

### Task 1: Capture the missing section-response behavior

**Files:**
- Modify: `Test/Terraria.Dome.World.Loopback.Verification/Program.cs`

**Step 1: Write the failing test**

After the existing initial world stream, finish player activation. Send raw V1456 frame
`RequestSection` (`id=159`) for section `(0, 0)`, which is outside the initial spawn neighborhood.
Require the next relevant server response to be a `TileSection` whose decompressed origin is
`(0, 0)`.

**Step 2: Run test to verify it fails**

Run: `dotnet run --project Test\\Terraria.Dome.World.Loopback.Verification\\Terraria.Dome.World.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false`

Expected: failure because message 159 has no NLTX session route and the server closes the request
session instead of replying with a section.

### Task 2: Add typed RequestSection protocol support

**Files:**
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/RequestSectionPacket.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaMessageId.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaMessageCatalog.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Session/TerrariaSession.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatchOutcome.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatcher.cs`

**Step 1: Implement the narrow protocol boundary**

Define `RequestSection = 159`. Decode exactly four payload bytes as two little-endian unsigned
section coordinates; reject every other length. Mark the catalog entry client-to-server and
handled. Permit it only in the active session state and return a distinct dispatch outcome.

**Step 2: Run focused verifier**

Run the Task 1 command. Expected: it still fails because no server response has been implemented.

### Task 3: Project one requested authoritative section

**Files:**
- Modify: `src/Terraria.Dome.Server/Replication/WorldSectionReplication.cs`
- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`

**Step 1: Implement the minimal response**

Validate requested section coordinates against the authoritative world grid. If valid and not
current in the requesting session cursor, encode exactly one `TileSection` from the authoritative
snapshot plus existing per-section chest state. Invalid and already-current requests produce no
frame and do not terminate the session.

**Step 2: Run the focused verifier**

Run the Task 1 command. Expected: PASS, including origin `(0, 0)` and a surviving TCP session.

### Task 4: Verify affected world-entry behavior

**Files:**
- No source changes expected

**Step 1: Run scoped regressions**

Run:

```text
dotnet run --project Test\\Terraria.Dome.World.Protocol.Verification\\Terraria.Dome.World.Protocol.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.World.Loopback.Verification\\Terraria.Dome.World.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.SessionReplication.Verification\\Terraria.Dome.SessionReplication.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet build Terraria.Dome.sln -c Release -m:1 -p:UseSharedCompilation=false
```

**Step 2: Review the changed paths**

Confirm the new response is scoped to the requester, section coordinates are bounded, and no
generated output appears outside the repository `Build/` policy locations.
