# Version4 非权威 P14 proposed component design：持久化、恢复与配置

> 本文件是第二轮非权威 proposed 设计，不是实现、迁移完成报告、行为等价证明或最终跨分区 owner 裁决。

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

## 1. Scope and exclusions

本设计覆盖输入分区中的 11 个叶子子系统和 160 条成员记录：`MainSaveFavoritesAndSessionRefs`、`MainWorldPersistenceAndMetadata`、`PlayerFileMetadataAndSession`、`WorldFileMetadataIdentityState`、`WorldFileSessionAndValidityState`、`WorldFileTileHeaderCoreState`、`WorldFileTileHeaderExtensionState`、`WorldFileRecoveryVersionState`、`WorldFileTemporaryEventState`、`SharedSaveAndConfigurationAdapters`、`SharedGeneralFilePlatformUtilities`。

本分区负责提出存档快照、世界文件协议、恢复事务、配置/收藏适配和文件平台边界。它不负责裁决 Tile、Liquid、Player gameplay、NPC、World generation、Calendar/Event、UI、network protocol 或 external platform 的最终状态 owner；这些交接只写候选并标记 `crossSubsystemOwner: integration-review`。

不把 `Main` 中的所有字段合并为 `PersistenceComponent`。`Pings`、`_achievements`、`MenuUI`、`InGameUI`、`waterfallManager`、`sectionManager`、`liquid`、`liquidBuffer`、`teamColor`、`showItemText`、`statusText`、`dedServ`、`libPath` 和 `lo` 保留为跨边界 composition/deferred 输入，不能由本报告宣称为 P14 持久化权威状态。

## 2. Evidence read and status

### Version4 facts

| Evidence | Confirmed fact | Evidence status |
|---|---|---|
| `D:\TRbackup\Version4\Terraria.IO\FileMetadata.cs:6-91` | `FileMetadata` 编码包含 magic/type、revision 和 favorite；`SIZE=20`，读入会校验 magic、file type 和无效 type。 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\FavoritesFile.cs:10-101` | local/cloud favorites 以 type/file-name 双层字典保存为 JSON；读写经 `FileUtilities`，坏文件只记录到控制台而不重新抛出。 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\FileData.cs:3-59` | path/cloud/type/favorite 是文件描述信息；`SetFavorite` 会更新内存状态并可触发 favorites 持久化。 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:183-330` | metadata scan 读取版本、FileMetadata、名字、seed、GUID、WorldId、尺寸、模式和事件 flags；版本不支持或解析异常转为 invalid metadata。 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:658-808` | world load 读取 cloud/file bytes，按版本路由 header/tiles/sections，失败写 `LastThrownLoadException` 并设置 `WorldGen.loadFailed`。 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:878-1031` | save 通过 `IOLock` 串行化，写 memory buffer，写回后再读并 `ValidateWorld`，成功时维护 `.bak` rolling backups。 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:152-180`、`:1035-1099` | temp event state 在 save/load 前后 capture/restore；它是 transient restore context，不应成为长期持久化 component。 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1469-1645`、`:2570-2778` | Tile header bytes 控制 tile/wall/liquid/wire/slope/visibility/fullbright 和 vertical run compression；这些是 codec protocol，不是 Tile component fields。 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:26394-26418` | `SavePlayer` 的外层边界和错误处理存在，但 `InternalSavePlayerFile`、`Serialize`、`Deserialize` 在 Version4 中为空。 | partial / evidence-gap |
| `D:\TRbackup\Version4\Terraria\WorldGen.cs:6336-6377` | world load 失败后重试、切 `.bak`、再次加载，成功后 `SetOngoingToTemps` 再 `Hooks.WorldLoaded`。 | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs:1771-1852`、`:1895-1954` | world metadata enumeration、path sanitization、cloud/local path distinction 和 dedicated config parsing 位于 Main composition。 | confirmed |

### Current NLTX status

当前读取到的 NLTX 有 `src/WorldSession/WorldDescriptorState.cs`、`WorldDescriptorSnapshotValue.cs`、`src/WorldStorage/WorldStorageRoot.cs`、`TileMapStore.cs`、`src/Player/PlayerIdentityState.cs` 和 `PlayerLifecycleState.cs`。它们提供部分 world/player runtime model，但没有发现 P14 的 file transaction、FileMetadata codec、FavoritesFile、Preferences、cloud adapter、WorldFile tile codec 或 recovery implementation。状态为 `partial` 或 `missing`，不能因目录和类型存在而升级为已实现。

### Public and structural references

* tModLoader mirror: `D:\TRbackup\tmodloader-api-docs-stable\index.html` shows `tModLoader v2026.07`; `class_mod_system.html:334-343,603-635,1078-1148` defines world data/header save-load hooks and defensive loading; `class_mod_player.html:2366-2398,4499-4530` defines player custom data load/save; `class_world_file_data.html:8-30,376-404` exposes header lookup. These are public boundary cross-checks only.
* SS14 reference: `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Administration\Commands\PersistenceSaveCommand.cs` keeps save command, map lookup, configuration and map-loader effect boundaries explicit; `Content.IntegrationTests\Tests\Serialization\SerializationTest.cs` tests round-trip serialization; `Content.Tests\Shared\Gamestates\ComponentStateNullTest.cs` tests component construction. It supplies structure patterns only, not Terraria semantics.
* The first-round P14 `outputReport` path from the P14 prompt was checked and is absent. No other partition's in-progress report was read.

## 3. Checkpoint C01: save reference and favorites adapter

### Proposed boundary

`P14.C01 SaveReferenceAndFavoritesAdapter` is a proposed infrastructure boundary, not an ECS authority. It has two explicit parts:

* `status: proposed` `SaveFileReferenceSnapshot` carries path, `FileType`, cloud/local mode, display name and favorite bit as a read-only snapshot. It does not own Player, World, Tile, Network or external account identity.
* `status: proposed` `FavoritesIndexAdapter` owns local/cloud JSON lookup and the `FileData` favorite command. Its input is a `SaveFileReferenceSnapshot`; its effects are file/cloud reads and writes. `QueryFavorite` is pure only after a snapshot is supplied; it must not call the filesystem.

The existing `FavoritesFile._data` is an adapter cache, not an authoritative gameplay component. `FileMetadata` is a protocol value/codec, not an ECS component. `Main.LocalFavoriteData` and `Main.CloudFavoritesData` are composition roots and should become injected adapter instances only after integration review.

### C01 member mapping

| Inventory members | Proposed role | Owner / lifecycle | Evidence |
|---|---|---|---|
| `38 LocalFavoriteData`, `39 CloudFavoritesData` | proposed `FavoritesIndexAdapter` composition references | initialized with Main save paths; loaded during Main initialization; disposed/flush policy missing | `Main.cs:180-182`, `3332-3333`; confirmed |
| `40 WorldFileMetadata` | proposed `FileMetadata` protocol value at world save/load boundary | written by world codec and read by metadata scan; shared owner unresolved | `Main.cs:184`; `WorldFile.cs:1215`, `1966-1977`; confirmed; `crossSubsystemOwner: integration-review` |
| `41 Pings`, `42 _achievements`, `43 MenuUI`, `44 InGameUI`, `45 waterfallManager`, `46 sectionManager` | deferred composition references; not owned by P14 adapter | runtime-only or presentation/service lifecycle; no P14 persistence claim | `Main.cs:186-196`; partial; `crossSubsystemOwner: integration-review` |
| `2731 Path`, `2732 IsCloudSave`, `2733 _data`, `2734 _ourEncoder` | proposed `FavoritesIndexAdapter` state | adapter construction/load/save; dictionary invalidation after load is defined, shutdown flush is not | `FavoritesFile.cs:10-101`; confirmed |
| `2735 _path`, `2736 _isCloudSave`, `2737 Metadata`, `2738 Name`, `2739 Type`, `2740 _isFavorite`, `3890 Path`, `3891 IsCloudSave`, `3892 IsFavorite` | proposed `SaveFileReferenceSnapshot` source fields/projections | file descriptor lifetime; favorite writes command into adapter | `FileData.cs:3-59`; confirmed |
| `2741 MAGIC_NUMBER`, `2742 SIZE`, `2743 Type`, `2744 Revision`, `2745 IsFavorite` | proposed `FileMetadata` codec/value fields | binary header lifetime; revision increments at commit; no component registration | `FileMetadata.cs:6-91`; confirmed |

### C01 invariants and effects

* A favorite key is `(file type, normalized file name)`; it is not a persistent entity ID.
* local and cloud stores are distinct adapters; a cloud path must not be silently treated as a local path.
* `SaveFavorite` may write immediately in Version4, but proposed code should expose a command/commit result so duplicate writes, cloud failure and shutdown flush are observable.
* `FileMetadata` read rejects wrong magic/type and invalid file type; callers must distinguish malformed input from a missing file.

## 4. Checkpoint C02: world save composition boundary

### Proposed boundary

`P14.C02 WorldSaveCompositionBoundary` is a proposed orchestration boundary around the existing world save transaction. It does not own world gameplay fields. It exposes proposed `SaveWorldCommand`, `WorldSaveValidationQuery`, `WorldBackupPolicy` and `WorldSaveProjection` with all file/cloud/clock/log effects behind adapters.

The proposed sequence is: capture required world snapshot and temporary event context -> encode format header, world header and tile stream -> commit bytes through a file adapter under an explicit save lock -> re-read committed bytes -> validate -> rotate backups only after validation -> publish `WorldSaveCommitted` or `WorldSaveRejected`. A validation failure must leave the previous `.bak` recoverable; it must not be represented as a successful component mutation.

`WorldRollingBackupsCountToKeep`, `validateSaves`, `worldName`, `worldSurface`, `rockLayer`, `dungeonX`, `dungeonY` and the liquid arrays are not one component. The proposed boundary reads them through domain-specific queries/ports; final owners are integration-review where their readers span P14 and other partitions.

### C02 member mapping

| Inventory members | Proposed role | Owner / lifecycle | Evidence |
|---|---|---|---|
| `198 WorldRollingBackupsCountToKeep` | proposed `WorldBackupPolicy` input | server/config lifetime; clamp and invalid config behavior must be explicit | `Main.cs:563`, `WorldFile.cs:993-1031`; confirmed |
| `199 dungeonX`, `200 dungeonY` | proposed world-header read/write projection input | world session/structure owner unresolved; not P14 component authority | `Main.cs:565-567`, `WorldFile.cs:1317-1318`; confirmed; `crossSubsystemOwner: integration-review` |
| `201 liquid`, `202 liquidBuffer` | deferred Tile/Liquid storage state read by world serializer | world load/save lifetime; ownership belongs to liquid/tile storage integration | `Main.cs:569-571`, `WorldFile.cs:1469-1645`, `2570-2778`; confirmed; `crossSubsystemOwner: integration-review` |
| `203 dedServ`, `204 showItemText`, `205 validateSaves`, `206 libPath`, `207 lo`, `208 statusText` | proposed composition/runtime policy inputs or projections; not persistent component fields | process/session/UI/diagnostic lifetimes differ; no joint owner | `Main.cs:573-583`, `WorldFile.cs:907-980`; partial; `crossSubsystemOwner: integration-review` |
| `209 worldName`, `210 worldSurface`, `211 rockLayer` | world descriptor/header snapshot inputs; `worldName` candidate in `WorldDescriptorState`, surface/rock candidate in world terrain owner | loaded/generated world session; cross-partition schema unresolved | `Main.cs:585-589`, `WorldFile.cs:1263-1311`; confirmed; `crossSubsystemOwner: integration-review` |
| `212 teamColor` | deferred presentation/runtime table | process/UI lifetime; not persisted by this boundary | `Main.cs:591`; partial; `crossSubsystemOwner: integration-review` |

## 5. Proposed component/system register (remaining checkpoints)

| ID | Proposed role | Main input members | State kind | Status |
|---|---|---|---|---|
| `P14.C03 PlayerSaveSessionBoundary` | player save metadata snapshot + play-time system + player save adapter seam; consumes the `Main.ServerSideCharacter` policy input without owning it | `543-546`, `638-640`; policy handoff `47` | snapshot + transient session state | proposed |
| `P14.C04 WorldIdentitySnapshot` | world identity/size/seed/date metadata snapshot | `610-618`, `621-624` | persisted snapshot/value object | proposed |
| `P14.C05 WorldValidityAndRulesHandoff` | load status/error projection plus seed/mode/event rule handoff; gameplay owner not claimed | `619-620`, `625-637`, `641-648`; consumes C04 `624` as input | validity projection + cross-domain handoff | proposed; integration-review |
| `P14.C06 WorldTileHeaderCoreCodec` | core tile/wall/liquid/wire bit schema and codec seam | `547-565` | adapter protocol constants | proposed |
| `P14.C07 WorldTileHeaderExtensionCodec` | extension visibility/fullbright/wire4 schema and codec seam | `566-581` | adapter protocol constants | proposed |
| `P14.C08 WorldRecoveryTransactionAdapter` | lock/version/cloud/error/recovery transaction state | `582`, `592-593`, `608-609` | adapter state | proposed |
| `P14.C09 WorldTemporaryEventContext` | capture/restore transient weather, day/event payload | `583-591`, `594-607` | transient restore context | proposed |
| `P14.C10 SaveConfigurationAndMetadataAdapters` | configuration root/preferences and JSON/BSON policy; file metadata/favorite fields remain in C01 | `2746-2752` | adapter/value/projection | proposed |
| `P14.C11 FilePlatformBoundary` | file path, cloud/platform operations, extension filter and runtime capability | `2949-2950`, `2960`, `2964` | adapter/query/projection | proposed |

All remaining IDs, types, paths and interfaces in this section are proposed. The implemented
`PlayerSaveSessionComponent` checkpoint is recorded above; C01/C02/C11 are prior checkpoints and
are not current-session changes.

## 6. IDs and boundary distinctions

* Runtime entity ID, `WorldId`, `WorldFileData.UniqueId`, persistent file path, cloud path, network ID, player slot and external platform/cloud identifier are different values.
* `WorldId` is a Version4 world metadata integer; `UniqueId` is a GUID used by the map filename rule after `WorldGeneratorVersion` reaches the GUID threshold; neither is declared here as the shared NLTX `PersistentWorldId` owner.
* `FileMetadata.Revision` is a file protocol revision, not a world simulation revision or network snapshot revision.
* `TileCoordinate`, `WorldSectionId`, `EntityId`, `NetworkId`, `PersistentEntityId`, `PlayerEntityId` and `ExternalFileId` remain candidate shared types with `crossSubsystemOwner: integration-review`.

## 7. Proposed dependency and scheduling contract

```text
World/Player domain state
  -> proposed snapshot queries
  -> proposed format/header and temporary-context adapters
  -> proposed save/recovery command system
  -> proposed file/cloud adapter
  -> validation query
  -> backup rotation and commit projection
  -> integration events / client and UI projections
```

The minimum order is explicit: `CaptureSnapshot` before `Encode`; `Encode` before `Commit`; `Commit` before `ReadBack`; `ReadBack` before `Validate`; `ValidateSuccess` before `RotateBackup`; `RestoreTemporaryContext` before `WorldLoaded` publication. Player save and world save may be scheduled independently only after their shared player/world and platform effects are proven non-conflicting. No order is inferred from file names.

## 8. Checkpoint C03: player save session boundary

### Proposed boundary

`P14.C03 PlayerSaveSessionBoundary` is a proposed boundary between the player runtime binding, accumulated play time, and the encrypted player-file adapter. It is not a proposed replacement for the `Player` gameplay entity and it does not make the player serializer authoritative until the missing Version4 implementation is restored and verified.

The current session implemented only the `PlayerSaveSessionComponent` state under `src2`; the snapshot, clock, serializer, command, adapter and system roles in this boundary remain deferred.

The proposed split is deliberately narrow:

* `proposed PlayerSaveSessionBinding` exposes the active-player relation and save-file metadata handoff. The `_player` field and `Player` property are relation/binding state, not a second copy of the player component graph. Setting the binding may update the proposed display-name projection, matching the Version4 setter behavior, but it must not mutate gameplay state through a query.
* `proposed PlayerPlayTimeState` stores accumulated play time and the active/inactive session flag. `_playTime` is a value snapshot; `_isTimerActive` is transient session state. The `Stopwatch` in `_timer` is not a Component field: a proposed monotonic-clock/effect adapter supplies elapsed time to a proposed `PlayerPlayTimeSystem`.
* `proposed PlayerSavePolicyQuery` evaluates the server-side-character policy from the explicit composition input. `PlayerFileData.ServerSideCharacter` is a file-session policy result, while `Main.ServerSideCharacter` is a process/composition input. Neither is a persistent player gameplay field in this boundary.
* `proposed PlayerSaveAdapter` owns player-file versioning, metadata, encryption, local/cloud byte effects, backup handling and serializer compatibility. `proposed LastPlayedProjection` derives a view from the saved player timestamp; it is not an independently writable Component.

### Evidence and implementation status

Version4 directly confirms `PlayerFileData._player`, `_playTime`, `_timer`, `_isTimerActive`, `Player`, `ServerSideCharacter` and `LastPlayed` in `D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs:10-38`. Its timer transitions are present at `:52-100`: focus selects start or pause, start is idempotent by `_isTimerActive`, and stop accumulates `_timer.Elapsed` before reset. `Main.Update` calls `ActivePlayerFileData.UpdatePlayTimer()` at `D:\TRbackup\Version4\Terraria\Main.cs:11254-11257`.

The Version4 save call boundary exists at `D:\TRbackup\Version4\Terraria\Player.cs:26394-26418`: achievements and map save are attempted, `Main.ServerSideCharacter` gates `InternalSavePlayerFile`, and errors are reported and rethrown. However, `InternalSavePlayerFile`, `Serialize` and `Deserialize` are empty in this Version4 baseline. `PlayerFileData.SetAsActive`, `MoveToCloud` and `MoveToLocal` are also empty at `D:\TRbackup\Version4\Terraria.IO\PlayerFileData.cs:49-51`, and the baseline has no complete `GetPlayTime`, `SetPlayTime`, `UpdatePlayTimerAndKeepState` or `MarkAsServerSide` implementation.

The complete-reference files provide `full-reference-supplemented` evidence for the missing contract, not proof that Version4 currently implements it. `D:\TRbackup\无任何删减通过编译\Terraria.IO\PlayerFileData.cs:50-241` shows create/save, active binding, server-side marking, local/cloud migration, play-time read/refresh/set operations and rename. `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:55290-55346` shows the version `319` file envelope, metadata write, local backup, encrypted stream and cloud write; `:55348+` and `:55750+` show the serializer/deserializer fields and release-gated loading, including play-time ticks and later-version/unknown-error branches. Exact Version4 compatibility remains an `evidence-gap` until those differences are reconciled.

### C03 member mapping

| Inventory member | Proposed role | Owner, readers, writers and lifecycle | Evidence status |
|---|---|---|---|
| `543 _player` | proposed `PlayerSaveSessionBinding` relation to the active player; not a duplicated ECS payload | Set by the player load/create adapter; read by save command and `LastPlayedProjection`; cleared with the active player session; final player-entity ID owner is `crossSubsystemOwner: integration-review` | Version4 declaration confirmed; binding lifecycle partial |
| `544 _playTime` | proposed `PlayerPlayTimeState.Accumulated` value | Written only by the proposed play-time system or explicit load/set command; read by serializer and play-time query; session-scoped and serialized only after serializer evidence is closed | Version4 timer behavior confirmed; complete load/save contract full-reference-supplemented |
| `545 _timer` | proposed monotonic clock/effect adapter state, never a Component field | Created with the file session; read by timer system; stopped/reset on pause/stop; clock ownership and test clock are proposed ports | Version4 declaration and elapsed-time use confirmed; effect isolation proposed |
| `546 _isTimerActive` | proposed transient `PlayerPlayTimeState.IsActive` flag | Written by start/stop transitions; read by `UpdatePlayTimer`; not persisted as gameplay state; must be idempotent under repeated focus/update calls | Version4 declaration and transition behavior confirmed |
| `638 Player` | proposed binding property/adapter seam over `543`; setter may update the proposed name projection | Player load/create and active-session composition write it; save and projection queries read it; no query may replace the gameplay entity or silently perform I/O | Version4 getter/setter confirmed; owner remains integration-review |
| `639 ServerSideCharacter` | proposed player-file save-policy result | Set only by a proposed explicit policy command such as `MarkAsServerSide`; read by player-save adapter to skip file bytes; must not be inferred from cloud/local mode | Version4 property declaration confirmed; setter behavior full-reference-supplemented |
| `640 LastPlayed` | proposed derived `LastPlayedProjection` from the player saved-time field | Read-only projection for UI/session listing; never independently persisted by P14; null/missing player behavior must be defined before implementation | Version4 expression confirmed; null/error behavior partial |
| `47 Main.ServerSideCharacter` | proposed composition policy input consumed by `PlayerSavePolicyQuery`; no second P14 owner | Main/server setup writes it; `Player.SavePlayer` reads it before invoking the file adapter; P14 receives a snapshot/query result and must not write the Main global | Version4 declaration and save gate confirmed; cross-owner `integration-review` |

### C03 invariants and effects

1. A player entity reference, player save path, player slot, persistent player ID, network ID and cloud/external ID remain distinct values. Their shared type and owner are `crossSubsystemOwner: integration-review`.
2. `GetPlayTime` must include the currently running interval without resetting it; `UpdatePlayTimerAndKeepState` must accumulate and restart an active clock atomically; repeated start, pause and stop calls must not double-count. These are proposed behavioral contracts inferred from the complete reference and require focused verification.
3. A server-side character policy suppresses player-file byte publication but does not suppress unrelated map/achievement effects already visible at the `SavePlayer` outer boundary. The final split of those effects is `crossSubsystemOwner: integration-review`.
4. Player serialization must be a command/adapter effect. A `Query` may create a proposed immutable player snapshot but may not open a file, mutate `Player`, advance a clock, log, or call cloud APIs.
5. The save order is explicit: resolve policy -> capture player snapshot and play-time ticks -> encode version/metadata/player payload -> commit local/cloud bytes -> publish success or classified failure. A failed or result-unknown commit must not publish a successful player-save projection.

## Checkpoint C04: world identity snapshot

### Proposed boundary

`P14.C04 WorldIdentitySnapshot` is a proposed immutable file/header snapshot for world identity and metadata. It is not the authoritative world simulation component and it does not own terrain, tiles, liquids, progression flags, events or the runtime world entity. The proposed snapshot must be usable by metadata enumeration, world-header encoding, load validation and UI listing without exposing `BinaryReader`, `LocalizedText`, file handles or cloud clients to ECS state.

The proposed value contains the raw identity needed to round-trip the Version4 header: `WorldId`, `UniqueId`, `Name`, seed text, generator version, dimensions, game mode and creation/last-played timestamps. The numeric seed is a deterministic derived value from seed text and should be represented as a projection or validated cache, not a second independently editable identity. `WorldSizeName` is a localization projection from dimensions; the proposed persisted snapshot stores dimensions, not a `LocalizedText` object.

The GUID threshold and seed-text length are protocol constraints. `UseGuidAsMapName` is a pure compatibility query over `WorldGeneratorVersion`; it selects `UniqueId` only at or above `777389080577`, otherwise `WorldId`. This rule must not silently conflate a file map name, a persistent world ID or a runtime entity ID. All shared ID types and the final owner of `WorldDescriptorState` versus the persistence snapshot remain `crossSubsystemOwner: integration-review`.

### Evidence and current NLTX status

Version4 declares the C04 members in `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:15-45`. `WorldFile.GetAllMetadata` reads the file version, `FileMetadata`, name, seed text, generator version, GUID, `WorldId`, dimensions, game mode and timestamps with version/cloud fallbacks at `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:190-331`. `SaveWorldHeader` writes the corresponding name, seed, generator version, GUID, `WorldId`, dimensions, game mode, creation time and current UTC last-played value at `:1263-1289`. The `WorldFileData` projections `SeedText`, `Seed`, `WorldSizeName`, `HasValidSeed`, `UseGuidAsMapName` and `MapFileName` are declared at `:84-116`.

Current NLTX has `src/WorldSession/WorldDescriptorState.cs` and `WorldDescriptorSnapshotValue.cs`, with matching identity, seed text, generator version and dimensions, plus bounds and terrain placement fields. This is `partial`, not a P14 file snapshot implementation: no file version/metadata/time source, localization adapter, header codec or cloud/local read path was found. `src/WorldSession/WorldGeneration/WorldDescriptorState.cs` is a separate generation-side shell and cannot be selected as the final shared owner by this report.

### C04 member mapping

| Inventory member | Proposed role | Owner, readers, writers and lifecycle | Evidence status |
|---|---|---|---|
| `610 GUID_IN_WORLD_FILE_VERSION` | proposed `WorldIdentityProtocol` compatibility constant used by `UseGuidAsMapName` | read by map-name query and header compatibility logic; never runtime mutable; final protocol owner is integration-review | Version4 declaration and property use confirmed |
| `611 MAX_USER_SEED_TEXT_LENGTH` | proposed seed input validation policy | read by seed parser/creation command; not a world component field; invalid length is a rejected command | Version4 declaration/use confirmed; parser implementation partial |
| `612 CreationTime` | proposed persisted `WorldCreationTimestamp` snapshot value | written during world creation/header load fallback; read by metadata listing and header encoder; clock/file metadata effect comes through adapter | Version4 read/write paths confirmed |
| `613 LastPlayed` | proposed persisted `WorldLastPlayedTimestamp` value/projection | written by world header save or legacy file fallback; read by listing/UI; must not be replaced by player `LastPlayed` | Version4 read/write paths confirmed |
| `614 WorldSizeX`, `615 WorldSizeY` | proposed immutable `WorldDimensions` snapshot value | written by world generation/header load; read by tile storage, section sizing and header codec through query; final terrain/section owner is integration-review | Version4 declaration and header paths confirmed; NLTX descriptor partial |
| `616 WorldGeneratorVersion` | proposed world-format/generation compatibility value | written by generation/header load; read by GUID map-name query and seed/rules handoff; not a runtime code-version alias | Version4 declaration and header paths confirmed |
| `617 _seedText` | proposed persisted raw seed text | written by validated seed command/header load; read by seed/rules query and header encoder; preserve exact accepted text for round-trip | Version4 declaration, `SetSeed` and header paths confirmed |
| `618 _seed` | proposed derived seed projection/cache from `SeedText` | calculated by a pure seed translation query; never independently edited or serialized as an alternative source of truth | Version4 `TranslateSeed`/`SetSeed` behavior confirmed |
| `621 UniqueId` | proposed GUID identity value in the file snapshot | header read/write and map-name projection use it only under version rule; not the shared `PersistentWorldId` without integration review | Version4 declaration and header paths confirmed |
| `622 WorldId` | proposed legacy/world-file identity value | header read/write, footer validation and legacy map-name projection; distinct from runtime entity and persistent IDs | Version4 declaration and footer/header use confirmed |
| `623 _worldSizeName` | proposed localization projection from `WorldSizeX/Y` | created by size classification/localization adapter; never serialized as a localization object; missing localization falls back to an explicit display status | Version4 setter/projection confirmed; localization owner integration-review |
| `624 GameMode` | proposed mode value in the identity/header snapshot, with rules handoff | header read/write and mode projection read it; gameplay rule owner is not claimed by P14 | Version4 metadata/header paths confirmed; cross-owner integration-review |

### C04 invariants and scheduling

* `WorldId`, `UniqueId`, persistent world ID, runtime world entity ID, map filename and external cloud file ID are separate fields. A conversion table is required before implementation; this report makes no final shared-type decision.
* `WorldSizeName` is derived from dimensions. Changing a display locale must not change persisted bytes. Dimensions must be validated before tile/section allocation; the ordering is `read identity -> validate dimensions -> create storage -> load tile sections`.
* Creation and last-played timestamps have different source semantics. Local legacy files may use filesystem times; cloud legacy files may use the explicit fallback in Version4. A proposed clock/filesystem adapter must make this difference visible and testable.
* Save writes use one immutable snapshot. Runtime systems must not mutate world identity while `EncodeWorldHeader` is reading it; conflicting changes produce a rejected or retried command rather than a torn header.

## Checkpoint C05: world validity and rules handoff

### Proposed boundary

`P14.C05 WorldValidityAndRulesHandoff` is a proposed classification/projection boundary. It separates file-load outcome from world-generation and progression rules. `LoadStatus` and a sanitized error detail form a load-result projection; the rule flags and mode values form an explicit handoff snapshot. P14 does not own the corresponding gameplay transitions, seed activation, hard-mode progression, boss progression, or event scheduling.

`LoadException` must not become a mutable ECS component field or cross a network boundary as an exception object. A proposed `WorldLoadFailure` value carries a stable status/category, source context and optional diagnostic detail behind a diagnostics port. A proposed `WorldValidityQuery` computes `IsValid`, `HasValidSeed`, `HasCrimson`, `UseGuidAsMapName` and `MapFileName` without I/O or writes. `WorldSizeName` is the C04 dimensions-to-localization projection and is repeated here only as a query surface.

`seedOptionsInOrder` is a proposed rules registry/adapter input, not a component. The Version4 seed parser and `EnableSeedOptions` are incomplete in this baseline, so the proposed handoff may only expose validated raw seed text, parsed option tokens and an evidence status until parser compatibility is closed. `GameMode`, special seed flags, corruption/crimson, hard mode and defeated Moonlord must be handed to their eventual WorldSession/WorldGeneration/Progression owners through explicit commands or snapshots; final ownership is `crossSubsystemOwner: integration-review`.

### Evidence and current NLTX status

Version4 declares the C05 fields and properties in `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:35-108`. `GetAllMetadata` applies version-gated defaults for game mode and seed flags at `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:249-306`; `LoadHeader` applies the same compatibility branches to runtime Main/WorldGen state at `:2036-2094`. `FromInvalidWorld` creates a safe invalid metadata object with status, exception, empty seed, minimal dimensions and empty GUID at `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:325-349`. `TryApplyingCopiedSeed`, `EnableSeedOptions`, `TryParseSeedOptionValue` and `TryParseSecretSeed` show the intended seed/rule handoff but the parser methods and enable method are placeholders in Version4 (`:211-279`).

Current NLTX has `WorldRulesState`/`WorldRulesSnapshotValue` with a game-mode shell, but no P14 load-result classification, exception-free failure value, special-seed compatibility adapter or complete persistence-to-rules handoff was found. This status is `partial`; the existing rules shells are not evidence that all C05 fields are implemented.

### C05 member mapping

| Inventory member | Proposed role | Owner, readers, writers and lifecycle | Evidence status |
|---|---|---|---|
| `619 LoadStatus` | proposed `WorldValiditySnapshot.Status` classified load result | world load/recovery adapter writes it; UI/WorldSession/recovery queries read it; no gameplay system writes file status directly | Version4 declaration and load paths confirmed |
| `620 LoadException` | proposed diagnostic detail input to `WorldLoadFailure`, never a Component reference | load adapter captures/classifies it; diagnostics port may log it; network and ECS state receive only stable category/detail projection | Version4 declaration and invalid-world path confirmed; sanitization proposed |
| `625 DrunkWorld` | proposed special-seed rule handoff flag | world header parser writes the snapshot; WorldGeneration/WorldRules consumes it; P14 does not own generation behavior | Version4 version-gated read/write confirmed; final owner integration-review |
| `626 NotTheBees` | proposed special-seed rule handoff flag | same explicit handoff; not merged into generic validity or identity state | Version4 read/write confirmed; final owner integration-review |
| `627 ForTheWorthy` | proposed special-seed rule handoff flag | same explicit handoff; no implicit mutation of generation state from a query | Version4 read/write confirmed; final owner integration-review |
| `628 Anniversary` | proposed event/seed rule handoff flag | world rules/calendar integration consumes it; P14 only decodes/encodes the snapshot | Version4 read/write confirmed; final owner integration-review |
| `629 DontStarve` | proposed special-seed rule handoff flag | WorldGeneration/WorldSession consumes it through a command or snapshot | Version4 read/write confirmed; final owner integration-review |
| `630 RemixWorld` | proposed special-seed rule handoff flag | same handoff; compatibility defaults are version-gated | Version4 read/write confirmed; final owner integration-review |
| `631 NoTrapsWorld` | proposed special-seed rule handoff flag | same handoff; do not infer it from unrelated tile state | Version4 read/write confirmed; final owner integration-review |
| `632 ZenithWorld` | proposed derived/compatibility rule flag | decode explicit flag for supported versions, apply documented legacy derivation only in a migration adapter, then hand off | Version4 explicit flag and legacy derivation confirmed |
| `633 SkyblockWorld` | proposed special-seed rule handoff flag | header adapter writes/reads it; generation and progression own behavior | Version4 version-gated read/write confirmed; final owner integration-review |
| `634 HasCorruption` | proposed world-evil rule handoff value; `HasCrimson` is its pure inverse projection | world header/rules adapter writes it; world progression owns resulting biome behavior; no independent `HasCrimson` storage | Version4 declaration/property relation confirmed |
| `635 IsHardMode` | proposed progression snapshot input | world load/header adapter reads/writes the persisted flag; progression system owns transitions and invariants | Version4 declaration/header evidence partial; owner integration-review |
| `636 DefeatedMoonlord` | proposed progression snapshot input | header adapter reads/writes it; boss/progression owner consumes it; no P14 gameplay mutation | Version4 declaration/header evidence partial; owner integration-review |
| `637 seedOptionsInOrder` | proposed `WorldSeedRulesRegistry` adapter state | initialized by rules composition; parser/seed query reads it; not serialized as mutable world state and not an ECS Component | Version4 declaration confirmed; parser behavior partial |
| `641 SeedText` | proposed pure projection over C04 raw seed text | metadata/UI/rules queries read it; no independent write path | Version4 expression confirmed |
| `642 Seed` | proposed deterministic derived-seed query result over C04 `SeedText` | pure calculation/cache with explicit invalidation on seed change; no separate persistence authority | Version4 `TranslateSeed` confirmed |
| `643 IsValid` | proposed pure `WorldValidityQuery` over `LoadStatus` | UI/recovery/load orchestration reads it; no file or state mutation | Version4 expression confirmed |
| `644 WorldSizeName` | proposed localization projection over C04 dimensions | presentation/listing reads it; localization adapter supplies text; no persisted localized object | Version4 `SetWorldSize`/expression confirmed |
| `645 HasCrimson` | proposed inverse projection of `HasCorruption` | rules/UI queries read it; setter compatibility command, if retained, writes the single `HasCorruption` source through an explicit command | Version4 inverse getter/setter confirmed |
| `646 HasValidSeed` | proposed pure validity query over `WorldGeneratorVersion` | metadata/rules queries read it; it does not prove the parser or world file is valid | Version4 expression confirmed |
| `647 UseGuidAsMapName` | proposed protocol compatibility query over C04 generator version | map-file query reads it; no ID mutation | Version4 threshold expression confirmed |
| `648 MapFileName` | proposed derived map filename projection from `WorldId`/`UniqueId` and `UseGuidAsMapName` | map/storage adapter reads it; it must not be promoted to the persistent world ID | Version4 expression confirmed |

### C05 invariants and ordering

* A load status is not a gameplay state. A failed load may produce an invalid metadata projection for menus/recovery, but it must not publish a playable world or `WorldLoaded` event.
* Rules are applied only after the identity/header snapshot is parsed and accepted. The proposed order is `decode version -> build identity snapshot -> classify validity -> validate seed/mode/rule combinations -> hand off rules -> allocate/load world -> publish WorldLoaded`.
* `HasCrimson` is an inverse view of one evil source; storing both creates a contradiction risk. `Seed`, `HasValidSeed`, `UseGuidAsMapName` and `MapFileName` are projections, not independently serialized values.
* Unknown/future versions, malformed seed tokens and invalid dimensions remain explicit rejection/recovery outcomes. The incomplete Version4 seed parser is an evidence gap, not permission for a best-effort parser.

## Checkpoint C06: world Tile header core codec

### Proposed boundary

`P14.C06 WorldTileHeaderCoreCodec` is a proposed binary protocol adapter for the first two Tile header bytes and the fields they gate. The constants are not ECS components and do not own `Tile`, `Liquid`, `Wall`, `WorldSection` or tile-count state. A proposed codec receives an immutable Tile-cell encode view and emits/reads bytes; it returns a decoded value or classified malformed-input result and never writes runtime Tile storage by itself.

The core protocol has a layered header: Header 1 announces whether Header 2 exists; Header 2 announces whether Header 3 exists and carries core wire/slope/compression bits; Header 3 is covered by C07. Header 1 also encodes active tile, wall, liquid kind and run-length mode. The protocol must preserve the distinction between a one-byte and two-byte tile/wall ID, liquid kind versus liquid amount, and one-byte versus two-byte vertical compression length. The proposed codec must keep the byte layout explicit rather than hiding it behind a generic bitset.

### Evidence and current NLTX status

Version4 declares the 19 C06 masks in `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:23-61`. `SaveWorldTiles` sets active/tile and wall bits, tile/wall high-byte flags, liquid kind/amount, wire 1-3, half-brick/slope, continuation bits and compression bits at `:1473-1643`. `LoadWorldTiles` reads the same layers, restores tile/wall/liquid/wire/slope values and copies a decoded tile through a vertical run at `:2570-2777`. `ValidateWorld` independently walks the same byte protocol and checks stream positions at `:3080-3199`, proving that codec parsing is part of save validation rather than a runtime component update.

Current NLTX has `WorldStorage/TileMapStore.cs` and `WorldStorageRoot`, but no proposed/implemented Version4-compatible header codec or Tile byte protocol boundary was found. Storage shells are `partial`; Tile ownership, section allocation and liquid integration remain `crossSubsystemOwner: integration-review`.

### C06 member mapping

| Inventory member | Proposed role | Protocol meaning and owner | Evidence status |
|---|---|---|---|
| `547 Header1_1` | proposed core header mask | Header 1 continuation bit: Header 2 follows; codec controls presence only | Version4 declaration/read-write confirmed |
| `548 Header1_2` | proposed core header mask | active tile payload follows; Tile state owner remains integration-review | Version4 declaration/read-write confirmed |
| `549 Header1_4` | proposed core header mask | wall payload follows; Wall/Tile state owner remains integration-review | Version4 declaration/read-write confirmed |
| `550 Header1_8` | proposed core header mask | liquid payload follows with non-lava/non-honey base kind | Version4 declaration/read-write confirmed |
| `551 Header1_10` | proposed core header mask | lava liquid kind marker | Version4 declaration/read-write confirmed |
| `552 Header1_18` | proposed combined mask/value | honey liquid kind marker when combined with liquid presence | Version4 declaration/read-write confirmed |
| `553 Header1_20` | proposed core header mask | extended/high tile type byte follows | Version4 declaration/read-write confirmed |
| `554 Header1_40` | proposed core header mask | one-byte vertical run length follows | Version4 declaration/read-write confirmed |
| `555 Header1_80` | proposed core header mask | two-byte vertical run length follows | Version4 declaration/read-write confirmed |
| `556 Header1_C0` | proposed core mask group/value | two high bits select run-length encoding width | Version4 declaration/read-write confirmed |
| `557 Header2_1` | proposed continuation mask | Header 3 follows | Version4 declaration/read-write confirmed |
| `558 Header2_2` | proposed wire mask | wire 1 flag | Version4 declaration/read-write confirmed |
| `559 Header2_4` | proposed wire mask | wire 2 flag | Version4 declaration/read-write confirmed |
| `560 Header2_8` | proposed wire mask | wire 3 flag | Version4 declaration/read-write confirmed |
| `561 Header2_10` | proposed color/slope mask group | wall color and slope-related flags are decoded through Header 2/3; exact field combinations stay codec-owned | Version4 declaration/read-write confirmed |
| `562 Header2_20` | proposed core mask | wire 4/extension-related bit path is decoded with C07 boundary; do not duplicate ownership | Version4 declaration/read-write confirmed; cross-boundary |
| `563 Header2_40` | proposed wall-extension mask | high wall byte follows | Version4 declaration/read-write confirmed |
| `564 Header2_70` | proposed slope mask group | half-brick/slope value occupies the grouped slope bits | Version4 declaration/read-write confirmed |
| `565 Header2_80` | proposed liquid-extension mask | shimmer/liquid extension path is completed by C07 | Version4 declaration/read-write confirmed; cross-boundary |

### C06 invariants and scheduling

* The encoder must emit a continuation byte exactly when a later header byte or extension payload is present; the decoder must reject truncated continuation chains and impossible field lengths before mutating a tile store.
* Tile and wall IDs are unsigned protocol values. A high-byte marker changes the number of bytes consumed; it must not be interpreted as a signed value or as a different Tile component field.
* Liquid kind and amount are separate: a zero liquid amount means no payload; nonzero kind bits select water/lava/honey and C07 shimmer semantics. Liquid storage is external to this codec.
* Run-length compression applies to equal, batchable vertical tiles. Decoding must advance within bounds, preserve the decoded value for each copied cell and reject runs that pass the declared world height.
* `SaveWorldTiles`, `LoadWorldTiles` and `ValidateWorld` must share one proposed codec contract. No runtime System order may be inferred from the constants' declaration order; the explicit order is `Tile snapshot query -> encode/decode -> section validation -> Tile storage commit`.

## Checkpoint C07: world Tile header extension codec

### Proposed boundary

`P14.C07 WorldTileHeaderExtensionCodec` is a proposed extension protocol adapter for Header 3 and Header 4. It consumes and returns protocol flags that the core codec announces through continuation bits. It does not own actuator, wire, visibility, brightness, wall, shimmer, Tile or Liquid gameplay state. Its output is a pure decoded value passed to a Tile storage adapter or a read-back validator.

Header 3 carries color markers, actuator/inactive state, wire4, high wall ID and the presence of Header 4; Header 4 carries invisible block/wall and fullbright block/wall flags. Header 3 also carries shimmer through the liquid extension path. These flags have different runtime consumers and must remain grouped by byte protocol only, not combined into an `ExtendedTileComponent`.

### Evidence and current NLTX status

Version4 declares C07 masks at `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:63-93`. `SaveWorldTiles` emits Header 3/4 when colors, actuator/inactive/wire4, high wall ID, invisibility/fullbright or shimmer require them at `:1514-1599`; `LoadWorldTiles` restores those values at `:2631-2736`. `ValidateWorld` consumes extension bytes and compression without mutating runtime tiles at `:3167-3193`. The extension protocol is therefore part of the file contract and validation seam, not an ECS state bucket.

Current NLTX has no extension codec or proposed visibility/brightness adapter. `TileMapStore` is only a storage shell, so C07 is `missing` in NLTX; final Tile, Liquid and rendering owners are `crossSubsystemOwner: integration-review`.

### C07 member mapping

| Inventory member | Proposed role | Protocol meaning and owner | Evidence status |
|---|---|---|---|
| `566 Header3_1` | proposed Header 3 presence mask | announces Header 4 | Version4 declaration/read-write confirmed |
| `567 Header3_2` | proposed extension mask | actuator flag | Version4 declaration/read-write confirmed |
| `568 Header3_4` | proposed extension mask | inactive tile flag | Version4 declaration/read-write confirmed |
| `569 Header3_8` | proposed extension mask | tile/wall color payload presence paths | Version4 declaration/read-write confirmed |
| `570 Header3_10` | proposed extension mask | wall color payload path | Version4 declaration/read-write confirmed |
| `571 Header3_20` | proposed extension mask | wire4 flag | Version4 declaration/read-write confirmed |
| `572 Header3_40` | proposed extension mask | high wall ID byte follows | Version4 declaration/read-write confirmed |
| `573 Header3_80` | proposed extension mask | shimmer liquid marker | Version4 declaration/read-write confirmed |
| `574 Header4_1` | proposed Header 4 presence mask | Header 4 follows | Version4 declaration/read-write confirmed |
| `575 Header4_2` | proposed Header 4 mask | invisible block flag | Version4 declaration/read-write confirmed |
| `576 Header4_4` | proposed Header 4 mask | invisible wall flag | Version4 declaration/read-write confirmed |
| `577 Header4_8` | proposed Header 4 mask | fullbright block flag | Version4 declaration/read-write confirmed |
| `578 Header4_10` | proposed Header 4 mask | fullbright wall flag | Version4 declaration/read-write confirmed |
| `579 Header4_20` | proposed reserved/extension mask | reserved protocol position; decoder must preserve/validate it without assigning gameplay meaning until evidence exists | Version4 declaration present; use evidence-gap |
| `580 Header4_40` | proposed reserved/extension mask | reserved protocol position; no component owner claimed | Version4 declaration present; use evidence-gap |
| `581 Header4_80` | proposed reserved/extension mask | reserved protocol position; no component owner claimed | Version4 declaration present; use evidence-gap |

### C07 invariants and scheduling

* C07 consumes Header 3/4 only when C06 continuation bits announce them. A standalone Header 3/4 byte is malformed in the proposed codec unless the enclosing decode state allows it.
* Color, visibility and brightness values are protocol projections into separate Tile/rendering boundaries. They must not be persisted in one broad visual component without an owner decision.
* A high wall byte extends the wall ID assembled by C06; it is not a separate wall entity ID. Wall range validation occurs before a decoded value is handed to storage.
* Shimmer is a liquid kind extension, not a visual-only flag. The liquid adapter owns interpretation after codec decode; P14 owns only byte compatibility.
* Header 4 reserved positions must round-trip according to verified historical behavior or be rejected explicitly. The codec must not invent semantics for them.

## Checkpoint C08: world recovery transaction adapter

### Proposed boundary

`P14.C08 WorldRecoveryTransactionAdapter` is a proposed infrastructure adapter for the world-file transaction and recovery state. It is not an ECS component and it does not own world gameplay, Tile, Liquid, event, player, network or cloud-account state. The boundary separates four concerns that are co-located in Version4 `WorldFile`:

* `proposed WorldPersistenceLockPort` serializes save operations through the `IOLock` seam. The lock handle and monitor ownership never enter a Component or persistence snapshot.
* `proposed WorldFileVersionContext` carries the file version read from the stream and the proposed compatibility thresholds needed by version-gated decoders. `_versionNumber` and `VersionNumberForChestRework` are protocol context, not a world simulation revision.
* `proposed WorldStorageRoute` carries the local/cloud route selected from `_isWorldOnCloud`. It must preserve local and cloud stores as distinct effects and report an unavailable cloud as a classified outcome.
* `proposed WorldRecoveryFailureProjection` exposes a stable diagnostic/result value derived from `LastThrownLoadException`. The exception object itself must remain outside ECS, network snapshots and public component state.

The adapter exposes proposed save/load outcomes and recovery attempts to a proposed command/system boundary. It does not make a `bool` file-operation result into proof of atomicity, and it does not decide whether an uncertain cloud result is retryable, compensatable or result-unknown. That policy remains `crossSubsystemOwner: integration-review`.

### Evidence and current NLTX status

Version4 declares `IOLock`, `_versionNumber`, `_isWorldOnCloud`, `LastThrownLoadException` and `VersionNumberForChestRework` at `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:96`, `:116-118`, `:148-150`. `LoadWorld` selects the cloud route at `:658-675`, rejects an unavailable cloud before reading or publishing a world, reads the file version at `:721-730`, and records load exceptions at `:790-808`. `_versionNumber` gates legacy and versioned header/section readers throughout `:1846-2012`, `:2773-3012` and `:3244-3284`; the chest-rework threshold is consumed at `:2785-2796`. `SaveWorld` and `_SaveWorld` use the lock at `:878-931`, while `InternalSaveWorld` writes, reads back and validates bytes before backup handling at `:933-990`. The recovery callback performs two normal load attempts, replaces the file from `.bak`, retries, and only then restores temporary state and invokes `Hooks.WorldLoaded` at `D:\TRbackup\Version4\Terraria\WorldGen.cs:6336-6377`.

Current NLTX has world descriptor/storage shells but no proposed or implemented lock port, cloud route adapter, world-file transaction adapter, version-context value or recovery outcome protocol. C08 is therefore `missing` in NLTX; cloud-provider guarantees and final recovery/event ownership remain `crossSubsystemOwner: integration-review`.

### C08 member mapping

| Inventory member | Proposed role | Owner / lifecycle | Evidence status |
|---|---|---|---|
| `582 IOLock` | proposed `WorldPersistenceLockPort` implementation detail | acquired around a save transaction; released in both success and exception paths; never serialized or registered as a component | Version4 declaration and `Monitor.TryEnter`/fallback lock confirmed |
| `592 _versionNumber` | proposed `WorldFileVersionContext` value | set from the file header for each load/validation pass; read by version-gated decoders; discarded after the operation | Version4 read and downstream gates confirmed |
| `593 _isWorldOnCloud` | proposed `WorldStorageRoute` value | selected from active world metadata at load/save entry; routes every file effect; local/cloud ownership remains separate | Version4 route selection and cloud checks confirmed |
| `608 LastThrownLoadException` | proposed `WorldRecoveryFailureProjection` input, converted to a stable diagnostic | replaced on each failed load attempt; exposed only to recovery/logging callers; no exception object in ECS or network state | Version4 catch assignments confirmed; diagnostic contract proposed |
| `609 VersionNumberForChestRework` | proposed compatibility threshold in `WorldFileVersionContext`/codec policy | immutable protocol constant used by version-gated chest decoding; not a component field or gameplay rule | Version4 declaration and use confirmed |

### C08 transaction and recovery invariants

* A cloud route with no cloud provider fails early and must not be silently redirected to local storage. The proposed save and load outcomes must distinguish unavailable cloud, missing file, malformed bytes, unsupported future version and provider failure.
* The proposed lock port has one owner for the save transaction. `TryEnter`/skip behavior and the blocking fallback must remain explicit; a skipped save is not a successful commit and a recursive lock acquisition must not create a second transaction.
* Save ordering remains `capture -> encode -> commit -> read-back -> validate -> backup rotation -> publication`. The old bytes are restored to the primary path after a validation failure when available; backup rotation is not performed before validation succeeds.
* The read-back buffer is validated as the committed representation. A provider returning `false`, throwing, or returning an unconfirmed result must be classified before a commit projection is published. No cloud atomicity claim is made from the Version4 boolean alone.
* A normal load may be attempted twice. If both attempts fail, recovery may replace the primary file from `.bak` and retry according to the same bounded attempt policy. Missing backup and failed backup retries remain terminal recovery outcomes.
* `WorldLoaded` publication is downstream of successful load, recovery, temporary-state restoration and the final integration gate. C08 does not publish the event or restore event state itself; that collaboration is completed by C09 and remains `crossSubsystemOwner: integration-review`.
* Version values select protocol compatibility branches only. They must not be copied into world progression, network tick, save revision or persistent entity identity fields.

### C08 proposed interfaces and dependency direction

The proposed `WorldPersistenceLockPort` accepts a transaction delegate and returns an explicit acquired/skipped/faulted result. The proposed `WorldFileStorePort` accepts a `WorldStorageRoute` and exposes read, write, copy, delete and existence effects with classified errors. The proposed `WorldRecoveryTransactionAdapter` composes those ports with the C02 save command, C05 validity projection, C06/C07 codecs and the C09 temporary context. It returns a proposed `WorldRecoveryResult` to a caller-owned command/system; it does not mutate ECS state, call a Query for writes or publish `WorldLoaded` directly.

The dependency direction is:

```text
world snapshot / C09 temporary context
  -> C06/C07 format codecs
  -> C08 recovery transaction adapter
  -> lock and local/cloud file ports
  -> read-back and C05 validity query
  -> backup/recovery result projection
  -> integration-owned publication
```

## Checkpoint C09: world temporary event context

### Proposed boundary

`P14.C09 WorldTemporaryEventContext` is a proposed transient value/snapshot boundary used by world save and recovery. It is not a long-lived ECS component, not a second event authority and not a standalone persistence record. The context is encoded and decoded only as part of the versioned world-file protocol owned by C02/C08; its individual fields must not be registered as persistent runtime components.

The context has three proposed immutable value groups:

* `proposed WorldClockAndWeatherRestoreValue` carries time, day/night, moon phase, blood moon, eclipse, rain amount/time and Cultist Ritual delay.
* `proposed WorldPartyAndSandstormRestoreValue` carries birthday-party flags/cooldown/celebrating-NPC IDs and sandstorm state/severity values.
* `proposed WorldLanternAndPrecipitationRestoreValue` carries lantern-night flags/cooldown, coin-rain count and meteor-shower count.

`proposed WorldTemporaryEventCapturePort` reads the live world/event services into an immutable context, and `proposed WorldTemporaryEventRestoreSystem` applies a validated context after a successful load. `proposed WorldResetTimePolicy` is a separate command policy for `resetTime`; it must not be hidden inside a generic restore Query. The final live owners of Calendar/Event state remain `crossSubsystemOwner: integration-review`.

### Evidence and current NLTX status

Version4 declares the C09 fields at `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:98-114`, `:120-146`. `SetTempToOngoing` captures live state, clears and copies `BirthdayParty.CelebratingNPCs`, and records weather, party, sandstorm, lantern, coin-rain and meteor values at `:1071-1100`. `SetOngoingToTemps` restores those values and replaces the live celebrating-NPC list at `:152-182`. `InternalSaveWorld` invokes capture when `useTemps` is false and applies `ResetTempsToDayTime` when `resetTime` is true at `:933-945`; reset policy and secret-seed/anniversary branches are at `:1035-1070`. The same staging values are written by the world header codec at `:1312-1461` and read with version defaults at `:2125-2549`, with validation paths at `:3369-3382` and `:3597-3689`. Recovery restores them before `Hooks.WorldLoaded` at `D:\TRbackup\Version4\Terraria\WorldGen.cs:6375-6377`.

Current NLTX has world/session and partial calendar-related shells but no proposed or implemented C09 context, capture port, restore system or reset policy boundary. C09 is `missing` in NLTX; the final Calendar/Event owner and the exact cross-system publication contract remain `crossSubsystemOwner: integration-review`.

### C09 member mapping

| Inventory member group | Proposed role | Owner / lifecycle | Evidence status |
|---|---|---|---|
| `583 _tempTime`, `587 _tempDayTime`, `588 _tempBloodMoon`, `589 _tempEclipse`, `590 _tempMoonPhase`, `591 _tempCultistDelay` | proposed `WorldClockAndWeatherRestoreValue` fields | captured before encode or save-time reset; decoded into staging context; applied only after successful load | Version4 capture/restore and header read/write confirmed |
| `584 _tempRaining`, `585 _tempMaxRain`, `586 _tempRainTime` | proposed weather portion of `WorldClockAndWeatherRestoreValue` | capture/restore with `cloudAlpha` compatibility projection; not independent weather authority | Version4 capture/restore and header read/write confirmed |
| `594 _tempPartyGenuine`, `595 _tempPartyManual`, `596 _tempPartyCooldown`, `597 TempPartyCelebratingNPCs` | proposed `WorldPartyAndSandstormRestoreValue` party fields | list copied by value into the context and applied through a single restore effect; no mutable list leakage | Version4 clear/AddRange and version defaults confirmed |
| `598 _tempSandstormHappening`, `599 _tempSandstormTimeLeft`, `600 _tempSandstormSeverity`, `601 _tempSandstormIntendedSeverity` | proposed sandstorm portion of `WorldPartyAndSandstormRestoreValue` | transient restore payload; simulation system remains the live writer after restoration | Version4 capture/restore and header read/write confirmed |
| `602 _tempLanternNightGenuine`, `603 _tempLanternNightManual`, `604 _tempLanternNightNextNightIsGenuine`, `605 _tempLanternNightCooldown` | proposed lantern-night portion of `WorldLanternAndPrecipitationRestoreValue` | restored as one event payload; final event owner unresolved | Version4 capture/restore and header read/write confirmed |
| `606 _tempCoinRain`, `607 _tempMeteorShowerCount` | proposed precipitation/event portion of `WorldLanternAndPrecipitationRestoreValue` | restored before world publication; subsequent event transitions belong to Calendar/Event systems | Version4 capture/restore and header read/write confirmed |

### C09 invariants and scheduling

* Capture is a snapshot operation over explicit live-state ports. It must not retain references to mutable event lists, services or `Main` globals; `TempPartyCelebratingNPCs` is copied defensively.
* Restore is a command/system effect, not a Query. It applies the complete validated context once, with list replacement rather than append semantics, and reports a classified failure before publication if a required target is unavailable.
* The context may be serialized as fields inside the world header because Version4 does so. That does not make it an independently persisted ECS component or grant it ownership of the long-lived Calendar/Event state.
* `useTemps` controls whether save captures current ongoing values. `resetTime` applies the proposed reset policy after capture selection and before encoding; it is not equivalent to loading a saved context.
* Reset-to-daytime preserves Version4 defaults and exceptions: day time, `13500.0`, moon phase zero, no blood moon/eclipses, Cultist delay `86400`, cleared party/storm/lantern/meteor/coin-rain state, with the graveyard-bloodmoon and tenth-anniversary/non-skyblock branches retained as compatibility rules.
* Normal load/recovery ordering remains `decode -> validate -> restore temporary context -> WorldLoaded`. C09 must not restore before the final successful recovery attempt and must not publish `WorldLoaded` itself. The final event publication order is `crossSubsystemOwner: integration-review`.
* Version-gated missing fields use verified historical defaults. Truncated or contradictory event payloads are load failures, not partially applied live state.

### C09 proposed interfaces and dependency direction

The proposed `WorldTemporaryEventContext` is an immutable value returned by `WorldTemporaryEventCapturePort`. The proposed `WorldTemporaryEventRestoreSystem` accepts that value plus explicit Calendar/Event target ports and returns a restore result; it does not read files, acquire the C08 lock or invoke network/UI effects. The proposed C08 adapter supplies decoded context, while C02 supplies save-time capture intent. Calendar/Event systems remain the eventual live-state owners.

```text
live clock/weather/party/storm/lantern state
  -> C09 capture port
  -> immutable temporary event context
  -> C02/C08 versioned world codec and recovery result
  -> C09 restore system
  -> Calendar/Event live state
  -> integration-owned WorldLoaded publication
```

## Checkpoint C10: save configuration and metadata adapters

### Proposed boundary

`P14.C10 SaveConfigurationAndMetadataAdapters` is a proposed configuration/infrastructure boundary for the seven declared members `2746-2752`. It does not take ownership of the C01 file metadata/favorite fields `2731-2745` or `3890-3892`, and it does not turn configuration values into ECS components. It has two proposed adapter facets:

* `proposed GameConfigurationRootAdapter` accepts a JSON object at the boundary and exposes typed configuration lookup for world-generation pass configuration. The third-party `JObject` remains at the adapter edge; the proposed lookup returns a typed value/result and performs no file I/O or mutation.
* `proposed PreferencesStoreAdapter` owns the mutable key/value document, path, serializer settings, JSON/BSON choice, synchronization boundary and AutoSave effect. The dictionary, serializer settings and lock are private adapter state, not a component or network snapshot.

The complete-reference `Preferences` implementation is supplemental evidence only. It supplies the expected load/save/callback/conversion behavior to compare against after Version4 methods are restored; it does not prove that those methods exist in the checked Version4 baseline.

### Evidence and current NLTX status

Version4 `GameConfiguration` declares `_root` and performs `Get<T>` through `_root[entry].ToObject<T>()` at `D:\TRbackup\Version4\Terraria.IO\GameConfiguration.cs:1-20`; world-generation passes consume it at `D:\TRbackup\Version4\Terraria\WorldGen.cs:10564-10570`, `:10582-10583` and later pass registrations. Version4 `Preferences` currently ends after its constructor at `D:\TRbackup\Version4\Terraria.IO\Preferences.cs:1-53`, directly confirming `_data`, `_path`, `_serializerSettings`, `UseBson`, `_lock` and `AutoSave` only. The complete-reference supplement at `D:\TRbackup\无任何删减通过编译\Terraria.IO\Preferences.cs:56-223` shows locked JSON/BSON load/save, `OnLoad`/`OnSave` ordering, JSON text processing, typed/default reads, key enumeration and `AutoSave` on `Put`; those behaviors remain compatibility hypotheses for Version4.

Current NLTX has no proposed or implemented `ConfigurationAdapter`, preferences document adapter, serializer policy or configuration snapshot boundary. C10 is `missing` in NLTX. World-generation owns the meaning of pass entries; P14 only isolates configuration transport and file effects. Serializer compatibility, callback ordering and any cross-domain configuration owner remain `crossSubsystemOwner: integration-review` where they affect other subsystems.

### C10 member mapping

| Inventory member | Proposed role | Owner / lifecycle | Evidence status |
|---|---|---|---|
| `2746 GameConfiguration._root` | proposed `GameConfigurationRootAdapter` input | constructed for a generation/configuration scope; read by typed pass configuration queries; no persistence or ECS registration | Version4 declaration and `Get<T>` confirmed |
| `2747 Preferences._data` | proposed private preferences document state | mutable only inside the adapter synchronization boundary; loaded/replaced from a document and copied to read-only results | Version4 declaration confirmed; load/save behavior supplemental |
| `2748 Preferences._path` | proposed `PreferencesDocumentRoute` value | fixed at adapter construction; consumed by file effects; not a world/player path or external identity | Version4 declaration confirmed; effect usage supplemental |
| `2749 Preferences._serializerSettings` | proposed `PreferencesSerializationPolicy` value | fixed at construction from parse-all-types policy; controls JSON/BSON conversion; third-party serializer stays behind adapter | Version4 declaration and constructor policy confirmed; load/save behavior supplemental |
| `2750 Preferences.UseBson` | proposed format-selection value | immutable per adapter instance; selects BSON stream versus JSON text branch; not a gameplay setting | Version4 declaration confirmed; read/write branch supplemental |
| `2751 Preferences._lock` | proposed `PreferencesSynchronizationBoundary` implementation detail | serializes load/save/mutation effects and is released by the adapter; never a component field | Version4 declaration confirmed; locking behavior supplemental |
| `2752 Preferences.AutoSave` | proposed autosave policy input | mutation command may request save under the same synchronization boundary; failures must remain visible and bounded | Version4 declaration confirmed; `Put` behavior supplemental |

### C10 invariants and dependency direction

* `GameConfigurationRootAdapter` is a read-only transport boundary. A missing or non-convertible entry is a classified configuration result; it must not silently mutate the root or invoke persistence effects. Whether a missing entry preserves the direct Version4 null-failure behavior or becomes a stable result is a compatibility decision for implementation.
* Preferences reads and writes use one explicit synchronization owner. A failed load leaves a classified load result, and a failed save does not become a successful configuration commit merely because an in-memory dictionary changed.
* JSON and BSON are distinct wire formats. `UseBson` must select exactly one format per document; JSON-only text processing must not run on BSON bytes. No format migration or type-metadata policy is assumed from the incomplete Version4 file.
* Complete-reference callback ordering is a compatibility target, not direct Version4 evidence: load callbacks follow successful deserialization; save callbacks precede serialization; JSON text processing occurs after serialization and before write; `AutoSave` is a mutation-triggered effect. These points require a Version4 restoration decision and focused tests.
* A proposed Query may read a supplied immutable configuration snapshot, but it must not call `File.Exists`, deserialize, save or trigger `AutoSave`. Commands/adapters own all document effects.
* Configuration pass values flow into WorldGeneration through an explicit proposed input/snapshot. C10 does not own generation rules, random selection, world progression or any persistent world identity; those owners remain `crossSubsystemOwner: integration-review` where shared.

The proposed dependency direction is:

```text
configuration document / JSON root
  -> C10 configuration adapter
  -> immutable typed configuration snapshot
  -> WorldGeneration pass query/input

preferences mutation command
  -> synchronized PreferencesStoreAdapter
  -> JSON/BSON file effect
  -> classified save result / integration-owned observers
```

## Checkpoint C11: file platform boundary

### Proposed boundary

`P14.C11 FilePlatformBoundary` is a proposed boundary for file-platform values, path projections,
local/cloud file effects and runtime capability checks. It does not create a
`FilePlatformComponent` and it does not turn a regular expression, file handle, cloud client,
native dialog or runtime reflection probe into ECS state. The boundary has four proposed facets:

* `proposed ExtensionFilterValue` carries a filter name and a defensive copy of extension values.
  It is a value passed to a proposed file-browser adapter; it does not own UI selection or native
  dialog state.
* `proposed FilePathParsingAdapter` exposes pure filename and parent-path projections over the
  Version4-compatible separator grammar. It may use the proposed `FileNameRegex` implementation
  detail, but it must not perform I/O, normalize cloud identifiers as local paths, or mutate a
  file route.
* `proposed FilePlatformAdapter` owns local/cloud existence, read, write, copy, move, delete,
  read-only handling and file-browser effects. C08 owns world recovery transaction ordering and
  C10 owns configuration/favorites adapter composition; this boundary supplies their explicit
  file-effect port rather than duplicating those transactions.
* `proposed RuntimeCapabilityQuery` reports capability values through an explicit platform seam.
  `IsNet45OrNewer` is compatibility evidence for a reflection-based capability check, not a
  simulation or network revision. No direct Version4 consumer of that field was found.

### Evidence and current NLTX status

Version4 declares `ExtensionFilter.Name` and `Extensions` and assigns the supplied array reference
in `D:\TRbackup\Version4\Terraria.Utilities.FileBrowser\ExtensionFilter.cs:3-13`.
The complete-reference file-browser consumers flatten `ExtensionFilter.Extensions` into a native
dialog filter at `D:\TRbackup\无任何删减通过编译\Terraria.Utilities.FileBrowser\NativeFileDialog.cs:5-15`,
and construct filters at `D:\TRbackup\无任何删减通过编译\Terraria.Utilities.FileBrowser\FileBrowser.cs:12-27`.
The proposed value must copy the array at the boundary so later caller mutation cannot change a
pending dialog request; this is a proposed safety improvement, not a claim about Version4's
existing reference semantics.

Version4 declares `FileNameRegex` at
`D:\TRbackup\Version4\Terraria.Utilities\FileUtilities.cs:10-12`. `GetFileName` and
`GetParentFolderPath` use that regex at `:174-197`; the expression accepts both slash styles,
keeps an extensionless name as the filename, and treats the final dotted suffix as the extension.
`GetFullPath` leaves cloud paths unchanged and calls `Path.GetFullPath` only for local paths at
`:43-52`. Local/cloud existence and byte operations route through `File` or `SocialAPI.Cloud`
at `:14-149`, while read-only removal and parent-directory creation are local-write effects at
`:136-171`. `WorldFileData.FromInvalidWorld` and `FileData.GetFileName` are direct filename
consumers at `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:325-349` and
`D:\TRbackup\Version4\Terraria.IO\FileData.cs:37-42`; `Main` also guards generated paths with
`GetFullPath` at `D:\TRbackup\Version4\Terraria\Main.cs:1831-1835`.

Version4 declares `IsNet45OrNewer` at
`D:\TRbackup\Version4\Terraria.Utilities\NewRuntimeMethods.cs:6-8`, but no direct consumer of
that field was found in the Version4 tree. The complete-reference supplement uses the capability
for reflective GC collection at `D:\TRbackup\无任何删减通过编译\Terraria.Utilities\NewRuntimeMethods.cs:6-26`,
but that is supplemental evidence only. Current NLTX has no proposed or implemented file-platform
adapter, path projection, extension-filter value or runtime capability query; C11 is `missing` in
NLTX.

### C11 member mapping

| Inventory member | Proposed role | Owner / lifecycle | Evidence status |
|---|---|---|---|
| `2949 ExtensionFilter.Name` | `proposed ExtensionFilterValue.Name` | immutable request value; consumed by a file-browser adapter; no UI or native-dialog ownership in the value | Version4 declaration confirmed; consumer supplemented from complete reference |
| `2950 ExtensionFilter.Extensions` | `proposed ExtensionFilterValue.Extensions` | defensively copied immutable/read-only extension list; no mutation after request construction | Version4 declaration and assignment confirmed; defensive-copy rule proposed |
| `2960 FileUtilities.FileNameRegex` | private implementation detail of `proposed FilePathParsingAdapter` | initialized once for pure path projections; no persistence, cloud, UI or simulation lifecycle | Version4 declaration and both projections confirmed |
| `2964 NewRuntimeMethods.IsNet45OrNewer` | input to `proposed RuntimeCapabilityQuery` | process/runtime lifetime capability probe; not serialized, networked or registered as a component | Version4 declaration confirmed; direct consumer missing, complete-reference use supplemental |

### C11 invariants and dependency direction

* `ExtensionFilterValue` must preserve the filter name and extension ordering while copying the
  input array. Null/empty extension behavior is a compatibility decision for the file-browser
  caller, not an implicit UI fallback.
* Filename parsing must preserve both `\\` and `/`, extensionless names, dotted names and the
  checked Version4 final-extension behavior. `GetParentFolderPath` returns the matched path
  prefix; its `includeExtension` parameter is retained only as a compatibility surface because
  Version4 does not use it to alter the returned path.
* `FilePathParsingAdapter` is pure. It must not call `Path.GetFullPath` for cloud paths, resolve
  cloud identifiers against the local process, or silently normalize away separators that are
  meaningful to a provider.
* `FilePlatformAdapter` is the sole proposed owner of local/cloud file effects for this boundary.
  C08 and C10 call it through ports; they retain their own transaction and document ordering.
  Provider unavailable, timeout/result-unknown, permission, malformed path and read-only failures
  must be classified rather than converted into successful writes.
* `RuntimeCapabilityQuery` returns an explicit capability result and does not trigger GC, logging,
  file effects or reflection-based mutation. The absence of a direct Version4 consumer is an
  evidence gap; no current NLTX capability claim is made.
* File path, persistent file identity, cloud identity, external platform identity, world/player
  identity and network/entity IDs remain distinct. Any shared route or platform service owner is
  `crossSubsystemOwner: integration-review`.

The proposed dependency direction is:

```text
ExtensionFilterValue / path input
  -> pure FilePathParsingAdapter or file-browser request
  -> FilePlatformAdapter
  -> C08 recovery / C10 configuration and favorites effect ports
  -> classified result projection

runtime environment
  -> RuntimeCapabilityQuery
  -> explicit capability result
```

## 9. Evidence gaps and blocking decisions

### Evidence gaps

* P14 first-round output report is missing, so no prior member-level owner/lifecycle assessment can be reused.
* Version4 `Player.Serialize`, `Deserialize`, `InternalSavePlayerFile`, `WorldFileData.TryParseSeedOptionValue`, `TryParseSecretSeed`, `EnableSeedOptions`, and cloud move methods are empty/partial in the checked source. Complete-reference matching and exact player schema must be performed before implementation.
* `Preferences` in Version4 contains only constructor/state declarations in the checked file; its load/save methods and event ordering are not available in this baseline. Complete-reference behavior is supplemental and must not be promoted to Version4 fact without a compatibility decision.
* `WorldFile.ValidateWorld` details, all tile section compatibility branches, and cloud provider retry/atomicity semantics need focused verifier evidence before implementation.
* C08 confirms the Version4 lock, route, version and recovery ordering, but it does not establish provider-level durability, atomic rename, timeout, cancellation or retry semantics. No proposed adapter may claim those guarantees without an independently tested provider contract.
* C09 confirms field capture, restore, header placement and historical defaults, but exact Calendar/Event ownership, concurrent event mutation behavior and end-to-end restore/publication verification remain open.
* C10 confirms the configuration state declarations and `GameConfiguration.Get<T>` edge, but Version4 Preferences I/O, callbacks, conversion behavior and AutoSave semantics remain unimplemented evidence gaps.
* C11 confirms the Version4 filter declaration, path regex projections and local/cloud routing, but defensive-copy compatibility, platform error/retry/disposal policy, path security/normalization, file-browser ownership and the direct consumer of `IsNet45OrNewer` remain incomplete.

### Blocking decisions

1. `WorldDescriptorState` versus a new persistence snapshot owner: candidate owner is `WorldSession` for runtime identity and `WorldStorage/Persistence` for file schema; final owner is `crossSubsystemOwner: integration-review`.
2. Whether temporary event context is restored by the world load system or a calendar system: keeping it in P14 avoids persistence leakage, but the final event publication order is `crossSubsystemOwner: integration-review`.
3. Whether a failed cloud write is retryable, compensatable, or result-unknown: the adapter must not report success from a boolean alone; final retry policy is `crossSubsystemOwner: integration-review`.
4. Whether the file platform service is owned by P14, the external-platform boundary or a shared runtime layer; path security/normalization and provider error/retry semantics remain `crossSubsystemOwner: integration-review`.

## 10. Focused verifier plan (not run)

* Pure `FileMetadataCodec` vectors: exact 20-byte output, wrong magic, wrong type, invalid type, revision increment and favorite bit.
* Favorites adapter: local/cloud separation, missing file, malformed JSON, duplicate key update, write failure and no hidden Query writes.
* World save transaction: lock serialization, buffer encode, read-back validation, validation rejection preserving prior backup, backup count clamp, and commit event ordering.
* World load/recovery: missing file/autogen, later-version status, malformed header, two normal retries, backup replacement, final failure and no `WorldLoaded` before temporary-state restoration.
* Recovery transaction: cloud-unavailable early return, lock contention and skip/fallback behavior, version-gated decoding, write/read-back mismatch, validation failure restoring prior bytes, bounded normal/backup retries, and no false cloud success.
* Temporary event context: capture/restore inverse vectors, defensive party-list copy, old-version defaults, reset-time secret-seed/anniversary branches, truncated payload rejection and no `WorldLoaded` before restoration.
* Configuration adapters: JSON/BSON round trips, missing-file and malformed-document results, serializer setting policy, callback ordering, typed/default conversion, lock serialization, `AutoSave` write failures and no hidden Query I/O.
* File platform boundary: extension-filter value copying, Windows/Unix path parsing, filename/parent-path edge cases, local/cloud read/write/copy/move/delete results, cloud-path `GetFullPath` bypass, read-only handling, platform failure classification, runtime capability fallback and no implicit platform effect from a pure Query.
* Tile codec: all core/extension flag combinations, wall/tile ID overflow, liquid kind, vertical compression lengths and invalid data rejection.
* Player session: timer start/pause/stop idempotence, deterministic accumulated duration using injected clock, missing player guard, `ServerSideCharacter` policy, and save adapter failure propagation.
* Static checks: proposed paths follow domain-first ECS organization; no proposed component contains file handles, cloud clients, `Stopwatch`, exceptions, UI references or mutable internal dictionaries.

verificationStatus: component-build-passed-no-tests (full proposed boundary not-run; C03 non-Component roles and C04-C10 deferred)

## 11. Integration Handoff

subsystemId: PersistenceAndRecovery / RuntimeComposition / SharedRuntimeMechanisms (P14 non-authoritative)
taskNumber: P14
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-component-design.md

evidenceStatus: source inventory confirmed; C01/C02 boundary evidence confirmed; C03 player boundary is full-reference-supplemented but Version4 implementation remains partial; C04 identity evidence confirmed with partial NLTX owner; C05 validity/rules evidence confirmed with partial parser and NLTX owner; C06/C07 codec layout evidence confirmed with missing NLTX implementation and reserved-bit gaps; C08 recovery and C09 temporary-context boundaries source-confirmed with provider/event gaps; C10 configuration declarations source-confirmed but Preferences I/O is complete-reference-supplemented; C11 filter declaration, path parsing and local/cloud helper behavior source-confirmed, with runtime capability consumer, defensive-copy compatibility and platform contract gaps
nltxStatus: partial; PlayerSaveSessionComponent is build-verified in the current session; remaining proposed P14 boundaries are deferred under the Component-only scope
verificationStatus: component-build-passed-no-tests (C03 non-Component roles and C04-C10 deferred; no tests run)

confirmedOwners:
- Version4 FileMetadata binary header validation and revision/favorite protocol
- Version4 world save read-back validation and backup rotation boundary
- Version4 temporary event capture/restore boundary

proposedTypes:
- proposed SaveFileReferenceSnapshot
- proposed FavoritesIndexAdapter
- proposed WorldDescriptorPersistenceSnapshot
- proposed WorldValiditySnapshot
- proposed WorldSaveCommand / WorldSaveValidationQuery / WorldSaveProjection
- proposed WorldTileHeaderCoreCodec / WorldTileHeaderExtensionCodec
- proposed WorldRecoveryTransactionAdapter / WorldTemporaryEventContext
- implemented PlayerSaveSessionComponent (current session); proposed PlayerSaveAdapter
- proposed GameConfigurationRootAdapter / PreferencesStoreAdapter / PreferencesSerializationPolicy
- proposed ExtensionFilterValue / FilePathParsingAdapter / FilePlatformAdapter / RuntimeCapabilityQuery

sharedTypesForIntegrationReview:
- WorldId, PersistentWorldId, WorldFileMetadata, FileMetadataRevision
- EntityId, PersistentEntityId, NetworkId, PlayerEntityId, TileCoordinate, WorldSectionId
- World snapshot schema and WorldLoaded/SaveCommitted ordering

crossSubsystemReaders:
- World session, tile/storage, liquid, player, world progression/calendar, network, UI and diagnostics candidates

crossSubsystemWriters:
- World generation, player save, server shutdown/autosave, cloud/file adapters, recovery command and validation system candidates

orderingConstraints:
- capture -> encode -> commit -> read-back -> validate -> backup rotation -> publication
- load/restore temporary context -> WorldLoaded publication

boundaryChallenges:
- Keep file protocol and temporary restore payload outside authoritative ECS components.
- Do not merge unrelated Main globals into one persistence component.
- Treat Version4 player serialization and Preferences implementation gaps as unresolved until direct compatibility evidence is available.

evidenceGaps:
- missing first-round P14 report; partial Version4 player serializer/session methods despite complete-reference supplement; missing Version4 Preferences I/O with only complete-reference supplement; cloud atomicity, complete tile validation, extension-filter consumers, platform error/retry contract and runtime capability consumer evidence

blockingDecisions:
- final persistence/player/world snapshot owner, player serializer compatibility, rules/progression owner, Tile/Liquid/rendering codec owner, event restoration owner, shared ID/schema owner, cloud retry semantics

notImplemented:
- C03 non-Component roles and C04-C10 remain deferred; C01/C02/C11 are prior non-Component
  checkpoints and are outside the current session change set.
- The current session did not create an adapter, system, serializer, migration or verifier; the
  existing non-Component files in `src2` are not claimed as current-session changes.

verifierPlan:
- focused codec, save/load recovery, backup, cloud failure, player timer, tile flag, and static boundary checks listed above

本文件保留非权威的 proposed 边界和被排除的非 Component 工作记录。当前 session 只实现了
`src2/Player/PlayerSaveSessionComponent.cs`，并对受影响项目执行了串行构建；没有运行测试，
也没有宣称行为等价、网络闭合、持久化闭合或跨分区 owner 裁决已经完成。
