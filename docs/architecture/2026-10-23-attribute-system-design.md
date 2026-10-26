# 属性修饰系统设计

文档 ID：DOC-2026-10-23-ATTRIBUTE-SYSTEM-DESIGN  
状态：已实现共享契约；客户端迁移和宿主接入仍是后续工作  
日期：2026-10-23（Asia/Shanghai）

## 目标

把“来源对属性的数值贡献”统一成可追踪、可过期、可聚合的共享契约。当前实现位于
`src/NSSLC/Components/Share/Attributes`，采用项目 Share 层的边界：组件保存领域状态，
Calculator 进行无副作用计算，System 管理来源和时限。该机制不直接读取
`D:\TRbackup\最新的客户端` 的类型，也不复制其代码风格或命名。

## 设计决定

### 属性能力由组件声明

实体只有在组合中声明 `AttributeSetComponent` 时才拥有属性系统能力。属性计算 API 直接接收
这个组件；没有组件的实体不会获得隐含的全局属性集合，也不会由计算器自动创建组件。这让
ECS 查询可以用组件存在性表达“实体支持哪些属性”。

组件内保存：

- `BaseValues`：每个属性的基础值；通过 `SetBaseValue` 按属性键替换。
- `Modifiers`：活动的 `List<AttributeModifier>`；公开为只读视图，增删通过受控 API 完成。

生命池是明确的例外：当前生命、最大生命、最小生命和存活状态由
`Terraria.Combat.LifeComponent` 单独承载，不放入 `AttributeKind`、`BaseValues` 或
`Modifiers`。`LifeRegeneration` 若被使用，只表示战斗领域消费的恢复速率贡献；生命恢复、
持续伤害和生命变化仍由战斗领域负责。

### 来源分类

`AttributeSourceCategory` 提供八个分类：`Buff`、`Equipment`、`Accessory`、`Weapon`、
`Boss`、`Enemy`、`Npc` 和 `FriendlyCreature`。这些值是修饰来源的标签，不是实体继承树，
也不会自动判断 Boss 与 Enemy 的关系。创建来源时选择适用的分类；若规则需要同时表达多个
标签，应在来源层产生多条具有明确来源身份的贡献。

这些分类来自参考客户端中观察到的来源边界：Buff 更新、护甲/装备和前缀、饰品与武器效果，
以及 Boss、普通敌怪、NPC 和友好生物的实体来源。参考客户端的实体行为仍应由宿主映射，
共享组件只保存已经投影出的属性贡献。

### 修饰器格式和求值

每条修饰器包含属性键、来源分类、运算、数值、非零来源身份、优先级和剩余 tick。支持：

1. `Additive`：固定值加法，例如防御 `+8` 或耐力 `+0.10`。
2. `Multiplicative`：乘法，例如 `0.8` 倍；百分比单位由调用方约定，不由系统猜测。
3. `Override`：使用优先级最高者；优先级相同使用组件分配的加入序号，避免依赖列表遍历顺序。

固定求值顺序为：

```text
基础值（或调用方 fallback）
→ 汇总所有 Additive
→ 汇总所有 Multiplicative
→ 应用选中的 Override（若存在）
```

所有输入和结果都必须是有限浮点数。`RemainingTicks = -1` 表示永久，正数表示限时；零和
小于 `-1` 的新修饰器拒绝加入。`AttributeSystem.Advance` 每 tick 推进并移除到期项，
`RemoveSource` 可在卸装、驱散或来源销毁时批量删除贡献。

### 与 Buff 和战斗的边界

Buff 与属性修饰在“来源贡献数值”层面同构，但 Buff 还可能拥有免疫、刷新、叠层、周期伤害、
召唤物维护和表现等行为。Buff 可以向属性组件加入多条修饰器，不能被强制压缩为一个浮点值。

防御、耐力和伤害倍率可以由属性组件聚合；本次受击的防御公式、防御前减伤、最终减伤、闪避、
护盾、分伤和受击后消耗仍属于战斗结算系统。甲虫层数、日耀盾、圣骑士分伤和狱火周期效果
不能直接当作普通常驻属性修饰器。

### Player、Inventory 与共享状态的边界

Player 不拥有装备槽或 Buff/Debuff 数值的私有组件；服务器使用共享属性和保护窗口组件保存防御事实：

- 武器、护甲、饰品和弹药的槽位与归属由 `InventoryComponent`、`InventorySlotDefinition`、
  `InventorySlot` 和 `ContainedInInventoryComponent` 表达；
- 物品关系提交成功后，装备/饰品/武器向持有者的防御、抗性、耐力和伤害贡献以
  `AttributeModifier` 写入 `AttributeSetComponent`，移除或转移时按 `SourceId` 撤销；
- `AttributeSetComponent` 保存 Player 基础 `Defense` 与减伤属性；`ProtectionWindowComponent`
  保存共享保护窗口剩余 Tick；Inventory 和 Buff 的属性贡献直接进入 Attributes；
- Buff/Debuff 的数值效果同样只作为带来源、运算和期限的 `AttributeModifier`，不建立
  `PlayerBuffStateComponent`；
- 无敌帧、受击免疫、闪避冷却、护盾消耗等通用规则由 Combat 系统消费共享保护窗口组件，
  不能再创建一个客户端防御组件或第二份 Player 防御事实。

`BuffListComponent` 和 `BuffImmunityComponent` 若被实体组合使用，只表达非数值状态效果的
标识和免疫资格。它们不保存防御、抗性或其他属性结果，也不恢复 Player 私有 Buff 组件。

## 参考依据

完整源码证据和外部资料比较见：

- [属性修饰与 Buff 统一机制研究](../research/2026-10-23-attribute-modifier-system-research.md)
- 参考客户端 `D:\TRbackup\最新的客户端`，版本 `Terraria 1.4.5.8`，重点为
  `Terraria/Player.cs` 的 `ResetEffects`、Buff 更新、装备防御、耐力和 `Hurt` 路径。
- Unreal Engine Gameplay Attributes / Gameplay Effects 官方文档：属性值与效果分离，效果
  管理持续时间和修饰贡献。
- tModLoader `StatModifier` 与 `Player.HurtModifiers` 源码：区分加算、乘算、输入伤害倍率、
  防御和最终伤害阶段。

## 已知取舍

统一修饰器可以集中数学规则、追踪来源、按来源撤销并复用调试投影；代价是需要明确运算阶段、
来源实例、条件、网络重建和缓存失效，且把所有 Buff 行为塞进一个容器会形成万能系统。当前
实现选择共享“属性贡献和生命周期”契约，不改变客户端 Buff UI、网络协议、持久化和战斗顺序。

推荐后续以防御和 `Endurance` 做宿主试点：保留现有权威状态，将其投影为属性修饰器，逐项对照
旧公式、整数取整、来源移除和条件切换，再决定是否扩展到其他属性。


