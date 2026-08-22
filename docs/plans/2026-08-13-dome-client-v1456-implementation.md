# Dome Client V1456 Implementation Plan

> **For Codex:** Implement this plan task-by-task with test-first verification.

**Goal:** Replace DomeClient's JSON-line transport with a Terraria 1.4.5.6 V1456 client session.

**Architecture:** DomeClient owns a client-side V1456 bootstrap state machine. It uses typed
packet encoders from Terraria.Dome.Protocol.V1456, retains only connection/session facts, and
does not expose transport DTOs or protocol details to Terraria.Dome.Simulation. DomeServer
remains server-authoritative and unchanged.

**Tech Stack:** .NET 10, TcpClient, NetworkStream, Terraria V1456 framed protocol.

---

### Task 1: Define the V1456 client session contract

**Files:**
- Create: `src/Terraria.Dome.Client/DomeClientInput.cs`
- Create: `src/Terraria.Dome.Client/DomeClientSession.cs`
- Modify: `src/Terraria.Dome.Client/Terraria.Dome.Client.csproj`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1:** Add a verifier that connects DomeClient to DomeServer and requires a V1456 active
session, world data, 15 tile sections, and a server-assigned player slot.

**Step 2:** Run the verifier and observe failure because DomeClient still sends JSON.

**Step 3:** Add client-owned input/session DTOs and change the project reference from Transport
to Protocol.V1456.

**Step 4:** Re-run the verifier and keep it failing until the actual framed session is implemented.

### Task 2: Implement typed client packet encoding

**Files:**
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1:** Add verifier assertions that client-required packet types encode to their Terraria
message IDs: RequestWorldData, SpawnTileData, PlayerSpawn, PlayerControls, and bootstrap packets.

**Step 2:** Implement minimum typed encoders in the protocol project.

**Step 3:** Re-run packet and client verification.

### Task 3: Replace DomeClient's JSON session

**Files:**
- Modify: `src/Terraria.Dome.Client/DomeClient.cs`
- Modify: `src/Terraria.Dome.Client/Program.cs`
- Modify: `Test/Terraria.Dome.Verification/Program.cs`

**Step 1:** Implement the exact bootstrap sequence:
`Hello -> SetUserSlot -> SyncPlayer -> bootstrap packets -> RequestWorldData -> WorldData ->
SpawnTileData -> status/tile/initial world stream -> PlayerSpawn -> PlayerSpawn confirmation ->
FinishedConnecting`.

**Step 2:** Encode a caller input as message 13 after active state and return the retained session
facts rather than a JSON server snapshot.

**Step 3:** Verify a `MoveRight` input changes the server-owned snapshot; verify both projects
build without Transport as a DomeClient dependency.

### Task 4: Verify the complete replacement

**Files:**
- Modify: `progress.md`

**Step 1:** Run serial solution build.

**Step 2:** Run V1456 verification through DomeClient and the existing raw-frame isolation tests.

**Step 3:** Record exact exit codes and remaining scope: DomeClient V1456 compatibility is
verified; Steam Terraria integration remains separately blocked by the existing injection timing
issue and is not modified in this task.
