# Projectile 组件系统补齐执行计划

**文档 ID**：PLAN-2026-11-24-PROJECTILE-COMPONENT-SYSTEMS
**逻辑域**：plans
**产物类型**：plan
**策略**：sequential（P0 → P1 是硬前置；P2–P4 各自独立可验收；P5 是内容扩展，按需启动）
**状态**：in-progress（P0–P4 已完成并验证；P5 未开工）
**上游依据**：[LVCS 统一加载系统 API 设计](../architecture/2026-11-23-lvcs-unified-loading-api-design.md)、[Projectile 领域实现说明](../reviews/audits/2026-10-25-projectile-implementation.md)；两份原上游设计（`2026-10-24-projectile-ecs-system-design.md`、`2026-11-23-projectile-penetration-budget-linkage-design.md`）已于 2026-11-24 删除，现行口径见 [Projectile README](../../src/NSSLC/Projectile/README.md) 与 [投射物 P0–P4 CR](../cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md)
**适用约束**：[ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md)、[组件命名设计约束](../../Context/架构约束/组件命名设计约束.md)、[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md)、[构建与验证约束](../../Context/项目约束/构建与验证约束.md)、[Test 验证项目组织](../../Test/README.md)
**范围**：`src/NSSLC/Projectile/**`、`src/NSSLC/Share/Physics/ContactFact/**`、`src/NSSLC/Share/Physics/SpatialSimulation/**`、`src/NSSLC/Share/Ai/**`（只读复用）、`src/NSSLC.Server/Content/Projectile/**`、`src/NSSLC.Server/Program.cs` 与 `Registration/CombatSystemRegistration.cs` 的注册、`Test/Terraria.Components/Terraria.Projectile/**`
**排除范围**：不改参考客户端；不改 `src/NSSLC.Infrastructure/**`；不引入事件总线/CQRS；不改协议包号与载荷格式；不实现绘制、音效、尘、Gore、光照（表现层，见 §8）

---

## 1. 执行目标

把 Projectile 从「有组件骨架、链路断开」推进到「生成 → 行为 → 运动 → 接触 → 伤害 → 状态 → 终止」全链可运行、可验证。

判定口径（四条，缺一条不算达成）：

1. **可生成**：至少一个真实 `ProjectileDefinition` 经 LVCS 加载并注册；玩家使用对应物品后下一 tick 存在对应投射物实体。
2. **可分化**：至少两种行为（直线 / 追踪）与两种瓦片结果（撞墙销毁 / 撞墙反弹）在同一 tick 内共存，且由**内容定义字段**决定，不靠 `type` 分支写死。
3. **单写者守恒**：`Location`/`Velocity` 仍只有 `SpatialSimulationCoordinatorSystem` 写；目标 `Life` 仍只有 `DamageCommitSystem` 写；Projectile 自身的穿透/免疫/终止只由 Projectile 域写。
4. **有证据**：每期给出构建命令、退出码、警告/错误数，以及验证项目的 PASS 输出；不接受口头完成。

本计划**不追求**复刻 `Projectile.cs` 的 210 个 `aiStyle`。目标是让**组件与系统的协作面**成立，内容族按需增量填充。

## 2. 当前基线与证据等级

### 2.1 已成立（有代码证据）

| 能力 | 证据 |
| --- | --- |
| 组件组合 | `Projectile/Component/**` 15 个组件：owner、source metadata、behavior selection/state、kinematics、lifetime、penetration、collision policy、hit immunity、disposition、两个队列 |
| 定义与行为目录 | `ProjectileDefinition`、`ProjectileDefinitionCatalog`、`ProjectileBehaviorCatalog`、`ProjectileBehaviorProfile` |
| 生命周期 | `ProjectileLifecycleSystem`（`TrySpawn` / `TryTerminate` / `TerminateOwnedBy` / `CommitTermination`）、`ProjectileRuntimeRegistry` |
| Tick 注册 | `CombatSystemRegistration.cs:51-136`：SpawnRequest（Preparation）、MovementIntent（Preparation）、LifetimeTick（Simulation）、DamageRequest（Simulation）、DamageResult/ProtectionWindow/AttackRequest/DamageRequest/ProjectileTermination（Commit） |
| 接触→伤害 | `PhysicsContactCaptureSystem` → `PhysicsContactQueueComponent` → `ProjectileDamageRequestSystem` → `CombatAttackIntentQueue` → `DamageCommitSystem` → `ProjectileDamageResultSystem` → `ProjectileHitResolutionSystem` |

### 2.2 断链（本计划要修的核心）

| # | 断点 | 证据 | 影响 |
| --- | --- | --- | --- |
| B1 | 内容定义目录创建后**立即 Seal，零定义** | `Program.cs:202-203` | 任何 Spawn 都返回 `DefinitionUnknown` |
| B2 | 全仓**无 Spawn 入队方**；`PlayerItemUseFact` 只入队不消费 | `Program.cs:178`；`PlayerItemUseSystem.cs:115/194/233`；`PlayerItemUseFactQueue.Drain` 无调用 | 投射物一个也生不出来 |
| B3 | `ProjectileAiRegistration.CreateSystem` **零调用** | `ProjectileAiRegistration.cs:11-25` | 行为目录、Profile、AI 插件全闲置 |
| B4 | `ProjectileSimulationSystem` **零消费者** | `ProjectileSimulationSystem.cs:9`；`Test/.../Terraria.Projectile/Program.cs:286-288` 自述 | `extraUpdates` 子步不生效 |
| B5 | 位移权威只写 `MovementIntent.Stop` | `ProjectileMovementIntentSystem.cs:47` | AI 速度意图无法进入空间管线 |
| B6 | `ProjectileTargetFact` **无生产者** | 全仓仅 homing 读、无构造 | 追踪行为永远 `MissingComponents` |
| B7 | Bounce 两组件**零读取** | `ProjectileBounceCapabilityComponent` / `StateComponent` 仅互引 | 反弹链整条缺失 |
| B8 | 瓦片接触只有 `CollidedX/Y` | `SpatialTileMotionResult.cs:7-11`；`SpatialCommit.cs:9-12` | 无法表达法线、入射速度、接触点 |
| B9 | `substep` 恒为 0 | `PhysicsContactCaptureSystem.cs:80` | 子步命中顺序无法表达 |
| B10 | 液体事实算完即丢 | `SpatialSimulationCoordinatorSystem.cs:105-108` 仅当实体已有 `LiquidContactComponent` 才回写；定义未加该组件 | 水/岩浆/蜂蜜/微光全部无反应 |
| B11 | 穿透上限每次命中重读属性 | `ProjectileHitResolutionSystem.cs:146-151` | 与参考端生成时快照（`:10675`）不符 |
| B12 | `PendingTerminate` **从未赋值** | `ProjectileDispositionStateComponent.cs:26` 仅枚举 | 终止状态机是死的 |
| B13 | `TerminateOwnedBy` **零调用** | `ProjectileLifecycleSystem.cs:196` | owner 移除不级联清理 |
| B14 | `BuffApplyRequestComponent` **零生产者** | 全仓仅 `BuffApplyRequestSystem` 消费 | 命中附加效果无法落地 |

### 2.3 参考端行为事实（`D:\TRbackup\最新的客户端\Terraria\Projectile.cs`，81,411 行）

| 事实 | 行号 |
| --- | --- |
| 字段面：`penetrate:366`、`maxPenetrate:376`、`stopsDealingDamageAfterPenetrateHits:408`、`localNPCImmunity:368`、`perIDStaticNPCImmunity:296`、`playerImmune:432`、`minion:396`、`sentry:332`、`WhipPointsForCollision:496` | `:296-496` |
| 生成时快照上限 | `:10675` |
| 命中扣减（伤害之后，耗尽那击仍造成伤害） | `:14190-14206` |
| 免疫先于扣减 | `:12635` |
| 销毁点（`timeLeft--` / `penetrate==0`） | `:17308-17315` |
| 撞墙默认即死 | `:19965-19969` |
| 反弹三序：先判预算 / 先反弹后判死 | `:19020-19026` / `:18108-18168` |
| AI 分派（210 个 `aiStyle`） | `:23405` |
| 伤害矩阵 `Damage` / `Damage_CanDealDamage` | `:12502` / `:12461` |
| 命中附加 `StatusNPC` / 鞭标签 | `:11176` / `:11736` |
| 切块 `CutTiles` | `:15277` |
| 按类型元数据 `ProjectileID.Sets`（46 组数组） | `Terraria.ID/ProjectileID.cs:10` |

**结论**：参考端是巨型聚合类，本计划取「协作面」不取「类型清单」。

### 2.4 基线门（P-1，已执行）

接受本计划时工作树处于**外部命名空间重构中途**：391 个文件的命名空间从 `Terraria.X` 批量改为 `Terraria.Share.X`，覆盖 12 个域（Ai 154、Attributes 52、Combat 41、Physics 28、Entity 25、Movement 21、Lvcs 13、Inventory 12、Item 11、Player 10、Liquid 9、Relationships 5）。

基线处置（**不属于 Projectile 范围**，但为其提供可构建前提）：

| 步骤 | 命令 / 动作 | 结果 |
| --- | --- | --- |
| 1 | `dotnet build src/NSSLC/Terraria.Components.csproj` | 失败：**1029 个错误**（`CS0246` 找不到 `VelocityComponent` / `LocationComponent` / `MovementCapabilityComponent` 等） |
| 2 | 定位根因 | 命名空间已迁移且 `using` 已更新；错误来自 `src/NSSLC/Build/obj/**` **陈旧中间产物** |
| 3 | 清理 `src/NSSLC/Build/{bin,obj}`（绝对路径已确认）后重编 | 错误降至 **1 个**：`NpcAiSnapshotProvider.cs(14,16): CS0234 命名空间"Terraria"中不存在"Player"` |
| 4 | 修正 `NpcAiSnapshotProvider.cs:14`：`using Terraria.Player;` → `using Terraria.Share.Player;` | 基线通过 |
| 5 | `dotnet build src/NSSLC/Terraria.Components.csproj --nologo -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/` | **exit 0，0 警告 0 错误**，产物 `src/NSSLC/Build/bin/Debug/net10.0/Terraria.Components.dll` |
| 6 | `dotnet build Test/Terraria.Components/Terraria.Projectile/…` 后 `dotnet run --no-build --no-restore` | **exit 0**，23 项 PASS，末行 `PASS: projectile spawn produces a spatial-simulatable entity.` |

**结论**：基线可构建、可验证，后续各期以此为对照。

**已知陷阱**：批量 `CS0246` 先清 `Build/{bin,obj}` 再改代码——`GlobalUsings.g.cs` 陈旧会造成大面积假失败（本次即 1029 → 1 → 0 的收敛过程）。**不要**在未清理时按报错逐个改 `using`，那会把一次缓存问题放大成几百行无效改动。

**遗留**：第 4 步的 `NpcAiSnapshotProvider.cs:14` 属外部重构收尾，不在 Projectile 范围内，单独记录，不并入本计划各期提交。

## 3. 分期总览

```text
P0 内容族接入 LVCS        ─┐
P1 Spawn 生产者           ─┴→ 链路可生成（第一个可运行里程碑）
P2 AI 入口 + 目标获取      ──→ 行为可分化
P3 空间子步 + 瓦片接触事实  ──→ 接触可表达
P4 反弹策略 + 穿透预算      ──→ 策略可配置
P5 命中效果 + 内容扩展族    ──→ 内容可增量
```

| 期 | 主题 | 前置 | 验收门 |
| --- | --- | --- | --- |
| P0 | 投射物内容族接 LVCS | 无 | 至少 1 个定义注册成功，`Seal()` 后 `TryGet` 命中 |
| P1 | Spawn 生产者（ItemUse → Spawn 请求） | P0 | 使用物品后下一 tick 存在投射物实体 |
| P2 | AI 运行入口 + 目标获取 | P0 | 追踪弹能在 N tick 内改变速度朝向目标 |
| P3 | 空间子步 + `TileContactFact` | P0 | 高速弹不穿墙；子步序号非零 |
| P4 | 反弹策略 + 穿透预算状态机 | P3 | 预算 1 命中即死；预算 N 命中 N 次；反弹按 `ConsumeOrder` 生效 |
| P5 | 命中附加效果 + 召唤/鞭/反射/切块 | P2、P4 | 每种能力有独立验证用例 |

---

## 4. P0：投射物内容族接入 LVCS

### 4.1 输入

- `NSSLC.Infrastructure.Lvcs` 契约（`ILvcsCodec<TRaw,TValue>`、`LvcsFamilyId`、`LvcsDiagnosticSink`）与适配器（`FileLvcsSource`、`LvcsLoader`、`LvcsCodecRegistry`）——契约 2026-11-25 前在 `Terraria.Lvcs` 共享程序集，见 [CR](../cr/CR-2026-11-25-lvcs-contract-relocation.md)
- 既有 `ProjectileDefinitionCatalog`（`Seal()` 后为不可变目录）
- 参照族：`NSSLC.Server/Content/ItemUse/{ItemUseCodec,ItemUsePrototype}.cs`

### 4.2 任务

| # | 任务 | 落点 |
| --- | --- | --- |
| P0-1 | 新增 `ProjectilePrototype`：字段组 = id / behaviorId+profileId / kinematics(extraUpdates) / lifetime / collider / collisionPolicy(tileCollide, ignoreOwner) / gravity / liquidPolicy / penetrationPolicy / bouncePolicy / attackDefinitionId / damageType | `NSSLC.Server/Content/Projectile/ProjectilePrototype.cs` |
| P0-2 | 新增 `ProjectileCodec`：`Family = new LvcsFamilyId("projectile")`，`Extensions = [".json"]`；`Parse` 逐条校验、`Project` 建目录并 `Seal()` | `Content/Projectile/ProjectileCodec.cs` |
| P0-3 | 内容资产首批 3 条：`wooden-arrow`（直线、预算 1、撞墙销毁）、`homing-spark`（追踪、预算 3）、`bouncer-orb`（反弹、预算 5、SpeedScale 0.8） | `Content/Prototypes/Projectile/projectile.json` |
| P0-4 | 扩展 `ProjectileDefinition` 构造参数：补 `GravityPolicy` / `LiquidPolicy` / `PenetrationPolicy` / `BouncePolicy`（全部为不可变 `record struct`） | `Projectile/System/ProjectileDefinition.cs` |
| P0-5 | 扩展 `ProjectileEntityDefinitions.Build`：按定义装配能力组件（重力、液体、追踪、反弹），移除 `GravityCapabilityComponent(Enabled:false)` 硬编码 | `Projectile/System/ProjectileEntityDefinitions.cs:58-66` |
| P0-6 | `Program.LoadProjectileCatalog()` 用 LVCS 加载，失败打印全部诊断并抛 `InvalidDataException`；替换 `Program.cs:202-203` 的 `new()`+`Seal()` | `Program.cs` |

### 4.3 约束

- LVCS 契约文件不得引用 `System.IO`；文件枚举只在 `FileLvcsSource`。
- `ProjectileDefinitionCatalog` 必须 `Seal()` 后交给系统，符合输入边界规则 3。
- 字段组不要命名成 `ProjectileDataComponent` / `ProjectileInfo`；按组件命名约束用 `...Policy` / `...Profile`。

### 4.4 验证

```powershell
dotnet build src/NSSLC/Terraria.Components.csproj --nologo -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/
dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/
dotnet run --project Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj --no-build --no-restore
```

新增断言：`catalog-count == 3`、`catalog-sealed`、`definition-unknown-rejected`、`entity-composition-has-gravity-capability`（按定义值而非硬编码）。

### 4.5 退出条件

- 构建 0 警告 0 错误；
- 三条定义注册成功且 `Seal()` 后不可再注册；
- 启动日志无 LVCS 诊断错误。

---

## 5. P1：Spawn 生产者

### 5.1 背景

`PlayerItemUseSystem` 已产出 `PlayerItemUseFact`（物品、消耗弹药、通道、tick），但队列无消费者。这是唯一的「射击 → 生成」接缝，不需要新系统。

### 5.2 任务

| # | 任务 | 落点 |
| --- | --- | --- |
| P1-1 | 新增 `PlayerShootRequestSystem`：`Drain()` 射击事实 → 解析 `ItemUse` 的射击配置 → `ProjectileSpawnRequests.Enqueue` | `Share/ItemUse/Player/System/PlayerShootRequestSystem.cs` |
| P1-2 | `ItemUse` 内容增加射击字段：`ShootProjectileId`（可空）、`ShootVelocity`、`ShootOffset` | `Share/ItemUse/Content/ItemUse.cs` |
| P1-3 | 注册到 `EcsServerPhase.Simulation`，`after: [PlayerItemUse]`，保证同 tick 内入队 | `Program.cs` |
| P1-4 | 记录排空结果：失败（定义未知/owner 不可用）写诊断日志，不静默丢 | 同上 |

### 5.3 验证

- 断言：使用带 `ShootProjectileId` 的物品后，`ProjectileSpawnQueueComponent.Count == 1`；下一 tick 实体存在且 `ProjectileOwnerComponent.Owner` 正确。
- 断言：使用无射击配置的物品不产生请求。
- 断言：定义 id 非法时返回 `DefinitionUnknown` 且不入队脏数据。

### 5.4 退出条件

服务器真实启动后，玩家使用木箭类物品可看到投射物实体并在下一 tick 位移。

### 5.5 实施记录（as-built，2026-11-24）

实际落点与上表有三处偏差，理由见 CR §3：

| 项 | 计划 | 实际 | 原因 |
| --- | --- | --- | --- |
| P1-1 落点 | `Share/ItemUse/Player/System/` | `Projectile/ProjectileItemUseSpawnSystem.cs` | 共享层不得依赖非共享领域；系统归属其输出域，且 `*System.cs` 不得放在 `System/` 子目录 |
| P1-2 字段 | 三个独立字段 | `ItemUse.Shoot`（可空 `ItemShootProfile`） | 三个字段必须同时存在才有意义，合成一个值对象可让"配了一半"无法表达 |
| P1-3 相位 | Simulation，`after: [PlayerItemUse]` | Preparation，`before: [CombatProjectileSpawn]` | 生成队列按 tick 号排空；放在 Simulation 会入队到一个永不排空的 tick 上（实测丢请求） |

P1-4 的"不静默丢"以 `Results` 路由结果落地（入队 / 缺使用定义 / 无射击配置 / 定义未知 / 持有者不可用），
运行期日志接入仍属宿主职责，见 CR §5。

退出条件的验收口径调整：真实启动已通过，但"玩家使用物品"仍缺客户端输入适配（见 CR §5），
因此该条目的可判据改为"物品使用事实进入系统后，下一 tick 存在弹体实体并落在持有者偏移处"。

---

## 6. P2：AI 运行入口 + 目标获取

### 6.1 任务

| # | 任务 | 落点 |
| --- | --- | --- |
| P2-1 | 组合根构造 `AiSystem<ProjectileAiIntent>`（`ProjectileAiRegistration.CreateSystem`）并持有 `AiEntitySystem<EntityRuntimeHandle, ProjectileAiIntent>` | `Program.cs` |
| P2-2 | 新增 `ProjectileAiSnapshotProvider : IAiSnapshotProvider<ArchEntity>`：投影 `Location`、`Velocity`、`ProjectileHomingCapabilityComponent`、`ProjectileTargetFact` | `Projectile/System/ProjectileAiSnapshotProvider.cs` |
| P2-3 | 新增 `ProjectileTargetAcquisitionSystem`：按 `MinionTargeting` 策略搜最近敌对实体，写 `ProjectileTargetComponent`（句柄 + revision） | `Projectile/System/ProjectileTargetAcquisitionSystem.cs` |
| P2-4 | 新增 `ProjectileAiAdapter`：查询 → 快照 → `Calculate` → 写 `ProjectileBehaviorStateComponent`（调 `Advance`）+ 入 `ProjectileIntentQueueComponent` | `Projectile/System/ProjectileAiAdapter.cs` |
| P2-5 | 注册：`ProjectileTargetAcquisition`（Simulation）、`ProjectileAiAdapter`（Preparation 或 Simulation，取决于队列消费点） | `CombatSystemRegistration.cs` |

### 6.2 约束

- **不新建 `ProjectileBehaviorSystem`**：AI README 明确三类实体共用一个 `AiSystem<TIntent>`，适配器只做「快照 → 消费意图 → 提交领域状态」。
- 目标搜索不复用 `NpcAiSnapshotProvider`（其目标查询要求 `InventoryComponent`，只服务玩家）。
- 目标句柄必须带 revision；切换目标时递增，避免旧观察命中新实体。

### 6.3 验证

- `homing-turns-toward-target`：目标在侧方 200px，N tick 后速度朝向夹角减小。
- `target-revision-bumps-on-switch`：切换目标后 revision 增加。
- `no-target-keeps-velocity`：无目标时直线不变。
- `behavior-state-advances`：`Advance` 被调用，`Revision` 递增。

### 6.4 退出条件

追踪弹与直线弹在同一次运行中表现不同，且差异来自定义而非 `type` 分支。

### 6.5 实施记录（as-built，2026-11-24，行为断言未完成）

| 项 | 状态 | 说明 |
| --- | --- | --- |
| P2-1 AI 运行入口 | 完成 | 组合根构造 AI 系统与实体运行时，行为目录三条 profile（数值留 0 = 取内容能力值） |
| P2-2 快照提供者 | 完成 | 投影位置/速度/追踪能力/目标事实；目标丢失不写事实 |
| P2-3 目标获取 | 完成 | 换目标递增版本号；归属者排除；结构变更延后到查询之外 |
| P2-4 适配器 | 完成 | 绑定惰性补挂 AI 声明；写行为状态与一步移动意图（B5） |
| P2-5 注册 | 完成 | 两个注册键，均在 Preparation：目标获取 → 适配器（排在位移意图之后） |
| §6.3 四条断言 | 完成 | homing-turns-toward-target、target-revision-bumps-on-switch、no-target-keeps-velocity、behavior-state-advances 全部实测通过（验证项目 68 条 PASS） |

另注 1：追踪弹的移动加速度改为取内容转速，否则 AI 输出的速度会被空间协调系统按 0 加速度忽略，
行为差异永远到不了位移（B5 的下一半）。

另注 2（未闭合）：实测转向过程速率从 8.000 漂到 7.000。原因是 AI 与空间协调系统各做一次
`MoveTowards`（弦逼近），两次线性插值叠加。断言按实测区间写的，注释里点名目标值，避免把缺陷
固化成契约；严格不掉速需要 P3 让空间侧直接落地目标速度。

---

## 7. P3：空间子步 + 瓦片接触事实

### 7.1 任务

| # | 任务 | 落点 |
| --- | --- | --- |
| P3-1 | `SpatialSimulationSystem` 支持 `SubstepCount`（读 `ProjectileKinematicsStateComponent` 或通用 `SubstepCapabilityComponent`），每子步独立 `Move` | `Share/Physics/SpatialSimulation/SpatialSimulationSystem.cs` |
| P3-2 | `SpatialCommit` 增加 `TileContacts`（只读列表），或经 `TileContactQueueComponent` 旁路；**不塞进 `SpatialCommit` 的既有四项语义** | `SpatialSimulation/System/SpatialCommit.cs` |
| P3-3 | 新增 `TileContactFact`：`EntityRuntimeHandle` + `TileCoordinate` + `ContactPoint` + `Normal` + `IncomingVelocity` + `tick` + `revision` + `substep` | `Share/Physics/ContactFact/System/TileContactFact.cs` |
| P3-4 | 新增 `TileContactQueueComponent`：照 `PhysicsContactQueueComponent` 的 `BeginCapture/Add/Drain` 模式 | `Share/Physics/ContactFact/Component/TileContactQueueComponent.cs` |
| P3-5 | `SpatialSimulationCoordinatorSystem` 在子步碰撞时写入 tile 接触队列 | 同上目录 `SpatialSimulation/**` |
| P3-6 | `PhysicsContactCaptureSystem` 的 `substep` 改为真实子步号，不再恒 0 | `PhysicsContactCaptureSystem.cs:80` |

### 7.2 设计判据

- **为什么放在 Physics**：`ContactFact` README 的定位是「本 tick 有哪些接触，以及如何方向中立地交给下游；不做状态提交」。tile 接触与 entity 接触是同类事实，放同目录不同 fact 类型。**不得**新建 `TilePhysicsSystem` 平行体系。
- **为什么不塞 `SpatialCommit`**：`SpatialCommit` 是「允许写入的状态」清单（位置/速度/接地/液体），事实与状态是两类东西（穿透预算设计 §7 已论证）。
- **子步只能由唯一空间写者实现**：否则与 `SpatialSimulationCoordinatorSystem` 双写位置。

### 7.3 验证

- `fast-projectile-no-tunneling`：速度 40px/tick、墙厚 16px、`extraUpdates=3`，不穿墙。
- `substep-index-nonzero`：接触事实 `substep > 0`。
- `tile-contact-has-normal`：法线为单位向量且朝向入射反向。
- `entity-contact-substep-matches`：实体接触 `substep` 与当次子步一致。

> **注意现有断言的含义**：基线里已有 `kinematics-substep-not-advanced`（`substep=0`）与 `substep-does-not-scale-distance` 两条 PASS，它们固定的是**当前缺陷**（子步从未推进），不是期望行为。P3 完成后这两条必须**改写为** `substep-advances-within-tick` 与 `substep-partitions-delta-time`，否则等于把缺口固化成契约。这是本项目"测试固定现状而非意图"的典型风险，评审必查。

### 7.4 退出条件

高速弹不再穿墙；接触事实带法线与入射速度。

### 7.5 实施记录（as-built，2026-11-24）

| 项 | 状态 | 说明 |
| --- | --- | --- |
| P3-1 子步 | 完成 | 通用 `SubstepCapabilityComponent`（共享物理层），投射物按内容 `extraUpdates+1` 装配；每子步独立 `Move`，碰撞即停 |
| P3-2 接触传递 | 完成 | `SpatialCommit` 增 `TileContact` / `IncomingVelocity` / `Substep`，不塞进既有四项语义 |
| P3-3 事实类型 | 完成 | `TileContactFact` + `TileContactGeometry`（瓦片索引用两个整数，物理层不依赖 World 域） |
| P3-4 队列 | 完成 | `TileContactQueueComponent`，`BeginCapture/Add/Drain` 与实体接触同形 |
| P3-5 生产者 | 完成 | 求解器产出接触几何（接触点/法线/瓦片），协调系统写队列；无队列组件的世界照常推进 |
| P3-6 子步号 | 完成（部分） | 瓦片接触带真实子步号；**实体接触仍恒 0**——`PhysicsContactCaptureSystem` 在移动之后整体捕获一次，不在子步循环内 |

改写的两条断言：`kinematics-substep-not-advanced` → `substep-emits-one-move-per-substep`（隔离世界数空间查询次数），
`substep-does-not-scale-distance` → `substep-capability-follows-content` + `substep-partitions-delta-time`。
新增 `tile-contact-recorded`（法线 -X、接触点落在墙面）、`tile-contact-carries-substep-and-incoming`（子步 1、入射速度 +40）。

未闭合：实体接触的子步号需要把接触捕获搬进子步循环（或由空间系统回报），属 P4 反弹顺序的前置。

### 8.5 实施记录（as-built，2026-11-24）

| 项 | 状态 | 说明 |
| --- | --- | --- |
| P4-1 反弹策略组件 | 完成 | `ProjectileBouncePolicyComponent`（P0 已建），本阶段接入唯一消费者 |
| P4-2 删除旧两件套 | 完成（P0） | `Capability`/`State` 已随 P0 删除，未回退 |
| P4-3 预算三计数 | 完成 | `Remaining`/`PendingConsumed`/`Consumed` + `Unlimited = -1`；四条转移：`PreviewConsume`/`ConfirmConsume`/`RollbackPending`/`ConsumeFor` |
| P4-4 生成快照 | 完成 | `FromLimit(definition.Penetration.MaximumHits)`；负值保持无限 |
| P4-5 瓦片结算系统 | 完成 | `ProjectileTileResolutionSystem`：Drain → 按 `ConsumeOrder` 判定 → 扣预算 → 反射速度 + 入 `MovementIntentRequest`，或入 `TerminationQueue` |
| P4-6 回执扣减/回滚 | 完成 | 入队时 `TryPreviewConsume`；`Applied` → `ConfirmConsume`；`Rejected` → `RollbackPending` |

关键实现决定（与设计文档有偏差处）：

1. **反弹同时写 `VelocityComponent` 并入队意图**。设计文档只写"入 `MovementIntentRequest`"，但意图是**期望速度**，
   `SpatialSimulationSystem` 用 `MoveTowards(current, desired, acceleration*dt)` 逼近，而投射物加速度恒为 0——
   只入队意图的话反射永远不生效。两项都做，逼近退化为"保持"。
2. **`SingletonComponent<T>` 改为重复即抛错**。原先静默取"最后一个"：测试里建了第二个
   `TileContactQueueComponent`，生产者写 A、消费者读 B，现象是"队列永远为空"，排查三轮才定位。
3. **预算上限以生成时快照为准**，不再每帧回读 `AttributeKind.ProjectilePenetrationLimit`：
   中途改属性不该让已飞出的弹体换算法。

踩坑记录：测试里对已装配 `ProjectilePenetrationStateComponent` 的实体再 `Add` 一次，
实体被移到新原型，`ProjectileMovementIntentSystem` 的查询随即选不中它（意图 tick 卡在 0）。
组件重复添加不报错，只静默换原型——测试里必须避免。

验收对照（设计 §9）：预算 1 命中一次后销毁、预算 N 可命中 N 个、被拒命中不扣预算、
同窗口重复接触只扣一次、反弹与命中之和达标即销毁、`PenetrationCost = 0` 不扣预算——
前六项由 `ProjectileTileResolutionSystem` 与 `ProjectileHitResolutionSystem` 共同保证并有断言；
第七项（命中+反弹同 tick 双耗尽）依赖 `ProjectileTerminationQueue` 的 handle 去重与原因优先级，
**瓦片路径已断言，双路径同帧耗尽仍属未闭合**。

---

## 8. P4：反弹策略 + 穿透预算状态机

### 8.1 任务

| # | 任务 | 落点 |
| --- | --- | --- |
| P4-1 | 新增 `ProjectileBouncePolicyComponent`：`MaximumBounces`、`PenetrationCost`、`SpeedScale`、`ConsumeOrder` | `Projectile/Component/ProjectileBouncePolicyComponent.cs` |
| P4-2 | 删除 `ProjectileBounceCapabilityComponent` / `ProjectileBounceStateComponent`（零消费者，非破坏性） | 同上目录 |
| P4-3 | 改造 `ProjectilePenetrationStateComponent` 为 `(Remaining, PendingConsumed, Consumed, SuppressesDamage)` + `Unlimited = -1` | `Projectile/Component/ProjectilePenetrationStateComponent.cs` |
| P4-4 | 生成时快照上限：`TrySpawn` 时把 `ProjectilePenetrationLimit` 解析结果写入 `Remaining` | `ProjectileLifecycleSystem.TrySpawn` |
| P4-5 | 新增 `ProjectileTileResolutionSystem`：Drain tile 接触 → 按 `ConsumeOrder` 判定 → 消耗预算 → 入 `MovementIntentRequest`（反射速度）或 `TerminationQueue` | `Projectile/ProjectileTileResolutionSystem.cs` |
| P4-6 | 命中回执按 `Applied` 扣减、`Rejected` 回滚 `PendingConsumed` | `ProjectileHitResolutionSystem.ApplyCore` |

### 8.2 语义表

| `Remaining` | 含义 | 伤害 | 反弹 |
| --- | --- | --- | --- |
| `> 0` | 还能命中 N 次 | 是 | 是（若 `MaximumBounces` 允许） |
| `0` | 耗尽 | `SuppressesDamage` 为真时否 | 否 |
| `-1` | 无限 | 是 | 是 |

`ConsumeOrder`：`BeforeBounce`（先判预算，够了才反弹，对齐 `:19020`）、`AfterBounce`（先反弹后判死，对齐 `:18108`）。

### 8.3 验证（对齐穿透预算设计 §9 验收标准）

- 预算 1 命中一次后**当 tick 末**销毁，且该次伤害已落地；
- 预算 N 可命中 N 个不同目标，第 N 次后销毁；
- 被无敌帧拒绝的命中**不扣**预算，弹体继续飞；
- 同一目标同一免疫窗口内重复接触只扣一次；
- 反弹消耗预算的弹体，命中+反弹之和达到预算时销毁；
- `PenetrationCost = 0` 反弹不扣预算；
- 同 tick「命中 + 反弹」同时耗尽只入队一条终止请求，原因取优先级高者。

---

## 9. P5：命中效果与内容扩展族（按需）

| 能力 | 归属 | 并入方式 | 优先级 |
| --- | --- | --- | --- |
| 命中附加状态效果 | Attributes/Buff | 目标实体写 `BuffApplyRequestComponent`，由 `BuffApplyRequestSystem` 消费；弹体侧只加 `ProjectileOnHitEffectComponent`（effect id + 概率 + 时长） | 高 |
| 反射伤害落账 | Combat | 消费 `DamageAppliedReceipt.ReflectedDamage`，产出 Life 请求 | 高（值已算出无人用） |
| 吸血落账 | Combat | 消费 `DamageAppliedReceipt.LifeSteal` | 中 |
| 召唤/哨兵归属 | Projectile + Attributes | `ProjectileMinionComponent`（owner、槽位、`MinionSlots`）；`AttributeKind.MinionCapacity` 已有但零消费者 | 中 |
| 鞭击几何与标签 | Projectile + Attributes | `ProjectileWhipComponent`（`WhipPointsForCollision` + `tagEffectType` + `bonusTagDamage`）；`AttributeKind.WhipRange` 已有零消费者 | 低 |
| 切草/切藤 | World | `WorldInteractionOperation` 增加 `CutTiles`，弹体只提交请求 | 低 |
| 投射物网络投影 | Network | 照 `NpcNetworkProjectionSystem` 新增 `ProjectileNetworkProjectionSystem` | 中（联调前必做） |
| 液体响应 | Physics + Projectile | 定义补 `LiquidContactComponent` + `ProjectileLiquidPolicyComponent` | 中 |
| 钓鱼/钩爪 | World + Player | 内容行为族，不建投射物专用系统 | 最低 |

### 9.1 P5 逐项状态（2026-11-24）

| 能力 | 优先级 | 状态 | 阻塞点 |
| --- | --- | --- | --- |
| 命中附加状态效果 | 高 | 未开工 | 需要 `ProjectileOnHitEffectComponent` + 目标侧 `BuffApplyRequestComponent` 写入，属跨域契约变更 |
| 反射伤害落账 | 高 | 未开工 | `DamageAppliedReceipt.ReflectedDamage` 已算出但无消费者，接它要先定 Life 请求的所有者 |
| 吸血落账 | 中 | 未开工 | 与反射共享同一条 Life 请求通道 |
| 召唤/哨兵归属 | 中 | 未开工 | `AttributeKind.MinionCapacity` 零消费者，需先定槽位归属 |
| 鞭击几何与标签 | 低 | 未开工 | 需要新几何类型与标签结算，优先级低于闭合现有边界 |
| 切草/切藤 | 低 | 未开工 | 需要 `WorldInteractionOperation.CutTiles` |
| 投射物网络投影 | 中 | 未开工 | 联调前必做；照 `NpcNetworkProjectionSystem` 建对称系统 |
| 液体响应 | 中 | 未开工 | 内容侧补字段即可，产品代码已就绪 |
| 钓鱼/钩爪 | 最低 | 未开工 | 明确不建投射物专用系统 |

## 10. 每期通用完成定义

1. 受影响项目单独构建，退出码 0、0 警告 0 错误，产物在 `Build/bin/`；
2. `Test/Terraria.Components/Terraria.Projectile` 新增断言全 PASS，且既有断言无回归；
3. 记录命令、退出码、警告/错误数、产物路径；
4. 若改动触碰 `Program.cs` 注册，跑一次真实服务器启动对照（`ecs.server.ready` 且 stderr 为空）；
5. 更新 `Projectile/README.md` 的组合模型与系统清单；
6. `Context/progress.md` 只追加一行轮次记录（≤2 行规则）。

## 11. 风险与回滚

| 风险 | 触发条件 | 处置 |
| --- | --- | --- |
| 双写位置 | P3 子步被实现成第二个位移写入者 | 子步必须在 `SpatialSimulationSystem` 内，`SpatialSimulationCoordinatorSystem` 仍是唯一写者；评审必查 |
| 定义字段膨胀成"第二个 Projectile 聚合" | P0-1 字段组不断加内容字段 | 字段只放**策略参数**，行为逻辑留在 AI 行为类；超 15 字段要重新设计 |
| 追踪目标抖动 | P2 每 tick 重选最近目标 | 目标切换需 `revision` + 切换冷却，不每 tick 换 |
| 反弹与预算顺序歧义 | P4 `ConsumeOrder` 取值不当 | 默认 `BeforeBounce`，宝石法杖类显式 `AfterBounce` |
| LVCS 内容错误阻断启动 | P0-6 抛 `InvalidDataException` | 这是设计意图（内容错误必须在启动期暴露），不降级为警告 |
| 与外部改动的构建冲突 | 已知 `EcsServer.cs` 曾引用未定义方法 | 先跑一次基线构建确认干净，再开始 |

回滚粒度：每期独立提交，回滚单期不牵连前后期。P0 的 `ProjectileDefinition` 构造参数变更是唯一破坏性改动，回滚需同时回退 `ProjectileEntityDefinitions` 与内容资产。

## 12. 顺序与依赖图

```text
P0 ──┬──→ P1 ──→ (里程碑：可生成)
     ├──→ P2 ──┐
     └──→ P3 ──┴──→ P4 ──→ P5
```

- P1、P2、P3 都只依赖 P0，可并行；
- P4 依赖 P3（需要 tile 接触事实）；
- P5 依赖 P2（目标）与 P4（预算）。

## 13. 开放问题（已落定）

| # | 问题 | 决定 | 证据 |
| --- | --- | --- | --- |
| 1 | `ProjectilePenetrationLimit` 快照还是实时 | **快照**，生成时 `FromLimit` 一次写入 `Remaining`，运行期不再回读属性 | 属性是内容策略输入；中途改属性不该让已经飞出去的弹体换算法。参考端同为快照 |
| 2 | `SubstepCapabilityComponent` 是否共享 | **共享**（`Share/Physics/Movement/Component`），投射物按 `extraUpdates + 1` 装配 | 子步是"位移如何切分"的物理问题，不是投射物专有；放在 Projectile 会让物理层反向依赖领域。NPC 后续可复用 |
| 3 | `stopsDealingDamageAfterPenetrateHits` 是否实现 | **实现**：`SuppressesDamage` 落在预算组件与 `ProjectilePenetrationPolicy`，内容 JSON 可配（`penetration.suppressesDamage`） | "打空"只表示不能再穿；是否继续造成伤害是另一条内容策略，两者必须分开判据（`CanConsume` vs `CanDealDamage`） |
| 4 | 命中判据取 `Applied` 还是 `Applied && AcceptedDamage > 0` | **`Applied`** | 全部被护盾吸收仍算一次命中；扣减处不检查实际扣血。被免疫/阵营/目标已死拒绝的回执走 `Rejected` 分支回滚预扣 |

### 13.1 实施中发现的新问题（留给后续）

| # | 问题 | 现状 |
| --- | --- | --- |
| 5 | 实体接触的子步号恒为 0 | `PhysicsContactCaptureSystem` 在位移完成后整体捕获一次，不在子步循环内；瓦片接触已有真实子步号。修它要把捕获搬进循环，或删字段并明确语义 → [DEF](../defects/2026-11-24-projectile-entity-contact-substep-always-zero.md) |
| 6 | 追踪转向速率单调衰减（实测 8 → 7.000） | AI 侧归一化锚点是**当前**速度而非出厂速度，空间侧再逼近一次弦，每帧净缩一步。断言按实测放成 `6.5 < speed <= 8.05`，把缺陷写成了容差 → [DEF](../defects/2026-11-24-projectile-homing-speed-drift.md) |
| 7 | 双路径同帧耗尽时的终止唯一性靠隐式守卫维持 | 瓦片路径显式查 `Contains`，命中路径在 `CheckAdmission` 里顺带查；`Enqueue` 不去重、原因优先级未集中定义，唯一性无测试固定 → [DEF](../defects/2026-11-24-projectile-termination-uniqueness-implicit.md) |
| 8 | 客户端输入无 `PlayerItemUseRequest` 生产者 | Communication 层从未把客户端使用物品翻译成请求，端到端"真客户端开火"无法闭合，属独立项目 |
| 9 | AI 行为工厂按类型注册，profile 数值不生效 | `AiSystem` 工厂键是 `Type`；适配器只取 `BehaviorType`，`ProjectileAiRegistration.Bind` 无调用点。靠 profile 数值全为 0 而未触发 → [DEF](../defects/2026-11-24-projectile-homing-profile-values-not-applied.md) |
