# Version4 非权威 P14 proposed component execution plan：持久化、恢复与配置

> 本文件源自后续实施计划；当前 checkpoint 记录本 session 已实际完成的 Component 落地和构建结果。除明确列出的 Component 外，其他 proposed 文件未创建，迁移整体、行为等价及最终跨分区 owner 裁决均未完成。

partitionId: P14
sessionId: 57c42a509de04effaa77d81db8a90942
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\14-persistence-recovery-configuration.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-component-execution.md
designStatus: proposed
executionStatus: completed
implementationStatus: completed
verificationStatus: build-passed-no-tests
completedComponents:
- P14.C01 SaveReferenceAndFavoritesAdapter
- P14.C02 WorldSaveCompositionBoundary
- P14.C03 PlayerSaveSessionComponent
- P14.C11 FilePlatformBoundary
currentComponent: none (remaining proposed boundaries are deferred by the Component-only implementation scope)
pendingComponents: []
deferredComponents:
- P14.C03 PlayerSaveSessionBoundary non-Component roles (snapshot, clock, serializer, command, adapter, system)
- P14.C04 WorldIdentitySnapshot (proposed persistence snapshot, not an ECS Component)
- P14.C05 WorldValidityAndRulesHandoff (query/projection/handoff, not an ECS Component)
- P14.C06 WorldTileHeaderCoreCodec (protocol codec, not an ECS Component)
- P14.C07 WorldTileHeaderExtensionCodec (protocol codec, not an ECS Component)
- P14.C08 WorldRecoveryTransactionAdapter (adapter, not an ECS Component)
- P14.C09 WorldTemporaryEventContext (transient persistence context, not an ECS Component)
- P14.C10 SaveConfigurationAndMetadataAdapters (adapters, not ECS Components)
lastCheckpointUtc: 2026-09-12T08:08:07.391Z
evidence-gap: The P14 first-round outputReport named by the partition prompt does not exist. The current session changed only the Component source listed below; player serializer, player clock, active-player binding owner, persistence snapshot ownership, seed parser/rules ownership, Tile/Liquid/rendering codec ownership, cloud recovery semantics, Calendar/Event ownership, configuration compatibility, path security and cross-partition IDs remain unverified. Existing non-Component files in src2 are not claimed as changes by this session.
blocking-decision: PersistentEntityId/WorldEntityId, player save/session owner, WorldDescriptor versus persistence snapshot owner, rules/progression owner, Tile/Liquid/rendering codec owner, snapshot schema, file-platform service owner, path security/normalization, cloud retry semantics, runtime capability compatibility, and final owner/order for shared World, Tile, Player, Event, Network, and external file identities remain crossSubsystemOwner: integration-review. The deferred C04-C10 roles cannot be implemented under the current Component-only boundary.

implementationCheckpoint:
- component: P14.C01 SaveReferenceAndFavoritesAdapter
  implementationStatus: completed
  verificationStatus: focused-verified
  files: src2/WorldStorage/Persistence/SaveFileType.cs, src2/WorldStorage/Persistence/FileMetadataValue.cs, src2/WorldStorage/Persistence/FileMetadataCodec.cs, src2/WorldStorage/Persistence/SaveFileReferenceSnapshot.cs, src2/WorldStorage/Persistence/FavoritesIndexAdapter.cs
- component: P14.C11 FilePlatformBoundary
  implementationStatus: completed
  verificationStatus: focused-verified
  files: src2/WorldStorage/Platform/ExtensionFilterValue.cs, src2/WorldStorage/Platform/FilePathParsingAdapter.cs, src2/WorldStorage/Platform/RuntimeCapabilityQuery.cs, src2/WorldStorage/Platform/FilePlatformFailureKind.cs, src2/WorldStorage/Platform/FilePlatformFailure.cs, src2/WorldStorage/Platform/FilePlatformOperationResult.cs, src2/WorldStorage/Platform/FileReadResult.cs, src2/WorldStorage/Platform/FileExistenceResult.cs, src2/WorldStorage/Platform/ICloudFileStore.cs, src2/WorldStorage/Platform/FilePlatformAdapter.cs
- component: P14.C02 WorldSaveCompositionBoundary
  implementationStatus: completed
  verificationStatus: focused-verified
  files: src2/WorldStorage/Persistence/WorldSaveStage.cs, src2/WorldStorage/Persistence/WorldBackupPolicy.cs, src2/WorldStorage/Persistence/WorldSaveCommand.cs, src2/WorldStorage/Persistence/WorldSaveEncodeResult.cs, src2/WorldStorage/Persistence/IWorldSaveEncoder.cs, src2/WorldStorage/Persistence/WorldSaveValidationQuery.cs, src2/WorldStorage/Persistence/WorldSaveProjection.cs, src2/WorldStorage/Persistence/WorldSaveTransactionSystem.cs
- component: P14.C03 PlayerSaveSessionComponent
  implementationStatus: completed
  verificationStatus: build-passed-no-tests
  files: src2/Player/PlayerSaveSessionComponent.cs
  dependency-impact: The Component keeps save name/path/cloud metadata, accumulated play time and transient timer-active state. Existing snapshot/system consumers were not modified; no System, Query, Command, Adapter, Projection or test was added.
  unverified: player serialization, monotonic clock effects, active-player binding and server-side-character policy remain outside this Component-only change.
- verification: `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build','.\\src2\\Terraria.NonAuthoritative.Persistence.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false'); exit $LASTEXITCODE"`; exit code `0`, warning count `0`, error count `0`; artifact `D:\\TRbackup\\NLTX\\Build\\bin\\Terraria.NonAuthoritative.Persistence\\Debug\\net10.0\\Terraria.NonAuthoritative.Persistence.dll`; no test code or test command was added.
- dependency-impact: C01/C02/C11 entries are prior checkpoints. This session changed only `src2/Player/PlayerSaveSessionComponent.cs`; C04-C10 remain deferred and no other partition was touched.
- unverified: player serializer behavior, provider timeout/result-unknown injection, native file-browser consumer, path-security policy and integration-review ownership remain open.

## Current implementation-session boundary

The current session is restricted to Component source under `src2`. It updated only
`src2/Player/PlayerSaveSessionComponent.cs`. The remaining proposed C03 roles and all C04-C10
roles are deferred because they are snapshots, queries, codecs, adapters, systems, commands or
projections rather than Component state. No test, verifier, registration, project file, or
non-Component source was added by this session.

## 1. Proposed targets and implementation boundary

The C01, C02 and C11 target paths below are implemented in `src2` and focused-verified. The
current session additionally implemented only the `PlayerSaveSessionComponent` state and
verified its project build; the remaining proposed paths are still `status: proposed` until
their source and verifier checkpoints are completed.

| Proposed target | Proposed responsibility | Source boundary |
|---|---|---|
| `src2/WorldStorage/Persistence/SaveFileReferenceSnapshot.cs` | immutable file/path/cloud/type/favorite snapshot | `FileData`, `FileMetadata` |
| `src2/WorldStorage/Persistence/FavoritesIndexAdapter.cs` | local/cloud favorites JSON and commit effects | `FavoritesFile`, `Main.LocalFavoriteData`, `Main.CloudFavoritesData` |
| `src2/WorldStorage/Persistence/WorldSaveCommand.cs` | explicit save intent and backup policy | `WorldFile.SaveWorld` |
| `src2/WorldStorage/Persistence/WorldSaveTransactionSystem.cs` | capture/encode/commit/read-back/validate/backup order | `WorldFile._SaveWorld`, `InternalSaveWorld` |
| `src2/WorldSession/WorldDescriptorPersistenceSnapshot.cs` | world identity and file metadata snapshot | `WorldFileData` identity members |
| `src2/WorldSession/WorldValiditySnapshot.cs` | load status and derived validity projection | `WorldFileData` validity members |
| `src2/WorldStorage/Persistence/WorldTileHeaderCodec.cs` | core/extension header bit protocol | `WorldFile.TilePacker`, `SaveWorldTiles`, `LoadWorldTiles` |
| `src2/WorldStorage/Persistence/WorldRecoveryTransactionAdapter.cs` | lock, version, cloud and failure/recovery effects | `WorldFile` recovery members, `WorldGen.serverLoadWorldCallBack` |
| `src2/WorldSession/Calendar/WorldTemporaryEventContext.cs` | transient event capture/restore payload | `WorldFile` `_temp*` members |
| `src2/WorldStorage/Persistence/ConfigurationAdapter.cs` | Preferences/GameConfiguration and format policy | `Preferences`, `GameConfiguration` |
| `src2/WorldStorage/Platform/FilePlatformAdapter.cs` | file/cloud/platform/path/filter capabilities | `FileUtilities`, `ExtensionFilter`, `NewRuntimeMethods` |
| `src2/WorldStorage/Platform/ExtensionFilterValue.cs` | immutable file-browser filter name and extension values | `ExtensionFilter` |
| `src2/WorldStorage/Platform/FilePathParsingAdapter.cs` | pure filename and parent-path projections | `FileUtilities.FileNameRegex` |
| `src2/WorldStorage/Platform/RuntimeCapabilityQuery.cs` | explicit runtime capability result | `NewRuntimeMethods.IsNet45OrNewer` |
| `src2/Player/PlayerSaveSessionComponent.cs` | player save metadata and playtime session state | `PlayerFileData` |
| `src2/Player/PlayerSaveSystem.cs` | timer transitions and save command seam | `Main.Update`, `Player.SavePlayer` |

The proposed paths follow domain-first layout. No generic `Shared/Components/`, `Utils/`, or `Manager/` catch-all path is proposed. A final implementation must preserve public APIs until callers are migrated and must record source/target dependency impact before moving files.

## 2. Checkpoint C01 execution plan: save references and favorites

Source-to-role mapping:

| Source member group | Proposed target / role | Implementation order |
|---|---|---:|
| `FavoritesFile.Path`, `IsCloudSave`, `_data`, `_ourEncoder` | `FavoritesIndexAdapter` private adapter state; never a component | 1 |
| `FileData._path`, `_isCloudSave`, `Metadata`, `Name`, `Type`, `_isFavorite`, `Path`, `IsCloudSave`, `IsFavorite` | `SaveFileReferenceSnapshot` plus explicit favorite command | 2 |
| `FileMetadata.MAGIC_NUMBER`, `SIZE`, `Type`, `Revision`, `IsFavorite` | `FileMetadataCodec` value/protocol seam | 3 |
| `Main.LocalFavoriteData`, `CloudFavoritesData`, `WorldFileMetadata` | composition injection and world save protocol handoff | 4 |

Single write owner: `FavoritesIndexAdapter` for favorites bytes; `WorldSaveTransactionSystem` for world `FileMetadata` revision writes. No direct component mutation may write files. `QueryFavorite` receives a snapshot and does not touch I/O.

Compatibility plan: retain `FavoritesFile`/`FileData` API as an adapter facade during migration; read old JSON shape; write the same key shape until an explicit format version is approved. Preserve 20-byte metadata output and reject invalid magic/type rather than silently accepting it.

## 3. Checkpoint C02 execution plan: world save composition

Proposed sequence and rollback:

1. `CaptureWorldSnapshot` reads domain queries and a transient event context.
2. `EncodeWorldFile` emits format header, world header, tiles, remaining sections and footer into a bounded buffer.
3. `CommitWorldBytes` acquires the proposed lock port and writes local/cloud storage.
4. `ReadBackWorldBytes` reads the committed bytes and runs the proposed validation query.
5. On validation success, rotate the configured backup chain and publish a save-committed projection.
6. On validation failure or explicit write failure, preserve the prior backup, return a classified failure, and do not publish `SaveCommitted`.

The current Version4 implementation reads the just-written file before backup rotation (`WorldFile.cs:963-989`) and uses a lock (`:909-928`). These ordering constraints must be preserved. A boolean file-write result is not sufficient proof of atomicity, especially for cloud writes.

Network/snapshot policy: no file bytes or `FileMetadata.Revision` are sent as network authority. A separate network projection may consume a validated world snapshot only after integration review. Persistence schema migration must be version-gated and backward-readable; unknown future versions remain a load rejection or recovery decision, not a best-effort parse.

## 4. Checkpoint C03 execution plan: player save session boundary

The non-Component targets in this section remain `status: proposed`; this session created only
the `PlayerSaveSessionComponent` state recorded in the implementation checkpoint.

| Source member | Target or role | Planned effect boundary |
|---|---|---|
| `543 _player`, `638 Player` | proposed `src2/Player/PlayerSaveSessionBinding.cs` or a proposed binding inside `PlayerSaveSessionComponent.cs` | active player relation and display-name projection only; no implicit save or gameplay mutation |
| `544 _playTime` | implemented `src2/Player/PlayerSaveSessionComponent.cs` accumulated-duration state | stores non-negative accumulated duration; load/set command, timer transitions and serializer remain deferred |
| `545 _timer` | proposed `IMonotonicClock`/timer adapter behind `PlayerSaveSystem` | transient elapsed-time effect; never stored in a Component or serialized |
| `546 _isTimerActive` | implemented `src2/Player/PlayerSaveSessionComponent.cs` transient session flag | stores timer-active state; start/stop transitions and any persistence/network projection remain deferred |
| `639 ServerSideCharacter` | proposed `PlayerSavePolicy`/policy result | explicit server-side policy command; save adapter is the single writer of player bytes |
| `640 LastPlayed` | proposed `LastPlayedProjection` | pure read-only projection from saved timestamp; no independent write path |
| `47 Main.ServerSideCharacter` | composition input to the proposed policy query | remains owned by Main/server composition until `crossSubsystemOwner: integration-review` resolves the handoff |

### C03 implementation sequence and compatibility

1. Preserve the legacy `PlayerFileData` facade and capture the current active-player/path/cloud metadata into a proposed immutable session snapshot. Do not replace the Version4 empty serializer by assumption.
2. Introduce a proposed clock port and pure play-time transition rules. The adapter supplies elapsed duration; the system owns active-state transitions and accumulated duration. Use the complete-reference `GetPlayTime`, `UpdatePlayTimerAndKeepState`, `SetPlayTime` behavior as compatibility evidence, then add tests for a fake clock.
3. Introduce a proposed policy query that distinguishes `Main.ServerSideCharacter` from `PlayerFileData.ServerSideCharacter`. Preserve the outer `SavePlayer` gate and classify a skipped file write as policy, not as a successful serialized commit.
4. Add a proposed player-file adapter only after the Version4/full-reference serializer delta is enumerated. Preserve version `319`, `FileMetadata` placement, local `.bak` behavior, encryption and cloud/local separation where evidence requires it. Unknown versions, malformed bytes, missing paths and cloud result-unknown outcomes must remain classified failures or recovery states.
5. Migrate callers one at a time through the facade, then remove duplicate writes only after focused verifiers pass and integration review approves the player/persistent/network ID ownership.

Single write owner: the proposed `PlayerSaveAdapter` owns player save bytes and local/cloud file effects. The proposed play-time system writes only session state. A projection/query never writes bytes, advances the timer, or invokes cloud APIs.

Rollback conditions: restore the facade if playtime changes across pause/focus transitions, if a server-side character emits player bytes, if a legacy player file cannot be read, if encryption/metadata ordering changes, if a cloud write result is unknown but reported successful, or if `LastPlayed` is made independently writable. No old player/save file may be deleted as part of rollback.

## Checkpoint C04 execution plan: world identity snapshot

All paths and types in this section are `status: proposed`; none has been created by this session.

| Source member group | Proposed target or role | Compatibility constraint |
|---|---|---|
| `610 GUID_IN_WORLD_FILE_VERSION`, `616 WorldGeneratorVersion` | proposed `WorldIdentityProtocol` and `WorldDescriptorPersistenceSnapshot` fields | preserve the `777389080577` threshold and keep GUID map naming separate from WorldId naming |
| `611 MAX_USER_SEED_TEXT_LENGTH`, `617 _seedText`, `618 _seed` | proposed seed validation policy, raw seed snapshot and pure derived-seed query | validate accepted text before snapshot; retain raw text as the round-trip source; do not serialize a second conflicting seed authority |
| `612 CreationTime`, `613 LastPlayed` | proposed timestamp value fields plus clock/filesystem fallback adapter | preserve local/cloud and legacy-version fallback semantics; no ambient clock call from a pure snapshot or query |
| `614 WorldSizeX`, `615 WorldSizeY`, `623 _worldSizeName` | proposed dimensions value and localization projection | allocate sections only after dimension validation; keep `LocalizedText` out of ECS/persistence values |
| `621 UniqueId`, `622 WorldId` | proposed distinct identity fields and map-name query | preserve header/footer validation and legacy map-file naming; defer shared persistent-world owner |
| `624 GameMode` | proposed header snapshot field and rules handoff | preserve encoded value while deferring gameplay mode owner to integration review |

### C04 implementation sequence

1. Compare `WorldDescriptorState` and `WorldDescriptorSnapshotValue` with the proposed file snapshot and record the missing file-version, metadata, timestamp-source and localization dependencies. Do not merge the existing runtime descriptor into a persistence component by name.
2. Add a pure proposed identity snapshot constructor/validator with explicit `WorldId`, `UniqueId`, dimensions, seed text, generator version, mode and timestamp inputs. Invalid dimensions and overlong seed text must produce a classified rejection before storage allocation.
3. Route metadata scan and world-header encode/decode through the snapshot adapter while retaining the legacy `WorldFileData` facade. Preserve Version4 version gates, GUID threshold, creation/last-played fallbacks and footer identity checks.
4. Add focused round-trip tests for legacy and GUID map names, seed text/derived seed, dimensions, timestamps, mode, missing localization and local/cloud fallback. Only after those pass may the duplicate descriptor fields be retired under integration review.

Single write owner: the proposed world-header/persistence adapter writes identity bytes; world generation owns creation of new identity values; runtime descriptor systems publish snapshots but do not write files. Roll back if header bytes, footer validation, legacy map naming, cloud/local time fallback or section-allocation order changes.

## Checkpoint C05 execution plan: world validity and rules handoff

All types and paths below are `status: proposed`; no implementation is being performed in this session.

| Source member group | Proposed target or role | Planned compatibility behavior |
|---|---|---|
| `619 LoadStatus`, `620 LoadException` | proposed `WorldValiditySnapshot` and `WorldLoadFailure` value/diagnostic adapter | classify status and preserve diagnostics behind a port; never store an exception object in a Component or send it over network |
| `625-633` special-seed flags | proposed `WorldSeedRulesHandoff` and `WorldSeedRulesRegistry` adapter | preserve version-gated defaults and legacy Zenith derivation; defer gameplay owners to WorldGeneration/WorldSession integration review |
| `634 HasCorruption` | proposed single evil source in rules handoff | retain `HasCrimson` as an inverse projection/compatibility command, not a second field |
| `635 IsHardMode`, `636 DefeatedMoonlord` | proposed progression snapshot inputs | preserve header values but keep transition writers in progression/boss systems |
| `637 seedOptionsInOrder` | proposed immutable registry/adapter input | do not serialize the runtime option object list; close parser evidence before migration |
| `641-648` properties | proposed pure validity, seed, localization and map-name queries | preserve formulas and threshold; queries perform no I/O or writes |

### C05 implementation sequence

1. Define a proposed stable failure/status vocabulary and an exception-to-diagnostic adapter. Keep `WorldFileData.LoadStatus` as a compatibility facade while old callers migrate.
2. Decode version-gated rule flags into an immutable proposed handoff. Verify defaults, legacy combinations and invalid combinations before any world-generation or progression system consumes them.
3. Implement pure projections for validity, seed, size label, evil polarity, GUID map name and map filename. Test that projections cannot mutate identity or rules state.
4. Connect the handoff to WorldSession/WorldGeneration through an explicit command or snapshot port. Do not write `WorldRulesState` directly from the file adapter until `crossSubsystemOwner: integration-review` resolves the owner.
5. Preserve the recovery boundary: invalid/future files return classified metadata, no playable-world publication, and no `WorldLoaded` event. Roll back if any malformed or later-version input becomes playable or if an exception leaks into ECS/network state.

Single write owner: proposed world-file codec/adapter for file status and decoded header bytes; WorldGeneration/Progression systems for rule transitions. The C05 query/projection layer is read-only.

## Checkpoint C06 execution plan: world Tile header core codec

All codec names, paths and interfaces below are `status: proposed`; no codec file was created.

| Source members | Proposed target or role | Planned verifier |
|---|---|---|
| `547-556 Header1_*` | proposed `WorldTileHeaderCoreCodec` Header 1 masks and run-length selector | continuation, active/wall/liquid, high tile ID and one/two-byte run vectors |
| `557-565 Header2_*` | proposed core Header 2 mask table; extension handoff for C07 bits | wire 1-3, slope/half-brick, color, wall high-byte and continuation vectors |

### C06 implementation sequence

1. Define proposed immutable `TileEncodeView`, `TileDecodeValue` and `TileHeaderDecodeResult` values. They contain protocol values only and do not contain `Tile` references, file streams or mutable stores.
2. Implement pure mask/field encoding and decoding against byte-array/stream seams, preserving the Version4 layered continuation layout and unsigned widths. Treat malformed/truncated bytes, invalid IDs, impossible run lengths and end-of-section mismatches as classified failures.
3. Keep C07 extension masks as a separate proposed codec collaborator with one explicit handoff; do not duplicate Header 3/4 interpretation in the core codec.
4. Use the same proposed decoder for load and read-back validation. Add round-trip vectors for empty, active, wall, liquid, wires, slopes, high IDs, compressed runs, zero-length and overrun inputs before connecting to `TileMapStore`.
5. Migrate the legacy `WorldFile` calls behind the facade and retain the existing byte format until the vectors and integration ownership are approved.

Single write owner: the proposed world tile codec writes bytes; `TileMapStore`/Tile systems remain the runtime data owners. Roll back if byte offsets, compression boundaries, invalid-input classification or Tile/Liquid ownership changes.

## Checkpoint C07 execution plan: world Tile header extension codec

All extension names, paths and interfaces below are `status: proposed`; no implementation file was created.

| Source members | Proposed target or role | Planned verifier |
|---|---|---|
| `566-573 Header3_*` | proposed `WorldTileHeaderExtensionCodec` Header 3 masks and extension payload decoder | continuation, colors, actuator/inactive, wire4, wall high byte and shimmer vectors |
| `574-581 Header4_*` | proposed Header 4 mask table | invisible/fullbright flags, reserved-bit policy and truncated extension rejection |

### C07 implementation sequence

1. Define proposed extension values separate from runtime Tile/rendering/Liquid types: `TileMechanismFlags`, `TileVisibilityFlags`, `TileBrightnessFlags`, `WallExtensionValue` and `LiquidKindExtension` are candidate names only and remain integration-review.
2. Implement the C06-to-C07 continuation seam so Header 3 and Header 4 are consumed only when announced. Preserve byte order and use unsigned wall high-byte assembly.
3. Add pure vectors for each extension bit, combined flags, missing continuation bytes, invalid wall IDs, shimmer with/without liquid, and reserved Header 4 bits.
4. Reuse the extension decoder from `ValidateWorld`; validation must not write Tile state, publish rendering changes or invoke Liquid simulation.
5. Connect decoded values to proposed Tile/Liquid/rendering adapters only after integration review identifies single runtime owners. Keep the legacy codec facade during migration.

Single write owner: proposed extension codec for extension bytes; Tile/Liquid/rendering systems for decoded runtime state. Roll back if extension bits gain invented semantics, become network authority, or alter core codec offsets.

## Checkpoint C08 execution plan: world recovery transaction adapter

All C08 targets remain `status: proposed`; this session has not created them.

| Source member | Proposed target or role | Planned effect boundary |
|---|---|---|
| `582 IOLock` | proposed `WorldPersistenceLockPort` | one lock owner around a save transaction; release on success, exception and cancellation; no ECS or serialized state |
| `592 _versionNumber`, `609 VersionNumberForChestRework` | proposed `WorldFileVersionContext` and compatibility policy | decode/validate version gates only; preserve legacy branches and chest threshold; never expose as simulation or network revision |
| `593 _isWorldOnCloud` | proposed `WorldStorageRoute` | explicit local/cloud route selected at operation entry; unavailable cloud is a classified failure, never silent local fallback |
| `608 LastThrownLoadException` | proposed `WorldRecoveryFailureProjection` | translate the latest failed attempt into stable diagnostic data; retain exception handling in the adapter boundary |

### C08 implementation sequence and rollback

1. Define proposed lock, storage-route, version-context and classified-result values. Keep file handles, cloud clients, monitor objects and exceptions out of proposed Components.
2. Preserve the C02 save sequence: capture, encode, commit, read back, validate, then rotate backups and publish a commit projection. Keep the old primary bytes available so a validation failure can restore them after the failed write.
3. Implement a bounded load/recovery command seam: two normal `LoadWorld` attempts, `.bak` existence check, primary replacement from backup, and bounded retry of the restored file. Missing backup, failed copy/delete and final load failure remain explicit outcomes.
4. Gate the world-loaded projection behind successful load/recovery, C09 temporary-state restoration and the integration-owned publication decision. Do not let C08 call `WorldLoaded` directly.
5. Add provider contract tests for cloud-unavailable, write false, throw, timeout/result-unknown, read-back mismatch and retry classification before any provider-specific optimization. The Version4 boolean result does not prove cloud atomicity.

Single write/recovery owner: proposed `WorldRecoveryTransactionAdapter` for lock acquisition, route selection, classified file effects and retry state; C05 owns only read-only validity projection, and C09 owns only temporary event context. Roll back to the compatibility facade if lock release, old-byte restoration, backup ordering, normal retry count or `WorldLoaded` gating changes.

## Checkpoint C09 execution plan: world temporary event context

All C09 targets remain `status: proposed`; this session has not created them.

| Source member group | Proposed target or role | Planned effect boundary |
|---|---|---|
| `583-591` | proposed `WorldClockAndWeatherRestoreValue` plus capture/restore ports | copy values from live world services before encode and apply after successful recovery; no file or event publication side effects in the value |
| `594-601` | proposed party/sandstorm portion of `WorldTemporaryEventContext` | defensive copy of celebrating-NPC IDs and scalar storm values; one restore effect replaces the target list instead of appending |
| `602-607` | proposed lantern/precipitation portion of `WorldTemporaryEventContext` | restore as one validated payload; later event transitions remain Calendar/Event system effects |
| `resetTime` behavior | proposed `WorldResetTimePolicy` command | preserve day-time defaults, graveyard-bloodmoon exception and tenth-anniversary party exception; do not hide reset inside a Query |

### C09 implementation sequence and rollback

1. Define proposed immutable context values with explicit scalar fields and an immutable/read-only copy of celebrating-NPC IDs. Do not place service references, mutable lists or event callbacks in a Component.
2. Implement a proposed capture port matching `SetTempToOngoing`, including the exact live fields and defensive list copy. Make capture timing explicit relative to C02 snapshot/encode.
3. Implement proposed versioned codec mapping that preserves the existing header placement and historical defaults for missing fields. Reject truncated or contradictory payloads before touching live state.
4. Implement a proposed restore system matching `SetOngoingToTemps`, with one controlled list replacement and an explicit result. Run it only after C08 reports the final load/recovery success.
5. Implement the proposed reset-time policy separately and preserve `useTemps` versus `resetTime` semantics. Gate `WorldLoaded` after restore; the final Calendar/Event publication order remains `crossSubsystemOwner: integration-review`.

Single write owner: proposed `WorldTemporaryEventRestoreSystem` for live event-state restoration; the codec writes only protocol bytes, and Calendar/Event systems own later simulation transitions. Roll back if context capture aliases a mutable list, restoration occurs before final recovery, old-version defaults change, reset exceptions drift or `WorldLoaded` can publish before restoration.

## Checkpoint C10 execution plan: save configuration and metadata adapters

All C10 targets remain `status: proposed`; this session has not created them.

| Source member | Proposed target or role | Planned effect boundary |
|---|---|---|
| `2746 GameConfiguration._root` | proposed `GameConfigurationRootAdapter` | typed lookup at the generation-pass boundary; `JObject` stays behind the adapter and no query writes or performs file I/O |
| `2747 Preferences._data` | proposed private preferences document store | synchronized in-memory document state; callers receive typed/default values or immutable key snapshots, never the mutable dictionary |
| `2748 Preferences._path` | proposed `PreferencesDocumentRoute` | fixed document path consumed only by the file adapter; preserve path semantics and keep it distinct from world/player/external IDs |
| `2749 Preferences._serializerSettings`, `2750 UseBson` | proposed `PreferencesSerializationPolicy` | preserve parse-all-types settings and JSON/BSON branch; JSON text processing is only for JSON text |
| `2751 Preferences._lock`, `2752 AutoSave` | proposed synchronization boundary and autosave policy | serialize mutations and saves; an autosave failure returns a classified result and does not become hidden success |

### C10 implementation sequence and compatibility

1. Define proposed configuration-route, serializer-policy, typed-read-result and preferences-save-result values. Keep `JObject`, `Dictionary<string, object>`, serializer instances and lock objects inside adapter boundaries.
2. Preserve `GameConfiguration.Get<T>` as a compatibility facade while routing generation-pass reads through an explicit typed input seam. Decide and test missing-entry behavior before changing the direct null-failure semantics.
3. Restore or reimplement `Preferences` behavior only after comparing the Version4 source with the complete-reference supplement. Preserve local path handling, JSON/BSON selection, parse-all-types settings, missing-file result, malformed-document result, callback order and typed/default conversion where compatibility evidence requires it.
4. Keep `OnSave` before serialization, `OnLoad` after successful replacement and JSON `OnProcessText` after serialization/before write only if the Version4 compatibility decision confirms the supplemental behavior. Do not run text callbacks on BSON bytes.
5. Make `Put`/mutation plus `AutoSave` one explicit command/effect boundary under the proposed lock. Return a classified save result; do not let a Query trigger `Save` or hide a write failure.
6. Add focused adapter tests before any caller migration: JSON and BSON round trips, parse-all-types conversion, missing/malformed files, callback ordering, concurrent mutation/save serialization, autosave failure, path creation and read-only-file handling.

Single write owner: proposed `PreferencesStoreAdapter` for preference document bytes and mutation-triggered saves; proposed `GameConfigurationRootAdapter` is read-only. Roll back to the legacy facade if callback order, format bytes, default conversion, lock behavior, AutoSave semantics or failure visibility changes. No C10 adapter or configuration file was created in this session.

## Checkpoint C11 execution plan: file platform boundary

C11 targets are implemented and focused-verified; native file-browser integration and provider
timeout semantics remain explicitly unverified.

| Source member | Proposed target or role | Planned effect boundary |
|---|---|---|
| `2949 ExtensionFilter.Name`, `2950 ExtensionFilter.Extensions` | proposed `ExtensionFilterValue` | copy the extension sequence at request construction; pass only immutable filter data to the proposed file-browser adapter; no native dialog or UI state in the value |
| `2960 FileUtilities.FileNameRegex` | private implementation detail of proposed `FilePathParsingAdapter` | preserve Version4 filename and parent-prefix projections as pure functions; no file or cloud effects |
| `2964 NewRuntimeMethods.IsNet45OrNewer` | proposed `RuntimeCapabilityQuery` result source | isolate the reflection probe behind a platform seam; do not expose the static probe as component state or infer a direct Version4 consumer |

The proposed `FilePlatformAdapter` is the single effect owner for local/cloud exists, reads, writes,
copies, moves, deletes, read-only attribute handling and file-browser dispatch. C08 consumes it for
world recovery effects and C10 consumes it for configuration/favorites effects through explicit ports;
neither caller reimplements platform routing. `GetFullPath` must remain local-only, and cloud paths
must be passed through unchanged to the cloud provider. Provider unavailable, timeout/result-unknown,
permission, malformed-path and read-only failures must return classified results.

### C11 implementation sequence and rollback

1. Define proposed `ExtensionFilterValue`, `FilePathProjection`, `FilePlatformFailure` and
   `RuntimeCapabilityResult` values. Validate the extension-copy contract and keep path, persistent
   file, cloud, external-platform and entity/network IDs separate.
2. Implement pure path projections against table-driven vectors covering both separators,
   extensionless names, dotted names, empty/root-like inputs and the Version4 checked parent-path
   behavior. Do not call `Path.GetFullPath` from the cloud route.
3. Implement the proposed local/cloud port with explicit route selection and classified results for
   unavailable cloud, provider exceptions, timeouts/result-unknown, permission errors, read-only
   handling and missing files. Preserve Version4 write parent-directory creation and local
   read-only clearing only after the compatibility decision is recorded.
4. Implement the proposed file-browser adapter at the external UI/platform seam. Flatten copied
   extension values only at the native-dialog adapter boundary; do not give the filter value UI or
   platform ownership. Confirm whether this service belongs to P14, P15 or a shared runtime layer
   before caller migration.
5. Implement the runtime capability seam only after finding or explicitly documenting the missing
   Version4 consumer. A capability query may report support/fallback; it must not invoke GC or hide
   an unsupported operation. Add tests for both reflection-present and reflection-absent cases.
6. Migrate `WorldFileData`, `FileData`, C08, C10 and file-browser callers through compatibility
   facades, then remove duplicate routing only after focused tests and integration review approve
   error, retry, disposal and path-security semantics.

Single write/effect owner: proposed `FilePlatformAdapter`; proposed `FilePathParsingAdapter` and
`RuntimeCapabilityQuery` are read-only. Roll back if cloud paths are normalized locally, any failed
write is reported as success, filter arrays alias caller-owned storage, path traversal/security
checks change unexpectedly, or the direct consumer/compatibility policy for `IsNet45OrNewer` is
invented without evidence. No C11 adapter, value, query, platform file or test was created.

## 5. Remaining implementation order

| Order | Component | Required evidence before implementation | Planned verifier |
|---:|---|---|---|
| 3 | `PlayerSaveSessionBoundary` | complete player serializer or matching full-reference evidence; injected clock semantics | timer/save seam and failure propagation |
| 4 | `WorldIdentitySnapshot` | owner relation to `WorldDescriptorState`, `PersistentWorldId`, and header schema | identity round-trip and version defaults |
| 5 | `WorldValidityAndRulesHandoff` | rule owner and event/world progression boundaries | missing/invalid/future version matrix |
| 6 | `WorldTileHeaderCoreCodec` | exact header flags and invalid byte handling | flag/overflow/compression vectors |
| 7 | `WorldTileHeaderExtensionCodec` | exact extension semantics and Tile owner | extension flag vectors and compatibility |
| 8 | `WorldRecoveryTransactionAdapter` | cloud result semantics, lock/timeout/retry decision | backup recovery and no false success |
| 9 | `WorldTemporaryEventContext` | Calendar owner and publication ordering | capture/restore and idempotence |
| 10 | `SaveConfigurationAndMetadataAdapters` | complete Preferences load/save behavior and event ordering | JSON/BSON/config failure tests |
| 11 | `FilePlatformBoundary` | platform/cloud contract, path security, defensive-copy and disposal/exception policy | local/cloud/path/filter/capability tests |

After each implementation order, update both documents before starting the next order. The current
checkpoint is C02; C01 and C11 are the only completed components in this execution.

## 6. Side effects, retry, snapshot and persistence strategy

* File, cloud, platform, clock, random seed, process capability, logging and UI effects belong to adapters or projections. Proposed Components contain values/snapshots only.
* Save commit is a reliability boundary. The plan must classify explicit failure, timeout with unknown result, cancellation, retry limit, duplicate write and backup compensation. No unbounded retry is proposed.
* `WorldTemporaryEventContext` is staging state. It is captured before serialization and restored only after successful load, before the world-loaded event; it is never serialized as a standalone component.
* Preferences configuration is external document state. JSON/BSON bytes, serializer callbacks, path effects, locks and AutoSave remain adapter effects; no proposed Component contains them.
* Player playtime uses an injected clock in proposed code. `Stopwatch` and thread scheduling remain adapter/system concerns; timer transitions must be idempotent.
* Network snapshots and persistence snapshots are separate projections. Entity ID, persistent ID, network ID, file path and external cloud ID cannot share one field or owner without `crossSubsystemOwner: integration-review`.

## 7. Verification and build plan for a later implementation

This session will not run C# migration, compilation, tests, or behavior-equivalence verification. A future implementation session must:

* run focused pure codec/metadata tests first;
* run local/cloud adapter failure and recovery tests;
* run save/load ordering, backup, future-version, malformed-input and player timer tests;
* run static checks for domain-first paths, one public type per file, no hidden Query writes, and no external types in Components;
* inspect active `dotnet.exe`/`csc.exe` processes before any compile-capable command;
* run affected projects only through `Build/Tools/Invoke-SerialDotnet.ps1` with repository-required serial properties, then run verifiers with `--no-build --no-restore` and confirm artifacts under `Build/bin/`.

The historical commands in this plan are not evidence for the current session. Current-session
evidence is recorded at the top of this document: the Component-only change was built through the
repository serial wrapper with exit code `0`, `0` warnings and `0` errors; no test command was run.

## 8. Risks and rollback

Rollback before implementation: discard only the new proposed target files and restore the adapter facade; do not delete or rewrite old save files. During implementation, rollback if a read-back validation changes, a backup can be lost, a cloud result is unknown, old metadata cannot be read, a network/persistence ID is conflated, or `WorldLoaded`/save publication order changes. Keep the legacy adapter as the compatibility path until focused verifiers pass and integration review approves ownership.

Risks include missing player serialization, incomplete Version4 Preferences implementation, cloud provider semantics, Tile header version drift, shared Main field ownership, event restoration races, background save overlap, and behavior changes from converting static composition fields into ECS state.

## 9. Integration handoff

subsystemId: PersistenceAndRecovery / RuntimeComposition / SharedRuntimeMechanisms (P14 non-authoritative)
taskNumber: P14
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-component-execution.md

evidenceStatus: source inventory confirmed; the current session changed and built only `PlayerSaveSessionComponent`; C03 serializer/clock/binding effects and C04-C10 non-Component roles remain deferred or unverified; first-round P14 report missing
nltxStatus: partial; `PlayerSaveSessionComponent` is build-verified, while the remaining proposed P14 boundaries are deferred under the Component-only scope
verificationStatus: component-build-passed-no-tests (C03 non-Component roles and C04-C10 deferred; no tests run)

confirmedOwners:
- proposed plan preserves Version4 world save lock, read-back validation, backup and temporary restore order

proposedTypes:
- proposed snapshot, codec, command, system, adapter and projection targets listed in Section 1
- proposed GameConfigurationRootAdapter / PreferencesStoreAdapter / PreferencesSerializationPolicy
- proposed ExtensionFilterValue / FilePathParsingAdapter / FilePlatformAdapter / RuntimeCapabilityQuery

sharedTypesForIntegrationReview:
- persistent/world/network/entity IDs, snapshot schema, Tile/WorldSection values, event publication tokens

crossSubsystemReaders:
- WorldSession, WorldStorage, Player, Calendar, WorldGeneration, Network, UI and Diagnostics candidates

crossSubsystemWriters:
- save/load commands, WorldGeneration, Player save, shutdown/autosave, cloud/file platform and recovery paths

orderingConstraints:
- capture -> encode -> commit -> read-back -> validate -> backup -> publish
- restore temporary context -> WorldLoaded

boundaryChallenges:
- retain compatibility facades while extracting adapters; do not turn staging or codec state into ECS authority

evidenceGaps:
- missing first-round P14 report; incomplete player evidence; missing Version4 Preferences I/O with complete-reference supplement only; unresolved cloud, platform, path-security, defensive-copy and shared-owner decisions; no direct Version4 consumer found for IsNet45OrNewer

blockingDecisions:
- final owner of persistence/world snapshots, rules/progression, Tile/Liquid/rendering codec, recovery events, shared IDs/schema, cloud retry and cross-system ordering

notImplemented:
- C04-C10 remain deferred; C03 non-Component roles remain deferred.
- Native file-browser integration, provider timeout/result-unknown semantics and cross-partition owner decisions remain unverified.

verifierPlan:
- the current Component checkpoint has a repository-serial build result; focused tests and the
  broader verifier plan in Section 6 remain future work and were not added by this session

本文件当前 checkpoint 明确：执行状态为 `completed`，实现状态为 `completed`，验证状态为
`build-passed-no-tests`；本 session 只修改并构建了 `src2/Player/PlayerSaveSessionComponent.cs`，
C04-C10 及 C03 的非 Component 角色均为 deferred，未运行测试，也未宣称行为等价。
