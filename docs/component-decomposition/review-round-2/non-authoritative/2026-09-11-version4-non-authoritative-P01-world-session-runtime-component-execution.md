# Version4 非权威 P01 世界会话与运行时：proposed 实施计划

partitionId: P01
sessionId: 12ecaf3ffa2e43d490f5f18d19cda8d3
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\01-world-session-runtime.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P01-world-session-runtime-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P01-world-session-runtime-component-execution.md
designStatus: proposed
executionStatus: completed (src2-only implementation checkpoint)
implementationStatus: completed
verificationStatus: independently-verified (serial build, focused verifier, and 149-member coverage audit passed; integration and behavior-equivalence verification remain)
completedComponents: [FrameActivityStateComponent, RuntimeBootstrapAndWorldRuleStateComponent, FrameTimingAndSchedulingStateComponent, FrameControlAndTimeSkipStateComponent, WorldSeedAndSpecialRuleBoundary, SessionReadinessAndWorldCatalogBoundary, HostLoadAndShutdownBoundary, DerivedWorldSessionQueriesAndDifficultyDefinitions, ManEaterProtectionIndexComponent, WorldSessionPersistenceAndReplicationBoundary]
currentComponent: none (src2 implementation and focused verification complete)
pendingComponents: []
lastCheckpointUtc: 2026-09-12T07:32:20.4258455Z
evidence-gap: P01 专属第一轮 public-decomposition 输出文件不存在；输入库存只确认声明/计数，逐成员读者、写者、生命周期、网络/存档提交和宿主语义仍有 partial/unresolved；任务包中的 `Terraria\\WorldFile.cs` 与实际 `Terraria.IO\\WorldFile.cs` 不一致；`约束/公共拆分约束.md` 未在声明路径找到；src2 serial build、focused verifier 和 149-member coverage audit 已通过；`WorldSessionReplicationProjection` 目前仅提供 typed committed-snapshot pass-through，不闭合 WorldData packet 7 客户端恢复；`WorldPersistenceAdapter` 是内存边界，不是生产文件 I/O。
blocking-decision: crossSubsystemOwner: integration-review；需整合会话锁定 WorldSession 根实体、Hardmode 事务、FrameActivity 聚合、WorldData 客户端恢复、宿主/平台隔离、公共 ID/快照以及 Version4 时间与网络/持久化提交顺序。

> 本文件记录 P01 非权威实现执行状态。跨分区 owner、生产接线、Version4 行为等价、文件格式和 packet 7 客户端恢复仍未裁决；对应源码已保存到 `src2`，且串行 build、focused verifier 和 149 条成员覆盖审计已通过。这不表示生产 `src` 已迁移、行为等价、网络闭合或生产持久化完成。

## 当前执行状态

实际写入根目录为 `D:\TRbackup\NLTX\src2`。P01 源码清单为：`src2/WorldSession/**/*.cs`（119 个 C# 文件）、`src2/WorldSession/Terraria.WorldSession.csproj`、`src2/WorldInteraction/Components/ManEaterProtectionIndexComponent.cs`、`src2/WorldInteraction/Components/ManEaterProtectionSystem.cs`、`src2/WorldSessionVerification/Program.cs` 和 `src2/WorldSessionVerification/Terraria.WorldSessionVerification.csproj`。`src2/WorldInteraction/Tiles/**` 是其他共享 checkout 改动，本会话未修改或纳入 P01 owner 声明；`src` 未修改。

九个原计划 checkpoint 和最小 typed persistence/replication boundary 已保存。串行 build、focused verifier 和 149 条成员覆盖审计均已通过；当前 `implementationStatus: completed`、`verificationStatus: independently-verified`，但这不扩大为生产迁移或行为等价结论。

## 1. 目标文件与目录原则

原计划目标按领域优先和一文件一个核心公开类型落地到 `src2`；下表保留角色、边界和兼容意图，实际文件清单见“当前执行状态”：

| 目标角色 | proposed 目标路径/命名空间 | 说明 |
| --- | --- | --- |
| Frame activity | `src2/WorldSession/FrameActivityStateComponent.cs`, `Terraria.WorldSession.Components` | transient world-root state |
| Runtime bootstrap/rules | `src2/WorldSession/RuntimeBootstrapAndWorldRuleStateComponent.cs`, `Terraria.WorldSession.Components` | 仅保存内聚启动/规则状态 |
| Timing/scheduling | `src2/WorldSession/FrameTimingAndSchedulingStateComponent.cs`, `Terraria.WorldSession.Components` | pause/diagnostic state only |
| Time skip | `src2/WorldSession/WorldTimeSkipStateComponent.cs`, `Terraria.WorldSession.Components` | intent/cooldown state, not full clock |
| Readiness | `src2/WorldSession/SessionReadinessComponent.cs`, `Terraria.WorldSession.Components` | phase and stable failure code |
| Load progress | `src2/WorldSession/RuntimeLoadProgressComponent.cs`, `Terraria.WorldSession.Components` | process/host lifetime, not world save data |
| Man-eater guard | `src2/WorldInteraction/ManEaterProtectionIndexComponent.cs`, `Terraria.WorldInteraction.Components` | spatial transient index; final owner integration-review |
| Pure definitions/queries | `src2/WorldSession/DifficultyRuleDefinition.cs`, `DifficultyCurve.cs`, `DifficultyQuery.cs`, `SpecialSeedEligibilityQuery.cs` | no component registration; pure or immutable |
| External boundaries | `src2/WorldSession/Runtime/RuntimeHostAdapter.cs`, `WorldPersistenceAdapter.cs`, `WorldSessionReplicationProjection.cs` | proposed adapter/projection boundary |
| Commands/ports | `src2/WorldSession/Runtime/FrameControlCommand.cs`, `WorldTimeSkipCommand.cs`, `DeferredProcessPort.cs` | explicit intent/effect boundary |

Implementation mode keeps existing `src` untouched and does not silently rename public identities or register a second component key. The missing `约束/公共拆分约束.md` remains an evidence gap; the repository ECS organization, naming, style and side-effect rules were used for the saved `src2` files.

## 2. Implementation order

1. Lock a focused verifier for frame activity and Version4 tick trace.
2. Introduce the proposed component/port behind a read-only legacy facade; no second independent writer.
3. Migrate one write set at a time, first frame activity, then bootstrap/rules, timing/control, readiness, host, pure queries, and spatial protection.
4. Add typed persistence and replication projections only after the core commit boundary is observable.
5. Remove legacy writes only after the verifier proves one writer, lifecycle cleanup, and compatible snapshots.

The saved source is execution evidence that the implementation slice was attempted; it is not compile, runtime, migration, or behavior-equivalence evidence. The next gate is the serial build followed by the no-build verifier.

## 3. Ownership, compatibility and effects

- `FrameActivityStateComponent` has one proposed `FrameActivityCommitSystem` writer. Player/NPC/Projectile systems submit contributions through proposed accumulator commands; they do not write the component directly.
- `WorldSecretSeedRuleComponent`, `SessionReadinessComponent`, `WorldTimeSkipStateComponent` and `RuntimeLoadProgressComponent` each have one proposed owner system. `WorldRulesState`/Hardmode and public IDs remain `crossSubsystemOwner: integration-review`.
- Legacy `Main` fields may be exposed through a temporary read-only facade. The facade must not allow bidirectional synchronization. A field is removed only after all callers are routed through the proposed query/command/adapter.
- Persisted world headers and network packet 7 retain Version4 field order and version branches through adapters. The core components must not depend on binary reader/writer or third-party framework types.
- Randomness, wall clock, `GameTime`, file I/O, network send/receive, logs, UI, platform power state, window handles and background threads remain explicit ports/adapters. Timeout/unknown-result and retry behavior must be tested before implementation completion.

## 4. Proposed source-member mapping checkpoints

The following mapping is the execution checklist. It must cover all 149 input members before settlement. `source` paths are evidence paths; `target` paths are proposed only.

### Checkpoint 1: FrameActivityStateComponent

| Seq | Source member | Proposed role/target | Owner/lifecycle |
| ---: | --- | --- | --- |
| 1 | `Main.CurrentFrameFlags.ActivePlayersCount` | `FrameActivityStateComponent.ActivePlayerCount` | `FrameActivityCommitSystem`; reset/commit per world Tick |
| 2 | `Main.CurrentFrameFlags.SleepingPlayersCount` | `FrameActivityStateComponent.SleepingPlayerCount` | same transient aggregate |
| 3 | `Main.CurrentFrameFlags.AnyActiveBossNPC` | `FrameActivityStateComponent.AnyActiveBoss` | NPC contribution command; commit after NPC phase |
| 4 | `Main.CurrentFrameFlags.HadAnActiveInteractableProjectile` | `FrameActivityStateComponent.HadActiveInteractableProjectile` | Projectile contribution command; reset before Projectile phase |

Checkpoint 1 is implemented in `src2/WorldSession/Components/` with a focused verifier; it remains non-authoritative and has no production writer migration.

### Checkpoint 2: RuntimeBootstrapAndWorldRuleStateComponent

| Seq | Source member set | Proposed role/target | Migration boundary |
| ---: | --- | --- | --- |
| 7-9 | `Main.mapDelay`, `Assets`, `GameAskedToQuit` | `RuntimeBootstrapAndWorldRuleStateComponent` only for pacing/config; `RuntimeHostAdapter`/shutdown command for external references | Import validated config first; keep assets and quit intent outside persistent ECS state |
| 18-21 | `versionNumber`, `versionNumber2`, `AnnouncementBoxDisabled`, `AnnouncementBoxRange` | `RuntimeBuildIdentity` + `AnnouncementPolicy` | Read-only facade; no network/world snapshot inclusion |
| 22 | `AutogenSeedName` | proposed `WorldGenerationRequest` command payload | Consume once; do not double-write active seed flags |
| 23-35 | `drunkWorld`, `getGoodWorld`, `tenthAnniversaryWorld`, `dontStarveWorld`, `notTheBeesWorld`, `remixWorld`, `noTrapsWorld`, `zenithWorld`, `skyblockWorld`, `vampireSeed`, `infectedSeed`, `teamBasedSpawnsSeed`, `dualDungeonsSeed` | proposed `RuntimeBootstrapAndWorldRuleStateComponent.SecretSeedFlags` | Single world-load/generation commit owner; preserve legacy wire bits until projection migration |
| 36 | `_gameModeDifficultyOverride` | proposed `RuleOverrideSnapshot` input | `SimulationRuleOverrides` remains candidate owner; no base-rule write-back |
| 37 | `destroyerHB` | deferred to integration-review combat/event boundary | Do not move in P01 implementation slice |

Checkpoint 2 static guard: target component must not reference `IAssetRepository`, `Asset<Effect>`, `IntPtr`, `GameTime`, `Dictionary<string,string>`, platform handles, or third-party game services. All target names and files remain proposed.

### Checkpoint 3: FrameTimingAndSchedulingStateComponent

| Seq | Source member set | Proposed role/target | Migration boundary |
| ---: | --- | --- | --- |
| 48-52 | `clientUUID`, `GlobalTimeWrappedHourly`, `GlobalTimerPaused`, `gameTimeCache`, `ScreenShaderRef` | client identity/clock/asset adapters; only wrapped-hour and pause flags may have proposed runtime state | Keep engine time and assets outside core; snapshot only explicit immutable timing values |
| 53-54 | `DelayedProcesses`, `DelayedProcessesInGame` | proposed `DeferredProcessSchedulerPort` | Drain at explicit phases; define cancellation, failure and repeat semantics before changing writers |
| 55 | `npcStreamSpeed` | proposed `NpcStreamSchedulingPolicy` | Shared/network owner requires integration review |
| 56-58 | `dedServFPS`, `dedServCount1`, `dedServCount2` | proposed `DedicatedServerFrameMetrics` diagnostics state | Metrics projection only; no simulation write-back |
| 59-60 | `offLimitBorderTiles`, `maxMusic` | proposed `RuntimeLimitsDefinition` / `AudioCapacityAdapter` | Preserve constants and avoid creating mutable ECS state |

Checkpoint 3 static guard: no proposed core component may expose `IEnumerable<IEnumerator>`, engine `GameTime`, `Asset<Effect>`, client UUID, platform/service types, or a writable diagnostics collection. Delayed work must pass an explicit effect port.

## 5. Compatibility and rollback plan

For each later checkpoint, preserve the old read surface until the proposed owner is active. Roll back a checkpoint when any focused verifier observes a second writer, an ordering change, snapshot byte drift, readiness leakage, or cleanup failure. Rollback means disabling the proposed adapter/owner and restoring the legacy facade as the only writer; it does not mean deleting files or editing shared ledger state.

## 6. Focused verifier plan

The focused verifier is saved at `src2/WorldSessionVerification/Program.cs`; it has run successfully after the affected project was built serially:

- `P01-FrameActivityTrace`: reset, contribution, commit and consumer visibility.
- `P01-ReadinessGate`: phase transitions, generation barrier, failure and unload cleanup.
- `P01-TimeSkipCompatibility`: sundial/moondial command idempotence, cooldown and packet projection.
- `P01-HostBoundary`: no platform/third-party type crosses the proposed core state boundary.
- `P01-DifficultyQuery`: exact curve interpolation and difficulty override precedence.
- `P01-SpecialSeedQuery`: truth table for all 12 seed-derived properties.
- `P01-PersistenceProjection`: typed snapshot, Version4 header order, failed-save rollback.
- `P01-WorldDataProjection`: packet 7 golden bytes and client-only receive boundary; currently blocked by unresolved client case 7 path.
- `P01-OwnerGuard`: static and runtime proof of one writer per proposed component.

The build command was run through `Build/Tools/Invoke-SerialDotnet.ps1` for `src2/WorldSessionVerification/Terraria.WorldSessionVerification.csproj` with `-m:1`, `-nr:false`, `-p:UseSharedCompilation=false`, `-p:MSBuildNodeReuse=false`, and `-p:BuildInParallel=false`; it exited 0 with 0 warnings and 0 errors. `Terraria.WorldSession.dll` and `Terraria.WorldSessionVerification.dll` were confirmed under `Build/bin/`. The verifier then ran through the same wrapper with `--no-build --no-restore`, exited 0, and printed `P01 verifier passed.` The independent 149-member coverage audit also matched the expected sequence set exactly.

## 7. Checkpoint 4: FrameControlAndTimeSkipStateComponent

| Seq | Source member | Proposed role/target | Migration boundary |
| ---: | --- | --- | --- |
| 165 | `Main.hardMode` | proposed read-only `HardmodeSnapshot` seam owned by `WorldProgression` candidate | Migrate readers to a committed snapshot only after `WorldGen.StartHardmode` transaction and cross-domain owner are verified; `crossSubsystemOwner: integration-review` |
| 166 | `Main.maxQ` | proposed `FrameControlAndTimeSkipStateComponent.MaxQueryEnabled` | Preserve scene-reset writer and read-only facade; no persistence or network projection |
| 167 | `Main.DiscoR` | proposed `DiscoColorState.Red` presentation cache | Move with B/G as one color value; presentation writer only; no simulation or network ownership |
| 168 | `Main.DiscoB` | proposed `DiscoColorState.Blue` presentation cache | Same color migration boundary as seq 167 |
| 169 | `Main.DiscoG` | proposed `DiscoColorState.Green` presentation cache | Same color migration boundary as seq 167 |
| 170 | `Main.gamePaused` | proposed `FrameControlAndTimeSkipStateComponent.GamePaused` | Recompute at the legacy pause decision point; keep distinct from `GlobalTimerPaused` |
| 171 | `Main.ReHideCursor` | proposed `CursorVisibilityCommand` consumed by a host/client adapter | Convert callback writes to one-shot command consumption; never include in world snapshot |
| 172 | `Main.updatesCountedForFPS` | proposed `DedicatedServerFrameMetrics.UpdatesInCurrentWindow` | Keep diagnostics projection and reset semantics; prohibit simulation write-back |
| 173 | `Main.autoJoin` | proposed `AutoJoinRequest` command state | Consume once at connection boundary; preserve failure/duplicate-command behavior before removing legacy write |
| 470 | `Main.fastForwardTimeToDawn` | proposed `WorldTimeSkipStateComponent.FastForwardToDawn` | Consume at the explicit time-update phase; clear after the Version4-compatible jump; no full clock ownership |
| 471 | `Main.sundialCooldown` | proposed `WorldTimeSkipStateComponent.SundialCooldownTicks` | Preserve the cooldown decrement and reload cleanup semantics; one time-skip writer |
| 472 | `Main.fastForwardTimeToDusk` | proposed `WorldTimeSkipStateComponent.FastForwardToDusk` | Consume at the explicit time-update phase; clear after the Version4-compatible jump; no full clock ownership |
| 473 | `Main.moondialCooldown` | proposed `WorldTimeSkipStateComponent.MoondialCooldownTicks` | Preserve the cooldown decrement and reload cleanup semantics; one time-skip writer |

Checkpoint 4 guard: `hardMode` is represented only by the read projection seam; presentation, cursor, auto-join and diagnostic state must not cross the persistence or authoritative network boundary. The corresponding non-authoritative source is saved in `src2`.

## 8. Checkpoint 5: WorldSeedAndSpecialRuleBoundary

| Seq | Source member | Proposed role/target | Migration boundary |
| ---: | --- | --- | --- |
| 251 | `Main.rand` | proposed `WorldRandomPort` with thread/session scope | Replace direct global access with explicit injected RNG; preserve generation reproducibility and failure semantics before removing legacy access; `crossSubsystemOwner: integration-review` |
| 252 | `Main.moonType` | proposed `WorldPresentationRuleState.MoonType` or celestial adapter value | Decide persistence and writer with world/weather integration; do not place in RNG or general bootstrap component |
| 253 | `Main.UseExperimentalFeatures` | proposed immutable `ExperimentalFeaturePolicy` | Import once at process bootstrap; keep outside world save and authoritative replication |
| 254 | `Main.DefaultSeed` | proposed `DefaultSeedPolicy`/generation command input | Consume at generation boundary; validate before rule commit; never duplicate active seed state |
| 3857 | `SpecialSeedFeatures.ShouldDropExtraGel` | proposed pure `SpecialSeedEligibilityQuery.ShouldDropExtraGel` | Add truth-table verifier; no cached component field |
| 3858 | `SpecialSeedFeatures.ShouldDropExtraWood` | proposed pure `SpecialSeedEligibilityQuery.ShouldDropExtraWood` | Same pure-query guard |
| 3859 | `SpecialSeedFeatures.DungeonEntranceHasATree` | proposed pure `SpecialSeedEligibilityQuery.DungeonEntranceHasATree` | Same pure-query guard |
| 3860 | `SpecialSeedFeatures.DungeonEntranceIsBuried` | proposed pure `SpecialSeedEligibilityQuery.DungeonEntranceIsBuried` | Include `surfaceIsDesert` and underground dependency in truth table |
| 3861 | `SpecialSeedFeatures.DungeonEntranceIsUnderground` | proposed pure `SpecialSeedEligibilityQuery.DungeonEntranceIsUnderground` | Include drunk-world and `noSurface` precedence |
| 3862 | `SpecialSeedFeatures.NoDungeonGuardian` | proposed pure `SpecialSeedEligibilityQuery.NoDungeonGuardian` | Derive from shimmer-ocean rule input; no writer |
| 3863 | `SpecialSeedFeatures.BossesKeepSpawning` | proposed pure `SpecialSeedEligibilityQuery.BossesKeepSpawning` | Verify good-world, dont-starve and anniversary precedence |
| 3864 | `SpecialSeedFeatures.ShimmerSpawnHalfOfWorld` | proposed pure `SpecialSeedEligibilityQuery.ShimmerSpawnHalfOfWorld` | Derive only from committed rule inputs |
| 3865 | `SpecialSeedFeatures.RainbowSandAndBlackSandWalls` | proposed pure `SpecialSeedEligibilityQuery.RainbowSandAndBlackSandWalls` | Derive only from committed rule inputs |
| 3866 | `SpecialSeedFeatures.SpawnOnBeach` | proposed pure `SpecialSeedEligibilityQuery.SpawnOnBeach` | Verify anniversary/remix/dont-starve precedence |
| 3867 | `SpecialSeedFeatures.SpawnOnBeachOnDungeonSide` | proposed pure `SpecialSeedEligibilityQuery.SpawnOnBeachOnDungeonSide` | Compose from `SpawnOnBeach` and shimmer-ocean result; do not cache |
| 3868 | `SpecialSeedFeatures.Mechdusa` | proposed pure `SpecialSeedEligibilityQuery.Mechdusa` | Verify remix/good-world dependency |
| 2423 | `FixExploitManEaters.IndexesProtected` | proposed `ManEaterProtectionIndexComponent` and `ManEaterProtectionSystem` | `Update` clears, `ProtectSpot` writes, `SpotProtected` reads; prove frame cleanup and idempotent membership |

Checkpoint 5 guard: random sources, feature-derived booleans and exploit-protection indexes must not be serialized as a single world-rules component. The twelve special-seed properties remain queries until cache ownership and invalidation are independently verified.

## 9. Checkpoint 6: SessionReadinessAndWorldCatalogBoundary

| Seq | Source member | Proposed role/target | Migration boundary |
| ---: | --- | --- | --- |
| 422 | `Main._worldPreparationState` | proposed `SessionReadinessComponent.Phase` | Preserve generation/loading barrier input; migrate state transitions only with explicit failed/unload paths and one writer; `crossSubsystemOwner: integration-review` |
| 423 | `Main.motd` | proposed `SessionPresentationState.Motd` or host/network projection input | Keep CLI/command update semantics; decide network visibility before projection migration |
| 424 | `Main.gameMenu` | proposed readiness/menu runtime view | Keep distinct from session phase and entity gate; preserve menu entry/exit ordering |
| 425 | `Main.lockMenuBGChange` | proposed client-only `MenuPresentationPolicy` | Migrate with UI/background adapter; no persistence or authoritative replication |
| 426 | `Main.maxLoadWorld` | proposed `WorldLoadBudgetPolicy` | Import as host/session policy; do not register as world entity state |
| 427 | `Main.ActivePlayerFileData` | proposed `PlayerFileSelectionAdapter` | Replace external object exposure with typed selection metadata; retain adapter ownership of file object |
| 428 | `Main.WorldList` | proposed `WorldCatalogAdapter` and immutable `WorldCatalogProjection` | Migrate scan/sort/refresh and invalid-entry behavior before changing UI/CLI readers |
| 429 | `Main.ActiveWorldFileData` | proposed committed active-world metadata projection | Separate metadata snapshot from `WorldFileData`; block migration until persistence owner and packet projection are fixed; `crossSubsystemOwner: integration-review` |
| 430 | `Main.WorldPath` | proposed `WorldStoragePathPort` | Validate and normalize through path adapter; no core component field |
| 431 | `Main.CloudWorldPath` | proposed `CloudWorldStoragePort` | Keep cloud provider and availability outside ECS state; test offline/error behavior |
| 432 | `Main.PlayerPath` | proposed `PlayerStoragePathPort` | Keep player path in persistence adapter; no world snapshot field |

Checkpoint 6 guard: no `WorldFileData`, `PlayerFileData`, mutable catalog list, filesystem path root or cloud provider object may cross the proposed core component boundary. Readiness must never become Ready while the generation/loading barrier is active.

## 10. Checkpoint 7: HostLoadAndShutdownBoundary

| Seq | Source member | Proposed role/target | Migration boundary |
| ---: | --- | --- | --- |
| 463 | `Main._windowMover` | proposed `WindowHostAdapter` | Keep `WindowStateController` platform-owned; expose typed window capability only |
| 464 | `Main.sittingManager` | proposed `AnchoredEntityRelationAdapter.Sitting` | Keep collection ownership and cleanup in relation adapter; do not expose third-party collection in core state |
| 465 | `Main.sleepingManager` | proposed `AnchoredEntityRelationAdapter.Sleeping` | Preserve anchor clear/update ordering with frame activity; final owner `crossSubsystemOwner: integration-review` |
| 466 | `Main.oldStatusText` | proposed `RuntimeLoadProgressProjection.StatusText` | Keep de-duplication as presentation cache; do not make it readiness authority |
| 467 | `Main.autoGenFileLocation` | proposed `AutoGenerationStorageRequest` | Consume through validated persistence adapter; clear on success/failure; no world rule field |
| 468 | `Main.autoShutdown` | proposed `ShutdownPolicy` and `ShutdownRequestCommand` | Preserve host/CLI semantics; route actual exit effect through one host system |
| 469 | `Main.previousExecutionState` | proposed `PlatformPowerStateAdapter` | Preserve Windows sleep-token restoration; no ECS field |
| 3137 | `InitData.MaxNPCs` | proposed immutable `HostCapacityDefinition.MaxNpcs` | Keep capacity definition separate from NPC entity state; integration review owns cross-domain use |
| 3382 | `Program.IsXna` | proposed `RuntimePlatformCapabilities.IsXna` | Detect at bootstrap; expose typed capability only |
| 3383 | `Program.IsMono` | proposed `RuntimePlatformCapabilities.IsMono` | Detect at bootstrap; no world/session persistence |
| 3384 | `Program.LaunchParameters` | proposed `LaunchParameterAdapter` | Validate and freeze typed values; raw mutable dictionary stays outside core |
| 3385 | `Program.SavePath` | proposed `UserStoragePathPort` | Keep path service ownership and validation at adapter boundary |
| 3386 | `Program.TerrariaSaveFolderPath` | proposed immutable `UserStoragePathDefinition` | Preserve constant without mutable component registration |
| 3387 | `Program.ThingsToLoad` | proposed `RuntimeLoadProgressComponent.TotalWorkUnits` | One `RuntimeLoadProgressSystem` writer; progress projection only |
| 3388 | `Program.ThingsLoaded` | proposed `RuntimeLoadProgressComponent.CompletedWorkUnits` | Preserve monotonic progress/reset-on-failure semantics |
| 3389 | `Program.LoadedEverything` | proposed `RuntimeLoadProgressComponent.IsComplete` | Keep separate from world readiness and generation barrier |
| 3390 | `Program.JitForcedMethodCache` | proposed `JitRuntimeAdapter` | Keep `IntPtr`/JIT cache outside ECS and persistence |
| 3428 | `Ref<T>.Value` | proposed `ReferenceBridgeAdapter<T>` | Preserve aliasing/reference semantics; do not treat generic mutable reference as component data |
| 3666 | `WindowsLaunch._handleRoutine` | proposed `ShutdownSignalAdapter` | Register/unregister callback at host lifecycle boundary; callback emits command only |
| 3911 | `Terraria.Server.Game.Components` | proposed `ServerFrameworkAdapter.ComponentServices` | Third-party collection remains framework-owned |
| 3912 | `Terraria.Server.Game.Content` | proposed `ServerContentAdapter` | Keep `ContentManager` outside core state; test unavailable-content path |
| 3913 | `Terraria.Server.Game.GraphicsDevice` | proposed `ServerGraphicsAdapter` | Expose capability result, never a graphics object in simulation |
| 3914 | `Terraria.Server.Game.InactiveSleepTime` | proposed `HostTimingPolicy.InactiveSleepTime` | Keep host sleep policy separate from world clock |
| 3915 | `Terraria.Server.Game.IsActive` | proposed pure `HostLifecycleQuery.IsActive` | Query host lifecycle only; do not map to session readiness |
| 3916 | `Terraria.Server.Game.IsFixedTimeStep` | proposed `HostTimingPolicy.IsFixedTimeStep` | Preserve host timing semantics separately from simulation tick ordering |
| 3917 | `Terraria.Server.Game.IsMouseVisible` | proposed `WindowPresentationAdapter.IsMouseVisible` | Client/window adapter owns mutable visibility |
| 3918 | `Terraria.Server.Game.LaunchParameters` | proposed `ServerLaunchParameterAdapter` | Typed server bootstrap view; framework object stays outside core |
| 3919 | `Terraria.Server.Game.Services` | proposed `ServerServiceContainerAdapter` | Third-party service container stays host-owned |
| 3920 | `Terraria.Server.Game.TargetElapsedTime` | proposed `HostTimingPolicy.TargetElapsedTime` | Host scheduler input only; no world clock substitution |
| 3921 | `Terraria.Server.Game.Window` | proposed `WindowHostAdapter.Window` | Platform window view only; no ECS component field |

Checkpoint 7 guard: no proposed core component may reference `WindowStateController`, `AnchoredEntitiesCollection`, `ContentManager`, `GraphicsDevice`, `GameServiceContainer`, `GameWindow`, `HandlerRoutine`, `IntPtr`, `Ref<T>` or raw launch dictionaries. The adapter implementations are saved in `src2`; production host wiring remains deferred.

## 11. Checkpoint 8: DerivedWorldSessionQueriesAndDifficultyDefinitions

| Seq | Source member | Proposed role/target | Migration boundary |
| ---: | --- | --- | --- |
| 513 | `Main.SavePath` | proposed read-only `WorldSessionQuery.SavePath` | Source from validated storage port; no component writer |
| 514 | `Main.GameMode` | proposed `WorldSessionRuleQuery.GameMode` over active metadata | Replace setter with validated rule command and one commit owner; `crossSubsystemOwner: integration-review` |
| 515 | `Main.IsJourneyMode` | proposed pure `WorldSessionRuleQuery.IsJourneyMode` | Derive from effective GameMode; no cache required |
| 516 | `Main.NoFunctionalSurface` | proposed pure `WorldSurfaceQuery.NoFunctionalSurface` | Derive from world-surface input; do not make a mutable rule field |
| 517 | `Main.surviveHardcoreDeath` | proposed pure `WorldSessionRuleQuery.SurviveHardcoreDeath` | Truth-table verifier for seed flag precedence |
| 518 | `Main.onlyShimmerOceanWorlds` | proposed pure `WorldSessionRuleQuery.OnlyShimmerOceanWorlds` | Reuse immutable seed snapshot input; no second seed writer |
| 519 | `Main.masterMode` | proposed pure `DifficultyQuery.IsMasterOrAbove` | Derive from effective difficulty |
| 520 | `Main.expertMode` | proposed pure `DifficultyQuery.IsExpertOrAbove` | Derive from effective difficulty |
| 521 | `Main.Difficulty` | proposed pure `DifficultyQuery.EffectiveDifficulty` | Preserve override -> mode -> good-world ordering; verify boundary cases |
| 522 | `Main.Achievements` | proposed `AchievementServiceAdapter` | Keep external manager ownership and service lifecycle |
| 523 | `Main.UnpausedUpdateSeed` | proposed frame seed projection/cache | One frame-system writer; no persistence or external-ID reuse; `crossSubsystemOwner: integration-review` |
| 527 | `Main.GameUpdateCount` | proposed read-only `FrameSequenceQuery` | Preserve main-loop increment point; diagnostics only |
| 528 | `Main.worldID` | proposed typed `PersistentWorldIdQuery` | Keep distinct from ECS entity, network and external IDs; metadata owner unresolved; `crossSubsystemOwner: integration-review` |
| 529 | `Main.isThereAWorldSurface` | proposed pure `WorldSurfaceQuery.HasFunctionalSurface` | Derive from surface geometry |
| 530 | `Main.UnderworldLayer` | proposed pure `WorldGeometryQuery.UnderworldLayer` | Derive from world dimensions; no cached authoritative field |
| 532 | `Main.SceneMetrics` | proposed `SceneMetricsAdapter`/query projection | Keep camera/player metrics outside authoritative core |
| 535 | `Main.LocalPlayer` | proposed `LocalPlayerProjection` | Client entity selection projection; no world persistence |
| 536 | `Main.npcShop` | proposed `NpcShopSessionState` | Client interaction state; open/close owner remains UI/interaction boundary |
| 537 | `Main.playerPathName` | proposed `PlayerPathProjection` | Derive from selected player metadata; no component field |
| 538 | `Main.worldPathName` | proposed `WorldPathProjection` | Derive from selected world metadata; no component field |
| 539 | `Main.IsItAHappyWindyDay` | proposed pure `WeatherPresentationQuery.IsHappyWindyDay` | Weather owner supplies committed input; `crossSubsystemOwner: integration-review` |
| 540 | `Main.IsItStorming` | proposed pure `WeatherPresentationQuery.IsStorming` | Weather owner supplies committed input; `crossSubsystemOwner: integration-review` |
| 1169 | `LinearCurve.Key.input` | proposed immutable `DifficultyCurveKey.Input` | Preserve key values and interpolation order |
| 1170 | `LinearCurve.Key.output` | proposed immutable `DifficultyCurveKey.Output` | Preserve key values and interpolation order |
| 1171 | `LinearCurve.keys` | proposed immutable `DifficultyCurve.Keys` | Definition construction only; no runtime mutation |
| 1172 | `EnemyMaxLifeMultiplier` | proposed immutable difficulty rule definition | Add sampling verifier at Journey/Classic/Expert/Master/Legendary boundaries |
| 1173 | `EnemyDamageMultiplier` | proposed immutable difficulty rule definition | Preserve all key points including Legendary extension |
| 1174 | `HostileProjectileDamageMultiplier` | proposed immutable difficulty rule definition | Preserve interpolation and endpoint behavior |
| 1175 | `KnockbackToEnemiesMultiplier` | proposed immutable difficulty rule definition | Preserve decreasing curve behavior |
| 1176 | `EnemyMoneyDropMultiplier` | proposed immutable difficulty rule definition | Preserve duplicate output plateau at Expert/Master |
| 1177 | `TownNPCDamageMultiplier` | proposed immutable difficulty rule definition | Preserve Journey-to-Classic segment |
| 1178 | `DebuffTimeMultiplier` | proposed immutable difficulty rule definition | Preserve Classic/Expert/Master keys |
| 1179 | `LightningPlayerDamageScaling` | proposed immutable difficulty rule definition | Preserve fractional outputs and endpoints |
| 1180 | `GameDifficultyLevel.Journey` | proposed immutable `DifficultyLevelDefinition.Journey` | Constant definition only |
| 1181 | `GameDifficultyLevel.Classic` | proposed immutable `DifficultyLevelDefinition.Classic` | Constant definition only |
| 1182 | `GameDifficultyLevel.Expert` | proposed immutable `DifficultyLevelDefinition.Expert` | Constant definition only |
| 1183 | `GameDifficultyLevel.Master` | proposed immutable `DifficultyLevelDefinition.Master` | Constant definition only |
| 1184 | `GameDifficultyLevel.Legendary` | proposed immutable `DifficultyLevelDefinition.Legendary` | Constant definition only |

Checkpoint 8 guard: no proposed component may cache all 22 Main-derived properties or the 16 difficulty-definition members. Queries must be pure, definitions immutable, client/service projections one-way, and persistent/network IDs must remain distinct.

## 12. Checkpoint 9: WorldSessionPersistenceAndReplicationBoundary

This implementation checkpoint adds the smallest typed boundary needed by the existing P01 contract. `WorldSessionCommittedSnapshot` carries active-world metadata, committed secret-seed flags and a read-only Hardmode snapshot. `WorldPersistenceAdapter` accepts only the typed snapshot and exposes an explicit rollback operation; it does not perform production file I/O. `WorldSessionReplicationProjection` is a downstream pass-through projection and does not close Version4 WorldData packet 7 client recovery.

Saved files:

- `src2/WorldSession/Runtime/WorldSessionCommittedSnapshot.cs`
- `src2/WorldSession/Runtime/WorldPersistenceAdapter.cs`
- `src2/WorldSession/Runtime/WorldPersistenceResult.cs`
- `src2/WorldSession/Runtime/WorldSessionReplicationProjection.cs`
- `src2/WorldSessionVerification/Program.cs` includes commit, projection and rollback assertions.

The persistence/network direction remains `committed state -> typed snapshot -> adapter/projection`; no incoming wire DTO is treated as authoritative state. Production file format, retry semantics, packet 7 byte compatibility, client case 7 restoration, and cross-partition snapshot ownership remain `crossSubsystemOwner: integration-review` evidence gaps.

## 13. Execution-Side Contract Handoff

Component contracts are implemented as state-only types with one owning system per transient state. System contracts expose reset, import, update, consume, commit and cleanup transitions explicitly. Query contracts are pure and accept typed snapshots or immutable inputs. Command contracts represent one-shot intent and are consumed at named boundaries. Adapter contracts isolate host, platform, file, persistence, randomness, time, service and framework types. Projection contracts are one-way outputs and do not become state owners.

The P01 execution mapping covers all 149 input members: `1-4`; `7-9`; `18-37`; `48-60`; `165-173`; `251-254`; `422-432`; `463-473`; `513-523`, `527-530`, `532`, `535-540`; `1169-1184`; `2423`; `3137`; `3382-3390`; `3428`; `3666`; `3857-3868`; and `3911-3921`. The range expansions for `18-21`, `23-35`, `56-58` and `59-60` are explicit above. The independent coverage audit parsed the source report and matched the expected 149-member sequence set exactly.

## 14. Verification Record

The following commands were run serially after confirming that no active `dotnet.exe`/`csc.exe` build process owned the checkout:

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 `
  -DotnetArguments @(
    'build',
    '.\src2\WorldSessionVerification\Terraria.WorldSessionVerification.csproj',
    '-m:1',
    '-nr:false',
    '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false'
  )

& .\Build\Tools\Invoke-SerialDotnet.ps1 `
  -DotnetArguments @(
    'run',
    '--project',
    '.\src2\WorldSessionVerification\Terraria.WorldSessionVerification.csproj',
    '--no-build',
    '--no-restore',
    '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false'
  )
```

Recorded evidence: the build exited 0 with 0 warnings and 0 errors; `Terraria.WorldSession.dll` and `Terraria.WorldSessionVerification.dll` were confirmed under `Build/bin/`; the no-build verifier exited 0 and printed `P01 verifier passed.` The independent 149-member coverage audit matched the expected sequence set exactly. `verificationStatus: independently-verified` records only this src2-focused evidence; it does not claim production integration, full Version4 behavior equivalence, packet 7 closure, or production persistence.

## 15. Integration Handoff

1. Confirm the unique owners for `WorldSession` root, Hardmode transaction, FrameActivity aggregation, persistent World ID, snapshot value objects and packet 7 recovery.
2. Preserve the proposed typed boundaries until Version4 file ordering, retry/failure behavior, network field ordering and client case 7 restoration have focused evidence.
3. Reconcile P01's candidate system order with entity update, `UpdateTime`, WorldGen/invasion, server update, persistence commit and network send slots.
4. Keep `src` unchanged; any production integration requires a separate authorized change and a new verification record.

The runner settlement completed with the current manual session ID after both checkpoint documents passed binding and status consistency checks: `status=completed`, `exitCode=0`, `lockReleased=true`.
