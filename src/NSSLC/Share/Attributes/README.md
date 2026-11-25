# 属性修饰系统

`Attributes` 保存跨运行时复用的属性契约和无平台依赖的属性计算。它把“来源向某个属性
贡献一个数值修改”统一起来，因此装备、Buff/Debuff 和其他来源对防御、抗性、耐力、伤害
的数值修饰不再由 Player 私有组件保存。Player 的基础防御和抗性由
`AttributeSetComponent` 保存；受击免疫窗口不属于属性，由共享 `ImmunityWindowComponent` 按伤害通道保存。装备关系由 Inventory 持有，属性系统只接收物品/效果提交的贡献；
状态效果的免疫、刷新、叠层、周期行为和其他非数值行为仍留在各自领域。

## 子域划分

根层是十四类泛 Buff 形态的**共有抽象**（属性键、贡献、来源描述符、集合、计算、生命周期）。
形态专属的部分留在两个子域：

| 子域 | 承载 | 形态数 |
| --- | --- | --- |
| `Buff/` | 真正走 Buff 列表的形态：限时、永久、食物 | 3 |
| `Contribution/` | 独立管线形态：套装、饰品、前缀、附魔、召唤物、坐骑、幸运值、旗帜、潜行、状态布尔、冷却 | 11 |

两个子域都不定义自己的贡献类型或写入函数，只使用根层的共有类型。子域之间的差别是
**状态 owner 与期限语义**不同，不是计算规则不同。详见各自的 README。

## 共有抽象

十四类形态都以"某个来源提出若干条数值贡献"的形式进入属性系统，因此共有四件事，
各自对应一个根层类型：

| 类型 | 形态 | 职责 |
| --- | --- | --- |
| `AttributeContribution` | readonly record struct | 一条贡献：属性键 + 运算 + 数值 + 优先级 |
| `AttributeSourceDescriptor` | readonly record struct | 来源身份 + 来源分类 + 剩余期限（`-1` 永久） |
| `AttributeContributionSet` | sealed class | 一个来源的一组贡献，整体加入或整体撤销 |
| `AttributeContributionSystem` | static class | 翻译贡献并委托 `AttributeSystem` 增删 |

`AttributeSetComponent` 只保存属性数值和修饰器；状态效果的实例、行为组件和资源组件由各自
领域持有。上述四个类型是定义侧与写入侧的值对象，不进入实体存储。

## 属性键分类（B1）

`AttributeKind` 仍保留旧键以支持迁移期读取，但每个键现在由
`AttributeKindMetadata` 明确分类。普通 `AttributeContribution` 和
`AttributeModifier` 只允许 `ComputedScalar`、`Capacity` 或 `Policy`：

- `ComputedScalar`：防御、伤害、移动速度、抗性、暴击、生命再生等派生数值；
- `Capacity`：最大护盾、呼吸容量等容量参数；
- `Policy`：由专用系统解释的派生规则参数；
- `ResourcePool`：当前护盾等资源当前值，必须由资源组件持有；
- `Cooldown`：剩余冷却，必须由冷却组件持有；
- `Identity`：坐骑身份等关系状态，必须由身份/关系组件持有；
- `StatusFlag`：隐身、眩晕等具体状态，必须由具体行为组件持有。

旧的 `Shield`、冷却、`MountKind` 和无具体标识的 `StatusFlag` 键只保留定义和分类，用于
枚举身份稳定；生产代码中已无写入，也没有任何读取路径（当前护盾读 `ShieldPoolComponent`，
冷却读 `CooldownComponent`，呼吸读 `BreathComponent`，坐骑身份读 `MountStateComponent`）。
删除这些枚举成员会把其后的键整体前移，改变已有序列化身份，因此本轮保留并在
[执行计划](../../../../docs/plans/2026-11-20-attribute-system-execution-plan.md) 记为技术债。

```csharp
AttributeContributionSet set = new(
    new AttributeSourceDescriptor(sourceId: 7001, AttributeSourceCategory.Buff),
    [new AttributeContribution(AttributeKind.Defense, AttributeModifierOperation.Additive, 5f)]);

AttributeContributionSystem.Apply(attributes, set);
```

`Apply` 会先按 `SourceId` 撤销旧贡献再写入，因此同一来源重复登记是替换而非叠加。

## 跨域请求边界（B8）

外部来源不直接写 `AttributeSetComponent`。Buff 应用走 `BuffApplyRequestComponent`，由
`BuffApplyRequestSystem` 在 Attributes 域内完成准入、叠层、刷新和生命周期登记；周期行为
再由 `StatusEffectPeriodicSystem` 生成本域请求。

护盾伤害结算保留一个受控同步例外：Combat 必须在同一提交调用中知道护盾是否完整吸收，
因此通过 `AttributeShieldConsumeRequestComponent` 调用 Attributes 的窄接口并立即读取结果；
该接口只写 `ShieldPoolComponent`，不再写属性基础值。

Inventory 的装备属性桥接在本轮**明确延期**：`InventoryAttributeContributionSystem` 保留为
历史文件，不注册、不作为验收入口，也不存在 `AttributeContributionRequestComponent` /
`AttributeContributionApplySystem`。Inventory 只验证槽位、堆叠、物品关系和输入写入。

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
| `SetBonus` | 护甲套装效果的贡献 |
| `Prefix` | 物品前缀（修饰词）的贡献 |
| `Imbue` | 武器附魔（药剂瓶）的贡献 |
| `Pet` | 召唤物、宠物或光照宠的贡献 |
| `Mount` | 坐骑的贡献 |
| `Environment` | 世界环境来源，例如怪物旗帜与幸运值 |
| `StatusFlag` | 布尔状态标记镜像与潜行 |
| `Cooldown` | 冷却与恢复窗口 |

Boss 和 Enemy 是否同时适用由来源创建者决定；这个枚举不会偷偷推断客户端的实体分类。
`Contribution/` 子域的十一类形态通过
`AttributeContributionSystem.ToSourceCategory` 映射到上表后六项。

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
`CriticalRollInput`；元素抗性使用各元素抗性键；护盾容量使用 `MaxShield`，当前池使用
`ShieldPoolComponent`；反射和吸血使用 `ReflectRatio` 与 `LifeStealRatio`。

`MaxShield` 是计算结果，`ShieldPoolComponent.Current` 是唯一的当前护盾权威值：
`MaxShield` 变化只由 `LifeAttributeSynchronizationSystem` 收缩当前池，绝不反向写容量。
`AttributeShieldCommands.Apply` 是护盾池的唯一外部消费入口，`ShieldPoolComponent.Consume`
是唯一扣减实现；属性基础值不再保存当前护盾。结算顺序为：攻击属性和暴击 → 防御 →
元素抗性 → 最终减伤 → 护盾吸收 → 实际生命伤害。`True` 伤害跳过防御、抗性和最终减伤，
但当前仍经过护盾。

护盾完全吸收时，Combat 回执的 `HasEffect` 为真而 `HasDamage` 为假；这允许 Projectile 等来源
消费一次有效命中，同时不会把护盾消耗误报为生命伤害。

Projectile 命中边界只从属性取免疫窗口：`ProjectileHitImmunityTicks` 提供每目标免疫窗口的
Tick 数。穿透上限不由属性承载——它来自内容族穿透 section 的上限，生成时快照进穿透预算组件，
`Remaining = -1` 表示无限；`ProjectilePenetrationLimit` 这个键保留，但已经没有写入者与运行期
读者。属性只提供数值结果，不保存目标关系、命中计数、免疫到期记录或终止状态；这些运行态分别由
Projectile 免疫/穿透组件和生命周期系统维护。

## 生命周期边界

`AttributeSystem.Advance` 每推进一个 tick 就减少限时修饰器的剩余时间，并移除到期项。
`RemoveSource` 和 `RemoveSourceCategory` 用于来源失效时清理列表。组件、计算器和系统不执行
网络、文件、UI、日志、伤害、治疗、召唤物生成或周期事件。

同一个期限只能有一个推进点。`BuffListComponent.AdvanceOneTick` 是 Buff 期限的唯一推进点，
因此 `BuffSystem` 写入的修饰器一律为永久修饰器，过期时按 `SourceId` 整体撤销。这消除的是
重复状态：实测两个推进点同时启用时二者同步到期，但只要只启用其中一个，两份期限就会分叉。

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

## 状态效果生命周期与行为组件（B6 / B7）

`BuffDefinition` 同时声明四类内容：`Modifiers`（属性贡献）、`Behaviors`（行为贡献）、
`PeriodicEffects`（周期效果）与即时效果（尚未建模）。`BuffListComponent` 仍是效果实例、
来源、期限、层数和免疫的唯一 owner，`BuffListComponent.AdvanceOneTick` 是期限的唯一推进点。

`StatusEffectBehaviorSystem` 把行为贡献物化为具体行为组件，并按来源同步增删：

| 行为 | 组件 | 消费方 |
| --- | --- | --- |
| 周期治疗 | `LifeRegenerationComponent` | `StatusEffectPeriodicSystem` → `LifeRestoreRequestQueueComponent` → `LifeRestoreRequestSystem` |
| 周期伤害 | `DamageOverTimeComponent` | `StatusEffectPeriodicSystem` → `CombatDamageRequestQueueComponent` → `CombatDamageRequestSystem` |
| 隐身 | `InvisibilityComponent` | `NpcAiSnapshotProvider` 的可见性投影（`NpcAiTargetFact.IsVisible`） |
| 眩晕 / 移动限制 | `StunComponent` | 组件已建模，移动域消费仍待接线（见技术债） |
| 伤害免疫 | `DamageImmunityStatusComponent` | `CombatDamageSystem` 免疫判定 |

行为组件不承载期限：来源撤销或过期时，`StatusEffectBehaviorSystem` 在同一 tick 把来源
从组件中移除，空组件随即删除，因此不会留下孤儿行为状态。周期行为只产生目标域请求，
从不直接写 `LifeComponent`、移动或网络状态。Regeneration、Poison、Invisibility 三条代表性
闭环由 `Test/Terraria.Attributes.Verification` 用真实系统与真实 tick 顺序验证。

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

