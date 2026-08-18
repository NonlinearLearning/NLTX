# NPC ECS 迁移执行提案

## 状态

- 状态：已确认范围，待实施。
- 来源：`D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`。
- 目标实现：`src\Terraria.Dome.Simulation`，协议和兼容投影留在各自适配层。
- 首阶段承诺：核心 NPC 行为切片。
- 首阶段不承诺：全部 `AI_###`、全部 Boss、全部入侵/世界事件、完整城镇服务和完整
  Terraria 行为等价。
- 本文不恢复已物理删除的旧 `NPC.cs`，也不把旧类重新加入 ECS 运行时依赖。

## 决策摘要

NPC 不再作为一个拥有状态、生成器、AI、掉落和网络字段的巨型对象迁移。迁移后的
NPC 是 Arch ECS 世界中的实体，由共享能力组件和 NPC 专属组件组合而成。系统读取
稳定快照，产生命令或事件，在明确的提交点修改世界；协议层只读取 NPC 投影 DTO。

首阶段采用“核心可执行切片 + 后续行为族工作包”策略：先建立可复现的普通敌怪、一个
城镇 NPC 和一个分段 NPC 骨架，再逐个引入 Boss、入侵、事件和特殊 AI。每个行为族都
必须先有来源映射、状态不变量和独立验证，不能以增加 API 数量代替行为迁移。

## 来源证据

Version4 的 `NPC.cs` 是历史来源，不是新的继承基类。当前文件的结构证据如下：

| 来源区域 | 证据 | 迁移含义 |
| --- | --- | --- |
| 类声明 | `NPC : Entity`，约第 30 行 | 位置、速度、尺寸等通用实体状态应拆到共享组件 |
| 生成器 | `NPC.Spawner`，约第 39 行；`SpawnNPC`、`TrySpawnAnNPC`、`GetSpawnRate` | 自然生成必须成为 Spawn 领域系统和快照 |
| 实例状态 | `active`、`type`、`ai[0..3]`、`localAI[0..3]`、`life`、`defense` 等，约第 5,897 行以后 | 运行时状态不能继续集中在 `NpcComponent` 或旧类字段中 |
| 全局状态 | Boss 索引、`downed*`、塔状态、入侵积分和波次 | 应迁移为 World Event/Boss 资源，不属于某一个 NPC |
| 初始化 | `SetDefaults`、`SetDefaults_ForNetId`，约第 7,399 至 8,133 行 | 静态类型定义和实例初始化分离 |
| AI | `AI()` 及 `AI_###` 方法，约第 18,642 行以后；源文件约 157 处 AI 标记 | 每个行为族使用有类型状态和独立系统，不复制 `ai[]` 公共接口 |
| 目标 | `TargetClosest`、`GetTargetData`、目标跟踪方法 | 迁移为目标选择和目标有效性系统 |
| 生命周期 | `CheckActive`、`checkDead`，约第 64,438 至 64,571 行 | 拆为活跃性、死亡判定、死亡提交和销毁/墓碑命令 |
| 掉落和事件 | `NPCLoot`、死亡事件和入侵进度，约第 65,323 行以后 | 掉落、成就、事件进度通过事件和资源系统处理 |
| 生成入口 | `NewNPC`、`SpawnBoss`、`SpawnOnPlayer`，约第 66,679 至 67,051 行 | 统一为受验证的 Spawn 命令，不允许网络包直接创建实体 |
| 网络 | `netUpdate`、`netSpam`、同步方法和 `NetMessage` 调用 | 网络节流和编码放在 Replication/Protocol 适配层 |

源文件约 79,688 行、约 354 个方法；这证明不能按字段机械搬运，也不能把全文件压入
一个首阶段任务。源文件只用于行为族盘点和迁移映射，不能作为 Simulation 的运行时
引用。

当前仓库的可复用基础包括：

- `NpcTagComponent`、`NpcTargetComponent`、`NpcAiStateComponent`；
- `TransformComponent`、`VelocityComponent`、`ColliderComponent`、`HealthComponent`；
- `DamageNpcCommand`、`NpcHandle`、`NpcSnapshot` 和 `NpcReplicationSnapshot`；
- `TileCollisionSystem`、现有玩家/投射物系统、`DomeSimulation` 的 tick 和快照边界；
- `LootTable` 和 `DomeSimulationSnapshot` 的持久化形状。

这些类型是首阶段的迁移落点，不代表最终目录已经完成领域整理。

## 目标边界

### Simulation 允许依赖

- Arch ECS `World`、`Entity` 和查询；
- 纯值类型组件、定义、快照、命令和事件；
- `WorldGrid`、确定性随机源和共享移动/物理/战斗系统；
- 领域内的注册表，例如 `NpcDefinitionRegistry` 和 `NpcBehaviorRegistry`。

### Simulation 禁止依赖

- `Terraria.NPC`、`Main.npc`、旧 `Player`、旧 `Projectile`；
- `Main`、`NetMessage`、`MessageBuffer`、socket、客户端会话和渲染/UI；
- 直接读取旧 `ai[]`、`localAI[]`、`whoAmI` 或固定数组槽位；
- 通过反射或目录枚举隐式决定系统顺序。

兼容读取或旧世界导入应位于 `Terraria.WorldCompatibility` 或世界文件模块，转换成
定义/快照后再进入 Simulation。适配器不能把旧 NPC 实例暴露给 ECS 系统。

## 组件拆分

### 共享能力组件

| 领域 | 组件 | 责任 | 首阶段 |
| --- | --- | --- | --- |
| Entity | `EntityIdentityComponent` | 稳定 ECS/复制身份映射 | 建立契约 |
| Entity | `TransformComponent` | 位置和朝向所需空间状态 | 复用并收口 |
| Movement | `VelocityComponent` | 当前速度 | 复用并收口 |
| Physics | `ColliderComponent` | 碰撞盒尺寸和偏移 | 复用 |
| Physics | `PhysicsStateComponent` | 接触、落地、碰撞结果 | 复用 |
| Combat | `HealthComponent` | 当前生命和最大生命 | 复用 |
| Combat | `DefenseComponent` | 防御和受击修正输入 | 新增 |
| Combat | `HitImmunityComponent` | 来源/目标免疫计时 | 新增 |
| StatusEffects | `BuffCollectionComponent` | 有类型 Buff、持续时间和免疫 | 先建最小集合 |
| Movement | `MovementIntentComponent` | AI/控制系统产生的移动意图 | 新增 |

共享组件应放入表达能力的领域，不创建新的全局 `Shared/Components` 堆积目录。

### NPC 专属组件

| 组件 | 主要字段/不变量 | 来源 |
| --- | --- | --- |
| `NpcTagComponent` | 无状态标记 | 现有 `NpcTagComponent` |
| `NpcDefinitionComponent` | `DefinitionId`、`NetId`、阵营、类别 | `type`、`netID`、`SetDefaults` |
| `NpcTargetComponent` | 目标 Entity、目标有效性、锁定原因 | `target`、`TargetClosest` |
| `NpcBehaviorStateComponent` | `BehaviorId`、阶段、计时器和行为参数 | `aiStyle`、`ai[]`、`localAI[]` |
| `NpcSpawnStateComponent` | 来源、雕像/事件/难度缩放、释放者 | `Spawner`、`SpawnedFromStatue`、`releaseOwner` |
| `NpcLifecycleComponent` | 活跃、剩余时间、可替换和消失原因 | `active`、`timeLeft`、despawn 方法 |
| `NpcHomeComponent` | 家、无家、找家计时和城镇变体 | `homeTileX/Y`、`homeless`、town 状态 |
| `NpcSegmentComponent` | 根实体、父子关系、段索引和共享生命策略 | `realLife`、worm/segment 逻辑 |
| `NpcInteractionComponent` | 玩家交互位图的实体化集合、最后交互者 | `playerInteraction`、`lastInteraction` |
| `NpcReplicationComponent` | 复制 ID、revision、网络脏标记和节流状态 | `netUpdate`、`netSpam`、槽位同步 |

`NpcBehaviorStateComponent` 不提供 `float[] Ai` 字段。对旧协议需要的 `ai[0..3]` 只在
`NpcStateProjector` 内由 `BehaviorId + typed state` 映射得到。

### 世界级资源

以下状态不能挂在 NPC 实体上：

- `BossEncounterState`：Boss 当前实体、阶段和倒计时；
- `InvasionState`：入侵类型、剩余规模、积分和波次；
- `MoonEventState`：南瓜月、霜月等事件积分与波次；
- `TownNpcProgressionState`：城镇 NPC 解锁和世界进度；
- `NpcSpawnBudgetState`：全局生成上限、保护槽和生成周期。

这些资源由 `NpcSpawning`、`Boss` 和 `WorldEvents` 系统读写，通过快照在 tick 边界替换。

## 系统拆分

系统必须显式注册，并按下列顺序执行；系统不通过反射、文件名或查询创建顺序排序。

1. `NpcSpawnEligibilitySystem`：读取世界、玩家和 `NpcSpawnBudgetState`，产生
   `SpawnNpcCommand`，不直接创建实体。
2. `NpcSpawnCommitSystem`：验证定义、预算、空间和来源，创建实体并初始化组件。
3. `NpcTargetSelectionSystem`：从有效玩家/实体快照选择目标，输出目标组件更新。
4. `NpcBehaviorSystem`：按 `BehaviorId` 分派有类型行为；首阶段至少实现普通追逐和一个
   城镇行走骨架。
5. `NpcMovementIntentSystem`：把 AI 状态变为移动意图，不直接改碰撞结果。
6. 共享 `GravitySystem`/`MovementSystem`/`TileCollisionSystem`：执行运动和碰撞。
7. `NpcContactEffectSystem`：产生接触伤害或交互命令，不直接扣除生命。
8. 共享 `DamageResolutionSystem`：检查防御、免疫、来源和伤害上限，产生伤害事实。
9. `NpcLifecycleSystem`：处理失活、超时、消失和分段根实体约束。
10. `NpcDeathSystem`：处理死亡前状态、掉落/事件事件和 `DespawnNpcCommand`。
11. `NpcLootSystem`：按不可变 `NpcLootDefinition` 生成世界物品命令。
12. `NpcReplicationSystem`：根据组件差异递增 revision，生成稳定 NPC 快照。

一次 tick 中，结构变更只在统一提交点执行。系统之间通过命令/事件传递意图和事实，
不能在遍历查询时直接删除实体或创建子实体。

## 首阶段行为切片

首阶段只实现三类可证明行为：

### 普通敌怪

- 从 `NpcDefinition` 创建实体；
- 选择最近有效玩家；
- 在有地面碰撞的条件下追逐；
- 产生接触伤害；
- 接收投射物/近战伤害，死亡后生成确定性掉落并发布失活 revision。

### 城镇 NPC 骨架

- 使用 `NpcHomeComponent` 保存家位置和无家状态；
- 实现昼夜/家位置驱动的最小移动行为；
- 不在首阶段实现完整商店、对话、住房判定和城镇服务。

### 分段 NPC 骨架

- `NpcSegmentComponent` 表达根、父段、子段和段序号；
- 验证根实体死亡或分裂时的提交顺序；
- 不在首阶段复制全部 Worm/Boss AI，只验证关系和生命周期不变量。

Boss、入侵、事件、捕捉/释放、完整 Buff、所有特殊掉落和所有 `AI_###` 进入后续行为族
工作包；每个工作包必须独立添加定义、状态、系统、协议投影和验证。

## 数据流与协议

```text
NpcDefinition + WorldSnapshot + PlayerSnapshot
  -> NpcSpawn/Target/Behavior/Movement systems
  -> Npc/Combat commands and events
  -> deterministic commit
  -> NpcReplicationSnapshot
  -> NpcStateProjector
  -> SyncNPC packet codec
```

NPC 协议投影至少需要以下来源：

| 协议字段 | ECS 来源 |
| --- | --- |
| NPC 索引/复制 ID | `EntityIdentityComponent` + `NpcReplicationComponent` |
| 位置、速度 | `TransformComponent` + `VelocityComponent` |
| 目标 | `NpcTargetComponent` |
| 类型/NetId | `NpcDefinitionComponent` |
| 方向和方向 Y | `FacingComponent` 或 NPC 方向组件 |
| 生命/最大生命 | `HealthComponent` |
| 四槽 AI 兼容字段 | `NpcBehaviorStateComponent` 的有类型投影 |
| 生成来源/难度/释放者 | `NpcSpawnStateComponent` |
| 活跃和 revision | `NpcLifecycleComponent` + `NpcReplicationComponent` |

协议编码器只接收 `NpcSyncPacket` DTO。它不能直接读取 Arch `World`，也不能调用
`AI()`、`SetDefaults()`、`NewNPC()` 或 `NetMessage.SendData()`。

## 迁移批次

### Batch 0：来源清点和契约冻结

- 固定 Version4 文件哈希、行数、方法分类和行为族清单；
- 固定 `NpcDefinitionId`、`BehaviorId`、复制 ID 和 snapshot 字段；
- 对现有 `DomeSimulation` 的 NPC 查询、创建、tick、快照路径建立基线；
- 输出 `NpcMigrationCoverage`，把每个来源区域标记为 `planned`、`partial`、`excluded`
  或 `verified`。

### Batch 1：组件和定义

- 将 NPC 专属状态从集中式 `DomeSimulation` 拆到组件；
- 建立 `NpcDefinition`、注册表、Spawn 参数和稳定复制身份；
- 保持旧 Dome 公共 API 的最小兼容外观，避免一次性改动服务器协议。

### Batch 2：生成、目标、普通 AI 和运动

- 把生成资格和实体创建拆成命令/提交系统；
- 迁移最近目标、追逐、重力、地形碰撞和接触效果；
- 用纯 Simulation 验证每个系统的输入快照和输出命令。

### Batch 3：战斗、死亡、掉落和复制

- 迁移防御、伤害、免疫、死亡和确定性掉落；
- 产生失活 revision 和世界物品命令；
- 完成 `NpcReplicationSnapshot` 和 `SyncNPC` 投影的字段来源。

### Batch 4：城镇和分段骨架

- 引入 `NpcHomeComponent` 和最小城镇行为；
- 引入 `NpcSegmentComponent` 和根/子段生命周期；
- 为两类组合实体增加独立测试和快照回放。

### Batch 5+：后续行为族

- Boss 阶段与 Boss encounter 资源；
- 入侵、事件、月事件和世界进度；
- Buff/DoT、捕捉/释放、城镇服务、特殊掉落；
- 按 `BehaviorId` 分批迁移特殊 AI，不接受一次性复制整个 `AI()`。

### Final：旧依赖清除

- 搜索 Simulation 对 `Terraria.NPC`、`Main.npc`、`ai[]` 和旧网络 API 的引用；
- 删除只用于过渡的兼容代码；
- 只有在所有目标行为族和协议门槛通过后，才允许清除旧来源依赖。

## 验证闸门

每个批次必须同时满足模型、行为和边界验证；绿色编译不能替代行为等价证据。

### 模型闸门

- 没有 `Npc : Entity` 或新的 NPC 继承层次；
- NPC 状态由组件组合，静态规则由 Definition 提供；
- 系统不直接持有旧 NPC 实例或固定数组槽位；
- 结构变更只经命令提交，快照在 tick 边界稳定。

### 行为闸门

- 普通敌怪目标选择、移动、碰撞、接触伤害、受伤、死亡和掉落可回放；
- 城镇骨架的 home/无家状态可回放；
- 分段骨架的根/父/子关系和死亡顺序可回放；
- 同一种子、同一输入和同一初始快照产生相同 snapshot/revision 序列。

### 协议闸门

- `SyncNPC` 字段顺序、宽度、稀疏 AI 标志和生命编码有字节级 fixture；
- 复制 ID/revision 在创建、更新、死亡和重新连接后稳定；
- 客户端包不能直接写生命、目标或实体生命周期；
- 协议投影不读取旧 `NPC` 字段。

### 工程闸门

- 使用仓库选定 SDK 和 `-p:UseSharedCompilation=false` 串行执行；
- 产物只能进入 `Build/bin`、`Build/obj` 和 `Build/generated`；
- 每个批次记录实际命令、退出状态、警告和未验证项；
- 首阶段完成前不声称完整 Terraria NPC 等价。

## 回滚和风险边界

- 不恢复或修改 Version4 的物理删除文件；Version4 只作为只读来源证据。
- 迁移失败时只回滚当前批次新增的 Simulation 文件、注册表和调度注册，不回滚用户的
  其他未相关修改。
- 若旧世界导入需要 NPC 数据，先在兼容层转成 `NpcReplicationSnapshot` 或定义输入，
  不把旧 `NPC` 对象带入 Simulation。
- 首阶段最主要风险是把 `DomeSimulation` 的集中式逻辑拆分时改变 tick 顺序；因此每次
  拆分都要先固定 snapshot/replay 基线，再替换一个系统。
- 第二个风险是把 `ai[0..3]` 重新当成内部状态；计划以类型化行为状态和单向协议投影阻断。
- 第三个风险是全局 Boss/事件状态污染 NPC 实体；计划用资源和独立系统隔离。

## 首阶段完成定义

只有以下条件全部满足，才可把首阶段标记为完成：

1. 普通敌怪、城镇 NPC 骨架、分段 NPC 骨架均由组件组合创建，未继承旧 NPC。
2. 生成、目标、行为、移动、战斗、死亡、掉落和复制系统按显式顺序运行。
3. 纯 Simulation 回放、快照恢复和 NPC 复制 fixture 通过。
4. `SyncNPC` 投影与协议测试通过，Simulation 无旧 Terraria NPC/网络依赖。
5. `NpcMigrationCoverage` 清楚列出后续行为族，且没有把 `partial` 误报为完整迁移。

完整 Terraria NPC 迁移仍需后续行为族全部通过各自闸门后另行确认。
