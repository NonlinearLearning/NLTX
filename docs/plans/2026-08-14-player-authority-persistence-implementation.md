# Player Authority Persistence Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Import original Terraria player bootstrap state once, persist it by
UUID, and restore server-owned inventory, buffs, equipment, and loadouts on
later connections.

**Architecture:** Protocol parsers construct immutable bootstrap records; the
session accumulates them until world entry; the server applies a first import
or restores an existing account record through simulation-owned state. State
format v2 stores account records while retaining a v1 reader.

**Tech Stack:** .NET 10, C#, Arch ECS, binary Terraria V1456 frames, custom
Dome binary persistence, console verification projects.

---

### Task 1: Define protocol bootstrap value types

**Files:**
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/PlayerEquipmentPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/PlayerBuffsPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/PlayerUuidPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/PlayerVitalsPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/PlayerLoadoutPacket.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Test: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write the failing test**

Add assertions which decode the captured IDs 5, 16, 42, 50, 68, and 147. Use
the exact captured payloads, verify every decoded field, and assert malformed
lengths, invalid player-slot ownership, slot 990, negative stack values, and
unterminated buff payloads fail.

**Step 2: Run test to verify it fails**

Run: `dotnet run --project .\Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false`

Expected: FAIL because the bootstrap packet decoder APIs do not exist.

**Step 3: Write minimal implementation**

Add exact binary packet decoding and encoding. Preserve original equipment
fields rather than projecting them to `ItemStack`. Use constants for 990 slots
and the supported buff count. Keep wire validation in Protocol, not Server.

**Step 4: Run test to verify it passes**

Run: `dotnet run --project .\Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false`

Expected: PASS with packet parsing assertions.

### Task 2: Accumulate and freeze the bootstrap session

**Files:**
- Create: `src/Terraria.Dome.Protocol.V1456/Session/PlayerBootstrapState.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Session/TerrariaSession.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatcher.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatchOutcome.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatchResult.cs`
- Test: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write the failing test**

Replay the captured bootstrap sequence after `SyncPlayer`, then issue
`RequestWorldData`. Assert that the result contains a complete immutable
bootstrap state, the session becomes `WorldDataRequested`, and packet 5 is
rejected after `PlayerSpawn`.

**Step 2: Run test to verify it fails**

Run: `dotnet run --project .\Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false`

Expected: FAIL because ID 68 is currently unsupported.

**Step 3: Write minimal implementation**

Route only the six bootstrap packet families while in
`PlayerProfileReceived`. Require a UUID before world data. Freeze a defensive
copy of all 990 slots and buff entries at world request. Do not accept broad
unknown-packet fallthrough.

**Step 4: Run test to verify it passes**

Run: `dotnet run --project .\Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false`

Expected: PASS and unsupported active-session bootstrap input remains rejected.

### Task 3: Introduce simulation-owned persistent player records

**Files:**
- Create: `src/Terraria.Dome.Simulation/Players/PlayerPersistentState.cs`
- Create: `src/Terraria.Dome.Simulation/Players/PlayerPersistentItem.cs`
- Create: `src/Terraria.Dome.Simulation/Players/PlayerPersistentBuff.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- Test: `Test/Terraria.Dome.Items.Verification/Program.cs`

**Step 1: Write the failing test**

Create a complete persistent record, import it for a new UUID, and assert the
first ten inventory slots project into `InventoryComponent` without dropping
the remaining 980 protocol slots. Assert a second import for the same UUID
does not replace stored values.

**Step 2: Run test to verify it fails**

Run: `dotnet run --project .\Test\Terraria.Dome.Items.Verification\Terraria.Dome.Items.Verification.csproj -p:UseSharedCompilation=false`

Expected: FAIL because no account store exists.

**Step 3: Write minimal implementation**

Add a UUID-keyed persistent-record store in `DomeSimulation`, with a
first-import API and a server-record restore API. Keep state ownership in
Simulation. Project only the runtime hotbar range through the existing item
definition registry; retain unsupported items in the raw persistent record.

**Step 4: Run test to verify it passes**

Run: `dotnet run --project .\Test\Terraria.Dome.Items.Verification\Terraria.Dome.Items.Verification.csproj -p:UseSharedCompilation=false`

Expected: PASS with first-import and server-record-wins assertions.

### Task 4: Persist player records in Dome state format v2

**Files:**
- Modify: `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs`
- Modify: `src/Terraria.Dome.Server/Persistence/DomeStateSaveCoordinator.cs`
- Test: `Test/Terraria.Dome.Persistence.Verification/Program.cs`

**Step 1: Write the failing test**

Write a v2 snapshot containing a full account record, read it back, and compare
UUID, profile, vitals, buffs, loadout, all 990 slots, and flags. Add a v1
fixture proving an empty account collection is still accepted.

**Step 2: Run test to verify it fails**

Run: `dotnet run --project .\Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj -p:UseSharedCompilation=false`

Expected: FAIL because the format only supports version 1.

**Step 3: Write minimal implementation**

Read both v1 and v2. Write only v2. Serialize bounded account counts in stable
UUID order, write each 990-slot record, and reject duplicates, malformed UUIDs,
count overflows, invalid slots, and trailing bytes.

**Step 4: Run test to verify it passes**

Run: `dotnet run --project .\Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj -p:UseSharedCompilation=false`

Expected: PASS with v1 compatibility and v2 round-trip assertions.

### Task 5: Apply authoritative account policy during TCP world entry

**Files:**
- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolCommand.cs`
- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Test: `Test/Terraria.Dome.PlayerAuthority.Verification/Program.cs`

**Step 1: Write the failing test**

Connect once with a captured UUID and non-default bootstrap item data. Connect
again using the same UUID and intentionally different item data. Assert the
second session receives and activates with the original server-owned record.

**Step 2: Run test to verify it fails**

Run: `dotnet run --project .\Test\Terraria.Dome.PlayerAuthority.Verification\Terraria.Dome.PlayerAuthority.Verification.csproj -p:UseSharedCompilation=false`

Expected: FAIL because session creation does not carry bootstrap state.

**Step 3: Write minimal implementation**

Pass frozen bootstrap state through a typed server command. Apply a first
import or existing-record restoration on the simulation tick. Before initial
world frames, send account profile, vital values, buffs, loadout, and every
equipment slot. Preserve existing player ownership and replication queues.

**Step 4: Run test to verify it passes**

Run: `dotnet run --project .\Test\Terraria.Dome.PlayerAuthority.Verification\Terraria.Dome.PlayerAuthority.Verification.csproj -p:UseSharedCompilation=false`

Expected: PASS with first-import and subsequent server-record-wins assertions.

### Task 6: Verify the original client path

**Files:**
- Test artifact: `Build/diagnostics/terraria-7777-frame-trace.log`
- Test: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Run focused verification projects**

Run the Task 1, 2, 3, 4, and 5 verification projects serially with
`-p:UseSharedCompilation=false`.

Expected: all exit with code 0.

**Step 2: Restart the traced local endpoint**

Run the server on its direct `127.0.0.1:7777` endpoint or retain the existing
transparent proxy configuration while checking the captured original-client
traffic.

**Step 3: Connect the original Terraria client twice**

Require the first connection to pass beyond the bootstrap frames and the second
connection to receive server-restored values despite changed local state.

**Step 4: Record evidence**

Require the trace to show `RequestWorldData` after bootstrap on both
connections, no EOF caused by ID 68, and server-to-client account state frames
before world entry.
