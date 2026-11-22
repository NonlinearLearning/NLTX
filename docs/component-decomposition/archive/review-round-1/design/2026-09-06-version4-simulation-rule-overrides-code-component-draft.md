# SimulationRuleOverrides 实际代码组件草案

## 1. 草案元数据

| 字段 | 值 |
| --- | --- |
| `subsystemId` | `SimulationRuleOverrides` |
| `sourceDesign` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-simulation-rule-overrides-component-design.md` |
| `sourceReport` | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-simulation-rule-overrides-public-decomposition.md` |
| `draftStatus` | `draft` |
| `implementationStatus` | `source-created` |
| `verificationStatus` | `build-verified-no-tests` |
| `targetProject` | `D:\TRbackup\NLTX\src\SimulationRuleOverrides\Terraria.SimulationRuleOverrides.csproj` |
| `targetDirectory` | `D:\TRbackup\NLTX\src\SimulationRuleOverrides\` |
| `namespaceCandidate` | `Terraria.SimulationRuleOverrides` |
| `componentCount` | `5` 个候选 Component |
| `subagentPolicy` | 本会话未启动子代理 |

本文档是根据 Component-only Design 整理的 C# 类型草案。候选代码现已按用户指定的位置
写入 `src\SimulationRuleOverrides`，但文档中的 Component `status` 仍保持 `proposed`，
因为跨子系统 owner 和持久化决策尚未锁定。本文档不宣称行为迁移已经闭合；本次只做了
生产项目构建，没有添加或运行测试。

## 2. 草案定位与边界

候选代码只保存 `SimulationRuleOverrides` 的内聚状态：

- 世界作用域的 normalized 覆写输入和持续性 toggle；
- 玩家作用域的 Creative toggle 和 per-player slider 输入；
- 按稳定规则键组织的默认权限与当前权限；
- 一个观察版本上的世界派生快照；
- 一个观察版本上的玩家派生快照。

组件类型不直接执行文件、网络、UI、时钟、随机数、日志、环境修改、伤害结算或 NPC
生成。输入映射、权限写入、快照发布、存档边界和下游消费仍属于组件之外的整合边界。

草案中的所有组件均使用 `status: proposed`。`RuleKey` 和 `PowerPermissionLevel` 是
代码形状所需的支撑类型，但当前 NLTX 尚未裁决它们的最终 owner，因此本文件只引用其
候选名称，不在本文件中创建新的共享类型。

## 3. 候选文件布局

小型能力域采用扁平目录。每个文件只有一个核心公开类型，不创建泛化的 `Shared` 或空的
`Components` 子目录。

| 候选文件 | 核心公开类型 | 作用域 | status |
| --- | --- | --- | --- |
| `RuleOverrideStateComponent.cs` | `RuleOverrideStateComponent` | 一个 `WorldEntity` | `proposed` |
| `PlayerRuleOverrideStateComponent.cs` | `PlayerRuleOverrideStateComponent` | 一个 `PlayerEntity` | `proposed` |
| `RuleOverridePermissionStateComponent.cs` | `RuleOverridePermissionStateComponent` | 一个待裁决的 authority entity | `proposed` |
| `RuleOverrideSnapshotComponent.cs` | `RuleOverrideSnapshotComponent` | 一个 `WorldEntity` | `proposed` |
| `PlayerRuleOverrideSnapshotComponent.cs` | `PlayerRuleOverrideSnapshotComponent` | 一个 `PlayerEntity` | `proposed` |

候选根目录：

```text
D:\TRbackup\NLTX\src\SimulationRuleOverrides\
```

候选命名空间：`Terraria.Dome.Simulation.RuleOverrides`

## 4. 代码约定

- 使用 `readonly record struct` 表达不含引用集合的权威值或不可变快照值。
- 使用 `sealed class` 表达包含两个权限映射的状态，避免值类型复制引用型集合造成误解。
- 构造函数只做格式、范围和集合一致性检查，不执行外部副作用。
- normalized 输入使用 finite 的 `[0, 1]` 范围。
- 快照不提供可变 setter，也不作为权威输入的第二个可变写集。
- `long?` revision 仅是候选的版本元数据表达；版本 owner 仍由整合审查裁决。
- 代码使用 2 个空格缩进；代码块中的绝对路径是候选目标文件，不代表文件已经写入。

## 5. 实际代码组件草案

### 5.1 `RuleOverrideStateComponent`

目标文件：

```text
D:\TRbackup\NLTX\src\SimulationRuleOverrides\RuleOverrideStateComponent.cs
```

组件元数据：

```text
componentId: SRO-COMP-WORLD-OVERRIDE
name: RuleOverrideStateComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: exactly one WorldEntity per simulation world
lifecycle: world creation -> validated recovery/default -> controlled replacement -> world teardown
```

字段对应关系：

| 属性 | 类型 | 候选默认值 | 分类 |
| --- | --- | --- | --- |
| `TimeRateNormalized` | `float` | `0.0f` | 权威输入 |
| `DifficultyNormalized` | `float` | `0.0f` | 权威输入 |
| `WindDirectionAndStrengthNormalized` | `float` | `0.0f` | 权威输入；显式 reset 语义未决 |
| `RainStrengthNormalized` | `float` | `0.0f` | 权威输入；显式 reset 语义未决 |
| `FreezeTimeEnabled` | `bool` | `false` | 权威状态 |
| `FreezeWindEnabled` | `bool` | `false` | 权威状态 |
| `FreezeRainEnabled` | `bool` | `false` | 权威状态 |
| `StopBiomeSpreadEnabled` | `bool` | `false` | 权威状态 |
| `AuthorityRevision` | `long?` | `null` | 候选权威版本元数据 |

候选源码：

```csharp
using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct RuleOverrideStateComponent
{
  public RuleOverrideStateComponent(
    float timeRateNormalized = 0.0f,
    float difficultyNormalized = 0.0f,
    float windDirectionAndStrengthNormalized = 0.0f,
    float rainStrengthNormalized = 0.0f,
    bool freezeTimeEnabled = false,
    bool freezeWindEnabled = false,
    bool freezeRainEnabled = false,
    bool stopBiomeSpreadEnabled = false,
    long? authorityRevision = null)
  {
    ValidateNormalized(timeRateNormalized, nameof(timeRateNormalized));
    ValidateNormalized(difficultyNormalized, nameof(difficultyNormalized));
    ValidateNormalized(
      windDirectionAndStrengthNormalized,
      nameof(windDirectionAndStrengthNormalized));
    ValidateNormalized(rainStrengthNormalized, nameof(rainStrengthNormalized));
    ValidateRevision(authorityRevision, nameof(authorityRevision));

    TimeRateNormalized = timeRateNormalized;
    DifficultyNormalized = difficultyNormalized;
    WindDirectionAndStrengthNormalized = windDirectionAndStrengthNormalized;
    RainStrengthNormalized = rainStrengthNormalized;
    FreezeTimeEnabled = freezeTimeEnabled;
    FreezeWindEnabled = freezeWindEnabled;
    FreezeRainEnabled = freezeRainEnabled;
    StopBiomeSpreadEnabled = stopBiomeSpreadEnabled;
    AuthorityRevision = authorityRevision;
  }

  public float TimeRateNormalized { get; }

  public float DifficultyNormalized { get; }

  public float WindDirectionAndStrengthNormalized { get; }

  public float RainStrengthNormalized { get; }

  public bool FreezeTimeEnabled { get; }

  public bool FreezeWindEnabled { get; }

  public bool FreezeRainEnabled { get; }

  public bool StopBiomeSpreadEnabled { get; }

  public long? AuthorityRevision { get; }

  private static void ValidateNormalized(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The normalized value must be finite and within [0, 1].");
    }
  }

  private static void ValidateRevision(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The revision must be non-negative when present.");
    }
  }
}
```

实现边界：`TargetTimeRate`、`DifficultyMultiplier`、`WindSpeedTarget`、`RainStrength`
和 `AllowInfectionSpread` 不放入该权威组件，以免把派生值与 normalized 输入形成两个可变
写集。风雨的实际 reset 和持久化资格仍对应 `E-GAP-COMP-03`。

### 5.2 `PlayerRuleOverrideStateComponent`

目标文件：

```text
D:\TRbackup\NLTX\src\SimulationRuleOverrides\PlayerRuleOverrideStateComponent.cs
```

组件元数据：

```text
componentId: SRO-COMP-PLAYER-OVERRIDE
name: PlayerRuleOverrideStateComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: one PlayerEntity
lifecycle: player creation -> validated profile/default recovery -> controlled replacement -> player teardown
```

候选源码：

```csharp
using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct PlayerRuleOverrideStateComponent
{
  public PlayerRuleOverrideStateComponent()
    : this(false, true, 0.5f)
  {
  }

  public PlayerRuleOverrideStateComponent(
    bool godmodeEnabled,
    bool farPlacementRangeEnabled,
    float spawnRateNormalized)
  {
    ValidateNormalized(spawnRateNormalized, nameof(spawnRateNormalized));

    GodmodeEnabled = godmodeEnabled;
    FarPlacementRangeEnabled = farPlacementRangeEnabled;
    SpawnRateNormalized = spawnRateNormalized;
  }

  public bool GodmodeEnabled { get; }

  public bool FarPlacementRangeEnabled { get; }

  public float SpawnRateNormalized { get; }

  private static void ValidateNormalized(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The normalized value must be finite and within [0, 1].");
    }
  }
}
```

该类型的显式无参构造函数表达候选默认值 `false`、`true`、`0.5f`。未经验证的
`default(PlayerRuleOverrideStateComponent)` 不应被当成玩家恢复结果使用，因为值类型的
CLR 默认值不能表达 `FarPlacementRangeEnabled=true` 的候选默认策略。

`LegacyPlayerSlot`、玩家 profile ID、网络身份、连接句柄、下游伤害结果和 NPC 生成结果均
不属于该组件。`SpawnRateNormalized == 0` 只表达权威输入，禁刷资格在玩家快照中单独表达。

### 5.3 `RuleOverridePermissionStateComponent`

目标文件：

```text
D:\TRbackup\NLTX\src\SimulationRuleOverrides\RuleOverridePermissionStateComponent.cs
```

组件元数据：

```text
componentId: SRO-COMP-PERMISSION
name: RuleOverridePermissionStateComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: world-authority entity or server-session authority entity, unresolved
lifecycle: authority initialization -> reviewed replacement -> reset/recovery -> authority teardown
```

支撑类型状态：

- `RuleKey`：候选稳定规则键，当前 NLTX 尚未确认最终 owner。
- `PowerPermissionLevel`：候选三值权限类型，成员候选为 `LockedForEveryone`、
  `CanBeChangedByHostAlone` 和 `CanBeChangedByEveryone`；当前 NLTX 尚未确认最终 owner。

候选源码：

```csharp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.SimulationRuleOverrides;

public sealed class RuleOverridePermissionStateComponent
{
  public RuleOverridePermissionStateComponent(
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> defaultPermissionByRule,
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> currentPermissionByRule,
    long? permissionRevision = null)
  {
    var defaultPermissions = CopyAndValidate(
      defaultPermissionByRule,
      nameof(defaultPermissionByRule));
    var currentPermissions = CopyAndValidate(
      currentPermissionByRule,
      nameof(currentPermissionByRule));

    ValidateSameRuleSet(defaultPermissions, currentPermissions);
    ValidateRevision(permissionRevision, nameof(permissionRevision));

    DefaultPermissionByRule = new ReadOnlyDictionary<RuleKey, PowerPermissionLevel>(
      defaultPermissions);
    CurrentPermissionByRule = new ReadOnlyDictionary<RuleKey, PowerPermissionLevel>(
      currentPermissions);
    PermissionRevision = permissionRevision;
  }

  public IReadOnlyDictionary<RuleKey, PowerPermissionLevel> DefaultPermissionByRule { get; }

  public IReadOnlyDictionary<RuleKey, PowerPermissionLevel> CurrentPermissionByRule { get; }

  public long? PermissionRevision { get; }

  private static Dictionary<RuleKey, PowerPermissionLevel> CopyAndValidate(
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> source,
    string parameterName)
  {
    if (source is null)
    {
      throw new ArgumentNullException(parameterName);
    }

    if (source.Count == 0)
    {
      throw new ArgumentException(
        "At least one registered rule is required.",
        parameterName);
    }

    var copy = new Dictionary<RuleKey, PowerPermissionLevel>(source.Count);
    foreach (var entry in source)
    {
      if (!IsDefinedPermission(entry.Value))
      {
        throw new ArgumentException(
          "The permission value is not part of the supported permission domain.",
          parameterName);
      }

      copy.Add(entry.Key, entry.Value);
    }

    return copy;
  }

  private static void ValidateSameRuleSet(
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> defaults,
    IReadOnlyDictionary<RuleKey, PowerPermissionLevel> current)
  {
    if (defaults.Count != current.Count)
    {
      throw new ArgumentException(
        "Default and current permission maps must contain the same rule set.");
    }

    foreach (var ruleKey in defaults.Keys)
    {
      if (!current.ContainsKey(ruleKey))
      {
        throw new ArgumentException(
          "Default and current permission maps must contain the same rule set.");
      }
    }
  }

  private static bool IsDefinedPermission(PowerPermissionLevel permission)
  {
    return permission is PowerPermissionLevel.LockedForEveryone
      or PowerPermissionLevel.CanBeChangedByHostAlone
      or PowerPermissionLevel.CanBeChangedByEveryone;
  }

  private static void ValidateRevision(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The revision must be non-negative when present.");
    }
  }
}
```

该类没有公开字典写入方法；一次权限替换应以新的组件值替换旧组件值。客户端收到的
`CreativePowerPermissionModulePacket` 不能直接成为该组件的可信 writer。权限组件究竟附着
世界 authority entity 还是服务器 session authority entity，以及谁可以产生新实例，由
`BD-COMP-02` 决定。

### 5.4 `RuleOverrideSnapshotComponent`

目标文件：

```text
D:\TRbackup\NLTX\src\SimulationRuleOverrides\RuleOverrideSnapshotComponent.cs
```

组件元数据：

```text
componentId: SRO-COMP-WORLD-SNAPSHOT
name: RuleOverrideSnapshotComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: one WorldEntity per published observation
lifecycle: valid authority/context -> immutable replacement per observation -> world teardown
```

候选源码：

```csharp
using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct RuleOverrideSnapshotComponent
{
  public RuleOverrideSnapshotComponent(
    long tick,
    int targetTimeRate,
    float difficultyMultiplier,
    float windSpeedTarget,
    float rainStrength,
    bool freezeTimeEnabled,
    bool freezeWindEnabled,
    bool freezeRainEnabled,
    bool allowInfectionSpread,
    long? sourceRevision = null)
  {
    ValidateTick(tick, nameof(tick));
    ValidateRevision(sourceRevision, nameof(sourceRevision));
    ValidateTimeRate(targetTimeRate, nameof(targetTimeRate));
    ValidateDifficultyMultiplier(difficultyMultiplier, nameof(difficultyMultiplier));
    ValidateRange(windSpeedTarget, -0.8f, 0.8f, nameof(windSpeedTarget));
    ValidateRange(rainStrength, 0.0f, 1.0f, nameof(rainStrength));

    Tick = tick;
    SourceRevision = sourceRevision;
    TargetTimeRate = targetTimeRate;
    DifficultyMultiplier = difficultyMultiplier;
    WindSpeedTarget = windSpeedTarget;
    RainStrength = rainStrength;
    FreezeTimeEnabled = freezeTimeEnabled;
    FreezeWindEnabled = freezeWindEnabled;
    FreezeRainEnabled = freezeRainEnabled;
    AllowInfectionSpread = allowInfectionSpread;
  }

  public long Tick { get; }

  public long? SourceRevision { get; }

  public int TargetTimeRate { get; }

  public float DifficultyMultiplier { get; }

  public float WindSpeedTarget { get; }

  public float RainStrength { get; }

  public bool FreezeTimeEnabled { get; }

  public bool FreezeWindEnabled { get; }

  public bool FreezeRainEnabled { get; }

  public bool AllowInfectionSpread { get; }

  private static void ValidateTick(long value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The tick must be non-negative.");
    }
  }

  private static void ValidateRevision(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The revision must be non-negative when present.");
    }
  }

  private static void ValidateTimeRate(int value, string parameterName)
  {
    if (value < 1 || value > 24)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The target time rate must be within [1, 24].");
    }
  }

  private static void ValidateDifficultyMultiplier(float value, string parameterName)
  {
    ValidateRange(value, 0.5f, 3.0f, parameterName);

    var stepPosition = (value - 0.5f) / 0.05f;
    var nearestStep = MathF.Round(stepPosition);
    if (MathF.Abs(stepPosition - nearestStep) > 0.0001f)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The difficulty multiplier must use 0.05 increments.");
    }
  }

  private static void ValidateRange(
    float value,
    float minimum,
    float maximum,
    string parameterName)
  {
    if (float.IsNaN(value)
      || float.IsInfinity(value)
      || value < minimum
      || value > maximum)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The value must be finite and within its candidate range.");
    }
  }
}
```

`TargetTimeRate`、`DifficultyMultiplier`、`WindSpeedTarget`、`RainStrength` 和
`AllowInfectionSpread` 都是派生快照值，不能反向写入 `RuleOverrideStateComponent`。
在首次有效观察完成前，不应使用值类型的 `default(RuleOverrideSnapshotComponent)` 作为
可消费的世界规则事实；当前构造约束会使其不能满足快照字段范围。

### 5.5 `PlayerRuleOverrideSnapshotComponent`

目标文件：

```text
D:\TRbackup\NLTX\src\SimulationRuleOverrides\PlayerRuleOverrideSnapshotComponent.cs
```

组件元数据：

```text
componentId: SRO-COMP-PLAYER-SNAPSHOT
name: PlayerRuleOverrideSnapshotComponent
status: proposed
componentOwner: SimulationRuleOverrides (candidate)
crossSubsystemOwner: integration-review
entityScope: one PlayerEntity for the corresponding world observation
lifecycle: valid world observation -> immutable replacement -> player/world teardown
```

候选源码：

```csharp
using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct PlayerRuleOverrideSnapshotComponent
{
  public PlayerRuleOverrideSnapshotComponent(
    long tick,
    bool godmodeEnabled,
    bool farPlacementRangeEnabled,
    float spawnRateMultiplier,
    bool disableSpawns,
    long? sourceRevision = null)
  {
    ValidateTick(tick, nameof(tick));
    ValidateRevision(sourceRevision, nameof(sourceRevision));
    ValidateRange(spawnRateMultiplier, 0.1f, 10.0f, nameof(spawnRateMultiplier));

    Tick = tick;
    SourceRevision = sourceRevision;
    GodmodeEnabled = godmodeEnabled;
    FarPlacementRangeEnabled = farPlacementRangeEnabled;
    SpawnRateMultiplier = spawnRateMultiplier;
    DisableSpawns = disableSpawns;
  }

  public long Tick { get; }

  public long? SourceRevision { get; }

  public bool GodmodeEnabled { get; }

  public bool FarPlacementRangeEnabled { get; }

  public float SpawnRateMultiplier { get; }

  public bool DisableSpawns { get; }

  private static void ValidateTick(long value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The tick must be non-negative.");
    }
  }

  private static void ValidateRevision(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The revision must be non-negative when present.");
    }
  }

  private static void ValidateRange(
    float value,
    float minimum,
    float maximum,
    string parameterName)
  {
    if (float.IsNaN(value)
      || float.IsInfinity(value)
      || value < minimum
      || value > maximum)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The value must be finite and within its candidate range.");
    }
  }
}
```

`Tick` 必须与关联世界快照的观察版本相等；`SourceRevision` 如果最终保留，也必须对应
同一批已接受的世界状态。`SpawnRateMultiplier` 和 `DisableSpawns` 是两个独立的只读字段，
不能用倍率为零替代禁刷资格，也不能把快照字段反写到玩家权威组件。玩家 slot、连接身份、
网络包和 profile 字节不属于该快照。

## 6. Component 组合

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
| --- | --- | --- | --- | --- |
| 一个世界 authority entity | `RuleOverrideStateComponent`、`RuleOverridePermissionStateComponent` | `RuleOverrideSnapshotComponent` | 另一个同义 Creative world authority Component | 世界当前值与权限映射按 authority 生命周期存在；快照是只读观察值 |
| 一个玩家实体 | `PlayerRuleOverrideStateComponent` | `PlayerRuleOverrideSnapshotComponent` | 按 `LegacyPlayerSlot` 索引的第二份 Creative 玩家 authority | 玩家字段属于玩家实体；玩家快照与一个有效世界观察版本绑定 |
| 普通世界实体 | 无本子系统 Component | 无 | 因为拥有天气、Tile 或生成字段而附加 Creative rule Component | 本子系统只附着唯一的世界 authority |
| 单个 Power 记录 | 无 | 无 | `PowerEntity` 或每 Power 一组 Component | Power ID 是兼容键，Power 不是独立 ECS 实体生命周期 |
| 世界生成输入对象 | 无本子系统 Component | 既有 `WorldRuleSnapshotComponent` | 与 Creative snapshot 合并 | 生成输入与 Creative 运行时覆写的 owner 和生命周期不同 |

组合关系只表达数据范围，不表达执行顺序。`WorldEntityId`、`PlayerEntityId`、
`PersistentWorldId`、`PersistentPlayerId`、`NetworkId` 和 `LegacyPlayerSlot` 仍由各自
身份、持久化或协议边界持有。

## 7. 设计到代码的映射

| 设计字段 | 代码位置 | 代码处理 |
| --- | --- | --- |
| normalized 世界 slider | `RuleOverrideStateComponent` | 构造时检查 finite 和 `[0, 1]` |
| 世界冻结和生态 toggle | `RuleOverrideStateComponent` | 独立 `bool` 属性，不使用速率零或可空标志代替 |
| normalized 玩家 slider | `PlayerRuleOverrideStateComponent` | 构造时检查 finite 和 `[0, 1]` |
| 玩家权限/默认权限 | `RuleOverridePermissionStateComponent` | 防御性复制、非空检查、规则键集合一致性检查 |
| 世界 Tick 派生值 | `RuleOverrideSnapshotComponent` | 构造时检查 Tick、时间速率、难度、风和雨候选范围 |
| 玩家 Tick 派生值 | `PlayerRuleOverrideSnapshotComponent` | 构造时检查 Tick、revision 和刷怪倍率候选范围 |
| `PowerId` | 不进入组件属性 | 保留在兼容映射边界 |
| packet、WLD bytes、UI 和外部身份 | 不进入组件属性 | 保留在相应边界 |

## 8. 当前 NLTX 对应关系

以下现有类型不被本草案覆盖，也不因名称相近而被改写：

| 现有类型 | 当前状态 | 与草案的关系 |
| --- | --- | --- |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldRuleState.cs` | `existing` | 普通世界难度、天气、PVP 和 game mode；与 Creative authority 相邻，最终字段 owner 需整合审查 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldRuleSnapshot.cs` | `existing` | 只有 Tick、时间和昼夜，是窄时间投影，不是完整 Creative 快照 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\Components\WorldRuleSnapshotComponent.cs` | `existing` | 世界生成输入，不是运行时 Creative authority |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Npc\Snapshots\NpcSpawnPlayerReadiness.cs` | `existing` | NPC 下游读取的禁刷资格，不拥有玩家 Creative 状态 |
| `D:\TRbackup\NLTX\src\Player\PlayerIdentityState.cs` | `existing` | 身份和 `LegacySlot` 边界，不承接 Creative 玩家字段 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Players\PlayerPersistentState.cs` | `existing` | 当前 profile、生命、物品等持久状态，是否承接 Creative 字段由 `BD-COMP-03` 决定 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\CreativePowerModulePacket.cs` | `existing` | 协议输入 DTO，不是世界或玩家 authority |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Protocol.V1456\Packets\CreativePowerPermissionModulePacket.cs` | `existing` | 权限协议 DTO，当前解码后未形成权限 authority |
| `D:\TRbackup\NLTX\dome\src\Terraria.WorldFile.V319\Format\WldCreativePowerReader.cs` | `existing` | opaque Creative section 兼容资料，不是行为组件 |

## 9. 未决项与影响

### `BD-COMP-01`：世界覆写字段与普通世界状态的 owner

- 冲突字段：Creative time rate、difficulty、wind/rain 输入、冻结 toggle、生态传播开关，
  可能与现有 `WorldRuleState` 的普通字段共同影响消费者。
- 候选方案 A：Creative normalized authority 由本子系统持有，普通天气/难度继续由
  `WorldRuleState` 持有；本草案的字段保持独立。
- 候选方案 B：由 `WorldSession` 统一持有部分世界规则；这会改变本草案的字段组成和恢复边界。
- 候选方案 C：按时间、天气、生态和难度拆成多个世界 Component；这会增加跨组件一致性要求。
- 当前不能裁决：现有 `WorldRuleState` 已被其他 NLTX 消费者使用，本次草案无权把其字段迁移或合并。

影响：`RuleOverrideStateComponent` 和 `RuleOverrideSnapshotComponent` 的最终 owner
继续标记为 `crossSubsystemOwner: integration-review`。

### `BD-COMP-02`：权限 Component 的可信 writer 和实体范围

- 冲突字段：`CurrentPermissionByRule` 的可信写入者可以是 server config、world authority、
  server session authority 或经过审查的管理入口。
- 候选方案 A：Component 附着世界实体，只允许受控服务端 authority 修改。
- 候选方案 B：Component 附着服务器 session authority，适配会话级权限生命周期。
- 候选方案 C：协议输入先经过可信 session boundary，再映射为新的 Component 值。
- 当前不能裁决：Version4 权限网络模块的反序列化实现为空，不能把客户端 DTO 直接提升为 writer。

影响：`RuleOverridePermissionStateComponent` 的 `entityScope` 和 `componentOwner` 仍为候选值。

### `BD-COMP-03`：玩家 Creative 字段的持久化 owner

- 冲突字段：`GodmodeEnabled`、`FarPlacementRangeEnabled` 和 `SpawnRateNormalized` 的
  profile 或独立 Creative section 归属。
- 候选方案 A：扩展既有 `PlayerPersistentState`，使 profile 和玩家 Component 同生命周期。
- 候选方案 B：增加独立的 Creative 玩家持久化边界，保持通用 profile 边界不变。
- 候选方案 C：首轮只恢复世界作用域，玩家 Component 暂不承诺 persistence-ready。
- 当前不能裁决：当前 Version4 Manager 没有完整玩家保存闭环，当前 NLTX profile 也没有这些字段。

影响：玩家权威 Component 的恢复路径只能保持为候选，不得声明持久化行为已经闭合。

### `BD-COMP-04`：快照 owner、revision 和未知兼容记录

- 冲突字段：`Tick`、`SourceRevision`、WorldEntity/PlayerEntity 关系，以及 opaque WLD record
  在恢复时的保留、跳过或拒绝策略。
- 候选方案 A：WorldEntity 保存世界快照，PlayerEntity 保存玩家快照；玩家快照只用观察版本关联世界快照。
- 候选方案 B：WorldEntity 保存按玩家 ID 索引的集合；这会把玩家状态重新集中为大型可变集合。
- 候选方案 C：取消统一 snapshot Component，由各下游读取 authority；这会丢失共同观察版本。
- 当前不能裁决：Version4 Creative 代码没有统一 Tick、revision 或实体关系定义。

影响：两个 snapshot Component 的 `Tick` 和 `SourceRevision` 只能作为候选字段，未知兼容记录
也不能直接写入组件。

## 10. 组件级 evidence-gap

| ID | 缺口 | 影响类型 | 草案处理 |
| --- | --- | --- | --- |
| `E-GAP-COMP-01` | Version4 当前 Manager 没有完整 per-player 保存/恢复入口 | `PlayerRuleOverrideStateComponent` | 保留字段形状，不锁定持久化 owner |
| `E-GAP-COMP-02` | Godmode 的 Version4 消费点是注释代码 | 玩家 authority 与玩家 snapshot | 只保留状态和只读投影，不承诺伤害效果 |
| `E-GAP-COMP-03` | Version4 风/雨 slider 更新方法和 reset/持久化时点不完整 | `RuleOverrideStateComponent` | 保留 normalized 输入，显式 reset 语义标为 unresolved |
| `E-GAP-COMP-04` | 没有统一 Creative Tick、revision 或 WorldEntity/PlayerEntity 关系 | 两个 snapshot Component | 使用候选 `Tick`/`SourceRevision`，等待整合 owner |
| `E-GAP-COMP-05` | 权限网络接收体为空，可信 writer 尚未确认 | `RuleOverridePermissionStateComponent` | 客户端 DTO 不直接写入组件 |
| `E-GAP-COMP-06` | 未知 Power ID/WLD opaque record 的恢复策略未裁决 | authority 和兼容边界 | 兼容记录不进入组件 |

## 11. 不纳入本次代码草案的内容

本文件不增加以下运行时类型或代码：

- System、Query、Command、Event、Adapter、Projection 或调度流程；
- 网络 packet、协议编解码、连接 session 或外部调用者身份；
- WLD reader/writer、section offset、原始字节或未知记录容器；
- UI slider、按钮、显示文本、排序索引或其他表现数据；
- 时钟、随机数、日志、文件、网络和环境写入；
- 玩家伤害结算、NPC 生成、生态传播和天气更新实现；
- 测试文件、项目文件、迁移脚本和生成输出。

## 12. 草案状态与检查范围

本次实现已写入 `src\SimulationRuleOverrides`，并新增对应的
`Terraria.SimulationRuleOverrides.csproj`。没有新增测试文件，也没有修改现有 `Test`、
`dome\src` 或 `dome\Test` 内容。本次生产项目构建已执行；没有运行测试、verifier 或
其他测试命令。

计划中的后续源码实现必须在 `BD-COMP-01` 至 `BD-COMP-04` 和支撑类型 owner 裁决后，
重新检查字段、目录、项目包含项、跨域引用、持久化边界和快照发布关系。

## 13. 最终声明

本文件是根据 `SimulationRuleOverrides` Component-only Design 生成的实际代码形状草案。

本文件不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、
测试计划或迁移计划。

所有 `status: proposed` 的 Component 都只是设计提案。本文件不把候选片段描述为源码落地、
迁移完成、与现有实现保持相同行为，或通过编译与测试检查。`verificationStatus` 保持为
`not-run`。
