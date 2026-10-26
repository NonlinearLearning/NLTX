# 属性修饰系统

`Attributes` 保存跨运行时复用的属性契约和无平台依赖的属性计算。它把“来源向某个属性
贡献一个数值修改”统一起来，因此装备、Buff/Debuff 和其他来源对防御、抗性、耐力、伤害
的数值修饰不再由 Player 私有组件保存。Player 的基础防御和抗性由
`AttributeSetComponent` 保存，保护窗口由共享 `ProtectionWindowComponent` 保存。装备关系由 Inventory 持有，属性系统只接收物品/效果提交的贡献；
状态效果的免疫、刷新、叠层、周期行为和其他非数值行为仍留在各自领域。

## 能力门槛

只有声明 `AttributeSetComponent` 的实体才拥有属性系统能力。宿主查询实体时应先查询这个
组件，再把组件传给 `AttributeCalculator` 或 `AttributeSystem`。没有组件的实体不会自动获得
默认属性，也不会通过全局注册表生成一个隐含属性集合。

`AttributeSetComponent` 保存两个列表的内部实现：

- `BaseValues` 保存实体的基础属性值，并通过 `SetBaseValue` 按属性键替换。
- `Modifiers` 保存当前活动的 `List<AttributeModifier>`。公开 API 返回只读视图，增删由组件
  和 `AttributeSystem` 控制，避免调用方绕过来源和序号规则修改集合。

`AttributeSetComponent`、`BuffListComponent` 和 `BuffImmunityComponent` 都是共享能力组件，
不属于 Player、NPC 或 Projectile 的私有状态。实体是否允许接收某类 Buff，应由共享 Buff
领域的接收能力/策略声明（例如规划中的 `BuffReceptionCapabilityComponent`），再由
`BuffAdmissionSystem` 在同一准入边界检查；不要为 NPC 或 Player 各自复制一套
`NpcBuffStateComponent`/`PlayerBuffStateComponent` 作为 Buff 权威状态。没有接收能力时，
Buff 请求应被拒绝或按定义转为无效结果，不能隐式创建组件。

生命池不属于这个组件。当前生命、最大生命、最小生命和生命存活状态由
`Terraria.Combat.LifeComponent` 单独保存；生命恢复、持续伤害和生命变化由战斗领域的组件
与系统负责。`LifeRegeneration` 如果被来源修饰，也只表示供战斗领域消费的恢复速率贡献，
不表示当前生命或最大生命；属性修饰器不能通过 `AttributeKind` 直接创建或保存生命池。

## 来源分类

`AttributeSourceCategory` 是修饰来源的分类，不是实体继承树：

| 分类 | 用途 |
| --- | --- |
| `Buff` | 限时状态或药剂向属性提供的贡献 |
| `Equipment` | 护甲、套装或其他装备槽的贡献 |
| `Accessory` | 饰品及其前缀提供的贡献 |
| `Weapon` | 武器本身、前缀或持有状态提供的贡献 |
| `Boss` | Boss 来源实体提供的属性贡献 |
| `Enemy` | 普通敌怪来源实体提供的属性贡献 |
| `Npc` | NPC 来源实体提供的属性贡献 |
| `FriendlyCreature` | 友好生物、召唤物或同伴来源提供的贡献 |

Boss 和 Enemy 是否同时适用由来源创建者决定；这个枚举不会偷偷推断客户端的实体分类。

## 计算规则

`AttributeModifier` 记录属性键、来源分类、来源身份、运算、数值、优先级和剩余 tick。`-1`
表示永久修饰器，正数表示剩余 tick；零和小于 `-1` 的新修饰器会被拒绝。来源身份必须非零，
以便卸下装备、驱散 Buff 或销毁来源时按 `SourceId` 移除贡献。

`AttributeCalculator.Resolve` 使用固定顺序：

```text
value = base value or caller fallback
value += all Additive modifiers
value *= all Multiplicative modifiers
value = highest-priority Override, if any
```

相同 Override 优先级使用组件分配的加入序号解决，不能依赖 `List` 的遍历顺序。统一系统不
自动规定百分比单位、属性上下限或防御取整；这些属于具体属性和战斗结算域的契约。

## Combat 属性

伤害系统不为 Player、NPC 或 Projectile 建立元素抗性、护盾、反射、吸血和暴击专属组件。
这些数值由 `AttributeKind` 和 `AttributeCombatCalculator` 提供。随机暴击使用显式
`CriticalRollInput`；元素抗性使用各元素抗性键；护盾使用 `Shield` 当前池和 `MaxShield`
容量键；反射和吸血使用 `ReflectRatio` 与 `LifeStealRatio`。

`Shield` 的当前值是 Attributes 的基础值，不能用修饰器重复叠加；`AttributeSystem.ConsumeShield`
是护盾池的唯一消费入口。Combat 负责请求排序、准入和生命提交，后续效果 owner 负责提交
反射伤害或吸血治疗。结算顺序为：攻击属性和暴击 → 防御 → 元素抗性 → 最终减伤 → 护盾吸收
→ 实际生命伤害。`True` 伤害跳过防御、抗性和最终减伤，但当前仍经过护盾。

护盾完全吸收时，Combat 回执的 `HasEffect` 为真而 `HasDamage` 为假；这允许 Projectile 等来源
消费一次有效命中，同时不会把护盾消耗误报为生命伤害。

Projectile 命中边界也使用相同的数值聚合：`ProjectilePenetrationLimit` 提供有效穿透上限，
负值表示无限；`ProjectileHitImmunityTicks` 提供每目标免疫窗口的 Tick 数。两个属性只提供
数值结果，不保存目标关系、命中计数、免疫到期记录或终止状态；这些运行态分别由 Projectile
免疫/穿透组件和生命周期系统维护。

## 生命周期边界

`AttributeSystem.Advance` 每推进一个 tick 就减少限时修饰器的剩余时间，并移除到期项。
`RemoveSource` 和 `RemoveSourceCategory` 用于来源失效时清理列表。组件、计算器和系统不执行
网络、文件、UI、日志、伤害、治疗、召唤物生成或周期事件。

属性系统取代 Player 私有的 Buff/Debuff 状态仅适用于数值贡献：例如防御、抗性、耐力、
伤害倍率、移动速度或生命恢复率都应成为带有明确 `SourceId` 和期限的
`AttributeModifier`。`PlayerBuffStateComponent` 不再创建；装备槽也不在 Player 组件中
重复保存。`BuffListComponent`/`BuffImmunityComponent` 如果被实体组合使用，只表达非数值
效果标识和免疫资格，不能作为属性值的第二份来源。

共享 Buff 处理建议保持以下写入闭包：

```text
BuffApplyIntent / BuffApplyCommand
    → BuffAdmissionSystem
        → 检查接收能力、免疫、来源、阵营和叠层规则
    → BuffSystem
        → 更新 BuffListComponent
        → 添加、刷新或移除 AttributeModifier
        → 产生周期效果/Combat intent
    → AttributeSystem / CombatSystem / MovementSystem 消费结果
```

`BuffSystem` 是所有实体类别共用的规则 owner；NPC 只通过组件组合进入查询。Buff 对属性的
数值贡献必须以 `AttributeModifier` 进入 `AttributeSetComponent`，由 `AttributeSystem` 计算
和推进；Buff 的周期伤害、治疗、击退或召唤请求不能直接改目标的 `LifeComponent`，而应由
Combat 系统按自己的准入和提交规则处理。`BuffListComponent` 当前保存效果 ID、来源、
剩余时间和层数；如果以后需要周期触发进度，应继续扩展共享 Buff 状态模型，而不是重新引入
NPC 私有 Buff 状态。

Buff 仍然可以拥有免疫、刷新、叠层、周期伤害、召唤物维护或表现效果；这些行为由 Buff 或
战斗领域管理。它可以向本组件贡献一条或多条数值修饰器，但不能把所有 Buff 语义压扁成一个
浮点值。防御和耐力可以由这里聚合，某次受击如何使用防御、最终减伤、闪避、护盾和受击后
消耗则仍由战斗结算系统负责。

## 示例

```csharp
var attributes = new AttributeSetComponent();
attributes.SetBaseValue(new AttributeBaseValue(AttributeKind.Defense, 20f));

attributes.AddModifier(new AttributeModifier(
  AttributeKind.Defense,
  AttributeSourceCategory.Equipment,
  AttributeModifierOperation.Additive,
  8f,
  sourceId: 1001));

attributes.AddModifier(new AttributeModifier(
  AttributeKind.Endurance,
  AttributeSourceCategory.Buff,
  AttributeModifierOperation.Additive,
  0.10f,
  sourceId: 2001,
  remainingTicks: 600));

float defense = AttributeCalculator.Resolve(attributes, AttributeKind.Defense);
```

本组件表达“实体拥有什么属性贡献”，而不是“玩家、Boss 或 NPC 是什么继承类型”。宿主可
在 ECS 查询中使用 `AttributeSetComponent` 作为能力门槛，再把计算结果交给移动、战斗或 UI
投影。`BuffListComponent` 和 `BuffImmunityComponent` 同样按共享能力查询；NPC、Player、
Projectile 可以拥有、拒绝或按策略接收相同 Buff，而不会形成三套 Buff 系统。

