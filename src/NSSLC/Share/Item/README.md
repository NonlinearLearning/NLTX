# Item 领域

本目录是**共享载荷层**：只表达"一个物品是什么"，不表达"它此刻在世界的哪里"或"它在谁的容器里"。
后两者分别是世界宿主与容器宿主的职责，见 §"双宿主边界"。

```text
ItemDefinition                    # 每物品一条不可变定义（MaxStack / Categories / 属性修饰器）
    ↓ ItemId
ItemStack                         # 值对象：ItemId + Quantity + Prefix
    ↓ 被两个宿主各自持有
世界宿主：WorldItemState.Item      容器宿主：InventorySlot.Content
```

`ItemDefinitionRegistry` 只管理不可变定义，不保存物品实例、库存或世界槽位。

## 载荷只有一份定义

物品身份（`ItemId`）、数量（`Quantity`）、前缀（`Prefix`）**只以 `ItemStack` 表达**。
任何宿主都不得把这三项平铺成自己的字段，否则同一物品会出现两份权威来源。

这条不是风格约定，是守恒的前提：拾取时"世界扣多少、背包收多少"要对得上，
只有当两侧读的是同一种值对象时，这个等式才可能被验证而不是被推断。

## 双宿主边界

```text
                共享载荷层（无 tick、无 owner、无世界字段）
                            ItemStack / ItemDefinition
                            │
        ┌───────────────────┴───────────────────┐
        ▼                                       ▼
  世界宿主（瞬态）                        容器宿主（持久）
  WorldItemStore + WorldItemState          InventoryComponent + InventorySlot
  · 400 槽运行时表，不写入存档              · 随玩家/箱子存档
  · 位置、速度、归属预留、生命周期计数        · 只存 ItemStack 值
  · 只有 WorldItemRequestSystem 能写        · 只有 InventorySystem 能写
        └────────── 转移只经请求组件，有方向 ──────────┘
```

判定口径与验收断言见
[物品载荷层与双宿主 owner 执行计划](../../../../docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md) §3 与
[Item 与 WorldItem 拆分判定审计](../../../../docs/reviews/audits/2026-11-23-item-worlditem-split-necessity.md)。

## 当前实现状态（2026-11-23）

世界物品走的是**非 ECS** 的 `WorldItemStore`（`Terraria.World` 命名空间，位于 `src/NSSLC/WorldItem/`）。
它对齐 Version4 的 `Main.item[401]` 槽表：400 个槽位、槽位号即网络身份、进程内瞬态、不入存档。

**实体化物品路径已删除**（执行计划 P1-A）。`ItemStackComponent`、`ItemWorldStateComponent`、
`ContainedInInventoryComponent` 与 `InventorySlot.ItemEntityId` 全部移除：全仓没有
`new ItemStackComponent(`，只有读取分支，保留它等于允许同一物品在世界与容器两端各存一份权威。

被删除的能力："物品实体自带子库存"（如枪械内置弹药）不再有容器表达。当前仓库没有这类内容；
若将来需要，必须作为独立设计重新引入，而不是恢复一条无生产者的声明——见
[执行计划](../../../../docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md) P1-B 的代价说明。

## 装备与属性贡献

装备、饰品和武器对属性的贡献由物品定义提供，经
`InventoryAttributeContributionSystem` 作为 `AttributeModifier` 来源提交给持有者的
`AttributeSetComponent`。`Item` 不保存一份玩家装备槽，`Player` 也不拥有装备关系组件；
槽位类别是唯一的装备归属事实。
