# SpatialSimulation Component Code Draft

## 1. 草案元数据

~~~text
subsystemId: SpatialSimulation
taskNumber: 09
sourceDesign: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-spatial-simulation-component-design.md
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-spatial-simulation-public-decomposition.md
outputDraftPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-spatial-simulation-component-code-draft.md
draftScope: component-code-only
designStatus: decision-required
codeStatus: implemented-build-verified
implementationStatus: created-under-src
verificationStatus: build-only-tests-not-run
componentCount: 7
selectionMethod: 以已确认的 Component-only Design 为唯一结构输入；保留当前 NLTX 的 C# 命名和集合封装经验；对尚未裁决的 namespace、坐标类型、液体枚举、状态 owner 和默认值使用显式候选，不宣称可直接合并
~~~

本文件记录实际 C# 组件代码草案及其对应的源文件。代码块按独立类型组织，源文件已在 src/SpatialSimulation 领域项目中创建；本轮只完成组件层实现，没有接入实体运行时。

所有代码块中的架构类型和 owner 仍标记为 status: proposed。对应 .cs 与 .csproj 已创建，但没有声明行为等价、API 兼容或运行时接入已经完成。

## 2. 草案边界与代码约定

### 2.1 只包含组件

本草案只包含以下七个组件类型：

| 设计名称 | 代码类型名称 | 组件 ID | 状态 |
|---|---|---|---|
| MovementState | MovementStateComponent | SPATIAL.COMP.MOVEMENT_STATE | proposed |
| CollisionShape | CollisionShapeComponent | SPATIAL.COMP.COLLISION_SHAPE | proposed |
| SpatialReference | SpatialReferenceComponent | SPATIAL.COMP.SPATIAL_REFERENCE | proposed |
| MotionHistory | MotionHistoryComponent | SPATIAL.COMP.MOTION_HISTORY | proposed |
| CollisionPolicy | CollisionPolicyComponent | SPATIAL.COMP.COLLISION_POLICY | proposed |
| LiquidContact | LiquidContactComponent | SPATIAL.COMP.LIQUID_CONTACT | proposed |
| CollisionResult | CollisionResultComponent | SPATIAL.COMP.COLLISION_RESULT | proposed |

不在本草案中加入执行逻辑、空间规则、世界 Tile 状态、液体传播状态、网络或存档字段，也不加入其他运行时类型。

### 2.2 实际代码布局

以下路径是本轮实际创建的代码落位；canonical namespace、共享 owner 和跨项目依赖仍需结合 BD-COMP-02 和 EG-COMP-09 裁决。

| 代码类型 | 暂定路径 | 暂定 namespace | 主要整合风险 |
|---|---|---|---|
| MovementStateComponent | src/SpatialSimulation/MovementStateComponent.cs | Terraria.SpatialSimulation.Components | 当前 Position/Velocity 分别位于根共享组件，owner 未决 |
| CollisionShapeComponent | src/SpatialSimulation/CollisionShapeComponent.cs | Terraria.SpatialSimulation.Components | 当前 ColliderComponent 位于 EntityEcs.Components，canonical namespace 未决 |
| SpatialReferenceComponent | src/SpatialSimulation/SpatialReferenceComponent.cs | Terraria.SpatialSimulation.Components | EntityReference、SpatialSpaceId 的共享 owner 未决 |
| MotionHistoryComponent | src/SpatialSimulation/MotionHistoryComponent.cs | Terraria.SpatialSimulation.Components | 根与 Dome 已有两套历史组件 |
| CollisionPolicyComponent | src/SpatialSimulation/CollisionPolicyComponent.cs | Terraria.SpatialSimulation.Components | 根与 Dome 字段集合和默认值不一致 |
| LiquidContactComponent | src/SpatialSimulation/LiquidContactComponent.cs | Terraria.SpatialSimulation.Components | LiquidKind 与 LiquidType、生成边界未决 |
| CollisionResultComponent | src/SpatialSimulation/CollisionResultComponent.cs | Terraria.SpatialSimulation.Components | TileCoordinate owner、集合生命周期和 struct/class 形状未决 |

源文件统一使用 Terraria.SpatialSimulation.Components，以保持本轮组件引用一致。该 namespace 已由新增领域项目使用，但仍不是最终跨项目 owner 决策。

### 2.3 代码级状态约定

- 每个代码块顶部都标记 status: proposed。
- compileStatus: draft-not-run 只表示本文件没有执行编译；它不是失败或成功结论。
- crossSubsystemOwner: integration-review 的类型和字段不在本草案内重新定义。
- 组件方法只负责构造、局部验证、快照替换和清理，不实现空间规则或跨实体行为。
- 所有浮点位置、速度、加速度、尺寸、偏移和法线在显式构造或替换入口校验有限性。
- 值类型的 default 仍可能绕过构造函数；这项语言语义不能由代码注释伪装成已经解决。

## 3. 依赖与未决外部类型

代码草案引用当前 NLTX 已存在或已被研究报告确认的类型：

| 类型 | 当前候选来源 | 草案用途 | 状态 |
|---|---|---|---|
| Vector2 | System.Numerics | 位置、速度、加速度、法线 | confirmed API type |
| GravityDirection | Terraria.Physics | 重力方向 | existing；owner 仍需整合 |
| SlopeCollisionMode | Terraria.Physics | 斜坡策略 | existing；字段集合未决 |
| CollisionAxisMask | Terraria.Physics | 碰撞轴结果 | existing |
| CollisionShapeKind | EntityEcs.Components | 碰撞形状种类 | existing；当前只确认 Rectangle |
| SpatialSpaceId | EntityEcs.Components | 空间标识 | existing；crossSubsystemOwner: integration-review |
| MotionHistoryKind | EntityEcs.Components | 历史采样种类 | existing；根/Dome namespace 未统一 |
| EntityReference | Terraria.Relationships | 父实体和接触实体引用 | existing；crossSubsystemOwner: integration-review |
| TileCoordinate | Terraria.WorldInteraction.Tiles 候选 | 接触 Tile 坐标 | 两个 namespace 重复；crossSubsystemOwner: integration-review |
| LiquidKind | EntityEcs.Components | 实体液体接触类型候选 | existing；与 Dome LiquidType 不等价 |

草案不重新声明以上 enum、ID 或值类型。若最终 owner 变更，只调整引用边界，不在本文件中制造第三套同义类型。

## 4. Component 代码草案

### 4.1 MovementStateComponent

#### 代码草案

~~~csharp
using System;
using System.Numerics;

using Terraria.Physics;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.MOVEMENT_STATE
// crossSubsystemOwner: integration-review
// compileStatus: draft-not-run
public struct MovementStateComponent
{
  public MovementStateComponent(
    Vector2 position = default,
    Vector2 velocity = default,
    Vector2 acceleration = default,
    GravityDirection gravityDirection = GravityDirection.Down,
    float gravityScale = 0.0f,
    bool isGrounded = false,
    bool isMovementLocked = false)
  {
    EnsureFinite(position, nameof(position));
    EnsureFinite(velocity, nameof(velocity));
    EnsureFinite(acceleration, nameof(acceleration));

    if (!float.IsFinite(gravityScale))
    {
      throw new ArgumentOutOfRangeException(
        nameof(gravityScale),
        gravityScale,
        "GravityScale must be finite.");
    }

    Position = position;
    Velocity = velocity;
    Acceleration = acceleration;
    GravityDirection = gravityDirection;
    GravityScale = gravityScale;
    IsGrounded = isGrounded;
    IsMovementLocked = isMovementLocked;
  }

  public Vector2 Position;
  public Vector2 Velocity;
  public Vector2 Acceleration;
  public GravityDirection GravityDirection;
  public float GravityScale;
  public bool IsGrounded;
  public bool IsMovementLocked;

  public bool IsGravityInverted =>
    GravityDirection == GravityDirection.Up;

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Vector components must be finite.");
    }
  }
}
~~~

#### 字段说明

- Position 和 Velocity 暂用 Vector2，对应 Version4 Entity.position 和 Entity.velocity。
- Acceleration、GravityDirection、GravityScale、IsGrounded 和 IsMovementLocked 延续当前 PhysicsStateComponent 的字段。
- GravityScale = 0.0f 只是草案构造默认；零值究竟代表无重力还是未初始化，仍属于 EG-COMP-01。
- struct 的 default 会产生零向量、Down、零重力缩放和 false 标志，但会绕过 EnsureFinite；当前候选字段没有引用型非法默认值。
- Position/Velocity 的唯一 owner 仍由 BD-COMP-01 决定，因此不能把此类型视为可以立即替代 LocationComponent 和 VelocityComponent。

#### 现有映射

- src/Physics/PhysicsStateComponent.cs:5-14：Acceleration、GravityDirection、GravityScale、IsGrounded、IsMovementLocked。
- src/Share/Entity/Components/LocationComponent.cs:3-13：LocationComponent 的 X/Y。
- src/Share/Entity/Components/VelocityComponent.cs:3-13：VelocityComponent 的 X/Y。
- Version4 Terraria/Entity.cs:10、:12：position、velocity。

### 4.2 CollisionShapeComponent

#### 代码草案

~~~csharp
using System;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.COLLISION_SHAPE
// crossSubsystemOwner: integration-review
// compileStatus: draft-not-run
public readonly record struct CollisionShapeComponent
{
  public CollisionShapeComponent(
    float width,
    float height,
    float offsetX = 0.0f,
    float offsetY = 0.0f,
    CollisionShapeKind kind = CollisionShapeKind.Rectangle,
    bool isEnabled = true)
  {
    EnsureFinite(width, nameof(width));
    EnsureFinite(height, nameof(height));
    EnsureFinite(offsetX, nameof(offsetX));
    EnsureFinite(offsetY, nameof(offsetY));

    if (width < 0.0f)
    {
      throw new ArgumentOutOfRangeException(
        nameof(width),
        width,
        "Width must not be negative.");
    }

    if (height < 0.0f)
    {
      throw new ArgumentOutOfRangeException(
        nameof(height),
        height,
        "Height must not be negative.");
    }

    Width = width;
    Height = height;
    OffsetX = offsetX;
    OffsetY = offsetY;
    Kind = kind;
    IsEnabled = isEnabled;
  }

  public float Width { get; }
  public float Height { get; }
  public float OffsetX { get; }
  public float OffsetY { get; }
  public CollisionShapeKind Kind { get; }
  public bool IsEnabled { get; }

  public bool HasArea =>
    IsEnabled && Width > 0.0f && Height > 0.0f;

  private static void EnsureFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Value must be finite.");
    }
  }
}
~~~

#### 字段说明

- Width、Height、OffsetX、OffsetY 使用当前 NLTX 的 float 形状表达。
- Kind 当前只确认 Rectangle；枚举存在不等于 Version4 已支持其他形状。
- IsEnabled 的显式构造默认沿用当前 ColliderComponent 的 true，但 default(CollisionShapeComponent) 仍是 false，形成 EG-COMP-02。
- HasArea 是派生属性，不是额外权威字段。
- Version4 Entity.width 和 Entity.height 是 int；转换、舍入和偏移映射不能由此代码草案自行决定。

#### 现有映射

- src/Share/Entity/Components/ColliderComponent.cs:3-33 已有相同的 float 尺寸、偏移、Rectangle 和 IsEnabled 形状。
- Version4 Terraria/Entity.cs:22-24 使用 int 的 width、height。
- Version4 Terraria/Entity.cs:48-183 的 Hitbox、Size 和边缘点属于派生几何，不作为额外字段复制。

### 4.3 SpatialReferenceComponent

#### 代码草案

~~~csharp
using System;

using EntityEcs.Components;
using Terraria.Relationships;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.SPATIAL_REFERENCE
// crossSubsystemOwner: integration-review
// compileStatus: draft-not-run
public struct SpatialReferenceComponent
{
  public SpatialReferenceComponent(
    EntityReference? parentEntity = null,
    float parentOffsetX = 0.0f,
    float parentOffsetY = 0.0f,
    bool isParentRelative = false,
    SpatialSpaceId space = SpatialSpaceId.World)
  {
    EnsureFinite(parentOffsetX, nameof(parentOffsetX));
    EnsureFinite(parentOffsetY, nameof(parentOffsetY));

    ParentEntity = parentEntity;
    ParentOffsetX = parentOffsetX;
    ParentOffsetY = parentOffsetY;
    IsParentRelative = isParentRelative;
    Space = space;
  }

  public EntityReference? ParentEntity;
  public float ParentOffsetX;
  public float ParentOffsetY;
  public bool IsParentRelative;
  public SpatialSpaceId Space;

  public EntityReference? ParentEntityId
  {
    get => ParentEntity;
    set => ParentEntity = value;
  }

  public bool IsWorldSpace =>
    Space == SpatialSpaceId.World && !IsParentRelative;

  private static void EnsureFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Value must be finite.");
    }
  }
}
~~~

#### 字段说明

- ParentEntity、父偏移、IsParentRelative 和 Space 必须共同表达空间关系。
- ParentOffsetX/Y 不是实体 Position，也不是 Tile 坐标。
- EntityReference 当前是 Guid 加 Scope 的关系值，不能改解释为 whoAmI、网络 ID 或持久化 ID。
- SpatialSpaceId、WorldSectionId/Coordinates 和 TileCoordinate 的共享 owner 仍由 BD-COMP-02 决定。
- 该 struct 的默认值自然对应 null、零偏移、false 和 World；但“非相对时是否强制清零偏移”仍未裁决。

#### 现有映射

- src/Share/Entity/Components/SpatialReferenceComponent.cs:5-20 已有相同字段和派生属性。
- src/Relationships/EntityReference.cs:5-10 定义了 Guid+Scope 的 EntityReference。
- src/Share/Entity/Components/SpatialSpaceId.cs:3-8 定义了 World 和 Subspace。

### 4.4 MotionHistoryComponent

#### 代码草案

~~~csharp
using System;
using System.Numerics;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.MOTION_HISTORY
// crossSubsystemOwner: integration-review
// compileStatus: draft-not-run
public struct MotionHistoryComponent
{
  public MotionHistoryComponent(
    Vector2 previousPosition = default,
    Vector2 previousVelocity = default,
    int previousDirection = 0,
    long? recordedAtTick = null,
    MotionHistoryKind kind = MotionHistoryKind.Tick)
  {
    EnsureFinite(previousPosition, nameof(previousPosition));
    EnsureFinite(previousVelocity, nameof(previousVelocity));
    EnsureValidTick(recordedAtTick, nameof(recordedAtTick));

    PreviousPosition = previousPosition;
    PreviousVelocity = previousVelocity;
    PreviousDirection = previousDirection;
    RecordedAtTick = recordedAtTick;
    Kind = kind;
  }

  public Vector2 PreviousPosition;
  public Vector2 PreviousVelocity;
  public int PreviousDirection;
  public long? RecordedAtTick;
  public MotionHistoryKind Kind;

  public MotionHistoryKind HistoryKind
  {
    get => Kind;
    set => Kind = value;
  }

  public void Record(
    Vector2 previousPosition,
    Vector2 previousVelocity,
    int previousDirection,
    long? recordedAtTick,
    MotionHistoryKind kind)
  {
    EnsureFinite(previousPosition, nameof(previousPosition));
    EnsureFinite(previousVelocity, nameof(previousVelocity));
    EnsureValidTick(recordedAtTick, nameof(recordedAtTick));

    PreviousPosition = previousPosition;
    PreviousVelocity = previousVelocity;
    PreviousDirection = previousDirection;
    RecordedAtTick = recordedAtTick;
    Kind = kind;
  }

  public void Clear()
  {
    PreviousPosition = Vector2.Zero;
    PreviousVelocity = Vector2.Zero;
    PreviousDirection = 0;
    RecordedAtTick = null;
    Kind = MotionHistoryKind.Tick;
  }

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Vector components must be finite.");
    }
  }

  private static void EnsureValidTick(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Tick must be non-negative.");
    }
  }
}
~~~

#### 字段说明

- PreviousPosition 和 PreviousVelocity 必须通过同一次构造或 Record 调用更新。
- PreviousDirection 是 Entity.oldDirection 的兼容候选；当前根/Dome 组件尚未统一拥有它。
- RecordedAtTick 为空表示尚未绑定明确采样 tick；非空时不得为负数。
- MotionHistoryKind.ExtraUpdate 不能仅凭普通 tick 整数隐式等同，Projectile 的 extra update 语义仍属于 EG-COMP-03。
- Record 和 Clear 只维护本组件快照，不回写当前运动状态。

#### 现有映射

- src/Share/Entity/Components/MotionHistoryComponent.cs:3-16 已有 PreviousPosition、PreviousVelocity、Kind 和 RecordedAtTick，但使用 LocationComponent/VelocityComponent。
- dome/src/Terraria.Dome.Simulation/Movement/Components/MotionHistoryComponent.cs:6-47 已有 Record、Clear、tick 和有限值校验。
- Version4 Terraria/Entity.cs:14、:16、:18 对应 oldPosition、oldVelocity、oldDirection。

### 4.5 CollisionPolicyComponent

#### 代码草案

~~~csharp
using Terraria.Physics;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.COLLISION_POLICY
// crossSubsystemOwner: integration-review
// compileStatus: draft-not-run
public struct CollisionPolicyComponent
{
  public CollisionPolicyComponent(
    bool collidesWithTiles,
    bool collidesWithEntities,
    bool ignoresLiquids,
    bool canFallThroughPlatforms = false,
    bool isFallingThroughPlatforms = false,
    bool ignoresDoors = false,
    bool ignoresAetheriumPlatforms = false,
    bool allowsHoikTraversal = true,
    SlopeCollisionMode slopeMode = SlopeCollisionMode.Default)
  {
    CollidesWithTiles = collidesWithTiles;
    CollidesWithEntities = collidesWithEntities;
    IgnoresLiquids = ignoresLiquids;
    CanFallThroughPlatforms = canFallThroughPlatforms;
    IsFallingThroughPlatforms = isFallingThroughPlatforms;
    IgnoresDoors = ignoresDoors;
    IgnoresAetheriumPlatforms = ignoresAetheriumPlatforms;
    AllowsHoikTraversal = allowsHoikTraversal;
    SlopeMode = slopeMode;
  }

  public bool CollidesWithTiles;
  public bool CollidesWithEntities;
  public bool IgnoresLiquids;
  public bool CanFallThroughPlatforms;
  public bool IsFallingThroughPlatforms;
  public bool IgnoresDoors;
  public bool IgnoresAetheriumPlatforms;
  public bool AllowsHoikTraversal;
  public SlopeCollisionMode SlopeMode;

  public bool CanApplyFallThrough =>
    CanFallThroughPlatforms && IsFallingThroughPlatforms;
}
~~~

#### 字段说明

- 三项基础开关和平台、门、hoik、斜坡策略沿用当前根 CollisionPolicyComponent 的字段形状。
- CanApplyFallThrough 是两个平台字段的派生判断，不是第三个权威布尔字段。
- Projectile.tileCollide、Projectile.ignoreWater、Projectile.correctSlopeCollision 和 Item.noWet 是实体域策略来源候选，不是本组件的行为实现。
- 本草案保留当前构造函数的必填基础开关；default(CollisionPolicyComponent) 会把它们置为 false，业务默认仍是 EG-COMP-07。
- 根与 Dome 的字段集合不完全一致，最终生产者和覆盖规则由 BD-COMP-05 决定。

#### 现有映射

- src/Physics/CollisionPolicyComponent.cs:3-39 已存在完整的根组件形状。
- dome/src/Terraria.Dome.Simulation/Physics/Components/CollisionPolicyComponent.cs:5-41 已存在 Dome 版本，但字段集合不同。
- Version4 Terraria/Projectile.cs:194、:202、:250 和 Terraria/Item.cs 约 :258 提供专用策略来源证据。

### 4.6 LiquidContactComponent

#### 代码草案

~~~csharp
using System;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.LIQUID_CONTACT
// crossSubsystemOwner: integration-review
// compileStatus: draft-not-run
public struct LiquidContactComponent
{
  public long? ResolvedAtTick;
  public bool IsWet;
  public bool IsLavaWet;
  public bool IsHoneyWet;
  public bool IsShimmerWet;
  public byte WetTickCount;
  public LiquidKind? DominantLiquidKind;

  public bool HasAnyLiquidContact =>
    IsWet || IsLavaWet || IsHoneyWet || IsShimmerWet;

  public bool HasNonWaterLiquidContact =>
    IsLavaWet || IsHoneyWet || IsShimmerWet;

  public void Replace(
    long tick,
    bool isWet,
    bool isLavaWet,
    bool isHoneyWet,
    bool isShimmerWet,
    LiquidKind? dominantLiquidKind = null)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(tick),
        tick,
        "Tick must be non-negative.");
    }

    if (dominantLiquidKind.HasValue &&
      !Enum.IsDefined(dominantLiquidKind.Value))
    {
      throw new ArgumentOutOfRangeException(
        nameof(dominantLiquidKind),
        dominantLiquidKind,
        "Liquid kind must be a defined value.");
    }

    bool hadContinuousContact =
      HasAnyLiquidContact &&
      ResolvedAtTick.HasValue &&
      ResolvedAtTick.Value + 1 == tick;

    IsWet = isWet;
    IsLavaWet = isLavaWet;
    IsHoneyWet = isHoneyWet;
    IsShimmerWet = isShimmerWet;

    bool hasContact = HasAnyLiquidContact;
    DominantLiquidKind = hasContact ? dominantLiquidKind : null;
    WetTickCount = hasContact
      ? hadContinuousContact
        ? (byte)Math.Min(byte.MaxValue, WetTickCount + 1)
        : (byte)1
      : (byte)0;
    ResolvedAtTick = tick;
  }

  public void Clear()
  {
    ResolvedAtTick = null;
    IsWet = false;
    IsLavaWet = false;
    IsHoneyWet = false;
    IsShimmerWet = false;
    WetTickCount = 0;
    DominantLiquidKind = null;
  }
}
~~~

#### 字段说明

- canonical 字段只保存实体侧液体接触派生状态，不保存 Tile.liquid 数量、液体传播、合并或提交集合。
- DominantLiquidKind 暂引用根 EntityEcs.Components.LiquidKind；根枚举含 Nano 哨兵，不能直接与 Dome LiquidType 等同。
- 草案没有加入根 LiquidComponent.InLiquid，避免形成 InLiquid 与 DominantLiquidKind 的双重权威来源；旧字段只作为后续兼容映射候选。
- Replace 以一次接触快照替换所有派生字段，并基于连续 tick 更新 WetTickCount；这部分沿用 Dome 组件的局部状态形状，但不代表 LiquidSimulation owner 已确定。
- ResolvedAtTick 的生产边界、快照类型和最终 owner 由 EG-COMP-04、BD-COMP-03 决定。

#### 现有映射

- src/Share/Entity/Components/LiquidComponent.cs:3-35 已有液体标志、DominantLiquidKind、InLiquid、LiquidTimer 和 ResolvedAtTick。
- dome/src/Terraria.Dome.Simulation/Liquid/Components/LiquidContactComponent.cs:5-59 已有 Replace、Clear、连续湿计数和 LiquidType?。
- Version4 Terraria/Entity.cs:26-34 与 Terraria/Collision.cs:944-1120 证明实体湿状态和液体接触计算存在。
- Version4 Terraria/Liquid.cs:14-69、:1015-1212 与 LiquidBuffer.cs 证明液体世界写集不应进入本组件。

### 4.7 CollisionResultComponent

#### 代码草案

~~~csharp
using System;
using System.Collections.Generic;
using System.Numerics;

using Terraria.Physics;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.COLLISION_RESULT
// crossSubsystemOwner: integration-review
// compileStatus: draft-not-run
public sealed class CollisionResultComponent
{
  private readonly List<EntityReference> _touchedEntities = new();
  private readonly List<TileCoordinate> _touchedTiles = new();

  public long? ResolvedAtTick { get; private set; }
  public Vector2 BlockingNormal { get; private set; }
  public CollisionAxisMask BlockedAxes { get; private set; }
  public bool DidStepUp { get; private set; }

  public IReadOnlyList<EntityReference> TouchedEntities =>
    _touchedEntities.AsReadOnly();

  public IReadOnlyList<TileCoordinate> TouchedTiles =>
    _touchedTiles.AsReadOnly();

  public bool CollidedOnX =>
    (BlockedAxes & CollisionAxisMask.Horizontal) !=
    CollisionAxisMask.None;

  public bool CollidedOnY =>
    (BlockedAxes & CollisionAxisMask.Vertical) !=
    CollisionAxisMask.None;

  public bool HasEntityContact => _touchedEntities.Count != 0;
  public bool HasTileContact => _touchedTiles.Count != 0;

  public void Replace(
    long tick,
    CollisionAxisMask blockedAxes,
    bool didStepUp,
    IReadOnlyList<TileCoordinate> touchedTiles,
    Vector2 blockingNormal = default,
    IReadOnlyList<EntityReference>? touchedEntities = null)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(tick),
        tick,
        "Tick must be non-negative.");
    }

    ArgumentNullException.ThrowIfNull(touchedTiles);
    EnsureFinite(blockingNormal, nameof(blockingNormal));
    EnsureKnownAxes(blockedAxes);

    _touchedTiles.Clear();
    for (int index = 0; index < touchedTiles.Count; index++)
    {
      _touchedTiles.Add(touchedTiles[index]);
    }

    _touchedEntities.Clear();
    if (touchedEntities is not null)
    {
      for (int index = 0; index < touchedEntities.Count; index++)
      {
        _touchedEntities.Add(touchedEntities[index]);
      }
    }

    ResolvedAtTick = tick;
    BlockingNormal = blockingNormal;
    BlockedAxes = blockedAxes;
    DidStepUp = didStepUp;
  }

  public void Clear()
  {
    _touchedEntities.Clear();
    _touchedTiles.Clear();
    ResolvedAtTick = null;
    BlockingNormal = Vector2.Zero;
    BlockedAxes = CollisionAxisMask.None;
    DidStepUp = false;
  }

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Vector components must be finite.");
    }
  }

  private static void EnsureKnownAxes(CollisionAxisMask value)
  {
    const CollisionAxisMask knownAxes =
      CollisionAxisMask.Horizontal | CollisionAxisMask.Vertical;
    if ((value & ~knownAxes) != CollisionAxisMask.None)
    {
      throw new ArgumentOutOfRangeException(
        nameof(value),
        value,
        "Collision axis mask contains unknown flags.");
    }
  }
}
~~~

#### 字段说明

- CollisionResultComponent 暂用 sealed class，因为内部接触集合需要稳定封装和防御性复制；这与根 src 当前的 struct 形状不同，是明确的代码整合风险。
- BlockedAxes 是轴阻挡的 canonical 候选；CollidedOnX 和 CollidedOnY 是派生兼容属性，不再保存第二套布尔字段。
- TouchedEntities 与 TouchedTiles 是本次结果快照，不是永久实体关系或 Tile 世界事实。
- Replace 接受只读输入后复制到内部 List；外部随后修改输入列表不会改变已经形成的结果。
- TouchedTiles 暂引用 Terraria.WorldInteraction.Tiles.TileCoordinate。Terraria.WorldStorage.TileCoordinate 同名类型的存在使该引用仍为 crossSubsystemOwner: integration-review。
- ResolvedAtTick、列表清理点和结果是否跨 tick 保留由 EG-COMP-05、BD-COMP-04 决定。

#### 现有映射

- src/Physics/CollisionResultComponent.cs:8-34 已有 EntityReference、TileCoordinate、BlockingNormal、BlockedAxes、CollidedOnX/Y、DidStepUp 和 ResolvedAtTick。
- dome/src/Terraria.Dome.Simulation/Physics/Components/ContactResultComponent.cs:9-74 已有内部 List、只读视图、Replace、Clear 和结果校验。
- Version4 Terraria/Collision.cs:11-44、:1472 起、:1633-1797 提供 TileContact、Tile 碰撞和轴结果证据。
- Version4 Terraria/NPC.cs 约 :6371-6373 的 collideX/collideY 作为实体域兼容状态，不强行成为新的 canonical 字段。

## 5. 组件组合代码视图

以下是组合关系的非运行时代码视图，只展示实体能力与组件的静态组合候选，不创建实体类型，也不表达执行顺序。

~~~text
Player:
  required: MovementStateComponent, CollisionShapeComponent, CollisionPolicyComponent
  optional: SpatialReferenceComponent, MotionHistoryComponent,
            LiquidContactComponent, CollisionResultComponent

NPC:
  required: MovementStateComponent, CollisionShapeComponent, CollisionPolicyComponent
  optional: SpatialReferenceComponent, MotionHistoryComponent,
            LiquidContactComponent, CollisionResultComponent

Projectile:
  required: MovementStateComponent, CollisionShapeComponent,
            CollisionPolicyComponent, MotionHistoryComponent
  optional: SpatialReferenceComponent, LiquidContactComponent,
            CollisionResultComponent

Item/WorldItem:
  required: MovementStateComponent, CollisionShapeComponent
  optional: SpatialReferenceComponent, MotionHistoryComponent,
            CollisionPolicyComponent, LiquidContactComponent,
            CollisionResultComponent

Tile/World/WorldSection:
  no entity components from this draft
~~~

组合约束：

- MovementStateComponent 不代替 CollisionShapeComponent。
- MotionHistoryComponent 不成为 Position/Velocity 的第二个权威来源。
- LiquidContactComponent 不挂载 Tile、World 或液体世界写集。
- CollisionResultComponent.TouchedEntities 不代替 SpatialReferenceComponent.ParentEntity。
- CollisionResultComponent.TouchedTiles 不成为 Tile 世界事实的缓存镜像。
- Player、NPC、Projectile、Item 的专用策略来源仍由各自实体域决定；本草案只提供统一字段承载候选。

## 6. 当前 NLTX 到代码草案的映射

| 现有类型 | 现状标记 | 草案目标类型 | 映射状态 | 关键差异 |
|---|---|---|---|---|
| LocationComponent | status: existing | MovementStateComponent.Position | status: partial | 当前是两个 float 字段，Position/Velocity owner 未决 |
| VelocityComponent | status: existing | MovementStateComponent.Velocity | status: partial | 当前是两个 float 字段，尚未与 Position 统一 |
| PhysicsStateComponent | status: existing | MovementStateComponent 其余字段 | status: partial | 当前没有 Position/Velocity，GravityScale 零值语义未锁定 |
| ColliderComponent | status: existing | CollisionShapeComponent | status: partial | 当前已有 float 形状，但 int 尺寸转换和 default struct 语义未锁定 |
| SpatialReferenceComponent | status: existing | SpatialReferenceComponent | status: partial | 类型名可复用候选，但 namespace 和共享 owner 未锁定 |
| 根 MotionHistoryComponent | status: existing | MotionHistoryComponent | status: partial | 当前使用 Location/Velocity，缺 PreviousDirection |
| Dome MotionHistoryComponent | status: existing | MotionHistoryComponent | status: partial | 已有 Record/Clear，但与根 namespace 和字段不统一 |
| 根 CollisionPolicyComponent | status: existing | CollisionPolicyComponent | status: partial | 字段较全，默认和最终生产者未锁定 |
| Dome CollisionPolicyComponent | status: existing | CollisionPolicyComponent | status: partial | 缺少根版本的基础开关 |
| 根 LiquidComponent | status: existing | LiquidContactComponent | status: partial | 根有 InLiquid/LiquidTimer，不能与 canonical 字段双重权威 |
| Dome LiquidContactComponent | status: existing | LiquidContactComponent | status: partial | 使用 LiquidType，与根 LiquidKind 不统一 |
| 根 CollisionResultComponent | status: existing | CollisionResultComponent | status: partial | 当前是 nullable List 的 struct，草案改为封装集合的 class 候选 |
| Dome ContactResultComponent | status: existing | CollisionResultComponent | status: partial | 封装方式接近草案，但名称、坐标类型和生命周期未定 |

本表是映射记录，不是迁移指令。现有类型仍保留在原位置，草案不声明任何替换已发生。

## 7. 代码级 evidence-gap 与 owner 决策

### 7.1 代码级 evidence-gap

| ID | 影响的代码草案 | 未决内容 | 对代码的影响 |
|---|---|---|---|
| EG-COMP-01 | MovementStateComponent | Position/Velocity 是否与 PhysicsState 统一 | 可能保留 Location/Velocity 分离，或改为 Vector2 统一字段 |
| EG-COMP-02 | CollisionShapeComponent | int 尺寸、float 尺寸、偏移和 struct default | 影响构造函数、字段类型、HasArea 和有效形状判定 |
| EG-COMP-03 | MotionHistoryComponent | oldDirection 与 Projectile extra update 生命周期 | 影响 PreviousDirection、Kind、RecordedAtTick 和 Record 语义 |
| EG-COMP-04 | LiquidContactComponent | LiquidKind、LiquidType 和 dominant 规则 | 影响 DominantLiquidKind 类型、枚举验证和兼容字段 |
| EG-COMP-05 | CollisionResultComponent | 接触集合容器、复制和清理生命周期 | 影响 class/struct 选择、Replace、Clear 和只读集合 API |
| EG-COMP-06 | SpatialReferenceComponent、CollisionResultComponent | Space、section 和 TileCoordinate 的关系 | 影响 using、namespace 和跨项目引用 |
| EG-COMP-07 | CollisionPolicyComponent | 根/Dome 默认值和字段集合 | 影响构造函数参数、默认值和最终策略字段 |
| EG-COMP-08 | CollisionShapeComponent、CollisionResultComponent | 空 Tile、actuator、half-brick、slope 的只读表达 | 不新增实体组件，但可能改变外部值类型依赖 |
| EG-COMP-09 | 全部组件 | 根/Dome canonical namespace 和依赖方向 | 影响每个草案类型的最终文件和项目归属 |

### 7.2 代码级 owner 决策

| ID | 冲突代码边界 | 候选 A | 候选 B | 当前代码暂定 |
|---|---|---|---|---|
| BD-COMP-01 | MovementState 与现有 Location/Velocity/PhysicsState | 统一由 MovementState 持有 | 保留现有分离组件 | 使用统一字段草案，但不声明 owner |
| BD-COMP-02 | EntityReference、Space、section、TileCoordinate | 共享空间/关系值类型 | SpatialSimulation 内部候选类型 | 引用现有类型，owner 标记 integration-review |
| BD-COMP-03 | LiquidContact 生成和字段 owner | SpatialSimulation 保存实体接触 | LiquidSimulation 直接维护 | 保留实体侧组件形状，生成边界未定 |
| BD-COMP-04 | CollisionResult 集合生命周期 | 严格本次结果快照 | 实体域保留兼容结果 | 使用 class + Replace/Clear 候选 |
| BD-COMP-05 | CollisionPolicy 最终生产者 | 各实体域提供策略 | SpatialSimulation 提供统一默认 | 保留当前构造形状，生产者未定 |

## 8. 代码草案风险与不变量清单

### 8.1 值类型默认值风险

MovementStateComponent、SpatialReferenceComponent、MotionHistoryComponent、CollisionPolicyComponent 和 LiquidContactComponent 采用可变 struct 候选，以贴合当前根项目的组件形状。调用方可以通过 default 或值复制绕过构造函数和局部校验。该行为是代码形状风险，不得被解释为不变量已经得到运行时保证。

CollisionShapeComponent 使用 readonly record struct 候选，并保留“显式构造默认 enabled、struct default disabled”的已知差异。CollisionResultComponent 使用 sealed class 候选，以避免内部集合因值复制而产生所有权歧义。

### 8.2 可变集合风险

CollisionResultComponent 不直接暴露内部 List<T>。Replace 将输入集合逐项复制，读取侧只拿到 IReadOnlyList<T> 视图。该封装只能保护容器结构；元素类型本身仍受 EntityReference 和 TileCoordinate owner 决策影响。

### 8.3 世界状态隔离

代码草案没有 Tile 数量、液体传播、WorldSection 写集、空 Tile 创建或静态 Collision flags 字段。Version4 Collision.WetCollision、SlopeCollision、TileCollision 等遗留副作用不能通过增加组件字段来隐藏。

### 8.4 类型和 namespace 风险

草案实际使用的 Terraria.SpatialSimulation.Components 是本轮新增领域项目的暂定 namespace。当前 NLTX 仍存在 Terraria.Physics、EntityEcs.Components 以及 Dome 的独立 namespace。EG-COMP-09 解决前，不应把该 namespace 解释为最终跨项目 canonical namespace。

### 8.5 API 兼容风险

- 根 LiquidComponent.InLiquid 没有进入 canonical 草案；若仍需兼容，必须增加明确的映射边界，不能让它和 DominantLiquidKind 同时写成权威字段。
- 根 CollisionResultComponent 的 nullable List、Dome ContactResultComponent 的封装集合和草案 sealed class 之间存在公开 API 差异。
- MotionHistoryComponent 从 LocationComponent/VelocityComponent 改为 Vector2 会影响调用方赋值方式。
- TouchedTiles 使用哪个 TileCoordinate 需要整合裁决，不能通过 namespace alias 静默掩盖同名类型。

## 9. 草案交付状态

~~~text
componentCodeDraft: implemented-under-src
sourceCodeFilesCreated: true
projectFilesModified: true
runtimeBehaviorChanged: false
behaviorEquivalenceClaim: none
compileStatus: build-verified
testStatus: not-run
verificationStatus: build-only-tests-not-run
unresolvedEvidenceGaps: 9
unresolvedOwnerDecisions: 5
~~~

本轮已将组件声明形状实现为 src/SpatialSimulation 下的实际源文件，并新增对应领域项目。实现仅完成组件层，未创建测试，未接入实体运行时，也没有宣称当前 NLTX 已经完成这七个组件的完整迁移。

## 10. 最终声明

本文件是 SpatialSimulation 的实际 Component Code Draft。

所有代码类型的架构状态和最终 owner 仍为 status: proposed。本文件记录的源文件已经创建并完成项目构建，但不声明行为等价、API 兼容、运行时迁移或测试通过。

本文件只定义组件字段、局部构造/校验、快照替换、清理和实体组合候选；不定义运行时执行结构、世界写入边界、网络/存档边界或其他非组件实现。

组件级 evidence-gap 数量：9。

组件级 owner 决策数量：5。

未启动子代理；已新增 src/SpatialSimulation 生产组件源码和项目文件；未创建或运行测试；已完成一次串行项目构建。
