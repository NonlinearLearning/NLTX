# Projectile 完整生命周期：当前差距与实施设计

## 1. 文档目的与结论

本文基于当前 `D:\TRbackup\NLTX` 工作树、
`架构设计/Projectile-Player-NPC-彻底重构蓝图.md`、当前 Projectile/Protocol/Combat verifier，
以及 `docs/research/*projectile-*` 边界卡，给出提案第 4 节的差距盘点和可并行实施设计。

本轮设计已进入首批实现：新增 Tombstone 原因并接入三条已有 authoritative despawn 路径；其余方向仍保持设计态。

1. 仓库已经具备一个可运行的 Projectile 基础切片：定义注册、两种运动、扫掠固体碰撞、
   友好投射物对 NPC 的伤害、穿透、寿命、owner/identity、V1456 `SyncProjectile`/`KillProjectile`
   投影，以及带 revision 的单会话 PVS 游标。
2. 这套切片还不是“完整生命周期”。反射/反弹、类型特定命中盒、斜坡、液体交互、完整定义表、
   projectile-local immunity、hostile-player 路径、客户端特效协议边界和完整 Tombstone 语义仍未冻结或实现。
3. 并行开发前必须先冻结身份、状态、执行、证据四组公共契约。当前首批已冻结生命周期原因字段，第二批已加入
   单调 identity allocator 和 30-tick Tombstone retention policy；Projectile 的七个方向随后按
   明确 write-set 拆分，避免同时改动 `DomeSimulation`、定义组件和协议投影。
4. 完成标准不是“代码能编译”，而是每个行为都有旧源码 anchor、当前 owner、输入到快照的状态链、
   source-backed fixture、verifier 和新鲜的 Build diagnostic artifact。

## 2. 已阅读的参考材料

### 2.1 目标提案

- `架构设计/Projectile-Player-NPC-彻底重构蓝图.md`
  - 将 Projectile 建模为 `ProjectileMarker + Owner + Definition + Lifetime + Behavior + Motion + Collider + Damage`。
  - 行为链为 `Definition -> Spawn -> Behavior -> Movement -> Collision -> DamageRequestedEvent -> DamageResolution -> Lifetime`。
  - 网络边界要求消息 27 `SyncProjectile` 使用 `owner + identity` 匹配，消息 29 `KillProjectile` 使用
    `identity + owner` 销毁；`ai[0..2]` 只允许在协议适配层出现。
  - 目标模型明确禁止把静态默认值、生成器、AI 和网络字节读写放回 Projectile 实例。

### 2.2 当前边界卡

以下文档不是完整 parity 声明，而是已经验证的窄边界：

- `docs/research/2026-08-23-projectile-penetration-lifecycle-boundary.md`
- `docs/research/2026-08-24-projectile-definition-registry-boundary.md`
- `docs/research/2026-08-24-projectile-friendly-target-boundary.md`
- `docs/research/2026-08-24-projectile-hostile-player-boundary.md`
- `docs/research/2026-08-24-projectile-identity-boundary.md`
- `docs/research/2026-08-24-projectile-lifetime-domain-boundary.md`
- `docs/research/2026-08-24-projectile-motion-input-boundary.md`
- `docs/research/2026-08-24-projectile-penetration-domain-boundary.md`
- `docs/research/2026-08-24-projectile-replication-identity-boundary.md`
- `docs/research/2026-08-24-projectile-replication-scalar-boundary.md`
- `docs/research/2026-08-24-projectile-spawn-input-boundary.md`
- `docs/research/2026-08-24-projectile-tile-stop-boundary.md`

这些卡共同声明：当前是安全且可复现的子集，不宣称完整 Terraria projectile type/AI/network parity。

### 2.3 旧源码 oracle

边界卡记录的旧源码为：

`D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`

SHA-256：`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`

本设计使用的 anchor 区域：

| 行区间 | 领域事实 | 对应实施方向 |
|---:|---|---|
| 118、126-154 | identity、owner、timeLeft、friendly/hostile 等基础状态 | 身份、生命周期、目标资格 |
| 156、460-536 | penetration、默认字段、类型初始化 | 定义 registry、穿透 |
| 11480-11535 | `Damage_CanDealDamage` 的类型/AI/PVP 前置分支 | 目标资格、免疫、hostile-player 非目标 |
| 12858-12874 | 通用命中后的穿透递减和停止分支 | 穿透与 Tombstone |
| 14697、15232-15249、18706 | active/timeLeft/update guard | 生命周期和实体前置条件 |
| 15635-15704 | `Collision.TileCollision`、wet/slope/AI-style 分支 | 固体、斜坡、液体、反弹 |
| 16030-16060 | 线性运动分支 | 运动行为 |
| 16151-16166 | gravity 更新分支 | 重力行为 |

## 3. 当前实现证据与边界

### 3.1 定义 registry：已存在但不是完整定义表

当前 `src/Terraria.Dome.Simulation/Projectile/Definitions/ProjectileDefinition.cs:5-13`
只有以下字段：

```text
ProjectileType, BehaviorId, Damage, LifetimeTicks, Collider,
Friendly, Hostile, MaximumPenetration
```

`ProjectileDefinitionRegistry` 在 `.../ProjectileDefinitionRegistry.cs:11-46` 验证正数 type/behavior、
非负伤害、正寿命、有限正碰撞箱和 `-1` 无限穿透哨兵，并拒绝重复 type。
默认表目前只有 type 1 和 2（同文件 `:35-40`）。

缺失项：

- 类型特定命中盒的 offset、形状、方向和命中层级；
- tile collision policy、可穿透 tile、坡面策略、液体策略；
- bounce/reflect 次数、反射来源、速度响应；
- knockback、original damage、banner、协议 UUID 的 authoritative 来源；
- projectile-local immunity、目标类别和特殊伤害分支；
- 完整定义来源的版本、source hash、覆盖率和 unknown-type 处理。

设计上应保持 registry 为不可变 owner；行为系统只消费已解析 definition，不直接读取 legacy `aiStyle` 或
`Projectile` 默认字段。

### 3.2 生成、owner 和 identity：有校验，但输入契约仍偏宽

`ProjectileSpawnSystem.Spawn` (`.../ProjectileSpawnSystem.cs:12-49`) 已在 Arch 分配前校验 owner、identity、
有限坐标/速度、寿命、穿透域和 definition type 一致性，并写入 owner、network identity、definition、
penetration、transform、velocity、damage、lifetime 组件。

`DomeSimulation` 在生成提交阶段检查 owner 是否存在且 active，再分配 replication id（当前主链约在
`Simulation/DomeSimulation.cs:3529-3574`）。这能阻止未知或死亡玩家直接生成投射物。

仍有差距：

- `SpawnProjectileCommand` 同时携带 `Damage`、`LifetimeTicks`、`BehaviorId`、`MaximumPenetration`，
  允许客户端意图携带本应由 definition 决定的权威字段；
- identity、replication id、可选 UUID 的分配规则没有独立的不可变 allocator 合同；
- 当前没有 source sequence、expected revision 或 command provenance，无法审计“谁在何 tick 生成”；
- `ProjectileUuid` 在 spawn 处为 `null`，协议快照另外保留 `short Uuid`，两种 UUID 语义尚未统一；
- 生成失败目前以丢弃/拒绝为主，没有结构化的 rejection evidence。

### 3.3 运动和碰撞：只覆盖线性/重力与固体停毁

`ProjectileBehaviorSystem` 将 `BehaviorId` 映射到 `LinearProjectileBehavior` 和
`GravityProjectileBehavior`；两者会拒绝非有限 transform/velocity 和非法 tick。

`ProjectileCollisionSystem` (`.../ProjectileCollisionSystem.cs:7-89`) 使用 axis-aligned collider 对 tile
做离散扫掠，越界或 active tile 都视为命中。`DomeSimulation.DetectProjectileHits` 在命中固体时排队
`DespawnEntityCommand`，因此能证明“不能穿过 authoritative solid tile 伤害后方 NPC”。

仍有差距：

- collider 来自通用 `ColliderComponent`，没有每种 projectile 的 hitbox fixture；
- tile 命中只有 stop/despawn，没有法线、反射、反弹次数、速度衰减或 transform；
- `WorldTile` 的 slope/liquid 数据尚未进入 Projectile collision contract；
- wet、honey、lava、shimmer、半砖、斜坡、actuated/door/special tile 仍未分类；
- current path 在同一检测函数内混合 tile stop 和 NPC candidate，后续扩展容易违反确定性顺序。

### 3.4 伤害、穿透和免疫：只覆盖 friendly -> NPC 的窄路由

`ProjectileTargetEligibilitySystem.CanDamageNpc` 仅允许 `Friendly == true` 的 NPC 路径。
`ProjectileDamageSystem.Resolve` (`.../ProjectileDamageSystem.cs:16-56`) 按 projectile identity、target identity
排序，检查 entity alive、共享 hit immunity 和 penetration 域，递减正穿透并生成
`DamageRequestedEvent`。

`DomeSimulation` 随后提交 damage command；穿透到 0 时排队 projectile despawn。现有 fixture 已覆盖：

- one-penetration projectile 只造成一次 NPC 伤害并在下一 committed snapshot inactive；
- invalid penetration（0、低于 -1）被拒绝；
- forged penetration below -1 不会进入 damage route；
- friendly projectile 可以命中 NPC，hostile projectile 不会进入该 generic route。

仍有差距：

- shared immunity 不是 projectile/type/owner 维度的完整免疫模型；
- 不支持类型特定 `stopsDealingDamageAfterPenetrateHits`、penetration randomization、反射后来源重写；
- hostile -> player、PVP、owner immunity、目标阵营和特殊伤害前置未实现；
- 伤害来源、original damage、knockback、banner 的 authoritative component 尚未完整接入；
- death/lifecycle 事件与 projectile kill 的因果链没有独立事件模型。

### 3.5 生命周期与 Tombstone：有 inactive snapshot，但语义尚未冻结

`ProjectileLifetimeSystem.Advance` (`.../ProjectileLifetimeSystem.cs:10-27`) 将剩余 ticks 递减到 0，
并在过期时返回 true。`ProjectileReplicationSnapshot` 记录 `IsActive`、`Revision`、位置、速度、owner、
identity、UUID 和协议可投影字段。`DomeSimulation` 在 despawn commit 时更新 inactive revision，
`CombatReplicationAssembler` 再把它投影为 `KillProjectile`。

这已经支持“活体 -> inactive revision + reason -> KillProjectile”的基本链路；首批原因已覆盖
expired、tile hit、penetrated 和 behavior rejected，但仍缺少：

- reflected 的原因、持久化 tombstone record，以及 admin/reject 的外部审计语义；
- Tombstone 保留时长、重复发送、迟到包和 identity reuse 规则；
- owner + identity + UUID 的一致性校验和冲突处理；
- 服务器内部销毁与客户端请求 `KillProjectile` 的明确方向区分；
- projectile 是否进入 persistence 的明确非持久化合同。

### 3.6 V1456 协议：字节投影存在，客户端伪造边界尚不完整

`ProjectileSyncPacket` 定义了 identity、pixel position/velocity、owner、type、稀疏 ai0/1/2、banner、
damage、knockback、original damage、uuid。`TerrariaPacketCodec.EncodeProjectileSync` 位于
`.../TerrariaPacketCodec.cs:1860-1963`，按照消息 27 的字段顺序和 sparse flags 编码；
`EncodeProjectileDespawn` 位于 `:1965-1984`，按消息 29 编码 identity + owner。

`DecodeClientProjectileTermination` 和 session ownership 检查能拒绝错误 owner 的客户端 KillProjectile；
当前 protocol verifier 已覆盖 sparse flags、消息号、字节坐标、owned termination、trailing data 和 forged owner。

仍有差距：

- 客户端 `SyncProjectile` 仍被当作可接受的双向消息入口，尚未形成“客户端只能提交生成/意图，不能写入
  server-owned state”的完整 command adapter；
- decode 后尚未按 authoritative owner/identity/UUID 查找 ECS 实体并验证 type/definition；
- `ai[]` 的 typed behavior state 到协议字段的映射尚未实现，目前 snapshot 的 Ai0/Ai1/Ai2 是裸 scalar；
- client-only VFX（声音、粒子、light、trail）没有独立 projection/不变量。

### 3.7 PVS：有 revision cursor，但需要两会话权威验收

`CombatReplicationAssembler` 按 session visible sections 过滤 active projectile；仅对已经发送过的 inactive
projectile 发送 KillProjectile；`CombatReplicationCursor` 以 replication id + revision 去重。
Protocol verifier 已证明 hidden projectile 不发送、visible revision 变化会发送、inactive visible projectile
会投影 KillProjectile。

Loopback verifier 已证明一个 observer 可收到 projectile sync/despawn，而远离战斗的 hidden session 不应收到
目标 projectile/NPC combat frame。

仍有差距：

- 两会话验收目前依赖完整 server loop，缺少稳定的 fixture 记录（session section、输入 tick、发送序列）；
- projectile 从 visible section 移到 hidden section、再移动回 visible section 的 cursor 语义未单独覆盖；
- tombstone 对“曾经 visible 但当前已离开 PVS”的会话如何发送尚未写成固定合同；
- 只用 revision 去重，尚未检查 tick/source sequence 的单调性和跨 identity reuse 隔离。

## 4. 并行开发前冻结的四组公共接口

### 4.0 首批已实现：Tombstone 原因

`ProjectileTombstoneReason` 已加入 simulation snapshot，且 `DespawnEntityCommand` 携带原因到
deterministic commit。当前已接入三条可复现路径：`Expired`、`TileHit`、`Penetrated`；行为拒绝使用
`BehaviorRejected`，未标注的内部销毁回退为 `Administrative`。协议字节不变，KillProjectile 仍只从
authoritative inactive snapshot 投影 identity + owner。

首批验收由 `Test/Terraria.Dome.Combat.Verification` 的单个高价值 verifier 覆盖三种原因，未扩展
完整 projectile parity；反射、液体、PVP、免疫和 tombstone retention 仍是后续批次。

### 4.1 第二批已实现：Identity allocator

`ProjectileIdentityAllocator` 是 server-owned、单调且不回收的 identity 分配器。有效域为
`1..int.MaxValue - 1`；到达耗尽边界后拒绝新 identity，避免 identity reuse 让迟到的
`KillProjectile` 或 `SyncProjectile` 绑定到新实体。它替代了 `DomeSimulation` 中不可单测的裸自增字段。

这一批只证明单进程生命周期内的分配顺序、耗尽行为和 snapshot retention 边界；跨进程恢复和 UUID
generation 仍 deferred。`DomeSimulationSnapshot` 现在保留 `NextProjectileIdentity` 并在内存恢复时
恢复 allocator 游标；磁盘格式尚未写入该字段，因为当前工作树的 v32 sign-tombstone 尾部仍需要先
稳定其兼容性。

### 4.2 第三批已实现：客户端 SyncProjectile 只校验并忽略

协议 dispatcher 仍完整解析客户端 `SyncProjectile` 并校验 owner，但返回
`ClientProjectileSyncIgnored`，不再把客户端包报告为 authoritative active synchronization，也不
写入 server-owned projectile state。消息 27 的 wire bytes 不变；跨 slot、非法长度和非有限字段仍
由既有 decoder/session guard 拒绝。

### 4.3 第四批已实现：Projectile-local immunity

`HitImmunityComponent` 现在同时保留 NPC 通用免疫和 `(ProjectileIdentity, TargetIdentity)` 局部免疫。
`ProjectileDamageSystem` 只使用局部键：同一 projectile 重复命中同一 target 会被阻止，不同 projectile
命中同一 target 不再互相误伤。tick 清理两类 immunity，其他 NPC/player damage 路径保持不变。

### 4.4 第五批代码：Hostile player eligibility route

定义 registry 增加 hostile-only fixture type `3`，目标资格系统新增 `CanDamagePlayer`；检测代码已
分出 hostile projectile 的 player candidate 分支，跳过 owner、要求 active player，并复用现有
`DamagePlayerCommand` commit。当前 verifier 只锁定 eligibility（hostile=true、friendly-only=false）；
完整移动/碰撞到 player health 的 integration fixture 仍需单独稳定，PVP 和 owner immunity 继续 deferred。

该 integration fixture 已稳定：hostile type `3` 在非 owner player 的碰撞盒内生成，经过 authoritative
candidate、`DamagePlayerCommand` commit 后目标 health 从 `100` 变为 `90`，owner 保持 `100`。
这只覆盖 hostile non-PVP player damage，不代表完整 PVP/owner immunity parity。

### 4.5 第六批代码：客户端 KillProjectile 受限请求

客户端 `KillProjectile` 仍进行完整 frame、identity、owner 和 session 校验，但 dispatcher 返回
`ClientProjectileTerminationIgnored`。服务器只会从 authoritative inactive snapshot 产生真正的
KillProjectile projection；客户端请求不会被报告为销毁事实，也不会反向改变 ECS 或 tombstone。

以下契约必须在分支并行前冻结。冻结只允许增加版本化字段，不允许各方向私自改含义。

### 4.1 身份契约

```csharp
PlayerHandle            // owner/account identity; only server resolves validity
NpcHandle               // simulation target identity
ProjectileReplicationId // server-assigned monotonic replication address
ProjectileNetworkIdentity
  Owner
  Identity
  OptionalUuid
Item/TileEntity ids     // retained for shared snapshot and PVS conventions
```

约束：

- ECS `Entity` 不是协议 identity；`Owner + Identity` 在 projectile 生命周期内稳定。
- replication id、protocol identity 和 optional UUID 必须分别命名、分别验证，不能隐式互换。
- 服务器是 owner、identity、type、definition、lifecycle、UUID 的唯一写者；客户端输入只能引用它们。
- Tombstone 必须保留 owner、identity、replication id、revision、death reason 和最后 authoritative position。
- identity reuse 必须等待 tombstone retention 结束，或携带 generation/UUID 以阻止迟到包复活旧实体。

### 4.2 状态契约

所有对外状态必须是 immutable snapshot。建议增加共享 envelope（名字可按现有项目命名调整）：

```text
AuthoritativeStateMetadata
  TickNumber       // world clock tick at commit
  Revision         // entity state revision, starts at 1
  SourceSequence   // deterministic command/event sequence
  ExpectedRevision // input-side compare-and-apply guard; -1 means no precondition
```

Projectile snapshot 至少应稳定包含：

```text
ReplicationId, Owner, Identity, OptionalUuid, ProjectileType
Position, Velocity, Hitbox/DefinitionKey
Damage, OriginalDamage, Knockback, PenetrationState
RemainingLifetime, IsActive, TombstoneReason
Section, Metadata
```

协议 DTO 只投影需要的字段；不得把 snapshot 的 `Ai0/Ai1/Ai2` 重新变成 simulation 公共状态。

### 4.3 执行契约

Projectile 的每个行为必须遵循同一条链：

```text
Input
  -> Validation
  -> System
  -> Command/Event
  -> Deterministic Commit
  -> Immutable Snapshot
  -> Protocol/PVS Replication
```

推荐固定 tick 内顺序：

```text
1. accept/validate spawn and client intents
2. resolve definition and create authoritative projectile
3. advance typed behavior and motion
4. detect tile/slope/liquid collision and produce collision command
5. detect target overlaps and produce DamageRequestedEvent
6. resolve damage, immunity and penetration in sorted identity order
7. commit damage, bounce/reflect, lifetime and Tombstone commands
8. build snapshots and then run per-session PVS projection
```

系统不得直接发网络包；协议层不得直接调用 `SetDefaults`、`AI()` 或 `Update()`。

### 4.4 证据契约

每个方向必须交付同一组 artifact：

```text
docs/research/2026-08-24-projectile-<boundary>.md
Test/Terraria.Dome.Combat.Verification/Fixtures/<ProjectileFixture>.cs
Test/<area>.Verification/Program.cs
Build/diagnostics/projectile/<run-id>/
  build.log
  evidence.json
  diff.txt (if source-backed comparison exists)
  runtime.log (for loopback/real-client runs)
```

`evidence.json` 至少记录：旧源码 path/hash/anchor、当前 owner、输入 command、validation result、
system output、commit sequence、snapshot before/after、wire message、PVS session/section。

绿灯测试不能替代 provenance；大删除、大行为替换或网络字段变更必须有 source-backed evidence。

## 5. 推荐目标架构

### 5.1 Simulation 内部模型

保留现有组件化方向，并补充类型化能力：

```text
ProjectileMarkerComponent
ProjectileDefinitionComponent
ProjectileOwnerComponent
ProjectileNetworkIdentityComponent
ProjectileBehaviorComponent<TypedState>
TransformComponent + VelocityComponent
ProjectileColliderComponent (definition key + shape/offset)
ProjectileDamageComponent
ProjectilePenetrationComponent
ProjectileImmunityComponent
ProjectileLifetimeComponent
ProjectileFluidContactComponent
ProjectileLifecycleComponent (active/tombstone reason)
```

`ProjectileDefinition` 建议拆出只读 definition facts：

```text
DefinitionKey
BehaviorId
CollisionPolicy
HitboxSpec
MotionSpec
DamageSpec
PenetrationSpec
LifetimeSpec
FluidSpec
ReplicationSpec
```

具体 `Linear`、`Gravity`、`Bounce`、`Reflect` 行为通过 `IProjectileBehavior` 或等价注册接口实现；
行为 state 是具名字段，协议 adapter 才负责映射到 ai0/1/2。

### 5.2 Tile、斜坡和液体边界

不要在 `ProjectileCollisionSystem` 里直接读取所有 legacy tile 细节。推荐建立只读查询：

```text
TileCollisionSnapshot
  IsSolid
  SlopeKind
  IsHalfBlock
  LiquidKind
  LiquidAmount
  IsActuated/IsDoorLike (if supported)
```

碰撞系统输出 typed result，而不是直接改 projectile：

```text
ProjectileCollisionResult
  Kind: None | Solid | Slope | Liquid | Boundary
  ContactNormal
  SurfaceVelocity
  FluidEffect
  SuggestedVelocity
```

随后由 deterministic commit 决定 stop、bounce、reflect、slow、destroy 或继续运动。这样可以为每种
projectile type 绑定策略，同时保持 tile 查询和 projectile 行为的 ownership 分离。

### 5.3 Tombstone 与复制

当前先把 `TombstoneReason` 作为 snapshot 字段；后续需要 retention/迟到包合同后，再独立为
显式 `ProjectileTombstone` record：

```text
ProjectileTombstone
  ReplicationId
  Owner
  Identity
  OptionalUuid
  Revision
  TickNumber
  LastPosition
  Reason
  SourceSequence
```

服务器只从 authoritative inactive snapshot 生成 `KillProjectile`。客户端收到 `KillProjectile` 只能移除
本地表现实体，不能反向改变 server snapshot。客户端自发的 termination 包只能进入受限的“请求停止”路径，
不能直接提交死亡、owner、identity 或 revision。

### 5.4 客户端表现边界

客户端表现应建立在服务器投影之上：

```text
Server snapshot -> SyncProjectile/KillProjectile -> client visual state
                                     \-> VFX hint/event (optional, non-authoritative)
```

VFX hint 不得携带可影响 simulation 的 damage、position authority、penetration 或 lifecycle；丢失 VFX
不影响服务器结果。当前版本可以先只记录“客户端表现未纳入 authoritative contract”，不要伪造完整特效 parity。

## 6. 分阶段实施与 write-set

下表是建议的七个并行方向。每一行都有独立 owner 和不重叠的主要写集；公共契约冻结前不要开始这些分支。

| 方向 | 主要 owner | 主要 write-set | 交付 |
|---|---|---|---|
| A. Definition registry/type hitbox | `Projectile/Definitions` | `ProjectileDefinition*`、definition fixtures、definition research | 版本化 definition table、hitbox spec、unknown-type fail-closed |
| B. Motion/collision | `Projectile/Behaviors`、`ProjectileCollisionSystem` | behavior/collision files、tile query/result types | 类型命中盒、扫掠、固体、斜坡、反弹/反射、液体策略 |
| C. Damage/penetration/immunity | `ProjectileDamageSystem`、Combat systems | damage/penetration/immunity/events | 目标资格、局部免疫、穿透、伤害事件和 deterministic order |
| D. Spawn/identity/lifecycle | `ProjectileSpawnSystem`、`DomeSimulation` projectile commit | spawn command/allocator/lifecycle/tombstone | server-owned owner/UUID/identity、生成输入、寿命、击杀原因 |
| E. Protocol projection | `Protocol.V1456/Packets` | `ProjectileSyncPacket`、codec、projector、session input adapter | 消息 27/29 字节合同、typed AI projection、伪造输入 fail-closed |
| F. PVS replication | `Server/Replication` | combat assembler/cursor/session fixture | 两会话 visible/hidden/enter/leave/tombstone 复制规则 |
| G. Evidence/client boundary | `Test/Terraria.Dome.Combat.*`、`docs/research` | fixtures、verifiers、research docs、diagnostic scripts | source-backed fixtures、runtime evidence、VFX 非权威声明 |

跨方向禁止事项：

- A 不修改协议 DTO；E 不修改 simulation definition 默认值。
- B 不直接扣血或发 KillProjectile；C 不改变 tile 查询实现。
- D 不直接编码网络字节；F 不改变 authoritative simulation state。
- G 不通过放宽断言来“修绿”其他方向；失败应记录为 evidence。

## 7. Source-backed fixture 与验收矩阵

每个 fixture 需要给出最小输入、权威 commit、snapshot before/after 和必要 wire/PVS 输出。

| 行为 | 旧源码 anchor | 最小 fixture | 必须断言 |
|---|---|---|---|
| definition lookup | 460-536、156 | type 1/2/unknown/duplicate | known type 解析；unknown 拒绝；定义不可变 |
| type hitbox | 118、460-536 | 两种不同尺寸/offset 的 projectile | overlap 只由对应 hitbox 决定，不由通用默认值决定 |
| linear/gravity | 16030-16060、16151-16166 | finite、overflow、tick sequence | 合法推进；非有限输入不突变 |
| solid tile stop | 15635-15704 | projectile -> solid tile -> NPC | tile 前停止/销毁，后方 NPC 不受伤 |
| slope | 15635-15704 | half-block/four slope fixtures | 接触法线和策略确定；不穿坡、不误伤 |
| bounce/reflect | 15635-15704、12858-12874 | single/multi bounce、reflect owner | 速度、次数、来源和 owner 由服务器提交 |
| liquid | 15635-15704 | water/lava/honey/shimmer | 每种液体策略显式；未支持液体 fail-closed |
| penetration | 12858-12874 | 1、2、-1、invalid | 伤害次数和剩余穿透确定；到 0 产生 tombstone |
| immunity/damage | 11480-11535 | same target/same tick/type | 免疫按定义规则生效；hostile 不自动进入 NPC 路径 |
| lifetime | 138、15232-15233 | 1 tick、expiry、overflow | 只在 authoritative tick 过期；原因写入 tombstone |
| spawn authority | 126-138、535-536 | unknown owner/dead owner/forged damage | 客户端不能决定 owner、type、damage、lifetime |
| identity/UUID | 118、14697、18706 | identity reuse/late kill | owner+identity(+UUID) 匹配；迟到包不能复活 |
| SyncProjectile | blueprint message 27；codec 1860-1963 | sparse ai/banner/damage/uuid | 字节顺序、pixel conversion、flags 与 typed state 一致 |
| KillProjectile | blueprint message 29；codec 1965-1984 | active/inactive/forged owner | 只有 inactive server snapshot 能投影 kill |
| PVS | current assembler/cursor | visible, hidden, leave, re-enter, two sessions | 只复制可见权威状态；已发送实体才收到 tombstone |
| client VFX | blueprint client boundary | dropped/duplicated VFX hint | VFX 不影响 snapshot、damage、lifecycle |

## 8. 推荐验证命令与诊断产物

仓库 `global.json` 固定 .NET 10 SDK；串行构建应从仓库根目录执行，并使用
`-p:UseSharedCompilation=false`，必要时同时使用 `-p:MSBuildNodeReuse=false -m:1`。

建议每个方向使用新鲜 run id 和独立 artifact 路径，例如：

```powershell
$run = "projectile-20260824-definition-v1"
$diag = "Build/diagnostics/projectile/$run"
New-Item -ItemType Directory -Force $diag | Out-Null

dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj `
  -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -m:1 `
  *> "$diag/build-simulation.log"

dotnet run --project Test/Terraria.Dome.Combat.Verification/ `
  -c Release --no-restore -p:UseSharedCompilation=false `
  *> "$diag/runtime-combat.log"

dotnet run --project Test/Terraria.Dome.Combat.Protocol.Verification/ `
  -c Release --no-restore -p:UseSharedCompilation=false `
  *> "$diag/runtime-protocol.log"

dotnet run --project Test/Terraria.Dome.Combat.Loopback.Verification/ `
  -c Release --no-restore -p:UseSharedCompilation=false `
  *> "$diag/runtime-loopback.log"
```

实现阶段每次运行还应输出 `$diag/evidence.json`，至少包含：

```json
{
  "source": { "path": "...Projectile.cs", "sha256": "...", "anchors": ["..."] },
  "owner": "ProjectileCollisionSystem",
  "input": { "tick": 12, "sourceSequence": 4 },
  "validation": "accepted",
  "commit": { "commands": ["BounceProjectileCommand"], "revision": 3 },
  "snapshot": { "isActive": true, "section": "10,1" },
  "replication": { "message": "SyncProjectile", "session": "observer" }
}
```

本文写作阶段没有重新执行 build、combat verifier 或 loopback verifier，因此不能把当前工作树声明为
“本轮验证通过”；以上命令是实施阶段的基线和 artifact 规范。

## 9. 实施顺序与闸门

建议按以下顺序落地：

1. 先冻结四组公共契约和 `ProjectileTombstone`/metadata 形状；不改变 wire bytes。
2. 先做 A、D：definition/identity/lifecycle 是 B、C、E、F 的输入来源。
3. 做 B、C：碰撞结果与伤害事件必须在同一 deterministic commit 顺序中闭合。
4. 做 E：只把 typed state 投影到已有 V1456 字节格式，不让协议字段反向成为 simulation state。
5. 做 F：用两会话 fixture 覆盖 visible/hidden/leave/re-enter/tombstone。
6. 做 G：补齐每种行为的 source-backed evidence 和 research boundary；客户端特效只作为非权威层。
7. 最后执行 serial build、focused verifier、loopback verifier，并核对 diagnostics 中没有未解释的失败。

每个阶段的完成条件：

- changed block 遵守 `约束/Google-CSharp-Style-Guide-约束.md`；
- 有最小 source-backed fixture，而不是只增加宽泛单元测试；
- 有输入、validation、system、command、commit、snapshot、replication 的链路记录；
- owner、identity、UUID、revision、tombstone 均由服务器决定；
- 失败路径不会分配实体、扣血、发权威 kill 或污染 revision；
- 没有把 deferred 的 hostile-player、完整类型表或客户端特效误报成已完成。

## 10. 明确非目标

本设计不承诺：

- 一次性重写 legacy `Projectile.cs` 的全部数百种 AI；
- 在没有 source-backed definition 的情况下猜测完整 Terraria projectile type 表；
- 仅凭 hostile flag 实现 PVP/player damage；
- 把客户端粒子、声音、光照或 trail 作为 authoritative state；
- 将 volatile projectile 状态加入当前 WLD persistence，除非另有持久化合同；
- 为了满足绿灯而删除或放宽现有 owner、finite scalar、identity 和 PVS 安全验证。

## 11. 当前工作树状态

本文只新增本文件。现有未提交的 worldgen 文件和 `Test/Terraria.Dome.WorldGeneration.Verification/Program.cs`
等改动不属于 Projectile 设计范围，应在后续实现或提交时保持原样，不得回滚。

## 12. B9 已实现节点：Tombstone 复制保留期

`CombatReplicationAssembler` 现在可以接收 authoritative `currentTick`。对于带有
`TombstoneRetainedUntilTick` 的 inactive projectile：

- `currentTick < TombstoneRetainedUntilTick` 时，保留既有 replication cursor 语义并投影
  `KillProjectile`；
- `currentTick >= TombstoneRetainedUntilTick` 时，不再发出过期 tombstone；
- 截止点为 `0` 的历史 fixture 继续沿用原有行为，避免旧快照被静默丢弃。

服务器复制循环传入 `_simulation.TickNumber`，协议 verifier 已覆盖 retained/expired 的边界。
本节点不改变 wire format，也不解决跨进程 identity cursor、反射、液体或 PVP 等 deferred 项目。

## 13. B10 已实现节点：identity cursor 磁盘恢复

`DomeStatePersistenceFormat` 在现有 sign tombstone 尾部追加 `DomeSimulationSnapshot.NextProjectileIdentity`。
读取时对范围 `1..int.MaxValue-1` 做严格校验；没有该可选尾字段的历史状态回退到 `1`，因此不需要
改变协议字节或破坏已有持久化版本读取。服务器从恢复的 snapshot 构造 simulation 后，allocator 会继续
使用恢复的 cursor，避免重启后重新分配旧 projectile identity。

Persistence verifier 已覆盖当前 round-trip 和移除 cursor 尾字段的 legacy default。

## 14. B11 已实现节点：authoritative bounce

新增 type `4` 的单次 bounce definition，并通过 `ProjectileBounceComponent` 保存剩余次数。
tile collision 检测阶段只产生 `BounceProjectileCommand`，commit 阶段才修改速度和安全位置：

- 水平/垂直运动轴分别决定反射分量；
- 反射后位置回退到碰撞前安全采样点，避免下一 tick 仍位于固体 tile 内；
- 次数递减到零后，下一次 solid hit 仍按 `TileHit` 产生 tombstone。

这保持了“检测 -> command -> deterministic commit -> snapshot/replication”的生命周期链路，
没有把客户端输入或协议字段提升为 bounce authority。combat verifier 已覆盖反射和反射后继续移动。

## 15. B12 已实现节点：liquid policy

新增 `ProjectileLiquidPolicy`：默认定义为 `Pass`，type `5` 显式使用 `Destroy`。检测阶段读取
projectile collider 覆盖区域内的 `WorldTile.LiquidAmount`，命中后产生 `DespawnEntityCommand`
和 `LiquidHit` tombstone；liquid world state 不由 projectile 路径直接修改。

combat verifier 已覆盖 destroy-on-liquid 的 authoritative inactive snapshot；随后串行运行的
protocol/persistence verifier 也保持通过。

## 16. B13 已实现节点：slope impact axis

projectile collision 复用现有 `WorldTile.Slope` 编码，不把坡面当成普通 full tile：

- slope `1/2`（top-slope）选择垂直反射轴；
- slope `3/4`（bottom-slope）选择水平反射轴；
- `IsHalfBrick && Slope == 0` 选择垂直反射轴；
- 未命中上述几何时沿原有 swept movement axis 反射。

该节点只改变 `BounceProjectileCommand` 的轴选择，仍由 commit 阶段修改速度和安全位置。
combat verifier 已覆盖 top-slope 的轴判定，三套 focused verifier 串行通过。

## 17. B14 已实现节点：slope boundary closure

验证覆盖已扩展为 top-slope、bottom-slope 和 half-brick 三个几何边界，分别断言垂直、水平、
垂直反射轴。期间修复了工作树中 `NpcProjectileSpawnSystem` 的编译边界：显式别名
`Arch.Core.World`，并引用正确的 `Terraria.Dome.Simulation.SimulationVector` 所在命名空间。

combat、protocol、persistence 三套 Release verifier 已再次串行通过。

## 18. B15 已实现节点：owner immunity contract

`ProjectileTargetEligibilitySystem.CanDamagePlayerTarget` 现在统一表达 player target 边界：

- projectile 必须是 hostile；
- target handle 必须有效；
- target 不得等于 projectile owner；
- 其他合法 player 才能进入 damage candidate。

`DomeSimulation.DetectProjectileHits` 使用该 contract，owner immunity 不再只由局部循环条件
隐式保证。该节点仍保持当前 non-PVP hostile route，尚未开启 PVP。

## 19. B16 已实现节点：PVP opt-in policy

新增 `PlayerDamagePolicy`：`HostileNonPvp` 保持 type `3` 的现有 hostile route；`PvpOptIn`
用于 type `6`，只有 authoritative `WorldRuleState.IsPvpEnabled` 为 true 才能命中 player；
`None` 不进入 player damage route。因此 hostile flag 不再隐式打开 PVP，默认世界规则仍关闭 PVP。

## 35. B32 已实现节点：retention-aware sidecar compaction

`NpcProjectileCursorSaveCoordinator.Compact` 在 sidecar 写入前接收 authoritative NPC projectile
snapshots 和 current tick，只保留 active projectile 与 retention 尚未到期的 tombstone entries；
缺失或已过 retention 的 replication ID 从所有 account cursor 中删除。`DomeServer.Dispose` 在
保存 sidecar 前执行 compaction，避免重启 replay 恢复历史陈旧 cursor。

本批验证了 compaction 规则和 server/persistence gates；下一批补非空 account 的实际重启 replay
以及 compaction 中断后的恢复组合场景。

## 36. B33 已实现节点：non-empty restart replay combination

Persistence verifier 现在覆盖完整 sidecar 组合：非空 account cursor save，load，按 authoritative
projectile/tick compaction，重新 save，再模拟主文件损坏和有效 `.tmp` recovery。结果保留
retention 内 cursor、删除过期 cursor，并在中断恢复后保持同一 account/revision；没有重新激活
已被 compaction 删除的 replication ID。

这完成了 projectile cursor 的进程重启和 crash-recovery 组合证据；完整 Terraria projectile
definition parity、客户端视觉特效和其他 deferred protocol parity 仍不在本目标已实现范围内。

## 33. B30 已实现节点：DomeServer sidecar lifecycle binding

`DomeServer.ConfigureNpcProjectileCursorPersistence(path)` 成为显式 startup 配置边界：调用时
加载 account-keyed NPC projectile cursor；必须在 `Start` 前配置。`Dispose` 在停止 listener、等待
session tasks 和 simulation task 后，通过 `NpcProjectileCursorSaveCoordinator` 原子保存最新
cursor。未配置 path 的既有 server 保持纯内存行为。

Persistence focused verifier 已覆盖配置 server 生命周期创建 sidecar、二次 server 加载 sidecar
以及空 cursor round-trip；下一批仅需继续审计进程崩溃/中断 shutdown 时的临时文件恢复和原子性。

## 34. B31 已实现节点：sidecar crash recovery

`NpcProjectileCursorSaveCoordinator.Load` 现在先读取并严格验证主 sidecar；主文件损坏或缺失时
尝试 `<path>.tmp`。若 candidate 有效，则 promotion 为主文件并删除临时残留；若两者都无效则
显式失败，不返回空 cursor，避免崩溃恢复时静默丢失 NPC projectile revision state。有效主文件
读取也会清理旧 `.tmp`。

Persistence verifier 已覆盖主文件损坏 + tmp 恢复和双无效拒绝；下一批处理 retention-aware
sidecar compaction 与 restart replay，不再重复修改 world persistence 格式。

## 32. B29 已实现节点：account cursor sidecar persistence

新增独立 `NpcProjectileCursorPersistenceFormat` 和 `NpcProjectileCursorSaveCoordinator`，不改
世界状态格式。sidecar 以 account UUID 为主键保存 `(ReplicationId, Revision)` entries，严格
校验 magic、version、数量、UUID、重复 replication ID 和非负 revision；写入先 flush-to-disk
到临时文件，再替换目标文件。缺失 sidecar 返回空，旧 world header 不会被误读为 cursor state。

本批完成 durable format/coordinator API，但尚未把文件路径注入 `DomeServer` startup/shutdown；
因此跨进程恢复的实际运行闭环仍待下一批接线和重启 verifier。

## 31. B28 已实现节点：cursor retention cleanup

server 在 combat replication 前使用 authoritative `NpcProjectileReplicationSnapshot` 列表和
current tick 清理 NPC projectile cursor。active projectile 与 retention 尚未到期的 tombstone
继续保留；缺失的 replication ID 和已过 retention 的 inactive tombstone 被删除，避免 account
store 的 cursor 无限增长或重连恢复陈旧终止事实。

本批 cleanup 仍运行在进程内 account-keyed store；跨进程 durable account store、写入原子性和
重启恢复仍未完成，不能由 cleanup verifier 推断磁盘持久化已闭环。

## 22. B19 已实现节点：NPC typed termination tail

NPC projectile replication 现在携带 `TombstoneReason` 与 `TombstoneRetainedUntilTick`。在实体
销毁前，`NpcProjectileReplicationSystem.ProjectTombstone` 可生成 inactive typed snapshot，
保留 `NpcHandle` owner、identity、位置和速度；`NpcProjectileLifetimeSystem` 负责确定性的
剩余寿命递减。focused NPC verifier 已覆盖 expiry、`Expired` 原因、零剩余寿命与 retention
边界。

这批刻意不修改 V1456 player `SyncProjectile` 合同，也不把 `NpcHandle` 转换成 `PlayerHandle`。
审计仍确认 `DomeSimulation` 的 `_projectileQuery` 只收集
`ProjectileNetworkIdentityComponent`，因此 NPC projectile 的主循环移动、碰撞、终止提交和
批量 replication 调度仍是下一批工作；B19 的绿色 verifier 不能被解读为完整 NPC projectile
lifecycle 已完成。

## 23. B20 已实现节点：NPC projectile authoritative tick integration

NPC projectile 不再只存在 standalone spawn/replication verifier。`DomeSimulation` 现在拥有
独立的 NPC projectile query 和 typed replication dictionary，并提供 `CreateNpcProjectile` 作为
authoritative 创建入口。每个 `AdvanceProjectiles` tick 会推进行为、位置和 lifetime，处理
solid tile、liquid、player damage 与 penetration，过期或被拒绝的实体通过共享 despawn commit
写入 NPC typed tombstone；`CreateNpcProjectileReplicationSnapshots` 按 retention 过滤迟到终止
事实。

focused NPC verifier 已通过真实 DomeSimulation tick 覆盖 expiry tombstone，combat verifier
继续通过 player projectile 回归。V1456 `SyncProjectile` 仍明确拒绝 NPC snapshot；没有将
`NpcHandle` 转换为 `PlayerHandle`，因此协议层仍是下一条独立 contract 工作，不被本批绿色测试
误报为已完成。

## 24. B21 已实现节点：typed NPC replication contract

V1456 的 `SyncProjectile` 仍只能表达 `PlayerHandle` owner，因此没有扩展其 wire payload。server
replication 层现在在 `CombatReplicationBatch.NpcProjectiles` 提供独立的
`NpcProjectileReplicationSnapshot` typed 列表；assembler 对 active projectile 使用 PVS 过滤，
对 inactive projectile 使用 tombstone retention 过滤，并保留 NPC owner、identity、reason 和
revision。该列表明确不进入 `Frames`，也不调用 `ProjectileStateProjection.Project`，避免
把 NPC owner 静默压缩为 player slot。

这完成了 simulation -> server typed batch 的边界，但不代表客户端协议已支持 NPC projectile。
下一节点需要独立的 capability negotiation 和客户端可消费的 typed wire contract；在此之前，
V1456 客户端继续只接收 player-owned projectile frames。

## 26. B23 已实现节点：NPC projectile consumer and revision cursor

typed NPC envelope 现在有独立的 `NpcProjectileReplicationConsumer`。consumer 按
`ReplicationId` 保存最新 snapshot，只接受严格递增 revision；重复或旧帧被丢弃。inactive
tombstone 还必须满足当前 tick 尚未越过 `TombstoneRetainedUntilTick`，否则拒绝，避免重连或
迟到包复活已过期的 NPC projectile 终止事实。

server 侧 `CombatReplicationCursor` 同样新增独立 NPC projectile revision map，batch confirm
只更新对应 typed cursor，不影响 player projectile cursor。当前 consumer/cursor 仅为内存态，
跨重连持久化和 cursor restore 仍需下一批审计；V1456 `SyncProjectile` 合同继续不变。

## 27. B24 已实现节点：consumer reconnect restore boundary

`NpcProjectileReplicationConsumer` 现在可以导出并恢复完整 typed snapshot 集合。restore 必须
携带 authoritative `currentTick`：已过 retention 的 inactive tombstone 在恢复时直接丢弃，仍
有效的 tombstone 和 active snapshot 保留其 revision，旧 active frame 因 revision 不单调而被
拒绝。该边界覆盖内存 consumer 的重连恢复，但尚未接入 durable server session storage。

本批 persistence verifier 未能启动，原因是工作树已有的
`TreeCanopyClearanceQuery.cs` 对多维 `HashSet` 调用 `ToFrozenSet` 的编译错误；这不是本批改动
产生的错误，不能用该 gate 推断 B24 已完成磁盘持久化。

## 28. B25 已实现节点：server cursor ownership API

server `CombatReplicationCursor` 现在能导出和恢复 NPC projectile 的
`(ReplicationId, Revision)` entries，`SessionReplicationState` 作为 session-owned boundary
暴露该 API。focused verifier 用一个新 session 恢复 cursor 后确认同 revision 不会再次发送，且
player projectile cursor 不受影响。

这仍是 ownership API，不是完整 reconnect wiring：`TerrariaProtocolSessionHost` 当前每次 TCP
连接创建新的 session state，尚未从 durable slot/account store 注入并在断开时保存该 snapshot。
下一批只处理这条 wiring，避免把 server API 级 restore 混同为真正跨连接持久化。

## 29. B26 已实现节点：session host cursor wiring

`DomeServer` 现在按 player slot 持有 NPC projectile cursor snapshot。`TerrariaProtocolSessionHost`
创建连接时通过 callback 注入该 snapshot 到新的 `SessionReplicationState`；session 移除前由
`RemoveSessionPlayer` capture 最新 cursor。该 wiring 只影响 NPC typed projectile revision，
不会恢复 player projectile cursor，也不会改变 `DomeSimulationSnapshot` 磁盘格式。

当前 slot store 只覆盖进程内连接重建；账号 identity keyed reconnect、跨进程 durable storage 和
cursor retention 清理仍未完成，不能由本批 server build 误报为完整 reconnect persistence。

## 30. B27 已实现节点：account-keyed reconnect restore

连接在 `RequestWorldData` 解析出 `PlayerPersistentState` 后，host 用其 canonical UUID 恢复
NPC projectile cursor；断开时 UUID 随 destroy command 进入 `DomeServer`，最新 cursor 同时写入
slot fallback 和 account-keyed in-process store。这样同一账号更换 player slot 时仍能保持 typed
NPC projectile revision 去重。

本批没有把 cursor 写入 `DomeSimulationSnapshot` 或玩家物品/账号磁盘格式，也没有清理已过
retention 的 cursor entry。跨进程 durable store 和 retention cleanup 仍需单独设计与验证。

## 25. B22 已实现节点：NPC projectile capability and wire envelope

在不改变既有 V1456 `SyncProjectile` 和旧 capability offer/ack body 的前提下，新增 NetModules
扩展 kind：NPC projectile capability offer/ack，以及独立 `NpcProjectileReplication` envelope。
envelope 保留 NPC owner、typed identity、位置/速度、lifetime、revision、section 和 tombstone
字段，并执行 finite、identity、revision、active/reason 一致性校验。

`TerrariaSession` 只有在基础 contract 已协商后才接受 NPC projectile capability；server
replication assembler 仅在 `SupportsNpcProjectile` 时把 typed batch 编码为该 envelope。未协商
或旧客户端仍不会收到 NPC projectile frame，V1456 player projectile wire 保持不变。
PVP rule 的 disk persistence 仍 deferred，避免在当前兼容尾部未经格式合同地写入新字段。

## 26. B34 已实现节点：authoritative projectile damage metadata

Projectile definitions and ECS definition components now carry `Knockback` and
`OriginalDamage`. Spawn systems normalize `OriginalDamage` to definition damage when omitted;
player and NPC replication snapshots preserve the normalized values. V1456 player projection
prefers these authoritative definition fields (while retaining compatibility with older
non-zero snapshot fields), so the existing `SyncProjectile` byte layout remains unchanged.
NPC typed replication envelopes include the same metadata with finite/positive validation and
round-trip coverage. This batch only closes the authoritative damage-metadata gap; complete
Terraria projectile definition parity, typed AI fields, visual effects, and all projectile
families remain deferred.

## 27. B35 已实现节点：authoritative spawn damage

`SpawnProjectileCommand.Damage` is now treated as non-authoritative compatibility input.
`ProjectileSpawnSystem` creates `ProjectileDamageComponent` from the resolved registry definition,
and `DomeSimulation` publishes definition damage/lifetime in the initial replication snapshot.
The focused combat fixture submits forged damage and proves the resulting authoritative snapshots
retain definition damage. Other legacy command fields remain a separately audited compatibility
surface; this does not claim full spawn-intent normalization.

## 28. B36 已实现节点：player projectile PVS re-entry reconciliation

`SessionReplicationState` records the section for each active player projectile confirmed to a
session. Replacing visible sections forgets the combat revision cursor only for projectiles whose
last sent section left PVS. Therefore an unchanged projectile is not sent while hidden but is
sent again after re-entry. The focused protocol fixture covers enter -> leave -> unchanged
re-entry. This is player projectile cursor behavior only; NPC typed projectile PVS transition
rules remain independently scoped.

## 29. B37 已实现节点：NPC projectile PVS re-entry reconciliation

The same PVS re-entry lifecycle is now applied to negotiated NPC projectile envelopes. Session
state tracks sent NPC projectile sections separately from player projectile state and clears only
the matching typed revision cursor when a section leaves PVS. A capability-enabled focused
fixture confirms the initial batch before proving enter -> leave -> unchanged re-entry emits the
typed replication envelope again.
This does not change the V1456 player projectile contract or client capability negotiation.

## 30. B38 已实现节点：authoritative spawn lifecycle scalars

Player spawn command `LifetimeTicks` and `MaximumPenetration` are now compatibility input only.
`ProjectileSpawnSystem` always initializes lifetime and penetration components from the resolved
definition, and the initial snapshot uses the same definition lifetime. Focused combat coverage
submits forged lifetime/penetration values, verifies definition lifetime in the snapshot, and
verifies a forged infinite-penetration command still produces the definition's `Penetrated`
tombstone. Position, facing, and finite velocity remain the intentional spawn intent surface.

## 31. B39 已实现节点：player projectile cursor retention prune

Player projectile replication now has the same retention-aware cursor compaction rule as typed NPC
projectiles. `DomeServer` prunes each active session before combat assembly: active snapshots and
tombstones still within retention keep their cursor; missing snapshots and tombstones at/after
their retention tick remove it. The focused protocol fixture verifies retained preservation,
missing removal, and expiry removal. This changes only session cursor memory, not simulation
projectile persistence.

## 32. B40 已实现节点：typed projectile behavior AI projection

Registered Linear and Gravity behavior state now has an explicit simulation-layer projection into
`ProjectileReplicationSnapshot.Ai0/Ai1/Ai2`: Linear publishes phase, while Gravity publishes
vertical velocity and phase. Unknown behavior IDs, negative phases, and non-finite state fail
closed. Legacy projectile AI families without a typed mapping remain deferred.

## 33. B41 实施节点：server-owned projectile UUID allocation

Player projectile spawn now allocates a non-empty server-owned `Guid` in
`ProjectileNetworkIdentityComponent`; subsequent authoritative refreshes already copy the
identity into snapshots rather than allocating again. The focused UUID lifecycle fixture proves
the initial and refreshed player snapshots retain one non-empty UUID. Existing V1456 `short Uuid`
projection is intentionally unchanged, and NPC UUID generation remains separately audited.

## 34. B42 已确认节点：defer legacy short UUID projection

The legacy V1456 `short Uuid` remains unset. It encodes the old owner-scoped integer
`projUUID` contract, not the authoritative `Guid` introduced in B41; therefore no truncation,
hashing, or implicit conversion is permitted. Current default projectile types `1..6` have no
source-backed `NeedsUUID` entry. A legacy integer UUID contract is deferred until a complete,
source-backed type table can enable it per type.

## 35. B43 实施节点：NPC projectile V2 behavior-state replication

NPC projectile replication now retains the same authoritative Linear/Gravity AI projection as
player projectiles. Existing `NpcProjectileReplication` V1 remains fixed and compatible; a
separate V2 extension kind is selected only after NPC capability negotiation includes bit 2.
V2 carries finite `Ai0/Ai1/Ai2` values for active snapshots and tombstones, while V1 clients
continue receiving the original envelope. Focused protocol coverage proves V2 round-trip,
non-finite rejection, capability selection, and V2 emission; NPC coverage proves a Gravity
projectile's AI state survives authoritative tick advancement and expiry. Opaque behavior data,
legacy V1456 `short Uuid`, and unimplemented AI families remain deferred.

## 36. B44 实施节点：NPC server-owned Guid in V3 replication

NPC projectile spawn now allocates a non-empty server-owned `Guid`, and the authoritative
snapshot preserves it through active updates and tombstones. NPC V3 carries a fixed
`hasGuid + 16-byte Guid` tail and rejects absent or empty Guid values; V1 and the existing
AI-only V2 envelope remain byte-for-byte unchanged. Focused protocol coverage proves the V3
Guid round-trip and V3 PVS emission, while
NPC lifecycle coverage proves spawn-to-expiry Guid stability. This does not project Guid into
V1456 `SyncProjectile` or infer legacy `short Uuid` semantics.

## 37. B45 实施节点：NPC V3 compatibility repair

The Guid tail originally extended the fixed V2 body, which would have made already-negotiated
V2 peers reject it. V2 is restored as the AI-only B43 envelope. A separate V3 extension kind
and capability bit 4 now carries Guid, and server replication selects V3, then V2, then V1.
Focused protocol coverage proves both the old V2 no-Guid round-trip and the V3 Guid round-trip.

## 38. B46 实施节点：NPC projectile motion authority

`ProjectileBehaviorSystem` is now the single transform writer for both player and NPC
projectiles. The NPC loop previously advanced transform again after Linear/Gravity behavior had
already done so, causing double displacement. A one-tick Gravity fixture now proves the final
position is `19.75` from a `20.0` origin with velocity `0.0`, while preserving AI state and the
expiry tombstone contract.

## 39. B47 实施节点：NPC capability negotiation reconciliation

An unnegotiated session may inspect typed NPC projectile snapshots, but it must not advance its
NPC projectile revision cursor until a typed wire frame is emitted. `ConfirmCombatBatch` now
marks that cursor only after NPC projectile capability negotiation. Focused protocol coverage
proves a visible unchanged projectile emits its initial V3 frame after negotiation.

## 40. B48 实施节点：immutable NPC projectile capability

NPC projectile capability selection is now session-immutable, matching base capability
negotiation. A repeated offer is rejected before it can replace the selected V1/V2/V3 mask and
invalidate the already-confirmed replication cursor schema.

## 41. B49 实施节点：legacy aiStyle 2 generic core

Default projectile type 3 now uses its source-backed `aiStyle = 2` definition: a 22-pixel square
collider, friendly ownership, penetration 4, and behavior ID 3. The behavior increments its typed
timer, starts at tick 20, applies vertical acceleration `0.4`, horizontal drag `0.97`, and caps
vertical velocity at `32`; its timer and phase have an explicit typed replication projection.
The former type-3 hostile assumptions are now explicit test fixture registries. Type-specific
legacy `aiStyle = 2` branches remain deferred and must not use this generic behavior implicitly.

## 42. B50 实施节点：legacy aiStyle 2 delayed core

Default projectile type 69 now uses a separate source-backed delayed behavior: its 14-pixel
collider, friendly ownership, and penetration 1 match `SetDefaults`; its typed timer begins
gravity at tick 10 with `Vy += 0.25` and `Vx *= 0.99`, retaining the global `Vy <= 32` cap.
The shared legacy type 70/621 on-kill world-conversion effects and all other type-specific
`aiStyle = 2` branches remain deferred.

## 43. B51 实施节点：legacy aiStyle 2 immediate gravity

Default projectile type 249 now uses its own source-backed immediate behavior: its 12-pixel
collider, friendly ownership, and penetration 1 match `SetDefaults`; each tick increments the
typed timer and applies `Vy += 0.25` immediately, preserving horizontal velocity and retaining
the global `Vy <= 32` cap. The typed projection exposes timer and phase as `Ai0` and `Ai1`.
Type 249 on-kill VFX and all remaining type-specific `aiStyle = 2` branches remain deferred.

## 44. B52 实施节点：legacy aiStyle 2 five-tick gravity

Default projectile type 347 now uses a separate source-backed five-tick behavior. Its 6-pixel
collider, hostile flag, and infinite penetration match `SetDefaults`; the typed timer adds
`Vy += 0.25` starting at timer 5, preserves horizontal velocity, and retains the global
`Vy <= 32` cap. The server maps its hostile flag to `HostileNonPvp`; legacy `SetDefaults` does
not assign damage, so the authoritative definition deliberately retains `Damage = 0` rather than
trusting a client-provided spawn value. Remaining type-specific `aiStyle = 2` branches are
deferred.

## 45. B53 实施节点：legacy aiStyle 2 sixty-tick and tile policy

Default projectile type 300 now uses a separate source-backed behavior: a 38-pixel hostile,
infinite-penetration projectile whose timer starts `Vy += 0.2` and `Vx *= 0.99` at tick 60.
Its source `tileCollide = false` contract required a new typed `CollidesWithTiles` definition
field, propagated through both player and NPC spawn paths and consumed by both collision loops.
Focused player and NPC fixtures prove it crosses a solid tile without a tile tombstone. The
initial sound and other presentation effects are deferred; the current simulation has no liquid
drag, so source `ignoreWater = true` is represented by the existing non-destructive liquid policy.

## 46. B54 实施节点：legacy aiStyle 2 generic type 48

Default projectile type 48 now binds to the already source-backed generic `aiStyle = 2` behavior.
Its 12-pixel collider, friendly flag, and penetration 2 match `SetDefaults`; the first 20 behavior
advances retain velocity, then use the shared `Vx *= 0.97` and `Vy += 0.4` motion. An integration
fixture proves authoritative spawn followed by the twentieth behavior advance, whose simulation
phase is tick 21 because spawn commit precedes behavior execution. Type 48 rotation and destruction
effects remain presentation work. Type 54 and 93 remain deferred because they add random on-hit or
damage rules beyond this generic behavior contract.

## 47. B55 实施节点：legacy aiStyle 2 generic type 599

Default projectile type 599 now binds to the same source-backed generic `aiStyle = 2` behavior.
Its 22-pixel collider, friendly flag, and penetration 6 match `SetDefaults`; authoritative spawn
and the twentieth behavior advance produce the shared `Vx = 9.7`, `Vy = 0.4`, and typed timer
projection. Type 599-specific rotation, sound, and dust are presentation-only. Type 520 is not
included in this batch; its source contract requires a separate type-specific audit.

## 48. B56 实施节点：legacy aiStyle 2 random-frame type 909

Default projectile type 909 now uses a dedicated behavior: its 12-pixel hostile projectile starts
the shared `Vy += 0.4` and `Vx *= 0.97` motion at timer 38. The legacy `ai[1]` frame variant is
stored in typed `Secondary` and projected as `Ai1`; player and NPC spawn paths initialize a stable
1-6 variant from the server replication identity so snapshots and replay do not depend on global
RNG order. This preserves the source variant contract while making the server result deterministic.
Type 909 frame selection and destruction dust remain presentation-only.

## 49. B57 Implementation node: legacy aiStyle 2 generic type 520

The prior type-520 child-spawn assumption was incorrect: legacy source creates type 522 children
from type 521, while type 520 destruction only emits sound and dust. Default type 520 is therefore
now registered with the existing generic behavior: 22-pixel collider, friendly ownership,
penetration 3, timer-20 `Vx *= 0.97` and `Vy += 0.4` motion. The integration fixture verifies its
authoritative registry spawn and twentieth behavior advance. Rotation, sound, and dust remain
presentation-only; type 521 child spawning remains separately deferred.

## 50. B58 Implementation node: legacy aiStyle 2 type 501 explosion

Default type 501 now has its source-backed 14-pixel hostile definition and a dedicated behavior
that begins `Vx *= 0.995` and `Vy += 0.2` at timer 18. Legacy destruction expands its centered
hitbox from 14 to 134 pixels before `Damage()`; the server expresses that as a validated
definition-owned `OnDespawnAreaDamage` of 8.375 tiles. At command commit, before the projectile is
removed, the effect emits ordinary typed player-damage commands. The existing damage resolver still
owns mitigation, immunity, health changes, death events, and replication. Type 501 has no default
damage in legacy `SetDefaults`, so only the already validated NPC ranged spawn path supplies its
positive authoritative damage; player spawn remains definition-controlled rather than trusting a
client damage field. Sound, dust, and gore remain presentation-only.

## 51. B59 Implementation node: legacy aiStyle 2 type 240 explosion

Default type 240 now uses a dedicated hostile behavior: 16-pixel collider, infinite penetration,
and timer-16 `Vx *= 0.991` / `Vy += 0.18` motion. Its legacy destruction expands a centered
16-pixel projectile to 96 by 96 pixels and calls `Damage()`; the existing definition-owned
on-despawn area contract represents this as 6 by 6 tiles and reuses the typed NPC-projectile to
player-damage commit path established for type 501. The NPC integration fixture verifies the timer
boundary, server-validated ranged damage, expiry tombstone, and in-area player damage. Presentation
effects remain deferred.

## 52. B60 Implementation node: legacy aiStyle 29 types 521 and 522

Default type 521 is now a friendly 14-pixel parent with an authoritative on-despawn child
definition; type 522 is its friendly 8-pixel child. The parent keeps source-compatible linear
motion while type 522 stores its timer as typed state projected to `Ai1`, applies `Vy += 0.2`,
`Vx *= 0.98`, caps vertical velocity at 18, and expires normally after its forty-first advance.
Its source tile response is a definition-owned reflection policy, which preserves velocity in the
published player/NPC snapshot after bounce commit.

When an active player-owned 521 is committed for destruction, the server derives a deterministic
3..5 child set from the parent replication identity rather than legacy global `Main.rand` order.
Each child is queued for the following command commit at the parent center, keeps the owner, uses
speed 7..10, receives `floor(parent damage * 0.8)`, and receives parent knockback times `0.8`.
The child command is internal-only: public spawn calls clear both authoritative damage and
knockback fields. A deduplicated destruction loop prevents repeated tombstones from generating a
second child set. The focused Combat verifier covers public forgery rejection, validated item
damage, child identity/owner/center/speed/damage/knockback, tile reflection, and normal timer
expiry; the shared NPC verifier was rerun for registry and bounce-projection regression.

Exact legacy global RNG ordering, alpha/dust/sound, the aiStyle-29 8-pixel hitbox inset, and
client-side `Main.myPlayer == owner` presentation semantics remain deferred. The deterministic
server selection preserves the legacy child count, direction, and speed state space, not its
global RNG sequence.

## 53. B61 Implementation node: legacy aiStyle 2 type 162 friendly area damage

Default type 162 now has its source-backed friendly 16-pixel, penetration-4 definition and a
dedicated timer behavior. Its first seventeen advances retain velocity; advance 18 applies
`Vx *= 0.99` and `Vy += 0.28`. Legacy destruction expands its center-preserving hitbox to 64 by
64 pixels before `Damage()`; the existing validated on-despawn area model represents that as 4 by
4 tiles.

The area emitter now has the missing friendly target route: active in-area NPC entities receive
the existing same-commit `DamageCommand`, while player damage remains governed exclusively by the
hostile player policy. This avoids an enqueued `DamageNpcCommand` after the NPC pipeline has
already committed and preserves the normal NPC snapshot update path. Focused Combat evidence
proves the timer boundary, in-area NPC damage, out-of-area NPC/player exclusion, and expiry
tombstone; the shared NPC verifier confirms registry and NPC command-path regression safety.

Alpha/sound/dust/gore, visual random ordering, and unrelated legacy aiStyle-2 branches remain
deferred.

## 54. B62 Implementation node: legacy aiStyle 49 type 281 bounded motion and tile response

Default type 281 now has its source-backed friendly 28-pixel, unlimited-penetration, 600-tick
definition. Its dedicated typed behavior retains velocity for the first seventeen advances; at
advance 18 it applies `Vx *= 0.99`, `Vy += 0.28`, and caps vertical velocity at `15.9`.
The timer is projected as `Ai0` for active snapshots and tombstones.

Definition-owned `BounceVelocityMultiplier` and `MinimumBounceSpeed` now pass through both player
and NPC projectile spawn materialization. Collision continues to emit only commands: a type 281
impact at speed at least 2 commits the source `0.5` axis reflection; a slower impact produces a
single `TileHit` tombstone. Focused Combat evidence covers the definition, timer/cap boundary,
fast reflection, and slow terminal impact; the Simulation Release build passed with 0 warnings
and 0 errors.

The legacy terminal NPC 614 release, `ai[1]` owner target gate, static NPC immunity, boss damage
scaling, and all alpha/sound/dust/gore presentation remain deferred. The current slow terminal
path intentionally creates no substitute NPC because the required authoritative NPC-614 spawn
contract has not been recovered in this simulation layer.

## 55. B63 Implementation node: legacy aiStyle 2 type 21 generic definition

Default type 21 is now registered as a source-backed friendly 16-pixel, single-penetration,
3600-tick projectile using the existing generic aiStyle-2 behavior. Its server snapshot reaches
the established tick-20 drag/gravity boundary and publishes the typed timer through `Ai0/Ai1`.
The physical collider remains 1x1 simulation units; legacy scale, wind singleton input, damage
class modifiers, and on-kill sound/dust remain deferred. Focused Combat verification and the
Simulation Release build passed with 0 warnings and 0 errors.

## 56. B64 Implementation node: legacy aiStyle 2 type 330 generic definition

Default type 330 is now registered as a source-backed friendly 22-pixel, six-penetration,
3600-tick projectile using the existing generic aiStyle-2 behavior. Its authoritative spawn
reaches the established tick-20 drag/gravity boundary and projects the timer through `Ai0/Ai1`.
The source's type-specific on-kill effects, scale/rotation, wind input, and other presentation
details remain deferred. Focused Combat verification and the Simulation Release build passed with
0 warnings and 0 errors.

## 57. B65 Implementation node: legacy aiStyle 2 type 589 generic definition

Default type 589 is now registered as a source-backed friendly 10-pixel, single-penetration,
3600-tick projectile using the existing generic aiStyle-2 behavior. Its authoritative spawn
reaches the tick-20 drag/gravity boundary and projects the timer through `Ai0/Ai1`.
The source's on-kill sound/particle behavior remains deferred. Focused Combat verification and
the Simulation Release build passed with 0 warnings and 0 errors.

## 58. B66 Implementation node: legacy aiStyle 2 type 166 bounded motion

Default type 166 now has a dedicated source-backed behavior and a friendly 14-pixel,
single-penetration, 3600-tick definition. Its first nineteen advances retain velocity; advance
20 applies `Vx *= 0.98` and `Vy += 0.3`, with the timer projected through `Ai0/Ai1`.
The owner-local town/player collision gate, cold-damage semantics, and on-kill presentation remain
deferred. Focused Combat verification and the Simulation Release build passed with 0 warnings and
0 errors.

## 59. B67 Implementation node: legacy aiStyle 2 delayed types 70 and 621

Default types 70 and 621 now join type 69 in the source-backed delayed aiStyle-2 family. Both
are friendly 14-pixel, single-penetration, 3600-tick projectiles and reuse the existing typed
timer behavior: advances one through nine retain velocity; advance ten and later apply
`Vx *= 0.99`, `Vy += 0.25`, and the global vertical-speed cap. Authoritative spawn fixtures
verify each type's definition, motion boundary, and `Ai0/Ai1` projection.

All three legacy variants call different `WorldGen.Convert` modes on destruction. That is an
authoritative world mutation, not a presentation effect, but this Simulation currently has no
recovered typed biome-conversion command and deterministic commit contract. It remains deferred
for types 69, 70, and 621; sound and dust remain deferred as presentation. Focused Combat
verification and the Simulation Release build passed with 0 warnings and 0 errors.

## 60. B68 Implementation node: legacy aiStyle 2 hostile type 471

Default type 471 now has its source-backed hostile 16-pixel, single-penetration, 3600-tick
definition and reuses the generic aiStyle-2 timer behavior. The definition explicitly selects
`HostileNonPvp`, preserving the existing authority path: server validation produces typed damage
commands, commit resolves the effect, and snapshots remain immutable. An authoritative-spawn
fixture proves the type's definition, tick-20 `Vx *= 0.97` / `Vy += 0.4` boundary, and
`Ai0/Ai1` projection. The only other source branch is on-kill sound/dust presentation, which
remains deferred. Focused Combat verification and the Simulation Release build passed with
0 warnings and 0 errors.

## 61. B69 Implementation node: legacy aiStyle 2 type 1012

Default type 1012 now has its source-backed friendly 18-pixel, single-penetration, 3600-tick
definition and reuses the generic aiStyle-2 timer behavior. The authoritative-spawn fixture
proves the physical definition, tick-20 `Vx *= 0.97` / `Vy += 0.4` boundary, and `Ai0/Ai1`
projection. Legacy source contains no additional motion, hit, owner-input, or world-mutation
branch for this type; its on-kill sound/dust remains deferred as presentation. Focused Combat
verification and the Simulation Release build passed with 0 warnings and 0 errors.

## 62. B70 Implementation node: legacy aiStyle 2 type 304 combat decay lifecycle

Default type 304 now has its source-backed friendly 30-pixel, single-penetration definition and
dedicated linear timer behavior. A typed post-behavior effect executes before the existing lifetime
advance and command commit: advances 1-29 preserve runtime combat state; advance 30 and later
apply legacy-truncated `damage *= 0.9` and `knockback *= 0.9`; advance 55 sets remaining lifetime
to zero so the existing command pipeline publishes an `Expired` tombstone.

The behavior remains responsible only for deterministic motion/timer state while the effect owns
per-entity combat/lifecycle mutation; no legacy client, presentation, or transport dependency was
introduced. The focused fixture proves the 30-tick boundary (`100 -> 90`, `10 -> 9`), the
55-tick terminal state, typed `Ai0/Ai1` projection, and the actual committed tombstone. Its initial
test velocity was reduced from 10 to 4 after evidence showed the faster path reached the world
boundary at advance 39 and correctly produced an unrelated `TileHit` tombstone. Alpha, rotation,
sound, dust, gore, and melee damage-class semantics remain deferred. The Simulation Release build
and focused Combat verifier passed with 0 warnings and 0 errors.

## 63. B71 Implementation node: aiStyle 2 shared vertical-speed cap

The legacy aiStyle-2 tail clamps `velocity.Y` to 32 after type-specific branches. The dedicated
type 162, 166, and 304 behaviors now retain that invariant before transform projection. Types 162
and 166 apply their own gravity step before the cap; type 304 applies the cap even though its
dedicated path does not apply gravity.

Focused Combat fixtures cover the type-162 and type-166 gravity boundaries from `31.9 -> 32`, and
the type-304 no-gravity path from an initial 35 to 32. The Simulation Release build and Combat
verifier passed with 0 warnings and 0 errors; the scoped diff has no whitespace errors. This is
only a shared-motion hardening result, not complete aiStyle-2 parity. Types 370, 371, and 936
retain their player/NPC radius-80 Buff routing as deferred pending a typed multi-target status and
replication contract; presentation and other specialized branches also remain deferred.

## 64. B72 Implementation node: aiStyle 2 multi-target status effects

Default types 370, 371, and 936 now have the source-backed shared timer behavior: advances one
through fourteen retain velocity; advance 15 and later applies `Vx *= 0.98`, `Vy += 0.3`, and the
shared `Vy <= 32` cap. Their definitions declare typed on-despawn status effects: respectively
Buff 119, 120, and 320 for 1800 ticks within the legacy 80-pixel radius, represented as five
Simulation units.

Despawn emits an area-status command and deterministic commit selects living, active player and
NPC centers using strict Euclidean distance. Both target families use the shared Buff collection;
direct, queued, and restored NPC spawn paths retain that component. Duration advances on both
families. Immutable state snapshots include revisioned per-target entries, including empty NPC
states so expiration can clear client state.

V1456 uses a separately negotiated `NpcStatusEffect` NetModules extension rather than modifying
the fixed V2 NPC body or reusing NPC-projectile capability. Server combat replication applies PVS
and a per-session revision cursor, and only confirms the cursor after a successful outbound batch.
Focused Combat, Combat Protocol, and Protocol Compatibility verifiers passed with 0 warnings and
0 errors; scoped diff and changed-file width checks passed. This establishes the typed status
route, not complete projectile parity: presentation and unrelated source-specific branches remain
deferred.

## 20. B17 已实现节点：NPC projectile ownership boundary

NPC ranged projectile 的 commit/spawn 边界现在要求 definition policy 为 `HostileNonPvp`；
PVP-only type `6` 不会被 NPC 发射路径接受。通过后仍保存 `NpcHandle` owner、独立 network identity
和 `NpcProjectileReplicationSystem` typed projection，避免把 NPC ownership 误投影成 player owner。
NPC verifier 已覆盖 rejection、owner 和 identity/replication parity。

## 21. B18 已实现节点：NPC protocol projection guard

V1456 的 `ProjectileStateProjection` 现在对 `NpcProjectileReplicationSnapshot` 明确抛出
`NotSupportedException`。这是有意的 trust boundary：现有 `SyncProjectile` owner 字段只能表达
player slot，不能把 NPC owner 静默压缩成 player owner。NPC projectile 继续停留在 typed
simulation/replication projection 层，直到协议有独立的 NPC projectile contract。

## 19. B16 已实现节点：PVP opt-in policy

新增 `PlayerDamagePolicy`：`HostileNonPvp` 保持 type `3` 的现有 hostile route；`PvpOptIn`
用于 type `6`，只有 authoritative `WorldRuleState.IsPvpEnabled` 为 true 才能命中 player。
`None` 不进入 player damage route。因此 hostile flag 不再隐式打开 PVP，默认世界规则仍关闭 PVP。
