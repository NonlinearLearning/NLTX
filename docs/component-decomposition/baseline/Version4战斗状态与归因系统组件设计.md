# Version4 战斗、状态与归因系统组件设计

> 依据：[Version4 战斗、状态与归因系统代码盘点及组件拆分报告](Version4战斗状态与归因系统代码盘点及组件拆分报告.md)。
>
> 本文只定义组件、组件内嵌值对象的字段和只读派生属性；不定义 System、Query、Command、事件、
> Adapter、Projection、持久化格式或测试。所有“最终”均指目标 ECS 的字段归属，不表示这些字段
> 已在 `src/` 中实现或完成迁移。

## 1. 设计范围和字段约定

`Player.Hurt`、`NPC.StrikeNPC`、`Projectile.Damage`、Buff 数组和 `NPCDamageTracker` 的字段来源、
读写路径与状态证据见前述盘点报告第 2--4 节。本设计遵循 `public-decomposition` 的状态所有权规则：

- 一个组件只保存同一变更原因、相同生命周期的权威状态；不以“都与战斗有关”为由合并。
- 以 `=>` 定义的成员都是由本组件权威字段重复计算的派生属性，不持久化、不作为网络写入源。
- `private` 数组或集合是组件的唯一可变集合；跨领域调用方只能接收只读视图或不可变快照，不能直接
  修改状态。
- `EntityReference`、持久玩家账户 UUID、网络 ID、旧数组槽位和内容定义 ID 不是同一种身份。本文
  只在确有需要时保存稳定实体/持久身份；不把 `whoAmI`、`Main.npc[index]` 或 `Projectile.identity`
  当成组件的长期实体主键。
- 旧名称已经存在于 `src/Combat` 的组件以现有名称为最终名称：`HealthComponent`、
  `DefenseComponent`、`ImmunityComponent`、`HealthRegenerationStateComponent`。本设计描述它们
  的目标字段集合，避免新增同义的 `HealthStateComponent` 或 `HitImmunityComponent`。

| 组件 | 状态边界 | 生命周期 | 主要 Version4 证据 |
| --- | --- | --- | --- |
| `HealthComponent` | 当前与最大生命 | 可受伤实体存续期 | `Player.cs:1357-1361`; `NPC.cs:6341-6343` |
| `HealthRegenerationStateComponent` | 生命再生/退化积算 | 可受伤实体存续期 | `Player.cs:1369-1373,22335`; `NPC.cs:6111-6115` |
| `DefenseComponent` | 整数防御值 | 当前属性重算周期 | `Player.cs:1355`; `NPC.cs:6325` |
| `DamageReductionComponent` | 百分比减伤输入 | 当前属性重算周期 | `Player.cs:767,22263-22273` |
| `DamageAcceptancePolicyComponent` | 非冷却型受伤拒绝规则 | 实体存续期或 NPC AI 阶段 | `NPC.cs:6125,6143,6335,6387`; `Projectile.cs:11607-11634` |
| `ImmunityComponent` | 按命中键的冷却窗口 | 实体存续期 | `Player.cs:968-976,2475,22221-22334`; `NPC.cs:6303`; `Projectile.cs:158-162,256-258` |
| `KnockbackPolicyComponent` | 击退许可与抗性 | 实体存续期或属性重算周期 | `Player.cs:1383,22342-22347`; `NPC.cs:6359` |
| `StatusEffectSlotsComponent` | 有序 Buff 槽位和变更版本 | 实体存续期 | `Player.cs:1027-1033,3541-3731`; `NPC.cs:6065-6071,76385-76480` |
| `StatusEffectImmunityComponent` | Buff 类型免疫表 | 实体存续期或定义/效果重算周期 | `Player.cs:1033`; `NPC.cs:6071` |
| `EntityProvenanceComponent` | 生成/命中因果来源 | 实体创建到销毁 | `AEntitySource_OnHit.cs:3-13`; `EntitySource_*.cs` |
| `EncounterDamageCreditComponent` | 单场遭遇的已结算伤害贡献 | 遭遇开始到关闭/过期 | `NPCDamageTracker.cs:58-73,178-310`; `BossDamageTracker.cs:37-85` |

以下组件中，前九个归入 `Combat` 能力域。`EntityProvenanceComponent` 是所有可追溯实体共同的
身份/因果能力，应归入 `Entity` 域；它被 Combat 读取但不归 Combat 独占。`EncounterDamageCreditComponent`
归入 `Combat`，并附着在独立的遭遇根实体，而不附着在任意一个可能被重用的 NPC 槽位。

## 2. `HealthComponent`

保留现有组件名称。`Player.statLife`、`statLifeMax*` 与 `NPC.life`、`lifeMax` 是同一“生命存量”
概念；它们共同被伤害、治疗和再生更新，因而不再拆成玩家/NPC 各自的生命组件。`statLifeMax`
与 `statLifeMax2` 的历史差异由属性重算输入处理，本组件只保存本 tick 可受伤的有效上限。

```csharp
public struct HealthComponent
{
  // 权威：当前可被伤害、治疗和再生修改的生命值。
  public int Current;

  // 权威：当前属性重算后的有效生命上限，不保存定义默认上限的副本。
  public int Maximum;

  // 派生：生命为零不等同于死亡；死亡/复活状态归生命周期领域。
  public bool IsDepleted => Current <= 0;

  // 派生：供治疗资格、UI Projection 等读取。
  public int Missing => Math.Max(0, Maximum - Current);
}
```

不放入：`dead`、`deadTime`、`respawnTimer`、NPC `active`、掉落和墓碑。这些是玩家复活或 NPC
消失的生命周期状态，不能与生命存量合并。

## 3. `HealthRegenerationStateComponent`

保留现有组件名称。现有 `Rate` 和 `Accumulator` 对应 `lifeRegen` 与 `lifeRegenCount`；为保持玩家
受击后重置 `lifeRegenTime` 和 NPC 对预期损失率的需求，补足显式时间/预期字段。`ExpectedLossPerSecond`
使用可空值代替 Version4 的 `-1` 哨兵值。

```csharp
public struct HealthRegenerationStateComponent
{
  // 权威：每个再生步长的净生命变化；负数表示生命退化。
  public int Rate;

  // 权威：尚未结算为整数生命变化的积算量。
  public int Accumulator;

  // 权威、暂态：自上次成功伤害结算后的时间；受击时重置。
  public float TimeSinceLastDamage;

  // 权威、NPC 可选：UI/规则使用的预期每秒生命损失；null 表示没有该语义。
  public int? ExpectedLossPerSecond;

  // 派生：不另存 IsRegenerating/IsDegenerating 标志。
  public bool IsRegenerating => Rate > 0;
  public bool IsDegenerating => Rate < 0;
}
```

不放入：魔力再生、`manaRegenCount`、魔力延迟和 `statMana`。它们由 PlayerGameplay 的资源能力拥有，
即使某些受击效果会改变它们也只通过明确的资源变更协作。

## 4. `DefenseComponent`

保留现有组件名称和单一字段边界。防御是伤害计算的整数输入，和百分比耐久减伤的更新原因不同，
故不能合并为大而全的 `DamageMitigationStateComponent`。

```csharp
public struct DefenseComponent
{
  // 权威、派生于本 tick 属性重算：整数防御值。
  public int Value;

  // 派生：防御是否可能改变基础伤害计算。
  public bool HasValue => Value != 0;
}
```

不放入：攻击者的 `armorPenetration`、`meleeArmorPenetration`、投射物伤害类别或攻击值。这些随武器、
物品使用与投射物实例生命周期变化，属于攻击输入快照或其原有领域。

## 5. `DamageReductionComponent`

`Player.endurance` 在 `Hurt` 中于防御计算之后扣减伤害。它是受击目标的百分比减伤输入，而不是防御
整数的别名；独立组件允许只有百分比减伤的实体不携带防御状态。

```csharp
public struct DamageReductionComponent
{
  // 权威、派生于当前效果/装备：0.0 表示无百分比减伤。
  // 取值范围由结算规则限制，组件不缓存已经应用后的实际伤害。
  public float Endurance;

  // 派生：供资格和结算输入读取。
  public bool HasReduction => Endurance > 0f;
}
```

不放入：Solar、Beetle 或队伍分摊等一次命中特例的剩余次数、目标选择或临时计算值；它们不是所有
实体长期共有状态，必须留在各自能力状态或单次伤害输入中。

## 6. `DamageAcceptancePolicyComponent`

这不是命中冷却。它拥有目标的非冷却型“能否接受某类伤害”规则，来自 NPC 的
`dontTakeDamage`、`dontTakeDamageFromHostiles`、`trapImmune` 和 `immortal`。将它和
`ImmunityComponent` 分开，能让伤害资格不因免疫计时器而隐式改变。

```csharp
public struct DamageAcceptancePolicyComponent
{
  // 权威：拒绝普通伤害；对应 dontTakeDamage。
  public bool RejectAllDamage;

  // 权威：拒绝 hostile/NPC 敌对来源的伤害。
  public bool RejectHostileDamage;

  // 权威：拒绝陷阱来源的伤害。
  public bool RejectTrapDamage;

  // 权威：命中可产生局部效果但不得导致普通死亡/伤害后果的生命保护规则。
  public bool IsImmortal;

  // 派生：只描述通用资格；来源种类的细分判断不写入组件。
  public bool CanAcceptAnyDamage => !RejectAllDamage;
}
```

不放入：玩家 `creativeGodMode`、Shimmer 规避和阵营/PvP 关系。前者是创造模式能力，后两者分别
缺少可确认实现或需要跨实体关系查询；不能为方便而写成永久的组件布尔字段。

## 7. `ImmunityComponent`

保留现有组件名称，但用有序的命中窗口替代当前单一 `RemainingTicks`。这覆盖 Player 的普通免疫、
`hurtCooldowns`，NPC 按攻击者的 `immune[]`，以及投射物 local/static NPC 命中免疫的统一键模型。
旧数组索引只能作为导入/协议 Adapter 的输入，不能进入字段。

```csharp
public struct ImmunityComponent
{
  // 权威：组件私有的命中窗口；始终按 HitImmunityKey 的稳定排序保存。
  private HitImmunityWindow[] _windows;

  // 权威：每次窗口增删或剩余时间改变时递增，供复制/缓存失效使用。
  public int Revision;

  // 派生：只读视图，调用方不能替换或重排窗口。
  public ReadOnlyMemory<HitImmunityWindow> Windows => _windows;
  public bool HasActiveWindow => _windows.Length != 0;
}

public struct HitImmunityWindow
{
  // 权威：窗口的作用域和来源键。
  public HitImmunityKey Key;

  // 权威、暂态：剩余 Tick；零值窗口应在当前 Tick 清理。
  public int RemainingTicks;

  // 派生：不另存 IsActive。
  public bool IsActive => RemainingTicks > 0;
}

public readonly record struct HitImmunityKey(
    HitImmunityScope Scope,
    EntityReference? SourceEntity,
    int? SourceDefinitionId);

public enum HitImmunityScope : byte
{
  General,
  Attacker,
  ProjectileInstance,
  ProjectileDefinition
}
```

`immuneNoBlink`、`immuneAlpha` 和闪烁方向是表现状态，不进入本组件。命中窗口的 `SourceEntity`
使用稳定实体引用；若来源已经销毁，窗口仍可按原键自然到期，不回读已重用的数组槽位。

## 8. `KnockbackPolicyComponent`

该组件只定义受击目标是否接受击退以及其抗性。位置、速度、落点和碰撞结果属于 MovementPhysics，
绝不写入本组件。

```csharp
public struct KnockbackPolicyComponent
{
  // 权威：对应 Player.noKnockback 等能力规则。
  public bool IsImmune;

  // 权威：对应 NPC.knockBackResist；1.0 为完全接受，0.0 为完全抗性。
  public float Resistance;

  // 派生：不保存第二个反向布尔值。
  public bool CanReceiveKnockback => !IsImmune && Resistance > 0f;
}
```

## 9. `StatusEffectSlotsComponent`

Player 有 44 个 Buff 槽、NPC 有 20 个 Buff 槽，且 `AddBuff`、`DelBuff` 的刷新、淘汰和压缩顺序
可观察。因此保留有序槽位模型，初期不把每一个 Buff 机械转换为独立实体。容量由 `_slots.Length`
表达，避免第二份可能不同步的 `Capacity` 字段。

```csharp
public struct StatusEffectSlotsComponent
{
  // 权威：固定容量、有序的槽位。空槽以 DefinitionId == 0 表示。
  private StatusEffectSlot[] _slots;

  // 权威：每次应用、刷新、移除、淘汰或压缩后递增。
  public int Revision;

  // 派生：只读视图和数组长度，均不单独持久化。
  public ReadOnlyMemory<StatusEffectSlot> Slots => _slots;
  public int Capacity => _slots.Length;
}

public struct StatusEffectSlot
{
  // 权威：内容目录中的 Buff/状态效果定义 ID；0 是空槽。
  public int DefinitionId;

  // 权威、暂态：剩余 Tick；零值与空槽保持一致。
  public int RemainingTicks;

  // 派生：不维护第二套有效位图。
  public bool IsOccupied => DefinitionId != 0 && RemainingTicks > 0;
}
```

不放入：Buff 的定义、互斥分组、时长上限、难度倍率、宠物/近战类别及网络可移除资格。这些是
`StatusEffectDefinitionCatalog` 的只读定义与规则输入，不是每个实体实例状态。

## 10. `StatusEffectImmunityComponent`

该组件表达“目标对某个状态效果定义是否免疫”，与命中后的 cooldown 免疫不同。Version4 已用
`bool[BuffID.Count]` 表示它，目标 ECS 保持按定义 ID 的稠密映射，但不公开可写数组。

```csharp
public struct StatusEffectImmunityComponent
{
  // 权威：索引等于状态效果定义 ID；true 表示拒绝该效果。
  private bool[] _immuneByDefinitionId;

  // 权威：定义免疫集合重建时递增，供状态效果资格缓存失效。
  public int Revision;

  // 派生：只读视图和定义容量。
  public ReadOnlyMemory<bool> ImmunityByDefinitionId => _immuneByDefinitionId;
  public int DefinitionCapacity => _immuneByDefinitionId.Length;
}
```

不放入：Buff 槽位、持续时间或状态效果来源。前两者属于 `StatusEffectSlotsComponent`，来源只在
单次效果应用命令/快照中冻结，不能变成可长期伪造的实体字段。

## 11. `EntityProvenanceComponent`

该组件放在 `Entity` 域而不是 `Combat` 域。它保存实体创建时的因果关系，覆盖 ItemUse、Projectile、
Parent、OnHit、Tile、Wiring、WorldEvent、Loot 等 `IEntitySource` 类别。没有稳定身份的旧 SourceId
只能在 Adapter 中转换，不能取代实体引用。

```csharp
public struct EntityProvenanceComponent
{
  // 权威、创建时写入：最直接的创建/命中来源类别。
  public EntityProvenanceKind Kind;

  // 权威、可空：直接导致本实体出现的稳定实体引用。
  public EntityReference? SourceEntity;

  // 权威、可空：因果链上的父实体；不和 SourceEntity 合并。
  public EntityReference? ParentEntity;

  // 权威、可空：OnHit 类来源的受击实体，不用槽位索引表示。
  public EntityReference? StruckEntity;

  // 权威、可空：导致本实体出现的物品定义 ID。
  public int? SourceItemDefinitionId;

  // 权威、可空：导致本实体出现的投射物定义 ID。
  public int? SourceProjectileDefinitionId;

  // 权威、可空：WorldEvent 等没有实体源时的事件类别。
  public WorldEventSourceKind? WorldEvent;

  // 派生：仅当没有上游实体或定义引用时成立。
  public bool IsRootCause =>
      SourceEntity is null &&
      ParentEntity is null &&
      SourceItemDefinitionId is null &&
      SourceProjectileDefinitionId is null;
}

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

public enum WorldEventSourceKind : byte
{
  None,
  StandardWorldEvent,
  Invasion,
  BossEvent,
  SeasonalEvent
}
```

`EntityReference` 是 ECS 的稳定实体引用值类型，由 Entity 域定义；它不能替换为网络 ID、持久 ID、
账户 ID 或数组槽位。`PlayerDeathReason` 仍是协议/文本投影，它的旧索引字段不回流到这里。

## 12. `EncounterDamageCreditComponent`

该组件附着在一个独立的遭遇根实体。多节 Boss 或复合 Boss 的成员关系由只读内容定义/关系查询
判断；组件只累计已经由伤害结算接受并裁剪后的数值。它不保存显示名称，名称应由 Projection 从
玩家/世界身份解析，避免把可变玩家名当作账本身份。

```csharp
public struct EncounterDamageCreditComponent
{
  // 权威、运行期：本次遭遇的稳定运行时 ID。
  public ulong EncounterId;

  // 权威：贡献账本；按 CombatContributorId 稳定排序，调用方只读。
  private DamageCreditEntry[] _credits;

  // 权威：无法归属到参与者的环境/世界伤害。
  public int WorldDamage;

  // 权威、可空：最后一次产生已接受伤害的参与者。
  public CombatContributorId? LastContributor;

  // 权威、运行期：遭遇开始和最后一次已接受伤害的模拟 Tick。
  public long StartedAtTick;
  public long LastHitAtTick;

  // 权威：Active、Closed 或 Expired；取代散落的活跃/最近列表关系。
  public EncounterCreditLifecycle Lifecycle;

  // 权威：每次账本或生命周期变化递增。
  public int Revision;

  // 派生：不保存第二份总伤害、空状态或时长缓存。
  public ReadOnlyMemory<DamageCreditEntry> Credits => _credits;
  public bool IsEmpty => _credits.Length == 0 && WorldDamage == 0;
  public long DurationTicks => Math.Max(0, LastHitAtTick - StartedAtTick);
}

public struct DamageCreditEntry
{
  // 权威：玩家持久身份或 World 身份；不使用本次会话槽位。
  public CombatContributorId Contributor;

  // 权威：已接受、已裁剪后的累计伤害。
  public int AppliedDamage;
}

public readonly record struct CombatContributorId(
    CombatContributorKind Kind,
    string? PlayerAccountUuid);

public enum CombatContributorKind : byte
{
  Player,
  World
}

public enum EncounterCreditLifecycle : byte
{
  Active,
  Closed,
  Expired
}
```

当 `Kind` 为 `Player` 时，`PlayerAccountUuid` 必须采用现有
`PlayerIdentityComponent.CanonicalAccountUuid` / `PlayerPersistentState.Uuid` 的规范化 UUID 字符串；
当 `Kind` 为 `World` 时它必须为 `null`。这保留了当前持久化模型而不借用临时玩家槽位或整数
`255`。Boss 击败旗标、入侵积分和世界事件进度都不属于本组件，只能消费其已结算贡献事实。

## 13. 排除的非组件状态

下列对象与战斗调用链有关，但不应为了“完整”而变成长期组件字段：

| 数据 | 正确边界 | 排除理由 |
| --- | --- | --- |
| `DamageIntent`、`DamageResolution`、`DamageResolvedFact`、`DamageAttributionSnapshot` | 单次命令/不可变快照 | 每次命中独有；把它们挂在实体上会造成旧命中覆盖和重复结算风险。 |
| 投射物 `damage`、`knockBack`、`owner`、`penetrate`、轨迹和寿命 | Projectile | 随投射物生成、命中和销毁变化，Combat 只消费冻结的伤害输入。 |
| 玩家/物品攻击伤害、暴击、穿甲 | ItemGameplay / PlayerGameplay | 攻击方属性，不是受击目标状态，也不应与 `DefenseComponent` 混合。 |
| `dead`、复活倒计时、NPC `active`、掉落、墓碑 | PlayerSpawnAndRespawn / NpcSpawnAiTown | 是生命周期与结构变更状态，生命耗尽不是它们的同义字段。 |
| `CombatText`、音效、粉尘、粒子、闪烁 alpha | ClientPresentationProjection | 只消费已提交的伤害/治疗事实，不能成为权威状态。 |
| `PlayerDeathReason` 二进制字段、网络包、旧数组槽位 | Adapter / Projection | 保持协议兼容时可编码/解码，但不能成为 ECS 的实体身份真值。 |
| Buff 定义、可叠加时长、互斥分组、难度倍率 | `StatusEffectDefinitionCatalog` | 全局只读内容，不按实体复制。 |

因此，目标实现的 Combat 目录只新增或演进前九个 Combat 组件；`EntityProvenanceComponent` 进入
Entity 领域；所有命令、快照、System、Query 与投影均保持在组件字段设计之外。
