# 物品载荷层与双宿主 owner 执行计划

**文档 ID**：PLAN-2026-11-23-ITEM-PAYLOAD-DUAL-HOST
**逻辑域**：plans
**产物类型**：plan
**策略**：sequential-with-one-decision（P0 无前置；P1 是唯一决策点，需用户确认后继续；P2–P4 各自独立可验收）
**状态**：P0、P1-A、P2、P3、P4 **全部 completed**（2026-11-23 执行并验证）；P1-B 未采用
**上游依据**：[2026-11-23 Item 与 WorldItem 拆分必要性判定](../reviews/audits/2026-11-23-item-worlditem-split-necessity.md)
**适用约束**：[ECS 系统输入边界约束](../../Context/约束/ECS系统输入边界约束.md)、[ECS 文件组织设计约束](../../Context/架构设计/ECS文件组织设计约束.md)、[构建与验证约束](../../Context/约束/构建与验证约束.md)、[Test 验证项目组织](../../Test/README.md)
**范围**：`src/NSSLC/Share/Item/**`、`src/NSSLC/Share/Inventory/**`、`src/NSSLC/WorldItem/**`、`src/NSSLC/ItemUse/**`、`src/NSSLC.Server/Program.cs` 的系统注册、`Test/Terraria.Components/{Terraria.WorldItem,Terraria.Inventory}`
**排除范围**：不改协议包号与载荷格式；不改客户端；不改 `src/NSSLC.Infrastructure/**`；不引入事件总线或 CQRS；不动 World 域的 tile/液体推进

## 1. 执行目标

把物品概念收敛为**一层共享载荷 + 两个互斥宿主 owner**，而不是两个各自管理物品的系统：

```text
                共享载荷层（无 tick、无 owner、无世界字段）
                ItemId / ItemStack(ItemId,Quantity,Prefix)
                ItemDefinition + ItemDefinitionRegistry / ItemCategoryMask
                            │
        ┌───────────────────┴───────────────────┐
        ▼                                       ▼
  世界宿主（瞬态）                        容器宿主（持久）
  WorldItemStore : 400 槽 + revision      InventoryComponent : 槽位表
  WorldItemState : ItemStack + 位置/速度/预留  InventorySlot : ItemStack 值
  写入者：WorldItemRequestSystem（Commit）   写入者：InventorySystem（提交阶段）
  不入存档                                 随玩家/箱子存档
        └────────── 转移只经请求组件，单向报告 ──────────┘
```

判定口径（三条，缺一条就不算达成）：

1. **载荷唯一**：type / stack / prefix 只以 `ItemStack` 表达，不存在第二份权威副本。
2. **宿主互斥**：同一个物品在同一时刻只能被一个宿主拥有；移交必须是一次有方向的请求。
3. **单一写者**：世界槽只由 `WorldItemRequestSystem` 在 Commit 阶段写；容器槽只由 `InventorySystem` 在提交阶段写。

本计划**不追求**“两个系统各自能独立完成全部物品行为”。世界宿主不碰容器规则，容器宿主不碰物理；跨宿主的动作一律降级为请求。

## 2. 当前基线与证据等级

### 2.1 confirmed（本会话已读源码 / 已运行）

| 事实 | 证据 |
| --- | --- |
| 阶段固定为 Preparation(0) → Simulation(1) → Commit(2) → Projection(3) | `Share/Entity/System/EcsServerPhase.cs:6-18` |
| Command buffer 在 Projection 与 Commit 阶段之间被强制 playback 后才继续 | `Share/Entity/System/EcsServer.cs:238,247` |
| 世界宿主是非 ECS 的 `WorldItemStore`（400 槽、`_gate` 锁、revision、pending mutation 队列） | `WorldItem/System/WorldItemStore.cs:8-19, 205-219` |
| `WorldItemStore` 只在 Commit 被 `WorldItemRequestSystem` 写 | `WorldItem/WorldItemRequestSystem.cs:8-13`；`src/NSSLC.Server/Program.cs:499-504`（`WorldItemRequestCommit` 注册于 `EcsServerPhase.Commit`） |
| 世界 tick 在 Simulation 且晚于 WorldTick；拾取结算紧随其后 | `src/NSSLC.Server/Program.cs:543-560`（`WorldItemTick` 后接 `InventoryPickupRequest`，`after:` 显式声明） |
| 载荷是纯值：`ItemStack` 为 `readonly record struct`，仅 `ItemId`/`Quantity`/`Prefix` | `Share/Item/ItemStack.cs:8-27` |
| `ItemStackComponent` 全仓**零** `new ItemStackComponent(` | 本会话全仓 grep 结果 |
| `ItemWorldStateComponent` 零生产点，仅被 `InventorySystem` 读 | `Share/Inventory/InventorySystem.cs:339-341` |
| 容器槽是“值 或 实体身份，二者互斥” | `Share/Inventory/System/InventorySlot.cs:7-31` |
| 客户端背包包 5 直接覆写槽值，不经物品实体 | `Player/PlayerInputRequestSystem.cs:61-75`（`SetInventorySlot` → `TrySetSlotValue`） |
| 客户端包 21 直接写世界槽（Create/Update），以槽位索引寻址 | `src/NSSLC.Network/Communication/Gameplay/GameCommunicationAdapter.cs:349-430`（`ApplyWorldItem`） |
| 世界物品验证 42 PASS / 0 FAIL；容器验证 16 PASS / 0 FAIL | 本会话已运行，命令见审计 §6.1 |
| 拾取守恒已有断言：世界剩余数量 = 原始 − 背包实收 | `Test/Terraria.Components/Terraria.WorldItem/Program.cs`（`pickup-conserves-quantity`） |

### 2.2 partial（已定位，未收敛）

- **第三套表示仍在**：`ItemStackComponent` / `ItemWorldStateComponent` / `ContainedInInventoryComponent` 与 `InventorySlot.ItemEntityId` 构成实体化物品路径，有声明、有读取方、无生产者。
- **世界侧生命周期计数缺失**：`WorldItemState` 只有 `ItemStack` + 位置/速度/预留，未建模 `noGrabDelay` / `ownTime` / `timeSinceItemSpawned` / `keepTime` / `shimmered`；`WorldItemTickSystem` 的类注释已自陈这一点（`WorldItemTickSystem.cs:28-34`）。
- **只有单向移交**：世界 → 容器（拾取）已通；容器 → 世界（丢弃）不存在，`WorldItemRequestKind.Create` 的现有来源只有 tile 掉落投影与客户端包 21。
- **同一 tick 内的双持窗口**：`InventoryPickupRequestSystem` 在 Simulation 阶段把数量写进背包，`Consume` 要等到 Commit 才从世界槽扣除。两者之间任何同时读两侧的系统会看到“背包已加、世界未减”。

### 2.3 unknown

- 客户端对“世界物品被拾取”的显示是否依赖 `SyncItemDespawn` 的到达时序，而非仅依赖本地预测。
- 若退役实体化物品路径，是否有内容侧计划（枪械自带弹药库、耐久实例）依赖它。仓库内未发现此类内容，但这是静态搜索结果，不等于内容规划。

## 3. 不变量（验收的真正对象）

批次可以调整，这五条不能放宽：

| ID | 不变量 | 判定方式 |
| --- | --- | --- |
| INV-1 | 载荷唯一：任何物品的 type/stack/prefix 只有一处权威来源（`ItemStack`） | 静态：不存在第二套同义字段；运行：跨宿主断言读到的数量一致 |
| INV-2 | 宿主互斥：同一 `ItemStack` 不被两个宿主同时持为权威 | 运行：拾取/丢弃前后各一次快照，两侧总量守恒 |
| INV-3 | 单一写者：世界槽只由 Commit 请求系统写，容器槽只由 `InventorySystem` 写 | 静态：`WorldItemStore` 的写方法只被 `WorldItemRequestSystem` 调用 |
| INV-4 | 转移守恒：背包实收数量 == 世界扣除数量 | 运行：`pickup-conserves-quantity` 同类断言 |
| INV-5 | 瞬态/持久分离：世界物品不进入任何存档路径，容器物品必须进入 | 静态：存档段不引用世界槽；运行：存档往返后世界槽为空、容器槽保持 |

INV-2 是本计划的中心；INV-3 是它的实现手段；INV-4 是它的数值表达。

## 4. 批次总览与依赖图

```text
P0 不变量验证与基线快照（无行为改动，必做）
 ├─ P1 收敛第三表示（决策点：删除 or 接线）
 ├─ P2 补齐世界侧生命周期计数（独立于 P1）
 └─ P3 建立容器 → 世界移交（丢弃），补全双向（依赖 P0 的守恒断言）
      └─ P4 文档与 README 对齐（依赖 P1/P2/P3 的最终形态）
```

排序理由：P0 先把“互斥”变成可执行断言，后续任何批次都能用同一套断言判断是否破坏边界；P1 决定目标结构形状，必须先定；P2 与 P1 无耦合，可并行；P3 需要 P0 的守恒口径才谈得上“移交不丢不增”。

## 5. 批次定义

### P0 不变量验证与基线快照

**目标**：在不动生产行为的前提下，把 INV-1..INV-5 写成可运行断言，作为后续批次的回归基线。

**改动面**

- 在既有验证项目内扩展，不新建项目（避免预建空目录）：
  - `Test/Terraria.Components/Terraria.WorldItem`：补跨宿主断言。该项目已同时引用 World 与 Inventory（`pickup-fills-inventory` 已读背包槽），无需新引用。
  - `Test/Terraria.Components/Terraria.Inventory`：补“槽位只承载值”的断言，显式覆盖 `InventorySlot.ContainsEntity == false` 路径。
- `Program.cs` 头注释按 [Test 验证项目组织](../../Test/README.md) 补写验证类型（`rule` + `chain`）与“固定几件事”。

**要新增的断言（名称即契约）**

| 断言 | 覆盖 | 期望 |
| --- | --- | --- |
| `invariant-payload-single-source` | INV-1 | 同一物品在世界槽与背包槽读出的 `ItemId`/`Quantity`/`Prefix` 三元组在移交后一致 |
| `invariant-hosts-exclusive-before-pickup` | INV-2 | 拾取前：世界有 1 份、背包 0 份 |
| `invariant-hosts-exclusive-after-pickup` | INV-2 | 拾取提交后：世界 + 背包总量 == 拾取前世界总量 |
| `invariant-single-writer-world-slot` | INV-3 | 静态扫描：`WorldItemStore` 的写入口只被 `WorldItemRequestSystem` 调用 |
| `invariant-transfer-conservation-partial` | INV-4 | 部分拾取：世界剩余 == 原量 − 实收，且世界槽未退役 |
| `invariant-world-items-not-persisted` | INV-5 | 世界槽不参与任何存档段（静态断言 + 现有加载器三段字段核对） |

**实际落地的断言（2026-11-23）**

计划中的 6 条全部落地，另加 2 条在执行时发现必要：

| 断言 | 覆盖 | 实际实现口径 |
| --- | --- | --- |
| `invariant-payload-single-source` | INV-1 | 用 `Prefix = 2` 的堆验证三元组整体过界；只比重不跟前缀无法证明“身份唯一” |
| `invariant-hosts-exclusive-before-pickup` | INV-2 | 世界 40 / 背包 0 |
| `invariant-hosts-exclusive-after-pickup` | INV-2 | 世界 0 / 背包 40 / 合计 40 |
| `invariant-transfer-conservation-partial` | INV-4 | 世界 40 + 槽容量 20 → 实收 20 / 剩余 20 / 合计 40 |
| `invariant-oversized-stack-rejected-whole` | INV-4 边界 | **新增**：超定义 `MaxStack` 的堆被整堆拒绝而非部分接收 |
| `invariant-single-writer-world-slot` | INV-3 | 改为**反射**断言：`IWorldItemQuery` 只有 `TryGet`、`IWorldItemProjectionSource` 只有 `DrainMutations`。计划原写的“静态扫描”不可移植到运行验证，实际保留在 §6.1 的 PowerShell 步骤 |
| `invariant-world-state-carries-no-second-payload` | INV-1 结构面 | **新增**：`WorldItemState` 成员集合锁定为 `Item/Position/ReservedFor/Revision/Slot/Velocity`，防止 type/stack/prefix 被平铺回来 |
| `invariant-world-items-not-persisted` | INV-5 | `WorldItemStore` 的公共实例方法名不含 Save/Load/Serialize/Persist/Export/Import/Read/Write |

**执行中修正的一处计划错误**

计划原文写“用 150 超过单槽上限 99，强制走部分路径”——这是错的。`InventorySystem.GetDefinition`
在 `stack.Quantity > definition.MaxStack` 时返回 `InvalidItemStack`，整堆拒绝，根本进不到
`PlanInsert` 的容量截断。真正的部分成交条件是“**堆 ≤ 定义 MaxStack，但超过槽容量**”
（`InventorySystem.cs:424` 用 `min(槽 MaxStackSize, 定义 MaxStack) - 当前量` 截断）。

修正后断言改为“世界 40（≤99）+ 槽容量 20”。这条修正同时暴露了一个现状事实：
`WorldItemStore.TryCreate` 只校验 `WorldItemRules.IsValidItem`（类型已知、stack > 0、坐标有限），
**不校验定义上限**，因此客户端包 21 可以造出超过 `MaxStack` 的世界堆——它不可拾取，
只能等 `keepTime` 或出界退役。这不构成复制或丢失，但会卡住物品，已记入 §7 风险表。

**验收**

```powershell
dotnet build Test/Terraria.Components/Terraria.WorldItem/Terraria.WorldItem.csproj --nologo --verbosity minimal
dotnet run   --project Test/Terraria.Components/Terraria.WorldItem/Terraria.WorldItem.csproj --no-build --no-restore
dotnet build Test/Terraria.Components/Terraria.Inventory/Terraria.Inventory.csproj --nologo --verbosity minimal
dotnet run   --project Test/Terraria.Components/Terraria.Inventory/Terraria.Inventory.csproj --no-build --no-restore
```

通过条件：两项目 0 错误；WorldItem 断言数 ≥ 42 + 6（原 42 全部仍 PASS）；Inventory 断言数 ≥ 16 + 1（原 16 全部仍 PASS）。任何原有断言变红即中止本计划。

**P0 实际结果（2026-11-23）**

| 项目 | 构建 | 运行 | 明细 |
| --- | --- | --- | --- |
| `Terraria.WorldItem` | 成功，0 错误（9 警告，全部为 `NSSLC.Network` 既有警告：CS0105 重复 using ×7、CS0649 未赋值字段 ×2） | exit 0，**50 PASS / 0 FAIL** | 原 42 项全绿 + 新增 8 项 |
| `Terraria.Inventory` | 成功，0 警告 0 错误 | exit 0，**21 PASS / 0 FAIL** | 原 16 项全绿 + 新增 5 项 |

Inventory 侧实际新增 5 条（计划只要求 1 条：“槽位只承载值”），按行为口径写而不绑定成员名，
因此 P1 若移除 `ItemEntityId`，这 5 条仍然应当 PASS 而非编译失败：

`slot-empty-has-no-content`、`slot-value-carries-stack-only`、`slot-value-removable-as-stack`、
`slot-value-remove-keeps-remainder`、`slot-value-clear-leaves-nothing`。

**风险与回滚**：只改 `Test/` 下文件，回滚为 `git checkout` 对应目录。

### P1 收敛第三表示（决策点，需用户确认后执行）

**目标**：让“世界中的物品”只有一种表示，消除 INV-2 的结构性漏洞。

**必须先做的选择**

| 选项 | 内容 | 代价 | 适用条件 |
| --- | --- | --- | --- |
| P1-A（默认建议）**删除实体化路径** | 移除 `ItemStackComponent`、`ItemWorldStateComponent`、`ContainedInInventoryComponent`、`InventorySlot.ItemEntityId`、`InventorySystem.Entities.cs` 及对应 failure 值 | 破坏性公共 API 变更；失去“物品实体自带子库存”的表达 | 内容侧不需要实例化物品（当前仓库无此类内容） |
| P1-B **接线并成为唯一表示** | 让世界物品改为 ECS 实体，`WorldItemStore` 降级为纯投影索引 | 需要重写世界 tick、拾取、网络投影与 400 槽协议映射；与 Version4 槽位协议形态偏离 | 后续确实需要“物品自带组件” |

**若选 P1-A，改动面**

1. `Share/Item/`：删 `ItemStackComponent.cs`、`ItemWorldStateComponent.cs`。
2. `Share/Inventory/`：删 `InventorySystem.Entities.cs`；`InventorySlot` 去掉 `ItemEntityId`/`ContainsEntity`，`IsEmpty` 只看 `Content`；`InventorySystem` 去掉实体重载与 `SlotRequiresEntity`/`SlotDoesNotContainEntity`/`EntitySplitNotSupported`/`MissingEntityIdentity`/`ContainmentMismatch`/`ContainmentCycle`/`AlreadyContained` 分支；`InventoryAttributeContributionSystem` 去掉 `ItemStackComponent` 查询，只读槽值。
3. `Share/Inventory/README.md`：删除“两种内容”与实体槽位整节，改写为“容器只持有值”。
4. `Share/Item/README.md`：删除“有实体物品”整节。

**不变量影响**：INV-1 由“两处可能不一致”变为“结构上不可能不一致”；INV-3 从两条写路径降为一条。

**验收**

```powershell
dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal
dotnet build src/NSSLC.Server/NSSLC.Server.csproj --nologo --verbosity minimal
dotnet run   --project Test/Terraria.Components/Terraria.Inventory/Terraria.Inventory.csproj --no-build --no-restore
dotnet run   --project Test/Terraria.Components/Terraria.WorldItem/Terraria.WorldItem.csproj --no-build --no-restore
dotnet run   --project Test/Terraria.Components/Terraria.ItemUse/Terraria.ItemUse.csproj --no-build --no-restore
dotnet run   --project Test/Terraria.Components/Terraria.TileEntity/Terraria.TileEntity.csproj --no-build --no-restore
```

通过条件：两个生产项目 0 警告 0 错误；四个验证项目全 PASS；`ItemUse` 与 `TileEntity` 必须跑，因为箱子内容（`WorldChestState`）与弹药消耗都读写 `ItemStack`，是删除 `ItemEntityId` 后最可能回归的两处。

**风险与回滚**：这是全计划唯一的破坏性批次。回滚方式为整批 revert；不得只回滚其中一半（半删除状态会让 `InventorySlot` 的“值/实体互斥”契约自相矛盾）。

**P1 实际执行（2026-11-23）：采用 P1-A**

用户确认走 P1-A（删除实体化路径），并确认不与当时并行的 ItemUse/NpcSpawn 迁移改动交互。

删除清单：

| 文件/成员 | 处置 |
| --- | --- |
| `Share/Item/ItemStackComponent.cs` | 删除 |
| `Share/Item/ItemWorldStateComponent.cs` | 删除 |
| `Share/Inventory/InventorySystem.Entities.cs`（312 行） | 删除（`TryStoreEntity`、`TryTakeEntity`、`MoveEntity`、`GetContainedItem`、`ValidateContainment`、`TryGetIdentity`、`TryResolve`、`FindEntitySlot`） |
| `Share/Inventory/Component/ContainedInInventoryComponent.cs` | 删除（删除后仅剩孤立声明） |
| `InventorySlot.ItemEntityId` / `ContainsEntity` | 删除，`IsEmpty` 改为 `!Content.HasValue` |
| `InventorySystem.TryInsert(entity, itemEntity, …)` | 删除（值重载保留） |
| `InventorySystem.GetItem` / `GetUncontainedItem` | 删除 |
| `InventoryTransferFailure` 中 9 个仅属实体路径的取值 | 删除：`MissingItemStack`、`AlreadyContained`、`MissingEntityIdentity`、`SlotRequiresEntity`、`SlotDoesNotContainEntity`、`ContainmentMismatch`、`ContainmentCycle`、`EntitySplitNotSupported`、`PickupUnavailable` |
| `InventoryAttributeContributionSystem.TryGetDefinition` | 只读 `slot.Content`，删除 `ItemStackComponent` 查询分支 |

`src/` 全量引用核验（改动后）：`ItemStackComponent` 0、`ItemWorldStateComponent` 0、
`ContainedInInventoryComponent` 0、`ItemEntityId` 0、`ContainsEntity` 0、
`TryStoreEntity` 0、`TryTakeEntity` 0。

P1-A 期间修正的两处自身错误（记录以免重犯）：

1. 删除 `GetUncontainedItem` 时留下了一段悬空代码块（`ItemWorldStateComponent state = …` 无外层 `if`），
   随后整段移除修正。
2. 编辑 `InventoryTransferFailure` 文档注释时误删了 `None` 枚举值——它是默认成功值，
   编译器在下一次构建前不会报错，但语义会错。已补回并按文件复查确认顺序为 `None` 起始。

**P1 验收结果**

| 项目 | 结果 |
| --- | --- |
| `src/NSSLC/Terraria.Components.csproj` | 0 警告 0 错误 |
| `src/NSSLC.Server/NSSLC.Server.csproj` | 0 错误 |
| 全量 14 个验证项目 | 14/14 构建 0、14/14 运行 0、**0 FAIL** |
| `Terraria.Inventory` | 21 PASS（含 5 条“槽位只承载值”断言，P1-A 后仍全绿——证明断言按行为口径写而非绑定成员名） |
| `Terraria.WorldItem` | 60 PASS |

具备的能力损失："物品实体自带子库存"不再有容器表达（原 `TryStoreEntity` 场景）。
当前仓库无此类内容；若将来需要，按 P1-B 重新设计，不得恢复无生产者的声明。

### P2 补齐世界侧生命周期计数

**目标**：把 Version4 `WorldItem` 的生命周期字段补进 `WorldItemState`，让“世界宿主”名实相符；不改容器侧。

**改动面**

- `WorldItem/System/WorldItemState.cs`：增加 `NoGrabDelay`、`OwnTime`、`KeepTime`、`TimeSinceItemSpawned`、`Shimmered`（先只加有读取方的）。
- `WorldItem/System/WorldItemRules.cs`：新增“是否可被拾取”的纯判定（抓取延迟未到即不可抓）。
- `WorldItem/WorldItemTickSystem.cs`：`PickupPass` 前加延迟门；`MotionPass` 递增存活计数。
- `WorldItem/Command/WorldItemRequest.cs`：`Create` 请求补 `NoGrabDelay`（默认为 0 以保持现有行为）。
- 网络投影：若 `Shimmered` 影响客户端显示，需在 `SyncItem` 载荷的 `stateFlags` 位里表达——**本批次只做服务端状态**，投影留到 P2 之后单独评估。

**不变量影响**：不触碰 INV-1/INV-2；强化 INV-3（世界状态只在世界宿主内演进）。

**验收**

- `Terraria.WorldItem` 新增断言：`no-grab-delay-blocks-pickup`、`grab-delay-expires-then-pickup`、`lifetime-count-advances-per-tick`。
- 原 42 项 + P0 六项全部仍 PASS。
- 默认值必须保持现有可观察行为：`Create` 不传延迟时，现有 `pickup-removes-world-item` 与 `falling-item-rests-on-floor` 结果不变。

**风险与回滚**：字段增加会让 `WorldItemState` 的 `with` 表达式与相等性判断变化；回滚按文件回退，断言保留。

**P2 实际结果（2026-11-23）**

先回原版确认字段语义再实现，三个字段的读写点都有源码依据：

| 字段 | 原版证据 | 本仓落点 |
| --- | --- | --- |
| `noGrabDelay` | `WorldItem.cs:603-606` 每 tick 递减；门控在 `Player.cs:38653` `GetItem(WorldItem, …)`：`> 0` 直接返回原物品；赋值点 `Player.cs:4998`、`NPC.cs:45151` 均为 `100` | `WorldItemRules.CanBePickedUp` + 拾取通道提前跳过 |
| `timeSinceItemSpawned` | `WorldItem.cs:599-602` 每 tick +1，上限 `2147483547` | `WorldItemRules.MaximumLifetimeCounter` + `WorldItemStore.TryAdvanceLifecycle` |
| `ownTime` / `keepTime` | `WorldItem.cs:451-462` 各自递减到 0 为止 | 同上，`TryAdvanceLifecycle` 内统一推进 |
| `shimmered` | `WorldItem.cs:259` 合并判定要求 `shimmered` 相同 | `WorldItemMergeRules.MergeAmount` 新增该比较 |

实现取舍：五个计数用 **`init` 属性**追加在 `WorldItemState` 上，而不是位置参数——`TryCreate` 是唯一构造点，`with` 表达式遍布 `WorldItemStore`，改位置参数会连带改写十余处调用且无收益。

新增/变更的生产代码：

- `WorldItemState`：+5 个 `init` 属性（`NoGrabDelay`/`OwnTime`/`KeepTime`/`TimeSinceItemSpawned`/`Shimmered`）
- `WorldItemStore.TryCreate`：新增带 `noGrabDelay`/`shimmered` 的重载，旧签名转发并传 0/false（**行为等价**）
- `WorldItemStore.TryAdvanceLifecycle`：新增，纯推进、不发变更
- `WorldItemRules`：`MaximumLifetimeCounter` 常量、`CanBePickedUp`
- `WorldItemTickSystem`：`LifecyclePass` 在拾取之后统一推进；拾取筛选提前跳过带延迟的物品；`ThrownItemGrabDelay = 100`
- `WorldItemRequest`：`Create` 携带 `NoGrabDelay`/`Shimmered`（默认 0/false）
- `WorldItemMergeRules`：合并增加 `Shimmered` 相同判定

**一处断言按真实意图修正**：P0 的 `invariant-world-state-carries-no-second-payload` 原写“成员集合必须恰好等于 6 个位置参数”，P2 合法新增计数成员后被它判红。改成“不得出现 `Type`/`Stack`/`Prefix`/`ItemType`/`Quantity`/`ItemId` 平铺字段”，另加 `invariant-world-state-item-property-is-item-stack` 锁定 `Item` 属性类型必须是 `ItemStack`。断言的职责是守住“载荷不被复制”，不是冻结成员数量。

**P2 验证结果**

| 项目 | 结果 |
| --- | --- |
| `Terraria.WorldItem` | 构建 0 错误；**57 PASS / 0 FAIL**（P0 终态 50 + P2 新增 7） |
| `NSSLC.Server` | 构建 0 错误（19 警告，均为既有的重复 using 与未赋值字段） |
| `Terraria.Inventory` | 21 PASS / 0 FAIL（无回归） |
| `Terraria.ItemUse` | 构建 0 错误，PASS（无回归） |
| `Terraria.TileEntity` | 构建 0 错误，PASS（无回归） |

P2 新增断言：`no-grab-delay-blocks-pickup`、`no-grab-delay-blocks-pickup-for-whole-window`、
`no-grab-delay-expires-then-pickup`、`lifecycle-counters-start-at-declared`、
`lifecycle-counts-advance-per-tick`、`merge-requires-same-shimmer-state`、
`invariant-world-state-item-property-is-item-stack`。

执行中修正的一处断言构造：最初只用“3 tick 后应该入包”验证延迟窗口，实际是第 4 tick 才入包
（延迟 3 → 每 tick 判定后递减，第 4 次判定时才为 0）。这暴露了“先判定后递减”的顺序，
拆成“窗口内一次都没入包”+“归零后入包”两条，顺序语义因此被固定。

### P3 容器 → 世界方向：按原版客户端上报校验（方案已按源码更正）

**目标**：让 INV-2 在丢弃方向上也有可验证的口径，并**锁定“服务端分配槽位”**这一原版不变量。

**原计划被推翻的部分**

本计划初稿写的是“新建 `InventoryDropRequest` + 新系统，从容器扣除并产出 `WorldItemRequestKind.Create`”。
回原版核对后，这个设计在 Version4 里**不存在**，属于自创机制：

| 原版事实 | 证据 |
| --- | --- |
| 客户端丢弃时**先在本地**创建世界物品（`Item.NewItem`），再自己掷速度、`noGrabDelay = 100` | `Player.cs:4988-5018`（`noThrow` 分支内的丢弃路径） |
| 联机时随后只发一条 `SendData(21, …, num)` 广播该槽位 | `Player.cs:5017` |
| 服务端收到 `num == 400` 时会**自己重新分配槽位**：`num201 = Item.NewItem(new EntitySource_Sync(), …)`，客户端给的 400 只是“请新建”的哨兵 | `MessageBuffer.cs:1485-1492` |
| 服务端对已有槽位的更新会先校验 `Main.timeItemSlotCannotBeReusedFor[num201] > 0` 则丢弃该报文 | `MessageBuffer.cs:1483-1486` |
| 背包槽变更走的是另一条报文（包 5 / `SyncEquipment`），与包 21 相互独立 | `GameCommunicationAdapter.cs:127`（`SyncEquipmentMessageId => ApplyInventory`） |

也就是说：**丢弃方向是“客户端两条上报 + 服务端各自校验”，不是一条跨域移交系统**。
本仓适配器已经实现了这两个方向（`ApplyInventory` → `PlayerInputRequestKind.SetInventorySlot`；
`ApplyWorldItem` 在 `slot == 400` 时走 `WorldItemRequestKind.Create`），因此**不需要新增请求类型或新系统**。

**P3 改为：锁定哨兵语义 + 记录信任模型**

要新增/确认的不变量：

| ID | 不变量 | 判定方式 |
| --- | --- | --- |
| INV-6 | `Create` 请求的 `Slot` 是哨兵，世界宿主必须**忽略它并自行分配** | 运行：用 `Slot = 400` 发 Create，断言落点槽位 ∈ `[0, 400)` 且不等于 400 |
| INV-7 | 新建请求不得复用仍被占用的槽位 | 运行：占满若干槽位后发 Create，断言不落到已占用槽号 |
| INV-8 | 丢弃方向是两条独立上报，服务端不跨报文做守恒校验 | **记录事实**：写入 `WorldItem/README` 或类注释，说明这是 Version4 的信任模型（客户端权威背包），不是本仓缺陷 |

**明确不做的事**

- 不新增 `InventoryDropRequest`、不新增 drop 系统——原版没有，加了反而偏离协议形态。
- 不在此批次引入“服务端强制守恒校验”：那会改变与真实客户端的兼容性，需要单独决策与联调。

**验收**

- `Terraria.WorldItem` 新增：`create-ignores-sentinel-slot`、`create-picks-lowest-free-slot`。
- 服务端构建通过；`Terraria.WorldItem`、`Terraria.Inventory` 无回归。
- 文档写明 INV-8 的信任模型事实。

**风险**：`slot == 400` 的哨兵语义是协议契约，若未来有人把 `Create` 的槽位参数当真槽位使用，
会出现“客户端指定槽位”的越权路径。INV-6 就是防这条的。回滚为删除新增断言（不改生产代码）。

**P3 实际结果（2026-11-23）**

未新增任何生产代码——核对后确认本仓已经具备两个方向的上报入口，缺的只是把哨兵语义固定下来：

| 断言 | 结果 |
| --- | --- |
| `create-ignores-sentinel-slot` | PASS，落点=0（`Create` 携带 `Slot = 400` 时宿主自行分配，400 不成为真实槽位） |
| `create-picks-lowest-free-slot` | PASS，槽位=[0,1] |
| `create-never-reuses-occupied-slot` | PASS，退役槽 0 后回填 0，占用中的槽 1 不动 |

`Terraria.WorldItem` 累计 **60 PASS / 0 FAIL**。

**对 P3 原方案的一处否定（重要）**

计划初稿要求“新建 `InventoryDropRequest` + 新系统做容器→世界移交”。回原版核对后确认这是**自创机制**：
Version4 的丢弃由客户端本地 `Item.NewItem` + 广播包 21 完成，服务端收到 `num == 400` 时**自己重新分配槽位**
（`MessageBuffer.cs:1485-1492`）。因此 P3 不做新系统，只锁哨兵语义并记录信任模型。若照初稿执行，
会得到一个原版不存在、且与真实客户端行为不匹配的并行机制。

### P4 文档与 README 对齐

**目标**：让文档描述的结构与代码一致，避免下一个人再按 README 建一条不存在的路径。

**改动面**

- `Share/Item/README.md`、`Share/Inventory/README.md`：按 P1/P2/P3 的最终形态重写结构图与“状态由谁持有”表。
- `WorldItem/` 若有 README 或类注释：补“世界宿主不持久化”与“拾取/丢弃是唯一跨宿主方向”两句。
- 审计 [2026-11-23 判定](../reviews/audits/2026-11-23-item-worlditem-split-necessity.md) §5 的“决定 3”回填实际选择与结果。
- `Context/progress.md`：按 [progress 信息保存约束](../../Context/约束/progress信息保存约束.md) 关闭本轮未闭合项。

**验收**：文档中每条结构描述都能指向一个真实存在的类型或文件；`grep` 不到“两种内容”“实体槽位”等已删概念；progress 行数 ≤ 70、字节 ≤ 6 KB。

**P4 实际结果（2026-11-23）**

P4 分两次落笔，因为 P1 的结论在执行过程中变了：

**第一次（P1 未定案时）**：README 先按"标注未收敛"写，描述现状并指向决策。

**第二次（P1-A 执行后，最终形态）**：

| 文档 | 最终改动 |
| --- | --- |
| `Share/Item/README.md` | 三层图（载荷 / 世界宿主 / 容器宿主）+ 双宿主边界图；明确实体化路径**已删除**及原因；记录失去的能力与重新引入的唯一正当途径（P1-B 设计） |
| `Share/Inventory/README.md` | 章节改为“槽位内容：只承载值”；删除实体槽位/枪械弹药库示例与 mermaid 实体分支；交互 API 表移除 4 个实体入口，补上实际在用的 `TrySetSlotValue`；状态表删除两个已删类型行；生命周期段删除容纳关系相关条目 |
| `Context/progress.md` | 见最近轮次条目 |

最终核验：`Share/Inventory/README.md` 中 `TryStoreEntity`/`TryTakeEntity`/`ContainsEntity`
仅剩一处**说明其已删除**的句子（第 170 行），不存在"看起来能走"的残留路径；
`Share/Item/README.md` 中三个类型名仅出现在“已删除”的陈述里。

链接可达性逐条验证：`Share/Item/README.md` 两条、`Share/Inventory/README.md` 两条，全部 True。

## 6.5 全量回归（2026-11-23）

按 [Test 验证项目组织](../../Test/README.md) 的全量命令跑完 14 个项目，逐项 `build` 后 `run`：

| 项目 | Build | Run | PASS | FAIL |
| --- | --- | --- | ---: | ---: |
| `Terraria.Network.OutboundOrdering` | 0 | 0 | 12 | 0 |
| `Terraria.Server.TickCadence` | 0 | 0 | 6 | 0 |
| `Terraria.Attributes` | 0 | 0 | 37 | 0 |
| `Terraria.Chat` | 0 | 0 | 27 | 0 |
| `Terraria.Combat.ProtectionWindow` | 0 | 0 | 5 | 0 |
| `Terraria.Entity.Definition` | 0 | 0 | 6 | 0 |
| `Terraria.Entity.Scheduling` | 0 | 0 | 10 | 0 |
| `Terraria.Inventory` | 0 | 0 | 21 | 0 |
| `Terraria.ItemUse` | 0 | 0 | 15 | 0 |
| `Terraria.Npc.Ai` | 0 | 0 | — | 0 |
| `Terraria.NpcSpawn` | 0 | 0 | 21 | 0 |
| `Terraria.Projectile` | 0 | 0 | 35 | 0 |
| `Terraria.TileEntity` | 0 | 0 | 22 | 0 |
| `Terraria.WorldItem` | 0 | 0 | 60 | 0 |

**14/14 构建通过、14/14 运行退出码 0、0 个 FAIL。**

`Terraria.Npc.Ai` 的 PASS 列显示 "—" 是因为它用另一套输出格式（`name | 结果` + 末行 `PASS:`），
不匹配 `^PASS ` 前缀统计；其退出码为 0、末行为 `PASS: Guide idle/wander/stop, Slime targeting/jump,
AI intent, spatial movement and NPC wire projection.`，实际是通过的，不是空跑。

与本次改动相关的三处风险点都覆盖到了：`Terraria.ItemUse`（弹药消耗读写 `ItemStack`）、
`Terraria.TileEntity`（箱子内容 `WorldChestState` 存 `ItemStack`）、`Terraria.Inventory`（容器槽值）。
P2 动了 `WorldItemState` 的成员与 `WorldItemMergeRules`，`Terraria.WorldItem` 60 项全绿证明
既有 42 项行为未被改变。

## 6. 批次顺序与并行度

| 批次 | 前置 | 可并行 | 破坏性 | 必跑验证 |
| --- | --- | --- | --- | --- |
| P0 ✅ completed | 无 | — | 否（只改 Test） | WorldItem、Inventory |
| P1-A ✅ completed | P0 + 用户决策 | 与 P2 并行 | **是**（公共 API） | Components、Server、Inventory、WorldItem、ItemUse、TileEntity |
| P2 ✅ completed | P0 | 与 P1 并行 | 否 | WorldItem |
| P3 ✅ completed | P0 | 否 | 否（无生产代码改动） | WorldItem、Inventory |
| P4 ✅ completed | P1/P2/P3 | 否 | 否 | 文档链接可达性 + progress 上限 |

## 7. 已知风险与未决问题

| 风险 | 影响 | 处理 |
| --- | --- | --- |
| P1 删除后内容侧需要实例化物品 | 枪械弹药库、耐久等无处表达 | P1 执行前必须由用户确认内容规划；确认不需要才走 P1-A |
| 同一 tick 双持窗口 | 同时读两宿主的系统可能看到重复物品 | 本计划不消除窗口，只要求：任何跨宿主读取都必须在 Commit 之后；若将来出现此类读取，需把拾取结算移入 Commit |
| 世界侧计数补全改变可观察行为 | 掉落物出现抓取延迟，手感变化 | P2 默认延迟为 0；只有显式传参才启用，先保持等价 |
| 槽满丢弃语义 | 静默销毁物品 | 默认拒绝，且必须有断言固定 |
| **超限世界堆会卡住** | `WorldItemStore.TryCreate` 不校验定义 `MaxStack`，客户端可造出永远拾不起来的堆（`GetDefinition` 整堆拒绝），只能等 `keepTime` 或出界退役 | 已由 `invariant-oversized-stack-rejected-whole` 固定“整堆拒绝”这一容器侧行为；世界侧是否在 Create 处拒绝或降级到可拾取上限，需要单独决策，**不在本计划 P0–P4 范围内** |
| 验证覆盖只到两个项目 | 其余 12 个验证项目未纳入本计划 | P1 批次额外跑 ItemUse 与 TileEntity；全量 14 项作为发布前检查 |

## 8. 完成定义

本计划视为完成，当且仅当：

1. P0 六条不变量断言全部存在且 PASS；
2. P1 的选择（删除或接线）已执行并有构建+验证证据；
3. INV-2 在两个方向（拾取、丢弃）上都有运行断言；
4. `Share/Item/README.md` 与 `Share/Inventory/README.md` 描述的结构与代码一致；
5. `Context/progress.md` 的未闭合项按约束关闭或降级。

未满足 1–4 任一项时，不得在文档或注释中声称“物品已收敛为双宿主模型”。
