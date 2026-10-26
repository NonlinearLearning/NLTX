# Player 状态归属调整

- 日期：2026-10-25
- 范围：NSSLC Player、Inventory、Attributes 和 Combat 的组件边界
- 状态：已落实到共享组件设计和 Player Arch 系统

## 决定

1. `PlayerEquipmentComponent` 不再创建。背包、武器、护甲、饰品、弹药和物品归属由
   `InventoryComponent`、`InventorySlotDefinition`、`InventorySlot` 和物品实体关系表达。
2. `PlayerDefenseStateComponent` 保留为服务器权威 Player 状态，保存基础 `Defense`、基础
   `Resistance` 和保护窗口剩余 Tick。`PlayerDefenseStateSystem` 只负责保护窗口逐 Tick 过期。
3. 装备和 Buff/Debuff 对防御、抗性、耐力、伤害等数值的贡献使用
   `AttributeSetComponent` 中带来源和期限的 `AttributeModifier`。这些贡献不复制回
   Player 防御组件。
4. `BuffListComponent` 和 `BuffImmunityComponent` 只表达非数值效果的当前实例、免疫、刷新
   和叠层资格；周期伤害、治疗、击退和生命变化仍必须产生 Combat intent，由 Combat owner
   提交。
5. 客户端控制、输入、瞄准、UI、摄像机和表现状态不进入上述服务器组件。

## 影响

Inventory 物品关系提交后，`InventoryAttributeContributionSystem` 重建 Armor、Accessory
和 Weapon 来源；`AttributeTickSystem` 管理限时贡献；Combat 结算读取基础 Player 防御与
属性修饰后的输入，并负责伤害准入、最终减伤和保护窗口语义。Player 领域不再维护装备或
数值 Buff 的第二份状态。

当前 Projectile 局部公式使用基础 `Resistance` 与 Attributes 的 `DamageReduction`/`Endurance`。
`Defense` 已作为服务器事实保存，但攻击类型、伤害 profile 和防御到最终伤害的公式尚未确定，
因此没有把它伪装成已经生效的客户端等价公式或复制成另一份减伤状态。

## Combat 接入

`ProjectileCombatCommitSystem` 是当前第一条共享 Combat 接入路径。它在 Commit 阶段读取
Projectile 候选，检查目标 Player 的保护窗口，合并 `PlayerDefenseStateComponent` 的基础
抗性与 `AttributeSetComponent` 的 `DamageReduction`/`Endurance` 修饰，调用唯一的
`DamageCommitSystem` 写入 Life，成功后把 `AcceptedHitReceipt` 交给 Projectile owner。
它不把装备、Buff 或客户端状态复制到 Player 组件。

## 验证

- `dotnet build src/NSSLC/Terraria.Components.csproj --no-restore`：0 warnings，0 errors。
- 从构建产物反射实例化 `PlayerDefenseStateComponent`，验证防御/抗性边界、保护窗口延长、
  逐 Tick 递减、清除和非法抗性拒绝。
- `dotnet run --project .agent-workplace/buff-smoke/Smoke.csproj --no-restore`：
  Buff/NPC 和 Projectile ECS smoke 均通过；其中 Projectile smoke 覆盖 Player 保护窗口和
  Attributes 减伤。

## 未决项

完整 Combat owner 仍需补齐通用伤害请求、护盾消耗、闪避、击退、状态效果和 NPC/Player
内容规则；当前 Projectile 候选的准入、减伤、Life 提交和 Projectile 回执已经形成局部闭环。
本次没有伪造客户端输入或网络适配器。
