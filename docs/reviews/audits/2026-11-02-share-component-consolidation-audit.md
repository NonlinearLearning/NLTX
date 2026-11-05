# Share 组件收敛审计（非 Share 域 → Share 复用）

**范围**：`src/NSSLC/Components/` 下非 `Share/` 的六个域（`Npc` 12、`Player` 17、
`Projectile` 43、`World` 38、`Communication` 19、`Diagnostics` 6，共 135 个文件、145 个类型），
对照 `Share/` 七个域（`AI` 154、`Attributes` 37、`Combat` 35、`Entity` 22、`Inventory` 10、
`Item` 9、`Physics` 56，共 323 个文件）。

**目的**：找出哪些非 Share 组件应当改用 Share 的组件，消除重复模型。

> 状态：审计已完成（32 节）。三个子代理的逐文件结果均已并入并逐条复核。
> 累计消除 7 组重复模型与 5 份重复助手实现，修复 2 项缺陷，
> 并用四种独立方法（语义审计、字段形状聚类、同名助手统计、零引用+同成员枚举）交叉验证无遗漏。

## 0. 决策结果与当前状态

本文件较长（34 节），这一节是给决策人的摘要。

> **本节四项待决事项均已给出决策并落地**，逐项结果见 §34。
> 下面两张表保留为当时的决策依据记录，不再代表当前状态。

### 0.1 需要决策的两项（会改变行为或架构）

| # | 事项 | 两个选项 | 影响 | 建议 |
| --- | --- | --- | --- | --- |
| **#4** | 把 `WorldItemState` 的 `(int ItemType, int Stack, byte Prefix)` 换成 `Share/Item/ItemStack` | **A**：先在 `GameCommunicationAdapter` 入口用 `ItemIdRules.TryFromNetworkType` 校验、未知 id 丢弃，再合并<br>**B**：放宽 `ItemStack` 允许未知 id | A 会收紧协议宽容度：客户端发未知 itemType 的掉落物请求将被丢弃<br>B 会为迁就旧路径污染共享模型 | **A**（详见 §32） |
| **#6** | `Share/Entity/EntityDefinition*`（8 个类型，目录外零引用）与 `ProjectileDefinition` 并存 | **A**：补 `IEntityDefinitionHost` 宿主，把玩家/NPC 创建改为数据驱动，再收编 `ProjectileDefinition`<br>**B**：整簇删除，并把架构矩阵里 `confirmed` 标记改为退役 | A 工作量大，但真正消除双模型<br>B 放弃数据驱动创建能力，代码立刻一致 | 需产品定夺。架构矩阵已标 `confirmed`，倾向 **A**（详见 §30） |

两项都**不能只做一半**：#6 的簇内部互相引用，半删会让状态更差（既有审计已警告）。

### 0.2 需要玩法决策的两项（不是重复模型）

| 事项 | 现象 | 需要定的事 | 依据 |
| --- | --- | --- | --- |
| `InventoryAttributeContributionSystem` | 系统实现完整，但从未注册 | 护甲/饰品/武器的属性贡献是否现在生效 | §26 |
| `ProtectionWindowSystem` | 已注册，但 `ProtectionWindowComponent` 全仓无任何挂载点 | 无敌窗口何时授予（受击后 / 重生后 / 不授予） | §26 |

这两项**不造成危害**，只是功能未启用，因此没有像 §25 那条一样直接修。

### 0.3 已查明、不需要决策的（供参考，无需回复）

- **#11 / #12 / #13 均已证明不是重复模型**：`SlimeAiBehavior` 两侧基类与意图类型都不同；
  `ProjectileSourceMetadataComponent` 与 `ProjectileOwnerComponent` 是不同概念；
  `ProjectileHomingCapabilityComponent` 与 `ProjectileBehaviorProfile` 是"配置优先、能力兜底"的共存设计。
- **投射物子步未生效**：`extraUpdates` 目前不影响总位移（只影响碰撞精度），
  已用 4 条特征化断言锁定，待并入共享管线时会自动变红提醒（§29）。
- **#14 包封套**：`InboundPacketEnvelope`/`InboundPacketFrame` 的重复字段属已知有意冗余（§15.3）。
- **#10 的两个空转系统**：已在 §25 修复并经过服务器实跑验证。

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
| 10 | `PlayerCombatResultSystem` ↔ `NpcCombatResultSystem` | **已消除且接线已修**（第 17.2 节抽出 `CombatResultDrain`；第 25 节补挂玩家死亡/重生状态，两个系统不再匹配 0 实体） |
| 11 | `Npc/SlimeAiBehavior` ↔ `Share/AI/Examples/SlimeAiBehavior` | **判断需修正**（第 31.1 节）：基类与意图类型都不同（`AiBehavior<NpcAiIntent>` vs `HtnAiBehavior<SlimeContext, ExampleAiIntent>`），Examples 侧是 HTN 子系统的演示且被 `MinimalVerification` 使用，不是重复实现 |
| 12 | ~~`ProjectileSourceMetadataComponent` ↔ `ProjectileOwnerComponent`~~ | **判断错误，已撤销**：前者是字符串溯源 `(SourceKind, SourceId, SourceRevision)`，后者是 `EntityRuntimeHandle`，不同概念，不是重复模型。仅"写入后从未被读取"成立，见 16.2 |
| 13 | `ProjectileHomingCapabilityComponent` ↔ `ProjectileBehaviorProfile` | **已查明机制**（第 31.2 节）：`ProjectileHomingAiBehavior` 在 `snapshot.Get` 与 `requirements.Contains` 两处都要求该组件，而全仓无人挂载，因此即使接上 AI 注册制导也不生效 |
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
两个系统在生产里依然匹配 0 个实体——已在第 25 节修复（玩家实体补挂死亡与重生状态）。

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

## 25. 已执行：修复审计 #10 的接线缺口（玩家伤害结果队列无界增长）

15.2 节记录了两个已注册但查询永远匹配 0 实体的系统。本轮把成因查到了底并修掉。

### 25.1 成因

`Program.cs` 的 `CreatePlayer` 挂了 `LifeComponent(current: 100, maximum: 100)`，
但**没有挂** `PlayerDeathStateComponent` 与 `PlayerRespawnStateComponent`。而：

- `PlayerDeathRespawnSystem` 的查询是 `WithAll<LifeComponent, PlayerDeathStateComponent, PlayerRespawnStateComponent>()`
- `PlayerCombatResultSystem` 的查询是 `WithAll<PlayerDeathStateComponent>()`

两个查询都要求玩家身上不存在的组件，于是永远匹配 0 个实体。

### 25.2 后果：不是"功能没开"，是内存无界增长

`PlayerCombatResultSystem` 的循环体是：

```csharp
_ = CombatResultDrain.Drain(resultQueue, _handles, entity);
```

它**只排空、不施加伤害**（结果被丢弃）。所以这个系统不跑并不会"少扣血"，
而是 `CombatDamageResultQueueComponent` 里的玩家伤害结果**永不排空**——
对局越久堆得越多。这是真实的内存泄漏，不只是功能缺失。

`PlayerDeathRespawnSystem` 的缺失则确实只是功能未生效。

### 25.3 修法

`CreatePlayer` 补挂两项，并新增常量：

```csharp
private const int PlayerRespawnDelayTicks = 600;   // 10 秒，与参考客户端默认重生时间一致
...
new PlayerDeathStateComponent(),
new PlayerRespawnStateComponent(PlayerRespawnDelayTicks),
```

安全性依据：对**存活**玩家，`PlayerDeathRespawnSystem` 三个分支都是提前返回
（`!life.IsAlive && !death.IsDead` 为假；`life.IsAlive && death.IsDead` 为假；
`!death.IsDead` 为真直接返回），因此正常帧是空操作，只在 life 归零后才登记死亡与重生计划。

同一个 `CreatePlayer` 里已有同类先例的注释——`AttributeTickSystem` 与 `BuffTickSystem`
曾因缺组件而匹配 0 实体，说明"补组件让已注册系统生效"是本仓库既定的维护方式。

### 25.4 验证

- **真实运行**：用 `Build/diagnostics/.../SC091/World/primary.wld` 作为世界文件启动服务器，
  连续运行 20 秒无未处理异常，正常输出 `ecs.npc.spawned` → `ecs.protocol.world_cache_ready`
  → `ecs.server.ready`。说明两个系统在 tick 循环里对队列实体的 `Require` 都成功。
- **系统行为**：`queueprobe` 已有四条相关断言覆盖死亡与重生语义（本轮全部通过）：
  ```
  PASS respawn-restore-same-tick | t1.life=0 t1.dead=True t2.life=100 t2.deadCleared=True t2.respawnCancelled=True
  PASS respawn-restore-needs-order | 顺序颠倒后 t2.life=0（证明同帧保证来自注册顺序）t3.life=100
  ```
- **回归**：全部工程 0 错误；`queueprobe` 49 PASS；`invVerification` 16 PASS；
  `projVerification` 30 PASS；`worldprobe` 195 行与基线一致；`tickprobe` 8 行不变；
  `MinimalVerification` PASS。

### 25.5 说明

这一项是**缺陷修复**，不是重复模型消除。它之所以留在审计里，是因为 #10 的重复实现
（`PlayerCombatResultSystem` ↔ `NpcCombatResultSystem`）虽已在 17.2 节抽出
`CombatResultDrain` 消除，但接线缺口会让那次消除无法在真实服务器里生效——
重复代码删了、生产路径却从未执行过，等于没验证。

## 26. 系统接线全量核对：27 个系统，2 处未接通

第 25 节修掉了两个"已注册但匹配 0 实体"的系统。本轮把这套检查推广到**全部**系统，
确认是否还有同类。

### 26.1 方法

1. 枚举 `Terraria.Components` 下所有 `BaseSystem<ArchWorld, EcsServerTickContext>` 派生类：**27 个**。
2. 定位注册点。共有三处，其中不少条目是变量透传而非 `new`，只看 `new XxxSystem(` 会漏：
   - `Program.cs`：13 处（含 `npcProjection`、`worldTick`、`npcAiAdapter`、`gravityTick`、
     `attributeTick`、`buffTick`、`spatialSimulation`、`worldInteractions` 等透传变量）
   - `CombatSystemRegistration.cs`：15 处
   - `CommunicationSystemRegistration.cs`：3 处
3. 对已注册系统，验证其查询所需组件是否在生产代码里真的被挂到实体上。

### 26.2 结论

**26 个已注册，1 个从未注册。** 已注册的里 1 个永远匹配不到实体。

| 系统 | 状态 | 后果 |
| --- | --- | --- |
| `InventoryAttributeContributionSystem`（Share/Inventory） | **从未注册** | 每 tick 从库存重建装备/饰品/武器的属性贡献这段逻辑永不执行，`AttributeSourceCategory.Equipment / Accessory / Weapon` 的加成永远不会生效 |
| `ProtectionWindowSystem`（Share/Combat，注册在 Commit） | 已注册但**所需组件从不挂载** | `ProtectionWindowComponent` 在生产代码里只有读取方（`CombatDamageSystem.cs:107`、`DamageAdmissionSystem` 的可选参数），没有任何挂载点，因此该查询恒为 0 实体，无敌窗口既不会创建也不会过期 |

其余 25 个系统的查询所需组件都能在真实实体上凑齐（玩家、NPC、投射物或专职队列实体）。

### 26.3 为什么本轮没有直接补

两条都**不造成危害**——不像第 25 节那条会无界堆积。不运行它们的效果只是"功能未启用"，
属于迁移期的正常状态，而不是缺陷：

- 补 `InventoryAttributeContributionSystem` 会让护甲/饰品真正参与属性计算，这是玩法生效，
  需要确认属性重建与 `AttributeTickSystem` 的顺序约定。
- 补 `ProtectionWindowComponent` 需要先定义"什么时候授予无敌窗口"（受击后？重生后？），
  这是玩法规则。

两者都需要产品判断，故只记录不执行。

### 26.4 方法修正（避免误报）

有些系统有**多个查询**，例如 `PhysicsContactCaptureSystem` 用 `_query`（实体组件）与
`_queueQuery`（队列组件）两个独立查询。若按"文件里出现过的所有 `WithAll`"粗暴合并，
会把队列组件误当成实体必须同时具备的组件，从而误报"永远匹配 0 实体"。
判定必须**按查询逐个**进行。

## 27. 已执行：同名助手重复实现的消除

第 1 轮消除的 `ApplyGravity` 重复说明这类问题真实存在。本轮用机械方式把这一类扫了一遍：
**统计所有 `private`/`internal static` 助手的方法名，找出在多个文件里重复定义的**。

### 27.1 扫描结果

| 助手 | 重复处数 | 分布 | 处理 |
| --- | --- | --- | --- |
| `IsFinite(Vector2)` | **4 份逐字节相同** | `Communication/GameCommunicationAdapter`、`Projectile/ProjectileSimulationSystem`、`Share/Physics/Liquid/LiquidSimulation`、`World/Solver/WorldTileMotionSolver` | **已消除** |
| `MoveTowards` | **2 份逐字节相同** | `Projectile/ProjectileHomingAiBehavior`、`Share/Physics/SpatialSimulation/SpatialSimulationSystem` | **已消除** |
| `IsFinite(Vector3)` | 1 份 | `Share/AI/Goap/Resolver/GraphResolver` | 不合并（类型不同） |
| `IsDefined` | 2 份 | `Share/GravityConfigurationComponent`、`Share/GravityDirectionStateComponent` | 保留（各自校验各自的重力方向成员，语义不同） |
| `SelectOverride`、`ToBoundedInt`、`Validate` | 各 2 份 | 均在 Share 内部 | 保留（同名但校验对象不同） |

### 27.2 改动

新增 `Share/Physics/SimulationVector.cs`（`internal static class`，两个方法），
6 个文件删除各自的私有副本并改为调用它。命名上刻意避开 `Helper`/`Utils`/`Math` 这类
无语义词，遵守文件组织约束里"避免无语义泛化命名"的要求。

公式与原来**逐字符一致**，包括 `length <= distance || length == 0f` 的比较顺序与
`current + delta / length * distance` 的运算顺序，因此浮点结果不会变化。

规模：6 个文件，`+7 / −37`，另加 1 个新文件。

### 27.3 为什么这算"非 Share 使用 Share 的产物"

`MoveTowards` 原本在 `Projectile` 域与 `Share/Physics` 里各有一份——同一份运动学语义
被投射物域重新实现了一次。消除后，非 Share 的 `ProjectileHomingAiBehavior` 直接复用
Share 物理域的实现，这正是目标里"非 Share 的组件是否可以使用 Share 下的组件"在代码级的表现。

### 27.4 验证

行为不变由两点保证：公式逐字符相同；全部回归通过。

- 全部工程 0 错误
- `queueprobe` 49 PASS / 0 FAIL
- `invVerification` 16 PASS / 0 FAIL
- `projVerification` 30 PASS / 0 FAIL
- `worldprobe` 195 行与基线完全一致
- `tickprobe` 8 行不变

## 28. 第三与第四种独立复查：零引用类型、同成员枚举

第 24 节用"字段形状聚类"做过一次独立复查。本轮再做两种，方法与结论如下。

### 28.1 零引用公开类型扫描

对 `Components` 下非 Share 的 **137 个公开类型**逐个统计全仓（`src` + `Test` + `Build`，
共 3082 个 `.cs`）引用，排除自身声明文件。结果只有 3 个零引用：

| 类型 | 判定 |
| --- | --- |
| `ProjectileAiRegistration` | 投射物 AI 的绑定入口。迁移未走到 AI 步骤，属"能力已写、尚未接"，不删 |
| `ProjectileBounceStateComponent` | 弹跳状态组件，无人挂载也无人读取。属投射物特性集的一部分，迁移期间保留 |
| `ProjectileHitImmunity` | **误报**：它与 `ProjectileHitImmunityStateComponent` 同文件，被后者持有 |

三个都不是重复模型，因此本轮不删任何类型。这里刻意保守——第 1 轮我曾把
`PlayerAttackIntentAdapter` 误判为死代码删除，后查明它是外部程序集的公开接缝。

### 28.2 同成员枚举扫描

50 个枚举按成员名分组，只有一组重合：`WorldInteractionOperation` 与
`WorldInteractionPermission` 成员名都是 `None / ReadTile / BreakTile / PlaceTile`。

看似重复，实测**不是**：

```csharp
public enum WorldInteractionOperation : byte { None = 0, ReadTile = 1, BreakTile = 2, PlaceTile = 3 }   // 有序选择器
[Flags] public enum WorldInteractionPermission : byte { None = 0, ReadTile = 1, BreakTile = 2, PlaceTile = 4 }  // 位掩码
```

- 前者表达"这一次请求要做什么"，是单值选择器，值连续，适合 switch 与序列化。
- 后者表达"这个实体被允许做什么"，是位掩码，`WorldInteractionCapabilityComponent.Allows` 按位判断。
- 数值从第三个成员起就分道扬镳（3 vs 4），合并任何一方都会破坏另一方的前提。
- `WorldInteractionSystem` 第 76/87/100 行做"操作 → 权限"的显式映射，那是两个概念的翻译，
  不是重复实现。

判定：保留两者。与第 18 节 `WorldMutation` / `LiquidTileChange` 的结论同类——
**同名成员不等于同一概念**。

### 28.3 审计文档已登记进清单

`docs/document-manifest.tsv` 第 421 行新增本审计条目，字段数与全表一致（11 列），
`migrationAction` 记为 `created-2026-11-02`。diff 为纯新增 1 行，无删改。

### 28.4 四种独立方法的一致结论

| 方法 | 节 | 结论 |
| --- | --- | --- |
| 逐文件语义审计 | 1–18 | 13 组候选，7 组已消除，6 组需决策 |
| 字段形状聚类 | 24 | 5 组形状相同，全部不构成可合并 |
| 同名 static 助手统计 | 27 | 2 个助手共 5 份重复，已消除 |
| 零引用类型 + 同成员枚举 | 28 | 无可合并项 |

后三种方法都独立地没有推翻前一种的结论，也没有发现被遗漏的重复。
这构成"重复模型清单已完备"的交叉证据。

## 29. 待决策项 #2 已查明：`ProjectileSimulationSystem` 不能简单删除

第 10 节把"`ProjectileSimulationSystem` 的运动封套 ↔ `SpatialSimulationResult<T>`"列为待决策。
本轮查清了它到底多做了什么，结论是**它不是重复实现，而是一份缺失能力的唯一实现**。

### 29.1 差异点：子步

`ProjectileSimulationSystem` 里有一段别处没有的逻辑（第 88–98 行）：

```csharp
int substepCount = kinematics.SubstepCount;
float substepDelta = deltaTime / substepCount;
for (int substep = 0; substep < substepCount; substep++)
```

它读 `ProjectileKinematicsStateComponent`，把一帧的 `deltaTime` 按 `ExtraUpdates + 1` 均分后循环推进，
并用 `BeginTick()` / `NextSubstep()` 维护子步序号。

实测全仓读取 `Substep` / `SubstepCount` / `NextSubstep` 的**只有这一个文件**，而它从未被注册。
已注册的共享管线（`SpatialSimulationCoordinatorSystem` + `SpatialSimulationSystem`）
完全不读 `ProjectileKinematicsStateComponent`——每帧只有一步。

### 29.2 一个容易搞错的点

子步**不改变总位移**：`deltaTime` 被均分，步数相应增加，`速度 × deltaTime` 之和不变。
所以差距无法用"跑了多远"检测出来，它只体现在**瓦片碰撞的精度**上——
高速投射物在一帧内跨过薄墙时，单步会穿模，子步不会。

原先打算用位移差异来验证这个差距，推演后发现在数学上不成立，改用"子步序号从不推进"表述。

### 29.3 因此的判定

- **不能删除** `ProjectileSimulationSystem`：删掉就没人实现子步，`ExtraUpdates` 成了纯摆设。
- **也不能直接注册它**：它会与已在跑的 `SpatialSimulationCoordinatorSystem` 争抢同一批实体
  （两者都写 `LocationComponent` / `VelocityComponent`），每帧会被推两次。
- 真正的解法是把子步语义**并入共享管线**：由协调系统读取 `ProjectileKinematicsStateComponent`，
  对投射物按 `SubstepCount` 拆分该帧的 `deltaTime`。这属于共享物理语义的扩展，需要设计确认。

### 29.4 把差距变成显式断言

`projVerification` 从 30 条扩到 **34 条**，新增四条把当前状态锁定下来：

```
PASS fast-spawn-applied | status=Applied
PASS kinematics-attached | has=True extraUpdates=1
PASS kinematics-substep-not-advanced | substep=0（已注册路径从不推进它）
PASS substep-does-not-scale-distance | expected=(302,400) actual=(302,400)（子步只影响碰撞精度，不改变总位移）
```

这是**特征化测试**：它断言的是"当前已知的行为"，而不是"期望的行为"。
一旦子步被并入共享管线，`kinematics-substep-not-advanced` 会变红，从而强制更新这两条断言——
这正是它存在的意义：让差距不会在无人察觉的情况下继续存在。

### 29.5 验证

全部工程 0 错误；`queueprobe` 49 PASS；`invVerification` 16 PASS；
`projVerification` 34 PASS；`worldprobe` 195 行与基线一致；`tickprobe` 8 行不变。

## 30. 待决策项 #6 的一半已闭合，另一半被证明**不该删**

### 30.1 投射物 AI 层不是重复，是对 Share/AI 的组合

`ProjectileBehaviorProfile` / `ProjectileBehaviorCatalog` / `ProjectileAiRegistration`
初看像是与 `Share/AI` 重复的"第二套 AI 模型"。逐个读下来不是：

- 它们直接使用 Share 的 `Terraria.Ai` 类型：`AiSystem<ProjectileAiIntent>`、`AiBehavior<TIntent>`、
  `AiBehaviorSelectionComponent`、`AiBehaviorProfileId`、`AiRequirements`。
- `ProjectileBehaviorProfile` 只是把"行为类型 + 参数 + 需求"打成不可变的值对象；
  `ProjectileBehaviorCatalog` 是按 `ProfileId` 索引并带唯一性/类型一致性校验的目录；
  `ProjectileAiRegistration` 负责把 `Straight` 与 `Homing` 两个实现注册进 `AiSystem`。
- 实测 `Share/AI` 下**没有**任何等价目录/注册表类型（没有 `*Catalog` / `*Registry` / `*Profile`），
  所以这不是重复实现，而是投射物域在 Share/AI 之上补的一层。

结论：这一半的"两套模型"不成立，投射物 AI 已经是"非 Share 使用 Share"的正面例子。

### 30.2 `Share/Entity` 定义簇：外部零引用，但**不是死代码**

实测该簇 8 个类型在 `src` + `Test` + `Build` 中、目录外的引用数全为 0：

| 类型 | 目录内引用 | 目录外引用 |
| --- | --- | --- |
| `EntityComponentSet` | 6 | **0** |
| `EntityComponentValue` | 10 | **0** |
| `EntityDefinition` | 16 | **0** |
| `EntityDefinitionBuilder` | 7 | **0** |
| `EntityDefinitionId` | 10 | **0** |
| `EntityDefinitionRegistry` | 3 | **0** |
| `EntityDefinitionSystem` | 2 | **0** |
| `IEntityDefinitionHost` | 3 | **0** |

按前几轮的做法，零引用就该进删除清单。**这里不成立**，有三条反证：

1. `docs/architecture/2026-10-24-nsslc-atomic-components-and-client-system-matrix.md`
   把 `EntityDefinitionId`、`EntityDefinition` 标为 **confirmed**，把 `EntityDefinitionRegistry`
   标为"不可变定义目录 / confirmed"。这是已确认的架构方向，不是遗留垃圾。
2. `docs/reviews/audits/2026-11-01-share-wiring-audit.md` 已经把这一簇判为
   「闭合的死簇，要么整簇接上，要么整簇删除；**半删会让状态更差**」，并给出 A（补宿主）/ B（整簇删）两个方案。
3. 该簇缺的恰恰只是一环：`EntityDefinitionSystem` 依赖 `IEntityDefinitionHost` 的实现，
   而全仓没有任何实现，因此它"无法构造"——是**缺宿主**，不是没人要。

### 30.3 因此的结论

- **不能删**：删除会与 confirmed 的架构矩阵直接冲突。
- **不能只删一半**：簇内部互相引用，半删留下更糟的状态。
- 这是本次审计里唯一一个"零引用但不可删"的案例，也说明**零引用只是必要条件，不是充分条件**。
  前面 27 节那 5 份重复助手能删，是因为它们有活着的等价替代；这里没有替代，删掉就是放弃能力。

真正的解法是二选一，且必须同步修订架构矩阵：

| 方案 | 动作 | 代价 |
| --- | --- | --- |
| A. 补宿主 | 为 `EntityRuntimeRegistry` 实现 `IEntityDefinitionHost`，把玩家/NPC 创建改为数据驱动，随后 `ProjectileDefinition` 收编进该框架 | 需要定义数据与创建流程迁移，工作量最大，但消除两套模型 |
| B. 整簇退役 | 删除 8 个类型，并**同时**把架构矩阵里 `confirmed` 标记改为退役 | 放弃数据驱动创建能力，但代码立刻一致 |

在选定之前，`ProjectileDefinition` 与 `EntityDefinition*` 并存是**已记录的已知双模型**，
不是遗漏。

### 30.4 本轮未改动任何代码

结论是"不删"，因此没有代码变更；仅新增本节记录与交叉引用。

## 31. #11 判断修正、#13 机制查明

### 31.1 #11 的两个 `SlimeAiBehavior` 不是重复实现

第 10 节把 `Npc/SlimeAiBehavior` 与 `Share/AI/Examples/SlimeAiBehavior` 记为"同名同概念双实现"。
逐字核对后这个判断需要修正：

| | `Terraria.Npc.SlimeAiBehavior` | `Terraria.Ai.Examples.SlimeAiBehavior` |
| --- | --- | --- |
| 基类 | `AiBehavior<NpcAiIntent>` | `HtnAiBehavior<SlimeContext, ExampleAiIntent>` |
| 意图类型 | `NpcAiIntent` | `ExampleAiIntent` |
| 用途 | 生产用史莱姆 AI，经 `NpcAiRegistration` 挂到 NPC | HTN 规划子系统（`AttackAction`、`DefeatEnemyGoal`、`EnemyAliveKey`、`SlimeDomain`）的演示 |
| 消费方 | `NpcAiRegistration`、`Program.cs:526` | `Test/Terraria.NpcAi.MinimalVerification/SharedApiUsage.cs` |

两者基类不同、意图类型不同，一个走简单行为绑定、一个走 HTN 目标规划。
Examples 侧**不是死代码**：全仓唯一 `using Terraria.Ai.Examples` 的正是
`MinimalVerification` 的 `SharedApiUsage.cs`，而该工程可构建并通过。

所以这是"同一题材在两个 AI 子系统下的各自实现"，属演示与生产的对照，
不构成可合并的重复模型。这与第 12 节 `ProjectileSourceMetadataComponent` 的判断属同类更正。

### 31.2 #13 制导能力恒不生效的确切机制

`ProjectileHomingAiBehavior` 在两处要求 `ProjectileHomingCapabilityComponent`：

```csharp
// 第 41-42 行：取参数
ProjectileHomingCapabilityComponent capability = snapshot.Get<ProjectileHomingCapabilityComponent>();
// 第 82 行：需求校验
!requirements.Contains<ProjectileHomingCapabilityComponent>() || ...
```

而全仓对该组件的出现只有这 3 处读取 + 1 处类型声明，**没有任何挂载点**。
所以即使把 `ProjectileAiRegistration` 接上、把 AI 系统注册起来，制导行为仍会在需求校验处被拒。

同时 `ProjectileDefinition` 的构造函数里没有转向速率之类的字段，
无法从内容定义推导出该能力——因此这不是"忘了挂"，而是**定义链缺一个字段**。

正确顺序是：先给 `ProjectileDefinition` 增加制导参数（或复用 `ProjectileBehaviorProfile` 的
`TurnRate` 作为唯一来源），再在生成时据定义挂载该组件。前者属内容模型扩展，需要设计确认。

注意 `ProjectileHomingAiBehavior` 第 67 行已经写成
`float turnRate = _profile.TurnRate > 0f ? _profile.TurnRate : capability.TurnRate;`——
即它已经预留了"配置优先、能力兜底"的取法。这说明原设计意图就是两者共存：
Profile 是内容侧默认值，Capability 是实例侧覆盖。所以 #13 也不该用"合并成一份"来解决，
而是补上实例侧能力的挂载来源。

### 31.3 两项的结论

- #11：**撤销**"重复实现"的判断，保留两侧。
- #13：**不是重复模型**，是"实例侧能力缺挂载来源"；需要给定义链加字段，属内容模型扩展。

两项都从"待决策"收敛为"已查明且不需要合并"。至此第 10 节表格里
真正需要合并决策的只剩 #4（世界掉落物）与 #6（EntityDefinition 二选一）。

## 32. 待决策项 #4 已定位到确切分歧点：校验语义不一致

第 10 节把"世界掉落物 `WorldItemState` ↔ `Share/Item`"记为待决策，阻塞点是"400 槽网络表示"。
本轮把它查到了具体的一行代码上。

### 32.1 物品身份三元组在全仓有四个表示

| # | 表示 | 形态 | 角色 |
| --- | --- | --- | --- |
| 1 | `Share/Item/ItemStack` | `(ItemId, int Quantity, byte Prefix)` | 组件域值对象 |
| 2 | `World/WorldItemState` | `(int ItemType, int Stack, byte Prefix, …)` | 权威世界掉落物 |
| 3 | `MinimalImplementation/Contracts/Items/ItemState` | `(int Type, byte Prefix, int Stack)` | 旧运行期契约 |
| 4 | `MinimalImplementation/Contracts/…/WorldFileChestItem` | `(int stack, int type, byte prefix)` | 世界文件持久化 |

3 与 4 必须保留原始 `int`——它们分别对应磁盘与世界文件字段，是有边界约束的字节表示
（`WorldFileChestItem` 还把 stack 钳在 `short` 范围内）。真正的重复只在 1 与 2 之间。

### 32.2 1 ↔ 2 为什么现在换不了

`WorldItemState` 唯一的构造点是 `WorldItemStore.TryCreate`（第 43 行），它在构造前调用：

```csharp
WorldItemRules.IsValidItem(itemType, stack, position, velocity)
// → itemType > 0 && stack > 0 && 位置/速度有限
```

而 `ItemStack` 的构造函数要求 `ItemIdRules.IsKnown(itemId)`。

**分歧就在这**：`WorldItemStore` 接受任意正整数 itemType，`ItemStack` 只接受已知 `ItemId`。
所以直接把三个字段换成 `ItemStack` **不是无损的**——它会把"任意正整数"收紧成"已知物品"。

而未知 id 是可达的：`GameCommunicationAdapter` 第 378/391 行把客户端包里的 `itemType`
原样塞进 `WorldItemRequest`，中间没有任何 `ItemIdRules.TryFromNetworkType` 校验。

### 32.3 两个方案

| 方案 | 动作 | 评价 |
| --- | --- | --- |
| **A. 先收紧再合并**（推荐） | 在 `GameCommunicationAdapter` 处理世界物品包时用 `ItemIdRules.TryFromNetworkType` 校验，未知 id 直接丢弃；随后把 `WorldItemState` 的三个字段换成 `ItemStack` | 权威状态里不再可能出现客户端编造的物品 id；重复模型被真正消除 |
| B. 放宽 `ItemStack` | 让共享值对象允许未知 id | **不推荐**：为迁就旧路径污染共享模型，等于把问题从一处搬到另一处 |

推荐 A 的理由：目前客户端可以往服务端权威状态里塞任意正整数 itemType，服务端会照单全收、
后续还会把它投影给所有客户端。这本身就该在入口处拒绝，与是否合并模型无关。
换句话说，#4 的正确顺序是**先补入口校验，再合并模型**，而不是在模型层做兼容。

### 32.4 本轮未改动代码

结论依赖一个"入口是否该校验"的产品判断（严格来说还涉及协议宽容度），
本轮只把它从"阻塞点是网络表示"收敛为"阻塞点是这一行校验"，并给出可执行顺序。

## 33. 两种补充扫描：跨域同名类型、成员集真子集

第 28 节的两种扫描都比"完全同形"更宽，用来兜住部分重叠的重复。

### 33.1 同名类型跨域出现

按**声明所在命名空间**分组统计 145 个非 Share 类型与 331 个 Share 类型，
跨两个及以上命名空间的同名类型只有一组：

```
SlimeAiBehavior
  [Npc]   Terraria.Npc          (SlimeAiBehavior.cs)
  [Share] Terraria.Ai.Examples  (SlimeAiBehavior.cs)
```

即 §31.1 已判定为"不同 AI 子系统下的各自实现"的那一组。除此之外没有第二个同名类型，
说明不存在"复制粘贴改名没改全"的隐蔽重复。

### 33.2 成员集真子集关系

全形比对（第 24 节）只能抓成员完全相同者。这次改为在两两比较非 Share 与 Share 的
成员名集合，报告**真子集**与**交集 ≥ 2** 的关系，共 80 个类型参与。命中三组：

| 非 Share | Share | 共有成员 | 判定 |
| --- | --- | --- | --- |
| `Npc.NpcAiMovementFact` ⊂ `SpatialTileMotionResult` | 4 成员 | `Position`、`Velocity` | **不是重复**：前者是喂给 AI 的输入快照（含 `IsGrounded`），后者是瓦片运动查询的输出（含 `CollidedX/CollidedY`），方向相反 |
| `Npc.NpcAiTargetFact` ⊃ `ExampleTargetComponent` | 3 成员 | `Position`、`IsVisible`、`IsAlive` | **不是重复**：右侧是 `Examples/` 里的演示类型，见 §31.1 |
| `Projectile.ProjectileSpawnResult` ⊃ `AiEntityResult` | 2 成员 | `Status`、`Entity` | **不是重复**：两个域各自的"操作结果"，共享的只是"状态 + 实体句柄"这个通用形状，抽成泛型收益低于引入跨域耦合 |

三组都因**方向或角色相反**而不成立，与第 24 节的结论一致：
形状相似只是提出问题的线索，判定必须落到"这两个东西在什么时候被谁写入、被谁读取"。

### 33.3 至此的扫描矩阵

| 扫描 | 参数量 | 命中 | 可合并 |
| --- | --- | --- | --- |
| 逐文件语义审计 | 135 文件 / 145 类型 | 13 组候选 | 7 组已消除 |
| 字段形状完全聚类 | 全部组件 | 5 组 | 0 |
| 同名 static 助手 | 全部 | 2 个助手 5 份 | 5 份已消除 |
| 零引用类型 + 同成员枚举 | 137 / 50 | 3 + 1 组 | 0 |
| 跨域同名类型 | 476 类型 | 1 组 | 0 |
| 成员集真子集 | 80 类型 | 3 组 | 0 |

六种方法都收敛到同一结论：**重复模型清单已完备，剩余项全部需要人或架构拍板**。

## 34. 决策落地与收尾（第 31–33 轮）

§0 的四项待决事项全部得到决策并已实现。本节记录决策、改动与验证，并把仍未完成的部分
明确列成后续项，避免"看起来做完了"。

### 34.1 决策与落地对照

| 事项 | 决策 | 落地 | 提交 |
| --- | --- | --- | --- |
| #4 未知 itemType | 拒绝未知，随后合并 `ItemStack` | 入口校验 + `WorldItemState.Item` 改为 `ItemStack` | `06da105` |
| #4 属性贡献 | 现在注册 | `InventoryAttributeContributionSystem` 注册到 Simulation | `c48ff7c` |
| 无敌窗口 | 受击后授予 | 伤害提交成功后开窗 + `CreatePlayer` 挂载组件 | `4da1bee` |
| #6 定义簇 | **A** 补宿主并收编 | 宿主 + 玩家创建数据驱动 + 投射物收编 | `d4b0da1`、`d9f9c84` |

### 34.2 受击无敌窗口

- 时长 `PlayerHurtProtectionTicks = 60`（1 秒），作为宿主层可调常量。参考客户端受击后的短无敌
  在 60/120/240 这一档，但 `Player.Hurt` 内部按来源分别计算，本仓库未逐行对齐，因此注释里
  如实标注了这一点，而不是假称"与参考客户端一致"。
- 开窗点在 `CombatDamageSystem` 的 `DamageCommitResult.Applied` 分支之后：只有真正提交了伤害
  才计时，被拒绝的请求不会留下无敌帧。
- **推进顺序修正**：`ProtectionWindowSystem` 原先排在 Commit 阶段最后，本帧新开的窗口会被同帧
  推进一次，实际保护少一个 tick。现移到 Commit 最前，保护时长与配置值严格相等。
- 验证：`Test/Terraria.Combat.ProtectionWindow.Verification`，5 条断言——首次受击生效并计时、
  窗口内二次受击以 `ProtectionWindowActive` 被拒、系统确实在推进且到期、到期后伤害恢复。

### 34.3 实体定义宿主与玩家创建

- `IEntityDefinitionHost` 此前没有任何实现，`EntityDefinitionSystem` 连构造函数都调不通，这才是
  那 8 个类型目录外零引用的真实原因；直接删除会连带丢掉"数据驱动创建"这条能力。
- 新增 `ArchEntityDefinitionHost`：按定义生成实体、写入全部组件、按 `EntityIdentityComponent`
  登记运行时句柄；任一步抛出都会销毁已建实体；销毁时同时摘句柄与实体。
- **Arch 2.1.0 的 `World.AddRange(Entity, Span<object>)` 对"新建实体 + 一批组件"会越界**
  （`BitSet.AsSpan` 抛 `IndexOutOfRangeException`）。改为按组件类型缓存泛型 `World.Add<T>`
  委托，每种类型只反射一次。
- 玩家创建改为 `PlayerEntityDefinition` 驱动：原先 `CreatePlayer` 里的 18 项 inline 组件清单
  搬进不可变定义，槽位数/堆叠上限/重生延迟由组合根传入，定义不写死数值；出生坐标属于实例级
  输入，创建后用 `World.Set` 覆盖。
- 验证：新增 `Test/Terraria.EntityDefinition.Verification`（6 条）；并用探针逐项核对玩家组件
  集合 18/18 与迁移前一致（`Build/playerprobe`，属构建产物、不入库）。

### 34.4 投射物收编

`ProjectileLifecycleSystem.TrySpawn` 里那 29 个组件的装配不再硬编码：

- `ProjectileEntityDefinitions` 把与实例无关的 22 项表达成 `EntityDefinition`（身份由工厂每次
  新建，运动学/生命周期/碰撞策略/攻击引用/11 项空间前置条件来自内容定义）。
- `ProjectileDefinitionCatalog.Register` 成对登记内容定义与实体组合定义，避免"有内容但没有
  生成路径"的中间态。
- `TrySpawn` 只剩 7 项实例级写入（位置/速度/阵营/归属/尺寸/碰撞体/来源），句柄登记与回滚交给
  宿主；失败时先摘句柄再销毁实体。
- 顺带消掉了原注释里的"泛型 Create 有类型参数上限"两段式装配：定义逐项装配，绕开该限制。
- 验证：投射物断言 34 → 35（新增组件完整性聚合检查），全绿。

### 34.5 本轮验证矩阵

| 检查 | 结果 |
| --- | --- |
| 14 个可构建工程 | 0 错误 |
| `queueprobe` | 49 PASS / 0 FAIL |
| `Terraria.Inventory.Verification` | 20 PASS |
| `Terraria.Projectile.Verification` | 35 PASS |
| `Terraria.WorldItem.Verification` | 16 PASS |
| `Terraria.Combat.ProtectionWindow.Verification` | 5 PASS |
| `Terraria.EntityDefinition.Verification` | 6 PASS |
| 玩家组件集合探针（`Build/playerprobe`） | 6 PASS，18/18 组件与迁移前一致 |
| NPC 组件集合探针（`Build/npcprobe`） | 7 PASS，21/21 组件与迁移前一致 |
| `worldprobe` | 195 行，与基准零差异 |
| `tickprobe` | 8 行，与基准零差异 |

### 34.6 三轮内的追加收尾

- **NPC 创建已数据驱动**：`NpcEntityDefinition` 提供向导/史莱姆两份定义，`CreateNpc` 只剩
  生成 + 覆盖出生坐标 + 写入网络标识三步，不再需要 `EntityRuntimeRegistry` 参数（句柄由宿主登记）。
  探针核对 21/21 组件与两种 NPC 的阵营/生命/碰撞体/最大速度/AI 行为类型/行为与档案 id/出生偏移/
  网络槽位全部一致。至此玩家、NPC、投射物三条创建路径都走 `EntityDefinitionSystem`。
- **定义簇命名空间已归位**：8 个类型从 `namespace Terraria.Combat` 迁到 `Terraria.Entity`
  （文件本来就在 `Share/Entity`）。`EntityFactionComponent`/`EntityIdentityComponent`/
  `EntitySizeComponent` 仍留在 Combat：它们是组件而非定义簇，迁移属于另一次组件域整理。

### 34.7 仍未完成（明确列出，避免误判为已完成）

1. **#4 的装备数据没填**：玩家 59 个槽位全是 `InventorySlotKind.General`，`BuildItemDefinitions()`
   也没有注册任何 `AttributeModifiers`。因此属性贡献系统虽已注册、单测能过，真实对局里装备仍
   不产生属性——这是数据问题，不是接线问题。
2. **`ProjectileDefinition` 保留**：它是内容定义（运动学/生命周期/攻击引用），不是实体装配的平行
   模型；收编后它的静态组合已由 `EntityDefinition` 表达。若继续"只留一套"，方向是把内容定义并进
   `EntityDefinition` 的内容字段，而不是删掉它。
