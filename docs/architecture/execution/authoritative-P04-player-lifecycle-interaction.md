# Authoritative P04 Player Lifecycle And Interaction Execution

partitionId: P04  
sourceSessionId: `910f8127b89041cc98bf563325fab350`  
executionStatus: proposed-plan  
implementationStatus: partial  
evidenceStatus: partial  
verificationStatus: partial  

## Objective

Implement the proposed P04 boundaries in `D:\TRbackup\NLTX\src\NSSLC` as owner and integration
contracts are resolved. This is an execution plan with partial rest and lifecycle implementation
slices, not an execution-success record. The authoritative input was the single P04 claim; its report
session was already settled as `completed`. This derivative document does not claim or open
another partition session.

The design is in
[`authoritative-P04-player-lifecycle-interaction.md`](../subsystems/authoritative-P04-player-lifecycle-interaction.md).

## Current Baseline

The source report covers 13 groups, 112 members, and remains `proposed` / `partial` / `not-run`.
The local `PlayerRestInteractionSystem` owns the rest activity flags, stop order, sleep timer, and
packet-intent result. `PlayerLifecycleSystem` owns local connection transitions, lifecycle phase,
dead elapsed ticks and the respawn countdown, and the initial death-record commit after synchronous
rest cleanup. Type 14
connection transitions return hook intent only when the active value changes; spawn activation
commits active state without synthesizing those hooks. Neither System
has a gameplay caller or world schedule; the new dead-tick adapter is an isolated ghost-owner
boundary and has no production caller. Duplicate lifecycle stage and respawn
component candidates without C# consumers were removed; identity/lifecycle state types are
immutable projections. The complete reference tree shows petting, sitting, and sleeping as
independent flags, with different entry cleanup rules; the local state therefore uses a flag set
rather than claiming global exclusivity.

Evidence uses two distinct source baselines:

- `D:\TRbackup\Version4` is the designated target tree used for the report's CPG paths and direct
  source fallback; the manifest does not bind an exact source snapshot. The 2026-10-01 read-only
  Query API recheck returned `ImportStatus=complete`, manifest
  `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`, and
  project fingerprint `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`, with
  `SourceSnapshotId=null`. Selected-path queries found 4 `Spawn` calls (2 `Player.cs`, 2
  `MessageBuffer.cs`) and 6 `StopVanityActions` calls (`Player.cs`). Member-use counts in the same
  two shards were `active` 17, `dead` 21, `deadTime` 3, `pvpDeath` 4, `spectating` 11, and
  `respawnTimer` 8; confirmed write counts were 3, 4, 1, 2, 4, and 4, with remaining directions
  `partial`/`Unknown`. These are indexed facts, not a complete caller/effect inventory. Query source
  excerpts were unavailable through the reader, so Version4 and complete-reference method bodies
  were read directly. The API recheck for `PetAnimal`, `PetMount`, `SitDown`, and `StartSleeping`
  returned `partial / NoMatchingFactInScannedScope`; the local rest-entry decision therefore uses
  reference-only behavior and does not close the Version4 target mapping.
- Follow-up selected-path call-site queries returned six `StopVanityActions` sites in `Player.cs`,
  four `StopPettingAnimal` sites across `Player.cs` and `Mount.cs`, and five `StopSleeping` sites
  across `Player.cs` and `PlayerSleepingHelper.cs`. These complete indexed selections remain
  bounded facts rather than a runtime caller closure.
- The latest selected-path API query found one `AdjustRespawnTimerForWorldJoining` call in
  `Player.cs` and four `Spawn` calls across `Player.cs` and `MessageBuffer.cs`. The
  `UpdateDead` call-site query stayed `partial / NoMatchingFactInScannedScope`, while direct source
  review confirms its `Player.Update` call in both trees. `lastTimePlayerWasSaved` resolved as a
  Version4 field symbol, but its member-use query was partial with no matching selected-path fact;
  the complete reference body directly confirms the guarded read. Version4's adjustment body is
  empty, so its outer Spawn branch preserves the existing dead value; the reference offline-expiry
  transition remains proposed for the target.
- `CanSpectate` resolved as a Version4 method; the call-site API returned one selected call, while
  direct source also shows the `Player.Update` call. The Version4 and complete-reference bodies
  match on negative/self slots, active candidates, and the selected-dead `< 180` tick rule. The
  local System now exposes this rule over explicit candidate facts; slot lookup remains adapter
  work.
- The complete reference's `ClosestSpectatablePlayerTo` scans slots `0..254`, excludes self,
  filters through `CanSpectate`, and compares player centers by squared distance. Strict `<` means
  the lower slot wins ties; no match returns `-1`, which `SpectateNextClosestPlayer` passes to
  `SetOrRequestSpectating`. Version4's `SpectateNextClosestPlayer` body is empty and the CPG query
  for `ClosestSpectatablePlayerTo` is `partial / NoMatchingFactInScannedScope`. The new local
  `FindClosestSpectatablePlayer` is therefore a reference-derived proposal with no production
  caller. The CPG call-site query returned one selected `CanSpectate` call, three selected
  `SetOrRequestSpectating` calls, and one selected `SpectateNextClosestPlayer` call within
  `Terraria/Player.cs`; those bounded results do not close callers or runtime reachability. Source
  anchors are `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:17424`, `:17473`, and `:17528`,
  plus `Terraria\Entity.cs:50` and `:206`; Version4's empty fallback is at
  `D:\TRbackup\Version4\Terraria\Player.cs:10100`.
- The complete reference's `SpectateNextPlayer` scans cyclically from the current target (or the
  observer slot), uses `±1` direction and `includeSelf`, returns false offline or without a
  candidate, and commits only after finding a slot. Its selected Version4 symbol query was
  `partial / NoMatchingFactInScannedScope`, and no declaration exists in the reviewed Version4
  `Player.cs`. `FindNextSpectatablePlayer` models candidate selection only and returns `null` on
  exhaustion; the adapter must retain the online guard and avoid a state commit for `null`. Source
  anchors in the complete reference are `Player.cs:17494`, `:17404`, and `:44217`.
- `SetOrRequestSpectating` differs across sources. The Version4 CPG query found four selected call
  sites in `Player.cs` and `MessageBuffer.cs`; direct source writes the requested slot before
  validation and calls an empty closest-target fallback, with no visible mode branch. The complete
  reference branches by network mode, stages local client requests, and on the server commits
  before fallback or section/broadcast effects. Type 150 also writes lifecycle state directly in
  `MessageBuffer`. The 2026-10-01 Query API recheck resolved one confirmed method symbol and four
  confirmed selected-path call sites; callable facts stayed `partial` with
  `CalleeEffectsNotExpanded`, while `SpectateNextPlayer` stayed `partial` /
  `NoMatchingFactInScannedScope`. `RequestSpectatingTarget` expresses the complete-reference local
  request policy as state/effect intents. Packet-specific type 150 stop/receive handling remains
  behind the declaration-only `IPlayerSpectatingNetworkAdapter.Apply`; no packet implementation or
  production caller is connected, and Version4 parity is `unknown`. Source anchors are
  Version4 `Player.cs:10101` and `MessageBuffer.cs:3265`, and complete reference
  `Player.cs:17535` and `MessageBuffer.cs:4345`.
- `UpdateDead` resolves in the Version4 `Player.cs` shard, but its selected-path call-site query is
  `partial / NoMatchingFactInScannedScope`. Direct source inspection confirms one `Player.Update`
  call under the `dead` branch in both trees. The selected member-use queries returned 3 `deadTime`
  and 8 `respawnTimer` uses; direct source confirms per-tick increment/countdown. The complete
  reference locally spawns when a normal respawn timer expires and applies a local/server gate to
  hardcore ghost transition; the reviewed Version4 body differs. CPG zero-hit is not used as proof
  that the caller is absent.
- `D:\TRbackup\无任何删减通过编译` supplies read-only complete method bodies for selected
  reference behavior, including team spawn, bed wake-up/offset, and player serialization. It has
  no Git metadata. Its root contains `TerrariaServer.sln` and `TerrariaServer.csproj` targeting
  `net40`; its `Main.cs` labels the source `v1.4.5.6`, matching Version4's version label. This is a
  complete source reference for the checked paths, not proof of exact provenance or target parity.
  An earlier full-reference build is recorded under Verification Record.
- The type 12 follow-up query resolved `Spawn` and returned four selected `CallTargets` (two
  `MessageBuffer.cs`, two `Player.cs`). `SpawnX`/`SpawnY` each returned five selected member-use
facts: the `MessageBuffer` writes are confirmed, while `NetMessage` reads and the
`Player.Spawn`/`Spawn_SetPosition` flow remain partial or unknown. Direct comparison of
  `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:604-642` with
  `D:\TRbackup\无任何删减通过编译\Terraria\MessageBuffer.cs:898-939` confirms that the
  complete reference overrides the declared slot only in server mode, whereas Version4 has an
  unconditional `whoAmI` assignment. Both read the same signed-short coordinate and companion
fields before calling `Spawn`; this does not prove target parity or caller closure.
  `Get-CpgCallableFacts(Spawn)` is `partial` with `CalleeEffectsNotExpanded`, so the execution
  plan keeps position, team-spawn, network and entry-world effects outside this slice.
- The type 13 slot bound was rechecked against the complete reference `Terraria/Main.cs:1088`,
  where `maxPlayers = 255` while the backing player array has length 256. The API-only adapter does
  not enforce this bound or select a slot. Accepted slot range, session binding, and production
  `MessageBuffer` reachability remain `unknown`.
- The follow-up CPG query resolved Version4 `ghost` and returned confirmed assignment-left uses in
  `Player.cs` and `MessageBuffer.cs`, with partial read uses in the selected scope. The complete
  reference writes ghost state only for local/server hardcore expiry. `PlayerDeadTickAdapter` now
  consumes that intent through the existing `PlayerGhostStateComponent`; its current `Ghost` value
  overrides a stale input snapshot on repeat calls. Respawn, inventory UI, spawn position and
  network effects remain outside the adapter.

The complete reference resolves body-level blanks in that copy only. It does not prove that the
Version4 target or NLTX migration has matching semantics.

## Execution Gates

### 1. Freeze The Evidence And Owners

Before integrating any System, compare the two source trees for the exact selected methods and
record the accepted behavior baseline. Resolve the following in an integration review:

- Production ownership for player slot/session/persistent identity binding and `active` writes;
  Version4 case 14 has an unconditional early `break` before the indexed active writes.
- One production lifecycle/death authority; the local System's owner remains proposed until real
  callers, storage binding, and external effects are connected.
- The local rest owner uses one activity flag set. Keep PetMount's no-cleanup entry distinct from
  PetAnimal, sitting, and sleeping entries that call `StopVanityActions`; the Version4 source path,
  dispatch and preconditions remain `unknown` where the CPG query returned no symbol fact. The
  entry adapter still owns target validation, movement, grapples, dismount, target registration,
  presentation and packet effects.
- Commit boundaries for death effects and teleport position/network state.
- The exact Player save fields, release/version rules, and failure recovery contract.

If an owner or behavior input remains unknown, block only the dependent integration; do not invent
an adapter result or mark the behavior migrated.

### 2. Integrate The Lifecycle Owner

The local API is in `src/NSSLC/Component/Player/PlayerLifecycleSystem.cs`. It makes connection
state the identity component's single active source, returns hook intent for type 14 only when
active changes, and commits death phase/record facts after synchronous rest cleanup. Its guard
inputs and resolved respawn duration are explicit. Keep these calls synchronous until source
evidence demonstrates a deferred contract.

`ResolveDeath` takes an explicit `SpectatingNetworkMode` and returns the existing
`RequestSpectatingTarget(-1)` result after committing death facts. This preserves the complete
reference's mode-dependent clear/request/broadcast decision without adding packet behavior to the
System. In the complete reference, `KillMe` sets `dead` before calling `SetOrRequestSpectating(-1)`
(`Player.cs:39275-39277`); the request policy is at `Player.cs:17535-17579`. Version4's differing
mode-free policy remains `unknown`, and no production caller consumes this result yet.

The partial `CommitSpawn` now receives spawn context, a saved-time binary value, and an explicit
UTC `DateTime` snapshot.
For a non-preserved spawn with `WasPvpDeath` set, the result returns
`ShouldApplyPvpDeathRecovery` before clearing that marker. This is an intent for the external
vital/immunity owner; the System does not write those components and no production caller consumes
the intent.
For an already-dead world join it applies the complete reference's elapsed-time adjustment only
when the player is local and the saved timestamp is nonzero, before synchronously stopping rest.
It subtracts at most 1000 seconds at 60 ticks per second, bounded by the current timer using the
source `Utils.Clamp` comparison order, then preserves death only if the resulting timer is nonzero.
The non-preserved branch clears death elapsed time and the PvP marker; both branches activate
through the same identity writer without synthesizing type 14 hooks and clear spectating. The
timestamp adapter, real spawn caller and schedule remain unknown; no position, health, immunity,
team route, network or entry-world effect is executed.

The type 12 boundary is represented by the `IPlayerSpawnNetworkAdapter.Apply` declaration and its
input/result/status/context records in `src/NSSLC/Component/Player`. There is no concrete packet
validation, slot selection, spawn/death write, or call to `PlayerLifecycleSystem.CommitSpawn`.
Version4's client/server slot difference and production reachability remain `unknown`.

The type 13 boundary consists of the `IPlayerRestPacket13Adapter.Apply` and
`IPlayerPacket13RouteQuery.Evaluate` declarations plus input/result/status records. They contain no
network-mode validation, self-echo route, sender selection, rest commit, or relay calculation.
`CanRelayToPeers` and `ShouldBroadcast` are data-contract fields only. Position/velocity, mount,
camera, pose, decoding, session binding, and packet publication remain unimplemented or outside
P04. The CPG Query API resolved `GetData` with one selected call site; `isSitting` direction and
the sleeping helper's callee effects remain partial/unknown. Direct source comparison with the
complete reference (`D:\TRbackup\无任何删减通过编译\Terraria\MessageBuffer.cs:949-1050` and
`Terraria.GameContent/PlayerSleepingHelper.cs:70-89`) confirms the reference slot remap, joined
sender relay condition, and pose side effect. The target policy remains `unknown`.

`AdvanceDeadTick` commits one elapsed-death tick. Normal respawn clamps a one-tick countdown step;
the hardcore branch decrements only when the starting timer is positive and leaves a nonpositive
timer unchanged before returning the authority-gated ghost intent. `PlayerDeadTickAdapter` consumes
the ghost intent through `PlayerGhostStateComponent` and treats that component as the authoritative
repeat guard; no real `UpdateDead` caller is connected to this System. Respawn and inventory effects
remain returned intents. Its explicit ghost input preserves `Player.Update`'s ghost early return
when used directly by a non-adapter caller.
The complete-reference local Spawn and hardcore ghost outcomes remain proposed because Version4's
reviewed `UpdateDead` body differs.

The type 150 composition has only the `IPlayerSpectatingNetworkAdapter.Apply` declaration and its
input/result/status records. Sender authentication, slot/identity validation, lifecycle mutation,
mode policy, and effect selection have no concrete implementation. There is no packet I/O, session
lookup, or production `MessageBuffer` caller; Version4 compatibility and runtime reachability remain
`unknown`.

Type 16 has only the `IPlayerVitalPacket16Adapter.Apply` declaration and decoded input record. Its
signature spans the existing P06 vital owner and P04 lifecycle owner; no slot remapping, field
write, dead-state derivation, or packet publication is implemented. Keep Version4 slot policy,
authenticated session binding, and the cross-owner commit contract `unknown` until the Network and
Vital owners are integrated.

Types 65 and 118 now have input records and abstract `Apply` declarations only:
`IPlayerTeleportPacket65Adapter` and `IPlayerDeathPacket118Adapter`. The type 65 record retains the
raw selector flags, signed entity slot, position, style, optional extra info, network mode, and
sender context. The type 118 record retains damage, direction, PvP state, sender/mode context, and
all eight optional death-reason fields in `PlayerDeathReasonPacketInput`. These declarations do not
decode bytes, remap slots, route player/NPC branches, commit teleport/death state, update
acknowledgements, relay, or publish packets. The different Version4 and complete-reference slot and
network guards, production callers, and cross-owner integration remain `unknown`.

The type 14 client side has only the `IPlayerConnectionNetworkAdapter.Apply` declaration and its
input/result/status records. It does not validate the addressed slot, call
`PlayerLifecycleSystem.SetConnectionState`, or select hook intent. Version4's early case 14 break,
production `MessageBuffer` wiring, and target policy remain `unknown`.

Remaining integration work includes concrete implementations and production callers for the type
12/13/14/16/65/118/150 API declarations, resolution of Version4 slot-remapping policies, and session
owner selection. Preserve the
complete-reference rule that connect/disconnect hooks follow an actual active-state change, while
keeping Version4 case 14 reachability `unknown`. Death resolution preserves creative/practice/
already-dead early returns and the initial order for rest cleanup then death facts. Drops/penalties,
text, network, persistence, and consumption of the returned PvP recovery intent through an actual
spawn caller remain external integration work. Do not give Combat, Items, or Network a second writer for lifecycle
facts.

### 3. Implement The Rest Coordinator

The partial state owner is in `src/NSSLC/Component/Player/PlayerRestInteractionSystem.cs`. It commits
petting/sitting/sleeping activity bits, advances the sleep timer and resets it on an explicit
`hasReasonToActUp` input, clears sitting and sleeping detail only when those activities are active,
and returns separate stop-packet intents. The complete reference resets the timer when its world/item
reason check succeeds; Version4's helper is a stub returning false, so target policy remains
`unknown`. Petting stop only clears its active bit, matching the complete reference method body. No
caller or tick schedule is connected yet. Its `DecideEntry` Query maps reference entry rules from explicit eligibility,
activity, and resolved-position inputs. `PrepareEntry` applies the selected stop synchronously:
repeated petting and same-position sitting/sleeping stop only their current activity, while new
animal-pet, sitting, and sleeping entries run the full rest cleanup. The adapter then publishes
returned sitting/sleeping stop-packet intents in order, when local-player and broadcast conditions
allow, before later target/movement effects. After those effects, `CommitEntry` commits the
activity bit. The Version4 mapping remains `unknown`;
the Query API returned `partial / NoMatchingFactInScannedScope` for the four entry methods. The
Rest System commits adapter-validated anchor, facing, feature/offset and stack-index snapshots for
sitting/sleeping along with their activity flags. The adapter still owns target validation, petting
target identity, grapples, dismount, position, stack-manager effects, achievement and presentation
effects, and packet publication.

Preserve the observed stop order of petting, sitting, then sleeping. Stop is synchronous; the System
does not perform packet I/O. Immediately after a prepared cleanup and before later entry effects, the
network adapter must publish returned sitting and sleeping packet intents in that same order only
for the local player when broadcast is enabled. Tile, entity
registry, and stack-manager access remain outside this System. `QueryState` consumes the committed
activity flags and sleep timer only.

No separate `IsPetting`/`IsSitting`/`IsSleeping` booleans or parallel rest ticker may write active
state. Entry adapters must preserve the source-specific cleanup order; the activity flag set itself
does not impose global mutual exclusion.

### 4. Integrate Adjacent Owners

#### Keep Type 13 Packet Behavior Deferred

The complete reference `MessageBuffer` relays type 13 when the server-side sender reaches
connection state `10`. The reviewed Version4 body at `MessageBuffer.cs:652-738` replaces the
declared slot with `whoAmI` and has no matching relay block. The local API contains only
`IPlayerRestPacket13Adapter.Apply` and `IPlayerPacket13RouteQuery.Evaluate` declarations;
`CanRelayToPeers` and `ShouldBroadcast` are fields with no calculated behavior. There is no
type 13 behavioral test or production caller. Target relay policy, session ownership, and slot
mapping remain `unknown`.

The type 65 and type 118 API inputs preserve the selected wire payloads without implementing
packet behavior. Version4 `MessageBuffer.cs:2303-2374` and `2961-2970` unconditionally replace the
declared slots with `whoAmI`; the complete reference `MessageBuffer.cs:2972-3068` and `3893-3909`
does so only when `Main.netMode == 2`. Type 65 also has player/NPC selector and acknowledgement
branches, while type 118 carries the full `PlayerDeathReason` plus damage, adjusted direction, and
PvP bit. Since the sources disagree on slot mapping and mode guards, these interfaces deliberately
leave resolution to integration review. No decoder, route, state commit, acknowledgement, relay,
death-effect, or packet-publication code is added, and the five core assertions do not exercise
these APIs.

Connect teleport requests through the reviewed Teleportation/Movement owner. The rest stop must
finish synchronously before teleport side effects proceed. Preserve style-specific ordering around
grapple/shimmer cleanup, entry effects, position and pressure-plate updates, portal/pylon state,
acknowledgements, exit effects, and transition timers.
The complete reference adds local-player camera/biome refresh effects after position commit that
are absent from the reviewed Version4 method. CPG symbol queries for those helpers returned
`partial / NoMatchingFactInScannedScope`; direct method-body comparison establishes the local
difference only, not global absence. Keep that contract open until the Teleportation, Movement, and
Presentation owners resolve it.

Treat type 13 rest/input data as adapter input, not direct component mutation. Integrate persistence
only after the complete reference and target source have a member/version compatibility map. Do
not infer serialization from `WorldFile`, and do not advance a save checkpoint on failed output.

P09 container links, WorldInteraction anchors, Commerce, Items, Combat, Movement, and P11
presentation remain their owners. Only add explicit ports/projections where their real call path
requires them.

### 5. Write The Initial Core Test Slice

Keep the first executable slice to roughly 10% of the full P04 behavior matrix. It should exercise
high-risk commit invariants rather than every field:

| Test contract | Minimum assertions | Current status |
|---|---|---|
| Rest state and sleep threshold | Initial inactivity, 119/120-tick boundary, petting/sleeping coexistence, and wake-reason timer reset while sleep remains active. | One aggregate assertion; passed in the current focused verifier. |
| Rest cleanup | Missing detail rejects before cleanup; valid seat/bed snapshots pass through prepare/commit; stop clears active details and repeat stop is inert. | One aggregate assertion; passed in the current focused verifier. |
| Dead tick and world-join timer | Local timer expiry requests respawn; an already-ghosted player leaves timers untouched; a local world join subtracts saved elapsed time, clears death on expiry, and a non-local player keeps the full timer. | One aggregate assertion; passed in the 2026-10-01 focused verifier. |
| Connection transition | Network hooks follow active-state changes; duplicate input and spawn activation do not request hooks. | One aggregate assertion; passed in the current focused verifier. |
| Death and PvP recovery handoff | Accepted death stops rest, records one PvP death, requests a server spectating stop, and duplicate input does not recount; the next spawn returns its PvP recovery intent before clearing the marker. | One aggregate assertion; passed in the 2026-10-01 focused verifier. |

These five aggregate assertions are approximately 10% of the former 53-assertion P04 verifier surface.
The packet API types are declarations and data contracts only. They receive compile coverage from
the Player project build, but no packet decision, validation, routing, mutation, or relay behavior
is tested or claimed. This is not full P04 coverage. Do not add tests for unknown team-spawn, session
restoration, serializer, or network replay semantics before those contracts are mapped.

The world-join cases use a fixed UTC save time and assert the complete-reference rule at
`Terraria/Player.cs:56382-56391`: subtract ten seconds at 60 ticks per second, cap the subtraction
at the remaining timer, and clear death only when that timer reaches zero. The non-local case checks
the reference guard. Version4's indexed method body is empty, so these assertions cover the
reference-derived proposed behavior in `CommitSpawn`; they do not establish Version4 parity.

The death case first commits a server spectating target, then verifies that accepted death clears
the target and returns `BroadcastServerTarget`. This matches the complete-reference `KillMe` call
order at the lifecycle intent boundary; the verifier does not publish type 150 or establish
Version4 parity.
The same aggregate now also commits a non-preserved spawn and verifies the PvP recovery intent is
returned before `WasPvpDeath` is cleared. The intent represents the complete-reference branch that
restores full life and grants 300 ticks of immunity; no vital/immunity owner consumes it yet.

### 6. Verification Record

The following build and verifier results belong to an earlier revision, before validated entry
details were carried through `EntryPreparationResult` and before the focused rest assertion was
updated to exercise prepare/commit. That revision used SDK `10.0.400` and the repository serial
wrapper. It exited `0` with `0` warnings and `0` errors:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

That earlier build exited `0`, with `0` warnings and `0` errors. Its verifier and Player outputs were under
`Build/bin/Terraria.Player.LifecycleInteraction.Verification/Debug/net10.0/` and
`Build/bin/Terraria.Player/Debug/net10.0/`.

The focused verifier used this argument vector with the same wrapper:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '--no-build', '--no-restore',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

That earlier verifier exited `0` and printed `PASS: player lifecycle/rest core invariants`; it
contained five aggregate assertions and did not exercise packet API behavior.

The current revision was built and its focused verifier was run through the repository serial
wrapper. The build command was:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

It exited `0` with `0` warnings and `0` errors. Outputs were written under
`Build/bin/Terraria.Player.LifecycleInteraction.Verification/Debug/net10.0/` and
`Build/bin/Terraria.Player/Debug/net10.0/`.

The current focused verifier command was:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '--no-build', '--no-restore',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

It exited `0` and printed `PASS: player lifecycle/rest core invariants`. The verifier contains five
aggregate assertions. No packet behavior, production caller, world schedule, or full P04 behavior
was exercised; the P04 source report remains `proposed` / `partial` / `not-run`.

After adding the type 65/118 declaration-only inputs and adapter interfaces, the affected Player
project and focused verifier were rebuilt with:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

The build exited `0` with `0` warnings and `0` errors. Outputs were written under
`Build/bin/Terraria.Player.LifecycleInteraction.Verification/Debug/net10.0/` and
`Build/bin/Terraria.Player/Debug/net10.0/`. The five-assertion core verifier was then run with:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '--no-build', '--no-restore',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

It exited `0` and printed `PASS: player lifecycle/rest core invariants`. These checks provide
compile coverage for the packet declarations and the existing 10% lifecycle/rest slice only; no
packet behavior was tested. The designated full reference source and project were unchanged; their
previous serial build result remains `85` warnings and `0` errors.

On 2026-10-01, the focused verifier was extended within the existing five aggregate checks to cover
local offline timer reduction (`900` to `300` after ten seconds), expiry (`600` to `0` and death
cleared), and a non-local join (timer unchanged). The affected verifier project was rebuilt through
the repository serial wrapper with SDK `10.0.400`; it exited `0` with `0` warnings and `0` errors.
Outputs are under `Build/bin/Terraria.Player.LifecycleInteraction.Verification/Debug/net10.0/` and
`Build/bin/Terraria.Player/Debug/net10.0/`. The verifier was then run with `--no-build --no-restore`
through the same wrapper and printed `PASS: player lifecycle/rest core invariants` (exit `0`).

The same 2026-10-01 revision now returns the server spectating-stop intent from `ResolveDeath`. The
affected project was rebuilt with the repository serial wrapper using:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

It exited `0` with `0` warnings and `0` errors. The verifier was then run through the same wrapper
with this argument vector:

```powershell
$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '--no-build', '--no-restore',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

It exited `0` and printed `PASS: player lifecycle/rest core invariants`. The existing five aggregate
checks now cover a death-time server target clear and `BroadcastServerTarget` intent; type 150 is
still not decoded, routed, or published.

The designated complete reference solution was also built through the same wrapper. Its SDK-style
`net40` project has an existing generated source file under its `obj/` tree; the first redirected
build exposed that file and failed with a duplicate target-framework attribute. The successful
retry excluded the reference project's existing `obj/**` while keeping new intermediates/output in
NLTX:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'build',
  'D:\TRbackup\无任何删减通过编译\TerrariaServer.sln',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false',
  '-p:DefaultItemExcludesInProjectFolder=obj/**',
  '-p:BaseOutputPath=D:\TRbackup\NLTX\Build\bin\CompleteReference\',
  '-p:BaseIntermediateOutputPath=D:\TRbackup\NLTX\Build\obj\CompleteReference\'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

It exited `0` with `85` warnings and `0` errors. The artifact is
`D:\TRbackup\NLTX\Build\bin\CompleteReference\Debug\net40\TerrariaServer.exe`. The warning set
comes from the unchanged reference source. Reference compilation confirms buildability only; it
does not prove Version4/NLTX behavior parity. The P04 source report remains `proposed` / `partial` /
`not-run`; this partial design slice does not claim migration success.

The same full solution command was rerun on 2026-10-01. It exited `0` with `0` warnings and `0`
errors and produced the same artifact path. The incremental invocation reported all projects up to
date; the earlier full compilation's `85` warnings remain the source-build warning baseline.

On 2026-10-01, direct comparison found that the complete reference decrements the hardcore timer
only when its starting value is positive. `AdvanceDeadTick` previously clamped negative hardcore
timers to zero before producing the ghost intent. It now preserves those expired timer values and
keeps clamping confined to the positive hardcore and normal-respawn branches. The P04 CPG recheck
used `CpgEvidence.ps1` against manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`
(`SourceSnapshotId=null`): `UpdateDead` symbol lookup was `complete`, selected call-site lookup was
`partial / NoMatchingFactInScannedScope`, and callable facts were `partial` with
`CalleeEffectsNotExpanded`. Direct source inspection of the complete reference established the
branch semantics; Version4 behavior and production reachability remain `unknown`.

The affected verifier project was built from the NLTX repository root through the serial wrapper:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

It exited `0` with `0` warnings and `0` errors. Artifacts are under
`Build/bin/Terraria.Player.LifecycleInteraction.Verification/Debug/net10.0/` and
`Build/bin/Terraria.Player/Debug/net10.0/`. The existing five-aggregate core verifier was run
through the serial wrapper with:

```powershell
$env:PATH = "C:\Users\shan\.dotnet;$env:PATH"
$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Player.LifecycleInteraction.Verification\Terraria.Player.LifecycleInteraction.Verification.csproj',
  '--no-build', '--no-restore',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

It exited `0` and printed `PASS: player lifecycle/rest core invariants`; the dead-tick aggregate now
also asserts an expired hardcore timer remains `-1` when ghost intent is returned.

The complete reference solution was then built serially with the command recorded above, targeting
`D:\TRbackup\无任何删减通过编译\TerrariaServer.sln` and redirecting outputs to NLTX. This invocation
exited `0` with `0` warnings and `0` errors and produced
`D:\TRbackup\NLTX\Build\bin\CompleteReference\Debug\net40\TerrariaServer.exe`. It was an
incremental build; an earlier full compilation reported `85` warnings and `0` errors. Neither
reference build proves Version4/NLTX parity.

The continuation then added `ShouldApplyPvpDeathRecovery` to `SpawnCommitResult` and kept its
consumer outside P04. The existing five-aggregate verifier now checks both branches: a not-yet-due
world join preserves death and the PvP marker without requesting recovery; the next non-preserved
spawn returns the recovery intent before clearing the marker. The affected verifier project's build
command block above was rerun unchanged on 2026-10-01. It exited `0` with `0` warnings and `0` errors;
outputs are under `Build/bin/Terraria.Player.LifecycleInteraction.Verification/Debug/net10.0/` and
`Build/bin/Terraria.Player/Debug/net10.0/`. The focused `run --no-build --no-restore` command block
above was also rerun unchanged through the serial wrapper; it exited `0` and printed
`PASS: player lifecycle/rest core invariants`. No packet behavior was exercised. The complete
reference source was not changed by this slice.

## 2026-10-01 Recheck

The read-only CPG Query API was initialized against
`D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`. It reported `ImportStatus=complete`,
manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`, 967 shards, and
`SourceSnapshotId=null`. `Find-CpgSymbols` resolved `MessageBuffer.GetData`, `Player.Spawn`, and
`Player.SetOrRequestSpectating` as `complete`; the selected-path call-site query for
`SetOrRequestSpectating` returned four `CallTargets` across `Player.cs` and `MessageBuffer.cs`.
`Get-CpgCallableFacts(GetData)` remained `partial` with `CallableNodeBudgetExhausted` and
`CalleeEffectsNotExpanded`; this result does not close packet handling or runtime callers. The
index is not bound to the current Version4 source files, so source semantics remain based on direct
source inspection.

The P04 packet-facing API audit covered the eight interfaces for types 12, 13, 14, 16, 65, 118,
and 150. They contain function declarations only; no concrete packet implementation or production
caller is present in this P04 slice. Packet behavior was not tested. Type 147 loadout is outside
the claimed P04 scope and was not changed in this pass.

The full designated reference solution was rebuilt through
`Build/Tools/Invoke-SerialDotnet.ps1`: `D:\TRbackup\无任何删减通过编译\TerrariaServer.sln` exited `0`
with `0` warnings and `0` errors. The verified artifact is
`D:\TRbackup\NLTX\Build\bin\CompleteReference\Debug\net40\TerrariaServer.exe`.

The focused `Terraria.Player.LifecycleInteraction.Verification.csproj` build exited `0` with `0`
warnings and `0` errors. Its artifacts were verified under
`Build/bin/Terraria.Player.LifecycleInteraction.Verification/Debug/net10.0/` and
`Build/bin/Terraria.Player/Debug/net10.0/`. Running the existing five-aggregate verifier through
the serial wrapper with `--no-build --no-restore` exited `0` and printed
`PASS: player lifecycle/rest core invariants`. This preserves the requested small core-test slice;
it does not exercise packet behavior or establish migration success. The authoritative P04 source
report remains `proposed` / `partial` / `not-run`.

## Rollback And Completion

Retain the legacy writer and remove only the new adapter path if a focused comparison changes
active hooks, death state/effect order, spawn routing, rest stop fields/broadcasts, teleport
ordering, persistence, or same-tick visibility. Do not delete legacy entry points while any owner,
caller, network, or persistence gap remains open.

The implementation can advance beyond `proposed` only when the unique owners are explicit, real
entry points call the new APIs, the focused core slice passes against the new System composition,
and all remaining gaps are listed. Migration success and old-code deletion require the broader
project gates; neither follows from this plan or a build alone.
