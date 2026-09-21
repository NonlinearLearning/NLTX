# WorldProgressionAndTransition 实际代码组件草案

## 1. 草案元数据

```text
subsystemId: WorldProgressionAndTransition
sourceDesign: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-world-progression-and-transition-component-design.md
draftPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-06-world-progression-and-transition-actual-code-component-draft.md
draftScope: component-code-shape-only
designStatus: proposed
implementationRoot: D:\TRbackup\NLTX\src\WorldSession\WorldProgression
implementationProject: D:\TRbackup\NLTX\src\WorldSession\Terraria.WorldSession.csproj
implementationStatus: source-created
verificationStatus: build-passed; test-not-created-by-request
sourceFilesCreated: true
runtimeBehaviorChanged: false
```

本文件把已确认的 Component-only 设计翻译为接近真实 C# 文件的代码形状。代码块先作为草案供评审，随后按用户指定的 `src` 根目录创建了对应的状态载体；本次没有创建 System、Query、Command、Adapter、Projection、存档、网络或运行时调度实现。

所有新类型均为 `status: proposed`。字段、枚举值、ID 结构和文件路径中仍带有 `decision-required` 或 `integration-review` 的部分，不得被视为已经锁定的公共 API。

## 2. 草案边界

### 2.1 包含内容

- 9 个目标 Component 的 C# 类型形状；
- Component 所需的最小状态枚举和关系值类型；
- 现有 `HardmodeOreTierState` 值对象的复用方式；
- 建议源码路径、命名空间和项目边界；
- 字段默认值、派生属性和数据级不变量的注释；
- 当前 NLTX 类型到代码草案的映射。

### 2.2 明确不包含

本草案不定义或实现以下内容：

- System、Query、Command、Event、Adapter 或 Projection；
- Hardmode 资格判断、计划生成、后台变换、Tile 写入或网络重同步算法；
- 调度顺序、线程、Task、锁、随机实例、异常对象或外部句柄；
- 存档编解码、网络协议、客户端消息和表现效果；
- 测试、verifier、迁移脚本、项目文件和注册代码。

Component 只承载状态。任何 I/O、时钟、随机性、日志、持久化、网络或 UI 副作用都必须在未来的显式边界中实现，而不能通过给 Component 增加方法或句柄来隐藏。

## 3. 逻辑 Component 到 C# 类型的映射

设计文档中的逻辑名称不带 `Component` 后缀；实际 C# 草案使用 `Component` 后缀，以区分现有状态值对象和 ECS 状态载体。

| componentId | 逻辑设计名 | C# 草案类型 | 建议源码路径 | 候选项目/命名空间 | 状态 |
|---|---|---|---|---|---|
| C-WPT-01 | `WorldHardmodeRuleState` | `WorldHardmodeRuleStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/WorldHardmodeRuleStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed |
| C-WPT-02 | `WorldInfectionPolicyState` | `WorldInfectionPolicyStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/WorldInfectionPolicyStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed |
| C-WPT-03 | `HardmodeOreTierState` | `HardmodeOreTierStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/HardmodeOreTierStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed |
| C-WPT-04 | `ProgressionTransitionState` | `ProgressionTransitionStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/ProgressionTransitionStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed |
| C-WPT-05 | `TransitionPlanState` | `TransitionPlanStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionPlanStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed |
| C-WPT-06 | `ProgressionCommitState` | `ProgressionCommitStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/ProgressionCommitStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed |
| C-WPT-07 | `TransitionRecoveryState` | `TransitionRecoveryStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionRecoveryStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed |
| C-WPT-08 | `RuntimeHardmodeCompatibilityState` | `RuntimeHardmodeCompatibilityStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/RuntimeHardmodeCompatibilityStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed |
| C-WPT-09 | `WorldMetadataHardmodeState` | `WorldMetadataHardmodeStateComponent` | `dome/src/Terraria.Dome.Simulation/WorldProgression/WorldMetadataHardmodeStateComponent.cs` | `Terraria.Dome.Simulation.WorldProgression` | proposed；WorldStorage owner 未决 |

### 3.1 路径选择说明

本草案把 9 个代码形状暂时放在 `WorldProgression` 领域目录中，保持一个固定子系统的组件边界集中可读。用户确认后，本次实际实现覆盖到 `src/WorldSession/WorldProgression/` 和 `Terraria.WorldSession.csproj`。`WorldMetadataHardmodeStateComponent` 的长期 owner 候选仍是 `WorldStorage`，因此它未来可能移动到 `src/WorldStorage/` 或对应的共享世界元数据项目。

如果发生移动，必须记录源路径、目标路径、项目依赖、命名空间和恢复/序列化影响；不能以文件枚举顺序表达运行时顺序，也不能在移动时顺便改变公共字段语义。

### 3.2 类型形态选择

本草案暂时使用 `sealed class` 而不是 `struct`，理由是这些状态具有世界级生命周期、需要避免无意的值拷贝，并且现有 C# 约束要求默认优先使用 class。只有经过 ECS 容器和访问模式验证后，才可以把某个组件改为 `record struct` 或其他值类型。

所有代码块使用 2 个空格缩进和完整大括号。每个代码文件只展示一个核心公开类型；共享枚举和值类型也按“一类型一文件”拆开列出。

## 4. 共享候选值类型和枚举

以下类型是组件字段所需的候选稳定类型。它们尚未决定最终 owner；任何跨 `WorldSession`、`WorldProgressionAndTransition`、`WorldGenerationAndEcology`、`WorldStorage` 或 `ExternalBoundaries` 的类型都保留 `crossSubsystemOwner: integration-review`。

### 4.1 `HardmodeRulePhase`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/HardmodeRulePhase.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public enum HardmodeRulePhase : byte
{
  PreHardmode,
  Announced,
  Committed,
}
```

`Announced` 是为兼容 Version4 在后台变换前提前写入 `Main.hardMode` 的候选阶段，不是 Version4 已有字段。若整合会话不接受双阶段语义，该枚举必须在实现前重新裁决。

### 4.2 `WorldProgressionTransitionKind`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/WorldProgressionTransitionKind.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
public enum WorldProgressionTransitionKind : byte
{
  None,
  Hardmode,
}
```

当前任务只覆盖 Hardmode。其他长期世界过渡不能因为共享此枚举就自动并入本子系统固定边界。

### 4.3 `TransitionPhase`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionPhase.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
public enum TransitionPhase : byte
{
  Idle,
  Requested,
  Planned,
  Executing,
  ReadyToCommit,
  Committed,
  Failed,
  Recovering,
}
```

该枚举只表达数据状态，不包含进入状态的行为、时间调度或副作用。

### 4.4 `TransitionCommitStatus`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionCommitStatus.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
public enum TransitionCommitStatus : byte
{
  NotStarted,
  Committed,
  Failed,
  Unknown,
}
```

`Unknown` 不等同于 `Failed`。结果不明时禁止通过默认值或恢复流程直接宣称提交没有发生。

### 4.5 `RecoveryPhase`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/RecoveryPhase.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
public enum RecoveryPhase : byte
{
  None,
  Recoverable,
  Terminal,
}
```

### 4.6 `TransitionFailureCode`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionFailureCode.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
public enum TransitionFailureCode : byte
{
  Unknown,
  EligibilityRejected,
  PlanningFailed,
  MutationFailed,
  CommitConflict,
  PersistenceFailed,
  ResynchronizationFailed,
}
```

这些是稳定分类候选，不是异常类型，也不应携带异常对象、堆栈、线程、文件路径或外部连接。

## 5. 共享候选 ID 和关系值类型

### 5.1 `WorldEntityId`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/WorldEntityId.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public readonly record struct WorldEntityId(long Value)
{
  public static WorldEntityId Empty { get; } = new(0);

  public bool IsEmpty => Value == 0;
}
```

### 5.2 `TransitionId`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionId.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public readonly record struct TransitionId(long Value)
{
  public static TransitionId Empty { get; } = new(0);

  public bool IsEmpty => Value == 0;
}
```

### 5.3 `PlanId`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/PlanId.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public readonly record struct PlanId(long Value)
{
  public static PlanId Empty { get; } = new(0);

  public bool IsEmpty => Value == 0;
}
```

### 5.4 `WorldSectionId`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/WorldSectionId.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public readonly record struct WorldSectionId(int X, int Y);
```

区段坐标是关系值，不代表区段内容，也不拥有 Tile 存储生命周期。

### 5.5 `WorldRevision`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/WorldRevision.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public readonly record struct WorldRevision(long Value)
{
  public static WorldRevision Empty { get; } = new(0);

  public bool IsEmpty => Value == 0;
}
```

Version4 和当前 NLTX 尚未证明存在 Hardmode 专用的全局 `WorldRevision`。该类型只是为计划基线和提交结果保留明确位置，不应在实现阶段凭空定义递增规则。

### 5.6 `WorldSectionVersion`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/WorldSectionVersion.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public readonly record struct WorldSectionVersion(
  WorldSectionId SectionId,
  long Version);
```

### 5.7 `TransitionSectionPrecondition`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionSectionPrecondition.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public readonly record struct TransitionSectionPrecondition(
  WorldSectionId SectionId,
  WorldSectionVersion ExpectedVersion);
```

该值只描述计划的预期版本，不拥有区段版本的写入权，也不复制区段内容。

### 5.8 `PersistentWorldId`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/PersistentWorldId.cs`

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldProgression;

// status: proposed
// crossSubsystemOwner: integration-review
public readonly record struct PersistentWorldId(Guid Value)
{
  public bool IsEmpty => Value == Guid.Empty;
}
```

当前 `WorldDescriptorState.WorldId` 和 `UniqueId` 已存在，但它们与存档 ID、外部会话 ID 的最终协议角色仍未由整合会话锁定。实现时不得未经裁决地把它们复制到所有 Component。

## 6. 目标 Component 代码草案

### 6.1 `WorldHardmodeRuleStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/WorldHardmodeRuleStateComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-01
// status: proposed
// componentOwner: WorldSession candidate
// crossSubsystemOwner: integration-review
// entityScope: active WorldEntity
public sealed class WorldHardmodeRuleStateComponent
{
  public bool IsHardMode { get; set; }
  public HardmodeRulePhase RulePhase { get; set; } = HardmodeRulePhase.PreHardmode;
  public TransitionId? LastCommittedTransitionId { get; set; }
  public long LastCommittedSequence { get; set; }
}
```

字段说明：

- `IsHardMode` 是活动世界的长期规则事实候选，默认值为 `false`；
- `RulePhase` 用于区分旧 Version4 早期兼容写入和最终提交，不能从 `Announced` 推断 Tile 已全部完成；
- `LastCommittedTransitionId` 只关联已确认提交，不能记录请求、计划或失败过渡；
- `LastCommittedSequence` 是候选的单调提交序列，不是 Version4 已有字段；
- 世界清理是新生命周期，不能通过普通更新把已进入 Hardmode 的世界静默改回 `false`。

### 6.2 `WorldInfectionPolicyStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/WorldInfectionPolicyStateComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-02
// status: proposed
// componentOwner: WorldSession / WorldGenerationAndEcology candidate
// crossSubsystemOwner: integration-review
// entityScope: active WorldEntity
public sealed class WorldInfectionPolicyStateComponent
{
  public bool IsInfectionSpreadAllowed { get; set; } = true;
}
```

字段说明：

- 该字段是世界范围的规则值，默认值 `true` 是当前 NLTX 状态的候选初始化语义；
- 生态预算、采样游标、感染 Tile 和单个 pass 不属于该 Component；
- 现有 `WorldEcologyScheduleState.IsInfectionSpreadAllowed` 只能在明确同步方向后作为读取镜像，不能形成无声明的第二写入根。

### 6.3 `HardmodeOreTierStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/HardmodeOreTierStateComponent.cs`

```csharp
using Terraria.Dome.Simulation.WorldGeneration;

namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-03
// status: proposed
// componentOwner: WorldGenerationAndEcology / WorldStorage candidate
// crossSubsystemOwner: integration-review
// entityScope: active WorldEntity
public sealed class HardmodeOreTierStateComponent
{
  public HardmodeOreTierState Value { get; set; } = HardmodeOreTierState.Uninitialized;
}
```

原始草案计划复用 dome 中的 `HardmodeOreTierState` 值对象；但用户要求实际代码放在 `src`，而 `Terraria.WorldSession.csproj` 不能反向引用 dome 项目。因此本次 `src` 实现提供了同一数据形状的项目内值对象，不声称 dome 类型已经迁移或两个项目已经统一 owner。实际值对象文件为：

`D:\TRbackup\NLTX\src\WorldSession\WorldProgression\HardmodeOreTierState.cs`

字段/值说明：

- `Value` 包含 `CobaltTileType`、`MythrilTileType` 和 `AdamantiteTileType`；
- `(-1, -1, -1)` 是 `src` 值对象的 `Uninitialized` 语义，不是声称 Version4 静态默认值已改变；
- 三阶矿阶作为一个值集合恢复和校验，不拆成三个独立 Component；
- `AltarCount`、随机实例和 Tile 内容都不放入该 Component；
- 矿阶候选与感染/墙体 Tile 变更是否共享原子提交，仍由 `BD-COMP-03` 裁决。

### 6.4 `ProgressionTransitionStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/ProgressionTransitionStateComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-04
// status: proposed
// componentOwner: WorldProgressionAndTransition candidate
// crossSubsystemOwner: integration-review
// entityScope: one active record per active WorldEntity
public sealed class ProgressionTransitionStateComponent
{
  public TransitionId? TransitionId { get; set; }
  public WorldEntityId? WorldEntityId { get; set; }
  public WorldProgressionTransitionKind TransitionKind { get; set; }
  public TransitionPhase Phase { get; set; }
  public long RequestedAtTick { get; set; }
  public PlanId? PlanId { get; set; }
  public int ActiveTransformationCount { get; set; }
  public bool IsTransforming => ActiveTransformationCount > 0;
}
```

字段说明：

- `TransitionId` 是一次 Hardmode 过渡的候选身份；
- `WorldEntityId` 必须指向承载该 Component 的活动世界；
- `Phase` 只表示数据状态，不包含状态转换逻辑；
- `ActiveTransformationCount` 对应 Version4 的 `_transformingWorld` 计数；
- `IsTransforming` 是派生值，不单独持久化，也不应被独立写入；
- 不保存 `Task`、锁、线程句柄、回调、异常对象或随机实例。

候选数据约束：当 `Phase == TransitionPhase.Idle` 时，`TransitionId`、`PlanId` 必须为空且 `ActiveTransformationCount` 必须为零；`ActiveTransformationCount` 不得为负。由于这些约束目前没有运行时验证实现，本代码块不添加构造函数或校验方法。

### 6.5 `TransitionPlanStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionPlanStateComponent.cs`

```csharp
using System.Collections.Immutable;
using Terraria.Dome.Simulation.WorldGeneration;

namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-05
// status: proposed
// componentOwner: WorldProgressionAndTransition / WorldGenerationAndEcology candidate
// crossSubsystemOwner: integration-review
// entityScope: one immutable plan snapshot per active transition
public sealed class TransitionPlanStateComponent
{
  public PlanId PlanId { get; set; }
  public TransitionId TransitionId { get; set; }
  public WorldRevision? BaseWorldRevision { get; set; }
  public ulong BaseGenerationRevision { get; set; }
  public ulong RandomCursor { get; set; }
  public ImmutableArray<WorldSectionId> AffectedSections { get; set; } = ImmutableArray<WorldSectionId>.Empty;
  public ImmutableArray<TransitionSectionPrecondition> SectionPreconditions { get; set; } = ImmutableArray<TransitionSectionPrecondition>.Empty;
  public int ExpectedBatchCount { get; set; }
  public HardmodeOreTierState OreTierCandidate { get; set; } = HardmodeOreTierState.Uninitialized;
  public bool RequiresResync { get; set; }
}
```

字段说明：

- `AffectedSections` 只保存区段关系，不保存每个 Tile；
- `SectionPreconditions` 只保存计划基线，不拥有区段版本写入权；
- `RandomCursor` 只保存可重放计划元数据，不保存 `UnifiedRandom` 或其他随机对象；
- `OreTierCandidate` 是计划候选，不能在提交前覆盖 `HardmodeOreTierStateComponent.Value`；
- `RequiresResync` 只有在范围和提交策略明确后才能作为可靠结果使用；
- 计划建立后原则上只读，但本草案不使用 `init` 或只读包装，以避免在未确定 ECS 反射/序列化约定前伪造可用 API。

### 6.6 `ProgressionCommitStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/ProgressionCommitStateComponent.cs`

```csharp
using System.Collections.Immutable;

namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-06
// status: proposed
// componentOwner: WorldStorage / WorldProgressionAndTransition candidate
// crossSubsystemOwner: integration-review
// entityScope: one result summary per active transition
public sealed class ProgressionCommitStateComponent
{
  public TransitionCommitStatus CommitStatus { get; set; }
  public long CommitSequence { get; set; }
  public int AppliedBatchCount { get; set; }
  public int TotalBatchCount { get; set; }
  public WorldRevision? WorldRevisionBefore { get; set; }
  public WorldRevision? WorldRevisionAfter { get; set; }
  public ImmutableArray<WorldSectionVersion> ChangedSections { get; set; } = ImmutableArray<WorldSectionVersion>.Empty;
  public bool IsResultKnown { get; set; }
}
```

字段说明：

- 该 Component 只记录提交结果摘要，不保存 Tile 内容；
- `ChangedSections` 是结果快照，不拥有区段版本的长期写入权；
- `CommitStatus.Unknown` 不能被恢复代码自动解释为 `Failed` 或 `Committed`；
- `WorldRevisionAfter` 只有确认成功时才允许非空；
- `CommitSequence` 是候选单调序列，尚未由 Version4 或当前 NLTX 证明。

该 Component 不应增加 `NetworkSent`、`ChatPublished`、`AchievementGranted` 等字段；外部副作用不是世界提交结果的权威证明。

### 6.7 `TransitionRecoveryStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/TransitionRecoveryStateComponent.cs`

```csharp
using System.Collections.Immutable;

namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-07
// status: proposed
// componentOwner: WorldProgressionAndTransition candidate
// crossSubsystemOwner: integration-review
// entityScope: at most one recovery record per active transition
public sealed class TransitionRecoveryStateComponent
{
  public RecoveryPhase RecoveryPhase { get; set; }
  public TransitionPhase? LastSafePhase { get; set; }
  public ImmutableArray<int> PendingBatchIndexes { get; set; } = ImmutableArray<int>.Empty;
  public TransitionFailureCode? FailureCode { get; set; }
  public int RetryCount { get; set; }
  public bool ResultUnknown { get; set; }
  public long RecoveryRevision { get; set; }
}
```

字段说明：

- `PendingBatchIndexes` 只保存可恢复的批次索引，不复制 Tile 内容；
- `FailureCode` 只保存稳定分类，不保存异常实例、堆栈、路径或外部句柄；
- `ResultUnknown` 优先于重试意图，不能在结果未知时直接重新应用感染、矿阶或规则变化；
- `LastSafePhase` 只能引用已经确认的阶段；
- `RecoveryRevision` 是候选恢复快照版本，不等同于世界内容版本。

### 6.8 `RuntimeHardmodeCompatibilityStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/RuntimeHardmodeCompatibilityStateComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-08
// status: proposed
// componentOwner: WorldSession candidate
// crossSubsystemOwner: integration-review
// entityScope: compatibility view of active WorldEntity
public sealed class RuntimeHardmodeCompatibilityStateComponent
{
  public WorldEntityId? WorldEntityId { get; set; }
  public bool LegacyHardModeValue { get; set; }
}
```

字段说明：

- `LegacyHardModeValue` 只表达 `Main.hardMode` 兼容观察值，不拥有 Hardmode 业务权威性；
- 它与 `WorldHardmodeRuleStateComponent.IsHardMode` 的差异必须可解释；
- 不再增加第三个 Hardmode 布尔镜像；
- 新业务读者不能把该字段当作 Tile、矿阶、存档和网络结果都已提交的证明。

### 6.9 `WorldMetadataHardmodeStateComponent`

候选路径：`dome/src/Terraria.Dome.Simulation/WorldProgression/WorldMetadataHardmodeStateComponent.cs`

```csharp
namespace Terraria.Dome.Simulation.WorldProgression;

// componentId: C-WPT-09
// status: proposed
// componentOwner: WorldStorage candidate
// crossSubsystemOwner: integration-review
// entityScope: WorldMetadataEntity / world description object
public sealed class WorldMetadataHardmodeStateComponent
{
  public PersistentWorldId? PersistentWorldId { get; set; }
  public bool IsHardMode { get; set; }
}
```

字段说明：

- `IsHardMode` 是世界文件头部/发现元数据快照，不是活动世界规则的唯一证明；
- 无效世界仍使用 `false` 的兼容默认语义，但缺失或旧版本头部不能被静默解释为已经进入 Hardmode；
- 该 Component 不附着到活动 WorldEntity、区段或客户端会话；
- `PersistentWorldId` 的 owner、序列化形式和与 `WorldDescriptorState.WorldId/UniqueId` 的关系仍由 `BD-COMP-05` 和 `BD-COMP-06` 裁决；
- 如果最终由 `WorldStorage` 独立持有，本文件中的候选路径和命名空间必须随项目边界一起调整。

## 7. 组件组合草案

### 7.1 活动世界的最小组合

```text
WorldEntity
├─ WorldHardmodeRuleStateComponent
├─ WorldInfectionPolicyStateComponent
├─ HardmodeOreTierStateComponent
└─ RuntimeHardmodeCompatibilityStateComponent
```

这 4 个 Component 表达活动世界的长期规则、生态开关、矿阶状态和旧运行时兼容观察值。它们不包含计划、提交结果或恢复记录。

### 7.2 活动过渡组合

```text
WorldEntity
├─ ProgressionTransitionStateComponent
├─ TransitionPlanStateComponent
├─ ProgressionCommitStateComponent
└─ TransitionRecoveryStateComponent [失败/结果不明时才需要]
```

这些 Component 通过 `TransitionId` 和 `PlanId` 形成关系，但不能合并成一个巨型 Component：

- 过渡状态是可变生命周期事实；
- 计划是建立后应保持稳定的快照；
- 提交是确认后的结果摘要；
- 恢复是失败和结果不明的最小元数据。

### 7.3 世界元数据组合

```text
WorldMetadataEntity
└─ WorldMetadataHardmodeStateComponent
```

元数据实体与活动世界实体保持分离。元数据不能反向成为活动 Hardmode 规则的写入根。

### 7.4 不建立的组合

以下组合不在本草案中出现：

- 每个 Tile 一个 Hardmode Component；
- 每个后台 Task 一个过渡 Component；
- 每个网络客户端一个提交 Component；
- 把 `WorldRulesState`、计划、提交、恢复和兼容值重新装进一个 `WorldProgressionStateComponent`；
- 把 `HardmodeOreTierState` 值对象和 `HardmodeOreTierStateComponent` 重复定义为两个同名公共类型。

## 8. 当前 NLTX 类型映射

| 当前类型/字段 | 草案目标 | 映射状态 | 处理方式 |
|---|---|---|---|
| `src/WorldSession/WorldSessionComponents.cs:40-48 WorldRulesState.HardMode` | `WorldHardmodeRuleStateComponent.IsHardMode` | partial | 仅作为字段来源，不声明已经迁移 |
| `src/WorldSession/WorldSessionComponents.cs:44-48 WorldRulesState.InfectionSpreadAllowed` | `WorldInfectionPolicyStateComponent.IsInfectionSpreadAllowed` | partial | owner 和生态镜像关系仍未决 |
| `src/WorldSession/WorldSessionComponents.cs:48 OreTierState` | `HardmodeOreTierStateComponent.Value` | partial | 从过宽容器拆出三阶 Hardmode 值；不修改现有代码 |
| `dome/src/Terraria.Dome.Simulation/WorldGeneration/HardmodeOreTierState.cs` | `HardmodeOreTierStateComponent.Value` | existing value type | 复用现有值对象，不重复定义 |
| `dome/src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationProgressionState.cs` | `ProgressionTransitionStateComponent` | partial | 只复用计数/生成快照证据，不把生成状态宣称为 Hardmode 过渡组件 |
| `src/WorldSession/WorldGeneration/WorldGenerationLifecycleState.cs` | `TransitionPlanStateComponent.BaseGenerationRevision` | partial | 只作为计划的候选基线关系 |
| `WorldGen._transformingWorld` | `ProgressionTransitionStateComponent.ActiveTransformationCount` | confirmed for legacy shape | 不带入 Task、锁或回调 |
| `WorldGen.TransformingWorld` | `ProgressionTransitionStateComponent.IsTransforming` | confirmed derived shape | 只做派生属性，不单独持久化 |
| `WorldFileData.IsHardMode` | `WorldMetadataHardmodeStateComponent.IsHardMode` | partial | 保持元数据与活动规则分离 |
| `Main.hardMode` | `RuntimeHardmodeCompatibilityStateComponent.LegacyHardModeValue` | partial | 只作为旧兼容视图，不增加新的业务写入根 |
| `WorldSectionReplication` / 局部 section version | `TransitionPlanStateComponent` 和 `ProgressionCommitStateComponent` 的区段关系 | partial | 只引用区段 ID/版本，不复制区段内容 |

## 9. 实现前必须裁决的事项

### 9.1 命名和现有类型冲突

已确认的处理是：

- 逻辑设计名 `HardmodeOreTierState` 映射到代码载体 `HardmodeOreTierStateComponent`；
- 现有 `HardmodeOreTierState` 继续作为值对象复用；
- 不新建另一个同名 `HardmodeOreTierState`；
- 若最终 ECS 框架不需要 `Component` 后缀，必须在架构记录中明确说明命名例外。

### 9.2 项目和命名空间 owner

本草案暂以 `Terraria.Dome.Simulation.WorldProgression` 作为候选命名空间，因为目标是世界模拟状态，且现有 dome 工程已经包含 Hardmode 矿阶值对象和生成进度状态。该选择不是最终项目依赖裁决。

`WorldMetadataHardmodeStateComponent` 仍可能属于 `src/WorldStorage`。如果它移动到存储项目，必须解决 `PersistentWorldId`、序列化边界和活动世界读取关系，不能由 `using` 语句隐式跨项目耦合。

### 9.3 默认值和验证责任

本草案只提供字段初始化值，不提供运行时校验方法：

- `false`、`true`、`0`、`null` 和 `Uninitialized` 是候选初值；
- `RulePhase`、`TransitionPhase`、`CommitStatus` 和 `RecoveryPhase` 的组合不变量由未来显式状态转换边界验证；
- Component 不应通过构造函数启动线程、读存档、写 Tile、发网络消息或请求外部服务；
- `ImmutableArray<T>` 只是计划/结果快照的候选容器，最终序列化和 ECS 反射支持需单独确认。

### 9.4 ID、版本和关系 owner

以下类型必须由整合会话决定最终 owner：

- `WorldEntityId`；
- `TransitionId`；
- `PlanId`；
- `PersistentWorldId`；
- `WorldSectionId`；
- `WorldRevision`；
- `WorldSectionVersion`。

在 owner 锁定前，不能把这些候选值类型发布成稳定跨项目 API，也不能把它们复制到多个状态容器中。

### 9.5 状态和兼容值关系

这些关系仍必须在扩展到运行时行为、持久化或跨项目整合前裁决：

1. `WorldHardmodeRuleStateComponent.IsHardMode` 与 `RuntimeHardmodeCompatibilityStateComponent.LegacyHardModeValue` 是否允许在过渡中短暂不同；
2. `WorldMetadataHardmodeStateComponent.IsHardMode` 何时从活动规则刷新；
3. `TransitionPlanStateComponent.OreTierCandidate` 何时写入 `HardmodeOreTierStateComponent.Value`；
4. `ProgressionCommitStateComponent.CommitStatus == Unknown` 时的恢复策略；
5. `WorldRevision` 与局部 section version 的比较和提交语义。

## 10. 代码草案验收清单

- [x] 9 个逻辑 Component 均有对应的 C# 草案类型；
- [x] 每个逻辑 Component 的 C# 名称带有明确的 `Component` 后缀；
- [x] `HardmodeOreTierStateComponent` 与 `HardmodeOreTierState` 分离；`src` 项目内没有重复声明同名类型；
- [x] 组件只包含数据字段和派生属性，没有 System、Query、Command、Adapter 或 Projection；
- [x] 没有把 Task、线程、锁、随机实例、异常对象或外部句柄放入 Component；
- [x] 跨子系统 ID、版本和关系均标记为 `integration-review` 候选；
- [x] 所有实际代码文件均按“一核心公开类型一个同名文件”放在 `src/WorldSession/WorldProgression/`；
- [x] 未创建测试项目、测试源码或运行时注册代码；
- [x] C# 编译验证：`Terraria.WorldSession.csproj` 已通过；
- [ ] ECS 容器注册验证：未运行；
- [ ] 序列化/恢复验证：未运行；
- [ ] 行为等价验证：未运行。

## 11. 实际实现记录

本次实际创建了 `src/WorldSession/WorldProgression/` 下的 24 个 C# 文件：9 个 Component、6 个枚举、8 个关系/版本值类型和 1 个 `HardmodeOreTierState` 值对象。组件使用 `Terraria.WorldProgression.Components` 命名空间，并由 `Terraria.WorldSession.csproj` 自动包含。

本次没有创建测试文件。此前为 TDD 试运行而临时创建的 focused verifier、其项目文件和对应 `Build/bin`、`Build/obj` 生成物均已删除；仓库中其他既有测试和验证材料未修改。

实际构建命令为：

```powershell
$dotnetArgs = @(
  'build',
  '.\src\WorldSession\Terraria.WorldSession.csproj',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs
```

构建结果为退出码 `0`，输出位于 `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll`。没有运行测试命令。

## 12. 最终声明

本文件起点是 `WorldProgressionAndTransition` 的实际代码形状草案；当前已按用户指定在 `src` 下创建状态载体，但仍不是完整运行时实现，也不是稳定 API 定义。

本文件中的所有新类型、路径、字段和枚举均为 `status: proposed`。`src` 中的 `HardmodeOreTierState` 是本次为避免 dome 反向依赖而创建的项目内值对象；它与 dome 中的同名值对象尚未进行迁移合并或行为等价声明。

本次创建了 `src/WorldSession/WorldProgression/` 下的组件和候选值类型，并修改了本 Markdown 以记录实际路径；没有创建测试、运行时注册、存档代码或网络代码。已运行一次受影响项目的串行 build，未运行测试，因此 `verificationStatus: build-passed; test-not-created-by-request`。

本文件不声明迁移完成、行为等价、编译通过、序列化兼容、网络兼容或 API 已锁定。
