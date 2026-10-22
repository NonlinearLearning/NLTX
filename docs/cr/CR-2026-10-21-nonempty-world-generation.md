# CR-2026-10-21: Integrate Full Generation Into Server World Creation

- ID: `CR-2026-10-21-NONEMPTY-WORLD-GENERATION`
- Status: implemented; runtime acceptance follow-ups in progress
- Date: 2026-10-21
- Level: major
- Scheduled iteration: current task, dedicated multi-batch implementation
- Execution strategy: `spec`
- Approved by: user
- Related change: [CR-2026-10-21-worldfile-326-write.md](CR-2026-10-21-worldfile-326-write.md)

## Original request

> 没有加载出世界数据,整个世界是空的

## Confirmed change request

> 按重大变更接入完整生成器

The change was recorded and initially assessed in
`.agent-workplace/changes/2026-10-21-nonempty-world-generation-draft.md`. The user confirmed
the full-generator scope and major classification, then selected `本任务 + spec（推荐）` for
schedule and execution strategy.

## Problem evidence

- The isolated 326 world file used for diagnosis is valid but its Tile section encodes 4,200
  columns of 1,200 empty tiles. The empty file content, not the world loader, explains the blank
  world.
- `P08BServerRuntime.TryCreateWorldDocumentAsync` is the active create-world API implementation and
  currently calls `WorldFileTilePayloadEncoder.EncodeEmpty` for a 4200x1200 world.
- `NSSLC.WorldGeneration.WorldCreation.Create` produces a `GeneratedWorld` with tiles and metadata,
  but its project references unavailable projects in `src/NSSLC/Component/WorldSession` and
  `src/NSSLC/Component/SpatialSimulation`.
- The generation project has 318 source files and currently references removed `WorldSession` and
  `SpatialSimulation` project paths, so current buildability must be checked. Existing
  `WorldGeneration/README.md` and the 2026-10-04 persistence round-trip report document successful
  standard-world generation and full projection in earlier diagnostics. `Snapshots/TileSnapshot.cs`
  has empty save/restore methods, but the README explicitly says world creation does not use disk
  snapshots; these are not in scope unless call-path evidence makes them necessary.
- The generator's `GeneratedWorld` and the current minimal server's `WorldTile` / P03 persistence
  document are separate models. A complete integration must use or adapt the existing generated-
  world projection into the minimal server's document, not only replace the empty tile encoder call.
- The existing WorldFile encoder writes 326; current loading remains compatible with supported
  319 and 326 files. Latest-client loading of a newly generated output remains to be verified.
- An existing 319 save was loaded on isolated listener `127.0.0.1:7782` as a diagnostic fallback.
  Do not stop or replace the existing listener on `127.0.0.1:7780` during implementation or
  validation.

## Scope

Connect the full terrain generator to the existing server create-world API. The API must generate a
non-empty world, project its tile data and supported creation metadata into the server's P03 model,
write WorldFile 326, and pass the result through the existing validation, decoding, and publication
path. Reuse the existing `WorldCreationRequest` inputs and creation entry point. Preserve existing
319 and 326 load support and the current refusal to overwrite an existing world file.

The implementation covers only generator project dependencies required for server generation, the
generated-world projection into the minimal server's contract, creation/persistence integration,
and targeted diagnostics. Reuse existing generator behavior; repair a runtime function only if the
server creation path uses it and verification demonstrates a defect. Network protocol changes,
imported-world repair, legacy 319 writes, and changes to existing listeners or running worlds are
out of scope.

## Acceptance criteria

1. The existing create-world API generates a non-empty tile map whose dimensions match its buffer
   and the active server world facts.
2. The generated output is WorldFile 326 and passes the current validator, decoder, and loader.
3. Tile data, spawn, surface and rock layers, world identity/settings, and supported generated
   chests, signs, and tile entities survive projection and reload.
4. Generation, projection, or validation failure does not commit a partial file. Create-world
   never overwrites an existing world file.
5. Existing version 319 and 326 loading remains supported.
6. An isolated server startup confirms loaded non-empty tile data. When the real client is available,
   check the generated world on an isolated listener. Leave `127.0.0.1:7780` untouched.

## Impact assessment

```json
{
  "status": "scheduled",
  "change_id": "CR-2026-10-21-NONEMPTY-WORLD-GENERATION",
  "level": "major",
  "impact": {
    "db_change": false,
    "refactor": true,
    "affected_features": [
      "server world creation",
      "WorldFile 326 persistence",
      "WorldFile 319 and 326 loading compatibility"
    ]
  },
  "decision": "accepted",
  "approved_by": "user",
  "scheduled_iteration": "current task (2026-10-21)"
}
```

- Persistence schema: no new database or file format version; continue writing WorldFile 326.
- Runtime: generation is CPU- and memory-intensive, uses shared static state, and requires guarded
  lifecycle, cancellation, and failure cleanup.
- Compatibility risk: generated tile IDs, frame flags, object records, and facts must agree across
  the generator, server world model, and 326 serializer while retaining 319/326 load behavior.
- Estimate: at least four implementation and verification batches. The initial readiness batch
  must resolve project references and confirm current generator output; historical evidence lowers
  generation-algorithm risk, while minimal-server projection and API integration remain new work.
- Verification boundary: build affected projects, inspect generated tile/data counts, validate and
  reload the resulting document, and use only isolated runtime/client validation. Keep output under
  `Build/bin/` and `Build/runtime/`.

## Change history

| Decision | User answer | Outcome |
| --- | --- | --- |
| Scope and level | `按重大变更接入完整生成器` | Full generator integration; major |
| Schedule and strategy | `本任务 + spec（推荐）` | Current task; formal spec batches |

## Implementation and runtime acceptance

The create-world API now calls the complete generator and projects its output through the current
tile encoder, generated metadata projection, initial document builder and atomic document saver.
The saved document is validated and loaded through the existing publication path. Generator-local
housing and metrics calculations replace its unavailable references to deleted ECS projects.
Creation retains both an initial existence check and a no-overwrite atomic file move.

The generated save is WorldFile 326, 4200x1200, 2,984,847 bytes, with 2,662,645 active tiles,
174 chests, 2 NPCs, spawn (2095,232), surface 331 and rock layer 427. The current seed has no signs
or tile entities, so nonempty contents of those sections are not covered by this run. The generated
326 and existing 319 worlds remain loadable. Diagnostic outputs are under `Build/runtime/`.

Real-client entry exposed two runtime dependencies hidden by the previous empty map: a missing
liquid random source and liquid seeding that overflowed the 75,000-entry queue. The Host now
supplies its random adapter; world publication seeds only unsettled liquid cells. Manual update
diagnostics completed ten Applied updates on both the generated 326 and the existing 319 save.
The user then confirmed `能看到地形并正常进入`.

The next report, `但是依旧有新问题,到达一定格数之后新的地形不会被加载出来`, identified
an absent Host movement-to-section-send path. Accepted player movement now captures unseen sections
in a radius-1 neighborhood, then sends packet 9 and the existing packet-10 fragment groups using
the transfer receipts and visibility ledger. A header-only check avoids copying the world on every
movement packet. Controls now receive bounded wetness/dismount collision facts; the resize uses
the existing server tile-movement queries. Exact parity with the client's separate slope-resize
probes is not claimed.

At 21:03:18–21:03:29 the real client's log records successful movement batches for new section
columns 7, 6 and 5. Visual acceptance was interrupted by the next report: `新问题使用武器会断开
连接这是什么导致的`, clarified as `所有武器`. At 21:03:43 the transport closed with
`ProtocolFailure` and a packet trailing-bytes error. The current 326 client has changed projectile
27/29 wire identities relative to the server's older codecs; the precise failing message is being
identified before assigning this occurrence to a particular packet.

These bounded Host/runtime repairs continue the user's playability acceptance. They do not claim
complete 326 combat support; unrelated packet routes and a full protocol migration remain outside
the generation implementation. The listener on `127.0.0.1:7780` remains PID 36472. The current
diagnostic instance uses port 7783 and `--no-save-on-stop` against the generated save.

### Continued playability acceptance

The user reported:

> 问题已解决现在新的问题,破坏物体不会掉落东西,例如破坏蘑菇不会掉落,破坏物块不会掉落,并且再次进入服务器之前的破坏都消失了,丢弃物品会立刻断开连接

The user asked to continue implementation. Log and source evidence identify these blockers:

- Packet 17 is decoded but has no Host route, so tile changes are never committed to the
  authoritative world. This also explains why reconnecting during the same server process restores
  the old terrain.
- Packet 21 accepts only its legacy 24-byte base layout, while the supplied client appends optional
  shimmer and enemy-pickup-delay fields. The observed disconnect is `TrailingBytes` at offset 24.
- World-item stores and transaction/drop planning already exist, but the built-in Host does not
  consume the world-item projection journal. A committed item alone would not appear on connected
  clients.

The accepted follow-up scope is to route Packet 17 through the authoritative world owner, create
supported tile drops through the shared inventory/world-item transaction, accept current Packet 21
extensions, and drain committed item projections. The original 7780 listener remains protected;
the isolated 7783 process may only be replaced after its live PID and command line are rechecked.
This work does not claim full Terraria tile-destruction parity for multi-tile furniture or every
random and conditional drop rule.
