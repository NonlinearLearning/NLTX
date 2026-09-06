# LeashedEntitySimulation Component Code Draft

## 1. 草案元数据

| 项目 | 内容 |
|---|---|
| 文档类型 | 实际代码形状草案（Markdown-only） |
| 设计基线 | `D:\TRbackup\NLTX\docs\design\2026-09-05-version4-leashed-entity-simulation-component-design.md` |
| 目标项目 | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj` |
| 目标框架 | `net10.0`（来自当前 Simulation project） |
| 目标能力 | `LeashedEntitySimulation` |
| 目标 Component 数量 | 8 |
| 总体状态 | `proposed` |
| 代码状态 | 未创建 `.cs` 文件；以下代码块只是可拆分的文件草案 |
| 编译状态 | 未编译 |
| 行为状态 | 未声明与 Version4 行为等价 |
| 验证状态 | 未运行构建、测试、运行或发布命令 |

本文件只描述 Component 的代码形状、字段、派生属性和类型引用，不是实现提交，也不替换原 Component Design。代码块中的 `status: proposed` 是每个类型的强制状态标签：在 owner、类型边界和恢复/网络语义完成整合前，不得把它们视为已建立的 authoritative component。

本次草案不创建实际 C# 文件，不修改已有 Leash 草图，不修改 `.csproj`，不创建测试，也不启动子代理。

## 2. 代码形状约束

### 2.1 文件与命名空间

建议未来每个 Component 保持一个同名的 public 类型文件，并继续使用现有能力命名空间：

```text
dome/src/Terraria.Dome.Simulation/Leash/
├── LeashedEntityStateComponent.cs
├── LeashedEntityLifecycleComponent.cs
├── LeashedEntityMotionComponent.cs
├── LeashedEntityAnchorRelationComponent.cs
├── LeashedEntitySectionMembershipComponent.cs
├── LeashedCritterBehaviorComponent.cs
├── LeashedKiteBehaviorComponent.cs
└── LeashedEntityLegacySlotComponent.cs
```

建议命名空间为：

```csharp
namespace Terraria.Dome.Simulation.Leash;
```

这样可以沿用当前 `Leash` 草图的命名空间，避免仅为目录整理而改变 public API。实际迁移时，`LeashedEntityStateComponent.cs` 不能与当前同名草图并存；需要在后续迁移任务中合并并明确旧字段的去向。本文件没有执行该迁移。

### 2.2 字段和属性规则

- Component 保存字段，不保存系统、命令、协议 writer/reader、TileEntity 宿主对象或表现历史数组。
- 派生属性只能从本 Component 已有字段计算，不作为独立持久化或网络 authority。
- `EntityReference`、`TileEntityId`、legacy slot、`DefinitionId` 和协议 module ID 表达不同层次的身份，不互相复用。
- 无法从主 Version4 基线确认的默认值不被代码注释伪装成已确认行为默认；结构体默认值只表示“未完成绑定”或“未初始化”，不是可运行实体默认值。
- 代码采用当前 NLTX 的 2-space 缩进、public PascalCase 成员和可变 struct Component 形状；这不表示代码已经满足项目编译闭环。

## 3. 类型映射与 unresolved 边界

代码块优先复用当前 NLTX 已存在的类型，但下表中标为候选的类型仍需要跨项目整合裁决。

| 代码中的类型 | 当前来源 | 本草案的用法 | 状态 |
|---|---|---|---|
| `LocationComponent` | `src/Share/Entity/Components/LocationComponent.cs` | Motion 的位置字段 | existing；已作为映射候选，尚未证明已经接入 Leashed authority |
| `VelocityComponent` | `src/Share/Entity/Components/VelocityComponent.cs` | Motion 的速度字段 | existing；已作为映射候选，尚未证明已经接入 Leashed authority |
| `DirectionComponent` | `src/Share/Entity/Components/DirectionComponent.cs` | Motion 的水平朝向字段 | existing；`int` 语义仍需和行为核对 |
| `EntityReference` | `src/Relationships/EntityReference.cs` | Anchor 的 runtime entity reference | existing；不等同于 `Arch.Core.Entity`、UUID 或持久化 ID |
| `TileCoordinate` | `src/WorldStorage/TileCoordinate.cs` | Anchor 坐标和 critter 目标坐标的候选值对象 | candidate；仓库中存在同名的其他命名空间类型，最终 owner 未锁定 |
| `TileEntityId` | `src/WorldStorage/TileEntityId.cs` | Persistent anchor ID 候选 | candidate；WorldStorage 与 Leash relation 的 owner 未锁定 |
| `WorldSectionCoordinates` | `dome/src/Terraria.Dome.Simulation/World/WorldSectionCoordinates.cs` | Section membership 的当前 Dome 类型 | existing；与 `Terraria.WorldStorage.SectionCoordinate` 的统一仍为 evidence-gap |
| `LeashedEntityLifecycle` | `dome/src/Terraria.Dome.Simulation/Leash/LeashedEntityLifecycle.cs` | Lifecycle 的状态字段 | existing；当前仅有 enum，组合生命周期尚未闭合 |
| `System.Numerics.Vector2` | NLTX Entity query 使用 | Motion 的 `Center`/`Size` 派生结果 | mapping candidate；Version4 原型使用 XNA `Vector2`，不直接把 XNA 类型引入 Dome |

代码层面的两个可空化处理需要特别保留：

1. `AnchorCoordinate` 使用 `TileCoordinate?`，让“当前没有可接受锚点关系”与 `(0, 0)` 坐标分开；这不是对最终 relation schema 的定案。
2. `TargetPosition` 使用 `TileCoordinate?`，让“还没有有效目标”与真实的 tile `(0, 0)` 分开；若后续行为 owner 证明目标始终存在，可再收窄为非空值对象。

## 4. Component 代码草案

### 4.1 `LeashedEntityStateComponent`

建议文件：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityStateComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.Leash;

/// <summary>
/// The definition binding for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review (definition and identity usage)
/// </summary>
public struct LeashedEntityStateComponent
{
  public LeashedEntityStateComponent(int definitionId)
  {
    DefinitionId = definitionId;
  }

  /// <summary>
  /// Version4 registry definition reference. Zero means unbound/invalid candidate.
  /// It is not an entity instance ID, a network module ID, or a legacy slot.
  /// </summary>
  public int DefinitionId;
}
```

字段说明：

- `DefinitionId` 对应 Version4 `LeashedEntity.Type` 和注册目录中的 definition 候选。
- Version4 `Registry.RegisterAll` 先放置 null 占位，当前有效类型范围候选为 `1..19`；范围校验不在 Component 中执行。
- 当前草图中的 `TypeId` 可映射到此字段。
- 当前草图中的 `InstanceId`、`Anchor`、`Active`、`LifecycleState` 和 `Spawned` 不应继续放在 State 中，分别由 LegacySlot、AnchorRelation 和 Lifecycle 承接或暂缓。
- 结构体的隐式 `default` 会产生 `DefinitionId == 0`；这里表示未绑定，不表示有效实体。

### 4.2 `LeashedEntityLifecycleComponent`

建议文件：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityLifecycleComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.Leash;

/// <summary>
/// Lifecycle state for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: none for local state; transition consumers remain external
/// </summary>
public struct LeashedEntityLifecycleComponent
{
  public LeashedEntityLifecycleComponent()
    : this(LeashedEntityLifecycle.Inactive, false, 0)
  {
  }

  public LeashedEntityLifecycleComponent(
    LeashedEntityLifecycle state,
    bool spawned,
    ulong transitionSequence)
  {
    State = state;
    Spawned = spawned;
    TransitionSequence = transitionSequence;
  }

  /// <summary>
  /// The only stored lifecycle state candidate for active/inactive status.
  /// </summary>
  public LeashedEntityLifecycle State;

  /// <summary>
  /// Whether the instance has entered its spawned runtime state.
  /// </summary>
  public bool Spawned;

  /// <summary>
  /// Candidate idempotency/revision field. Version4 does not provide this field.
  /// status: proposed; evidence-gap: transition ownership is unresolved.
  /// </summary>
  public ulong TransitionSequence;

  /// <summary>
  /// Derived view only; do not persist as a second active authority.
  /// </summary>
  public bool IsActive => State == LeashedEntityLifecycle.Active;

  /// <summary>
  /// Derived terminal-state view only.
  /// </summary>
  public bool IsRemoved => State == LeashedEntityLifecycle.Removed;
}
```

字段说明：

- `State` 复用当前已存在的 `Inactive`、`Active`、`Despawning`、`Removed` 枚举。
- `Spawned` 从当前 `LeashedEntityStateComponent` 中拆出；它不由 section cache 或 anchor host 各自复制。
- `TransitionSequence` 是候选辅助字段，不是 Version4 已确认成员。若整合后没有明确 owner，应删除，而不是复制到其他 Component。
- `IsActive` 和 `IsRemoved` 是派生属性，不能序列化成另一份 authority。
- `Removed` 不应回到 `Active`；具体状态转换由后续系统/迁移任务决定，本草案不定义其执行者。

### 4.3 `LeashedEntityMotionComponent`

建议文件：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityMotionComponent.cs`

```csharp
using System.Numerics;
using EntityEcs.Components;

namespace Terraria.Dome.Simulation.Leash;

/// <summary>
/// Shared geometric state for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review (motion value-object mapping)
/// </summary>
public struct LeashedEntityMotionComponent
{
  public LeashedEntityMotionComponent(
    LocationComponent position,
    VelocityComponent velocity,
    DirectionComponent direction,
    int width,
    int height)
  {
    Position = position;
    Velocity = velocity;
    Direction = direction;
    Width = width;
    Height = height;
  }

  /// <summary>
  /// Top-left position candidate, expressed with the existing NLTX value type.
  /// Version4's source prototype uses Vector2; the mapping is not final.
  /// </summary>
  public LocationComponent Position;

  /// <summary>
  /// Velocity candidate. Packed wire values do not belong here.
  /// </summary>
  public VelocityComponent Velocity;

  /// <summary>
  /// Horizontal direction candidate. Kite rotation does not replace this field.
  /// </summary>
  public DirectionComponent Direction;

  /// <summary>
  /// Width in the shared geometry unit. Must be non-negative when bound.
  /// </summary>
  public int Width;

  /// <summary>
  /// Height in the shared geometry unit. Must be non-negative when bound.
  /// </summary>
  public int Height;

  /// <summary>
  /// Derived geometry; do not store or network-sync as a second authority.
  /// </summary>
  public Vector2 Center => new(
    Position.X + Width / 2.0f,
    Position.Y + Height / 2.0f);

  /// <summary>
  /// Derived geometry; do not store or network-sync as a second authority.
  /// </summary>
  public Vector2 Size => new(Width, Height);
}
```

字段说明：

- `Position`、`Velocity`、`Direction` 复用 NLTX 已有值类型作为迁移候选；Version4 的 `Vector2`/`int` 原型差异必须由 integration owner 处理。
- `Width`、`Height` 是 Version4 基类的共同几何字段候选；`0` 只能作为结构体未初始化值，不能宣称为行为默认。
- `Center` 和 `Size` 只由当前字段计算，不创建额外 Component。
- `PackedPositionOffset`、`PackedVelocity`、`netOffset`、`oldPos`、`oldRot` 和 `oldSpriteDirection` 不进入此 Component。
- 此代码块中的 `Position` 注释使用“top-left candidate”，因为完整参考的 `Center` 由 `position + width/height / 2` 计算；最终坐标语义仍需与 NLTX Geometry owner 统一。

### 4.4 `LeashedEntityAnchorRelationComponent`

建议文件：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityAnchorRelationComponent.cs`

```csharp
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.Dome.Simulation.Leash;

/// <summary>
/// Entity-side relation to a leashed anchor host.
/// status: proposed
/// componentOwner: LeashedEntitySimulation (entity-side relation)
/// crossSubsystemOwner: integration-review (TileEntity, WorldStorage, relation identity)
/// </summary>
public struct LeashedEntityAnchorRelationComponent
{
  public LeashedEntityAnchorRelationComponent(
    EntityReference? anchorReference,
    TileCoordinate? anchorCoordinate,
    TileEntityId? persistentAnchorId,
    long? relationRevision)
  {
    AnchorReference = anchorReference;
    AnchorCoordinate = anchorCoordinate;
    PersistentAnchorId = persistentAnchorId;
    RelationRevision = relationRevision;
  }

  /// <summary>
  /// Runtime relation to the anchor entity. This is not a persistence key.
  /// </summary>
  public EntityReference? AnchorReference;

  /// <summary>
  /// Tile-space relation fact and persistence candidate. Null means no accepted
  /// coordinate is currently bound; it does not mean coordinate (0, 0).
  /// </summary>
  public TileCoordinate? AnchorCoordinate;

  /// <summary>
  /// Candidate durable anchor identity. Owner is unresolved.
  /// </summary>
  public TileEntityId? PersistentAnchorId;

  /// <summary>
  /// Candidate relation revision for stale attach/remove rejection.
  /// Version4 does not provide an independent relation revision.
  /// </summary>
  public long? RelationRevision;

  /// <summary>
  /// Derived runtime-reference view; do not use it as a persistence check.
  /// </summary>
  public bool HasRuntimeAnchor =>
    AnchorReference.HasValue && !AnchorReference.Value.IsEmpty;
}
```

字段说明：

- `AnchorReference` 采用当前 NLTX `EntityReference` 候选，而不是把 `Arch.Core.Entity` 直接暴露为持久化字段。
- `AnchorCoordinate` 与 Version4 `AnchorPosition` 对应空间关系候选；它不等于 section coordinate。
- `PersistentAnchorId` 仅是 WorldStorage/整合 owner 的候选，不能由 runtime UUID、`whoAmI` 或 legacy slot 代替。
- `RelationRevision` 没有 Version4 直接证据，只有在 attach/remove 幂等策略确定后才能保留。
- 锚点 TileEntity 的 `Origin`、`ItemType`、item 内容、tile 合法性和宿主生命周期不复制到此 Component。
- `HasRuntimeAnchor` 只是派生运行时视图，不能证明 `PersistentAnchorId` 已存在。

### 4.5 `LeashedEntitySectionMembershipComponent`

建议文件：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntitySectionMembershipComponent.cs`

```csharp
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Leash;

/// <summary>
/// Entity-side section membership and local activity observation.
/// status: proposed
/// componentOwner: LeashedEntitySimulation (membership cache)
/// crossSubsystemOwner: integration-review (global section activity)
/// </summary>
public struct LeashedEntitySectionMembershipComponent
{
  public LeashedEntitySectionMembershipComponent(
    WorldSectionCoordinates section,
    int sectionSlot,
    bool isSectionActive,
    long? lastActivationTick)
  {
    Section = section;
    SectionSlot = sectionSlot;
    IsSectionActive = isSectionActive;
    LastActivationTick = lastActivationTick;
  }

  /// <summary>
  /// Current Dome section value. The cross-project SectionCoordinate mapping
  /// remains unresolved.
  /// </summary>
  public WorldSectionCoordinates Section;

  /// <summary>
  /// Reusable slot in the current section bucket; -1 means not indexed.
  /// </summary>
  public int SectionSlot;

  /// <summary>
  /// Local observation/cache only; not the global section activity authority.
  /// </summary>
  public bool IsSectionActive;

  /// <summary>
  /// Candidate observation tick. It is not a global clock owner.
  /// </summary>
  public long? LastActivationTick;

  /// <summary>
  /// Derived membership view; compaction may change the slot.
  /// </summary>
  public bool IsIndexed => SectionSlot >= 0;
}
```

字段说明：

- `Section` 采用 Dome 当前实际存在的 `WorldSectionCoordinates`，其分割规则来自 `WorldGrid.SectionWidth == 200` 和 `SectionHeight == 150`；本 Component 不自行重复计算全局 grid。
- `SectionSlot` 只代表 section bucket 内的可复用位置，不能作为实体身份、持久化 key 或协议 module ID。
- `IsSectionActive`、`LastActivationTick` 是本地观察/缓存候选；`ActiveSections`、`ActiveSectionList`、`RemoteClient` 的 global activity owner 不并入此 Component。
- `BySection` 和 `SectionEntityList` 是外部索引/工作集，不是实体 Component 的字段。
- `IsIndexed` 由 slot 派生，不能独立持久化。

### 4.6 `LeashedCritterBehaviorComponent`

建议文件：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedCritterBehaviorComponent.cs`

```csharp
using Terraria.WorldStorage;

namespace Terraria.Dome.Simulation.Leash;

/// <summary>
/// Critter-only behavior state for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation (critter-specific state)
/// crossSubsystemOwner: integration-review (NPC content, anchor style, random state)
/// </summary>
public struct LeashedCritterBehaviorComponent
{
  public LeashedCritterBehaviorComponent(
    int? npcType,
    int anchorStyle,
    bool isAquatic,
    TileCoordinate? targetPosition,
    uint randomState,
    short waitTime,
    byte state)
  {
    NpcType = npcType;
    AnchorStyle = anchorStyle;
    IsAquatic = isAquatic;
    TargetPosition = targetPosition;
    RandomState = randomState;
    WaitTime = waitTime;
    State = state;
  }

  /// <summary>
  /// NPC content id; not an NPC runtime instance id.
  /// </summary>
  public int? NpcType;

  /// <summary>
  /// Compatibility/style candidate. A zero value is not evidence of a source default.
  /// </summary>
  public int AnchorStyle;

  /// <summary>
  /// Critter behavior capability candidate. The struct default is not a confirmed
  /// prototype default; the source owner must provide this value before activation.
  /// </summary>
  public bool IsAquatic;

  /// <summary>
  /// Current behavior target. Null means no accepted target yet.
  /// </summary>
  public TileCoordinate? TargetPosition;

  /// <summary>
  /// Wire-compatible random cursor candidate; internal random type is unresolved.
  /// </summary>
  public uint RandomState;

  /// <summary>
  /// Behavior wait timer candidate.
  /// </summary>
  public short WaitTime;

  /// <summary>
  /// Critter behavior state value. Interpretation belongs to the selected critter definition.
  /// </summary>
  public byte State;

  /// <summary>
  /// Derived target-presence view only.
  /// </summary>
  public bool HasTargetPosition => TargetPosition.HasValue;
}
```

字段说明：

- `NpcType`、`AnchorStyle`、`IsAquatic`、`TargetPosition`、`RandomState`、`WaitTime` 和 `State` 来自完整参考 `LeashedCritter` 的字段分布与当前协议 state 形状，但默认值和最终 owner 仍是 partial。
- `RandomState` 是协议可携带的 `uint` 候选，不等同于 Version4 内部 `LCG32Random` 的完整语义；不得让表现随机或全局随机源覆盖它。
- `AnchorStyle` 可能最终属于 definition catalog 或 anchor relation；在 owner 裁决前保留为候选字段，不得与同一值在多个组件双写。
- `IsAquatic == false` 在隐式结构体默认中只是零值，不得解读成 prototype 已确认的“不具备水生能力”。
- `LeashedCritterBehaviorComponent` 与 `LeashedKiteBehaviorComponent` 互斥。
- `frame`、`frameCounter`、`spriteDirection`、`netOffset`、dummy NPC、绘制 offset 和历史轨迹不进入此 Component。

### 4.7 `LeashedKiteBehaviorComponent`

建议文件：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedKiteBehaviorComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.Leash;

/// <summary>
/// Kite-only behavior state for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation (kite-specific state)
/// crossSubsystemOwner: integration-review (projectile content and motion boundary)
/// </summary>
public struct LeashedKiteBehaviorComponent
{
  public const float DefaultKiteDistance = 250.0f;

  public LeashedKiteBehaviorComponent(
    int? projectileType,
    float rotation,
    float kiteDistance,
    float windTarget,
    float windCurrent,
    float timeCounter,
    int timeWithoutWind,
    float projectileLocalAI0,
    float projectileLocalAI1)
  {
    ProjectileType = projectileType;
    Rotation = rotation;
    KiteDistance = kiteDistance;
    WindTarget = windTarget;
    WindCurrent = windCurrent;
    TimeCounter = timeCounter;
    TimeWithoutWind = timeWithoutWind;
    ProjectileLocalAI0 = projectileLocalAI0;
    ProjectileLocalAI1 = projectileLocalAI1;
  }

  /// <summary>
  /// Projectile content id; not a projectile runtime entity id.
  /// </summary>
  public int? ProjectileType;

  /// <summary>
  /// Rotation candidate. Authority versus presentation ownership is unresolved.
  /// </summary>
  public float Rotation;

  /// <summary>
  /// Source-confirmed candidate default from the complete LeashedKite field.
  /// </summary>
  public float KiteDistance;

  /// <summary>
  /// Kite wind target candidate.
  /// </summary>
  public float WindTarget;

  /// <summary>
  /// Kite wind current candidate.
  /// </summary>
  public float WindCurrent;

  /// <summary>
  /// Kite behavior timer candidate; not a global tick.
  /// </summary>
  public float TimeCounter;

  /// <summary>
  /// No-wind timer candidate. Non-negative policy remains behavior-owned.
  /// </summary>
  public int TimeWithoutWind;

  /// <summary>
  /// Kite-local projectile-like AI state. It is not ordinary projectile authority.
  /// </summary>
  public float ProjectileLocalAI0;

  /// <summary>
  /// Kite-local projectile-like AI state. It is not ordinary projectile authority.
  /// </summary>
  public float ProjectileLocalAI1;
}
```

字段说明：

- `KiteDistance` 的 `250.0f` 是完整参考 `LeashedKite` 字段的直接初始化候选；这不是对其他未确认字段默认值的推断。
- `ProjectileType` 是 projectile content ID，不是普通 projectile runtime ID、network ID 或 entity UUID。
- `Rotation` 的角度单位、权威归属和表现归属仍是 evidence-gap；不得在整合前与 `DirectionComponent` 双写。
- `WindTarget`、`WindCurrent`、`TimeCounter`、`TimeWithoutWind`、`ProjectileLocalAI0` 和 `ProjectileLocalAI1` 是 kite behavior 候选字段，不接管普通 projectile 的 owner、damage、lifetime、registry 或生命周期。
- `CloudAlpha`、`frame`、`frameCounter`、`spriteDirection`、`oldPos`、`oldRot`、`oldSpriteDirection`、`netOffset` 不进入此 Component。
- `LeashedKiteBehaviorComponent` 与 `LeashedCritterBehaviorComponent` 互斥。

### 4.8 `LeashedEntityLegacySlotComponent`

建议文件：`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Leash\LeashedEntityLegacySlotComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.Leash;

/// <summary>
/// Compatibility mapping to Version4's reusable whoAmI slot.
/// status: proposed
/// componentOwner: LeashedEntitySimulation compatibility boundary
/// crossSubsystemOwner: integration-review (legacy slot and protocol/session)
/// </summary>
public struct LeashedEntityLegacySlotComponent
{
  public LeashedEntityLegacySlotComponent()
    : this(-1, 0)
  {
  }

  public LeashedEntityLegacySlotComponent(int slot, uint slotGeneration)
  {
    Slot = slot;
    SlotGeneration = slotGeneration;
  }

  /// <summary>
  /// Reusable Version4 whoAmI/ByWhoAmI slot. -1 means unassigned.
  /// </summary>
  public int Slot;

  /// <summary>
  /// Candidate reuse guard. Version4 does not provide a generation field.
  /// status: proposed; evidence-gap: no source generation exists.
  /// </summary>
  public uint SlotGeneration;

  /// <summary>
  /// Derived slot-assignment view only.
  /// </summary>
  public bool IsAssigned => Slot >= 0;
}
```

字段说明：

- `Slot` 对应 Version4 `whoAmI` 和 `ByWhoAmI` 的兼容映射候选，`-1` 表示未分配。
- `SlotGeneration` 是为了表达 slot reuse 风险而提出的候选字段；Version4 与完整参考都没有此字段，不得声称它已经存在于协议。
- `Slot` 不是 runtime UUID、persistent entity ID、persistent anchor ID、definition ID 或 module ID。
- Version4 module ID 是 `13`，而 slot 是每个实体的可复用索引；两者不放入同一字段。
- 旧协议的 stale packet 风险、generation 是否需要上 wire、以及 slot 释放顺序仍由 integration owner 裁决。

## 5. 组合草案

下表是实体 archetype 形状，不是运行时创建代码，也不规定系统顺序。

| Entity 形状 | 必需 Component | 可选 Component | 互斥 Component |
|---|---|---|---|
| critter Leashed entity | `State`、`Lifecycle`、`Motion`、`AnchorRelation`、`SectionMembership`、`CritterBehavior` | `LegacySlot`；通用 `EntityIdentityComponent` 仍由跨域身份决策决定 | `KiteBehavior` |
| kite Leashed entity | `State`、`Lifecycle`、`Motion`、`AnchorRelation`、`SectionMembership`、`KiteBehavior` | `LegacySlot`；通用 `EntityIdentityComponent` 仍由跨域身份决策决定 | `CritterBehavior` |
| Leashed anchor TileEntity | 现有 `TileEntityAnchorComponent`、`LeashedEntityAnchorComponent` | 与 entity-side relation 的外部关联 | 不附着以上 entity-side Component |

概念组合可表示为：

```text
critter entity
  ├─ LeashedEntityStateComponent
  ├─ LeashedEntityLifecycleComponent
  ├─ LeashedEntityMotionComponent
  ├─ LeashedEntityAnchorRelationComponent
  ├─ LeashedEntitySectionMembershipComponent
  ├─ LeashedCritterBehaviorComponent
  └─ LeashedEntityLegacySlotComponent (optional)

kite entity
  ├─ LeashedEntityStateComponent
  ├─ LeashedEntityLifecycleComponent
  ├─ LeashedEntityMotionComponent
  ├─ LeashedEntityAnchorRelationComponent
  ├─ LeashedEntitySectionMembershipComponent
  ├─ LeashedKiteBehaviorComponent
  └─ LeashedEntityLegacySlotComponent (optional)
```

组合不变量：

- 一个 Leashed entity 必须恰好拥有 critter 或 kite 行为 Component 之一；不能同时拥有或同时缺失两者。
- `DefinitionId` 无法解析时，不创建半初始化的行为组合。
- `active` 不作为 State、AnchorRelation 或 SectionMembership 的重复字段；它的唯一存储候选是 `Lifecycle.State`，其他位置只能有派生视图或观察缓存。
- `EntityIdentityComponent.UUID`、`LegacySlot.Slot`、`PersistentAnchorId`、`DefinitionId` 和协议 module ID 必须保持分离。
- Anchor TileEntity 的宿主状态和 Leashed entity 的关系状态可以同时存在，但不能把 `ItemType`、`Origin` 或 item 内容镜像进 entity-side Component。

## 6. 现有草图到代码草案的映射

| 当前文件 | 当前字段/内容 | 代码草案去向 | 处理结论 |
|---|---|---|---|
| `dome/src/Terraria.Dome.Simulation/Leash/LeashedEntityStateComponent.cs` | `InstanceId`、`TypeId`、`Anchor`、`AnchorTileX/Y`、`Active`、`LifecycleState`、`Spawned` | `TypeId → DefinitionId`；`Anchor → AnchorReference`；`AnchorTileX/Y → AnchorCoordinate`；生命周期字段 → Lifecycle；`InstanceId → LegacySlot` 候选 | 当前文件是混合草图，不能直接视为目标实现 |
| `dome/src/Terraria.Dome.Simulation/Leash/LeashedEntityLifecycle.cs` | 四值 enum | `LeashedEntityLifecycleComponent.State` | enum 可复用；转换 owner 仍未闭合 |
| `dome/src/Terraria.Dome.Simulation/Leash/LeashAnchorStateComponent.cs` | tile 坐标、`Entity?`、style、active、派生 section | `AnchorRelation`、`CritterBehavior.AnchorStyle` 候选、`SectionMembership` | 不把 `Arch.Core.Entity` 直接当成持久化身份 |
| `dome/src/Terraria.Dome.Simulation/Leash/LeashBehaviorRefComponent.cs` | behavior、style、NPC/projectile type、aquatic | critter/kite 两个互斥行为 Component | 不保留一个同时承载两种 content kind 的引用组件 |
| `dome/src/Terraria.Dome.Simulation/Leash/LeashSectionIndexComponent.cs` | section、slot、active、last activation tick | `SectionMembership` | global section activity 不进入该 Component |
| `src/Share/Entity/Components/LocationComponent.cs` | X/Y | `Motion.Position` 候选 | 现有值类型被复用，但未证明 Leashed authority 已接线 |
| `src/Share/Entity/Components/VelocityComponent.cs` | X/Y | `Motion.Velocity` 候选 | 同上 |
| `src/Share/Entity/Components/DirectionComponent.cs` | `Horizontal` | `Motion.Direction` 候选 | 方向语义需行为核对 |
| `src/Relationships/EntityReference.cs` | Guid + scope | `AnchorRelation.AnchorReference` | runtime relation，不是 durable ID |

## 7. 不纳入 Component 的对象

以下对象在代码草案中故意没有类型字段：

| 对象 | 不纳入原因 |
|---|---|
| Version4 `Registry.Prototypes` 和各类 prototype | 这是静态 definition 目录，不是每个 entity 的实例状态；entity 只保存 `DefinitionId` |
| `BySection`、`ActiveSectionList`、`SectionEntityList` | 这是索引、工作集和可压缩 bucket，不是 Component authority |
| `TELeashedEntityAnchor`、`TELeashedEntityAnchorWithItem`、`TEKiteAnchor`、`TECritterAnchor` | 这是 TileEntity 宿主、tile 合法性、item 内容和触发边界；不能持有 entity 的 motion/lifecycle |
| `ItemType`、`Origin` 和 tile placement data | 属于 anchor host；entity relation 只保存关系候选 |
| `LeashedEntityModulePacket`、full/partial/remove payload | 是协议表示，不是 authority；wire packed 值不进入组件 |
| protocol module ID `13`、session ID、socket state | 属于 Protocol/Session 层，不等于实体 slot 或 definition |
| dummy NPC、dummy projectile | 是表现/兼容计算对象，不嵌入 Component |
| `frame`、`frameCounter`、`spriteDirection`、`CloudAlpha` | 当前证据不足以证明是 Leashed authority；优先视作表现状态 |
| `oldPos`、`oldRot`、`oldSpriteDirection`、`netOffset` | 是历史、插值或传输缓存，不应反向写模拟状态 |
| `ProjectileAttachmentKind.Leashed` | 只表示 projectile 侧 attachment 分类，不接管 Leashed 的 identity、anchor 或 lifecycle |

## 8. Version4 证据与当前处理

代码形状使用以下只读证据作为字段来源：

| 证据 | 直接支持的字段/结论 | 当前限制 |
|---|---|---|
| `D:\TRbackup\Version4\Terraria.GameContent\LeashedEntity.cs` | registry 占位和注册顺序、`whoAmI`、`active`、`Type`、`AnchorPosition`、`SectionCoordinates`、section slot 和 section 索引 | 主文件的若干 mutation、remove、stream 和行为路径不完整，不能从缺失实现推导完整行为 |
| `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\LeashedEntity.cs` | 完整参考的基类几何字段、`Center`/`Size` 计算、slot add/remove/lookup 形状 | 仅用于补足同签名成员的参考，不能改写 Version4 主基线的当前行为状态 |
| `D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedCritter.cs` | `anchorStyle`、`npcType`、`rand`、`WaitTime`、`State`、`TargetPosition`、`isAquatic` 等 critter 候选字段 | 内部随机类型、默认值、行为 owner 和表现边界仍 partial |
| `D:\TRbackup\Version4\Terraria.GameContent.LeashedEntities\LeashedKite.cs` | `projType`、`rotation`、`kiteDistance = 250f`、wind/time/projectile-local-AI 候选字段 | rotation authority、表现字段和普通 projectile 边界仍 partial |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\*.cs` | module 13 的 `Slot`、full/partial 类型、critter/kite payload 字段形状 | decoder 存在不等于 authoritative component apply 已接通 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldGrid.cs` | section 宽度 200、section 高度 150 和 `WorldSectionCoordinates` 使用方式 | 不裁决跨项目 `SectionCoordinate` 最终 owner |

协议层的关键分离保持如下：

```text
module id 13
  ≠ message type (Remove / FullSync / PartialSync)
  ≠ entity slot (whoAmI / LegacySlot.Slot)
  ≠ definition id (LeashedEntity.Type / State.DefinitionId)
  ≠ persistent anchor id
  ≠ runtime entity UUID
```

full payload 可以携带 slot、type、anchor X/Y 和 subtype state；partial payload 不携带 anchor 坐标。这个差异只影响后续 Adapter/Apply 边界，不允许通过在 Component 中复制 packet record 来解决。

## 9. 未决 owner 与 evidence-gap

| decision/gap | 影响 Component | 代码草案处理 | 不能提前定案的原因 |
|---|---|---|---|
| runtime UUID、persistent entity ID、legacy slot、network module ID 的关系 | State、LegacySlot、AnchorRelation | 分字段建模；LegacySlot 单独可选；不把 UUID 加入 State | Version4 只有可复用 `whoAmI`，NLTX UUID 与存档/协议映射未建立 |
| `AnchorReference`、`AnchorCoordinate`、`PersistentAnchorId` 和 `RelationRevision` 的 owner | AnchorRelation | 允许四类字段候选，但保持可空和注释 | Version4 直接确认的是 anchor position；restore/delete 方法和跨域 owner 未闭合 |
| section 全局活跃事实 | SectionMembership、Lifecycle | `IsSectionActive` 和 tick 只标本地观察 | Version4 同时存在 ActiveSections 与 RemoteClient 相关缓存 |
| `AnchorStyle` 是 definition 还是行为/关系状态 | CritterBehavior、AnchorRelation | 暂保留 critter 候选字段并注明 unresolved | 不能从当前混合 `LeashBehaviorRefComponent` 直接确定唯一 owner |
| critter `RandomState` 的真实类型和默认种子 | CritterBehavior | 使用协议兼容 `uint` 候选 | `LCG32Random` 内部语义不能由 wire 字段独立反推 |
| kite `Rotation` 的 authority | KiteBehavior、Motion | 留在 kite 候选字段，不并入 Direction | full/partial 语义与表现归属还没有统一证据 |
| `TransitionSequence` 是否需要 | Lifecycle | 候选字段，owner 未决 | Version4 没有独立 transition revision |
| `SlotGeneration` 是否上 wire | LegacySlot、Protocol Adapter | 候选字段，不声称是 Version4 字段 | 主基线与完整参考都没有 generation |

在这些 owner 决策完成前，以下行为均禁止从本代码草案推断：

- 已经创建了八个实际 Component；
- 已经完成旧 Leash 文件迁移；
- 已经建立 `Arch.Core.World` 的实体组合；
- 已经把协议 decoder 连接到 authoritative apply；
- 已经复现 Version4 的 section activation、remove、restore 或 stream 行为；
- 已经解决 stale slot、anchor recovery 或 partial packet 的幂等性。

## 10. 草案边界与未验证声明

本文件的代码块可以作为未来拆分 `.cs` 文件的起点，但当前仍存在以下明确限制：

1. 代码未写入项目源目录，因此不存在“代码已创建”的事实。
2. 代码未经过 C# 编译；类型引用、项目引用、nullable 行为和 Arch/ECS 接入均未验证。
3. 未运行 `dotnet restore`、`build`、`test`、`run`、`publish`、`pack` 或 `msbuild`。
4. 未创建测试，不提供状态转换、协议 apply、恢复、section compaction 或 stale packet 的测试证据。
5. 现有同名/近似草图被视为用户已有文件；本草案不覆盖、不重写、不删除它们。
6. 所有 `status: proposed`、`candidate`、`unresolved` 和 `evidence-gap` 标记必须在实际实现或迁移前重新审查。

交付结论：本文件只交付八个目标 Component 的接近可编译代码形状和其字段归属说明；它没有交付运行时组件、系统、协议适配器、测试或行为验证。
