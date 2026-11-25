# Item 领域

本目录是**合并后的单一 Item 域**：一层共享载荷 + 一条记录 + 两种互斥状态。
它同时表达"一个物品是什么"（载荷）和"它此刻驻留在哪里"（状态），
不再把世界物品与容器物品拆成两个系统。

```text
ItemId / ItemStack(ItemId, Quantity, Prefix)   # 载荷唯一权威（Item.cs / ItemStack.cs）
    ↓ 被记录持有
ItemState                                       # 一条记录 = 载荷 + Presence + 地址
    ├─ Presence = Embodied → Slot 0..399        # 在世界里，有位置/速度，逐 tick 推进
    └─ Presence = Dormant  → Container(宿主,槽)  # 在容器里，无世界字段，不 tick
    ↓
ItemStore                                       # _worldSlots(400) + _containerSlots(地址索引)
```

## 载荷只有一份定义

物品身份（`ItemId`）、数量（`Quantity`）、前缀（`Prefix`）**只以 `ItemStack` 表达**。
任何宿主都不得把这三项平铺成自己的字段，否则同一物品会出现两份权威来源。

这条不是风格约定，是守恒的前提：拾取时"世界扣多少、背包收多少"要对得上。
合并后这个等式由**改址**保证，而不是由两侧对账保证。

## 一条记录，一个地址

| 形态 | 地址 | 有 tick | 入存档 | 网络身份 |
| --- | --- | --- | --- | --- |
| **Embodied** | `Slot`（0..399） | 是 | 否（瞬态） | 世界槽号（包 21/22/39） |
| **Dormant** | `Container(container, slot)` | 否 | 是（随容器） | 容器实体 + 槽索引 |

地址恒有且只有一个，不存在既不在世界也不在容器的游离记录。改址是一次原子提交：

- **Embodied → Dormant**（拾取）：`ItemStore.TryTransferToContainer`，同一 lock 内释放世界槽并建立容器槽；
- **Dormant → Embodied**（丢弃）：`ItemStore.TryTransferToWorld`。

## 目录职责

| 路径 | 职责 |
| --- | --- |
| `Item.cs` / `ItemStack.cs` / `ItemId.g.cs` / `ItemRegistry.cs` / `ItemCategoryMask.cs` | 共享载荷层：不可变定义与值对象，无状态、无 tick |
| `ItemState.cs` | 记录 + `ItemPresence` + `ItemContainerAddress` + `ItemMutation` |
| `ItemStore.cs` | 权威存储：世界槽段 + 容器段，单一锁 |
| `ItemSystem.cs` | **Commit 阶段唯一写者**，排空请求队列 |
| `ItemTickSystem.cs` | Embodied 记录的物理推进（合并 / 运动 / 拾取候选 / 生命周期），只产请求 |
| `System/ItemRules.cs` 等 | 纯规则：合法性、合并、拾取、运动积分 |
| `Command/` `Component/` `Query/` | 请求队列、只读端口（`IItemQuery` / `IItemProjectionSource`） |

## 与容器域的边界

容器域（`Share/Inventory/`）只持有**地址**，不持有载荷：
`InventorySlot.Content : ItemContainerAddress?`。因此数量永远只有一份权威，
"背包已加、世界未减"的中间态在存储层不可能出现。

判定口径与验收断言见
[Item 系统设计：单域 · 双状态](../../../docs/architecture/2026-11-25-item-single-domain-dual-state-design.md)
与[物品载荷与单一物品系统设计约束](../../../docs/architecture/物品载荷与单一物品系统设计约束.md)。

## 装备与属性贡献

装备、饰品和武器对属性的贡献由物品定义提供，经
`InventoryAttributeContributionSystem` 作为 `AttributeModifier` 来源提交给持有者的
`AttributeSetComponent`。`Item` 不保存一份玩家装备槽，`Player` 也不拥有装备关系组件；
槽位类别是唯一的装备归属事实。
