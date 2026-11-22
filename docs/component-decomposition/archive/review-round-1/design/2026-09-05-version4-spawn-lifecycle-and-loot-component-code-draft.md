# Version4 Spawn Lifecycle and Loot 实际代码组件草案

本文件把 Version4 Spawn Lifecycle and Loot Component-only Design 收敛为可落地的 C# 组件代码草案。

来源设计：
D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-spawn-lifecycle-and-loot-component-design.md

本文件仍然是设计草案，不是已提交源码。代码块中的路径是建议路径；本轮没有在 src/ 或 dome/src/ 下创建这些文件。

## 1. 草案元数据

| 字段 | 值 |
|---|---|
| artifactType | Component-code-draft |
| sourceDesign | D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-spawn-lifecycle-and-loot-component-design.md |
| designStatus | decision-required |
| codeStatus | proposed |
| evidenceStatus | partial |
| nltxStatus | missing |
| verificationStatus | not-run |
| canonicalComponentCount | 8 |
| proposedCanonicalComponentCount | 8 |
| sourceFilesCreated | 0 |
| subagentsStarted | 0 |

### 1.1 草案使用规则

- 8 个 canonical Component 都以 status: proposed 对待；代码中的 proposed 注释是设计状态，不是运行时标记。
- 现有 NLTX 类型只作为依赖或映射参考，不在本文件中改名、删除或替换。
- 组件只保存内聚的实体状态；不在组件中加入 System、Query、Command、Event、Adapter、Projection、调度或外部 I/O。
- 组件构造函数负责可在本地确定的不变量；跨组件不变量仍属于未决 owner 的评审范围。
- 为了避免把候选接口误报成已验证 API，所有建议路径、命名空间和类型均需要后续在具体项目边界中确认。
- 本轮不运行构建、测试或其他 compile-capable 命令，因此代码块没有编译通过声明。

## 2. 建议文件布局

遵循领域优先和一核心公开类型一文件的规则，建议将代码分布如下：

| 建议路径 | 核心类型 | 状态 |
|---|---|---|
| src/Entity/EntityId.cs | EntityId | proposed supporting value type |
| src/Entity/LifecyclePhase.cs | LifecyclePhase | proposed supporting enum |
| src/Entity/TerminationReason.cs | TerminationReason | proposed supporting enum |
| src/Entity/EntityLifecycleState.cs | EntityLifecycleState | proposed canonical Component |
| src/Entity/SpawnAdmissionStatus.cs | SpawnAdmissionStatus | proposed supporting enum |
| src/Entity/SpawnSourceKind.cs | SpawnSourceKind | proposed supporting enum |
| src/Entity/SpawnAuthorityKind.cs | SpawnAuthorityKind | proposed supporting enum |
| src/Entity/SpawnRejectionReason.cs | SpawnRejectionReason | proposed supporting enum |
| src/Entity/SpawnAdmissionState.cs | SpawnAdmissionState | proposed canonical Component |
| src/Entity/NetworkEntityId.cs | NetworkEntityId | proposed supporting value type |
| src/Entity/EntityIdentityState.cs | EntityIdentityState | proposed canonical Component |
| src/Entity/EntityRelationKind.cs | EntityRelationKind | proposed supporting enum |
| src/Entity/EntityRelationState.cs | EntityRelationState | proposed canonical Component |
| src/Npc/NpcDefinitionReferenceState.cs | NpcDefinitionReferenceState | proposed canonical Component |
| src/Npc/NpcPopulationLifetimeState.cs | NpcPopulationLifetimeState | proposed canonical Component |
| src/Items/Loot/LootCommitState.cs | LootCommitState | proposed supporting enum |
| src/Items/Loot/LootResolutionState.cs | LootResolutionState | proposed canonical Component |
| src/Items/Loot/LootAttributionState.cs | LootAttributionState | proposed canonical Component |

目录选择说明：

- 通用实体生命周期、接纳、身份和关系属于明确的 Entity 能力，建议归入 src/Entity/。
- NPC 类型引用和 NPC 人口寿命只属于 NPC 领域，建议归入 src/Npc/。
- 掉落解析和掉落归因属于 Items/Loot 能力，建议归入 src/Items/Loot/。
- 不创建 src/Shared/Components/、src/Common/ 或 src/Misc/ 作为无法归属类型的收容目录。
- 本文件中的路径是草案路径，不表示目录移动已经执行，也不改变现有命名空间。

## 3. C# 类型映射决策

| 设计字段语义 | 草案 C# 类型 | 选择理由 |
|---|---|---|
| RuntimeEntityId | EntityId | 让运行时实体身份与关系引用、兼容槽位和网络身份显式区分 |
| CompatibilitySlot | int? | 与现有槽位整数语义兼容；null 表示没有兼容槽位 |
| NetworkId | NetworkEntityId? | 避免把 NPC 类型 NetId 和实体网络身份混用 |
| PersistentId | EntityId? | 使用明确的实体 ID 值语义；是否分配由持久化 owner 决定 |
| Generation | uint | 代际编号不会为负；0 表示未使用代际区分 |
| Logical tick | long 或 long? | 对齐当前 NLTX 的 tick 字段；构造时拒绝负数 |
| Revision | long | 对齐现有组件；构造时拒绝负数，单调性由 owner 保证 |
| EligibleRecipients | IReadOnlySet<EntityReference> | 对外只读并表达去重；内部使用 HashSet |
| LootTableId | int | 对齐现有 LootSourceComponent；0 表示未关联掉落表 |
| Npc type | NpcTypeId | 重用当前 NLTX 类型，不用原始 int 隐藏定义域语义 |
| NPC definition NetId | NpcNetId | 重用当前 NLTX 类型，并明确它不是 NetworkEntityId |
| Source position | WorldPosition | 重用当前 NLTX 位置值类型，不创建新的位置 Component |

## 4. 通用 Entity supporting types

### 4.1 src/Entity/EntityId.cs

```csharp
using System;

namespace Terraria.Entity;

public readonly record struct EntityId(Guid Value)
{
  public static EntityId None => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
```

约束：

- EntityId.None 是未分配运行时或持久化身份的显式表示。
- EntityId 只表达一个身份值，不表达关系、网络传输或 NPC 类型。
- Guid.Empty 不得被解释为有效实体身份。

### 4.2 src/Entity/LifecyclePhase.cs

```csharp
namespace Terraria.Entity;

public enum LifecyclePhase : byte
{
  Uninitialized,
  Admitted,
  Active,
  Ending,
  Retired,
}
```

### 4.3 src/Entity/TerminationReason.cs

```csharp
namespace Terraria.Entity;

public enum TerminationReason : byte
{
  None,
  LifetimeExpired,
  ExplicitlyRemoved,
  Collision,
  ParentDestroyed,
  WorldUnload,
  DefinitionInvalid,
  CompatibilityRemoval,
}
```

## 5. EntityLifecycleState 代码草案

建议路径：src/Entity/EntityLifecycleState.cs

canonicalComponent：SLL-COMP-01

owner：entity-lifecycle-authority，未决

crossSubsystemOwner：integration-review

```csharp
using System;

namespace Terraria.Entity;

// status: proposed
public sealed class EntityLifecycleState
{
  public EntityLifecycleState(
    LifecyclePhase phase = LifecyclePhase.Uninitialized,
    bool isActive = false,
    TerminationReason terminationReason = TerminationReason.None,
    bool pendingCleanup = false,
    long revision = 0)
  {
    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    Phase = phase;
    IsActive = isActive;
    TerminationReason = terminationReason;
    PendingCleanup = pendingCleanup;
    Revision = revision;
    Validate();
  }

  public LifecyclePhase Phase { get; }

  public bool IsActive { get; }

  public TerminationReason TerminationReason { get; }

  public bool PendingCleanup { get; }

  public long Revision { get; }

  public void Validate()
  {
    if (Phase == LifecyclePhase.Active && !IsActive)
    {
      throw new InvalidOperationException(
        "An active entity must report IsActive = true.");
    }

    if (Phase != LifecyclePhase.Active && IsActive)
    {
      throw new InvalidOperationException(
        "Only the Active phase may report IsActive = true.");
    }

    if ((Phase == LifecyclePhase.Ending || Phase == LifecyclePhase.Retired)
      && TerminationReason == TerminationReason.None)
    {
      throw new InvalidOperationException(
        "Ending and Retired entities require a termination reason.");
    }

    if (PendingCleanup
      && Phase != LifecyclePhase.Ending
      && Phase != LifecyclePhase.Retired)
    {
      throw new InvalidOperationException(
        "Pending cleanup is only valid after termination has started.");
    }
  }
}
```

代码决策：

- 使用 sealed class，避免 ECS 组件被无意继承并重新引入行为层。
- 属性为只读，状态变化草案采用替换整个组件值的方式，避免通过任意 setter 绕过跨字段不变量。
- Validate() 只检查本组件内可确定的不变量，不负责清理实体、发送同步消息或触发掉落。
- Revision 的单调递增不能由单个不可变实例证明；owner 必须在替换组件时保证它不倒退。

## 6. SpawnAdmissionState supporting types

### 6.1 src/Entity/SpawnAdmissionStatus.cs

```csharp
namespace Terraria.Entity;

public enum SpawnAdmissionStatus : byte
{
  NotRequested,
  Pending,
  Admitted,
  Rejected,
  Cancelled,
}
```

### 6.2 src/Entity/SpawnSourceKind.cs

```csharp
namespace Terraria.Entity;

public enum SpawnSourceKind : byte
{
  Unknown,
  Direct,
  Statue,
  Replacement,
  DespawnReplacement,
  External,
}
```

### 6.3 src/Entity/SpawnAuthorityKind.cs

```csharp
namespace Terraria.Entity;

public enum SpawnAuthorityKind : byte
{
  Unknown,
  Server,
  World,
  Compatibility,
  External,
}
```

### 6.4 src/Entity/SpawnRejectionReason.cs

```csharp
namespace Terraria.Entity;

public enum SpawnRejectionReason : byte
{
  None,
  CapacityReached,
  InvalidDefinition,
  InvalidPosition,
  DuplicateAdmission,
  AuthorityDenied,
  Cancelled,
}
```

## 7. SpawnAdmissionState 代码草案

建议路径：src/Entity/SpawnAdmissionState.cs

canonicalComponent：SLL-COMP-02

owner：spawn-admission-authority，未决

crossSubsystemOwner：integration-review

```csharp
using System;

namespace Terraria.Entity;

// status: proposed
public sealed class SpawnAdmissionState
{
  public SpawnAdmissionState(
    SpawnAdmissionStatus status = SpawnAdmissionStatus.NotRequested,
    SpawnSourceKind sourceKind = SpawnSourceKind.Unknown,
    string? admissionKey = null,
    SpawnAuthorityKind authority = SpawnAuthorityKind.Unknown,
    uint attemptCount = 0,
    SpawnRejectionReason rejectionReason = SpawnRejectionReason.None)
  {
    Status = status;
    SourceKind = sourceKind;
    AdmissionKey = admissionKey;
    Authority = authority;
    AttemptCount = attemptCount;
    RejectionReason = rejectionReason;
    Validate();
  }

  public SpawnAdmissionStatus Status { get; }

  public SpawnSourceKind SourceKind { get; }

  public string? AdmissionKey { get; }

  public SpawnAuthorityKind Authority { get; }

  public uint AttemptCount { get; }

  public SpawnRejectionReason RejectionReason { get; }

  public void Validate()
  {
    bool isRejected = Status == SpawnAdmissionStatus.Rejected
      || Status == SpawnAdmissionStatus.Cancelled;

    if (isRejected && RejectionReason == SpawnRejectionReason.None)
    {
      throw new InvalidOperationException(
        "Rejected or cancelled admission requires a rejection reason.");
    }

    if (!isRejected && RejectionReason != SpawnRejectionReason.None)
    {
      throw new InvalidOperationException(
        "A rejection reason is only valid for rejected or cancelled admission.");
    }

    if (AdmissionKey is not null && string.IsNullOrWhiteSpace(AdmissionKey))
    {
      throw new ArgumentException(
        "An admission key must contain non-whitespace characters.",
        nameof(AdmissionKey));
    }

    if (Status == SpawnAdmissionStatus.Admitted
      && Authority == SpawnAuthorityKind.Unknown)
    {
      throw new InvalidOperationException(
        "An admitted candidate requires a known authority.");
    }
  }
}
```

代码决策：

- AttemptCount 是无符号计数；代码没有提供重置方法，避免把同一次接纳伪装成新接纳。
- AdmissionKey 只识别接纳意图，不作为 RuntimeEntityId、NetworkId 或掉落幂等键。
- WorldItem 候选可以使用此组件，但并不因此把 WorldItem 纳入 NPC 或 Projectile 组件组合。
- 生成规则本身不进入组件；组件只记录候选的状态和来源分类。

## 8. EntityIdentityState supporting types

### 8.1 src/Entity/NetworkEntityId.cs

```csharp
namespace Terraria.Entity;

public readonly record struct NetworkEntityId(int Value)
{
  public static NetworkEntityId None => new(-1);

  public bool IsAssigned => Value >= 0;
}
```

说明：

- NetworkEntityId? 的 null 是默认未分配表示；NetworkEntityId.None 只用于需要显式值的边界转换。
- 当前 NpcNetId 不应直接转换成 NetworkEntityId，因为前者属于 NPC 定义标识。
- -1 只在 NetworkEntityId.None 内部使用；正常已分配网络身份必须满足 Value >= 0。

## 9. EntityIdentityState 代码草案

建议路径：src/Entity/EntityIdentityState.cs

canonicalComponent：SLL-COMP-03

owner：entity-identity-authority，未决

crossSubsystemOwner：integration-review

```csharp
using System;

namespace Terraria.Entity;

// status: proposed
public sealed class EntityIdentityState
{
  public EntityIdentityState(
    EntityId runtimeEntityId = default,
    int? compatibilitySlot = null,
    NetworkEntityId? networkId = null,
    EntityId? persistentId = null,
    uint generation = 0)
  {
    if (compatibilitySlot is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(compatibilitySlot));
    }

    if (networkId.HasValue && !networkId.Value.IsAssigned)
    {
      throw new ArgumentOutOfRangeException(nameof(networkId));
    }

    if (persistentId.HasValue && !persistentId.Value.IsAssigned)
    {
      throw new ArgumentOutOfRangeException(nameof(persistentId));
    }

    if (!compatibilitySlot.HasValue && generation != 0)
    {
      throw new InvalidOperationException(
        "Generation requires a compatibility slot.");
    }

    RuntimeEntityId = runtimeEntityId;
    CompatibilitySlot = compatibilitySlot;
    NetworkId = networkId;
    PersistentId = persistentId;
    Generation = generation;
  }

  public EntityId RuntimeEntityId { get; }

  public int? CompatibilitySlot { get; }

  public NetworkEntityId? NetworkId { get; }

  public EntityId? PersistentId { get; }

  public uint Generation { get; }

  public bool HasRuntimeIdentity => RuntimeEntityId.IsAssigned;

  public bool HasCompatibilityIdentity =>
    CompatibilitySlot.HasValue;

  public bool HasNetworkIdentity => NetworkId.HasValue;

  public bool HasPersistentIdentity =>
    PersistentId.HasValue;
}
```

代码决策：

- 运行时身份、兼容槽位、网络身份、持久化身份和代际编号是五个不同字段。
- RuntimeEntityId 允许使用 EntityId.None 构造未分配阶段；是否允许实体挂载未分配身份需要由 owner 决定。
- Generation 只有在存在兼容槽位时才有意义；槽位复用时必须由 owner 提供递增代际。
- PersistentId 是实体持久化身份，不替代现有 ItemInstanceComponent.PersistentInstanceId；两个类型的统一仍是 evidence-gap。
- 本组件不携带实体类型、owner、parent、网络 dirty 或持久化快照字段。

## 10. EntityRelationState supporting type

### 10.1 src/Entity/EntityRelationKind.cs

```csharp
namespace Terraria.Entity;

public enum EntityRelationKind : byte
{
  None,
  Owner,
  Parent,
  LinkedLife,
  Source,
}
```

## 11. EntityRelationState 代码草案

建议路径：src/Entity/EntityRelationState.cs

canonicalComponent：SLL-COMP-04

owner：entity-relation-authority，未决

crossSubsystemOwner：integration-review

```csharp
using System;
using Terraria.Relationships;

namespace Terraria.Entity;

// status: proposed
public sealed class EntityRelationState
{
  public EntityRelationState(
    EntityReference relatedEntity = default,
    EntityRelationKind relationKind = EntityRelationKind.None,
    long expectedRevision = 0,
    long? attachedAtTick = null)
  {
    if (expectedRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(expectedRevision));
    }

    if (attachedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(attachedAtTick));
    }

    RelatedEntity = relatedEntity;
    RelationKind = relationKind;
    ExpectedRevision = expectedRevision;
    AttachedAtTick = attachedAtTick;
    Validate();
  }

  public EntityReference RelatedEntity { get; }

  public EntityRelationKind RelationKind { get; }

  public long ExpectedRevision { get; }

  public long? AttachedAtTick { get; }

  public bool IsAttached =>
    RelationKind != EntityRelationKind.None
    && !RelatedEntity.IsEmpty;

  public bool IsDetached => !IsAttached;

  public void Validate()
  {
    if (RelationKind == EntityRelationKind.None
      && !RelatedEntity.IsEmpty)
    {
      throw new InvalidOperationException(
        "A relation kind is required for a non-empty entity reference.");
    }

    if (RelationKind != EntityRelationKind.None
      && RelatedEntity.IsEmpty)
    {
      throw new InvalidOperationException(
        "An attached relation requires a related entity.");
    }
  }
}
```

代码决策：

- EntityReference 继续使用当前 NLTX 的 Guid EntityId + EntityReferenceScope。
- RelatedEntity 表达“关联谁”，不表达当前实体是谁。
- ExpectedRevision 只用于关系一致性检查候选，不是 RuntimeEntityId、Generation 或执行顺序。
- 一个实体需要多个并行关系时，不能把关系类别编码成字符串或无类型集合；本轮不新增多关系组件。

## 12. NpcDefinitionReferenceState 代码草案

建议路径：src/Npc/NpcDefinitionReferenceState.cs

canonicalComponent：SLL-COMP-05

owner：npc-definition-domain

```csharp
using System;

namespace Terraria.Npc;

// status: proposed
public sealed class NpcDefinitionReferenceState
{
  public NpcDefinitionReferenceState(
    NpcTypeId typeId = default,
    NpcNetId netId = default,
    int catalogRevision = 0,
    NpcTypeId? initialTypeId = null)
  {
    if (catalogRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(catalogRevision));
    }

    TypeId = typeId;
    NetId = netId;
    InitialTypeId = initialTypeId ?? typeId;
    CatalogRevision = catalogRevision;
    Validate();
  }

  public NpcTypeId TypeId { get; }

  public NpcNetId NetId { get; }

  public NpcTypeId InitialTypeId { get; }

  public int CatalogRevision { get; }

  public bool UsesNetIdVariant => NetId.IsVariant;

  public bool IsInitialized =>
    TypeId.IsValid && InitialTypeId.IsValid;

  public void Validate()
  {
    if (TypeId.IsValid && !InitialTypeId.IsValid)
    {
      throw new InvalidOperationException(
        "An assigned type requires an assigned initial type.");
    }
  }
}
```

代码决策：

- 重用当前 NpcTypeId、NpcNetId，不复制它们的值对象定义。
- InitialTypeId 在构造时固定，避免重分类后丢失初始定义。
- CatalogRevision 是定义目录版本，不是 EntityLifecycleState.Revision。
- NpcNetId 仍然是定义域标识；它不能填入 EntityIdentityState.NetworkId。
- 本组件不包含 NPC 生命值、AI、掉落规则、兼容槽位或网络 dirty 状态。

## 13. NpcPopulationLifetimeState 代码草案

建议路径：src/Npc/NpcPopulationLifetimeState.cs

canonicalComponent：SLL-COMP-06

owner：npc-population-domain，未决

crossSubsystemOwner：integration-review

```csharp
using System;

namespace Terraria.Npc;

// status: proposed
public sealed class NpcPopulationLifetimeState
{
  public NpcPopulationLifetimeState(
    long remainingTicks = 0,
    float populationSlotCost = 0.0f,
    bool countsAgainstPopulation = false,
    bool despawnEncouraged = false,
    NpcDespawnReason pendingDespawnReason = NpcDespawnReason.None)
  {
    if (remainingTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingTicks));
    }

    if (populationSlotCost < 0.0f
      || float.IsNaN(populationSlotCost)
      || float.IsInfinity(populationSlotCost))
    {
      throw new ArgumentOutOfRangeException(nameof(populationSlotCost));
    }

    if (!countsAgainstPopulation && populationSlotCost > 0.0f)
    {
      throw new InvalidOperationException(
        "An entity outside population counting cannot consume a slot cost.");
    }

    RemainingTicks = remainingTicks;
    PopulationSlotCost = populationSlotCost;
    CountsAgainstPopulation = countsAgainstPopulation;
    DespawnEncouraged = despawnEncouraged;
    PendingDespawnReason = pendingDespawnReason;
  }

  public long RemainingTicks { get; }

  public float PopulationSlotCost { get; }

  public bool CountsAgainstPopulation { get; }

  public bool DespawnEncouraged { get; }

  public NpcDespawnReason PendingDespawnReason { get; }

  public bool IsExpired => RemainingTicks == 0;

  public float EffectivePopulationCost =>
    CountsAgainstPopulation ? PopulationSlotCost : 0.0f;
}
```

代码决策：

- float PopulationSlotCost 保留当前 NpcLifetimeComponent 的数值形态，因为人口成本可能不是整数。
- RemainingTicks == 0 只表示没有剩余额度；它不自动宣告生命周期进入 Ending。
- PendingDespawnReason 使用现有 NpcDespawnReason，但它不等同于 TerminationReason。
- Projectile 的 timeLeft 不挂载此 NPC 专属组件。
- 本组件不包含通用 IsActive、兼容槽位、网络身份或掉落解析字段。

## 14. Loot supporting type

### 14.1 src/Items/Loot/LootCommitState.cs

```csharp
namespace Terraria.Items.Loot;

public enum LootCommitState : byte
{
  Unresolved,
  Resolving,
  Resolved,
  Committed,
  Skipped,
  Failed,
}
```

说明：

- LootCommitState 表达一次来源实体的掉落解析/提交状态，不表达 WorldItem 是否已被拾取。
- Committed 只允许出现在 HasResolvedLoot == true 且 ResolutionSequence > 0 时。
- Failed 与 Skipped 是结果状态，不提供重试算法或调度语义。

## 15. LootResolutionState 代码草案

建议路径：src/Items/Loot/LootResolutionState.cs

canonicalComponent：SLL-COMP-07

owner：loot-resolution-domain，未决

crossSubsystemOwner：integration-review

```csharp
using System;

namespace Terraria.Items.Loot;

// status: proposed
public sealed class LootResolutionState
{
  public LootResolutionState(
    int lootTableId = 0,
    LootSourceKind sourceKind = LootSourceKind.Unknown,
    bool hasResolvedLoot = false,
    long resolutionSequence = 0,
    long? resolvedAtTick = null,
    string? resolutionKey = null,
    LootCommitState commitState = LootCommitState.Unresolved)
  {
    if (lootTableId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lootTableId));
    }

    if (resolutionSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(resolutionSequence));
    }

    if (resolvedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(resolvedAtTick));
    }

    if (resolutionKey is not null
      && string.IsNullOrWhiteSpace(resolutionKey))
    {
      throw new ArgumentException(
        "A resolution key must contain non-whitespace characters.",
        nameof(resolutionKey));
    }

    if (!hasResolvedLoot && commitState == LootCommitState.Committed)
    {
      throw new InvalidOperationException(
        "Unresolved loot cannot be committed.");
    }

    if (commitState == LootCommitState.Committed
      && resolutionSequence == 0)
    {
      throw new InvalidOperationException(
        "Committed loot requires a positive resolution sequence.");
    }

    LootTableId = lootTableId;
    SourceKind = sourceKind;
    HasResolvedLoot = hasResolvedLoot;
    ResolutionSequence = resolutionSequence;
    ResolvedAtTick = resolvedAtTick;
    ResolutionKey = resolutionKey;
    CommitState = commitState;
  }

  public int LootTableId { get; }

  public LootSourceKind SourceKind { get; }

  public bool HasResolvedLoot { get; }

  public long ResolutionSequence { get; }

  public long? ResolvedAtTick { get; }

  public string? ResolutionKey { get; }

  public LootCommitState CommitState { get; }
}
```

代码决策：

- LootTableId 对齐当前 LootSourceComponent 的 int 字段；0 表示未关联规则表。
- ResolutionKey 与 SpawnAdmissionState.AdmissionKey 分开，因为接纳意图和掉落解析意图有不同 owner。
- ResolutionSequence 是来源实体的解析序列，不是 LootAttributionState.AttributionRevision。
- 不复制 CommonDrop 的概率、数量或链式规则字段；它们属于规则定义层。
- 不把 WorldItem 的物品实例或预留状态放入本组件。

## 16. LootAttributionState 代码草案

建议路径：src/Items/Loot/LootAttributionState.cs

canonicalComponent：SLL-COMP-08

owner：loot-attribution-domain，未决

crossSubsystemOwner：integration-review

```csharp
using System;
using System.Collections.Generic;
using Terraria.Items;
using Terraria.Relationships;

namespace Terraria.Items.Loot;

// status: proposed
public sealed class LootAttributionState
{
  private readonly HashSet<EntityReference> _eligibleRecipients;

  public LootAttributionState(
    EntityReference killer = default,
    IReadOnlyCollection<EntityReference>? eligibleRecipients = null,
    EntityReference luckOwner = default,
    WorldPosition sourcePosition = default,
    long attributionRevision = 0)
  {
    if (attributionRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(attributionRevision));
    }

    _eligibleRecipients = eligibleRecipients is null
      ? new HashSet<EntityReference>()
      : new HashSet<EntityReference>(eligibleRecipients);

    if (_eligibleRecipients.Contains(EntityReference.None))
    {
      throw new ArgumentException(
        "Eligible recipients cannot contain EntityReference.None.",
        nameof(eligibleRecipients));
    }

    Killer = killer;
    LuckOwner = luckOwner;
    SourcePosition = sourcePosition;
    AttributionRevision = attributionRevision;
  }

  public EntityReference Killer { get; }

  public IReadOnlySet<EntityReference> EligibleRecipients =>
    _eligibleRecipients;

  public EntityReference LuckOwner { get; }

  public WorldPosition SourcePosition { get; }

  public long AttributionRevision { get; }

  public bool HasEligibleRecipients =>
    _eligibleRecipients.Count > 0;
}
```

代码决策：

- 内部使用 HashSet 保证 recipient 去重；对外只暴露 IReadOnlySet。
- Killer、LuckOwner 的空引用表示未知或不适用，不自动推断为世界实体。
- SourcePosition 是来源位置快照，不是 WorldItem 当前移动位置。
- LuckOwner 不由代码自动回退为 Killer。
- 本组件不表示掉落已解析或已提交；这由 LootResolutionState 独立表达。
- 本组件没有公开修改集合的方法，因此草案阶段不会绕过 recipient 去重不变量。

## 17. 8 个 canonical Component 的示例组合

以下只展示数据组合，不定义实体类、系统、查询或运行顺序。

| 实体语义 | 组件 |
|---|---|
| 生成候选 NPC | SpawnAdmissionState + EntityIdentityState + NpcDefinitionReferenceState + EntityLifecycleState |
| 活跃 NPC | EntityIdentityState + NpcDefinitionReferenceState + EntityLifecycleState + NpcPopulationLifetimeState |
| 具有关联关系的 NPC | 活跃 NPC 组合 + EntityRelationState |
| 生成候选 Projectile | SpawnAdmissionState + EntityIdentityState + EntityLifecycleState |
| 具拥有者的 Projectile | 生成候选 Projectile 组合 + EntityRelationState |
| 可掉落 NPC 来源 | NPC 组合 + LootAttributionState + LootResolutionState |
| 可掉落 Projectile 来源 | Projectile 组合 + LootAttributionState + LootResolutionState |
| WorldItem | 现有 WorldItemComponent + WorldItemReservationComponent + ItemInstanceComponent；按需保留来源引用 |

组合不变量：

- EntityLifecycleState 不替代 SpawnAdmissionState。
- EntityIdentityState 不替代 EntityRelationState。
- NpcDefinitionReferenceState 不替代 EntityIdentityState。
- NpcPopulationLifetimeState 不挂载到仅仅因为拥有 timeLeft 的 Projectile。
- LootResolutionState 不替代 LootAttributionState。
- LootAttributionState 不替代 WorldItemComponent、WorldItemReservationComponent 或 ItemInstanceComponent。

## 18. 与现有 NLTX 类型的映射

| 现有类型 | 草案关系 | 处理结论 |
|---|---|---|
| NpcLifecycleComponent | EntityLifecycleState 的 partial 覆盖 | 不删除；其 Stage、RemainingDespawnTicks 仍需与新状态字段逐一比对 |
| NpcLifetimeComponent | NpcPopulationLifetimeState 的 partial 覆盖 | 不删除；现有 IsExpired、EffectivePopulationCost 等派生属性不直接复制成权威字段 |
| NpcEntityIdentityComponent | EntityIdentityState 的 partial 覆盖 | 不删除；InstanceId 与 LegacySlot 的身份分类仍需确认 |
| NpcInstanceId | EntityId / 兼容身份的 partial 候选 | 不自动转换；Value 的全局唯一性与生命周期需补证 |
| NpcSlot | CompatibilitySlot 的 partial 候选 | 不自动转换；槽位复用必须补 Generation 证据 |
| NpcNetId | NpcDefinitionReferenceState.NetId 的 partial 候选 | 不转换为 NetworkEntityId |
| NpcDefinitionReferenceComponent | NpcDefinitionReferenceState 的 partial 覆盖 | 不删除；CatalogRevision 当前为 int，是否升级需另行决定 |
| NpcParentRelationComponent | EntityRelationState 的 partial 覆盖 | 不删除；parent identity 需从关系语义重新核对 |
| ProjectileLifetimeComponent | EntityLifecycleState 的 partial 覆盖 | 不删除；Projectile 专属 end reason 仍需保留其领域语义 |
| ProjectileOwnerComponent | EntityRelationState 的 partial 覆盖 | 不删除；owner 的引用 scope 和关联版本需补证 |
| LootSourceComponent | LootResolutionState 的 partial 覆盖 | 不删除；当前没有 ResolutionKey 和 CommitState |
| LootAttributionComponent | LootAttributionState 的 partial 覆盖 | 不删除；当前使用 IReadOnlyList，需要确认是否升级为去重集合 |
| WorldItemComponent | existing | 保留物品世界状态，不映射为掉落来源解析状态 |
| WorldItemReservationComponent | existing | 保留预留状态，不映射为 LootCommitState |
| ItemInstanceComponent | existing | 保留物品实例身份，不直接替代 EntityIdentityState.PersistentId |
| Relationships.EntityReference | existing supporting value | 作为关系和归因的引用值，不创建替代类型 |
| Items.WorldPosition | existing supporting value | 作为掉落归因的位置值，不创建通用位置组件 |
| Items.WorldVector | existing supporting value | 不纳入本轮 canonical Component |

## 19. 草案级证据和实现缺口

| Gap ID | 影响代码 | 当前缺口 | 代码草案处理 |
|---|---|---|---|
| BD-COMP-01 | EntityLifecycleState | 全实体生命周期阶段、TerminationReason 和 Revision 仍无统一证据 | 以只读属性和 Validate 表达候选不变量，保持 proposed |
| BD-COMP-02 | SpawnAdmissionState | 接纳 authority、AdmissionKey、重试和拒绝原因没有当前统一契约 | 构造时校验状态/原因一致性，不实现接纳行为 |
| BD-COMP-03 | EntityIdentityState、EntityId、NetworkEntityId | Runtime、兼容、网络、持久化 ID 的分配与复用边界未闭合 | 使用显式值类型和可空字段，保留 owner gap |
| BD-COMP-04 | EntityRelationState | 多关系 cardinality、RelationKind 和断开条件未闭合 | 只支持一个主要关系，禁止无类型关系集合 |
| BD-COMP-05 | NpcDefinitionReferenceState | 类型 NetId 与实例 NetworkId、目录版本和重分类契约未闭合 | 重用现有 NpcTypeId/NpcNetId，初始类型只读 |
| BD-COMP-06 | NpcPopulationLifetimeState | 人口槽位、RemainingTicks=0 和待终止原因语义未闭合 | 保留 float 槽位成本和显式布尔字段，不把 0 自动转为 Ending |
| BD-COMP-07 | LootResolutionState | 解析幂等键、序列和提交事实模型未闭合 | 独立 ResolutionKey、ResolutionSequence、CommitState |
| BD-COMP-08 | LootAttributionState | recipient、luck、未知归因、位置快照和 Revision 契约未闭合 | 使用只读去重集合，空引用保留为未知/不适用 |

## 20. 后续落地前的人工决策点

本节不是迁移计划，只记录代码落地前必须明确的决策。

- 是否将通用组件命名为 State，还是与当前 Component 后缀保持一致。
- 是否允许实体在 EntityLifecycleState 存在期间暂时没有 RuntimeEntityId。
- NetworkEntityId 的分配域、复用时机和协议兼容范围。
- PersistentId 与 ItemInstanceComponent.PersistentInstanceId 是否使用同一值类型。
- SpawnSourceKind 是否需要覆盖所有现有生成来源，还是保留 Unknown 作为兼容边界。
- LootSourceKind 是否需要新增 Projectile 或其他来源枚举值；本轮不修改现有枚举。
- LootCommitState 是否足以表达失败后的再次解析；本轮不提供重试行为。
- 当前 src 与 dome 的并行组件是否共享同一 canonical 字段契约；本轮不假定已经统一。
- 是否采用不可变组件替换模型；如果项目 ECS 要求原地修改，需要另行设计受控更新接口和验证边界。

## 21. 最终声明

本文件是实际 C# 组件代码草案，不是已提交的运行时代码。

本文件提出 8 个 proposed canonical Component 及其 supporting types，并给出建议文件路径、命名空间、字段、默认值、构造约束和本组件内不变量。代码块不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、网络协议、持久化格式、测试计划或迁移计划。

这些代码草案不表示源码已创建，不表示 src 与 dome 已统一，不表示行为等价，不表示已经编译、测试或验证通过。designStatus 保持 decision-required，evidenceStatus 为 partial，nltxStatus 为 missing，verificationStatus 为 not-run。

本轮只新增本 Markdown 草案，未修改 src/、dome/src/、测试、项目文件、研究报告或既有设计文件，未启动子代理，未运行构建或测试。
