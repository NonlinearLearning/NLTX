# Version4 第二轮审查综合报告

> 本文将 `docs/component-decomposition/review-round-2` 下的 4 份只读审查报告整理为一份综合文档。原报告正文全部保留；本文件只增加统一导航、跨报告共识和报告层级，不把候选自动升级为正式 owner，也不把设计的 verifier 当成本轮已运行验证。
>
> 原始 4 个文件已在本综合文档完成核验后删除。下方目录中的文件名是历史来源记录，不再指向独立文件。

## 1. 合并范围与状态边界

- 范围：仅合并 `docs/component-decomposition/review-round-2` 中本轮的 4 份 Markdown。
- 证据语义：`confirmed`、`full-reference-supplemented`、`boundary-qualified`、`partial`、`missing` 等状态沿用各原报告定义，不能互相替代。
- 交付语义：本文件仍是只读审查和 Integration Review 输入，不是迁移完成证明、行为等价证明、API 兼容证明或正式索引变更。
- 验证语义：原报告标记的 `verificationStatus: not-run` 保持不变；本次仅验证文档内容完整性，没有运行 .NET build/test。

## 2. 阅读导航

| 章节 | 历史源文件 | 内容定位 |
| --- | --- | --- |
| 4.1 | `2026-09-07-version4-additional-subsystem-boundary-review.md` | 总览：新增子系统、运行时边界、资格矩阵与后续验证要求 |
| 4.2 | `2026-09-07-version4-reverse-discovered-subsystems-follow-up.md` | 反向发现：Shimmer、玩家生成、入侵进度、规则覆盖及排除项 |
| 4.3 | `2026-09-07-version4-second-round-follow-up-boundary-challenges.md` | 边界挑战：世界掉落、制作事务、交易、Banner 与 Bestiary 进度账本 |
| 4.4 | `2026-09-07-world-item-lifecycle-and-pickup-boundary-challenge.md` | 专题深挖：WorldItem 生命周期、拾取、提交根与整合裁决 |

## 3. 跨报告共识

### 3.1 统一边界裁决标准

各报告反复使用四项门槛判断候选是否值得进入整合裁决：

1. 是否存在独立生命周期或调度阶段。
2. 是否存在独立权威状态、唯一写入根或受控提交边界。
3. 是否存在跨两个以上既有领域的稳定输入/输出、协议或数据边界。
4. 是否可以定义独立且可执行的 focused verifier。

命中门槛只说明候选具备边界证据，不等同于已经写入正式子系统索引。

### 3.2 共同的整合风险

- 同一权威状态被多个 owner、System 或事务路径写入，导致所有权冲突。
- 实体生命周期、库存/容器、网络复制、持久化和结果提交之间缺少唯一 structural commit root。
- 将 proposed Component、System、Command、Adapter 或 Projection 误读为当前已存在实现。
- 将源码中设计的 PASS 断言、扫描器结果或报告自洽性误读为本轮 verifier 已通过。

### 3.3 综合阅读顺序

先阅读第 4.1 节的总览和资格矩阵，再阅读第 4.2、4.3 节的反向发现与边界挑战，最后用第 4.4 节的 WorldItem 专题核对世界掉落候选的具体生命周期和提交边界。所有最终 owner、索引和 structural commit root 决策仍需经过整合阶段。

## 4. 原始审查正文

### 4.1 Version4 第二轮子系统边界审查

**日期：** 2026-09-07
**审查类型：** 只读反向发现、边界挑战与 owner 整合
**输入范围：** `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1` 下全部 Markdown，以及当前 Version4/NLTX 子系统索引、全量审查报告和相关源码证据
**使用方法：** `public-decomposition` + 用户指定的 `pua` 检查流程
**verificationStatus：** `not-run`
**交付状态：** `final-readonly-review`
**正式索引变更：** `deferred`

> 本文记录的是反向审查和 Integration Review 输入，不是迁移完成证明。`WiringAndMechanisms` 与 `MountAndVehicleSimulation` 是本轮确认达到一级权威玩法 owner 门槛的责任面；它们以及本文其他候选都不会仅凭本报告自动写入或重写 `Version4子系统索引.json`、`Version4源码覆盖.tsv`。

#### 1. 结论

相对于第一轮固定的 19 个游戏模拟子系统，本轮反向扫描把候选分成四层。这个分层同时保留了“发现了独立边界”和“已经裁决为一级 owner”之间的差异：

1. 已确认可新增一级权威玩法 owner：`WiringAndMechanisms`、`MountAndVehicleSimulation`。它们分别拥有独立的传播/机械生命周期、挂载/车辆生命周期、跨域写集和可设计的 focused verifier；当前索引已经记录前者 `missing`、后者 `partial`，但本报告没有自动修改索引。
2. 已达到四项边界门槛、但仍需 Integration Review 才能写入正式 owner 索引的高优先级候选：`OldOnesArmyEventSimulation`（权威玩法）和 `TileEntityRuntime`（运行时基础设施）。它们不再只是“单个事件类”或“单个 TileEntity 类型”的候选。
3. 具有独立状态和协议边界、但建议先作为既有 owner 内部模块或 seam 的 `boundary-challenge`：`BannerProgressionAndClaim`、`WeightedPressurePlateActivationRuntime`。Banner 推荐先挂在 `WorldProgressionAndUnlocks` 的 `BannerLedger` seam 下；压力板推荐留在 Wiring 的 occupancy/edge seam。
4. 可作为既有 owner 内部 seam、不能另立一级 owner：`WorldGenerationControlAndRecovery`、`TileObjectPlacementAndFraming`、`DungeonGenerationPipeline`、`EmergencyStacking`、`PressurePlateHelper`。

`WorldItemSimulation`（也可称 `WorldDropLifecycle`）保留为“有力候选”，但不在本报告中直接写成已经裁决的新 owner：世界掉落实体的生成、拾取、堆叠、销毁和复制，仍必须先与 `SpawnLifecycleAndLoot`、`ItemContainerAndEconomy` 和统一 structural commit root 对齐。

必须保留的当前索引事实与语义建议差异：

- `docs/migration/ledgers/Version4源码覆盖.tsv` 当前把 `Terraria/Minecart.cs` 记为 `SharedRuntimeMechanisms`，而本报告基于挂载、速度、碰撞、轨道和 Wiring 的共同不变量，建议把它纳入 `MountAndVehicleSimulation`；这只是语义建议，TSV 尚未同步。
- 同一 TSV 当前把 `Terraria.GameContent/BannerSystem.cs` 和 `Terraria.GameContent.Events/DD2Event.cs` 记为 `SharedRuntimeMechanisms`；本报告把 Banner 保留为内部 `boundary-challenge`，把 DD2 提升为 `boundary-qualified` 候选，但没有声称索引已经改为新 owner。
- `docs/migration/ledgers/Version4子系统索引.json` 与 TSV 中的 21 个 authoritative owner 仍是当前统计分母；本报告的候选和 boundary challenge 不自动增加这个数量。

本轮的新增候选中，`WiringAndMechanisms` 和 `MountAndVehicleSimulation` 满足以下四项标准：

- 独立生命周期或调度；
- 独立权威状态根或受控提交边界；
- 稳定的跨领域输入/输出边界；
- 可独立设计 focused verifier。

`OldOnesArmyEventSimulation` 和 `TileEntityRuntime` 已找到四项标准的源码证据，但其正式 owner 仍须通过 integration review 解决唯一提交根；`BannerProgressionAndClaim` 与 `WeightedPressurePlateActivationRuntime` 则建议优先作为已有聚合/传播边界的内部模块。不能用报告文字替代 integration review。`Crafting`、`Commerce`、`Buff`、`Potion`、`Minecart`、`Lighting`、`Map`、`Chat`、`Achievement/Social` 等候选经过反向检查后，应归入已有子系统、运行时基础设施、外部边界或客户端投影。

本报告的“发现”不等于“已迁移实现”、行为等价、API 兼容或运行时可用性声明。当前 NLTX 映射仍以索引中的 `missing`、`partial`、`confirmed` 和 `excluded` 为准。

#### 2. 审查范围与证据规则

##### 2.1 第一轮材料

`docs\component-decomposition\review-round-1` 当前包含 59 个 Markdown 文件，其中包括：

- 公共 `public-decomposition` 协议；
- 19 个固定子系统审查提示词；
- 子系统组件设计文档；
- 组件代码草案；
- Component-only Design 会话提示词；
- Version4 成员迁移查找源提示词。

公共协议把第一轮定义为 19 个固定游戏模拟子系统，并规定相邻候选只能作为 `boundary-challenge` 交给最终整合，不能在单个子系统审查中直接改变固定清单。

见：

- [第一轮公共审查协议](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-public-decomposition-common-protocol.md:3)
- [公共协议的固定范围和只读约束](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-public-decomposition-common-protocol.md:9)
- [WorldInteractionAndStructures 对 Wiring 的 boundary-challenge 约束](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-world-interaction-and-structures-public-decomposition.md:51)

第一轮固定子系统为：

```text
CombatAndStatus
DeathPenaltyAndRevenge
FishingAndCatchSimulation
ItemContainerAndEconomy
LeashedEntitySimulation
LiquidSimulation
NpcAndTownSimulation
PlayerGameplay
ProjectileSimulation
SimulationRuleOverrides
SpatialSimulation
SpawnLifecycleAndLoot
TeleportationAndTraversal
WorldCalendarAndEventOrchestration
WorldGenerationAndEcology
WorldInteractionAndStructures
WorldProgressionAndTransition
WorldProgressionAndUnlocks
WorldSession
```

##### 2.2 证据优先级

本轮按以下顺序使用证据：

```text
D:/TRbackup/Version4
→ D:/TRbackup/无任何删减通过编译（只补同路径同类型的成员缺口）
→ D:/TRbackup/tmodloader-api-docs-stable（只交叉验证公开边界）
→ NLTX 当前 src/、Test/、dome/src/ 和既有报告
```

`D:\TRbackup\Version4` 是唯一覆盖分母。完整参考只能补充 Version4 已存在文件中的空实现、删减成员或相邻调用链，不能扩大 Version4 的覆盖范围。

候选不能根据目录名、单一类型、单个字段、Hook、消息号、AI style 或渲染器直接升格；必须反向盘点状态、读者、写者、生命周期、副作用和提交方向。

##### 2.3 当前统计

当前 [Version4 子系统索引](D:/TRbackup/NLTX/docs/migration/ledgers/Version4子系统索引.json:8) 和 [全量审查报告](D:/TRbackup/NLTX/docs/component-decomposition/baseline/Version4权威游戏模拟子系统全量审查报告-2026-09-05.md:9) 给出的基线为：

| 指标 | 数量 |
| --- | ---: |
| Version4 基线 C# 文件 | 967 |
| 一级子系统/owner | 32 |
| `full-reference-supplemented` 文件 | 2 |
| `source-gap` 文件 | 0 |
| `authoritative-simulation` owner | 21 |
| `runtime-infrastructure-and-adapters` owner | 10 |
| `client-presentation-and-toolchain` owner | 1 |

当前 NLTX 映射状态：

| 状态 | 数量 |
| --- | ---: |
| `confirmed` | 1 |
| `partial` | 16 |
| `missing` | 12 |
| `excluded` | 3 |

Version4 文件分类：

| 分类 | 数量 |
| --- | ---: |
| `excluded` | 61 |
| `external-dependency-or-generated` | 6 |
| `shared-runtime-mechanism` | 724 |
| `subsystem-evidence` | 176 |

21 个 `authoritative-simulation` owner 可以解释为：

```text
第一轮固定 19 项
+ WiringAndMechanisms
+ MountAndVehicleSimulation
```

上述 21 个是当前索引已经记录的 owner，不因本轮报告而自动增加。当前报告的候选处置如下：

| 候选 | 本轮处置 | 是否计入当前索引/21 个 authoritative owner | 原因 |
| --- | --- | --- | --- |
| `WorldItemSimulation` | `strong-candidate` | 否 | 世界掉落实体具有独立权威生命周期，但仍需把生成、拾取、堆叠、销毁和复制绑定到唯一 structural commit root |
| `BannerProgressionAndClaim` | `boundary-challenge` | 否 | 击杀账本、可领取数量、持久化和 claim 网络闭合了领域边界，但最小提交根仍应先落在 `WorldProgressionAndUnlocks` 的 `BannerLedger`，并通过 ItemContainer 结果端口发放物品 |
| `OldOnesArmyEventSimulation` | `boundary-qualified` | 否 | DD2 波次/胜负/刷怪/清场/掉落已满足四项门槛；仍需把 Calendar 编排与 Spawn/NPC/Projectile/Item 的结果提交协议拆开 |
| `TileEntityRuntime` | `boundary-qualified-runtime` | 否 | runtime identity、双索引、更新队列、类型注册和恢复协议已满足四项门槛；仍需与 WorldStorage 的持久化 substrate 和结构命令收敛 |
| `WeightedPressurePlateActivationRuntime` | `boundary-challenge` | 否 | 玩家占用 edge 是独立状态，但它与 Wiring 传播共享机制触发边界；当前优先定义为 Wiring 内部 occupancy/edge seam |

##### 2.4 交叉参考证据记录

本轮按 `public-decomposition` 要求做了两类只读交叉核对：

- tModLoader 本地稳定文档页眉为 `tModLoader v2026.07`。`class_tile_entity.html` 的 `Update` 锚点 `a483e78bf9c57dcc8d265703a67e91167` 明确说明 TileEntity 更新不在多人客户端执行；`NetSend`/`NetReceive` 锚点 `a85d2692b4774c6140a968ef92accc6df`、`a55d31824ac839a08c69b4c9e7c04c486` 明确说明服务端发送、客户端接收，且接收可能在同一位置替换旧实例；`SaveData` 锚点 `a2799ce9901b2c8efdb060491ea78869f` 只用于确认公开持久化 hook 边界。证据页为 [class_tile_entity.html](D:/TRbackup/tmodloader-api-docs-stable/class_tile_entity.html)；它不能证明 Version4 私有 registry 或协议实现。
- 同一版本的 [class_mod_n_p_c.html](D:/TRbackup/tmodloader-api-docs-stable/class_mod_n_p_c.html) 中，`Banner` 锚点 `a4e69ae68d865e139ca8b7524ee4e06dc` 和 `BannerItem` 锚点 `a3efbcf16cc5e74e0c64b802b687716c5` 只确认 NPC 内容定义如何参与 banner drop/bonus 与击杀阈值；它不替代 Version4 `BannerSystem` 的 293 项账本、存档或 claim 协议。
- Space Station 14 只用于 ECS 组织参考：[WiresComponent.cs](C:/Users/shan/Downloads/ECS/space-station-14-master/Content.Server/Wires/WiresComponent.cs:6)（字段范围 `:6-66`）把实体局部线路数据放在 Component；[WiresSystem.cs](C:/Users/shan/Downloads/ECS/space-station-14-master/Content.Server/Wires/WiresSystem.cs:43)（事件/更新/网络/初始化/`Dirty` 证据 `:43-59,301,394,465,613`）把事件摄取、周期更新、网络动作和 `Dirty` 放在 System；[MapAtmosphereComponent.cs](C:/Users/shan/Downloads/ECS/space-station-14-master/Content.Shared/Atmos/Components/MapAtmosphereComponent.cs:8)（字段范围 `:8-23`）说明世界级状态可以挂在 Map/World entity，并与派生 overlay 分离。它们不证明 Terraria 的私有算法、字段语义或执行顺序。

##### 2.5 本轮状态标记解释

`evidenceStatus: confirmed` 仅表示当前源码已经确认该成员或调用事实；`referenceStatus: full-reference-supplemented` 表示完整参考只补充了 Version4 同路径同类型的缺口；`decision: boundary-qualified`/`boundary-qualified-runtime` 表示满足独立边界门槛但尚未进入正式 owner 索引；`nltxMapping: partial/missing` 表示当前 NLTX 目标实现仍未闭合。所有 proposed 类型、System、Command、Adapter 和 Projection 都不是当前已存在实现。

#### 3. 相对第一轮的 13 个额外 owner

相对于第一轮 19 项，当前全量索引另外记录了 13 个 owner/责任面。它们不能全部称为“新增玩法子系统”。

| owner | 层级 | 当前 NLTX | 本轮判断 |
| --- | --- | --- | --- |
| `RuntimeComposition` | runtime infrastructure | `missing` | 全局启动、Tick 阶段、系统编排和失败可见性 |
| `PersistenceAndRecovery` | runtime infrastructure | `missing` | 世界/玩家保存、加载、格式验证、临时文件、回滚和恢复 |
| `NetworkSessionAndSectionStreaming` | runtime infrastructure | `missing` | 连接、握手、section 可见性、超时和复制调度 |
| `ContentLifecycleAndRegistration` | runtime infrastructure | `missing` | 内容加载、注册、跨内容 setup、finalization、卸载/重建 |
| `WiringAndMechanisms` | authoritative simulation | `missing` | 线信号、逻辑门、执行器、泵和机制提交 |
| `MountAndVehicleSimulation` | authoritative simulation | `partial` | 坐骑、车辆、钻头、矿车和玩家移动模式 |
| `WorldStorage` | runtime infrastructure | `partial` | Tile、section、container 和 TileEntity 持久化 substrate；运行时 registry 归属待整合 |
| `ContentCatalog` | runtime infrastructure | `partial` | 内容 ID、定义、Recipe 和声明式掉落规则 |
| `IntentAndInteraction` | runtime infrastructure | `partial` | 外部 intent 验证和向权威 writer 委派 |
| `ExternalBoundaries` | runtime infrastructure | `missing` | 复制、持久化 adapter、session transport 和客户端/服务端适配 |
| `ClientPresentationAndTools` | client presentation | `excluded` | UI、渲染、音频、诊断和客户端工具 |
| `SharedRuntimeMechanisms` | runtime infrastructure | `excluded` | 通用集合、值类型、定位和诊断等共享机制 |
| `ExternalDependencyOrGenerated` | runtime infrastructure | `excluded` | 第三方库、平台互操作和生成/程序集元数据 |

因此完整分类为：

```text
真正新增的权威玩法子系统：
  WiringAndMechanisms
  MountAndVehicleSimulation

第一轮之外的运行时/边界责任面：
  RuntimeComposition
  PersistenceAndRecovery
  NetworkSessionAndSectionStreaming
  ContentLifecycleAndRegistration
  WorldStorage
  ContentCatalog
  IntentAndInteraction
  ExternalBoundaries

明确排除的迁移目标：
  ClientPresentationAndTools
  SharedRuntimeMechanisms
  ExternalDependencyOrGenerated

本轮新增但未写入当前索引的候选：
  WorldItemSimulation / WorldDropLifecycle
  BannerProgressionAndClaim
  OldOnesArmyEventSimulation
  TileEntityRuntime
  WeightedPressurePlateActivationRuntime
```

这些 owner 的责任、证据、NLTX 映射和风险在索引中分别记录：

- [WiringAndMechanisms 和 MountAndVehicleSimulation](D:/TRbackup/NLTX/docs/migration/ledgers/Version4子系统索引.json:304)
- [WorldStorage、ContentCatalog、IntentAndInteraction](D:/TRbackup/NLTX/docs/migration/ledgers/Version4子系统索引.json:543)
- [ExternalBoundaries 和排除 owner](D:/TRbackup/NLTX/docs/migration/ledgers/Version4子系统索引.json:1151)

#### 4. 新增候选一：WiringAndMechanisms

##### 4.1 Version4 成员与生命周期

核心文件为 [Version4/Terraria/Wiring.cs](D:/TRbackup/Version4/Terraria/Wiring.cs:151)。其关键入口和阶段包括：

| 阶段 | 成员 | 责任 |
| --- | --- | --- |
| 初始化 | `Initialize()` | 初始化机制工作集、队列和注册状态 |
| 清理 | `ClearAll()` | 清理信号、逻辑门、泵和触发工作集 |
| 周期更新 | `UpdateMech()` | 逐 Tick 处理机械冷却和机制更新 |
| 外部触发 | `HitSwitch()` | 处理玩家、网络或车辆进入的开关触发 |
| 逻辑门 | `PokeLogicGate()`、`LogicGatePass()` | 进入和推进逻辑门传播 |
| 结构效果 | `Actuate()` | 提交执行器相关 Tile 结构变化 |
| 信号传播 | `TripWire()`、`HitWire()` | 建立传播区域并按 wire 类型处理 |
| 泵/液体 | `XferWater()` | 处理机制驱动的液体转移工作 |

内部存在独立的传播状态和工作集，包括：

- `_wireList`；
- `_wireDirectionList`；
- `_toProcess`；
- `_GatesCurrent`、`_GatesNext`、`_GatesDone`；
- `_LampsToCheck`；
- `_PixelBoxTriggers`；
- 泵坐标和泵计数；
- `_mechX`、`_mechY`、`_mechTime` 机械冷却状态。

这形成了明确的：

```text
触发输入
→ 传播队列
→ 机械冷却/限流
→ 逻辑门判定
→ 执行器/泵/Tile/实体效果
→ 网络和世界结构提交
```

##### 4.2 跨领域证据

Wiring 的调用和写入范围跨越多个原有 owner：

| 调用方/相邻领域 | 证据 | 影响 |
| --- | --- | --- |
| 空间碰撞 | [Collision.cs](D:/TRbackup/Version4/Terraria/Collision.cs:2549) | 压力板和碰撞触发机制 |
| 矿车/车辆 | [Minecart.cs](D:/TRbackup/Version4/Terraria/Minecart.cs:1276) | 轨道开关进入 Wiring 传播链 |
| 网络摄取 | [MessageBuffer.cs](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:903) | 网络命令调用 `PokeLogicGate()`、`Actuate()` |
| 世界结构 | [WorldGen.cs](D:/TRbackup/Version4/Terraria/WorldGen.cs:39523) | Tile、执行器和结构变化 |
| 逻辑传感器 | `TELogicSensor` | 传感器触发和传播入口 |
| 实体生成/掉落 | `NPC.MechSpawn`、`Item.MechSpawn` | 机制触发的实体结果 |

因此它不是单个压力板、单个逻辑门、单个 `TELogicSensor` 或单个网络消息，而是拥有自己的传播写集和受控 Tile/实体结果提交阶段的机制模拟器。

##### 4.3 资格判断

| 标准 | 证据判断 |
| --- | --- |
| 独立生命周期 | `Initialize()`、`UpdateMech()`、`HitSwitch()`、`TripWire()`、`LogicGatePass()` 和 `HitWire()` 形成独立更新/传播阶段 |
| 独立权威状态/受控提交 | 队列、逻辑门集合、泵工作集和机械冷却共同组成独立工作状态；结果通过执行器、液体、Tile 和实体路径提交 |
| 跨域边界 | 同时连接 Spatial、WorldStorage、WorldInteraction、Spawn/Loot、Vehicle 和 Network |
| 独立 verifier | 可独立验证冷却、传播顺序、逻辑门稳定性、执行器结果、泵结果和同步结果 |
| 结论 | **可升格为一级权威玩法子系统** |

##### 4.4 Version4 证据缺口

Version4 中下列实现被缩减为空体或桩：

- `CheckMech()`；
- `XferWater()`；
- `CheckLogicGate()`。

位置见 [Wiring.cs 的删减实现](D:/TRbackup/Version4/Terraria/Wiring.cs:464)。完整参考在同路径文件中提供了匹配成员和邻近调用链的补充实现：[完整参考 Wiring.cs](D:/TRbackup/无任何删减通过编译/Terraria/Wiring.cs:477)。

因此证据状态必须保持：

```text
referenceStatus: full-reference-supplemented
```

这表示完整参考补充了 Version4 已存在文件的成员级缺口，不表示 Version4 自身已经拥有完整算法，也不允许扩大 Version4 覆盖分母。

##### 4.5 当前 NLTX 状态

当前 NLTX 有局部 Wiring 目录、组件和 verifier，例如：

```text
src/WorldInteraction/Wiring/
dome/src/Terraria.Dome.Simulation/Wiring/
dome/Test/Terraria.Dome.Wiring.Verification/
dome/Test/Terraria.Dome.WiringLiquidChest.Contracts.Verification/
dome/Test/Terraria.Dome.WiringLiquidChest.Loopback.Verification/
```

但索引仍将它标为：

```text
nltxMapping.status: missing
```

原因是目前没有证据证明以下完整闭环已经闭合：

```text
wire graph
→ trigger queue
→ mechanism cooldown
→ logic-gate propagation
→ actuator/pump effects
→ structural/entity commit
→ synchronized replication
```

已有 verifier 可以证明局部组件契约或局部 loopback 行为，但不能自动证明 Version4 对齐的完整权威执行链。

#### 5. 新增候选二：MountAndVehicleSimulation

##### 5.1 Version4 成员与生命周期

核心坐骑文件为 [Version4/Terraria/Mount.cs](D:/TRbackup/Version4/Terraria/Mount.cs:647)。关键生命周期包括：

| 阶段 | 成员/位置 | 责任 |
| --- | --- | --- |
| 注册初始化 | `Mount.Initialize()` | 建立坐骑注册表和定义 |
| 装备后更新 | `UpdateAfterEquips()` | 根据装备和玩家状态更新坐骑能力 |
| 钻头更新 | `UpdateDrill()` | 处理钻头坐骑的空间、Tile 和 Projectile 交互 |
| 移动表现状态 | `UpdateFrame()` | 根据移动状态、速度和接地状态推进坐骑状态 |
| 能力效果 | `UpdateEffects()` | 处理能力、效果、冷却和玩家边界 |
| 附着/解除 | `SetMount()`、`Dismount()` | 修改玩家挂载关系和移动模式 |

关键源码位置：

- [Mount.UpdateAfterEquips](D:/TRbackup/Version4/Terraria/Mount.cs:2595)
- [Mount.UpdateDrill](D:/TRbackup/Version4/Terraria/Mount.cs:2650)
- [Mount.UpdateFrame](D:/TRbackup/Version4/Terraria/Mount.cs:3089)
- [Mount.UpdateEffects](D:/TRbackup/Version4/Terraria/Mount.cs:4097)

矿车具有独立但共享边界的轨道运算：

- [Minecart.TrackCollision](D:/TRbackup/Version4/Terraria/Minecart.cs:566)
- [Minecart.FrameTrack](D:/TRbackup/Version4/Terraria/Minecart.cs:953)
- [Minecart.GetOnTrack](D:/TRbackup/Version4/Terraria/Minecart.cs:1185)
- [Minecart.TrackRotation](D:/TRbackup/Version4/Terraria/Minecart.cs:1246)
- [Minecart.HitTrackSwitch](D:/TRbackup/Version4/Terraria/Minecart.cs:1276)

##### 5.2 跨领域写集

Mount/Vehicle 领域同时影响：

- Player 的附着、解除和移动模式；
- Spatial 的碰撞、速度和轨道位置；
- Projectile 的钻头和相关能力；
- Tile/轨道的写入；
- Wiring 的轨道开关触发；
- 坐骑能力、冷却、飞行和疲劳状态；
- 网络同步和客户端表现边界。

tModLoader 的公开边界也能交叉验证以下生命周期：

- `SetMount`；
- `Dismount`；
- `UseAbility`；
- 坐骑更新；
- 玩家和坐骑的附着关系。

交叉证据：

- [class_mount.html](D:/TRbackup/tmodloader-api-docs-stable/class_mount.html:169)
- [class_mount_loader.html](D:/TRbackup/tmodloader-api-docs-stable/class_mount_loader.html:100)

##### 5.3 资格判断

| 标准 | 证据判断 |
| --- | --- |
| 独立生命周期 | 注册、附着、解除、装备后更新、能力、钻头、帧和效果均有专门路径 |
| 独立权威状态/受控提交 | 坐骑注册、挂载关系、移动模式、能力冷却、钻头/车辆状态共同决定玩家移动和结果提交 |
| 跨域边界 | 同时连接 Player、Spatial、Projectile、Tile、Wiring 和 Network |
| 独立 verifier | 可独立验证 attach/dismount、移动模式、能力冷却、轨道碰撞、轨道切换和速度提交 |
| 结论 | **可升格为一级权威玩法子系统** |

##### 5.4 为什么 Minecart 不单独升格

`Minecart` 的轨道算法复杂，但它与 Mount、Player、Spatial 共享同一组关键不变量：

- 玩家附着状态；
- 速度和位置提交；
- 碰撞响应；
- 轨道方向和轨道切换；
- Wiring 触发；
- 车辆能力和移动模式。

`Minecart.HitTrackSwitch()` 会进入 `Wiring.HitSwitch()`，见 [Minecart.cs](D:/TRbackup/Version4/Terraria/Minecart.cs:1276)。如果把 Minecart 另立为一级系统，而没有先定义 Mount/Vehicle、Spatial 和 Player 的唯一提交阶段，容易产生两个系统分别提交速度、位置或碰撞结果的问题。

因此当前归属为：

```text
MountAndVehicleSimulation
```

而不是：

```text
MinecartSimulation
```

##### 5.5 当前 NLTX 状态

当前 NLTX 有：[PlayerMountState.cs](D:/TRbackup/NLTX/src/Player/PlayerMountState.cs) 以及与 Teleportation/Movement 相关的局部内容。

索引状态为：

```text
nltxMapping.status: partial
```

尚未闭合的范围包括：

- mount registry；
- attach/dismount 执行链；
- movement mode；
- ability/cooldown；
- drill 行为；
- minecart/track 行为；
- Wiring 联动；
- Player、Spatial、Projectile 之间的统一提交阶段。

#### 6. 强候选：WorldItemSimulation / WorldDropLifecycle

##### 6.1 边界定义

这里的候选不是泛化的 `ItemSimulation`。`Item` 定义、前缀规则、容器余额和库存事务仍属于 `ContentCatalog` 与 `ItemContainerAndEconomy`。候选只拥有“已经进入世界槽位的掉落实体”及其世界生命周期：生成/槽位接纳、空间运动、保留、合并、玩家或敌人拾取、越界保护、过期销毁、Shimmer 状态和世界物品复制。

Version4 的 `WorldItem` 将掉落实体状态和 `Item inner` 的投影属性集中在一个运行时对象中。字段本身不能机械地全部搬入一个 ECS component；`inner` 的内容定义/实例字段与世界生命周期字段必须拆开，并由唯一的世界掉落 owner 协调。

##### 6.2 成员级证据

| Member | Declaring Type | Visibility | Read By | Written By | Lifecycle | Access Pattern | State Kind | Evidence | Status | Candidate |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `inner`、`active/type/stack/maxStack` 等投影属性 | `WorldItem` | public field / public property | `WorldItem`、`Player`、`NPC`、`Item`、网络路径 | `SetDefaults`、`Prefix`、`TurnToAir`、合并/拾取路径 | 槽位创建至销毁 | `WorldItem` 读取实例，属性把部分 `Item` 状态投影到掉落实体 | 实例引用 + 派生投影；不是内容定义 | `WorldItem.cs:15-17,47-163,202-225` | `partial` | `WorldItemSimulation` |
| `ownTime`、`playerIndexTheItemIsReservedFor`、`noGrabDelay`、`keepTime` | `WorldItem` | public fields | `UpdateItem`、`FindOwner`、玩家接纳/拾取逻辑 | `UpdateItem`、`FindOwner`、生成/拾取路径 | 生成、保留、释放、接纳 | 以 player slot 为键的 reservation/anti-grab 状态 | 权威生命周期状态 | `WorldItem.cs:19-23,33-43,357-414`；`WorldItem.cs:257-347` | `confirmed` | `WorldItemSimulation` |
| `shimmered`、`shimmerTime`、`beingGrabbed`、`onConveyor` | `WorldItem` | public fields | 运动、合并、Shimmer、玩家接纳和投影 | Shimmer/运动/拾取路径 | 环境接触、移动和接纳期间 | 环境查询结果驱动的可变状态 | 权威状态，部分值由环境查询导出 | `WorldItem.cs:25-43,227-255,626-929`；Shimmer `WorldItem.cs:1530-1571` | `confirmed` | `WorldItemSimulation` |
| `UpdateItem(int)`、`MoveInWorld(...)`、`CheckInWorld(...)`、`DespawnIfMeetingConditions(...)` | `WorldItem` | public/private methods | `Main`/世界 Tick、掉落槽位循环 | `WorldItem` 自身以及 `Main.item` 槽位 | 每 Tick；越界/过期时终止 | 旧式直接写入，迁移时须收敛到 system + command | 行为与副作用边界 | `WorldItem.cs:357-558,560-623,626-944` | `confirmed` | `WorldItemSimulation` |
| `TryCombiningIntoNearbyItems(...)` | `WorldItem` | public method | 世界物品 Tick | 两个 `WorldItem` 的 stack/position/velocity、`TurnToAir`、`NetMessage` | 每 Tick 的合并窗口 | 读邻近实体并提交成对变更 | 事务/结构变更 | `WorldItem.cs:227-255` | `confirmed` | `WorldItemSimulation` |
| `Item.NewItem(...)`、`Main.item[]`、`timeItemSlotCannotBeReusedFor[]` | `Item` / `Main` | public static / public static fields | NPC loot、WorldGen、机制结果等生成方 | 生成函数接纳、槽位复用门控、网络广播 | 初始化、生成、槽位回收 | 固定槽位 + slot reuse guard；不能与 entity identity 混为一谈 | 结构身份与生成命令 | `Item.cs:48686-48797`；`Main.cs:934-936,3465-3469` | `confirmed` | `WorldItemSimulation` |
| 玩家接纳、保留和拾取 | `Player` / `WorldItem` | public/private methods | 玩家 Tick、物品空间查询 | 玩家库存/容器事务、WorldItem 销毁或 stack 减少 | reservation → pickup → container commit | 跨 `Player`、`ItemContainerAndEconomy` 和 network 的事务边界 | 跨子系统命令 | `WorldItem.cs:257-347,945-1065`；`Player.cs:19833-19913,19920-19986,22811-22819` | `partial` | `WorldItemSimulation` |

##### 6.3 资格判断与当前映射

它已经显示出独立生命周期、独立权威状态/受控提交、稳定跨域 I/O 和独立 verifier 的候选边界：生成和槽位接纳不等于库存创建；保留/拾取需要 player ownership；运动跨越 Spatial/Liquid/Shimmer；合并和销毁必须产生可复制的结构变更。但这些事实还没有解决它与 `SpawnLifecycleAndLoot`、`ItemContainerAndEconomy` 的唯一提交根冲突，所以不能直接等同于已裁决的一级 owner。

当前 NLTX 已有：

```text
src/Items/WorldItemComponent.cs
src/Items/WorldDrops/WorldItemStateComponent.cs
src/Items/WorldDrops/WorldItemReservationComponent.cs
src/WorldStorage/WorldItemSlot.cs
dome/src/Terraria.Dome.Simulation/Items/WorldItemStore.cs
dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItem*.cs
```

这些文件说明该方向已有局部实现和 verifier 入口，但不能证明完整闭环已经完成。尤其需要继续确认：

- `WorldItemStore` 是否是唯一的 runtime/replication identity owner；
- Spawn、Pickup、Stacking、Destroy 是否都进入同一 structural commit root；
- Pickup 的最终 ItemContainer 写入与 WorldItem 销毁是否原子；
- reservation 的唯一写者是否与 Player ownership 一致；
- Shimmer、Liquid、Spatial 查询和网络快照是否只读消费已提交状态。

本轮建议将其记为：

```text
candidate: WorldItemSimulation
decision: strong-candidate
nltxMapping: partial
indexMutation: deferred
```

##### 6.4 Proposed ECS seam

以下是设计草案，不是当前代码中已经存在的类型；每一项都必须保持：

```text
status: proposed
crossSubsystemOwner: integration-review
```

建议的最小边界是：

- `WorldItemInstanceComponent`：世界槽位关联的 Item instance identity、type/prefix/stack 等实例数据；不持有内容定义注册表；
- `WorldItemLifecycleComponent`：active、spawn age、keep/no-grab delay、expiry 状态；
- `WorldItemReservationComponent`：reserved player、ignore owner、reservation timer、enemy pickup delay；
- `WorldItemKinematicsComponent`：position、velocity、wet/conveyor/shimmer/being-grabbed 等世界运动状态；
- `WorldItemSpawnSystem`、`WorldItemMotionSystem`、`WorldItemReservationSystem`、`WorldItemPickupSystem`、`WorldItemStackingSystem`、`WorldItemDestroySystem`：分别拥有状态转换，不互相直接改写对方的 component；
- `CreateWorldItemCommand`、`ReserveWorldItemCommand`、`PickupWorldItemCommand`、`MergeWorldItemsCommand`、`DestroyWorldItemCommand`：显式描述结构变化和跨 owner 事务；
- `ItemContainerCommitAdapter`、`WorldItemReplicationProjection`：分别负责容器提交和网络/section 投影，不能反向成为权威状态。

##### 6.5 调度和 verifier 计划

建议的候选顺序为：

```text
Spawn/Loot intent
→ WorldItem slot admission
→ WorldItem motion/environment query
→ reservation/eligibility query
→ pickup/stack transaction
→ one structural commit root
→ ItemContainer commit
→ replication projection
```

focused verifier 至少应覆盖：槽位复用门控、越界保护、保留转移、同类合并、stack 归零销毁、拾取与容器提交的原子性、Shimmer/液体运动、重复命令和重复复制。当前没有运行这些 verifier，本候选的运行验证状态仍是 `not-run`。

#### 7. 高优先级边界候选：TileEntityRuntime

##### 7.1 为什么不能继续隐藏在单个 TileEntity 类型里

`TileEntity` 不是一个单独的训练假人、物品框或逻辑传感器。Version4 具有统一的 runtime identity、anchor index、update schedule、type registry、placement/removal、world recovery、network/persistence serialization 和 tile validity。单个 `TE*` 类型只能是 capability-specific state，不能成为一级 owner。

| Member | Declaring Type | Visibility | Read By | Written By | Lifecycle | Access Pattern | State Kind | Evidence | Status | Candidate |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `manager`、`UpdateEntities`、`ByID`、`ByPosition`、`TileEntitiesNextID` | `TileEntity` | public static fields | update、lookup、save/load、interaction | `Clear`、`Add`、`Remove`、load recovery | initialize → active → clear/load | dual index by runtime ID and anchor coordinate plus update list | 权威 runtime registry | `TileEntity.cs:13-33,38-79,80-128` | `confirmed` | `TileEntityRuntime` |
| `ID`、`Position`、`type`、`RequiresUpdates` | `TileEntity` | public fields | `TryGet`/`TryGetAt`、serialization、capability handlers | placement/load/type registration | placement → update → removal | runtime identity、anchor、type identity、schedule flag 分离读取 | 实体身份 + 调度元数据 | `TileEntity.cs:25-33,147-170` | `confirmed` | `TileEntityRuntime` |
| `PerformUpdates`、`Add`、`Remove`、`Kill` | `TileEntity` | public static methods | `WorldGen.UpdateWorld`、tile break、network placement | registry and update-list mutation | per Tick / structure change | centralized lifecycle transitions | 行为与结构提交边界 | `TileEntity.cs:48-128`；`WorldGen.cs:59400-59418,54348` | `confirmed` | `TileEntityRuntime` |
| `InitializeAll`、`RegisterAll`、`GenerateInstance`、`CheckValidTile`、`NetPlaceEntity` | `TileEntity` / `TileEntitiesManager` | public methods | startup、network placement、deserialization | manager type table and instance factory | content initialization → network/load use | type definition registry separated from instance registry | 内容注册与实例工厂 | `TileEntity.cs:129-145`；`TileEntitiesManager.cs:6-87` | `confirmed` | `TileEntityRuntime` |
| `Write`、`Read`、`WriteExtraData`、`ReadExtraData` | `TileEntity` | public/static or virtual | `WorldFile` and network serialization | binary readers/writers and capability type | save/load/full state | persistence ID omitted from network form, position/type retained | persistence/network projection boundary | `TileEntity.cs:171-226`；`WorldFile.cs:3385-3444` | `confirmed` | `TileEntityRuntime` |
| save/load clear, duplicate-anchor replacement, world-validity cleanup, `OnWorldLoaded` | `WorldFile` + `TileEntity` | public static methods | world load and recovery | clear/re-number/add/remove/recovery path | world recovery | recovery transaction over both indexes and tile validity | persistence/recovery state transition | `WorldFile.cs:3400-3444` | `confirmed` | `TileEntityRuntime` |

同路径完整参考 `D:\TRbackup\无任何删减通过编译\Terraria.DataStructures\TileEntity.cs` 补充了 Version4 主文件中的 `AssignNewID`、完整 `Place` 和 `BasicOpenCloseInteraction`。这是同路径同类型的成员级补证，必须标记为 `full-reference-supplemented`，不能扩大 Version4 的 967 文件分母。

tModLoader `v2026.07` 的公开页 [class_tile_entity.html](D:/TRbackup/tmodloader-api-docs-stable/class_tile_entity.html) 仅用于交叉确认公开边界：`Update`（锚点 `a483e78bf9c57dcc8d265703a67e91167`）是放置 TileEntity 的周期行为且不在多人客户端执行；`NetSend`/`NetReceive`（锚点 `a85d2692b4774c6140a968ef92accc6df`、`a55d31824ac839a08c69b4c9e7c04c486`）分别位于服务端发送和客户端接收边界，接收端可能用同位置的新实例替换旧实例；`SaveData`（锚点 `a2799ce9901b2c8efdb060491ea78869f`）确认公开持久化扩展点。该文档不证明 Version4 的 `ByID`/`ByPosition` 实现或私有网络格式。

四项边界门槛的逐项判断如下：

| 门槛 | Version4 证据 | 判断 |
| --- | --- | --- |
| 独立生命周期/调度 | `InitializeAll`、`Add`、`Remove`、`Kill`、`Clear`、`PerformUpdates`，以及 WorldFile 清理/重建 | `confirmed` |
| 独立权威状态或受控提交 | runtime ID、anchor position、type registry、update list、合法性检查和实例工厂 | `confirmed` |
| 稳定跨域 I/O | Tile/WorldGen、WorldFile、MessageBuffer/NetMessage、Wiring/Player/NPC/Teleportation 查询 | `confirmed` |
| 独立 verifier | 双索引、重复 anchor、非法 Tile、更新资格、存档恢复和网络替换均可隔离验证 | `designable; not-run` |

这证明的是一个“TileEntity runtime host”边界，而不是把 11 个 `TE*` 类型各自升格为子系统。

##### 7.2 当前 NLTX 与整合裁决

当前同时存在以下相关形状：

```text
src/WorldStorage/TileEntityStore.cs
src/WorldStorage/TileEntityRecord.cs
src/WorldStorage/TileEntityUpdateSchedule.cs
src/WorldInteraction/TileEntities/*
dome/src/Terraria.Dome.Simulation/WorldObjects/Definitions/*
dome/src/Terraria.Dome.Simulation/Snapshots/TileEntityPersistentState.cs
dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs
```

它们说明 runtime、storage、capability definitions 和 snapshot 已分别出现，但仍需解决 `_tileEntities` 与 `_tileEntityStore` 的并存、唯一 runtime/persistent/network ID owner、跨 Tile 原子提交、恢复顺序和 capability-specific update 的归属。

因此本轮结论不是“没有 TileEntity 边界”，而是：

```text
candidate: TileEntityRuntime
decision: boundary-qualified-runtime
nltxMapping: partial / split ownership
indexMutation: deferred pending integration-review
```

本轮推荐的 owner 合同是：`TileEntityRuntime` 拥有 identity/index/schedule/recovery 和 capability 实例生命周期；`WorldStorage` 只拥有 tile/section/persistence substrate；`WorldInteractionAndStructures` 只提交 anchor/tile structural commands；`WiringAndMechanisms` 和 `TeleportationAndTraversal` 只消费特定 capability 的查询/命令，不直接拥有通用 registry。正式索引仍必须等 integration review 解决跨 Tile 原子提交和恢复阶段。

##### 7.3 Proposed seam 与 verifier

设计草案全部标记：

```text
status: proposed
crossSubsystemOwner: integration-review
```

建议拆成 `TileEntityRuntimeIdentityComponent`、`TileEntityAnchorComponent`、`TileEntityKindComponent`、`TileEntityUpdateScheduleComponent` 和 capability-specific components；由 `TileEntityRuntimeSystem` 统一注册、放置、移除、更新和恢复，由 `TileEntityPersistenceAdapter`/`TileEntityNetworkProjection` 处理外部格式。`TileEntityLookupQuery` 只做 ID/anchor 资格查询，不能修改 registry。

verifier 必须覆盖双索引一致性、ID/anchor 分离、重复 anchor、非法 tile、update list 加入/移除、save/load round-trip、network form 不泄漏 persistence ID、clear/recovery 顺序和 capability update 的唯一写者。当前未运行。

#### 8. 机制内部边界挑战：WeightedPressurePlateActivationRuntime

##### 8.1 Version4 的真实边界

压力板不是“检测当前有无 actor”的纯 Query。它保存坐标到 `bool[255]` 玩家占用位的账本，并在从零到一、从一到零、传送、连接/断开和 tile 销毁时触发 edge transition。Wiring 负责 `HitSwitch` 的传播，但不应因此拥有玩家占用账本。

| Member | Declaring Type | Visibility | Read By | Written By | Lifecycle | Access Pattern | State Kind | Evidence | Status | Candidate |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `EntityCreationLock`、`PressurePlatesPressed`、`NeedsFirstUpdate` | `PressurePlateHelper` | public static fields | update、save/load、reset、destroy | `Reset`、`MoveInto`、`MoveAwayFrom`、load | world init/load → occupancy → clear | coordinate → per-player occupancy bitset | 权威 occupancy ledger | `PressurePlateHelper.cs:7-17,19-45`；完整参考 `:127-170` | `confirmed` | `WeightedPressurePlateActivationRuntime` |
| `PlayerLastPosition`、`pressurePlateBounds` | `PressurePlateHelper` | private static fields | `UpdatePlayerPosition` | movement/teleport update | per player movement | previous/current hitbox delta; not a general spatial cache | edge detection working state | `PressurePlateHelper.cs:15-17,57-100` | `confirmed` | `WeightedPressurePlateActivationRuntime` |
| `UpdatePlayerPosition`、`ResetPlayer`、`DestroyPlate` | `PressurePlateHelper` | public static methods | `Player` tick/teleport, connect/disconnect, `WorldGen` tile break | occupancy map and edge transitions | movement, transfer, destruction | event-driven edge maintenance plus spatial query | lifecycle behavior | `Player.cs:17657,21764,21782`；`WorldGen.cs:54348`；`PressurePlateHelper.cs:46-112` | `confirmed` | `WeightedPressurePlateActivationRuntime` |
| `MoveInto`、`MoveAwayFrom`、`UpdatePlatePosition` | `PressurePlateHelper` | private methods; complete in same-path reference | position delta logic | per-player bool slot, map add/remove | zero→one and one→zero transitions | edge-triggered occupancy mutation | authoritative transition state | `无任何删减通过编译/Terraria.GameContent/PressurePlateHelper.cs:113-170` | `full-reference-supplemented` | `WeightedPressurePlateActivationRuntime` |
| `Update`、`PokeLocation` | `PressurePlateHelper` | public/private methods | `Main` Tick、Wiring、network | clear map, `Wiring.blockPlayerTeleportationForOneIteration`, `Wiring.HitSwitch`, packet 59 | deferred first update and activation | occupancy edge → mechanism command/projection | cross-subsystem command boundary | `PressurePlateHelper.cs:19-33`；完整参考 `:172-182`；`Main.cs:11459` | `confirmed` | `WeightedPressurePlateActivationRuntime` |
| `SaveWeightedPressurePlates`、`LoadWeightedPressurePlates` | `WorldFile` | public static methods | world save/load | serialized coordinates, `Reset`, `NeedsFirstUpdate` and empty bitsets | persistence recovery | snapshot stores active coordinates, then recomputes occupancy | persistence projection + recovery input | `WorldFile.cs:3446-3474` | `confirmed` | `WeightedPressurePlateActivationRuntime` |

完整参考补证的 `MoveInto`/`MoveAwayFrom`/`PokeLocation` 只补充 Version4 同路径桩方法的行为，不能把完整参考独有文件计入 Version4 分母。

##### 8.2 当前 NLTX 与 integration review

当前 NLTX 有：

```text
src/WorldInteraction/PressurePlates/PressurePlateOccupancyComponent.cs
dome/src/Terraria.Dome.Simulation/Wiring/Systems/PressurePlateDetectionSystem.cs
dome/src/Terraria.Dome.Simulation/Wiring/Components/PressurePlateComponent.cs
dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs
```

`PressurePlateOccupancyComponent` 已有 `_occupantsByPlate` 和 `NeedsFirstUpdate`，但当前未发现可确认的写入 API；`PressurePlateDetectionSystem` 当前按 actor 位置做 level detection，并不维护 Version4 的每玩家 enter/leave edge。`DomeSimulation.AdvanceWiring` 已在 Wiring 相关阶段产生 activation candidate，但尚未证明它拥有 occupancy、Wiring 传播和 structural commit 的唯一顺序。也未发现已闭合的 WLD pressure-plate save/load restore。

因此：

```text
candidate: WeightedPressurePlateActivationRuntime
decision: boundary-challenge
nltxMapping: partial / edge semantics missing
indexMutation: deferred pending integration-review
```

建议的 ownership contract 是：

```text
WeightedPressurePlateActivationRuntime owns:
  previous/current player occupancy
  enter/leave edge transition
  reset on disconnect/teleport/tile destruction

WiringAndMechanisms owns:
  switch hit
  propagation queue
  logic-gate/actuator/pump effects

WorldStorage/PersistenceAndRecovery owns:
  tile validity and coordinate snapshot transport

RuntimeComposition owns:
  phase ordering and one structural commit root
```

##### 8.3 Proposed seam 与 verifier

设计草案均标记：

```text
status: proposed
crossSubsystemOwner: integration-review
```

建议 `PressurePlateOccupancyComponent` 只持有 plate coordinate → occupant entity references 以及 first-update flag；`PressurePlateOccupancySystem` 读取 movement/teleport/connect/disconnect 事实并提交 enter/leave commands；`PressurePlateEdgeQuery` 只做纯资格判断；`PressurePlateActivationAdapter` 把 edge event 转换给 Wiring，而不直接改 Tile 或玩家位置；`PressurePlatePersistenceProjection` 只读写恢复格式。

候选顺序为：

```text
Player movement/teleport/connect/disconnect facts
→ occupancy delta query
→ enter/leave edge command
→ Wiring HitSwitch input
→ Wiring propagation
→ structural commit
→ network projection
```

verifier 必须覆盖多玩家同板、最后一人离开、重复位置帧、传送前后、断线、板被破坏、世界加载 first update、客户端不可写和 Wiring 重复触发抑制。当前未运行。

#### 9. 既有 owner 内部边界：BannerProgressionAndClaim

##### 9.1 权威状态与生命周期

`BannerSystem` 不是只有一个显示图标或单个击杀计数。Version4 在 [BannerSystem.cs](D:/TRbackup/Version4/Terraria.GameContent/BannerSystem.cs:11) 中持有：

| Member | Declaring Type | Visibility | Read By | Written By | Lifecycle | Access Pattern | State Kind | Evidence | Status | Candidate |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `MaxBannerTypes`、`killCount`、`claimableBanners` | `BannerSystem` | public const + private arrays | threshold logic、save/load、network packet、claim consumers | `Clear`、`Load`、`AddKill`、`AddClaimableBanner` | world create/load → kill progression → save/clear | fixed-index ledger keyed by banner id | 权威世界状态 | `BannerSystem.cs:52-67` | `confirmed` | `BannerProgressionAndClaim` |
| `AnyNewClaimableBanners` | `BannerSystem` | public field | claim/UI consumers | `Clear`、`AddClaimableBanner` | threshold crossing → claim handling | derived notification flag | 派生状态 | `BannerSystem.cs:58-65,157-165` | `confirmed` | `BannerProgressionAndClaim` |
| `NPCtoBanner`、`BannerToNPC`、`BannerToItem` | `BannerSystem` | public methods | NPC death、Player/Projectile、Bestiary/content queries | none or definition-side setup | content initialization and each lookup | pure content mapping; not mutable ledger | 内容定义查询 | `BannerSystem.cs:174-238,993-1010`；`CommonEnemyUICollectionInfoProvider.cs:21-46` | `confirmed` | `ContentCatalog` seam |
| `Clear()`、`Save()`、`Load()`、`ValidateWorld()` | `BannerSystem` | public static methods | `WorldFile` and world validation | arrays and binary reader/writer path | world create/load/save/clear | versioned sequential persistence | 持久化生命周期 | `BannerSystem.cs:59-132`；`WorldFile.cs:577,1379,2256` | `confirmed` | `BannerProgressionAndClaim` |
| `AddNPCKillBy()`、`AddKill()`、`AddClaimableBanner()` | `BannerSystem` | public/private static methods | NPC death and threshold branch | kill/claim arrays、flag、network broadcasts | each eligible death and threshold crossing | event-to-ledger transition | 权威进度转换 | `BannerSystem.cs:133-172`；`NPC.cs:66168-66179` | `confirmed` | `BannerProgressionAndClaim` |
| `NetBannersModule` full/kill/claim writers and claim request/response | `BannerSystem.NetBannersModule` | public methods; some only in complete reference | join state、incremental replication、claim path | packet bytes; server claim path changes ledger | full join + incremental update + claim transaction | projection/adapter, not owner of ledger | 网络/库存边界 | `BannerSystem.cs:13-50`；完整参考 `BannerSystem.cs:56-316`；`MessageBuffer.cs:587` | `full-reference-supplemented` | `BannerProgressionAndClaim` |

Version4 的 `NetBannersModule` 已包含完整状态、击杀计数和 claim 数量更新包；同路径完整参考进一步补充了 `MessageType` 的 claim request/response、`Deserialize()`、`RequestBannerClaim()`、服务器扣减和客户端结果提交。因此这里必须写成：

```text
referenceStatus: full-reference-supplemented
```

不能把完整参考中的补充成员伪装成 Version4 当前文件已经具备的完整网络行为。

##### 9.2 真实读者、写者和跨域边界

| 方向 | Version4 证据 | 结论 |
| --- | --- | --- |
| 写入 | `NPC.CountKillForBannersAndDropThem()` → `BannerSystem.AddNPCKillBy()`（`NPC.cs:66168-66179`；死亡路径还见 `NPC.cs:45911`、`:65330`） | NPC 死亡事实是账本输入，但账本自己决定阈值和可领取数量 |
| 读取 | `Player.cs:11977`、`:11998`、`:12010`；`Projectile.cs:10531`、`:12263` | Player 增益和 Projectile 响应读取 banner 映射 |
| 内容/资格查询 | `CommonEnemyUICollectionInfoProvider.cs:21-35`、`:42-46` | Bestiary/内容层读取 banner 所需击杀阈值；不能反向写账本 |
| 持久化 | `WorldFile.cs:577`、`:1379`、`:2256` | 世界校验、保存和加载都直接穿过 BannerSystem |
| 网络注册/同步 | `Terraria.Initializers/NetworkInitializer.cs:24`；`MessageBuffer.cs:587` | 独立 NetModule、完整状态下发和增量广播 |
| 领取结果 | 完整参考 `BannerSystem.cs:268-316` | 服务器扣减 claimable 数量，客户端通过 `FakeCursorItem` 暂存并提交库存/溢出物品 |

因此它满足“独立权威状态、持久化、网络和跨域 I/O”四项大部分条件。但它同时触及两个已有 owner：

1. `WorldProgressionAndUnlocks` 已将“改变权威资格的持久化击杀/发现事实”定义为自身责任；第一轮还明确把“单个击杀计数”列入不拆分项。
2. `ItemContainerAndEconomy` 应持有真实物品库存和交易提交；Banner claim 只能发布经过验证的结果，不能让 `FakeCursorItem` 成为库存 authority。

##### 9.3 裁决与 focused verifier

当前裁决是“保留为 `WorldProgressionAndUnlocks` 内部的 `BannerLedger` 模块”，不是静默新增一级 owner。原因不是证据不足：Banner 已有独立账本、生命周期、持久化和网络边界；原因是其最小语义仍是改变世界资格的持久化击杀进度，而这正是 `WorldProgressionAndUnlocks` 的已冻结职责。正式整合必须保证只有一个写入根：

- 归入 `WorldProgressionAndUnlocks`，新增内部稳定 seam `BannerLedger`；该 seam 独占 `killCount`/`claimableBanners`，ItemContainer 只接收已授权的物品结果。
- 只有未来 integration review 证明 Banner claim 已成为独立于世界 progression 的长期权威事务，才重新评估新增 `BannerProgressionAndClaim`；该替代方案当前不采纳，且必须先把 BannerSystem 从 `SharedRuntimeMechanisms` 移出并明确事件/命令方向。

本节涉及的设计类型统一标记为：

```text
status: proposed
crossSubsystemOwner: integration-review
```

建议在既有 owner 内定义 `BannerLedgerComponent`、`BannerLedgerSystem`、`BannerCatalogQuery`、`BannerPersistenceAdapter` 和 `BannerReplicationProjection`；前两者持有并推进唯一账本，Query 只读内容定义，Adapter/Projection 只处理外部格式。它们都不能把 claim 结果或网络快照反向写成权威状态。上述类型均为 `status: proposed`，不是当前已存在实现。

可独立设计但尚未运行的 focused verifier：

- 击杀数在 `threshold - 1`、`threshold`、`threshold + 1` 的边界只产生一次 claim；
- `Save`/`Load`/`ValidateWorld` 的版本 289 分支和数组长度截断；
- 重复 claim、不足 claim、非法数量和服务器/客户端方向拒绝；
- 完整状态、击杀增量和 claim 增量乱序/重复到达时，客户端投影不反向写权威账本；
- 服务器扣减成功、库存已满和响应失败时，`FakeCursorItem` 的 provisional state 可回滚且不会凭空生成真实物品。

#### 10. 高优先级边界候选：OldOnesArmyEventSimulation

##### 10.1 权威状态与生命周期

`DD2Event` 在 [DD2Event.cs](D:/TRbackup/Version4/Terraria.GameContent.Events/DD2Event.cs:15) 中形成了一个完整的事件运行态，而不是单个通知消息。其权威/运行状态包括：

```text
DownedInvasionT1/T2/T3
LostThisRun / WonThisRun / Ongoing
ArenaHitbox / OngoingDifficulty
_deadGoblinSpots
_crystalsDropping_lastWave / _crystalsDropping_toDrop / _crystalsDropping_alreadyDropped
_timeLeftUntilSpawningBegins
_damageTracker
```

| Member/方法组 | Read By | Written By | Lifecycle / side effect | State kind | Evidence | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `DownedInvasionT1/T2/T3` | `NetMessage`、世界进度/玩家资格路径 | `WinInvasionInternal`、`ResetProgressEntirely`、`Load` | 跨世界会话持久化的难度通关事实 | authoritative persistent progression | `DD2Event.cs:45-49,93-128`；`NetMessage.cs:325-328` | `partial`（胜利实现由完整参考补证） |
| `Ongoing`、`LostThisRun`、`WonThisRun`、`OngoingDifficulty` | `Main`、`NPC`、`Player`、`Projectile`、建造/刷怪资格 | `StartInvasion`、`StopInvasion`、`ReportLoss`、胜利路径 | 一次侵袭 run 的启停、失败和胜利 | authoritative run state | `DD2Event.cs:51-65,175-224,337-348`；`Main.cs:13116` | `confirmed` |
| `ArenaHitbox`、`_arenaHitboxingCooldown` | `Player`、建造阻挡和竞技场判断 | `FindArenaHitbox`、reset | 周期重算场地并限制结构操作 | authoritative boundary + derived cache | `DD2Event.cs:61-63,497-550`；`Player.cs:15585-15587` | `confirmed` |
| `_timeLeftUntilSpawningBegins`、`TimeLeftBetweenWaves`、`EnemySpawningIsOnHold` | `UpdateTime`、NPC gate spawn、wave progression | `StartInvasion`、`CheckProgress`、`ReportLoss`、`SetEnemySpawningOnHold` | 每 Tick 倒计时、波次等待和刷怪门控 | authoritative scheduler state | `DD2Event.cs:75-91,138-173,234-347`；完整参考 `DD2Event.cs:1035-1061` | `full-reference-supplemented` |
| `NPC.waveNumber`、`NPC.waveKills`、`NPC.totalInvasionPoints` 的 DD2 写集 | `CheckProgress`、进度广播、死亡路径 | `StartInvasion`、`CheckProgress`、清理路径 | 击杀积分推进波次和 progress packet | cross-subsystem authoritative event state | `DD2Event.cs:192-194,234-334`；`NPC.cs:64759-64797` | `confirmed` |
| `_deadGoblinSpots`、晶体 drop tracker、`DamageTracker` | 奖励/水晶判定、伤害统计 | `AnnounceGoblinDeath`、`ShouldDropCrystals`、`Start/Stop`、波次路径 | 敌人死亡位置、掉落配额和胜负伤害跟踪 | run-scoped authoritative state | `DD2Event.cs:59-73,194-202,490-495,551-664` | `partial`（难度算法由完整参考补证） |
| `SpawnMonsterFromGate`、`SummonCrystal`、`WipeEntities` | NPC/网络输入和事件清理 | DD2 event command path | 从入口刷怪、水晶召唤、塔/NPC/水晶清理及网络通知 | behavior + cross-domain command boundary | `DD2Event.cs:361-489`；`MessageBuffer.cs:2918-2940,3220-3228` | `confirmed` |

生命周期证据为：`Save()`/`Load()`（`:93-116`）、`ResetProgressEntirely()`（`:118-128`）、`UpdateTime()`（`:138-173`）、`StartInvasion()`（`:175-203`）、`StopInvasion()`（`:205-224`）、`CheckProgress()`（`:234` 起）、`ReportLoss()`（`:337`）、`SpawnMonsterFromGate()`（`:361`）、`SummonCrystal()`（`:379`）、`WipeEntities()`（`:430`）、`FindArenaHitbox()`（`:497`）、`ShouldBlockBuilding()`（`:544`）、`DropMedals()`（`:551`）、`ShouldDropCrystals()`（`:564`）和 `AttemptToSkipWaitTime()`（`:671`）。

Version4 中 `IncludeDamageFor()`、`WinInvasionInternal()`、`FindProperDifficulty()`、`GetInvasionStatus()` 以及若干难度专属 spawn/奖励方法存在空体或桩；完整参考同路径文件补充了这些 Version4 已存在成员。因此该候选的算法证据也只能标记为：

```text
referenceStatus: full-reference-supplemented
```

##### 10.2 真实调用链与冲突 owner

| 方向 | Version4 证据 | 影响 |
| --- | --- | --- |
| 调度 | `Main.cs:13116` 调用 `DD2Event.UpdateTime()` | 每 Tick 的波次等待、停止和进度广播 |
| 启动/网络输入 | `MessageBuffer.cs:2936` 调用 `SummonCrystal()`；`:3222` 调用 `AttemptToSkipWaitTime()` | 客户端请求必须由事件 owner 验证并委派 |
| NPC/生成 | `NPC.cs:41698` 调用 `SpawnMonsterFromGate()`；`:64759`、`:68411` 调用 `CheckProgress()`；`:65723-65725` 记录 goblin death | 事件刷怪和击杀进度穿过 NPC/Spawn 生命周期 |
| 世界/玩家/投射物 | `Player.cs:15585-15587` 更新 arena；`Player.cs:23298`、`:25227`、`:25404`、`:25408` 读取 `Ongoing`；`Projectile.cs:438`、`:15228-15230` 读取事件状态 | 建造、物品使用、投射物和玩家资格受事件态约束 |
| 持久化/网络 | `WorldFile.cs:1412`、`:2337`；`NetMessage.cs:325-327`；`Main.cs:11831-11833` | 世界进度、连接初始化和客户端侵袭状态同步 |
| 清理/奖励 | `WipeEntities()` 清理塔、DD2 敌人和晶体；`DropMedals()` 通过 NPC 掉落 | 同时写 Projectile、NPC、Chest/Item 和 Network |

因此 `DD2Event` 已满足独立运行态、调度、跨域副作用和 focused verifier 四项门槛；但它的唯一提交根尚未独立于：

| 门槛 | 判断 | 依据 |
| --- | --- | --- |
| 独立生命周期/调度 | `confirmed` | `StartInvasion` → wave wait/update → `CheckProgress` → win/loss → `StopInvasion`/`WipeEntities`，并由 `Main.cs:13116` 每 Tick 调用 |
| 独立权威状态或受控提交 | `confirmed` | run 状态、wave/difficulty、arena、damage/drop tracker 和胜负 flags 组成独立状态闭包；实体结果通过明确 spawn/cleanup/drop 入口提交 |
| 稳定跨域 I/O | `confirmed` | 连接 Main、NPC、Player、Projectile、Tile/WorldGen、Chest/Item、WorldFile 和 NetMessage |
| 独立 verifier | `designable; not-run` | 波次边界、胜负幂等、竞技场限制、清场、存档和客户端不可写均可隔离验证 |

- `WorldCalendarAndEventOrchestration` 的日历资格和事件实例边界；
- `SpawnLifecycleAndLoot` 的 NPC/掉落实体提交；
- `NpcAndTownSimulation`、`ProjectileSimulation` 的实体更新和清理；
- `ItemContainerAndEconomy` 的晶体/宝物结果；
- `NetworkSessionAndSectionStreaming` 的状态复制。

##### 10.3 裁决与 focused verifier

当前裁决为 `boundary-qualified`，但正式 owner 仍 deferred：它不是因为“一个事件类”就应被排除，而是因为 Integration Review 尚未定义完整 Old One's Army 模式自己的事件状态根、刷怪/清理命令和结果提交协议。若该协议不能独立于 Calendar、Spawn、NPC、Projectile 和 Item，才退回 `WorldCalendarAndEventOrchestration` 内的稳定 `DD2EventRuntime` seam。第一轮“不拆分单个事件类”的约束见 [WorldCalendarAndEventOrchestration 不拆分清单](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-world-calendar-and-event-orchestration-public-decomposition.md:76)。

本节涉及的设计类型统一标记为：

```text
status: proposed
crossSubsystemOwner: integration-review
```

建议的 seam 是 `OldOnesArmyEventComponent`、`OldOnesArmyWaveSystem`、`OldOnesArmySpawnCommand`、`OldOnesArmyCleanupCommand`、`OldOnesArmyPersistenceAdapter` 和 `OldOnesArmyReplicationProjection`。它们必须通过事件/命令与 Calendar、Spawn、NPC、Projectile、Item 和 Network 交互，不得直接共享这些 owner 的可变字段。

可独立设计但尚未运行的 focused verifier：

- 每 Tick 只推进一次倒计时，波次边界不会重复生成或重复广播；
- `StartInvasion`、`StopInvasion(win)`、`ReportLoss` 和断线/重入路径幂等；
- NPC 击杀在波次阈值前后只提交一次进度和正确奖励；
- 水晶召唤、跳过等待和建造阻挡分别验证权限、竞技场范围和事件阶段；
- `WipeEntities` 不遗留 DD2 NPC、塔、晶体或过期网络投影；
- 存档加载、客户端完整状态和增量进度同步不会让客户端反向成为权威写者。

#### 11. 候选资格矩阵

下表把“证据存在”和“owner 已裁决”分开记录。`可设计` 不是“已实现”或“已运行”。

| Candidate ID | 源码证据 | 初始化/注册路径 | 真实读者/写者 | 生命周期/更新频率 | 权威状态 | 派生/缓存 | 持久化边界 | 网络边界 | 失败/取消/重试/回滚 | 现有 owner 重叠 | focused verifier | evidenceStatus | 最终建议 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `WiringAndMechanisms` | `Wiring.cs:151,267,395,464,625,666` | `Wiring.Initialize()`；WorldGen/Network/TE 触发 | Wiring、WorldGen、Liquid、NPC、Item、Projectile、Minecart | Tick 更新 + 触发传播 | 队列、逻辑门集合、泵工作集、机械冷却 | 去重/传播阶段集合 | 当前未形成独立存档协议 | 机制命令、同步结果 | 冷却、循环、失败传播需验证 | WorldInteraction、Liquid、Mount、Spawn | 可设计 | `full-reference-supplemented` | 已确认新增一级 owner |
| `MountAndVehicleSimulation` | `Mount.cs:645,2595,2650,3089,4097,4721`；`Minecart.cs:566,1185,1276` | `Mount.Initialize()`；Player/轨道/Wiring 触发 | Player、Spatial、Projectile、Tile、Wiring、Network | attach/dismount + Tick/轨道更新 | 挂载关系、移动模式、能力冷却、车辆状态 | frame、轨道方向、表现状态 | 需与 Player/WorldStorage 恢复协议对齐 | Player/vehicle projection | attach 失败、解除幂等、速度唯一提交 | PlayerGameplay、Spatial、Wiring、Projectile | 可设计 | `confirmed`（Minecart owner 仍有索引冲突） | 已确认新增一级 owner；修正索引前不可声称同步 |
| `WorldItemSimulation` | `WorldItem.cs` 的生成、拾取、堆叠和销毁路径 | Main/Spawn/Item 入口 | Spawn、Player、ItemContainer、Network | 实体 Tick + transfer | WorldItem slot、位置、堆叠、生命周期 | pickup range、stack candidate | WorldItem 是否持久化需裁决 | item/entity replication | slot 冲突、拾取失败、重复 transfer | SpawnLifecycle、ItemContainer、WorldStorage | 可设计 | `confirmed` 但 owner 未闭合 | 保留 strong-candidate，不直接新增 |
| `BannerProgressionAndClaim` | `BannerSystem.cs:11-171`；完整参考 `:56-316` | `NetworkInitializer.cs:24`；NPC 死亡/WorldFile | NPC、Player、Projectile、Bestiary、WorldFile、Inventory | 击杀事件 + claim 请求 | `killCount`、`claimableBanners` | `AnyNewClaimableBanners`、内容映射 | `Save/Load/ValidateWorld` | full state、kill/claim 增量、claim request/response | 非法数量、不足 claim、客户端 provisional 回滚 | WorldProgression、ItemContainer、SharedRuntime | 可设计 | `full-reference-supplemented` | 归入 `WorldProgressionAndUnlocks.BannerLedger`；不新增一级 owner |
| `OldOnesArmyEventSimulation` | `DD2Event.cs:45-75,93-224,234-438,497-679` | `Main.UpdateTime`；Network message 113（召唤水晶）/143（跳过等待） | Main、NPC、Player、Projectile、Chest/Item、WorldFile、NetMessage | 每 Tick + 波次/请求 | `Ongoing`、wave、difficulty、胜负、arena、drop tracker | Arena hitbox、进度/掉落比例 | `Save/Load` 仅部分进度 | invasion progress、world flags、request result | loss/win、skip、重入、清场幂等 | Calendar、Spawn、NPC、Projectile、Item、Network | 可设计 | `full-reference-supplemented` | boundary-qualified；正式 owner deferred pending integration-review |
| `TileEntityRuntime` | 当前报告已核对的身份、坐标索引、更新队列、恢复路径 | WorldStorage/WorldGen/Network 注册 | Tile、Wiring、WorldFile、结构交互 | Tick/section load/update queue | TileEntity identity、位置、存在性 | 索引/更新队列 | WorldFile/section restore | TileEntity sync | 重建、坐标冲突、section 卸载 | WorldStorage、WorldInteraction、Wiring | 可设计 | `partial` | boundary-qualified-runtime；正式 owner deferred pending integration-review |
| `WeightedPressurePlateActivationRuntime` | `PressurePlateHelper`/Collision/Wiring 触发路径 | Wiring/碰撞入口 | Player/Spatial/Wiring/Tile | 接触 edge + 机制传播 | pressed/occupancy 状态 | edge 去重/按权重结果 | `WorldFile` 保存压力板坐标；占用位在加载后重建 | 通过机制结果同步 | 离开、重复触发、并发占用 | Spatial、Wiring、WorldInteraction | 可设计 | `partial` | Wiring 内部 seam 优先 |

#### 12. 反向审查：不新增的候选

第一轮各子系统的“不拆分”清单已经排除了大量容易误报的候选。例如：

- CombatAndStatus 明确排除单个伤害数字、单个 Buff、单个 NPC 命中、单个 Hook 和 `DeathPenalty` marker，见 [CombatAndStatus 不拆分清单](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-combat-and-status-public-decomposition.md:77)；
- ItemContainerAndEconomy 明确排除单个 Item 字段、Recipe、掉落规则、Chest、金币实体和商店 UI，见 [ItemContainerAndEconomy 不拆分清单](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-item-container-and-economy-public-decomposition.md:78)；
- WorldInteractionAndStructures 明确排除单个压力板、逻辑门、TileEntity、网络消息、Wiring Hook 和单个 Tile 写入，见 [WorldInteractionAndStructures 不拆分清单](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-world-interaction-and-structures-public-decomposition.md:77)；
- SpatialSimulation 明确排除单个碰撞轴、单个 AABB 查询、单个 Collider 字段、`LiquidRenderer` 和 `SceneMetrics` 缓存，见 [SpatialSimulation 不拆分清单](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-spatial-simulation-public-decomposition.md:76)。

反向收口如下：

| 候选 | 正确归属 | 不升格原因 |
| --- | --- | --- |
| `Crafting`、`Commerce`、`Shop`、`Currency` | `ItemContainerAndEconomy` | 是物品/容器事务内部的规则、请求、报价或结果提交，不拥有独立权威状态根 |
| `Buff`、`Potion`、生命回复 | `CombatAndStatus`、`PlayerGameplay` | 是状态效果、物品使用或玩家规则阶段；单个 Buff/Potion 不是生命周期 owner |
| `Minecart` | `MountAndVehicleSimulation` | 与 Mount、Player、Spatial 共享速度、碰撞、挂载和轨道不变量 |
| `TileEntityLifecycle`（单个能力类型或局部 Hook） | `WorldStorage`、`WorldInteractionAndStructures`、`WiringAndMechanisms`；通用 runtime 由 `TileEntityRuntime` 候选承接 | 单个能力类型不能代表通用 registry；身份、持久化、结构命令和机制传播仍须经过 `TileEntityRuntime` 的 boundary-qualified-runtime 整合裁决，不能重复创建第二套生命周期根 |
| `Lighting`、`LightMap`、`SceneMetrics`、`BiomeScene` | `ClientPresentationAndTools` 或只读查询 | 主要是客户端派生缓存和环境查询，不拥有服务器权威写集 |
| `Map`、`WorldMap`、`MapUpdateQueue` | `ClientPresentationAndTools` | 是探索状态、地图投影和缓存，不是服务器权威模拟 |
| `Chat`、`Console`、`ChatCommand` | `IntentAndInteraction`、`ExternalBoundaries` | 是外部输入适配，成功后委派给对应权威 writer |
| `Achievement`、`Social`、平台通知 | `WorldProgressionAndUnlocks` 或投影/适配器 | 只有会改变权威资格的发现事实属于 progression；平台通知本身不是状态根 |
| `Ambience`、音乐、云、粒子、天气表现 | `WorldSession` 的事实或 `ClientPresentationAndTools` | 表现数据不能反向成为世界权威状态 |
| `GolfState` | `ClientPresentationAndTools` 或局部玩法机制 | 当前证据显示它是局部状态/工具边界，不满足独立跨域权威提交门槛 |

#### 13. 运行时与边界责任面

虽然下列责任面不是新增玩法模拟子系统，但它们对 ECS 运行时能否闭环非常重要。

##### 13.1 RuntimeComposition

`Main.cs` 负责服务初始化、实体槽位初始化、全局 Tick、世界准备门控和阶段顺序。它的职责应是组合和调度，不应拥有 Player、NPC、Tile 或 Projectile 的玩法状态。

关键证据包括：

- `Terraria/Main.cs:3324-3507`：初始化和实体初始化；
- `Terraria/Main.cs:11193`：`DoUpdate()`；
- `Terraria/Main.cs:11411`：世界 Tick；
- `Terraria/Main.cs:12972`：时间更新。

当前 NLTX 状态：`missing`。主要缺口是没有统一的可执行 Tick 阶段、命令提交根和失败可见性。

##### 13.2 PersistenceAndRecovery

`WorldFile`、`PlayerFileData` 和 `WorldFileData` 共同形成保存、加载、格式验证、临时文件、备份、回滚和恢复边界。

关键证据包括：

- `Terraria.IO/WorldFile.cs:658`：`LoadWorld()`；
- `Terraria.IO/WorldFile.cs:870-933`：新世界保存和世界保存；
- `Terraria.IO/WorldFile.cs:1808` 起：版本化加载；
- `Terraria.IO/WorldFile.cs:2008` 起：Header 验证；
- `Terraria.IO/WorldFile.cs:2570` 起：Tile 加载；
- `Terraria.IO/WorldFile.cs:2779` 起：Chest 加载；
- `Terraria.IO/WorldFile.cs:3400` 起：TileEntity 加载；
- `Terraria.IO/WorldFile.cs:3484` 起：TownManager、Bestiary 和 Creative Powers 加载。

它不应只是 `ExternalBoundaries` 中的一个文件 I/O helper。当前 NLTX 状态：`missing`。

##### 13.3 NetworkSessionAndSectionStreaming

`Netplay`、`RemoteClient`、`RemoteServer`、`MessageBuffer`、`NetMessage` 和 `WorldSections` 共同拥有连接、握手、包摄取、section 可见性、超时、带宽和复制调度生命周期。

关键证据包括：

- `Terraria/Netplay.cs:247`：服务器启动；
- `Terraria/Netplay.cs:307`：服务器循环；
- `Terraria/Netplay.cs:323`：连接客户端更新；
- `Terraria/Netplay.cs:470`：主线程更新；
- `Terraria/MessageBuffer.cs:127`：包数据摄取；
- `Terraria/MessageBuffer.cs:530-630`：section 发送和连接初始化邻域。

低层网络模块是 Adapter 机制，但 session、握手、超时和 section streaming 共同形成独立状态根和 verifier 边界。当前 NLTX 状态：`missing`。

##### 13.4 ContentLifecycleAndRegistration

`NetworkInitializer`、`Main.Initialize_AlmostEverything()`、各种 `Terraria.Initializers` 和 `ContentSamples` 共同形成内容加载、注册、跨内容 setup、finalization、卸载和重建顺序。

关键证据包括：

- `Terraria.Initializers/NetworkInitializer.cs:14-28`；
- `Terraria/Main.cs:3324-3507`；
- `Terraria.ID/ContentSamples.cs`；
- tModLoader `class_mod_system.html:186-191,210` 的 Load/Setup/PostSetupContent 生命周期交叉证据。

单个内容 ID 或单个 initializer 不是子系统，但跨内容注册屏障和失效重建是独立生命周期。当前 NLTX 状态：`missing`。

#### 14. 当前全量 owner 的边界原则

本轮不把 32 个 owner 都表述为玩法系统，而是采用以下三层结构：

```text
权威玩法模拟层
  WorldSession
  WorldCalendarAndEventOrchestration
  WorldProgressionAndTransition
  WorldProgressionAndUnlocks
  SimulationRuleOverrides
  SpatialSimulation
  WorldInteractionAndStructures
  PlayerGameplay
  NpcAndTownSimulation
  ProjectileSimulation
  TeleportationAndTraversal
  FishingAndCatchSimulation
  CombatAndStatus
  ItemContainerAndEconomy
  WorldGenerationAndEcology
  SpawnLifecycleAndLoot
  LiquidSimulation
  DeathPenaltyAndRevenge
  LeashedEntitySimulation
  WiringAndMechanisms
  MountAndVehicleSimulation

运行时基础设施和适配层
  RuntimeComposition
  PersistenceAndRecovery
  NetworkSessionAndSectionStreaming
  ContentLifecycleAndRegistration
  WorldStorage
  ContentCatalog
  IntentAndInteraction
  ExternalBoundaries
  SharedRuntimeMechanisms
  ExternalDependencyOrGenerated

客户端表现和工具链
  ClientPresentationAndTools
```

边界原则：

1. `WorldSession`、`WorldStorage`、`ContentCatalog` 和 `RuntimeComposition` 不合并成一个“全局核心”。
2. `WorldStorage` 持有 Tile、section、container 及持久化快照 substrate；`TileEntityRuntime` 候选拥有运行时 registry、实例索引和 update schedule，二者通过明确的 persistence adapter 连接；玩法系统只能通过受控命令提交结构变化。
3. `ContentCatalog` 持有不可变定义、ID 映射、Recipe 和声明式规则；它不替代 Item 事务或玩法系统。
4. `ExternalBoundaries`、`PersistenceAndRecovery` 和 `NetworkSessionAndSectionStreaming` 不应把外部副作用直接泄漏进核心状态模块。
5. 派生值、网络快照、持久化快照和客户端投影不能反向成为权威状态。
6. `WiringAndMechanisms`、`MountAndVehicleSimulation`、`OldOnesArmyEventSimulation` 和 `TileEntityRuntime` 的最终接入必须由显式 Tick/恢复阶段和唯一提交者约束，而不能依赖文件/目录顺序。

#### 15. 未决问题与后续验证要求

##### 15.1 WiringAndMechanisms

后续需要确认：

- Version4 空实现与完整参考实现的逐成员等价范围；
- 传播队列的去重、顺序和循环终止条件；
- 机械冷却的唯一写者和跨 Tick 语义；
- 逻辑门稳定性和失败/重试行为；
- 泵与 LiquidSimulation 的提交顺序；
- 执行器、Tile、NPC/Item 生成和网络同步是否共享一个 structural commit root；
- NLTX 现有 Wiring verifier 是否覆盖端到端权威闭环，而不仅是局部契约。

##### 15.2 MountAndVehicleSimulation

后续需要确认：

- Mount registry 的初始化和内容生命周期 owner；
- attach/dismount 的幂等性和失败边界；
- Mount、Minecart、Player 和 Spatial 的速度/位置唯一提交者；
- Drill 对 Tile、Projectile 和资源结果的写入顺序；
- 轨道切换与 Wiring 传播的阶段关系；
- ability/cooldown、飞行和疲劳状态的权威持有者；
- 网络快照和客户端表现如何只读消费已提交状态。

##### 15.3 WorldItemSimulation

后续需要确认：

- `Main.item[]` 槽位、WorldItem runtime identity 和 replication identity 的唯一 owner；
- Spawn/Loot、Pickup、Stacking、Destroy 是否共享同一 structural commit root；
- ItemContainer 接纳与 WorldItem 销毁/减堆是否原子；
- reservation 转移、敌人拾取、Shimmer/液体环境和越界保护的行为闭合；
- `WorldItemSimulation` 是否升格，还是保留为 Spawn/ItemContainer 之间的内部 transfer seam。

##### 15.4 BannerProgressionAndClaim

后续需要确认：

- `WorldProgressionAndUnlocks` 的 `BannerLedger` 与独立 `BannerProgressionAndClaim` 的唯一写入根二选一；
- 293 项 ledger、threshold claim 和 NPC death attribution 的事件顺序与幂等性；
- WorldFile 版本分支、full state、kill/claim 增量和 claim request/response 是否使用同一权威快照；
- ItemContainer 只接收已授权的 claim result，客户端 provisional 状态是否可回滚；
- Version4 主文件桩与完整参考补证的成员范围。

##### 15.5 OldOnesArmyEventSimulation

后续需要确认：

- DD2 事件实例/波次状态是否从 `WorldCalendarAndEventOrchestration` 独立出来；
- wave progress、NPC/Projectile spawn、tower/crystal cleanup、medal/drop result 的提交顺序；
- Start/Stop/Win/Loss、skip wait、断线/重入和清场是否幂等；
- arena building guard、请求验证、完整状态和增量进度复制的权威方向；
- 事件专属状态是否有独立 persistence/network identity，且不重复持有 NPC/Item/Projectile 状态。

##### 15.6 TileEntityRuntime 与 WeightedPressurePlateActivationRuntime

后续需要先做 integration review，而不是继续增加组件：

- `TileEntityRuntime` 的 runtime/persistence/network identity 是否唯一，双索引和 update schedule 是否由同一 registry 持有；
- TileEntity capability、WorldStorage substrate、结构命令和 Wiring 之间是否只有一个 structural commit root；
- 压力板 occupancy 的 enter/leave edge 是否由独立 owner 持有，还是作为 Wiring 内部 seam；
- 传送、连接/断开、Tile 销毁和 WLD 恢复是否都能清理 occupancy；
- client packet、persistence snapshot 和 derived query 是否永远不能反向写 authority。

##### 15.7 运行时闭环

继续实现前，优先建立：

```text
RuntimeComposition 的显式 Tick phases
→ 一个统一 structural commit root
→ 玩家输入
→ 移动/车辆
→ 战斗
→ 死亡
→ 复制
```

这比继续增加更多被动组件或更多子系统名称更能验证当前边界是否真正可运行。

#### 16. 补充候选最终收口：Banner、Creative Research 与入侵波次

本节用于把继续反向扫描后形成的最终边界判断写入报告。它不改变前文对
`WiringAndMechanisms`、`MountAndVehicleSimulation`、`WorldItemSimulation`、
`TileEntityRuntime` 和 `OldOnesArmyEventSimulation` 的证据分级，也不自动修改
`Version4子系统索引.json` 或 `Version4源码覆盖.tsv`。

##### 16.1 当前正式 owner 数量仍为 32

当前机器可读索引仍包含 32 个一级责任面。相对于第一轮固定的 19 个游戏模拟子系统，
已经确认并进入当前索引的新增权威玩法 owner 是：

```text
WiringAndMechanisms
MountAndVehicleSimulation
```

`OldOnesArmyEventSimulation` 和 `TileEntityRuntime` 已达到四项边界门槛，分别属于
`boundary-qualified` 和 `boundary-qualified-runtime`，但仍需要 Integration Review
裁决唯一提交根、恢复顺序以及与既有 owner 的依赖方向；它们不应在本报告中被误写成已经
注册的第 33、34 个正式 owner。

继续扫描得到的候选分级如下：

| 候选 | 当前级别 | 推荐归属/下一步 | 是否新增正式一级 owner |
| --- | --- | --- | --- |
| `BannerProgressionAndClaim` | `boundary-challenge` | 先归入 `WorldProgressionAndUnlocks.BannerLedger`，由 `CombatAndStatus` 只读消费战斗修正，`ItemContainerAndEconomy` 接收已授权领取结果 | 否 |
| `CreativeResearchAndItemUnlocks` | `boundary-challenge / historical candidate` | 与 `SimulationRuleOverrides` 的 Creative Power 分离；先裁决玩家研究账本的 Player/World scope 和唯一持久化根 | 否 |
| `InvasionAndWaveSimulation` / `OldOnesArmyEventSimulation` | `boundary-qualified` | 普通入侵、DD2 永久进度、单次运行和波次状态先由 Calendar/Progression/Spawn/Combat 共同整合；DD2 可单独进入 Integration Review | 暂不新增 |
| `TileEntityRuntime` | `boundary-qualified-runtime` | 由运行时 registry、实例索引、更新调度和恢复协议形成候选；与 `WorldStorage` 的 substrate 分离 | 暂不新增 |
| `WorldItemSimulation` | `strong-candidate` | 先冻结 WorldItem slot、拾取事务和 structural commit root | 暂不新增 |

因此，本轮不能把“发现了更多具有独立生命周期的边界”简化为“正式子系统数量已经
增加”。正式数量、候选数量和细化 seam 必须分别记录。

##### 16.2 BannerProgressionAndClaim 的最终判断

`BannerSystem` 具有真实的跨域权威账本，不应继续被语义上当作普通共享工具：

- [BannerSystem.cs](D:/TRbackup/Version4/Terraria.GameContent/BannerSystem.cs:11) 持有
  `killCount`、`claimableBanners` 和 `AnyNewClaimableBanners`，见 `:52-58`；
- `Clear`、`Save`、`Load`、`ValidateWorld` 形成世界生命周期，见 `:59-132`；
- `AddNPCKillBy`、`AddKill`、`AddClaimableBanner` 形成击杀到阈值领取的提交链，见
  `:133-172`；
- [NPC.cs](D:/TRbackup/Version4/Terraria/NPC.cs:66168) 的
  `CountKillForBannersAndDropThem` 是击杀事实入口；
- [Player.cs](D:/TRbackup/Version4/Terraria/Player.cs:11960) 和 `:11994-12012`
  读取 Banner 映射并把它转换为战斗修正；
- [WorldFile.cs](D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:577)、`:1379`、`:2256`
  分别参与校验、保存和加载；
- [MessageBuffer.cs](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:587) 在玩家加入时
  发送完整状态，`BannerSystem.cs:26-46` 生成击杀和 claim 数量增量；
- [WorldGen.cs](D:/TRbackup/Version4/Terraria/WorldGen.cs:6410) 在世界清理时重置状态。

这已经满足独立生命周期、权威状态、跨域 I/O 和 focused verifier 四个判断。但是，
当前 Version4 的 `NetBannersModule.Deserialize` 在
[BannerSystem.cs:48-50](D:/TRbackup/Version4/Terraria.GameContent/BannerSystem.cs:48)
是空体/默认返回。完整同路径参考才补充了 full state、kill/claim 增量、claim request
和 claim response 的处理，见
[完整参考 BannerSystem.cs](D:/TRbackup/无任何删减通过编译/Terraria.GameContent/BannerSystem.cs:79)。

当前采用的 owner 判断是：

```text
WorldProgressionAndUnlocks
└── BannerLedger
    ├── BannerLedgerComponent
    ├── BannerLedgerSystem
    ├── BannerCatalogQuery
    ├── BannerPersistenceAdapter
    └── BannerReplicationProjection

CombatAndStatus
└── BannerCombatModifierQuery

ItemContainerAndEconomy
└── AuthorizedBannerClaimResult
```

这不是否认 Banner 的独立边界，而是避免把“持久化击杀进度”“战斗派生修正”“可领取
物品结果”和“网络适配”重新合并成一个巨型 owner。只有当 Integration Review 证明
领取事务已经独立于世界 progression，且具有自己的长期权威提交根，才重新评估新增
`BannerProgressionAndClaim` 一级 owner。

##### 16.3 CreativeResearchAndItemUnlocks 的最终判断

Creative Research 必须与 Creative Power 分离：

```text
Creative Power、权限与世界规则覆写
    → SimulationRuleOverrides

Creative Research、Item Sacrifice 与研究账本
    → CreativeResearchAndItemUnlocks 候选
```

当前 Version4 已显示出研究账本的数据形状，但没有形成行为闭环：

- [CreativeUnlocksTracker.cs](D:/TRbackup/Version4/Terraria.GameContent.Creative/CreativeUnlocksTracker.cs:5)
  声明 `IPersistentPerWorldContent` 和 `IOnPlayerJoining`，持有 `ItemSacrifices`，但
  `Save`、`Load`、`ValidateWorld`、`Reset`、`OnPlayerJoining` 在 `:9-13` 全部为空体；
- [ItemsSacrificedUnlocksTracker.cs](D:/TRbackup/Version4/Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs:8)
  已声明 `_sacrificeCountByItemPersistentId`、`_sacrificesCountByItemIdCache`、
  `_unlockedByTeammate`、`_newlyUnlocked`、`AnyNewUnlocksFromTeammates` 和 `LastEditId`，
  但其持久化、校验、重置和加入同步方法在 `:31-35` 为空体；
- [CreativeItemSacrificesCatalog.cs](D:/TRbackup/Version4/Terraria.GameContent.Creative/CreativeItemSacrificesCatalog.cs:8)
  当前主要完成 `Sacrifices.tsv` 的读取和阈值目录建立，初始化入口为
  [Main.cs:3367](D:/TRbackup/Version4/Terraria/Main.cs:3367)；
- [NetCreativeUnlocksPlayerReportModule.cs](D:/TRbackup/Version4/Terraria.GameContent.NetModules/NetCreativeUnlocksPlayerReportModule.cs:6)
  的 `Deserialize` 在 `:8-10` 为空体；
- [Player.cs:486](D:/TRbackup/Version4/Terraria/Player.cs:486) 持有 `creativeTracker`，
  [Player.cs:26573](D:/TRbackup/Version4/Terraria/Player.cs:26573) 初始化它，但当前 Version4
  没有找到同路径完整参考中的 Player Creative Research 保存/加载调用。

完整同路径参考补充了研究查询、`RegisterItemSacrifice`、保存/加载、`LastEditId`、
玩家保存/加载和网络转发路径，例如：

- [完整 ItemsSacrificedUnlocksTracker.cs:40](D:/TRbackup/无任何删减通过编译/Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs:40)；
- [完整 CreativeUnlocksTracker.cs:9](D:/TRbackup/无任何删减通过编译/Terraria.GameContent.Creative/CreativeUnlocksTracker.cs:9)；
- [完整 Player.cs:55509](D:/TRbackup/无任何删减通过编译/Terraria/Player.cs:55509)；
- [完整 Player.cs:56309](D:/TRbackup/无任何删减通过编译/Terraria/Player.cs:56309)；
- [完整 NetCreativeUnlocksPlayerReportModule.cs:8](D:/TRbackup/无任何删减通过编译/Terraria.GameContent.NetModules/NetCreativeUnlocksPlayerReportModule.cs:8)。

完整参考只能补证 Version4 中已有同路径成员，不能把补证行为写成当前 Version4 已实现。
同时，`CreativeUnlocksTracker` 的 `IPersistentPerWorldContent` 声明与其实际挂在 Player
上的保存范围存在 scope 冲突；完整参考的 `ValidateWorld` 文本还存在同名递归调用疑点，
不能未经核对当作可靠的校验实现。

因此当前结论为：

```text
CreativeResearchAndItemUnlocks = boundary-challenge / historical candidate
```

下一轮只能先冻结以下问题：研究账本究竟是 Player-local、World-shared 还是“玩家本地
持久化 + 队友增量投影”；`PersistentItemId`、Item Net ID、Player ID 和 World ID 不得
混用；研究目录只能属于 `ContentCatalog`，不能成为研究进度 authority。

##### 16.4 InvasionAndWaveSimulation 的最终判断

普通入侵和 DD2 都具有独立运行状态，但第一轮 World Calendar 设计已经将它们拆为不同
生命周期的状态集合，而不是要求每一个事件类都成为一级系统：

```text
WorldCalendarAndEventOrchestration
├── WorldInvasionState
├── Dd2RunState
└── Dd2WaveRuntimeState

WorldProgressionAndUnlocks
├── InvasionHistoryState
└── Dd2PersistentProgressState

SpawnLifecycleAndLoot / NpcAndTownSimulation / CombatAndStatus
└── 生成、死亡、掉落与伤害归因结果
```

普通入侵的状态、启动和完成证据见：

- [Main.cs:1059-1081](D:/TRbackup/Version4/Terraria/Main.cs:1059)；
- [Main.cs:12542](D:/TRbackup/Version4/Terraria/Main.cs:12542)；
- [Main.cs:12631](D:/TRbackup/Version4/Terraria/Main.cs:12631)；
- [Main.cs:13470](D:/TRbackup/Version4/Terraria/Main.cs:13470)。

DD2 的永久进度、单次运行和波次运行态见：

- [DD2Event.cs:45-75](D:/TRbackup/Version4/Terraria.GameContent.Events/DD2Event.cs:45)；
- `Save`/`Load`：`DD2Event.cs:93-116`；
- `UpdateTime`、`StartInvasion`、`StopInvasion`、`CheckProgress`：
  `DD2Event.cs:138-347`；
- 进度投影：[DD2Event.cs:130-136](D:/TRbackup/Version4/Terraria.GameContent.Events/DD2Event.cs:130)；
- 世界标志投影：[NetMessage.cs:325-328](D:/TRbackup/Version4/Terraria/NetMessage.cs:325)。

当前 Version4 仍有 `DamageTracker.IncludeDamageFor`、`WinInvasionInternal`、
`FindProperDifficulty`、`GetInvasionStatus`、难度专属刷怪和部分奖励方法空体或默认返回，
见 `DD2Event.cs:25-27`、`:225`、`:233`、`:349-360`、`:665-670`。因此状态形状和
跨域边界已确认，但行为闭合度仍为 `partial`/`full-reference-supplemented` 的混合状态。

本报告保留：

```text
InvasionAndWaveSimulation / OldOnesArmyEventSimulation = boundary-challenge
```

如果后续 Integration Review 能为 DD2 独立冻结事件实例、波次账本、刷怪/清场命令、
结果提交和网络身份，它可以升格为独立 owner；否则应保留为
`WorldCalendarAndEventOrchestration` 内的稳定 `DD2EventRuntime` seam，不能仅因
`DD2Event.cs` 文件较大就扩大一级系统数量。

##### 16.5 本轮最终声明

```text
当前正式责任面：32

相对第一轮已确认新增：
  WiringAndMechanisms
  MountAndVehicleSimulation

尚未无争议升格的强候选：
  BannerProgressionAndClaim
  CreativeResearchAndItemUnlocks
  InvasionAndWaveSimulation / OldOnesArmyEventSimulation
  TileEntityRuntime
  WorldItemSimulation

不升格：
  AnglerQuestAndDailyTask
  SummoningAndMinionSimulation
  BossDamageTracker / InvasionDamageTracker
  Crafting / Commerce / Door / Map / Golf / Achievement
```

#### 17. 验证状态与变更声明

本报告是只读审查产物：

```text
verificationStatus: not-run
```

本轮没有运行：

- `dotnet restore`；
- `dotnet build`；
- `dotnet test`；
- `dotnet run`；
- 任何会写入 `Build/bin`、`Build/obj` 或测试结果的命令。

本轮没有修改：

- `src/`；
- `Test/`；
- `dome/src/`；
- `D:\TRbackup\Version4`；
- `D:\TRbackup\无任何删减通过编译`；
- `Version4子系统索引.json`；
- `Version4源码覆盖.tsv`；
- 第一轮审查报告；
- 其他已有用户变更。

当前工作树原有的用户修改保持不变。本轮仅修改这一份审查产物：

```text
D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-07-version4-additional-subsystem-boundary-review.md
```

---

### 4.2 Version4 第二轮反向发现子系统补充审查

**报告 ID：** `SECOND-ROUND-REVERSE-SUBSYSTEM-2026-09-07`
**日期：** 2026-09-07
**审查类型：** 只读架构审查、反向子系统发现、责任边界挑战
**输入范围：** `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1` 下的 Markdown、当前子系统索引、Version4 源码、同路径完整参考补证、NLTX `src/` 与 `dome/src/`
**使用方法：** `public-decomposition` 证据协议 + 用户指定的 PUA 检查流程
**verificationStatus：** `not-run`
**变更范围：** 只新增本报告；未修改生产代码、测试、索引、覆盖表或既有审查报告

#### 1. 执行摘要

本轮审查的目标不是把每个类型、协议包或目录都提升为子系统，而是从第一轮 Markdown 已识别的责任面出发，反向检查是否存在满足以下至少三项条件的遗漏边界：

1. 独立生命周期或独立调度阶段；
2. 独立权威状态、唯一写入根或受控提交边界；
3. 跨两个以上既有领域的稳定输入/输出、协议或数据边界；
4. 可以定义独立的 focused verifier。

当前结论如下：

| 候选 | 本轮结论 | 当前 NLTX 状态 | 处理建议 |
| --- | --- | --- | --- |
| `ShimmerFluidAndTransmutation` | 推荐新增一级候选 | `partial` | 先冻结 owner、命令形状、结果提交和 verifier；暂不修改索引 |
| `PlayerSpawnAndRespawn` | 高优先级 `boundary-challenge` | `partial` | 先与 `PlayerGameplay`、`DeathPenaltyAndRevenge`、`TeleportationAndTraversal` 做排他 owner 裁决 |
| `InvasionProgressAndCredit` | 保留 `boundary-challenge` | 局部实现较强 | 先确认普通入侵、DD2、Pumpkin Moon、Frost Moon 是否共享统一 progress commit root |
| `SimulationRuleOverrides` | 不新增；存在状态漂移 | `partial / version-drift` | 更新审查结论时修正状态，但不把协议 DTO 当成执行闭环 |
| `TeamAndPvpRelations` | 不新增 | 分散在现有责任面 | 由 `PlayerGameplay`、`CombatAndStatus`、`IntentAndInteraction` 和网络投影协作 |
| `WorldItemSimulation` | 暂不新增 | 与 `SpawnLifecycleAndLoot`、`ItemContainerAndEconomy` 重叠 | 等 structural commit root 明确后再裁决 |

其中最强的新候选是 `ShimmerFluidAndTransmutation`。它不是 `LiquidSimulation` 的别名：液体系统负责液体事实、流动和合并；Shimmer 转化负责实体接触后的资格判断、计时、转化规则、结果命令、跨域提交和防重复语义。

`PlayerSpawnAndRespawn` 已经具有独立生命周期的迹象，但其权威状态目前仍可合理地归入 `PlayerGameplay`。本报告因此不把它写成已经裁决的新一级 owner，只把它列为需要整合审查的高优先级候选。

本报告中的“推荐新增”“候选”和“boundary-challenge”都不表示迁移完成、行为等价、API 兼容或运行时可用。

#### 2. 范围、基线与证据规则

##### 2.1 覆盖基线

`D:\TRbackup\Version4` 是唯一的完整覆盖基线。`D:\TRbackup\无任何删减通过编译` 只用于补充 Version4 中相同路径、相同类型、相同成员的裁剪实现，不能扩大 Version4 的文件分母。

因此本报告采用以下证据等级：

| 等级 | 含义 |
| --- | --- |
| `version4-confirmed` | Version4 当前文件中可以直接确认成员、调用点或状态边界 |
| `full-reference-supplemented` | Version4 当前成员存在但实现为空/裁剪，同路径完整参考补充了实现；不扩展覆盖分母 |
| `partial` | 只能确认局部状态、入口或调用关系，完整行为/提交链仍缺失 |
| `boundary-challenge` | 具备真实跨域边界，但与既有 owner 存在尚未解决的写入根竞争 |
| `excluded` | 目前属于内部机制、派生查询、表现层、协议机制或已有子系统范围 |

tModLoader 资料只用于交叉确认公开 API、服务端/客户端边界、钩子和生命周期形状；不用于证明 Terraria 私有实现。Space Station 14 只用于参考 Query、System、事件和提交边界的 ECS 组织方式；不用于证明 Terraria 语义。`dome/src/**/LegacyReference` 只属于兼容参考，不能被当成当前目标运行时已经实现。

##### 2.2 既有 32 个责任面

当前 [`docs/migration/ledgers/Version4子系统索引.json`](D:/TRbackup/NLTX/docs/migration/ledgers/Version4子系统索引.json) 重新读取后共有 32 个责任面：

```text
RuntimeComposition
LiquidSimulation
DeathPenaltyAndRevenge
LeashedEntitySimulation
PersistenceAndRecovery
NetworkSessionAndSectionStreaming
ContentLifecycleAndRegistration
WiringAndMechanisms
MountAndVehicleSimulation
WorldSession
WorldCalendarAndEventOrchestration
WorldProgressionAndTransition
WorldProgressionAndUnlocks
SimulationRuleOverrides
WorldStorage
ContentCatalog
IntentAndInteraction
SpatialSimulation
WorldInteractionAndStructures
PlayerGameplay
NpcAndTownSimulation
ProjectileSimulation
TeleportationAndTraversal
FishingAndCatchSimulation
CombatAndStatus
ItemContainerAndEconomy
WorldGenerationAndEcology
SpawnLifecycleAndLoot
ExternalBoundaries
ClientPresentationAndTools
SharedRuntimeMechanisms
ExternalDependencyOrGenerated
```

当前索引中 NLTX 映射统计为：

```text
confirmed=1
partial=16
missing=12
excluded=3
```

这些数字是当前索引的状态快照，不是行为迁移完成度，也不是运行时就绪度。

#### 3. 一级子系统资格复核

##### 3.1 `ShimmerFluidAndTransmutation`

**建议状态：**

```text
candidateStatus: recommended-new-subsystem
nltxStatus: partial
evidenceStatus: full-reference-supplemented / partial
indexStatus: not-modified
```

###### 3.1.1 与 `LiquidSimulation` 的边界

`LiquidSimulation` 负责液体世界事实：

- Tile 中的液体数量和液体类型；
- 液体传播和工作队列；
- 水、岩浆、蜂蜜、Shimmer 的液体交互与合并；
- Tile 写集、脏区和液体网络投影。

`ShimmerFluidAndTransmutation` 负责液体接触之后的业务结果：

- WorldItem 的 Shimmer 接触计时、冷却和完成标记；
- Item → Item、Item → NPC、Item → 金币；
- NPC → NPC、NPC → Item；
- Item 解构、进度锁和月相相关转化；
- 城镇 NPC Shimmer 变体；
- 玩家 Shimmer 状态参与的资格和状态交接；
- 结果堆叠、来源归因、幂等和重复 Tick 防重；
- 向库存、NPC、WorldItem、玩家和网络投影提交结果命令。

因此不能因为 `LiquidContactStateComponent.IsShimmerWet` 存在，就把整个转化链归入 `SpatialSimulation` 或 `LiquidSimulation`。接触只是输入事实，不是转化结果的 owner。

###### 3.1.2 Version4 证据

| 证据面 | 成员和位置 | 证明内容 | 证据级别 |
| --- | --- | --- | --- |
| 液体入口 | [`Terraria/Liquid.cs:1420-1425`](D:/TRbackup/Version4/Terraria/Liquid.cs:1420) | `Liquid.ShimmerCheck` 把 Shimmer 送入液体检查链；它不承担实体转化结果 | `version4-confirmed` |
| WorldItem 状态 | [`Terraria/WorldItem.cs:25-27`](D:/TRbackup/Version4/Terraria/WorldItem.cs:25) | `shimmered` 与 `shimmerTime` 是世界物品的权威状态字段 | `version4-confirmed` |
| WorldItem 生命周期 | [`Terraria/WorldItem.cs:498-513`](D:/TRbackup/Version4/Terraria/WorldItem.cs:498) | Shimmer 接触、状态推进、冷却、重新堆叠和网络相关路径构成独立行为阶段 | `version4-confirmed` |
| WorldItem 裁剪点 | [`Terraria/WorldItem.cs:625`](D:/TRbackup/Version4/Terraria/WorldItem.cs:625) | 当前 Version4 的 `Shimmering()` 为空体，不能单独证明完整算法 | `partial` |
| WorldItem 接触补证 | [`Terraria/WorldItem.cs:848-887`](D:/TRbackup/无任何删减通过编译/Terraria/WorldItem.cs:848) | 同路径完整参考补充接触检测、计时、服务端/客户端条件和阈值触发 | `full-reference-supplemented` |
| WorldItem 结果补证 | [`Terraria/WorldItem.cs:1841-2035`](D:/TRbackup/无任何删减通过编译/Terraria/WorldItem.cs:1841) | 同路径完整参考补充金币转换、物品变体、NPC 生成、解构、堆叠、同步和来源归因 | `full-reference-supplemented` |
| NPC 接触入口 | [`Terraria/NPC.cs:33285-33295`](D:/TRbackup/Version4/Terraria/NPC.cs:33285) | NPC 检查 Shimmer 液体并进入 `GetShimmered()` | `version4-confirmed` |
| NPC 阈值生命周期 | [`Terraria/NPC.cs:77624-77658`](D:/TRbackup/Version4/Terraria/NPC.cs:77624) | NPC Shimmer 透明度推进、阈值触发和转化入口独立于普通 NPC 行为 | `version4-confirmed` |
| NPC 结果补证 | [`Terraria/NPC.cs:93124-93199`](D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs:93124) | 同路径完整参考补充 NPC 变体、NPC → Item、城镇 NPC 状态和网络/表现副作用 | `full-reference-supplemented` |
| Player 状态入口 | [`Terraria/Player.cs:17132-17147`](D:/TRbackup/Version4/Terraria/Player.cs:17132) | 玩家通过 `Collision.shimmer` 进入 Shimmer 状态 | `version4-confirmed` |
| Player 状态消费 | [`Terraria/Player.cs:22208-22218`](D:/TRbackup/Version4/Terraria/Player.cs:22208) | Shimmer 状态参与玩家伤害规避 | `version4-confirmed` |
| Item 规则表 | [`Terraria.ID/ItemID.cs:85-93`](D:/TRbackup/Version4/Terraria.ID/ItemID.cs:85) | Item Shimmer 转化、等价物、解构和进度锁是独立规则数据 | `version4-confirmed` |
| Item 资格补证 | [`Terraria/Item.cs:49209-49254`](D:/TRbackup/无任何删减通过编译/Terraria/Item.cs:49209) | `CanShimmer()`、等价物和进度条件补充了资格查询 | `full-reference-supplemented` |
| Transform 规则补证 | [`Terraria.GameContent/ShimmerTransforms.cs:6-143`](D:/TRbackup/无任何删减通过编译/Terraria.GameContent/ShimmerTransforms.cs:6) | 解构配方、进度锁、Item 转换和月相相关转换 | `full-reference-supplemented` |
| NPC 规则表 | [`Terraria.ID/NPCID.cs:4834-4840`](D:/TRbackup/Version4/Terraria.ID/NPCID.cs:4834) | NPC 免疫、NPC → NPC、NPC → Item 和城镇 NPC 变体表 | `version4-confirmed` |
| WorldItem 网络投影 | [`Terraria/NetMessage.cs:99-105`](D:/TRbackup/Version4/Terraria/NetMessage.cs:99) | WorldItem Shimmer 状态改变同步消息选择 | `version4-confirmed` |
| WorldItem 状态同步 | [`Terraria/NetMessage.cs:638-661`](D:/TRbackup/Version4/Terraria/NetMessage.cs:638) | 同步 `shimmered` 和 `shimmerTime` | `version4-confirmed` |
| NPC 网络投影 | [`Terraria/NetMessage.cs:704-710`](D:/TRbackup/Version4/Terraria/NetMessage.cs:704) | NPC 同步含 Shimmer 透明度标记 | `version4-confirmed` |
| 网络恢复 | [`Terraria/MessageBuffer.cs:1082-1144`](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:1082) | 恢复网络传入的 WorldItem Shimmer 状态 | `version4-confirmed` |
| Shimmer 效果消息 | [`Terraria/MessageBuffer.cs:3230-3235`](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:3230) | 接收 Shimmer 效果消息，说明结果还跨越表现/网络边界 | `version4-confirmed` |

这些证据共同说明：Shimmer 不是“液体类型枚举的一个成员”，而是一条从接触事实到多种实体结果的转化业务链。

###### 3.1.3 当前 NLTX 映射

| 当前路径 | 已有内容 | 不能据此宣称的内容 | 状态 |
| --- | --- | --- | --- |
| [`src/SpatialSimulation/LiquidContactComponent.cs:13-31`](D:/TRbackup/NLTX/src/SpatialSimulation/LiquidContactComponent.cs:13) | `IsShimmerWet`、接触计数和派生湿润属性 | 没有转化计时、转化规则或结果命令 | `partial` |
| [`src/Share/Entity/Components/LiquidContactStateComponent.cs:6-14`](D:/TRbackup/NLTX/src/Share/Entity/Components/LiquidContactStateComponent.cs:6) | 实体侧 Shimmer 接触状态 | 不是 WorldItem/NPC/Player 的转化 authority | `partial` |
| [`src/Share/Entity/Components/LiquidComponent.cs:6-35`](D:/TRbackup/NLTX/src/Share/Entity/Components/LiquidComponent.cs:6) | Shimmer 液体种类和接触派生属性 | 不拥有液体之外的实体结果 | `partial` |
| [`dome/src/Terraria.Dome.Simulation/Liquid/Definitions/LiquidRuleRegistry.cs:62-128`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Liquid/Definitions/LiquidRuleRegistry.cs:62) | Shimmer 定义和液体合并规则 | 不包含 Item/NPC/Player 转化 | `partial` |
| [`dome/src/Terraria.Dome.Simulation/Liquid/Definitions/LiquidInteractionClassifier.cs:23-28`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Liquid/Definitions/LiquidInteractionClassifier.cs:23) | Shimmer 与其他液体的交互分类 | 仍属于液体交互，不是 Shimmer 结果提交 | `partial` |
| [`dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcTownVariantSystem.cs:5-10`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Npc/Systems/NpcTownVariantSystem.cs:5) | Shimmer 城镇 NPC 变体索引判断 | 没有 NPC 转化执行器和状态提交 | `partial` |
| [`dome/src/Terraria.Dome.Protocol.V1456/Npc/NpcSyncPacket.cs:19-25`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Protocol.V1456/Npc/NpcSyncPacket.cs:19) | NPC Shimmer 透明度 DTO | DTO 不是权威状态 writer | `partial` |
| [`src/Content/RecipeDefinition.cs:5-18`](D:/TRbackup/NLTX/src/Content/RecipeDefinition.cs:5) | `CustomShimmerResults` 配方元数据 | 没有 Shimmer 事务消费者 | `partial` |

排除目标运行时符号搜索后，当前非 `LegacyReference` 代码中没有找到完整的：

```text
shimmered
shimmerTime
GetShimmered
CanShimmer
Shimmering
ShimmerTransform
ShimmerActions
SyncItemsWithShimmer
```

`dome/src/Terraria.Dome.Protocol.V1456/LegacyReference` 中存在对应兼容源码或消息恢复资料；这只能作为协议和历史边界证据，不能改写为当前 NLTX 已经拥有这些 authority。

###### 3.1.4 推荐边界

```text
LiquidContactReadView
  -> ShimmerEligibilityQuery
  -> ShimmerTransmutationCommand
  -> Item/NPC/Player/WorldItem result commit
  -> inventory/world-item/NPC/world-state updates
  -> network/persistence/client projection
```

建议 owner 划分：

| 责任 | 推荐 owner |
| --- | --- |
| 液体数量、类型、流动和液体合并 | `LiquidSimulation` |
| 接触后的转化资格、计时、规则选择、幂等 | `ShimmerFluidAndTransmutation` |
| 库存、金币和堆叠事务 | `ItemContainerAndEconomy` |
| NPC/WorldItem 实体分配与生命周期 | `SpawnLifecycleAndLoot` |
| 普通 NPC 行为和城镇生命周期 | `NpcAndTownSimulation` |
| 玩家权威状态写入 | `PlayerGameplay` |
| 复制、恢复和可见性调度 | `NetworkSessionAndSectionStreaming` |
| 粒子、音效、透明度渲染 | `ClientPresentationAndTools` |

###### 3.1.5 最小 focused verifier

至少应覆盖：

- Item → Item 转化表、等价物和进度锁；
- Item → NPC、NPC → Item、NPC → NPC；
- 金币换算和解构结果守恒；
- 同一个 WorldItem 在连续接触 Tick 中不能重复结算；
- `shimmered`、`shimmerTime`、堆叠和来源归因在完成后保持一致；
- 客户端不能直接提交转化结果；
- 网络恢复不能把已转化物品再次触发；
- NPC 免疫和城镇 NPC 变体条件不绕过普通 NPC 生命周期；
- 玩家 Shimmer 状态只作为已提交接触/资格事实的消费结果，不反向改变 Tile 液体 authority。

##### 3.2 `PlayerSpawnAndRespawn`

**建议状态：**

```text
candidateStatus: strong-boundary-challenge
nltxStatus: partial
evidenceStatus: version4-confirmed / full-reference-supplemented
indexStatus: not-modified
```

###### 3.2.1 Version4 证据

| 证据面 | 成员和位置 | 证明内容 | 证据级别 |
| --- | --- | --- | --- |
| 出生入口 | [`Terraria/Player.cs:21798-21908`](D:/TRbackup/Version4/Terraria/Player.cs:21798) | `Spawn(PlayerSpawnContext)` 清理死亡/表现状态，恢复生命，设置 `dead=false`，选择位置，清零速度并恢复免疫 | `version4-confirmed` |
| 世界出生点 | [`Terraria/Player.cs:21910-21960`](D:/TRbackup/Version4/Terraria/Player.cs:21910) | 世界出生点和一般出生点的安全位置查询 | `version4-confirmed` |
| 出生区域资格 | [`Terraria/Player.cs:21977-22072`](D:/TRbackup/Version4/Terraria/Player.cs:21977) | 地面、液体、特殊地牢和危险区域检查 | `version4-confirmed` |
| 位置提交点 | [`Terraria/Player.cs:22075-22081`](D:/TRbackup/Version4/Terraria/Player.cs:22075) | 出生位置最终写入；团队出生点方法在当前 Version4 中为空体 | `partial` |
| 死亡结算 | [`Terraria/Player.cs:22572-22690`](D:/TRbackup/Version4/Terraria/Player.cs:22572) | 死亡统计、死亡位置/时间、掉落、墓碑、`dead=true`、`respawnTimer`、网络和聊天副作用 | `version4-confirmed` |
| 死亡 Tick | [`Terraria/Player.cs:9948-10072`](D:/TRbackup/Version4/Terraria/Player.cs:9948) | `deadTime` 和 `respawnTimer` 递进/递减 | `version4-confirmed` |
| 网络恢复 | [`Terraria/MessageBuffer.cs:604-620`](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:604) | 恢复出生点、重生计时、死亡统计、队伍并调用 `Spawn` | `version4-confirmed` |
| 进入世界 | [`Terraria/MessageBuffer.cs:1920-1924`](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:1920) | 连接成功后调用 `Spawn(SpawningIntoWorld)` | `version4-confirmed` |
| 网络投影 | [`Terraria/NetMessage.cs:427-438`](D:/TRbackup/Version4/Terraria/NetMessage.cs:427) | 投影出生坐标、重生计时、死亡统计和队伍 | `version4-confirmed` |
| 团队出生点宿主 | [`Terraria.GameContent/ExtraSpawnPointManager.cs:10-177`](D:/TRbackup/Version4/Terraria.GameContent/ExtraSpawnPointManager.cs:10) | 团队出生点生成、准备、清理、存档和网络写入 | `version4-confirmed` |
| 团队出生点补证 | [`Terraria.GameContent/ExtraSpawnPointManager.cs:99-435`](D:/TRbackup/无任何删减通过编译/Terraria.GameContent/ExtraSpawnPointManager.cs:99) | 完整参考补充团队出生点生成、随机选择和回退策略 | `full-reference-supplemented` |
| 个人出生点补证 | [`Terraria/Player.cs:38118-38131`](D:/TRbackup/无任何删减通过编译/Terraria/Player.cs:38118) | 个人出生点、团队出生点、世界出生点的选择顺序 | `full-reference-supplemented` |
| 床点恢复补证 | [`Terraria/Player.cs:55203-55240`](D:/TRbackup/无任何删减通过编译/Terraria/Player.cs:55203) | 个人出生点查找和删除 | `full-reference-supplemented` |

###### 3.2.2 当前 NLTX 状态

NLTX 已经具有局部执行链：

- [`src/Player/PlayerSpawnPointComponent.cs`](D:/TRbackup/NLTX/src/Player/PlayerSpawnPointComponent.cs) 有个人出生点和 `SpawnX/SpawnY` 兼容字段；
- [`src/Player/PlayerLifecycleComponent.cs`](D:/TRbackup/NLTX/src/Player/PlayerLifecycleComponent.cs) 有死亡、重生计时、死亡位置、死亡时间和统计状态；
- [`dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerDeathSystem.cs:7-35`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerDeathSystem.cs:7) 有死亡资格和重生计时提交；
- [`dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerRespawnSystem.cs:7-42`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerRespawnSystem.cs:7) 有位置、速度、生命、生命周期和掉落状态清理；
- [`dome/src/Terraria.Dome.Simulation/Players/PlayerLifecycleSystem.cs:9-48`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Players/PlayerLifecycleSystem.cs:9) 有死亡 Tick、倒计时和重生命令排队；
- [`dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:4393-4408`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:4393) 有 `QueueRespawnPlayer`；
- [`dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:5906-5936`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:5906) 有重生命令提交、出生系统调用、坐骑清理和复活事件；
- [`dome/src/Terraria.Dome.Server/Replication/WorldSectionReplication.cs:31-38`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Server/Replication/WorldSectionReplication.cs:31) 有请求坐标或默认世界出生坐标选择；
- [`dome/src/Terraria.Dome.Protocol.V1456/Packets/PlayerSpawnPacket.cs:3-11`](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Protocol.V1456/Packets/PlayerSpawnPacket.cs:3) 有出生坐标、重生计时、死亡统计、队伍和 SpawnContext。

仍未闭合的部分包括：

- 个人床点完整解析和持久化回放；
- 团队出生点完整策略；
- 世界出生点回退顺序；
- 出生危险区域检查；
- 失败回退和重试语义；
- `SpawningIntoWorld`、`ReviveFromDeath`、`TeamSwap` 等上下文的完整区别；
- 存档、网络恢复和重生提交之间的一致回放契约。

###### 3.2.3 owner 排他决策

升格为独立一级子系统前，必须解决以下问题：

1. `PlayerGameplay` 是否拥有玩家死亡到重生的完整生命周期？
2. `TeleportationAndTraversal` 是否只拥有一般位置迁移，出生位置解析是否属于另一个 Query/策略层？
3. `SpawnLifecycleAndLoot` 是否明确只负责 NPC、Projectile、WorldItem 的生成和清理，而不拥有 Player respawn？
4. `DeathPenaltyAndRevenge` 产生的 `RespawnCommand` 是否只是复仇标记的外部输入，还是会与玩家自然重生竞争写入根？

建议先采用以下候选链进行 integration review：

```text
PlayerGameplay
  -> death fact
  -> PlayerSpawnAndRespawn candidate
  -> spawn eligibility / landing query
  -> RespawnPlayerCommand
  -> player spatial/lifecycle commit
  -> replication and persistence projection
```

如果最终由 `PlayerGameplay` 独占玩家生命周期，并将出生点解析、空间验证和结构提交拆成清晰的 Query/Command，则 `PlayerSpawnAndRespawn` 应作为 `PlayerGameplay` 的内部阶段，不必新增索引项。只有当它拥有独立的阶段契约、写入根、恢复身份和 verifier 时，才应升格为一级责任面。

##### 3.3 `InvasionProgressAndCredit`

**结论：保留 `boundary-challenge`，暂不新增。**

Version4 的证据很强：

- [`Terraria/Main.cs:12542-12692`](D:/TRbackup/Version4/Terraria/Main.cs:12542) 处理普通入侵启动、方向、延迟和规模；
- [`Terraria/Main.cs:11807-11846`](D:/TRbackup/Version4/Terraria/Main.cs:11807) 处理普通入侵、Pumpkin Moon、Frost Moon 和 DD2 的网络进度投影；
- [`Terraria.GameContent.Events/DD2Event.cs:15-204`](D:/TRbackup/Version4/Terraria.GameContent.Events/DD2Event.cs:15) 有 DD2 状态、波次、冷却、伤害追踪、保存和同步；
- [`Terraria.GameContent/NPCDamageTracker.cs:144-255`](D:/TRbackup/Version4/Terraria.GameContent/NPCDamageTracker.cs:144) 有伤害归属 tracker 生命周期；
- [`Terraria/NPC.cs:64759-64798`](D:/TRbackup/Version4/Terraria/NPC.cs:64759) 有普通入侵击杀规模减少和同步；
- [`Terraria/NPC.cs:65069-65090`](D:/TRbackup/Version4/Terraria/NPC.cs:65069) 与 [`Terraria/NPC.cs:65198-65219`](D:/TRbackup/Version4/Terraria/NPC.cs:65198) 有 Frost Moon、Pumpkin Moon 波次积分；
- [`Terraria.IO/WorldFile.cs:1338-1379`](D:/TRbackup/Version4/Terraria.IO/WorldFile.cs:1338) 等位置有事件持久化和恢复。

NLTX 也已经具有：

- `WorldInvasionProgressCommand`；
- `WorldInvasionStartCommand`；
- `WorldInvasionTransition`；
- `WorldProgressionSystem`；
- `WorldInvasionSizeSystem`；
- `WorldInvasionStartEligibilitySystem`；
- `WorldInvasionTravelSystem`；
- `WorldInvasionProgressProjectionSystem`；
- `NpcCheckDeadInvasionProgressPolicy`；
- `DomeSimulation` 中的入侵阶段、进度提交、完成事件、Transition 和请求清理。

但当前尚未证明普通入侵、DD2、Pumpkin Moon、Frost Moon 共享一个统一的 progress commit root。应先裁决：

```text
方案 A：入侵进度属于 WorldCalendarAndEventOrchestration 的内部策略；
方案 B：建立独立 InvasionProgressAndCredit 提交根。
```

在统一写入根、事件身份、积分归因和 focused verifier 未冻结之前，不新增一级索引项。

##### 3.4 `SimulationRuleOverrides` 的状态漂移

**结论：不新增；当前应由 `missing` 修正为 `partial / version-drift`。**

第一轮任务文档仍写着当前状态为 `missing`，并称只有 Creative Presentation 元数据；见：

- [`2026-09-05-version4-simulation-rule-overrides-public-decomposition.md:15`](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-simulation-rule-overrides-public-decomposition.md:15)
- [`2026-09-05-version4-simulation-rule-overrides-public-decomposition.md:52`](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-simulation-rule-overrides-public-decomposition.md:52)

但当前 checkout 已经存在：

- `src/SimulationRuleOverrides/RuleOverrideStateComponent.cs`；
- `src/SimulationRuleOverrides/RuleOverrideSnapshotComponent.cs`；
- `src/SimulationRuleOverrides/PlayerRuleOverrideStateComponent.cs`；
- `src/SimulationRuleOverrides/PlayerRuleOverrideSnapshotComponent.cs`；
- `src/SimulationRuleOverrides/RuleOverridePermissionStateComponent.cs`；
- `src/SimulationRuleOverrides/PowerPermissionLevel.cs`；
- `src/SimulationRuleOverrides/RuleKey.cs`；
- `CreativePowerModulePacket`、`CreativePowerPermissionModulePacket`、`JourneySpawnRatePacket`；
- `TerrariaSession` 中的 Creative Power、权限和 Journey spawn-rate 解码及玩家归属检查。

这些内容证明已经有部分数据形状和协议输入，但仍缺少：

- 权限授权执行器；
- 统一 mutation commit；
- 世界/玩家状态的实际应用闭环；
- 保存/加载和网络重放 verifier；
- 客户端请求与权威快照之间的版本和失败语义。

因此正确表述是：

```text
SimulationRuleOverrides: partial / version-drift
```

协议 DTO、组件构造器和 JSON 自洽性都不能代替真实 authority。

#### 4. 其他边界挑战与排除项

##### 4.1 保留为 `boundary-challenge`

| 候选 | 建议归属 | 暂不升格原因 |
| --- | --- | --- |
| `TeamAndPvpRelations` | `PlayerGameplay` + `CombatAndStatus` + `IntentAndInteraction` + 网络投影 | 状态量小，尚未证明独立写入根；涉及关系资格而非独立长期生命周期 |
| `PlayerRestAndStacking` | `PlayerGameplay` 内部阶段 | 坐下、睡眠、堆叠和输入中断仍与玩家能力、交互和空间状态强耦合 |
| `WorldItemSimulation` | `SpawnLifecycleAndLoot` 与 `ItemContainerAndEconomy` 的 transfer seam | WorldItem 具有 Tick、拾取、堆叠、销毁和复制，但 structural commit root 未独立裁决 |
| `CraftingAndRecipeTransactions` | `ItemContainerAndEconomy` | Recipe catalog 和 crafting queue 不能单独证明独立 authority |
| `NpcInteractionAndCommerce` | `NpcAndTownSimulation` + `ItemContainerAndEconomy` | 交互资格、商店和库存事务跨域，但没有独立生命周期根 |
| `BuffAndPotionState` | `PlayerGameplay` + `CombatAndStatus` | 状态变换与能力/战斗写集重叠，边界尚未形成独立提交链 |

##### 4.2 明确排除

以下方向当前不形成新的一级责任面：

- `BestiaryProgressPersistence`：由 `WorldProgressionAndUnlocks` 覆盖；
- `WorldEventClockAndScheduler`：由 `WorldCalendarAndEventOrchestration` 覆盖；
- `SecretSeedRuleSet`：属于 `WorldSession`、`SimulationRuleOverrides` 和 `WorldGenerationAndEcology` 的规则源；
- `InventoryTransferTransactions`：属于 `ItemContainerAndEconomy` 的内部事务；
- `DoorTraversalAndTileMutation`：属于 `WorldInteractionAndStructures`；
- `TilePlacementAndFraming`：属于 `WorldInteractionAndStructures` / `WorldStorage`；
- `TileEntityRuntime`：当前仍属于 `WorldStorage` / `WorldInteractionAndStructures` 的运行时交接，除非统一 registry、持久化和网络身份形成独立提交根；
- `WorldEcologyAndSpread`：属于 `WorldGenerationAndEcology` 内部生成/生态管线；
- `TeamBasedSpawnPointNetwork`：并入 `PlayerSpawnAndRespawn` 候选的策略和投影；
- `GolfSimulation`：当前偏客户端派生状态和 Projectile 专用规则；
- `Lighting`：当前证据更接近客户端派生光场、缓存和 Adapter；
- `AchievementProgressPersistence`：主要是本地、平台和云端 Adapter，只有参与权威资格的事实才进入 `WorldProgressionAndUnlocks`；
- `PauseAndShutdown`：属于 `RuntimeComposition` 生命周期；
- `WorldDeletionAndCloudMigration`：属于 `PersistenceAndRecovery`；
- `AmbientEntitySpawnSystem`：尚未证明独立权威实体写入根；
- `PressurePlateAndAnchors`：属于 `WiringAndMechanisms`、`WorldInteractionAndStructures` 和 `SpatialSimulation` 的交接；
- `RandomnessAndAttribution`：属于 `SharedRuntimeMechanisms` 的来源协议；
- `DungeonStructurePlacementSystem`：更像 `WorldGenerationAndEcology` 内部生成管线；
- `DontStarveDarknessDamage`：真实伤害消费链未闭合；
- `SceneMetrics`：派生环境查询/缓存，不是世界权威状态根；
- `CinematicManager`、`ParticleOrchestrator`、`Animation`、`PopupText`：表现层或短生命周期表现机制；
- 单个 Creative Power、单个权限字段、单个网络消息、单个 TileEntity、单个 AI style：粒度过细。

#### 5. 推荐的 owner 关系

当前最小可执行的责任图如下：

```text
LiquidSimulation
  └─ 液体事实、传播、液体合并、液体接触只读视图
       └─ ShimmerFluidAndTransmutation
            ├─ 转化资格、计时、规则选择、幂等
            ├─ Item/NPC/Player/WorldItem result command
            └─ 交接给 ItemContainer、SpawnLifecycle、PlayerGameplay、NPC 和网络投影

PlayerGameplay
  ├─ 玩家死亡事实和基础生命周期
  └─ PlayerSpawnAndRespawn candidate
       ├─ 出生资格和落点 Query
       ├─ RespawnPlayerCommand
       └─ 玩家空间/生命周期提交与复制投影

WorldCalendarAndEventOrchestration
  └─ 普通入侵、DD2、Pumpkin Moon、Frost Moon 的事件事实
       └─ InvasionProgressAndCredit boundary challenge
            └─ 等待统一 progress commit root 和归因 verifier
```

关键约束：

- Query 只能读取已提交的权威事实，不能借查询名义写入另一个子系统；
- Component 只保存明确 owner 的状态形状，不能因为字段名称相同就复制 authority；
- DTO、网络消息和兼容读取器不是权威写者；
- 派生属性不是持久化或网络 authority；
- `dome` 的局部执行链不能替代尚未闭合的跨域 structural commit root；
- 目录结构和文件顺序不能定义运行时执行顺序。

#### 6. 证据缺口与阻塞决策

##### 6.1 `ShimmerFluidAndTransmutation`

| ID | 阻塞问题 | 影响 |
| --- | --- | --- |
| `BD-SHIMMER-01` | 转化 command 是否由 Shimmer 子系统直接输出，还是先进入统一 Item/NPC transaction bus | 决定跨域提交原子性 |
| `BD-SHIMMER-02` | `shimmered`、`shimmerTime` 的最终 authority 属于 WorldItem 组件还是转化运行时状态 | 决定网络恢复和幂等边界 |
| `BD-SHIMMER-03` | Item → NPC 的实体分配是否通过 `SpawnLifecycleAndLoot` 的通用 SpawnRequest | 防止转化系统直接分配实体 slot |
| `BD-SHIMMER-04` | 玩家 Shimmer 状态是 `PlayerGameplay` 的状态，还是转化域的短期资格快照 | 防止 Player 和 Shimmer 双写 |
| `BD-SHIMMER-05` | 解构结果、金币结果和普通库存接纳是否共享 ItemContainer 的原子提交 | 防止部分成功和重复奖励 |
| `BD-SHIMMER-06` | 客户端 Shimmer 效果消息与服务器结果消息的因果关系 | 防止表现先于 authority 或被重放 |

##### 6.2 `PlayerSpawnAndRespawn`

| ID | 阻塞问题 | 影响 |
| --- | --- | --- |
| `BD-SPAWN-01` | PlayerGameplay 与 PlayerSpawnAndRespawn 的 owner 排他 | 决定是否新增一级索引项 |
| `BD-SPAWN-02` | DeathPenaltyAndRevenge 的 respawn 输入与自然重生的竞争关系 | 决定重生 command 是否唯一 |
| `BD-SPAWN-03` | TeleportationAndTraversal 是否拥有出生位置写入 | 防止一般迁移与重生提交双写位置 |
| `BD-SPAWN-04` | 个人、团队、世界出生点优先级及失败回退是否冻结 | 决定 Query 的确定性和回放能力 |
| `BD-SPAWN-05` | SpawnContext 是否进入持久化/网络身份，还是仅作为一次性 command metadata | 决定恢复和 verifier 形状 |

##### 6.3 `SimulationRuleOverrides`

| ID | 阻塞问题 | 影响 |
| --- | --- | --- |
| `BD-RULE-01` | Creative Power 权限数据的唯一授权写者 | DTO 解码不能直接写组件 |
| `BD-RULE-02` | 世界覆盖和玩家覆盖的版本/失效关系 | 决定 Tick 快照一致性 |
| `BD-RULE-03` | 保存/加载、网络重放和实际模拟消费是否共用同一快照 | 决定 `partial` 能否继续收敛 |

#### 7. 推荐下一步

只建议优先做一个动作：

> 冻结 `ShimmerFluidAndTransmutation` 的 owner 决策、Command 形状、结果类型、幂等键和 focused verifier 清单；暂时不要先写实现。

最小决策包应明确：

```text
LiquidSimulation
  owns liquid facts and fluid reactions

ShimmerFluidAndTransmutation
  owns contact-to-transformation rule selection and idempotency

ItemContainerAndEconomy
  owns inventory/coin/stack commit

SpawnLifecycleAndLoot
  owns spawned NPC/WorldItem allocation

NpcAndTownSimulation
  owns ordinary NPC lifecycle

PlayerGameplay
  owns player-state commit

NetworkSessionAndSectionStreaming
  owns replication/recovery projection
```

完成后再做 `PlayerSpawnAndRespawn` 的 owner 排他裁决。这样可以避免同时把 Shimmer、玩家重生、库存、NPC 生成和普通液体逻辑重新塞回一个宽泛的 `PlayerGameplay` 或 `LiquidSimulation`。

#### 8. 验证状态与变更声明

本报告是只读审查产物：

```text
verificationStatus: not-run
```

本轮没有运行：

- `dotnet restore`；
- `dotnet build`；
- `dotnet test`；
- `dotnet run`；
- focused verifier；
- 任何 compile-capable 命令。

本轮没有修改：

- `src/`；
- `Test/`；
- `dome/src/`；
- `D:\TRbackup\Version4`；
- `D:\TRbackup\无任何删减通过编译`；
- `docs/migration/ledgers/Version4子系统索引.json`；
- `docs/migration/ledgers/Version4源码覆盖.tsv`；
- `docs/component-decomposition/review-round-1/` 下既有报告。

已有的同日第二轮报告 `2026-09-07-version4-additional-subsystem-boundary-review.md` 未覆盖、未重写；本报告作为独立补充产物新增。

本报告不表示：

- Version4 到 NLTX 的迁移已经完成；
- 运行时行为已经等价；
- 协议/API 已经兼容；
- 组件数量等于迁移覆盖；
- 索引自洽或扫描器通过就等于运行时就绪。

---

### 4.3 Version4 第二轮反向发现补充审查：掉落、事务、交易与进度账本

**日期：** 2026-09-07  
**审查类型：** 第一轮 Markdown 复读后的反向发现、边界挑战与 owner 整合输入  
**输入范围：** `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1` 下的 Markdown、当前 Version4/NLTX 索引、Version4 源码、同路径完整参考补证、现有 `src/`/`dome/src/` 状态  
**方法：** `public-decomposition`；按用户指定的 `pua` 规则主动检查遗漏、上下游影响、证据闭合和伪完成风险  
**verificationStatus：** `not-run`  
**正式索引变更：** `deferred`  
**文档状态：** `final-readonly-follow-up`

> 本文是现有 `docs/component-decomposition/review-round-2/2026-09-07-version4-additional-subsystem-boundary-review.md` 的补充交接，不覆盖、不改写那份关于 `WiringAndMechanisms`、`MountAndVehicleSimulation`、`TileEntityRuntime` 等边界的报告。本文专门记录在继续反向阅读第一轮材料后确认的另一组候选：世界掉落实体生命周期、制作事务、交易结算、Banner 进度领取和 Bestiary 知识账本。
>
> 本文不是迁移完成证明、行为等价证明、API 兼容证明或当前 NLTX 已实现能力证明。所有新增名称均为 `boundary-challenge`、`conditional boundary-challenge` 或 `internal strong boundary`，除非最终整合明确裁决，否则不得直接写入正式索引。

#### 1. 结论先行

##### 1.1 最可信的新增边界挑战

本轮最值得交给最终整合会话的五个候选是：

| 候选 ID | 建议层级 | 核心理由 | 当前裁决 |
| --- | --- | --- | --- |
| `WorldItemLifecycleAndPickup` | `boundary-challenge / high priority` | 世界掉落实体有独立槽位生命周期、运动/环境状态、reservation、合并、拾取和销毁；跨接 Spawn、Player、ItemContainer、Network | 优先处理；先锁定与 `SpawnLifecycleAndLoot`、`ItemContainerAndEconomy` 的唯一 structural commit root |
| `CraftingAndRecipeTransactions` | `boundary-challenge / high priority` | 配方静态定义之外存在资格检查、材料 reservation、远程请求、服务端重验、消费、产物和 refund 的完整事务链 | 可作为 `ItemContainerAndEconomy` 内部稳定边界，或经整合后成为独立事务 owner；不能让两者同时写 reservation/consume/result |
| `CommerceAndTrading` | `boundary-challenge / high priority` | 商店库存、价格快照、NPC mood、货币扣款/找零、购买/出售/退款、receipt 和幂等性构成独立结算边界 | 倾向独立事务边界；`ContentCatalog` 只拥有静态 offer/currency 定义，`ItemContainerAndEconomy` 只接收最终物品提交 |
| `BannerProgressionAndClaiming` | `boundary-challenge / high priority` | 长期 kill ledger、claimable 数量、阈值、存档、网络增量和 claim result 闭合成独立进度账本 | 建议先落在 `WorldProgressionAndUnlocks.BannerLedger` seam；不立即增加一级 owner |
| `BestiaryKnowledgeLedger` | `internal strong boundary / conditional boundary-challenge` | kill/sight/chat 三类知识状态聚合、持久化、玩家加入同步和 NPC 资格读取具有稳定语义 | 先作为 `WorldProgressionAndUnlocks` 内部强边界；最终整合可决定是否升格 |

##### 1.2 条件性候选

`CreativeResearchAndPermissions` 必须拆成两个不同问题：

1. Creative Power 对世界规则的 override 已经属于 `SimulationRuleOverrides`，不应重复建立新的世界规则 owner；
2. research/sacrifice unlock ledger 具有独立的玩家/世界持久化和网络上报边界，只有这部分具备条件性 `boundary-challenge` 资格。

在没有明确 player/world scope、唯一持久化 owner、权限提交和 Inventory 交接之前，不把它升为一级正式子系统。

##### 1.3 暂不升格，但必须保留为内部稳定 seam

以下边界在源码中有可辨认的生命周期或状态，但目前更适合挂在已有 owner 内部：

| 候选 | 暂定归属 | 不升格原因 |
| --- | --- | --- |
| `InvasionProgressAndCredit` | `WorldCalendarAndEventOrchestration` / `WorldProgressionAndTransition` | 当前 `WorldProgressionSystem` 同时推进事件、入侵和 Slime Rain；另立 owner 会出现重复写入根 |
| `PlayerSpawnAndRespawn` | `PlayerGameplay` + `SpawnLifecycleAndLoot` 交接 | 需要明确实体重建、出生点、死亡结果和网络身份的结构提交顺序 |
| `TeamSpawnPointNetwork` | `PlayerSpawnAndRespawn` 内部边界 | 主要是 spawn 资格/网络投影，不足以单独持有玩家生命周期 |
| `LogicSensorRuntime` | `WorldInteractionAndStructures` + `WiringAndMechanisms` + `WorldStorage` | 读取结构/线路状态并产出触发事实，通用 runtime 与结构存储仍是外部依赖 |
| `QuickStackAndEmergencyTransfer` | `ItemContainerAndEconomy` 内两个事务 seam | emergency transfer 与普通 pickup 的 ownership 规则不同，不能粗暴合并，也不应重复成为一级 owner |
| `DoorTraversalAndTileMutation` | `WorldInteractionAndStructures` | 门的 traversal 与 Tile mutation 共享结构提交根 |
| `PlayerRestAndAnchors` | `PlayerGameplay` + `WorldInteractionAndStructures` | 玩家休息资格与世界锚点/结构事实跨域，但没有独立长期状态根 |
| `DungeonGenerationAndProtection` | `WorldGenerationAndEcology` | 是生成阶段和结构保护能力，不是独立持续 Tick owner |
| `NPCInteractionAndServices` | `IntentAndInteraction` → `NpcAndTownSimulation` | 请求入口和 NPC 服务实现分层，不应另造第三套服务状态根 |
| `TileEntityRuntime` | `WorldStorage`/`WorldInteractionAndStructures` 的基础 runtime seam | 现有报告已单独提出；正式 owner 仍需 integration review 解决 registry、持久化和结构提交的唯一根 |

##### 1.4 明确排除

以下名称不能因为存在一个类、一个数组、一个消息号或一个 UI 页面就升格为子系统：

`Lighting`、`WorldMap`/`MapUpdateQueue`、`GolfState`、`Achievement`/`Social`、`AmbientSpawn`、单个 `Recipe`、`RecipeGroup`、商店 UI、货币类型、NPC 类型、AI style、Hook、消息号、单个 `TileEntity`、Pylon、Banner item。

它们分别属于派生查询/客户端投影、已有事务 owner、内容定义、外部适配器或某个更大生命周期的内部策略。

#### 2. 范围、证据与状态语义

##### 2.1 第一轮约束仍然有效

第一轮公共协议把审查固定为 19 个游戏模拟子系统；单个子系统报告发现的相邻边界只能交给最终整合，不能自行增加、改名、合并或拆分固定清单。

本轮因此不修改固定清单，不修改 `Version4子系统索引.json`，不修改 `Version4源码覆盖.tsv`。本文只产生候选、边界证据、冲突 owner 和 focused verifier 计划。

相关文件：

- [第一轮公共审查协议](D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-05-version4-public-decomposition-common-protocol.md)
- [现有第二轮边界审查](D:/TRbackup/NLTX/docs/component-decomposition/review-round-2/2026-09-07-version4-additional-subsystem-boundary-review.md)
- [当前 Version4 子系统索引](D:/TRbackup/NLTX/docs/migration/ledgers/Version4子系统索引.json)
- [当前 Version4 覆盖 TSV](D:/TRbackup/NLTX/docs/migration/ledgers/Version4源码覆盖.tsv)

##### 2.2 证据优先级

本轮使用以下顺序：

```text
D:/TRbackup/Version4
→ D:/TRbackup/无任何删减通过编译（只补 Version4 同路径同类型缺口）
→ D:/TRbackup/tmodloader-api-docs-stable（只交叉确认公开边界）
→ D:/TRbackup/NLTX 当前 src/、Test/、dome/src/ 和既有报告
```

`D:\TRbackup\Version4` 仍然是唯一完整覆盖基线。完整参考只允许补充 Version4 已存在文件中的空方法、删减成员或邻近调用链，不能扩大覆盖分母。源码存在不等于 NLTX 已迁移，提案类型不等于已实现类型。

##### 2.3 状态枚举

本文使用以下含义：

| 标记 | 含义 |
| --- | --- |
| `confirmed` | 当前源码中已确认成员、调用者、写者或生命周期事实 |
| `partial` | 已确认边界，但有空实现、删减实现、跨域写者或恢复路径缺口 |
| `unresolved` | 存在两个以上合理 owner，当前证据不足以安全裁决 |
| `proposed` | 仅是 ECS 拆分设计，不表示当前路径或类型存在 |
| `deferred` | 暂不升格，等待最终整合或先挂在已有 owner seam |
| `not-run` | 本轮没有执行 build/test/restore/run 或独立 verifier |

#### 3. 当前分母和索引语义

当前索引仍有 32 个责任面，其中 21 个为 `authoritative-simulation`，其余是 runtime infrastructure、adapter、client presentation 或排除项。本文不改变这些数字。

特别需要在最终整合时修正或至少解释以下语义冲突：

| 文件/责任面 | 当前索引语义 | 本轮发现 | 风险 |
| --- | --- | --- | --- |
| `Terraria/WorldItem.cs` | `SharedRuntimeMechanisms` | 实际包含独立掉落实体生命周期和跨域拾取事务 | 会把权威世界实体误归为通用工具，掩盖 reservation/stack commit |
| `Terraria.GameContent/BannerSystem.cs` | `SharedRuntimeMechanisms` | 实际包含长期 kill ledger、claimable 数量、持久化和 claim 网络 | 会丢失 WorldProgression 的唯一写入根 |
| `Terraria.GameContent.Bestiary/BestiaryUnlocksTracker.cs` | `SharedRuntimeMechanisms` | 实际聚合 kill/sight/chat 的持久化知识账本 | 与 JSON 的 `WorldProgressionAndUnlocks` 语义 owner 冲突 |
| `Terraria.GameContent/CraftingRequests.cs` | 当前多半由 `ItemContainerAndEconomy` 解释 | 存在远程请求、pending、服务端重验、消费、响应和 refund | 若只看 Recipe 定义，会漏掉运行时事务 |
| `Terraria.GameContent.UI/CustomCurrencySystem.cs` | shared/UI 侧 | 完整参考显示货币扣款、合并、找零和回滚协议 | Commerce 与 ItemContainer 的写者会重复 |

索引、TSV、现有报告之间的差异必须由最终整合统一收敛，不能通过本报告文本假装已经同步。

#### 4. 候选一：WorldItemLifecycleAndPickup

##### 4.1 为什么这是最高优先级

`WorldItem` 不是一个单纯的 `Item` 包装类，也不是只负责掉落生成的辅助对象。它拥有从槽位创建到世界运动、reservation、合并、环境销毁、玩家拾取和网络投影的完整运行时生命周期。

当前最危险的误拆方式是：

```text
SpawnLifecycleAndLoot 直接创建 stack
Player 直接写 Inventory
ItemContainer 直接删 WorldItem
Network decoder 直接回写 WorldItem
EmergencyStacking 再维护一套 ownership
```

这种结构会让同一份 stack 同时拥有多个写者，且把 `Main.item[]` 的兼容槽位误当成持久实体身份。必须先确定一次 structural commit，再决定是否将它升格为一级 owner。

##### 4.2 Version4 事实和成员盘点

| 成员/路径 | 读取者与写者 | 生命周期/副作用 | 状态分类 | 证据状态 |
| --- | --- | --- | --- | --- |
| `WorldItem : Entity`、`inner` Item payload | `WorldItem`、`Player`、`NPC`、`Item`、网络路径 | 生成至销毁；承载物品实例与世界实体关系 | 权威实体状态 + Item 实例引用 | `confirmed` |
| `ownTime`、`playerIndexTheItemIsReservedFor`、`noGrabDelay`、`keepTime` | `FindOwner`、`UpdateItem`、玩家接纳/拾取 | 生成、保留、释放、接纳；涉及玩家竞争 | reservation/lifecycle 权威状态 | `confirmed` |
| `shimmered`、`shimmerTime`、`beingGrabbed`、`onConveyor` | 运动、液体、Shimmer、拾取和投影路径 | 环境接触、移动、接纳和销毁 | 运动/环境权威状态，部分值来自查询 | `confirmed` |
| `TryCombiningIntoNearbyItems(int)` | 世界物品 Tick | 直接修改两个 WorldItem stack，清空 donor，发送网络消息 | 成对结构事务 | `confirmed` |
| `FindOwner()` | `Main` 的 reservation 重算和玩家有效性路径 | 更新 reservation、释放超时或失效玩家 | 所有权重算行为 | `confirmed` |
| `UpdateItem(int)` | `Main` 每帧更新 400 个槽位 | 重力、液体、Shimmer、合并、敌人拾取、越界、过期和环境销毁 | 独立实体 Tick | `confirmed` |
| `Item.NewItem(...)` | NPC loot、WorldGen、机制结果等生成方 | 选择槽位、初始化位置/速度/stack，并发布网络结果 | 生成 Command/兼容槽位入口 | `confirmed` |
| `Player.GrabItems`、`PickupItem`、`PullItem_ToVoidVault` | Player Tick、磁力、Void Vault | reservation → pickup → container commit；当前部分方法为空 | 跨 Player/ItemContainer 事务 | `partial` |

主要 Version4 证据：

- `D:\TRbackup\Version4\Terraria\WorldItem.cs:15-47`：类型、payload、reservation、age、Shimmer 和 conveyor 状态；
- `WorldItem.cs:227-255`：附近掉落合并、stack 直接修改、donor 清空和网络发布；
- `WorldItem.cs:257-348`：玩家扫描、容量、距离、磁力、hopper 和 reservation；
- `WorldItem.cs:357-625`：独立 Tick、运动、液体、Shimmer、合并、敌人拾取、越界和自然过期；
- `WorldItem.cs:1557-1571`：section 内 WorldItem 同步路径；
- `D:\TRbackup\Version4\Terraria\Main.cs:934-936`：401 个 WorldItem 槽位及复用保护数组；
- `Main.cs:11553-11570`：每帧更新 400 个 WorldItem；
- `Main.cs:12757-12781`：reservation 重算、超时、玩家失效和 `EmergencyStacking.ProcessPendingTransfers`；
- `D:\TRbackup\Version4\Terraria\Item.cs:48686-48797`：`Item.NewItem` 的槽位选择、初始化、位置、速度和网络发布；
- `D:\TRbackup\Version4\Terraria\Player.cs:19851-19920`：`GrabItems`、容量、磁力和 Void Vault；`PickupItem`、`PullItem_ToVoidVault` 当前为空，但调用链真实存在。

##### 4.3 当前 NLTX 状态

当前已找到与该边界相关的代码，但尚未证明写入闭合：

- `dome/src/Terraria.Dome.Simulation/Items/WorldItemStore.cs`；
- `dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemSpawnSystem.cs`；
- `dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemStackingSystem.cs`；
- `dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemPickupSystem.cs`；
- `dome/src/Terraria.Dome.Simulation/Items/SimulationCommandQueue.cs`；
- `src/Items/WorldDrops/WorldItemStateComponent.cs`；
- `src/Items/WorldDrops/WorldItemReservationComponent.cs`。

当前结论是 `partial`，而不是“已实现 WorldItem 子系统”。仍缺少可确认的单一 commit root、reservation 与背包交接的原子性、槽位复用保护与实体身份分离。

##### 4.4 proposed owner 与调用方向

以下均为 `status: proposed`：

```text
SpawnLifecycleAndLoot
  └─ WorldItemSpawnCommand
       ↓
WorldItemLifecycleAndPickup
  ├─ lifetime / motion / environment
  ├─ reservation / ownership reevaluation
  ├─ merge / despawn
  └─ pickup intent/result
       ↓
ItemContainerAndEconomy
  └─ final inventory/container commit
       ↓
NetworkSessionAndSectionStreaming
  └─ committed section/entity projection
```

建议的最小状态/行为 seam：

- `WorldItemInstanceComponent`：Item instance identity、type、prefix、stack 和 max stack；不持有 ContentCatalog 定义；
- `WorldItemLifecycleComponent`：active、spawn age、keep/no-grab delay、expiry；
- `WorldItemReservationComponent`：reserved player、ignore owner、reservation timer、enemy pickup delay；
- `WorldItemKinematicsComponent`：position、velocity、wet/conveyor/Shimmer/being-grabbed；
- `WorldItemMotionSystem`、`WorldItemReservationSystem`、`WorldItemStackingSystem`、`WorldItemPickupSystem`、`WorldItemDestroySystem`：每个 System 只写自己的 authority 或通过命令提交；
- `CreateWorldItemCommand`、`ReserveWorldItemCommand`、`PickupWorldItemCommand`、`MergeWorldItemsCommand`、`DestroyWorldItemCommand`；
- `ItemContainerCommitAdapter`、`WorldItemReplicationProjection`：只做交接和投影，不能反向成为权威状态。

实体身份必须分开：`EntityUuid` 是运行时身份根，`whoAmI`/`Main.item[]` 槽位是兼容索引，replication identity 是协议投影，存档身份另由持久化 adapter 管理。旧槽位被复用后，失效 command 必须解析失败，不能静默作用于新实体。

##### 4.5 focused verifier 计划

当前未运行。至少需要以下场景：

1. 两个玩家同时竞争一个 WorldItem，只生成一个成功 pickup receipt；
2. reservation 过期、断线或死亡后可以重新分配；
3. merge 不超过 stack limit，且 donor/recipient 只提交一次；
4. pickup 部分成功时 WorldItem 数量、容器数量和 receipt 数量一致；
5. 槽位复用后旧 command 不能命中新实体；
6. lava、Shimmer、越界和自然过期销毁只产生一次 committed destroy；
7. section join 只投影 committed 状态，不把网络快照写回 authority。

#### 5. 候选二：CraftingAndRecipeTransactions

##### 5.1 必须与静态 Recipe 定义分开

`Recipe`、`RecipeGroup`、材料/产物、Tile/液体/生物群系/事件条件和注册索引属于 `ContentCatalog` 的声明式内容。它们不能代表运行时制作事务。

运行时制作至少包含：

```text
recipe eligibility
  → material reservation
  → local/remote branch
  → server revalidation
  → consume
  → result creation
  → inventory/container commit
  → response / refund / timeout
```

##### 5.2 Version4 证据

| 证据 | 事实 | 状态 |
| --- | --- | --- |
| `D:\TRbackup\Version4\Terraria.GameContent\CraftingRequests.cs:9-32` | `RemoteCraftRequest`、`_pendingCrafts`、`HasPendingRequests`，说明存在远程制作请求和 pending 生命周期；当前网络 Deserialize 默认返回，不能把它当成完整实现 | `partial` |
| `D:\TRbackup\Version4\Terraria\Main.cs:11802` | pending crafting 会阻塞本地 inventory actions，制作事务与容器操作存在明确调度依赖 | `confirmed` |
| `D:\TRbackup\Version4\Terraria\Recipe.cs:12-81` | Recipe 持有配方、材料、产物、RecipeGroup、Tile、液体和环境条件 | `confirmed`，但这是静态定义 |
| `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\CraftingRequests.cs:108-405` | 同路径完整参考补充本地制作、远程制作、材料消费、服务端重验、pending、响应、refund 和恢复保存 | `full-reference-supplemented` |

##### 5.3 当前 NLTX 与 owner 冲突

当前 NLTX 已有：

- `src/Items/Crafting/CraftingStateComponent.cs`；
- `src/Items/Crafting/CraftingReservationComponent.cs`；
- `src/Items/Crafting/CraftingMaterialReservation.cs`；
- dome 中与 Crafting 相关的协议包和模拟路径。

但最终必须二选一：

**方案 A：**

```text
ItemContainerAndEconomy
  └─ CraftingAndRecipeTransactions（内部稳定边界）
```

**方案 B：**

```text
CraftingAndRecipeTransactions
  ├─ eligibility / reservation / request / response / idempotency
  └─ 生成已授权的 container commit result

ItemContainerAndEconomy
  └─ actual item/container commit
```

无论选择哪一个，以下状态只能有一个最终写者：`CraftingState`、material reservation、pending request、operation ID、expected revision、consumption result 和 refund result。不能由 `CraftingSystem` 与 `ItemContainerSystem` 各写一份。

##### 5.4 proposed seam 与 verifier

`ContentCatalog` 只提供不可变的 `RecipeDefinition` 和条件 Query；制作事务 owner 负责把输入快照绑定到 operation ID，并在服务端重新验证 revision、材料数量、站点条件和权限。最终由 `ItemContainerAndEconomy` 进行一次可回滚的扣除/产物提交。

focused verifier：

- 本地制作成功、材料不足、环境条件不满足；
- 远程 request 重复抵达时 operation ID 幂等；
- 服务端 revalidation 拒绝客户端过期 revision；
- consume 成功但 result commit 失败时完整 refund；
- pending crafting 阻止冲突的 inventory action，但不会永久锁死玩家；
- 断线、超时、重连后 pending/result 状态只有一个权威结论。

#### 6. 候选三：CommerceAndTrading

##### 6.1 边界不是商店 UI，也不是单一货币类型

建议的责任面是交易结算，而不是把商店界面、NPC 类型或金币 Item 单独升格：

- NPC shop inventory 生成；
- 普通/旅行商店 stock revision；
- price snapshot；
- NPC mood price adjustment；
- ordinary/custom currency；
- purchase/sale/refund；
- receipt、ledger、幂等性和网络确认。

##### 6.2 Version4 证据

| 证据 | 事实 | 状态 |
| --- | --- | --- |
| `D:\TRbackup\Version4\Terraria.GameContent.UI\CustomCurrencySystem.cs:8-50` | 定义 currency definition/settlement API；Version4 中 Count/Combine/Purchase/Accepts/Price 多为空或默认 | `partial` |
| `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.UI\CustomCurrencySystem.cs:30-296` | 同路径完整参考补充余额统计、货币合并、扣款、找零和回滚 | `full-reference-supplemented` |
| `D:\TRbackup\Version4\Terraria\Chest.cs:1135-1221` | 旅行商店生成、幸运值、稀有度、去重和数量；库存不是静态 UI 文本 | `confirmed` |
| `D:\TRbackup\Version4\Terraria\Chest.cs:1222` | 普通 `SetupShop(int type)` 当前为空，普通商店成员需要完整参考/调用链补证 | `partial` |
| `D:\TRbackup\Version4\Terraria.GameContent\ShopHelper.cs:49-66` | 价格上下文存在，`ProcessMood` 当前为空；价格不能只由 UI 本地计算 | `partial` |
| `D:\TRbackup\Version4\Terraria\Main.cs:3336,3426-3431` | 商店和货币初始化处于全局内容/运行时初始化路径 | `confirmed` |

##### 6.3 当前 NLTX 的写集风险

当前已找到：

- `ShopOfferDefinition`；
- `ShopOfferCatalogSystem`；
- `ShopPurchaseSystem`；
- `ShopPurchaseState`；
- `CurrencyBalanceComponent`；
- `CommerceLedgerComponent`；
- `CustomCurrencyDefinitionRegistry`。

尤其是 `dome/src/Terraria.Dome.Simulation/Items/Systems/ShopPurchaseSystem.cs:49-320` 已经直接处理 purchase、货币扫描、找零和 Inventory slot 写入，并由 `ShopPurchaseState` 保存 buy-once 状态。这说明交易逻辑已经形成可识别事务边界，同时也暴露了它与 `ItemContainerAndEconomy` 的写集重叠。

##### 6.4 proposed owner 图

```text
NpcAndTownSimulation
  └─ NPC / mood / shop-access facts

ContentCatalog
  ├─ ShopOfferDefinition
  └─ CurrencyDefinition

CommerceAndTrading
  ├─ price snapshot / stock revision
  ├─ debit / credit / purchase / sale / refund
  └─ receipt / ledger / idempotency
       ↓
ItemContainerAndEconomy
  └─ item representation and final container commit
       ↓
NetworkSessionAndSectionStreaming
  └─ request/response projection
```

必须消除三套货币 authority：

1. logical `CurrencyBalance`；
2. Inventory 中表示金币的 Item；
3. `CustomCurrencySystem` 扫描结果。

它们可以是不同的视图或 adapter，但只能有一个最终 debit/credit 提交者。focused verifier 至少覆盖 stock revision 过期、价格快照篡改、货币不足、custom currency 合并/找零、buy-once 重复请求、purchase/result commit 失败回滚和断线重试。

#### 7. 候选四：BannerProgressionAndClaiming

##### 7.1 不能与 Boss/Invasion damage tracker 合并

Banner kill count 是长期世界进度，claimable banner 是可领取奖励状态；BossDamageTracker 是短期 Boss 贡献账本，InvasionDamageTracker 是事件组临时账本，Banner buff 则是 Player/Combat 的只读派生效果。

把它们统称为 `BossProgressionAndCredit` 会掩盖持久化范围、阈值和 claim 事务的差异。

##### 7.2 Version4 事实

| 证据 | 事实 | 状态 |
| --- | --- | --- |
| `D:\TRbackup\Version4\Terraria.GameContent\BannerSystem.cs:52-58` | `MaxBannerTypes`、`killCount`、`claimableBanners` 是长期账本的核心状态 | `confirmed` |
| `BannerSystem.cs:61-131` | Clear、Save、Load、ValidateWorld 生命周期闭合 | `confirmed` |
| `BannerSystem.cs:133-173` | NPC 类型映射、阈值、claimable 更新和网络更新 | `confirmed` |
| `D:\TRbackup\Version4\Terraria\NPC.cs:65312-65331,66168-66180` | NPC death/loot 路径登记 Banner | `confirmed` |
| `D:\TRbackup\Version4\Terraria\Player.cs:11960-12012,23908-23913` | Banner buff 只读读取，效果消费不应成为 kill ledger 写者 | `confirmed` |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs` | Banner save/load/validate 接入世界存档 | `confirmed` |
| `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\BannerSystem.cs:121-315` | 补充 claim request、server claim、response 和库存 item result | `full-reference-supplemented` |

##### 7.3 proposed owner

```text
CombatAndStatus / NpcAndTownSimulation
  └─ committed NPC death fact
       ↓
BannerProgressionAndClaiming
  ├─ NPC-to-banner mapping
  ├─ kill count / threshold / claimable count
  ├─ claim request / reject / commit
  └─ persistence / replication
       ↓
ItemContainerAndEconomy
  └─ authorized Banner item commit

PlayerGameplay / CombatAndStatus
  └─ read-only Banner buff effect
```

建议先将它实现为 `WorldProgressionAndUnlocks.BannerLedger` 的内部 seam。关键 blocking decision 是：`WorldProgressionAndUnlocks` 与独立 `BannerProgressionAndClaiming` 不能同时写 293 项 ledger、threshold claim 或 claim result。

focused verifier：重复 death fact 不重复计数；阈值跨越只创建正确 claimable 数量；非法 claim、库存不足和客户端 provisional claim 可拒绝/回滚；WorldFile save/load/validate 后账本不漂移；player join/full state 与增量 update 不互相覆盖。

#### 8. 候选五：BestiaryKnowledgeLedger

##### 8.1 为什么它是强内部边界

Bestiary 不是单一的 NPC kill counter，而是三类长期知识状态的聚合：

- 击杀知识；
- 接近/观察知识；
- 与 NPC 对话知识。

这些状态共同参与保存、加载、验证、重置、玩家加入同步和 NPC 资格查询，但不应因此把 Bestiary UI、NPC 定义或 Zoologist 的表现层纳入同一个 authority。

##### 8.2 Version4 事实

| 证据 | 事实 | 状态 |
| --- | --- | --- |
| `D:\TRbackup\Version4\Terraria.GameContent.Bestiary\BestiaryUnlocksTracker.cs:5-57` | 聚合 kill/sight/chat 三类 tracker，并提供 Save/Load/ValidateWorld/Reset/OnPlayerJoining | `confirmed` |
| `NPCKillsTracker.cs:23-115` | 使用稳定 Bestiary credit ID，支持击杀登记、持久化和玩家加入同步 | `confirmed` |
| `NPCWasNearPlayerTracker` | 区域观察扫描和持久化 | `confirmed` |
| `NPCWasChatWithTracker` | 对话解锁和持久化 | `confirmed` |
| `D:\TRbackup\Version4\Terraria\NPC.cs:45909,65328` | 对话/击杀路径提供输入事实 | `confirmed` |
| `D:\TRbackup\Version4\Terraria\Player.cs:3385`、`NPC.cs:42259` | chat 输入路径 | `confirmed` |
| `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:592` | 玩家加入同步入口 | `confirmed` |
| `D:\TRbackup\Version4\Terraria\Main.cs:14001-14004,14167-14170` | Bestiary progress 参与 Zoologist 等 NPC 资格 | `confirmed` |

##### 8.3 当前 NLTX 状态与 proposed seam

当前已找到：

- [`src/WorldProgressionAndUnlocks/ProgressionAggregate.cs`](D:/TRbackup/NLTX/src/WorldProgressionAndUnlocks/ProgressionAggregate.cs:6)，已有 kill/sight/chat 三类集合；
- [`src/WorldProgressionAndUnlocks/BestiaryDiscoveryCache.cs`](D:/TRbackup/NLTX/src/WorldProgressionAndUnlocks/BestiaryDiscoveryCache.cs:7)，当前更像扫描缓存；
- [`dome/src/Terraria.WorldFile.V319/Format/WldBestiaryReader.cs`](D:/TRbackup/NLTX/dome/src/Terraria.WorldFile.V319/Format/WldBestiaryReader.cs:7)，主要保留兼容数据并限制读取数量。

尚未确认闭合的事件订阅、commit、join sync 和资格交接链。因此当前映射应标为 `partial`，不能写成“Bestiary 已迁移”。

建议：

```text
WorldProgressionAndUnlocks
  └─ BestiaryKnowledgeLedger（内部强边界）
       ├─ kill / sight / chat progress
       ├─ persistent credit ID
       ├─ save / load / validate
       ├─ join synchronization
       └─ qualification read view
```

`BestiaryUnlocksTracker.cs` 当前 TSV 归为 `SharedRuntimeMechanisms`，而 JSON 责任语义已经有 `WorldProgressionAndUnlocks`；最终整合必须修正这种语义冲突，或者明确给出保留 shared 分类的理由。

#### 9. 条件候选：CreativeResearchAndPermissions

##### 9.1 已由现有 owner 覆盖的部分

Creative Power overrides 包括 FreezeTime、时间速率、天气/风、难度、放置范围、生态扩散、刷怪率、权限和世界/玩家快照。这部分已有明确的 `SimulationRuleOverrides` 语义，不应另立 `CreativeResearchAndPermissions` 世界规则 owner。

证据：

- `D:\TRbackup\Version4\Terraria.GameContent.Creative\CreativePowerManager.cs:35-198`：register、initialize、reset、save/load/validate、player join sync；
- `D:\TRbackup\Version4\Terraria\Main.cs:3159-3175`：时间覆盖；
- `Main.cs:11364-11374`：难度覆盖；
- `D:\TRbackup\Version4\Terraria\WorldGen.cs:59408-59415`：生态扩散覆盖。

##### 9.2 需要另行判断的 research/sacrifice ledger

`CreativeUnlocksTracker` 和 `ItemsSacrificedUnlocksTracker` 涉及：

- sacrifice count、cap、teammate unlock、new unlock；
- 玩家/世界持久化；
- unlock network report；
- 最终 item inventory 交接。

Version4 中以下成员存在空实现或默认路径：

- `CreativeUnlocksTracker.cs:5-13`；
- `ItemsSacrificedUnlocksTracker.cs:8-35`；
- `NetCreativeUnlocksPlayerReportModule.Deserialize`；
- 多个 Creative Power 的 `DeserializeNetMessage` / `UsePower`。

当前 NLTX 的 `WldCreativePowerReader.cs` 主要把 creative power 数据保留为 opaque compatibility payload。因而本候选只能是 `conditional boundary-challenge`。

blocking decision：必须先确定 research 是 player-owned、world-owned 还是二者分层；确定 unlock ledger 的唯一持久化 owner；确定权限 command 的服务端提交者；确定 sacrifice 物品是由 ledger 直接扣除还是通过 `ItemContainerAndEconomy` 事务提交。

#### 10. 反向审查的内部边界与排除矩阵

| 候选 | 当前归属/处理 | 证据判断 |
| --- | --- | --- |
| `InvasionProgressAndCredit` | `WorldCalendarAndEventOrchestration` / `WorldProgressionAndTransition` 内部 seam | 有独立状态和 Tick，但现有 `AdvanceInvasion` 与事件编排共用写入根，需先整合 |
| `PlayerSpawnAndRespawn` | `PlayerGameplay` 与 `SpawnLifecycleAndLoot` 的交接 | 需要独立身份创建、spawn point、死亡结果和网络投影，但目前不是单一 owner |
| `TeamSpawnPointNetwork` | Spawn/respawn 内部 network seam | 网络索引不是持久化/实体身份根 |
| `LogicSensorRuntime` | Wiring/WorldInteraction/WorldStorage 之间的内部查询和触发 seam | sensor 结果是事实，不能拥有通用结构状态 |
| `QuickStackAndEmergencyTransfer` | `ItemContainerAndEconomy` 内部两个事务 | emergency transfer 与 ordinary pickup 不能共享隐含 ownership 规则 |
| `DoorTraversalAndTileMutation` | `WorldInteractionAndStructures` | Tile mutation 与 traversal 必须同一 structural commit root |
| `PlayerRestAndAnchors` | PlayerGameplay + WorldInteraction | 资格和锚点跨域，但不存在独立持久化/网络生命周期 |
| `DungeonGenerationAndProtection` | WorldGenerationAndEcology | 生成阶段、保护检查和结构提交的内部能力 |
| `NPCInteractionAndServices` | IntentAndInteraction → NpcAndTownSimulation | 请求入口与服务执行分层 |
| `TileEntityRuntime` | 继续按现有报告做 integration review | runtime identity/index/schedule 有边界，但 persistence substrate 和 structural commit 未收敛 |
| `Lighting`、`WorldMap`、`MapUpdateQueue` | query/cache/projection | 没有本轮确认的服务器权威长期状态根 |
| `GolfState` | 局部玩法或客户端工具 | 未形成稳定跨域提交边界 |
| `Achievement`、`Social` | progression read model / platform adapter | 平台通知不是权威状态根 |
| 单个 Recipe、RecipeGroup、货币类型、NPC 类型、AI style、Hook、消息号、TileEntity、Pylon、Banner item | 已有 owner 的定义、策略或协议边界 | 单个类型/消息不是独立生命周期 |

#### 11. 统一 owner 图与唯一提交根

本轮最重要的不是再增加名称，而是明确以下单向事实流：

```text
Committed NPC death fact
       ├──> BestiaryKnowledgeLedger
       │       └──> NPC qualification read view
       └──> BannerProgressionAndClaiming
               └──> authorized Banner claim result
                       └──> ItemContainer commit

Spawn / Loot result
       └──> WorldItemLifecycleAndPickup
               ├──> motion / merge / reservation / despawn
               └──> ItemContainer commit

ContentCatalog
       ├──> CraftingAndRecipeTransactions
       │       └──> ItemContainer commit
       └──> CommerceAndTrading
               ├──> currency debit / credit
               ├──> stock / receipt
               └──> ItemContainer commit

WorldCalendar / WorldProgression
       └──> InvasionProgressAndCredit（内部 seam）
               ├──> NPC spawn eligibility
               ├──> wave progress
               └──> network progress projection
```

唯一 owner 原则：多个系统可以发布事实、提供 Query 或提交 Command，但每个权威状态必须只有一个最终写者。特别禁止：

- WorldItem、Inventory、ItemContainer、MessageBuffer 同时写同一 stack；
- Crafting 与 ItemContainer 同时写 material reservation 和 refund；
- Commerce 同时以 logical balance、金币 Item 和 currency scan 作为三个独立扣款权威；
- Banner 与 Bestiary 账本被 NPC、Player、Network decoder 多处直接改写；
- `whoAmI` 槽位、replication ID、persistent ID 和运行时 `EntityUuid` 互相替代。

#### 12. Integration Review 交接

##### 12.1 第一优先级：WorldItem 与 ItemContainer 的结构提交

先裁决：

1. `WorldItemLifecycleAndPickup` 是否升格为一级 owner，还是保留为 `SpawnLifecycleAndLoot` 与 `ItemContainerAndEconomy` 之间的 transfer seam；
2. WorldItem 的 Item instance、reservation、motion、slot 和 runtime identity 分别由谁拥有；
3. pickup 成功的唯一 commit root 是哪一个；
4. 网络/section projection 如何只投影 committed state。

在此之前，不应继续扩大 Crafting、Commerce、EmergencyStacking 的组件列表，因为它们都会继续碰到 stack 和 reservation 的重复写者问题。

##### 12.2 第二优先级：事务 owner

对 Crafting 和 Commerce 分别锁定：

- operation ID 与 expected revision 的 owner；
- request/response/retry/idempotency 的 owner；
- material/currency debit 的最终提交者；
- result item/container commit 的 adapter；
- refund、timeout、disconnect 和 stale request 的失败语义。

##### 12.3 第三优先级：长期进度账本

对 Banner、Bestiary、Creative research 统一确认：

- world scope 与 player scope；
- Save/Load/Validate 的唯一 owner；
- player join full state 与增量 update 的方向；
- 资格查询是否只读；
- claim/sacrifice 结果如何通过 ItemContainer 提交；
- 客户端 provisional 状态如何拒绝、回滚和重放。

#### 13. focused verifier 计划总表

本表只提出验证计划，本轮没有执行：

| 边界 | 必须证明的性质 |
| --- | --- |
| WorldItem | 竞争拾取唯一成功、reservation 失效可重分配、merge 不超上限、部分成功数量一致、槽位复用隔离、销毁幂等、section 只投影 committed state |
| Crafting | recipe eligibility 与 server revalidation 一致、材料 reservation 唯一、重复 request 幂等、result commit 失败可 refund、pending 不永久锁死 |
| Commerce | stock/price revision 防旧请求、普通/自定义货币扣款原子、找零正确、buy-once 幂等、receipt 可追踪、断线重试不重复扣款/发货 |
| Banner | kill fact 幂等、threshold 只增量一次、claim reject/commit 正确、save/load/validate 稳定、join full state 与 delta 不覆盖 |
| Bestiary | kill/sight/chat 分开记账、stable credit ID、持久化恢复、join sync、资格 Query 只读 |
| Creative research | scope 正确、cap 和 sacrifice 一致、teammate unlock 幂等、权限拒绝、网络 report 与存档不分叉、Inventory 扣除/解锁提交原子 |

建议验证顺序：

```text
WorldItem ↔ ItemContainer structural commit
→ Crafting / Commerce transaction receipts
→ Banner / Bestiary progression persistence
→ Creative research permission and sacrifice
```

#### 14. 验证和变更声明

##### 14.1 本轮实际验证

本轮只读了规则、报告、索引和源码证据，并写入本文档。未运行：

- `dotnet restore`；
- `dotnet build`；
- `dotnet test`；
- `dotnet run`；
- 任何 compile-capable verifier；
- 任何会写入 `Build/bin`、`Build/obj`、测试结果或源代码生成物的命令。

因此本文的统一验证状态为：

```text
verificationStatus: not-run
```

##### 14.2 本轮写入范围

本轮没有修改：

- `src/`、`Test/`、`dome/src/`；
- `D:\TRbackup\Version4`；
- `D:\TRbackup\无任何删减通过编译`；
- `docs/migration/ledgers/Version4子系统索引.json`；
- `docs/migration/ledgers/Version4源码覆盖.tsv`；
- 第一轮审查文件；
- 已有第二轮审查报告。

本轮新增文件仅为：

```text
D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-07-version4-second-round-follow-up-boundary-challenges.md
```

#### 15. 最终交接摘要

```text
reportId: version4-second-round-follow-up-boundary-challenges
reportPath: D:/TRbackup/NLTX/docs/component-decomposition/review-round-2/2026-09-07-version4-second-round-follow-up-boundary-challenges.md
scope: reverse discovery after first-round subsystem markdown review
evidenceStatus: confirmed/partial/mixed by candidate; see sections 4-10
verificationStatus: not-run

confirmedBoundaryChallenges:
- WorldItemLifecycleAndPickup
- CraftingAndRecipeTransactions
- CommerceAndTrading
- BannerProgressionAndClaiming
- BestiaryKnowledgeLedger (strong internal boundary; conditional elevation)

conditionalBoundaryChallenge:
- CreativeResearchAndPermissions

deferredInternalBoundaries:
- InvasionProgressAndCredit
- PlayerSpawnAndRespawn
- TeamSpawnPointNetwork
- LogicSensorRuntime
- QuickStackAndEmergencyTransfer
- DoorTraversalAndTileMutation
- PlayerRestAndAnchors
- DungeonGenerationAndProtection
- NPCInteractionAndServices
- TileEntityRuntime

currentIndexChange: deferred
sharedOwnersForIntegrationReview:
- ItemContainerAndEconomy
- SpawnLifecycleAndLoot
- WorldProgressionAndUnlocks
- WorldCalendarAndEventOrchestration
- ContentCatalog
- NetworkSessionAndSectionStreaming
- WorldStorage
- Entity identity / slot / persistence / replication adapters

blockingDecisions:
- Choose one WorldItem/ItemContainer structural commit root.
- Separate static Recipe/Shop/Currency definitions from runtime transactions.
- Choose one owner for crafting reservation/operation/result state.
- Choose one owner for commerce debit/credit/receipt state.
- Choose Banner and Bestiary world/player persistence scopes.
- Decide whether Creative research is player-owned, world-owned, or split.

notImplemented:
- None of the proposed owners, components, systems, commands, adapters or projections is claimed as implemented by this document.
```

---

### 4.4 Version4 第二轮补充边界挑战：WorldItem 生命周期与拾取

**reviewId：** WorldItemLifecycleAndPickup  
**日期：** 2026-09-07  
**审查类型：** 只读反向发现、ECS 边界分析与 owner 整合建议  
**使用方法：** public-decomposition；按用户要求使用 pua 的穷尽搜索、反转假设和证据闭环流程  
**classification：** strong-boundary-challenge  
**qualification：** 四项候选门槛命中 4/4  
**nltxStatus：** partial  
**verificationStatus：** not-run  
**formalIndexChange：** none  
**decision：** integration-review-required

> 本文件是第二轮的补充审查产物，不把 WorldItemLifecycleAndPickup 直接写入正式子系统索引。它记录了一个已经达到独立边界证据门槛、但仍需要在 ItemContainerAndEconomy、SpawnLifecycleAndLoot 与统一 structural commit root 之间裁决唯一 owner 的候选。

#### 1. 执行摘要

第一轮固定的 19 个子系统不能改名、合并、拆分或扩展。既有全量审查已经形成 32 个责任面。本轮对第一轮 Markdown、Version4 覆盖表、Version4 真实源码以及当前 dome 实现进行有界反向搜索后，发现最强的新候选是：

WorldItemLifecycleAndPickup

整合阶段也可以考虑 WorldItemSimulation 或 WorldDropLifecycleAndPickup 作为名称，但名称不是裁决依据。

它不是普通的库存 Item，也不是单一掉落规则。Version4 的 WorldItem 拥有自己的实体槽位、世界坐标、移动、预约、拾取延迟、被动堆叠、越界/销毁、section 同步和网络更新生命周期；当前 NLTX 也已经有对应的 store、组件、command、event、system、提交顺序和独立 Items verifier 项目。

按仓库已经确认的四项候选门槛，它命中全部四项：

| 门槛 | 当前判断 | 主要证据 |
| --- | --- | --- |
| 独立生命周期/调度阶段 | 命中 | WorldItem.UpdateItem、Main.item[]、owner/reservation loop；NLTX 的 spawn、move、stack、pickup、destroy、delay systems |
| 独立权威状态/写入根/受控提交 | 命中 | Version4 的 Main.item[] 与 WorldItem 状态；NLTX 的 WorldItemStore 与 DomeSimulation.CommitWorldItem* |
| 跨两个以上领域且有稳定 I/O/协议边界 | 命中 | Item/container、Player、NPC loot、Spatial/Liquid、network section、persistence |
| 独立 verifier | 命中 | Terraria.Dome.Items.Verification 及 Items loopback verifier 项目 |

但是，正式新增一级 owner 仍然不能在本文件中自行宣布。第一轮 Item 设计已经明确记录：WorldItemStateComponent、WorldItemReservationComponent、位置/运动/寿命以及 reservation 与 SpawnLifecycleAndLoot 的分工仍然是 decision-required。因此本轮正式结论是：

1. 保留为强 boundary-challenge；
2. 暂不修改 docs/migration/ledgers/Version4子系统索引.json；
3. 暂不修改 docs/migration/ledgers/Version4源码覆盖.tsv；
4. 交由最终 integration review 裁决是否成为正式第 33 个责任面，或作为 Item/Spawn 之间的内部 transfer seam。

本轮没有发现第二个证据强度相同、能够脱离现有 32 项而独立成立的新一级子系统。WorldObjectLifecycle 和 DungeonGeneration 均保留为已有责任面的内部能力或结构边界。

#### 2. 审查范围、基线与只读边界

##### 2.1 输入材料

本轮读取和交叉检查了：

- D:\TRbackup\NLTX\docs\component-decomposition\review-round-1 下的 Markdown；
- docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md；
- docs\plans\component-decomposition\2026-09-05-version4-all-subsystem-discovery-new-session-guide.md；
- docs\component-decomposition\baseline\Version4权威游戏模拟子系统全量审查报告-2026-09-05.md；
- docs\migration\ledgers\Version4子系统索引.json；
- docs\migration\ledgers\Version4源码覆盖.tsv；
- D:\TRbackup\Version4\Terraria\WorldItem.cs；
- D:\TRbackup\Version4\Terraria\Main.cs；
- D:\TRbackup\Version4\Terraria\Item.cs；
- D:\TRbackup\Version4\Terraria\NetMessage.cs；
- D:\TRbackup\Version4\Terraria\MessageBuffer.cs；
- 当前 dome/src/Terraria.Dome.Simulation 的 WorldItem 相关实现；
- 当前 dome/src/Terraria.Dome.Server/DomeServer.cs 的 tick 与复制路径；
- dome/Test/Terraria.Dome.Items.Verification 及相关 loopback verifier 项目。

Version4 是完整覆盖基线。完整可编译参考源码只能补充 Version4 中同路径、同类型、同调用邻域的成员证据，不能扩大 Version4 的分母。本文件不以目录名、namespace 或单个类型名直接宣布新子系统。

##### 2.2 已有正式基线

第一轮固定 19 项为：

CombatAndStatus、DeathPenaltyAndRevenge、FishingAndCatchSimulation、ItemContainerAndEconomy、LeashedEntitySimulation、LiquidSimulation、NpcAndTownSimulation、PlayerGameplay、ProjectileSimulation、SimulationRuleOverrides、SpatialSimulation、SpawnLifecycleAndLoot、TeleportationAndTraversal、WorldCalendarAndEventOrchestration、WorldGenerationAndEcology、WorldInteractionAndStructures、WorldProgressionAndTransition、WorldProgressionAndUnlocks、WorldSession。

既有全量审查总数为 32 个责任面，并且已从旧聚合面反向拆出 LiquidSimulation、DeathPenaltyAndRevenge、LeashedEntitySimulation、PersistenceAndRecovery、NetworkSessionAndSectionStreaming、ContentLifecycleAndRegistration、WiringAndMechanisms 与 MountAndVehicleSimulation 等责任面。

当前覆盖表仍把 Terraria/WorldItem.cs 归在 shared-runtime-mechanism / SharedRuntimeMechanisms：

- [Version4源码覆盖.tsv:966](D:/TRbackup/NLTX/docs/migration/ledgers/Version4源码覆盖.tsv:966)
- EmergencyStacking 对应 [Version4源码覆盖.tsv:549](D:/TRbackup/NLTX/docs/migration/ledgers/Version4源码覆盖.tsv:549)

本报告提出的是对该归属的 boundary challenge，不是静默改表。

##### 2.3 本轮变更边界

本轮只写本 Markdown，不修改：

- src、Test、dome/src 下的生产代码和测试；
- D:\TRbackup\Version4；
- docs\migration\ledgers\Version4子系统索引.json；
- docs\migration\ledgers\Version4源码覆盖.tsv；
- 第一轮审查文件；
- 构建策略或验证程序。

本轮没有运行任何 compile-capable 命令。文中所有当前验证结论均使用 verificationStatus: not-run。源码中出现的 PASS 字符串只说明 verifier 设计了对应断言，不能被解释为本轮运行通过。

#### 3. Version4 真实代码证据

##### 3.1 WorldItem 拥有独立的世界实体状态

D:\TRbackup\Version4\Terraria\WorldItem.cs 声明 WorldItem : Entity，并持有一个内部 Item payload，同时维护世界实体生命周期字段：

- ownTime；
- playerIndexTheItemIsReservedFor；
- noGrabDelay；
- shimmered 与 shimmerTime；
- instanced；
- ownIgnore；
- timeSinceTheItemHasBeenReservedForSomeone；
- timeLeftInWhichTheItemCannotBeTakenByEnemies；
- timeSinceItemSpawned；
- beingGrabbed；
- onConveyor；
- keepTime。

证据起点见 [WorldItem.cs:15](D:/TRbackup/Version4/Terraria/WorldItem.cs:15) 至 [WorldItem.cs:47](D:/TRbackup/Version4/Terraria/WorldItem.cs:47)。这组字段不是 Item definition 或 container contents 的重复表示，而是世界中可变化实体的状态。

##### 3.2 有独立槽位、初始化和 tick 驱动

Version4 的 Main 单独持有 public static WorldItem[] item = new WorldItem[401]，并在初始化时逐槽创建 WorldItem、设置 whoAmI；主更新循环单独调用 item[num4].UpdateItem(num4)。

证据：

- [Main.cs:934](D:/TRbackup/Version4/Terraria/Main.cs:934)
- [Main.cs:3465](D:/TRbackup/Version4/Terraria/Main.cs:3465)
- [Main.cs:11553](D:/TRbackup/Version4/Terraria/Main.cs:11553)

此外，Main 还存在单独的 reservation/owner 处理循环：

- 无 reservation 时周期性调用 FindOwner；
- 有 reservation 时推进 reservation age；
- owner 无效或达到周期时重新寻找 owner；
- 处理 EmergencyStacking.ProcessPendingTransfers。

证据见 [Main.cs:12757](D:/TRbackup/Version4/Terraria/Main.cs:12757) 至 [Main.cs:12781](D:/TRbackup/Version4/Terraria/Main.cs:12781)。这满足独立生命周期/调度门槛，而不是只在玩家库存操作时被动出现。

##### 3.3 生成和槽位复用是独立入口

Item.NewItem 同时处理：

- 生成条件与 stack 输入；
- 槽位分配；
- 已占用槽位的 replacement；
- EmergencyStacking 待转移清理；
- 创建新的 WorldItem；
- item defaults、prefix、stack；
- position、velocity、wet 状态；
- spawn age；
- network broadcast。

证据见：

- [Item.cs:48686](D:/TRbackup/Version4/Terraria/Item.cs:48686)
- [Item.cs:48761](D:/TRbackup/Version4/Terraria/Item.cs:48761)
- [Item.cs:48766](D:/TRbackup/Version4/Terraria/Item.cs:48766)
- [Item.cs:48768](D:/TRbackup/Version4/Terraria/Item.cs:48768)
- [Item.cs:48793](D:/TRbackup/Version4/Terraria/Item.cs:48793)

因此 WorldItem 的 spawn 并不等价于“给库存增加一个 Item”。它会建立一个具有位置、速度、实体槽位身份和复制生命周期的世界对象。

##### 3.4 每 tick 生命周期包含多个相互依赖阶段

WorldItem.UpdateItem 的调用闭包包含：

1. slot reuse guard；
2. inactive/instanced 处理；
3. gravity、wet/shimmer/honey 环境速度；
4. owner time 和 ignore owner；
5. shimmer 转换；
6. passive stacking；
7. enemy pickup protection 与 enemy pickup；
8. world movement；
9. lava/environment death；
10. out-of-world recovery 或 turn-to-air；
11. special despawn；
12. visual state 与 spawn age 推进。

关键入口与阶段位置：

- [WorldItem.cs:357](D:/TRbackup/Version4/Terraria/WorldItem.cs:357)
- [WorldItem.cs:402](D:/TRbackup/Version4/Terraria/WorldItem.cs:402)
- [WorldItem.cs:512](D:/TRbackup/Version4/Terraria/WorldItem.cs:512)
- [WorldItem.cs:526](D:/TRbackup/Version4/Terraria/WorldItem.cs:526)
- [WorldItem.cs:531](D:/TRbackup/Version4/Terraria/WorldItem.cs:531)
- [WorldItem.cs:550](D:/TRbackup/Version4/Terraria/WorldItem.cs:550)

##### 3.5 堆叠和预约跨越 Player、Item、空间和网络边界

TryCombiningIntoNearbyItems 会：

- 遍历其他 WorldItem；
- 判断 Item.CanStack；
- 检查 shimmer 和 reservation owner；
- 修改 receiver/donor stack；
- 修改 position 和 velocity；
- 对归零 donor 执行 TurnToAir；
- 发出 item network update。

证据见 [WorldItem.cs:227](D:/TRbackup/Version4/Terraria/WorldItem.cs:227) 至 [WorldItem.cs:255](D:/TRbackup/Version4/Terraria/WorldItem.cs:255)。

FindOwner 会读取玩家状态、拾取范围、ItemSpace/hopper 等条件，并写回 playerIndexTheItemIsReservedFor，见 [WorldItem.cs:257](D:/TRbackup/Version4/Terraria/WorldItem.cs:257) 起的调用闭包。

这两个入口同时读取/写入多个领域，因此不能仅按 Item 文件名把它们归为容器内部 helper。

##### 3.6 网络和 section streaming 有独立协议边界

Version4 的 item 网络消息单独序列化 slot/replication index、position、velocity、stack、prefix、item type、shimmer 状态和 enemy pickup protection。

证据：

- [NetMessage.cs:638](D:/TRbackup/Version4/Terraria/NetMessage.cs:638) 至 [NetMessage.cs:666](D:/TRbackup/Version4/Terraria/NetMessage.cs:666)
- [NetMessage.cs:1659](D:/TRbackup/Version4/Terraria/NetMessage.cs:1659) 至 [NetMessage.cs:1664](D:/TRbackup/Version4/Terraria/NetMessage.cs:1664)

join 时会单独发送 active world items，见 [MessageBuffer.cs:564](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:564) 至 [MessageBuffer.cs:570](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:570)。接收 item spawn/update、destroy/release 和 owner release 也分别有处理路径：

- [MessageBuffer.cs:1108](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:1108)
- [MessageBuffer.cs:1162](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:1162)
- [MessageBuffer.cs:1751](D:/TRbackup/Version4/Terraria/MessageBuffer.cs:1751)

WorldItem.SyncItemsInSection 在 section 激活时筛选 active item 并发送复制，见 [WorldItem.cs:1557](D:/TRbackup/Version4/Terraria/WorldItem.cs:1557) 至 [WorldItem.cs:1570](D:/TRbackup/Version4/Terraria/WorldItem.cs:1570)。这满足稳定网络/section I/O 边界。

#### 4. 当前 NLTX 映射

##### 4.1 权威状态和 ECS 层

当前 NLTX 已有 WorldItemComponent，保存 ReplicationId、ItemStack、Position、IsActive、Revision、Section、WorldState、InstanceState 和 TimeSinceSpawnedTicks。

见 [WorldItemComponent.cs:6](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Items/WorldItemComponent.cs:6)。

World-side state 与 ownership 被拆为：

- ItemWorldStateComponent：active、pickup delay、spawn source、owner revision、merge tick、reservation 和 revision；
- ItemOwnershipComponent：player、container、source entity、ownership revision。

见：

- [ItemWorldStateComponent.cs:6](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Items/Components/ItemWorldStateComponent.cs:6)
- [ItemOwnershipComponent.cs:5](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Items/Components/ItemOwnershipComponent.cs:5)

WorldItemStore 维护 Dictionary<int, Entity>，并负责 replication ID 到 ECS entity 的映射、active count、add/get/synchronize、stack/revision/finite-position/active invariant 校验、ItemDefinition stack limit 校验、pickup ownership 记录和 runtime component 同步。

见 [WorldItemStore.cs:7](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Items/WorldItemStore.cs:7) 至 [WorldItemStore.cs:151](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Items/WorldItemStore.cs:151)。

##### 4.2 行为 System、Command 和 Projection

当前已经存在以下 WorldItem 专属 System：

| 类型 | 当前职责 | 文件 |
| --- | --- | --- |
| WorldItemSpawnSystem | stack、definition、ID、输入校验和实体构造 | dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemSpawnSystem.cs |
| WorldItemMotionSystem | revision guarded position/section 更新 | dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemMotionSystem.cs |
| WorldItemPickupDelaySystem | delay 和 spawn age 推进 | dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemPickupDelaySystem.cs |
| WorldItemDestroySystem | active item 到 inactive tombstone | dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemDestroySystem.cs |
| WorldItemPickupSystem | range、reservation、库存接纳和 partial pickup | dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemPickupSystem.cs |
| WorldItemStackingSystem | 同类世界掉落合并和 donor 归零 | dome/src/Terraria.Dome.Simulation/Items/Systems/WorldItemStackingSystem.cs |

命令和结果边界包括：

- [CreateWorldItemCommand.cs:7](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Items/Commands/CreateWorldItemCommand.cs:7)
- [PickupWorldItemCommand.cs:3](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Items/Commands/PickupWorldItemCommand.cs:3)
- [DestroyWorldItemCommand.cs:3](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Items/Commands/DestroyWorldItemCommand.cs:3)
- MoveWorldItemCommand
- WorldItemCreatedEvent
- WorldItemPickedUpEvent
- WorldItemDestroyedEvent
- WorldItemSnapshot
- ItemReplicationSnapshot

这些类型只作为当前实现证据记录。本文件不把它们解释为已批准的最终组件 API。

##### 4.3 DomeSimulation 中已有受控提交顺序

当前 DomeSimulation.CommitCommands 已显式调用：

1. CommitWorldItemMoves；
2. CommitWorldItemDestructions；
3. CommitWorldItemPassiveStacking；
4. CommitWorldItemPickups；
5. AdvanceWorldItemPickupDelays。

证据见 [DomeSimulation.cs:5811](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:5811) 至 [DomeSimulation.cs:5831](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:5831)。

具体提交函数分别位于：

- [DomeSimulation.cs:7935](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:7935)
- [DomeSimulation.cs:7954](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:7954)
- [DomeSimulation.cs:7974](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:7974)
- [DomeSimulation.cs:8014](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:8014)
- [DomeSimulation.cs:8063](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:8063)

生成入口和外部命令入口位于：

- [DomeSimulation.cs:3813](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:3813)
- [DomeSimulation.cs:3848](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:3848)
- [DomeSimulation.cs:3878](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:3878)
- [DomeSimulation.cs:3889](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:3889)
- [DomeSimulation.cs:3898](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:3898)

pickup 提交还会检查 player 是否 active、inventory 是否存在、距离和 reservation，并通过 InventoryTransferSystem 接纳数量，写回 WorldItem 剩余 stack/active/revision，记录 winner ownership，发布 inventory change 和 pickup event。

这说明当前实现已有一个明显的 WorldItem write set，但它仍嵌在 DomeSimulation 总提交根中，尚不能据此宣布正式一级 owner 已经完成。

##### 4.4 Server loop 和复制路径

DomeServer.SimulationLoopAsync 当前每 tick：

1. 更新 network isolation adapter；
2. 消费协议命令；
3. 队列 proximity world-item pickup；
4. 调用 _simulation.Tick；
5. 提交 tile changes；
6. 创建 snapshot；
7. 复制 liquid、section、player、inventory、item、chest、door、sign、combat 和 tile entity 状态。

证据见 [DomeServer.cs:1034](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Server/DomeServer.cs:1034) 至 [DomeServer.cs:1075](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Server/DomeServer.cs:1075)。WorldItem 复制单独从 CreateItemReplicationSnapshots 获取，见 [DomeServer.cs:2052](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Server/DomeServer.cs:2052)。

##### 4.5 当前实现的边界风险

1. WorldItemStore、DomeSimulation、DomeServer 都参与世界物品状态的可见性和提交；最终唯一 structural commit root 仍需明确。
2. 玩家丢弃、NPC loot、extractinator 输出和其他掉落来源均会调用 WorldItem spawn；drop rule owner 与 world entity owner 尚未在正式索引层分离。
3. WorldItem pickup 会同步修改 Inventory；需要明确是 WorldItem 子系统提交跨域事务，还是 ItemContainer 提供受控接纳端口并由更高层事务提交。
4. Version4 的 reservation、enemy pickup、shimmer、液体环境、越界恢复和 slot reuse 保护，在当前 NLTX 中尚未由同一个 focused loopback verifier 完整证明行为一致。
5. 当前 item replication 和 persistence snapshot 证明了投影/恢复形状存在，但不能等同于 Version4 网络和存档行为等价。

#### 5. 所有权与边界建议

##### 5.1 建议的责任分配

以下只是 integration review 的 proposed 方向，不是已批准 owner：

| 状态或行为 | 候选 owner | 不应由谁重复持有 |
| --- | --- | --- |
| item definition、prefix、variant、stack limit | ItemContainerAndEconomy / ContentCatalog 边界 | WorldItem 不应复制 definition 全量状态 |
| 世界掉落的来源规则和 loot table | SpawnLifecycleAndLoot | WorldItem 不应决定 NPC 掉什么 |
| WorldItem entity identity、slot/replication identity | WorldItemLifecycleAndPickup 候选 | Item payload、Player 或 Inventory 不应充当世界实体 ID |
| 世界 position、section、spawn age、pickup delay、active/tombstone | WorldItemLifecycleAndPickup 候选 | ItemContainer 不应持有第二份世界生命周期 |
| reservation/ownership 的世界侧资格状态 | WorldItemLifecycleAndPickup 候选；最终需整合裁决 | ItemOwnershipComponent 不应未经裁决成为第二个 reservation authority |
| inventory 接纳、stack transfer、container revision | ItemContainerAndEconomy | WorldItem 不应复制库存 contents 或直接成为库存 owner |
| player active、位置和 pickup permission context | PlayerGameplay | WorldItem 不应拥有 Player lifecycle |
| network session、编码、section send policy | NetworkSessionAndSectionStreaming | WorldItem core 不应持有外部 packet 类型 |
| persistence envelope、版本、恢复协调 | PersistenceAndRecovery | WorldItem core 不应直接依赖存档文件格式 |
| tile/liquid structural mutation | WorldStorage / LiquidSimulation | WorldItem 不应直接成为 tile commit root |

##### 5.2 两个可供整合裁决的解释

这是会改变架构方向的 blocking-decision，本文件不自行选择最终答案。

###### 方案 A：正式新增 WorldItemLifecycleAndPickup

该候选成为第 33 个责任面，负责世界掉落实体的实体生命周期、预约、移动、堆叠、拾取竞争、销毁和 WorldItem snapshot seam。

优点：

- 直接匹配 Version4 的独立生命周期和 Main.item[] 写集；
- 可把世界实体 identity 与 Item payload identity 分开；
- 可为 pickup race、tombstone、stacking 和 section replication 建立独立 verifier；
- 降低 ItemContainerAndEconomy 把世界空间实体状态吞回巨型组件的风险。

代价和前置条件：

- 必须定义与 SpawnLifecycleAndLoot 的 drop-result seam；
- 必须定义与 ItemContainerAndEconomy 的 inventory acceptance transaction；
- 必须解决 pickup 后 WorldItem 减堆/销毁和 inventory 增量的原子性；
- 必须把 network/persistence 只保留为 Adapter/Projection，不复制 authority；
- 必须重新同步 JSON、TSV、报告和覆盖校验，不可只新增名称。

###### 方案 B：不新增一级 owner，保留为两个既有 owner 之间的内部 seam

SpawnLifecycleAndLoot 负责产生 CreateWorldItemCommand，ItemContainerAndEconomy 负责 payload/transfer，空间运动和复制分别归 Spatial/Network；WorldItem 只作为跨域实体模型和 transfer seam。

优点：

- 正式子系统数量不增加；
- 与第一轮 Item 草案当前的 componentOwner 候选保持兼容；
- 避免过度原子化和第三套 owner。

风险：

- 很容易把 WorldItem 的位置、寿命、reservation 和销毁写回多个旧 owner；
- ItemContainerAndEconomy 可能重新形成同时包含 inventory、drop entity 和 world motion 的巨型边界；
- pickup race 和 passive stacking 的唯一提交根更难审计；
- 覆盖表中的 WorldItem.cs 继续属于 SharedRuntimeMechanisms 时，责任闭包可能再次变得不可见。

##### 5.3 当前推荐

当前推荐不是立即选择方案 A 或 B，而是先冻结以下整合问题：

1. WorldItemStateComponent 的唯一 owner；
2. WorldItemReservationComponent 的唯一 owner；
3. WorldItem 位置、运动、寿命的唯一 writer；
4. SpawnLifecycleAndLoot 到 WorldItem 的 drop-result seam；
5. WorldItem 到 ItemContainer 的 pickup acceptance seam；
6. WorldItem destroy/decrement 与 inventory increment 的原子提交根。

如果这些问题最终由一组稳定的 WorldItem systems 和一个受控提交根承担，则应把它升格为正式一级 owner；如果它们仍被两个既有 owner 以明确端口协作承担，则可以保留为内部 seam，但不能继续把整个责任面笼统归为 shared runtime mechanism。

#### 6. Proposed ECS 边界（仅供整合评审）

以下类型、路径和接口均为 proposed，不代表当前已经创建或批准。

##### 6.1 Proposed Component

| Proposed component | 权威内容 | 明确不包含 |
| --- | --- | --- |
| WorldItemIdentityComponent | runtime entity identity、replication identity、persistent identity 映射关系 | item type、container slot、外部 packet identity |
| WorldItemLifecycleComponent | active、spawn tick、pickup delay、despawn/tombstone、revision | Item definition 全量内容 |
| WorldItemLocationComponent | world position、section、必要的运动状态 | Player position、tile storage authority |
| WorldItemReservationComponent | reserved player、reservation age、ignore owner、reservation revision | inventory contents、Player lifecycle |
| WorldItemStackComponent | 当前世界实体持有的数量关系，或明确引用 ItemStackComponent | 第二份 definition/prefix authority |
| WorldItemEnvironmentComponent | wet/shimmer/conveyor/enemy-pickup protection 等世界侧状态 | 客户端视觉粒子和 UI |

是否需要把 stack 单独拆为 WorldItemStackComponent，取决于 ItemContainer 的最终 quantity owner。不能同时让 WorldItemComponent、ItemStackComponent 和 WorldItem payload 成为并行数量 writer。

##### 6.2 Proposed System

候选系统边界：

- WorldItemSpawnSystem：验证 CreateWorldItemCommand，分配 world replication identity，产生新实体；
- WorldItemMotionSystem：处理世界运动、section 迁移和环境影响；
- WorldItemReservationSystem：处理 owner/reservation 的资格与超时；
- WorldItemStackingSystem：按稳定顺序合并世界掉落；
- WorldItemPickupSystem：只提交已授权的 pickup request，并通过 ItemContainer port 接纳数量；
- WorldItemDestructionSystem：处理越界、寿命、环境死亡和 tombstone；
- WorldItemReplicationProjection：把已提交 WorldItem state 转成稳定 snapshot，不反向写 authority。

系统调度顺序必须显式写入 SimulationTickSchedule 或等价调度契约，不能由文件/目录顺序表达。一个待整合评审的顺序是：

Resolve item/drop commands → validate world-item identity and input → apply world motion/environment → resolve reservation/stacking → resolve pickup acceptance → commit world-item destruction/tombstones → publish inventory/world-item events → create network/persistence projections

这只是顺序候选。当前 NLTX 已有的提交顺序是 move → destruction → passive stacking → pickup → pickup delay；Version4 的真实行为与未来统一 structural commit root 仍需 verifier 证明，不能仅凭命名认定上述顺序正确。

##### 6.3 Proposed Commands、Events 和 Seams

建议保留稳定的领域命令方向：

- CreateWorldItemCommand；
- MoveWorldItemCommand；
- ReserveWorldItemCommand；
- PickupWorldItemCommand；
- MergeWorldItemCommand；
- DestroyWorldItemCommand。

建议事件只表达已经提交的事实：

- WorldItemCreatedEvent；
- WorldItemReservedEvent；
- WorldItemPickedUpEvent；
- WorldItemMergedEvent；
- WorldItemDestroyedEvent。

跨域 seam：

1. SpawnLifecycleAndLoot → CreateWorldItemCommand；
2. WorldItemLifecycleAndPickup → ItemContainer.TryAcceptWorldItemPort；
3. WorldItemLifecycleAndPickup → PlayerGameplay pickup context query；
4. WorldItemLifecycleAndPickup → NetworkSessionAndSectionStreaming snapshot adapter；
5. WorldItemLifecycleAndPickup → PersistenceAndRecovery snapshot adapter。

以上接口必须使用稳定的领域 ID 和快照类型，不能让 packet、存档 DTO 或旧 Entity 类型渗透核心 authority。

#### 7. 不新增的候选与反证

##### 7.1 WorldObjectLifecycle：不升格

当前 dome/src/Terraria.Dome.Simulation/WorldObjects 下有 chest、sign、door、tile entity、training dummy、food platter 和 placement 相关类型，但它们不是一个共享生命周期的单一权威对象：

- chest 是容器和持久化居民；
- sign 有文本、revision 和 tombstone；
- door 有 transition 和 wiring 关联；
- tile entity 有独立的 identity、anchor、link、lifecycle 和 update scheduler；
- training dummy 有自己的 activation/ownership；
- food platter 主要是结构销毁和掉落联动；
- placement 是 WorldInteractionAndStructures 到 WorldStorage 的结构提交能力。

当前 DomeSimulation 也分别持有 _doors、_signs、_chests 和 _tileEntityStore，没有证据表明这些对象共享同一个完整 lifecycle、authority root 和 structural transaction。

结论：

WorldObjectLifecycle：rejected-as-new-subsystem  
保留为 WorldInteractionAndStructures、WorldStorage、WiringAndMechanisms 和 ItemContainerAndEconomy 的边界组合。

这与既有全量报告对“单个 TileEntity 不构成一级子系统”的结论一致，见 [全量审查报告:207](D:/TRbackup/NLTX/docs/component-decomposition/baseline/Version4权威游戏模拟子系统全量审查报告-2026-09-05.md:207) 至 [全量审查报告:218](D:/TRbackup/NLTX/docs/component-decomposition/baseline/Version4权威游戏模拟子系统全量审查报告-2026-09-05.md:218)。

##### 7.2 DungeonGeneration：保留为 WorldGenerationAndEcology 内部能力

Version4 的 Dungeon 类型数量很多，但其职责分布在 entrance、hall、room、feature、layout provider、style、bounds、platform 和 secret-seed/dual-dungeon policy 等生成阶段。

当前 NLTX 的 DungeonGenerationState 主要表达 generation context 的 setup/reset：

- [DungeonGenerationState.cs:7](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/WorldGeneration/DungeonGenerationState.cs:7)
- [DungeonGenerationState.cs:19](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/WorldGeneration/DungeonGenerationState.cs:19)
- [DungeonGenerationState.cs:27](D:/TRbackup/NLTX/dome/src/Terraria.Dome.Simulation/WorldGeneration/DungeonGenerationState.cs:27)

已有 Dungeon verifier 主要覆盖 geometry、settings、enums 和 generation policy，例如 [DungeonBounds verifier:33](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.DungeonBounds.Verification/Program.cs:33) 和 [DungeonBounds verifier:146](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.DungeonBounds.Verification/Program.cs:146)。这不能证明它已有独立的持续运行时生命周期、跨域 authoritative loop 或独立 commit root。

结论：

DungeonGeneration：internal-capability  
owner：WorldGenerationAndEcology

##### 7.3 其他已复核候选

| 候选 | 当前归属 |
| --- | --- |
| HousingAndTownRoom | NpcAndTownSimulation 的住房资格/查询边界 |
| CraftingAndRecipeResolution | ItemContainerAndEconomy 的交易/容器事务 |
| BestiaryPresentation | 解锁事实归 WorldProgressionAndUnlocks，UI/database 为 projection |
| 单个 CreativePower | SimulationRuleOverrides 的策略/投影 |
| Golf | Player、Projectile、Client presentation 相关机制，未证明独立 authority root |
| Lighting、Rain、AmbienceServer | 表现或环境投影 |
| SceneMetrics、BiomeScene | 派生查询或缓存 |
| MapUpdateQueue、WorldMap、Pylon map layer | 客户端地图 projection |
| AchievementManager、Social | 平台适配或外部 projection |
| 单个 TileEntity、Pylon、AI style、Hook、消息号、压力板、逻辑门 | 实例、策略、扩展点、adapter 或 Wiring 内部机制 |

#### 8. focused verifier 计划与实际状态

##### 8.1 已存在的 verifier 项目

当前存在：

- dome/Test/Terraria.Dome.Items.Verification；
- dome/Test/Terraria.Dome.Items.Loopback.Verification；
- 相关 Combat、Persistence verifier 用于交叉验证 pickup/drop、持久化和实体提交边界。

Terraria.Dome.Items.Verification/Program.cs 已设计覆盖：

- authoritative Definition stack limit；
- world item spawn 和 replication ID；
- ECS component synchronization；
- pickup race 和 duplicate pickup rejection；
- inactive player rejection；
- partial pickup；
- out-of-range pickup；
- pickup delay；
- stale move/destroy revision；
- inactive tombstone；
- passive stacking；
- player drop；
- NPC loot；
- persistence restore；
- item replication snapshot。

代表性断言位置：

- [Program.cs:487](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:487)
- [Program.cs:509](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:509)
- [Program.cs:2711](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:2711)
- [Program.cs:2767](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:2767)
- [Program.cs:2837](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:2837)
- [Program.cs:2856](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:2856)
- [Program.cs:3950](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:3950)
- [Program.cs:3978](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:3978)
- [Program.cs:4091](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:4091)
- [Program.cs:8070](D:/TRbackup/NLTX/dome/Test/Terraria.Dome.Items.Verification/Program.cs:8070)

##### 8.2 尚需补强的 verifier

在决定是否升格前，建议增加或确认以下跨域闭环：

1. player drop：inventory decrement、WorldItem creation、drop event 必须作为一个可审计提交单元；
2. NPC loot：loot rule output、WorldItem spawn 和 source attribution 必须幂等；
3. pickup race：同 tick 多玩家竞争时只能有一个 winner，partial pickup 不能允许第二个 winner；
4. pickup acceptance failure：inventory 满、stack limit 错误、玩家 inactive 或越界时 WorldItem 不得提前减少；
5. passive stacking：receiver/donor revision、donor tombstone 和 event/replication 顺序必须稳定；
6. reservation：owner release、reservation age、敌人拾取保护和断线清理必须统一；
7. environment：液体、shimmer、lava、conveyor 和越界恢复必须由显式 port 输入，不能让 WorldItem 直接持有外部 I/O；
8. persistence/network：snapshot restore、tombstone、section activation 和 replication revision 必须分别验证，不能只验证 DTO 构造；
9. identity：runtime entity ID、world slot ID、replication ID、persistent ID、player/container ID 必须不能互相替代；
10. system order：在正常 tick、暂停 tick、错误和重复命令路径下都验证顺序约束。

##### 8.3 实际验证状态

本轮未运行 dotnet restore、dotnet build、dotnet test、dotnet run，也未运行任何会写入 Build/bin、Build/obj 或测试结果的命令。

因此本文件不得写成“构建通过”“verifier 通过”“行为等价”或“迁移完成”。

verificationStatus: not-run

#### 9. Integration Handoff

subsystemId: WorldItemLifecycleAndPickup  
taskNumber: second-round-boundary-challenge  
reportPath: docs/component-decomposition/review-round-2/2026-09-07-world-item-lifecycle-and-pickup-boundary-challenge.md

evidenceStatus: version4-confirmed; current NLTX implementation skeleton observed  
nltxStatus: partial  
verificationStatus: not-run

confirmedOwners:

- Version4 WorldItem 的世界实体生命周期、槽位和 active state；
- WorldItem movement、pickup delay、passive stacking、reservation、pickup 和 destroy 调用闭包；
- WorldItem 的 item network update 和 section activation synchronization boundary；
- 当前 NLTX WorldItemStore、WorldItem systems、commands/events/snapshots 的局部 authority。

proposedTypes:

- proposed WorldItemIdentityComponent；
- proposed WorldItemLifecycleComponent；
- proposed WorldItemLocationComponent；
- proposed WorldItemReservationComponent；
- proposed WorldItemEnvironmentComponent；
- proposed WorldItemSpawnSystem；
- proposed WorldItemMotionSystem；
- proposed WorldItemReservationSystem；
- proposed WorldItemStackingSystem；
- proposed WorldItemPickupSystem；
- proposed WorldItemDestructionSystem；
- proposed WorldItemReplicationProjection。

sharedTypesForIntegrationReview:

- ItemStack / Item definition / stack limit；
- Item instance / prefix / variant state；
- player pickup context；
- inventory acceptance transaction；
- loot/drop result；
- runtime entity ID / replication ID / persistent ID；
- world section and snapshot types。

crossSubsystemReaders:

- ItemContainerAndEconomy；
- PlayerGameplay；
- SpatialSimulation；
- LiquidSimulation；
- NetworkSessionAndSectionStreaming；
- PersistenceAndRecovery。

crossSubsystemWriters:

- SpawnLifecycleAndLoot；
- ItemContainerAndEconomy player-drop path；
- NpcAndTownSimulation loot path；
- LiquidSimulation/environment path；
- Network/session input adapters。

orderingConstraints:

- WorldItem move、destruction、passive stacking、pickup 和 pickup-delay ordering must be explicit；
- Inventory acceptance and WorldItem decrement/destruction must share an auditable commit boundary；
- Snapshot/replication must observe committed state only；
- Reservation release must occur before a new pickup winner is accepted。

boundaryChallenges:

- decide whether WorldItem is formal subsystem 33 or an internal seam；
- assign one owner to WorldItemStateComponent；
- assign one owner to WorldItemReservationComponent；
- separate drop-rule ownership from world-drop-entity ownership；
- separate ItemContainer quantity ownership from WorldItem world-entity ownership；
- close the structural commit root for pickup/drop/stacking/destroy。

evidenceGaps:

- Version4 and current NLTX behavior equivalence was not run or proven in this review；
- Version4 persistence details for every WorldItem field require a dedicated recovery pass；
- enemy pickup, shimmer, liquid, conveyor and out-of-world branches require focused loopback coverage；
- current formal TSV/JSON ownership has not been recomputed after this challenge。

blockingDecisions:

- formal new owner versus internal transfer seam；
- final WorldItem state/reservation/quantity owner；
- unified structural commit root location。

notImplemented:

- no production code was changed；
- no test or verifier was changed；
- no formal index or TSV was changed。

#### 10. 最终声明

本文件完成的是一次证据驱动的第二轮边界挑战，不是迁移实现报告。

可以确认：

- WorldItemLifecycleAndPickup 是当前发现的最强新增候选；
- 它满足四项候选门槛中的四项；
- Version4 有独立 WorldItem 生命周期、状态写集、网络/section 边界；
- 当前 NLTX 已有相当完整但尚未完成整合闭环的实现骨架；
- 当前第一轮 Item 设计确实留下了 WorldItem owner 的 integration decision。

不能确认：

- 它已经是正式第 33 个索引子系统；
- 当前 NLTX 已经行为等价；
- 当前 verifier 已通过；
- WorldItem、Inventory、Loot、Network、Persistence 已形成唯一且闭合的 structural commit root；
- 迁移、API 兼容或完整运行时实现已经完成。

下一步唯一优先动作：

> 由最终整合会话先裁决 WorldItemStateComponent、WorldItemReservationComponent、WorldItem 运动/寿命、ItemContainer 数量接纳和 Spawn/Loot 结果之间的唯一 owner 与提交根；裁决后再决定是否把本候选写入正式索引，并同步 JSON、TSV、主报告和覆盖校验。
