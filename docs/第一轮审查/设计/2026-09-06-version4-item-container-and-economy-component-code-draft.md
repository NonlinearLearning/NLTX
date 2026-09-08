# Version4 ItemContainerAndEconomy 实际代码组件草案

## 1. 草案元数据

| 项目 | 内容 |
|---|---|
| subsystemId | ItemContainerAndEconomy |
| sourceDesign | D:/TRbackup/NLTX/docs/design/2026-09-05-version4-item-container-and-economy-component-design.md |
| outputPath | D:/TRbackup/NLTX/docs/design/2026-09-06-version4-item-container-and-economy-component-code-draft.md |
| draftScope | code-shape-only |
| componentCount | 14 |
| componentStatus | proposed |
| implementationStatus | not-created |
| compilationStatus | not-run |
| testStatus | not-run |
| ownerStatus | decision-required |
| sourceChanges | none |

本文件把已有 Component Design 转译为接近当前 NLTX 风格的 C# 类型草案。代码块按拟议
路径组织，但本文件没有创建任何 .cs 文件，也没有声明这些片段已经通过编译。

本草案只表达组件字段、集合封装、默认值、纯派生属性、强类型 ID 和兼容映射。
本草案不表达 System、Query、Command、Event、Adapter、Projection、调度顺序、
事务算法、存档/网络协议、测试实现或迁移步骤。

## 2. 代码形状约束

1. 每个 public 核心类型最终放入与类型同名的 PascalCase 文件。
2. 组件保持领域优先目录；拟议子目录只有在实际落地时才按文件规模和依赖边界创建。
3. 数量只由 ItemStackComponent 表达，容器内容只由 ContainerContentsComponent 表达，
   访问 lease 只由 ContainerAccessComponent 表达。
4. 构造函数复制输入集合，公开接口使用只读集合，组件不暴露内部可变集合。
5. 组件只包含数据和确定性的派生属性；I/O、时钟读取、日志、随机数、持久化、网络和
   实体创建/销毁不进入组件类型。
6. default 值只表示未提供或未知，不等于 Version4 已确认的业务默认值。
7. RuntimeEntityId、PersistentItemId、PersistentContainerId、NetworkId、ReplicationId、
   ExternalContentId、TransactionId、OperationId 和 ReservationId 不得互相替代。
8. 本文件是代码形状草案，不是实际 .cs 文件；拟议路径上的 status 一律为 proposed。

## 3. 拟议文件布局

| Component | 拟议文件 |
|---|---|
| ItemInstanceComponent | src/Items/Instances/ItemInstanceComponent.cs |
| ItemStackComponent | src/Items/Instances/ItemStackComponent.cs |
| ContainerLayoutComponent | src/Items/Containers/ContainerLayoutComponent.cs |
| ContainerCapacityComponent | src/Items/Containers/ContainerCapacityComponent.cs |
| ContainerContentsComponent | src/Items/Containers/ContainerContentsComponent.cs |
| ContainerAccessComponent | src/Items/Containers/ContainerAccessComponent.cs |
| EquipmentRelationComponent | src/Items/Equipment/EquipmentRelationComponent.cs |
| WorldItemStateComponent | src/Items/WorldDrops/WorldItemStateComponent.cs |
| WorldItemReservationComponent | src/Items/WorldDrops/WorldItemReservationComponent.cs |
| CraftingStateComponent | src/Items/Crafting/CraftingStateComponent.cs |
| CraftingReservationComponent | src/Items/Crafting/CraftingReservationComponent.cs |
| ShopInventoryComponent | src/Items/Commerce/ShopInventoryComponent.cs |
| CurrencyBalanceComponent | src/Items/Commerce/CurrencyBalanceComponent.cs |
| CommerceLedgerComponent | src/Items/Commerce/CommerceLedgerComponent.cs |

## 4. 共享值对象代码草案

以下类型不是 Component。为方便审阅集中展示；实际落地时必须拆成同名文件，每个文件只保留
一个 public 核心类型。它们的共享 owner 仍为 integration-review。

~~~csharp
using System;

namespace Terraria.Items;

public readonly record struct RuntimeEntityId(Guid Value)
{
  public static RuntimeEntityId Empty => new(Guid.Empty);

  public bool IsEmpty => Value == Guid.Empty;
}

public readonly record struct PersistentItemId(Guid Value)
{
  public static PersistentItemId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}

public readonly record struct PersistentContainerId(Guid Value)
{
  public static PersistentContainerId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}

public readonly record struct TransactionId(Guid Value)
{
  public static TransactionId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}

public readonly record struct OperationId(Guid Value)
{
  public static OperationId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}

public readonly record struct ReservationId(Guid Value)
{
  public static ReservationId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}

public readonly record struct ExternalAccountId(Guid Value)
{
  public static ExternalAccountId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
~~~

上面的集中代码仅用于表达值对象形状，不应直接作为一个最终源文件提交；否则会违反一文件一个
核心 public 类型的仓库约束。

~~~csharp
namespace Terraria.Items;

public readonly record struct NetworkId(long Value)
{
  public bool IsAssigned => Value != 0;
}

public readonly record struct ReplicationId(long Value)
{
  public bool IsAssigned => Value != 0;
}

public readonly record struct ExternalContentId(string Domain, int TypeId)
{
  public bool IsDefined => !string.IsNullOrWhiteSpace(Domain);
}

public readonly record struct ItemDefinitionRef(
  ExternalContentId ContentId,
  int DefinitionRevision)
{
  public bool IsKnown => ContentId.IsDefined;
}

public readonly record struct RecipeDefinitionRef(
  ExternalContentId ContentId,
  int DefinitionRevision)
{
  public bool IsKnown => ContentId.IsDefined;
}

public readonly record struct LootSourceRef(
  ExternalContentId SourceId,
  long SourceRevision)
{
  public bool IsKnown => SourceId.IsDefined;
}

public readonly record struct SlotIndex(int Value)
{
  public bool IsValid => Value >= 0;
}

public readonly record struct ContainerSlotRole(string Name)
{
  public bool IsDefined => !string.IsNullOrWhiteSpace(Name);
}

public enum ReservationState : byte
{
  Unknown,
  Active,
  Committed,
  Released,
  Expired,
  Cancelled,
}

public enum CraftingPhase : byte
{
  Unknown,
  Idle,
  Accepted,
  Consuming,
  Producing,
  Completed,
  Rejected,
  Cancelled,
}
~~~

### 4.1 Commerce 值对象

CommerceTransactionKind 和 CommerceTransactionState 沿用当前类型名；以下只增加
Component Design 所需的商品和账本记录形状。

~~~csharp
using Terraria.Items;

namespace Terraria.Items.Commerce;

public readonly record struct CommerceOffer(
  ExternalContentId OfferId,
  ItemDefinitionRef ItemDefinition,
  ExternalContentId VariantId,
  int AvailableQuantity,
  int MaximumQuantity,
  long BasePrice,
  ExternalContentId CurrencyId,
  long Revision);

public readonly record struct CommerceLedgerEntry(
  TransactionId TransactionId,
  OperationId OperationId,
  long Sequence,
  CommerceTransactionKind Kind,
  CommerceTransactionState State,
  long? CommittedAtTick,
  ExternalAccountId AccountId,
  ExternalAccountId? CounterpartyAccountId,
  ExternalContentId CurrencyId,
  long Amount,
  ExternalContentId OfferId,
  PersistentItemId? ResultItemId)
{
  public bool IsUnknownOutcome => State == CommerceTransactionState.Unknown;
}
~~~

## 5. Component 代码草案

### 5.1 IC-01 ItemInstanceComponent

拟议路径：src/Items/Instances/ItemInstanceComponent.cs  
status: proposed  
componentOwner: ItemContainerAndEconomy candidate  
crossSubsystemOwner: integration-review

职责边界：保存实例身份和实例修饰；不保存数量、容器槽、价格结算和世界运动。

~~~csharp
namespace Terraria.Items;

public sealed class ItemInstanceComponent
{
  public ItemInstanceComponent(
    ItemDefinitionRef definitionRef,
    PersistentItemId persistentInstanceId,
    int prefixId = 0,
    ExternalContentId variantId = default,
    int dyeId = 0,
    bool isFavorited = false,
    string? nameOverride = null)
  {
    DefinitionRef = definitionRef;
    PersistentInstanceId = persistentInstanceId;
    PrefixId = prefixId;
    VariantId = variantId;
    DyeId = dyeId;
    IsFavorited = isFavorited;
    NameOverride = nameOverride;
  }

  public ItemDefinitionRef DefinitionRef;
  public PersistentItemId PersistentInstanceId;
  public int PrefixId;
  public ExternalContentId VariantId;
  public int DyeId;
  public bool IsFavorited;
  public string? NameOverride;

  public bool HasDefinition => DefinitionRef.IsKnown;
  public bool HasPersistentIdentity => PersistentInstanceId.IsAssigned;
  public bool HasVariant => VariantId.IsDefined;
  public bool HasNameOverride => !string.IsNullOrWhiteSpace(NameOverride);
}
~~~

代码约束：

- 空实例的清理语义不能只通过本组件的派生属性猜测；
- Prefix、variant、dye 和 nameOverride 都是实例修饰，不复制到 ItemStackComponent；
- 旧快照缺失的 definition 或 persistent ID 必须由加载边界显式传入 default/unknown。

### 5.2 IC-02 ItemStackComponent

拟议路径：src/Items/Instances/ItemStackComponent.cs  
status: proposed  
componentOwner: ItemContainerAndEconomy candidate  
crossSubsystemOwner: integration-review

职责边界：保存 item entity 的唯一数量 owner；不保存内容定义、容器关系或 reservation。

~~~csharp
namespace Terraria.Items;

public struct ItemStackComponent
{
  public ItemStackComponent(
    int quantity,
    int maximumQuantity,
    ulong stackKey = 0,
    bool isUnlimited = false)
  {
    Quantity = quantity;
    MaximumQuantity = maximumQuantity;
    StackKey = stackKey;
    IsUnlimited = isUnlimited;
  }

  public int Quantity;
  public int MaximumQuantity;
  public ulong StackKey;
  public bool IsUnlimited;

  public bool IsEmpty => Quantity <= 0;

  public bool IsFull =>
    !IsUnlimited &&
    MaximumQuantity > 0 &&
    Quantity >= MaximumQuantity;

  public bool IsValid =>
    Quantity >= 0 &&
    (IsUnlimited || (MaximumQuantity >= 0 && Quantity <= MaximumQuantity));
}
~~~

代码约束：

- ItemState.Stack、StackableItemComponent.Quantity 和 dome ItemStack.Quantity 只能作为
  兼容输入或兼容读法；
- 数量归零不自动释放 reservation、删除实体或生成退款；
- IsUnlimited 是兼容候选，不得由默认值推断普通物品无限堆叠。

### 5.3 IC-03 ContainerLayoutComponent

拟议路径：src/Items/Containers/ContainerLayoutComponent.cs  
status: proposed  
componentOwner: ItemContainerAndEconomy candidate；PlayerGameplay 对选中关系有交接责任  
crossSubsystemOwner: integration-review

职责边界：保存稳定槽位布局和选择关系；不保存槽中 item、不保存容量数值。

~~~csharp
using System.Collections.Generic;

namespace Terraria.Items;

public sealed class ContainerLayoutComponent
{
  private readonly List<ContainerSlotRole> _slotRoles;

  public ContainerLayoutComponent(
    IReadOnlyList<ContainerSlotRole> slotRoles,
    SlotIndex? selectedSlot = null,
    long layoutRevision = 0)
  {
    _slotRoles = new List<ContainerSlotRole>(slotRoles);
    SelectedSlot = selectedSlot;
    LayoutRevision = layoutRevision;
  }

  public SlotIndex? SelectedSlot;
  public long LayoutRevision;

  public IReadOnlyList<ContainerSlotRole> SlotRoles => _slotRoles;

  public int SlotCount => _slotRoles.Count;

  public bool HasSelectedSlot =>
    SelectedSlot.HasValue &&
    SelectedSlot.Value.Value >= 0 &&
    SelectedSlot.Value.Value < _slotRoles.Count;
}
~~~

代码约束：

- SelectedSlot 只表示容器内局部关系，不得当作 item definition 或数量；
- SlotCount 是布局派生值，不能成为 ContainerCapacityComponent 的第二个 writer；
- selected slot 越界时应由外部边界处理，不能静默改动 contents。

### 5.4 IC-04 ContainerCapacityComponent

拟议路径：src/Items/Containers/ContainerCapacityComponent.cs  
status: proposed  
componentOwner: container capability candidate  
crossSubsystemOwner: integration-review

职责边界：保存容器类别、槽位容量、重量约束和嵌套规则；不保存内容关系。

~~~csharp
namespace Terraria.Items;

public sealed class ContainerCapacityComponent
{
  public ContainerCapacityComponent(
    ContainerKind kind,
    int slotCount,
    long? maximumWeight = null,
    bool allowsNestedContainers = false)
  {
    Kind = kind;
    SlotCount = slotCount;
    MaximumWeight = maximumWeight;
    AllowsNestedContainers = allowsNestedContainers;
  }

  public ContainerKind Kind;
  public int SlotCount;
  public long? MaximumWeight;
  public bool AllowsNestedContainers;

  public bool IsValid =>
    SlotCount >= 0 &&
    (!MaximumWeight.HasValue || MaximumWeight.Value >= 0);
}
~~~

代码约束：

- ContainerComponent.Capacity 不能与 SlotCount 形成第二套容量 authority；
- 容量不足不能静默截断 ItemStackComponent.Quantity；
- 嵌套规则的循环检测不在 Component 内实现。

### 5.5 IC-05 ContainerContentsComponent

拟议路径：src/Items/Containers/ContainerContentsComponent.cs  
status: proposed  
componentOwner: ItemContainerAndEconomy candidate  
crossSubsystemOwner: integration-review

职责边界：保存槽位到 item entity 的唯一权威关系；不保存 item 的数量、definition 和
访问 lease。

~~~csharp
using System.Collections.Generic;

namespace Terraria.Items;

public sealed class ContainerContentsComponent
{
  private readonly List<RuntimeEntityId?> _slots;

  public ContainerContentsComponent(
    IReadOnlyList<RuntimeEntityId?> slots,
    long revision = 0,
    long lastMutationTick = 0)
  {
    _slots = new List<RuntimeEntityId?>(slots);
    Revision = revision;
    LastMutationTick = lastMutationTick;
  }

  public long Revision;
  public long LastMutationTick;

  public IReadOnlyList<RuntimeEntityId?> Slots => _slots;

  public int OccupiedSlotCount
  {
    get
    {
      int occupiedSlotCount = 0;

      foreach (RuntimeEntityId? slot in _slots)
      {
        if (slot.HasValue && !slot.Value.IsEmpty)
        {
          occupiedSlotCount++;
        }
      }

      return occupiedSlotCount;
    }
  }

  public bool IsEmpty => OccupiedSlotCount == 0;

  public bool IsFull =>
    _slots.Count > 0 &&
    OccupiedSlotCount == _slots.Count;
}
~~~

代码约束：

- _slots 的长度必须与 ContainerLayoutComponent 和 ContainerCapacityComponent 一致；
- ContainerComponent.Contents、Player 数组、Chest.item 和网络解码副本不能并行成为 writer；
- LastMutationTick 只是已提交变化的快照/诊断字段，不拥有时钟。

### 5.6 IC-06 ContainerAccessComponent

拟议路径：src/Items/Containers/ContainerAccessComponent.cs  
status: proposed  
componentOwner: WorldStorage 或 PlayerGameplay candidate  
crossSubsystemOwner: integration-review

职责边界：保存容器持久身份、空间位置、锁定和短期访问关系；不直接保存或消费物品。

~~~csharp
namespace Terraria.Items;

public sealed class ContainerAccessComponent
{
  public ContainerAccessComponent(
    PersistentContainerId persistentContainerId,
    TileCoordinates? tilePosition = null,
    bool isLocked = false,
    ulong requiredAccessFlags = 0,
    RuntimeEntityId? currentAccessor = null,
    long? accessLeaseUntilTick = null,
    bool isShared = true)
  {
    PersistentContainerId = persistentContainerId;
    TilePosition = tilePosition;
    IsLocked = isLocked;
    RequiredAccessFlags = requiredAccessFlags;
    CurrentAccessor = currentAccessor;
    AccessLeaseUntilTick = accessLeaseUntilTick;
    IsShared = isShared;
  }

  public PersistentContainerId PersistentContainerId;
  public TileCoordinates? TilePosition;
  public bool IsLocked;
  public ulong RequiredAccessFlags;
  public RuntimeEntityId? CurrentAccessor;
  public long? AccessLeaseUntilTick;
  public bool IsShared;

  public bool HasActiveAccessorAt(long currentTick) =>
    CurrentAccessor.HasValue &&
    AccessLeaseUntilTick.HasValue &&
    currentTick < AccessLeaseUntilTick.Value;
}
~~~

代码约束：

- TileCoordinates、Chest index 和 PersistentContainerId 不互相替代；
- lease 到期只改变访问关系，不改变 ContainerContentsComponent.Revision；
- 旧存档缺少持久容器 ID 时，由恢复边界保留未知状态，不用坐标伪造永久身份。

### 5.7 IC-07 EquipmentRelationComponent

拟议路径：src/Items/Equipment/EquipmentRelationComponent.cs  
status: proposed  
componentOwner: PlayerGameplay 与 ItemContainerAndEconomy candidate  
crossSubsystemOwner: integration-review

职责边界：保存装备宿主到功能、外观和染色槽的 item entity 关系；不保存装备属性和战斗效果。

~~~csharp
using System.Collections.Generic;

namespace Terraria.Items;

public sealed class EquipmentRelationComponent
{
  private readonly Dictionary<EquipmentSlot, RuntimeEntityId?> _functionalSlots;
  private readonly Dictionary<EquipmentSlot, RuntimeEntityId?> _vanitySlots;
  private readonly Dictionary<EquipmentSlot, RuntimeEntityId?> _dyeSlots;
  private readonly HashSet<EquipmentSlot> _hiddenSlots;

  public EquipmentRelationComponent(
    RuntimeEntityId ownerEntity,
    IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>? functionalSlots = null,
    IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>? vanitySlots = null,
    IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?>? dyeSlots = null,
    IReadOnlySet<EquipmentSlot>? hiddenSlots = null,
    long revision = 0)
  {
    OwnerEntity = ownerEntity;
    _functionalSlots = functionalSlots is null
      ? []
      : new Dictionary<EquipmentSlot, RuntimeEntityId?>(functionalSlots);
    _vanitySlots = vanitySlots is null
      ? []
      : new Dictionary<EquipmentSlot, RuntimeEntityId?>(vanitySlots);
    _dyeSlots = dyeSlots is null
      ? []
      : new Dictionary<EquipmentSlot, RuntimeEntityId?>(dyeSlots);
    _hiddenSlots = hiddenSlots is null
      ? []
      : new HashSet<EquipmentSlot>(hiddenSlots);
    Revision = revision;
  }

  public RuntimeEntityId OwnerEntity;
  public long Revision;

  public IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?> FunctionalSlots =>
    _functionalSlots;

  public IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?> VanitySlots =>
    _vanitySlots;

  public IReadOnlyDictionary<EquipmentSlot, RuntimeEntityId?> DyeSlots =>
    _dyeSlots;

  public IReadOnlySet<EquipmentSlot> HiddenSlots => _hiddenSlots;

  public bool IsHidden(EquipmentSlot slot) => _hiddenSlots.Contains(slot);
}
~~~

代码约束：

- FunctionalSlots、VanitySlots 和 DyeSlots 是不同关系集合，不能互相覆盖；
- 属性、伤害、状态效果和资格判断留在其他边界；
- 同一个 item entity 与背包/装备关系的冲突由外部提交边界处理。

### 5.8 IC-08 WorldItemStateComponent

拟议路径：src/Items/WorldDrops/WorldItemStateComponent.cs  
status: proposed  
componentOwner: ItemContainerAndEconomy payload candidate  
crossSubsystemOwner: integration-review

职责边界：保存世界掉落实体到 item entity 的绑定、来源、active 状态和复制身份；不复制
ItemStackComponent 的数量。

~~~csharp
namespace Terraria.Items;

public sealed class WorldItemStateComponent
{
  public WorldItemStateComponent(
    RuntimeEntityId itemEntity,
    WorldPosition worldPosition,
    long spawnedAtTick,
    bool active,
    ReplicationId replicationId,
    LootSourceRef? spawnSource = null,
    long? despawnAtTick = null,
    bool isInstanced = false,
    bool isBeingGrabbed = false,
    bool isOnConveyor = false,
    long revision = 0)
  {
    ItemEntity = itemEntity;
    WorldPosition = worldPosition;
    SpawnedAtTick = spawnedAtTick;
    Active = active;
    ReplicationId = replicationId;
    SpawnSource = spawnSource;
    DespawnAtTick = despawnAtTick;
    IsInstanced = isInstanced;
    IsBeingGrabbed = isBeingGrabbed;
    IsOnConveyor = isOnConveyor;
    Revision = revision;
  }

  public RuntimeEntityId ItemEntity;
  public LootSourceRef? SpawnSource;
  public bool Active;
  public ReplicationId ReplicationId;
  public WorldPosition WorldPosition;
  public long SpawnedAtTick;
  public long? DespawnAtTick;
  public bool IsInstanced;
  public bool IsBeingGrabbed;
  public bool IsOnConveyor;
  public long Revision;

  public bool IsExpiredAt(long currentTick) =>
    DespawnAtTick.HasValue &&
    currentTick >= DespawnAtTick.Value;
}
~~~

代码约束：

- ItemEntity、ReplicationId、LootSourceRef 属于不同身份层级；
- Active 为 false 不能被解释为 payload 已安全转移；
- 位置、运动和过期 owner 仍可由 SpawnLifecycleAndLoot 接管，本组件不添加运动字段。

### 5.9 IC-09 WorldItemReservationComponent

拟议路径：src/Items/WorldDrops/WorldItemReservationComponent.cs  
status: proposed  
componentOwner: SpawnLifecycleAndLoot / ItemContainerAndEconomy candidate  
crossSubsystemOwner: integration-review

职责边界：保存拾取 reservation、忽略 owner 和拾取资格时间；不直接改变数量。

~~~csharp
namespace Terraria.Items;

public sealed class WorldItemReservationComponent
{
  public WorldItemReservationComponent(
    ReservationId? reservationId = null,
    RuntimeEntityId? reservedFor = null,
    long? reservationExpiresAt = null,
    RuntimeEntityId? ignoreOwner = null,
    long? ignoreOwnerUntilTick = null,
    long noGrabUntilTick = 0,
    long enemyPickupBlockedUntilTick = 0,
    long reservationRevision = 0)
  {
    ReservationId = reservationId;
    ReservedFor = reservedFor;
    ReservationExpiresAt = reservationExpiresAt;
    IgnoreOwner = ignoreOwner;
    IgnoreOwnerUntilTick = ignoreOwnerUntilTick;
    NoGrabUntilTick = noGrabUntilTick;
    EnemyPickupBlockedUntilTick = enemyPickupBlockedUntilTick;
    ReservationRevision = reservationRevision;
  }

  public ReservationId? ReservationId;
  public RuntimeEntityId? ReservedFor;
  public long? ReservationExpiresAt;
  public RuntimeEntityId? IgnoreOwner;
  public long? IgnoreOwnerUntilTick;
  public long NoGrabUntilTick;
  public long EnemyPickupBlockedUntilTick;
  public long ReservationRevision;

  public bool HasReservationAt(long currentTick) =>
    ReservedFor.HasValue &&
    ReservationExpiresAt.HasValue &&
    currentTick < ReservationExpiresAt.Value;

  public bool CanBeGrabbedAt(long currentTick) =>
    currentTick >= NoGrabUntilTick;

  public bool IsOwnerIgnoredAt(long currentTick) =>
    IgnoreOwner.HasValue &&
    IgnoreOwnerUntilTick.HasValue &&
    currentTick < IgnoreOwnerUntilTick.Value;
}
~~~

代码约束：

- ReservationId、ReservedFor 和 ReplicationId 不能共用一个 ID 类型；
- reservation 只参与占用状态，不预扣 ItemStackComponent.Quantity；
- 重复提交、竞争、超时和销毁的终态由外部边界定义，本组件不执行结算。

### 5.10 IC-10 CraftingStateComponent

拟议路径：src/Items/Crafting/CraftingStateComponent.cs  
status: proposed  
componentOwner: ItemContainerAndEconomy candidate  
crossSubsystemOwner: integration-review

职责边界：保存制作事务状态、Recipe 引用、请求数量和来源容器 revision 快照；不保存
Recipe 全量定义和材料数量副本。

~~~csharp
using System.Collections.Generic;

namespace Terraria.Items;

public sealed class CraftingStateComponent
{
  private readonly Dictionary<PersistentContainerId, long> _expectedSourceRevisions;

  public CraftingStateComponent(
    TransactionId? transactionId = null,
    RecipeDefinitionRef? recipeRef = null,
    int requestedQuantity = 0,
    IReadOnlyDictionary<PersistentContainerId, long>? expectedSourceRevisions = null,
    long? acceptedAtTick = null,
    long craftSequence = 0,
    CraftingPhase phase = CraftingPhase.Idle)
  {
    TransactionId = transactionId;
    RecipeRef = recipeRef;
    RequestedQuantity = requestedQuantity;
    _expectedSourceRevisions = expectedSourceRevisions is null
      ? []
      : new Dictionary<PersistentContainerId, long>(expectedSourceRevisions);
    AcceptedAtTick = acceptedAtTick;
    CraftSequence = craftSequence;
    Phase = phase;
  }

  public TransactionId? TransactionId;
  public RecipeDefinitionRef? RecipeRef;
  public int RequestedQuantity;
  public long? AcceptedAtTick;
  public long CraftSequence;
  public CraftingPhase Phase;

  public IReadOnlyDictionary<PersistentContainerId, long> ExpectedSourceRevisions =>
    _expectedSourceRevisions;

  public bool HasTransaction => TransactionId.HasValue;

  public bool HasActiveCraft =>
    TransactionId.HasValue &&
    RecipeRef.HasValue &&
    RequestedQuantity > 0 &&
    Phase is CraftingPhase.Accepted or CraftingPhase.Consuming or CraftingPhase.Producing;
}
~~~

代码约束：

- Phase 的未知或未决状态不能当作成功；
- ExpectedSourceRevisions 是提交校验快照，不是容器 revision 的第二个 writer；
- CraftingMaterialReservation 的数量不能复制成 ItemStackComponent 之外的权威数量。

### 5.11 IC-11 CraftingReservationComponent

拟议路径：src/Items/Crafting/CraftingReservationComponent.cs  
status: proposed  
componentOwner: ItemContainerAndEconomy candidate  
crossSubsystemOwner: integration-review

职责边界：保存制作事务对来源容器、槽位和 item entity 的临时占用关系；不直接减少数量。

~~~csharp
namespace Terraria.Items;

public sealed class CraftingReservationComponent
{
  public CraftingReservationComponent(
    ReservationId reservationId,
    TransactionId transactionId,
    PersistentContainerId sourceContainerId,
    SlotIndex sourceSlot,
    RuntimeEntityId sourceItemEntity,
    int quantity,
    long expectedContainerRevision,
    long? createdAt = null,
    long? expiresAt = null,
    ReservationState state = ReservationState.Active)
  {
    ReservationId = reservationId;
    TransactionId = transactionId;
    SourceContainerId = sourceContainerId;
    SourceSlot = sourceSlot;
    SourceItemEntity = sourceItemEntity;
    Quantity = quantity;
    ExpectedContainerRevision = expectedContainerRevision;
    CreatedAt = createdAt;
    ExpiresAt = expiresAt;
    State = state;
  }

  public ReservationId ReservationId;
  public TransactionId TransactionId;
  public PersistentContainerId SourceContainerId;
  public SlotIndex SourceSlot;
  public RuntimeEntityId SourceItemEntity;
  public int Quantity;
  public long ExpectedContainerRevision;
  public long? CreatedAt;
  public long? ExpiresAt;
  public ReservationState State;

  public bool IsActive => State == ReservationState.Active;

  public bool IsExpiredAt(long currentTick) =>
    IsActive &&
    ExpiresAt.HasValue &&
    currentTick >= ExpiresAt.Value;
}
~~~

代码约束：

- Quantity 是占用量，不是已消费量；
- ExpectedContainerRevision 用于外部提交边界的再次比较；
- ReservationState 终态不能由单个字段归零隐式推断；
- ReservationId、TransactionId、SourceContainerId 和 SourceItemEntity 必须保持类型区分。

### 5.12 IC-12 ShopInventoryComponent

拟议路径：src/Items/Commerce/ShopInventoryComponent.cs  
status: proposed  
componentOwner: Commerce candidate  
crossSubsystemOwner: integration-review

职责边界：保存商店商品快照、库存和 restock 元数据；不保存买方余额、买方容器和账本。

~~~csharp
using System.Collections.Generic;

namespace Terraria.Items.Commerce;

public sealed class ShopInventoryComponent
{
  private readonly List<CommerceOffer> _offers;

  public ShopInventoryComponent(
    IReadOnlyList<CommerceOffer>? offers = null,
    long shopRevision = 0,
    long? restockAtTick = null,
    long? lastRestockTick = null)
  {
    _offers = offers is null
      ? []
      : new List<CommerceOffer>(offers);
    ShopRevision = shopRevision;
    RestockAtTick = restockAtTick;
    LastRestockTick = lastRestockTick;
  }

  public long ShopRevision;
  public long? RestockAtTick;
  public long? LastRestockTick;

  public IReadOnlyList<CommerceOffer> Offers => _offers;

  public bool IsRestockDueAt(long currentTick) =>
    RestockAtTick.HasValue &&
    currentTick >= RestockAtTick.Value;
}
~~~

代码约束：

- CommerceOffer 是商品快照值对象，不是 Component；
- offer、stock、price snapshot 和 ShopRevision 不能由 Item 静态 price 字段替代；
- Version4 Chest.SetupShop 的空实现使商品全集仍是 partial，不得从完整参考源码推断为已闭合。

### 5.13 IC-13 CurrencyBalanceComponent

拟议路径：src/Items/Commerce/CurrencyBalanceComponent.cs  
status: proposed  
componentOwner: Commerce candidate within ItemContainerAndEconomy  
crossSubsystemOwner: integration-review

职责边界：保存逻辑账户余额和上限；不把 coin item 槽扫描结果直接当作逻辑余额。

~~~csharp
using System.Collections.Generic;

namespace Terraria.Items.Commerce;

public sealed class CurrencyBalanceComponent
{
  private readonly Dictionary<ExternalContentId, long> _balances;
  private readonly Dictionary<ExternalContentId, long> _currencyCaps;

  public CurrencyBalanceComponent(
    ExternalAccountId accountId,
    IReadOnlyDictionary<ExternalContentId, long>? balances = null,
    IReadOnlyDictionary<ExternalContentId, long>? currencyCaps = null,
    long revision = 0,
    TransactionId? lastTransactionId = null)
  {
    AccountId = accountId;
    _balances = balances is null
      ? []
      : new Dictionary<ExternalContentId, long>(balances);
    _currencyCaps = currencyCaps is null
      ? []
      : new Dictionary<ExternalContentId, long>(currencyCaps);
    Revision = revision;
    LastTransactionId = lastTransactionId;
  }

  public ExternalAccountId AccountId;
  public long Revision;
  public TransactionId? LastTransactionId;

  public IReadOnlyDictionary<ExternalContentId, long> Balances => _balances;

  public IReadOnlyDictionary<ExternalContentId, long> CurrencyCaps =>
    _currencyCaps;

  public bool IsInitialized => AccountId.IsAssigned;

  public int TotalCurrencyKinds
  {
    get
    {
      int totalCurrencyKinds = 0;

      foreach (long balance in _balances.Values)
      {
        if (balance > 0)
        {
          totalCurrencyKinds++;
        }
      }

      return totalCurrencyKinds;
    }
  }
}
~~~

代码约束：

- 逻辑余额与金币 Item 表示分离；
- cap 缺失不自动解释为无限；
- 结算未知时不能通过重复扣款或盲目加回恢复；
- balances 和 currencyCaps 的合法性检查、写入和结算顺序不在 Component 内实现。

### 5.14 IC-14 CommerceLedgerComponent

拟议路径：src/Items/Commerce/CommerceLedgerComponent.cs  
status: proposed  
componentOwner: Commerce candidate within ItemContainerAndEconomy  
crossSubsystemOwner: integration-review

职责边界：保存交易记录、幂等线索、未知结果和保留边界；不保存商店目录或玩家全部物品。

~~~csharp
using System.Collections.Generic;

namespace Terraria.Items.Commerce;

public sealed class CommerceLedgerComponent
{
  private readonly List<CommerceLedgerEntry> _entries;

  public CommerceLedgerComponent(
    IReadOnlyList<CommerceLedgerEntry>? entries = null,
    long lastSequence = 0,
    long retentionFloorSequence = 0)
  {
    _entries = entries is null
      ? []
      : new List<CommerceLedgerEntry>(entries);
    LastSequence = lastSequence;
    RetentionFloorSequence = retentionFloorSequence;
  }

  public long LastSequence;
  public long RetentionFloorSequence;

  public IReadOnlyList<CommerceLedgerEntry> Entries => _entries;

  public bool ContainsUnknownOutcome
  {
    get
    {
      foreach (CommerceLedgerEntry entry in _entries)
      {
        if (entry.IsUnknownOutcome)
        {
          return true;
        }
      }

      return false;
    }
  }
}
~~~

代码约束：

- Entries 是账本历史，不是 UI 缓存；
- LastSequence 不得替代 CurrencyBalanceComponent.Revision 或 ShopInventoryComponent.ShopRevision；
- 重复 OperationId 只能由外部结算边界去重；
- Unknown outcome 必须保留为未知，不能在 Component 内转成成功或失败。

## 6. Entity 组合代码形状

本节只展示组合，不定义实体创建 API、系统顺序或行为。

### 6.1 普通 item entity

~~~csharp
// Conceptual composition only; not an entity factory.
ItemInstanceComponent instance;
ItemStackComponent stack;
~~~

非空 item 至少需要实例和数量；WorldItemStateComponent、装备关系或容器内容关系通过
外部 entity 组合发生，不把它们嵌套进 ItemInstanceComponent。

### 6.2 玩家、银行和 Chest 容器

~~~csharp
// Conceptual composition only; not an entity factory.
ContainerLayoutComponent layout;
ContainerCapacityComponent capacity;
ContainerContentsComponent contents;
ContainerAccessComponent? access;
~~~

ContainerComponent 不能继续同时拥有 capacity 和 contents；布局、容量、内容和访问的
revision/lifecycle 彼此分离。

### 6.3 玩家装备宿主

~~~csharp
// Conceptual composition only; not an entity factory.
EquipmentRelationComponent equipment;
~~~

装备关系不拥有装备属性；同一 item 与容器内容关系的冲突由跨组件提交边界裁决。

### 6.4 世界掉落实体

~~~csharp
// Conceptual composition only; not an entity factory.
WorldItemStateComponent worldItem;
WorldItemReservationComponent? reservation;
~~~

世界掉落引用 item entity；数量仍位于该 item entity 的 ItemStackComponent，不在
WorldItemStateComponent 中复制。

### 6.5 制作事务实体

~~~csharp
// Conceptual composition only; not an entity factory.
CraftingStateComponent crafting;
CraftingReservationComponent? reservation;
~~~

非 active 制作事务不能保留 active reservation；这个约束由外部状态转换实现，而不是由
Component 构造函数隐式完成。

### 6.6 商店与经济账户

~~~csharp
// Conceptual composition only; not an entity factory.
ShopInventoryComponent shop;
CurrencyBalanceComponent accountBalance;
CommerceLedgerComponent ledger;
~~~

商店不持有买方余额或买方容器；账户余额不把 coin item slot 变成内嵌数组。

## 7. 现有 NLTX 类型兼容映射

| 现有类型 | 草案目标 | 处理原则 |
|---|---|---|
| src/Items/ItemDefinitionComponent.cs | ItemInstanceComponent.DefinitionRef | 现有 int ContentId 通过边界转换为 ExternalContentId；不能直接把 int 当作所有外部 ID |
| src/Items/ItemInstanceComponent.cs | ItemInstanceComponent | 保留 prefix、variant、dye、收藏和名称覆盖语义；Guid 显式映射为 PersistentItemId |
| src/Items/ItemState.cs | ItemInstanceComponent + ItemStackComponent | Type 与 Stack 不再作为两个 Component 的并行 authority |
| src/Items/StackableItemComponent.cs | ItemStackComponent | Quantity 迁入唯一数量 owner；MaximumQuantity 仍需 definition owner 裁决 |
| src/Items/ContainerComponent.cs | ContainerCapacityComponent + ContainerContentsComponent | Capacity 与 Contents 拆开，旧类型不能继续双写 |
| src/Items/ContainerCapacityComponent.cs | ContainerCapacityComponent | 保留 kind、slotCount、weight、nested 的代码形状 |
| src/Items/ContainerContentsComponent.cs | ContainerContentsComponent | EntityReference 需要转为 RuntimeEntityId；revision 保留但写入 owner 未闭合 |
| src/Items/ContainerAccessComponent.cs | ContainerAccessComponent | EntityReference accessor 需要转为 RuntimeEntityId；补充 PersistentContainerId |
| src/Items/InventoryComponent.cs | ContainerLayoutComponent + ContainerContentsComponent | ammo、coin、trash 和 selected 不继续混在一个泛化组件中 |
| src/Items/EquipmentComponent.cs | EquipmentRelationComponent | 三类槽关系与 hidden 集合保留；最终 owner 仍需 PlayerGameplay 交接 |
| src/Items/WorldItemComponent.cs | WorldItemStateComponent | 保留 world position、寿命和表现字段形状，但位置/寿命 owner 仍未决 |
| src/Items/WorldItemReservationComponent.cs | WorldItemReservationComponent | EntityReference 转为强类型关系；增加 ReservationId 和 revision |
| src/Items/CraftingComponent.cs | CraftingStateComponent | active recipe、requested、accepted、sequence 映射到新字段；reserved materials 分离 |
| src/Items/CraftingMaterialReservation.cs | CraftingReservationComponent | 从 record value 转为带 transaction、reservation、container revision 的状态关系 |
| src/Items/ShopInventoryComponent.cs | ShopInventoryComponent | ShopOffer 转为 CommerceOffer；Revision 改为 ShopRevision |
| src/Items/ShopOffer.cs | CommerceOffer | ItemContentId、CurrencyId 等 int 需要经 ExternalContentId 边界转换 |
| src/Items/Commerce/CurrencyBalanceComponent.cs | CurrencyBalanceComponent | Guid account 和 last transaction 转为强类型 ID；增加 caps |
| src/Items/Commerce/CommerceLedgerComponent.cs | CommerceLedgerComponent | entries 保留；entry identity 扩展 OperationId、账户和结果 item 关联 |
| src/Items/Commerce/CommerceLedgerEntry.cs | CommerceLedgerEntry | Guid/EntityReference 字段转为 TransactionId、OperationId 和强类型关系 |
| src/Items/Commerce/CommerceTransactionKind.cs | 沿用 | 枚举名可沿用，具体状态协议未在本草案裁决 |
| src/Items/Commerce/CommerceTransactionState.cs | 沿用 | Unknown、Committed、Rejected 语义保留，完整补偿状态未在本草案裁决 |

兼容映射是边界输入说明，不是迁移计划；本文件不决定旧类型何时删除或是否保留。

## 8. 不变量与唯一 writer 速查

| 状态 | 唯一 proposed owner | 禁止的第二表示 |
|---|---|---|
| item instance definition、prefix、variant、dye | ItemInstanceComponent | ItemState.Type/Prefix 作为并行写者 |
| item quantity | ItemStackComponent | StackableItemComponent、dome ItemStack、WorldItem payload 数量副本 |
| container layout/selection | ContainerLayoutComponent | InventoryComponent 的重复 layout |
| container capacity | ContainerCapacityComponent | ContainerComponent.Capacity |
| container contents | ContainerContentsComponent | ContainerComponent.Contents、Player/Chest 数组并行写入 |
| container access/lease | ContainerAccessComponent | WorldStorage 坐标伪造永久身份 |
| equipment relations | EquipmentRelationComponent candidate | EquipmentComponent 与 Player 数组并行写入 |
| world item binding | WorldItemStateComponent | WorldItem payload 的第二份 item identity |
| pickup reservation | WorldItemReservationComponent candidate | ownership 字段直接充当 ReservationId |
| crafting state | CraftingStateComponent | Recipe 全量定义或 reserved materials 数量副本 |
| crafting material reservation | CraftingReservationComponent | 预扣 ItemStackComponent.Quantity |
| shop stock | ShopInventoryComponent | Item 静态价格字段充当 shop stock authority |
| logical balance | CurrencyBalanceComponent candidate | coin item 槽扫描直接覆盖 balance |
| commerce history | CommerceLedgerComponent candidate | UI receipt 或 last transaction 单独充当账本 |

## 9. 未决项

以下项目来自 Component Design 的 decision-required 状态，本代码草案不擅自裁决：

1. quantity 与 maximumQuantity 的最终 owner，以及 definition、variant、扩展如何影响上限；
2. ContainerLayout、ContainerCapacity、ContainerContents 和 ContainerAccess 的跨
   PlayerGameplay/WorldStorage owner；
3. EquipmentRelation 与 PlayerGameplay、CombatAndStatus 之间的关系提交边界；
4. WorldItem 状态、运动、寿命、拾取 reservation 与 SpawnLifecycleAndLoot 的分工；
5. 逻辑货币余额与金币 Item 表示的双向转换和旧存档恢复策略；
6. 各强类型 ID 的共享定义、序列化和跨进程映射；
7. persistence、network、replication、content、transaction 和 reservation 的正式 owner；
8. CommerceOffer 的价格单位、无限库存表示、offer revision 和商品结果 identity；
9. CraftingPhase、ReservationState 的完整状态集合与未知结果恢复语义；
10. long? tick 字段的时钟来源、回放语义和持久化策略。

## 10. 当前验证状态

| 检查项 | 结果 | 说明 |
|---|---|---|
| 文档结构 | passed-static-only | 已核对 11 节、14 个 Component、14 个 status/proposed path、23 对 C# 代码围栏和花括号平衡 |
| C# 编译 | not-run | 未创建 .cs 文件，未运行 dotnet |
| 单元/集成测试 | not-run | 未创建运行时代码和测试 |
| 现有设计一致性 | review-required | 草案依据现有 Component Design，仍需 owner review |
| 研究报告完整性 | unchanged | 不修改 sourceReport |
| 工作树范围 | review-required | 只应新增本草案文件，其他已有改动不处理 |

## 11. 交付声明

本文件是 ItemContainerAndEconomy 的实际代码组件草案，不是已落地实现。

所有组件和共享值对象均是 proposed。代码片段只用于评审字段形状、类型边界、集合封装和
兼容映射；它们没有被写入 src/，没有经过 dotnet 编译，也没有经过测试。

本文件仍然不定义 System、Query、Command、Event、Adapter、Projection、运行时调度、
事务算法、存档协议、网络协议、迁移计划或测试实现。
