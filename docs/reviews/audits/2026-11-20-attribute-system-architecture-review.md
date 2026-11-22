# 属性系统与 Buff / 状态效果架构审查报告

**审查日期**：2026-11-20  
**审查范围**：`src/NSSLC/Components/Share/Attributes/**`、相关 Combat / Inventory / Projectile / Entity 装配代码、服务器系统注册，以及参考项目中的属性、生命和状态效果实现  
**审查目的**：验证当前项目是否符合以下设计设想：

> 属性系统负责可计算的数值；角色生命和其他核心资源保留为独立权威状态；其他类似 Buff 的效果以 ECS 组件承载其生命周期、行为和副作用。

**结论级别**：中高程度偏离，建议进入架构整改队列。当前实现可以作为迁移基础，不建议继续向 `AttributeKind` 添加资源、冷却、身份或行为状态。

## 1. 审查结论

当前项目已经完成了“数值型 Buff 进入属性修饰器”的第一阶段，但没有完成完整的状态效果分层。

目前实际形成的链路是：

```text
装备 / 数值型 Buff
    -> AttributeModifier
    -> AttributeSetComponent
    -> AttributeCalculator
    -> Combat / Movement 读取
```

目标设计需要形成的链路应为：

```text
装备 / Buff / 环境 / 坐骑
    -> 属性贡献请求
    -> AttributeSetComponent（只保存可计算数值）

Buff / StatusEffect
    -> 生命周期组件
    -> 数值贡献 + 行为组件
    -> Combat / Life / Movement 请求
```

因此结论不是“属性系统方向错误”，而是**职责边界尚未收敛**：

| 检查项 | 判定 | 说明 |
| --- | --- | --- |
| 当前生命独立于属性系统 | **confirmed / 符合** | `LifeComponent` 是生命状态容器，Combat 负责提交变化 |
| 防御、伤害、速度、抗性、暴击等数值进入属性系统 | **confirmed / 基本符合** | 已有 `AttributeModifier`、计算器和战斗消费者 |
| Buff 的 duration、source、stack、免疫独立保存 | **confirmed / 部分符合** | `ActiveBuff` / `BuffListComponent` 已实现 |
| 非数值效果由专用组件承载 | **high deviation / 未完成** | 当前没有 DoT、HoT、眩晕、隐身等完整行为模型 |
| 资源、冷却、身份与属性分离 | **high deviation / 不符合** | `AttributeKind` 仍混入 Shield、Cooldown、Mount、StatusFlag 等 |
| 所有可战斗实体统一具备属性能力 | **partial / 不完整** | 玩家和投射物有属性组件，NPC 定义没有 |
| 跨域写入遵循请求组件边界 | **confirmed / 存在偏离** | Inventory、Buff、Combat 仍有直接或同步命令写入路径 |
| 属性贡献生命周期和来源身份可独立撤销 | **partial / 有风险** | Inventory 使用物品定义索引作为 SourceId |

## 2. 审查依据和判定方法

审查依据包括：

- 当前项目的 ECS 系统输入边界约束：
  [ECS系统输入边界约束.md](../../../Context/约束/ECS系统输入边界约束.md)
- 当前属性系统说明：
  [Attributes README](../../../src/NSSLC/Components/Share/Attributes/README.md)
- 当前 Buff 与属性实现：
  `AttributeSetComponent`、`AttributeCalculator`、`BuffSystem`、`BuffListComponent`、`InventoryAttributeContributionSystem`
- 当前实体装配和调度：
  `PlayerEntityDefinition`、`NpcEntityDefinition`、`Program.BuildRegistrations`
- 当前战斗读取和提交：
  `CombatDamageSystem`、`DamageMitigationSystem`、`DamageCommitSystem`
- 参考项目：
  - Valence：`examples/potions.rs`、`crates/valence_entity/src/active_status_effects.rs`
  - Space Station 14：`Content.Shared/StatusEffectNew/**`、`Content.Shared/Armor/**`、`Content.Shared/Damage/**`
  - Veloren：`common/src/comp/health.rs`、`common/src/comp/stats.rs`、`common/src/comp/buff.rs`

判定采用以下证据等级：

- **confirmed**：源码中存在明确组件、系统、调用路径或装配证据。
- **partial**：机制存在，但只覆盖部分实体、部分效果或存在未闭合路径。
- **unknown**：静态代码中无法确认运行时是否接线，不能据此宣称已完成。
- **proposed**：本报告提出的目标设计或迁移建议，不代表当前代码已经实现。

本报告是静态架构审查。本轮没有修改属性系统源码，也没有执行构建、测试或运行时联调。

## 3. 当前实现中已经成立的设计

### 3.1 生命是独立的权威状态

`LifeComponent` 保存 `Current`、`Maximum`、`Minimum`，并通过 `WithCurrent` 产生新的有界生命值。它没有被建模为普通 `AttributeKind`，这是正确的职责划分。

证据：

- [LifeComponent.cs](../../../src/NSSLC/Components/Share/Combat/LifeComponent.cs:3)
- [DamageCommitSystem.cs](../../../src/NSSLC/Components/Share/Combat/DamageCommitSystem.cs:75)

`CombatDamageSystem` 读取目标生命和属性，`DamageCommitSystem` 负责最终生命写入，基本符合“计算与提交分离”的方向。

### 3.2 数值型属性已经有统一聚合模型

`AttributeSetComponent` 保存基础值和活动修饰器。`AttributeCalculator.Resolve` 使用固定计算顺序：

```text
基础值
  -> Additive
  -> Multiplicative
  -> 最高优先级 Override
```

证据：

- [AttributeSetComponent.cs](../../../src/NSSLC/Components/Share/Attributes/AttributeSetComponent.cs:8)
- [AttributeCalculator.cs](../../../src/NSSLC/Components/Share/Attributes/AttributeCalculator.cs:5)

当前适合留在属性系统中的键包括：

```text
Defense
Damage
MeleeDamage
RangedDamage
MagicDamage
SummonDamage
AttackSpeed
MovementSpeed
CriticalChance
CriticalMultiplier
ArmorPenetration
ElementalPenetration
FireResistance / IceResistance / PoisonResistance / ...
DamageReduction
Endurance
Knockback
LifeStealRatio
ReflectRatio
Luck
Stealth
Aggro
MaxShield
```

这些键的共同点是：它们是供战斗、移动、AI 或其他规则读取的计算参数，而不是某个资源的当前剩余量，也不是具有独立生命周期的效果实例。

### 3.3 Buff 生命周期已经开始与属性修饰分离

`ActiveBuff` 保存：

- Effect
- ImmunityKey
- SourceId
- RemainingTicks
- Stacks

`BuffSystem` 负责准入、刷新、叠层、属性贡献添加和过期撤销；`BuffTickSystem` 负责推进 `BuffListComponent`。当前 Buff 生成的属性修饰器使用永久期限，由 Buff 过期时按来源整体删除，避免同一 duration 被两个系统重复推进。

证据：

- [ActiveBuff.cs](../../../src/NSSLC/Components/Share/Attributes/Buff/ActiveBuff.cs:5)
- [BuffListComponent.cs](../../../src/NSSLC/Components/Share/Attributes/Buff/BuffListComponent.cs:11)
- [BuffSystem.cs](../../../src/NSSLC/Components/Share/Attributes/Buff/BuffSystem.cs:22)
- [BuffTickSystem.cs](../../../src/NSSLC/Components/Share/Attributes/Buff/BuffTickSystem.cs:15)

这部分可以作为后续 `StatusEffect` 运行时容器的基础。

### 3.4 属性系统的调度顺序已经显式化

服务器注册将库存贡献、属性期限和 Buff 期限放在 Simulation 阶段，并声明了依赖顺序：

```text
PlayerInputApply
    -> InventoryAttributeContribution
    -> AttributeTick
    -> BuffTick
```

证据：

- [Program.cs](../../../src/NSSLC.Server/Program.cs:601)
- [Program.cs](../../../src/NSSLC.Server/Program.cs:726)

这解决了同 tick 换装和属性读取之间的部分时序问题，但不改变跨域写入边界问题。

## 4. 主要架构偏离

### 4.1 `AttributeKind` 混合了标量、资源、冷却、身份和状态

当前枚举同时包含以下几类语义：

| 类别 | 当前键 | 审查结论 |
| --- | --- | --- |
| 可计算标量 | Defense、Damage、MovementSpeed、Resistance、Critical 等 | **保留** |
| 可计算容量/上限 | MaxShield | **可保留，但要与当前池分离** |
| 当前资源池 | Shield | **应迁移到 `ShieldPoolComponent`** |
| 冷却剩余量 | PotionCooldownTicks、RestorationCooldownTicks、MushroomCooldownTicks | **应迁移到 `CooldownComponent`** |
| 呼吸当前容量 | BreathCapacityTicks | **应迁移到 `BreathComponent`** |
| 身份/枚举 | MountKind | **应迁移到 `MountStateComponent`** |
| 泛状态标记 | StatusFlag | **应拆为具体状态组件或状态效果实例** |
| 命中规则参数 | ProjectileHitImmunityTicks、ProjectilePenetrationLimit | **参数可保留，命中记录不能放在属性中** |

证据：

- [AttributeKind.cs](../../../src/NSSLC/Components/Share/Attributes/AttributeKind.cs:6)
- [AttributeCombatCalculator.cs](../../../src/NSSLC/Components/Share/Attributes/AttributeCombatCalculator.cs:75)

问题不在于属性系统不能提供这些系统所需的参数，而在于不能把“当前状态”伪装成普通标量。例如：

- `MaxShield` 可以是属性，`Shield.Current` 不应该是属性。
- 冷却缩减可以是属性，`PotionCooldownTicks` 不应该是属性。
- 坐骑速度加成可以是属性，`MountKind` 不应该是属性。
- 隐身强度或潜行度可以是属性，`Invisible = 1` 不应该通过通用 `StatusFlag` 表达。

### 4.2 NPC 没有加入属性能力组合

玩家定义包含 `AttributeSetComponent` 和 `BuffListComponent`：

- [PlayerEntityDefinition.cs](../../../src/NSSLC.Server/Registration/PlayerEntityDefinition.cs:41)
- [PlayerEntityDefinition.cs](../../../src/NSSLC.Server/Registration/PlayerEntityDefinition.cs:48)
- [PlayerEntityDefinition.cs](../../../src/NSSLC.Server/Registration/PlayerEntityDefinition.cs:50)

向导和史莱姆定义目前只有 `LifeComponent`，没有 `AttributeSetComponent`：

- [NpcEntityDefinition.cs](../../../src/NSSLC.Server/Registration/NpcEntityDefinition.cs:49)
- [NpcEntityDefinition.cs](../../../src/NSSLC.Server/Registration/NpcEntityDefinition.cs:94)

战斗系统因此用可选属性处理目标：

- [CombatDamageSystem.cs](../../../src/NSSLC/Components/Share/Combat/CombatDamageSystem.cs:144)

当前结果是 NPC 缺少属性组件时按 fallback 运行。这个 fallback 可以作为兼容路径，但不能作为“所有战斗实体统一使用属性系统”的完成证明。

建议所有需要接收或提供属性修正的战斗实体都显式组合 `AttributeSetComponent`。如果某种 NPC 确实不支持属性，应在实体定义中明确记录该能力缺失，而不是让 Combat 通过 null 隐式推断。

### 4.3 Buff 定义只覆盖数值贡献，行为效果闭环缺失

`BuffDefinition` 当前只有 `AttributeContribution` 列表：

- [BuffDefinition.cs](../../../src/NSSLC/Components/Share/Attributes/Buff/BuffDefinition.cs:7)

当前注册的内容只有：

- Ironskin：Defense +8
- Swiftness：MovementSpeed +0.25
- Regeneration：LifeRegeneration +4

证据：

- [BuffContent.cs](../../../src/NSSLC/Components/Share/Attributes/Buff/BuffContent.cs:25)

扫描当前 `src/NSSLC` 未发现 `LifeRegeneration` 被周期系统消费并转成 `LifeRestoreRequestComponent` 的完整路径。因此 `Regeneration` 当前可以计算出属性值，但不能据此确认实际回血已经闭环。该项应标记为 **partial**，而不是完成。

以下效果不能简单压成一个浮点属性：

```text
DamageOverTime
InstantDamage
InstantHealth
PeriodicHeal
Stun
Root / MovementRestriction
Silence
Invisibility
Invulnerability
DamageImmunity
KnockbackOnTick
SummonMaintenance
```

它们需要来源、持续时间、叠层、周期、应用/移除副作用、免疫和请求提交等语义。属性系统可以提供其中的数值参数，但不能取代效果实例和行为系统。

### 4.4 Inventory 跨域直接写入属性组件

`InventoryAttributeContributionSystem` 直接调用：

```csharp
attributes.RemoveBySourceCategory(...);
attributes.AddModifier(...);
```

证据：

- [InventoryAttributeContributionSystem.cs](../../../src/NSSLC/Components/Share/Inventory/InventoryAttributeContributionSystem.cs:105)
- [InventoryAttributeContributionSystem.cs](../../../src/NSSLC/Components/Share/Inventory/InventoryAttributeContributionSystem.cs:123)

根据当前边界约束，Inventory 应产生 Attributes 域的请求组件，由 Attributes 域系统更新 `AttributeSetComponent`。当前实现是功能上可工作的过渡方案，但架构上属于已确认的跨域写入偏离。

建议迁移为：

```text
InventoryAttributeContributionSystem
    -> AttributeContributionRequestComponent
    -> AttributeContributionApplySystem（Attributes 域）
    -> AttributeSetComponent
```

### 4.5 Buff 应用 API 仍允许调用者直接改变属性状态

`BuffSystem.TryApply` 接收可变 `BuffListComponent` 和可选 `AttributeSetComponent`，并在同一个调用中直接更新二者：

- [BuffSystem.cs](../../../src/NSSLC/Components/Share/Attributes/Buff/BuffSystem.cs:22)

当前扫描没有发现明确的服务器运行时调用点，因此“已激活违规”尚不能确认；但 API 形状本身允许外部域绕过请求和调度边界，应标记为 **confirmed API risk / active path unknown**。

建议保留同域内部规则实现，把外部入口改为：

```text
BuffApplyRequestComponent
    -> StatusEffectAdmissionSystem
    -> StatusEffectApplySystem
    -> Buff / StatusEffect 状态与属性贡献
```

### 4.6 Combat 对 Shield 的同步修改仍然越过属性状态 owner

Combat 在提交伤害时调用 `AttributeShieldCommands.Apply`，最终通过 `AttributeSystem.SetBaseValue` 改写 `AttributeSetComponent.Shield`：

- [DamageCommitSystem.cs](../../../src/NSSLC/Components/Share/Combat/DamageCommitSystem.cs:75)
- [AttributeSystem.cs](../../../src/NSSLC/Components/Share/Attributes/AttributeSystem.cs:23)

即使它被包装成同步 command，仍然存在两个问题：

1. 当前属性组件承担了资源池当前值；
2. Combat 直接触发 Attributes 状态写入。

目标形态应为：

```text
Combat
    -> ShieldConsumeRequestComponent
    -> ShieldConsumeSystem（Shield / Attributes owner）
    -> ShieldConsumeResult
```

如果为了伤害结算的原子性保留同步命令，应将其记录为受控例外，并限制为 Attributes 域公开的窄接口；不能继续扩展为通用跨域写入模式。

### 4.7 Inventory SourceId 不能唯一标识物品来源

当前实现使用：

```csharp
long sourceId = slot.Definition.Index + 1L;
```

证据：

- [InventoryAttributeContributionSystem.cs](../../../src/NSSLC/Components/Share/Inventory/InventoryAttributeContributionSystem.cs:105)

这不是物品实例身份。同一实体的两个槽位可以放置相同定义，从而得到相同 `SourceId`。当前系统每 tick 按类别重建，因此短期内不一定产生错误；但它会破坏：

- 单槽位移除；
- 来源追踪和诊断；
- 增量更新；
- 物品实例前缀和附魔；
- 网络或 UI 的来源显示。

建议引入统一的 `AttributeSourceIdentity`，至少包含：

```text
OwnerEntityId
SlotId
ItemInstanceId
Generation
ContributionCategory
```

## 5. 参考项目对照

### 5.1 Valence

Valence 将属性修正、生命、护盾和表现/旗标分开：

- `EntityAttributes` 保存属性修正，见 `examples/potions.rs:158`。
- 最大生命属性变化后单独同步生命上限，见 `examples/potions.rs:170`。
- `Health` 独立保存生命，见 `examples/potions.rs:11` 及实体模块。
- `Absorption` 独立表示吸收护盾，见 `examples/potions.rs:11`、`:218`。
- 即时治疗和即时伤害直接走生命效果，见 `examples/potions.rs:226`。
- 隐身和发光通过 `Flags` 处理，见 `examples/potions.rs:236`。
- `ActiveStatusEffects` 独立保存效果实例和期限，见 `crates/valence_entity/src/active_status_effects.rs:28`。

这与本项目应采用的边界直接对应：数值贡献进属性，生命/护盾/旗标/周期行为由专用组件和系统处理。

### 5.2 Space Station 14

SS14 没有把所有 Buff-like 状态压成一个通用属性字典：

- `StatusEffectContainerComponent` 保存当前状态效果容器，见 `Content.Shared/StatusEffectNew/Components/StatusEffectContainerComponent.cs:6`。
- `StatusEffectsSystem` 管理效果应用、延迟开始和结束，见 `Content.Shared/StatusEffectNew/StatusEffectsSystem.cs:11`。
- `ArmorComponent` 独立提供装备伤害修正，见 `Content.Shared/Armor/ArmorComponent.cs:8`。
- `DamageModifierStatusEffectComponent` 只负责状态对伤害的修正，见 `Content.Shared/Damage/Systems/DamageModifierStatusEffect.cs:6`。
- 眩晕、击倒、静音、失明等使用具体组件或系统，而不是通用 `StatusFlag`。

### 5.3 Veloren

Veloren 明确分离：

- `Health`：当前生命、基础最大生命和修正后的最大生命，见 `common/src/comp/health.rs:37`。
- `Stats`：伤害、移动速度、减伤、最大生命修正和攻击/受击效果参数，见 `common/src/comp/stats.rs:76`。
- `Buffs`：来源、分类、持续时间和效果，见 `common/src/comp/buff.rs:33`。

Veloren 的 Buff 类型同时包含再生、燃烧、冻结、无敌、最大生命修正等内容，但这些内容由 Buff 系统解释，并不意味着所有效果都变成属性标量。

## 6. 目标架构

建议采用四层模型。

### 6.1 核心状态组件

```text
LifeComponent
ShieldPoolComponent
ManaComponent
BreathComponent
CooldownComponent
MountStateComponent
```

这些组件保存当前值、容量、计时器、实体关系或身份状态。

### 6.2 数值属性组件

`AttributeSetComponent` 只保存稳定基础值和可计算修正，例如：

```text
Defense
Damage
MovementSpeed
AttackSpeed
CriticalChance
CriticalMultiplier
Resistance
ArmorPenetration
Knockback
LifeRegenerationRate
LifeStealRatio
ReflectRatio
MaxShield
MaximumLife
```

`AttributeCalculator` 只计算结果，不直接产生伤害、治疗或行为副作用。

### 6.3 有生命周期的状态效果

建议将当前 `BuffListComponent` 逐步泛化为 `StatusEffectListComponent`，保留兼容的 `BuffId` / `BuffDefinition` 内容定义。

```text
StatusEffectListComponent
ActiveStatusEffect
    EffectId
    SourceId
    RemainingTicks
    Stacks
    ImmunityKey
    RefreshPolicy
    StackPolicy
```

Buff 可以是状态效果的一种内容类别，但不应成为所有运行时状态的唯一抽象名称。

### 6.4 行为组件

```text
DamageOverTimeComponent
LifeRegenerationComponent
StunComponent
MovementRestrictionComponent
SilenceComponent
InvisibilityComponent
DamageImmunityComponent
StatusFlagComponent（带具体 FlagId）
```

行为组件保存状态，系统负责将其转化为 Combat、Life、Movement 或 Network 请求。

## 7. 关键不变量

后续实现应固定以下不变量：

1. `LifeComponent.Current` 只有 Life / Combat 提交系统可以修改。
2. `LifeComponent.Maximum` 不能与属性计算结果双写。
3. `AttributeSetComponent` 不保存当前生命、当前护盾、冷却剩余量、坐骑身份或通用布尔状态。
4. `AttributeCalculator` 只能返回派生数值，不执行伤害、治疗、网络或实体生成。
5. Buff / StatusEffect 的期限只有一个推进 owner。
6. 周期回血必须转为 `LifeRestoreRequestComponent`，不能让属性计算器直接改生命。
7. 周期伤害必须转为 Combat damage request，不能用负的 `LifeRegeneration` 伪装所有 DoT。
8. 跨域属性写入通过目标域请求组件或明确记录的同步窄命令完成。
9. 同一实体不同来源必须有不同的 `SourceId`。
10. 需要属性能力的 NPC、玩家和投射物必须通过组件组合显式声明，而不是依赖 null fallback。

## 8. 建议迁移顺序

### 阶段一：锁定属性分类

给现有 `AttributeKind` 增加内部分类或元数据，至少区分：

```text
ComputedScalar
ResourcePool
Cooldown
Identity
StatusFlag
Policy
```

此阶段先不删除枚举值，但禁止新增错误类别。

### 阶段二：补齐 NPC 属性组合

为参与 Combat、可接收 Buff 或可提供攻击属性的 NPC 增加 `AttributeSetComponent`。为每种 NPC 设置明确基础值；若某实体不支持属性，则在定义和文档中显式声明。

### 阶段三：迁移资源和身份状态

按以下顺序拆分：

1. `Shield` -> `ShieldPoolComponent`。
2. Potion / Restoration / Mushroom cooldown -> `CooldownComponent`。
3. `BreathCapacityTicks` -> `BreathComponent`。
4. `MountKind` -> `MountStateComponent`。
5. `StatusFlag` -> 具体行为组件或带 `FlagId` 的状态组件。

`MaxShield`、冷却缩减、呼吸最大容量等计算参数可以继续留在属性系统。

### 阶段四：扩展状态效果定义

将 `BuffDefinition` 从单一 `AttributeContribution[]` 扩展为：

```text
AttributeContribution[]
BehaviorContribution[]
PeriodicEffectDefinition[]
ImmediateEffectDefinition[]
```

优先闭合以下三个示例：

```text
Regeneration
    LifeRegenerationRate
    -> LifeRestoreRequestComponent

Poison
    DamageOverTimeComponent
    -> CombatDamageRequestComponent

Invisibility
    InvisibilityComponent
    -> Network / Visibility Projection
```

### 阶段五：收敛跨域输入

将以下路径改为请求组件：

```text
Inventory -> AttributeContributionRequestComponent
Combat -> ShieldConsumeRequestComponent
外部 Buff 来源 -> BuffApplyRequestComponent
```

由 Attributes / StatusEffect 所属系统在固定阶段消费请求并修改权威组件。

### 阶段六：修正来源身份和测试

建立统一来源身份生成器，覆盖物品实例、槽位、Buff、坐骑和环境来源。随后补充以下验证：

- 相同物品定义放入两个槽位时可独立撤销。
- Buff 刷新和叠层不会重复累加修饰器。
- Buff 到期只撤销自身来源。
- NPC 和玩家使用相同属性查询得到一致结果。
- 最大生命下降时 Current 被正确 clamp。
- Shield 当前池消耗不影响 MaxShield 计算。
- Regeneration 产生生命恢复请求并由 Life 系统提交。
- DoT 产生 Combat 请求并保留来源与伤害类型。
- 隐身、眩晕、免疫到期后只移除自身行为组件。

## 9. 风险和未确认项

### 已确认

- `AttributeKind` 混合了多类语义。
- NPC 定义没有 `AttributeSetComponent`。
- Buff 定义只有属性贡献，未覆盖完整行为效果。
- Inventory 直接写入属性组件。
- Shield 当前池通过属性基础值保存。
- Inventory SourceId 使用物品定义索引。

### 部分确认

- `Regeneration` 已生成属性修饰器，但当前源码未形成实际生命恢复消费闭环。
- Buff 生命周期逻辑已实现，但当前服务器是否有完整外部应用入口仍需单独运行时确认。
- Combat 读取属性的路径已经存在，但 NPC 缺失属性组件导致实体覆盖不完整。

### 尚未确认

- 生产环境是否存在未被静态扫描发现的 `BuffSystem.TryApply` 调用。
- 所有 NPC 类型是否都由当前 `NpcEntityDefinition` 装配；本报告只确认了向导和史莱姆定义。
- 参考客户端所有装备、套装、坐骑和环境效果是否已经接入当前 Inventory / Contribution 管线。
- 当前 `LifeRestoreRequestSystem` 是否已被所有再生来源正确使用。

这些项目应在实施阶段通过构建、验证程序和运行时日志补齐证据，不能仅凭类名或注册表存在宣称完成。

## 10. 审查结论

当前项目的正确发展方向应明确为：

> **属性系统负责可计算的数值；ECS 组件负责资源、身份、生命周期和行为。**

当前实现已经完成：

```text
装备 / 数值型 Buff
    -> AttributeModifier
    -> AttributeSetComponent
```

当前尚未完成：

```text
Buff / StatusEffect
    -> 生命周期组件
    -> 数值贡献
    -> 行为组件
    -> Combat / Life / Movement 请求
```

因此建议将本报告作为后续属性系统重构的基线：保留现有 `AttributeSetComponent`、`AttributeCalculator` 和 `BuffListComponent` 的可复用部分，优先拆出当前资源与行为状态，再收敛跨域写入边界，最后扩展状态效果内容模型。不要通过继续增加 `AttributeKind` 枚举项来覆盖新的 Buff-like 语义。
## 11. 收口结论（2026-11-22）

本报告第 7 节的十条不变量已逐条落地，执行计划 B0–B9 的适用验收项闭合（B3 的 Inventory 属性贡献
与装备来源身份按用户要求保持 deferred）。逐条对照：

| 不变量 | 落地方式 | 证据 |
| --- | --- | --- |
| 1 生命当前值只有 Life / Combat 提交写 | `LifeRestoreRequestSystem`、`DamageCommitSystem` 是唯一写入方 | Attributes 验证：再生经请求把 50 提到 54 |
| 2 生命上限不与属性双写 | `LifeAttributeSynchronizationSystem` 单点夹取 | 上限 20→10 时当前值夹到 10 |
| 3 属性集不保存当前值 | `AttributeSetComponent` 只保留 base 与 modifiers | 分类 5 项与准入 3 项断言 |
| 4 计算器只返回派生值 | `AttributeCalculator` 无副作用 | 同上 |
| 5 Buff 期限只有一个推进 owner | `BuffListComponent.AdvanceOneTick` | 过期后行为组件与修饰器同 tick 撤销 |
| 6 周期回血走请求组件 | `StatusEffectPeriodicSystem` → `LifeRestoreRequestComponent` | 队列清空且生命 +4 |
| 7 周期伤害走 Combat 请求 | `StatusEffectPeriodicSystem` → `CombatDamageRequestComponent` | 净 −4，结果带 DamageOverTime 与来源 |
| 8 跨域写入走目标域请求 | Buff / Combat / Life 三域均走请求组件；护盾走 `AttributeShieldCommands.Apply` | 静态扫描：`BuffSystem.TryApply` 唯一调用方是 `BuffApplyRequestSystem` |
| 9 来源身份唯一 | `AttributeSourceDescriptor` + 按 `SourceId` 撤销 | 刷新替换单一来源、双来源独立撤销 |
| 10 能力靠组合显式声明 | `CombatDamageSystem` 缺 `AttributeSetComponent` 即返回 `MissingAttributeCapability` | Combat.ProtectionWindow 与 Attributes 验证 |

第 3 节「Invisibility / Stun 只写不读」的现状已部分改善：`InvisibilityComponent` 现由
`NpcAiSnapshotProvider` 读入 `NpcAiTargetFact.IsVisible`，从而影响 NPC 索敌与攻击；`StunComponent`
仍缺移动消费者，作为技术债记录在计划与任务表。

`AttributeKind` 未新增枚举项以承载 Buff 语义：状态效果通过 `BuffDefinition` 的数值贡献、行为贡献、
周期效果三类内容表达，未把布尔状态或时长塞回属性集。
