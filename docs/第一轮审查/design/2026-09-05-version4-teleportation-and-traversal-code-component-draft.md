# TeleportationAndTraversal 实际代码组件草案

## 1. 文档元数据

~~~text
subsystemId: TeleportationAndTraversal
taskNumber: 14
sourceDesign: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-teleportation-and-traversal-component-design.md
sourceResearch: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-teleportation-and-traversal-public-decomposition.md
outputPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-teleportation-and-traversal-code-component-draft.md
draftStatus: implemented-provisional
implementationStatus: partial
designStatus: decision-required
evidenceStatus: partial
verificationStatus: production-build-only
writeScope: component source files under src; no test source was added
subagentStatus: not-started
~~~

本文是从 Component-only Design 推导出的 C# 组件代码骨架。对应的候选类型现在已经以 provisional 形式落在 src/，但代码块仍用于固定候选类型、字段和默认值，不能据此声明已经注册到 ECS、已经完成迁移或已经保持运行时行为等价。

本草案只覆盖以下 5 个组件：

1. PylonRegistryComponent
2. PortalEndpointComponent
3. PortalLinkStateComponent
4. TeleportCooldownStateComponent
5. PortalTraversalCooldownStateComponent

不在本草案中创建或模拟 System、Query、Command、Event、Adapter、Projection、调度顺序、主循环、网络发送、存档流程、旅行算法、位置迁移、碰撞处理或测试代码。

## 2. 草案使用规则

### 2.1 状态标记

| 标记 | 含义 |
|---|---|
| status: existing | 对应类型或局部字段已在某个现有工作区文件中出现；不表示根目录运行时已经接入 |
| status: partial | 有局部代码或字段形状，但 owner、生命周期、组合关系或行为覆盖仍不完整 |
| status: proposed | 目标类型/目标路径尚未在根目录创建 |
| status: unresolved | 仅有候选形状，类型、默认值或所有权尚未由证据锁定 |
| crossSubsystemOwner: integration-review | 该字段涉及跨 WorldStorage、Teleportation、主体领域、空间模拟或网络实体边界，需要整合裁决 |

### 2.2 代码块规则

- 代码块是“候选声明”，每个代码块顶部都标明候选文件路径和状态。
- 组件只保留持续状态、关系字段和纯派生属性；不把写入逻辑塞进组件。
- 默认值采用当前 NLTX 类型或结构默认值作为草案基线；标记为 unresolved 的默认值不能视为最终合同。
- 代码块中的 internal set、类型归一化和只读集合访问面只是候选实现形状，不表示已经决定唯一 writer。
- 现有根目录类型与 dome 类型并存时，以根目录命名空间和根目录公共类型为候选目标；dome 文件只作为局部证据，不能直接复制其 ECS 框架类型。

## 3. 组件总览

| componentId | 组件 | 候选文件路径 | entity/world 范围 | 当前状态 | 主要 unresolved |
|---|---|---|---|---|---|
| TT-COMP-01 | PylonRegistryComponent | src/WorldStorage/PylonRegistryComponent.cs | 一个 loaded world 的世界实体 | proposed；PylonRegistryState 为 partial | TileCoordinate owner、entry identity、revision 溢出/重置语义、集合 writer |
| TT-COMP-02 | PortalEndpointComponent | src/Teleportation/PortalEndpointComponent.cs | 一个 Portal endpoint entity | partial | Direction 类型和值域、Form 值域、支撑有效性、EntityReference 生命周期 |
| TT-COMP-03 | PortalLinkStateComponent | src/Teleportation/PortalLinkStateComponent.cs | 一个 endpoint 的可选 peer 关系 | proposed；dome counterpart 为 existing | PeerEndpoint 的实体引用类型、双向一致性、PortalGroup owner |
| TT-COMP-04 | TeleportCooldownStateComponent | src/Teleportation/TeleportCooldownStateComponent.cs | 一个 Player/NPC 等旅行主体 | proposed；TeleportCooldownState 为 partial | cooldown lane 共存策略、StartedAtTick owner、失败时写入语义 |
| TT-COMP-05 | PortalTraversalCooldownStateComponent | 根目录接入路径尚未锁定；候选为 src/Teleportation/PortalTraversalCooldownStateComponent.cs | 一个 Portal traversal subject | dome 为 existing，根目录 adoption 为 proposed | LastPortal 类型、SubjectKind owner、CooldownGroup 是否存在 |

所有 5 个组件的 crossSubsystemOwner 暂定为 integration-review。这表示草案保留了边界，不表示整合 owner 已经被指定。

## 4. 候选命名空间和辅助类型

### 4.1 已有根目录类型

草案依赖以下现有类型，但不在本文重复声明：

| 类型 | 当前路径 | 草案用途 | 状态 |
|---|---|---|---|
| Terraria.Relationships.EntityReference | src/Relationships/EntityReference.cs | Portal endpoint、peer 或最近 Portal 的实体关系候选 | existing；关系 scope 仍需整合确认 |
| Terraria.Relationships.EntityReferenceScope | src/Relationships/EntityReferenceScope.cs | 区分 Player、NPC、Projectile 等引用范围 | existing |
| Terraria.WorldStorage.PylonRegistryEntry | src/WorldStorage/PylonRegistryEntry.cs | Pylon registry 集合值 | existing；与 Version4 TeleportPylonInfo 不是已证明等价类型 |
| Terraria.WorldStorage.TileCoordinate | src/WorldStorage/TileCoordinate.cs | Pylon registry 位置候选 | existing；与另一个同名 TileCoordinate 存在 owner 冲突 |
| Terraria.WorldStorage.TileEntityId | src/WorldStorage/TileEntityId.cs | NLTX Pylon entry 的结构实体 ID 候选 | existing；Version4 没有对应字段 |
| Terraria.WorldInteraction.Tiles.TileCoordinate | src/WorldInteraction/Tiles/TileCoordinate.cs | 当前 Portal endpoint 支撑 Tile 字段类型 | existing；是否统一到 WorldStorage 类型未裁决 |
| Terraria.Teleportation.TeleportSource | src/Teleportation/TeleportSource.cs | 通用旅行冷却的来源值 | existing |

### 4.2 尚未锁定的辅助类型

下列形状可以出现在后续实现中，但本草案不新增对应 .cs 文件：

~~~text
PortalSubjectKind:
  status: proposed / partial in dome
  owner: Teleportation candidate
  unresolved: root namespace and whether subject category belongs in a component

Portal endpoint identity:
  candidates: EntityReference, Arch.Core.Entity, Projectile slot, project-specific entity ID
  status: unresolved
  crossSubsystemOwner: integration-review

PortalGroup:
  candidate type: int
  unresolved: relationship value, persistence ID, or network ID must not be conflated
~~~

## 5. 组件候选代码

### 5.1 PylonRegistryComponent

~~~text
componentId: TT-COMP-01
status: proposed
candidatePath: D:\TRbackup\NLTX\src\WorldStorage\PylonRegistryComponent.cs
namespace: Terraria.WorldStorage
componentOwner: WorldStorage candidate
crossSubsystemOwner: integration-review
entityScope: one world entity per loaded world
existingMapping: PylonRegistryState.cs is partial; dome counterpart is existing
~~~

职责：保存已经由结构边界提交的当前 Pylon 端点快照、用于差异计算的上一快照、registry 刷新节奏和 revision。Pylon TileEntity 的结构校验、掉落、损坏清理和宿主生命周期仍由 WorldStorage/WorldInteraction 的其他 owner 负责。

候选代码骨架：

~~~csharp
using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

// status: proposed
// componentId: TT-COMP-01
// crossSubsystemOwner: integration-review
// candidatePath: src/WorldStorage/PylonRegistryComponent.cs
// draft-only: accessors and collection writer are not final.
public sealed class PylonRegistryComponent
{
  // default: empty; current authoritative snapshot candidate.
  // invariant: entries must originate from a structurally valid Pylon source.
  // unresolved: duplicate identity is Position + Kind, TileEntityId, or another key.
  public IReadOnlyList<PylonRegistryEntry> CurrentPylons { get; internal set; } =
    Array.Empty<PylonRegistryEntry>();

  // default: empty; previous snapshot used only for registry diffing.
  // invariant: must never be treated as a second set of current teleport targets.
  public IReadOnlyList<PylonRegistryEntry> PreviousPylons { get; internal set; } =
    Array.Empty<PylonRegistryEntry>();

  // default: 0; registry refresh lifecycle state, not subject travel cooldown.
  // invariant: must be >= 0; 0 means the registry may be refreshed.
  public int RefreshCooldownTicksRemaining { get; internal set; }

  // default: 0; accepted-snapshot revision candidate.
  // unresolved: reset and uint overflow policy are not locked.
  public uint Revision { get; internal set; }

  // derived: do not store independently.
  public int Count => CurrentPylons.Count;

  // derived: preserves the current NLTX name/meaning pending owner review.
  // note: the name HasPendingRefresh is semantically counterintuitive.
  public bool HasPendingRefresh => RefreshCooldownTicksRemaining == 0;
}
~~~

字段和实现边界：

- CurrentPylons 与 PreviousPylons 都是集合快照，不是每个 Pylon 的独立 ECS component。
- RefreshCooldownTicksRemaining 只能控制 registry 刷新节奏，不能写入 Player/NPC 的任何 cooldown component。
- Revision 是候选权威版本；当前设计没有锁定 reset 是否归零、是否允许 uint 溢出或是否需要另一个版本类型。
- PylonRegistryEntry.Position 是 Tile 坐标，TileEntityId 是 NLTX 结构关系候选，二者都不是网络 ID。
- SceneMetrics、NPC 数量、Danger、Biome、资格结果和网络包字段不属于该组件。

默认值表：

| 成员 | 候选类型 | 草案默认值 | 状态 |
|---|---|---|---|
| CurrentPylons | IReadOnlyList<PylonRegistryEntry> | Array.Empty<PylonRegistryEntry>() | partial |
| PreviousPylons | IReadOnlyList<PylonRegistryEntry> | Array.Empty<PylonRegistryEntry>() | partial |
| RefreshCooldownTicksRemaining | int | 0 | partial |
| Revision | uint | 0 | partial |
| Count | int | derived | partial |
| HasPendingRefresh | bool | derived from ticks | partial |

### 5.2 PortalEndpointComponent

~~~text
componentId: TT-COMP-02
status: partial
candidatePath: D:\TRbackup\NLTX\src\Teleportation\PortalEndpointComponent.cs
namespace: Terraria.Teleportation
componentOwner: Teleportation candidate
crossSubsystemOwner: integration-review
entityScope: one Portal endpoint entity / Portal projectile host
existingMapping: root component already exists with a partial five-field shape
~~~

职责：保存一个 Portal endpoint 的几何、方向、形式、关联实体和支撑 Tile 事实。它不保存 peer 关系、主体 cooldown、Player/NPC 位置或速度。

候选代码骨架：

~~~csharp
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Teleportation;

// status: partial
// componentId: TT-COMP-02
// candidatePath: src/Teleportation/PortalEndpointComponent.cs
// crossSubsystemOwner: integration-review
// draft-only: validity and lifecycle wiring are intentionally omitted.
public struct PortalEndpointComponent
{
  // default: 0f; angle normalization range is unresolved.
  public float Angle;

  // default: 0; root and dome use different candidate shapes/defaults.
  // unresolved: int, sbyte, or a domain value type; valid value range is not locked.
  public int Direction;

  // default: 0; value domain must match the endpoint/Projectile form contract.
  public int Form;

  // default: EntityReference default (empty Guid / None scope candidate).
  // invariant: must not be interpreted as NetworkId, TileEntityId, or PortalGroup.
  public EntityReference PortalEntity;

  // default: (0, 0) struct default; zero as an invalid sentinel is unresolved.
  // unresolved: WorldInteraction.Tiles.TileCoordinate versus WorldStorage.TileCoordinate.
  public TileCoordinate SupportTile;

  // existing compatibility alias in the root file; retain only if public API review accepts it.
  // status: partial; not a second identity field.
  public EntityReference PortalEntityId
  {
    get => PortalEntity;
    set => PortalEntity = value;
  }
}
~~~

字段和实现边界：

- PortalEntity 是端点自身的实体关系候选，不能直接当作 PeerEndpoint。
- SupportTile 只是支撑 Tile 坐标，不是 TileEntityId；支撑是否仍有效由外部 owner 检查。
- Direction 的根目录类型为 int，dome 局部材料使用 sbyte 并有不同默认值，不能静默替换。
- 当前没有在组件里加入 Active、SupportedByTile 或 IsValid，因为这些字段的根目录接线和失效语义尚未形成已验证合同。
- endpoint 失效时需要清理本组件；peer 的双向清理由 PortalLinkStateComponent 的 owner 另行裁决。

默认值表：

| 成员 | 候选类型 | 草案默认值 | 状态 |
|---|---|---|---|
| Angle | float | 0f | partial |
| Direction | int（类型 unresolved） | 0 | version-drift |
| Form | int | 0 | partial |
| PortalEntity | EntityReference | struct default | partial |
| SupportTile | WorldInteraction.Tiles.TileCoordinate（owner unresolved） | struct default | partial |
| PortalEntityId | EntityReference alias | PortalEntity | existing / partial |

### 5.3 PortalLinkStateComponent

~~~text
componentId: TT-COMP-03
status: proposed
candidatePath: D:\TRbackup\NLTX\src\Teleportation\PortalLinkStateComponent.cs
namespace: Terraria.Teleportation
componentOwner: Teleportation candidate
crossSubsystemOwner: integration-review
entityScope: one Portal endpoint entity carrying an optional peer relation
existingMapping: dome component exists; root PortalNetworkState is a partial aggregate
~~~

职责：保存一个 endpoint 与 peer endpoint 的关系事实、关系分组和可派生完整性。它把“端点几何”与“端点配对”分开，避免端点自身变化时复制整组网络状态。

候选代码骨架：

~~~csharp
using Terraria.Relationships;

namespace Terraria.Teleportation;

// status: proposed
// componentId: TT-COMP-03
// candidatePath: src/Teleportation/PortalLinkStateComponent.cs
// crossSubsystemOwner: integration-review
// draft-only: EntityReference normalization and bidirectional writer are unresolved.
public struct PortalLinkStateComponent
{
  // default: 0; this is a relationship value, not a NetworkId.
  // unresolved: whether this value is persisted, generated, or derived.
  public int PortalGroup;

  // candidate normalization from dome Entity? / root PortalNetworkState EntityReference?.
  // default: null; non-null must point to a different live endpoint.
  // unresolved: EntityReference, Arch.Core.Entity, or project-specific entity handle.
  public EntityReference? PeerEndpoint;

  // derived: do not store independently.
  public bool IsPairComplete => PeerEndpoint.HasValue;
}
~~~

字段和实现边界：

- PeerEndpoint 与 PortalEndpointComponent.PortalEntity 表示不同关系，不能合并为一个字段。
- IsPairComplete 只能由 PeerEndpoint 派生，不能直接复制 Version4 的 world-level anyPortalAtAll。
- PortalGroup 不是 NetworkId、TileEntityId、Player ID 或 NPC ID；其 owner 尚未锁定。
- peer 被删除或失效时，至少当前 endpoint 的关系必须不再被视为完整；是否由一个唯一 writer 执行双向清理仍是 BD-COMP-03。
- 未能证明 peer 有效时必须保持 PeerEndpoint = null，不能根据单个 endpoint 的几何字段推断配对完成。

默认值表：

| 成员 | 候选类型 | 草案默认值 | 状态 |
|---|---|---|---|
| PortalGroup | int | 0 | partial |
| PeerEndpoint | EntityReference?（实体类型 unresolved） | null | unresolved |
| IsPairComplete | bool | derived from PeerEndpoint | partial |

### 5.4 TeleportCooldownStateComponent

~~~text
componentId: TT-COMP-04
status: proposed
candidatePath: D:\TRbackup\NLTX\src\Teleportation\TeleportCooldownStateComponent.cs
namespace: Terraria.Teleportation
componentOwner: Teleportation candidate
crossSubsystemOwner: integration-review
entityScope: one travel subject entity; candidate Player/NPC scope
existingMapping: TeleportCooldownState.cs is a partial ordinary state class, not an ECS Component
~~~

职责：保存旅行主体的通用离散旅行冷却、来源和开始 tick。它不保存 registry 刷新节奏，也不默认覆盖 Portal 专用的最近端点关系。

候选代码骨架：

~~~csharp
namespace Terraria.Teleportation;

// status: proposed
// componentId: TT-COMP-04
// candidatePath: src/Teleportation/TeleportCooldownStateComponent.cs
// crossSubsystemOwner: integration-review
// draft-only: the coexistence rule with PortalTraversalCooldownStateComponent is unresolved.
public struct TeleportCooldownStateComponent
{
  // default: 0; positive values mean the generic travel lane is active.
  // invariant: must never be negative; expiry returns to zero.
  public int RemainingTicks;

  // default: TeleportSource.None; does not represent registry refresh.
  // unresolved: whether Source must be reset to None whenever ticks reach zero.
  public TeleportSource Source;

  // default: null; clock owner and persistence/replay semantics are unresolved.
  public long? StartedAtTick;

  // derived: do not store independently.
  public bool IsOnCooldown => RemainingTicks > 0;
}
~~~

字段和实现边界：

- 组件挂在旅行主体实体上，不挂在 World、Pylon 或 Portal endpoint 上。
- 主体实体本身承载 Player/NPC identity；组件不重复保存 PlayerEntityId 或 NpcEntityId。
- TeleportSource 是领域值，不是网络 ID；PylonRegistryComponent.RefreshCooldownTicksRemaining 不得写入该字段。
- StartedAtTick 是审计/兼容候选。Version4 的 Portal 路径直接使用数组和 ticks 递减，没有证明相同的时间戳语义。
- 失败时是否消耗冷却、是否保留旧 Source，以及与 Portal 专用 cooldown 是否同时存在，属于 BD-COMP-04，不能由该骨架自行裁决。

默认值表：

| 成员 | 候选类型 | 草案默认值 | 状态 |
|---|---|---|---|
| RemainingTicks | int | 0 | partial |
| Source | TeleportSource | TeleportSource.None | partial |
| StartedAtTick | long? | null | partial / version-drift |
| IsOnCooldown | bool | derived from ticks | partial |

### 5.5 PortalTraversalCooldownStateComponent

~~~text
componentId: TT-COMP-05
status: partial in dome; proposed for root adoption
candidatePath: D:\TRbackup\NLTX\src\Teleportation\PortalTraversalCooldownStateComponent.cs
namespace: Terraria.Teleportation candidate
componentOwner: Teleportation candidate
crossSubsystemOwner: integration-review
entityScope: one Portal-traversal subject entity
existingMapping: dome component exists; root directory has no equivalent runtime wiring
~~~

职责：保存 Portal 穿越特有的冷却、最近端点、主体分类和分组关系。它不保存 Portal 几何、peer 关系、主体位置或速度。

候选代码骨架：

~~~csharp
using Terraria.Relationships;

namespace Terraria.Teleportation;

// status: proposed for root adoption; partial in dome
// componentId: TT-COMP-05
// candidatePath: src/Teleportation/PortalTraversalCooldownStateComponent.cs
// crossSubsystemOwner: integration-review
// draft-only: LastPortal and SubjectKind normalization are unresolved.
public struct PortalTraversalCooldownStateComponent
{
  // default: 0; must not share storage with registry refresh ticks.
  public int RemainingTicks;

  // candidate root representation; dome currently uses Arch.Core.Entity?.
  // default: null; non-null must refer to a still-valid Portal endpoint.
  // unresolved: EntityReference, Arch.Core.Entity, or another entity handle.
  public EntityReference? LastPortal;

  // default: PortalSubjectKind.Unknown; root owner and need for this field are unresolved.
  // Version4 expresses Player/NPC branches directly rather than this enum.
  public PortalSubjectKind SubjectKind;

  // default: null; Version4 does not prove this field exists.
  // unresolved: whether it is a PortalGroup, a cooldown lane, or should be removed.
  public int? CooldownGroup;

  // derived: do not store independently.
  public bool IsCoolingDown => RemainingTicks > 0;
}
~~~

候选辅助枚举（只用于说明字段 owner，不表示本文件会新增源码）：

~~~csharp
// status: unresolved; current shape exists only in dome material.
// candidate namespace: Terraria.Teleportation
public enum PortalSubjectKind : byte
{
  Unknown,
  Player,
  Npc,
  Projectile,
}
~~~

字段和实现边界：

- LastPortal 的存在和语义主要来自 dome 组织参考；Version4 直接确认的是 Player/NPC 的 Portal cooldown 数组，不是最近端点字段。
- SubjectKind 是行为分类，不是 Player/NPC/Projectile identity，不能替代实体引用。
- CooldownGroup 没有 Version4 的充分证据，暂时保留为 unresolved；如果最终删除，不能把 PortalGroup 直接改名复用。
- Portal traversal cooldown 与通用旅行 cooldown 不得共享同一个 RemainingTicks 存储。二者是共存、互斥还是优先级覆盖，需要 BD-COMP-04 的 owner 决策。
- 成功穿越、端点失效、主体 slot 重用和 tick 递减的具体状态转换不写在组件内。

默认值表：

| 成员 | 候选类型 | 草案默认值 | 状态 |
|---|---|---|---|
| RemainingTicks | int | 0 | partial |
| LastPortal | EntityReference?（与 dome Entity? 冲突） | null | unresolved |
| SubjectKind | PortalSubjectKind | Unknown | version-drift / partial |
| CooldownGroup | int? | null | unresolved |
| IsCoolingDown | bool | derived from ticks | partial |

## 6. Entity/World 组合草案

以下组合只描述组件挂载范围，不定义任何运行时 System 或执行顺序。

~~~text
World entity
  required candidate: PylonRegistryComponent
  forbidden: TeleportCooldownStateComponent, PortalTraversalCooldownStateComponent

Pylon TileEntity / structure host
  required Teleportation component: none
  relation: may be represented by PylonRegistryEntry.TileEntityId
  note: structure validity, destruction, drop, and tile ownership remain outside these components

Valid Portal endpoint entity
  required: PortalEndpointComponent
  optional: PortalLinkStateComponent
  note: an unpaired endpoint may still have valid endpoint geometry

Paired Portal endpoint entity
  required: PortalEndpointComponent + PortalLinkStateComponent
  invariant: PeerEndpoint must refer to a different valid endpoint

Player or NPC travel subject
  optional: TeleportCooldownStateComponent
  optional: PortalTraversalCooldownStateComponent
  unresolved: whether both may coexist is BD-COMP-04
  external owner: position, velocity, identity, and Player/NPC compatibility state
~~~

组合约束：

1. PylonRegistryComponent 只能有世界范围实例；它不是单个 Pylon TileEntity 的替代物。
2. PortalEndpointComponent 和 PortalLinkStateComponent 可以分开存在；端点本身不因尚未配对而消失。
3. TeleportCooldownStateComponent 和 PortalTraversalCooldownStateComponent 的存储不能合并；共存策略保持未决。
4. LocationComponent、SpatialReferenceComponent 或主体领域的位置字段不在这些组件中重复声明。
5. PortalNetworkState 的两个端点、两个支撑 Tile 和 UpdatedAtTick 是旧的聚合候选；草案只把它拆映射到多个端点实例、link 关系和外部时间 owner，不把它列为第六个组件。

## 7. ID、坐标和外部引用边界

| 数据 | 草案表示 | 不能等同于 | 当前裁决 |
|---|---|---|---|
| Pylon 位置 | TileCoordinate | TileEntityId、NetworkId | WorldStorage.TileCoordinate 与 WorldInteraction.Tiles.TileCoordinate 的统一 owner unresolved |
| Pylon 结构实体 | TileEntityId 候选 | Tile 坐标、NetworkId | NLTX 已有；Version4 TeleportPylonInfo 没有此字段 |
| Portal endpoint entity | EntityReference 候选 | Projectile slot、PortalGroup、NetworkId | EntityReference 与 dome Arch.Core.Entity 的归一化 unresolved |
| Portal peer | EntityReference? 候选 | endpoint 自身 PortalEntity | 双向清理和引用 scope unresolved |
| Portal grouping | int PortalGroup 候选 | NetworkId、TileEntityId | owner/persistence semantics unresolved |
| Travel source | TeleportSource | NetworkId、主体 ID | existing enum；lane owner unresolved |
| World tick | long? StartedAtTick 候选 | real-time timestamp、network sequence | 时钟 owner、持久化和恢复语义 unresolved |
| Subject category | PortalSubjectKind 候选 | Player/NPC entity ID | dome 局部类型；根目录 owner unresolved |

### 7.1 必须保留的 unresolved 标记

- EG-COMP-01：Pylon registry 的跨域 owner、registry entry identity 和重复项规则。
- EG-COMP-02：Portal endpoint identity、Projectile slot、EntityReference 和网络实体 ID 的映射。
- EG-COMP-03：Portal peer 关系的双向清理和 world-level lookup 是否需要额外缓存。
- EG-COMP-04：通用 travel cooldown 与 Portal traversal cooldown 的共存、互斥或覆盖策略。
- EG-COMP-05：Portal endpoint 的 Direction、Form、支撑有效性以及根目录/dome 的类型漂移。
- EG-COMP-06：StartedAtTick、CooldownGroup 和 LastPortal 是否为 Terraria Version4 的权威事实。

这些 unresolved 项不应通过把字段改名、改变 namespace 或直接复制 dome 类型来静默消除。

## 8. 现有文件映射

| 现有文件 | 代码草案中的处理 | 状态说明 |
|---|---|---|
| src/WorldStorage/PylonRegistryState.cs | 提供 PylonRegistryComponent 的局部字段基线 | 现有普通状态类；不是目标 Component |
| src/WorldStorage/PylonRegistryEntry.cs | 作为 CurrentPylons/PreviousPylons 的值类型候选 | 与 Version4 TeleportPylonInfo 的 identity 关系仍 partial |
| src/Teleportation/PortalEndpointComponent.cs | 保留现有五字段作为 partial skeleton | 不增加未经证实的 Active/validity 字段 |
| src/Teleportation/PortalNetworkState.cs | 拆分映射到 endpoint/link；不再作为第六个组件 | 现有聚合状态，目标归属 partial |
| src/Teleportation/TeleportCooldownState.cs | 提供通用 cooldown 字段基线 | 现有普通状态类；不等于 ECS Component |
| src/Teleportation/TeleportSource.cs | 作为 TeleportCooldownStateComponent.Source 的现有值类型 | Source 与 cooldown lane owner 未锁定 |
| dome/src/Terraria.Dome.Simulation/Teleportation/PylonRegistryComponent.cs | 仅作为集合封装、revision 和 clear 形状参考 | 不代表根目录 NLTX 已接入 |
| dome/src/Terraria.Dome.Simulation/Teleportation/PortalLinkStateComponent.cs | 作为 Portal link 局部字段证据 | Arch.Core.Entity 不能直接复制到根目录 |
| dome/src/Terraria.Dome.Simulation/Teleportation/PortalTraversalCooldownStateComponent.cs | 作为 Portal 专用 cooldown 局部字段证据 | 根目录 adoption 仍 proposed |
| dome/src/Terraria.Dome.Simulation/Teleportation/PortalSubjectKind.cs | 作为 SubjectKind 候选证据 | 根目录 namespace/owner unresolved |

## 9. 后续实现前的裁决清单

在继续扩大这些组件的运行时接入之前，至少需要完成以下裁决：

1. 确认 PylonRegistryComponent 的唯一世界实体 owner，以及 PylonRegistryEntry 的 identity 规则。
2. 确认 WorldStorage.TileCoordinate 和 WorldInteraction.Tiles.TileCoordinate 的边界/转换 owner。
3. 确认 PortalEndpointComponent.PortalEntity 与 PortalLinkStateComponent.PeerEndpoint 的引用类型、scope 和清理责任。
4. 决定 PortalGroup 是领域关系值、持久化 ID、网络 ID 还是不应保留的缓存字段。
5. 决定 TeleportCooldownStateComponent 与 PortalTraversalCooldownStateComponent 是否允许同一主体同时拥有。
6. 决定 StartedAtTick 是否是权威状态、审计字段、可选快照字段或应删除的兼容字段。
7. 为 LastPortal 和 CooldownGroup 提供 Version4/NLTX 的直接 owner 证据；没有证据时不能升级为 confirmed。
8. 确认 Direction、Form、PortalSubjectKind 的最终 namespace、类型和值域。
9. 为每个组件确定唯一 writer、创建/替换/清理时机以及失败时是否保持旧状态。
10. 完成 focused production verifier、owner 裁决和源码迁移记录后，才允许扩大运行时接入；本次用户明确排除测试项目。

## 10. 迁移边界和未包含内容

本轮实现没有执行以下动作：

- 已创建以下 provisional 组件相关源码：
  - src/WorldStorage/PylonRegistryComponent.cs
  - src/Teleportation/PortalLinkStateComponent.cs
  - src/Teleportation/TeleportCooldownStateComponent.cs
  - src/Teleportation/PortalTraversalCooldownStateComponent.cs
  - src/Teleportation/PortalSubjectKind.cs
- PortalEndpointComponent 已存在于 src/Teleportation/PortalEndpointComponent.cs，本轮保留其现有 partial 形状。
- 没有保留测试项目或测试源码；没有修改 dome/src/、dome/Test/ 或 Version4/完整参考源码。
- 没有将 PortalNetworkState、PylonRegistryState 或 TeleportCooldownState 删除、重命名或改写。
- 没有生成 System、Query、Command、Event、Adapter、Projection 或网络协议结构。
- 没有推断 Player/NPC 的 Portal 调用链，也没有把表现/兼容字段迁入 Teleportation component。
- 没有声明任何组件已注册到 ECS、已覆盖 Version4 行为或已通过测试。

## 11. 验证记录

~~~text
verificationStatus: production-build-only
compile: passed for src/Teleportation and src/WorldStorage
tests: not-written and not-run by user scope
source changes: provisional component files under src
subagents: none
~~~

本轮完成了文本级检查和两个受影响生产项目的串行编译。没有运行测试，也没有创建测试项目；不得把生产项目编译表述为行为验证或 Version4 等价性验证。

## 12. 最终声明

这是一份 TeleportationAndTraversal 的实际代码组件骨架草案。它的价值是把既有 Component-only Design 转成接近 C# 文件形状的候选声明，同时显式保留跨子系统 owner、类型漂移和生命周期未决项。

status: proposed、status: partial 和 status: unresolved 在完成 owner 裁决、源码接入、focused production verifier 和受控迁移之前不得升级为已完成或已验证。当前源码实现是 provisional，verificationStatus: production-build-only；测试状态按用户要求保持 not-written and not-run。
