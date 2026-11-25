# Projectile 域与共享系统的职责重叠审计

| 项 | 值 |
| --- | --- |
| 文档 ID | `AUDIT-2026-11-25-PROJECTILE-SHARED-OVERLAP` |
| 状态 | `open`（§4 第 1–5 项已落地并复核；6–9 未开工） |
| 触发 | 用户质疑：`ProjectileAiAdapter` 这类「数据融合模型」的职责已被共享系统解决，Projectile 下还有多少同类 |
| 范围 | `src/NSSLC/Projectile/**` 全部 62 个文件，对照 `src/NSSLC/Share/**` 八个共享域（审计当时计 9，含 2026-11-25 迁出到基础设施的 LVCS） |
| 判据 | 「被共享系统解决」= 共享域已提供同职责的类型或协议，且 Projectile 侧实现没有超出该协议的额外语义 |

## 1. 结论摘要

62 个文件里，**6 处**存在「共享域已提供等价能力，Projectile 侧另行实现」的重叠；另有 **3 处**是本域自造的死值或过期物，不涉及共享域。

| # | 模型 | 共享域已有的等价能力 | 重叠性质 |
| --- | --- | --- | --- |
| 1 | `ProjectileAiSnapshotProvider` | `IAiSnapshotProvider<T>` + `NpcAiSnapshotContributors` 的按源域拆分模式 | 机制重复 |
| 2 | `ProjectileTargetAcquisitionSystem` | `NpcAiTargetSnapshotContributor` 的最近目标选择 | 算法重复 |
| 3 | `ProjectileBehaviorCatalog` + `ProjectileBehaviorProfile` | `AiBehaviorCatalog<TIntent>` + `AiBehaviorDeclaration<TIntent>` | 机制重复（处置为不合并，§3 第 5 行） |
| 4 | `ProjectileHitAdmissionResult` | `DamageRejectionReason`（同名 `TargetImmune` 已存在） | 码重复 |
| 5 | `ProjectileAiAdapter` / `ProjectileMovementIntentSystem` 直写 `MovementIntent` | `MovementIntentRequestQueue` + `MovementIntentRequestSystem` | 写路径重复 |
| 6 | `ProjectileRuntimeRegistry` 的三个注册别名 | `EntityRuntimeRegistry` 自身已有 `Register`/`RegisterEntity`/`RegisterOwner` | 别名套别名 |

本域内自造、与共享域无关的：

| # | 模型 | 问题 |
| --- | --- | --- |
| 7 | `ProjectileSpawnRequests` | 注释自述「迁移期兼容入口」，但其唯一生产调用者已在同程序集 |
| 8 | `ProjectileAiAdapter.AttachedLastUpdate` | 写后零读取（连测试都没读） |
| 9 | `ProjectileRuntimeRegistry._terminated` | 终止状态的第二个记录点，边界需要重新定义（**不建议直接删**，见 §3） |

## 2. 逐项证据

### 2.1 `ProjectileAiSnapshotProvider` —— 共享投影协议的第二个实现

- 共享侧：`Share/AI/Query/IAiSnapshotProvider.cs` 定义协议；`Share/AI/System/AiRequirements.cs` 用 `Components` 声明行为要读的输入；NPC 把投影拆成 4 个 `INpcAiSnapshotContributor`（movement / target / life / world），由 `NpcAiSnapshotContributors.CreateDefault` 组装。
- Projectile 侧：`Projectile/System/ProjectileAiSnapshotProvider.cs:35-66` 在一个方法里内联四个 `if (requiredComponents.Contains(...))` 分支——正是四个 contributor 压平成一个类。
- 不重叠的部分：`:68-84` 用 `IEntityRuntimeResolver` 解析目标句柄，并在「目标已死 / 隐身生效」时不写事实。这是投射物特有的降级语义，共享协议没有。
- 判定：**投影机制重复、投影语义不重复**。处置方向是把「按 `requiredComponents` 分派」换成 contributor 组装，让 Projectile 只贡献自己的两个 fact 类型。

### 2.2 `ProjectileTargetAcquisitionSystem` —— 与 NPC 目标选择同算法

- NPC 侧：`Npc/System/NpcAiSnapshotContributors.cs:112-151`，Query `EntityFactionComponent + LifeComponent + LocationComponent`，排除自身、判阵营、判存活、`Vector2.DistanceSquared` 取最近，顺带算可见性。
- Projectile 侧：`Projectile/ProjectileTargetAcquisitionSystem.cs:98-150`，同样的 Query 形态、同样的距离平方比较、同样的阵营与存活过滤，额外排除 owner 句柄。
- 差异：阵营判据相反（NPC 找 `Faction.Friendly`，投射物找「不等于我这边」）；Projectile 写 `ProjectileTargetComponent`，NPC 只投影 fact 不写组件。
- 判定：**「最近实体」的空间选择算法在仓里有两份**。可抽共享的最近实体选择器（输入原点、半径、判据、排除句柄；输出候选），两侧各自保留判据与落点。
- 已处置：共享选择器 `Share/Entity/Query/NearestEntityQuery.cs`，NPC 与 Projectile 两侧同批改为调用它；逐分支等价性核对见 §6.2。

### 2.3 `ProjectileBehaviorCatalog` / `ProjectileBehaviorProfile` —— 与共享行为目录同构

- 共享侧：`AiBehaviorCatalog<TIntent>` 按 `Type` 键、`Seal()` 后拒绝新增、重复注册抛异常；`AiBehaviorDeclaration<TIntent>` 承载 `BehaviorType` + 工厂 + `Reads`。
- Projectile 侧：`ProjectileBehaviorCatalog.cs:14-44` 按 `AiBehaviorProfileId` 键、同样的 `Seal`、同样的重复拒绝，外加「同一 BehaviorId 绑定不同 BehaviorType 则拒绝」（`:28-36`）。`ProjectileBehaviorProfile` 承载 `BehaviorType`/`Requirements`/`MaximumSpeed`/`TurnRate`/`Range`。
- 重叠面：`BehaviorType` 与 `Requirements` 已被 `AiBehaviorDeclaration` 承载；Seal 与重复拒绝的目录语义已被 `AiBehaviorCatalog` 承载。
- 不重叠面：`MaximumSpeed`/`TurnRate`/`Range` 是内容参数，profile 维度（同一行为多套参数）也是共享目录没有的。
- 判定：**目录机制重复、参数内容不重复**。
- 已处置（不并入）：厘清边界后，两者键与载荷都不同——共享目录的键是行为类型、载荷是「谁实现、读哪些事实、怎么造实例」，投射物目录的键是 `(BehaviorId, ProfileId)`、载荷是「这条内容用哪组数」。并入的代价与替代路径见 §3 第 5 行；边界由两条拒绝固定，见 §6.3 第 8 行。参数化交给链路修复计划 P2。

### 2.4 `ProjectileHitAdmissionResult` —— 与共享 `DamageRejectionReason` 码重复

| Projectile 枚举值 | 共享 `DamageRejectionReason` 对应值 |
| --- | --- |
| `ProjectileUnavailable` | `SourceUnavailable` |
| `ProjectileInactive` | `SourceInactive` |
| `TargetImmune` | `TargetImmune`（同名同义） |
| `Accepted` | `None` |

- 现状（审计时）：生产侧只在 `ProjectileDamageRequestSystem.cs:83` 与 `ProjectileHitResolutionSystem` 内部流转；**测试全仓零引用**。
- 已处置：先补 5 条准入覆盖，再把本枚举整体换成 `DamageRejectionReason`，枚举文件删除；裸值 `0` 仍是「放行」，只是名字从 `Accepted` 变成 `None`。
- 判定：这是[错误处理约束](../../cr/CR-2026-11-25-error-handling-constraint.md)「单一码注册表」要清理的典型样本——同一语义在两个域各有一套码。

### 2.5 `ProjectileAiAdapter` / `ProjectileMovementIntentSystem` 直写 `MovementIntent`

这是用户最初指的那一处，也是本审计里**唯一有共享协议明文规定写路径**的一处。

- 共享侧：`Share/Physics/Movement/MovementIntentRequestSystem.cs` 是意图安装点，注释写明「注册在仿真阶段所有生产者之后、空间协调器之前」；生产者应入 `MovementIntentRequestQueue`。
- NPC 侧：`NpcAiIntentAdapter.cs:152` 走 `MovementIntentRequestQueue.Require(World)`，并受 `NpcAiIntentCommitTable` 的提交表约束（每个 intent kind 只能写表里声明的那一个目标）。
- Projectile 侧有**两条互相矛盾的写法**：
  - `ProjectileAiAdapter.cs:142`：`World.Get<MovementIntent>(entity) = new MovementIntent(...)` —— 直写实体组件；
  - `ProjectileTileResolutionSystem.cs:73-138`：`MovementIntentRequestQueue.Require(World).Enqueue(...)` —— 走共享队列；
  - `ProjectileMovementIntentSystem.cs:47`：`intent = MovementIntent.Stop(tick)` —— 又一次直写。
- 判定：同一个域里，同一份状态有三种写法，其中两种绕过共享协议。这不是「模型冗余」而是**写权限没收敛**；[链路修复计划](../../plans/2026-11-25-projectile-chain-repair-execution-plan.md)的 P1-4 / P5-1 正是这件事，状态仍是 `proposed`，未实施。

### 2.6 `ProjectileRuntimeRegistry` 的注册别名

- 共享 `EntityRuntimeRegistry` 已把 `RegisterEntity` 与 `RegisterOwner` 都实现成 `Register` 的纯别名（`:90-98`），本身已经是「一实两别名」。
- `ProjectileRuntimeRegistry` 又叠了第三个：`RegisterProjectile` → `RegisterOwner` → `Register`（`:34-52`）。
- 判定：别名套别名。要么只留 `Register` 一个入口，要么把三个名字的语义差异写清楚；现状是三个名字同一实现，读者无法判断该用哪个。
- 已处置：`RegisterProjectile` 与 `RegisterEntity` 零调用者，`RegisterOwner` 只被验证项目当作通用绑定入口用。三者合并为单个 `Register`；测试里两处原本登记的是「目标」却写成 `RegisterOwner`，正是旧名字误导的地方。

### 2.7 `ProjectileSpawnRequests` —— 迁移期入口，理由已消失

`ProjectileSpawnRequests.cs:11-13` 自述「迁移期的旧投射物实现位于另一个程序集，需要一条不依赖内部约定的入队路径」。现状：生产侧唯一调用者是同程序集的 `ProjectileItemUseSpawnSystem.cs:109`，另有两处测试调用；跨程序集的旧实现已不存在。

### 2.8 `ProjectileAiAdapter.AttachedLastUpdate` —— 写后零读取

`ProjectileAiAdapter.cs:54` 声明、`:58` 清零、`:125` 自增；src 与 Test 全仓零读取。

## 3. 判据不成立、需要另立口径的四处

| 候选 | 为什么不能按「共享已解决」处理 |
| --- | --- |
| `ProjectileRuntimeRegistry._terminated` | 实体存活时终止状态由 `ProjectileDispositionStateComponent.Terminated` 承载；实体销毁后组件消失，只剩 `_terminated` 能区分「已终止」与「陈旧句柄」（`ProjectileLifecycleSystem.cs:165,238` 是它的两个读者）。共享 `EntityRuntimeRegistry` 只保存代际（`_generations`），**不保存终止历史**，所以它不是共享域的等价物，而是 Projectile 私有的注册表扩展。直接删会让「重复终止」返回 `StaleReference` 而不是 `AlreadyTerminated` |
| `ProjectileId` vs 共享 `EntityId` | 同形（readonly record struct + string + 校验）但异义：`EntityId` 是实体身份，`ProjectileId` 是内容定义键。合并会让内容键获得实体语义 |
| `ProjectileSpawnQueueComponent` / `ProjectileTerminationQueueComponent` | 共享层有 10 个同形 tick 队列组件，但**没有**通用 `TickScopedQueue<T>` 基类。属「形态重复待抽取」，不属于「已被共享系统解决」 |
| `ProjectileLifecycleStatus` | `Deferred`/`AlreadyTerminated`/`StaleReference` 是领域状态机，共享 `DamageProcessStatus` 语义范围不同 |
| `ProjectileBehaviorCatalog` 与 `AiBehaviorCatalog` | 两者都叫目录、都 Seal、都拒绝重复注册，但**键与载荷不同**：共享目录的键是行为**类型**，载荷是「谁实现、读哪些事实、怎么造实例」；投射物目录的键是 `(BehaviorId, ProfileId)`，载荷是「这一条内容用哪组数」。并入只有两条路——把 `MaximumSpeed`/`TurnRate`/`Range` 挤进共享域，或让共享目录认识 profile；前者污染领域边界，后者的正确落点是[链路修复计划](../../plans/2026-11-25-projectile-chain-repair-execution-plan.md) P2 的「按 profile 造实例」工厂（该计划状态 proposed，尚未开工）。因此本轮只把边界固定成两条拒绝（同一 `BehaviorId` 只能绑一个实现类型；profile 必须由两个键共同定位），把参数化交给 P2 |

## 4. 处置顺序与状态

第 3 行是审计初稿漏记的一项（§1 第 6 号的注册别名），补回；序号不以「已落地」为准，只表示顺序。

| 序 | 动作 | 风险 | 前置条件 | 状态 |
| --- | --- | --- | --- | --- |
| 1 | 删 `AttachedLastUpdate` | 无 | 无 | 已落地 |
| 2 | `ProjectileSpawnRequests` 收回 `internal` 并删过时注释 | 低 | 确认无跨程序集调用者 | 已落地（确认：无） |
| 3 | `ProjectileRuntimeRegistry` 三个注册别名收敛为 `Register` | 低 | 无 | 已落地 |
| 4 | `ProjectileHitAdmissionResult` 并入 `DamageRejectionReason` | 中 | 先补覆盖（审计时零测试） | 已落地 |
| 5 | 目标选择抽共享「最近实体」选择器 | 中 | NPC 与 Projectile 两侧同批改 | 已落地并复核（`Share/Entity/Query/NearestEntityQuery`，两侧同批改；与 1–4 非同一轮，见 §6.2） |
| 6 | 意图写路径统一到 `MovementIntentRequestQueue` | 中 | 依赖链路修复计划 P1-4 / P5-1 | 已落地（依赖按其口径在域内自建：意图生产者入队、安装步骤在空间求解前执行，见 §6.3） |
| 7 | 快照投影改 contributor 形态 | 中高 | 先补 Projectile 快照覆盖 | 已落地（先补三条覆盖，再改投影形态，见 §6.3） |
| 8 | 行为目录并入 `AiBehaviorCatalog` | 高 | 已厘清 profile 参数与 declaration 的边界 | 已处置：**不并入**，判据见 §3 第 5 行；边界由两条断言固定，见 §6.3 |
| 9 | `_terminated` 归属重新定义 | 高 | 先决定终止历史归注册表还是归域 | 已落地：终止历史归投射物生命周期（独立日志），注册表退回纯句柄解析，见 §6.3 |

第 5 项与前面几项不同轮：两侧差异远不止阵营判据——NPC 版距离上限写死 3000、
顺带算 `InvisibilityComponent` 可见性；Projectile 版距离由调用方传入且 0 表示不限、排除 owner
句柄。落地时按「选择器只管挑最近、其余留在两侧」收敛，五处差异各自有归属：

| 差异 | 归属 |
| --- | --- |
| 阵营判据（同阵营 / 不是我这边的） | 调用方判据，不进选择器 |
| 距离上限与 0 表示不限 | 选择器参数：正数为上限，0 或负数为不限 |
| 排除句柄（owner、自身） | 调用方判据，投射物侧仍由判据内解析句柄 |
| 附带事实（NPC 要可见性、投射物要句柄） | 选择器只返回实体与坐标，附加事实由调用方用实体再查一次 |
| 平距口径（`<` 先到者胜 / `<=` 后到者胜） | 统一为先到者胜，结果只取决于查询顺序 |

因此选择器的签名只有四件事：原点、半径、判据、输出候选。距离上限由「小于」改为「严格小于」
后，投射物在恰好等于射程的边界上由「选中」变为「不选中」，其余路径逐分支等价。

## 5. 与内容组合分解计划的关系

[Projectile 内容组合分解计划](../../plans/2026-11-25-projectile-content-composition-decomposition-plan.md)的范围是**内容定义与装配链路**，其「排除范围」明文写了「AI 行为与 Profile」「运行期系统」。因此本审计的 6 项重叠**都不在该计划范围内**——那计划完成，不代表这些重叠已消除。本审计是它们的 canonical 记录。

## 6. 落地记录

### 6.1 2026-11-25：§4 第 1–4 项

| 序 | 动作 | 结果 | 验证 |
| --- | --- | --- | --- |
| 1 | 删 `AttachedLastUpdate` | 属性、每帧清零与自增三处一并删除；无读者受影响 | 投射物验证项目 0 失败 |
| 2 | `ProjectileSpawnRequests` 收回 `internal` | 类型改 `internal`，注释改为「同程序集入队接缝」，删掉已消失的迁移期理由 | 生产侧唯一调用者与测试调用者均在可访问范围内（后者靠既有的 `InternalsVisibleTo Terraria.Projectile`） |
| 3 | `ProjectileRuntimeRegistry` 注册别名收敛 | `RegisterProjectile`、`RegisterEntity`、`RegisterOwner` 三者合并为 `Register`，委派共享 `EntityRuntimeRegistry.Register`；3 处测试调用点改名 | 三者原本同名同实现，前两个零调用者；改名后编译器兜底，`Terraria.Projectile` 全绿 |
| 4 | `ProjectileHitAdmissionResult` 并入 `DamageRejectionReason` | 枚举文件删除；`CheckAdmission` / `Apply` / `ApplyCore` 改为返回共享码，判定顺序与分支归属逐行原样；`CheckAdmission` 补 remarks 说明弹体视角下 `Source*` / `TargetImmune` 各指什么 | 先补 `admission-*` 五项覆盖并跑绿，再改码，改完再跑绿 |

验证命令与结果（本轮实测）：

| 命令 | 结果 |
| --- | --- |
| `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal` | 退出码 0，0 警告 0 错误，产物 `Build/bin/Terraria.Components/Debug/net10.0/` |
| `dotnet build Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj --nologo --verbosity minimal` | 退出码 0，0 警告 0 错误，产物 `Build/bin/Terraria.Projectile/Debug/net10.0/` |
| `dotnet run --project Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj --no-build --no-restore` | 退出码 0，无 `FAIL` 行，末行 `PASS: projectile spawn produces a spatial-simulatable entity.` |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo --verbosity minimal` | 退出码 0，0 错误（28 个警告全部来自 `NSSLC.Infrastructure.WorldGeneration` 既有代码，与本次改动无关） |

新增覆盖落在 `Test/Terraria.Components/Terraria.Projectile/Program.cs` 的「命中准入四态」一节，
固定五件事：活跃弹体放行、句柄未绑定实体判源不可用、处置已终止判源不活跃、
已记录命中在窗口内判目标免疫、窗口到期后回到放行。

第 3 项的语义映射写进了 `ProjectileHitResolutionSystem.CheckAdmission` 的 remarks：源与目标按弹体视角读——
`SourceUnavailable` 是弹体句柄解析不到，`SourceInactive` 是弹体已终止或已排队终止，`TargetImmune` 是**本弹体**
对该目标的命中冷却（目标侧保护窗口仍由 `CanHit` 走 `DamageImmunityQuery` 负责，两者不可混）。

验证方式与顺序：先补 `admission-*` 五项行为基线（四态各一条 + 免疫到期回到放行），再改返回类型；
改后同一批断言全绿，证明「换码不换判定」。命令与退出码见本轮变更记录。

### 6.2 2026-11-25：§4 第 5 项

| 项 | 内容 |
| --- | --- |
| 新增 | `src/NSSLC/Share/Entity/Query/NearestEntityQuery.cs`：`NearestEntityQuery.TryFind`（世界、候选查询、原点、半径、判据、输出候选）、`NearestEntityPredicate`、`NearestEntityCandidate` |
| 共享侧调用方 | `NpcAiSnapshotContributors.cs` 的 `NpcAiTargetSnapshotContributor`：上限提为 `MaximumTargetDistance` 常量，可见性改为选中后用实体再查一次 |
| 域侧调用方 | `ProjectileTargetAcquisitionSystem.TryFindNearestHostile`：半径仍由 `ProjectileHomingCapabilityComponent.Range` 传入（0 表示不限），排除自身与归属者的判据留在调用方 |
| 新增覆盖 | `Test/Terraria.Components/Terraria.Npc.Ai/Program.cs` 的 `VerifyNearestEntityQuery`：最近候选与距离平方、半径严格上限、判据放行时选中最近者（含死亡实体）、全体被拒返回 `false` |

逐分支等价性核对（旧实现对照）：

| 差异 | 旧实现 | 新实现 | 影响 |
| --- | --- | --- | --- |
| 半径边界 | NPC 侧 `<`（3000 不含端）；投射物侧 `<=`（射程含端） | 统一 `<` | 投射物在距离恰好等于射程处由「选中」变「不选中」，即 §4 记录的口径统一，其余分支相同；追踪行为自身那一层仍按「不大于」判定，只在同一个边界值上退化为匀速直线（已在 `TryFindNearestHostile` 的 remarks 写明） |
| 平距（等距离候选） | 两侧都取先遍历到的（严格 `<` 不覆盖） | 同 | 无 |
| 自身排除、阵营、存活、归属者 | 查询回调内过滤 | 调用方判据内过滤 | 无（判据整体移出选择器） |
| 可见性 / 运行时句柄 | 命中时顺带算出 | 选中后用实体再查一次 | 无（多一次查询，且只在命中时发生） |
| 参数校验 | 无（NaN 原点会让候选全部落选或全部入选） | 非有限原点/半径抛 `ArgumentOutOfRangeException` | 新失败通道；两条调用路径的入参都来自世界状态，异常会向上冒到 `EcsServer.Update` 并置 `_faulted`，属「坏状态不静默」的有意选择 |

验证命令与结果（本轮实测）：

| 命令 | 结果 |
| --- | --- |
| `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal` | 退出码 0，0 警告 0 错误 |
| `dotnet build Test/Terraria.Components/Terraria.Npc.Ai/Terraria.Npc.Ai.csproj` | 退出码 0，0 警告 0 错误 |
| `dotnet run --project Test/Terraria.Components/Terraria.Npc.Ai/Terraria.Npc.Ai.csproj --nologo` | 退出码 0，无 `FAIL` 行，末行 `PASS: Guide idle/wander/stop, Slime targeting/jump, AI intent, spatial movement and NPC wire projection.`（`shared-api: ai=ok buffer=ok condition=ok contributionKinds=11`） |
| `dotnet build Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj` | 退出码 0，0 警告 0 错误 |
| `dotnet run --project Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj --nologo` | 退出码 0，无 `FAIL` 行，`PASS p2-target-acquired`（`locked=True`）与 `PASS target-revision-bumps-on-switch`（`revision=2`）保持通过 |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo --verbosity minimal` | 退出码 0，0 警告 0 错误 |
| `dotnet run --project Test/NSSLC.Server/Terraria.Server.Lvcs/Terraria.Server.Lvcs.csproj --nologo` | 退出码 0，`RESULT: PASS` |

§4 第 6–9 项见 §6.3（本轮落地）。第 6 项原先挂的「依赖链路修复计划 P1-4 / P5-1」不再阻塞：该依赖要的是「意图只有一个写者」这个结果，域内即可满足。

### 6.3 2026-11-25：§4 第 6–9 项

| 序 | 动作 | 结果 | 验证 |
| --- | --- | --- | --- |
| 6 | 意图写路径统一到 `MovementIntentRequestQueue` | 适配器与投射物意图系统都不再持有 `MovementIntent` 的写权限：两者只解析本域实体句柄并把意图投进共享队列，由共享安装系统在空间求解前落地。原先挂的「依赖链路修复计划 P1-4 / P5-1」取消阻塞——那两条要的是结果，不是那批次的实现载体 | 新增 `ai-intent-goes-through-request-queue`：适配器跑完、安装之前组件未被改写且队列非空；安装后组件 tick/source/desired 全部更新。只测最终值测不出「没写权限」这件事 |
| 7 | 快照投影改 contributor 形态 | `ProjectileAiSnapshotProvider` 只剩「按声明过滤、按顺序调用」，四种事实分别住进位置 / 速度 / 追踪能力 / 目标四个 contributor；同一事实被两个 contributor 认领在构造期失败 | 先补三条覆盖再改形态：默认集合覆盖四项事实、重复认领构造期抛 `ArgumentException`、只声明一项时不调用其余 contributor（用计数 contributor 观察） |
| 8 | 行为目录并入 `AiBehaviorCatalog` | **不并入**，判据见 §3 第 5 行。边界落成两条拒绝：同一 `BehaviorId` 绑定不同实现类型即失败；profile 必须由 `(BehaviorId, ProfileId)` 共同定位。参数化（按 profile 造实例）留给链路修复计划 P2 | 新增 `behavior-catalog-type-held-by-declaration`、`behavior-catalog-keyed-by-both-ids` |
| 9 | `_terminated` 归属重新定义 | 终止历史从共享句柄注册表搬进投射物自己的 `ProjectileTerminationJournal`；注册表退回纯「句柄→实体」解析，域内只剩一个注册入口。实体销毁后仍能区分「重复终止」与「陈旧句柄」 | 新增四条：首次提交 `Applied` 且历史记一条、同句柄二次提交 `AlreadyTerminated` 且历史不增、从未记过的句柄 `StaleReference`、`Clear` 后历史清空 |

#### 本轮顺带暴露：子步实体的位移与速度被均分

第 6 项落地后，既有用例 `bounce-reverses-velocity-end-to-end` 由「恰好 `-20`」变成 `-10`。
根因不在接缝本身，而是接缝修好后才显形的一处旧缺陷：

| 项 | 内容 |
| --- | --- |
| 现象 | `extraUpdates = 1`（两子步）的弹体每 tick 掉一半速度，位置也少走一半；最终速度按子步数的幂次衰减（两帧 `-20 → -10`） |
| 根因 | 共享空间求解的提交把「每子步速度」又乘了一次 `substepDelta`。位移的时间基应是整帧 `deltaTime`：`motionVelocity` 已按子步归一，再乘子步时长等于把本帧位移与回写速度一起除以子步数 |
| 为何此前没显形 | 旧接缝下 AI 自己写组件，撞墙后没人改写意图，tick+1 的空间求解读到的是撞墙帧的零目标意图，位移走 `(0,0) * deltaTime` 这条恒等式，均分被掩盖 |
| 处置 | 位移时间基改回整帧 `deltaTime`。单子步实体两侧相等，NPC 与玩家路径行为不变 |
| 影响面 | 全仓 `SubstepCapabilityComponent` 只有投射物内容装配（`extraUpdates + 1`），NPC 与玩家恒为 1 子步 |
| 验证 | 该用例回到 `-20.00`；`Terraria.Npc.Ai`、`Terraria.Entity.Scheduling`、`Terraria.Entity.Definition` 与真实启动均无回归 |

验证命令与结果（本轮实测）：

| 命令 | 结果 |
| --- | --- |
| `dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal` | 退出码 0，0 警告 0 错误 |
| `dotnet build Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj` | 退出码 0，0 警告 0 错误 |
| `dotnet run --project Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj --no-build --no-restore` | 退出码 0，111 条 PASS / 0 条 FAIL（含本轮新增 9 条），末行 `PASS: projectile spawn produces a spatial-simulatable entity.` |
| `dotnet run --project Test/Terraria.Components/Terraria.Npc.Ai/Terraria.Npc.Ai.csproj --nologo` | 退出码 0，无 `FAIL` 行 |
| `dotnet run --project Test/Terraria.Components/Terraria.Entity.Scheduling/Terraria.Entity.Scheduling.csproj --nologo` | 退出码 0，无 `FAIL` 行 |
| `dotnet run --project Test/Terraria.Components/Terraria.Entity.Definition/Terraria.Entity.Definition.csproj --nologo` | 退出码 0，无 `FAIL` 行 |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo --verbosity minimal` | 退出码 0，0 错误（28 个警告全部来自 `NSSLC.Infrastructure.WorldGeneration` 既有代码） |
| `dotnet run --project Test/NSSLC.Server/Terraria.Server.Lvcs/Terraria.Server.Lvcs.csproj --nologo` | 退出码 0，`RESULT: PASS` |
| 真实启动 20 s（`NSSLC_WORLD_FILE=src/World/科研.wld`） | `ecs.server.ready` 出现 1 次，stderr 0 行，`ecs.projectile.precheck` 持续输出 `admittedAuthorityRejected=0` |

未闭合：本审计已无剩余处置项。§3 记录的四类「判据不成立」仍不属本审计范围；
[链路修复计划](../../plans/2026-11-25-projectile-chain-repair-execution-plan.md) P2 接手 profile 参数化，
其 P5-1 已由本轮第 6 项先行满足。
