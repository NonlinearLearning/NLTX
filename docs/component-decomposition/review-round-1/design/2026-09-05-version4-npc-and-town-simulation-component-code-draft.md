# NpcAndTownSimulation Component Code Draft

## 1. 文档元数据

| 项目 | 值 |
|---|---|
| `subsystemId` | `NpcAndTownSimulation` |
| `taskNumber` | `12` |
| `sourceComponentDesign` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-npc-and-town-simulation-component-design.md` |
| `sourceResearchReport` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-npc-and-town-simulation-public-decomposition.md` |
| `outputPath` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-npc-and-town-simulation-component-code-draft.md` |
| `draftScope` | `component-code-draft` |
| `componentCount` | `12` |
| `designStatus` | `decision-required` |
| `verificationStatus` | `not-run` |
| `implementationStatus` | `not-created` |
| `sourceCodeModified` | `false` |

本文件是面向 C# 实现者的组件代码草案。代码块尽量采用接近实际项目的命名空间、类型和字段形式，但不是可直接编译的提交物；所有 `status: proposed` 或 `status: partial` 的类型都必须经过 owner、值对象和项目引用裁决后才能落入 `.cs` 文件。

本次只生成 Markdown，不创建或修改 `.cs`、`.csproj`、测试文件、源生成文件或运行时文件。

## 2. 草案使用规则

### 2.1 代码形状

- 一个目标 Component 对应一个建议文件和一个核心 public 类型。
- NPC 个体组件放在 `src/Npc`；城镇居民组件放在 `src/Town`；世界住房集合组件放在 `src/WorldSession/WorldGeneration`。
- 代码块中的 `TODO(decision)` 表示当前证据不能替实现者决定；不得用猜测值替换。
- 代码块中的异常校验和数组长度校验是草案级不变量表达，不代表 Version4 行为已经得到独立验证。
- `EntityReference`、`NpcInstanceId`、`NpcSlot`、`NpcTypeId`、`NpcNetId`、`TownRoomTilePoint`、`TilePosition` 和现有枚举按当前源码引用；本文件不重新定义这些已有值对象。

### 2.2 状态分类

| 标记 | 含义 |
|---|---|
| `authoritative` | 组件候选中的权威状态；最终写入 owner 仍须满足跨子系统裁决 |
| `derived` | 可从其他字段计算，不得成为第二个可写权威源 |
| `compatibility` | 为 Version4 旧字段或旧索引保留的兼容状态 |
| `metadata` | 用于修订、时间或目录版本的辅助状态；不自动等同业务事实 |
| `unresolved` | 类型、默认值、owner 或生命周期证据不足，代码只保留占位 |

### 2.3 共享 ID 约束

`NpcInstanceId`、通用 Entity GUID、旧 NPC slot、持久化 ID、实例网络 ID、定义/变体 ID 和外部 session ID 必须保持不同语义。代码草案不会从 `whoAmI` 或 `NpcNetId` 推导稳定实例身份；相关共享 owner 统一标记为 `crossSubsystemOwner: integration-review`。

## 3. 现有类型引用与暂不重写的值对象

下列类型是代码草案的输入依赖，不是本文件新增的 Component：

| 类型 | 当前路径 | 草案用途 | 状态 |
|---|---|---|---|
| `NpcInstanceId` | `src/Npc/NpcInstanceId.cs:3-6` | NPC 稳定实例 ID 候选 | existing；Version4 稳定 ID 证据 missing |
| `NpcSlot` | `src/Npc/NpcSlot.cs:3-6` | NPC 旧数组槽位 | existing；与 `src/WorldStorage/NpcSlot.cs` 同名冲突 |
| `NpcTypeId` | `src/Npc/NpcTypeId.cs:3-6` | NPC 定义类型 | existing |
| `NpcNetId` | `src/Npc/NpcNetId.cs:3-6` | NPC 定义/变体 ID | existing；不是实例网络 ID |
| `NpcLifecycleStage` | `src/Npc/NpcLifecycleStage.cs:3-8` | 生命周期阶段 | existing；初始阶段仍需证据确认 |
| `NpcTargetKind` | `src/Npc/NpcTargetKind.cs:3-8` | 目标类别 | existing |
| `NpcDespawnReason` | `src/Npc/NpcDespawnReason.cs:3-12` | 离场原因候选 | existing；本次不放进目标生命周期 Component |
| `EntityReference` | `src/Relationships/EntityReference.cs:5-10` | 目标实体关系 | existing |
| `TownResidentCapabilities` | `src/Town/TownResidentCapabilities.cs:5-14` | 居民能力位集合 | existing |
| `TownHousingStatus` | `src/Town/TownHousingStatus.cs:3-9` | 住房状态兼容/派生表达 | existing；不得与住房权威字段双写 |
| `TownRoomTilePoint` | `src/Town/TownRoomTilePoint.cs:3` | NPC 住房坐标 | existing |
| `TilePosition` | `src/WorldSession/WorldGeneration/TilePosition.cs:3` | 世界住房索引坐标 | existing |
| `TownHousingResidentKey` | `src/WorldSession/WorldGeneration/TownHousingResidentKey.cs:3-5` | 住房 registry key | partial；当前仅固定 `NpcType` |

## 4. Component 文件布局草案

| Component | 建议路径 | status | 当前映射 |
|---|---|---|---|
| `NpcIdentityComponent` | `src/Npc/NpcIdentityComponent.cs` | `proposed` | `NpcEntityIdentityComponent` 中的 `InstanceId` |
| `NpcLegacySlotComponent` | `src/Npc/NpcLegacySlotComponent.cs` | `proposed` | `NpcEntityIdentityComponent` 中的 `LegacySlot` |
| `NpcDefinitionReferenceComponent` | `src/Npc/NpcDefinitionReferenceComponent.cs` | `partial` | 当前同名组件 |
| `NpcBehaviorComponent` | `src/Npc/NpcBehaviorComponent.cs` | `partial` | `NpcBehaviorStateComponent` 的直接字段 |
| `NpcLocalBehaviorStateComponent` | `src/Npc/NpcLocalBehaviorStateComponent.cs` | `proposed` | `NpcAiStateComponent` 的局部 AI 证据 |
| `NpcTargetComponent` | `src/Npc/NpcTargetComponent.cs` | `partial` | 当前同名组件，需补旧索引字段 |
| `NpcLifecycleComponent` | `src/Npc/NpcLifecycleComponent.cs` | `proposed` | 当前生命周期字段的重新分组 |
| `NpcParentRelationComponent` | `src/Npc/NpcParentRelationComponent.cs` | `proposed` | 当前同名关系组件 |
| `NpcHealthComponent` | `src/Npc/NpcHealthComponent.cs` | `partial` | 当前同名组件；与 Combat health 冲突 |
| `TownResidentComponent` | `src/Town/TownResidentComponent.cs` | `partial` | 当前同名组件 |
| `TownHousingRelationComponent` | `src/Town/TownHousingRelationComponent.cs` | `proposed` | `NpcHousingAssignmentComponent` |
| `TownHousingRegistryComponent` | `src/WorldSession/WorldGeneration/TownHousingRegistryComponent.cs` | `proposed` | `TownHousingRegistry` |

## 5. 代码草案公共约定

以下 using 和命名空间只表达代码草案中的引用关系，不构成项目文件修改方案：

```csharp
using System;
using System.Collections.Generic;
using Terraria.Npc;
using Terraria.Relationships;
using Terraria.Town;
using Terraria.WorldGeneration.Components;
```

草案默认遵守当前仓库的 C# 约束：PascalCase public 类型和成员、camelCase 参数、`_camelCase` 私有成员、2-space 缩进、控制流使用大括号、同名文件只放一个核心 public 类型。数组和集合的可变性、序列化形式、线程/实体容器集成方式仍不在本文件锁定。

## 6. Component 代码草案

### 6.1 NpcIdentityComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-IDENTITY` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `stateClassification` | authoritative |
| `lifecycle` | Entity 建立时创建；实体有效期内保持；实体清理时失效 |
| `blockingGap` | `EG-COMP-01`、`BD-COMP-01` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `InstanceId` | `NpcInstanceId` | `Value = 0` 表示 invalid；不自动生成 | authoritative / unresolved | 有效 NPC 必须为非零；Entity 生命周期内不改变 |

#### C# 草案

```csharp
using System;

namespace Terraria.Npc;

// status: proposed
// evidenceStatus: missing for Version4 stable instance identity
// crossSubsystemOwner: integration-review
public sealed class NpcIdentityComponent
{
  public NpcIdentityComponent(NpcInstanceId instanceId)
  {
    if (!instanceId.IsValid)
    {
      throw new ArgumentException(
        "NpcInstanceId must be valid for an initialized NPC entity.",
        nameof(instanceId));
    }

    InstanceId = instanceId;
  }

  public NpcInstanceId InstanceId { get; }
}
```

#### 实现备注

- 不把 `LegacySlot` 放回本类；它属于 `NpcLegacySlotComponent`。
- 不用 `EntityIdentityComponent.UUID`、`whoAmI` 或 `NpcNetId` 自动构造 `InstanceId`。
- 稳定 ID 的生成、恢复和跨域映射未决；草案故意要求调用方提供有效值，而不是擅自生成值。

### 6.2 NpcLegacySlotComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-LEGACY-SLOT` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `stateClassification` | compatibility |
| `lifecycle` | Entity 建立后可未分配；旧槽位分配或释放时更新 |
| `blockingGap` | `EG-COMP-01`、`BD-COMP-01` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `LegacySlot` | `NpcSlot` | `new NpcSlot(-1)` | compatibility | `Value >= 0` 才是已分配槽位；槽位变化不改变实例 ID |

#### C# 草案

```csharp
namespace Terraria.Npc;

// status: proposed
// evidenceStatus: confirmed for Version4 whoAmI/slot usage
// crossSubsystemOwner: integration-review
public sealed class NpcLegacySlotComponent
{
  public NpcLegacySlotComponent(NpcSlot legacySlot = default)
  {
    LegacySlot = legacySlot;
  }

  public NpcSlot LegacySlot { get; private set; }

  public bool IsAssigned => LegacySlot.IsAssigned;

  // Draft-only mutation seam. The final owner of slot allocation is unresolved.
  public void SetLegacySlot(NpcSlot legacySlot)
  {
    LegacySlot = legacySlot;
  }
}
```

#### 实现备注

- `default(NpcSlot)` 的实际数值由当前值对象默认构造决定；若实现需要明确 `-1`，应在值对象层提供命名的 unassigned 值，不在本 Component 中猜测。
- `src/WorldStorage/NpcSlot.cs` 存在另一个同名类型；落地前必须解决 namespace owner，不能仅依赖 using 顺序。

### 6.3 NpcDefinitionReferenceComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-DEFINITION` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `stateClassification` | authoritative definition reference + compatibility metadata |
| `lifecycle` | 定义初始化时创建；变体/恢复时更新；实体清理时移除 |
| `blockingGap` | `EG-COMP-05` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `TypeId` | `NpcTypeId` | invalid `Value <= 0` 不可作为已初始化定义 | authoritative | 必须是有效定义类型 |
| `NetId` | `NpcNetId` | `new NpcNetId(0)` | authoritative definition reference | 只表达定义/变体；不得当作实例网络 ID |
| `InitialTypeId` | `NpcTypeId` | 创建时通常等于 `TypeId`；恢复语义 unresolved | compatibility | 当前值与初始值不同必须可解释 |
| `CatalogRevision` | `int` | `0` 作为兼容草案值 | metadata / unresolved | 不得把未知版本静默解释为兼容 |

#### C# 草案

```csharp
using System;

namespace Terraria.Npc;

// status: partial
// evidenceStatus: confirmed for type/netID; partial for catalog revision
// crossSubsystemOwner: integration-review
public sealed class NpcDefinitionReferenceComponent
{
  public NpcDefinitionReferenceComponent(
    NpcTypeId typeId,
    NpcNetId netId,
    int catalogRevision = 0)
  {
    if (!typeId.IsValid)
    {
      throw new ArgumentException(
        "NpcTypeId must be valid.",
        nameof(typeId));
    }

    TypeId = typeId;
    NetId = netId;
    InitialTypeId = typeId;
    CatalogRevision = catalogRevision;
  }

  public NpcTypeId TypeId { get; private set; }

  public NpcNetId NetId { get; private set; }

  public NpcTypeId InitialTypeId { get; }

  public int CatalogRevision { get; }

  public bool UsesNetIdVariant => NetId.IsVariant;

  public bool IsInitialized => TypeId.IsValid && InitialTypeId.IsValid;
}
```

#### 实现备注

- Version4 的 `NPC.netID` 归入定义/变体引用；不要新增名为 `NetworkId` 的字段并填入同一个值。
- `CatalogRevision` 当前仅有 NLTX 局部证据；落地前需确认它是否为持久化字段、兼容字段或删除候选。

### 6.4 NpcBehaviorComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-BEHAVIOR` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `stateClassification` | authoritative |
| `lifecycle` | 定义初始化时创建；活动期更新；实体清理时移除 |
| `blockingGap` | `EG-COMP-07` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `AiStyle` | `int` | `0` 仅为当前草案候选 | authoritative | 必须可由 NPC 定义解释 |
| `Action` | `int` | `0` 仅为当前草案候选 | authoritative | 不承载生命周期阶段 |
| `AiSlots` | `float[]` | 四个 `0f` | authoritative | 长度固定为 4；槽位顺序稳定 |

#### C# 草案

```csharp
using System;

namespace Terraria.Npc;

// status: partial
// evidenceStatus: confirmed for aiStyle/aiAction/ai[4]
// crossSubsystemOwner: integration-review
public sealed class NpcBehaviorComponent
{
  public const int AiSlotCount = 4;

  public NpcBehaviorComponent(
    int aiStyle,
    int action,
    ReadOnlySpan<float> aiSlots)
  {
    if (aiSlots.Length != AiSlotCount)
    {
      throw new ArgumentException(
        $"NPC AI state must contain exactly {AiSlotCount} slots.",
        nameof(aiSlots));
    }

    AiStyle = aiStyle;
    Action = action;
    AiSlots = aiSlots.ToArray();
  }

  public int AiStyle { get; private set; }

  public int Action { get; private set; }

  public float[] AiSlots { get; }
}
```

#### 实现备注

- 不把当前 NLTX 的 `BehaviorKind` 和 `LastUpdatedTick` 自动加入权威字段。
- 不把 `localAI` 复用为 `AiSlots` 的第二个名称；两组数组必须有不同 Component。
- 若未来需要数组不可变视图，应由项目已有集合约定裁决，当前草案不引入新的共享容器类型。

### 6.5 NpcLocalBehaviorStateComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-LOCAL-BEHAVIOR` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `stateClassification` | authoritative local state |
| `lifecycle` | NPC 建立时创建；活动期更新；实体清理时移除 |
| `blockingGap` | `EG-COMP-07` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `LocalAiSlots` | `float[]` | 四个 `0f` | authoritative local state | 长度固定为 4；不得默认为公开同步字段 |

#### C# 草案

```csharp
using System;

namespace Terraria.Npc;

// status: proposed
// evidenceStatus: confirmed for Version4 localAI field; restore semantics partial
// crossSubsystemOwner: integration-review
public sealed class NpcLocalBehaviorStateComponent
{
  public const int LocalAiSlotCount = 4;

  public NpcLocalBehaviorStateComponent(ReadOnlySpan<float> localAiSlots)
  {
    if (localAiSlots.Length != LocalAiSlotCount)
    {
      throw new ArgumentException(
        $"NPC local AI state must contain exactly {LocalAiSlotCount} slots.",
        nameof(localAiSlots));
    }

    LocalAiSlots = localAiSlots.ToArray();
  }

  public float[] LocalAiSlots { get; }
}
```

#### 实现备注

- `NpcAiStateComponent.Style`、`Timer` 和 `State0..State3` 与当前行为模型重叠，不能未经 owner 裁决直接复制进来。
- 是否恢复或保存 `localAI` 尚未由 Version4 当前证据闭合；草案不添加存档专用字段。

### 6.6 NpcTargetComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-TARGET` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `stateClassification` | authoritative relation + compatibility |
| `lifecycle` | 创建时为空；目标选择时更新；目标失效或实体清理时清除 |
| `blockingGap` | `EG-COMP-02`、`EG-COMP-07` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `TargetReference` | `EntityReference?` | `null` | authoritative relation | 非空 scope 必须与 `TargetKind` 相容 |
| `TargetKind` | `NpcTargetKind` | `NpcTargetKind.None` | authoritative classification | `None` 必须与空目标一致 |
| `LegacyTargetIndex` | `int` | `-1` | compatibility | `-1` 表示无旧目标；不能单独解释 Player/NPC |
| `PreviousLegacyTargetIndex` | `int` | `-1` | compatibility | 只表示前一旧编码，不代替当前关系 |

#### C# 草案

```csharp
using Terraria.Relationships;

namespace Terraria.Npc;

// status: partial
// evidenceStatus: partial because current NLTX lacks both legacy index fields
// crossSubsystemOwner: integration-review
public sealed class NpcTargetComponent
{
  public NpcTargetComponent(
    NpcTargetKind targetKind = NpcTargetKind.None,
    EntityReference? targetReference = null,
    int legacyTargetIndex = -1,
    int previousLegacyTargetIndex = -1)
  {
    TargetKind = targetKind;
    TargetReference = targetReference;
    LegacyTargetIndex = legacyTargetIndex;
    PreviousLegacyTargetIndex = previousLegacyTargetIndex;
  }

  public EntityReference? TargetReference { get; private set; }

  public NpcTargetKind TargetKind { get; private set; }

  public int LegacyTargetIndex { get; private set; }

  public int PreviousLegacyTargetIndex { get; private set; }

  public bool HasTarget =>
    TargetKind != NpcTargetKind.None && TargetReference.HasValue;
}
```

#### 实现备注

- 现有 `src/Npc/TargetingComponent.cs` 不能与本 Component 同时作为可写目标源。
- `SelectedAtTick` 是当前 NLTX 的记录性字段；它不进入本次最小权威字段，除非时间值 owner 和恢复语义被确认。
- `EntityReference.EntityId` 是通用 Entity GUID，不等同于 NPC 实例 ID。

### 6.7 NpcLifecycleComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-LIFECYCLE` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `stateClassification` | authoritative |
| `lifecycle` | Entity 建立时进入明确的初始阶段；活动期更新；清理完成后失效 |
| `blockingGap` | `EG-COMP-06` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `IsActive` | `bool` | `false` 作为未初始化草案值 | authoritative | 不得仅凭槽位有效判断活动状态 |
| `RemainingActiveTicks` | `int` | `0` | authoritative | 不得小于 0；耗尽转换仍需确认 |
| `Stage` | `NpcLifecycleStage` | 构造函数必填；不推断默认枚举项 | authoritative | 阶段与活动标志必须保持一致 |

#### C# 草案

```csharp
namespace Terraria.Npc;

// status: proposed
// evidenceStatus: partial for active/timeLeft to lifecycle mapping
// crossSubsystemOwner: integration-review
public sealed class NpcLifecycleComponent
{
  public NpcLifecycleComponent(
    bool isActive,
    int remainingActiveTicks,
    NpcLifecycleStage stage)
  {
    if (remainingActiveTicks < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(remainingActiveTicks),
        "Remaining active ticks cannot be negative.");
    }

    IsActive = isActive;
    RemainingActiveTicks = remainingActiveTicks;
    Stage = stage;
  }

  public bool IsActive { get; private set; }

  public int RemainingActiveTicks { get; private set; }

  public NpcLifecycleStage Stage { get; private set; }
}
```

#### 实现备注

- 当前 `src/Npc/NpcLifecycleComponent.cs` 的 `RemainingDespawnTicks` 不足以直接证明 `NPC.timeLeft` 的完整语义；名称调整必须保留映射说明。
- `NpcLifetimeComponent` 中的人口成本、剩余时间和离场原因不自动并入本 Component。
- `NpcDespawnReason` 不在此草案字段中，因为死亡、人口压力、住房失效和世界卸载的 owner 尚未闭合。

### 6.8 NpcParentRelationComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-PARENT-RELATION` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个子 NPC Entity |
| `stateClassification` | authoritative relation candidate + compatibility |
| `lifecycle` | 子实体建立时可为空；附着时创建/更新；父关系失效或清理时移除 |
| `blockingGap` | `EG-COMP-01`、`EG-COMP-06`、`BD-COMP-03` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `ParentInstanceId` | `NpcInstanceId` | `Value = 0` 表示无父关系 | authoritative relation / unresolved | 有效时必须指向 NPC 实例；稳定性未确认 |
| `ParentLegacySlot` | `NpcSlot` | 未分配槽位 | compatibility | 只用于旧 `realLife` 映射 |
| `AttachedAtTick` | `long` | `0`；世界 tick 起点 unresolved | metadata | 不得晚于当前世界时刻 |

#### C# 草案

```csharp
namespace Terraria.Npc;

// status: proposed
// evidenceStatus: partial; Version4 directly confirms realLife legacy relation only
// crossSubsystemOwner: integration-review
public sealed class NpcParentRelationComponent
{
  public NpcParentRelationComponent(
    NpcInstanceId parentInstanceId,
    NpcSlot parentLegacySlot,
    long attachedAtTick)
  {
    if (attachedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(attachedAtTick),
        "Attached tick cannot be negative in this draft.");
    }

    ParentInstanceId = parentInstanceId;
    ParentLegacySlot = parentLegacySlot;
    AttachedAtTick = attachedAtTick;
  }

  public NpcInstanceId ParentInstanceId { get; }

  public NpcSlot ParentLegacySlot { get; }

  public long AttachedAtTick { get; }

  public bool HasParentInstance => ParentInstanceId.IsValid;

  public bool HasLegacyParentSlot => ParentLegacySlot.IsAssigned;
}
```

#### 实现备注

- `NPC.realLife` 只支持旧父/关联索引证据；不能据此复制父实体生命值。
- `ParentInstanceId` 的默认无效值和跨加载保持规则依赖 `BD-COMP-01`。
- 反向子集合不进入本 Component，避免把集合范围索引和单实体关系混合。

### 6.9 NpcHealthComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `NPC-COMP-HEALTH` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个 NPC Entity |
| `stateClassification` | authoritative candidate |
| `lifecycle` | NPC 初始化时创建；生命变化期间更新；实体清理时移除 |
| `blockingGap` | `EG-COMP-06`、`EG-COMP-07`、`BD-COMP-03` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `CurrentLife` | `int` | `0` 作为值对象安全默认 | authoritative | 稳定状态候选为 `0 <= CurrentLife <= MaximumLife` |
| `MaximumLife` | `int` | `0` 作为未初始化候选 | authoritative | 不得为负；变化必须说明对当前生命的影响 |

#### C# 草案

```csharp
namespace Terraria.Npc;

// status: partial
// evidenceStatus: confirmed for NPC life/lifeMax; owner conflicts with Combat health
// crossSubsystemOwner: integration-review
public sealed class NpcHealthComponent
{
  public NpcHealthComponent(int currentLife, int maximumLife)
  {
    if (maximumLife < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(maximumLife),
        "Maximum NPC life cannot be negative.");
    }

    if (currentLife < 0 || currentLife > maximumLife)
    {
      throw new ArgumentOutOfRangeException(
        nameof(currentLife),
        "Current NPC life must be within the draft health bounds.");
    }

    CurrentLife = currentLife;
    MaximumLife = maximumLife;
  }

  public int CurrentLife { get; private set; }

  public int MaximumLife { get; private set; }

  public bool IsDead => CurrentLife <= 0;
}
```

#### 实现备注

- 当前 `src/Combat/HealthComponent.cs` 也有 `Current/Maximum`；落地时只允许一个权威当前/最大生命源。
- 上述边界校验是草案不变量，不证明 Version4 所有过渡状态都满足该边界；死亡、共享生命和多段 NPC 行为必须在 `BD-COMP-03` 后调整。
- `realLife` 不在本类中增加第二组生命字段。

### 6.10 TownResidentComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `TOWN-COMP-RESIDENT` |
| `status` | `partial` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个城镇居民 NPC Entity |
| `stateClassification` | authoritative resident capability |
| `lifecycle` | 居民资格确认时创建；定义变体/恢复时更新；资格移除或清理时移除 |
| `blockingGap` | `EG-COMP-04` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `IsTownResident` | `bool` | Component 存在即 `true` | authoritative | 无 Component 不得读取为居民 |
| `IsFriendly` | `bool` | `false` | authoritative | 不由住房结果推导 |
| `HousingCategory` | `int` | `0` | authoritative constraint | 只能由 NPC 定义解释 |
| `Capabilities` | `TownResidentCapabilities` | `None` | authoritative capability | 未设置能力不得推断资格 |

#### C# 草案

```csharp
namespace Terraria.Town;

// status: partial
// evidenceStatus: confirmed for townNPC/friendly/housingCategory; capability mapping partial
// crossSubsystemOwner: integration-review
public sealed class TownResidentComponent
{
  public TownResidentComponent(
    bool isFriendly,
    int housingCategory,
    TownResidentCapabilities capabilities)
  {
    IsTownResident = true;
    IsFriendly = isFriendly;
    HousingCategory = housingCategory;
    Capabilities = capabilities;
  }

  public bool IsTownResident { get; }

  public bool IsFriendly { get; private set; }

  public int HousingCategory { get; private set; }

  public TownResidentCapabilities Capabilities { get; private set; }

  public bool CanUseHousing =>
    (Capabilities & TownResidentCapabilities.CanUseHousing) != 0;

  public bool CanOpenDialogue =>
    (Capabilities & TownResidentCapabilities.CanOpenDialogue) != 0;
}
```

#### 实现备注

- 居民 Component 不保存 `HomeTile`、无家倒计时或 World/Town 房间索引。
- 当前 `Capabilities` 是 NLTX 设计值对象；它不能反向证明 Version4 私有资格算法。
- 空体的 `CheckDialogue` 和 `TrySyncingUniqueTownNPCData` 不能被当作额外居民字段证据。

### 6.11 TownHousingRelationComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `TOWN-COMP-HOUSING-RELATION` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | 单个城镇居民 NPC Entity |
| `stateClassification` | authoritative relation + metadata |
| `lifecycle` | 居民资格存在时创建；分配/驱逐/恢复时更新；居民移除或清理时清空 |
| `blockingGap` | `EG-COMP-03`、`EG-COMP-06`、`EG-COMP-07`、`BD-COMP-02` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `IsHomeless` | `bool` | `false` 作为候选初始化值 | authoritative | 稳定无家时不应同时持有已确认住房 |
| `HomeTile` | `TownRoomTilePoint?` | `null` | authoritative relation | 非空时必须代表有效住房坐标 |
| `HomelessDespawn` | `bool` | `false` | authoritative boundary state | 不等同于 `IsHomeless` |
| `HomeSearchTimeout` | `int` | `0` | authoritative | 不得小于 0 |
| `AssignmentRevision` | `uint` | `0` | metadata | 关系变化时单调递增；跨加载规则 unresolved |

#### C# 草案

```csharp
using System;
using Terraria.Town;

namespace Terraria.Town;

// status: proposed
// evidenceStatus: partial for normalized status and revision semantics
// crossSubsystemOwner: integration-review
public sealed class TownHousingRelationComponent
{
  public TownHousingRelationComponent(
    bool isHomeless,
    TownRoomTilePoint? homeTile,
    bool homelessDespawn,
    int homeSearchTimeout,
    uint assignmentRevision)
  {
    if (homeSearchTimeout < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(homeSearchTimeout),
        "Home search timeout cannot be negative.");
    }

    if (isHomeless && homeTile.HasValue)
    {
      throw new ArgumentException(
        "A stable homeless relation cannot also have an assigned home tile.",
        nameof(homeTile));
    }

    IsHomeless = isHomeless;
    HomeTile = homeTile;
    HomelessDespawn = homelessDespawn;
    HomeSearchTimeout = homeSearchTimeout;
    AssignmentRevision = assignmentRevision;
  }

  public bool IsHomeless { get; private set; }

  public TownRoomTilePoint? HomeTile { get; private set; }

  public bool HomelessDespawn { get; private set; }

  public int HomeSearchTimeout { get; private set; }

  public uint AssignmentRevision { get; private set; }

  public bool HasHome => HomeTile.HasValue && !IsHomeless;
}
```

#### 实现备注

- 当前 `NpcHousingAssignmentComponent.Status` 不直接复制进本类作为第二权威字段。
- `TownHousingStatus` 的 `LookingForHome` 和 `EvictionPending` 是否可由上述字段无损派生尚未确认；如不能无损表达，应保留为明确的兼容字段并记录 owner，而不是静默丢失。
- `TownRoomTilePoint` 只是位置值，不是住房实例 ID；合法房间判定不属于本 Component 的字段不变量实现。

### 6.12 TownHousingRegistryComponent

#### 元数据

| 项目 | 值 |
|---|---|
| `componentId` | `TOWN-COMP-HOUSING-REGISTRY` |
| `status` | `proposed` |
| `componentOwner` | `NpcAndTownSimulation`（candidate） |
| `crossSubsystemOwner` | `integration-review` |
| `entityScope` | World/Town 住房集合 |
| `stateClassification` | authoritative collection index + unresolved mode |
| `lifecycle` | 世界建立时初始化；住房事实变化时更新；保存/加载时恢复；世界卸载时销毁 |
| `blockingGap` | `EG-COMP-03`、`EG-COMP-06`、`BD-COMP-02` |

#### 字段草案

| 字段 | 类型 | 草案默认值 | 分类 | 草案不变量 |
|---|---|---|---|---|
| `RoomsByResidentKey` | `Dictionary<TownHousingResidentKey, TilePosition>` | 空字典 | authoritative index | 同一修订中一个 key 至多对应一个房间 |
| `HomelessResidentKeys` | `HashSet<TownHousingResidentKey>` | 空集合 | authoritative index / derived candidate | 稳定状态下不得与有房 key 同时出现 |
| `Revision` | `ulong` | `0` | metadata | 住房事实变化时单调递增 |
| `ResidentKeyMode` | `TownHousingKeyMode` | `Unresolved` | unresolved compatibility configuration | 所有 key 必须遵守同一裁决模式 |

#### 辅助值枚举草案

此枚举不是 Component；它只是让住房 key 未决状态在代码草案中可见。`Type`、`Instance` 和 `Dual` 的实际语义不能在本文件自行选定。

```csharp
namespace Terraria.WorldGeneration.Components;

// status: proposed supporting draft type
// crossSubsystemOwner: integration-review
public enum TownHousingKeyMode : byte
{
  Unresolved,
  Type,
  Instance,
  Dual,
}
```

#### C# 草案

```csharp
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

// status: proposed
// evidenceStatus: partial; Version4 type-level key is confirmed, final key mode unresolved
// crossSubsystemOwner: integration-review
public sealed class TownHousingRegistryComponent
{
  public Dictionary<TownHousingResidentKey, TilePosition> RoomsByResidentKey { get; } = new();

  public HashSet<TownHousingResidentKey> HomelessResidentKeys { get; } = new();

  public ulong Revision { get; private set; }

  public TownHousingKeyMode ResidentKeyMode { get; private set; } =
    TownHousingKeyMode.Unresolved;

  public int AssignedRoomCount => RoomsByResidentKey.Count;

  public int HomelessResidentCount => HomelessResidentKeys.Count;

  public bool IsEmpty =>
    RoomsByResidentKey.Count == 0 && HomelessResidentKeys.Count == 0;
}
```

#### 实现备注

- 当前 `src/WorldSession/WorldGeneration/TownHousingRegistry.cs` 已有相近字段，但目标类型需要明确 Component 语义和 owner。
- 当前 `TownHousingResidentKey` 只有 `NpcType`；在 `ResidentKeyMode` 裁决前不得把它扩展为实例 key，也不得假定一个 type 只能有一个居民。
- 房间扫描临时缓存不进入本 Component；本 Component 只保存明确的集合范围住房事实。

## 7. Entity 与 Component 组合草案

此处只表示静态字段组合，不表达执行顺序或调用关系。

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥约束 |
|---|---|---|---|
| 普通 NPC Entity | `NpcIdentityComponent`、`NpcLegacySlotComponent`、`NpcDefinitionReferenceComponent`、`NpcBehaviorComponent`、`NpcLocalBehaviorStateComponent`、`NpcTargetComponent`、`NpcLifecycleComponent`、`NpcHealthComponent` | `NpcParentRelationComponent` | `NpcIdentityComponent` 与通用 Entity identity 不互相替代 |
| 有父级的 NPC Entity | 普通 NPC 基础集合 | `NpcParentRelationComponent` | 无效父 ID 和有效父关系不能同时被解释为同一事实 |
| 城镇居民 NPC Entity | 普通 NPC 基础集合 | `TownResidentComponent`、`TownHousingRelationComponent` | 没有 `TownResidentComponent` 时不附着住房关系 |
| 有住房的居民 Entity | 普通 NPC 基础集合、居民和住房关系 | `NpcParentRelationComponent` | `IsHomeless = true` 与已确认有效 `HomeTile` 互斥 |
| 无住房的居民 Entity | 普通 NPC 基础集合、居民和住房关系 | 无 | 不单独创建“无家” Component |
| World/Town 住房集合 | `TownHousingRegistryComponent` | 无 | 不附着在单个 NPC Entity 上 |
| 同时具有通用 GUID 的 NPC Entity | 通用 `EntityIdentityComponent` + NPC 组件集合 | 无 | GUID 不得静默替换 `NpcInstanceId` |
| 具有通用生命模型的 NPC Entity | `NpcHealthComponent` 或通用 `HealthComponent` 的单一权威选择 | 另一种只能是只读映射 | 两个生命类型不得同时可写 |

## 8. 代码落地前的未决项

### 8.1 `BD-COMP-01`：稳定身份

草案暂时采用 `NpcIdentityComponent.InstanceId`，但要求由外部调用方注入有效 ID，不提供生成逻辑。最终需要在以下方案中裁决：

1. NPC 实例 ID为 NPC 域稳定身份，通用 Entity GUID 和持久化 ID保持独立映射。
2. 通用 Entity GUID 为唯一实例身份，NPC 实例 ID降为领域别名或兼容值。
3. 三种身份均保留，并增加显式映射规则。

该决定会改变 `NpcIdentityComponent` 的字段、构造函数和关系组件依赖，`crossSubsystemOwner: integration-review`。

### 8.2 `BD-COMP-02`：住房 key

草案将 `ResidentKeyMode` 保持为 `Unresolved`，避免把当前 `NpcType` 误判为最终 key。候选方案为 type key、instance key 或 dual key；这会改变 `TownHousingResidentKey` 的形状、registry 的索引数量和住房关系一致性，`crossSubsystemOwner: integration-review`。

### 8.3 `BD-COMP-03`：生命和父子共享关系

草案保留 `NpcHealthComponent`，但不复制 `realLife` 的生命值。最终需要在 NPC 专用生命、通用 Combat 生命、或单向映射加共享生命关系三种方案中裁决；该决定会改变 NPC 的必需 Component 组合，`crossSubsystemOwner: integration-review`。

### 8.4 `BD-COMP-04`：实例网络身份

草案把 `NpcNetId` 限制为定义/变体引用，不增加实例网络身份字段。由于 Version4 当前证据没有闭合 NPC 实例网络 ID，后续可能保持现清单、增加独立身份 Component，或复用通用 Entity 网络身份；`crossSubsystemOwner: integration-review`。

### 8.5 组件级 evidence-gap 索引

代码草案不隐藏来源 Component Design 中的证据缺口。以下 9 项必须在代码落地前继续保持可追踪：

| ID | 缺口 | 对代码草案的影响 |
|---|---|---|
| `EG-COMP-01` | Version4 没有稳定 NPC 实例 ID 或持久化 ID | `NpcIdentityComponent` 的生成、恢复和关系引用不能自行决定 |
| `EG-COMP-02` | 没有确认 NPC 实例网络 ID；接收边界证据不闭合 | `NpcNetId` 不能升级为实例网络身份字段 |
| `EG-COMP-03` | 住房 key 的 type/instance/dual 模式未裁决 | `TownHousingResidentKey` 和 registry 索引形状保持占位 |
| `EG-COMP-04` | 三个 Version4 hook 为空体或缺少预期语义证据 | 不从 hook 名称补造居民、同步或掉落字段 |
| `EG-COMP-05` | 完整参考缺少 NPC 成员级差异补证记录 | 不用完整参考源码静默补写 Version4 私有默认值或行为 |
| `EG-COMP-06` | 活动、计时、生命、父关系和清理写集未闭合 | 生命周期、生命和父关系构造约束仍可能调整 |
| `EG-COMP-07` | 当前 NLTX 存在 identity、AI、target、health 和住房状态重叠 | 不能让重叠类型同时成为可写权威源 |
| `EG-COMP-08` | 当前 checkout 缺少 `约束\\公共拆分约束.md` | 草案不能升级为项目约束已完整确认的基线 |
| `EG-COMP-09` | 本会话未运行构建、测试或验证程序 | 代码块不能被表述为编译通过或行为验证通过 |

## 9. 当前源码映射与禁止的直接替换

| 当前源码 | 草案处理 | 不能直接做的替换 |
|---|---|---|
| `src/Npc/NpcEntityIdentityComponent.cs:3-15` | 拆成身份与旧槽位两个目标 Component | 不能把当前合并类改名后宣称拆分完成 |
| `src/Npc/NpcBehaviorStateComponent.cs:3-23` | 映射 `AiStyle`、`Action`、四个公开 AI 槽位 | 不能把 `BehaviorKind`、`LastUpdatedTick` 当作 Version4 权威字段 |
| `src/Npc/NpcAiStateComponent.cs:3-26` | 作为本地 AI/重叠状态证据 | 不能与公开 AI 数组无条件合并 |
| `src/Npc/NpcTargetComponent.cs:5-25` | 保留关系和类别，补旧索引兼容区 | 不能与 `TargetingComponent` 同时可写 |
| `src/Npc/NpcLifetimeComponent.cs:3-29` | 保持为未决相邻状态 | 不能将人口成本、离场原因静默塞入生命周期 Component |
| `src/Npc/NpcHealthComponent.cs:3-15` | 保留为 NPC health 候选 | 不能与 `src/Combat/HealthComponent.cs:3-14` 同时作为权威源 |
| `src/Town/NpcHousingAssignmentComponent.cs:3-35` | 映射为住房关系草案 | 不能保留 `Status`、bool、坐标三个可写镜像 |
| `src/WorldSession/WorldGeneration/TownHousingRegistry.cs:5-14` | 映射为 World/Town Component 草案 | 不能在 key owner 未裁决前扩展 type key 语义 |
| `src/Npc/NpcReplicationDirtyState.cs:5-17` | 排除出 12 个权威 Component | 不能把 per-client 字典或脏标记塞入 NPC 身份/行为状态 |

## 10. 代码草案验收状态

| 检查项 | 结果 | 证据 |
|---|---|---|
| 12 个 Component 均有独立章节 | 通过 | 本文件第 6 节 `6.1` 至 `6.12` |
| 每个 Component 均有 `componentId`、`status`、owner、范围和生命周期 | 通过 | 本文件第 6 节各元数据表 |
| 每个 Component 均有字段类型、草案默认值和不变量 | 通过 | 本文件第 6 节各字段草案表 |
| 代码块未把稳定实例 ID、住房 key 或 health owner 未决项静默定案 | 通过 | 第 8 节；相关代码注释和 `integration-review` 标记 |
| 代码块可作为 `.cs` 直接编译 | 未声称 | 本文件定义为 non-compile-ready draft |
| 当前源码已被重命名、删除或替换 | 否 | `sourceCodeModified: false` |
| 构建、测试或验证程序已运行 | 否 | `verificationStatus: not-run` |

## 11. 最终声明

本文件是 `NpcAndTownSimulation` 的实际 C# 组件代码草案，不是生产实现。

草案中的代码块用于表达类型形状、字段类型、默认值候选、构造约束和组件组合；它们不能单独证明 Version4 行为等价、当前 NLTX 已迁移、API 兼容、持久化兼容或网络兼容。

所有 `status: proposed` 和 `status: partial` 的 Component 都需要在代码落地前完成 owner、ID、住房 key、生命边界和生命周期证据裁决。未决项统一保留为 `unresolved` 或 `crossSubsystemOwner: integration-review`，不得用实现者猜测填充。

本次只写入：

`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-npc-and-town-simulation-component-code-draft.md`

本次未修改生产源码、测试源码、dome 源码、项目文件、原研究报告或原 Component Design；未运行构建或测试，`verificationStatus` 保持 `not-run`；未启动子代理或创建其他会话。
