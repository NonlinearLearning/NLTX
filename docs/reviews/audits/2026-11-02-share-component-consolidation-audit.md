# Share 组件收敛审计（非 Share 域 → Share 复用）

**范围**：`src/NSSLC/Components/` 下非 `Share/` 的六个域（`Npc` 12、`Player` 17、
`Projectile` 43、`World` 38、`Communication` 19、`Diagnostics` 6，共 135 个文件、145 个类型），
对照 `Share/` 七个域（`AI` 154、`Attributes` 37、`Combat` 35、`Entity` 22、`Inventory` 10、
`Item` 9、`Physics` 56，共 323 个文件）。

**目的**：找出哪些非 Share 组件应当改用 Share 的组件，消除重复模型。

> 状态：本文件记录已由主代理逐条验证的结论。三个子代理（Player/Npc、Projectile、
> World/Communication）的逐文件审计仍在进行，结果待并入。

## 1. 方法

1. 按**实际声明的类型名**（非文件名）扫描 145 个类型的外部引用数，找出零引用者。
   - 文件名与类型名并不总是一致：`NpcAiRegistration.cs` 实际声明 `NpcAiRuntime`；
     `CommunicationSessionSystems.cs` 实际声明 `ICommunicationPacketHandler`、
     `CommunicationSessionTickSystem`、`CommunicationOutboundProjectionSystem`。
     按文件名扫描会得出错误的死代码清单，已改用声明名重扫。
2. 对疑似重复的模型逐字段比对，判断是"同一概念两套定义"还是"同形异义"。
3. 检查每个候选类型是否真的被接线（实例化、注册、挂在实体上），因为**未接线的
   Share 组件无法被"改用"**——它本身没有运行路径。

## 2. 已确认的重复模型

### 2.1 库存：两套 —— 已消除（见第 7 节）

| | Share/Inventory（迁移前未接线） | Player/PlayerInventoryComponent（迁移前在跑） |
| --- | --- | --- |
| 文件 | `InventoryComponent`、`InventorySlot`、`InventorySlotDefinition`、`InventorySlotKind`、`ContainedInInventoryComponent`、`InventorySystem`、`InventorySystem.Entities`、`InventoryAttributeContributionSystem`、`InventoryTransferFailure`、`InventoryTransferResult` | `PlayerInventoryComponent` + 嵌套 `PlayerInventorySlot` |
| 存储 | 固定数组，由 `InventorySlotDefinition[]` 驱动，按 index 排序并校验唯一 | `Dictionary<int, PlayerInventorySlot>` 稀疏表，无定义 |
| 槽位规则 | 有：`InventorySlotKind`（7 种）、`ItemCategoryMask`、`MaxStackSize`、`Accepts(ItemDefinition)` | 无 |
| 物品表示 | `ItemStack?(ItemId, Quantity, Prefix)` 或 `EntityUuid`（二者互斥） | `(ItemType:int, Stack:int, Prefix:byte)` |
| 客户端标志 | 无 | `Favorited:bool` |
| 变更入口 | `InventorySystem`（`CaptureSlots`/`CommitSlots` 计划-提交） | 直接 `Set(slot, item)` |
| 接线 | **零实例化**：`new InventoryComponent(` 全仓 0 处，`InventorySystem` 未注册 | `Program.cs:355` 挂在玩家实体；`PlayerInputRequestSystem` 写入 |

**采用 Share 的前置条件（均不满足）**：

1. `ItemDefinitionRegistry` 全仓**从未实例化**——物品定义数据（`maxStack`、`categories`、
   属性修饰）不存在。5000+ 物品的 `ItemId.g.cs` 只有枚举，没有定义。
2. `ItemStack` 构造函数校验 `ItemIdRules.IsKnown(itemId)`，未知 id 直接抛异常；
   而协议入站包可能携带任意 `int`。
3. `Favorited` 在 Share 模型里没有位置。
4. 变更必须经 `InventorySystem`，需按 `ECS系统输入边界约束` 补请求队列并注册。

### 2.2 世界掉落物：两套，且是架构差异

| | Share/Item（未接线） | World（在跑） |
| --- | --- | --- |
| 载体 | ECS 实体：`ItemStackComponent` + `ItemWorldStateComponent` + 共享空间组件 | 扁平记录 `WorldItemState`，存在 `WorldItemStore` 的 400 槽表里 |
| 物品字段 | `ItemStack(ItemId, Quantity, Prefix)` | `ItemType:int, Stack:int, Prefix:byte` |
| 位置/速度 | 由共享空间组件持有（`ItemWorldStateComponent` 注释明示） | 内联 `Position`/`Velocity` |
| 生命周期 | `PickupDelayTicks`、`LifetimeTicks`、`CanBePickedUp`、`IsExpired` | 无（只有 `Revision`） |
| 归属 | `ContainedInInventoryComponent`（库存关系） | `SessionEpoch? ReservedFor`（网络会话预留） |

**这不是字段级重复，是数据表 vs ECS 实体的架构差异。** 改用 Share 模型等于把世界掉落物
从槽表改成实体，影响 `WorldItemStore`、`WorldItemRequest`、`WorldItemRules`、
`WorldNetworkProjectionSystem` 的整条链路。

### 2.3 交互能力：两套，一套是死代码

| | 定义 | 状态 |
| --- | --- | --- |
| `Player/PlayerInteractionCapabilityComponent` + `PlayerInteractionCapability`（`PickupItems`/`UseItems`/`Craft`/`ModifyTiles`，ushort） | Player 域 | **零外部引用**，从未挂到任何实体 |
| `World/WorldInteractionCapabilityComponent` + `WorldInteractionPermission`（`ReadTile`/`BreakTile`/`PlaceTile`，byte） | World 域 | `Program.cs:360` 挂在玩家实体，在跑 |

两者语义重叠但不完全等价（`ModifyTiles` ≈ `BreakTile` + `PlaceTile`）。

## 3. 已排除的假重复（不要合并）

| 候选 | 结论 |
| --- | --- |
| `Player/PlayerResourceStateComponent` vs `Share/Combat/LifeComponent` | **不同资源**：前者是法力（`CurrentMana`/`MaximumMana`/回复），后者是生命（`Current`/`Maximum`/`Minimum`）。`AttributeKind` 枚举里没有 Mana 通道，Share 也没有通用资源池抽象，所以法力无 Share 对应物 |
| `Projectile/ProjectileDamageRequestSystem`、`ProjectileDamageResultSystem` vs `Share/Combat` | **已经是共享模型**。两者 `using Terraria.Combat`，消费 `CombatDamageResultQueue`、产出 `CombatDamageRequest`，是适配器而非第二套实现 |
| `MinimalImplementation/Inventory/State/InventoryStore` 里的 `InventorySlot` | 是 `InventoryStore` 的**私有嵌套 record**，与 `Terraria.Inventory.InventorySlot` 无关 |
| `Share/Item/ItemId`（生成枚举） | **活的**，被 MinimalImplementation 引用 77 处，不是死代码 |

## 4. 零外部引用的类型（7 个）

按声明名扫描 145 个类型得出：

| 域 | 类型 | 文件 |
| --- | --- | --- |
| Player | `PlayerAttackIntentAdapter` | `PlayerAttackIntentAdapter.cs` |
| Player | `PlayerCraftingContextComponent` | `PlayerCraftingContextComponent.cs` |
| Player | `PlayerInteractionCapability` | `PlayerInteractionCapabilityComponent.cs` |
| Player | `PlayerResourceSystem` | `PlayerResourceSystem.cs` |
| Player | `PlayerTargetingSystem` | `PlayerTargetingSystem.cs` |
| Projectile | `ProjectileAiRegistration` | `ProjectileAiRegistration.cs` |
| Projectile | `ProjectileSimulationSystem` | `ProjectileSimulationSystem.cs` |

注意：`PlayerResourceSystem`（法力回复）与 `ProjectileSimulationSystem` 零引用意味着
**这两条运行路径当前没有接线**，不是"被 Share 替代"，而是根本没跑。

## 5. Share 侧的未接线孤岛

除 `ItemId.g.cs`（活）外，`Share/Item` 与整个 `Share/Inventory` 共 18 个文件构成一个
闭合孤岛：`ItemStack`、`ItemDefinition`、`ItemDefinitionRegistry`、`ItemCategoryMask`、
`ItemAttributeModifierDefinition`、`ItemStackComponent`、`ItemWorldStateComponent`、
`ItemIdRules`，加 `Share/Inventory` 全部 10 个文件——岛外引用均为 0。

## 6. 待定问题

1. ~~库存方向~~：已按"让 Share/Inventory 支持槽位赋值，再迁移玩家库存"执行，见第 7 节。
2. 世界掉落物：是否接受"槽表 → ECS 实体"的架构改动？
3. 交互能力：删除 `PlayerInteractionCapabilityComponent`，还是把
   `WorldInteractionPermission` 提升为通用能力模型？
4. 7 个零引用类型是否一并清理？

## 7. 已执行：库存双模型消除

采用方案「让 Share/Inventory 支持槽位赋值，再把玩家库存迁过去」，理由是该模型的
`TryInsert/TryRemove/TryMove` 都是实体式的（要求物品先是带 `ItemStackComponent` 的 ECS 实体），
而线上协议路径是槽位赋值式的（包 5 直接给出 itemType/stack/prefix），没有物品实体可搬。

### 7.1 新增的槽位赋值入口

`InventorySystem.TrySetSlotValue(ArchEntity inventoryEntity, int slotIndex, in ItemStack stack)`

- `stack` 为空 → 清空该槽位；否则整格替换，**不做跨槽合并**（与 `TryInsert` 的语义分界）。
- 拒绝路径逐条前置，拒绝时不留下部分写入：槽位越界 → `InvalidSlot`；
  槽位被实体占用 → `SlotRequiresEntity`；缺物品定义 → `MissingItemDefinition`；
  超物品自身堆叠上限 → `InvalidItemStack`（由 `ItemDefinition.AcceptsQuantity` 判定）；
  分类不符 → `NoCompatibleSlot`；超槽位上限 → `InvalidQuantity`。

### 7.2 迁移改动

| 文件 | 改动 |
| --- | --- |
| `Player/PlayerInventoryComponent.cs` | **删除**（连同 `PlayerInventorySlot`） |
| `Share/Inventory/InventorySystem.cs` | 新增 `TrySetSlotValue` |
| `Player/PlayerInputRequest.cs` | 载荷由 `PlayerInventorySlot` 改为 `(int ItemType, int Stack, byte Prefix)` |
| `Player/PlayerInputRequestSystem.cs` | 构造注入 `InventorySystem`，经 `TrySetSlotValue` 写入 |
| `Communication/GameCommunicationAdapter.cs` | `Has<PlayerInventoryComponent>` → `Has<InventoryComponent>`；只提交意图 |
| `Npc/NpcAiSnapshotProvider.cs` | 玩家识别过滤键改为 `InventoryComponent` |
| `NSSLC.Server/Program.cs` | 建 `ItemDefinitionRegistry` + `InventorySystem`；玩家实体 59 格定义表；注册系统 |
| `Test/…/MinimalVerification/Program.cs` | 用 `InventoryComponent` 单格 |

### 7.3 行为等价性处理

- **堆叠上限**：迁移前 `PlayerInventorySlot.Stack` 无上界（包字段是 `Int16`，可达 32767，
  代码只拒绝负数）。物品定义与槽位上限定为 `short.MaxValue`，与该宽度对齐，不放紧行为。
- **空槽语义**：迁移前 `IsEmpty => ItemType <= 0 || Stack <= 0`。新路径显式要求
  `ItemType > 0 && Stack > 0` 才构造 `ItemStack`，否则写空堆清槽，语义一致。
- **`Favorited` 丢弃**：迁移前该字段**只写不读**（全仓无读取点），因此丢弃不改变可观察行为。
  它作为客户端 UI 标志在 Share 模型中无归属，未污染共享模型。
- **未知物品 id**：协议层 `itemType` 若是 `ItemId` 枚举外的值，`TryFromNetworkType` 返回 false
  → 写入空堆（清槽）。这与迁移前"存下任意 int"不同，但迁移前也没有任何读取方。

### 7.4 验证

| 项目 | 结果 |
| --- | --- |
| `Terraria.Components` / `NSSLC.Server` / `MinimalVerification` 构建 | 0 错误 |
| `queueprobe` | 49 PASS / 0 FAIL |
| `invprobe`（新增，12 项覆盖 `TrySetSlotValue` 契约） | 12 PASS / 0 FAIL |
| `worldprobe` | 195 行与 `phase0-before.txt` 完全一致 |
| `tickprobe` | 8 行不变 |
| `MinimalVerification` | PASS |

### 7.5 接线现状

`InventoryComponent`、`InventorySystem`、`InventorySlot`、`InventorySlotDefinition`、
`InventorySlotKind` 现已全部有岛外引用。仍为 0 岛外引用的是 `ContainedInInventoryComponent`、
`InventoryTransferResult`、`InventoryTransferFailure`——前者属于尚未启用的实体式包含关系，
后两者仅由返回值承载、无人显式命名。`InventorySystem` 的实体式方法
（`TryInsert`/`TryRemove`/`TryMove`/`TryRemoveInventoryComponent`）仍未接线。

## 8. 全量逐文件审计结果（并入）

对 `Player`(17) / `Npc`(12) / `Projectile`(43) / `World`(38) / `Communication`(19) /
`Diagnostics`(6) 共 135 个文件逐文件比对后，重复模型共 **11 组**。

### 8.1 对 Share 的重复

| # | 重复组 | 状态 |
| --- | --- | --- |
| 1 | `PlayerInventoryComponent`/`PlayerInventorySlot` ↔ `Share/Inventory/*` | **已消除**（第 7 节） |
| 2 | `ProjectileSimulationSystem` + `ProjectileSimulationResult` + `ProjectileSimulationStatus` + `ProjectileMotionResult` ↔ `Share/Physics/SpatialSimulation/*` + `SpatialTileMotionResult` | **部分重复**：结果封套与 `ApplyGravity` 是平行复制，但系统本身已调用 Share 的 `frame.World.Move` 并带子步/到期/终止逻辑，**不可整体删除**（见 8.3） |
| 3 | `WorldTileMotionResult` ↔ `SpatialTileMotionResult` | **已消除**（第 9 节） |
| 4 | `WorldItemState`/`WorldItemStore`/`WorldItemRules` ↔ `Share/Item` | 待决策；容器层是「400 槽表 vs ECS 实体」架构分歧 |
| 5 | `WorldRuntimeStateComponent.(WorldEpoch, Revision)` ↔ `SpatialWorldRevision` | **已消除**（第 13 节）；投影表达式原先写了两遍，现收为单点 |
| 6 | `ProjectileDefinition`/`ProjectileDefinitionCatalog` ↔ `Share/Entity/EntityDefinition*` | 待决策；**Share 侧才是 0 消费者的死抽象**，不能机械替换 |
| 7 | `ProjectileCollisionPolicyComponent` 的 4 个 bool 中 2 个 ↔ 共享能力/关系组件 | **已消除**（第 23 节）：`CollideWithEntities`、`IgnoreSameFaction` 已删除；`CollideWithTiles` 是投射物特有问题，保留 |
| 8 | `PlayerInteractionCapability` ↔ `WorldInteractionPermission` | **已消除**（第 9 节）；Player 侧 0 消费者，已删除 |
| 9 | `WorldMutation` ↔ `LiquidTileChange` | **已处理**（第 17.1 节）：确认不是同一概念，加区分注释而非合并 |

### 8.2 目录内重复（无 Share 参与）

| # | 重复组 | 说明 |
| --- | --- | --- |
| 10 | `PlayerCombatResultSystem` ↔ `NpcCombatResultSystem` | **重复实现已消除**（第 17.2 节，抽出 `CombatResultDrain`）；但两者在服务器里仍匹配 0 实体——**接线缺口未修**，见 15.2 |
| 11 | `Npc/SlimeAiBehavior` ↔ `Share/AI/Examples/SlimeAiBehavior` | 同名同概念双实现 |
| 12 | ~~`ProjectileSourceMetadataComponent` ↔ `ProjectileOwnerComponent`~~ | **判断错误，已撤销**：前者是字符串溯源 `(SourceKind, SourceId, SourceRevision)`，后者是 `EntityRuntimeHandle`，不同概念，不是重复模型。仅"写入后从未被读取"成立，见 16.2 |
| 13 | `ProjectileHomingCapabilityComponent` ↔ `ProjectileBehaviorProfile` | 前者从未挂到实体，制导恒不生效 |
| 14 | `InboundPacketEnvelope` / `InboundPacketFrame` | 包头字段存了两遍；**已核实后建议不合并**，见 15.3 |

### 8.3 修正：Projectile 的战斗一侧并不重复

先前判断"Projectile 伤害系统已用共享模型"是对的，但**不能据此认为整个 Projectile 域没有重复**。
战斗一侧（contacts → attackIntents → 共享伤害 → results → projectile state）确实正确建立在
`Share/Combat` 之上；重复的是**运动一侧**（第 8.1 节第 2 项）。

**再进一步核对后的修正**：子代理建议"删这 4 个文件"，实际读代码后该建议过于激进。
`ProjectileSimulationSystem` 已经在调用 `frame.World.Move(...)` 拿到 Share 的
`SpatialTileMotionResult`，并使用 Share 的 `TileCollisionCapabilityComponent`、
`GravityDirection`、`VelocityComponent`；它还承担子步推进、到期判定、终止请求判定这些
投射物特有逻辑。因此真正重复的只有两处：

1. 结果封套 `ProjectileSimulationResult`/`ProjectileSimulationStatus` 与
   `SpatialSimulationResult<T>`/`SpatialSimulationStatus` 平行（`ProjectileMotionResult`
   额外带 `CompletedSubsteps`，属合理扩展，不算重复）。
2. `ApplyGravity` 与 `SpatialSimulationSystem` 里的同名私有方法逐行等价
   （同公式、同 `MaxFallSpeed` 夹取，仅写法不同）。

**结论：这一项不能机械删除，需要决策**——要么把结果封套统一到 Share 的泛型形状并抽出公共
重力积分，要么保留投射物专用封套并只消除 `ApplyGravity` 的重复。

同一个审计还发现一个接线缺陷：`ProjectileLifecycleSystem` 创建投射物实体时没有附带
`GroundingComponent`/`MovementCapability`/`GravityCapability`/`TileCollisionCapability` 等空间能力组件，
因此 `SpatialSimulationCoordinatorSystem` 的查询匹配不到任何投射物——**投射物当前不会移动**。

### 8.4 确认不要合并的

- `PlayerResourceStateComponent`（法力）vs `LifeComponent`（生命）：`AttributeKind` 无法力通道。
- `ProjectileLifetimeStateComponent` vs `ProtectionWindowComponent`：结构同、语义相反（0=该死 vs >0=受保护）。
- `WorldTileState`/`WorldTileCoordinate`：**Share 反向依赖 World**，Share 内没有第二套 tile 模型。
- `Diagnostics` 全部 6 个：Share 下无任何日志/诊断类型。
- 各域请求队列组件：仓库既有统一模式（9 份同形），属样板重复而非概念重复。
- `NpcAiTargetFact`/`NpcAiMovementFact`/`NpcAiIntentKind` vs `Share/AI/Examples/*`：
  性质是"生产类型没晋升进 Share 正式命名空间"，应以晋升 + 样例改名消除，而非删生产类型。

### 8.5 零消费者类型（已按含探针的扫描校正）

先前按 `src` + `Test` 扫描得出 7 个零引用类型，**漏了 `Build/` 下的探针**。把探针纳入后，
145 → 140 个类型（本轮删除若干），真正零外部引用的只剩 5 个：

| 类型 | 处置 |
| --- | --- |
| `PlayerCraftingContextComponent` | 已删除 |
| `PlayerResourceSystem` + `PlayerResourceStateComponent` | 已删除（组件只被该死系统引用） |
| `PlayerTargetingSystem` + `PlayerTargetingStateComponent` | 已删除（同上） |
| `ProjectileAiRegistration` | **保留**，属待决策的 Projectile 项 |
| `ProjectileSimulationSystem` | **保留**，属待决策的 Projectile 项 |

**一处必须纠正的判断**：`PlayerAttackIntentAdapter` 曾被列为"零消费者、被生产路径取代"，
但 `CombatAttackIntentQueue` 是 `internal`，该适配器是**外部程序集提交攻击的唯一公共接缝**，
`queueprobe` 正是依赖它做"缺队列时快速失败"的断言。按 `src`+`Test` 扫描会漏掉这类
"仅被探针使用"的公共 API，因此不能只看生产消费者就判死。已恢复该类型。

另有一处：2 个已注册但查询永远匹配 0 实体的系统（`PlayerDeathRespawnSystem`、
`PlayerCombatResultSystem`）不属死代码——它们缺的是玩家实体上的 `PlayerDeathStateComponent`，
正确处理是补挂组件或更换过滤器，删除会丢掉既有调度位置。


## 9. 已执行：第二轮消除

### 9.1 `WorldTileMotionResult` → `SpatialTileMotionResult`

两个类型逐字段相同、`IsFinite` 实现相同，唯一差别是命名空间。`World` 域本来就 `using
Terraria.Physics`，求解器签名本身收 Share 的 `TileCollisionCapabilityComponent`/
`GravityDirection`，所以原注释声称的"World 拥有这个形状、由 `WorldRuntime` 投影到
`Terraria.Physics`"在代码里从未成立过。

改动：

| 文件 | 改动 |
| --- | --- |
| `World/Model/WorldTileMotionResult.cs` | **删除** |
| `World/Solver/WorldTileMotionSolver.cs` | 返回类型与 3 处构造改用 `SpatialTileMotionResult` |
| `World/WorldRuntime.cs` | `Move` 直接返回求解器结果，删掉 5 行逐字段投影 |

### 9.2 `PlayerInteractionCapabilityComponent` → 只保留 `WorldInteractionPermission`

同一件事（"这个实体被允许做什么"）的两套授权模型：Player 侧
`PlayerInteractionCapability`（`PickupItems`/`UseItems`/`Craft`/`ModifyTiles`，ushort）
与 World 侧 `WorldInteractionPermission`（`ReadTile`/`BreakTile`/`PlaceTile`，byte）。
`ModifyTiles` 与 `BreakTile|PlaceTile` 表达同一权限。

Player 侧全仓零消费者（从未挂到任何实体、从未读取、从未构造），因此删除它不改变行为，
也不需要先在两个活模型之间做选择。真正需要在跑的授权源是 `WorldInteractionCapabilityComponent`
（`Program.cs` 挂在玩家实体，`GameCommunicationAdapter`、`WorldInteractionSystem` 读取）。

### 9.3 验证

| 项目 | 结果 |
| --- | --- |
| `Terraria.Components` / `NSSLC.Server` / `MinimalVerification` 构建 | 0 错误 |
| `queueprobe` | 49 PASS / 0 FAIL |
| `invprobe` | 12 PASS / 0 FAIL |
| `worldprobe` | 195 行与 `phase0-before.txt` 完全一致 |
| `tickprobe` | 8 行不变 |
| `MinimalVerification` | PASS |

`worldprobe` 是关键回归项：`WorldTileMotionSolver` 正是通过 `WorldRuntime.Move` 被
`SpatialSimulationCoordinatorSystem` 调用的，输出逐字节未变说明这次类型替换没有改变任何
数值行为。

## 10. 剩余项与所需决策

| 项 | 需要决定什么 |
| --- | --- |
| 世界掉落物（8.1 #4） | 是否接受"400 槽表 → ECS 实体"的架构改动 |
| Projectile 内容定义（8.1 #6） | Share 的 `EntityDefinition*` 本身是 0 消费者死抽象，退役 Share 侧还是收编 Projectile 侧 |
| Projectile 运动封套（8.1 #2） | 统一到 Share 泛型封套，还是只抽公共 `ApplyGravity` |
| `WorldRuntimeStateComponent` 的 epoch/revision（8.1 #5） | 是否改为持有 `SpatialWorldRevision` 字段并合并两处投影 |
| 死代码（8.5） | 已按校正后的清单清理，见第 11 节 |

## 11. 已执行：第三轮

### 11.1 抽出公共重力积分

`SpatialSimulationSystem` 与 `ProjectileSimulationSystem` 各有一个私有 `ApplyGravity`，
逐行等价（同公式、同 `MaxFallSpeed` 夹取，仅一处用 `new Vector2(...)` 另一处改 `velocity.Y`）。
新增 `Share/Physics/SpatialSimulation/GravityIntegration.cs`（`internal static`，与两个消费者同程序集），
两处改为调用它并删除私有副本。这样夹取规则只剩一个真相，不会各自漂移。

### 11.2 清理零消费者死代码

删除 5 个文件（均无任何外部引用）：

| 文件 | 说明 |
| --- | --- |
| `Player/PlayerCraftingContextComponent.cs` | 无字段消费方 |
| `Player/PlayerResourceSystem.cs` | 法力回复系统，从未注册 |
| `Player/PlayerResourceStateComponent.cs` | 只被上面那个系统引用 |
| `Player/PlayerTargetingSystem.cs` | 目标清理系统，从未注册 |
| `Player/PlayerTargetingStateComponent.cs` | 只被上面那个系统引用 |

保留 `ProjectileAiRegistration` / `ProjectileSimulationSystem`——它们不是死代码而是
尚未接线的投射物 AI/运动实现，删除会抢先于第 10 节的 Projectile 决策。

### 11.3 验证

| 项目 | 结果 |
| --- | --- |
| 全部 7 个项目构建 | 0 错误 |
| `queueprobe` | 49 PASS / 0 FAIL |
| `invprobe` | 12 PASS / 0 FAIL |
| `worldprobe` | 195 行与 `phase0-before.txt` 完全一致 |
| `tickprobe` | 8 行不变 |
| `MinimalVerification` | PASS（含 spatial movement 断言） |

`SpatialSimulationSystem` 的数值路径由 `MinimalVerification` 的 spatial movement 断言覆盖，
输出与抽取前一致。

## 12. 仓内验证

`Build/` 被 `.gitignore` 忽略，此前所有探针（61 条断言）都不在仓库里。这轮新增的
`InventorySystem.TrySetSlotValue` 是消除库存双模型的关键公共 API，不能只靠仓外探针覆盖。

新增 `Test/Terraria.Inventory.Verification`（16 条断言，退出码即结论）：

- **阶段 1**：`TrySetSlotValue` 契约——正常写入、整格覆盖、空堆清槽、清空空槽、
  槽位越界、负索引、缺定义、超槽位上限、超物品上限（报 `InvalidItemStack`）、
  分类不符、无库存组件、恰好等于上限，并验证每条拒绝路径都不留下部分写入。
- **阶段 2**：迁移后的端到端路径——`PlayerInputRequestSystem` 经 `TrySetSlotValue`
  写入共享 `InventoryComponent`；`SetPosition` 生效；未知玩家被跳过；
  `ItemType<=0` 按空槽处理（对齐迁移前 `PlayerInventorySlot.IsEmpty` 语义）。

```
PASS: single inventory model — TrySetSlotValue contract and player input path.
```

## 13. 已执行：世界版本投影收敛

`WorldRuntimeStateComponent` 用 `(WorldEpoch, Revision)` 两个字段表示"世界快照版本"，
而 `Share/Physics` 已经有 `SpatialWorldRevision(ulong Epoch, long Version)`——同一概念的两种表示。
表现为 `WorldRuntime` 里同一句投影写了两遍：

```csharp
// WorldRuntime.cs:105（Revision 属性）
return new SpatialWorldRevision(State.WorldEpoch, State.Revision);
// WorldRuntime.cs:438（CurrentRevisionUnsafe）
return new SpatialWorldRevision(State.WorldEpoch, State.Revision);
```

改法（保持公开表面完全不变，因此无行为变更）：把投影收到组件自身——

```csharp
public SpatialWorldRevision SpatialRevision => new(WorldEpoch, Revision);
```

两处调用点改为 `State.SpatialRevision`。

没有采纳"用 `SpatialWorldRevision` 当唯一后备字段"的更强方案：`WorldEpoch`/`Revision`
还要被 `TryAdvance`/`TryAdvanceRevision`/`TryReload` 分别读写，改成单结构体只会把
`with` 表达式搬进这些方法，去重收益相同而改动面更大。真正的重复是那句投影，已消除。

验证：全部工程 0 错误；`queueprobe` 49 PASS；`Terraria.Inventory.Verification` 16 PASS
（退出码 0）；`worldprobe` 195 行完全一致；`tickprobe` 8 行不变；`MinimalVerification` PASS。
`WorldRuntime.Revision` 同时是 `ISpatialWorldRevisionSource.Current` 的实现，
由 `MinimalVerification` 的 spatial movement 断言间接覆盖。

## 14. Share 侧的对外暴露面

目标的前半句是"非 Share 的组件能否改用 Share 的组件"。这需要先知道 Share 的哪些部分
真的被外部消费——一个没有任何外部消费者的 Share 抽象，不可能被"改用"。

按文件统计 331 个 Share 类型，其中 **152 个（46%）没有任何来自 `Share/` 之外的引用**：

| 子域 | 零外部引用类型数 |
| --- | --- |
| AI | 118 |
| Combat | 11 |
| Physics | 10 |
| Entity | 7 |
| Attributes | 4 |
| Inventory | 1 |
| Item | 1 |

**方法学的限制（必须说明）**：这是按类型名做词边界正则匹配的静态统计。当类型名与常见
标识符撞名时（`Slot`、`Sequence`、`Domain`、`Effect`、`Node`、`Graph` 等），匹配会被
无关代码灌满，引用数虚高。因此：

- **"零引用"这一侧是可信的下界**——撞名只会让死类型被误判为活，不会让活类型被误判为死。
- **"有引用"那一侧不可信**，不能用来列举 Share 的实际 API 面。要精确统计需要 Roslyn
  语义分析，不是正则能做到的。

**结论**：Share/AI 子域 154 个类型里至少 118 个从未被非 Share 代码引用，说明各域对
Share/AI 的消费面很窄（Npc/Projectile 只用到 `AiSystem`、`AiEntitySystem`、
`AiBehavior<T>`、`IAiSnapshotProvider`、`AiSnapshot`、`AiRequirements`、`AiOutput`、
`AiComponent`、`AiBinding` 这几类）。这意味着"把非 Share 的 AI 相关模型改成复用 Share AI"
的空间比子代理报告暗示的要小得多——与其说它们在重复 Share，不如说 Share/AI 大部分
是无人使用的框架。

这也是第 8.4 节判定"`NpcAiIntentKind`/`NpcAiTargetFact` 等与 `Share/AI/Examples/*` 重叠
但不应合并"的量化依据：重叠的另一侧本身就没有消费者。

同理，`Share/Entity/EntityDefinition*` 这一组（第 8.1 节 #6）中
`EntityDefinitionBuilder`、`EntityDefinitionSystem` 已是类型级零引用，
`EntityDefinition`/`EntityDefinitionRegistry`/`IEntityDefinitionHost` 只被该簇内部互相引用——
"Share 侧才是死抽象"的判断成立。

## 15. 已核实的接线缺陷与降级判断（三小节，逐条给出证据）

这两条原本来自子代理报告，已由主代理独立复核。它们直接影响第 10 节待决策的
Projectile 项与"空转系统"项。

### 15.1 投射物当前不会移动

`SpatialSimulationCoordinatorSystem` 构造查询时要求四个组件
（`SpatialSimulationCoordinatorSystem.cs:37-42`）：

```
QueryDescription().WithAll<
    LocationComponent,
    MovementVelocityComponent,
    ColliderComponent,
    GroundingComponent>();
```

而 `ProjectileLifecycleSystem.cs:83-102` 创建投射物实体时给的是：

```
identity, faction, request.Size, location, velocity, request.Collider,
definition.Collision, owner, request.Source, definition.Behavior,
ProjectileBehaviorStateComponent, definition.Kinematics, definition.Lifetime,
ProjectilePenetrationStateComponent, definition.Attributes.Clone(),
ProjectileHitImmunityStateComponent, definition.CollisionPolicy,
ProjectileAttackDefinitionReferenceComponent, disposition
```

`Location`、`Velocity`、`Collider` 都有，**唯独没有 `GroundingComponent`**（也没有
`GravityCapabilityComponent`、`TileCollisionCapabilityComponent`、
`LiquidInteractionCapabilityComponent`、`MovementIntent`）。因此空间协调系统的查询对投射物
一个都不匹配——**投射物实体存在、但不参与任何运动模拟**。

这与"`ProjectileSimulationSystem` 从未注册"是同一结论的两面：投射物运动目前完全缺失，
而那个系统正是没人调用的、Share 空间模拟的平行副本。

### 15.2 两个已注册系统在服务器里永远匹配 0 个实体

`PlayerDeathRespawnSystem` 的查询（`PlayerDeathRespawnSystem.cs:18`）：

```
.WithAll<LifeComponent, PlayerDeathStateComponent, PlayerRespawnStateComponent>();
```

`PlayerCombatResultSystem` 的查询（`PlayerCombatResultSystem.cs:17-18`）：

```
.WithAll<PlayerDeathStateComponent>();
```

而 `Program.cs` 的 `CreatePlayer` 给玩家实体挂的组件里，`LifeComponent` 有，
`PlayerDeathStateComponent` 与 `PlayerRespawnStateComponent` **都没有**。全仓搜索
`new PlayerDeathStateComponent` 只命中 `Build/queueprobe/Program.cs:532,578` 两处探针，
**生产代码从未实例化它**。

结论：这两个系统在探针里被合成实体验证过，但生产组装从未满足其前置条件，所以在服务器里
是纯空转——不报错、不做事。

**这不是死代码，是接线缺口**：正确处理是给玩家实体补挂 `PlayerDeathStateComponent` +
`PlayerRespawnStateComponent`（会启用死亡/重生与玩家伤害结果排空），或者确认这两个系统
不该按这个过滤器工作。两种走法都改变运行时行为，需要先定意图。

### 15.3 包封套的重复头部字段：实测后建议不动

第 8.1 节 #14 提到 `InboundPacketEnvelope` 与 `InboundPacketFrame` 重复保存包头部。
核实结果：

| 类型 | 被引用文件数 | 重复的字段 |
| --- | --- | --- |
| `InboundPacketEnvelope` | 1669 | `MessageId`、`ModuleId`、`RawFrame` |
| `InboundPacketFrame` | 2 | `MessageId`、`ModuleId`、`FrameBytes` |
| `InboundCommunicationPacket` | 13 | 同时持有上述两者 |

`InboundCommunicationPacket` 里同一个 `MessageId`、`ModuleId` 和同一份原始字节确实存了两遍。

但 **`InboundPacketEnvelope` 有 1669 个引用文件**（`src/MinimalImplementation` 的包处理
签名普遍以它作为入参类型）。任何改动它的形状都会波及整个移植层，而重复的三个字段里
`MessageId`/`ModuleId` 只是两个很小的值类型。

**建议：不合并。** 收益（省掉两个字段和一份 `ReadOnlyMemory` 引用）与代价
（触及 1669 个文件、且 `InboundPacketFrame` 的设计意图正是"领域侧不依赖封套"）完全不成比例。
这一项从"重复模型"降级为"已知的有意冗余"，不建议列入改造范围。

另一个佐证：`InboundPacketFrame` 只有 2 个消费者（`InboundCommunicationPacket` 与
`TcpCommunicationHost`），说明它作为独立载体存在的时间不长、耦合很浅；
真要收敛，动它的成本也远低于动封套。

## 16. 投射物子系统整体未接线（定量证据）

第 15.1 节证明了"投射物不会移动"。继续追查生成链路后，结论要更强：
**整个投射物子系统在服务器里是休眠的，一个投射物都不会被创建。**

### 16.1 生命周期入口零调用

`ProjectileLifecycleSystem` 的三个公开方法在全仓（`src` + `Test` + `Build`，排除自身文件）
的外部引用数：

| 方法 | 外部引用 |
| --- | --- |
| `TrySpawn(in ProjectileSpawnRequest)` | **0** |
| `TryTerminate(...)` | **0** |
| `TerminateOwnedBy(...)` | **0** |

并且 `new ProjectileSpawnRequest(...)` 在全仓**没有任何构造点**，连探针都没有——
这个生成请求类型从未被实例化过。

旁证：唯一命中 `.Spawn(` 的地方是
`src/MinimalImplementation/Verification/Mechanisms/Program.cs:1712` 的
`registry.Spawn(...)`，那是 LeashedEntity 注册表，与投射物无关。

`ProjectileLifecycleSystem` 本身在 `Program.cs:114` 被构造、经
`CombatSystemRegistration` 传递，`ProjectileTerminationCommitSystem` 也持有它——
**但整条链上没有任何一处真正调用生成或终止**。这是一个装配完整、调用为零的服务。

### 16.2 对剩余决策的影响

这改变了第 10 节几个待决项的权衡：

- **#2 投射物运动封套**、**#6 `ProjectileDefinition` ↔ Share `EntityDefinition*`**：
  两条路径都是休眠的。也就是说，无论选"合并到 `SpatialSimulationResult<T>`"还是
  "退役 Share 死抽象"，**都没有任何运行时行为需要保持等价**，验证成本大幅低于原先估计。
  但也意味着无法用运行结果反推正确设计——只能靠意图。
- **#12 已撤销**：`ProjectileSourceMetadataComponent` 用 `(string SourceKind, string SourceId,
  long SourceRevision)` 做字符串溯源，`ProjectileOwnerComponent` 用
  `EntityRuntimeHandle` 做实体句柄归属，两者不是同一概念，不构成重复模型。
  唯一成立的是它写入实体后从未被任何代码读取——属于"休眠路径上的死状态"，
  而非"两套模型"。原判断是子代理误判，已撤销。
- **#13 `ProjectileHomingCapabilityComponent`**：只被 `ProjectileHomingAiBehavior` 引用，
  而后者同样未注册——休眠路径内部的自洽引用，不是需要现在处理的重复。

### 16.3 需要提醒的一点

休眠不等于可以随意删除。`ProjectileLifecycleSystem.TrySpawn` 创建的实体已经覆盖了
19 个组件（含 `EntityCollisionCapabilityComponent`、`ProjectileBehaviorProfile`、
`GravityDirection`/kinematics、attack definition 引用等），说明这条路径是按
"接上就能跑"的完整度写的，缺的只是调用方和 `GroundingComponent`。
删除它会丢掉这份已完成的装配，正确动作是接线，不是清理。

## 17. 已执行：剩余两个无需决策的项

做完这两项后，第 10 节表里剩下的每一项都需要产品判断，没有"可以自己决定"的活了。

### 17.1 `WorldMutation` ↔ `LiquidTileChange`：确认不是同一概念

两个类型的字段看起来重叠：

```csharp
// World/Model/WorldMutation.cs（5 个字段）
WorldMutation(WorldTileCoordinate Coordinate, WorldTileState Previous,
              WorldTileState Current, int DropItemType = 0, int DropStack = 1)

// Share/Physics/Liquid/LiquidSimulation.cs:565（3 个字段，internal）
LiquidTileChange(WorldTileCoordinate Coordinate, WorldTileState Previous,
                 WorldTileState Current)
```

前三个字段完全相同，但含义不同：

- `WorldMutation` 是**已提交、待投影给客户端**的变更，因此带掉落信息——客户端要据此显示物品弹出。
- `LiquidTileChange` 是**液面步进传给调用方的日志记录**，意思是"这一步改写了哪些格子"。

调用方 `WorldRuntime.cs:336-340` 负责把后者投影成前者：

```csharp
for (int index = 0; index < changed.Count; index++)
{
  LiquidTileChange change = changed[index];
  RecordMutationUnsafe(change.Coordinate, change.Previous, change.Current);
}
```

合并会让液面逻辑依赖世界投影模型，并被迫携带它既不产生也不关心的掉落字段。
**处理方式：不合并，两侧各加区分注释**，说明为什么前三个字段相同却必须分开。

### 17.2 `PlayerCombatResultSystem` ↔ `NpcCombatResultSystem`：抽出 `CombatResultDrain`

两个系统对"排空本域伤害结果"的定义必须一致——先按实体句柄解析运行时身份，再一次性
取走该身份名下的全部结果，解析不到就当作没有结果（实体可能在两次 tick 之间销毁）。
这段规则此前写了两遍：

```csharp
// Player 侧                       // Npc 侧
if (_handles.TryGetHandle(          if (!_handles.TryGetHandle(
    entity, out var source))            entity, out var source)) return;
{ _ = queue.ConsumeResults(source); } IReadOnlyList<...> results =
                                        queue.ConsumeResults(source);
```

新增 `Share/Combat/CombatResultDrain.cs`（`internal static`，与消费方同程序集），
两处改为单行调用。放在 `Share/Combat` 而非某个域，是因为被排空的队列
`CombatDamageResultQueueComponent` 本来就归 Share 所有。

**行为完全不变**：Player 侧仍然丢弃返回值，Npc 侧仍然遍历结果并写决策。
`queueprobe` 覆盖了两条路径（用带 `PlayerDeathStateComponent` 的合成实体），
重构后 49 条断言全过。

注意：这次只消除了重复实现，**没有修** 15.2 说的接线缺口——
两个系统在生产里依然匹配 0 个实体，那需要先定意图。

## 18. 已执行：投射物接入共享空间管线

第 16 节证明投射物子系统完全休眠。本轮先补上其中可以在 `Terraria.Components` 内完成的一半：
**让 `TrySpawn` 产出的实体真正可被共享空间管线驱动。**

### 18.1 改动

`ProjectileLifecycleSystem.TrySpawn` 在创建实体后追加十项空间组件（`Location`、`Velocity`、
`Collider` 原本就有）：`GroundingComponent.Airborne`、`MovementCapabilityComponent`、
`GravityCapabilityComponent`、`GravityConfigurationComponent`、`GravityDirectionStateComponent`、
`GravityModifierCollectionComponent`、`GravityStateComponent`、`TileCollisionCapabilityComponent`、
`LiquidInteractionCapabilityComponent`、`MovementIntent`。

这样 `SpatialSimulationCoordinatorSystem` 的查询能匹配到投射物，并且回调里六次 `TryGet`
全部命中，投射物由共享管线位移与碰撞，**不需要注册 `ProjectileSimulationSystem` 这份平行副本**——
正符合"用 Share 的组件取代两套模型"的目标。

### 18.2 两个不显然的点

**（一）Arch 的泛型 `Create` 有类型参数上限。** 原调用 19 项已接近上限，直接加到 28 项时
编译器退化到 `ComponentType` 重载，报出 58 条"无法从 X 转换为 ComponentType"的错误。
改法是先 `Create` 原有 19 项，再 `Add` 那十项（放在原来的 try 块内，失败时仍旧销毁实体）。

**（二）`acceleration` 必须为 0，否则投射物会被逐帧减速到静止。**
`SpatialSimulationSystem.ResolveFreeVelocity` 的逻辑是：

```csharp
Vector2 desired = intent.HasDesiredVelocity ? intent.DesiredVelocity : current;
return MoveTowards(current, desired, movement.Acceleration * deltaTime);
```

而 `MovementIntent.Stop(tick)` 的实现是 `new MovementIntent(Vector2.Zero, tick, priority)`，
`hasDesiredVelocity` 取默认值 **true**——也就是说目标速度明确为零，不是"没有意图"。
若 `acceleration > 0`，`MoveTowards` 会把当前速度按每帧 `acceleration * deltaTime` 拉向零，
投射物飞出去就停住了。取 `acceleration = 0` 时移动距离为 0，速度原样保留，正好是匀速直线。

同时用 `MovementMode.Flight`：`SpatialSimulationSystem` 只在
`gravityCapability.Enabled && movement.Mode != MovementMode.Flight` 时施加重力，
Flight 模式直接跳过，投射物不会下坠。`maximumSpeed` 取 0 让钳制分支整体失效
（代码只在 `> 0` 时生效），避免快投射物被限速。

这三个取值都由 `Test/Terraria.Projectile.Verification` 逐条断言，其中
`intent-is-stop-with-zero-desired` 特意固定了"`Stop` 的目标速度是零"这一前提，
防止有人日后改掉它而让减速 bug 悄悄复活。

### 18.3 仍然缺的一半：没有生产者

以上只解决了"生成出来的投射物能飞"。**`TrySpawn` 至今零外部调用**（第 16.1 节），
所以服务器里仍然一个投射物都不会被创建。补生产者需要决定"哪些协议事件应当生成投射物"，
这落在 `Terraria.Components` 之外的适配层（`MinimalImplementation` 的 `ProjectileManager`
负责处理投射物包），属于需要产品判断的一步，本轮未做。

逐类型重力也仍然缺席：`ProjectileDefinition` 没有任何重力或瓦片碰撞内容字段，
现在所有投射物一律匀速直线。要做箭矢抛物线，需要先给定义加字段——这是第 10 节 #6
待决策项的一部分。

**补充（见第 19 节）**：生产者缺席的根因不是"漏了调用"，而是
`MinimalImplementation.Combat.Projectiles` 里已经有一套正在跑的完整投射物实现。
在迁移边界确定之前贸然接生产者，会让系统从两套变成三套。

### 18.4 顺带消除：死掉的 `Terraria.Combat.VelocityComponent` 兼容垫片

写验证时编译器报出
`"VelocityComponent"是"Terraria.Combat.VelocityComponent"和"Terraria.Movement.VelocityComponent"之间的不明确的引用`。
查出 `Share/Physics/Movement/CombatVelocityComponent.cs` 里确实还有第二个 `VelocityComponent`：

```csharp
[Obsolete("Use Terraria.Movement.VelocityComponent for authoritative spatial state.")]
public readonly record struct VelocityComponent(Vector2 LinearVelocity)
{
  public Terraria.Movement.VelocityComponent ToMovementVelocity() { ... }
}
```

它的文档注释自称是"Compatibility projection for callers that still use the former velocity namespace"。
即这是一个**显式声明的重复类型**，为迁移期兼容而保留。

核实消费者（三条独立证据）：

1. `ToMovementVelocity` 在全仓零引用。
2. `Terraria.Components` 全量重建（`-t:Rebuild`）**0 条 CS0618**——使用 `[Obsolete]` 类型必然告警。
3. `NSSLC.Server`、`MinimalVerification`、`queueprobe` 全部重建同样 0 条 CS0618。

并且全仓没有任何 `#pragma warning disable CS0618` 或 `NoWarn` 屏蔽，证据链完整。

结论：这个兼容垫片已经没有任何调用方，属于"为兼容而保留、但兼容对象早已消失"的死类型，
**已删除**。这是第 6 组被消除的重复模型，也是唯一一组由代码自身显式声明为重复的。
删除后全部工程 0 错误，四个验证全过。

`SpatialSimulationCoordinatorSystem` 等处的
`using MovementVelocityComponent = Terraria.Movement.VelocityComponent;` 别名写法
现在已无必要（歧义源已消失），但保留它没有成本，本轮未动，避免和主线改动混在一起。

## 19. 投射物"两套模型"的真实边界（重要修正）

第 18.3 节说"还缺生产者"。追查生产者落在何处时，发现了比"缺一个调用方"更根本的情况。

### 19.1 存在两个完整的投射物子系统

| | `Terraria.Projectile`（`src/NSSLC/Components/Projectile`） | `MinimalImplementation.Combat.Projectiles` |
| --- | --- | --- |
| 文件数 | 43 | 62 |
| 状态 | **完全未接线**：`TrySpawn` 零调用（第 16.1 节） | **在跑**：包处理、同步、运动、碰撞、命中、追踪齐全 |
| 关键类型 | `ProjectileLifecycleSystem`、`ProjectileSimulationSystem`、`ProjectileDefinition`、`ProjectileDefinitionCatalog` | `ProjectileManager`、`ProjectileMovementService`、`ProjectileCollisionService`、`ProjectileNpcCombatService`、同名 `ProjectileDefinition`、同名 `ProjectileDefinitionCatalog` |
| 命名空间 | `Terraria.Projectile` | `MinimalImplementation.Combat.Projectiles` |

两边的 `ProjectileDefinition`、`ProjectileDefinitionCatalog` 同名同职责，但是不同的类型、不同的程序集。

### 19.2 这意味着什么

1. **组件域内的"投射物两套模型"（第 8.1 节 #2、#6）不是主要矛盾。**
   真正的两套是"旧实现在跑、新域没接"，中间横着迁移边界。把组件域的投射物接上生产者，
   如果不以退役 `MinimalImplementation` 版本为前提，结果会是**三套**并存而不是一套。

2. **第 18 节的接线仍然有价值，但它证明的是"接上就能跑"，不是"已经接上"。**
   现在的能力是：任何调用方调用 `TrySpawn`，拿到的投射物都会被共享空间管线正确地
   位移与碰撞。生产者缺席是迁移决策，不是缺陷修复。

3. **这也是为什么 `ProjectileSimulationSystem` 这份平行副本一直没被注册。**
   它不是"忘了注册"，而是在等迁移方式确定：要么把它的职责并入共享
   `SpatialSimulationCoordinatorSystem`（第 18 节已证明可行），要么随旧实现一起退役。

### 19.3 由此调整的建议

第 10 节里 #2（运动封套）与 #6（`ProjectileDefinition` ↔ Share `EntityDefinition*`）的
真正前置问题不是"合并到哪边"，而是：

> 组件域的投射物是否要取代 `MinimalImplementation.Combat.Projectiles`？

- 若**要取代**：则应先把旧实现的包处理/同步/运动逐个迁到组件域，`ProjectileSimulationSystem`
  与 Share 空间管线的取舍在迁移中自然确定。
- 若**暂不取代**：组件域的投射物就是"已具备能力的备用实现"，`TrySpawn` 零调用是合理状态，
  不必急着接生产者；此时任何合并都缺乏验证手段。

这个判断无法从代码推断，需要产品意图。

## 20. 已执行：投射物迁移第 1 步（生成接缝）

方向已定：**组件域投射物取代 `MinimalImplementation.Combat.Projectiles`，分步迁移。**
第 1 步只建立接缝，不改任何现有行为——旧实现照旧在跑，组件域获得一个可被驱动、
可被验证的入口。

### 20.1 改动

新增三个文件（照 `ProjectileTerminationQueue*` 的同形模式）：

- `ProjectileSpawnQueueComponent`：tick 作用域的生成请求队列，`Enqueue`/`Drain`/`BeginTick`，
  挂在一个专职的世界级实体上。
- `ProjectileSpawnQueue`：`internal static Require(world)`，经 `SingletonComponent<T>` 定位。
- `ProjectileSpawnRequestSystem`：排空队列并逐条调用 `TrySpawn`，结果保留在
  `Results` 里，失败不抛出也不静默丢弃。

接线两处：`CombatSystemRegistration` 以 **Preparation** 相位注册该系统
（生成的实体当帧即可被 Simulation 相位的空间协调系统驱动）；
`Program.cs` 在终止队列实体旁创建 `ProjectileSpawnQueueComponent` 实体。

### 20.2 这一步的边界

- **没有**改变任何现有行为：旧 `ProjectileManager` 仍是唯一真正的生成方，无人向新队列入队。
- **没有**动 `ProjectileSimulationSystem` 的去留——等运动职责迁移时再定（第 19.3 节）。
- 现在的能力：任何一方调用 `ProjectileSpawnQueue.Enqueue(...)`，当帧就会得到装配完整、
  可被共享管线驱动的投射物实体。这为下一步"旧实现双写"提供了落点。

### 20.3 验证

`Test/Terraria.Projectile.Verification` 从 18 条扩到 **22 条**，新增四条覆盖接缝：

```
PASS queue-accepts | count=1
PASS queue-spawned-one | results=1 status=Applied
PASS queue-drained | count=0
PASS queued-entity-simulatable | Grounding + MovementCapability
```

最后一条是关键：接缝产出的实体必须同样可模拟，否则"接上生产者"只是把不生效挪了个地方。

回归：全部工程 0 错误；`queueprobe` 49 PASS；`invVerification` 16 PASS；
`worldprobe` 195 行与基线一致；`tickprobe` 8 行不变。

### 20.4 后续步骤（按依赖顺序）

1. **旧实现双写**：`MinimalImplementation.ProjectileManager.Commit`（生成提交处）在成功后
   向新队列入队，让组件域开始接收真实流量，同时两个模型可以对照。
2. **同步/投影迁移**：`ProjectileView`/`ProjectileSyncPreparation` 的职责搬到组件域。
3. **运动迁移**：确定 `ProjectileSimulationSystem` 的下场——并入共享
   `SpatialSimulationCoordinatorSystem`（第 18 节已证明可行）或保留并注册。
4. **退役旧实现**：以上对齐后删除 `MinimalImplementation.Combat.Projectiles`。

每一步都应保持"旧路径仍可回退"，直到第 4 步。

## 21. 自我纠错：第 18 节的接线其实只在第 0 帧有效

第 18 节我声称"投射物接上就能跑"，依据是组件齐备的断言。本轮把断言升级为**真实位移验证**后，
第一次运行就红了：

```
FAIL coordinator-moved-projectile | moveCount=0 before=(100,200) after=(100,200)
FAIL movement-is-velocity-times-dt | expected=(103,198.5) actual=(100,200)
```

`moveCount = 0` 说明协调系统连 `Move` 都没调用——查询虽然匹配到了实体，但模拟被前置校验拦下。

### 21.1 根因

`SpatialSimulationSystem.Simulate` 的第一道校验（`SpatialSimulationSystem.cs:26`）：

```csharp
if (intent.Tick != frame.Tick)
{
  return Rejected(..., SpatialSimulationStatus.Conflict,
      "The movement intent belongs to another simulation tick.");
}
```

而我在 `TrySpawn` 里写的是 `MovementIntent.Stop(tick: 0)`。**意图 tick 恒为 0，于是投射物
只在 tick 0 那一帧能被模拟，其它每一帧都被拒绝。** 换句话说，第 18 节的接线在真实服务器里
（tick 从 1 开始递增）等于完全没接。

### 21.2 为什么 NPC 没这个问题

NPC 侧有 `NpcAiIntentAdapter.cs:151` 每帧写入 `MovementIntent.Stop(tick)`，玩家侧由输入写入。
投射物没有任何意图生产者——它压根不是意图驱动的。缺的不是一次赋值，而是"每帧都要有"。

### 21.3 修法

新增 `ProjectileMovementIntentSystem`（Preparation 相位）：每个 tick 为所有
`disposition.IsActive` 且带 `GroundingComponent` 的投射物写入 `MovementIntent.Stop(tick)`。

注册顺序上**必须排在生成系统之后**，这样同帧新生成的投射物也能拿到当帧意图，
当帧就能被 Simulation 相位驱动。

这里写 `Stop` 仍是刻意的：投射物不追求目标速度，配合生成时的 `acceleration = 0` 与
`MovementMode.Flight`，速度原样保留，表现为匀速直线。

### 21.4 修正后的验证

`projVerification` 从 22 条扩到 **26 条**，其中四条是针对这个缺陷的：

```
PASS intent-refreshed-to-current-tick | intentTick=8
PASS coordinator-moved-projectile | moveCount=2 before=(100,200) after=(103,198.5)
PASS movement-is-velocity-times-dt | expected=(103,198.5) actual=(103,198.5)
PASS velocity-unchanged-no-deceleration | velocity=(6,-3)
```

位移恰好等于 `速度 × deltaTime`（`(6,-3) × 0.5`），且速度未被改变——同时反证了
第 18.2 节"`acceleration` 必须为 0"那个决定：若加速度非零，速度会被拉向
`MovementIntent.Stop` 的零目标，速度断言会红。

### 21.5 教训

"组件齐备"不等于"能跑"。本轮之前的所有断言都是在检查**静态结构**，而缺陷在**动态前置条件**里。
只有真正跑一次模拟、比较位置与速度，才能发现意图 tick 这种跨帧状态问题。
后续迁移步骤（同步/投影、运动职责）必须同样以"跑一遍并比对结果"为准，不能停在结构断言。

## 22. 已执行：发布公开入队接缝，并厘清第 2 步的依赖边界

第 20.4 节把"旧实现双写"列为迁移第 2 步。本轮先补齐它**必需的缺失件**，同时查清了
它真正的依赖代价。

### 22.1 公开接缝

新增 `ProjectileSpawnRequests`（`public static`）：

```csharp
public static void Enqueue(ArchWorld world, in ProjectileSpawnRequest request, long tick)
```

原因是现有入队路径不可用：队列组件挂在专职世界级实体上，定位它的
`ProjectileSpawnQueue` 是 `internal`，外部程序集不该依赖这个单例定位约定。
现在外部只需 `world` + 请求 + tick 即可入队，负 tick 会抛 `ArgumentOutOfRangeException`
而不是静默入队。

这与仓库既有约定一致：**组件库发布能力，宿主决定接线**。

### 22.2 第 2 步的依赖代价（实测）

`MinimalImplementation.Combat.csproj` 的全部 ProjectReference 只有一条：

```xml
<ProjectReference Include="..\Contracts\MinimalImplementation.Contracts.csproj" />
```

**它不引用 `Terraria.Components`。** 因此"让 `ProjectileManager.Commit` 直接向新队列入队"
意味着给旧移植层新增一条指向新 ECS 领域的项目引用——这是 legacy → new 的依赖方向，
属于架构边界决策，不是一次普通改动。

对照：`NSSLC.Server.csproj` **同时**引用了 `Terraria.Components` 与
`MinimalImplementation.World` / `Contracts`，即宿主层本来就横跨两边。

### 22.3 由此得出的建议

第 2 步不要改 `MinimalImplementation.Combat`，改为**在宿主层架桥**：

- `MinimalImplementation.Combat` 保持零新依赖（旧层不需要知道 ECS 域存在）。
- 由 `NSSLC.Server` 侧观察旧实现的生成提交，转调 `ProjectileSpawnRequests.Enqueue`。
- 这样"双写"成为宿主的一个接线动作，可随时摘除，符合第 20.4 节
  "每一步都保持旧路径可回退"的要求。

具体落点需要先弄清 `ProjectileManager.Commit` 的调用链在服务器里由谁驱动
（`MinimalImplementation.Server` 还是 `NSSLC.Server` 的适配层），这属于第 2 步的实现细节，
本轮未展开。

### 22.4 验证

`projVerification` 从 26 条扩到 **30 条**，新增四条覆盖公开接缝：

```
PASS public-seam-enqueues | count=1
PASS public-seam-spawned | results=1 status=Applied
PASS public-seam-drained | count=0
PASS public-seam-rejects-negative-tick | threw=True
```

回归：全部工程 0 错误；`queueprobe` 49 PASS；`invVerification` 16 PASS；
`worldprobe` 195 行与基线一致；`MinimalVerification` PASS。

## 23. 已执行：投射物碰撞策略去掉重复的能力声明

`ProjectileCollisionPolicyComponent` 原本有四个标志，实测读取方如下：

| 标志 | 读取方 |
| --- | --- |
| `CollideWithTiles` | `ProjectileSimulationSystem.cs:94` |
| `IgnoreOwner` | `ProjectileDamageRequestSystem.cs:74` |
| `CollideWithEntities` | **无**（只有声明与构造处） |
| `IgnoreSameFaction` | **无**（只有声明与构造处） |

后两个是把别处已有的模型在投射物实例上又声明了一遍：

- `CollideWithEntities` 与共享的 `EntityCollisionCapabilityComponent` 重复。而且
  `ProjectileDefinition` 本来就**单独**携带后者（`Collision` 属性），也就是说同一份
  "是否参与实体碰撞"在定义与实例上各有一个真相来源。
- `IgnoreSameFaction` 属于阵营关系判定，归 `Terraria.Combat.DamageRelationSystem` 负责。
  让每个投射物实例自己声明，等于把一处全局规则复制成 N 份实例状态。

两个字段都已删除，策略类型现在只剩 `CollideWithTiles` 与 `IgnoreOwner` 两个投射物特有判定。
这是第 7 组被消除的重复模型。

**行为不变的依据**：两者在全仓（`src` + `Test` + `Build`）除声明与构造处外零出现，
没有任何读取路径，因此删除不可能改变运行结果。

验证：全部工程 0 错误；`queueprobe` 49 PASS；`invVerification` 16 PASS；
`projVerification` 30 PASS；`worldprobe` 195 行与基线一致；`tickprobe` 8 行不变；
`MinimalVerification` PASS。

## 24. 结构级重复的独立复查：零可合并项

第 1–18 节都是按**名字与语义**逐个读文件的审计。为验证"没有漏掉改过名字的同一模型"，
本轮换了独立的第二种查法：**按字段形状聚类**——不管叫什么，字段类型序列完全相同就成组。

### 24.1 方法

- **主构造函数式**（`public record struct X(...)`）：43 个 ≥2 字段的组件。
- **属性式**（`public T Prop { get; }`）：67 个 ≥2 属性的组件。

两次扫描各自的输出按规范化后的类型序列分组。

### 24.2 结果：5 组形状相同，全部不合并

| 形状 | 组内类型 | 判定 |
| --- | --- | --- |
| `bool\|bool` | `Projectile.ProjectileCollisionPolicyComponent` / `Share.GravityCapabilityComponent` | 形状偶然相同，语义无关（碰撞策略 vs 重力能力）。**不合并** |
| `EntityRuntimeHandle\|long` | `ProjectileHitImmunityStateComponent` / `ProjectileHitImmunity` | 同一文件内的嵌套值记录，前者持有后者列表，不是两个组件。**误报** |
| `int\|int` | `ProjectileKinematicsStateComponent` / `EntitySizeComponent` / `WorldTileStorageComponent` | `(ExtraUpdates, Substep)` vs `(宽, 高)` vs 瓦片存储维度，语义无关。**误报** |
| `LiquidContactFlags\|byte` | `Share.LiquidContactComponent` / `Share.SpatialLiquidContact` | 后者是世界查询边界的输出 DTO，且带显式 `ToComponent()`。属于刻意的查询边界类型。**不合并** |
| `Vector2\|Vector2` | `Share.DamageContact` / `Share.EntityCollisionContact` | 见 24.3。**不合并** |

### 24.3 `DamageContact` 与 `EntityCollisionContact` 值得单独说明

两者形状与语义都是"接触点 + 法线"，且确实存在重复代价——`ProjectileDamageRequestSystem.cs:91`
正在逐字段搬运：

```csharp
new DamageContact(fact.Contact.Point, fact.Contact.Normal)
```

看起来该合并。但实测依赖方向：

- `Share/Combat` 下对 `Terraria.Physics` 的引用：**0 处**
- `Share/Physics` 下对 `Terraria.Combat` 的引用：**0 处**

两层是**刻意互相解耦**的。合并会凭空造出一条 `Terraria.Combat → Terraria.Physics` 的层间依赖。
而那条逐字段搬运恰好位于非 Share 的 `Terraria.Projectile` 里——**适配器负责跨域翻译**，
这正是六边形边界要求的职责分配。所以它是解耦的代价，不是重复实现。

### 24.4 结论

结构级复查**没有发现任何被名字掩盖的遗漏**。第 10 节列出的剩余项全部是语义或产品判断
（运动封套、世界掉落物、内容定义模型、零实体接线、制导能力、包封套），不是漏掉的结构重复。

这也给出一条可复用的判据：**形状相同 ≠ 重复**。判定必须同时满足
同语义、同生命周期、且合并后不新增层间依赖。
