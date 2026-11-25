# 投射物内容族、射击接缝、AI 入口、子步接触与反弹预算（执行计划 P0–P4）

文档 ID：CR-2026-11-24-PROJECTILE-P0-P1-CONTENT-AND-SHOOT-SEAM
逻辑域：projectile
产物类型：execution
状态：in-progress（P0–P4 完成并验证；P5 为按需菜单，未开工）
日期：2026-11-24
canonical 路径：`docs/cr/CR-2026-11-24-projectile-p0-p1-content-and-shoot-seam.md`
（路径中保留 `p0-p1` 字样是为了不打断既有引用；本 CR 现已覆盖 P0–P4）
上游依据：[Projectile 组件系统补齐执行计划](../plans/2026-11-24-projectile-component-systems-execution-plan.md) §4；
[LVCS 统一加载系统 API 设计](../architecture/2026-11-23-lvcs-unified-loading-api-design.md)。

## 1. 授权与范围

用户要求执行 `docs/plans/2026-11-24-projectile-component-systems-execution-plan.md`。本 CR 覆盖：

- **P0**：内容族接 LVCS + 定义扩展 + 能力装配；
- **P1**：物品使用 → 投射物生成请求的射击接缝；
- **P2**：目标获取 + AI 适配器（首个 `ProjectileTargetFact` 生产者，修复 B5/B6）；
- **P3**：子步推进 + 瓦片接触事实（修复 B8 高速穿墙）；
- **P4**：反弹策略 + 穿透预算状态机（B7/B11/B12）。

**P5**（命中效果与内容扩展族）是「按需」能力菜单，本期未开工，逐项状态见计划 §9.1。范围限定在 `src/NSSLC/Projectile/**`、`src/NSSLC/Share/ItemUse/**`、
`src/NSSLC.Server/Content/**`、`src/NSSLC.Server/Registration/**`、`src/NSSLC.Server/Program.cs` 的
加载、攻击定义与系统接线，以及三个验证项目；未改参考客户端、协议包号、`src/NSSLC.Infrastructure/**`。

## 2. 变更清单

### P0（内容族）

| 动作 | 路径 | 说明 |
| --- | --- | --- |
| 新增 | `Projectile/System/ProjectileGravityPolicy.cs` | 定义侧重力策略 |
| 新增 | `Projectile/System/ProjectileLiquidPolicy.cs` | 定义侧液体策略 |
| 新增 | `Projectile/System/ProjectilePenetrationPolicy.cs` | 定义侧穿透上限（`-1` 无限） |
| 新增 | `Projectile/System/ProjectileBouncePolicy.cs`、`ProjectileBounceConsumeOrder.cs` | 定义侧反弹策略与消费顺序 |
| 新增 | `Projectile/System/ProjectileHomingPolicy.cs` | 定义侧追踪参数 |
| 新增 | `Projectile/Component/ProjectileBouncePolicyComponent.cs` | 反弹策略组件（替换旧两件套） |
| 删除 | `Projectile/Component/ProjectileBounceCapabilityComponent.cs`、`ProjectileBounceStateComponent.cs` | 全仓零读取；反弹次数与穿透预算共用计数 |
| 改写 | `Projectile/System/Projectile.cs` | 构造参数补策略组、碰撞体/尺寸默认值、伤害类型与基础伤害 |
| 改写 | `Projectile/System/ProjectileEntityDefinitions.cs` | 按定义装配重力/液体/追踪/反弹能力，移除 `GravityCapabilityComponent(Enabled:false)` 硬编码 |
| 改写 | `Projectile/System/ProjectileCatalog.cs` | 补 `Count` / `Definitions` / `IsSealed`，供加载与攻击定义投影 |
| 新增 | `NSSLC.Server/Content/Projectile/ProjectilePrototype.cs` | JSON 解析目标（嵌套字段组不承载行为） |
| 新增 | `NSSLC.Server/Content/Projectile/ProjectileCodec.cs` | 族 `projectile`，`.json`；逐条校验 + 整批投影并 `Seal()` |
| 新增 | `NSSLC.Server/Content/Prototypes/Projectile/projectile.json` | `wooden-arrow` / `homing-spark` / `bouncer-orb` 三条 |

### P1（射击接缝）

| 动作 | 路径 | 说明 |
| --- | --- | --- |
| 新增 | `Share/ItemUse/Content/ItemShootProfile.cs` | 射击内容参数（投射物 id 文本 + 速度 + 出生偏移） |
| 改写 | `Share/ItemUse/Content/ItemUse.cs` | 增加可空 `Shoot`（init），既有八参构造调用点不受影响 |
| 改写 | `NSSLC.Server/Content/ItemUse/ItemUsePrototype.cs` | `shoot` 嵌套解析目标（`projectileId/velocityX/velocityY/offsetX/offsetY`） |
| 改写 | `NSSLC.Server/Content/ItemUse/ItemUseCodec.cs` | 投影 `Shoot`；空 id / 零速度 / 非有限偏移在加载期拒绝 |
| 改写 | `NSSLC.Server/Content/Prototypes/ItemUse/item-use.json` | 铜弓射 `wooden-arrow`、魔法导弹射 `homing-spark` |
| 新增 | `Projectile/ProjectileItemUseSpawnSystem.cs` | 排空物品使用事实 → 查内容 `Shoot` → 入队生成请求，逐条记录路由结果 |
| 新增 | `Projectile/System/ProjectileItemUseSpawnStatus.cs`、`ProjectileItemUseSpawnResult.cs` | 事实的路由结果（入队 / 缺使用定义 / 无射击配置 / 定义未知 / 持有者不可用） |
| 改写 | `NSSLC.Server/Registration/EcsServerRegistrationKeys.cs` | 新增 `combat.projectile.item-use-spawn` |
| 改写 | `NSSLC.Server/Program.cs` | 投射物目录改走 LVCS 并按内容注册攻击定义（P0）；射击接缝注册在 Preparation、`before: [CombatProjectileSpawn]`（P1） |
| 改写 | `Test/Terraria.Components/Terraria.Projectile/Program.cs` | P0 装配断言 + P1 射击接缝断言（含跨 tick 端到端） |
| 改写 | `Test/NSSLC.Server/Terraria.Server.Lvcs/Program.cs` | 投射物与物品使用两族的真实内容加载、反例与分派解析断言 |

### P2（目标获取 + AI 入口）

| 动作 | 路径 | 说明 |
| --- | --- | --- |
| 新增 | `Projectile/Component/ProjectileTargetComponent.cs` | 目标句柄 + `Revision`；只在目标变化时递增，供下游判断"意图是否过期" |
| 新增 | `Projectile/ProjectileTargetAcquisitionSystem.cs` | 首个 `ProjectileTargetFact` 生产者（修复 B6：该事实此前无人产出）；取最近的异阵营实体并排除持有者 |
| 新增 | `Projectile/System/ProjectileAiSnapshotProvider.cs` | 把位置/速度/追踪能力/目标投影成 AI 可读事实 |
| 新增 | `Projectile/ProjectileAiAdapter.cs` | 查询 → `AiEntitySystem.Calculate` → 写行为状态与本帧速度意图（修复 B5：追踪参数此前无消费者）；`AiComponent` 在查询外延迟添加 |
| 新增 | `Projectile/System/ProjectileHomingAiBehavior.cs` | `MoveTowards` 之后按原速率归一，修复 8 → 3.84 的速率坍缩 |
| 改写 | `NSSLC.Server/Program.cs` | 三条行为 profile（直线/追踪/反弹）、构造 `AiSystem`/`AiEntitySystem`/快照提供者/适配器，Preparation 阶段接线 |

### P3（子步 + 瓦片接触）

| 动作 | 路径 | 说明 |
| --- | --- | --- |
| 新增 | `Share/Physics/Movement/Component/SubstepCapabilityComponent.cs` | 子步数归共享物理层（1..1025），不归投射物域 |
| 改写 | `Share/Physics/SpatialSimulation/System/SpatialEntitySnapshot.cs` | 可选携带子步能力 |
| 改写 | `Share/Physics/SpatialSimulation/SpatialSimulationSystem.cs` | 子步循环：同段位移切 N 段分别碰撞，撞到即停；提交带接触几何与入射速度 |
| 改写 | `Share/Physics/SpatialSimulation/SpatialSimulationCoordinatorSystem.cs` | 读子步能力；瓦片接触队列存在才写（可选依赖） |
| 新增 | `Share/Physics/SpatialQuery/TileContactGeometry.cs` | 触点数/法线/瓦片索引（用两个整数，物理层不依赖 World 域） |
| 改写 | `Share/Physics/SpatialQuery/SpatialTileMotionResult.cs`、`System/SpatialCommit.cs` | 携带接触几何、入射速度、子步号 |
| 改写 | `World/System/WorldTileMotionSolver.cs` | `DescribeContact`：把"撞了"翻译成接触点/法线/瓦片索引 |
| 新增 | `Share/Physics/ContactFact/System/TileContactFact.cs`、`Component/TileContactQueueComponent.cs` | 瓦片接触事实与队列，与实体接触同形 |
| 改写 | `Projectile/System/ProjectileEntityDefinitions.cs` | 按内容 `extraUpdates + 1` 装配子步数 |

### P4（反弹 + 穿透预算）

| 动作 | 路径 | 说明 |
| --- | --- | --- |
| 改写 | `Projectile/Component/ProjectilePenetrationStateComponent.cs` | 三计数 `Remaining`/`PendingConsumed`/`Consumed` + `Unlimited` + `SuppressesDamage`；`PreviewConsume`/`ConfirmConsume`/`RollbackPending`/`ConsumeFor` |
| 改写 | `Projectile/System/ProjectilePenetrationPolicy.cs` | 增加 `SuppressesDamage`（内容侧策略） |
| 改写 | `Projectile/ProjectileHitResolutionSystem.cs` | 入队前 `TryPreviewConsume`；`Applied` → `ConfirmConsume` 并判终止；新增 `Reject` 回滚 |
| 改写 | `Projectile/ProjectileDamageRequestSystem.cs` | 预扣失败则不入队攻击意图 |
| 改写 | `Projectile/ProjectileDamageResultSystem.cs` | `Rejected` 也回滚预扣（此前只处理 `Applied`） |
| 新增 | `Projectile/ProjectileTileResolutionSystem.cs` | 消费瓦片接触：按 `ConsumeOrder` 判预算 → 反射速度 + 入位移意图，或入终止队列 |
| 新增 | `Projectile/System/ProjectileTileResolutionResult.cs` | 处理去向（Bounced / BouncedAndExhausted / Terminated）+ 反射速度 |
| 改写 | `NSSLC.Server/Content/Projectile/ProjectilePrototype.cs`、`ProjectileCodec.cs` | `penetration.suppressesDamage` 字段投影 |
| 改写 | `NSSLC.Server/Content/Prototypes/Projectile/projectile.json` | 新增 `gem-bolt`（`AfterBounce` + 不哑火），反弹顺序两种取值都有真实内容覆盖 |
| 改写 | `Share/Entity/Component/SingletonComponent.cs` | 单例组件出现两个宿主实体时直接抛错（此前静默取最后一个） |
| 改写 | `NSSLC.Server/Registration/CombatSystemRegistration.cs`、`EcsServerRegistrationKeys.cs` | 瓦片结算注册在 Simulation、紧随接触捕获 |
| 改写 | `Projectile/README.md` | 组合模型、按阶段的系统清单、单例约束 |

## 3. 设计要点

**为什么策略单独成类型而不是继续加构造参数。** 每个策略自带取值范围校验，定义只负责组合；
`Projectile` 的字段数是"内容策略参数"而不是"第二个 Projectile 聚合"，行为逻辑仍在共享 AI 行为类里。

**为什么穿透上限写进属性而不是新开字段。** `AttributeKind.ProjectilePenetrationLimit` 已是上限的
唯一来源（可被装备/Buff 修饰），定义构造时把 `PenetrationPolicy.MaximumHits` 写成属性基础值，
生成期由生命周期快照为剩余预算；不在定义里再存一份"还能穿几个"。

**为什么重力必须同时写配置。** `GravityTickSystem` 每 tick 按 `GravityConfigurationComponent`
重算 `GravityStateComponent`；只写状态会在第一个 tick 被默认值覆盖。受重力的弹体移动模式取
`Free`（`Flight` 会跳过重力积分），关闭重力则保持 `Flight`。

**为什么液体要补 `LiquidContactComponent`。** 空间协调系统只在实体已有该组件时回写液体接触；
缺它时"液体策略"是一个没人读的开关（原缺陷 B10 的一半）。

**为什么删除反弹两件套。** `ProjectileBounceCapabilityComponent` 与 `ProjectileBounceStateComponent`
全仓零读取，且"还能反弹几次"若单独计数，就会与穿透预算形成两个真相来源。反弹参数收进
`ProjectileBouncePolicyComponent`，运行时次数归穿透预算。

**为什么伤害类型与基础伤害进内容。** 参考端 `projectile.damage` 属于内容；投射物只引用攻击 id，
宿主按内容注册 `CombatAttack`，伤害计算仍留在 Combat，弹体域不复制伤害模型。

**为什么射击参数长在物品内容上、以 id 文本表达。** 参考端"用弓箭射出木箭"是物品（弹药）的属性，
不是投射物域自己的知识；`ItemUse` 属于共享物品域，不能引用非共享的 `ProjectileId` 类型，
所以只存 id 文本，解析与存在性校验留给弹体域。名字合法但定义缺失的条目在生成期被跳过，
内容错误仍由加载期诊断暴露。

**为什么射击接缝放在弹体域而不是 `Share/ItemUse`。** 计划草稿把它放在物品使用域，但那会让
共享目录依赖非共享领域（Share → Unshared 反向）。系统的输出是弹体生成请求，输入是共享事实队列
（只读），按"系统归属其输出域"的边界放在 `Projectile` 域根，与 ECS 文件组织约束一致。

**为什么射击接缝在 Preparation、`before` 生成排空方。** 生成队列按 tick 号排空：本 tick 内入队的
请求只在本 tick 的排空里有效。物品使用事实在 Simulation 产生，若接缝放 Simulation，请求会带上
`tick N` 而排空方在 `tick N+1` 的 Preparation 调 `Drain`——永远取不到，还会在下一次 `Enqueue`
时被 `BeginTick` 清掉（实现时真实发生过）。放到下一 tick 的 Preparation、且排在生成之前，
事实 → 请求 → 生成三步共享同一个 tick 号；代价是使用到弹体差一 tick，与验收口径
"使用物品后下一 tick 存在投射物实体"一致。

**为什么被拒的使用不射击。** 事实携带 `Outcome`；只有 `Applied` 才有内容语义，
`MissingAmmo` / `NotChanneling` 等只是"玩家按了一下"。

**为什么失败不抛异常也不写日志。** tick 管线里抛异常会打断整帧，而"射击配置引用了不存在的定义"属于
内容问题，应在加载或验证期暴露。系统把每条 `Applied` 事实的路由结果记进 `Results`（入队 /
缺使用定义 / 无射击配置 / 定义未知 / 持有者不可用），宿主与验证项目都能读到，因此不是静默丢弃；
运行期日志接入留给宿主，见 §5。

**为什么阵营跟持有者。** 弹体命中规则按阵营区分敌我；玩家射出的弹体继承 `Friendly`，
缺阵营组件的持有者按 `Neutral` 处理，与共享 `Faction` 默认值一致。

**为什么目标获取独立成系统。** `ProjectileTargetFact` 此前没有任何生产者（B6），追踪行为读的
是永远为空的事实。目标锁定与"用目标算出速度"是两件事：前者决定"打谁"，后者决定"怎么飞"，
合成一个系统会让"目标没变但速度要重算"和"目标变了要重建意图"两种路径纠缠。

**为什么 `ProjectileTargetComponent` 带 `Revision`。** 适配器用 `sourceRevision` 标记意图来自
哪个目标版本；没有它就无法区分"同一目标的第二次刷新"和"换了目标"，重定向会丢掉。

**为什么子步归共享物理层。** "位移如何切分"是物理问题，不是投射物专有；放在 `Projectile`
会让物理层反向依赖领域。`SubstepCapabilityComponent` 放在 `Share/Physics/Movement/Component`，
投射物只是它的第一个使用者。

**为什么撞到就停在那个子步。** 继续推进等于沿墙滑，把"撞了"这个结果抹掉；反弹与终止都依赖
"撞在第几个子步"来判断顺序。

**为什么接触几何用两个整数表示瓦片索引。** `TileCoordinate` 在 Infrastructure 层，物理层不能
引用它；用 `int TileX/TileY` 让事实保持自足，消费方自己决定要不要换算。

**为什么反弹同时写速度并入队意图。** 意图是**期望速度**，`SpatialSimulationSystem` 用
`MoveTowards(current, desired, acceleration * dt)` 逼近，而投射物加速度恒为 0——只入队意图
反射永远不生效。两项都做，逼近退化为"保持"。加速度是留给 AI 转向的，不为反弹破例调大。

**为什么预算要三计数。** 入队在 Simulation、确认在 Commit；同 tick 两次命中若只读 `Consumed`，
"还剩几次"和伤害衰减都会少算一次。`PendingConsumed` 就是这段异步窗口的账。

**为什么拒绝也要回滚。** 被免疫/阵营/目标已死拒绝的命中没有落地，不回滚就成了"打空也扣穿透"，
与参考端 `:12635` 的 early return 相悖。

**为什么"打空"和"哑火"分开判据。** `CanConsume`（能否再穿/反弹）与 `CanDealDamage`
（能否继续造成伤害）是两个问题；`SuppressesDamage` 为假时弹体穿完继续飞、继续打。

**为什么单例组件出现两个宿主就直接抛错。** 生产者写 A、消费者读 B 时现象是"队列永远为空"，
排查成本远高于一条明确异常。本轮为此花了三轮才定位。

**为什么内容里要有 `AfterBounce` 的真实条目。** 两种消费顺序在原版是两套写法（`:19020` 与
`:18108`）；只实现一种、另一种只存在于枚举里，等于把分支写成死代码。`gem-bolt` 让它有真实覆盖。

## 4. 验证

| 命令 | 退出码 | 警告 | 错误 | 产物 |
| --- | --- | --- | --- | --- |
| `dotnet build src/NSSLC/Terraria.Components.csproj --nologo -p:BaseOutputPath=Build/bin/ -p:BaseIntermediateOutputPath=Build/obj/` | 0 | 0 | 0 | `src/NSSLC/Build/bin/Debug/net10.0/Terraria.Components.dll` |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj …`（同上参数） | 0 | 28（全部位于 `NSSLC.Network`、`NSSLC.Infrastructure/WorldGeneration` 的既有文件） | 0 | `src/NSSLC.Server/Build/bin/Debug/net10.0/NSSLC.Server.dll` |
| `dotnet build Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj …` | 0 | 0 | 0 | `Test/…/Terraria.Projectile/Build/bin/Debug/net10.0/` |
| `dotnet Test/…/Terraria.Projectile/Build/bin/Debug/net10.0/Terraria.Projectile.dll` | 0 | — | — | **84 条 PASS / 0 FAIL**，末行 `PASS: projectile spawn produces a spatial-simulatable entity.` |
| `dotnet build Test/Terraria.Components/Terraria.ItemUse/Terraria.ItemUse.csproj …` | 0 | 0 | 0 | `Test/…/Terraria.ItemUse/Build/bin/Debug/net10.0/` |
| `dotnet Test/…/Terraria.ItemUse/…/Terraria.ItemUse.dll` | 0 | — | — | **15 条 PASS / 0 FAIL** |
| `dotnet build Test/NSSLC.Server/Terraria.Server.Lvcs/Terraria.Server.Lvcs.csproj …` | 0 | 0 | 0 | `Test/…/Terraria.Server.Lvcs/Build/bin/Debug/net10.0/` |
| `dotnet Test/…/Terraria.Server.Lvcs/…/Terraria.Server.Lvcs.dll` | 0 | — | — | **72 条 PASS / 0 FAIL**，末行 `RESULT: PASS` |
| 真实服务器启动（`NSSLC_WORLD_FILE=src/World/科研.wld`，12 s 后终止） | — | — | stderr 为空 | 日志出现 `ecs.server.ready listen=127.0.0.1:7777 world=8400x2400`，无 LVCS 诊断 |

### 4.1 P3–P4 追加验证（2026-11-24）

| 命令 | 退出码 | 警告 | 错误 | 产物 |
| --- | --- | --- | --- | --- |
| `dotnet build src/NSSLC/Terraria.Components.csproj …` | 0 | 0 | 0 | `Build/bin/Debug/net10.0/Terraria.Components.dll` |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj …` | 0 | 28（全部位于 `NSSLC.Network` / `NSSLC.Infrastructure/WorldGeneration` 的既有文件，本次改动零命中） | 0 | `Build/bin/Debug/net10.0/NSSLC.Server.dll` |
| `dotnet Test/…/Terraria.Projectile/…/Terraria.Projectile.dll` | 0 | — | — | **84 条 PASS / 0 FAIL** |
| `dotnet Test/…/Terraria.ItemUse/…/Terraria.ItemUse.dll` | 0 | — | — | **15 条 PASS / 0 FAIL** |
| `dotnet Test/…/Terraria.Npc.Ai/…/Terraria.Npc.Ai.dll` | 0 | — | — | **1 条 PASS / 0 FAIL** |
| `dotnet Test/…/Terraria.WorldItem/…/Terraria.WorldItem.dll` | 0 | — | — | **60 条 PASS / 0 FAIL**（子步循环的顺带回归） |
| `dotnet Test/…/Terraria.Server.Lvcs/…/Terraria.Server.Lvcs.dll` | 0 | — | — | **72 条 PASS / 0 FAIL** |
| 真实服务器启动（科研.wld，12 s） | — | — | stderr 0 字节 | `ecs.server.ready listen=127.0.0.1:7777 world=8400x2400 spawn=4195,541` |

P3 新增/改写断言：`substep-capability-follows-content`、`substep-emits-one-move-per-substep`、`substep-partitions-delta-time`、`tile-contact-recorded`（法线 `<-1,-0>`、接触点落在墙面）、`tile-contact-carries-substep-and-incoming`（`substep=1`、入射 `+40`）。

P4 新增断言：`penetration-limit-snapshots-on-spawn`、`penetration-unlimited-stays-unlimited`、`penetration-preview-does-not-spend`、`penetration-confirm-spends`、`penetration-reject-rolls-back`、`tile-reflection-reverses-normal-component`、`exhausted-budget-blocks-bounce`、`unlimited-budget-never-blocks`、`bounce-spends-shared-budget`、`zero-cost-bounce-keeps-budget`、`bounce-reverses-velocity-end-to-end`（`+40 → -20`）、`bounce-consumes-shared-budget-end-to-end`、`bounce-decrements-remaining-bounces`。

新增断言覆盖：P0 的策略按定义装配（重力 0.25/8、液体接触组件存在、追踪/反弹参数、无策略不挂组件、
`Seal()` 后拒绝再注册、未知定义拒绝生成）；P1 的 `item-use-facts-drained`、
`item-use-enqueued-one-spawn`（不带 `Shoot` 的物品与被拒的使用都不产出）、内容速度/偏移/阵营/来源、
跨 tick 端到端 `item-use-spawn-reaches-lifecycle` 与 `item-use-shot-spawned-at-owner-offset`、
`item-use-missing-definition-recorded`（定义未知进入结果而不是消失）；
LVCS 的 `shoot-projected`、`shoot-absent-when-not-declared`、`shoot-zero-velocity-rejected`、
`dispatch-real-content-carries-shoot`。

## 5. 未闭合

> 2026-11-24 更新：P2、P3、P4 已落地并验证，移除对应条目；新增项见 §5.1。
> §5.1 第 1–4 项均按缺陷形状单独成文，正文见 [缺陷记录目录](../defects/)；CR 只留现象与闭合口径。

- 运行期日志缺口：射击接缝的路由失败目前只在 `Results` 上可观测，宿主尚未把它接进运行期日志；
  接日志需要在宿主侧增加每 tick 的检查点，属宿主职责。
- 客户端输入缺口：`PlayerItemUseRequest` 没有生产者——请求队列只有消费方（物品使用系统），
  Communication 层尚未把客户端的物品使用输入翻译成该请求；因此"真实客户端使用物品 → 生成投射物"
  除本 CR 的射击接缝外，还缺一段协议 → 请求的输入适配，不在本计划范围内，需单独立项。
- 瞄准方向未建模：协议里没有可用的朝向状态（包 13 只有位置与速度，包 41 只进旧网络层），
  射击速度与偏移由内容声明，方向暂为固定值；待协议补齐朝向后由输入驱动。
- P0/P1 的端到端验收"真实客户端使用物品 → 弹体实体"在上述输入适配落地前无法闭环。

> 2026-11-25：下表第 1–4 项与 §5 的输入缺口已排期，见
> [Projectile 链路修复执行计划](../plans/2026-11-25-projectile-chain-repair-execution-plan.md)（P0–P5 六批）。

## 5.1 2026-11-24 新增未闭合

| # | 项 | 现象 | 闭合条件 |
| --- | --- | --- | --- |
| 1 | 实体接触子步号恒 0 | `PhysicsContactCaptureSystem` 在位移完成后整体捕获一次，不在子步循环内；瓦片接触已带真实子步号 | 把捕获搬进子步循环，或由空间系统回报每个子步的接触 → [DEF-2026-11-24-projectile-entity-contact-substep-always-zero](../defects/2026-11-24-projectile-entity-contact-substep-always-zero.md) |
| 2 | 追踪转向速率单调衰减（实测 8 → 7.000） | AI 侧把模长归一化到**当前**速度（不是出厂速度），随后空间侧再逼近一次弦，每帧净缩一步；断言按实测放成 `6.5 < speed <= 8.05`，把缺陷写成了容差 | 断言回到 `|speed - 8| < 0.05` 并先红后绿；再收敛到单一权威速度写入点 → [DEF-2026-11-24-projectile-homing-speed-drift](../defects/2026-11-24-projectile-homing-speed-drift.md) |
| 3 | 双路径同帧耗尽时终止唯一性靠隐式守卫维持 | 瓦片路径显式查 `Contains`，命中路径在 `CheckAdmission` 里顺带查；`Enqueue` 不去重、原因优先级未集中定义，唯一性无测试固定 | 增加同时命中 + 反弹并打空预算的用例 → [DEF-2026-11-24-projectile-termination-uniqueness-implicit](../defects/2026-11-24-projectile-termination-uniqueness-implicit.md) |
| 4 | AI 行为工厂按类型注册，profile 数值不生效 | `AiSystem` 工厂键是 `Type`，`CreateSystem` 只闭包一个 homing profile；适配器只取 `BehaviorType`，`Bind` 无调用点；靠数值全 0 而未触发 | 要么按 `(Type, ProfileId)` 建厂，要么删掉 profile 数值与 `ProfileId` 让能力组件成唯一来源 → [DEF-2026-11-24-projectile-homing-profile-values-not-applied](../defects/2026-11-24-projectile-homing-profile-values-not-applied.md) |
| 5 | P5 为「按需」能力菜单，本期未开工 | 逐项状态见计划 §9.1 | 由真实内容需求驱动，逐项立项 |

## 6. 回滚

按文件逐条还原：删除本轮新增的 `Projectile/System/Projectile*Policy*.cs`、
`ProjectileBounceConsumeOrder.cs`、`ProjectileBouncePolicyComponent.cs`、
`Projectile/ProjectileItemUseSpawnSystem.cs`、`Share/ItemUse/Content/ItemShootProfile.cs`、
`Content/Projectile/**`、`Content/Prototypes/Projectile/**`；
`git checkout -- src/NSSLC/Projectile/System/Projectile.cs src/NSSLC/Projectile/System/ProjectileEntityDefinitions.cs
src/NSSLC/Projectile/System/ProjectileCatalog.cs src/NSSLC/Share/ItemUse/Content/ItemUse.cs
src/NSSLC.Server/Content/ItemUse/ItemUsePrototype.cs src/NSSLC.Server/Content/ItemUse/ItemUseCodec.cs
src/NSSLC.Server/Content/Prototypes/ItemUse/item-use.json src/NSSLC.Server/Registration/EcsServerRegistrationKeys.cs
src/NSSLC.Server/Program.cs src/NSSLC/Projectile/README.md`，并恢复已删除的两个 bounce 组件文件。
回滚需同时回退三个验证项目，否则编译面缺类型。
