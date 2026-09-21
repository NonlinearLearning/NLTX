# Version4 P07 玩家移动、跳跃、抓钩与穿越组件拆分设计

partitionId: P07
sessionId: 557fbd2f1ba042b49770c6351375f1c3
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P07-Player-Mobility.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P07-player-mobility-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P07-player-mobility-component-execution.md
designStatus: proposed
executionStatus: failed
implementationStatus: implemented
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: source-saved-player-build-blocked-5-errors
completedComponents: [PlayerBeetleArmorState, PlayerSolarAndNebulaArmorState, PlayerMagnetAndUtilityAccessoryState, PlayerDashAndGroundTraversalState, PlayerRopeAndPulleyState, PlayerSlideAndCarpetTraversalState, PlayerWingsAndFlightState, PlayerJumpAvailabilityState, PlayerJumpExecutionState, PlayerJumpMobilityModifiers, PlayerGrappleAndRocketState]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T10:50:38Z
evidence-gap: C01-C11 component sources are saved. The serial Terraria.Player build is blocked by five
existing errors in src/Player/Progression outside P07; no focused verifier ran, and the existing DLL is
older than this build attempt. Nebula helper semantics, BitsByte/shared-value closure, frame/effect
projections, Item/Equipment/Combat/Spatial/Projectile/Mount owners, network/persistence format,
scheduler ordering, and behavior-equivalence evidence remain open. C11 keeps rocketSoundDelay and
rocketFrame deferred to the proposed effect projection.
blocking-decision: Component-only implementation is allowed, but the affected Player project cannot
establish fresh compile evidence until its unrelated Progression references are repaired. Do not
implement or claim the proposed Systems, Queries, Projections, Ports, Adapters, tests, registration,
network, persistence, or runtime writer replacement.

## 1. 范围与状态

本设计只覆盖 P07 权威报告中的 11 个叶子子系统、112 个字段和 0 个属性。它是 ECS
组件边界、所有权和迁移输入，不是行为等价证明、公共 API 兼容证明、网络闭合证明或持久化
闭合证明。本轮允许将已明确的组件状态写入 `src`，并同步本分区两份 Markdown；不修改测试、
项目文件、Version4 源码、权威报告、其他会话文档、ledger 或任何非组件实现。

叶子组是检查点，不意味着每组落成一个巨型组件。相同声明区域仍按共同读写者、生命周期、
状态种类和副作用边界拆成 Component、System、Query、Command、Adapter、Projection 或
deferred seam。所有 proposed 类型都明确标记 `status: proposed`。

| 检查点 | 权威叶子子系统 | 成员 | 建议边界 | 当前证据 |
|---|---|---:|---|---|
| C01 | PlayerBeetleArmorState | 10 | 甲虫套装能力状态、球体运动状态、动画投影 | partial; completed for design |
| C02 | PlayerSolarAndNebulaArmorState | 10 | 日耀护盾运动/冲刺与星云资源等级 | partial |
| C03 | PlayerMagnetAndUtilityAccessoryState | 8 | 配饰派生能力与环境/Combat 交接 | partial |
| C04 | PlayerDashAndGroundTraversalState | 12 | 冲刺阶段、地面穿越和空间碰撞交接 | partial |
| C05 | PlayerRopeAndPulleyState | 10 | 绳索/滑轮/宝石资格与移动状态 | partial |
| C06 | PlayerSlideAndCarpetTraversalState | 10 | 滑行、冰鞋、飞毯和短期表现状态 | partial |
| C07 | PlayerWingsAndFlightState | 6 | 飞行资格、飞行时间和翅膀帧投影 | partial |
| C08 | PlayerJumpAvailabilityState | 18 | 额外跳跃资格与可再次跳跃窗口 | partial |
| C09 | PlayerJumpExecutionState | 11 | 额外跳跃执行阶段和 pogo 技巧标记 | partial |
| C10 | PlayerJumpMobilityModifiers | 5 | 跳跃修正、自动跳和额外下落 | partial |
| C11 | PlayerGrappleAndRocketState | 12 | 抓钩关系、火箭靴计时和释放投影 | partial |

## 2. 证据登记

| 来源 | 已核对事实 | 支持的结论 | evidenceStatus |
|---|---|---|---|
| P07 权威报告 | P07 报告的 11 个叶子节；来源序号 488..1257；字段 112、属性 0 | 成员、C# 类型、Version4 路径/行号和原始声明 | confirmed |
| Version4 Player 声明 | `D:\TRbackup\Version4\Terraria\Player.cs:565-759,892-902,2056-2158` | P07 全部字段的直接声明、默认值和数组容量 | confirmed |
| Version4 构造/生成 | `Player.cs` 的 `Player()` 路径和 `Spawn`：包含 `grappling[0] = -1`、玩家实例数组和重置边界 | 数组初始化、实体生成和重生清理是生命周期入口 | confirmed; partial for all fields |
| Version4 装备/派生能力 | `Player.cs:6865` `UpdateEquips`；`8008` `ApplyEquipVanity`；`8138` `ApplyEquipFunctional`；`9547` `UpdateArmorSets`；`9607` `ApplySetBonus_Solar`；`9684` `UpdateArmorSets_Always_Beetle`；`9745`/`9777` 甲虫 set bonus | 装备和 Buff 派生能力写入能力状态；达到阈值可能产生 Buff、伤害或表现效果 | confirmed for inspected paths; partial for all readers |
| Version4 reset/lifecycle | `Player.cs:10272` `ResetEffects`；`11319` `UpdateJumpHeight`；`11373` `FindPulley`；`21798` `Spawn`；`19743` `RemoveAllGrapplingHooks` | 每 tick 派生重置、生成清理、抓钩清理和移动资格边界 | confirmed; persistence partial |
| Version4 movement | `Player.cs:11493` `HorizontalMovement`；`12055` `JumpMovement`；`12558` `DashMovement`；`13005` `WallClimbMovement`；`13152` `WallslideMovement`；`13246` `CarpetMovement`；`13431` `WingMovement`；`13655` `GrappleMovement`；`13789` `RefreshDoubleJumps`；`13832` `StickyMovement`；`14124` `SlopeDownMovement`；`14144`/`14212` 液体；`14311` `TileCollision`；`14789` `Update` | 移动状态不是一个每 Tick 全量写入的 `PlayerMobilityComponent`；各阶段有不同输入、清理和 Spatial seam | confirmed for named methods; partial for call ordering |
| Version4 frame/cleanup | `Player.cs:18034` `WingFrame`；`19743` `RemoveAllGrapplingHooks` | wing/beetle/pulley/carpet frame 属于表现或短期投影，抓钩清理是关系 owner 的终端动作 | confirmed; partial for projection transport |
| Version4 world tick | `D:\TRbackup\Version4\Terraria\Main.cs:11420-11472` | `Main.DoUpdateInWorld` 逐玩家调用 `player[i].Update(i)`；调度必须显式重建 | confirmed |
| Version4 network input/output | `MessageBuffer.cs` case 13；`NetMessage.cs` case 13 | 控制位、pulley、gravDir、位置/速度和坐骑位进入/离开协议；P07 字段不能自行假设所有字段网络同步 | confirmed; P07 field-level coverage partial |
| tModLoader public mirror | `D:\TRbackup\tmodloader-api-docs-stable\index.html` shows `v2026.07`; `class_player.html`, `class_mod_player.html`, `class_mount.html`, `class_projectile.html` | 公开 Player/ModPlayer/Mount/Projectile boundary and hook shape only | confirmed-public-boundary; not Version4 proof |
| Current NLTX | `dome/src/Terraria.Dome.Simulation/Player/Components/PlayerMovementStateComponent.cs`, `PlayerFlightStateComponent.cs`, `PlayerGrappleStateComponent.cs`, `PlayerControlSystem.cs`, `MovementSystem.cs` | 仅部分 flight/grapple/fall 状态和简化输入/位置更新已存在；没有完整 P07 authority | existing-evidence; partial |
| Missing constraint | `D:\TRbackup\NLTX\约束\公共拆分约束.md` 不存在 | 不能声称该约束已读取；以 AGENTS、ECS 文件组织和副作用规范为准 | missing/partial |

Version4 当前 checkout 中 `GetGrapplingForces`、`RefreshMovementAbilities`、`SolarDashStart`
等路径存在 stub/空实现，Player 直接序列化入口也不能证明完整保存格式。因此本设计的总体
`evidenceStatus` 保持 `partial`，不把 public mirror 或 full reference 当作 Version4 私有行为
等价证据。

## 3. 边界模型与不变量

### 3.1 所有权方向

```text
input/network command
  -> capability/qualification Query
  -> one owner System validates and commits P07 state
  -> movement/spatial/combat integration command or event
  -> immutable network/persistence/presentation projection
```

- `Player` 的装备定义、Item 实例、Buff catalog、Projectile 实体、Tile/Collision 数据和 Mount
  定义不由 P07 组件复制拥有。能力字段可以保存玩家侧派生事实，但装备来源通过只读 Query 或
  显式 Command 进入。
- `EntityReference`、legacy player slot、Projectile slot、Projectile identity、persistent ID
  和 20 个 `grappling` 槽位的整数关系保持不同类型；P07 不把数组值提升为全局实体 ID。
- Query 只读、确定性、无写回；`Main.rand`、时钟、音频、Dust、Lighting、Gore、网络发送、
  日志和持久化都位于 Adapter/Port/System 效果边界。
- 一个权威事实只有一个 writer。兼容 Adapter 可以读旧字段或输出旧形状，但禁止旧字段与
  proposed component 双写。
- `wingFrame`、`beetleFrame`、`pulleyFrame`、`carpetFrame` 等帧值默认是表现投影；只有在
  具体网络/保存证据闭合后才可进入协议或持久化 DTO。
- `meleeEnchant` 位于权威报告的 rope/pulley 叶子组，但最终语义更接近 Combat/Status；本
  分区只保留 `crossSubsystemOwner: integration-review`，不宣布 P07 的最终 owner。

### 3.2 proposed 模块与路径

下表的类型、路径和命名空间全部是 `status: proposed`；实现前仍需记录源/目标路径和依赖
影响。小的 Player capability 保持在 `src/Player`，有稳定的 Armor、Mobility、Jump、Grapple
边界时使用 capability 子目录，不创建 `Shared/Components/` 或通用杂项目录。

| 能力边界 | proposed 类型 | proposed 路径 | proposed namespace | writer/seam |
|---|---|---|---|---|
| Beetle | `PlayerBeetleArmorStateComponent`, `PlayerBeetleOrbKinematicsComponent` | `src/Player/Armor/PlayerBeetleArmorStateComponent.cs`, `src/Player/Armor/PlayerBeetleOrbKinematicsComponent.cs` | `Terraria.Player.Armor` | `PlayerBeetleArmorSystem`; `IEcsRandomSource`, `IPlayerCombatCommandPort` |
| Beetle frame | `PlayerBeetleArmorFrameProjection` | `src/Player/Armor/PlayerBeetleArmorFrameProjection.cs` | `Terraria.Player.Armor` | `PlayerBeetleArmorFrameSystem`; renderer adapter |
| Solar/Nebula | `PlayerSolarArmorStateComponent`, `PlayerNebulaResourceStateComponent`, `PlayerSolarShieldKinematicsComponent` | `src/Player/Armor/PlayerSolarArmorStateComponent.cs`, `src/Player/Armor/PlayerNebulaResourceStateComponent.cs`, `src/Player/Armor/PlayerSolarShieldKinematicsComponent.cs` | `Terraria.Player.Armor` | `PlayerSolarNebulaSystem`; Combat/Resource seams |
| Utility | `PlayerUtilityCapabilityComponent`, `PlayerUtilityCapabilityQuery` | `src/Player/Accessories/PlayerUtilityCapabilityComponent.cs`, `src/Player/Accessories/PlayerUtilityCapabilityQuery.cs` | `Terraria.Player.Accessories` | `PlayerUtilityCapabilitySystem`; Item/Combat/World queries |
| Ground/dash | `PlayerGroundTraversalStateComponent`, `PlayerDashStateComponent`, `PlayerGroundTraversalSystem` | `src/Player/Mobility/PlayerGroundTraversalStateComponent.cs`, `src/Player/Mobility/PlayerDashStateComponent.cs`, `src/Player/Mobility/PlayerGroundTraversalSystem.cs` | `Terraria.Player.Mobility` | spatial commit port; `crossSubsystemOwner: integration-review` for position/velocity |
| Pulley/rope | `PlayerRopeStateComponent`, `PlayerPulleyStateComponent`, `PlayerRopeAndPulleySystem` | `src/Player/Mobility/PlayerRopeStateComponent.cs`, `src/Player/Mobility/PlayerPulleyStateComponent.cs`, `src/Player/Mobility/PlayerRopeAndPulleySystem.cs` | `Terraria.Player.Mobility` | pulley/rope command; projectile relation adapter |
| Slide/carpet | `PlayerSlideStateComponent`, `PlayerCarpetTraversalStateComponent`, `PlayerSlideAndCarpetSystem` | `src/Player/Mobility/PlayerSlideStateComponent.cs`, `src/Player/Mobility/PlayerCarpetTraversalStateComponent.cs`, `src/Player/Mobility/PlayerSlideAndCarpetSystem.cs` | `Terraria.Player.Mobility` | spatial commit port; presentation projection |
| Wings | `PlayerFlightStateComponent` successor, `PlayerWingFrameProjection` | `src/Player/Mobility/PlayerFlightStateComponent.cs`, `src/Player/Mobility/PlayerWingFrameProjection.cs` | `Terraria.Player.Mobility` | `PlayerFlightSystem`; mount/spatial seam |
| Jump | `PlayerJumpAvailabilityComponent`, `PlayerJumpExecutionComponent`, `PlayerJumpMobilityModifiersComponent` | `src/Player/Jump/PlayerJumpAvailabilityComponent.cs`, `src/Player/Jump/PlayerJumpExecutionComponent.cs`, `src/Player/Jump/PlayerJumpMobilityModifiersComponent.cs` | `Terraria.Player.Jump` | `PlayerJumpSystem`; `PlayerJumpQualificationQuery` |
| Grapple/rocket | `PlayerGrappleRelationComponent`, `PlayerRocketStateComponent` | `src/Player/Grapple/PlayerGrappleRelationComponent.cs`, `src/Player/Grapple/PlayerRocketStateComponent.cs` | `Terraria.Player.Grapple` | `PlayerGrappleAndRocketSystem`; `IProjectileRelationPort` |

### 3.3 Explicit scheduler contract

The following order is a proposed runtime contract; it is not inferred from Markdown or file order.
It must be confirmed against the Version4 `Update(int)` call chain and a focused verifier before code
migration:

1. Apply decoded input/network commands and reject duplicate or stale sequence values.
2. Rehydrate/clear player capability relations at spawn, respawn, disconnect, and terminal cleanup.
3. Reset derived equipment/Buff capability facts, then run Armor and Utility capability systems.
4. Run jump qualification and jump execution; consume an extra-jump fact at most once per tick.
5. Resolve dash, rope/pulley, slide/carpet, wing/flight and grapple movement phases in an explicit
   priority order; each phase emits a movement intent rather than writing another phase's state.
6. Submit spatial/collision and liquid queries; a single movement commit owns position/velocity.
7. Submit Combat/Projectile commands for armor/dash/rocket effects; side effects run through ports.
8. Advance timers and relation cleanup, then produce network, persistence and presentation snapshots.
9. Run frame projections after committed gameplay facts; frame projections never write authority.

The exact ordering between `WingMovement`, `GrappleMovement`, `TileCollision`, `WetCollision`,
`DryCollision`, `SlopeDownMovement` and `Update` early returns remains an evidence gap. `outOfRange`
must short-circuit only the documented remote-player update path and cannot silently disable local
authority.

## 4. Current NLTX boundary assessment

Current NLTX contains useful partial prototypes but no complete P07 replacement:

- `PlayerFlightStateComponent` already models `Wings`, `WingsLogic`, `WingTime` and `WingTimeMax`.
  It is a partial capability component, not proof that `wingFrame` or all flight behavior is owned.
- `PlayerGrappleStateComponent` already models 20 integer targets, Count, attach/remove/clear.
  It must be reconciled with Projectile slot/identity semantics before it can be authoritative.
- `PlayerMovementStateComponent` only models `FallStart` and `FallStart2`.
- `PlayerControlSystem` directly writes `VelocityComponent` for simplified horizontal movement,
  jump and mount flight. `MovementSystem` directly adds velocity to location and does not cover the
  Version4 collision/liquid/dash/wing/grapple chain.
- Existing verifier coverage is focused on basic Tile, half-block, slope and platform physics; it
  does not close P07 armor, jump, dash, flight, pulley, grapple or rocket behavior.

Migration therefore means converging these prototypes behind one owner writer, not adding a second
set of same-named authorities.

## 5. Checkpoint C01: PlayerBeetleArmorState

### 5.1 Boundary decision

`PlayerBeetleArmorState` is split into three proposed boundaries. The counters and capability flags
are authoritative gameplay facts; `beetlePos`/`beetleVel` are per-player orb kinematics; frame fields
are a presentation projection. A single `PlayerBeetleArmorSystem` owns gameplay state, a dedicated
orb system owns kinematics, and a frame projection owns only animation output. No Query may call
`Main.rand`, mutate `Player`, emit Dust/Lighting/Gore, or send network data.

### 5.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 488 | `beetleOrbs: int` | authoritative Component; `PlayerBeetleArmorStateComponent.BeetleOrbCount`; writer `PlayerBeetleArmorSystem` | `src/Player/Armor/PlayerBeetleArmorStateComponent.cs`; `Terraria.Player.Armor`; `status: proposed` | `UpdateBuffs` derives levels from Beetle buffs; `UpdateArmorSets_Always_Beetle` reads count; set-bonus methods add/remove Buffs and constrain 0..3. Created at player construction, refreshed by Buff/equipment phase, cleared on armor/Buff loss and spawn. Gameplay counter; Buff add/remove is an effect through `IPlayerBuffCommandPort`. | No equivalent Beetle field; generic `src/Player/PlayerAbilityComponent.cs` is only partial. | `Player.cs:565,4731-4763,4800-4830,9703-9710`; report 488 |
| 489 | `beetleCounter: float` | authoritative timer Component; `PlayerBeetleArmorStateComponent.BeetleCounter`; writer same owner | same proposed component/path; `status: proposed` | `ApplySetBonus_BeetleDefense` increments and gates orb grants; `ApplySetBonus_BeetleDamage` decrements/clamps and derives attack tier; `UpdateArmorSets_Always_Beetle` clears when no Beetle set. Tick-local deterministic timer; threshold transitions emit Buff/combat commands. | No equivalent timer; `PlayerAbilityComponent` does not establish ownership. | `Player.cs:567,9684-9691,9745-9774,9777-9821`; report 489 |
| 490 | `beetleCountdown: int` | authoritative transient Component field; `PlayerBeetleArmorStateComponent.BeetleCountdown`; writer same owner | same proposed component/path; `status: proposed` | `ApplySetBonus_BeetleDamage` increments and resets when target orb tier changes; reset on player spawn/effect invalidation must be verified. Short-lived counter, not a persisted armor definition; no direct network claim. | No equivalent. | `Player.cs:569,9781-9815`; report 490; reset closure partial |
| 491 | `beetleDefense: bool` | derived capability Component; `PlayerBeetleArmorStateComponent.HasDefenseSet`; writer Armor/Buff commit | same proposed component/path; `status: proposed` | `ResetEffects`/effect reset clears derived flags; `ApplySetBonus_BeetleDefense` sets true; `UpdateBuffs` reads it to validate defense buffs. Recomputed each effect phase, not durable authority. Buff validity and Combat defense output cross a command seam. | No Beetle capability; existing ability component is too broad and has no reset proof. | `Player.cs:571,9745-9775,4734-4763`; report 491; reset path partial |
| 492 | `beetleOffense: bool` | derived capability Component; `HasOffenseSet`; writer Armor/Buff commit | same proposed component/path; `status: proposed` | `ApplySetBonus_BeetleDamage` sets true; `UpdateBuffs` reads it to validate offense Buffs; reset/recompute each tick. Gameplay qualification; no direct render/network effect. | No equivalent. | `Player.cs:573,9777-9829,4796-4830`; report 492 |
| 493 | `beetleBuff: bool` | derived Buff-present Component flag; `BeetleBuffActive`; writer Armor/Buff system | same proposed component/path; `status: proposed` | `UpdateBuffs` sets it while accepting Beetle buff tiers; downstream armor/effect readers consume it. Clear on reset/Buff removal; no independent writer or persistence. Buff list remains Buff-domain owner. | No equivalent. | `Player.cs:575,4731-4763,4796-4830`; report 493 |
| 512 | `beetlePos: Vector2[]` length 3 | authoritative per-player orb kinematics Component; `PlayerBeetleOrbKinematicsComponent.Positions`; writer orb system | `src/Player/Armor/PlayerBeetleOrbKinematicsComponent.cs`; `Terraria.Player.Armor`; `status: proposed` | `UpdateArmorSets_Always_Beetle` zeroes inactive slots and advances active positions by velocity; `PlayerFrame`/armor visuals read positions. Initialize 3 slots, clear inactive slots on count decrease/spawn, commit as gameplay/visual snapshot. Position is not an entity ID or Projectile relation. | No equivalent. | `Player.cs:614,9684-9743`; report 512; visual call graph partial |
| 513 | `beetleVel: Vector2[]` length 3 | authoritative per-player orb kinematics Component; `Velocities`; writer orb system with injected random source | same kinematics path; `status: proposed` | `UpdateArmorSets_Always_Beetle` advances/limits velocity and subtracts player velocity; `Main.rand.Next` perturbs each active orb. `IEcsRandomSource` and any Dust/Lighting/audio/Gore/Combat effects are adapters; Query is pure over an injected random sample. Clear inactive slots and retain stable length 3. | No equivalent; no random/effect isolation in current Player prototype. | `Player.cs:616,9703-9743`; report 513; `Main.rand` side effect boundary |
| 514 | `beetleFrame: int` | Projection; `PlayerBeetleArmorFrameProjection.BeetleFrame`; writer frame system only | `src/Player/Armor/PlayerBeetleArmorFrameProjection.cs`; `Terraria.Player.Armor`; `status: proposed` | `UpdateArmorSets_Always_Beetle` advances 0..2 when defense/offense is active; renderer/frame code reads it. Reset to documented initial frame at spawn or inactive state; do not persist or replicate until evidence. Animation state, not armor authority. | No equivalent frame component. | `Player.cs:618,9688-9701`; report 514; presentation reader closure partial |
| 515 | `beetleFrameCounter: int` | Projection; `PlayerBeetleArmorFrameProjection.BeetleFrameCounter`; writer frame system | same projection path; `status: proposed` | `UpdateArmorSets_Always_Beetle` increments and wraps with frame; cleared/reinitialized with frame lifecycle. No gameplay writer, save writer or network writer established. | No equivalent. | `Player.cs:620,9693-9701`; report 515 |

### 5.3 Compatibility, IDs, network and persistence

- The six gameplay facts remain Player-scoped values. The three-element orb arrays are fixed-capacity
  value state and do not become three entities, Projectile slots, or persistent IDs.
- No P07 C01 member is declared network-authoritative solely because `MessageBuffer` case 13 carries
  movement data. Armor/Buff replication requires an explicit packet/owner decision; until then the
  proposed projection is read-only and `verificationStatus: not-run`.
- Version4 `Player` serialization entry points in this checkout do not prove a complete save format.
  C01 fields therefore remain `persistence: deferred` pending a save adapter evidence pass. A save
  adapter may translate a committed snapshot, but may not write the old and new fields together.
- Prediction/rollback stores a deterministic pre-tick snapshot of the C01 Component plus the random
  sample/seed supplied by `IEcsRandomSource`; replay must reproduce orb positions and threshold
  commands. Side-effect commands are deduplicated by tick/command identity and replayed only at the
  commit boundary.
- Public legacy names and namespace remain at an Adapter boundary during migration. The old Player
  writer is removed only after the C01 focused verifier proves one writer and unchanged Buff/Combat
  ordering.

### 5.4 Focused verifier plan

1. `BeetleBuffTierTransition`: defense/offense Buff tiers map to 0..3 orbs, invalid tiers are
   removed in the same order, and `beetleBuff` clears when the source Buff disappears.
2. `BeetleCounterTiming`: defense grant threshold, offense decay, countdown reset and inactive-set
   cleanup match the Version4 tick sequence.
3. `BeetleOrbKinematics`: three-slot initialization, inactive-slot zeroing, velocity damping,
   player-velocity subtraction and deterministic injected-random samples match expected values.
4. `BeetleEffectsIsolation`: random, Dust, Lighting, Gore, audio, Buff, Combat and network effects
   are emitted only through explicit ports and never from a Query or Component.
5. `BeetleFrameProjection`: frame wrap and reset are one-way output, with no write back to armor
   authority or persistence unless a later protocol decision explicitly permits it.
6. `BeetleRollbackAndDuplicateCommand`: replay and duplicate tick commands leave one committed
   result and do not double-add Buffs or Combat effects.

### 5.5 C01 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: retain legacy `Terraria.Player` adapter reads; proposed ownership is under
  `src/Player/Armor/`; no source file is moved in this session.
- Dependency impact: Buff/Equipment supplies capability facts; Combat receives explicit results;
  Presentation consumes frame/position projection; Random/Effects/Network/Persistence remain ports.
- Rollback: keep the legacy writer if any tier, counter, orb position, frame or side-effect ordering
  diverges; do not enable dual-write.

### 5.6 Actual component implementation checkpoint

- Saved component sources:
  - `src/Player/Armor/PlayerBeetleArmorStateComponent.cs`
  - `src/Player/Armor/PlayerBeetleOrbKinematicsComponent.cs`
- Implemented state: the six armor/Buff facts (`BeetleOrbCount`, `BeetleCounter`,
  `BeetleCountdown`, `HasDefenseSet`, `HasOffenseSet`, `BeetleBuffActive`) and fixed-capacity
  three-slot orb `Positions`/`Velocities` arrays.
- Defaults: scalar state uses the source-compatible C# zero defaults; the two arrays are allocated
  at capacity 3 and start with zero `Vector2` values. Frame fields 514-515 remain a deferred
  presentation projection.
- Dependency impact: Buff/Equipment, Combat, random sampling, presentation, network, persistence,
  and Spatial ownership remain external. No System, Query, Command, Adapter, Projection, test, or
  registration code was added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

### 6.6 Actual component implementation checkpoint

- Saved component sources:
  - `src/Player/Armor/PlayerSolarArmorStateComponent.cs`
  - `src/Player/Armor/PlayerSolarShieldKinematicsComponent.cs`
  - `src/Player/Armor/PlayerNebulaResourceStateComponent.cs`
- Implemented state: Solar `ShieldCount`, `SolarCounter`, `IsSolarDashing`,
  `SolarDashConsumedFlare`; fixed-capacity length-3 shield `Positions`/`Velocities`; Nebula
  `LifeLevel`, `ManaLevel`, `NebulaManaCounter`, and `DamageLevel`.
- Defaults: scalar fields use C# zero defaults; shield arrays have capacity 3 and start with zero
  `Vector2` values. Dust, shader, Lighting, Combat, Buff, Resource, network and persistence effects
  remain external. The Version4 Nebula helper is still incomplete, so no tier algorithm was added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 6. Checkpoint C02: PlayerSolarAndNebulaArmorState

### 6.1 Boundary decision

日耀和星云不能合并成一个只按字段数量切出的 Armor 组件。日耀护盾包含固定 3 槽的
位置/速度、Buff tier 和冲刺消费标记；星云字段是由 Buff/装备效果派生的三种资源等级及
其计时器。`PlayerSolarNebulaSystem` 可以协调同一装备阶段，但权威状态、护盾运动和资源
等级仍通过窄组件分开。Solar 的 Dust、shader、Lighting、伤害和 Buff 操作通过显式 port；
Nebula 的完整等级更新仍受空实现限制，不能声称行为闭合。

### 6.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 494 | `solarShields: int` | authoritative Component; `PlayerSolarArmorStateComponent.ShieldCount`; writer `PlayerSolarNebulaSystem` | `src/Player/Armor/PlayerSolarArmorStateComponent.cs`; `Terraria.Player.Armor`; `status: proposed` | `UpdateBuffs` maps Solar shield Buffs to tiers 1..3 and removes invalid tiers; `ApplySetBonus_Solar` grants/replenishes Buffs; shield movement reads count. Created at player spawn, reset when set/Buff is absent, constrained to 0..3. Buff add/remove is `IPlayerBuffCommandPort` effect. | No Solar component. `PlayerAbilityComponent` is generic and does not own shield tiers. | `Player.cs:577,4770-4793,9607-9644`; report 494 |
| 495 | `solarCounter: int` | authoritative set-bonus timer Component; `PlayerSolarArmorStateComponent.SolarCounter`; writer same owner | same proposed component/path; `status: proposed` | `ApplySetBonus_Solar` increments toward the shield interval, clamps at the interval, and resets after granting a shield; `UpdateArmorSets` clears when `setSolar` is false. Tick timer; threshold emits Buff, Dust and shader commands. | No equivalent timer. | `Player.cs:579,9600-9644`; report 495 |
| 496 | `solarShieldPos: Vector2[]` length 3 | authoritative shield kinematics Component; `PlayerSolarShieldKinematicsComponent.Positions`; writer Solar shield system | `src/Player/Armor/PlayerSolarShieldKinematicsComponent.cs`; `Terraria.Player.Armor`; `status: proposed` | `ApplySetBonus_Solar` clears inactive slots, advances active slots and derives the orbit target from `miscCounter`, direction and mount state. Frame/render code reads positions. Fixed capacity 3; inactive tail is cleared whenever count falls. Not a Projectile or entity relation. | No equivalent. | `Player.cs:582,9646-9670`; report 496; render ownership partial |
| 497 | `solarShieldVel: Vector2[]` length 3 | authoritative shield kinematics Component; `Velocities`; writer same system | same kinematics path; `status: proposed` | `ApplySetBonus_Solar` computes velocity toward the orbit target and adds it on the next tick; movement/render paths consume it. Position and velocity commit must be deterministic from the input snapshot. Any Dust/shader output is an effect adapter. | No equivalent; current MovementSystem cannot express orbit slots. | `Player.cs:584,9650-9670`; report 497 |
| 498 | `solarDashing: bool` | authoritative short-lived dash state; `PlayerSolarArmorStateComponent.IsSolarDashing`; writer dash/solar command seam | same state path; `status: proposed` | Solar set/dash logic reads and writes this flag around `dashDelay`; `UpdateArmorSets_Always_Solar` clears it while dash is unavailable and includes it in dash qualification. Lifecycle is dash start/end, respawn and effect reset. The actual velocity, hit and flare effects belong to Mobility/Combat integration. | No Solar dash state; `PlayerControlSystem` only has generic `input.Dash` and mount dash. | `Player.cs:586,9672-9680`; `SolarDashStart` stub; report 498; `crossSubsystemOwner: integration-review` |
| 499 | `solarDashConsumedFlare: bool` | authoritative dash-consumption guard; `SolarDashConsumedFlare`; writer Solar dash commit | same state path; `status: proposed` | Solar dash resets this guard when `dashDelay >= 0`; dash start consumes it at most once before issuing the flare/Combat command. Spawn/reset clears it. It is transient and not a definition or independent Item fact. | No equivalent. | `Player.cs:588,9672-9678`; Solar dash call path is partial |
| 500 | `nebulaLevelLife: int` | authoritative derived resource-level Component; `PlayerNebulaResourceStateComponent.LifeLevel`; writer Nebula Buff system | `src/Player/Armor/PlayerNebulaResourceStateComponent.cs`; `Terraria.Player.Armor`; `status: proposed` | Nebula life Buff tiers pass through `UpdateBuffs_NebulaBuffs(ref nebulaLevelLife,...)`; life regen/effect readers consume the level. Clear/recompute with Buff/effect reset. Resource level is not life itself; Combat/Resource owns applied stat change. | No Nebula state. | `Player.cs:590,4832-4840,6227`; the helper is empty in Version4 checkout, so exact tier semantics are partial |
| 501 | `nebulaLevelMana: int` | authoritative derived resource-level Component; `ManaLevel`; writer Nebula Buff system | same proposed component/path; `status: proposed` | `UpdateBuffs_NebulaBuffs` is the named writer boundary for Mana tiers; magic/mana effects read it. Recompute each effect phase and clear on Buff loss. Do not conflate level with `statMana` or `manaMagnet`. | No equivalent. | `Player.cs:592,4832-4835,6227`; report 501; helper stub |
| 502 | `nebulaManaCounter: int` | transient resource timer Component; `NebulaManaCounter`; writer Nebula system | same proposed component/path; `status: proposed` | Declared adjacent to Nebula levels and consumed by the Nebula effect path; exact readers/writers are not complete in this checkout. Treat as tick-local counter until call graph is recovered. Reset at spawn/effect reset; no persistence/network claim. | No equivalent. | `Player.cs:594,6227`; report 502; reader/writer gap |
| 503 | `nebulaLevelDamage: int` | authoritative derived resource-level Component; `DamageLevel`; writer Nebula Buff system | same proposed component/path; `status: proposed` | `UpdateBuffs_NebulaBuffs` derives damage tier and `UpdateBuffs` applies the level to melee/ranged/magic damage. Clear/recompute with Buff phase; Combat receives committed modifier rather than reading mutable Player storage. | No equivalent. | `Player.cs:596,4841-4848,6227`; report 503; helper stub |

### 6.3 Compatibility, IDs, network and persistence

- `solarShields` is a three-level Buff-derived capability; `solarShieldPos` and `solarShieldVel`
  are exactly three value slots and never become entity IDs or Projectile handles.
- Solar dash must use a command key that includes player entity and tick. Repeated input cannot
  consume two flares or emit two hit results. Position/velocity ownership remains with the Spatial
  integration review, not the armor component.
- Nebula level fields are derived resource facts, not direct mana/life/damage authority. Their
  Resource/Combat projection must be committed before consumers run; no P07 system writes combat
  totals directly.
- No C02 field is declared network or persistent authority until the missing Version4 serialization
  and field-level packet evidence is supplied. Public field names remain Adapter compatibility only.
- Prediction snapshots include shield count/counter/flags, fixed arrays and Nebula levels/counter.
  Random/effect commands are buffered and deduplicated; if the Nebula helper remains unknown, replay
  fails closed rather than inventing a level transition.

### 6.4 Focused verifier plan

1. `SolarShieldTierAndReset`: Buff tiers 0..3, invalid Buff removal, set loss and spawn reset.
2. `SolarShieldOrbit`: fixed length 3, inactive tail zeroing, orbit target and velocity update with
   mount/direction inputs, and deterministic tick ordering.
3. `SolarDashConsumption`: start/end, delay gate and one-time flare consumption under duplicate input.
4. `NebulaLevelBoundary`: once the helper evidence is supplied, verify life/mana/damage tier update,
   stat projection and reset; until then mark the scenario blocked/partial.
5. `SolarEffectIsolation`: Dust, shader, Lighting, audio, Buff and Combat commands occur only at
   commit/effect adapters.
6. `SolarRollbackAndProjection`: replay restores all arrays/counters and network/save projections
   remain one-way.

### 6.5 C02 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: add proposed Armor types under `src/Player/Armor/`; retain legacy fields at an
  Adapter boundary until Solar/Nebula verifiers pass.
- Dependency impact: Equipment/Buff supplies set facts; Resource/Combat consumes level projections;
  Spatial owns final dash/position commit; Presentation owns shield visual/effect output.
- Rollback: keep the legacy writer when Solar/Nebula tier, orbit, dash or effect ordering differs;
  do not dual-write.

## 7. Checkpoint C03: PlayerMagnetAndUtilityAccessoryState

### 7.1 Boundary decision

这 8 个字段不是普通移动参数，而是装备/Buff 重算阶段产生的玩家能力事实。磁力能力由
Item pickup 查询消费，`chiselSpeed` 由工具速度计算消费，`lifeForce` 由生命资源投影消费，
`hasDeadCellsDownDash` 与跳跃资格交接，`calmed` 与敌对目标/环境交接，`inferno` 与 Combat
和 Lighting 交接。建议一个 `PlayerUtilityCapabilityComponent` 保存能力事实，一个
`PlayerUtilityCapabilitySystem` 在 ResetEffects 后重算；拾取、Combat、World/Environment 和
Jump 只通过 Query/Command 使用，不在 P07 复制其状态。

### 7.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 504 | `manaMagnet: bool` | derived capability Component; `PlayerUtilityCapabilityComponent.ManaMagnet`; writer utility system | `src/Player/Accessories/PlayerUtilityCapabilityComponent.cs`; `Terraria.Player.Accessories`; `status: proposed` | `ApplyEquipFunctional`/equipment paths set it; pickup and item grab-range logic read it for mana item types; `ResetEffects` clears it before re-derivation. Capability fact, not Item ownership; pickup command remains Items/World seam. | No utility component; existing player ability state is not a unique owner. | `Player.cs:598,8226-8230,8578-8591,10477-10481,19887-19894,20003-20006`; report 504 |
| 505 | `lifeMagnet: bool` | derived capability Component; `LifeMagnet`; writer utility system | same proposed component/path; `status: proposed` | Buff 105/equipment paths set it; pickup and grab range read it for life items; `ResetEffects` clears it. Health restoration/economy effects are external commands and not direct component writes. | No equivalent. | `Player.cs:600,4563-4566,10478,19891-19894,20011-20014`; report 505 |
| 506 | `treasureMagnet: bool` | derived pickup-range capability; `TreasureMagnet`; writer utility system | same proposed component/path; `status: proposed` | Equipment path sets it; item grab-range calculation adds treasure range when true; reset/recompute per effect phase. The Item catalog owns item classification and range constants. | No equivalent. | `Player.cs:602,8270-8272,8924-8928,10479,20015-20018`; report 506 |
| 507 | `chiselSpeed: bool` | derived tool-speed capability; `ChiselSpeed`; writer utility system | same proposed component/path; `status: proposed` | Equipment path sets it; `PickSpeed`/tool calculation subtracts the documented speed adjustment; `ResetEffects` clears it. Tool timing and tile mutation belong to Item/WorldInteraction seams. | No equivalent. | `Player.cs:604,8168-8170,8924-8928,10480,6987-6989`; report 507 |
| 508 | `lifeForce: bool` | derived life-capacity capability; `LifeForce`; writer utility system | same proposed component/path; `status: proposed` | Buff 113 sets it and immediately contributes to `statLifeMax2`; reset clears it. The proposed state is only the capability input; health capacity and healing remain Resource/Combat owners. | No equivalent. | `Player.cs:606,4622-4626,10481`; report 508 |
| 509 | `hasDeadCellsDownDash: bool` | derived jump/dash capability; cross-system read through `PlayerJumpQualificationQuery` | `src/Player/Accessories/PlayerUtilityCapabilityComponent.cs`; `Terraria.Player.Accessories`; `status: proposed`; `crossSubsystemOwner: integration-review` for final jump/dash owner | Item type 5465 sets it; `JumpMovement` reads it with down control, airborne and non-mount guards to permit Down Dash. Reset clears it. The ability fact may live in utility projection, while execution state belongs Jump/Mobility. | No Dead Cells/down-dash component; current control system has only generic Jump input. | `Player.cs:608,8163-8167,10476,12199-12252`; report 509 |
| 510 | `calmed: bool` | derived environment/aggro capability; owner pending integration review | `src/Player/Accessories/PlayerUtilityCapabilityComponent.cs`; `Terraria.Player.Accessories`; `status: proposed`; `crossSubsystemOwner: integration-review` | Buff 106 sets it; reset clears it. NPC targeting/aggro and environment systems consume the fact; P07 does not write NPC state or target lists. | No equivalent. | `Player.cs:610,4567-4569,10486`; report 510 |
| 511 | `inferno: bool` | derived Combat/environment capability; owner pending Combat integration | same proposed component/path; `status: proposed`; `crossSubsystemOwner: integration-review` | Buff 116 sets it; UpdateBuffs emits Lighting and periodically applies inferno damage using `infernoCounter`, with local/remote conditions. Reset clears it. Damage, Lighting and network effects are explicit ports; `infernoCounter` is outside P07. | No inferno capability; no effect-port closure in current prototype. | `Player.cs:612,4637-4646,10475,14981-14987`; report 511 |

### 7.3 Compatibility, IDs, network and persistence

- These booleans are capability facts derived from Item/Buff inputs. The proposed component does not
  store Item instances, Buff slots, health/mana totals, NPC targets or Tile mutation state.
- The reset order is strict: clear prior derived capability facts, apply equipment/Buff definitions,
  publish a committed capability snapshot, then let pickup/tool/Jump/Combat consumers query it.
  A stale capability must not survive unequipping or Buff expiry.
- `hasDeadCellsDownDash` is a qualification input, not an execution flag. `inferno` is a qualification
  input, not a damage result. `calmed` is a targeting input, not an NPC mutation.
- No C03 field is network/persistence authority until Version4 packet and serialization evidence is
  closed. A projection may expose the committed booleans to an adapter, but no client projection may
  write them back.
- Prediction snapshots contain the eight booleans and the source revision/tick. Invalid or unknown
  Item/Buff definitions fail closed; repeated equipment commands are idempotent by commit key.

### 7.4 Focused verifier plan

1. `UtilityResetAndRecompute`: each capability clears when the source Item/Buff disappears and is
   restored only by the next committed effect calculation.
2. `MagnetPickupQuery`: mana/life/treasure item range decisions use capability snapshots and issue
   one Item pickup command without mutating the capability component.
3. `ChiselToolTiming`: chisel capability affects tool speed through a World/Item query seam only.
4. `LifeForceProjection`: life capacity receives one committed modifier and does not treat the bool
   as current health or a persistence field.
5. `DownDashQualification`: down-dash availability honors airborne, direction, mount and execution
   guards and does not consume the flag itself.
6. `CalmedAndInfernoEffects`: targeting, damage, Lighting and local/remote behavior are emitted by
   their owners through ports; no Query performs effects.

### 7.5 C03 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: proposed capability component under `src/Player/Accessories/`; no source file
  moves in this session.
- Dependency impact: Equipment/Buff writes source facts; Items/WorldInteraction, Resource, Jump,
  NPC targeting and Combat consume explicit snapshots/commands.
- Rollback: retain legacy capability writer if reset order, pickup range, tool timing, life capacity,
  down-dash qualification or inferno/calmed side effects diverge; never dual-write.

### 7.6 Actual component implementation checkpoint

- Saved component source: `src/Player/Accessories/PlayerUtilityCapabilityComponent.cs`.
- Implemented state: `ManaMagnet`, `LifeMagnet`, `TreasureMagnet`, `ChiselSpeed`, `LifeForce`,
  `HasDeadCellsDownDash`, `Calmed`, and `Inferno`.
- Dependency impact: the component contains only derived capability facts. Item pickup, tool timing,
  life resources, Jump execution, NPC targeting, Combat damage, Lighting, network, and persistence
  remain external owners; no System or Query was added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 8. Checkpoint C04: PlayerDashAndGroundTraversalState

### 8.1 Boundary decision

冲刺是有开始判定、双击窗口、活动阶段、延迟和结束清理的状态机；阶梯下落、斜坡和地表
加速则是 Spatial/Collision 输入或中间事实。建议将 Dash 状态与 GroundTraversal 状态分开，
由 `PlayerGroundTraversalSystem` 协调但不直接成为位置/速度的第二 writer。`outOfRange` 是
远程玩家在 Tile 数据不可用时的更新短路标记，必须由网络/可见性整合 owner 决定。`flapSound`
是效果门控而不是移动 authority。

### 8.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 524 | `stairFall: bool` | authoritative traversal input/state; `PlayerGroundTraversalStateComponent.StairFall`; writer collision traversal system | `src/Player/Mobility/PlayerGroundTraversalStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed`; `crossSubsystemOwner: integration-review` for final collision writer | `TileCollision` sets/clears it from fall-through, grapple and gravity conditions and `Collision.SlopeCollision` output; movement stages read it. Reset per collision tick and spawn. It is not a generic jump flag. | Existing `PhysicsStateComponent` has ground flags but no Version4 stair-fall state. | `Player.cs:638,14311-14339`; report 524; Spatial owner partial |
| 525 | `outOfRange: bool` | runtime visibility/update short-circuit state; `PlayerOutOfRangeStateComponent`; network/visibility owner | `src/Player/Mobility/PlayerOutOfRangeStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed`; `crossSubsystemOwner: integration-review` | `Update` resets it, remote-player section checks can set it when world section/tile data is unavailable, then skip selected updates. Must clear on valid section/respawn and never skip local authority. It is not a persistent movement ability. | No equivalent. | `Player.cs:640,14789-14955`; report 525; section protocol reader/writer partial |
| 532 | `sloping: bool` | collision result fact; `PlayerGroundTraversalStateComponent.IsSloping`; writer slope system | same ground component/path; `status: proposed`; `crossSubsystemOwner: integration-review` | `SlopeDownMovement` clears then sets when slope changes vertical velocity; frame/collision consumers read it. Recomputed each movement tick; reset on spawn and non-slope result. | Existing Tile/slope verifiers cover geometry but no Player `sloping` authority. | `Player.cs:654,14124-14142`; report 532 |
| 544 | `dashType: int` | derived dash capability/definition key; `PlayerDashStateComponent.DashType`; writer equipment/mobility capability commit | `src/Player/Mobility/PlayerDashStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed`; `crossSubsystemOwner: integration-review` | Equipment paths assign dash types; Solar set can promote it; `DashMovement` consumes it and reset clears it. Mount may override to type 6. Keep definition/capability separate from activity timers. | Current `PlayerControlSystem` treats `input.Dash` generically and has no dash type component. | `Player.cs:678,8684-8701,9672-9680,12558-12575`; report 544 |
| 545 | `dash: int` | authoritative active dash mode; `PlayerDashStateComponent.ActiveDash`; writer dash system | same dash component/path; `status: proposed` | `DashMovement` copies `dashType` at start, clears activity when no dash, and uses mode to choose speed/effects. Spawn/effect reset clears. Dash mode is not an Item ID or Projectile ID. | No equivalent. | `Player.cs:680,6043-6047,12558-12937`; report 545 |
| 546 | `dashTime: int` | authoritative double-tap/input window timer; `PlayerDashStateComponent.DashTime`; writer dash input system | same dash component/path; `status: proposed` | `DashMovement` increments/decrements toward a second directional press and starts dash when release/window conditions pass. Reset on spawn and direction lifecycle. Input edge comes from command, not a Query. | `ControlInputComponent` has dash edge state, but no Version4 timing state. | `Player.cs:682,12945-12997`; report 546 |
| 547 | `timeSinceLastDashStarted: int` | authoritative cooldown/elapsed timer; `PlayerDashStateComponent.TimeSinceLastDashStarted`; writer dash system | same dash component/path; `status: proposed` | `DashMovement` resets on dash start; `Update` increments and clamps at 300; wing/run logic reads it for behavior. Tick timer, reset on spawn, not a definition. | No equivalent. | `Player.cs:684,12969-12992,14988-14992,18018-18019`; report 547 |
| 548 | `dashDelay: int` | authoritative dash phase/lock timer; `PlayerDashStateComponent.DashDelay`; writer dash system | same dash component/path; `status: proposed` | `DashMovement` distinguishes positive pre-start delay, negative active phase and terminal reset; Solar and frame/equipment readers use sign/value. It gates horizontal movement and is cleared on invalid/no dash. | No equivalent; current control writes velocity directly with no dash lock. | `Player.cs:686,11571-11719,12566-12937,9672-9680`; report 548 |
| 551 | `accRunSpeed: float` | derived movement parameter; `PlayerGroundTraversalStateComponent.AcceleratedRunSpeed`; writer equipment/mount/mobility calculation | `src/Player/Mobility/PlayerGroundTraversalStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed`; `crossSubsystemOwner: integration-review` | Equipment and mount effects set/adjust it; `HorizontalMovement` reads it with max speed, slowed/chilled, wings and mount logic; reset to max run speed in `Update`. It is a derived parameter, not final velocity. | `PlayerControlSystem` uses a fixed player speed and mount registry, no equivalent derived speed. | `Player.cs:692,8172-8195,11493-11719,14802-14806,16201-16228`; report 551 |
| 582 | `powerrun: bool` | derived surface capability; `PlayerGroundTraversalStateComponent.PowerRun`; writer collision/surface query | same ground component/path; `status: proposed`; `crossSubsystemOwner: integration-review` | `TileCollision` derives it from the supporting tile and `HorizontalMovement` applies speed/acceleration/slowdown modifiers; reset when no supporting surface. Tile catalog is external read-only input. | Existing Tile collision has no Version4 surface capability. | `Player.cs:755,14379-14401,16117-16122`; report 582 |
| 583 | `runningOnSand: bool` | derived surface classification; `PlayerGroundTraversalStateComponent.RunningOnSand`; writer collision/surface query | same ground component/path; `status: proposed`; `crossSubsystemOwner: integration-review` | `TileCollision` derives conversion sets; horizontal movement applies desert-boot modifiers; reset when no ground. No Item or tile mutation belongs here. | No equivalent. | `Player.cs:757,14379-14401,16123-16128`; report 583 |
| 584 | `flapSound: bool` | effect gate/projection state; `PlayerFlightEffectProjection.FlapSoundPlayed`; writer flight/effect adapter | `src/Player/Mobility/PlayerFlightEffectProjection.cs`; `Terraria.Player.Mobility`; `status: proposed`; `crossSubsystemOwner: integration-review` | Wing/movement logic checks it to avoid repeating flap sound, sets/clears around wing movement. Sound call is effect adapter; the bool is transient and must not be a sound authority or save field. | No audio/effect projection in current Player prototype. | `Player.cs:759,16422-16433`; report 584 |

### 8.3 Compatibility, IDs, network and persistence

- Dash mode values are capability/behavior keys, not item or projectile identifiers. The dash timers
  are player-scoped ticks and must not be reused as network sequence IDs.
- `stairFall`, `sloping`, `powerrun` and `runningOnSand` depend on Tile/Collision snapshots. A single
  Spatial commit owns final position/velocity; P07 cannot write both a traversal state and the shared
  `LocationComponent`/`VelocityComponent` independently.
- `outOfRange` must be scoped to the remote-player section/visibility path. A missing Tile section
  produces a short-circuit fact, not an invented position or a local-player authority loss.
- Network and persistence treatment for every C04 field is deferred. The case-13 movement packet
  does not prove that dash timers, surface flags or frame/effect flags are serialized. Legacy fields
  remain adapter-compatible reads until packet/save evidence is closed.
- Prediction snapshots include dash state and traversal facts; Tile query inputs and command IDs are
  part of replay context. Effect commands for dust/sound/lighting are buffered and deduplicated.

### 8.4 Focused verifier plan

1. `DashDoubleTapAndPhase`: directional edge/window, dash type, positive/negative delay, active
   timer, cooldown clamp and invalid reset.
2. `DashMovementOwnership`: only the spatial commit writes final velocity/position; Dash emits an
   intent and effect commands in stable order.
3. `SlopeAndStairTraversal`: fall-through, grapple, gravity direction and slope collision update
   `stairFall`/`sloping` without leaking stale flags.
4. `SurfaceRunModifiers`: tile classification derives power-run/sand facts and applies modifiers
   once; no query mutates Tile state.
5. `OutOfRangeSectionShortCircuit`: remote section absence sets the marker and skips only documented
   work; local authority and subsequent section recovery remain active.
6. `FlapEffectGate`: flap sound is emitted once per intended transition through an effect port and
   the projection cannot mutate flight authority.

### 8.5 C04 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: proposed Mobility types under `src/Player/Mobility/`; no source file moves in
  this session.
- Dependency impact: Equipment/Buff/Mount supply parameters; Spatial/Collision owns final movement;
  Network/Visibility owns remote short-circuit; Presentation/Audio owns flap output.
- Rollback: retain the legacy writer if dash phase, collision flags, speed modifiers, out-of-range
  semantics or effect order diverge; never dual-write.

### 8.6 Actual component implementation checkpoint

- Saved component sources:
  - `src/Player/Mobility/PlayerDashStateComponent.cs`
  - `src/Player/Mobility/PlayerGroundTraversalStateComponent.cs`
  - `src/Player/Mobility/PlayerOutOfRangeStateComponent.cs`
- Implemented state: dash type/mode/window/timer/phase fields; stair, slope, run-speed, power-run,
  and sand facts; and the remote out-of-range marker.
- Deferred state: `flapSound` remains the proposed `PlayerFlightEffectProjection` boundary. These
  components do not write `LocationComponent`, `VelocityComponent`, Tile state, audio, network, or
  visibility state outside their own fields.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 9. Checkpoint C05: PlayerRopeAndPulleyState

### 9.1 Boundary decision

绳索/滑轮字段包含三类不同事实：绳索使用短计时与装备资格、宝石/大宝石的周期性库存
扫描缓存，以及滑轮在绳索上的移动阶段与帧投影。建议 `PlayerRopeStateComponent` 保存资格和
短期 rope counter，`PlayerPulleyStateComponent` 保存移动阶段，`PlayerPulleyFrameProjection`
只输出帧。`gem`/`ownedLargeGems` 是从库存扫描得到的玩家能力投影，不是 Item 实例 owner；
`meleeEnchant` 不由本分区重新拥有。

### 9.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 541 | `ropeCount: int` | transient rope grace/cooldown Component; `PlayerRopeStateComponent.RopeCount`; writer rope/pulley system | `src/Player/Mobility/PlayerRopeStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed` | `Update` decrements it; pulley entry uses zero plus grapple/control/mount checks; moving down rope sets it to 10. Tick-local relation guard, reset at spawn and terminal movement cleanup; not an item count. | No rope state; current grapple component has only projectile targets. | `Player.cs:672,15717-15723,16028-16033`; report 541 |
| 552 | `cordage: bool` | derived rope capability; `PlayerRopeStateComponent.HasCordage`; writer equipment/Buff reset | same proposed component/path; `status: proposed` | Functional equipment sets it; rope/pulley discovery and movement read it through the item/effect path; `ResetEffects` clears it. Item definition remains external. | No equivalent. | `Player.cs:694,8643-8646,10580-10584`; report 552 |
| 553 | `gem: int` | derived inventory scan result; `PlayerRopeStateComponent.SelectedGem`; writer inventory/rope capability system | same proposed component/path; `status: proposed` | Initialized `-1`; periodic inventory scan resets and selects large gem type/index; rope hook logic reads it. This is an Item catalog/index projection, not an entity ID or persistent Item reference. | No equivalent. | `Player.cs:696,9957-9960,15323-15344`; report 553 |
| 554 | `gemCount: int` | transient inventory scan counter; `PlayerRopeStateComponent.GemScanCounter`; writer rope/inventory scan system | same proposed component/path; `status: proposed` | Increments each update; at 10 resets `gem`/bitmask and scans inventory slots. Clear on spawn/reset; no network or save authority without evidence. | No equivalent. | `Player.cs:698,15323-15344`; report 554 |
| 555 | `ownedLargeGems: Terraria.BitsByte` | derived capability bitset; `PlayerRopeStateComponent.OwnedLargeGems`; writer inventory scan | same proposed component/path; `status: proposed` | Reset to zero with gem scan, then sets bits for item types 1522..1527/3643; rope/hook qualification reads it. Bit positions are catalog values, not entity/network IDs. | No equivalent. | `Player.cs:700,9957-9960,15323-15344`; report 555 |
| 556 | `meleeEnchant: byte` | deferred Combat/Status projection; no P07 authority | `src/Combat/Status/PlayerMeleeEnchantProjection.cs` or Combat-owned path; `status: proposed`; `crossSubsystemOwner: integration-review` | Buff update maps status types 71, 73..79 to values 1..8; melee/projectile hit logic consumes the effect. `ResetEffects` clears it. P07 records the source boundary but Combat/Status must own final writer. | No melee enchant component in P07; existing Combat status components are possible owner candidates. | `Player.cs:702,6175-6207,10721-10724`; report 556 |
| 557 | `pulleyDir: byte` | authoritative pulley phase/direction; `PlayerPulleyStateComponent.Direction`; writer pulley movement system | `src/Player/Mobility/PlayerPulleyStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed`; `crossSubsystemOwner: integration-review` for final position | `FindPulley`/pulley movement assigns 1/2 based on rope side and collision; grapple or direction changes reset/adjust it. Lifecycle is pulley enter/exit, spawn and collision recovery. It is a small phase enum, not a player facing ID. | No equivalent. | `Player.cs:704,11373-11458,15740-16049,23231-23239`; report 557 |
| 558 | `pulley: bool` | authoritative pulley-active state; `PlayerPulleyStateComponent.IsActive`; writer pulley system | same pulley path; `status: proposed`; `crossSubsystemOwner: integration-review` | `FindPulley`/movement enters and exits pulley; movement, frame and grapple paths read it; grappling clears pulley. Reset on spawn, invalid rope, mount/terminal cleanup. Position/velocity changes go through Spatial commit. | No equivalent. | `Player.cs:706,15721-16068,20765-20771,25130-25134`; report 558 |
| 559 | `pulleyFrame: int` | presentation Projection; `PlayerPulleyFrameProjection.Frame`; writer frame projection | `src/Player/Mobility/PlayerPulleyFrameProjection.cs`; `Terraria.Player.Mobility`; `status: proposed` | Pulley movement advances and wraps 0..1 based on vertical speed; body frame consumes it. Clear at entry/exit as required by frame path; no gameplay, save or network authority. | No equivalent. | `Player.cs:708,16050-16063,20765-20771`; report 559 |
| 560 | `pulleyFrameCounter: float` | presentation Projection state; `PlayerPulleyFrameProjection.Counter`; writer frame projection | same projection path; `status: proposed` | Pulley movement accumulates absolute vertical-speed contribution, advances frame over threshold and clears counter; it is animation cadence only. | No equivalent. | `Player.cs:710,16050-16063`; report 560 |

### 9.3 Compatibility, IDs, network and persistence

- `gem` and `ownedLargeGems` are catalog/index facts obtained from inventory scans. They cannot be
  used as EntityReference, Item persistent ID, Projectile slot or network identity.
- `pulleyDir` values 1/2 are a movement phase and direction convention. The final player position,
  velocity and Tile collision remain one Spatial writer; a pulley system emits intents and state facts.
- `meleeEnchant` must be handed to Combat/Status and cannot be dual-written by P07. Its Buff slots,
  hit behavior, network and save policy are `crossSubsystemOwner: integration-review`.
- Rope scan counters and frame counters are transient. No C05 field is declared durable or replicated
  until Version4 save and packet evidence is supplied; public names are retained in compatibility reads.
- Prediction snapshots include rope/pulley state and the inventory source revision; scan and movement
  commands are idempotent and stale scan results are rejected.

### 9.4 Focused verifier plan

1. `RopeGraceLifecycle`: decrement, set-to-10 on downward exit, zero gating and spawn cleanup.
2. `GemScanProjection`: 10-tick scan interval, selected gem, bitset reset and inventory mutation
   ordering; bit positions remain catalog values.
3. `MeleeEnchantHandoff`: Buff values reach exactly one Combat/Status owner and are cleared on reset.
4. `PulleyDiscoveryAndDirection`: rope tile discovery, direction 1/2 transitions, obstacle recovery,
   grapple/mount exclusion and invalid cleanup.
5. `PulleyMovementOwnership`: only Spatial commit writes position/velocity; pulley state emits phase
   and direction facts.
6. `PulleyFrameProjection`: frame/counter wrap is one-way output and does not become network/save
   authority.

### 9.5 C05 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: proposed Mobility rope/pulley components and frame projection; Combat Status
  remains the integration owner for `meleeEnchant`.
- Dependency impact: Inventory/Item supplies gem scans and cordage; Tile/Collision supplies rope;
  Projectile supplies grapple relation; Spatial commits movement; Combat owns melee enchant.
- Rollback: keep legacy rope/pulley writer if relation, scan, direction, collision or frame behavior
  diverges; do not dual-write.

### 9.6 Actual component implementation checkpoint

- Saved component sources:
  - `src/Player/Mobility/PlayerRopeStateComponent.cs`
  - `src/Player/Mobility/PlayerPulleyStateComponent.cs`
- Implemented state: rope grace counter, cordage capability, selected gem with source default `-1`,
  gem scan counter, byte-backed large-gem bit payload, pulley direction, and pulley-active state.
- Type/dependency gap: the affected Player project does not expose a reusable `Terraria.BitsByte`
  value type. `OwnedLargeGems` stores the same eight-bit payload as `byte` until that shared value
  boundary is resolved; no helper or protocol type was added.
- Deferred state: `meleeEnchant` remains Combat/Status-owned; pulley frame fields remain a
  presentation projection. Inventory, Tile, Projectile, Spatial, network, and persistence behavior
  is not implemented here.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 10. Checkpoint C06: PlayerSlideAndCarpetTraversalState

### 10.1 Boundary decision

滑行由 `WallslideMovement`/地面移动根据 `slideDir`、`spikedBoots`、控制输入和碰撞结果
决定；飞毯则是空中可用资格、300 tick 计时、下落抑制和帧投影。二者共享移动优先级输入，
但不应写入同一状态机。`iceSkate` 是地表摩擦能力，`spikedBoots` 是装备派生参数，
`snowBallLauncherInteractionCooldown` 是 Item 交互冷却，最终 owner 留在 integration-review。

### 10.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 562 | `sliding: bool` | authoritative short-lived traversal state; `PlayerSlideStateComponent.IsSliding`; writer slide system | `src/Player/Mobility/PlayerSlideStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed`; `crossSubsystemOwner: integration-review` for spatial commit | `WallslideMovement` clears then sets it when wall/edge geometry and input permit; `JumpMovement`, frame selection and collision read it. Clear at each movement phase, landing, mount and spawn. Final velocity/position are Spatial-owned. | No slide state; existing PlayerMovementState only has fall starts. | `Player.cs:714,13005-13150,13152-13244,12200-12289`; report 562 |
| 563 | `slideDir: int` | authoritative traversal direction; `PlayerSlideStateComponent.Direction`; writer collision/slide system | same slide component/path; `status: proposed`; `crossSubsystemOwner: integration-review` | Movement derives -1/1 from wall/velocity collision, resets at start of Update, and uses it for control/geometry; frame code reads it. It is direction state, not EntityReference. | No equivalent. | `Player.cs:716,13014-13027,13156-13168,17471-17635`; report 563 |
| 564 | `snowBallLauncherInteractionCooldown: int` | transient Item interaction timer; deferred Item/WorldInteraction owner | `src/Items/Interactions/SnowballLauncherInteractionCooldownProjection.cs` or integration-owned path; `Terraria.Items`; `status: proposed`; `crossSubsystemOwner: integration-review` | `Update` decrements it; snowball launcher interaction reads/writes it in Item behavior. It should be a command payload or Item interaction state, not a movement Component. Reset at spawn/terminal cleanup. | No matching Item interaction component in the inspected P07/NLTX Player prototypes. | `Player.cs:718,14966-14974`; report 564; complete Item call graph partial |
| 565 | `iceSkate: bool` | derived surface capability; `PlayerSlideStateComponent.IceSkate`; writer equipment reset/effect system | `src/Player/Mobility/PlayerSlideStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed`; `crossSubsystemOwner: integration-review` | Equipment sets it; `HorizontalMovement` uses it to modify acceleration/max speed on slippery surfaces; `ResetEffects` clears it. Tile catalog and Item definition remain read-only external inputs. | No equivalent; current MovementSystem has no surface capability input. | `Player.cs:720,9017-9029,10629-10633,16131-16148`; report 565 |
| 566 | `carpet: bool` | derived flight/traversal capability; `PlayerCarpetTraversalStateComponent.HasCarpet`; writer equipment/effect system | `src/Player/Mobility/PlayerCarpetTraversalStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed` | Equipment/effect phase sets it; `CarpetMovement` qualifies it with grapple, extra jumps, jump, rocket, wings and mount states; reset clears it and frame. It is a capability input, not a carpet Item owner. | No equivalent. | `Player.cs:722,13250-13291,10629-10631,15701-15711`; report 566 |
| 567 | `spikedBoots: int` | derived equipment level; `PlayerSlideStateComponent.SpikedBootsLevel`; writer equipment system | same slide component/path; `status: proposed` | Equipment assigns levels; `WallslideMovement` gates wall slide and sets velocity/effects based on threshold. Reset to 0 in effect reset. This is a level, not an Item slot or durability. | No equivalent. | `Player.cs:724,8696-8701,10631-10633,13039-13081,13179-13230`; report 567 |
| 568 | `carpetFrame: int` | presentation Projection; `PlayerCarpetFrameProjection.Frame`; writer carpet frame projection | `src/Player/Mobility/PlayerCarpetFrameProjection.cs`; `Terraria.Player.Mobility`; `status: proposed` | `CarpetMovement` advances and wraps frame 0..5 while active, otherwise sets -1; frame/render code reads it. Reset when carpet unavailable, grounded or grappling. No gameplay/network/save authority. | No equivalent. | `Player.cs:726,13258-13291,13655-13666,15701-15711`; report 568 |
| 569 | `carpetFrameCounter: float` | presentation Projection state; `PlayerCarpetFrameProjection.Counter`; writer frame projection | same projection path; `status: proposed` | `CarpetMovement` increments by speed-dependent amount and advances frame at threshold; cleared when inactive/grounded. It is animation cadence only. | No equivalent. | `Player.cs:728,13272-13285,15706-15711`; report 569 |
| 570 | `canCarpet: bool` | authoritative re-use qualification; `PlayerCarpetTraversalStateComponent.CanStart`; writer traversal reset/landing system | same carpet component/path; `status: proposed` | `CarpetMovement` consumes it on activation; pulley/grapple/landing paths restore or clear it; gravity reversal clears it; `Update` resets on grounded/sliding. It is a one-use qualification gate, not a render flag. | No equivalent. | `Player.cs:730,13250-13258,13664-13666,15701-15717`; report 570 |
| 571 | `carpetTime: int` | authoritative active traversal timer; `PlayerCarpetTraversalStateComponent.RemainingTime`; writer carpet system | same carpet component/path; `status: proposed` | `CarpetMovement` starts at 300 and decrements while control is held; inactive path clears frame. Spawn/ground/grapple/mount reset according to existing movement lifecycle. Timer is tick state, not Item definition. | No equivalent. | `Player.cs:732,13253-13266,15706-15711`; report 571 |

### 10.3 Compatibility, IDs, network and persistence

- `slideDir` is a local movement direction and `carpetFrame` is an animation index; neither is a
  network identity or entity relation. `spikedBoots` is a capability level, not an equipment object.
- `carpet` qualification must run after Grapple/Jump/Wing exclusion facts are committed and before
  Spatial movement; exact priority is an integration scheduler decision. `sliding` and `carpetTime`
  cannot each write the shared velocity.
- `snowBallLauncherInteractionCooldown` remains outside the Mobility authority until Item/World
  Interaction identifies the command and failure/retry semantics.
- No C06 field is declared network/persistent authority from the inspected evidence. Frame fields and
  interaction cooldown are deferred; legacy names remain compatibility adapter reads.
- Prediction snapshots include slide/carpet state and the current movement input. Rollback discards
  dust/sound/Item commands and restores the previous single-writer state.

### 10.4 Focused verifier plan

1. `WallSlideDirectionAndReset`: wall geometry, input direction, spiked-boot thresholds, gravity
   reversal and stale slide clearing.
2. `IceSkateSurfaceModifier`: slippery tile plus equipment capability changes acceleration only once.
3. `CarpetQualificationAndExclusion`: grapple, extra-jump, rocket, wing, mount and grounded
   exclusions; `canCarpet` consumed exactly once.
4. `CarpetTimerAndFrame`: 300 tick timer, held control, inactive reset, fixed frame range and
   speed-dependent counter.
5. `SnowballInteractionHandoff`: cooldown belongs to the Item command path and cannot be changed by
   movement queries.
6. `SlideCarpetSpatialOwnership`: one position/velocity commit and deterministic rollback.

### 10.5 C06 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: proposed Mobility slide/carpet components and frame projection; Item cooldown
  remains integration-owned.
- Dependency impact: Equipment supplies capability; Tile/Collision supplies surface/wall facts;
  Jump/Wing/Grapple/Mount supply exclusions; Spatial commits movement; Presentation consumes frames.
- Rollback: retain legacy slide/carpet writer if collision, timer, qualification, Item interaction or
  frame behavior diverges; never dual-write.

### 10.6 Actual component implementation checkpoint

- Saved component sources:
  - `src/Player/Mobility/PlayerSlideStateComponent.cs`
  - `src/Player/Mobility/PlayerCarpetTraversalStateComponent.cs`
- Implemented state: sliding/direction/ice-skate/spiked-boot facts and carpet capability,
  re-use qualification, and active remaining time.
- Deferred state: `snowBallLauncherInteractionCooldown` remains Item/WorldInteraction-owned;
  carpet frame fields remain presentation projection state. Components do not write spatial position,
  velocity, Tile data, Item state, audio, network, or persistence.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 11. Checkpoint C07: PlayerWingsAndFlightState

### 11.1 Boundary decision

飞行能力本身与翅膀动画必须分开。`wingTime` 是消耗型玩家资源，`wings` 是装备/定义引用，
`wingsLogic` 是行为/视觉逻辑 key，`wingTimeMax` 是派生容量；`wingFrame` 和
`wingFrameCounter` 只应由 frame projection 写入。现有 NLTX `PlayerFlightStateComponent`
覆盖前四个字段，但其 `TryConsume`/`Restore` 不能代替 Version4 `WingMovement`、液体、
重力、坐骑和火箭优先级证据。

### 11.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 650 | `wingTime: float` | authoritative flight resource; `PlayerFlightStateComponent.WingTime`; writer `PlayerFlightSystem` | `src/Player/Mobility/PlayerFlightStateComponent.cs`; `Terraria.Player.Mobility`; `status: proposed` | `WingMovement` consumes while flying/hovering, resets/restores on landing/jump/rope/pulley and movement transitions; `Update`/mount paths also read it. Create at player spawn, clamp to 0..max, clear on terminal lifecycle. Final velocity remains Spatial owner. | Existing `dome/src/Terraria.Dome.Simulation/Player/Components/PlayerFlightStateComponent.cs` has `WingTime`, `TryConsume`, `Restore`; behavior and scheduler are partial. | `Player.cs:892,13431-13654,16064-16068,16348-16438`; report 650 |
| 651 | `wings: int` | definition reference/capability input; `PlayerFlightDefinitionComponent.Wings`; writer equipment/definition adapter | `src/Player/Mobility/PlayerFlightDefinitionComponent.cs`; `Terraria.Player.Mobility`; `status: proposed` | Equipment/armor phase assigns the wing definition; `WingMovement`/`WingFrame` read it to choose functionality; reset when unequipped. It is a catalog key, not an entity ID or renderer-owned mutable definition. | Existing flight component stores `Wings` but has no definition catalog/Equipment owner proof. | `Player.cs:894, UpdateArmorSets/ApplyEquip paths,13431-13654,18034-19085`; report 651; public `class_player.html` only boundary reference |
| 652 | `wingsLogic: int` | derived behavior logic key; `PlayerFlightDefinitionComponent.WingsLogic`; writer equipment/wing definition system | same definition path; `status: proposed` | Wing functionality and visual helpers read it; it may differ from `wings` for logic overrides. Recomputed with equipment/definition, reset on unequip. No direct persistence/network claim. | Existing component stores `WingsLogic`, but no Version4-equivalent `GetWingsFunctionalityForVisuals` boundary is present. | `Player.cs:896,13431-13654,18523-18526`; report 652; tModLoader public boundary only |
| 653 | `wingTimeMax: int` | derived resource capacity; `PlayerFlightStateComponent.WingTimeMax`; writer equipment/wing definition system | same flight state path; `status: proposed` | Equipment/wing definitions establish max; `WingMovement` and jump/landing restore time to it; clamping and mount resets read it. It is capacity, not current resource. | Existing component stores `WingTimeMax` and clamps `WingTime`; lacks complete definition and mount integration. | `Player.cs:898,11319-11371,13431-13654,16064-16068`; report 653 |
| 654 | `wingFrame: int` | presentation Projection; `PlayerWingFrameProjection.Frame`; writer `PlayerWingFrameSystem` | `src/Player/Mobility/PlayerWingFrameProjection.cs`; `Terraria.Player.Mobility`; `status: proposed` | `WingFrame` chooses frame from wing definition, input, velocity, wing time, liquid and grapple/pulley state; renderer reads it. Reset on landing/unequip/invalid definition. Frame is not flight authority. | No wing frame state in current NLTX Flight component. | `Player.cs:900,18034-19085,20558-20589`; report 654 |
| 655 | `wingFrameCounter: int` | presentation Projection state; `PlayerWingFrameProjection.Counter`; writer frame system | same projection path; `status: proposed` | `WingFrame` increments/resets according to the selected wing animation pattern; sound/dust visual helpers may use transitions. It must remain output state with effect adapters, not a persistent gameplay counter. | No equivalent. | `Player.cs:902,18034-19085`; report 655 |

### 11.3 Compatibility, IDs, network and persistence

- `wings` and `wingsLogic` are definition/behavior keys. They must not be interpreted as Item entity
  IDs, Projectile IDs, network sequence values or persistent references without a catalog adapter.
- `wingTime` and `wingTimeMax` are a resource pair. A flight command can consume time only once per
  accepted tick; landing/grounding restore rules must run in the explicit schedule after Jump/Carpet/
  Grapple exclusion facts are known.
- Current case-13 network evidence covers control/position/velocity and not the six C07 members.
  Network and persistence remain deferred; any future packet/save projection is one-way from committed
  state and must preserve client/server authority.
- Prediction snapshots include flight state and wing definition revision. Frame counters and effect
  commands are predicted only as presentation output and are discarded/recomputed on rollback.

### 11.4 Focused verifier plan

1. `FlightResourceConsumption`: finite/non-negative time, max clamp, repeated input and no consume
   while disallowed by mount/grapple/liquid rules.
2. `WingDefinitionBoundary`: equipment changes update wings, logic and max as one committed revision;
   old definition cannot leak into the next tick.
3. `FlightResetAndRestore`: landing, jump, pulley, grapple, spawn, respawn and mount transitions
   match the documented restore/clear order.
4. `WingFrameProjection`: frame selection and counter cadence consume committed facts only and never
   write `wingTime` or definition state.
5. `FlightEffectIsolation`: flap sound, dust, lighting and network outputs use explicit adapters.
6. `FlightRollbackAndAuthority`: predicted consumption rolls back without duplicating effects or
   changing shared Spatial ownership.

### 11.5 C07 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: reconcile the existing NLTX Flight prototype into the proposed Mobility
  capability boundary; add frame projection separately; no source file moves in this session.
- Dependency impact: Equipment/Armor supplies definition; Jump/Carpet/Grapple/Mount supply exclusion
  facts; Spatial commits motion; Presentation/Audio owns frame and effects.
- Rollback: retain legacy flight writer if resource consumption, definition revision, restore order,
  frame or effects diverge; do not introduce a second FlightState writer.

### 11.6 Actual component implementation checkpoint

- Saved component sources:
  - `src/Player/Mobility/PlayerFlightStateComponent.cs`
  - `src/Player/Mobility/PlayerFlightDefinitionComponent.cs`
- Implemented state: `WingTime`, `WingTimeMax`, `Wings`, and `WingsLogic`.
- Deferred state: `wingFrame` and `wingFrameCounter` remain presentation projection state. The
  components do not implement flight consumption, gravity, liquid, mount, Spatial, audio, network,
  persistence, or frame behavior.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 12. Checkpoint C08: PlayerJumpAvailabilityState

### 12.1 Boundary decision

八种额外跳跃（Cloud、Sandstorm、Blizzard、Fart、Sail、Unicorn、Santank、WallOfFleshGoat、
Basilisk 中的报告组合）每种都需要保留 `hasJumpOption_*` 资格与 `canJumpAgain_*` 可用
窗口的区别。资格由装备/Buff/挂载能力派生，`canJumpAgain` 由落地、滑行、执行和消耗边界
维护。建议使用固定的 `PlayerJumpAvailabilityComponent`，并让 `PlayerJumpQualificationQuery`
只读返回某种跳跃是否可开始；它不能直接改变任何布尔值。

### 12.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 1208 | `hasJumpOption_Cloud: bool` | derived qualification state; `PlayerJumpAvailabilityComponent.HasCloudOption`; writer equipment/Buff ability system | `src/Player/Jump/PlayerJumpAvailabilityComponent.cs`; `Terraria.Player.Jump`; `status: proposed` | Equipment/Buff phase establishes qualification; `JumpMovement` reads it to select Cloud execution; reset on effect recompute and spawn. It is not the consumed window. | No extra-jump component; current control only checks grounded jump. | `Player.cs:2060,12199-12252,15590-15700`; report 1208; full effect call graph partial |
| 1209 | `canJumpAgain_Cloud: bool` | authoritative availability window; `CanJumpAgainCloud`; writer Jump system | same proposed component/path; `status: proposed` | `JumpMovement` consumes/clears it on Cloud jump; `RefreshDoubleJumps` restores it at landing/grounded or sliding boundary; reset on spawn/death. | No equivalent. | `Player.cs:2062,12199-12252,12280-12289,15590-15620`; report 1209 |
| 1211 | `hasJumpOption_Sandstorm: bool` | derived qualification state; `HasSandstormOption`; writer ability system | same proposed component/path; `status: proposed` | Equipment/Buff/extra-jump definition supplies qualification; `JumpMovement` reads it; effect reset clears/recomputes. | No equivalent. | `Player.cs:2066,12199-12252`; report 1211; full source writer partial |
| 1212 | `canJumpAgain_Sandstorm: bool` | authoritative availability window; `CanJumpAgainSandstorm`; writer Jump system | same proposed component/path; `status: proposed` | Jump execution consumes it; landing/sliding refresh restores it; spawn/death clears. | No equivalent. | `Player.cs:2068,12199-12252,12280-12289,15590-15620`; report 1212 |
| 1214 | `hasJumpOption_Blizzard: bool` | derived qualification state; `HasBlizzardOption`; writer ability system | same proposed component/path; `status: proposed` | Qualification is read by `JumpMovement`; reset/recompute follows equipment/Buff phase; no direct consumption. | No equivalent. | `Player.cs:2072,12199-12252`; report 1214 |
| 1215 | `canJumpAgain_Blizzard: bool` | authoritative availability window; `CanJumpAgainBlizzard`; writer Jump system | same proposed component/path; `status: proposed` | Consumed by Blizzard jump and restored by landing/sliding refresh; clear on spawn/death. | No equivalent. | `Player.cs:2074,12199-12252,12280-12289,15590-15620`; report 1215 |
| 1217 | `hasJumpOption_Fart: bool` | derived qualification state; `HasFartOption`; writer ability system | same proposed component/path; `status: proposed` | Qualification read by Jump; reset/recomputed from source effect. Fart visual/particle output is an explicit effect command. | No equivalent. | `Player.cs:2078,12199-12252`; report 1217 |
| 1218 | `canJumpAgain_Fart: bool` | authoritative availability window; `CanJumpAgainFart`; writer Jump system | same proposed component/path; `status: proposed` | Consumed at Fart jump, restored by landing/sliding refresh, cleared on spawn/death; effects remain external. | No equivalent. | `Player.cs:2080,12199-12252,12280-12289`; report 1218 |
| 1220 | `hasJumpOption_Sail: bool` | derived qualification state; `HasSailOption`; writer ability system | same proposed component/path; `status: proposed` | Equipment/Buff/extra-jump source sets qualification; Jump reads it; effect reset clears/recomputes. | No equivalent. | `Player.cs:2084,12199-12252`; report 1220 |
| 1221 | `canJumpAgain_Sail: bool` | authoritative availability window; `CanJumpAgainSail`; writer Jump system | same proposed component/path; `status: proposed` | Consumed at Sail jump and restored by landing/sliding refresh; spawn/death clear. | No equivalent. | `Player.cs:2086,12199-12252,12280-12289`; report 1221 |
| 1223 | `hasJumpOption_Unicorn: bool` | derived qualification state; `HasUnicornOption`; writer ability system | same proposed component/path; `status: proposed` | Qualification read by Jump; Unicorn movement/effect side effects pass through command/adapter. | No equivalent. | `Player.cs:2090,12199-12252`; report 1223 |
| 1224 | `canJumpAgain_Unicorn: bool` | authoritative availability window; `CanJumpAgainUnicorn`; writer Jump system | same proposed component/path; `status: proposed` | Consumed at Unicorn jump; refresh on landing/sliding; reset on spawn/death. | No equivalent. | `Player.cs:2092,12199-12252,12280-12289`; report 1224 |
| 1226 | `hasJumpOption_Santank: bool` | derived qualification state; `HasSantankOption`; writer ability system | same proposed component/path; `status: proposed` | Qualification read by Jump; source effect/visual output external; reset/recompute per effect phase. | No equivalent. | `Player.cs:2096,12199-12252`; report 1226 |
| 1227 | `canJumpAgain_Santank: bool` | authoritative availability window; `CanJumpAgainSantank`; writer Jump system | same proposed component/path; `status: proposed` | Consumed at Santank jump; landing/sliding refresh; spawn/death reset. | No equivalent. | `Player.cs:2098,12199-12252,12280-12289`; report 1227 |
| 1229 | `hasJumpOption_WallOfFleshGoat: bool` | derived qualification state; `HasWallOfFleshGoatOption`; writer ability system | same proposed component/path; `status: proposed` | Qualification read by Jump; execution effect is outside Query; reset/recompute from source. | No equivalent. | `Player.cs:2102,12199-12252`; report 1229 |
| 1230 | `canJumpAgain_WallOfFleshGoat: bool` | authoritative availability window; `CanJumpAgainWallOfFleshGoat`; writer Jump system | same proposed component/path; `status: proposed` | Consumed at Goat jump; landing/sliding refresh and spawn/death clear. | No equivalent. | `Player.cs:2104,12199-12252,12280-12289`; report 1230 |
| 1232 | `hasJumpOption_Basilisk: bool` | derived qualification state; `HasBasiliskOption`; writer ability system | same proposed component/path; `status: proposed` | Qualification read by Jump; source/visual effect external; reset/recompute with ability phase. | No equivalent. | `Player.cs:2108,12199-12252`; report 1232 |
| 1233 | `canJumpAgain_Basilisk: bool` | authoritative availability window; `CanJumpAgainBasilisk`; writer Jump system | same proposed component/path; `status: proposed` | Consumed at Basilisk jump; landing/sliding refresh; spawn/death clear. | No equivalent. | `Player.cs:2110,12199-12252,12280-12289`; report 1233 |

### 12.3 Compatibility, IDs, network and persistence

- `hasJumpOption_*` is a source-derived capability and `canJumpAgain_*` is a consumable window;
  neither is an Item ID, Mount ID, Projectile ID, network sequence or persistent entity identity.
- The eight pairs must be updated atomically with the selected jump kind. A failed or duplicate
  execution command cannot consume a second window; an unknown jump kind fails closed.
- `RefreshDoubleJumps` is a lifecycle command boundary. Landing/sliding/respawn reset behavior must
  be explicit and cannot be reconstructed from a generic `IsGrounded` flag alone.
- The case-13 movement packet does not prove replication of these 18 fields. Network/persistence are
  deferred; adapters may expose an immutable committed snapshot after authority is decided.
- Prediction snapshots include all pairs and the selected jump command key. Visual/effect commands
  are buffered and discarded on rollback; the component itself contains no clocks/random/effects.

### 12.4 Focused verifier plan

1. `JumpQualificationMatrix`: each source kind maps to only its `hasJumpOption` pair and unknown
   source definitions fail closed.
2. `ExtraJumpConsumeOnce`: each `canJumpAgain` is consumed once, with duplicate input and rollback
   replay producing one committed jump.
3. `RefreshDoubleJumpsLifecycle`: grounded, sliding, spawn, death and invalid movement transitions
   restore/clear the correct pairs without cross-kind leakage.
4. `JumpPriorityAndExclusion`: mount, wing, carpet, grapple, down-dash and normal jump precedence
   is explicit and matches the source call chain.
5. `ExtraJumpProjection`: network/save snapshots are one-way and preserve pair ordering only after
   protocol evidence is supplied.

### 12.5 C08 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: proposed `src/Player/Jump/PlayerJumpAvailabilityComponent.cs` plus
  `PlayerJumpQualificationQuery`; no source file moves in this session.
- Dependency impact: Equipment/Buff supplies qualification; Jump owns consume/refresh; Mobility,
  Mount, Wing, Carpet and Grapple provide exclusion facts; Spatial commits motion; Presentation and
  effects are external.
- Rollback: retain legacy jump writer if any pair, refresh boundary, priority or effect order differs;
  never dual-write.

### 12.6 Actual component implementation checkpoint

- Saved component source: `src/Player/Jump/PlayerJumpAvailabilityComponent.cs`.
- Implemented state: all 18 qualification/window fields, keeping every `Has*Option` capability
  separate from its corresponding `CanJumpAgain*` consumable window.
- Dependency impact: Equipment/Buff qualification, Jump consume/refresh, mount/wing/carpet/grapple
  exclusion, Spatial motion, effects, network, and persistence remain external owners. No Query or
  execution behavior was added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 13. Checkpoint C09: PlayerJumpExecutionState

### 13.1 Boundary decision

额外跳跃的执行 flags 不是资格字段的别名：它们描述当前一次跳跃正在执行，驱动上升阶段
的速度、地面 modifier、粒子/声音/视觉和与 Wing/Carpet/Grapple 的排他关系。建议使用一个
`PlayerJumpExecutionComponent` 保存 10 种执行旗标和 `isPerformingPogostickTricks`，由
`PlayerJumpSystem` 在选择并提交 JumpCommand 后统一写入/清理；能力效果通过 `JumpEffectPort`
输出。Down Dash 旗标与 `PlayerJumpMobilityModifiers` 的计时器相关但不合并为一个字段。

### 13.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 1206 | `isPerformingJump_DownDash: bool` | authoritative execution flag; `PlayerJumpExecutionComponent.IsPerformingDownDash`; writer Jump system | `src/Player/Jump/PlayerJumpExecutionComponent.cs`; `Terraria.Player.Jump`; `status: proposed` | `JumpMovement` selects it from Down input/Dead Cells capability; `Update` increments down-dash time and clears on mount/dead/refresh; Wing/boot ability queries block while active; movement/collision reads it. Short-lived execution state, clear on terminal lifecycle. | No equivalent; current control only applies normal jump. | `Player.cs:2056,3080-3089,12093-12096,12249-12252,12285-12289,14993-15004,17998-18003,19743-19749`; report 1206 |
| 1210 | `isPerformingJump_Cloud: bool` | authoritative execution flag; `IsPerformingCloud`; writer Jump system | same proposed component/path; `status: proposed` | Set by Cloud selection in `JumpMovement`; `DoubleJumpVisuals` reads it; `RefreshDoubleJumps`/cancel visual paths clear it. Effects are commands, not component writes. | No equivalent. | `Player.cs:2064,12519-12525,13302-13312,13789-13810,20976-20984`; report 1210 |
| 1213 | `isPerformingJump_Sandstorm: bool` | authoritative execution flag; `IsPerformingSandstorm`; writer Jump system | same proposed component/path; `status: proposed` | Set by Sandstorm selection; visual/effect path reads it; cancel/refresh clears it. Movement remains Spatial seam. | No equivalent. | `Player.cs:2070,12311-12319,13313-13334,20979-20984`; report 1213 |
| 1216 | `isPerformingJump_Blizzard: bool` | authoritative execution flag; `IsPerformingBlizzard`; writer Jump system | same proposed component/path; `status: proposed` | Set by Blizzard selection; movement speed/wing visuals read it; visual path emits effects; cancel/refresh clears it. | No equivalent. | `Player.cs:2076,12321-12329,13386-13390,16159-16163,20980-20984`; report 1216 |
| 1219 | `isPerformingJump_Fart: bool` | authoritative execution flag; `IsPerformingFart`; writer Jump system | same proposed component/path; `status: proposed` | Set by Fart selection; `DoubleJumpVisuals` emits Dust/effects based on flag; movement modifiers read it; cancel/refresh clears. | No equivalent. | `Player.cs:2082,12362-12370,13335-13346,16164-16168,20981-20984`; report 1219 |
| 1222 | `isPerformingJump_Sail: bool` | authoritative execution flag; `IsPerformingSail`; writer Jump system | same proposed component/path; `status: proposed` | Set by Sail selection; visual path reads it; movement/wing behavior uses it; cancel/refresh clears. Sail effects use explicit random/effect ports. | No equivalent. | `Player.cs:2088,12331-12360,13356-13385,16189-16193,20982-20984`; report 1222 |
| 1225 | `isPerformingJump_Unicorn: bool` | authoritative execution flag; `IsPerformingUnicorn`; writer Jump system | same proposed component/path; `status: proposed` | Set by Unicorn selection; visual path reads and randomizes Dust/shader through effect boundary; movement modifiers read it; cancel/refresh clears. | No equivalent. | `Player.cs:2094,12389-12405,13347-13355,16169-16173,20983-20984`; report 1225 |
| 1228 | `isPerformingJump_Santank: bool` | authoritative execution flag; `IsPerformingSantank`; writer Jump system | same proposed component/path; `status: proposed` | Set by Santank selection; visual/ground movement reads it; effects are adapter commands; cancel/refresh clears. | No equivalent. | `Player.cs:2100,12472-12488,16174-16178,20984`; report 1228 |
| 1231 | `isPerformingJump_WallOfFleshGoat: bool` | authoritative execution flag; `IsPerformingWallOfFleshGoat`; writer Jump system | same proposed component/path; `status: proposed` | Set by Goat selection; movement speed/visual readers consume it; cancel/refresh clears. No direct NPC/Combat mutation in component. | No equivalent. | `Player.cs:2106,12417-12432,16179-16183,20984`; report 1231 |
| 1234 | `isPerformingJump_Basilisk: bool` | authoritative execution flag; `IsPerformingBasilisk`; writer Jump system | same proposed component/path; `status: proposed` | Set by Basilisk selection; movement speed/visual readers consume it; cancel/refresh clears. Effects remain explicit. | No equivalent. | `Player.cs:2112,12444-12458,16184-16188,20984`; report 1234 |
| 1235 | `isPerformingPogostickTricks: bool` | authoritative short-lived mount/jump execution flag; `IsPerformingPogostickTricks`; writer Jump system with Mount seam | same proposed component/path; `status: proposed`; `crossSubsystemOwner: integration-review` for mount-specific authority | `JumpMovement` sets it for mount type 43 while releasing jump in air; frame/pose/mount code reads it; clear on landing/reset/mount exit. Mount definition and pose effects remain external. | Current PlayerMount prototype has mount state but no pogo execution flag. | `Player.cs:2114,12172-12176,12284-12289`; report 1235 |

### 13.3 Compatibility, IDs, network and persistence

- Each execution flag is a mutually exclusive or explicitly combined phase fact; it is not a jump
  kind ID, mount ID, or network packet bit until protocol evidence says so. The selected kind and
  command ID remain separate from the bool fields.
- Down Dash may coexist with modifier timer state but cannot be re-entered while active. The other
  extra-jump flags are set only by the accepted JumpCommand and cleared by the documented cancel/
  refresh lifecycle.
- Dust, Gore, shader, sound, random visuals, mount pose, network and persistence are explicit
  adapters/ports. `DoubleJumpVisuals` must be a projection/effect system and cannot write flags.
- No C09 field is currently declared network or persistent authority. Prediction snapshots include
  execution flags and selected jump command; duplicate commands are rejected and rollback discards
  uncommitted effects.

### 13.4 Focused verifier plan

1. `ExtraJumpExecutionExclusive`: each kind selects the correct flag, clears conflicting flags and
   preserves the exact source priority order.
2. `DownDashExecutionLifecycle`: qualification, active time, mount cancellation, refresh and death
   reset match source behavior.
3. `JumpVisualEffectIsolation`: visual flags are read-only inputs; Dust/Gore/shader/sound/random use
   explicit effect ports and are deduplicated.
4. `PogostickMountBoundary`: mount type 43 sets/clears the flag without moving Mount ownership into
   Jump or writing pose state twice.
5. `ExecutionRollbackAndProjection`: snapshot restore and one-way network/save projection after a
   duplicate or rejected command.

### 13.5 C09 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: proposed Jump execution component/system under `src/Player/Jump/`; mount
  specific pogo behavior remains an integration seam.
- Dependency impact: Jump Availability supplies qualification; Jump owns execution flags; Mobility/
  Spatial consumes intents; Mount and Presentation consume explicit facts; effects are adapters.
- Rollback: retain legacy writer if execution priority, clearing, visual effects, mount behavior or
  down-dash lifecycle diverges; never dual-write.

### 13.6 Actual component implementation checkpoint

- Saved component source: `src/Player/Jump/PlayerJumpExecutionComponent.cs`.
- Implemented state: the 11 execution flags for Down Dash, the nine named extra jumps, and
  `IsPerformingPogostickTricks`.
- Dependency impact: Jump execution owns future flag transitions; Spatial, Mount, Presentation,
  effects, network, and persistence remain external. No Jump System, Mount seam, or effect port was
  added.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 14. Checkpoint C10: PlayerJumpMobilityModifiers

### 14.1 Boundary decision

这 5 个字段是 Jump 执行阶段的修正/短期结果，而不是额外跳跃资格或执行 flag。`downDashTime`
记录 Down Dash 已进行的 tick 数；`autoJump` 是装备/能力派生的输入策略；`justJumped` 是
当前移动 tick 的短期事实；`jumpSpeedBoost` 和 `extraFall` 是跳跃参数修正。建议由
`PlayerJumpMobilityModifiersComponent` 保存，并由 Jump modifier system 在 Jump execution
之后、Spatial commit 之前计算输出。它不得直接写共享速度/位置。

### 14.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 1207 | `downDashTime: int` | authoritative execution timer; `PlayerJumpMobilityModifiersComponent.DownDashTime`; writer Jump modifier system | `src/Player/Jump/PlayerJumpMobilityModifiersComponent.cs`; `Terraria.Player.Jump`; `status: proposed` | `Update` increments while `isPerformingJump_DownDash`, otherwise resets to 0; movement/achievement/effect paths read it. Clear on mount/death/refresh/spawn. Tick counter, not network sequence. | No equivalent. | `Player.cs:2058,14993-15004,17598-17605`; report 1207 |
| 1236 | `autoJump: bool` | derived input/mobility capability; `AutoJump`; writer equipment/effect system | same proposed component/path; `status: proposed` | Equipment paths set it; `JumpMovement` allows jump on held/released input when grounded/sliding; reset clears it. It is an input policy, not a jump execution result. | Current input has only supplied Jump bool and no auto-jump capability. | `Player.cs:2116,8172-8175,8197-8215,8434-8440,10749-10753,12160-12208`; report 1236 |
| 1237 | `justJumped: bool` | transient committed movement fact; `JustJumped`; writer Jump system | same proposed component/path; `status: proposed` | `JumpMovement` sets it when a grounded/sliding jump starts; wing/rocket reset logic reads it to restore flight/rocket time; clear/reset boundary must be explicit each tick. | No equivalent. | `Player.cs:2118,12204-12208,12284-12289,16351-16355,16435-16438`; report 1237 |
| 1238 | `jumpSpeedBoost: float` | derived jump parameter; `JumpSpeedBoost`; writer equipment/effect system | same proposed component/path; `status: proposed` | Equipment and `UpdateJumpHeight` add modifiers; final jump speed derives from base plus boost. Reset to 0 in `ResetEffects`; not final velocity or persistent Item data. | No equivalent. | `Player.cs:2120,8172-8175,11334-11359,10749-10753`; report 1238 |
| 1239 | `extraFall: int` | derived fall parameter; `ExtraFall`; writer equipment/effect system | same proposed component/path; `status: proposed`; `crossSubsystemOwner: integration-review` for final gravity/Spatial use | Equipment and `UpdateJumpHeight` add extra fall; gravity/fall logic consumes it; reset to 0 with effects. It must be passed to Spatial/Collision as a parameter, not write position directly. | No equivalent. | `Player.cs:2122,8172-8175,8197-8215,11339-11348,10749-10753`; report 1239 |

### 14.3 Compatibility, IDs, network and persistence

- `downDashTime` is a tick counter, not a command sequence or network identity. `justJumped` is a
  current-tick fact and should not be persisted. `jumpSpeedBoost`/`extraFall` are derived modifiers.
- `autoJump` must be recomputed after equipment/Buff reset and before Jump input resolution. The
  modifier system must publish a value snapshot; only the Spatial owner applies gravity/fall/velocity.
- No C10 field is declared network/persistent authority from current evidence. Prediction snapshots
  include timer and modifiers plus source revision; derived output is recomputed on rollback.
- Unknown equipment values, NaN/invalid boost values or duplicate JumpCommand fail closed. No Query
  invokes time, random, audio, logging or network effects.

### 14.4 Focused verifier plan

1. `DownDashTimerLifecycle`: increment/reset and mount/death/refresh boundaries.
2. `AutoJumpInputPolicy`: hold/release, grounded/sliding and source capability reset.
3. `JustJumpedTickFact`: one-tick set/clear semantics and wing/rocket restoration ordering.
4. `JumpModifierCalculation`: additive boost/fall values, equipment reset and finite-value validation.
5. `SpatialModifierHandoff`: final gravity/velocity writer consumes a snapshot once without component
   double-write.

### 14.5 C10 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: proposed Jump modifier component/system under `src/Player/Jump/`; no source
  file moves in this session.
- Dependency impact: Equipment/Buff supplies modifiers; Jump execution supplies phase facts; Spatial/
  Collision consumes immutable modifier output; Flight/Rocket consume `justJumped` result.
- Rollback: retain legacy modifier writer if timer, reset, parameter arithmetic or handoff order
  diverges; never dual-write.

### 14.6 Actual component implementation checkpoint

- Saved component source: `src/Player/Jump/PlayerJumpMobilityModifiersComponent.cs`.
- Implemented state: `DownDashTime`, `AutoJump`, `JustJumped`, `JumpSpeedBoost`, and `ExtraFall`.
- Dependency impact: the component stores jump modifiers and current-tick facts only. Equipment/Buff
  derivation, Jump ordering, Spatial/Collision application, Flight/Rocket interaction, network, and
  persistence remain external; the component does not write position, velocity, or gravity.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 15. Checkpoint C11: PlayerGrappleAndRocketState

### 15.1 Boundary decision

抓钩和火箭都属于移动穿越，但它们的关系和生命周期完全不同。`grappling` 是玩家到
Projectile 槽位的固定 20 槽关系，`grapCount` 是当前有效关系数量；这些值不能变成通用
EntityReference、持久化 ID 或网络 ID。火箭字段则拆成：`rocketBoots`/`vanityRocketBoots`
装备派生能力与表现选择、`rocketTime`/`rocketTimeMax` 资源容量、`rocketDelay`/`rocketDelay2`
阶段/节流计时、`rocketSoundDelay` 音效门控、`rocketRelease` 输入释放事实、`rocketFrame`
表现投影和 `canRocket` 一次性资格。建议 `PlayerGrappleRelationComponent` 与
`PlayerRocketStateComponent` 分开，由 `PlayerGrappleAndRocketSystem` 只协调命令顺序；最终
位置/速度和 Projectile 生成通过明确的 Spatial/Projectile ports 提交。

### 15.2 Full member mapping

| seq | Version4 member and type | proposed classification and owner | proposed path/namespace/status | Version4 readers, writers, lifecycle, state/effect | current NLTX | evidence |
|---:|---|---|---|---|---|---|
| 1246 | `grappling: int[]` length 20 | authoritative fixed-capacity player-to-Projectile relation; `PlayerGrappleRelationComponent.ProjectileSlots`; writer grapple attach/remove/clear system | `src/Player/Grapple/PlayerGrappleRelationComponent.cs`; `Terraria.Player.Grapple`; `status: proposed`; `crossSubsystemOwner: integration-review` for Projectile registry | Constructor/Spawn initialize invalid sentinel; `GrappleMovement` reads active slots and their Projectile positions; frame/visual paths average attached Projectile centers; `RemoveAllGrapplingHooks` clears and removes owned hook Projectiles. Values are legacy Projectile slots only, validated through Projectile relation port. | Existing `dome/.../PlayerGrappleStateComponent.cs` has 20 `int` targets, Count, attach/remove/clear; it is partial and must reconcile invalid-target and Projectile identity semantics. | `Player.cs:2136,13655-13788,17676-17678,19743-19754,20811-20826,26554-26557`; report 1246; `GetGrapplingForces` stub |
| 1247 | `grapCount: int` | authoritative relation count; `PlayerGrappleRelationComponent.Count`; writer same relation system | same proposed component/path; `status: proposed` | `GrappleMovement`, frame and cleanup use count; attach increments, removal/terminal cleanup decrements or resets; must remain 0..20 and match valid prefix/slot contract. Clear on spawn, death, disconnect and no-hook terminal path. | Existing component exposes `Count`; uniqueness, slot compaction and Projectile validation are not yet proven. | `Player.cs:2138,13687-13705,19748-19754,20818-20824`; report 1247 |
| 1248 | `rocketTime: int` | authoritative rocket resource; `PlayerRocketStateComponent.RocketTime`; writer rocket system | `src/Player/Grapple/PlayerRocketStateComponent.cs`; `Terraria.Player.Grapple`; `status: proposed` | Grounding/sliding restores to max; active rocket use decrements; wing/rocket transition can transfer time into wing time; clear when no boots. Tick resource, not Item definition or network sequence. | No rocket state. | `Player.cs:2140,16064-16071,16321-16327,16400-16410,16443-16477`; report 1248 |
| 1249 | `rocketTimeMax: int` | derived resource capacity; `PlayerRocketStateComponent.RocketTimeMax`; writer equipment/definition system | same proposed component/path; `status: proposed` | Default is 7; equipment/ability phase may establish capacity; grounding restores `rocketTime` to it. Keep capacity separate from current resource; no direct Item payload copy. | No equivalent. | `Player.cs:2142,16064-16068,16435-16442`; report 1249 |
| 1250 | `rocketDelay: int` | authoritative rocket phase/cooldown timer; `RocketDelay`; writer rocket system | same proposed component/path; `status: proposed` | Active rocket use sets delay, decrements it while thrusting, and controls `rocketFrame`/release behavior; reset when grounded, unavailable, grappled or no boots. It is not an audio delay or network sequence. | No equivalent. | `Player.cs:2144,13679-13685,16443-16504`; report 1250 |
| 1251 | `rocketDelay2: int` | transient rocket effect cadence timer; `RocketEffectDelay`; writer rocket/effect system | same proposed component/path; `status: proposed` | Rocket use and wing visuals set/decrement it for flame/sound cadence; reset on lifecycle. Sound/visual calls remain effect adapter. | No equivalent. | `Player.cs:2146,16449-16459,16483-16486,16727-16731,18040-18047`; report 1251 |
| 1252 | `rocketSoundDelay: int` | effect gate/projection state; `RocketSoundDelay`; writer effect adapter/system | `src/Player/Grapple/PlayerRocketEffectProjection.cs`; `Terraria.Player.Grapple`; `status: proposed`; `crossSubsystemOwner: integration-review` | Rocket use decrements it and sets it based on vanity boots before sound; no sound is emitted by a component. Reset on ground/no boots and prediction rollback. | No audio projection in current prototype. | `Player.cs:2148,16460-16481`; report 1252 |
| 1253 | `rocketRelease: bool` | authoritative input-release gate; `RocketRelease`; writer input/Jump/Rocket command system | `src/Player/Grapple/PlayerRocketStateComponent.cs`; `Terraria.Player.Grapple`; `status: proposed` | `JumpMovement` sets true when jump is released/rocket phase ends; rocket use requires it and clears/rearms at transitions. This is input edge state, not the equipment definition. | Current `ControlInputComponent` has Jump but no Version4 rocket release gate. | `Player.cs:2150,12275-12280,12551-12555,16443-16449`; report 1253 |
| 1254 | `rocketFrame: bool` | presentation/short-lived effect projection; `RocketFrame`; writer rocket effect system | `src/Player/Grapple/PlayerRocketEffectProjection.cs`; `Terraria.Player.Grapple`; `status: proposed` | Grapple/ground/jump paths clear it; active rocket delay sets it and calls `RocketBootVisuals`; render/effect paths read it. No gameplay or save authority. | No equivalent. | `Player.cs:2152,13681-13685,16068-16072,16487-16504`; report 1254 |
| 1255 | `rocketBoots: int` | derived rocket capability level; `PlayerRocketCapabilityComponent.BootLevel`; writer equipment system | `src/Player/Grapple/PlayerRocketCapabilityComponent.cs`; `Terraria.Player.Grapple`; `status: proposed`; `crossSubsystemOwner: integration-review` for equipment owner | Functional equipment assigns levels 1..5; movement/rocket logic gates ability and resets when no boots. It is a capability level, not the Item slot or vanity appearance. | No rocket capability component. | `Player.cs:2154,8189-8192,8655-8658,9011-9035,16208-16213,16324-16350`; report 1255 |
| 1256 | `vanityRocketBoots: int` | derived presentation/equipment appearance key; `PlayerRocketCapabilityComponent.VanityBootLevel`; writer vanity/equipment projection | same capability path; `status: proposed`; `crossSubsystemOwner: integration-review` | Vanity/armor paths assign visual boot levels; rocket sound/visual logic reads it for effect selection; reset when equipment changes. It must not grant functional rocket capability by itself. | No equivalent. | `Player.cs:2156,7017-7021,8190-8192,21025-21068`; report 1256 |
| 1257 | `canRocket: bool` | authoritative short-lived rocket qualification; `PlayerRocketStateComponent.CanRocket`; writer Jump/Rocket system | `src/Player/Grapple/PlayerRocketStateComponent.cs`; `Terraria.Player.Grapple`; `status: proposed` | Set while airborne in the valid velocity window; rocket use requires boots, release, delay and wing/down-dash constraints; clears when resource exhausted, grappled, grounded or invalid. No direct Projectile/velocity write in component. | No equivalent. | `Player.cs:2158,12275-12280,13681-13685,16347-16350,16443-16477`; report 1257 |

### 15.3 Compatibility, IDs, network and persistence

- `grappling[i]` values are validated legacy Projectile slot references. A relation adapter must check
  active state, owner and hook type before use; it must not merge slot index with Projectile identity,
  runtime EntityReference, replication ID or persistent ID. Stable ordering/compaction and duplicate
  attach semantics require a dedicated verifier.
- `grapCount` must equal the valid relation count and stay within 0..20. `RemoveAllGrapplingHooks`
  is a terminal command boundary; Projectile destroy/cleanup remains the Projectile owner.
- `rocketBoots` and `vanityRocketBoots` are equipment-derived capability/appearance keys. The Item
  definition and vanity slot remain external; `rocketTime` is a player resource and `rocketTimeMax`
  its capacity. Sound/frame delays are effect state, not durable gameplay facts.
- Case-13 movement packets do not prove wire coverage for grapple or rocket fields; persistence entry
  points do not prove a save byte format. All C11 network/save fields remain deferred and one-way.
- Prediction snapshots include fixed grapple relation, count, rocket timers, release/qualification
  state and source definition revision. Projectile/rocket commands are deduplicated by player/tick/
  command key; rollback discards uncommitted spawn, sound, dust and lighting effects.

### 15.4 Focused verifier plan

1. `GrappleRelation20Slot`: fixed capacity, invalid sentinel, attach/remove/clear, duplicate target,
   count consistency and slot compaction.
2. `GrappleProjectileIdentityBoundary`: validate slot/owner/type against Projectile registry and
   reject stale/reused slots; never treat slot as persistent or network identity.
3. `GrappleCleanup`: spawn, death, disconnect, terminal hook removal and `RemoveAllGrapplingHooks`
   clear exactly once and emit Projectile destroy commands through the proper owner.
4. `RocketResourceAndDelay`: max/current resource, ground restore, active delay, sound cadence and
   exhaustion transitions.
5. `RocketReleaseAndQualification`: release gate, airborne velocity window, wings/down-dash/grapple
   exclusions and duplicate input.
6. `RocketEquipmentProjection`: functional and vanity boot levels remain separate and source from one
   equipment revision.
7. `RocketEffectIsolationAndRollback`: frame/sound/dust/lighting/Projectile commands use ports and
   are not duplicated after prediction rollback.

### 15.5 C11 checkpoint decision

- Design status: `proposed`; component source implementation: recorded below; verification: `source-saved-player-build-blocked-5-errors`.
- Source/target plan: proposed Grapple relation, Rocket state/capability and effect projection under
  `src/Player/Grapple/`; reconcile existing NLTX 20-slot prototype instead of adding a second one.
- Dependency impact: Projectile owns hook entity/slot lifecycle; Equipment owns boot definitions;
  Jump/Flight/Carpet supply exclusions; Spatial owns final movement; Presentation/Audio owns effects;
  Network/Persistence remain adapters.
- Rollback: retain legacy writer if relation identity, cleanup, rocket timing, capability precedence,
  effect order or rollback behavior diverges; never dual-write.

### 15.6 Actual component implementation checkpoint

- Saved component sources:
  - `src/Player/Grapple/PlayerGrappleRelationComponent.cs`
  - `src/Player/Grapple/PlayerRocketStateComponent.cs`
  - `src/Player/Grapple/PlayerRocketCapabilityComponent.cs`
- Implemented state: a fixed 20-slot `int` Projectile relation initialized to `-1` with `Count`;
  rocket time, capacity, phase/effect delay, release, and qualification state; and functional versus
  vanity boot levels. `RocketTimeMax` retains the source default `7`.
- Deferred state: `rocketSoundDelay` and `rocketFrame` remain effect projection state. Slot attach,
  removal, cleanup, Projectile identity validation, rocket movement, effects, network, and persistence
  are not implemented in components.
- Verification: `source-saved-player-build-blocked-5-errors`; no focused verifier ran.

## 16. Cross-partition Integration Handoff

| Handoff | P07 surface | Decision not made by P07 |
|---|---|---|
| PlayerInventory/Equipment/Buff | armor-derived Beetle/Solar/Nebula and utility capability inputs | Item definition, equipment slot owner, Buff slot owner and effect reset order |
| PlayerCombat | Beetle offense/defense, solar shields/dash, `meleeEnchant`, inferno and rocket effects | Combat damage, immunity, cooldown, melee status and hit result writer; `crossSubsystemOwner: integration-review` |
| Spatial/Collision | dash, slope, stair, slide, carpet, wing and grapple position/velocity | canonical position/velocity/collision writer and tile/liquid semantics; `crossSubsystemOwner: integration-review` |
| Projectile | `grappling[20]`, `grapCount`, rocket spawn/release | Projectile entity/slot/identity registry and child-spawn command |
| Mount/Vehicle | flight, pulley, ground traversal and rocket capability interaction | mount definition, mount movement authority and mount-specific flight |
| PlayerPresentation | beetle/wing/pulley/carpet/rocket frames and sound/light/dust output | renderer, animation, audio, lighting and client-only projection owner |
| Network/Persistence | committed P07 snapshots and compatibility adapters | packet shape, server/client authority, save version and byte ordering |

## 17. Verification evidence and completion conditions

The final self-check confirms 11 leaf groups, 112 fields, 0 properties, unique source sequences
488..1257, every field mapped exactly once, all cross-domain owners marked, and component-only source
implementation saved under `src`. The required serial build was attempted with the command recorded
in the execution document and returned exit code `1`, 0 warnings, and 5 errors from unrelated
`src/Player/Progression` references (`Terraria.Relationships`, `Terraria.Projectile`,
`EntityReference`, and `ProjectileIdentityComponent`). The expected DLL path exists only as an older
artifact (`2026-09-12T02:04:19Z`) and was not refreshed by the failed build. Focused verifiers,
Systems, Queries, Commands, Adapters, Projections, effects, network and persistence remain deferred.
The authoritative P07 runner was settled with `Fail` using session
`557fbd2f1ba042b49770c6351375f1c3`; it returned exit code `1` and `lockReleased: true`.
