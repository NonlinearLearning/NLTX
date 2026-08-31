# Full Client KillProjectile Compatibility Implementation Plan


**Goal:** Keep a Terraria 1.4.5.6 client connected after world entry by safely
accepting its legitimate `KillProjectile` message 29.

**Architecture:** The protocol layer decodes the exact three-byte message 29
payload. The session layer accepts it only after activation and only when the
payload owner is the server-assigned player slot. Dome has no authoritative
mapping for client-created projectiles, so a valid client termination is a
validated compatibility notification; it cannot mutate or broadcast a
server-owned projectile.

**Tech Stack:** .NET 10, C#, Terraria V1456 framed protocol, executable
verification projects.

---

### Task 1: Define the client termination protocol boundary

**Files:**
- Create: `src/Terraria.Dome.Protocol.V1456/Packets/ClientProjectileTermination.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaMessageCatalog.cs`
- Test: `Test/Terraria.Dome.Combat.Protocol.Verification/Program.cs`

**Step 1: Write the failing verifier assertions.**

Assert that the captured client frame `[0x06, 0x00, 0x1D, 0x01, 0x00, 0x05]`
decodes as identity `1` and owner `5`, and that a trailing byte is rejected.
Assert that `KillProjectile` is bidirectional and handled.

**Step 2: Run the verifier and confirm the assertions fail.**

Run:

```powershell
dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected: failure because client message 29 has no typed decoder and is marked
server-to-client only.

**Step 3: Implement the smallest exact parser.**

Add an immutable `ClientProjectileTermination` record with `short Identity`
and `byte Owner`. Add `DecodeClientProjectileTermination`, requiring message
29 and exactly three payload bytes. Mark message 29 bidirectional while keeping
its support level handled.

**Step 4: Run the verifier and confirm it passes.**

Run the command from Step 2. Expected: exit code `0`.

### Task 2: Route only owned active-session terminations

**Files:**
- Modify: `src/Terraria.Dome.Protocol.V1456/Session/TerrariaSession.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatcher.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatchOutcome.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatchResult.cs`
- Test: `Test/Terraria.Dome.Combat.Protocol.Verification/Program.cs`

**Step 1: Write the failing session/dispatcher assertions.**

After a valid active-session bootstrap, dispatch the captured message 29 and
assert the session stays active and returns a dedicated compatibility outcome.
Dispatch the same identity with another player slot and assert rejection.

**Step 2: Run the verifier and confirm the assertions fail.**

Run the Task 1 command. Expected: message 29 is rejected by the dispatcher.

**Step 3: Implement the session boundary and dispatch route.**

Add `AcceptClientProjectileTermination` to `TerrariaSession`. It must require
`Active`, decode through the protocol codec, and require `Owner` to match the
assigned player slot. Route the result through a dedicated dispatch outcome.
The host deliberately performs no simulation mutation or outbound broadcast:
only server-owned projectiles can affect authoritative state.

**Step 4: Run the verifier and confirm it passes.**

Run the Task 1 command. Expected: exit code `0` and both positive and forged
owner cases covered.

### Task 3: Replay the recorded full-client disconnect boundary

**Files:**
- Modify: `Test/Terraria.Dome.FullClientBootstrap.Verification/Program.cs`
- Modify: `progress.md`

**Step 1: Write the failing loopback assertion.**

After full bootstrap and one normal control frame, send the captured valid
message 29. Require a subsequent ping response. Send a cross-slot message 29
on a second session and require only that session to close.

**Step 2: Run the focused loopback verifier and confirm the pre-fix failure.**

Run:

```powershell
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -c Release -p:UseSharedCompilation=false
```

Expected before Task 2: the valid client frame closes the connection or the
following ping times out.

**Step 3: Run the completed focused and broad verification set.**

Run:

```powershell
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Hardening.Loopback.Verification\Terraria.Dome.Hardening.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

Expected: every command exits `0`; build warnings and errors are recorded
verbatim in the result note.

**Step 4: Record evidence and residual scope.**

Append a dated entry to `progress.md` with the original frame, root cause,
protocol invariant, commands and exit status. State explicitly that full
server-authoritative creation and destruction of client projectiles remains a
separate feature and was not enabled by this compatibility fix.
