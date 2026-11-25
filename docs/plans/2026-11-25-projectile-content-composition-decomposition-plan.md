# Projectile 内容组合分解执行计划（去融合定义 + 无中心装配）

| 项 | 值 |
| --- | --- |
| 文档 ID | `PLAN-2026-11-25-PROJECTILE-CONTENT-COMPOSITION` |
| 状态 | `complete`（P0–P5 全部交付：快照、骨架与影子装配、单一入口、死值与派生收敛、不变量与错误面、读取器收口。验证：`Terraria.Projectile` **91 PASS / 0 FAIL**、`Terraria.Server.Lvcs` PASS；五条判据全过。测试口径按用户指令收窄为「只写 10% 测试，正式代码不缩水」，见 §7.2） |
| 上游依据 | [Projectile 链路修复计划](2026-11-25-projectile-chain-repair-execution-plan.md)（P5 为前置）；用户决策：保留现网 `projectile.json` 的 LVCS 契约，同时把 `ProjectileEntityDefinitions` 的装配链路换成「内容 → 组件」的无中心装配 |
| 适用约束 | [ECS 文件组织](../../Context/架构约束/ECS文件组织设计约束.md)、[组件命名](../../Context/架构约束/组件命名设计约束.md)、[ECS Entity 组织](../../docs/architecture/ECSEntity组织设计约束.md)、[ECS 系统输入边界](../../Context/项目约束/ECS系统输入边界约束.md)、[错误处理](../../Context/项目约束/错误处理约束.md)、[构建与验证](../../Context/项目约束/构建与验证约束.md)、[项目文件组织](../../Context/架构约束/项目文件组织设计约束.md) |
| 范围 | `src/NSSLC/Projectile/**` 的内容定义与装配路径、`src/NSSLC.Server/Content/Projectile/**` 的内容装载、以及直接读定义的三处消费者 |
| 排除范围 | `projectile.json` 与 `ProjectilePrototype` 的 JSON 形状；运行期系统（寿命、命中、瓦片、终止）；AI 行为与 Profile；物理层 `SpatialCommit`；`src/NSSLC.Infrastructure/**`；协议与存档 |

## 1. 目标与判据

**唯一目标**：把「一份投射物内容」从**一个持有运行时组件实例的聚合**改成**一组按内容族切分、各自声明自己组件贡献的 section**，装配退化为对 section 列表的顺序无关折叠。

五条判据，缺一条不算完成：

| # | 判据 | 判定方式 |
| --- | --- | --- |
| 1 | **无中央翻译表** | `ProjectileEntityDefinitions.Build` 不存在；`ProjectileRecipe.CreateComposition()` 体内不出现任何内容族名字 |
| 2 | **内容不持有运行时组件实例** | 任何 section 类型上不出现组件类型的字段（碰撞体与尺寸以标量 `Width`/`Height`/`PixelWidth`/`PixelHeight` 形式存在）；组件只在 `Contribute` 或消费者处构造 |
| 3 | **装配结果逐组件不变** | P0 记录的 4 条内容组件清单快照，改造后逐条相等（类型集合 + 关键数值） |
| 4 | **单点派生与死值清除** | `collider → size` 保持单一推导（`ceil` 只在 section 内）；穿透上限只剩 `ProjectilePenetrationStateComponent` 一处来源，属性侧写入消失；`ProjectileKinematicsStateComponent` 不再存在 |
| 5 | **新增一族零中央改动** | P5 之后新增内容族只需「1 个 section + 1 个 server 读取器 + JSON」，`ProjectileCodec` 与 `ProjectileRecipe` 均不改（用测试证明） |

> 判据 1、5 是这次改造的全部意义：如果只是把 124 行定义拆成若干文件，而新增一族仍要改三处，那属于机械拆文件，不算完成。

## 2. 现状证据

### 2.1 定义链路结构事实

| 事实 | 位置 | 后果 |
| --- | --- | --- |
| 内容定义 `Projectile` 124 行、17 个构造参数，其中 8 个是**运行时组件实例**（行为选择、运动学、寿命、属性集、碰撞策略、碰撞能力、碰撞体、尺寸） | `src/NSSLC/Projectile/System/Projectile.cs:20-124` | 内容与运行期状态共用类型，读者无法判断「这是默认值还是当前值」 |
| 定义构造函数**改写属性集**，把穿透上限写入 `AttributeKind.ProjectilePenetrationLimit` 基础值 | `Projectile.cs:59-63` | 穿透上限有两个来源（策略对象 + 属性），且写入发生在值对象构造里 |
| 装配是一个 120 行静态函数，按内容族逐个条件分支，含跨族耦合与硬编码常量 | `ProjectileEntityDefinitions.cs:31-119` | 新增一族必须改这个函数；跨族读数（homing 转向率 → 移动加速度）靠注释维系 |
| 装配函数用 10 行注释列出「空间协调系统的静默前置条件」，缺任一项弹体不动 | `ProjectileEntityDefinitions.cs:52-55` | 不变量只存在于注释，编译器与测试都不检查 |
| 编目 `Register` 内部直接调用装配函数，内容与实体组合「成对登记」 | `ProjectileCatalog.cs:43-45` | 装配入口被编目绑定，无法替换也无法单测 |
| 内容装载处一次性构造 17 参数对象字面量（约 40 行），另有 170 行逐字段校验 | `ProjectileCodec.cs:136-177`、`:202-373` | 校验与装配分居两处，新增一族要同时改两处 + 定义构造函数 |
| 定义同时是三种东西：实体模板（7 个组件进 `EntityDefinition`）、生成请求参数（`Collider`/`Size` 只被 `ProjectileItemUseSpawnSystem:117-118` 读）、启动期伤害来源（`AttackDefinitionId`/`DamageType`/`BaseDamage` 只被 `Program.cs:1032-1036` 读） | `ProjectileEntityDefinitions.cs:34-93`（无 `Collider`/`Size`）、`ProjectileItemUseSpawnSystem.cs:117-118`、`Program.cs:1032-1036` | 同一类型服务三种生命周期不同的读者，新增任一角色都在同一个 17 参数构造函数上排队 |
| `ProjectileKinematicsStateComponent` 在实体上**没有运行期读者**（唯一引用在待删的 `ProjectileSimulationSystem` 家族） | 全仓 grep：仅 `ProjectileSimulationSystem.cs`、`ProjectileSimulationResult.cs`、`Projectile.cs` | 组件把「内容默认值」和「子步游标」缝在一个类型里，而游标无人读 |
| 穿透上限属性的唯一读者是**两条测试断言**，运行期不读 | `Test/Terraria.Components/Terraria.Projectile/Program.cs:634`、`Test/NSSLC.Server/Terraria.Server.Lvcs/Program.cs:465`；运行期只读 `ProjectileHitImmunityTicks`（`ProjectileHitResolutionSystem.cs:166-176`） | 一个「内容写、测试读、运行期不用」的自证循环 |

### 2.2 六处融合

| 编号 | 融合 | 证据 | 拆除方式 |
| --- | --- | --- | --- |
| F1 类型融合 | 内容默认值用运行时组件类型承载（运动学、寿命、属性集、碰撞策略、碰撞能力、碰撞体、尺寸） | `Projectile.cs:20-37` | section 只持有标量与值对象，组件在 `Contribute` 内构造 |
| F2 装配中心 | 一个静态函数知道每一族的翻译规则 | `ProjectileEntityDefinitions.cs:31-119` | 每族一个 section，装配是折叠 |
| F3 双角色 | 定义既是实体模板、又是生成参数表、又是启动期伤害来源 | 见 §2.1 第 7 行 | 同一个 section 承接三种读者，但每个值只有一个读者、一个来源 |
| F4 死值 | 穿透上限属性基础值、`ProjectileKinematicsStateComponent` 的子步游标 | 见 §2.1 第 9、10 行 | 删除写入与组件类型，上限只留预算组件 |
| F5 构造点 | 40 行对象字面量 + 170 行校验 + 120 行装配，三处都要改 | `ProjectileCodec.cs:136-177`、`ProjectileEntityDefinitions.cs:31-119` | 校验随 section 走，codec 只做「按族调用读取器」 |
| F6 隐式耦合 | homing 转向率被塞进 `MovementCapability.acceleration`；液体系必须额外挂 contact 组件 | `ProjectileEntityDefinitions.cs:61-69`、`:95-100` | 跨族读数收进具名参数块（两遍收集），前置条件由具名函数 + 完整性校验承担 |

### 2.3 成员归属

`Projectile` 的每个成员按「谁读、谁写、生命周期」定归属。`conf` 列：`confirmed` 已有代码证据，`partial` 需在对应批次复核。

| 成员 | 现值 | 写者 | 运行期读者 | 生命周期 | 归属 | conf |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | JSON `id` | codec | 编目键、生成请求 | 永久 | `ProjectileRecipe.Id` | confirmed |
| `Behavior` | `behaviorId`/`profileId` | codec | `ProjectileAiAdapter` 绑定 | 生成期 | BasicsSection | confirmed |
| `Kinematics.ExtraUpdates` | JSON | codec | `SubstepCapabilityComponent` | 生成期 | BasicsSection（标量） | confirmed |
| `Kinematics.Substep` | 0 | 无 | **无** | 死值 | 删除 | confirmed |
| `Lifetime.RemainingTicks` | `lifetimeTicks` | codec | `ProjectileLifetimeTickSystem:34` | 生成期 + 运行期递减 | BasicsSection（标量）+ 生成时构造组件 | confirmed |
| `Attributes` | 空集 + 穿透上限注入 | codec + 定义构造 | `HitResolution:166` 只读免疫 tick | 生成期 clone | BasicsSection（每次生成新建） | confirmed |
| `CollisionPolicy` | `tileCollide`/`ignoreOwner` | codec | `TileCollisionCapabilityComponent`、`ProjectileCollisionPolicyComponent` | 生成期 | BasicsSection | confirmed |
| `Collision` 能力 | 硬编码 `Layer=1`、`Mask=全 1` | codec | 物理宽相位 | 生成期 | BasicsSection | confirmed |
| `Collider` | JSON | codec | 生成请求 | 生成期 | BasicsSection（存标量宽高，派生取整尺寸） | confirmed |
| `Size` | `ceil(collider)` | codec 派生 | 生成请求 | 生成期 | BasicsSection（`PixelWidth/PixelHeight` 单点派生） | confirmed |
| `AttackDefinitionId` | JSON | codec | `Program.cs:1032-1036` | 永久 | BasicsSection | confirmed |
| `DamageType`/`BaseDamage` | JSON | codec | 同上 | 永久 | BasicsSection | confirmed |
| `Gravity` | JSON 可缺省 | codec | 装配 3 个重力组件 + 移动模式 | 生成期 | GravitySection | confirmed |
| `Liquid` | JSON 可缺省 | codec | 装配能力 + 条件 contact | 生成期 | LiquidSection | confirmed |
| `Penetration` | JSON 必填 | codec | `FromLimit` 快照、`SuppressesDamage` | 生成期 | PenetrationSection | confirmed |
| `Bounce` | JSON 可缺省 | codec | `ProjectileBouncePolicyComponent` | 生成期 | BounceSection | confirmed |
| `Homing` | JSON 可缺省 | codec | 能力组件 + 转向加速度 | 生成期 | HomingSection（唯一跨族读数） | confirmed |

### 2.4 消费者清单（改动半径）

| 消费者 | 读什么 | 位置 |
| --- | --- | --- |
| 生成链路 | `GetEntityDefinition(id)`、`TryGet(id)` | `ProjectileLifecycleSystem.cs:64,90,110` |
| 物品使用射击 | `definition.Collider`、`definition.Size` | `ProjectileItemUseSpawnSystem.cs:117-118` |
| 攻击定义构建 | `AttackDefinitionId`、`BaseDamage`、`DamageType` | `Program.cs:1032-1036` |
| 内容装载 | 构造定义 + 逐字段校验 | `ProjectileCodec.cs:136-177`、`:202-373` |
| 验证项目 | 8 处 `new Projectile(...)`、1 处穿透属性断言 | `Terraria.Projectile/Program.cs:42,303,361,510,543,678,841,860,634` |
| 服务端 LVCS 验证 | 1 处穿透属性断言 | `Terraria.Server.Lvcs/Program.cs:465` |
| 文档 | 组合模型表 | `Projectile/README.md:26-42` |

## 3. 目标结构

### 3.1 目录与文件

按 [ECS 文件组织](../../Context/架构约束/ECS文件组织设计约束.md) §3.4，定义与值对象归 `System/`，不新建 `Content/`、`Model/` 等收容目录：

```text
Projectile/System/
  ProjectileRecipe.cs                 # 配方：id + section 列表 + 组合结果
  IProjectileSection.cs               # 协议：Collect / Contribute 两遍
  ProjectileSectionContext.cs         # 只读上下文：内容 id
  ProjectileCompositionParameters.cs  # 跨族参数块（唯一可写面）
  ProjectileComposition.cs            # 折叠 + 前置能力组 + 完整性校验
  ProjectileBasicsSection.cs          # 平铺字段族
  ProjectileGravitySection.cs
  ProjectileLiquidSection.cs
  ProjectilePenetrationSection.cs
  ProjectileBounceSection.cs
  ProjectileHomingSection.cs
```

删除：`System/Projectile.cs`、`System/ProjectileEntityDefinitions.cs`、5 个策略值对象（`ProjectileGravityPolicy`、`ProjectileLiquidPolicy`、`ProjectilePenetrationPolicy`、`ProjectileBouncePolicy`、`ProjectileHomingPolicy`）、`Component/ProjectileKinematicsStateComponent.cs`。

**为什么是 6 个 section 而不是 17 个 part**：切分依据是「同一段 JSON、同一个内容作者、同一次变更原因」，不是字段数。12 个以上碎片会让 `Collect`/`Contribute` 的调用序列变长、跨族参数块成为真正的耦合中心，属于过原子化。被否的 17-part 方案见 §11。

### 3.2 section 协议（两遍，消除顺序依赖）

```text
IProjectileSection
  Collect(in ProjectileSectionContext context, ProjectileCompositionParameters parameters)
  Contribute(in ProjectileSectionContext context, in ProjectileCompositionParameters parameters, EntityBuilder builder)
```

两遍的理由是 F6 的直接结果：`MovementCapability.acceleration` 的值来自 homing 族。若单遍，装配结果就取决于 section 在列表里的先后——那等于把隐式顺序重新引入，正好是这次要消除的东西。

| 遍 | 允许做 | 禁止做 |
| --- | --- | --- |
| `Collect` | 写自己那族的参数键（转向率、移动模式、子步数、瓦片碰撞开关、液体开关与标志） | 读取他族参数、构造组件、访问世界 |
| `Contribute` | 读参数块、构造并加入组件 | 改参数块（签名是 `in`） |

`ProjectileCompositionParameters` 在折叠结束后 `Seal()`：未写入的键保持默认值，重复写入抛异常（同一个键被两个 section 声明 = 内容自相矛盾）。

### 3.3 装配流

**改造前**：

```text
projectile.json → ProjectilePrototype → ProjectileCodec.Project（17 参数对象字面量，40 行）
                                     → ProjectileCatalog.Register
                                         → ProjectileEntityDefinitions.Build（120 行中央翻译）
                                             → EntityRegistry
```

**改造后**：

```text
projectile.json → ProjectilePrototype → ProjectileCodec.Project
                                     → 每族一个读取器：prototype 片段 → section
                                     → new ProjectileRecipe(id, sections)
                                         → ProjectileComposition.Create（折叠 + 前置能力组 + 校验）
                                             → EntityRegistry
```

关键差别：**折叠函数不含任何族名**；族的一切知识与该族 section 同文件。

### 3.4 不变量从注释变检查

`ProjectileComposition.Create` 末尾 `EnsureSpatialPrerequisites` 校验目标实体组合含以下能力，缺项抛出带内容 id 与缺失类型名的异常，由 codec 转成 LVCS 诊断：

| 必需项 | 缺了会怎样 | 现状 |
| --- | --- | --- |
| `GroundingComponent` | 空间协调查询不匹配，弹体不动 | 装配函数硬编码 |
| `MovementCapabilityComponent` | 同上 | 同上 |
| `GravityCapabilityComponent` + `GravityConfigurationComponent` + `GravityStateComponent` | 重力族静默失效 | 同上 |
| `TileCollisionCapabilityComponent` | 瓦片碰撞族静默失效 | 同上 |
| `LiquidInteractionCapabilityComponent` + `LiquidContactComponent`（液体启用时） | 液体接触事实不回写 | 同上 |
| `SubstepCapabilityComponent` | 高速弹穿墙 | 同上 |
| `MovementIntent` | 空间系统跳过 | 同上 |

前置能力组由一个具名函数 `ProjectileComposition.AddSpatialPrerequisites` 统一添加，不由任何内容族 section 顺带完成——否则删掉某个 section 会静默拆掉运动能力。
## 4. 决策

### D-1 按内容族切 6 个 section，不按字段切

**结论**：section 边界 = 「同一段 JSON 嵌套对象或同一组平铺字段 + 同一批读者 + 同一次变更原因」。`Basics`（平铺字段）、`Gravity`、`Liquid`、`Penetration`、`Bounce`、`Homing`，共 6 个。

**理由**：JSON 已经是这个形状（`gravity` / `liquid` / `penetration` / `bounce` / `homing` 五个嵌套对象 + 一组平铺字段），内容作者改一族就改一段，读者也按族区分。按字段切会得到 12 个以上碎片，`Collect` 序列变长、参数块变成新耦合中心。

**被否方案**：

| 方案 | 否因 |
| --- | --- |
| 17 个字段 → 12+ 个 part 文件 | 过原子化：多数 part 只有 1 个组件贡献，读者与变更原因相同，拆开只增加跳转成本 |
| 保留 `Projectile` 聚合，只删掉几个死字段 | 判据 1、5 都不成立：中央装配表还在，新增一族仍改三处 |
| 把 section 做成 ECS 组件挂在实体上 | 内容是启动期只读数据，不是实体状态；挂组件会让内容与实例再次混同 |

### D-2 两遍收集（Collect / Contribute）加参数块

**结论**：`Collect` 只写本族参数键，`Contribute` 只读参数块并构造组件；参数块折叠后 `Seal()`，重复写键抛异常。

**理由**：`MovementCapability.acceleration` 的值来自 homing 族的转向率，这是真实存在的跨族读数。单遍折叠会让结果取决于 section 顺序，等于把隐式顺序重新引入。两遍把「声明数值」与「构造组件」分开，顺序无关；键重复写即报错，避免两个族抢同一个键。

**被否方案**：

| 方案 | 否因 |
| --- | --- |
| 单遍 + 在文档里约定 section 顺序 | 顺序约束没有机器检查；插入新族时静默改变装配结果 |
| section 之间互相 `TryGetSection<T>()` 读值 | 直接依赖具体 section 类型，等于把中央表换成网状依赖 |
| 保留 `definition.Homing?.TurnRate ?? 0f` 的内联读法 | 就是 F6 本身：装配函数必须知道 homing 族 |

### D-3 section 只持标量与值对象，组件在 `Contribute` 内构造

**结论**：内容侧类型上不出现运行时组件类型字段。`AttributeSetComponent` 这类可变类型每次生成新建，不共享实例。

**理由**：F1 的根因是内容默认值用运行时组件类型承载，导致读者分不清默认值和当前值。分开之后，`ProjectileLifetimeStateComponent(RemainingTicks = 600)` 只可能出现在实体上，内容里只有 `int lifetimeTicks`。

### D-4 JSON 契约与 `ProjectilePrototype` 不动，读取器放服务端

**结论**：`projectile.json` 与 `ProjectilePrototype` 一字不改；新增 `NSSLC.Server/Content/Projectile` 侧的 `IProjectileSectionReader`（一族一个），codec 只做「遍历读取器」。

**理由**：LVCS 契约是现网资产，改形状会牵动内容作者与 `UnmappedMemberHandling.Disallow` 的未知字段拒绝行为。读取器放服务端还满足依赖方向：领域侧 section 不认识 JSON，也不认识 `ProjectilePrototype`。

**被否方案**：

| 方案 | 否因 |
| --- | --- |
| 把 JSON 拆成每族一个文件 | 改动现网内容资产与 LVCS 家族定义，超出「去融合定义」的目标 |
| 让 section 自己读 JSON | 领域层反向依赖服务端内容类型，破坏 [ECS 系统输入边界](../../Context/项目约束/ECS系统输入边界约束.md) |

### D-5 穿透上限只留预算组件，删除属性写入

**结论**：删掉 `Projectile` 构造里写 `AttributeKind.ProjectilePenetrationLimit` 基础值的三行；上限唯一来源是 `ProjectilePenetrationSection.MaximumHits`，生成时由 `FromLimit` 快照成 `Remaining`。`AttributeKind.ProjectilePenetrationLimit` 枚举值**保留**（属性域的事，另案处理）。

**理由**：该属性运行期无人读（`ProjectileHitResolutionSystem` 只读 `ProjectileHitImmunityTicks`），唯一读者是两条断言自己。保留写入等于保留第二真源与一条「内容写、测试读」的自证循环。

**代价**：`Test/Terraria.Components/Terraria.Projectile/Program.cs:634` 与 `Test/NSSLC.Server/Terraria.Server.Lvcs/Program.cs:465` 两条断言要改成断言 section 值/预算快照。

### D-6 删除 `ProjectileKinematicsStateComponent`

**结论**：删除该组件类型；`extraUpdates` 作为 `BasicsSection` 的标量，唯一消费者是 `SubstepCapabilityComponent(extraUpdates + 1)`。

**前置**：修复计划 P5-2 已经删掉 `ProjectileSimulationSystem` / `ProjectileSimulationResult`，否则该组件仍有引用。

**理由**：它在实体上没有任何运行期读者（子步推进由 `SubstepCapabilityComponent` 与空间系统承担），只把「内容默认值」和「子步游标」缝在同一个类型里，正是 F4。

**被否方案**：只删 `Substep` 字段保留组件 —— 剩下一个只装 `extraUpdates`、而该值已经被 `SubstepCapabilityComponent` 持有，等于第二真源。

### D-7 前置能力组由具名函数添加，并做完整性校验

**结论**：`ProjectileComposition.AddSpatialPrerequisites` 统一添加运动必需能力；`Create` 末尾 `EnsureSpatialPrerequisites` 校验，缺项抛异常（带内容 id 与缺失类型名），codec 转成 LVCS 诊断。

**理由**：现在这组前置条件只写在 10 行注释里，缺一项的后果是「弹体不动」——最难查的一类。校验把它变成启动期错误。

**被否方案**：让 `BasicsSection` 顺带加这组能力 —— 删掉或替换 Basics 时运动能力会静默消失，且违背「每个 section 只声明自己那族」。

## 5. 批次

### 5.0 先决条件

| # | 条件 | 检查方式 |
| --- | --- | --- |
| 1 | [修复计划](2026-11-25-projectile-chain-repair-execution-plan.md) P0–P5 已交付（尤其 P5-2 删除 `ProjectileSimulationSystem` 家族） | `grep ProjectileSimulationSystem src` 零命中 |
| 2 | 修复计划 P1 若已落地，`AddSpatialPrerequisites` 里的 `MovementIntent.Stop(tick: 0)` 要带上新的模长权威字段 | 编译期 |
| 3 | 基线：`Terraria.Components` 与 `NSSLC.Server` 构建 0 警告 0 错误；`Terraria.Projectile` 全绿 | 见 §7 |

未满足条件 1 时，本计划只能执行到 P1（骨架与影子装配），P2 起的删除动作必须等。

### P0 基线快照（只改验证项目）

| ID | 动作 | 落点 |
| --- | --- | --- |
| P0-1 | 新增组合快照断言：每种内容族组合各生成一个实体，用固定清单逐个 `world.Has<T>()` 断言组件存在**与缺失**，并断言关键数值（子步数、转向加速度、移动模式） | `Test/Terraria.Components/Terraria.Projectile/Program.cs` |
| P0-2 | 确认 `ProjectileKinematicsStateComponent` 在实体上零运行期读者（结论已记入 §2.1） | 本文件 |
| P0-3 | 确认 §5.0 三条先决条件并记录进 §7 | 本文件 |

**为什么用固定清单而不是反射枚举**：Arch 的实体不保证可枚举组件类型，且 `ProjectileCatalog.GetEntityDefinition` 是 `internal`（全仓无 `InternalsVisibleTo`）。固定清单同时是一份可读的行为契约。

**执行期修正（2026-11-25）**：原定「对 4 条真实内容各生成一个实体」在本验证项目里做不到——`Test/Terraria.Components/Terraria.Projectile` 不引用 `NSSLC.Server`，拿不到 `projectile.json`；而 `Terraria.Server.Lvcs` 没有 ECS 世界，无法生成实体。因此改为：

| 层 | 断言对象 | 位置 |
| --- | --- | --- |
| 组合（本计划要冻结的东西） | 三个形状 `straight` / `homing` / `bounce`，分别镜像 `wooden-arrow` / `homing-spark` / `bouncer-orb`（第四个 `gem-bolt` 与 `bouncer-orb` 同族，差别只有 `extraUpdates`，已由子步数断言覆盖） | `Terraria.Projectile` |
| 内容值（不受本计划影响） | 4 条真实条目的字段值 | `Terraria.Server.Lvcs:446-500`（已有） |

**实施记录（P0，已完成）**：

| 项 | 结果 |
| --- | --- |
| 新增断言 | 54 条（3 个形状 × 18 条） |
| 构建 | `dotnet build Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj` → 退出码 0，0 警告 0 错误 |
| 运行 | `dotnet run --project ... --no-build --no-restore` → **140 PASS / 0 FAIL**，退出码 0（改造前基线 86 PASS） |
| 快照命中的真实耦合 | `homing` 形状的 `MovementCapability.acceleration = 6`（来自 `turnRate`），正是 §2.2 F6 说的跨族读数；改造后必须仍为 6 |
| 有意保留的基线项 | `snapshot-*-kinematics-baseline` 断言 `ProjectileKinematicsStateComponent` 仍在，作为 P3 删除时的显式同步点 |
| 首次构建报错 | `SubstepCapabilityComponent` 无 `SubstepCount`，实际成员是 `Count`（2 处，已修） |
| 前置条件 1 | **未满足**：`grep ProjectileSimulationSystem src` 仍有命中 → P2 的删除动作保持阻塞 |
| 前置条件 2 | 修复计划 P1 未落地，`MovementIntent.Stop` 一处待其落地后再带上模长权威 |
| 前置条件 3 | 基线构建 0/0、`Terraria.Projectile` 全绿，满足 |
| 覆盖面补齐（同日） | 原 8 条/形状只覆盖 7 项装配贡献，漏掉寿命、穿透预算、碰撞策略、瓦片碰撞能力、液体能力、重力配置、攻击引用、行为选择；补齐为 18 条/形状 = 23 项固定清单聚合断言 + 族与数值断言，`movement-limits`/`gravity-disabled` 等按内容值断言 |
| 复核（同日，执行轮次 1） | `dotnet build Test/Terraria.Components/Terraria.Projectile/Terraria.Projectile.csproj` → 退出码 0，0 警告 0 错误，产物 `Build/bin/Terraria.Projectile/Debug/net10.0/`；`dotnet run --project … --no-build --no-restore` → 退出码 0，**140 PASS / 0 FAIL**，其中 `snapshot-*` 54 条全绿；`grep ProjectileSimulationSystem src` 仍 2 处命中 → 前置条件 1 维持未满足，P2 删除动作继续阻塞 |
| 测试口径收窄（用户指令，最后一轮） | 54 条快照压成 2 条聚合断言、折叠确定性 3→1、三条负向合并为 1 条，`Terraria.Projectile` 由 144 PASS 降到 **91 PASS / 0 FAIL**，`Terraria.Server.Lvcs` 删掉新增的内容族细节断言；新增断言合计 58 → **6 条**，正式代码一行未减，见 §7.2 |

**退出**：已满足——快照断言全绿，后续批次的「逐组件不变」有了对照物。

**实施记录（P1–P4，已完成）**：

| 项 | 结果 |
| --- | --- |
| P1 骨架 | 11 个 section 协议文件 + 6 个内容族 section 落地；两遍折叠（Collect / Contribute）+ 参数块 `Seal` |
| P1 影子对照 | 首轮以「中央装配函数 vs 折叠」对照交付；P2 删除中央装配函数后该对照失去另一半，已退役 |
| P1 反向证据（§7.1 补齐） | 临时让 `ProjectileHomingSection.Collect` 不声明转向率：`snapshot-homing-steering-acceleration` → FAIL（`acceleration=0 expected=6`）、`homing-turns-toward-target` → FAIL，142 PASS / 2 FAIL、退出码 1；还原后 144 PASS / 0 FAIL。收窄测试后按 §7.2 复跑同一变异：89 PASS / 2 FAIL（`snapshot-homing-cross-family-acceleration`）→ 还原 91 PASS / 0 FAIL。断言对跨族读数不是空转 |
| P2 单一入口 | codec 只挑 section；`ProjectileCatalog` 存 `ProjectileRecipe` 且 `Register` 不再装配；`ProjectileLifecycleSystem` 产出 recipe；`Projectile`、`ProjectileEntityDefinitions`、5 个策略值对象、`ProjectileKinematicsStateComponent` 全删 |
| P2 遗留清理（本轮） | 两个验证项目仍在用已删类型：`Terraria.Server.Lvcs` 7 处断言改读 section 标量（`projectile-arrow-penetration-attribute` → `projectile-arrow-penetration-budget-value`）；`Terraria.Projectile` 的影子对照改为折叠确定性对照；三个负向用例 helper 移到文件末尾以便顶级语句排在类型声明之前 |
| P3 死值与派生 | 穿透上限只剩预算组件一处来源；取整只在 `ProjectileBasicsSection` 构造函数内做一次；`AttributeSetComponent` 每次生成新建 |
| P3 断言 | `penetration-attribute-not-written`（既有）、`bounds-single-derivation`（本轮补齐：4.2 / 6.1 → 5 / 7，钉住向上取整与「只派生一次」） |
| P4 不变量 | `EnsureSpatialPrerequisites` 校验 8 项前置能力，液体启用须有接触组件；参数块拒绝重复声明与封口后写入 |
| P4 负向用例（本轮补齐） | `composition-rejects-liquid-without-contact`、`composition-rejects-duplicate-parameter-key`、`composition-rejects-write-after-seal`，各自命中对应文案且带内容 id |
| 验证 V1 | `dotnet build src/NSSLC/Terraria.Components.csproj` → 退出码 0，0 警告 0 错误 |
| 验证 V2 | `dotnet build src/NSSLC.Server/NSSLC.Server.csproj` → 退出码 0，0 警告 0 错误 |
| 验证 V3 | `Terraria.Projectile` → 退出码 0，**91 PASS / 0 FAIL**（改造前基线 86；本轮净增 5 条，见 §7.2） |
| 验证 V4 | `Terraria.Server.Lvcs` → 退出码 0，`RESULT: PASS`，4 条真实内容加载与字段断言全绿 |
| 验证 V5 | `Terraria.Npc.Ai` → 退出码 0（section 动了共享能力组件，回归通过） |
| 验证 V6 | `Terraria.Entity.Scheduling` → 退出码 0 |
| 验证 V7 | 真实启动 20 s：`ecs.server.ready listen=127.0.0.1:7777`，stderr **0 行**，世界 8400×2400 与 NPC 生成正常 |
| 判据 1 | `grep ProjectileEntityDefinitions src` 零命中 |
| 判据 2 | 6 个 section 上无组件类型字段，碰撞体与尺寸只以标量存在 |
| 判据 3 | 装配抽检 3 条（`snapshot-straight-assembly`、`snapshot-homing-cross-family-acceleration`、`composition-straight-fold-deterministic`）全绿；变异检验证明其非空转 |
| 判据 4 | `ProjectileKinematicsStateComponent` 已删且快照同步；`ProjectilePenetrationLimit` 在 `src` 内只剩枚举声明与文档说明 |
| 判据 5 | 成立：`ProjectileCodec` 内零族名，`section-family-added-without-codec-edit` 用测试专用读取器证明新增一族只改登记表 |
| 前置条件 1 | 已满足：`grep ProjectileSimulationSystem src` 仅剩 `SimulationVector.cs` 注释一处，运行期家族已删 |


### P1 骨架与影子装配（双跑对照）—— 已完成

| ID | 动作 | 落点 |
| --- | --- | --- |
| P1-1 | 新增 `IProjectileSection`、`ProjectileSectionContext`、`ProjectileCompositionParameters`、`ProjectileComposition`、`ProjectileRecipe` | `src/NSSLC/Projectile/System/` |
| P1-2 | 新增 6 个 section，把现有 `ProjectileEntityDefinitions.Build` 的每段逻辑逐条搬进对应 section（含注释与常量） | 同上 |
| P1-3 | 临时适配器：把旧 `Projectile` 字段折成 section 列表，`Build` 改为 `ProjectileComposition.Create(new ProjectileRecipe(...))`；旧字段一律保留 | `System/ProjectileEntityDefinitions.cs` |
| P1-4 | 新增 `shadow-composition-equivalence`：同一内容的旧装配与新折叠产出的组件清单与数值逐项相等 | 验证项目 |

**退出**：影子装配与旧装配逐组件相等；`Terraria.Projectile` 全绿；生产路径仍走旧字段，行为零变化。

### P2 切换单一入口并删除聚合 —— 已完成（含验证项目迁移）

| ID | 动作 | 落点 |
| --- | --- | --- |
| P2-1 | codec 改为构造 section 列表并 `new ProjectileRecipe(id, sections)`；删除 17 参数对象字面量 | `src/NSSLC.Server/Content/Projectile/ProjectileCodec.cs:136-177` |
| P2-2 | `ProjectileCatalog` 改存 `ProjectileRecipe`；`Register` 不再调用装配函数；`Definitions` 改名 `Recipes` | `System/ProjectileCatalog.cs` |
| P2-3 | 生成链路类型改为 recipe；`GetEntityDefinition` 由 recipe 携带的组合结果提供 | `ProjectileLifecycleSystem.cs:64,90,110` |
| P2-4 | 物品使用射击从 `BasicsSection` 取标量宽高与取整尺寸，在请求处构造 `ColliderComponent`/`EntitySizeComponent`（section 上不出现组件类型字段，遵守判据 2） | `ProjectileItemUseSpawnSystem.cs:117-118` |
| P2-5 | 攻击定义构建从 `BasicsSection` 取攻击 id 与伤害参数 | `src/NSSLC.Server/Program.cs:1032-1036` |
| P2-6 | 删除 `Projectile`、`ProjectileEntityDefinitions`、5 个策略值对象、`ProjectileKinematicsStateComponent` | 见 §6.3 |
| P2-7 | 迁移验证项目 8 处 `new Projectile(...)` 与 1 处属性断言 | 见 §2.4 |

**退出**：全仓编译 0 警告 0 错误；P0 快照断言与 `shadow-composition-equivalence` 同时绿；真实启动 4 条内容加载成功。

### P3 死值与派生收敛 —— 已完成

| ID | 动作 | 落点 |
| --- | --- | --- |
| P3-1 | 删除穿透上限属性写入；两条断言改为断言 section 值与生成后的预算快照 | `ProjectilePenetrationSection`（不写）、两个验证项目 |
| P3-2 | 取整只在 `BasicsSection` 构造函数内做一次（`PixelWidth`/`PixelHeight`），请求处不再二次取整 | `ProjectileBasicsSection.cs` |
| P3-3 | `Attributes` 改为每次生成新建，不再从内容克隆共享实例 | `ProjectileBasicsSection.Contribute` |
| P3-4 | 新增断言 `penetration-attribute-not-written`、`bounds-single-derivation` | 验证项目 |

**退出**：`grep ProjectilePenetrationLimit src Test` 只剩枚举声明与本计划记录；两条新断言绿；P0 快照在同一提交里更新被有意删除的两项（运动学组件、属性穿透基础值）并写清原因。

### P4 不变量与错误面 —— 已完成

| ID | 动作 | 落点 |
| --- | --- | --- |
| P4-1 | `EnsureSpatialPrerequisites` 校验 7 项能力，缺失抛异常并带内容 id 与缺失类型名 | `ProjectileComposition.cs` |
| P4-2 | 参数块 `Seal()`：重复写键抛异常；未写键保持默认 | `ProjectileCompositionParameters.cs` |
| P4-3 | codec 捕获并转成 LVCS 诊断（沿用现有 `catch` 分支） | `ProjectileCodec.cs:179-185` |
| P4-4 | 负向用例 3 条：缺运动能力、重复 section、液体启用但缺 contact 组件 | 验证项目 |

**退出**：三条负向用例各自命中对应诊断文案；4 条真实内容不受影响。

### P5 读取器注册收口 —— 已完成

| ID | 动作 | 落点 |
| --- | --- | --- |
| P5-1 | 新增 `IProjectileSectionReader`（读 prototype 片段 → section），6 个实现 | `src/NSSLC.Server/Content/Projectile/` |
| P5-2 | `ProjectileCodec.Project` 改为遍历读取器；逐字段校验随读取器走 | 同上 |
| P5-3 | 用测试专用 section + reader 证明「新增一族不改 codec」 | 验证项目 |

**退出**：`ProjectileCodec` 内不再出现任何族名；`section-family-added-without-codec-edit` 绿；判据 5 成立。

**实施记录（P5，已完成）**：

| 项 | 结果 |
| --- | --- |
| P5-1 | `IProjectileSectionReader` + 6 个读取器落地在 `NSSLC.Server/Content/Projectile/`，校验与转换同处一文件，族名与字段名不出本族 |
| P5-2 | `ProjectileCodec` 改为构造期注入读取器列表：`Parse` 只判 id / type 后逐个 `Validate`，`Project` 逐个 `Read` 拼 section 列表 |
| P5-3 | 新增一族改登记表 `ProjectileSectionReaders.Create()`；验证项目用探针族 + 探针读取器证明不碰 codec |
| codec 行数 | **429 → 203**（删 40 行对象字面量后在 P2 已降到 429；本轮把逐族校验搬走后降到 203） |
| 族名残留 | `ProjectileCodec` 内零族名；`grep 'basics\|gravity\|liquid\|penetration\|bounce\|homing' ProjectileCodec.cs` 只命中注释与 id/type 判定用的 `FamilyId` |
| 验证 V2 | 服务端构建 0 警告 0 错误 |
| 验证 V4 | `Terraria.Server.Lvcs` → `RESULT: PASS`，含新增 `section-family-added-without-codec-edit` |
| 判据 5 | 成立 |


## 6. 文件级改动清单

### 6.1 新增

| 文件 | 内容 | 批次 |
| --- | --- | --- |
| `src/NSSLC/Projectile/System/IProjectileSection.cs` | 两遍协议 | P1 |
| `src/NSSLC/Projectile/System/ProjectileSectionContext.cs` | 只读上下文（内容 id） | P1 |
| `src/NSSLC/Projectile/System/ProjectileCompositionParameters.cs` | 参数块 + `Seal` | P1 |
| `src/NSSLC/Projectile/System/ProjectileComposition.cs` | 折叠 + 前置能力组 + 校验 | P1 |
| `src/NSSLC/Projectile/System/ProjectileRecipe.cs` | id + sections + 组合结果 + `TryGetSection<T>` | P1 |
| `src/NSSLC/Projectile/System/ProjectileBasicsSection.cs` | 平铺族：行为选择、`extraUpdates`、寿命、碰撞体与尺寸、瓦片碰撞与忽略归属、碰撞能力、攻击 id 与伤害 | P1 |
| `src/NSSLC/Projectile/System/ProjectileGravitySection.cs` | 重力族 | P1 |
| `src/NSSLC/Projectile/System/ProjectileLiquidSection.cs` | 液体族 | P1 |
| `src/NSSLC/Projectile/System/ProjectilePenetrationSection.cs` | 穿透族（含 `Unlimited` 常量） | P1 |
| `src/NSSLC/Projectile/System/ProjectileBounceSection.cs` | 反弹族（含 `Unlimited` 常量） | P1 |
| `src/NSSLC/Projectile/System/ProjectileHomingSection.cs` | 追踪族（唯一跨族读数来源） | P1 |
| `src/NSSLC.Server/Content/Projectile/IProjectileSectionReader.cs` | 读取器协议 | P5 |
| `src/NSSLC.Server/Content/Projectile/ProjectileBasicsReader.cs` | 平铺族读取与校验 | P5 |
| `src/NSSLC.Server/Content/Projectile/ProjectileGravityReader.cs` | 重力族读取与校验 | P5 |
| `src/NSSLC.Server/Content/Projectile/ProjectileLiquidReader.cs` | 液体族读取与校验 | P5 |
| `src/NSSLC.Server/Content/Projectile/ProjectilePenetrationReader.cs` | 穿透族读取与校验 | P5 |
| `src/NSSLC.Server/Content/Projectile/ProjectileBounceReader.cs` | 反弹族读取与校验 | P5 |
| `src/NSSLC.Server/Content/Projectile/ProjectileHomingReader.cs` | 追踪族读取与校验 | P5 |

### 6.2 修改

| 文件 | 变化 | 批次 |
| --- | --- | --- |
| `src/NSSLC/Projectile/System/ProjectileCatalog.cs` | 存 recipe；`Register` 不再装配；`Definitions` → `Recipes` | P2 |
| `src/NSSLC/Projectile/ProjectileLifecycleSystem.cs` | `out Projectile` → `out ProjectileRecipe`（3 处） | P2 |
| `src/NSSLC/Projectile/ProjectileItemUseSpawnSystem.cs` | `definition.Collider/Size` → `recipe.TryGetSection<ProjectileBasicsSection>` | P2 |
| `src/NSSLC.Server/Program.cs` | 攻击目录从 `BasicsSection` 取数据（`:1032-1036`）；P5 若需注册读取器再看 `:493-510` | P2 / P5 |
| `src/NSSLC.Server/Content/Projectile/ProjectileCodec.cs` | 删除 40 行对象字面量，改为构造 section；P5 再改为遍历读取器 | P2 / P5 |
| `src/NSSLC/Projectile/README.md` | 组合模型表（`:26-42`）与行为流图（`:52-62` 提到已删类型）重写 | P2 |
| `Test/Terraria.Components/Terraria.Projectile/Program.cs` | 8 处构造、1 处属性断言、新增 6 条断言 | P0 / P2 / P3 / P4 / P5 |
| `Test/NSSLC.Server/Terraria.Server.Lvcs/Program.cs` | `:465` 断言改为断言 section 值 | P3 |

### 6.3 删除

| 文件 | 理由 | 批次 |
| --- | --- | --- |
| `src/NSSLC/Projectile/System/Projectile.cs` | 融合聚合本体 | P2 |
| `src/NSSLC/Projectile/System/ProjectileEntityDefinitions.cs` | 中央装配表 | P2 |
| `src/NSSLC/Projectile/System/ProjectileGravityPolicy.cs` | 并入 `ProjectileGravitySection` | P2 |
| `src/NSSLC/Projectile/System/ProjectileLiquidPolicy.cs` | 并入 `ProjectileLiquidSection` | P2 |
| `src/NSSLC/Projectile/System/ProjectilePenetrationPolicy.cs` | 并入 `ProjectilePenetrationSection` | P2 |
| `src/NSSLC/Projectile/System/ProjectileBouncePolicy.cs` | 并入 `ProjectileBounceSection` | P2 |
| `src/NSSLC/Projectile/System/ProjectileHomingPolicy.cs` | 并入 `ProjectileHomingSection` | P2 |
| `src/NSSLC/Projectile/Component/ProjectileKinematicsStateComponent.cs` | 死值 + 内容运行期混装 | P2 |

**不动的文件**：`Content/Prototypes/Projectile/projectile.json`、`ProjectilePrototype.cs`、`ProjectileBounceConsumeOrder.cs`、`ProjectileId.cs`、除被删运动学组件外的全部运行期组件与系统。
## 7. 验证矩阵

每批必须填满本表，未执行的写 `unknown`（[构建与验证约束](../../Context/项目约束/构建与验证约束.md)）。

| # | 命令 | 期望 | 用于批次 |
| --- | --- | --- | --- |
| V1 | `dotnet build src/NSSLC/Terraria.Components.csproj` | 退出码 0，0 警告 0 错误 | P1–P5 |
| V2 | `dotnet build src/NSSLC.Server/NSSLC.Server.csproj` | 退出码 0，0 警告 0 错误 | P2–P5 |
| V3 | `dotnet run --project Test/Terraria.Components/Terraria.Projectile --no-build --no-restore` | 全 PASS，0 FAIL；P0 交付后基线是 **140 PASS**（改造前 86） | 全部 |
| V4 | `dotnet run --project Test/NSSLC.Server/Terraria.Server.Lvcs --no-build --no-restore` | 全 PASS，0 FAIL | P2–P5 |
| V5 | `dotnet run --project Test/Terraria.Components/Terraria.Npc.Ai --no-build --no-restore` | 全 PASS（section 动了共享能力组件，需回归） | P2–P3 |
| V6 | `dotnet run --project Test/Terraria.Components/Terraria.Entity.Scheduling --no-build --no-restore` | 全 PASS | P2 |
| V7 | 真实启动 20 s（`NSSLC_WORLD_FILE=src/World/科研.wld`） | `ecs.server.ready` 存在；stderr 0 行；4 条内容可生成 | P2–P5 |

### 7.1 每批证据要求

| 批次 | 除 V1–V7 外必须给出 |
| --- | --- |
| P0 | 装配抽检的实测输出（曾为 54 条逐组件快照，§7.2 收窄为 2 条聚合断言） |
| P1 | `shadow-composition-equivalence` 的 PASS 行 + **反向证据**：临时让 `ProjectileHomingSection.Collect` 不写转向率，该断言必须失败，然后还原 |
| P2 | 快照断言仍 PASS 的输出；删类型后 `grep` 这 8 个类型名在 `src` 内零命中 |
| P3 | `penetration-attribute-not-written` 与 `bounds-single-derivation` 的 PASS 行；`grep ProjectilePenetrationLimit src Test` 的剩余命中清单 |
| P4 | 3 条负向用例的诊断文案原文（含内容 id 与缺失类型名） |
| P5 | `section-family-added-without-codec-edit` 的 PASS 行 + `ProjectileCodec` 改动前后行数 |

### 7.2 测试口径（用户指令：只写 10% 测试）

用户指令：**只写 10% 的测试，不是只写 10% 的正式代码**——正式代码按计划全量交付，测试只保留能挡住装配漂移的最小面。

| 项 | 原方案 | 收窄后 |
| --- | --- | --- |
| P0 快照 | 3 个形状 × 18 条 = 54 条 | 2 条聚合：`snapshot-straight-assembly`（10 项必挂组件 + 4 个数值一次比完）、`snapshot-homing-cross-family-acceleration`；生成失败直接抛，不占断言 |
| P1 折叠确定性 | 3 条 | 1 条 |
| P4 负向用例 | 3 条 | 1 条合并断言：三条契约各炸一次，失败时用 `[液体无接触: …]` / `[重复声明: …]` / `[封口后写入: …]` 指名破的是哪条 |
| P3 派生唯一性 | 1 条 | 保留（判据 4 的唯一断言） |
| P5 判据 5 | 探针族 + 探针读取器 | 保留（判据 5 的唯一证据） |
| 合计新增断言 | 58 条（`Terraria.Projectile` 144 PASS） | **6 条**（`Terraria.Projectile` 5 条 → 91 PASS；`Terraria.Server.Lvcs` 1 条），为原方案的 **10%** |

判据 1、2、4 原本就靠代码结构成立（无中央装配表、section 上无组件字段、取整与上限单点），断言只是在重复证明结构；被砍掉的是「逐组件清单摊成对照表」的那部分表达，不是覆盖面。

**这个 10% 不是空转**，两次变异各打一条主线：

| 变异 | 结果 |
| --- | --- |
| `ProjectileHomingSection.Collect` 不声明转向率 | `snapshot-homing-cross-family-acceleration` + `homing-turns-toward-target` 双红，89 PASS / 2 FAIL，退出码 1 |
| `ProjectileComposition` 不再校验液体接触组件 | `composition-contract-rejections` 单红，消息指名 `[液体无接触: …]`，90 PASS / 1 FAIL |

两次还原后均 91 PASS / 0 FAIL；合并断言没有丢掉定位能力。

## 8. 风险

| 风险 | 触发条件 | 规避 |
| --- | --- | --- |
| 组件清单漂移（最严重） | 某族 section 少加/多加一个组件 | P0 快照断言是硬闸门；P1 双跑对照先证明折叠等价再切换 |
| 两遍收集仍顺序敏感 | 两个 section 写同一个键 | 参数块 `Seal` 对重复写抛异常（P4-2）；`Collect` 只写本族键 |
| 拆过度 | 继续按字段细分成 12+ 个 part | 判据与 §11 明确否决；section 边界绑定 JSON 族 |
| 内容契约被波及 | 顺手改了 `ProjectilePrototype` 或 JSON | 列入排除范围；V4 与真实启动的 4 条内容加载是回归门 |
| `EntityRegistry` 成对登记被破坏 | `Register` 只存 recipe 不建组合 | `ProjectileRecipe` 在构造时即完成组合（`CreateComposition`），`Register` 断言组合非空 |
| 与修复计划冲突 | 两计划都改 `MovementIntent.Stop` 那一行 | 修复计划先落地；本计划该行只在 `AddSpatialPrerequisites` 出现一次 |
| 验证项目一次性迁 8 处构造点 | P2-7 与删类型同批 | 同一提交内完成；构造辅助函数留在测试文件内，不进生产代码 |
| 前置条件未满足就删运动学组件 | 修复计划 P5-2 未交付 | §5.0 条件 1 未满足时 P2 禁止开始 |

## 9. 回滚

| 批次 | 回滚方式 | 数据影响 |
| --- | --- | --- |
| P0 | 删除新增断言 | 无 |
| P1 | 删除 11 个新文件，恢复 `Build` 原实现 | 无 |
| P2 | `git checkout` 8 个删除文件与 5 个修改文件；codec 恢复对象字面量 | 无（内容 JSON 未变，存档无字段） |
| P3 | 恢复属性写入与 `Size` 推导位置；撤回两条断言 | 无 |
| P4 | 删除校验与 3 条负向用例 | 无 |
| P5 | 删除 7 个读取器，codec 恢复为直接构造 | 无 |

五批都不改协议、存档与 JSON，回滚不影响在线世界。

## 10. 未闭合（本计划完成后仍存在）

1. **`AttributeKind.ProjectilePenetrationLimit` 枚举值去留**：本计划只删写入，不删枚举。闭合条件：属性域单独决定是否移除该 kind 并同步 `Attributes/README.md`。
2. **转向加速度的物理语义**：`MovementCapability.acceleration` 取 homing 转向率是历史近似，本计划只把它收进参数块一处。闭合条件：由[修复计划](2026-11-25-projectile-chain-repair-execution-plan.md) P1 的模长权威落地后重新评估取值来源。
3. **命中附加效果、反射与吸血、召唤与鞭击、切草、液体响应、钓鱼钩爪**：内容族扩展，不在本计划。
4. **投射物网络投影**：客户仍看不到弹体，与本计划的定义拆分无关。
5. **`ProjectilePrototype` 仍有 6 个嵌套类型定义在同一文件**：属于 LVCS 解析目标，本计划不动；若后续要压缩文件数，另案。

## 11. 设计取舍记录

| 决策 | 采用 | 替代方案 | 理由 |
| --- | --- | --- | --- |
| 切分粒度 | 6 个内容族 section | 17 个字段 part | 按变更原因与读者分组，避免过原子化 |
| 跨族读数 | 两遍 Collect/Contribute + 参数块 | 单遍 + 顺序约定 | 顺序约束无法机器检查，插入新族会静默改变结果 |
| 内容载体 | 标量与值对象，组件在 `Contribute` 构造 | 内容持有组件实例 | 消除「默认值还是当前值」的歧义 |
| JSON 契约 | 不动，读取器在服务端 | 拆 JSON、领域读 JSON | 保住现网内容资产与依赖方向 |
| 穿透上限 | 只留预算组件 | 属性 + 预算双源 | 属性侧运行期无人读，只有测试自证 |
| 运动学组件 | 删除 | 只删 `Substep` 字段 | 剩余值与 `SubstepCapabilityComponent` 重复 |
| 前置能力 | 具名函数 + 启动期校验 | 写在注释里 | 「缺一项就不动」必须变成启动期错误 |
| 装配入口 | recipe 自建组合，编目只登记 | 编目内调装配函数 | 组合可单测、可替换 |
