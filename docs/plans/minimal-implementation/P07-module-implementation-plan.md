# P07 Packet 82 Modules — Implementation Plan

Status: active  
Owner: root session  
Master plan: `docs/plans/minimal-implementation/MASTER-execution-plan.md`  
Implementation boundary: `D:\TRbackup\NLTX\src\MinimalImplementation`  
Approved external code APIs: public APIs under `D:\TRbackup\NLTX\src\NSSLC.Infrastructure`  
Verification boundary: `src/MinimalImplementation/Verification/Modules`  
Evidence boundary: `Build/diagnostics/MinimalImplementation`

## Objective

Complete the Version4 Packet 82 module path from decoded module payload through authenticated
owner dispatch, commit result, recipient selection, outbound intent, encoding, and session cleanup.
Keep module state in ordinary OOP owners. Keep transport writes outside domain handlers. A handler
may report `Handled` only after its authoritative owner has committed or after it has deliberately
validated a presentation-only request and created the corresponding intent.

The `Packet82ModuleId` enum in `NSSLC.Infrastructure` defines the Version4 module ids 0–14. The
registered codec set is not evidence that every module has a server owner. The implementation must
map every inbound-capable module to exactly one owner or to an explicit, source-backed rejection
policy. It must not silently consume an unsupported payload.

## Scope and boundaries

- Implement the P07-owned Text/chat, Ping, Ambience, Bestiary, Creative Powers, Creative Unlock
  report, Particles, Creative permission, and Banner paths to the extent their Version4 behavior is
  established by the historical source and current Infrastructure codec contracts.
- Route Liquid, Teleport Pylon, Crafting Requests, TagEffect, Leashed Entity, and Unbreakable Wall
  messages through explicit adapters to their P03/P04/P05/P06 owners. P07 does not become a second
  writer for world, inventory, combat, player, or mechanism state.
- Keep one handler per module id. Root owns the final registry, server composition, project-file
  wiring, common packet framing, and the serialized build window.
- Preserve exact Version4 module ids, direction rules, field shapes, and trailing-byte checks.
  Use `NSSLC.Infrastructure` codecs as APIs; do not compile or reference Version4 implementation
  projects.
- All new production and verification source remains inside `src/MinimalImplementation`. Generated
  sources and build evidence remain below `Build/`.
- Disconnect cleanup must retire dispatcher replay state and per-session rate-limit state before
  an epoch can be reused. Snapshot restoration must validate immutable copies before publishing an
  owner.

## Current implementation baseline

- `Modules/ModuleDispatcher.cs` validates Packet 82 envelope identity, stage, module range, unique
  registration, and delegates replay handling to `ModuleDispatchReplayCache`.
- `Modules/ModuleDispatchReplayCache.cs` bounds retained results to 4096 and active epoch records
  to 255; `RetireEpoch` exists but is not yet wired into the P02 close path.
- `Modules/Social/Chat` implements the root-selected historical Party compatibility path. SC081
  decodes a module-1 two-string fixture, authenticates the author from dispatch context, selects
  same-team visible recipients, creates and decodes the S2C NetworkText intent, and checks the team
  color. It does not prove behavior in the current Version4 empty receive methods.
- `Modules/Social/Ping` implements the root-selected historical Ping server route: finite bounded
  positions become intents for visible active sessions other than the sender. Its coordinate limit
  is local defensive policy.
- `Modules/Effects` contains Ambience and Particles handlers. Particles uses authenticated identity,
  validates payload facts, limits requests per epoch, and supports epoch retirement.
- Bestiary, Banner, and Creative state owners expose capture paths. Bestiary and Banner have restore
  factories; Creative Power restoration and initial-join snapshot delivery remain to be reviewed.
- No single production registry currently composes every P07-owned and delegated handler. The
  Server readiness gate still reports module/domain owners and packet registration as incomplete.
- The selected SC081 verifier builds and runs against the historical Text compatibility decision.
  This does not verify current Version4 receive behavior, socket delivery, Ping, all module ids,
  production listening, or cross-owner lifecycle behavior.

## Version4 module inventory and owner boundary

| Id | Version4 module | P07 responsibility | Integration owner / rule |
| ---: | --- | --- | --- |
| 0 | Liquid | Do not write world liquid state in P07; preserve server-authored changed-liquid projections. | P03 World output adapter; reject client state because the current Version4 receiver is empty. |
| 1 | Text | Chat command parse, authenticated author, audience and S2C text intent. | P07 Social handler. |
| 2 | Ping | Validate the request and derive recipients from authenticated/session state. | P07 presentation handler with P02 recipient facts. |
| 3 | Ambience | Server-authored presentation intent; reject unauthorized client authority. | P07 Effects handler and committed P03/P05 event producers. |
| 4 | Bestiary | Apply only server-confirmed contributions; produce bounded join/update snapshots. | P07 Bestiary owner, fed by P05 committed NPC events. |
| 5 | Creative Powers | Validate power id/value kind and permission; commit authoritative state. | P07 Creative Power owner. |
| 6 | Creative Unlock Player Report | Re-resolve server-owned item/sacrifice facts; do not trust an empty client payload as proof. | P04 inventory/research adapter; P07 supplies only module routing. |
| 7 | Teleport Pylon | Emit pylon add/remove projections from committed state; reject client teleport requests. | P06 pylon output adapter; P02 retains player-movement authority. |
| 8 | Particles | Validate bounded presentation input, bind actor from authentication, rate-limit per epoch. | P07 Effects handler; accepted producer events originate after owner commit. |
| 9 | Creative Power Permissions | Send an immutable permission projection; reject client attempts to author policy. | P07 Creative permission projection. |
| 10 | Banners | Apply server-confirmed kill contributions and produce initial/update projections. | P07 Banner owner, fed by P05 committed NPC events. |
| 11 | Crafting Requests | Explicitly reject/pending under the current empty Version4 receiver; do not enable the local typed request route. | P04 owner remains sole crafting writer; no Packet 82 route until root selects a request contract. |
| 12 | TagEffect State | Emit only active Version4 server-authored projections from committed combat/status facts; reject client state. | P05 combat/status output adapter. |
| 13 | Leashed Entity | Emit full/partial/remove projections from committed entity state; reject client entity mutations. | P06 adapter after Version4 ownership audit. |
| 14 | Unbreakable Wall Scan | Explicitly reject/pending because current Version4 receive and broadcast methods are empty. | No active owner route until a source-backed notification path exists. |

The table is a target routing boundary, not a claim that every direction is already proven. Before
accepting client input for ids 0, 3, 4, 7, 9, 10, 13, or 14, the relevant child must cite the
Version4 receive/send call path and identify the authoritative writer. Unresolved behavior stays
explicitly rejected or pending; it must not be guessed from codec availability.

## Root compatibility decisions

The Version4 source tree is authoritative for what it currently executes. When Version4 retains a
wire/API shell but has an empty method body, an earlier complete snapshot may supply a compatibility
candidate; the resulting implementation must be reported as a root-selected historical
compatibility behavior, not as behavior proven by the current Version4 body.

For Text module 1 and Ping module 2, root selects the matching implementations in
`D:\TRbackup\无任何删减通过编译` as compatibility behavior for this minimal server:

- Text C2S carries two .NET strings: command id and text. The complete snapshot's
  `ChatMessage.Serialize` writes those fields in that order, `NetTextModule.DeserializeAsServer`
  reads a `ChatMessage`, and `PartyChatCommand.ProcessIncomingMessage` routes the authenticated
  sender's message only to players on the sender's nonzero team. An unteamed sender receives the
  localized `LegacyMultiplayer.10` server message; an empty Party message is consumed without a
  broadcast. Text S2C keeps the Version4 author, `NetworkText`, and RGB layout.
- Ping C2S carries two `Single` coordinates. The complete snapshot's `NetPingModule.Deserialize`
  reads the position and broadcasts it to everyone except the authenticated sender; clients add the
  decoded position to their local ping view.

Version4 `NetworkInitializer.Load` registers `NetTextModule` then `NetPingModule` immediately after
`NetLiquidModule`; `NetManager.Register` assigns sequential module ids, confirming module 1 and 2 in
the current tree. The current Version4 `NetTextModule.Deserialize`, `ChatCommandProcessor.ProcessIncomingMessage`,
`PartyChatCommand.ProcessIncomingMessage`, and `NetPingModule.Deserialize` bodies are empty. The
Version4 `MessageBuffer` routes outer packet 82 through `NetManager.Read`, which calls the registered
module's `Deserialize`; this confirms the dispatch boundary, but not the missing Text or Ping
behavior. Packet 25 is unused in the current Version4 source. SC081 therefore checks the selected
historical Text compatibility route and its S2C intent; it does not demonstrate current Version4
runtime parity or socket delivery.

For the stateful module wave, root freezes the following Version4-only receive policy after
reviewing the current module bodies, the registered creative power types, and the Infrastructure
codecs. An empty Version4 receiver or delegated power body remains unsupported; older complete
implementations are not selected for these module ids:

- Bestiary module 4 is server-authored. Accept only committed P05 kill/sight/chat facts and emit
  the current Version4 serializers' actions 0–2. Do not accept client Bestiary state; the current
  `NetBestiaryModule.Deserialize` is empty and returns false.
- Creative Powers module 5 accepts only registered power id 14's current live per-player slider
  receiver. Its wire input is the existing byte slot plus `Single` slider value; bind the effective
  player to the authenticated sender as Version4 does, and apply the registered power's permission
  rule. The other current registrations delegate to empty `DeserializeNetMessage` bodies and stay
  explicitly unsupported. The Infrastructure V4 codec is the wire-shape baseline; do not enable
  local-only slider interpretations for ids 6/7 or local-only single-player toggle actions.
- Creative Unlock Player Report module 6 remains explicitly rejected/pending. Version4's
  `NetCreativeUnlocksPlayerReportModule.Deserialize` is empty and Infrastructure models the
  module as empty. The historical player/item/amount payload is not selected; do not let a local
  typed codec turn it into a handled report.
- Creative Power Permissions module 9 is server-authored output only. Emit action 0 permission
  projections for all 15 powers when joining, using the current Version4 serializer's
  `byte action + UInt16 power id + byte level` layout. Reject client attempts to author policy;
  the current receiver is empty.
- Banners module 10 is server-authored output only. Emit current Version4 actions 0–2 from the
  Banner owner after P05 commits and on join. Reject inbound requests because the current receiver
  is empty. Historical claim actions 3/4 are not selected; keep them out of the active Version4
  codec and handler path.

These decisions preserve active Version4 behavior and label unsupported paths explicitly. They do
not claim that a module with an empty receive body has no clients that may attempt to send data.
The dispatcher must return a rejection/pending result for those attempts rather than report them as
handled.

For P07-C's delegated modules, root freezes the following route policy from the current Version4
receive bodies and callsites:

- Module 0 Liquid is server-authored output from committed P03 world changes. The current
  `NetLiquidModule.Deserialize` returns false without applying client state, so module 0 must not
  accept the local direct-assignment request handler. Keep only the current server-to-client
  changed-liquid projection path.
- Module 7 Teleport Pylon emits current server-authored pylon add/remove projections from P06
  state and join synchronization. `PlayerRequestsTeleport` has no current server receive behavior;
  reject client requests and leave movement authorization with P02/P06.
- Module 11 Crafting Requests is rejected/pending. The current `NetCraftingRequestsModule.Deserialize`
  is empty and Infrastructure's Version4 codec models this module as empty. Do not enable the local
  typed P04 request codec or return an approval for a decoded request.
- Module 12 TagEffect is server-authored output from P05 committed status facts. The current
  receiver returns false and `WriteNPCChange` is empty; reject client-authored state and emit only
  output forms with active Version4 producer callsites.
- Module 13 Leashed Entity emits current server-authored full/partial/remove projections from
  committed P06 entity state and join synchronization. Its current receiver returns false; reject
  client entity mutations.
- Module 14 Unbreakable Wall Scan remains explicitly rejected/pending. Both the current receiver
  and `BroadcastChange` are empty, so do not report client input handled or emit a local-only scan
  notification as Version4 behavior.

These decisions leave each existing empty receive path visible as unsupported while preserving
active Version4 output interactions. They do not select older implementations for these module ids.

## Work batches and gates

| Batch | Work | Gate |
| --- | --- | --- |
| P07.0 Evidence freeze | Read Version4 module registrations, receive/send handlers, and relevant state callbacks; compare complete historical bodies where Version4 methods are empty; compare the current Infrastructure DTO/codecs; record direction, field shape, owner, and compatibility decision per module. | Each accepted behavior has source/version evidence or an explicit root compatibility decision; every remaining unknown has an explicit policy and owner. |
| P07.1 Existing slice completion | Complete Social/Presentation adapters for Text, Ping, Ambience, and Particles. Wire presentation producers to committed domain events and authenticated recipient facts. | No handler trusts a payload author/slot; no client-controlled packet directly chooses its audience; rate-limit state is epoch-scoped. |
| P07.2 Stateful modules | Complete Bestiary, Banner, Creative Power, permission, and unlock-report owner adapters. Validate capture/restore inputs and initial-session projections. | One writer per state; snapshots are immutable; restore validates before publication; initial snapshots have deterministic order. |
| P07.3 Delegated adapters | Add explicit narrow adapters for modules owned by P03/P04/P05/P06. Freeze their public APIs and rejection behavior. | No P07 adapter duplicates a delegated owner; a delegated module cannot be reported handled without a commit/rejection result. |
| P07.4 Registry and lifecycle | Compose one handler for every Version4 module id, wire dispatcher replay state and particle rate limits into P02 disconnect cleanup, and connect outbound intents to the root encoder/sender path. | Registry covers ids 0–14 exactly once; close retires the epoch; no domain handler writes directly to a socket. |
| P07.5 Verification and evidence | Run only the frozen selected scenario, compile the affected Server and Modules verifier projects, and record status for all unrun scenarios. | The 10% test cap is respected; build and run evidence has command, exit code, warning/error count, and output path. |

## Frozen P07 functional test budget

The core inventory is ten scenarios, `SC081`–`SC090`. The allowed functional budget is exactly one
unique scenario (10%). SC081 was selected before execution because it exercises the end-to-end
decode, authenticated-context dispatch, recipient filtering, and S2C encoding seam. It has run and
passed against the root-selected historical Text compatibility route. That result does not establish
current Version4 receive behavior or socket delivery. No other P07 scenario may run in this plan.

| Scenario | Planned core assertion | Status |
| --- | --- | --- |
| SC081 | Historical Text compatibility decode, authenticated author, same-team recipient set, S2C text encoding. | PASS; only selected scenario; compatibility evidence, not a claim about empty Version4 receive bodies. |
| SC082 | Ping visibility and authenticated sender projection. | NOT RUN. |
| SC083 | Ambience server-authored intent and event-to-recipient mapping. | NOT RUN. |
| SC084 | Bestiary contribution authority, snapshot capture, and restore. | NOT RUN. |
| SC085 | Particle payload bounds, identity binding, rate limit, and epoch retirement. | NOT RUN. |
| SC086 | Creative power kind/permission validation and commit projection. | NOT RUN. |
| SC087 | Creative permission and unlock-report direction/authority. | NOT RUN. |
| SC088 | Banner contribution aggregation and initial snapshot. | NOT RUN. |
| SC089 | Delegated module adapter routes and owner commit result propagation. | NOT RUN. |
| SC090 | Complete registry, replay behavior, disconnect retirement, and outbound intent encoding. | NOT RUN. |

Builds are compile checks, not additional functional scenarios. SC082–SC090 remain unrun even if
their source compiles or receives static review.

## Parallel subplan boundaries

Write and review these subplans one at a time. Open one child session only after its subplan is
complete. Children inherit the root history and work only within their assigned source boundary;
root remains the integration and build owner.

| Subplan | Owns | Must not edit |
| --- | --- | --- |
| [P07-A Social and Presentation](P07-A-social-presentation-plan.md) | Text/Party, Ping, Ambience, Particles, producer API from committed events, per-epoch presentation limits. | Shared Server project, common Packet 82 registry, P02 session close path, other owners. |
| [P07-B Stateful Module Owners](P07-B-stateful-module-owners-plan.md) | Bestiary, Banner, Creative Power/permissions, creative unlock report boundary, immutable capture/restore and initial projections. | Shared Server project, common Packet 82 registry, P04 inventory writer, P05 NPC writer. |
| [P07-C Delegated Adapters and Lifecycle](P07-C-delegated-adapters-lifecycle-plan.md) | Narrow adapters for delegated module owners, exact registration requirements, epoch retirement contract, unresolved ownership handoff. | Delegated P03/P04/P05/P06 state implementations, shared project files, functional test execution. |

Root integrates only after each child reports its changed files, public APIs, Version4 evidence,
remaining gaps, and any required compile includes. Builds remain serialized by root.

## Build and evidence record

- The first Modules incremental build exited 1 with 0 warnings and 2 `CS0246` errors because the new Ping handler was missing the `MinimalImplementation.Server.Modules.Protocol` import. The import was added; no generated output was moved. Log: `Build/diagnostics/MinimalImplementation/SC081/Modules-build.log`.
- Modules incremental build: `dotnet build src/MinimalImplementation/Modules/MinimalImplementation.Modules.csproj -c Debug --no-restore`; after correcting one missing namespace import, final exit code 0, 0 warnings, 0 errors. Output: `Build/bin/MinimalImplementation.Modules/Debug/net10.0/MinimalImplementation.Modules.dll`; log: `Build/diagnostics/MinimalImplementation/SC081/Modules-build-final.log`.
- Server incremental build: `dotnet build src/MinimalImplementation/MinimalImplementation.Server.csproj -c Debug --no-restore`; final exit code 0, 0 warnings, 0 errors. Output: `Build/bin/MinimalImplementation.Server/Debug/net10.0/MinimalImplementation.Server.dll`; log: `Build/diagnostics/MinimalImplementation/SC081/Server-build-final.log`.
- Modules verifier incremental build: `dotnet build src/MinimalImplementation/Verification/Modules/MinimalImplementation.Verification.Modules.csproj -c Debug --no-restore`; final exit code 0, 0 warnings, 0 errors. Output: `Build/bin/MinimalImplementation.Verification.Modules/Debug/net10.0/MinimalImplementation.Verification.Modules.dll`; log: `Build/diagnostics/MinimalImplementation/SC081/Modules-verifier-build-final.log`.
- Functional scenario: `dotnet run --project src/MinimalImplementation/Verification/Modules/MinimalImplementation.Verification.Modules.csproj -c Debug --no-build --no-restore -- --scenario SC081 --output-root D:/TRbackup/NLTX/Build/diagnostics/MinimalImplementation/SC081`; final exit code 0. SC081 remains the only unique scenario selected/run; refreshed result: `Build/diagnostics/MinimalImplementation/SC081/SC081-result.md`; log: `Build/diagnostics/MinimalImplementation/SC081/SC081-run-final.log`.
- Build logs and exit-code records are in `Build/diagnostics/MinimalImplementation/SC081/`.

Passing SC081 and the incremental builds close only the listed compile and chat-intent checks. They
do not close the delegated module owners, production listener, or the unrun SC082–SC090 behavior.
