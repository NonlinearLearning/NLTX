# Version4 战斗、状态与归因组件设计报告

> 生成日期：2026-09-06  
> 报告类型：基于组件设计文档的结构化审查与实现前报告  
> `verificationStatus: not-run`  
> 子代理：未启动

## 1. 输入文件核对

用户指定的输入文件为：

```text
D:\TRbackup\NLTX\docs\design\2026-09-05-version4-combat-and-status-component-design.md
```

该路径在本次读取时不存在。仓库中实际找到并采用的同主题组件设计文件是：

```text
D:\TRbackup\NLTX\docs\Version4战斗状态与归因系统组件设计.md
```

本报告只根据上述现存设计文件整理，不把不存在的指定路径当作已读取内容。该现存设计文件的依据报告为 `docs/Version4战斗状态与归因系统代码盘点及组件拆分报告.md`；本报告沿用设计文件已经给出的 Version4 证据定位，不重新声称已经完成 Version4 源码、tModLoader 或 SS14 的独立复核。

本次没有覆盖或修改已有设计文件、源代码、测试、Version4 参考树、完整参考树或其他报告；只新增本报告文件。

## 2. 执行摘要

设计文档把战斗、状态与归因拆成 11 个顶层组件候选，并按状态范围分为三类：

1. `Combat` 能力域的实体战斗/状态组件：生命、生命再生、防御、百分比减伤、受伤准入、命中免疫、击退策略、状态效果槽位和状态效果免疫；
2. `Entity` 能力域的通用因果来源组件：`EntityProvenanceComponent`；
3. `Combat` 能力域但附着于遭遇根实体的贡献账本：`EncounterDamageCreditComponent`。

整体拆分方向是合理的。设计没有创建一个包含生命、免疫、Buff、击退和归因的巨型 `CombatState`，也没有把 `dead`、NPC `active`、掉落、网络包或表现字段塞进生命组件。它还明确区分了实体引用、持久账户 UUID、网络 ID、旧数组槽位和内容定义 ID，避免把可复用的旧索引当作长期身份。

但是，这是一份字段设计，不是可运行的迁移方案。它没有定义唯一写者、系统调度、命令提交、死亡生命周期、网络/存档协议或验证实现。因此当前最准确的结论是：

> 组件字段边界已形成可进入实现评审的候选设计；实现状态、行为等价、网络兼容、持久化兼容和运行时接线均未由本文证明。

实现前必须先处理三类问题：

- 每个权威字段的唯一写者、提交根和跨域交接尚未闭合；
- `EntityReference`、遭遇 ID、Buff 初始化容量、免疫窗口排序与数组兼容策略缺少可执行契约；
- `EncounterDamageCreditComponent` 的归属在正文与结尾存在表述不一致，需要在实现前定案。

## 3. 设计范围与状态口径

### 3.1 文档明确包含的内容

设计文档覆盖以下状态形态：

| 范围 | 设计结论 |
|---|---|
| 生命 | `HealthComponent` 只保存当前生命和有效最大生命；生命耗尽不等同于死亡。 |
| 再生 | `HealthRegenerationStateComponent` 保存再生/退化速率、整数积算和受击后计时。 |
| 伤害输入 | 防御、百分比减伤、非冷却型伤害拒绝、命中免疫和击退策略分开建模。 |
| 状态效果 | 保留有序 Buff 槽位及独立的按定义 ID 免疫表，不机械地为每个 Buff 创建实体。 |
| 归因 | 因果来源是通用 Entity 能力；遭遇贡献是独立遭遇根的 Combat 账本。 |
| 派生属性 | `=>` 属性由本组件权威字段计算，不单独持久化，也不作为网络写入源。 |
| 可变集合 | 私有数组/集合是唯一可变集合；外部只接收只读视图或不可变快照。 |

### 3.2 文档明确排除的内容

源设计明确不定义 System、Query、Command、事件、Adapter、Projection、持久化格式和测试。下列事项因此不能从本报告推断为已解决：

- 伤害请求的创建、排序、幂等键和重复提交处理；
- 伤害资格、减伤、暴击、随机数和状态效果叠加的实际计算顺序；
- 生命归零后的玩家复活、NPC 消失、掉落、墓碑和世界进度提交；
- `EntityReference` 的分配、销毁、代际或槽位重用保护；
- 网络包、存档字节、版本迁移和坏数据回滚；
- UI、CombatText、音效、粒子和闪烁效果；
- 具体 System 的调度顺序和主循环接线。

## 4. 组件总览与归属

设计文档第 1 节列出 11 个顶层组件。状态列表示“设计文件中的字段设计状态”，不是当前代码中的实现状态。

| # | 组件 | 建议能力域/附着对象 | 核心权威状态 | 主要派生值或只读视图 | 生命周期 | 设计状态 |
|---:|---|---|---|---|---|---|
| 1 | `HealthComponent` | `Combat`；可受伤实体 | `Current`、`Maximum` | `IsDepleted`、`Missing` | 实体存续期 | 字段已定义；未实现验证 |
| 2 | `HealthRegenerationStateComponent` | `Combat`；可受伤实体 | `Rate`、`Accumulator`、`TimeSinceLastDamage`、`ExpectedLossPerSecond` | `IsRegenerating`、`IsDegenerating` | 实体存续期 | 字段已定义；未实现验证 |
| 3 | `DefenseComponent` | `Combat`；受击目标 | `Value` | `HasValue` | 属性重算周期 | 字段已定义；未实现验证 |
| 4 | `DamageReductionComponent` | `Combat`；受击目标 | `Endurance` | `HasReduction` | 属性重算周期 | 新边界候选 |
| 5 | `DamageAcceptancePolicyComponent` | `Combat`；受击目标 | 四类拒绝/保护标志 | `CanAcceptAnyDamage` | 实体存续期或 NPC AI 阶段 | 新边界候选 |
| 6 | `ImmunityComponent` | `Combat`；受击实体或命中来源 | 私有 `HitImmunityWindow[]`、`Revision` | `Windows`、`HasActiveWindow` | 实体存续期 | 现有名称的目标字段演进 |
| 7 | `KnockbackPolicyComponent` | `Combat`；受击目标 | `IsImmune`、`Resistance` | `CanReceiveKnockback` | 实体存续期或属性重算周期 | 新边界候选 |
| 8 | `StatusEffectSlotsComponent` | `Combat`；状态效果承载实体 | 私有有序 `StatusEffectSlot[]`、`Revision` | `Slots`、`Capacity` | 实体存续期 | 新边界候选 |
| 9 | `StatusEffectImmunityComponent` | `Combat`；状态效果承载实体 | 私有 `bool[]`、`Revision` | `ImmunityByDefinitionId`、`DefinitionCapacity` | 实体存续期或定义/效果重算周期 | 新边界候选 |
| 10 | `EntityProvenanceComponent` | `Entity`；可追溯实体 | 来源类别、实体/定义来源、世界事件来源 | `IsRootCause` | 创建到销毁 | 跨域通用能力候选 |
| 11 | `EncounterDamageCreditComponent` | `Combat`；独立遭遇根实体 | 遭遇 ID、贡献数组、世界伤害、最近贡献者、时间、生命周期、版本 | `Credits`、`IsEmpty`、`DurationTicks` | 遭遇开始到关闭/过期 | 遭遇账本候选 |

设计文档说明前九项属于 Combat，`EntityProvenanceComponent` 属于 Entity，`EncounterDamageCreditComponent` 属于 Combat 且不应附着在可重用的 NPC 槽位上。这里的“前九项”是按文档总览表从 `HealthComponent` 数到 `StatusEffectImmunityComponent`；但文档第 13 节又写成“Combat 目录只新增或演进前九个 Combat 组件”，没有再次明确是否包含遭遇贡献组件。该处属于设计一致性问题，见第 10 节。

## 5. 组件字段审查

### 5.1 `HealthComponent`

```text
权威：Current, Maximum
派生：IsDepleted, Missing
```

设计正确地把 `dead`、`deadTime`、`respawnTimer`、NPC `active`、掉落和墓碑排除在外。这样可以保持“生命耗尽”和“死亡生命周期转换”两个不同概念，也避免把玩家和 NPC 的复活/消失状态强行塞进通用生命组件。

需要在实现前补充：

- `Maximum` 是当前属性重算后的有效上限，但没有定义其计算结果何时提交、伤害结算期间是否允许变化；
- `Current` 的合法范围、治疗溢出、负伤害和归零钳制没有形成契约；
- 生命写入者应与死亡写入者分开记录，否则“生命组件不拥有死亡”会在实现中被绕开。

主要证据定位：`Player.cs:1357-1361`、`NPC.cs:6341-6343`。

### 5.2 `HealthRegenerationStateComponent`

```text
权威：Rate, Accumulator, TimeSinceLastDamage, ExpectedLossPerSecond
派生：IsRegenerating, IsDegenerating
```

用 `int? ExpectedLossPerSecond` 替代 Version4 的 `-1` 哨兵值，提升了领域表达的清晰度，但会产生兼容映射要求：导入时必须把旧哨兵转换为 `null`，导出时必须按既有协议需要恢复哨兵或版本化字段。

设计明确不接管魔力再生、`manaRegenCount`、魔力延迟和 `statMana`，这保持了与 PlayerGameplay 资源能力的边界。

需要补充：

- `Rate` 与 `Accumulator` 的单位、每 tick 结算时点和负值下溢规则；
- `TimeSinceLastDamage` 使用的时钟来源，尤其是暂停、回放和服务器 tick 重置；
- `ExpectedLossPerSecond` 是 NPC 专用可选输入还是可被其他实体使用；
- 受击重置与成功伤害、被拒绝命中之间的精确条件。

主要证据定位：`Player.cs:1369-1373,22335`、`NPC.cs:6111-6115`。

### 5.3 `DefenseComponent` 与 `DamageReductionComponent`

设计将整数防御与百分比耐久减伤分开：

| 组件 | 权威字段 | 派生值 | 分离理由 |
|---|---|---|---|
| `DefenseComponent` | `Value` | `HasValue` | 整数防御是伤害计算输入，生命周期与百分比减伤不同。 |
| `DamageReductionComponent` | `Endurance` | `HasReduction` | 百分比减伤是受击目标输入，不是整数防御的别名。 |

该拆分避免了 `DamageMitigationStateComponent` 重新成为跨原因的大组件。设计也正确地把攻击方的穿甲、伤害类别和攻击值排除在受击目标组件之外。

实现前仍需决定：

- `DefenseComponent.Value` 虽被描述为“权威、派生于本 tick 属性重算”，应标为属性快照还是 Combat 权威输入；
- `Endurance` 的范围、非法浮点值、钳制和叠加规则；
- 防御和减伤的版本/快照是否必须与一次 `DamageRequest` 一起冻结，避免命中中途重算。

主要证据定位：`Player.cs:1355,767,22263-22273`、`NPC.cs:6325`。

### 5.4 `DamageAcceptancePolicyComponent`

```text
权威：RejectAllDamage, RejectHostileDamage, RejectTrapDamage, IsImmortal
派生：CanAcceptAnyDamage
```

该组件与 `ImmunityComponent` 分离是重要边界：前者是非冷却型资格/保护策略，后者是按命中键变化的时间窗口。设计没有把 `creativeGodMode`、Shimmer 规避或阵营/PvP 关系简化成永久布尔字段，避免把能力、空间关系和临时资格混成一类状态。

需要明确 `IsImmortal` 的语义：文档将其描述为“命中可产生局部效果但不得导致普通死亡/伤害后果”的生命保护规则，但没有定义它是否阻止生命扣除、是否允许击退、是否允许状态效果以及是否只对 NPC 有效。这个字段必须在资格查询和死亡提交之间有单独的行为契约。

主要证据定位：`NPC.cs:6125,6143,6335,6387`、`Projectile.cs:11607-11634`。

### 5.5 `ImmunityComponent`

设计把原有单一 `RemainingTicks` 目标扩展为按 `HitImmunityKey` 排序的窗口集合：

```text
ImmunityComponent
  - private HitImmunityWindow[] _windows
  - Revision
  - Windows (read-only)
  - HasActiveWindow

HitImmunityWindow
  - Key
  - RemainingTicks
  - IsActive

HitImmunityKey
  - Scope
  - SourceEntity?
  - SourceDefinitionId?
```

该模型可以表达普通免疫、按攻击者免疫、按投射物实例免疫和按投射物定义免疫，并明确禁止把旧数组索引直接提升为稳定身份。`SourceEntity` 销毁后仍按原键自然到期，也避免了槽位重用造成的误命中。

实现前的关键缺口：

- “稳定排序”的排序键未定义，特别是空来源、实体引用代际和定义 ID 同时存在时的顺序；
- `HitImmunityKey` 的有效组合未定义，例如 `General` 是否必须没有来源；
- `HasActiveWindow` 以 `_windows.Length != 0` 为条件，而窗口注释又规定零值窗口应在当前 tick 清理；这依赖一个尚未写出的“集合中永远没有零值窗口”的不变量；
- `Revision` 的递增时机、溢出策略以及“剩余时间变化是否每 tick 都递增”未定义；
- Player 普通免疫、Player `hurtCooldowns`、NPC `immune[]` 与 Projectile local/static 免疫的映射/导入顺序未定义。

`immuneNoBlink`、`immuneAlpha` 和闪烁方向被排除为表现状态，这一归属清晰。

主要证据定位：`Player.cs:968-976,2475,22221-22334`、`NPC.cs:6303`、`Projectile.cs:158-162,256-258`。

### 5.6 `KnockbackPolicyComponent`

```text
权威：IsImmune, Resistance
派生：CanReceiveKnockback
```

设计只保存受击目标对击退的许可和抗性，把位置、速度、落点及碰撞结果留给 Movement/Physics，避免 Combat 组件拥有移动状态。

需要补充 `Resistance` 的边界契约。文档说明 `1.0` 表示完全接受、`0.0` 表示完全抗性，并用 `Resistance > 0` 推导 `CanReceiveKnockback`，但没有说明大于 `1.0`、负数、NaN 或“免疫但有正抗性”时由谁处理。

主要证据定位：`Player.cs:1383,22342-22347`、`NPC.cs:6359`。

### 5.7 `StatusEffectSlotsComponent`

设计保留有序、固定容量 Buff 槽位，而不是把每个 Buff 机械地拆成独立实体：

```text
StatusEffectSlotsComponent
  - private StatusEffectSlot[] _slots
  - Revision
  - Slots (read-only)
  - Capacity

StatusEffectSlot
  - DefinitionId
  - RemainingTicks
  - IsOccupied
```

这是对 Version4 可观察槽位行为的保守建模。文档记录 Player 有 44 个槽、NPC 有 20 个槽，但把容量交给 `_slots.Length`，避免保留第二个可能不一致的 `Capacity` 字段。Buff 定义、互斥分组、时长上限、难度倍率和网络可移除资格被正确排除到内容定义/规则边界。

实现前需要闭合：

- Player/NPC 初始容量如何选择，是否允许其他实体携带不同容量；
- `DefinitionId == 0` 是否永远表示空槽，以及有效定义 ID 的范围；
- 槽位刷新、淘汰、压缩、零剩余清理和 `Revision` 递增的先后；
- `StatusEffectSlot` 是否需要来源、层数或应用时间；设计明确没有保存来源，因此归因必须通过单次命令/快照承担；
- 旧 `buffType[]`、`buffTime[]` 的网络/存档顺序如何映射到封装后的私有数组。

主要证据定位：`Player.cs:1027-1033,3541-3731`、`NPC.cs:6065-6071,76385-76480`。

### 5.8 `StatusEffectImmunityComponent`

```text
权威：private bool[] _immuneByDefinitionId, Revision
派生：ImmunityByDefinitionId, DefinitionCapacity
```

设计保留 Version4 的按 Buff 定义 ID 稠密映射，但不把可写数组暴露给调用方；它与 Buff 槽位和命中冷却免疫分开，边界正确。

实现前需要决定定义目录容量的来源、定义 ID 越界处理、内容热重载时的重建方式、旧数组版本映射和 `Revision` 的生命周期。文档没有定义状态效果定义目录的 owner，也没有把它作为本报告的组件对象，因此这部分必须在 ContentCatalog/Status 规则设计中补齐。

主要证据定位：`Player.cs:1033`、`NPC.cs:6071`。

### 5.9 `EntityProvenanceComponent`

设计把因果来源放入 `Entity` 域而不是 Combat 域，覆盖 ItemUse、Projectile、Parent、OnHit、Tile、Wiring、WorldEvent、Loot 等来源：

```text
权威：Kind, SourceEntity?, ParentEntity?, StruckEntity?,
      SourceItemDefinitionId?, SourceProjectileDefinitionId?, WorldEvent?
派生：IsRootCause
```

该归属避免 Combat 独占通用生成因果关系，也避免使用旧 `SourceId` 或数组槽位代替实体引用。`PlayerDeathReason` 保留为协议/文本投影，不回流为 ECS 实体身份，这一点与身份分层原则一致。

需要补充：

- `SourceEntity`、`ParentEntity` 和 `StruckEntity` 是否允许同时存在，以及三者的语义优先级；
- 因果链长度、循环引用和父实体已销毁时的读取策略；
- `WorldEvent` 与 `IsRootCause` 的关系；当前 `IsRootCause` 只检查实体/定义引用为空，因此有世界事件时也会被视作根因，需要确认这是有意设计；
- `CoinRain`、`RevengeSystem` 等枚举值是否属于稳定协议，还是只属于内部来源分类。

主要证据定位：`AEntitySource_OnHit.cs:3-13`、`EntitySource_*.cs`。

### 5.10 `EncounterDamageCreditComponent`

设计把贡献账本放在独立遭遇根实体，而不是 NPC 槽位：

```text
权威：EncounterId, _credits, WorldDamage, LastContributor,
      StartedAtTick, LastHitAtTick, Lifecycle, Revision
派生：Credits, IsEmpty, DurationTicks

DamageCreditEntry：Contributor, AppliedDamage
CombatContributorId：Kind, PlayerAccountUuid
```

该设计正确地区分了“已接受并裁剪的伤害贡献”和单个 `lastInteraction`，并规定玩家贡献者使用规范化账户 UUID，不借用临时玩家槽位或 `255`。Boss 击败旗标、入侵积分和世界事件进度不进入贡献组件，只消费已结算贡献事实。

需要补充：

- 遭遇根实体如何创建、如何关联多节 Boss、何时关闭或过期；
- `EncounterId` 的分配域、持久化需求和跨重启唯一性；
- 贡献数组的排序、重复贡献者合并、整数溢出和环境伤害归属；
- `LastContributor` 是否在拒绝命中、零伤害命中或死亡后提交时更新；
- `CombatContributorKind` 是否需要除 Player/World 外的 NPC、召唤物或服务器类别。

主要证据定位：`NPCDamageTracker.cs:58-73,178-310`、`BossDamageTracker.cs:37-85`。

## 6. 权威状态、派生值与身份分层

### 6.1 权威状态归属

| 权威状态 | 设计 owner | 外部读取方式 | 不应承担的职责 |
|---|---|---|---|
| 当前/最大生命 | `HealthComponent` | 只读字段/快照 | 不负责死亡、复活、NPC 消失、掉落 |
| 再生积算与受击计时 | `HealthRegenerationStateComponent` | 只读状态输入 | 不负责魔力资源 |
| 防御与耐久减伤输入 | `DefenseComponent`、`DamageReductionComponent` | 结算输入快照 | 不保存攻击者伤害或穿甲 |
| 非冷却型受伤规则 | `DamageAcceptancePolicyComponent` | 资格查询输入 | 不代替阵营关系或临时规避效果 |
| 命中窗口 | `ImmunityComponent` | 通过键查询只读窗口 | 不保存闪烁和 alpha 表现 |
| 击退许可/抗性 | `KnockbackPolicyComponent` | 击退结算输入 | 不写位置、速度或碰撞 |
| Buff 槽位与 Buff 免疫 | 两个 Status 组件 | 只读槽位/稠密免疫表 | 不持有全局 Buff 定义和规则 |
| 因果来源 | `EntityProvenanceComponent` | Entity/Combat 只读来源 | 不替代网络 ID、账户 ID、数组槽位 |
| 遭遇贡献账本 | 独立遭遇根的 `EncounterDamageCreditComponent` | 贡献快照/排序查询 | 不拥有世界进度、奖励旗标或 NPC 生命周期 |

### 6.2 身份分层要求

设计文件重复强调以下值不可互换：

| 身份/索引 | 正确用途 | 禁止的替代解释 |
|---|---|---|
| `EntityReference` | ECS 实体关系 | 不能当网络 ID 或持久账户 ID |
| 玩家账户 UUID | 贡献账本中的持久贡献者身份 | 不能用当前会话槽位替代 |
| 网络 ID | 协议/复制边界 | 不能作为长期实体主键 |
| `whoAmI`、`Main.npc[index]` | 旧数组槽位兼容 | 不能直接提升为稳定身份 |
| `Projectile.identity` | 旧投射物身份/协议语义 | 不能未经证据成为持久实体 ID |
| 内容定义 ID | Buff、物品、投射物等内容引用 | 不能当实体实例身份 |

该分层是本设计最重要的兼容保护之一。实现时应把映射放在 Adapter 或明确的关系/注册表边界，不把多种 ID 再压回一个“通用 ID”字段。

## 7. 边界图与访问方向

设计文档没有定义运行时 System，因此下面是根据其字段归属整理出的边界说明，而不是已经存在的调度图：

```text
Player / NPC / Projectile / ItemUse / Environment
             |
             | 只读战斗输入、旧字段 Adapter、EntityProvenance
             v
Combat 状态组件
  Health / Regen / Defense / Reduction
  Acceptance / Immunity / Knockback
  StatusEffectSlots / StatusEffectImmunity
             |
             | 待实现：资格、结算、生命周期提交
             v
死亡、掉落、复仇、世界进度、网络、存档、表现

Boss/Encounter root
             |
             v
EncounterDamageCreditComponent
             |
             | 只读贡献事实
             v
奖励、Boss/入侵进度、归因与投影
```

应保持的访问规则：

- `Player`、`NPC`、`Projectile` 和环境来源提供输入或通过 Adapter 构造命令，不直接改写其他实体的生命和状态；
- Combat 组件的私有数组只能由明确的状态变更 owner 修改；外部调用方拿到只读视图；
- Entity provenance 可被 Combat 读取，但 Combat 不独占其生命周期；
- Encounter credit 只累计已接受、已裁剪的伤害，不能从原始请求或被拒绝命中推导贡献；
- 网络、存档和客户端表现只消费已提交快照，不能反向成为组件权威写者。

## 8. 实现前必须定义的最小接口与测试 seam

源设计没有给出接口，本节只列实现前应补齐的 seam，不把它们宣称为已经存在的 API。

| Seam | 最小职责 | 读写限制 | 推荐验证替身 |
|---|---|---|---|
| `HealthStateWriter` | 以一次受控结果更新 `Current`，并返回旧值/新值/版本 | 只能写 `HealthComponent`；不能直接切换死亡生命周期 | 记录写入次数、旧值和新值的 fake writer |
| `ImmunityWindowStore` | 按稳定键新增、刷新、衰减和删除窗口 | 只拥有 `_windows`；不得修改 Health 或 Status 槽 | 确定性排序的内存 store |
| `StatusEffectSlotStore` | 应用、刷新、淘汰、压缩和过期槽位 | 只拥有槽位数组和 `Revision` | 固定容量、可审计的 recorder |
| `ProvenanceResolver` | 解析来源链和销毁后的引用状态 | 只读 Entity 关系；不把槽位索引当稳定身份 | 已销毁/槽位重用的 fake entity registry |
| `EncounterCreditWriter` | 合并已接受伤害并维护生命周期/版本 | 只写遭遇根账本；不写世界进度或 NPC 生命周期 | 可重复提交、溢出和排序 recorder |
| `LegacyStateAdapter` | 在旧数组/哨兵值与新封装状态间转换 | 兼容字段只在边界出现；禁止双写 | golden state/round-trip fixtures |
| `CommittedSnapshotSource` | 输出已提交只读状态给网络/存档/表现 | 只读；不能回写 Combat 组件 | immutable snapshot fake |

这些 seam 仍缺少最终接口名称、参数、错误类型和事务模型。真正实施前应先由整合设计裁决唯一写者和提交根，再把 seam 固化为源码 API。

## 9. 系统顺序与未定义行为

因为输入设计明确不包含 System 和调度，本报告不能把任何顺序写成已实现事实。实现评审至少要显式回答以下顺序问题：

1. 伤害输入在何处冻结攻击方、受击方、定义、来源和属性快照；
2. 资格查询是否完全无写入，拒绝命中是否推进免疫或再生计时；
3. 防御、减伤、暴击、随机数和伤害裁剪的先后；
4. 生命写入、免疫窗口写入、Buff 应用和击退命令的原子性边界；
5. 贡献登记相对于生命扣除和死亡判定的顺序；
6. `HealthComponent.IsDepleted` 何时触发 Player/NPC 各自的死亡生命周期；
7. 死亡事实何时交给掉落、复仇、世界进度、网络和表现；
8. 只有哪个提交版本可以生成存档和复制快照。

建议的实现验收顺序是“输入冻结 -> 资格 -> 数值结算 -> 权威状态提交 -> 贡献登记 -> 死亡转换 -> 下游事实 -> 投影”，但这只是待整合设计确认的候选顺序，不是本设计文档已经定义的行为。

## 10. 未决事项和设计内部不一致

### 10.1 `EncounterDamageCreditComponent` 是否属于 Combat 目录

文档第 1 节明确写道：`EncounterDamageCreditComponent` “归入 `Combat`”，并要求附着在独立遭遇根实体；但第 13 节结尾又写“目标实现的 Combat 目录只新增或演进前九个 Combat 组件”。按前面的组件表，前九项到 `StatusEffectImmunityComponent` 为止，遭遇贡献组件不在其中。

这会影响：

- 目标文件路径和目录归属；
- Combat 与 Boss/入侵/掉落系统的依赖方向；
- 贡献账本是否进入本轮实现批次；
- “组件数量”统计和验收清单。

建议在实现前采用以下明确表述之一：

- 若包含：改为“Combat 域包含十个组件候选，其中九个附着于受击实体，一个附着于遭遇根实体”；
- 若暂缓：把 `EncounterDamageCreditComponent` 标记为后续 Attribution 批次，并在总览中注明“Combat 归属但不属于本轮实体组件实现”。

### 10.2 设计状态与实现状态混用风险

文档声明所有“最终”只表示目标 ECS 字段归属，不表示已实现或迁移完成。报告应继续沿用这一口径。特别是现有名称 `HealthComponent`、`DefenseComponent`、`ImmunityComponent` 和 `HealthRegenerationStateComponent` 即使在 `src/Combat` 中存在，也不能据此推断它们已经具备文档中的完整字段集合或正确写入行为。

### 10.3 `ImmunityComponent` 的集合不变量

`HasActiveWindow` 依赖空窗口已经被清除，但文档没有定义构造、清理和每 tick 衰减系统。若这个不变量不被写成接口契约，派生属性可能在集合含有 `RemainingTicks == 0` 时错误返回 `true`。

### 10.4 `StatusEffectSlotsComponent` 的槽位容量

文档给出了 Player 44、NPC 20 的源码观察，但组件本身只通过数组长度表达容量。需要在初始化边界明确实体类型到容量的映射；否则 `Capacity` 虽是派生值，仍可能出现不同实体用错容量的结构性错误。

### 10.5 `DefenseComponent.Value` 的权威/派生措辞

设计把 `Value` 描述为“权威、派生于本 tick 属性重算”。这可能表示“当前 tick 的权威快照”，也可能表示“可由装备/效果重新计算的缓存”。两种解释对持久化、网络同步和伤害请求冻结时点不同，必须在实现前统一状态分类。

## 11. 不拆分项与保持策略

设计文档列出以下不应转成长生命周期组件的对象，本报告确认这些限制：

| 不拆分对象 | 保留边界 | 原因 |
|---|---|---|
| `DamageIntent`、`DamageResolution`、`DamageResolvedFact`、`DamageAttributionSnapshot` | 单次命令/不可变快照 | 每次命中独有，挂在实体上会产生覆盖和重复结算风险。 |
| 投射物伤害、击退、owner、penetrate、轨迹和寿命 | Projectile | 随投射物实例生命周期变化，Combat 只消费冻结输入。 |
| 玩家/物品攻击伤害、暴击、穿甲 | PlayerGameplay/ItemGameplay | 属于攻击方能力，不是受击目标的长期状态。 |
| `dead`、复活倒计时、NPC `active`、掉落、墓碑 | 各自生命周期/生成掉落边界 | 生命耗尽不等于死亡或结构销毁。 |
| CombatText、音效、粉尘、粒子、闪烁 alpha | Client presentation | 只消费已提交事实，不成为权威状态。 |
| `PlayerDeathReason` 二进制字段、网络包、旧数组槽位 | Adapter/Projection | 维护兼容格式，但不成为 ECS 身份真值。 |
| Buff 定义、互斥分组、时长上限、难度倍率 | `StatusEffectDefinitionCatalog` | 全局只读内容，不按实体复制。 |

这组排除项能防止“为了覆盖所有旧字段”而回流出巨型组件。后续若某一排除项被重新纳入，必须同时给出独立生命周期、权威写者和跨域边界证据。

## 12. 兼容性与迁移风险

### 12.1 兼容策略

基于设计文档，后续实现应遵循以下策略：

1. 保留旧字段/数组/哨兵值在边界 Adapter 中的映射，不把它们直接作为新的长期身份或公开可写集合；
2. `ExpectedLossPerSecond` 的 `-1` 与 `null` 建立显式双向转换；
3. Player/NPC Buff 槽位保留可观察顺序，Player 44 与 NPC 20 的容量由初始化契约固定；
4. 命中免疫从旧数组转换为稳定键窗口时保留作用域、来源和剩余 tick 语义；
5. 贡献账本使用规范化账户 UUID，禁止用玩家槽位或 `255` 作为持久贡献者身份；
6. 所有 `=>` 派生属性不单独持久化，不作为网络入站写入目标；
7. 先建立只读兼容外观和 golden fixtures，再切换唯一写者，禁止旧路径和新组件长期双写。

### 12.2 风险登记

| 风险 | 级别 | 触发条件 | 缓解要求 |
|---|---|---|---|
| 双写导致状态漂移 | 高 | 旧 `Hurt`/`StrikeNPC`/Buff 数组与新组件同时写入 | 明确单一写者，旧路径只做 Adapter，并记录迁移开关。 |
| 生命/死亡边界混淆 | 高 | `IsDepleted` 被直接当作完整死亡处理 | 分离 Health 提交、Player/NPC 生命周期和下游死亡事实。 |
| 免疫键不稳定 | 高 | 排序或 EntityReference 代际未定义 | 固定键规范、代际语义和集合不变量，并做重用场景测试。 |
| Buff 槽位行为改变 | 高 | 容量、淘汰、压缩或刷新顺序改变 | 用 44/20 fixtures 和逐 tick 状态 trace 比对旧语义。 |
| 归因身份错误 | 高 | 贡献账本使用槽位、网络 ID 或可变名称 | 只使用规范化账户 UUID/World 身份，单独测试重连和槽位复用。 |
| 遭遇账本归属漂移 | 中 | 组件附着 NPC 而非遭遇根，或 realLife/多节 Boss 映射不明 | 先定义遭遇根和成员关系，再允许贡献提交。 |
| 表现/协议反向写权威状态 | 高 | 网络包、存档 DTO 或 CombatText 直接改组件 | 只允许 committed snapshot 出站，入站转为验证后的命令。 |
| `Defense.Value` 快照时点错误 | 中 | 属性重算与命中结算并发/跨 tick | 明确冻结时点、版本和无效化规则。 |
| `EncounterId` 不可恢复 | 中 | 重启、保存/加载或重复遭遇使用相同 ID | 明确分配域、持久化策略和版本迁移。 |

## 13. 验证计划与当前结果

### 13.1 计划中的 focused verifier

以下是把本设计转入实现时的最小验证集合，当前均未创建或运行：

| 验证项 | 核心断言 | 当前状态 |
|---|---|---|
| `COMBAT-COMPONENT-SHAPE` | 11 个顶层组件名称、字段归属、私有集合和派生属性与设计一致 | 未运行 |
| `COMBAT-HEALTH-LIFECYCLE` | 生命耗尽不直接替代死亡；治疗/伤害边界和上下限明确 | 未运行 |
| `COMBAT-IMMUNITY-KEY` | 免疫键排序稳定、来源销毁安全、零窗口清理、Revision 可审计 | 未运行 |
| `COMBAT-STATUS-SLOTS` | Player/NPC 容量、刷新、淘汰、压缩和旧数组顺序保持 | 未运行 |
| `COMBAT-PROVENANCE-ID` | EntityReference、账户 UUID、网络 ID 和旧槽位不互相冒充 | 未运行 |
| `COMBAT-ENCOUNTER-CREDIT` | 只累计已接受伤害，贡献合并/排序/关闭/过期可重复 | 未运行 |
| `COMBAT-LEGACY-ROUNDTRIP` | 哨兵值、旧槽位、免疫数组和贡献身份转换可往返且不丢语义 | 未运行 |
| `COMBAT-WRITE-ROOT` | 每个权威组件有唯一写者，投影和 Adapter 不能反向写入 | 未运行 |

### 13.2 本次实际验证

本次工作是文档读取与报告生成，没有运行任何 compile-capable 命令，因此没有构建或测试的退出码、警告/错误计数，也没有新的 `Build/bin/` 产物可报告。

已完成的非编译检查：

- 已确认用户指定输入路径不存在；
- 已确认替代设计文件存在并完整读取，共 454 行；
- 已核对设计文档的顶层组件表、字段代码块、排除项和未决表述；
- 已新增本报告文件，未修改现有源码、测试或设计文件。

因此本报告状态保持：

```text
verificationStatus: not-run
implementationStatus: design-only
behavioralEquivalence: unverified
apiCompatibility: unverified
```

## 14. 交付结论

### 已形成的设计结论

- 生命、再生、防御、百分比减伤、受伤准入、命中免疫、击退和状态效果具有可分辨的状态边界；
- 状态效果槽位初期保留有序容器，不把每个 Buff 机械实体化；
- 生命耗尽与死亡生命周期分离；
- 因果来源属于 Entity 通用能力，不由 Combat 独占；
- 遭遇贡献附着于独立遭遇根，不附着于可能重用的 NPC 槽位；
- 派生值、网络/存档对象、表现数据、旧数组槽位和多种身份 ID 不成为新的权威状态源。

### 尚不能声称的事项

- 不能声称这些组件已经全部在 `src/` 中实现；
- 不能声称 Version4 行为已迁移或等价；
- 不能声称死亡、Buff、免疫、归因和遭遇贡献已经由唯一写者闭合；
- 不能声称网络、存档、客户端表现或公开 API 兼容；
- 不能声称构建、测试或 verifier 已通过。

### 建议的下一步

先裁决第 10 节的组件统计/目录归属、唯一写者和初始化契约，再为免疫窗口、Buff 槽位和遭遇贡献建立 focused verifier；完成这些契约后，才适合进入 C# 迁移或代码草稿实现。

## 15. Integration Handoff

```text
subsystemId: CombatAndStatus
sourceRequested: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-combat-and-status-component-design.md
sourceRequestedStatus: missing
sourceUsed: D:\TRbackup\NLTX\docs\Version4战斗状态与归因系统组件设计.md
reportPath: D:\TRbackup\NLTX\docs\第一轮审查\设计\2026-09-06-version4-combat-and-status-component-design-report.md

designScope: component-fields-and-derived-properties
topLevelComponentCount: 11
combatEntityComponents: 9
entityCrossDomainComponent: EntityProvenanceComponent
combatEncounterComponent: EncounterDamageCreditComponent
implementationStatus: design-only
verificationStatus: not-run
subagents: none

confirmedByDesignDocument:
- HealthComponent separates depleted health from death lifecycle.
- Defense and percentage damage reduction remain separate inputs.
- Non-cooldown acceptance policy is separate from hit immunity windows.
- Buff slots remain ordered and bounded; status immunity is a separate definition-indexed map.
- Entity provenance is not owned exclusively by Combat.
- Encounter damage credit is attached to an encounter root, not a reusable NPC slot.
- Derived properties and read-only views are not independent persisted/network authority.

unresolved:
- requested source path is absent;
- unique writer and commit root for every authority component;
- EntityReference lifecycle and stable ordering key;
- immunity key combinations, zero-window invariant and Revision policy;
- Player/NPC status slot initialization and legacy array round-trip;
- definition catalog capacity and status immunity rebuild;
- EncounterId allocation and encounter-root membership;
- Combat directory/count wording for EncounterDamageCreditComponent;
- persistence, replication, death lifecycle and scheduler contracts.

notImplemented:
- no Component, System, Query, Command, Adapter, Projection, test or project source was added;
- no existing source, test or design file was modified.

verificationPlan:
- COMBAT-COMPONENT-SHAPE
- COMBAT-HEALTH-LIFECYCLE
- COMBAT-IMMUNITY-KEY
- COMBAT-STATUS-SLOTS
- COMBAT-PROVENANCE-ID
- COMBAT-ENCOUNTER-CREDIT
- COMBAT-LEGACY-ROUNDTRIP
- COMBAT-WRITE-ROOT
```
