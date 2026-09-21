# Version4 P04 玩家身份、生命周期与世界交互组件执行计划

partitionId: P04
sessionId: 5230602080084a98a9b991b67e88416d
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P04-Player-Lifecycle-Interaction.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md
executionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P04-player-lifecycle-interaction-component-execution.md
executionStatus: in-progress
implementationStatus: in-progress
verificationStatus: partial
evidenceStatus: partial
currentNltxStatus: partial
completedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13]
currentComponent: ""
pendingComponents: []
lastCheckpointUtc: 2026-09-12T08:38:48.0851915Z
evidence-gap: C01-C12 的允许范围源码已落地，其中 C06、C11、C12 仍为 partial；C09/C13 按证据不创建 ECS component。C06 的 chest relation 与 P09 已有 PlayerContainerRelationComponent owner 冲突，C13 的 P11 owner、随机源注入、表现帧消费和跨客户端复现策略仍未闭合；本轮未实现 projection 或随机 adapter。
blocking-decision: integration-review

## 1. 执行边界

本文件是 P04 组件源码实施跟踪。只允许落地文档已明确的 ECS 组件类型、字段、默认值和必要兼容声明；禁止修改 csproj、测试、Version4、权威报告、ledger 或新增 System/Query/Command/Adapter/Projection。当前组件源码已保存，编译验证待本次收尾阶段串行执行。

全局不变量：

- 一个权威状态只能有一个 owner writer；兼容 Adapter 可以读旧字段，但禁止 dual-write。
- Query 只读并返回值/快照；Projection 只向外输出；Adapter 负责外部类型、文件和网络效果。
- 时间、随机数、ID、Tile/Projectile/TileEntity registry、日志、音频、网络和持久化通过显式 port 或 adapter 进入 effect boundary。
- 每个检查点完成后先同步 design/execution 两份文档，再进入下一个检查点。
- 文件路径和顺序不定义运行时执行顺序，必须由 scheduler contract 和 verifier 固定。

## 2. 实施检查点

| 检查点 | proposed 边界 | 前置证据 | focused verifier |
|---|---|---|---|
| C01 | 身份、活动、死亡记录 | `KillMe`/`Spawn`/network death fields | death record transition、唯一写者、DateTime port |
| C02 | 交互/效果与外部定义 adapter | `ResetEffects`、Minecart/Creative/Overhead/Builder 调用 | effect reset、外部依赖隔离、静态定义只读 |
| C03 | 传送事务 | `Teleport`、`UpdateTeleportVisuals`、ack input | stage transition、ack/replay、random/effect ordering |
| C04 | 死亡/复活/保存交接 | `UpdateDead`、`KillMe`、save marker | dead/respawn/spectating transitions、save unknown |
| C05 | 生成/返回路由 | `Spawn`、Potion of Return network | spawn selection、route clear、network round-trip |
| C06 | 容器/世界锚点 | `PlayerInteractionAnchor`、tracked projectile、chest sync | relation identity、clear/recovery、serialization adapter |
| C07 | Portal/targeting/cache | portal visual state、minion/projectile caches | cache reset、target invalidation、cross-owner handoff |
| C08 | Item action timing | `SetItemTime`、ItemCheck、Update | timer invariants、repeat/release、Wiring/Combat seam |
| C09 | ItemCheck command payload | `ItemCheckContext` local construction | no persistence/no mutation by Query, skip-consumption behavior |
| C10 | Petting relation | `PlayerPettingInfo.TryGetTarget`、update/stop | entity reference validity、input interruption、network flag |
| C11 | Sitting relation | `PlayerSittingHelper` and tile query | chair range, stack index, exit broadcast, pure query |
| C12 | Sleeping relation | `PlayerSleepingHelper` and 120 tick property | bed validity, fall-asleep threshold, rotation/reset, broadcast |
| C13 | Rabbit frame projection | `RabbitOrderFrameHelper` | seeded random, frame transition, no authoritative persistence |

## 3. Planned implementation sequence

1. For a checkpoint, first add or update a focused verifier and a compatibility read adapter without changing the legacy writer.
2. Define the smallest domain-first proposed type. Use one public core type per PascalCase file, place it in `src/Player`, `src/Teleportation`, `src/Items`, `src/WorldInteraction`, `src/Combat` or `src/Client` only when that domain owns the behavior; do not create `Shared/Components/`, `Common/`, `Misc/` or empty technical folders.
3. Route one legacy writer through the owner System/CommitPort. Keep old public names/namespaces and external types at the Adapter boundary until source, network and persistence evidence is closed.
4. Verify state transition, initialization/clear, error/timeout/unknown result, network projection, persistence projection, and cross-subsystem handoff before removing the old writer.
5. Record a serial build/test command only during an authorized implementation session, after checking active `dotnet.exe`/`csc.exe`; use `Build/Tools/Invoke-SerialDotnet.ps1` from repository root and put artifacts under `Build/bin/`.

## 4. Proposed path and namespace policy

| Domain | Proposed path examples | Proposed namespace | Boundary rule |
|---|---|---|---|
| Player lifecycle | `src/Player/PlayerLifecycleComponent.cs`, `PlayerDeathRecordComponent.cs`, `PlayerSpawnPointComponent.cs` | `Terraria.Player` | Player state only; death drops/Combat effects through command/adapter |
| Teleport | `src/Teleportation/PlayerTeleportTransitionComponent.cs` | `Terraria.Teleportation` | transition facts only; portal/pylon effects through ports |
| Player interaction | `src/Player/PlayerPettingComponent.cs`, `PlayerRestComponent.cs` | `Terraria.Player` | relation/phase facts; Tile and external entity queries remain adapters |
| Containers | `src/Player/PlayerContainerRelationComponent.cs` | `Terraria.Player` | player-to-container relation only; Item/Chest contents owned by Items/WorldStorage integration |
| Item action | `src/Player/PlayerItemActionTimingComponent.cs` | `Terraria.Player` | timing facts only; use effects and Wiring cooldown cross-owner |
| Presentation | `src/Player/PlayerRabbitFrameProjection.cs` or P11-owned path after review | `Terraria.Player` or P11 namespace | no authority, save, or network writer without evidence |

These are proposed paths only. Moving or adding code later requires recording source/target paths, dependency impact, namespace/API compatibility, and rollback conditions.

## 5. Compatibility and side-effect policy

- Network input is decoded by `MessageBuffer` adapters into commands; it must not directly assign multiple Components outside the owner system.
- Network output and save output consume committed snapshots. The Version4 save implementation/format is not established by this design, so no byte order or version is invented.
- `DateTime.Now` becomes an injected clock; `Main.rand` becomes an injected random source; particle, sound, chat, achievement, TileEntity, Projectile, shop, and `NetMessage` calls are explicit effect ports.
- External calls record read/write, ordering, failure/timeout-unknown, cancellation, retry and duplicate semantics. No log callback may drive business state.
- During a compatibility window, adapters may translate old fields to proposed snapshots, but there is no simultaneous old/new write path.

## 6. Verification command plan

编译验证在组件源码收尾阶段串行执行。每次执行前必须检查 active `dotnet.exe`/`csc.exe`，并按以下形状逐项目运行：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\src\<affected-project>\<affected-project>.csproj `
  -m:1 -nr:false -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false
```

After a successful build, verify the expected artifact under `Build/bin/`; run focused verifiers with `--no-build --no-restore`, and record project, exact command, exit code, warning/error counts and artifact path. The current aggregate `verificationStatus` is `partial`; checkpoint-level behavior verification remains `not-run` where no focused P04 verifier exists.

## 7. Rollback and completion conditions

- Roll back a checkpoint if a focused verifier finds changed death/respawn/teleport/rest/item-timing behavior, a second writer, an invalid entity relation, changed network bytes, unknown persistence semantics, or an effect ordering regression.
- Leave the compatibility Adapter in place when persistence/network/owner evidence is incomplete; do not force completion by guessing a format or moving a cross-partition owner.
- A checkpoint is implementation-complete only when its allowed component source is saved, its fields and dependency gaps are recorded, and its verification status is truthful.
- The overall session is complete only after both Markdown files exist, allowed component source is saved, verification evidence and remaining gaps are recorded, and the runner is settled with the current `partitionId` and `sessionId`.

## 8. Checkpoint log

### C01 `PlayerIdentityAndDeathRecord`

- Design boundary: `PlayerIdentityComponent` plus `PlayerDeathRecordComponent`, with death penalty/network/persistence adapters at explicit seams.
- Source/target plan: retain `Terraria.Player` legacy fields through compatibility projection; proposed types belong under `src/Player/` and keep one core public type per file.
- Dependency impact: `Combat`/`DeathPenaltyAndRevenge` owns death resolution and drop effects; `WorldStorage`/`ExternalBoundaries` owns persistence/replication format; P04 does not announce either owner.
- Rollback: if death counts, lost coins, last-death record, or active/host transitions diverge, keep legacy writer and remove only the new read adapter.
- Implementation: `implemented`; verification: `not-run`.
- Actual source: `src/Player/PlayerIdentityComponent.cs`, `src/Player/PlayerDeathRecordComponent.cs`.
- Fields saved: `IsActive`, `DisplayName`, `LostCoins`, `LostCoinText`, `PveDeathCount`, `PvpDeathCount`, `LastDeathPosition`, `LastDeathTime`, `ShowLastDeath`.
- Dependency gap: `PlayerLifecycleComponent` still contains overlapping legacy death fields; no lifecycle writer, clock, DeathPenalty, network or persistence adapter was added in this component-only checkpoint.

#### C02 implementation checkpoint

- Actual source: `src/Player/PlayerRuntimeInteractionComponent.cs`.
- Fields saved: `EmoteRemainingTicks`, `SpelunkerRemainingTicks`, `BuilderToggleStatuses` initialized to `PlayerBuilderInteractionCatalog.Count`.
- Dependency gap: CreativeUnlocksTracker, OverheadMessage, Minecart/Grapple, soulDrain, DD2, basilisk and Combat tuning remain external/cross-partition boundaries; no adapter, query or system was added.
- Implementation: `implemented`; verification: `not-run`.

#### C03 implementation checkpoint

- Actual source: `src/Teleportation/PlayerTeleportTransitionComponent.cs`.
- Fields saved: `IsTeleporting`.
- Dependency gap: `teleportTime`/`teleportStyle` remain visual projection state, and `unacknowledgedTeleports` remains network acknowledgement adapter state; no movement, particle, random, network or system code was added.
- Implementation: `implemented`; verification: `not-run`.

#### C04 implementation checkpoint

- Actual source: `src/Player/PlayerDeathRespawnComponent.cs`, `src/Player/PlayerSaveCheckpointComponent.cs`.
- Fields saved: `IsDead`, `DeadElapsedTicks`, `SpectatingTarget`, `RespawnRemainingTicks`, `LastSavedBinaryTimestamp`.
- Dependency gap: `PlayerLifecycleComponent` remains the existing compatibility aggregate; rules, death effects, Combat/Items handoff, HitTile scratch, spectating registry, clock and persistence I/O were not implemented in components.
- Implementation: `implemented`; verification: `not-run`.

#### C05 implementation checkpoint

- Actual source: existing `src/Player/PlayerSpawnPointComponent.cs` updated in place.
- Fields/derived state saved: existing `SpawnX`, `SpawnY`, `ReturnOriginalUsePosition`, `ReturnHomePosition`, plus `HasSpawnCoordinates` and `HasReturnRoute` for paired reads.
- Dependency gap: no `PlayerReturnRouteComponent` was added; spawn selection, type 12/13 network adapters, Potion of Return writer, persistence and C04/C03 handoff remain external.
- Implementation: `implemented`; verification: `not-run`.

#### C06 implementation checkpoint

- Actual source: `src/Player/PlayerTrackedContainerLinkComponent.cs`, `src/Player/PlayerTileInteractionCapabilityComponent.cs`, `src/WorldInteraction/PlayerTileEntityAnchorComponent.cs`. `PlayerContainerRelationComponent.cs` was not modified because P09 already owns its bank/void-vault fields.
- Fields saved: independent Piggy Bank/Void Lens tracking flags, local slots, owner slots, identities and types; three tile capability flags; TileEntity reference, tile coordinate and `HasAnchor`. P04 chest current/previous fields remain unresolved across the P09 shared owner boundary.
- Dependency gap: no scratch, container contents, Projectile/TileEntity registry, network adapter, shop/door/eye projection or cross-domain writer was added; C10-C12 petting/sitting/sleeping are not duplicated.
- Implementation: `partial`; verification: `not-run`.

#### C07 implementation checkpoint

- Actual source: `src/Teleportation/PlayerPortalTraversalComponent.cs`, `src/Combat/PlayerMinionTargetComponent.cs`.
- Fields saved: `LastPortalColorIndex`, `PortalPhysicsRemainingTicks`, `PortalPhysicsRequested`, `LastPylonStyle`, `RestTargetPoint`, `AttackTarget` and `HasRestTarget`.
- Dependency gap: projectile/NPC caches, Fishron/mount handoff, grapple blacklist, strong-bee random effect, NPC registry validation and type 99/115 network projection were not implemented.
- Implementation: `implemented`; verification: `not-run`.

#### C08 implementation checkpoint

- Actual source: `src/Player/PlayerItemActionTimingComponent.cs`.
- Fields saved: `AnimationRemaining`, `AnimationDuration`, `UseRemaining`, `UseDuration`, `ToolUseMarker`.
- Dependency gap: existing `PlayerUseComponent` overlap, delay definition, Wiring cooldown, fall tracking, global projectile block, ItemCheck writer and C04 attackCD handoff were not connected in this component-only scope.
- Implementation: `implemented`; verification: `not-run`.

#### C09 implementation checkpoint

- Actual source: none; the evidence requires no ECS component.
- Confirmed field: `SkipItemConsumption` remains a short-lived `ItemCheckContext` payload with default `false`; it is not copied into a Player entity, save or network snapshot.
- Dependency gap: no confirmed read/write/consume site exists; Items consumption, mod/hook input, failure and duplicate semantics require evidence closure before any non-component adapter is considered.
- Implementation: `deferred-no-component`; verification: `not-run`.

#### C10 implementation checkpoint

- Actual source: `src/Player/PlayerPettingComponent.cs`.
- Fields saved: `IsPetting`, NPC/projectile slots and expected types, `MountId`, `IsMountTarget`, `OffsetFromPet`, `IsPetSmall`.
- Dependency gap: no `PlayerPetTargetReference`, registry query, network adapter or vanity system was added; slot reuse, target identity, active/type validation and Mount validity remain external owner gaps.
- Implementation: `implemented`; verification: `not-run`.

#### C11 implementation checkpoint

- Actual source: `src/Player/PlayerSittingComponent.cs`.
- Fields saved: `IsSitting`, `SeatFeatures`, `SeatOffset`, `StackIndex`; `StackIndex` defaults to `-1`.
- Dependency gap: Version4 `ExtraSeatInfo` is absent from current src; the component uses existing `RestSeatFeatures` only for the confirmed toilet flag. No seat query/stack/network adapter was added, and the existing `PlayerRestComponent` writer was not connected.
- Implementation: `partial`; verification: `not-run`.

#### C12 implementation checkpoint

- Actual source: `src/Player/PlayerSleepingComponent.cs`.
- Fields saved: `IsSleeping`, `StackIndex`, `TimeSleeping`, `BedVisualOffset`; `StackIndex` defaults to `-1`.
- Dependency gap: `SetOffsetbyBed` remains a Version4 stub; no bed eligibility, act-up predicate, rotation, sleepingManager, network/presentation adapter or `FullyFallenAsleep` query was implemented. C11/C12 unique writer ownership remains integration review.
- Implementation: `partial`; verification: `not-run`.

#### C13 implementation checkpoint

- Actual source: none; C13 is projection-only and this task permits component source only, so no `PlayerRabbitOrderFrameProjection` or `RabbitOrderFrameStateDefinition` was added.
- Confirmed boundary: DisplayFrame, frame counter/state, random selection, rendering consumer and Spawn reset remain P11/Client integration review; no ECS authority was invented.
- Dependency gap: P11 owner, random source, frame consumer, head-259 gate and cross-client determinism remain unresolved.
- Implementation: `deferred-no-component`; verification: `not-run`.

以上 checkpoint log 是当前实施状态的准据；下方 C02-C13 保留原始迁移设计和验证计划，不能覆盖上方的实际源码状态。当前仍不作行为等价、网络闭合或持久化闭合声明。

### C02 `PlayerRuntimeInteractionAndEffect`

- Proposed files: `src/Player/PlayerRuntimeInteractionComponent.cs`, `PlayerCreativeProgressionAdapter.cs`, `PlayerOverheadMessageProjection.cs`, `PlayerRuntimeEffectQuery.cs`; Combat tuning remains in the Combat domain after owner review.
- Migration order: characterize each field's writer; add immutable read snapshots; route local timers and builder toggle writes through one Player owner; add adapters for Creative/Chat/Mount/Grapple/Combat/DD2; remove no legacy field in the same batch.
- Dependency impact: `MinecartSettings`, `GoingDownWithGrapple`, `soulDrain`, `dd2Accessory`, `basiliskCharge` cross P03/P06/P07/P13; P04 sends the handoff only and does not modify those partitions.
- Side effects: content tracker, chat parsing/font measurement, Combat effects and external registries remain explicit adapters; static tuning is queried as definition data.
- Rollback: if timer reset, builder status, emote projection or any external handoff changes, retain the old writer and discard only the new projection/query path.
- Implementation: component `implemented`; remaining adapters/queries/systems: `not-started`; verification: `not-run`.

下方继续保留 C03-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C03 `PlayerTeleportTransition`

- Proposed files: `src/Teleportation/PlayerTeleportTransitionComponent.cs`, `PlayerTeleportVisualProjection.cs`, `PlayerTeleportAcknowledgementAdapter.cs`, and `PlayerTeleportTransitionQuery.cs`; `PlayerTeleportTransitionComponent.cs` is implemented in the component-only scope, while the visual projection, acknowledgement adapter and query remain design-only.
- Migration order: add a read-only characterization/verifier for the Wiring guard, visual timer/style, and acknowledgement count; then introduce a single `TeleportCommitSystem` command boundary while retaining the legacy writer; only after protocol evidence is closed may the compatibility adapter replace legacy fields.
- Member ownership: `teleporting` is a per-Wiring-iteration duplicate guard; `teleportTime` and `teleportStyle` are visual projection state; `unacknowledgedTeleports` belongs to a network acknowledgement adapter. No Query writes any of the three categories.
- Side effects: movement/section updates, portal and pylon effects, random dust, audio, network type 65 output, and type 3 input are explicit ports/adapters. Failed or unknown network output must not be treated as an acknowledgement.
- Dependency impact: Wiring, Portal/Pylon, Movement, Network and P11 Presentation require integration review; P04 does not claim their final owner or invent transaction IDs, retry, timeout, persistence, or wire-format semantics.
- Rollback: if duplicate teleport, visual countdown, position freeze, acknowledgement, or effect ordering changes, retain the legacy writer and remove only the proposed read/projection adapter.
- Focused verification: single-commit/guard reset, style-specific visual transitions, ack duplicate/late/wrong-player handling, and cross-partition unique-writer checks. Component implementation is saved; focused verification remains `not-run`.

下方继续保留 C04-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C04 `PlayerDeathRespawnAndSave`

- Proposed files: `src/Player/PlayerDeathRespawnComponent.cs`, `PlayerDeathRespawnRulesDefinition.cs`, `PlayerSaveCheckpointComponent.cs`, `PlayerSpectatingQuery.cs`, plus explicit Combat/WorldInteraction adapters; the two component files are implemented in the component-only scope, while rules, query and adapters remain design-only.
- Migration order: characterize KillMe/UpdateDead/Spawn/Ghost/Spectate transitions and save timestamp projection first; route one lifecycle writer through a command system while preserving C01 death-record and C05 spawn boundaries; remove no legacy field until network/save/owner evidence is closed.
- Member ownership: dead/deadTime/respawnTimer belong to lifecycle state; spectating is a player-slot relationship through an adapter; three timing constants are definitions; lastTimePlayerWasSaved is a persistence marker; attackCD, potionDelay, difficulty, wetSlime, hitTile and hitReplace remain explicit handoffs/scratch seams.
- Side effects: death drops, tombstones, chat/audio/particles, network type 150 and persistence I/O are ports. `DateTime.FromBinary` is retained at the adapter boundary, and no save writer or timestamp semantics are invented.
- Dependency impact: Combat/DeathPenalty, Items, Movement/Mount, WorldStorage and WorldInteraction require integration review. C04 does not claim final ownership of attackCD, difficulty, HitTile scratch, or C05 spawn coordinates.
- Rollback: if death idempotence, respawn countdown, spectating eligibility, save marker, or cross-owner timer ordering changes, retain the legacy lifecycle writer and discard only the proposed snapshot/adapter.
- Focused verification: death/respawn transition, spectating boundary, save checkpoint round-trip, unique timing writers, and isolated HitTile scratch. Component implementation is saved; focused verification remains `not-run`.

下方继续保留 C05-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C05 `PlayerSpawnAndReturn`

- Proposed boundary: reuse `src/Player/PlayerSpawnPointComponent.cs`/state as the compatibility model; add a separate return-route boundary only if field-level evidence proves it is needed. Do not create a second SpawnX/Y owner.
- Migration order: characterize spawn precedence and paired return-route semantics; add a pure `PlayerSpawnSelectionQuery`; route one spawn commit through a command system; keep C04 lifecycle, C03 teleport, and WorldGen/team fallback owners behind explicit seams.
- Member ownership: SpawnX and SpawnY are an atomic compatibility pair; the two Potion of Return positions are an atomic optional pair; no field is interpreted as a complete save model without persistence evidence.
- Side effects: tile validity, world/team spawn lookup, position/section updates, teleport effects, and type 12/type 13 network encoding are adapters/ports. Type 13 absence clears both return positions.
- Dependency impact: C04 dead/respawn, C03 Teleportation, WorldGen/team spawn, Network and WorldStorage require integration review. The current NLTX SpawnPoint model is existing evidence, not proof of Version4 persistence parity.
- Rollback: if spawn precedence, signed-short boundaries, route pairing, or lifecycle handoff changes, retain the legacy route writer and discard only the proposed query/adapter.
- Focused verification: spawn precedence, type 12 round-trip, paired return-route network behavior, and C04/C03/WorldGen unique-writer handoff. Component implementation is saved; focused verification remains `not-run`.

下方继续保留 C06-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C07 `PlayerPortalAndTargeting`

- Proposed files: `src/Projectile/PlayerOwnedProjectileCache.cs`, `src/NPC/PlayerNpcAggroSuppressionCache.cs`, `src/Teleportation/PlayerPortalTraversalComponent.cs`, `src/Combat/PlayerMinionTargetComponent.cs`, `src/Movement/PlayerGrapplingBlacklistScratch.cs`, and `src/Combat/PlayerBeeCombatEffectAdapter.cs`; the Portal and minion components are implemented in the component-only scope, while the caches, scratch and adapters remain design-only.
- Migration order: characterize reset/rebuild and target invalidation first; add immutable cache snapshots and explicit Portal/Movement/Minion commands; preserve legacy arrays and fields until registry/version/network evidence is closed.
- Member ownership: owned projectile counts and NPC no-aggro are per-tick caches; Portal color/physics/Pylon values are traversal metadata; MountFishron, Minion targets, grapple blacklist and strong-bee roll are explicit Mount/Projectile/NPC/Movement/Combat handoffs.
- Side effects: Projectile/NPC registry scans, type 99/115 network projections, Portal/Pylon visual effects, movement physics and injected random Combat rolls are adapters/ports. Queries never refresh caches or expose mutable collections.
- Dependency impact: C03 Teleportation, P03 Mount, P07/P12 Minion/Projectile, NPC targeting, Grapple/Movement and Combat require integration review; P04 does not claim their final owner.
- Rollback: if cache reset, Portal physics, target network behavior, grapple clearing or Combat randomization changes, retain the legacy writer and remove only proposed snapshots/adapters.
- Focused verification: cache rebuild/lifetime, aggro suppression, Portal handoff, minion protocol, and grapple/bee isolation. Component implementation is saved; focused verification remains `not-run`.

下方继续保留 C08-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C06 `PlayerContainerAndWorldAnchor`

- Proposed files: `src/Player/PlayerContainerRelationComponent.cs`, `PlayerTrackedContainerLinkComponent.cs`, `PlayerTileInteractionCapabilityComponent.cs`, `src/WorldInteraction/PlayerTileEntityAnchorComponent.cs`, `PlayerTileInteractionScratch.cs`, and explicit Shop/Door/Presentation adapters. P09 already owns the current `PlayerContainerRelationComponent` bank/void-vault fields, so P04 did not modify that shared type and its chest current/previous ownership remains unresolved. The tracked-link, tile-capability and TileEntity-anchor components are implemented; scratch and adapters remain design-only. `petting`, `sitting`, and `sleeping` are not duplicated here.
- Migration order: characterize current/previous chest transitions, projectile-link recovery, TileEntity anchor protocol, and per-tick tile scratch; introduce one relation writer per boundary while leaving Chest contents, Projectile registry, and TileEntity registry external.
- Member ownership: `chest` and `lastChest` are different current/previous relation facts; each tracked projectile reference is an independent owner/identity/type relation; TouchedTiles and capability flags are scratch/derived state; eye/shop/door/achievement are projections/adapters.
- Side effects: type 80 chest sync, type 142 projectile-link sync, type 122 TileEntity sync, registry lookups, collision/tile reads, shop calculation and presentation effects are explicit ports. No Query exposes mutable lists or external helper objects.
- Dependency impact: Items/WorldStorage, Projectile, TileEntity/WorldInteraction, P11 Presentation and C10-C12 require integration review. No chest contents or persistence bytes are assigned to P04 from this checkpoint.
- Rollback: if container transition effects, link recovery, anchor clear, per-tick scratch or capability derivation changes, retain the legacy writer and remove only the proposed snapshots/adapters.
- Focused verification: container relation, tracked projectile recovery, anchor protocol, scratch isolation, and deferred helper ownership. C06 remains `partial`; component verification remains `not-run`.

下方继续保留 C07-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C08 `PlayerItemActionTiming`

- Proposed files: `src/Player/PlayerItemActionTimingComponent.cs`, `src/Items/PlayerItemDelayDefinitionQuery.cs`, `src/WorldInteraction/PlayerWiringCooldownHandoff.cs`, `src/Movement/PlayerFallTrackingAdapter.cs`, and `src/Client/PlayerProjectileInteractionBlockAdapter.cs`; the timing component is implemented in the component-only scope, while the definition query and handoff adapters remain proposed.
- Migration order: add a focused characterization verifier for paired item timers and ItemCheck decrement/cleanup; add a read-only item-delay definition query; then introduce one Player timing command/commit seam while retaining legacy writers. Wiring cooldown, fall tracking and the static input block remain explicit handoffs until their writer/consumer closures are verified.
- Member ownership: `itemAnimation`/`itemAnimationMax` and `itemTime`/`itemTimeMax` are separate paired action states; `toolTime` is only a marker until its clear/reader closure is found; the three `*DelayTime` fields are rule-derived inputs; `wireOperationsCooldown` belongs at the Wiring handoff; `fallStart`/`fallStart2` belong at Movement/Minecart; static `BlockInteractionWithProjectiles` is client-global and is not a Player entity component.
- Compatibility: preserve `SetItemTime`, `SetDummyItemTime`, `SetItemAnimation`, ItemCheck negative-value normalization, animation-end max clearing, and the dummy `frames + 1` baseline. Do not create a second `attackCD` owner; C04 remains the existing boundary.
- Side effects: Item registry/effects, input release, network clear, Wiring operations, Movement/Collision/Minecart, Combat attack cooldown, and Main/client mouse state are explicit ports/adapters. `potionDelay` remains distinct from `potionDelayTime`; the current empty `ApplyPotionDelay` is recorded as an evidence gap.
- Dependency impact: Items/Combat, Wiring/WorldInteraction, Movement/Minecart, Network and Client/P11 require integration review. P04 does not claim the final owner of tool marker consumption, fall damage semantics, Wiring writer, or global input policy.
- Rollback: if repeat/release behavior, timer pairing, delay eligibility, Wiring lockout, Minecart fall input, or global block duration changes, retain legacy writers and remove only the proposed snapshot/query/adapters.
- Focused verification: paired timer transitions and reuse/release; delay-definition isolation; tool marker closure; Wiring tick/network handoff; fallStart/fallStart2 scheduler order; static block isolation; and unique-writer checks for C04 `attackCD`.

下方继续保留 C09-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C09 `PlayerItemCheckContext`

- Proposed files: `src/Items/PlayerItemCheckContext.cs` as a compatibility-facing command/context type, and a possible `src/Items/PlayerItemConsumptionDecision.cs` only after a real consumer is evidenced; no ECS persistent component is proposed.
- Migration order: first add a source-closure verifier and an immutable ItemCheck command snapshot; retain the existing public `Terraria.Player.ItemCheckContext` declaration and default construction; only route an evidenced consumer through an Items consumption commit port.
- Member ownership: `SkipItemConsumption` is one-call payload state. The current Version4 search finds the declaration and `default(ItemCheckContext)` construction but no read/write/consume site, so no owner writer or skip behavior is invented.
- Compatibility: default remains `false`; no carry-over between ItemCheck calls, save/network serialization, or Player entity copy. C08 timers and C04 `potionDelay` remain separate.
- Side effects: inventory stack changes, item consumption, mod/hook integration, network and persistence are explicit Items adapters/ports. Query code may calculate a decision but cannot mutate the context or inventory.
- Dependency impact: Items/Inventory, Player ItemCheck, ModHook/extension and Network/WorldStorage require integration review. P04 does not claim a final consumer until evidence identifies one.
- Rollback: if a future consumer changes skip/default, normal consumption, repeatability, or inventory failure semantics, keep the legacy local context and remove only the proposed adapter/decision layer.
- Focused verification: context lifetime, full read/write closure, query no-mutation, absence of save/network projection, and consumption boundary behavior once a consumer is found.

下方继续保留 C10-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C10 `PlayerPetting`

- Proposed files: `src/Player/PlayerPettingComponent.cs`, `src/Player/PlayerPetTargetReference.cs`, `src/Player/PlayerPettingTargetQuery.cs`, and `src/Player/PlayerPettingNetworkAdapter.cs`; the petting component is implemented in the component-only scope, while the target reference, query and network adapter remain proposed.
- Migration order: characterize the three target constructors and registry validation first; introduce a tagged relation snapshot while retaining `PlayerPettingInfo` as the compatibility adapter; route begin/stop/update through one Player vanity-interaction system; only then close target/network handoff semantics.
- Member ownership: `isPetting` is the interaction phase; `npc`, `proj`, `type`, and `mount` become a tagged target reference; `offsetFromPet` is relation geometry; `isPetSmall` is a network/presentation modifier. No raw slot is treated as a stable EntityId.
- Compatibility: preserve target-kind-specific type checks, Mount `TryGetTarget` null behavior, distance threshold, NPC `talkNPC` condition, input/pulley/mount/direction stop rules, and the fact that `StopPettingAnimal` currently clears only the phase.
- Side effects: NPC/Projectile/Mount registry access, type 13 bits 4/5, distance/collision, hold-style presentation, audio/particle effects and dismount broadcasts are explicit adapters/ports. Target relation wire format and identity version remain unresolved.
- Dependency impact: NPC, Projectile, Mount/Movement, Network, Client/P11 and C11/C12 rest/vanity scheduling require integration review. P04 does not claim their final owners.
- Rollback: if target validity, stop conditions, phase clearing, network flags or presentation offsets change, retain the legacy relation helper and remove only the proposed tagged snapshot/query/adapter.
- Focused verification: target-kind/identity validity, all stop conditions, phase lifecycle and stale-reference handling, type 13 projection, and one-way `isPetSmall` presentation.

下方继续保留 C11-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C11 `PlayerSitting`

- Proposed files: `src/Player/PlayerSittingComponent.cs`, `src/Player/PlayerSittingRulesDefinition.cs`, `src/WorldInteraction/PlayerSeatEligibilityQuery.cs`, `src/WorldInteraction/PlayerSeatStackAdapter.cs`, and `src/Player/PlayerSittingNetworkAdapter.cs`; the sitting component is implemented in the component-only scope, while the rules, query, stack adapter and network adapter remain proposed.
- Migration order: characterize tile/frame eligibility and stack-manager behavior; add a pure seat candidate query; retain `PlayerSittingHelper` as the compatibility adapter; route SitDown/SitUp through one rest owner only after the missing local `isSitting=true` writer and C12 mutual exclusion are evidenced.
- Member ownership: `ChairSittingMaxDistance` is immutable definition data; `isSitting`, `details`, `offsetForSeat`, and `sittingIndex` form one seat relation snapshot. `sittingIndex` remains an external manager index, not a stable EntityId.
- Compatibility: preserve tile/frame seat offsets, direction checks, invalid-target and input/pulley/mount exits, stack limit `>= 2`, SitUp reset values, toilet detail behavior, and type 13 bit 2 projection. Do not invent a local sit-down writer absent from current source.
- Side effects: Tile/Framing reads, `AnchoredEntitiesCollection`, position/render offsets, toilet gameplay, NetMessage broadcast and world reset are explicit ports/adapters.
- Dependency impact: WorldInteraction/Tile, NPC/toilet, Network, P11 Presentation and C12 rest scheduler require integration review. P04 does not claim the final entry protocol or stack-manager ownership.
- Rollback: if seat eligibility, offset/index arithmetic, exit conditions, reset/broadcast behavior or rest mutual exclusion changes, keep the legacy helper and remove only the proposed query/adapter.
- Focused verification: pure tile query, entry-writer closure, update/exit transitions, stack index lifecycle, type 13 projection, and C11/C12 unique-writer checks.

下方继续保留 C12-C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C12 `PlayerSleeping`

- Proposed files: `src/Player/PlayerSleepingComponent.cs`, `src/Player/PlayerSleepingRulesDefinition.cs`, `src/WorldInteraction/PlayerBedEligibilityQuery.cs`, `src/WorldInteraction/PlayerSleepingRestAdapter.cs`, and `src/Player/PlayerFullyFallenAsleepQuery.cs`; the sleeping component is implemented in the component-only scope, while the rules, bed query, rest adapter and fully-asleep query remain proposed.
- Migration order: characterize bed tile/frame eligibility, rotation, timers and sleepingManager behavior; add a pure bed candidate query; retain `PlayerSleepingHelper` as compatibility adapter; route sleep/stop through one rest scheduler only after the missing entry path, act-up predicate and bed offset implementation are evidenced.
- Member ownership: constants are definitions; `isSleeping`, `sleepingIndex`, `timeSleeping`, and `visualOffsetOfBedBase` form the sleep relation snapshot; `FullyFallenAsleep` is a read-only query. `sleepingIndex` is an external manager index.
- Compatibility: preserve 120-tick threshold, rotation/origin changes, invalid bed/input/pulley/mount/direction/item/stack exits, Stop cleanup, visual offset projection and type 13 bit 0. Treat `DoesPlayerHaveReasonToActUpInBed` and `SetOffsetbyBed` as source gaps, not as permission to invent behavior.
- Side effects: Tile/Framing reads, full rotation, sleepingManager, item interruption, Eye/WorldGen/Presentation reads, NetMessage broadcast and world reset are explicit ports/adapters.
- Dependency impact: WorldInteraction/Tile, Movement/Mount, Items, Network, Client/P11, WorldGen and C11 rest scheduler require integration review. P04 does not claim the final bed-entry or act-up owner.
- Rollback: if fall-asleep timing, rotation, bed offset, stack index, invalidation, network bit or C11/C12 mutex behavior changes, keep the legacy helper and remove only proposed query/adapter layers.
- Focused verification: bed query/stub evidence, sleep lifecycle and 120-tick boundary, rotation/offset reset, stack manager, network projection, and unique C11/C12 rest writers.

下方继续保留 C13 的原始迁移设计；实际实现状态以本节上方 checkpoint log 为准。

### C13 `PlayerRabbitOrderFrame`

- Proposed files: `src/Client/PlayerRabbitOrderFrameProjection.cs` or the P11 Presentation-owned equivalent, plus an optional `src/Client/RabbitOrderFrameStateDefinition.cs`; C13 remains projection-only and no component or projection was created in this component-only scope.
- Migration order: characterize frame ranges, counter thresholds, state transitions, head-259 gate and Spawn reset; introduce a projection snapshot with an injected random source; retain the legacy nested helper until P11 ownership and DisplayFrame consumption are verified.
- Member ownership: `DisplayFrame` is presentation output; `_frameCounter` and `_aiState` are private projection runtime state; the four `AIState_*` values are immutable definitions. None is a Player authority/save/network owner.
- Compatibility: preserve clamp ranges, Idle random bounds, state-specific frame delays, state transition to Idle, immediate Update after ChangeToAIState, head-259/skip gate, and Spawn reset behavior.
- Side effects: random selection, tick progression, rendering and reset invocation are explicit ports/adapters. No Item, NPC AI, persistence or network writer is assigned to C13.
- Dependency impact: P11/Client, Player frame rendering, random source and Spawn lifecycle require integration review. P04 does not claim the final presentation consumer or cross-client determinism contract.
- Rollback: if frame ranges, transition timing, random bounds, gating, reset or authority isolation changes, retain the nested helper and remove only the proposed projection/definition adapter.
- Focused verification: state-machine bounds, injected random behavior, presentation gate, Spawn reset, no save/network projection and unique P11 owner.

All P04 checkpoints are documented. The checkpoint log above is the current implementation record; the remaining design text preserves migration intent and does not claim that unimplemented adapters, systems, queries or projections exist.

## 9. Actual verification record

Every compile-capable command first inspected active `dotnet.exe`/`csc.exe` processes and then ran serially from the repository root through `Build/Tools/Invoke-SerialDotnet.ps1`. The wrapper was invoked with its explicit `-DotnetArguments` array so PowerShell passed each `-p:` property as a child argument. No raw `dotnet` command was used.

| Project/verifier | Exact argument set | Result | Output or artifact |
|---|---|---|---|
| `src/Player/Terraria.Player.csproj` | `build .\src\Player\Terraria.Player.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `1`; 0 warnings, 5 errors | No trusted artifact from this run. The existing `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` predates the run and is stale/untrusted. Errors are in unrelated session files `src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` and `SubmitMinionCapacityDeltaCommand.cs`, which lack `Terraria.Relationships`/`Terraria.Projectile` references. |
| `src/Teleportation/Terraria.Teleportation.csproj` | `build .\src\Teleportation\Terraria.Teleportation.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`; 0 warnings, 0 errors | `Build/bin/Terraria.Teleportation/Debug/net10.0/Terraria.Teleportation.dll` |
| `src/Combat/Terraria.Combat.csproj` | `build .\src\Combat\Terraria.Combat.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`; 0 warnings, 0 errors | `Build/bin/Terraria.Combat/Debug/net10.0/Terraria.Combat.dll` |
| `src/WorldInteraction/Terraria.WorldInteraction.csproj` | `build .\src\WorldInteraction\Terraria.WorldInteraction.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`; 0 warnings, 0 errors | `Build/bin/Terraria.WorldInteraction/Debug/net10.0/Terraria.WorldInteraction.dll` |
| `Test/Terraria.WorldInteraction.Components.Verification` | `run --project .\Test\Terraria.WorldInteraction.Components.Verification\Terraria.WorldInteraction.Components.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`; `PASS: world interaction component field composition` | Existing `Build/bin/Terraria.WorldInteraction.Components.Verification/Debug/net10.0/Terraria.WorldInteraction.Components.Verification.exe` |
| `Test/Terraria.Combat.Verification` | `run --project .\Test\Terraria.Combat.Verification\Terraria.Combat.Verification.csproj --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false` | exit `0`; `PASS: combat resolution, immunity, contribution, and attribution` | Existing `Build/bin/Terraria.Combat.Verification/Debug/net10.0/Terraria.Combat.Verification.exe` |

The two verifiers are existing domain smoke verifiers; they do not prove all P04 lifecycle, network, persistence or cross-partition writer behavior. No dedicated P04 Player/Teleportation verifier exists. The P04 Player build remains blocked by unrelated pre-existing Progression source errors, so overall `verificationStatus` is `partial`, not `completed`; behavior equivalence and network/persistence closure remain unverified.
