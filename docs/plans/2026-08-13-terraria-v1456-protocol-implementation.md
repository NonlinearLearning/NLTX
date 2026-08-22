# Terraria V1456 Protocol Implementation Plan

> **For Codex:** Implement this plan task-by-task with test-first verification.

**Goal:** Add a Terraria 1.4.5.6-compatible binary protocol adapter that isolates
legacy packet fields from the Arch ECS simulation and provides a real TCP server
handshake path.

**Architecture:** `Terraria.Dome.Protocol.V1456` owns framing, typed packet DTOs,
codec logic, session state and protocol projections. `Terraria.Dome.Server` owns
connections and maps each authorized session slot to a server-owned ECS player.
`Terraria.Dome.Simulation` remains unaware of sockets, message IDs, binary
writers, Terraria slots and legacy `ai[]` fields.

**Tech Stack:** .NET 10, C#, Arch 2.1.0, `TcpListener`, `NetworkStream`,
`BinaryReader`, `BinaryWriter`.

**Reference Evidence:**

- `D:\TRbackup\无任何删减通过编译\Terraria\NetMessage.cs:128-135` reserves
  two bytes, writes a message ID, and writes `Terraria319` for message 1.
- `D:\TRbackup\无任何删减通过编译\Terraria\NetMessage.cs:1686-1693` writes the
  total frame length as a little-endian `ushort`, including the length prefix.
- `D:\TRbackup\无任何删减通过编译\Terraria\MessageBuffer.cs:191-216` accepts
  `Terraria319` before assigning message-3 session state.
- A live local 1.4.5.6 reference server accepted
  `0F-00-01-0B-54-65-72-72-61-72-69-61-33-31-39` and returned
  `05-00-03-01-00`.

---

### Task 1: Create versioned protocol project and framed packet contract

**Files:**

- Create: `src/Terraria.Dome.Protocol.V1456/Terraria.Dome.Protocol.V1456.csproj`
- Create: `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaMessageId.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaFrame.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaFrameCodec.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaProtocolVersion.cs`
- Modify: `Terraria.Dome.sln`
- Modify: `Test/Terraria.Dome.Verification/Terraria.Dome.Verification.csproj`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write the failing verification.**

Add executable assertions which demand that the codec writes exactly the live
reference `Hello` frame and that decoding returns an ID and payload without
accepting truncated or oversized frames.

**Step 2: Run the verifier and confirm RED.**

Run:

```powershell
dotnet run --project Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false
```

Expected: build failure because `Terraria.Dome.Protocol.V1456` does not exist.

**Step 3: Implement the minimum framed codec.**

Use `ushort` little-endian total length, one message-ID byte and a bounded payload.
Reject total lengths below three bytes and payloads above the protocol maximum.

**Step 4: Run the verifier and confirm GREEN.**

Expected: byte-for-byte `Hello` assertion and invalid-frame rejection pass.

### Task 2: Add typed handshake packets and session transition guard

**Files:**

- Create: `src/Terraria.Dome.Protocol.V1456/Packets/HelloPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/SetUserSlotPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Session/TerrariaSessionState.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Session/TerrariaSession.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write failing assertions.**

Demand exact encode/decode of `Hello("Terraria319")` and
`SetUserSlot(slot: 1, isServerSideCharacter: false)`, and require a session to
reject a duplicate hello or gameplay packet before activation.

**Step 2: Run the verifier and confirm RED.**

Expected: missing packet/session symbols.

**Step 3: Implement typed handshake and state machine.**

Only the protocol layer knows `Terraria319`, message IDs and slot bytes. The
session exposes a typed result, never a legacy entity object.

**Step 4: Run the verifier and confirm GREEN.**

Expected: the known response `05-00-03-01-00` and invalid transition checks pass.

### Task 3: Build a protocol compatibility projection boundary

**Files:**

- Create: `src/Terraria.Dome.Protocol.V1456/Compatibility/IPlayerProtocolProjection.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Compatibility/PlayerProtocolState.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Compatibility/PlayerControlPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Compatibility/PlayerControlCodec.cs`
- Create: `src/Terraria.Dome.Server/Protocol/SimulationPlayerProjection.cs`
- Modify: `src/Terraria.Dome.Server/Terraria.Dome.Server.csproj`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write failing projection assertions.**

Use an immutable `PlayerSnapshot` to project a message-13 state. Assert that the
codec uses the legacy packet field order while the projection receives no old
`Player`, `Main`, `whoAmI`, or `ai[]` type.

**Step 2: Run the verifier and confirm RED.**

Expected: missing projection and codec symbols.

**Step 3: Implement the explicit compatibility DTO.**

Map ECS snapshot fields to protocol fields. Decode client message 13 into an
intent-only `PlayerControlPacket`; the server will bind the connection to its own
player and ignore client-supplied entity identity and position authority.

**Step 4: Run the verifier and confirm GREEN.**

Expected: message-13 byte layout and snapshot isolation assertions pass.

### Task 4: Serve the live V1456 handshake over TCP

**Files:**

- Create: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolServer.cs`
- Create: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write failing loopback assertions.**

Connect a raw `TcpClient` to `DomeServer`, write the verified Hello bytes and
require the exact `SetUserSlot` response. Send an invalid hello and require a
clean session close rather than a simulation mutation.

**Step 2: Run the verifier and confirm RED.**

Expected: `DomeServer` still expects JSON lines and the raw Terraria assertion
times out or fails framing.

**Step 3: Replace the production listener path with the protocol host.**

Keep the JSON client as a separately named demo only if it remains useful, but
make `DomeServer` use framed V1456 sessions. The protocol host must create a
server-owned player only after a successful hello; it must not deserialize client
JSON or expose the Arch entity to the connection.

**Step 4: Run loopback verification and compare against reference bytes.**

Expected: raw V1456 hello receives `05-00-03-01-00`; JSON is no longer the
production network path.

### Task 5: Establish the full-message registry and staged coverage contract

**Files:**

- Create: `src/Terraria.Dome.Protocol.V1456/Catalog/TerrariaMessageCatalog.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Catalog/TerrariaMessageSupport.cs`
- Create: `docs/protocol/terraria-v1456-message-catalog.md`
- Modify: `progress.md`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1: Write failing catalog assertions.**

Require every ID in the reference range `1..161` to have one explicit direction,
authority class and support state. Require the initial protocol implementation to
mark only proved handlers as implemented and to reject unsupported inbound packets
without accessing simulation internals.

**Step 2: Run the verifier and confirm RED.**

Expected: catalog does not exist.

**Step 3: Implement the catalog and documentation.**

Use the repository's packet classification audit as the source for direction and
authority. Do not claim a packet is implemented merely because it has an ID.

**Step 4: Run repository verification.**

```powershell
dotnet build .\Terraria.Dome.sln -m:1 -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false
```

Expected: zero errors, and all behavior, framed-protocol, session-isolation and
catalog checks pass.

### Task 6: Capture a Steam-client handshake as external integration evidence

**Files:**

- Create: `docs/protocol/evidence/README.md`
- Modify: `progress.md`

**Step 1: Run the V1456 server on a chosen local port.**

```powershell
dotnet run --project .\src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -- 7777
```

**Step 2: Connect `D:\SoftwarePackage\steam\steamapps\common\Terraria\Terraria.exe`.**

Enter `127.0.0.1:7777`, capture first frames in server diagnostic output, and
compare `Hello` and `SetUserSlot` bytes to the reference evidence.

**Step 3: Record only non-sensitive protocol facts.**

Do not record player identifiers, credentials or world data. Mark remaining world
load, player profile and message coverage stages accurately rather than treating a
successful first handshake as full client-entry proof.
