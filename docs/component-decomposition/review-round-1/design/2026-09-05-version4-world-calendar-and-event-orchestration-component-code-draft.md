# WorldCalendarAndEventOrchestration Component 代码草案

> draftStatus: implemented-code-contract  
> subsystemId: WorldCalendarAndEventOrchestration  
> taskNumber: 05  
> sourceDesign: `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-world-calendar-and-event-orchestration-component-design.md`  
> evidenceStatus: partial  
> nltxStatus: partial  
> verificationStatus: build-passed-no-tests  
> implementationStatus: implemented-in-src  
> targetProject: `src/WorldSession/Terraria.WorldSession.csproj`

## 1. 文档目的

本文将已有的 Component-only Design 转换为接近实际 C# 源码的组件契约。代码块继续用于评审类型形状、字段命名、默认值和纯不变量检查；对应的实际组件已按本契约加入 `src/WorldSession/Calendar/`。

本契约只包含 Component 数据类型和不产生副作用的 `Validate` 检查。实际落地同样没有加入系统、调度、随机算法、存档/网络流程或其他副作用。

### 1.1 设计状态

- 14 个核心代码类型已在 `src/WorldSession/Calendar/` 实现；跨域值类型仍为 provisional contract。
- `CalendarClock` 的 `timeOfDay` 初值仍未在 Version4 的 `13500.0` 和 Dome 的 `0.0` 之间裁决。
- `EventInstanceId`、`EntityId`、`PersistentEntityId`、`NetworkId`、`NpcEntityId`、`PlayerEntityId`、`WorldSectionId` 和 `EntityReference` 的最终 owner 仍为 `crossSubsystemOwner: integration-review`。
- Version4 的 `Main.rand` 与 Dome 的世界种子随机流不被本草案假定为兼容。
- `EventInstanceId`、实体 ID、网络 ID、事件枚举和世界几何值类型在本轮以独立文件落地为 provisional contract；最终 owner 仍需 integration-review，不能据此宣称跨域兼容已闭合。

### 1.2 本轮实现记录

- 实际目录：`D:\TRbackup\NLTX\src\WorldSession\Calendar\`。
- 实际项目：`D:\TRbackup\NLTX\src\WorldSession\Terraria.WorldSession.csproj`。
- 组件使用 `sealed class`，列表输入在构造时复制并以只读快照暴露；这是对原 `struct` 草案拷贝语义风险的实现裁决。
- `CalendarClockComponent` 要求调用方显式传入 `TimeOfDay`；本轮不替 Version4 `13500.0` 或当前 NLTX 的其他兼容约定作最终裁决。
- 未创建、未修改、未运行测试；本轮只执行了受约束的生产项目 `build`。

## 2. 代码草案约束

### 2.1 建议的文件位置

组件属于 `WorldCalendarAndEventOrchestration` 领域。当前规模已经足以形成稳定的领域边界，因此建议先使用领域目录，而不是建立泛化的 `Shared/Components/` 目录：

```text
D:\TRbackup\NLTX\src\WorldSession\Calendar\
  CalendarClockComponent.cs
  WorldEventInstanceStateComponent.cs
  BirthdayPartyStateComponent.cs
  LanternNightStateComponent.cs
  SandstormStateComponent.cs
  SlimeRainStateComponent.cs
  WorldInvasionStateComponent.cs
  InvasionHistoryStateComponent.cs
  PendingWorldEventStateComponent.cs
  WorldCalendarOverrideStateComponent.cs
  Dd2PersistentProgressStateComponent.cs
  Dd2RunStateComponent.cs
  Dd2WaveRuntimeStateComponent.cs
  WorldEventRandomStateComponent.cs
```

每个文件只放一个与文件同名的核心公开类型。上述目录和文件是本轮实际实现路径；没有移动已有类型，也没有改变现有命名空间。

### 2.2 C# 约定

- 使用 `PascalCase` 公共类型、字段和属性，私有成员使用 `_camelCase`。
- 代码缩进为 2 个空格，控制流保留 braces。
- Component 只保存状态；`Validate` 只做确定性的内存检查，不读时钟、随机数、日志、文件、网络或共享可变全局状态。
- `IReadOnlyList<T>` 字段必须由最终 owner 保证快照或防御性复制；实际实现通过构造时复制和只读包装收口。
- 原草案使用 `struct` 以贴合 Dome 数据组件；实际 `src/WorldSession` 实现对组件使用 `sealed class`，避免组件被复制后修改副本，并保持列表状态的引用语义安全。

## 3. 跨域前置类型

以下类型是代码草案中的跨域前置契约。为使 `src/WorldSession` 的组件可以独立编译，本轮对实际使用的类型增加了 Calendar 领域内的 provisional 定义；其最终命名空间、值域、序列化和 owner 仍未完成整合裁决：

| 类型 | 用途 | 状态 |
| --- | --- | --- |
| `EventInstanceId` | 事件实例的稳定身份 | `src/WorldSession/Calendar/EventInstanceId.cs` 的 provisional 类型；`crossSubsystemOwner: integration-review` |
| `EntityId` | 运行时实体引用 | `src/WorldSession/Calendar/EntityId.cs` 的 provisional 类型；`crossSubsystemOwner: integration-review` |
| `PersistentEntityId` | 跨重载实体引用 | `src/WorldSession/Calendar/PersistentEntityId.cs` 的 provisional 类型；`crossSubsystemOwner: integration-review` |
| `NetworkId` | 网络复制身份 | `src/WorldSession/Calendar/NetworkId.cs` 的 provisional 类型；`crossSubsystemOwner: integration-review` |
| `NpcEntityId` | Party 参与 NPC 或 DD2 关联 NPC | `src/WorldSession/Calendar/NpcEntityId.cs` 的 provisional 类型；不能直接复用 Version4 `whoAmI` |
| `PlayerEntityId` | 事件资格相关的玩家关系 | 当前 14 个组件未使用；`crossSubsystemOwner: integration-review` |
| `WorldSectionId` | 世界区域关系 | 当前 14 个组件未使用；`crossSubsystemOwner: integration-review` |
| `EntityReference` | 允许失效和恢复语义的关系封装 | 当前 14 个组件未使用；`crossSubsystemOwner: integration-review` |
| `WorldEventLifecycleState` | 通用事件生命周期值 | `src/WorldSession/Calendar/WorldEventLifecycleState.cs` 的 provisional 枚举；不能用多个布尔字段隐式代替而不记录状态 |
| `WorldEventKind` | 事件种类 | `src/WorldSession/Calendar/WorldEventKind.cs` 的 provisional 枚举；当前 Dome 仅覆盖部分事件 |
| `InvasionType` | 普通入侵种类 | 实际实现复用当前根目录 `src` 的局部枚举；Dome 目标 namespace 和兼容范围未裁决 |
| `WorldTileRectangle` | DD2 竞技场世界区域 | `src/WorldSession/Calendar/WorldTileRectangle.cs` 的 provisional 值类型；最终空间 owner 未裁决 |

这些 provisional 类型只为闭合当前 `WorldSession` 项目的编译边界，不代表已经完成跨子系统整合；替换它们时必须保留 ID 类型之间的区分，不能把旧 `whoAmI`、网络 ID 或几何类型直接互换。

## 4. Component 代码草案

### 4.1 CalendarClockComponent

逻辑 Component：`CalendarClock`。建议文件：`World/Calendar/CalendarClockComponent.cs`。

职责：保存世界时间位置、昼夜方向、时间推进率、周期长度、月相和暂停兼容状态。它不保存天气事件、节日、入侵或 DD2 状态。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct CalendarClockComponent
{
  public const int DefaultDayLengthTicks = 54000;
  public const int DefaultNightLengthTicks = 32400;

  public CalendarClockComponent(
    long tickNumber,
    double timeOfDay,
    bool isDayTime = true,
    int dayRate = 1,
    int dayLengthTicks = DefaultDayLengthTicks,
    int nightLengthTicks = DefaultNightLengthTicks,
    byte moonPhase = 0,
    bool isPaused = false)
  {
    TickNumber = tickNumber;
    TimeOfDay = timeOfDay;
    IsDayTime = isDayTime;
    DayRate = dayRate;
    DayLengthTicks = dayLengthTicks;
    NightLengthTicks = nightLengthTicks;
    MoonPhase = moonPhase;
    IsPaused = isPaused;
    Validate();
  }

  public long TickNumber;
  public double TimeOfDay;
  public bool IsDayTime;
  public int DayRate;
  public int DayLengthTicks;
  public int NightLengthTicks;
  public byte MoonPhase;
  public bool IsPaused;

  public int CurrentCycleLength => IsDayTime ? DayLengthTicks : NightLengthTicks;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(TickNumber);
    ArgumentOutOfRangeException.ThrowIfNegative(DayRate);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(DayLengthTicks);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(NightLengthTicks);

    if (!double.IsFinite(TimeOfDay) ||
        TimeOfDay < 0.0d ||
        TimeOfDay >= CurrentCycleLength)
    {
      throw new ArgumentOutOfRangeException(nameof(TimeOfDay));
    }

    if (MoonPhase > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(MoonPhase));
    }
  }
}
```

草案决策：`TimeOfDay` 不提供默认参数，因为 Version4 `Main.time = 13500.0` 与 Dome `WorldClock` 的 `timeOfDay = 0.0` 冲突。`DayRate = 0` 的运行语义也必须先由 integration-review 裁决；这里只允许非负值。

### 4.2 WorldEventInstanceStateComponent

逻辑 Component：`WorldEventInstanceState`。建议文件：`World/Calendar/WorldEventInstanceStateComponent.cs`。

职责：把事件种类和一次事件实例的身份、生命周期、时间边界及可选外部引用分开。它不保存 Party、Sandstorm、入侵或 DD2 的专属字段。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct WorldEventInstanceStateComponent
{
  public WorldEventInstanceStateComponent(
    EventInstanceId eventInstanceId,
    WorldEventKind eventKind,
    WorldEventLifecycleState lifecycleState)
  {
    EventInstanceId = eventInstanceId;
    EventKind = eventKind;
    LifecycleState = lifecycleState;
    StartedAtTick = null;
    EndedAtTick = null;
    OwnerEntityId = null;
    PersistentEntityId = null;
    NetworkId = null;
    Validate();
  }

  public EventInstanceId EventInstanceId;
  public WorldEventKind EventKind;
  public WorldEventLifecycleState LifecycleState;
  public long? StartedAtTick;
  public long? EndedAtTick;
  public EntityId? OwnerEntityId;
  public PersistentEntityId? PersistentEntityId;
  public NetworkId? NetworkId;

  public void Validate()
  {
    if (StartedAtTick.HasValue && StartedAtTick.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(StartedAtTick));
    }

    if (EndedAtTick.HasValue && EndedAtTick.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(EndedAtTick));
    }

    if (StartedAtTick.HasValue &&
        EndedAtTick.HasValue &&
        EndedAtTick.Value < StartedAtTick.Value)
    {
      throw new ArgumentException(
        "An event cannot end before it starts.",
        nameof(EndedAtTick));
    }
  }
}
```

`EventInstanceId` 的非零规则、`EventKind` 的完整枚举、生命周期状态的转移和值对象 namespace 未闭合，因此本代码不伪造 `IsValid` 或自动生成实例 ID。

### 4.3 BirthdayPartyStateComponent

逻辑 Component：`BirthdayPartyState`。建议文件：`World/Calendar/BirthdayPartyStateComponent.cs`。

职责：保存手动 Party、自然 Party、冷却和参与 NPC 的关系；`IsUp` 是派生值，`WasCelebrating` 只是边界缓存。

```csharp
using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public struct BirthdayPartyStateComponent
{
  public BirthdayPartyStateComponent(
    bool manualParty = false,
    bool genuineParty = false,
    int partyDaysOnCooldown = 0,
    IReadOnlyList<NpcEntityId>? celebratingNpcIds = null,
    bool wasCelebrating = false)
  {
    ManualParty = manualParty;
    GenuineParty = genuineParty;
    PartyDaysOnCooldown = partyDaysOnCooldown;
    CelebratingNpcIds = celebratingNpcIds ?? Array.Empty<NpcEntityId>();
    WasCelebrating = wasCelebrating;
    Validate();
  }

  public bool ManualParty;
  public bool GenuineParty;
  public int PartyDaysOnCooldown;
  public IReadOnlyList<NpcEntityId> CelebratingNpcIds;
  public bool WasCelebrating;

  public bool IsUp => ManualParty || GenuineParty;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(PartyDaysOnCooldown);
    ArgumentNullException.ThrowIfNull(CelebratingNpcIds);

    HashSet<NpcEntityId> ids = new();
    for (int index = 0; index < CelebratingNpcIds.Count; index++)
    {
      if (!ids.Add(CelebratingNpcIds[index]))
      {
        throw new ArgumentException(
          "A birthday party cannot contain a duplicate NPC reference.",
          nameof(CelebratingNpcIds));
      }
    }
  }
}
```

`CelebratingNpcIds` 必须在最终写入边界进行快照化；Version4 的 `List<int>` 是 `whoAmI` 候选，不得直接声明为 `NpcEntityId` 或 `PersistentEntityId`。

### 4.4 LanternNightStateComponent

逻辑 Component：`LanternNightState`。建议文件：`World/Calendar/LanternNightStateComponent.cs`。

职责：保存 Lantern Night 的手动/自然来源、下一晚预定、冷却和边界缓存。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct LanternNightStateComponent
{
  public LanternNightStateComponent(
    bool manualLanterns = false,
    bool genuineLanterns = false,
    bool nextNightIsLanternNight = false,
    int lanternNightsOnCooldown = 0,
    bool wasLanternNight = false)
  {
    ManualLanterns = manualLanterns;
    GenuineLanterns = genuineLanterns;
    NextNightIsLanternNight = nextNightIsLanternNight;
    LanternNightsOnCooldown = lanternNightsOnCooldown;
    WasLanternNight = wasLanternNight;
    Validate();
  }

  public bool ManualLanterns;
  public bool GenuineLanterns;
  public bool NextNightIsLanternNight;
  public int LanternNightsOnCooldown;
  public bool WasLanternNight;

  public bool IsUp => ManualLanterns || GenuineLanterns;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(LanternNightsOnCooldown);
  }
}
```

`NextNightIsLanternNight` 不得由 `IsUp` 派生；它表示未来窗口的预定事实。完整 `NaturalAttempt`、`BossIsActive` 和排期序列仍是 partial。

### 4.5 SandstormStateComponent

逻辑 Component：`SandstormState`。建议文件：`World/Calendar/SandstormStateComponent.cs`。

职责：保存沙尘暴活动、剩余时间、当前强度和目标强度；不保存风速、沙漠区域判定或视觉表现。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct SandstormStateComponent
{
  public const int MaximumDurationTicks = 86400;

  public SandstormStateComponent(
    bool happening = false,
    int timeLeft = 0,
    float severity = 0.0f,
    float intendedSeverity = 0.0f)
  {
    Happening = happening;
    TimeLeft = timeLeft;
    Severity = severity;
    IntendedSeverity = intendedSeverity;
    Validate();
  }

  public bool Happening;
  public int TimeLeft;
  public float Severity;
  public float IntendedSeverity;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(TimeLeft);
    if (TimeLeft > MaximumDurationTicks)
    {
      throw new ArgumentOutOfRangeException(nameof(TimeLeft));
    }

    ValidateSeverity(Severity, nameof(Severity));
    ValidateSeverity(IntendedSeverity, nameof(IntendedSeverity));
  }

  private static void ValidateSeverity(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
```

Version4 `UpdateSeverity` 会处理 NaN，当前代码草案选择在组件边界拒绝非法值；这项“拒绝还是归零”的兼容决策需要在行为保持审查中单独确认，不能仅由类型草案决定。

### 4.6 SlimeRainStateComponent

逻辑 Component：`SlimeRainState`。建议文件：`World/Calendar/SlimeRainStateComponent.cs`。

职责：保留 Version4 `slimeRainTime` 的正/负双重语义、活动标志、击杀计数和警告计时。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct SlimeRainStateComponent
{
  public const int DefaultWarningDelay = 420;

  public SlimeRainStateComponent(
    bool active = false,
    double timeState = 0.0d,
    int killCount = 0,
    int warningTime = 0,
    int warningDelay = DefaultWarningDelay)
  {
    Active = active;
    TimeState = timeState;
    KillCount = killCount;
    WarningTime = warningTime;
    WarningDelay = warningDelay;
    Validate();
  }

  public bool Active;
  public double TimeState;
  public int KillCount;
  public int WarningTime;
  public int WarningDelay;

  public void Validate()
  {
    if (!double.IsFinite(TimeState))
    {
      throw new ArgumentOutOfRangeException(nameof(TimeState));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(KillCount);
    ArgumentOutOfRangeException.ThrowIfNegative(WarningTime);
    ArgumentOutOfRangeException.ThrowIfNegative(WarningDelay);

    if (Active && TimeState <= 0.0d)
    {
      throw new ArgumentException(
        "An active Slime Rain must have a positive active time state.",
        nameof(TimeState));
    }
  }
}
```

`TimeState < 0` 表示 Version4 停止后的冷却阶段，不应被压扁成非负的 `TimeRemaining`。`SlimeRainNpcSlots` 和 `slimeRainNPC` 不属于本 Component，而属于生成压力边界。

### 4.7 WorldInvasionStateComponent

逻辑 Component：`WorldInvasionState`。建议文件：`World/Calendar/WorldInvasionStateComponent.cs`。

职责：保存普通入侵的类型、世界坐标、规模、延迟、警告和显示进度；不保存完成历史或 NPC 生成策略。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct WorldInvasionStateComponent
{
  public WorldInvasionStateComponent(
    InvasionType type = InvasionType.None,
    double positionX = 0.0d,
    int size = 0,
    int sizeStart = 0,
    int delay = 0,
    int warningTimer = 0,
    int progress = 0,
    int progressMax = 0,
    int progressIcon = 0,
    int progressWave = 0)
  {
    Type = type;
    PositionX = positionX;
    Size = size;
    SizeStart = sizeStart;
    Delay = delay;
    WarningTimer = warningTimer;
    Progress = progress;
    ProgressMax = progressMax;
    ProgressIcon = progressIcon;
    ProgressWave = progressWave;
    Validate();
  }

  public InvasionType Type;
  public double PositionX;
  public int Size;
  public int SizeStart;
  public int Delay;
  public int WarningTimer;
  public int Progress;
  public int ProgressMax;
  public int ProgressIcon;
  public int ProgressWave;

  public bool IsActive => Type != InvasionType.None && Size > 0;

  public void Validate()
  {
    if (!double.IsFinite(PositionX))
    {
      throw new ArgumentOutOfRangeException(nameof(PositionX));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(Size);
    ArgumentOutOfRangeException.ThrowIfNegative(SizeStart);
    ArgumentOutOfRangeException.ThrowIfNegative(Delay);
    ArgumentOutOfRangeException.ThrowIfNegative(WarningTimer);
    ArgumentOutOfRangeException.ThrowIfNegative(Progress);
    ArgumentOutOfRangeException.ThrowIfNegative(ProgressMax);
    ArgumentOutOfRangeException.ThrowIfNegative(ProgressIcon);
    ArgumentOutOfRangeException.ThrowIfNegative(ProgressWave);

    if (IsActive && SizeStart < Size)
    {
      throw new ArgumentException(
        "An active invasion cannot exceed its starting size.",
        nameof(SizeStart));
    }

    if (!IsActive && Type == InvasionType.None && Size != 0)
    {
      throw new ArgumentException(
        "An inactive invasion cannot retain a non-zero size.",
        nameof(Size));
    }
  }
}
```

`Progress` 与 `ProgressMax` 的精确单位仍需对齐 Version4 `ReportInvasionProgress` 和客户端显示语义；本草案只校验非负，不强制未经证据确认的上限关系。

### 4.8 InvasionHistoryStateComponent

逻辑 Component：`InvasionHistoryState`。建议文件：`World/Calendar/InvasionHistoryStateComponent.cs`。

职责：保存已经完成的普通入侵历史，和当前入侵运行时完全分离。

```csharp
namespace Terraria.Dome.Simulation.WorldModel;

public struct InvasionHistoryStateComponent
{
  public bool DefeatedGoblins;
  public bool DefeatedFrost;
  public bool DefeatedPirates;
  public bool DefeatedMartians;

  // Compatibility candidate only. Version4 direct evidence for this flag is incomplete.
  public bool DefeatedClown;
}
```

这些字段没有主动行为。历史标志的写入必须来自已确认的完成事实；当前入侵活动、警告或进度不得直接修改历史。`DefeatedClown` 仅为当前 NLTX 局部模型的兼容候选，不能作为 Version4 基线。

### 4.9 PendingWorldEventStateComponent

逻辑 Component：`PendingWorldEventState`。建议文件：`World/Calendar/PendingWorldEventStateComponent.cs`。

职责：保存尚未成为活动事件实例的世界事件意图和临时日历标志。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct PendingWorldEventStateComponent
{
  public PendingWorldEventStateComponent(
    bool spawnEye = false,
    int spawnHardBoss = 0,
    bool spawnMeteor = false,
    int meteorShowerCount = 0,
    bool afterPartyOfDoom = false,
    bool forceHalloweenForToday = false,
    bool forceChristmasForToday = false)
  {
    SpawnEye = spawnEye;
    SpawnHardBoss = spawnHardBoss;
    SpawnMeteor = spawnMeteor;
    MeteorShowerCount = meteorShowerCount;
    AfterPartyOfDoom = afterPartyOfDoom;
    ForceHalloweenForToday = forceHalloweenForToday;
    ForceChristmasForToday = forceChristmasForToday;
    Validate();
  }

  public bool SpawnEye;
  public int SpawnHardBoss;
  public bool SpawnMeteor;
  public int MeteorShowerCount;
  public bool AfterPartyOfDoom;
  public bool ForceHalloweenForToday;
  public bool ForceChristmasForToday;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(SpawnHardBoss);
    ArgumentOutOfRangeException.ThrowIfNegative(MeteorShowerCount);
  }
}
```

`SpawnMeteor = true` 不代表流星已经落地，`ForceHalloweenForToday = true` 也不代表 Pumpkin Moon 已经活动。字段的消费/保留跨越仍是 partial。

### 4.10 WorldCalendarOverrideStateComponent

逻辑 Component：`WorldCalendarOverrideState`。建议文件：`World/Calendar/WorldCalendarOverrideStateComponent.cs`。

职责：保存血月、日食、季节月和时间控制覆盖。基础时间位置仍由 `CalendarClockComponent` 所有。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct WorldCalendarOverrideStateComponent
{
  public WorldCalendarOverrideStateComponent(
    bool bloodMoon = false,
    bool eclipse = false,
    bool pumpkinMoon = false,
    bool snowMoon = false,
    bool fastForwardTimeToDawn = false,
    bool fastForwardTimeToDusk = false,
    int sundialCooldown = 0,
    int moondialCooldown = 0)
  {
    BloodMoon = bloodMoon;
    Eclipse = eclipse;
    PumpkinMoon = pumpkinMoon;
    SnowMoon = snowMoon;
    FastForwardTimeToDawn = fastForwardTimeToDawn;
    FastForwardTimeToDusk = fastForwardTimeToDusk;
    SundialCooldown = sundialCooldown;
    MoondialCooldown = moondialCooldown;
    Validate();
  }

  public bool BloodMoon;
  public bool Eclipse;
  public bool PumpkinMoon;
  public bool SnowMoon;
  public bool FastForwardTimeToDawn;
  public bool FastForwardTimeToDusk;
  public int SundialCooldown;
  public int MoondialCooldown;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(SundialCooldown);
    ArgumentOutOfRangeException.ThrowIfNegative(MoondialCooldown);

    if (PumpkinMoon && SnowMoon)
    {
      throw new ArgumentException(
        "Pumpkin Moon and Snow Moon cannot be active together.",
        nameof(SnowMoon));
    }

    if (BloodMoon && (PumpkinMoon || SnowMoon))
    {
      throw new ArgumentException(
        "Blood Moon cannot be active with a seasonal moon.",
        nameof(BloodMoon));
    }
  }
}
```

`Eclipse` 与 `BloodMoon` 的完整互斥关系不能仅由当前片段推导；本代码只实现有直接证据的季节月互斥和血月排除规则。

### 4.11 Dd2PersistentProgressStateComponent

逻辑 Component：`Dd2PersistentProgressState`。建议文件：`World/Calendar/Dd2PersistentProgressStateComponent.cs`。

职责：保存 DD2 Tier 1、Tier 2、Tier 3 的世界持久完成标志。

```csharp
namespace Terraria.Dome.Simulation.WorldModel;

public struct Dd2PersistentProgressStateComponent
{
  public bool DownedInvasionT1;
  public bool DownedInvasionT2;
  public bool DownedInvasionT3;
}
```

该组件不包含 `Ongoing`、`WonThisRun`、`LostThisRun`、Arena 或波次字段。Version4 `DD2Event.Save/Load` 对三个字段有直接证据；其余版本兼容分支仍需后续核对。

### 4.12 Dd2RunStateComponent

逻辑 Component：`Dd2RunState`。建议文件：`World/Calendar/Dd2RunStateComponent.cs`。

职责：保存一局 DD2 的活动、胜负、难度、车道生成率和竞技场空间值。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct Dd2RunStateComponent
{
  public Dd2RunStateComponent(
    bool ongoing = false,
    bool lostThisRun = false,
    bool wonThisRun = false,
    int ongoingDifficulty = 0,
    int laneSpawnRate = 60,
    WorldTileRectangle arenaHitbox = default)
  {
    Ongoing = ongoing;
    LostThisRun = lostThisRun;
    WonThisRun = wonThisRun;
    OngoingDifficulty = ongoingDifficulty;
    LaneSpawnRate = laneSpawnRate;
    ArenaHitbox = arenaHitbox;
    Validate();
  }

  public bool Ongoing;
  public bool LostThisRun;
  public bool WonThisRun;
  public int OngoingDifficulty;
  public int LaneSpawnRate;
  public WorldTileRectangle ArenaHitbox;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(OngoingDifficulty);
    ArgumentOutOfRangeException.ThrowIfNegative(LaneSpawnRate);

    if (LostThisRun && WonThisRun)
    {
      throw new ArgumentException(
        "A DD2 run cannot be both won and lost.",
        nameof(WonThisRun));
    }
  }
}
```

`ArenaHitbox` 只是候选空间值或缓存，不代表地图几何的最终 owner。`Ongoing = false` 时是否保留最终胜负结果，需和 Version4 的停止路径一起裁决；本草案不自动清除字段。

### 4.13 Dd2WaveRuntimeStateComponent

逻辑 Component：`Dd2WaveRuntimeState`。建议文件：`World/Calendar/Dd2WaveRuntimeStateComponent.cs`。

职责：保存 DD2 当前波次、波次击杀、入侵点数、波次等待、死 Goblin 位置和水晶掉落缓存。

```csharp
using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public struct Dd2WaveRuntimeStateComponent
{
  public Dd2WaveRuntimeStateComponent(
    int currentWave = 0,
    float waveKills = 0.0f,
    float totalInvasionPoints = 0.0f,
    int timeLeftUntilSpawningBegins = 0,
    IReadOnlyList<WorldTileCoordinate>? deadGoblinPositions = null,
    int crystalsDroppingLastWave = 0,
    int crystalsDroppingToDrop = 0,
    int crystalsDroppingAlreadyDropped = 0)
  {
    CurrentWave = currentWave;
    WaveKills = waveKills;
    TotalInvasionPoints = totalInvasionPoints;
    TimeLeftUntilSpawningBegins = timeLeftUntilSpawningBegins;
    DeadGoblinPositions = deadGoblinPositions ?? Array.Empty<WorldTileCoordinate>();
    CrystalsDroppingLastWave = crystalsDroppingLastWave;
    CrystalsDroppingToDrop = crystalsDroppingToDrop;
    CrystalsDroppingAlreadyDropped = crystalsDroppingAlreadyDropped;
    Validate();
  }

  public int CurrentWave;
  public float WaveKills;
  public float TotalInvasionPoints;
  public int TimeLeftUntilSpawningBegins;
  public IReadOnlyList<WorldTileCoordinate> DeadGoblinPositions;
  public int CrystalsDroppingLastWave;
  public int CrystalsDroppingToDrop;
  public int CrystalsDroppingAlreadyDropped;

  public bool EnemySpawningIsOnHold => TimeLeftUntilSpawningBegins != 0;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(CurrentWave);
    ArgumentOutOfRangeException.ThrowIfNegative(TimeLeftUntilSpawningBegins);
    ArgumentOutOfRangeException.ThrowIfNegative(CrystalsDroppingLastWave);
    ArgumentOutOfRangeException.ThrowIfNegative(CrystalsDroppingToDrop);
    ArgumentOutOfRangeException.ThrowIfNegative(CrystalsDroppingAlreadyDropped);
    ArgumentNullException.ThrowIfNull(DeadGoblinPositions);

    if (!float.IsFinite(WaveKills) || WaveKills < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(WaveKills));
    }

    if (!float.IsFinite(TotalInvasionPoints) || TotalInvasionPoints < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(TotalInvasionPoints));
    }

    if (CrystalsDroppingAlreadyDropped > CrystalsDroppingToDrop)
    {
      throw new ArgumentException(
        "Dropped crystals cannot exceed the pending crystal count.",
        nameof(CrystalsDroppingAlreadyDropped));
    }
  }
}
```

`WorldTileCoordinate` 可复用当前 Dome 的值类型；`DeadGoblinPositions` 不是 NPC ID 集合。完整 DD2 波次、点数、Betsy 和水晶算法仍为 partial，不能由这个字段草案补齐。

### 4.14 WorldEventRandomStateComponent

逻辑 Component：`WorldEventRandomState`。建议文件：`World/Calendar/WorldEventRandomStateComponent.cs`。

职责：保存事件随机流的可恢复状态、世界种子关联、流版本和可选消费计数；不定义随机算法，也不保存随机结果的业务含义。

```csharp
using System;

namespace Terraria.Dome.Simulation.WorldModel;

public struct WorldEventRandomStateComponent
{
  public WorldEventRandomStateComponent(
    uint state,
    ulong worldSeed,
    int streamVersion,
    long consumedDrawCount = 0)
  {
    State = state;
    WorldSeed = worldSeed;
    StreamVersion = streamVersion;
    ConsumedDrawCount = consumedDrawCount;
    Validate();
  }

  public uint State;
  public ulong WorldSeed;
  public int StreamVersion;
  public long ConsumedDrawCount;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(StreamVersion);
    ArgumentOutOfRangeException.ThrowIfNegative(ConsumedDrawCount);
  }
}
```

此处不能把 `state = 0` 当作无效值，也不能把 `worldSeed` 直接等同于 Version4 文本种子。Dome 的 `WorldEventRandomState.Value` 和 Version4 的 `Main.rand` 只证明存在不同证据来源，不证明算法或恢复语义相同。

## 5. 组件之间的组合示例

以下仅说明数据组合，不定义 Entity 创建方式、System、Query、Command、Adapter、Projection 或调度顺序：

```text
World singleton
  CalendarClockComponent
  WorldCalendarOverrideStateComponent
  BirthdayPartyStateComponent
  LanternNightStateComponent
  SandstormStateComponent
  SlimeRainStateComponent
  WorldInvasionStateComponent
  InvasionHistoryStateComponent
  PendingWorldEventStateComponent
  Dd2PersistentProgressStateComponent
  Dd2RunStateComponent
  Dd2WaveRuntimeStateComponent
  WorldEventRandomStateComponent

World event instance entity or world event record (final boundary unresolved)
  WorldEventInstanceStateComponent
  one event-specific component when the instance requires it
```

组合约束：

- `CalendarClockComponent` 是时间位置的唯一候选来源；override 不复制 `TimeOfDay`。
- `WorldEventInstanceStateComponent` 只保存通用身份和生命周期；事件专属字段保留在各自 Component。
- `WorldInvasionStateComponent` 与 `InvasionHistoryStateComponent` 不互相复制当前规模和完成历史。
- DD2 的持久进度、运行状态和波次运行时状态拥有不同生命周期，不能合并为一个宽 Component。
- Pending 意图不等于活动实例；不使用同一个布尔字段同时表达两者。
- `WorldEventRandomStateComponent` 是随机依赖事实，不是任何事件的结果字段。

## 6. 当前 NLTX 映射

| 草案代码类型 | 当前映射 | 映射结论 |
| --- | --- | --- |
| `CalendarClockComponent` | `dome/.../World/WorldClock.cs`、`src/WorldSession/WorldSessionComponents.cs` 的 `WorldTimeWeatherState` | 局部已有；默认 `TimeOfDay` 存在 Version4/Dome 冲突 |
| `WorldEventInstanceStateComponent` | `WorldEventKind.cs`、`WorldClockTransition.cs`、各事件局部状态 | 没有统一实例身份，仍为 evidence-gap |
| `BirthdayPartyStateComponent` | `src/WorldSession/WorldSessionComponents.cs` 的 `BirthdayPartyState` | 字段形状接近，NPC ID 和完整自然行为未闭合 |
| `LanternNightStateComponent` | `src/WorldSession/WorldSessionComponents.cs` 的 `LanternNightState`、Dome `WorldProgressionState` | 存在排期序列与状态分散，判为 partial |
| `SandstormStateComponent` | `src/WorldSession/WorldSessionComponents.cs` 的 `SandstormState` | 四个核心字段接近，最终 writer 未裁决 |
| `SlimeRainStateComponent` | `WorldTimeWeatherState` 和 Dome `WorldProgressionState` | Version4 正/负计时与 Dome 非负 ticks/cooldown 表示不同 |
| `WorldInvasionStateComponent` | `InvasionRuntimeState`、Dome `WorldProgressionState` | 字段形状较完整，调用图和单一写集未确认 |
| `InvasionHistoryStateComponent` | `InvasionProgressFlags`、`WorldProgressionState` 的 defeated 字段 | 历史字段存在局部映射，完整持久边界未确认 |
| `PendingWorldEventStateComponent` | `PendingWorldEventsState` | 字段形状接近，消费/恢复边界未闭合 |
| `WorldCalendarOverrideStateComponent` | `WorldTimeWeatherState`、`PendingWorldEventsState` | 当前没有独立目标 Component |
| `Dd2PersistentProgressStateComponent` | `Dd2ProgressState.DownedTier1/2/3`、Version4 `DD2Event.Save/Load` | 持久三层字段有直接证据 |
| `Dd2RunStateComponent` | `Dd2ProgressState` 的运行字段、Version4 `DD2Event` | 当前没有独立 Arena 字段，判为 partial |
| `Dd2WaveRuntimeStateComponent` | DD2 局部字段与 NPC 静态波次字段 | 波次字段跨 `DD2Event`/`NPC`，owner 未裁决 |
| `WorldEventRandomStateComponent` | Dome `WorldEventRandomState`、持久化格式版本 37 | Dome 有局部实现，Version4 `Main.rand` 兼容性未证明 |

现有 `WorldSessionComponents.cs`、`WorldProgressionState.cs` 和 Dome 局部类型不能直接替换为本文代码块，也不能仅凭字段同名宣称迁移完成、行为等价或 API 兼容。

## 7. 不写入代码的内容

为了保持 Component-only 边界，本文和本轮实现均不提供以下行为代码：

- 任何 System、Query、Command、Adapter、Projection、Coordinator、Pipeline、Scheduler 或 Verifier；
- 事件启动/结束算法、昼夜推进算法、入侵移动算法、NPC 生成算法、DD2 波次算法或随机算法；
- 任何网络包、WorldFile 编解码、存档读取/写入、日志、指标或 UI 投影；
- 任何 `Main.rand`、全局静态状态、文件流、网络流、时钟服务或外部进程依赖；
- 任何跨域 ID 的最终整合实现；本轮仅提供标注为 provisional 的本地契约类型；
- 任何测试文件或测试项目；本轮未创建、未运行测试。

## 8. 实现前必须裁决的代码问题

| 决策 ID | 需要裁决的代码问题 | 影响 |
| --- | --- | --- |
| `BD-WCE-CODE-01` | `CalendarClockComponent.TimeOfDay` 初值采用 Version4 `13500.0` 还是 Dome `0.0`；`DayRate = 0` 的语义是什么 | 构造函数、恢复校验、兼容快照 |
| `BD-WCE-CODE-02` | Component 是否采用当前 Dome 的 `struct` 数据形态，或对列表和实例改用 class/不可变快照 | 拷贝语义、ECS 存储和可变集合安全 |
| `BD-WCE-CODE-03` | `EventInstanceId` 是否为独立 World 记录、ECS Entity 身份或持久实体引用 | 通用实例 Component 的全部关系字段 |
| `BD-WCE-CODE-04` | `InvasionType` 的 Dome namespace、枚举范围和 Version4 兼容值 | `WorldInvasionStateComponent` 的可编译性和反序列化 |
| `BD-WCE-CODE-05` | `WorldTileRectangle` 的实际几何值类型和空间 owner | `Dd2RunStateComponent.ArenaHitbox` |
| `BD-WCE-CODE-06` | `IReadOnlyList` 是否由不可变快照、数组复制或 ECS 专属集合承载 | Party 和 DD2 波次的状态安全 |
| `BD-WCE-CODE-07` | Sandstorm NaN 采用拒绝还是 Version4 的归零兼容语义 | `SandstormStateComponent.Validate` |
| `BD-WCE-CODE-08` | Version4 `Main.rand` 与 Dome 世界种子随机流是否兼容，是否需要显式 `StreamVersion` | `WorldEventRandomStateComponent` 的恢复和重放 |
| `BD-WCE-CODE-09` | Dome 当前持久化 `CurrentFormatVersion = 37` 如何与 Version4 字段版本边界共同解释 | 所有 snapshot/compatibility 字段 |

所有跨域问题保持 `crossSubsystemOwner: integration-review`，不能由本代码草案默认解决。

## 9. 草案检查记录

本轮实现记录如下：

- `verificationStatus: build-passed-no-tests`；
- 编译命令通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行，目标为 `src/WorldSession/Terraria.WorldSession.csproj`；退出码为 `0`，0 个警告，0 个错误；
- 产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.WorldSession\Debug\net10.0\Terraria.WorldSession.dll`；
- 未创建、未运行测试；未执行测试项目、覆盖率、格式化或源代码生成命令；
- 已修改 `src/WorldSession/Calendar/` 下的实际组件和本草案文档；没有改动既有 `WorldSessionComponents.cs`、`dome/src/`、Version4 或完整参考源码。

## 10. 最终声明

本文是 `WorldCalendarAndEventOrchestration` 的 C# 组件契约与实现记录，不是行为等价证明、迁移完成报告或 API 兼容证明。

本文提供 14 个 Component 的文件名、领域目录、命名空间、字段、默认值、纯不变量检查和组合边界；14 个核心组件已在 `src/WorldSession/Calendar/` 落地，跨域值类型仍标记为 provisional。

本文没有定义或实现 System、Query、Command、Adapter、Projection、Coordinator、Pipeline、Scheduler、Verifier、调度顺序、主循环、网络流程、存档流程、测试计划或迁移计划。跨域 ID、事件实例身份、空间几何、随机流、时间默认值和 Version4 缺失行为仍需 integration-review 裁决。

实际 `.cs` 文件已按仓库 ECS 文件组织约束逐个创建并完成生产项目编译验证；owner、跨域类型契约、Version4 行为保持证据、时间默认值和随机流兼容性仍需后续 integration-review，不能由本轮组件落地推导。
