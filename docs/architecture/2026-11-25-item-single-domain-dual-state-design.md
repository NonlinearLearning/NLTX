# Item 系统设计：单域 · 双状态（实体态 / 非实体态）

```text
文档 ID：DOC-2026-11-25-item-single-domain-dual-state
逻辑域：architecture
产物类型：design
状态：已确认，代码待迁移（当前实现仍是两套，见 §11）
canonical 路径：docs/architecture/2026-11-25-item-single-domain-dual-state-design.md
上位约束：docs/architecture/物品载荷与单一物品系统设计约束.md
取代：2026-11-23「Item 与 WorldItem 分离」的双宿主 owner 形态（载荷唯一部分保留）
```

## 1. 决定

把原 `Item`（非实体态）与 `WorldItem`（实体态）**两个系统合并为一个 Item 域**。合并的是
**状态归属与写者**，不是载荷定义：

| 维度 | 合并（本次决定） | 不合并（明确禁止） |
| --- | --- | --- |
| 域与写者 | 一个 Item 域、一个提交写者 | — |
| 状态表达 | 同一条物品记录上的状态判别 | 一个类型同时携带世界字段与容器字段的"万能物品类" |
| 载荷定义 | — | 把 `ItemStack` 拆成两份权威副本 |

一句话：**一个系统管理两种状态，而不是两个系统各管一种状态**。

### 1.1 与既有判定的关系

[2026-11-23 拆分判定](../reviews/audits/2026-11-23-item-worlditem-split-necessity.md) §0 的原话是：
"拆分轴不是『两个各自管理物品的系统』，而是一层共享物品载荷 + 两个互斥的状态 owner"。
它否决的是**方案 A（单一类型/单一系统，每个物品携带 `position`/`velocity`/`whoAmI`）**与
**方案 B（两个各存一份身份的 store）**，从未否决"一个域管理两种互斥状态"。

本次变更落在它没有取证的一个格子里：**一个 owner、一条记录、带状态判别**。因此 §1 表格中
"不合并"的两条红线正是该审计否决的两条，原证据继续有效。

## 2. 核心模型

### 2.1 一条记录，一个状态，一个地址

```text
ItemState
  ├─ ItemId      : 记录身份（域内唯一，不是网络身份）
  ├─ Stack       : ItemStack（ItemId + Quantity + Prefix）—— 载荷唯一权威
  ├─ Presence    : Embodied | Dormant
  ├─ Address     : ItemAddress —— 唯一位置，恰好一个
  │                 ├─ WorldSlot(short index 0..399)      Presence == Embodied
  │                 └─ ContainerSlot(container, index)    Presence == Dormant
  └─ Embodied 专用字段（仅 Presence == Embodied 时有意义）
        Position / Velocity / ReservedFor / NoGrabDelay / OwnTime / KeepTime
        / TimeSinceItemSpawned / Shimmered
```

三条硬规则：

1. `Address` **恒有且只有一个值**。不存在"游离物品"（既不在世界也不在容器）的中间态。
2. `Embodied` 字段只在 `Presence == Embodied` 时可读；`Dormant` 记录不分配、不复制这些字段。
3. 数量只存在于 `Stack`，容器槽与世界槽都不再各存一份。

### 2.2 两种状态的含义

| 状态 | 含义 | 是否有 tick | 是否入存档 | 网络身份 |
| --- | --- | --- | --- | --- |
| **Embodied（实体态）** | 物品在世界中，有位置与速度 | 是，世界 tick 推进物理与寿命 | 否（瞬态） | 世界槽号（包 21/22/39） |
| **Dormant（非实体态）** | 物品驻留在某个容器的槽位里 | 否，仅交互时读写 | 是，随所属容器写盘 | 容器实体 + 槽索引 |

Dormant 不是"被删除的世界物品"，Embodied 也不是"掉出来的容器物品"——它们是同一物品的两种驻留
形态，转移即改址。

### 2.3 为什么不做"单一类型"

`ItemState` 是**域内一条记录**，不是玩家背包里被复制 59 次的运行时对象。原版 `Item.Clone()`
是 `MemberwiseClone`，一旦让容器槽持有携带世界字段的实例，克隆就会连位置与预留一起复制
（[拆分判定](../reviews/audits/2026-11-23-item-worlditem-split-necessity.md) §3 方案 A）。
本设计用**地址 + 状态判别**避开这个陷阱：容器槽只存 `ItemId` 引用，不持有记录本体。

### 2.4 为什么不逐项实体化

物品记录留在**域内专用存储**，不为每条记录创建 ECS 实体。依据
[ECS Entity 组织设计约束](ECSEntity组织设计约束.md) §2.1 第 3 条："瓦片值、槽位、采样记录等
批量数据可以留在领域组件或专用存储内，不强制逐项实体化"。400 槽世界表与容器槽表都是密集槽位，
逐条实体化只会把槽表语义拆成 400 个独立生命周期。

## 3. 域内结构

合并后 Item 域是单一领域目录，按
[ECS 文件组织设计约束](../../Context/架构约束/ECS文件组织设计约束.md) §3.4 分四类职责目录：

```text
src/NSSLC/Item/                        # 合并后的 Item 域（服务端权威）
  ItemSystem.cs                        # 唯一提交写者（Commit 阶段）
  Component/
    ItemRequestQueueComponent.cs
  Command/
    ItemRequest.cs                     # 枚举 + 请求记录
    ItemRequestQueue.cs
  Query/
    IItemQuery.cs                      # 只读查询端口
  System/
    ItemState.cs                       # 记录 + 状态 + 地址
    ItemAddress.cs
    ItemPresence.cs
    ItemStore.cs                       # 域内专用存储（400 世界槽 + 容器索引）
    ItemRules.cs                       # 创建 / 合并 / 拾取 / 容量判定
    ItemMotionSolver.cs
    ItemPickupRules.cs
    ItemMergeRules.cs
    ItemMutation.cs                    # 已提交变更（供网络投影消费）
```

`src/NSSLC/Share/Item/` 仍是**共享载荷层**，职责不变：`ItemStack`、`Item`、`ItemRegistry`、
`ItemId`。它不持有任何状态、不参与 tick。

`src/NSSLC/Share/Inventory/` 收窄为**容器域**：槽布局、容量、类别接收规则、手持索引。
它不再持有物品数量——槽内容变成一个 `ItemId` 引用（指向 Item 域的记录）。

### 3.1 域内系统（不是"多个域"）

| 系统 | 职责 | 是否写权威存储 |
| --- | --- | --- |
| `ItemSystem` | 在 Commit 阶段排空请求队列并提交 | **是，唯一写者** |
| `ItemMotionSystem` | Embodied 记录的物理推进（重力/碰撞/水域） | 否，产出手动请求 |
| `ItemPickupSystem` | 筛选可拾取候选、提议转移 | 否，产出请求 |
| `ItemCombineSystem` | 邻近同堆合并判定 | 否，产出请求 |
| `ItemProjectionSystem` | 把已提交变更投影到网络 | 否，只读 |

依据 [ECS 系统输入边界约束](../../Context/项目约束/ECS系统输入边界约束.md) 规则 2：这些系统
同属一域，可以查询自己的状态并就地推进，**不需要**请求层。请求队列只保留一个用途——
**阶段屏障**：Projection 期间产生的变更推迟到下一 tick 的 Commit，避免投影观察未提交状态。

## 4. 不变量

| ID | 不变量 | 判定方式 | 相对 11-23 版 |
| --- | --- | --- | --- |
| INV-1 | **载荷唯一**：身份/数量/前缀只以 `ItemStack` 表达 | 静态：不存在同义平铺字段 | 保留 |
| INV-2 | **地址唯一**：一条记录同一时刻恰好一个 `ItemAddress`，不含游离态 | 运行：转移后地址数 == 记录数 | 由"宿主互斥"改写 |
| INV-3 | **单一写者**：只有 `ItemSystem` 在 Commit 阶段写权威存储 | 静态：只读端口不暴露写入口 | 由"两侧各一写者"合并 |
| INV-4 | **转移守恒**：改址前后总量守恒，部分成交时 `剩余 = 原量 − 实收` | 运行：`invariant-*` 断言 | 保留 |
| INV-5 | **瞬态/持久分离**：Embodied 不入任何存档路径，Dormant 必须入 | 静态：存储无 Embodied 写档 API | 保留 |
| INV-6 | **状态字段隔离**：Embodied 字段只为 Embodied 记录读取 | 静态：属性只在 `Presence == Embodied` 分支可达 | **新增** |

INV-6 是合并带来的新风险：两个系统分开时，世界字段不可能被容器侧读到；合并后必须用状态判别
把这条边界显式化。`ItemState` 的 Embodied 字段应以"仅在 Embodied 下有效"的方式暴露，
或以 `EmbodiedFields?` 子记录承载，让误读在类型层不可写。

## 5. 状态转移

转移是**原子改址**，不是"删一条、建一条"。

### 5.1 Dormant → Embodied（丢弃 / 掉落）

```text
容器槽 index 的记录 R（Dormant, Address=ContainerSlot）
  → 选一个空闲世界槽 s
  → R.Presence := Embodied, R.Address := WorldSlot(s)
  → 位置/速度 := 投放点，NoGrabDelay := 掉落者延迟
  → 同一提交内：容器槽 index 置空
```

触发方是客户端包 21（`num == 400` 是"请新建"的哨兵，服务端自行分配槽位），见
[上位约束](物品载荷与单一物品系统设计约束.md) §5.2。请求里的槽号**不得**当作客户端授权。

### 5.2 Embodied → Dormant（拾取）

```text
世界槽 s 的记录 R（Embodied, 数量 N）
  → 容器侧决定实际接收量 k（k ≤ N，受类别/容量限制）
  → 同一提交内：
       k == N : R.Presence := Dormant, Address := ContainerSlot(c, i)，世界槽 s 释放
       k <  N : 新建 Dormant 记录（数量 k）落到 ContainerSlot(c, i)
                R.Stack := R.Stack.WithQuantity(N − k)
```

**容器只回报实收数量，Item 域按该数量扣减**——这是 INV-4 的实现方式，不是事后对账。
部分成交不会复制也不会吞物品。

拾取门控顺序与 Version4 一致：`noGrabDelay > 0` 时直接不可拾取，连"能否装下"都不判断；
判定放在候选筛选（`ItemPickupRules.CanBePickedUp`），计数在 tick 末尾统一递减——
**先判定后递减**。

### 5.3 不得出现的转移

- 容器槽之间直接搬运数量而不经改址（会绕过 INV-2）；
- Embodied 记录被复制一份到容器且原记录保留（双重拥有）；
- 转移中引入"无地址"中间态（会破坏 INV-2 的可判定性）。

## 6. 提交点与调度

| 约束 | 理由 |
| --- | --- |
| Item 域 tick 晚于世界时钟推进 | 物理跟随世界时间 |
| 拾取结算紧随 Item tick | 同 tick 产生的拾取事实必须在本 tick 内结算完 |
| Item 请求只在 Commit 阶段执行 | 权威存储只在 Commit barrier 被写；Projection 产生的请求留到下一 tick |
| 网络投影在 Commit 之后 | 投影只观察已提交状态，不回写 |

合并后的已知窗口与合并前相同：Simulation 阶段容器侧已接受、Commit 阶段 Embodied 侧才扣减。
**任何跨状态读取都必须在 Commit 之后**；若将来有系统必须在该窗口内同时读两种状态，应把拾取
结算整体移入 Commit，而不是放宽 INV-2/INV-4。

## 7. 网络与持久化边界

| 边界 | Embodied | Dormant |
| --- | --- | --- |
| 网络 | 包 21/22/39，**以世界槽号寻址** | 走玩家背包槽报文，不含世界槽语义 |
| 存档 | 不入世界文件 | 随所属容器逐槽写盘 |

网络入口必须校验：包 21 的 `itemType` 经 `ItemIdRules.TryFromNetworkType` 转换，未知 id
直接丢弃。当前实现的缺口见 §11.3。

## 8. 与 Version4 的对应

| Version4 | 本设计 | 说明 |
| --- | --- | --- |
| `Terraria.Item`（不继承 `Entity`） | 共享载荷层 `ItemStack` | 身份 / 数量 / 前缀 |
| `Terraria.WorldItem : Entity`，持 `inner Item` | `ItemState`（Embodied） | 位置/速度/预留/寿命在记录上，不在载荷上 |
| `Main.item[401]` | `ItemStore` 的世界槽段 | 400 槽，槽号即网络身份 |
| `Player.inventory[59]` / `Chest.item` | `ItemStore` 的容器索引段 | 槽表仍归 Inventory 域，内容改成 `ItemId` 引用 |
| `Player.HeldItem => inventory[selectedItem]` | `InventoryComponent.Held` + 读该槽记录 | 手持是派生读取，不是第三个存储 |

Version4 用两个类型表达两种状态；本设计用**一条记录 + 状态判别**表达同一件事。差别在
"谁是状态 owner"，不在"物品有哪些字段"。

## 9. 合并带来的能力

- **物品自带子库存**（如枪械内置弹药库）重新可表达：Dormant 记录可以挂子容器地址，
  不必先让物品成为世界实体——这正是 11-23 删除 `ContainedInInventoryComponent` 时记录的代价。
- **单条记录的完整生命周期**：一条物品从容器到世界再回容器，不再跨两个存储重建身份。
- **守恒可直接判定**：总量 = Σ`Stack.Quantity`，一次扫描即可验证，不需要在世界侧与容器侧
  各扫一次再对账。

## 10. 代价

| 代价 | 说明 |
| --- | --- |
| 容器槽内容改引用 | `InventorySlot.Content` 由 `ItemStack?` 变为 `ItemId?`，所有读写方需改 |
| 拾取路径重写 | `InventoryPickupRequestSystem` 从"写自己槽位"改为"回报接受量"，Item 域统一改址 |
| 世界槽与容器槽共用存储 | 需要两段地址空间的一致性校验（槽号不撞、释放可回收） |
| 存档格式 | 容器存档仍需逐槽写 `ItemId + Stack`，格式不变；但加载时要重建记录表 |
| INV-6 的实现成本 | 状态字段需以类型手段隔离，否则合并后容易误读世界字段 |

## 11. 当前实现与迁移

### 11.1 现状（2026-11-25，代码未迁移）

| 项 | 当前位置 | 形态 |
| --- | --- | --- |
| 世界物品 | `src/NSSLC/WorldItem/` | `WorldItemStore`（400 槽）+ `WorldItemState` + `WorldItemRequestSystem` |
| 容器物品 | `src/NSSLC/Share/Inventory/` | `InventoryComponent` + `InventorySlot.Content : ItemStack?` |
| 载荷 | `src/NSSLC/Share/Item/` | `ItemStack` / `Item` / `ItemRegistry` |

两套存储各自持有数量，靠 `WorldItemRequestKind.Consume` 的回报量对账——这是**本次要消除的**
双写形态。

### 11.2 迁移批次

| 批次 | 内容 | 验收 |
| --- | --- | --- |
| M0 | 建 `ItemState`/`ItemAddress`/`ItemPresence` 与 `ItemStore`，把 `WorldItemStore` 的 400 槽搬进世界槽段 | 现有 `Terraria.WorldItem` 断言全绿 |
| M1 | 容器槽内容改 `ItemId` 引用，`InventorySystem` 的读写转到 `ItemStore` | `Terraria.Inventory` 的 `slot-*` / `held-*` 全绿 |
| M2 | 合并写者：`ItemSystem` 取代 `WorldItemRequestSystem`，拾取改为回报接受量 | `invariant-*` 全绿，含新的 INV-2 / INV-6 |
| M3 | 网络投影与包 21 入口校验收敛到合并后的 `ItemMutation` | 包 21 / 22 / 39 行为不变 |
| M4 | 删除 `src/NSSLC/WorldItem/` 与 `InventorySlot.Content` 旧路径 | 全仓零残留引用 |

每个批次都必须在旧路径与新路径**同时存在**期间保持单一写源，不得两侧各推一份再互相同步
（[ECS Entity 组织设计约束](ECSEntity组织设计约束.md) §10）。

### 11.3 顺带修复的既有缺口

- 世界侧缺 `MaxStack` 校验：`ItemStore` 创建路径必须校验定义的 `MaxStack`，否则客户端
  包 21 可造出超上限的世界堆，容器整堆拒绝后该堆永不可拾取。
- 包 21 的 `itemType` 入口校验缺失（`GameCommunicationAdapter` 直接透传），应在 M3 补齐。

## 12. 验收检查清单

- [ ] 新增字段是否只属于"载荷 / 状态（Embodied / Dormant）/ 容器布局 / 内容定义"中的恰好一层？
- [ ] 是否出现了第二条记录拥有同一物品数量的权威副本？
- [ ] 是否出现既不在世界也不在容器的"游离"记录？
- [ ] Embodied 字段是否可能被 Dormant 记录读到？
- [ ] 是否有写者绕过 `ItemSystem` 直接改 `ItemStore`？
- [ ] 容器槽是否仍持有 `ItemStack` 本体而不是 `ItemId` 引用？
- [ ] 转移是否在单次提交内完成，并且数量守恒？
- [ ] Embodied 记录是否被写进了任何存档路径？
- [ ] 新计数是否在 tick 末尾递减（保持"先判定后递减"）？
- [ ] `invariant-*` / `slot-*` / `held-*` 断言是否仍然全绿？

## 13. 变更记录

| 日期 | 变更 |
| --- | --- |
| 2026-11-25 | 建立本设计。原"两个宿主 owner"合并为单域双状态；保留载荷唯一、转移守恒、瞬态不入档；新增 INV-2（地址唯一）与 INV-6（状态字段隔离）；代码迁移排为 M0–M4，尚未执行 |
