# Version4 战斗、状态与归因系统实际代码组件草案

## 1. 文档元数据

| 项目 | 值 |
|---|---|
| subsystemId | CombatAndStatus |
| taskNumber | 16 |
| sourceDesign | D:\TRbackup\NLTX\docs\Version4战斗状态与归因系统组件设计.md |
| requestedSource | D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-combat-and-status-component-design.md |
| requestedSourceStatus | missing |
| outputPath | D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-06-version4-combat-and-status-component-code-draft.md |
| draftKind | actual-csharp-component-skeleton |
| draftScope | component-code-shape |
| componentCount | 11 |
| supportingValueTypeCount | 10 |
| designStatus | candidate-with-open-decisions |
| implementationStatus | component-skeleton-created |
| verificationStatus | not-run |
| compileStatus | passed-for-affected-projects |
| testStatus | not-written |
| testRunStatus | not-run |
| subagentUsed | false |

本文件把现存的 Component-only Design 转换为接近实际 C# 文件的代码组件草案。本轮已将对应的组件骨架写入 `src/`，但代码块和源码都只表达字段/值对象边界，不表示可以直接替换当前实现或已经完成运行时迁移。

用户指定的设计路径在本次读取时不存在，因此本文件以实际存在的同主题设计文件作为输入。该替代输入仍然是字段设计，不是已批准的运行时实现契约。

## 2. 草案边界

本草案覆盖设计文档中的 11 个顶层组件：

- Combat 实体组件：Health、再生、Defense、DamageReduction、DamageAcceptancePolicy、Immunity、KnockbackPolicy、StatusEffectSlots、StatusEffectImmunity；
- Entity 域通用组件：EntityProvenance；
- Combat 域遭遇根组件：EncounterDamageCredit。

本文件只描述：

- C# 组件、内嵌值对象和值类型；
- 字段、构造入口和只读派生属性；
- 私有集合的只读暴露形状；
- 候选文件路径、命名空间、当前 NLTX 映射和未决点。

本文件不定义或实现：

- System、Query、Command、Event、Adapter、Projection；
- 伤害、治疗、死亡、Buff、免疫、击退或贡献的状态转换；
- 时钟、随机数、日志、网络、存档、UI、音效或 I/O；
- 主循环接线、系统调度顺序、事务提交和重试；
- 当前 State 类型的删除、重命名、迁移或兼容双写；
- 测试项目、verifier 或完整运行时迁移；
- Version4 行为等价、API 兼容或迁移完成声明。

草案中的 TODO、unresolved 和 crossSubsystemOwner: integration-review 都是实现前决策点，不能通过补一个看起来合理的类型或构造函数来隐式裁决。

## 3. 代码形状决策

### 3.1 采用方案

| 方案 | 代码形状 | 取舍 | 本草案 |
|---|---|---|---|
| A | 只展示公共字段的最小 struct | 最小，但数组默认值、只读视图和文件边界不明显 | 否 |
| B | 每个组件对应一个同名文件，提供最小构造入口、字段、派生属性和值对象 | 可直接进入源码评审，仍不把运行时行为塞进组件 | 是 |
| C | 组件加完整校验、序列化、迁移和写入 API | 信息完整，但会过早锁定跨子系统契约 | 否 |

本草案选择方案 B。构造函数只承担初始字段复制和集合初始化，不读取或写入时钟、随机源、文件、网络、数据库，不创建或销毁实体，不发布消息，也不修改其他组件。

### 3.2 struct 与集合字段

设计文档以 struct 表达这些组件。本草案保留该形状，但必须显式注意：

- 包含数组的 struct 具有值复制语义；
- 数组元素仍然可变，即使通过 ReadOnlyMemory 只读暴露；
- default(struct) 的数组字段可能为 null；
- 后续实现必须决定这些组件是否最终保持 struct，或改为 sealed class/不可变状态对象；
- 在该决定完成前，不能把 ref、集合修改或组件复制行为写入运行时系统。

代码使用 null-safe 的只读属性形状，是为了让默认值可观察，不是为了定义最终初始化策略。

### 3.3 命名空间与文件规则

- Combat 组件和值对象使用 namespace Terraria.Combat；
- EntityProvenanceComponent 和其枚举使用 namespace EntityEcs.Components；
- EntityReference 复用当前 src/Relationships/EntityReference.cs；
- 不重新声明 EntityReference、EntityId、NetworkEntityId、Persistent ID 或内容目录类型；
- 不创建 generic Components/ 或 Shared/Components/ 目录；
- EntityProvenance 放在现有 Entity 能力域的 src/Share/Entity/Components/；
- EncounterDamageCredit 虽附着于遭遇根实体，仍归 Combat 能力域，不放入 NPC 槽位或 Entity 通用身份组件；
- 不使用文件顺序决定运行时执行顺序。

## 4. 依赖和值类型清单

### 4.1 复用类型

| 类型 | 当前来源 | 用途 | 状态 |
|---|---|---|---|
| EntityReference | src/Relationships/EntityReference.cs | 来源、攻击者、受击者和命中键的实体关系 | existing / partial |
| EntityReferenceScope | src/Relationships/EntityReferenceScope.cs | EntityReference 范围 | existing / partial |
| EntityId | src/Share/Entity/Components/EntityId.cs | Entity 域运行时身份参考 | existing / partial |
| EntityIdentityState | src/Share/Entity/Components/EntityIdentityState.cs | 身份分层参考，不在本文复制 | existing / partial |
| IReadOnlyList<T> | .NET | 构造时复制输入集合 | framework |
| ReadOnlyMemory<T> | .NET | 对外提供只读集合视图 | framework |
| Math.Max | .NET | 简单派生属性 | framework |

### 4.2 新支撑类型

以下类型是字段所需的值对象或枚举，不计入 11 个 Component 数量：

| 类型 | 建议文件 | 命名空间 | 状态 |
|---|---|---|---|
| HitImmunityWindow | src/Combat/HitImmunityWindow.cs | Terraria.Combat | proposed |
| HitImmunityKey | src/Combat/HitImmunityKey.cs | Terraria.Combat | proposed |
| HitImmunityScope | src/Combat/HitImmunityScope.cs | Terraria.Combat | proposed |
| StatusEffectSlot | src/Combat/StatusEffectSlot.cs | Terraria.Combat | proposed |
| EntityProvenanceKind | src/Share/Entity/Components/EntityProvenanceKind.cs | EntityEcs.Components | proposed |
| WorldEventSourceKind | src/Share/Entity/Components/WorldEventSourceKind.cs | EntityEcs.Components | proposed |
| DamageCreditEntry | src/Combat/DamageCreditEntry.cs | Terraria.Combat | proposed |
| CombatContributorId | src/Combat/CombatContributorId.cs | Terraria.Combat | proposed |
| CombatContributorKind | src/Combat/CombatContributorKind.cs | Terraria.Combat | proposed |
| EncounterCreditLifecycle | src/Combat/EncounterCreditLifecycle.cs | Terraria.Combat | proposed |

## 5. Component 文件布局草案

### 5.1 Combat 域

~~~text
D:\TRbackup\NLTX\src\Combat\
  HealthComponent.cs
  HealthRegenerationStateComponent.cs
  DefenseComponent.cs
  DamageReductionComponent.cs
  DamageAcceptancePolicyComponent.cs
  ImmunityComponent.cs
  HitImmunityWindow.cs
  HitImmunityKey.cs
  HitImmunityScope.cs
  KnockbackPolicyComponent.cs
  StatusEffectSlotsComponent.cs
  StatusEffectSlot.cs
  StatusEffectImmunityComponent.cs
  EncounterDamageCreditComponent.cs
  DamageCreditEntry.cs
  CombatContributorId.cs
  CombatContributorKind.cs
  EncounterCreditLifecycle.cs
~~~

### 5.2 Entity 域

~~~text
D:\TRbackup\NLTX\src\Share\Entity\Components\
  EntityProvenanceComponent.cs
  EntityProvenanceKind.cs
  WorldEventSourceKind.cs
~~~

前九个受击/状态组件、EncounterDamageCreditComponent 和所有 Combat 支撑值类型都仍需通过 Combat 项目引用关系确认。EntityProvenanceComponent 应由 EntityEcs 项目拥有；Combat 只读取它，不重新声明一份同名类型。

## 6. Component 代码草案

### 6.1 HealthComponent.cs

建议路径：src/Combat/HealthComponent.cs

~~~text
componentId: COMBAT-COMP-01
componentOwner: CombatAndStatus
attachment: damageable entity
status: proposed-evolution
currentNltx: existing-partial
evidence: Player.cs:1357-1361; NPC.cs:6341-6343
~~~

~~~csharp
using System;

namespace Terraria.Combat;

public struct HealthComponent
{
  public HealthComponent(int current, int maximum)
  {
    Current = current;
    Maximum = maximum;
  }

  // 权威：当前可被伤害、治疗和再生修改的生命值。
  public int Current;

  // 权威：当前属性重算后的有效生命上限。
  public int Maximum;

  // 派生：生命耗尽不等同于死亡生命周期。
  public bool IsDepleted => Current <= 0;

  // 派生：不保存第二份缺失生命值。
  public int Missing => Math.Max(0, Maximum - Current);
}
~~~

不放入 dead、deadTime、respawnTimer、NPC active、掉落、墓碑、重建、攻击者伤害、穿甲、暴击、投射物类别、UI、网络 DTO 或存档字节。

### 6.2 HealthRegenerationStateComponent.cs

建议路径：src/Combat/HealthRegenerationStateComponent.cs

~~~text
componentId: COMBAT-COMP-02
componentOwner: CombatAndStatus
attachment: damageable entity
status: proposed-evolution
currentNltx: existing-partial
evidence: Player.cs:1369-1373,22335; NPC.cs:6111-6115
~~~

~~~csharp
namespace Terraria.Combat;

public struct HealthRegenerationStateComponent
{
  public HealthRegenerationStateComponent(
    int rate,
    int accumulator,
    float timeSinceLastDamage = 0f,
    int? expectedLossPerSecond = null)
  {
    Rate = rate;
    Accumulator = accumulator;
    TimeSinceLastDamage = timeSinceLastDamage;
    ExpectedLossPerSecond = expectedLossPerSecond;
  }

  // 权威：每个再生步长的净生命变化；负数表示生命退化。
  public int Rate;

  // 权威：尚未结算为整数生命变化的积算量。
  public int Accumulator;

  // 权威、暂态：自上次成功伤害结算后的时间。
  public float TimeSinceLastDamage;

  // 权威、NPC 可选：预期每秒生命损失；null 表示没有该语义。
  public int? ExpectedLossPerSecond;

  // 派生：不保存第二套状态标记。
  public bool IsRegenerating => Rate > 0;

  // 派生：不保存第二套状态标记。
  public bool IsDegenerating => Rate < 0;
}
~~~

兼容要求：

- Version4 的 -1 哨兵值需要在 Adapter 中转换为 null；
- 导出旧格式时是否恢复 -1 取决于持久化/网络契约；
- TimeSinceLastDamage 的时钟单位和重置条件不在组件中实现；
- 魔力再生、manaRegenCount、魔力延迟和 statMana 不进入本组件。

### 6.3 DefenseComponent.cs

建议路径：src/Combat/DefenseComponent.cs

~~~text
componentId: COMBAT-COMP-03
componentOwner: CombatAndStatus
attachment: damageable entity
status: proposed-evolution
currentNltx: existing-partial
evidence: Player.cs:1355; NPC.cs:6325
~~~

~~~csharp
namespace Terraria.Combat;

public struct DefenseComponent
{
  public DefenseComponent(int value)
  {
    Value = value;
  }

  // 权威输入候选：当前属性重算后的整数防御值。
  public int Value;

  // 派生：是否存在非零防御输入。
  public bool HasValue => Value != 0;
}
~~~

Value 在设计中同时被描述为权威值和由本 tick 属性重算得到的值。本草案不决定它是 Combat 权威状态、PlayerGameplay 输入快照还是属性缓存。armorPenetration、攻击值和投射物伤害类别不进入此组件。

### 6.4 DamageReductionComponent.cs

建议路径：src/Combat/DamageReductionComponent.cs

~~~text
componentId: COMBAT-COMP-04
componentOwner: CombatAndStatus
attachment: damageable entity
status: proposed
currentNltx: no direct same-named component confirmed
evidence: Player.cs:767,22263-22273
~~~

~~~csharp
namespace Terraria.Combat;

public struct DamageReductionComponent
{
  public DamageReductionComponent(float endurance)
  {
    Endurance = endurance;
  }

  // 权威输入候选：0.0 表示没有百分比减伤。
  public float Endurance;

  // 派生：不缓存已经应用后的实际伤害。
  public bool HasReduction => Endurance > 0f;
}
~~~

Endurance 的合法范围、NaN/Infinity 处理和钳制策略未锁定。Solar、Beetle、队伍分摊和一次命中特例不进入长期公共组件。

### 6.5 DamageAcceptancePolicyComponent.cs

建议路径：src/Combat/DamageAcceptancePolicyComponent.cs

~~~text
componentId: COMBAT-COMP-05
componentOwner: CombatAndStatus
attachment: damageable entity
status: proposed
currentNltx: no direct same-named component confirmed
evidence: NPC.cs:6125,6143,6335,6387; Projectile.cs:11607-11634
~~~

~~~csharp
namespace Terraria.Combat;

public struct DamageAcceptancePolicyComponent
{
  public DamageAcceptancePolicyComponent(
    bool rejectAllDamage,
    bool rejectHostileDamage,
    bool rejectTrapDamage,
    bool isImmortal)
  {
    RejectAllDamage = rejectAllDamage;
    RejectHostileDamage = rejectHostileDamage;
    RejectTrapDamage = rejectTrapDamage;
    IsImmortal = isImmortal;
  }

  // 权威：拒绝普通伤害。
  public bool RejectAllDamage;

  // 权威：拒绝 hostile/NPC 敌对来源的伤害。
  public bool RejectHostileDamage;

  // 权威：拒绝陷阱来源的伤害。
  public bool RejectTrapDamage;

  // 权威：局部命中效果仍可能存在的生命保护规则候选。
  public bool IsImmortal;

  // 派生：只描述通用资格。
  public bool CanAcceptAnyDamage => !RejectAllDamage;
}
~~~

不放入 creativeGodMode、Shimmer 规避、阵营/PvP 关系、目标选择或命中冷却。IsImmortal 是否阻止生命扣除、击退、状态效果和死亡资格必须由后续行为设计裁决。

### 6.6 ImmunityComponent.cs

建议路径：src/Combat/ImmunityComponent.cs

~~~text
componentId: COMBAT-COMP-06
componentOwner: CombatAndStatus
attachment: damageable entity or hit-immunity owner
status: proposed-evolution
currentNltx: existing-partial-but-shape-conflict
evidence: Player.cs:968-976,2475,22221-22334; NPC.cs:6303; Projectile.cs:158-162,256-258
~~~

~~~csharp
using System;
using System.Collections.Generic;

namespace Terraria.Combat;

public struct ImmunityComponent
{
  public ImmunityComponent(
    IReadOnlyList<HitImmunityWindow>? windows = null,
    int revision = 0)
  {
    _windows = windows is null
      ? []
      : new List<HitImmunityWindow>(windows).ToArray();
    Revision = revision;
  }

  // 权威：按稳定 HitImmunityKey 排序的命中窗口。
  private HitImmunityWindow[]? _windows;

  // 权威：窗口增删或剩余时间变更的版本。
  public int Revision;

  // 只读视图：调用方不能替换、重排或直接写入内部数组。
  public ReadOnlyMemory<HitImmunityWindow> Windows => _windows ?? [];

  // 派生：有效集合不应保留零值窗口。
  public bool HasActiveWindow => _windows is { Length: > 0 };
}
~~~

本代码块不提供 Arm、Clear、Tick 或索引器。当前 ImmunityComponent 的单一 RemainingTicks、当前 HitCooldownComponent 的按目标字典和本文的多作用域窗口模型不是同一个生命周期，不能机械合并。

### 6.7 HitImmunityWindow.cs

建议路径：src/Combat/HitImmunityWindow.cs

~~~csharp
namespace Terraria.Combat;

public struct HitImmunityWindow
{
  public HitImmunityWindow(
    HitImmunityKey key,
    int remainingTicks)
  {
    Key = key;
    RemainingTicks = remainingTicks;
  }

  // 权威：窗口作用域和来源。
  public HitImmunityKey Key;

  // 权威、暂态：剩余模拟 Tick。
  public int RemainingTicks;

  // 派生：不保存第二份 IsActive。
  public bool IsActive => RemainingTicks > 0;
}
~~~

零值窗口的清理、重复键合并、来源实体销毁和排序由后续 owner 负责。本值对象不保存闪烁、alpha 或 immuneNoBlink。

### 6.8 HitImmunityKey.cs

建议路径：src/Combat/HitImmunityKey.cs

~~~csharp
using Terraria.Relationships;

namespace Terraria.Combat;

public readonly record struct HitImmunityKey(
  HitImmunityScope Scope,
  EntityReference? SourceEntity,
  int? SourceDefinitionId);
~~~

General 是否必须没有来源、各作用域允许的字段组合、EntityReference 代际和稳定排序比较器未定义。本 record struct 不自行添加排序逻辑或验证构造函数。

### 6.9 HitImmunityScope.cs

建议路径：src/Combat/HitImmunityScope.cs

~~~csharp
namespace Terraria.Combat;

public enum HitImmunityScope : byte
{
  General,
  Attacker,
  ProjectileInstance,
  ProjectileDefinition
}
~~~

该枚举只表达候选作用域，不表达命中资格顺序、优先级或持续时间。新增成员必须同步检查旧数组映射和 verifier。

### 6.10 KnockbackPolicyComponent.cs

建议路径：src/Combat/KnockbackPolicyComponent.cs

~~~text
componentId: COMBAT-COMP-07
componentOwner: CombatAndStatus
attachment: damageable entity
status: proposed
currentNltx: no direct same-named component confirmed
evidence: Player.cs:1383,22342-22347; NPC.cs:6359
~~~

~~~csharp
namespace Terraria.Combat;

public struct KnockbackPolicyComponent
{
  public KnockbackPolicyComponent(
    bool isImmune,
    float resistance)
  {
    IsImmune = isImmune;
    Resistance = resistance;
  }

  // 权威：目标是否完全免疫击退。
  public bool IsImmune;

  // 权威输入候选：1.0 表示完全接受，0.0 表示完全抗性。
  public float Resistance;

  // 派生：位置、速度和碰撞不属于此组件。
  public bool CanReceiveKnockback => !IsImmune && Resistance > 0f;
}
~~~

Resistance 的合法范围、负数、NaN、Infinity 和免疫与正抗性同时出现时的处理未定义。

### 6.11 StatusEffectSlotsComponent.cs

建议路径：src/Combat/StatusEffectSlotsComponent.cs

~~~text
componentId: COMBAT-COMP-08
componentOwner: CombatAndStatus
attachment: status-effect-bearing entity
status: proposed
currentNltx: existing-alternate-model-in-src/StatusEffects
evidence: Player.cs:1027-1033,3541-3731; NPC.cs:6065-6071,76385-76480
~~~

~~~csharp
using System;
using System.Collections.Generic;

namespace Terraria.Combat;

public struct StatusEffectSlotsComponent
{
  public StatusEffectSlotsComponent(
    IReadOnlyList<StatusEffectSlot>? slots = null,
    int revision = 0)
  {
    _slots = slots is null
      ? []
      : new List<StatusEffectSlot>(slots).ToArray();
    Revision = revision;
  }

  // 权威：固定容量且有序的状态效果槽位。
  private StatusEffectSlot[]? _slots;

  // 权威：应用、刷新、移除、淘汰或压缩后的版本。
  public int Revision;

  // 只读视图：调用方不能替换或重排槽位。
  public ReadOnlyMemory<StatusEffectSlot> Slots => _slots ?? [];

  // 派生：容量来自唯一内部数组。
  public int Capacity => _slots?.Length ?? 0;
}
~~~

Player 的 44 槽和 NPC 的 20 槽必须由实体初始化边界传入。本组件不提供 Add、Remove、Refresh、Expire 或 Compact 方法，也不保存 Buff 定义、互斥组、时长上限、难度倍率或网络权限。

### 6.12 StatusEffectSlot.cs

建议路径：src/Combat/StatusEffectSlot.cs

~~~csharp
namespace Terraria.Combat;

public struct StatusEffectSlot
{
  public StatusEffectSlot(
    int definitionId,
    int remainingTicks)
  {
    DefinitionId = definitionId;
    RemainingTicks = remainingTicks;
  }

  // 权威：内容目录中的状态效果定义 ID；0 表示空槽。
  public int DefinitionId;

  // 权威、暂态：剩余模拟 Tick。
  public int RemainingTicks;

  // 派生：不额外保存有效位图。
  public bool IsOccupied => DefinitionId != 0 && RemainingTicks > 0;
}
~~~

DefinitionId 不是状态效果实体实例 ID。本值对象不保存来源、层数或应用时间；来源需要由单次应用请求或快照冻结。

### 6.13 StatusEffectImmunityComponent.cs

建议路径：src/Combat/StatusEffectImmunityComponent.cs

~~~text
componentId: COMBAT-COMP-09
componentOwner: CombatAndStatus
attachment: status-effect-bearing entity
status: proposed
currentNltx: no direct same-named component confirmed
evidence: Player.cs:1033; NPC.cs:6071
~~~

~~~csharp
using System;
using System.Collections.Generic;

namespace Terraria.Combat;

public struct StatusEffectImmunityComponent
{
  public StatusEffectImmunityComponent(
    IReadOnlyList<bool>? immuneByDefinitionId = null,
    int revision = 0)
  {
    _immuneByDefinitionId = immuneByDefinitionId is null
      ? []
      : new List<bool>(immuneByDefinitionId).ToArray();
    Revision = revision;
  }

  // 权威：按状态效果定义 ID 索引的免疫表。
  private bool[]? _immuneByDefinitionId;

  // 权威：免疫集合重建后的版本。
  public int Revision;

  // 只读视图：调用方不能改写免疫表。
  public ReadOnlyMemory<bool> ImmunityByDefinitionId =>
    _immuneByDefinitionId ?? [];

  // 派生：容量来自唯一内部数组。
  public int DefinitionCapacity => _immuneByDefinitionId?.Length ?? 0;
}
~~~

定义目录容量、DefinitionId 越界处理、内容热重载、实体恢复和 Revision 来源未锁定。本组件不承载命中冷却或 Buff 槽位。

### 6.14 EntityProvenanceComponent.cs

建议路径：src/Share/Entity/Components/EntityProvenanceComponent.cs

~~~text
componentId: ENTITY-COMP-PROVENANCE-01
componentOwner: Entity
attachment: traceable entity
status: proposed
currentNltx: no confirmed same-named component
evidence: AEntitySource_OnHit.cs:3-13; EntitySource_*.cs
crossSubsystemOwner: integration-review
~~~

~~~csharp
using Terraria.Relationships;

namespace EntityEcs.Components;

public struct EntityProvenanceComponent
{
  // 权威、创建时写入：直接来源类别。
  public EntityProvenanceKind Kind;

  // 权威、可空：直接导致实体出现的稳定实体引用。
  public EntityReference? SourceEntity;

  // 权威、可空：因果链上的父实体。
  public EntityReference? ParentEntity;

  // 权威、可空：OnHit 类来源中的受击实体。
  public EntityReference? StruckEntity;

  // 权威、可空：导致实体出现的物品定义 ID。
  public int? SourceItemDefinitionId;

  // 权威、可空：导致实体出现的投射物定义 ID。
  public int? SourceProjectileDefinitionId;

  // 权威、可空：没有实体来源时的世界事件类别。
  public WorldEventSourceKind? WorldEvent;

  // 派生：不存在实体或内容定义上游引用时视为根因。
  public bool IsRootCause =>
    SourceEntity is null &&
    ParentEntity is null &&
    SourceItemDefinitionId is null &&
    SourceProjectileDefinitionId is null;
}
~~~

EntityReference、EntityId、网络 ID、持久化 ID、账户 ID 和旧数组槽位不可互换。PlayerDeathReason 继续作为协议/文本投影，不回流成为实体身份真值。

### 6.15 EntityProvenanceKind.cs

建议路径：src/Share/Entity/Components/EntityProvenanceKind.cs

~~~csharp
namespace EntityEcs.Components;

public enum EntityProvenanceKind : byte
{
  Unknown,
  Parent,
  ItemUse,
  ItemUseWithAmmo,
  Projectile,
  OnHit,
  Mount,
  TileInteraction,
  TileBreak,
  Wiring,
  WorldEvent,
  WorldGeneration,
  Loot,
  SpawnNpc,
  Sync,
  RevengeSystem,
  FishedOut,
  DropAsItem,
  OverfullChest,
  DebugCommand,
  CoinRain
}
~~~

该枚举只表达来源分类，不决定来源 owner、归因优先级、网络权限或持久化格式。

### 6.16 WorldEventSourceKind.cs

建议路径：src/Share/Entity/Components/WorldEventSourceKind.cs

~~~csharp
namespace EntityEcs.Components;

public enum WorldEventSourceKind : byte
{
  None,
  StandardWorldEvent,
  Invasion,
  BossEvent,
  SeasonalEvent
}
~~~

nullable WorldEvent 与 WorldEventSourceKind.None 是否表达同一事实，需要统一，避免双重空值语义。

### 6.17 EncounterDamageCreditComponent.cs

建议路径：src/Combat/EncounterDamageCreditComponent.cs

~~~text
componentId: COMBAT-COMP-10
componentOwner: CombatAndStatus
attachment: independent encounter root entity
status: proposed
currentNltx: DamageContributionComponent is not equivalent
evidence: NPCDamageTracker.cs:58-73,178-310; BossDamageTracker.cs:37-85
crossSubsystemOwner: integration-review
~~~

~~~csharp
using System;
using System.Collections.Generic;

namespace Terraria.Combat;

public struct EncounterDamageCreditComponent
{
  public EncounterDamageCreditComponent(
    ulong encounterId,
    IReadOnlyList<DamageCreditEntry>? credits = null,
    int worldDamage = 0,
    CombatContributorId? lastContributor = null,
    long startedAtTick = 0,
    long lastHitAtTick = 0,
    EncounterCreditLifecycle lifecycle = EncounterCreditLifecycle.Active,
    int revision = 0)
  {
    EncounterId = encounterId;
    _credits = credits is null
      ? []
      : new List<DamageCreditEntry>(credits).ToArray();
    WorldDamage = worldDamage;
    LastContributor = lastContributor;
    StartedAtTick = startedAtTick;
    LastHitAtTick = lastHitAtTick;
    Lifecycle = lifecycle;
    Revision = revision;
  }

  // 权威、运行期：本次遭遇的稳定运行时 ID。
  public ulong EncounterId;

  // 权威：按稳定贡献者键排序的账本。
  private DamageCreditEntry[]? _credits;

  // 权威：无法归属于参与者的环境/世界伤害。
  public int WorldDamage;

  // 权威、可空：最近一次产生已接受伤害的参与者。
  public CombatContributorId? LastContributor;

  // 权威、运行期：遭遇起始和最近已接受伤害的模拟 Tick。
  public long StartedAtTick;
  public long LastHitAtTick;

  // 权威：遭遇贡献生命周期。
  public EncounterCreditLifecycle Lifecycle;

  // 权威：账本或生命周期变化的版本。
  public int Revision;

  // 只读视图：不提供内部数组写入能力。
  public ReadOnlyMemory<DamageCreditEntry> Credits => _credits ?? [];

  // 派生：不保存独立总量或空状态标志。
  public bool IsEmpty => (_credits is null or { Length: 0 }) &&
    WorldDamage == 0;

  // 派生：不保存第二份持续时长缓存。
  public long DurationTicks => Math.Max(0, LastHitAtTick - StartedAtTick);
}
~~~

该组件只能累计伤害结算已经接受并裁剪后的数值。多节 Boss 的成员关系、EncounterId 分配、账本排序/合并/溢出和关闭后写入策略不在此组件实现。

### 6.18 DamageCreditEntry.cs

建议路径：src/Combat/DamageCreditEntry.cs

~~~csharp
namespace Terraria.Combat;

public struct DamageCreditEntry
{
  public DamageCreditEntry(
    CombatContributorId contributor,
    int appliedDamage)
  {
    Contributor = contributor;
    AppliedDamage = appliedDamage;
  }

  // 权威：玩家持久身份或 World 身份。
  public CombatContributorId Contributor;

  // 权威：已接受、已裁剪后的累计伤害。
  public int AppliedDamage;
}
~~~

AppliedDamage 不接受原始伤害、被拒绝伤害或未提交预测值。贡献合并与溢出策略由后续 writer 定义。

### 6.19 CombatContributorId.cs

建议路径：src/Combat/CombatContributorId.cs

~~~csharp
namespace Terraria.Combat;

public readonly record struct CombatContributorId(
  CombatContributorKind Kind,
  string? PlayerAccountUuid);
~~~

身份约束：

- Kind 为 Player 时，PlayerAccountUuid 必须是规范化的持久账户 UUID；
- Kind 为 World 时，PlayerAccountUuid 必须为 null；
- 不使用玩家当前槽位、网络 ID、可变显示名称或整数 255；
- 本 record struct 暂不添加格式校验，因为账户 UUID 的最终 owner 和持久化策略尚未裁决。

### 6.20 CombatContributorKind.cs

建议路径：src/Combat/CombatContributorKind.cs

~~~csharp
namespace Terraria.Combat;

public enum CombatContributorKind : byte
{
  Player,
  World
}
~~~

Player/World 两类是否足以表达召唤物、环境伤害、NPC 盟友或服务器脚本来源，需要由归因整合设计裁决。

### 6.21 EncounterCreditLifecycle.cs

建议路径：src/Combat/EncounterCreditLifecycle.cs

~~~csharp
namespace Terraria.Combat;

public enum EncounterCreditLifecycle : byte
{
  Active,
  Closed,
  Expired
}
~~~

该枚举不定义关闭原因、过期时间、奖励提交、掉落或 NPC 生命周期。

## 7. Entity 组合与附着关系

### 7.1 可受击实体

~~~text
Damageable Entity
  + HealthComponent
  + HealthRegenerationStateComponent
  + DefenseComponent
  + DamageReductionComponent (optional)
  + DamageAcceptancePolicyComponent
  + ImmunityComponent
  + KnockbackPolicyComponent
  + StatusEffectSlotsComponent (optional)
  + StatusEffectImmunityComponent (optional)
  + EntityProvenanceComponent (optional)
~~~

Player 和 NPC 的死亡/复活/消失状态不放入 HealthComponent。由哪个生命周期 owner 产生死亡提交，仍需整合层裁决。

### 7.2 投射物和攻击来源

投射物继续拥有自己的伤害、击退、owner、penetration、轨迹和寿命组件。Combat 只消费冻结的攻击输入和 EntityProvenance，不复制投射物实例字段到受击实体。

~~~text
Projectile Entity
  + Projectile damage/owner/penetration/lifetime components
  + EntityProvenanceComponent (optional)
  -> immutable damage input
  -> Combat target resolution
~~~

这里的箭头只是边界方向说明，不是本文件定义的 Command 或 System。

### 7.3 遭遇根实体

~~~text
Encounter Root Entity
  + EncounterDamageCreditComponent
  + relationship/content references (defined elsewhere)
  -> read-only contribution fact
  -> reward/progress/loot projections
~~~

EncounterDamageCreditComponent 不应附着于可能重用的 NPC 槽位。NPC 与遭遇根的成员关系必须通过稳定实体关系或内容定义查询表达。

## 8. 组件禁止承载的行为

以下行为即使写成方法也不应加入组件：

~~~csharp
public void ApplyDamage(...);
public void Tick();
public void AddBuff(...);
public void RemoveBuff(...);
public void SpawnLoot(...);
public void SendNetworkMessage(...);
public void Save(Stream stream);
public void Load(Stream stream);
~~~

这些行为涉及状态变更、时钟、随机源、实体结构、消息或 I/O，需要独立 owner、错误边界、顺序、重试和验证。组件只保存内聚权威数据以及由字段计算出的只读派生值。

## 9. 与现有实现的迁移映射

### 9.1 已有同名组件

| 当前文件 | 草案目标 | 迁移结论 |
|---|---|---|
| src/Combat/HealthComponent.cs | HealthComponent | 保留文件名和命名空间；增加派生属性前需确认调用方 |
| src/Combat/DefenseComponent.cs | DefenseComponent | 保留文件名和命名空间；Value 的权威/缓存分类待定 |
| src/Combat/HealthRegenerationStateComponent.cs | HealthRegenerationStateComponent | 保留文件名和命名空间；新增字段会改变构造和复制语义 |
| src/Combat/ImmunityComponent.cs | ImmunityComponent | 形状冲突；单一 RemainingTicks 不能直接扩展为按键窗口而不迁移调用方 |

### 9.2 相关但不等价的已有类型

| 当前文件 | 不等价原因 | 处理建议 |
|---|---|---|
| src/Combat/HitCooldownComponent.cs | 按目标字典是来源侧/命中侧冷却，生命周期不等于目标通用免疫窗口 | 保留为待整合输入，禁止机械合并 |
| src/Combat/DamageContributionComponent.cs | 按 EntityReference 挂载于当前实体，不表达遭遇根、账户 UUID、WorldDamage 和生命周期 | 保留兼容路径，建立账本映射后再决定替换 |
| src/StatusEffects/StatusEffectsComponent.cs | 可变 List<TimedStatusEffect>，包含与本文槽位不同的 Source/Stacks 模型 | 不与 StatusEffectSlotsComponent 双写 |
| src/StatusEffects/TimedStatusEffect.cs | Source 和 Stacks 不在本文 StatusEffectSlot 字段中 | 归因/层数由独立设计裁决 |
| src/Combat/DamageRequest.cs | 单次命中输入，不是长期组件 | 保持命令/快照边界，不挂到实体 |
| src/Combat/DamageResult.cs | 单次结算结果，不是长期组件 | 保持结果值对象边界，不作为 Health 字段 |
| src/Combat/DeathCause.cs | 死亡原因快照，不是生命字段 | 由死亡生命周期/归因设计拥有 |
| src/Combat/DamageResolutionSystem.cs | 行为实现，不是组件 | 本草案不修改、不复制 |
| src/Combat/DeathResolutionSystem.cs | 死亡行为实现，不是组件 | 本草案不修改、不复制 |

### 9.3 严禁的直接替换

- 不把本文代码块直接复制到与当前同名类型相同的项目中，形成重复公共类型；
- 不在旧组件和新组件之间建立未定义的长期双写；
- 不将 DamageContributionComponent 的 EntityReference 字典直接重命名为 EncounterDamageCreditComponent；
- 不将 StatusEffectsComponent.Effects 直接重命名为 StatusEffectSlotsComponent.Slots；
- 不把 ImmunityComponent.RemainingTicks 直接包装成一个 General 窗口来宣称覆盖所有免疫语义；
- 不因构造函数可以接受空值就宣称初始化、持久化、网络恢复和默认容量已经解决。

## 10. 未决实现门

| ID | 未决事项 | 影响类型 | 未决原因 |
|---|---|---|---|
| BD-CS-CODE-01 | HealthComponent 唯一写者与死亡生命周期 owner | Health、Player/NPC 边界 | 生命耗尽不等于死亡 |
| BD-CS-CODE-02 | Defense.Value 的权威/缓存/快照分类 | Defense、DamageRequest | 属性重算与一次命中冻结时点未定义 |
| BD-CS-CODE-03 | Endurance 合法范围和钳制策略 | DamageReduction | 领域字段不等于值对象验证契约 |
| BD-CS-CODE-04 | IsImmortal 的局部效果与死亡语义 | Acceptance、Health、Death | 生命、击退、状态和死亡影响未统一 |
| BD-CS-CODE-05 | HitImmunityKey 有效组合与稳定排序 | Immunity、HitImmunityKey | 旧数组、来源引用、定义 ID 和代际未闭合 |
| BD-CS-CODE-06 | Immunity 集合清理与 Revision 时机 | Immunity、HitImmunityWindow | HasActiveWindow 依赖零窗口不变量 |
| BD-CS-CODE-07 | Player 44 槽/NPC 20 槽初始化 | StatusEffectSlots | 容量必须由实体初始化边界提供 |
| BD-CS-CODE-08 | Buff Source/Stacks 是否需要独立状态 | StatusEffectSlot、StatusEffects | 当前 TimedStatusEffect 与设计 Slot 不一致 |
| BD-CS-CODE-09 | 状态定义目录容量和免疫表重建 | StatusEffectImmunity | DefinitionId 范围和版本来源未定义 |
| BD-CS-CODE-10 | EntityReference 生命周期和因果链清理 | EntityProvenance | Guid、Scope、销毁、代际和恢复关系未闭合 |
| BD-CS-CODE-11 | EncounterCredit 目录归属和统计口径 | EncounterDamageCredit | 正文归入 Combat，结尾前九个组件表述可能遗漏 |
| BD-CS-CODE-12 | EncounterId 分配、恢复和成员关系 | EncounterDamageCredit | 不能由 NPC 槽位或显示名称推导 |
| BD-CS-CODE-13 | 贡献账本写入和奖励消费顺序 | EncounterDamageCredit | 已接受伤害、死亡、奖励和世界进度必须分开 |
| BD-CS-CODE-14 | struct 与 class 的最终选择 | 含数组的组件 | struct 复制可能造成 Revision/数组 owner 误用 |
| BD-CS-CODE-15 | 旧组件与草案组件的迁移方式 | 全部同名/相关类型 | 调用方、项目引用和行为语义尚未完成迁移审查 |

## 11. 副作用登记

本草案中的代码形状不执行外部副作用：

| 项目 | 草案状态 |
|---|---|
| 文件/数据库/网络读取 | 无 |
| 时钟读取 | 无 |
| 随机源读取 | 无 |
| 实体创建/销毁 | 无 |
| 其他组件修改 | 无 |
| 事件/消息发布 | 无 |
| 日志/指标/UI/音效 | 无 |
| 持久化写入 | 无 |
| 网络发送 | 无 |
| 后台任务/线程/定时器 | 无 |

构造函数只复制输入集合并赋值字段。未来若将集合修改、Revision 更新、命令发布或快照生成加入组件，必须重新登记 owner、顺序、失败、重复和恢复语义。

## 12. focused verifier 计划

本轮没有创建或运行 verifier；按用户要求没有编写测试，也没有运行测试。已对组件所属的两个受影响项目执行串行编译。后续实际落地时至少需要：

| verifier | 核心断言 | 状态 |
|---|---|---|
| COMBAT-COMPONENT-SHAPE | 11 个核心组件、支撑值类型、命名空间和文件边界符合草案 | not-run |
| COMBAT-DEFAULT-STATE | default struct、空集合、容量和派生属性语义明确且不抛异常 | not-run |
| COMBAT-HEALTH-BOUNDARY | Current/Maximum 与死亡/复活/消失状态不混用 | not-run |
| COMBAT-IMMUNITY-KEY | 作用域组合、来源实体、稳定排序、零窗口清理和 Revision 可审计 | not-run |
| COMBAT-STATUS-SLOTS | Player 44/NPC 20、刷新、淘汰、压缩和定义 ID 映射可保持 | not-run |
| COMBAT-PROVENANCE-ID | EntityReference、账户 UUID、网络 ID、持久 ID 和旧槽位不互换 | not-run |
| COMBAT-ENCOUNTER-CREDIT | 只累计已接受伤害，排序、合并、关闭、过期和重复提交可观察 | not-run |
| COMBAT-LEGACY-MAPPING | 哨兵值、旧数组、旧冷却和贡献身份转换语义明确 | not-run |
| COMBAT-WRITE-ROOT | Component 不执行行为，旧 Adapter 不与目标组件长期双写 | not-run |

本轮编译记录：

~~~text
pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build','./src/Combat/Terraria.Combat.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')"
项目：src/Combat/Terraria.Combat.csproj；退出码：0；警告：0；错误：0
产物：Build/bin/Terraria.Combat/Debug/net10.0/Terraria.Combat.dll

pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build','./src/Share/Entity/Terraria.EntityEcs.csproj','-m:1','-nr:false','-p:UseSharedCompilation=false','-p:MSBuildNodeReuse=false','-p:BuildInParallel=false')"
项目：src/Share/Entity/Terraria.EntityEcs.csproj；退出码：0；警告：0；错误：0
产物：Build/bin/Terraria.EntityEcs/Debug/net10.0/Terraria.EntityEcs.dll
~~~

上述编译验证只覆盖源码可编译性，不代表 Version4 行为等价、迁移完成或 verifier 已通过。

## 13. 最终声明

本文件是 CombatAndStatus 的实际 C# 组件代码草案。对应的 11 个组件和 10 个支撑值类型骨架已经写入 `src/Combat` 与 `src/Share/Entity/Components`，并通过了受影响项目的编译；它们仍未完成运行时行为、迁移和跨子系统契约。

本文件不声明：

- 当前 src/Combat 已经完成字段演进；
- ImmunityComponent、StatusEffects 或贡献账本已经完成迁移；
- Version4 行为已经等价；
- 网络、存档、死亡生命周期或主循环已经接线；
- 构建、测试或 verifier 已通过。

本轮围绕本草案的实际源码/文档目标为：

~~~text
D:\TRbackup\NLTX\src\Combat\
D:\TRbackup\NLTX\src\Share\Entity\Components\
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-06-version4-combat-and-status-component-code-draft.md
~~~

本轮没有修改 `dome/src/`、`Test/`、Version4、完整参考源码、tModLoader 文档或已有设计/研究报告；没有编写或运行测试；没有启动子代理。

verificationStatus: not-run
implementationStatus: component-skeleton-created
compileStatus: passed-for-affected-projects
testStatus: not-written
testRunStatus: not-run
behaviorVerificationStatus: not-run
