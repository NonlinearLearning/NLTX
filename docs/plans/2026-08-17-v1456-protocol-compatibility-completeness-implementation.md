# Terraria V1456 Protocol Compatibility Completeness Implementation Plan


**Goal:** Make every V1456 packet handled on Dome's active client/server paths conform to the
complete original wire grammar, so an unimplemented Dome simulation feature cannot cause a
legal Terraria client frame to become misaligned or disconnect the session.

**Architecture:** Keep `TerrariaPacketCodec` and the existing dispatcher/session APIs as the
communication entry points. Move original V1456 field-order, flag-bit, optional-suffix, and
length-formula knowledge into `Terraria.Dome.Protocol.V1456.Compatibility`; it projects parsed
legacy state into existing Dome commands only where the Simulation has an authority model. The
Simulation must not reference Terraria `Main`, `Player`, `Mount`, `NetMessage`, or legacy socket
types. A legal but not-yet-simulated field is consumed and classified by the compatibility layer;
only a frame that violates the original V1456 grammar is malformed.

**Tech Stack:** .NET 10, C#, existing loopback TCP verifiers, Terraria V1456 reference source,
the current frame-trace tools under `Build/diagnostics`.

---

## Decision Record: Domain Migration Is Related, Not a Wire-Parsing Prerequisite

The full source under `D:/TRbackup/无任何删减通过编译/Terraria` couples V1456 packet handling
directly to legacy runtime objects. In `NetMessage.cs`, `case 13`, the sender reads
`Player.mount.Active` and conditionally appends `Player.mount.Type`; in `MessageBuffer.cs`,
`case 13`, the receiver reads that suffix then calls the legacy mount API. Dome intentionally
does not carry that legacy `Player` object graph into its Arch ECS Simulation.

That incomplete gameplay migration explains why Dome has no current authority model for mount
movement, collision, persistence, or cross-player replication. It does **not** justify failing
to read a legal original `PlayerControls(13)` frame. The direct mount-disconnect cause is that
the current codec projected the packet straight into the minimal `PlayerControlIntent`, consumed
only the fields that type needs, and treated the legally selected mount suffix as unexpected
trailing data.

The implementation must preserve these three independent obligations:

| Responsibility | Required outcome | Mount example |
|---|---|---|
| Wire grammar | Read and write every original field selected by V1456 flags, in original order and at the exact calculated length. | Consume `UInt16 mountType` when `flags2.bit7` is set. |
| Compatibility state | Retain or explicitly classify parsed state that has no current ECS owner. | Store session-local `MountActive`/`MountType`, or explicitly consume it as a documented non-authoritative field. |
| Authority semantics | Apply validated state to the Dome world only after its domain behavior has been migrated. | Later add mount physics, collision, persistence, and observer replication as a separate Simulation slice. |

The protocol layer must never use the absence of an ECS component as a reason to reject a
well-formed legacy packet. Conversely, it must not echo client-supplied legacy state to other
players merely to appear compatible: outgoing state must be sourced from an explicit authority
or compatibility projection. A missing semantic model may produce a documented inactive/default
field value on outbound frames when the V1456 predicate makes that valid; it may not produce an
unexplained frame-length difference.

The existing `TerrariaPacketSupport.Handled` classification is insufficient on its own. It means
only that Dome currently takes some route for a message; it does not prove full original grammar
coverage. The matrix and tests in this plan must instead track both:

```text
WireGrammarComplete: every legal V1456 field and length branch is decoded/encoded.
SemanticSupport:    Dome has an authoritative, validated behavior for the message's effect.
```

For example, `PlayerControls` cannot be called wire-complete until its velocity, mount, Potion
of Return, and camera suffix branches are all covered, even if its semantic support remains
limited to movement/item controls. `SyncProjectile` and every future codec path that rejects
"unsupported trailing data" must receive the same source-first audit.

## Non-Negotiable Compatibility Contract

For every supported V1456 message and direction, record all of the following before changing
its encoder or decoder:

1. Original source location and original `MessageBuffer`/`NetMessage` field order.
2. Fixed payload length and each conditional field's exact predicate and length.
3. The exact outbound length formula.
4. One of four explicit Dome dispositions for every parsed field:
   `AuthoritativeProjection`, `LegacySessionState`, `ExplicitlyIgnored`, or
   `MalformedWhenInvalid`.
5. Tests using raw, length-prefixed V1456 frames, not only Dome's convenience encoders.

`ExplicitlyIgnored` means the complete original field was read in the correct position and its
loss has been deliberately documented. It never means leaving unread bytes in a packet. A
different Dome world, NPC list, tile map, or ECS state can explain different field values; it
cannot explain a different V1456 framing layout without a documented protocol predicate.

### Task 1: Freeze the supported-protocol inventory and acceptance boundary

**Files:**
- Create: `docs/protocol/v1456-compatibility-matrix.md`
- Read: `src/Terraria.Dome.Protocol.V1456/Protocol/TerrariaMessageId.cs`
- Read: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatcher.cs`
- Read: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Read: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`
- Read: `D:/TRbackup/无任何删减通过编译/Terraria/MessageBuffer.cs`
- Read: `D:/TRbackup/无任何删减通过编译/Terraria/NetMessage.cs`

**Step 1: Build the message inventory from executable paths.**

List every `TerrariaMessageId` that the dispatcher accepts and every ID emitted by the server,
including its direction, current owner, and whether a real-client trace has observed it. Do not
mark an unobserved message as compatible merely because the enum defines it.

**Step 2: Add a row for every inventory message.**

Use this exact table shape in `v1456-compatibility-matrix.md`:

| Id | Name | Direction | Original source | Fixed bytes | Conditional suffixes | Wire grammar | Semantic support | Field disposition | Raw-frame test | Trace evidence |
|---:|---|---|---|---:|---|---|---|---|---|---|

`Wire grammar` must be one of `Unclassified`, `Partial`, or `Complete`; `Semantic support` must
be one of `None`, `CompatibilityOnly`, or `Authoritative`. For unimplemented messages, use
`Unclassified` rather than guessing a length or a semantic meaning. `Unclassified` is a tracked
compatibility debt and cannot be passed through silently.

**Step 3: Record the current PlayerControls defect as the first failing row.**

Document original receive source `MessageBuffer.cs`, `case 13`, and writer source
`NetMessage.cs`, `case 13`. Mark `PlayerControls` as `Partial` wire grammar and
`CompatibilityOnly` semantic support until its suffixes are consumed and its remaining state is
explicitly classified. Record its fixed payload of 14 bytes after message ID and its four
conditional suffixes:

```text
flags2.bit2 -> velocity Vector2          -> 8 bytes
flags2.bit7 -> mount type UInt16         -> 2 bytes
flags3.bit6 -> return-origin + return-home Vector2 -> 16 bytes
flags4.bit5 -> camera target Vector2     -> 8 bytes
```

**Step 4: State the protocol rejection policy.**

The document must say that a normal frame matching this grammar is never rejected solely because
Dome has no matching simulation feature. Reject a frame only for an invalid length prefix, a
truncated required or selected optional field, an impossible version-specific layout, or an
explicit server-authority validation failure with a controlled protocol outcome. A rejected
semantic action must still be parsed completely before its policy outcome is chosen.

**Step 5: Commit only the inventory document if the working tree is otherwise ready.**

```powershell
git add docs/protocol/v1456-compatibility-matrix.md
git commit -m "docs: define v1456 compatibility matrix"
```

Do not stage unrelated pre-existing changes in this dirty worktree.

### Task 2: Establish a raw PlayerControls mount regression before production changes

**Files:**
- Modify: `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs`
- Modify: `Test/Terraria.Dome.SessionReplication.Verification/Program.cs`
- Read: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Read: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`

**Step 1: Add a raw V1456 packet fixture builder in the protocol verifier.**

The helper must create the frame with `TerrariaFrameCodec.Encode`, not call
`EncodePlayerControls`. Write all four flag bytes and then append the conditional suffixes in
original order. The mount test must set `flags2.bit7`, append a little-endian `ushort` mount type,
and keep all other optional flags clear.

```csharp
static byte[] CreatePlayerControlsFrame(
  byte playerSlot,
  byte flags1,
  byte flags2,
  byte flags3,
  byte flags4,
  byte selectedItem,
  float positionX,
  float positionY,
  ushort? mountType)
{
  // Write the 14-byte fixed V1456 payload, then selected suffixes in source order.
}
```

Use a nonzero, valid-looking mount type such as `1`; assert the encoded packet payload is
exactly 16 bytes for the no-velocity/mount-active case.

**Step 2: Add the failing codec-level assertion.**

Call `TerrariaPacketCodec.DecodePlayerControls` with that frame. The desired assertion is that
the frame decodes without `InvalidDataException`, all fixed input fields retain their expected
values, and the compatibility projection reports mount active with type `1`. This test must fail
on the current source specifically with:

```text
Terraria PlayerControls packet has unsupported optional fields.
```

**Step 3: Run the focused verifier and preserve the red result.**

Run from the repository root:

```powershell
dotnet run --project Test/Terraria.Dome.Protocol.Compatibility.Verification \
  -c Release -p:UseSharedCompilation=false
```

Expected before implementation: nonzero exit caused by the mount suffix assertion, not a build
or fixture error. Record the failure message in the matrix row.

**Step 4: Add the failing live-session assertion.**

In `Terraria.Dome.SessionReplication.Verification`, activate a normal player, write the raw
mount-bearing frame, then write `TerrariaPacketCodec.EncodePing()`. Read until a `Ping` response
or timeout. Assert the session remains alive and the server still has exactly one player. This
must fail on current code because the unhandled decoder exception exits the host loop and its
`finally` queues `DestroySessionPlayerCommand`.

**Step 5: Run the session verifier and preserve the red result.**

```powershell
dotnet run --project Test/Terraria.Dome.SessionReplication.Verification \
  -c Release -p:UseSharedCompilation=false
```

Expected before implementation: no Ping response, or player count becomes zero, with
`LastSessionFault` identifying the unsupported optional-fields exception.

### Task 3: Model the complete V1456 PlayerControls wire state in the compatibility layer

**Files:**
- Create: `src/Terraria.Dome.Protocol.V1456/Compatibility/LegacyPlayerControlsState.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Compatibility/LegacyPlayerControlsProjection.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Compatibility/TerrariaV1456Compatibility.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Test: `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs`

**Step 1: Define the compatibility state before touching the decoder.**

Create a small immutable state model that has every original `PlayerControls(13)` field needed
to prove consumption, including the four raw flags, selected item, position, optional velocity,
optional mount type, optional return positions, and optional camera target. Use nullable value
types for values whose presence is controlled by a bit.

```csharp
internal sealed record LegacyPlayerControlsState(
  byte PlayerSlot,
  byte ControlFlags,
  byte SecondaryFlags,
  byte TertiaryFlags,
  byte QuaternaryFlags,
  byte SelectedItem,
  Vector2 Position,
  Vector2? Velocity,
  ushort? MountType,
  Vector2? ReturnOrigin,
  Vector2? ReturnHome,
  Vector2? CameraTarget);
```

Use the vector type already used by the protocol project. Do not introduce a Terraria legacy
runtime type or a Simulation dependency.

**Step 2: Implement a single complete compatibility decoder.**

Add a method on `TerrariaV1456Compatibility` that reads `PlayerControls(13)` in the exact
original source order. It must:

1. Require the 14-byte fixed payload.
2. Read all four flag bytes before reading the selected item and position.
3. Read velocity only when `SecondaryFlags & (1 << 2)` is nonzero.
4. Read mount type only when `SecondaryFlags & (1 << 7)` is nonzero.
5. Read both return vectors only when `TertiaryFlags & (1 << 6)` is nonzero.
6. Read camera target only when `QuaternaryFlags & (1 << 5)` is nonzero.
7. Reject only trailing bytes remaining after every selected suffix is consumed.

Do not use a generic “skip remaining bytes” fallback. That would conceal the next grammar
defect and would not establish packet-length alignment.

**Step 3: Project only existing Dome controls.**

`LegacyPlayerControlsProjection` converts the parsed state to the existing
`PlayerControlIntent` while retaining the full legacy state for the caller. Preserve the current
public `DecodePlayerControls(ReadOnlySpan<byte>)` API by making it call the compatibility decoder
then return the projected intent. The result is one source of truth for flag ordering and length.

**Step 4: Keep the test intentionally narrow.**

Do not add mount movement, collision, item behaviour, persistence, or an ECS component in this
task. The green condition is only that all legal packet-13 suffix combinations decode without
desynchronizing framing. Update the matrix only from `Partial` to `Complete` wire grammar; leave
semantic support as `CompatibilityOnly` until a separate authority migration has evidence.

**Step 5: Run the protocol verifier and confirm green.**

```powershell
dotnet run --project Test/Terraria.Dome.Protocol.Compatibility.Verification \
  -c Release -p:UseSharedCompilation=false
```

Expected: exit code `0`; the mount-bearing raw frame test passes.

### Task 4: Cover every PlayerControls length branch and malformed boundary

**Files:**
- Modify: `Test/Terraria.Dome.Protocol.Compatibility.Verification/Program.cs`
- Modify: `docs/protocol/v1456-compatibility-matrix.md`

**Step 1: Add one raw-frame test per selected suffix.**

Test the fixed 14-byte packet, velocity-only (22-byte), mount-only (16-byte), return-only
(30-byte), and camera-only (22-byte) forms. Assert each compatibility projection's nullable
fields, consumed payload length, and resulting `PlayerControlIntent`.

**Step 2: Add a combined maximum-suffix test.**

Set all four optional predicates and assert an exact 48-byte payload:

```text
14 fixed + 8 velocity + 2 mount + 16 return positions + 8 camera target = 48
```

Use distinct float values for all vectors so an ordering error cannot pass by coincidence.

**Step 3: Add truncated-suffix tests.**

Create a mount flag with only one mount byte, a return flag with fewer than 16 remaining bytes,
and a camera flag with fewer than 8 remaining bytes. Assert each fails as a truncation error.
Those are malformed frames; a legal complete suffix is not.

**Step 4: Run the verifier to green and update the matrix evidence.**

```powershell
dotnet run --project Test/Terraria.Dome.Protocol.Compatibility.Verification \
  -c Release -p:UseSharedCompilation=false
```

Record the exact formulas, verifier name, and exit status in the `PlayerControls` row.

### Task 5: Preserve session liveness and explicitly isolate non-authoritative legacy state

**Files:**
- Modify: `src/Terraria.Dome.Protocol.V1456/Session/TerrariaSession.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatchResult.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatcher.cs`
- Modify: `src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs`
- Modify: `Test/Terraria.Dome.SessionReplication.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Hardening.Loopback.Verification/Program.cs`

**Step 1: Add a failing state-isolation test.**

After a mount-bearing control packet, assert the session has the latest
`LegacyPlayerControlsState` and the command queue receives the existing `PlayerControlIntent`.
Assert no Terraria type enters `Terraria.Dome.Simulation` and no new simulation command is
created solely for `MountType` in this task.

**Step 2: Thread the compatibility projection through the existing dispatcher/session context.**

Make the session own the transient `LegacyPlayerControlsState` associated with its assigned
player slot. Return both the existing authoritative intent and the parsed compatibility state
from dispatch in a backwards-compatible result shape. The session host must continue to enqueue
only `ApplyPlayerControlCommand` until a separately approved mount simulation slice exists.

**Step 3: Define outbound behavior explicitly.**

Document and test one of these only after inspecting the real client trace:

- If self-authority frames are not emitted after activation, keep incoming mount state session
  local and do not synthesize an inactive server correction.
- If a server `PlayerControls(13)` projection is emitted for that player, its mount bit/value
  must come from an explicit compatibility projection rather than an accidental zero default.

Do not echo mount state to other sessions as a substitute for authority. Cross-player rendering,
mount physics, collision, and persistence belong to a later Simulation work item.

**Step 4: Run the two live-session verifiers.**

```powershell
dotnet run --project Test/Terraria.Dome.SessionReplication.Verification \
  -c Release -p:UseSharedCompilation=false

dotnet run --project Test/Terraria.Dome.Hardening.Loopback.Verification \
  -c Release -p:UseSharedCompilation=false
```

Expected: exit code `0`; a legal mount packet receives a Ping response and malformed frames
remain isolated from later healthy sessions.

### Task 6: Systematically close the remaining active-path grammar gaps

**Files:**
- Modify: `docs/protocol/v1456-compatibility-matrix.md`
- Modify: `src/Terraria.Dome.Protocol.V1456/Compatibility/TerrariaV1456Compatibility.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Compatibility/Legacy<Message>State.cs` as justified
- Modify: `src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs`
- Modify: the smallest relevant `Test/Terraria.Dome.*.Verification/Program.cs`

**Step 1: Prioritize one unclassified observed message at a time.**

Use a fresh full-client trace and choose the earliest chronological C2S or S2C message whose
matrix row lacks a source-derived grammar or has unexplained length variance. Do not repair
aggregate packet counts by emitting arbitrary frames.

**Step 2: Read the complete original send and receive branches.**

Before coding, record every flag, count, string encoding, loop, sentinel, and optional suffix in
the matrix. Confirm both payload byte count and field order from a raw captured packet.

**Step 3: Write one raw-frame failing test.**

The test must use a legal representation that contains the next currently-unconsumed field. It
must fail because the current codec misparses the frame, rejects its valid length, or encodes the
wrong original shape.

**Step 4: Add the smallest typed compatibility state and complete parser/encoder.**

Parse the complete original grammar in `Compatibility`, project only the authority that exists,
and classify the remainder. Keep each packet family in a focused type rather than expanding a
generic `Manager`, `Helper`, or untyped byte-array store.

**Step 5: Green the focused test and matrix row before moving to the next message.**

Run the relevant verifier with `-p:UseSharedCompilation=false`, then update its matrix row with
the source evidence, raw payload lengths, disposition, and command output.

### Task 7: Prove compatibility with an executable client and protect the baseline

**Files:**
- Create: `Build/diagnostics/v1456-compatibility-<timestamp>/trace.jsonl`
- Create: `Build/diagnostics/v1456-compatibility-<timestamp>/summary.json`
- Create: `Build/diagnostics/v1456-compatibility-<timestamp>/comparison.md`
- Create: `Build/diagnostics/v1456-compatibility-<timestamp>/client/mount-stable.json`
- Modify: `docs/protocol/v1456-compatibility-matrix.md`

**Step 1: Stop only the confirmed Dome executable before rebuilding.**

Verify its exact PID and path first. Do not stop unrelated Terraria, client, proxy, or complete
source-server processes. Build from the repository root:

```powershell
dotnet build src/Terraria.Dome.Server/Terraria.Dome.Server.csproj \
  -c Release -p:UseSharedCompilation=false
```

Record the exit code, warnings, and actual `Build/bin` output location.

**Step 2: Run focused and baseline verifiers serially.**

```powershell
dotnet run --project Test/Terraria.Dome.Protocol.Compatibility.Verification \
  -c Release -p:UseSharedCompilation=false

dotnet run --project Test/Terraria.Dome.SessionReplication.Verification \
  -c Release -p:UseSharedCompilation=false

dotnet run --project Test/Terraria.Dome.FullClientBootstrap.Verification \
  -c Release -p:UseSharedCompilation=false

python -m unittest Build/diagnostics/test_compare_packet_traces.py -v
```

Expected: every command exits `0`; report any warnings without calling the result clean.

**Step 3: Run a real-client mount scenario through the recorder.**

Start the rebuilt Dome server on `127.0.0.1:7778` through the existing transparent recorder.
Join with the normal full client, summon a mount after the local player is active, wait through
at least one replication interval, send/observe a follow-up Ping or normal control frame, and
write `mount-stable.json` with connection status, player slot, elapsed time, and terminal event.

**Step 4: Validate packet lengths rather than only successful connection state.**

For every captured `PlayerControls(13)` frame, calculate expected payload length from the four
flags and compare it with the raw payload. Require zero unexplained length mismatches; label
each observed length difference from a complete-server trace as content, flag predicate, version,
or defect.

**Step 5: Restore the server service and publish evidence.**

Restart the confirmed built Dome executable on port `7778`. Update the matrix and comparison
report with source files changed, commands, exit codes, trace paths, mount result, remaining
`Unclassified` message rows, and the explicit residual: mount gameplay is not yet an authority
feature unless a separately verified Simulation plan has been completed.

### Task 8: Commit in narrow, reviewable units

**Files:**
- Stage only files changed by each completed task.

**Step 1: Review the diff and working tree.**

```powershell
git diff --check
git status --short
```

Expected: no whitespace errors; unrelated pre-existing changes remain unstaged.

**Step 2: Commit the grammar, regression, and evidence work separately.**

```powershell
git add src/Terraria.Dome.Protocol.V1456/Compatibility \
  src/Terraria.Dome.Protocol.V1456/Packets/TerrariaPacketCodec.cs \
  Test/Terraria.Dome.Protocol.Compatibility.Verification \
  Test/Terraria.Dome.SessionReplication.Verification
git commit -m "fix: consume v1456 player control suffixes"
```

Commit the compatibility matrix and diagnostic report separately only when they contain no
machine-specific or regenerable `Build/` output. Do not commit any `Build/bin`, `Build/obj`,
`Build/generated`, or `Build/packages` artifacts.
