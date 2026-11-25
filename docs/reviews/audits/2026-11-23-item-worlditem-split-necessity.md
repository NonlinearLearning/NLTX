# Item 与 WorldItem 是否必须拆成两个系统：源码判定

```text
文档 ID：DOC-2026-11-23-item-worlditem-split-necessity
逻辑域：reviews
产物类型：audit、design
状态：concluded（静态检视 + 两项验证已运行）
范围：D:\TRbackup\客户端\Terraria\Item.cs、WorldItem.cs、Main.cs、Player.cs、
      Terraria.IO\WorldFile.cs、Terraria.ID\MessageID.cs、MessageBuffer.cs
      src/NSSLC/{Share/Item,Share/Inventory,WorldItem,ItemUse}、src/NSSLC.Network、
      src/NSSLC.Server/Program.cs
证据入口：见 §2 逐条行号
canonical 路径：docs/reviews/audits/2026-11-23-item-worlditem-split-necessity.md
```

## 0. 结论

**有必要拆**，但拆分轴不是“两个各自管理物品的系统”，而是**一层共享物品载荷 + 两个互斥的状态 owner**：

| 层 | 承担 | 权威状态 | 是否有 tick |
| --- | --- | --- | --- |
| 共享载荷层（原版 `Item` / 本仓 `ItemStack`+`ItemDefinition`） | 身份、数量、前缀、内容规则 | 无独立生命周期 | 否 |
| 世界 owner（原版 `WorldItem` / 本仓 `WorldItemStore`） | 槽位身份、位置速度、归属预留、存活寿命 | 400 槽运行时表，**不持久化** | 是，每 tick |
| 容器 owner（本仓 `InventoryComponent` / 原版 `Inventory`+`Chest` 的 `Item[]`） | 容器槽位内容 | 随玩家/箱子存档，**持久化** | 否，交互时写 |

反对的两种形态：

1. **合成一个系统管理物品**：世界物品的逐 tick 物理、槽位身份、预留与寿命会被迫进入容器槽的每个副本；容器侧 `Item.Clone()` 是 `MemberwiseClone`，一旦共享类型携带世界状态，克隆就会连位置和预留一起复制，且 400 槽主循环会遍历所有背包格。
2. **拆成两个各存一份身份的 store**：同一物品在世界侧和容器侧各存一份 type/stack，跨域每次移交都要双重写入，这是新的不一致源。原版没有这样做：`WorldItem.inner` 就是那个 `Item`，世界侧只多出世界状态。

因此源码给出的真实边界是**状态归属**，不是“物品概念”本身。世界物品与容器物品是同一载荷的两种宿主，宿主不同才需要两个系统；载荷必须只有一份定义。

## 1. 判定问题

“有没有必要把 item 概念拆成两个独立系统”只有在以下三问都有源码答案时才可判定：

1. 原版是否真的把“物品数据”和“世界中的物品”分成两个类型？
2. 这两个类型的状态是否共享同一生命周期（同一 owner 能否同时正确持有）？
3. 合并后是否会破坏任一方的既有语义（复制、存档、网络身份、逐 tick 更新）？

§2 逐条取证，§3 给合并与拆分的代价对照，§4 给本仓现状与偏差。

## 2. 源码证据

### 2.1 原版：两个类型，组合而非继承

| 事实 | 位置 | 说明 |
| --- | --- | --- |
| `public class Item : IEntitySourceTarget` | `D:\TRbackup\客户端\Terraria\Item.cs:18` | `Item` **不继承** `Entity`，没有 `position`/`velocity`/`whoAmI` 继承来源 |
| `public class WorldItem : Entity` | `D:\TRbackup\客户端\Terraria\WorldItem.cs:15` | 世界物品继承空间实体 |
| `public Item inner = new Item();` | `WorldItem.cs:17` | **组合**：世界物品持有一个物品载荷，而不是复制其字段 |
| `WorldItem` 对 `inner` 的委托访问器（`type`/`stack`/`prefix` 相关读写、`Name`、`value`、`damage`、`useTime` 等） | `WorldItem.cs:47-163` | 世界物品把内容语义全部转发给载荷 |
| `public Item Clone() => (Item)MemberwiseClone();` | `Item.cs:49418-49421` | 载荷是**可自由逐位复制**的值语义对象 |
| `public static bool CanStack(Item item1, Item item2)` 只比较 `type` 与 `prefix` | `Item.cs:49446-49452` | 堆叠判定只依赖载荷身份，不依赖世界状态 |
| `WorldItem.SetDefaults(int type)` → `ResetStats` + `inner.SetDefaults` + `UpdateEntityFields` | `WorldItem.cs:210-215` | 世界物品的“默认值”是“世界状态重置 + 载荷默认值”两件事的组合 |
| `private void UpdateEntityFields() { width = (height = 16); }` | `WorldItem.cs:380-383` | 世界碰撞盒属于世界侧，不来自载荷 |

`Item.cs` 全文出现 `position` 5 次、`velocity` 5 次，全部位于 `NewItem` 内的 `worldItem.position` 或对 `Main.item[i]` 的读取（`Item.cs:48994-48995`、`49325-49327`），**没有任何一处是 `Item` 自己的字段**；`whoAmI` 在 `Item.cs` 仅出现 1 次，即 `worldItem.whoAmI = num`（`Item.cs:49322`）。而 `WorldItem.cs` 中 `position`/`velocity` 共 181 处。这一分布直接证明：空间状态只存在于世界侧。

需要排除的一个误读：`Item` 确实声明了 `width`/`height`（`Item.cs:20,22`），但它们是**内容默认尺寸**，不含坐标；世界侧在 `UpdateEntityFields` 里统一覆盖为 16×16（`WorldItem.cs:380-383`）。尺寸不等于位置。

### 2.2 原版：两个容器的元素类型不同

| 容器 | 声明 | 位置 |
| --- | --- | --- |
| 世界物品表 | `public static WorldItem[] item = new WorldItem[401];` | `Main.cs:1675` |
| 世界物品槽数 | `public const int maxItems = 400;` | `Main.cs:1094` |
| 世界槽复用抑制表 | `public static int[] timeItemSlotCannotBeReusedFor = new int[401];` | `Main.cs:1677` |
| 玩家背包 | `public Item[] inventory = new Item[59];` | `Player.cs:1462` |
| 玩家护甲 / 染料 / 杂项 | `Item[] armor = new Item[20]`、`Item[] dye = new Item[10]`、`Item[] miscEquips = new Item[5]` | `Player.cs:1372,1374,1376` |
| 箱子 | `public Item[] item;` + `item = new Item[maxItems];`，`DefaultMaxItems = 40` | `Chest.cs:65,187,57` |

**世界表装 `WorldItem`，所有容器装 `Item`。** 这是“拆”的最直接证据：如果两者可互换，容器的元素类型会是同一类型。

### 2.3 原版：生命周期互斥

| 行为 | 世界物品 | 容器物品 |
| --- | --- | --- |
| 逐 tick 更新 | `for (int num4 = 0; num4 < 400; num4++)`（`Main.cs:17888`）内调用 `item[num4].UpdateItem(num4)`（`Main.cs:17894`、`17904`） | 无逐 tick 物理；玩家输入/交互时按槽位读写 |
| 生成 | `PickAnItemSlotToSpawnItemOn()` → `Main.item[num] = new WorldItem()` → `SetDefaults`/`Prefix`/`stack`/`position`/`velocity`（`Item.cs:49310-49330`） | 直接对 `inventory[i]` 赋值或 `Clone()` |
| 合并 | `TryCombiningIntoNearbyItems(myItemIndex)`，扫描 `myItemIndex+1..400`，曼哈顿距离 ≤ 30，且要求 `shimmered`、`playerIndexTheItemIsReservedFor` 相同（`WorldItem.cs:249-278`） | 无世界合并；容器内堆叠由槽位规则决定 |
| 归属 | `FindOwner()` 写 `playerIndexTheItemIsReservedFor`、`timeSinceTheItemHasBeenReservedForSomeone`，并发 22/39 号报文（`WorldItem.cs:280-377`） | 无“世界归属”概念 |
| 存活 | `ownTime`、`noGrabDelay`、`keepTime`、`timeSinceItemSpawned`、`timeLeftInWhichTheItemCannotBeTakenByEnemies`、`beingGrabbed`、`onConveyor`、`shimmered`/`shimmerTime`、`instanced`、`ownIgnore`（`WorldItem.cs:19-43`） | 仅 `type`/`stack`/`prefix`/`favorited` 等载荷字段 |
| 存档 | `Terraria.IO\WorldFile.cs` 全文对 `Main.item` **零引用** → 世界物品不写入世界文件 | `Player.cs:53865-53868` 逐槽写入 `type`/`stack`/`prefix`/`favorited` |
| 网络身份 | 报文 21/22/39 以**槽位索引**寻址；`MessageID.cs:51,53,89` 定义 `SyncItem=21`、`ItemOwner=22`、`ReleaseItemOwnership=39`；`SendData(21, ..., myItemIndex)`（`WorldItem.cs:273-274,482,625`） | 走玩家背包槽位报文，不含世界槽位语义 |

**关键点**：世界物品是**进程内瞬态**（不存档、重进即消失），容器物品是**持久实体**（写盘、跨会话存在）。一个系统无法同时是瞬态并且是持久的；这不是风格问题，是恢复语义问题。

### 2.4 原版：`Item.ToWorldItem()` 在本备份中是桩

`D:\TRbackup\客户端\Terraria\Item.cs:49231-49234`：

```csharp
public WorldItem ToWorldItem()
{
    return null;
}
```

该备份是去符号/裁剪版本，此处返回 `null`，**不能**据此认为“原版不允许 Item 转为 WorldItem”，也不能据此推断转换逻辑。真实转换只体现在 `NewItem` 的构造流程（§2.3）。本项记为 `evidence-gap`，不参与结论强度。

## 3. 合并与拆分的代价对照

| 方案 | 世界侧 | 容器侧 | 结论 |
| --- | --- | --- | --- |
| A. 单一类型/单一系统 | 需要在每个物品上携带 `position`/`velocity`/`whoAmI`/预留与寿命字段 | 每个背包槽都带一份永不使用的世界字段；`Clone()` 会把世界状态复制进背包 | **不可行**：字段基数与复制语义冲突 |
| B. 两个各存身份的 store | 存 type/stack | 也存 type/stack | **不可行**：移交需双写，守恒靠外部对账 |
| C. 共享载荷 + 两个宿主 owner（原版形态） | `WorldItem` 持 `Item` | 容器持 `Item` | **采用**：载荷单一定义，宿主各自持有自己的额外状态 |

判定依据来自 [ECS 系统输入边界约束](../../../Context/项目约束/ECS系统输入边界约束.md) 规则 1 与规则 2：状态由谁拥有决定是否需要请求层；
同一可变对象不得被两个以上系统共享。世界物品表与背包槽是**两个不同 owner 的不同状态**，符合“域内自读自写”，
而拾取这一跨域动作必须走请求组件。

## 4. 本仓现状（NSSLC）与偏差

### 4.1 已经符合的部分

| 项 | 位置 | 判定 |
| --- | --- | --- |
| 载荷是共享值 | `Share/Item/ItemStack.cs`（`readonly record struct`，只含 `ItemId`/`Quantity`/`Prefix`） | 符合：一份定义，无世界字段 |
| 定义与规则分离 | `Share/Item/ItemDefinition.cs`（不可变，含 `MaxStack`/`Categories`）、`ItemIdRules` | 符合 |
| 世界物品独立 owner | `WorldItem/System/WorldItemStore.cs`（400 槽、revision、变更队列），`WorldItem/System/WorldItemState.cs` 持有 `ItemStack` + 位置/速度/预留 | 符合：世界状态只在这里 |
| 世界状态不入存档 | 世界加载器只读 `world.chests`/`signs`/`tile-entities` 三段（见 [2026-11-06 审计](2026-11-06-server-systems-introduction.md) §3） | 符合：与原版一致的瞬态语义 |
| 跨域走请求 | `WorldItem/Command/WorldItemRequest.cs` + `WorldItemRequestSystem` 作为 Commit 阶段的唯一写入者；`InventoryPickupRequestSystem` 只回报实际接受数量 | 符合规则 1 |
| 使用配置独立 | `ItemUse/Content/ItemUseDefinition.cs`（`UseTime`/`UseAmmo`/`ManaCost` 等按 `ItemId` 注册） | 符合：使用档案不挂在世界状态上 |

### 4.2 偏差：第三套“物品表示”没有生产者

| 类型 | 声明 | 生产点 | 消费点 |
| --- | --- | --- | --- |
| `ItemStackComponent` | `Share/Item/ItemStackComponent.cs:8` | **全仓零 `new ItemStackComponent(`** | `InventorySystem.cs:317-321`、`InventorySystem.Entities.cs:27,95,126,144,209`、`InventoryAttributeContributionSystem.cs:165-170` |
| `ItemWorldStateComponent` | `Share/Item/ItemWorldStateComponent.cs:9` | **零生产点** | 仅 `InventorySystem.cs:339-341` |
| `ContainedInInventoryComponent` | `Share/Inventory/Component/ContainedInInventoryComponent.cs:7` | 仅 `InventorySystem.Entities.cs:66,193` 内的实体式转移路径 | 同文件 |

`Share/Item/README.md` 描述的“世界中的物品实体至少附着 `ItemStackComponent`，需要世界生命周期时再附着 `ItemWorldStateComponent`”这条链路**在生产代码里不存在**：世界物品走的是非 ECS 的 `WorldItemStore`，拾取走 `WorldItemRequestKind.Consume` 回报，而不是实体销毁。两项验证的实际通过项也印证这一点：世界物品验证断言 `pickup-removes-world-item` 是“槽位退役”，不是实体销毁。

这不是“需要拆”的证据，而是**拆分之后没有收敛干净**：同一概念（世界中的物品）当前有两套表示，一套在用（`WorldItemStore`），一套只有声明和读取分支（ECS 物品实体）。按 [项目文件组织设计约束](../../../Context/架构约束/项目文件组织设计约束.md) 与 [ECS 文件组织设计约束](../../../Context/架构约束/ECS文件组织设计约束.md) 的“按需建立”原则，无生产者的表示不应长期保留为并行路径。

## 5. 决定

1. **保留“载荷 / 世界 owner / 容器 owner”三分解**，不合并为单一系统。
2. **禁止**为世界物品与容器物品各自复制一份 type/stack/prefix 权威状态；两者都只持有 `ItemStack` 值。
3. **收敛 ECS 物品实体路径**：`ItemStackComponent`/`ItemWorldStateComponent`/`ContainedInInventoryComponent` 要么接上生产者并成为唯一世界物品表示，要么删除。当前“有声明、无生产者、却仍被 `InventorySystem` 读取”的状态应记为技术债，不得作为“已经支持实体化物品”的证据。

### 5.1 决定 3 的执行状态（2026-11-23 回填）

**已执行：采用 P1-A，实体化路径已删除。**

按 [物品载荷层与双宿主 owner 执行计划](../../plans/2026-11-23-item-payload-dual-host-execution-plan.md)
执行完毕，P0/P1-A/P2/P3/P4 全部完成，14/14 验证项目通过、0 失败。

| 决定 3 的选项 | 结果 |
| --- | --- |
| P1-A 删除实体化路径 | **已采用** |
| P1-B 接线让 ECS 实体成为唯一世界物品表示 | 未采用 |

删除范围：`ItemStackComponent`、`ItemWorldStateComponent`、`ContainedInInventoryComponent`、
`InventorySlot.ItemEntityId`/`ContainsEntity`、`InventorySystem.Entities.cs` 整个文件
（含 `TryStoreEntity`/`TryTakeEntity`）、`InventorySystem` 的实体重载与两个私有辅助、
`InventoryTransferFailure` 中 9 个仅属实体路径的取值。

因此本节 §4.2 记录的"第三套表示"不再存在：`src/` 下对上述四个标识符的引用数均为 0。
`Share/Item/README.md` 与 `Share/Inventory/README.md` 已同步为最终形态。

**代价（已知且已记录）**：失去"物品实体自带子库存"的容器表达。当前仓库无此类内容；
若将来需要，按执行计划 P1-B 重新设计，不得恢复无生产者的声明。

**同批完成的边界收敛**：世界侧补齐 `noGrabDelay`/`ownTime`/`keepTime`/`timeSinceItemSpawned`/`shimmered`；
合并判定加入 `shimmered` 比较；新建请求的哨兵槽语义被断言锁定；载荷唯一/宿主互斥/单一写者/
转移守恒/瞬态不入档五条不变量落成可执行断言。

## 6. 验证与缺口

### 6.1 已运行

| 命令 | 结果 |
| --- | --- |
| `dotnet build Test\Terraria.Components\Terraria.WorldItem\Terraria.WorldItem.csproj -v q --nologo` | 成功，0 警告 0 错误，耗时 1.89s |
| `dotnet run --project Test\Terraria.Components\Terraria.WorldItem\... --no-build --no-restore` | 42 PASS / 0 FAIL（运动、合并、拾取守恒、槽位退役、无变更投影） |
| `dotnet build Test\Terraria.Components\Terraria.Inventory\Terraria.Inventory.csproj -v q --nologo` | 成功，0 警告 0 错误，耗时 1.69s |
| `dotnet run --project Test\Terraria.Components\Terraria.Inventory\... --no-build --no-restore` | 16 PASS / 0 FAIL（`TrySetSlotValue` 契约、玩家输入写入共享库存） |

两项验证各自覆盖一侧 owner，且都通过：世界侧证明“槽位表 + 逐 tick 物理 + 拾取回报”可以独立运行；容器侧证明“槽位赋值 + 无世界状态”可以独立运行。两者互不依赖对方的组件，这是“两个 owner 而非一个系统”的运行级证据。

注：两项验证都未构造 `ItemStackComponent` 实体，因此 §4.2 的“无生产者”结论同时得到运行侧印证。

### 6.2 缺口

- `Item.ToWorldItem()` 在备份中是桩（`Item.cs:49231`），原版真实的 Item→WorldItem 转换细节为 `evidence-gap`。
- 原版 `Main.Update` 的完整调用链未静态展开，只确认了 400 槽循环与 `UpdateItem(i)` 调用点。
- `Item.DeepClone()` 与 `Clone()` 实现相同（都是 `MemberwiseClone`，`Item.cs:49418-49426`），未追查两者语义差异的历史意图。
- 未验证 ECS 物品实体路径若删除会影响哪些外部调用方；仓库内除 `InventorySystem` 与 `InventoryAttributeContributionSystem` 外未发现其他读取方，但这是静态搜索结果，不等于运行时闭包。
