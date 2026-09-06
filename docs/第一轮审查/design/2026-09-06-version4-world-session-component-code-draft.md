# Version4 WorldSession 实际代码组件草案

## 1. 文档元数据

~~~yaml
documentType: Component-code-draft
designId: WS.CODE-DRAFT.2026-09-06
subsystemId: WorldSession
taskNumber: 04
sourceDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-world-session-component-design.md
sourceReportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-session-public-decomposition.md
targetProject: D:\TRbackup\NLTX\src\WorldSession\Terraria.WorldSession.csproj
targetNamespace: Terraria.WorldSession.Components
targetRoot: D:\TRbackup\NLTX\src\WorldSession
componentCount: 6
draftStatus: implemented-from-draft
implementationStatus: partial
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: build-only
testStatus: not-created-by-user-request
generatedDate: 2026-09-06
pathStatus: proposed
~~~

本文件把 [WorldSession Component-only Design](D:\TRbackup\NLTX\docs\design\2026-09-05-version4-world-session-component-design.md) 中的 6 个 Component 候选落成 C# 类型草案；本轮已按该草案创建对应源码文件并完成生产项目编译。文档仍不表示运行时接线、行为等价、协议兼容或跨域 owner 已经完成裁决。

本文件不改变上一份设计中的证据结论。HardMode、readiness 完整转移、跨域事件/进度、共享世界身份和 WorldTickSnapshot 的最终 owner 仍然是 decision-required；所有相关跨子系统字段继续标记为 crossSubsystemOwner: integration-review。

## 2. 草案使用规则

### 2.1 代码形状

- 目标命名空间暂定为 Terraria.WorldSession.Components，以减少当前根 src/WorldSession/WorldSessionComponents.cs 的 API 迁移噪声。
- 组件按 WorldSession 领域直接放在 src/WorldSession/；当前领域规模尚不足以预建通用 Components 子目录。
- 一个核心公开类型对应一个同名 PascalCase 文件；本文中的 File 标记表示 canonical 文件路径。
- 普通 Component 使用 sealed class 和字段存储，便于后续接入当前尚未确认的 ECS attachment 机制；派生值只使用表达式属性，不形成第二份权威状态。
- WorldTickSnapshot 使用不可变值类型和只读属性；它不能反向修改五个核心状态 Component。
- 组件不创建线程、任务、文件流、锁、随机源、日志、网络连接或持久化 reader/writer。

### 2.2 代码草案与实际实现的区别

以下代码可以作为实现起点，但在合并到项目之前仍需要完成：

- 选择根 WorldSessionComponents.cs 与 WorldGeneration/ 中同名类型的唯一 owner；
- 确认 WorldGameMode、WorldSecretSeedFlags、WorldEvilType、OreTierState 和 WorldBounds 的最终命名空间及引用方；
- 确认 SessionReadinessPhase 是否保留 Version4 的 AwaitingData/ProcessingData 语义，还是由整合层引入更细的加载/生成阶段；
- 确认快照 revision 和跨域原子提交边界；
- 以 focused verifier 覆盖默认值、不变量、生命周期门控和快照不可变性；
- 按仓库的串行构建约束验证受影响项目。

## 3. 建议文件布局

这是小型 WorldSession 领域的扁平目标布局。它不要求本轮立即移动或删除已有文件。

~~~text
D:\TRbackup\NLTX\src\WorldSession\
  Terraria.WorldSession.csproj
  WorldBounds.cs
  WorldGameMode.cs
  WorldSecretSeedFlags.cs
  WorldEvilType.cs
  OreTierState.cs
  SessionReadinessPhase.cs
  WorldDescriptorState.cs
  WorldRulesState.cs
  WorldClockState.cs
  WorldWeatherState.cs
  SessionReadinessState.cs
  WorldDescriptorSnapshotValue.cs
  WorldRulesSnapshotValue.cs
  WorldClockSnapshotValue.cs
  WorldWeatherSnapshotValue.cs
  SessionReadinessSnapshotValue.cs
  WorldTickSnapshot.cs
~~~

目标布局中没有 Systems、Queries、Adapters 或 Projections 目录，因为本草案只定义 Component 和 Component 所需的值类型。快照构造的跨组件读取仍需由后续整合边界负责，不在 Component 文件中偷偷实现。

## 4. 共同的类型和所有权约束

### 4.1 ID 分类

| 类型 | 用途 | 是否由本草案拥有 |
|---|---|---|
| WorldId | Version4 世界持久整数身份 | 作为 WorldDescriptorState 字段候选；最终共享 owner 未决 |
| UniqueId | Version4 世界 GUID | 作为 WorldDescriptorState 字段候选；最终共享 owner 未决 |
| Entity ID | ECS 实体身份 | 不进入任何 WorldSession Component |
| Network ID | 网络会话/协议实体身份 | 不进入任何 WorldSession Component |
| WorldSectionId | 区段身份 | 不进入 Descriptor 或 Snapshot |
| 持久化文件键 | 文件边界键 | 不与 WorldId 或 UniqueId 合并 |
| 账户 ID / 连接 ID | 用户和连接身份 | 不进入任何 WorldSession Component |
| Revision | 快照版本候选 | 只服务 WorldTickSnapshot，不是网络序号或持久化 ID |

所有跨两个或以上子系统使用的字段、值类型、ID 或关系都仍然属于 crossSubsystemOwner: integration-review。代码中的字段名不是最终 owner 声明。

### 4.2 状态分类

- 权威字段：该概念在运行期的唯一候选写入来源。
- 派生属性：从同一 Component 的字段计算，不单独写入或持久化。
- 缓存字段：可以丢弃并重建，只有确认失效条件后才可保留。
- 兼容字段：服务于 Version4 导入/导出或版本兼容，不自动成为新的运行时 owner。
- 快照字段：某次提交点的不可变副本，不得反向驱动权威状态。

## 5. 支持值类型草案

这些类型是字段类型，不是可独立附着的 Component。每个公开类型仍应落在自己的同名文件中。

### 5.1 WorldBounds

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldBounds.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public readonly record struct WorldBounds(
  double Left,
  double Top,
  double Right,
  double Bottom);
~~~

当前不在值类型构造函数中强制边界严格关系，因为上一份设计将“边界严格递增、锚点始终在边界内”的统一证据标为 partial。加载/生成提交边界应在后续 owner 确认后校验，而不是由这个值类型猜测 Version4 规则。

### 5.2 WorldGameMode

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldGameMode.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public enum WorldGameMode : byte
{
  Classic,
  Expert,
  Master,
  Journey,
}
~~~

这是对当前 WorldGeneration.Components.WorldGameMode 候选的命名空间收敛草案。Version4 世界头的数值映射和 Creative/Journey 有效覆写 owner 仍需整合确认，不能只依据枚举顺序宣布协议兼容完成。

### 5.3 WorldSecretSeedFlags

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldSecretSeedFlags.cs

~~~csharp
using System;

namespace Terraria.WorldSession.Components;

[Flags]
public enum WorldSecretSeedFlags : ulong
{
  None = 0,
  Drunk = 1UL << 0,
  ForTheWorthy = 1UL << 1,
  TenthAnniversary = 1UL << 2,
  DontStarve = 1UL << 3,
  NotTheBees = 1UL << 4,
  Remix = 1UL << 5,
  NoTraps = 1UL << 6,
  Zenith = 1UL << 7,
  Skyblock = 1UL << 8,
  Vampire = 1UL << 9,
  Infected = 1UL << 10,
  TeamBasedSpawns = 1UL << 11,
  DualDungeons = 1UL << 12,
  GetGoodWorld = 1UL << 13,
}
~~~

该 flags 类型只表达解析后的标志值。解析算法、秘密种子有效覆写和 Version4 当前空体方法的兼容策略不放入 Component。

### 5.4 WorldEvilType

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldEvilType.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public enum WorldEvilType : byte
{
  Corruption,
  Crimson,
}
~~~

### 5.5 OreTierState

目标文件：D:\TRbackup\NLTX\src\WorldSession\OreTierState.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public readonly record struct OreTierState(
  int Copper,
  int Iron,
  int Silver,
  int Gold,
  int Cobalt,
  int Mythril,
  int Adamantite)
{
  public static OreTierState Uninitialized => new(
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1);
}
~~~

Uninitialized 是清理/兼容映射候选，不是对所有 Version4 生成路径默认值的确认。SavedOreTiers 的最终值仍必须由世界生成/加载边界提交。

### 5.6 SessionReadinessPhase

目标文件：D:\TRbackup\NLTX\src\WorldSession\SessionReadinessPhase.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public enum SessionReadinessPhase : byte
{
  AwaitingData,
  ProcessingData,
  Ready,
  Failed,
  Unloading,
}
~~~

Version4 直接证据只确认 AwaitingData、ProcessingData、Ready。Failed 和 Unloading 是 NLTX 生命周期候选扩展；它们必须由加载/生成/卸载整合边界确认后才能成为实现契约。当前代码草案没有加入 Generating，避免把现有枚举名称误写成 Version4 事实。

## 6. Component 代码草案

| componentId | Component | targetFile | designStatus | implementationStatus | currentNltxStatus |
|---|---|---|---|---|---|
| WS-COMP-01 | WorldDescriptorState | src/WorldSession/WorldDescriptorState.cs | proposed | created-draft | partial |
| WS-COMP-02 | WorldRulesState | src/WorldSession/WorldRulesState.cs | proposed | created-draft | partial |
| WS-COMP-03 | WorldClockState | src/WorldSession/WorldClockState.cs | proposed | created-draft | partial |
| WS-COMP-04 | WorldWeatherState | src/WorldSession/WorldWeatherState.cs | proposed | created-draft | partial |
| WS-COMP-05 | SessionReadinessState | src/WorldSession/SessionReadinessState.cs | proposed | created-draft | partial |
| WS-COMP-06 | WorldTickSnapshot | src/WorldSession/WorldTickSnapshot.cs | proposed | created-draft | partial |

### 6.1 WorldDescriptorState

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldDescriptorState.cs

~~~csharp
using System;

namespace Terraria.WorldSession.Components;

public sealed class WorldDescriptorState
{
  private const int SectionHeight = 150;
  private const int SectionWidth = 200;

  // 权威持久身份候选；不能替代 Entity ID、Network ID 或文件路径键。
  public int WorldId;

  // 权威 GUID 候选；与 WorldId 保持身份分离。
  public Guid UniqueId;

  // 权威显示元数据；空名规范化只允许发生在边界。
  public string Name = string.Empty;

  // 兼容/持久元数据；解析规则由加载边界负责。
  public string SeedText = string.Empty;
  public ulong WorldGeneratorVersion;

  // 权威世界几何元数据。
  public int SizeX;
  public int SizeY;
  public double LeftWorld;
  public double RightWorld;
  public double TopWorld;
  public double BottomWorld;

  // 权威世界锚点；坐标不是实体引用。
  public double SurfaceLayer;
  public double RockLayer;
  public int SpawnTileX;
  public int SpawnTileY;
  public int DungeonTileX;
  public int DungeonTileY;

  // 派生值；不能反向修改 SizeX、SizeY 或 Section 内容。
  public int SectionCountX => SizeX / SectionWidth;
  public int SectionCountY => SizeY / SectionHeight;
  public bool HasSurface => SurfaceLayer > 50.0;
  public WorldBounds Bounds => new(
    LeftWorld,
    TopWorld,
    RightWorld,
    BottomWorld);
}
~~~

实现约束：

- 该 Component 只附着于一个活动 WorldSession 根；不附着于 Player、NPC、Tile 或 Section entity。
- WorldId 与 UniqueId 必须分开映射；不可为了方便改成一个字符串身份。
- SectionCountX/SectionCountY 是派生值，不拥有区段内容，也不产生 WorldSectionId。
- 代码草案使用 Guid，因此实现文件需要显式 using System；若项目保持当前 ImplicitUsings 设置，需在编译验证中确认。

### 6.2 WorldRulesState

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldRulesState.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public sealed class WorldRulesState
{
  // 基础世界规则；不包含 Creative/Journey 的临时有效覆写。
  public WorldGameMode GameMode;

  // 规则事实快照候选；Hardmode 转换事务不由该 bool 表达。
  public bool HardMode;

  // 解析后的秘密种子标志；不隐藏解析算法。
  public WorldSecretSeedFlags SecretSeeds;

  // 基础世界邪恶类型；不得与 WorldGen 状态双向无主写入。
  public WorldEvilType WorldEvil;

  // 兼容/生成规则值；不拥有矿石 Tile 或区段内容。
  public OreTierState SavedOreTiers = OreTierState.Uninitialized;

  public bool IsJourneyMode => GameMode == WorldGameMode.Journey;
  public bool UsesDualDungeons =>
    (SecretSeeds & WorldSecretSeedFlags.DualDungeons) != 0;
  public bool IsSkyblockWorld =>
    (SecretSeeds & WorldSecretSeedFlags.Skyblock) != 0;
}
~~~

实现约束：

- 不把当前根 WorldRulesState 中的 DifficultyOverride、EffectiveDifficulty、InfectionSpreadAllowed、ShadowOrb* 或 AltarCount 静默加入本 Component；它们的 owner 属于相邻规则、生态或进度边界。
- HardMode=true 只表示已经提交的规则事实；它不能替代后台转换、I/O 锁、区段重置和主线程后续处理。
- SavedOreTiers 是值字段，不是矿石层实体或 Tile 状态。
- 不保存 LoadException、网络包、文件 reader 或外部 override 对象。

### 6.3 WorldClockState

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldClockState.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public sealed class WorldClockState
{
  // Version4 静态初始候选。
  public bool DayTime = true;
  public double Time = 13500.0;
  public int MoonPhase;

  // 整合快照元数据候选；不是 Terraria 原生时钟事实。
  public long ClockRevision;

  public bool HasValidMoonPhase => MoonPhase is >= 0 and <= 7;
}
~~~

实现约束：

- DayTime、Time 和 MoonPhase 由同一世界时钟 owner 成组读取和提交。
- MoonPhase 暂保留 int，以避免未经兼容验证就把 Version4 的字段变成新的枚举存储格式。
- ClockRevision 只用于区分快照新旧；若最终整合不需要 revision，应删除而不是让它成为第二套时钟。
- 时钟 Component 不保存客户端视觉时间、玩家计时器、事件实例、调度器或音乐状态。

### 6.4 WorldWeatherState

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldWeatherState.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public sealed class WorldWeatherState
{
  private const int EndlessRainThreshold = 5_184_000;

  // 权威雨事实候选。
  public bool IsRaining;
  public int RainTime;
  public float MaximumRainStrength;

  // 风目标是输入事实；当前值是模拟/缓存候选。
  public float WindSpeedTarget;
  public float WindSpeedCurrent;

  // 以下字段仅在确认失效条件和 owner 后保留为缓存。
  public int WeatherCounter;
  public int WindCounter;
  public int ExtremeWindCounter;
  public float PreviousMaximumRainStrength;

  public bool IsRainingForever => RainTime >= EndlessRainThreshold;
}
~~~

实现约束：

- IsRaining、RainTime、MaximumRainStrength 表达同一个雨状态，但其持久化和网络投影不等于 Component 本身。
- WindSpeedTarget 与 WindSpeedCurrent 不可合并；目标和插值结果有不同的写入原因。
- 四个计数/变化检测字段是缓存候选，不能被客户端表现层写入，也不能未经确认直接进入持久化协议。
- cloudAlpha、numClouds、numCloudsTemp、cloudBGActive、粒子、音乐和镜头不在本 Component 中。
- 代码草案不把雨强范围强制固定为 [0, 1]，因为上一份设计明确要求先补齐 Version4 override 全范围证据。

### 6.5 SessionReadinessState

目标文件：D:\TRbackup\NLTX\src\WorldSession\SessionReadinessState.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public sealed class SessionReadinessState
{
  // 权威生命周期事实候选。
  public SessionReadinessPhase Phase = SessionReadinessPhase.AwaitingData;

  // 只要屏障有效，就不能把世界交给完整实体更新。
  public bool GenerationBarrierActive;

  // 稳定失败码候选；不保存 Exception 引用或第三方异常文本。
  public int? FailureStatusCode;

  // 派生资格值；不能被外部组合层直接写入。
  public bool CanUpdateEntities =>
    Phase == SessionReadinessPhase.Ready &&
    !GenerationBarrierActive &&
    FailureStatusCode is null;
}
~~~

实现约束：

- 初始、加载/生成、失败和卸载阶段都必须阻断 CanUpdateEntities。
- Ready 不是“某一帧方法曾经执行”的别名；它必须表示相应世界头、生成/加载和必要屏障均已提交。
- Failed、Unloading 的实现转移仍待整合；当前代码草案不增加 Exception、Task、锁、文件流或网络连接字段。
- WorldGenerationLifecycleState.GenerationRevision 不自动迁移进本 Component；是否需要它属于 readiness owner 决策。

### 6.6 WorldTickSnapshot

WorldTickSnapshot 需要 5 个不可变值类型。值类型是快照的字段，不是额外 Component。

#### 6.6.1 WorldDescriptorSnapshotValue

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldDescriptorSnapshotValue.cs

~~~csharp
using System;

namespace Terraria.WorldSession.Components;

public readonly record struct WorldDescriptorSnapshotValue(
  int WorldId,
  Guid UniqueId,
  string Name,
  string SeedText,
  ulong WorldGeneratorVersion,
  int SizeX,
  int SizeY,
  WorldBounds Bounds,
  double SurfaceLayer,
  double RockLayer,
  int SpawnTileX,
  int SpawnTileY,
  int DungeonTileX,
  int DungeonTileY);
~~~

#### 6.6.2 WorldRulesSnapshotValue

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldRulesSnapshotValue.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public readonly record struct WorldRulesSnapshotValue(
  WorldGameMode GameMode,
  bool HardMode,
  WorldSecretSeedFlags SecretSeeds,
  WorldEvilType WorldEvil,
  OreTierState SavedOreTiers);
~~~

#### 6.6.3 WorldClockSnapshotValue

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldClockSnapshotValue.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public readonly record struct WorldClockSnapshotValue(
  bool DayTime,
  double Time,
  int MoonPhase,
  long ClockRevision);
~~~

#### 6.6.4 WorldWeatherSnapshotValue

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldWeatherSnapshotValue.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public readonly record struct WorldWeatherSnapshotValue(
  bool IsRaining,
  int RainTime,
  float MaximumRainStrength,
  float WindSpeedTarget,
  float WindSpeedCurrent);
~~~

天气计数器、云表现输入和粒子不进入该基础快照值。若最终协议需要它们，必须先确认它们是稳定跨域事实而不是表现缓存。

#### 6.6.5 SessionReadinessSnapshotValue

目标文件：D:\TRbackup\NLTX\src\WorldSession\SessionReadinessSnapshotValue.cs

~~~csharp
namespace Terraria.WorldSession.Components;

public readonly record struct SessionReadinessSnapshotValue(
  SessionReadinessPhase Phase,
  bool GenerationBarrierActive,
  int? FailureStatusCode)
{
  public bool CanUpdateEntities =>
    Phase == SessionReadinessPhase.Ready &&
    !GenerationBarrierActive &&
    FailureStatusCode is null;
}
~~~

#### 6.6.6 WorldTickSnapshot

目标文件：D:\TRbackup\NLTX\src\WorldSession\WorldTickSnapshot.cs

~~~csharp
using System;

namespace Terraria.WorldSession.Components;

public sealed class WorldTickSnapshot
{
  private WorldTickSnapshot(
    long revision,
    WorldDescriptorSnapshotValue? descriptor,
    WorldRulesSnapshotValue? rules,
    WorldClockSnapshotValue? clock,
    WorldWeatherSnapshotValue? weather,
    SessionReadinessSnapshotValue? readiness,
    bool isCommitted)
  {
    Revision = revision;
    Descriptor = descriptor;
    Rules = rules;
    Clock = clock;
    Weather = weather;
    Readiness = readiness;
    IsCommitted = isCommitted;
  }

  public static WorldTickSnapshot Uncommitted => new(
    0,
    null,
    null,
    null,
    null,
    null,
    false);

  public static WorldTickSnapshot CreateCommitted(
    long revision,
    WorldDescriptorSnapshotValue descriptor,
    WorldRulesSnapshotValue rules,
    WorldClockSnapshotValue clock,
    WorldWeatherSnapshotValue weather,
    SessionReadinessSnapshotValue readiness)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(revision);
    if (!readiness.CanUpdateEntities)
    {
      throw new ArgumentException(
        "A committed world snapshot requires an updateable session.",
        nameof(readiness));
    }

    return new WorldTickSnapshot(
      revision,
      descriptor,
      rules,
      clock,
      weather,
      readiness,
      true);
  }

  public long Revision { get; }
  public bool IsCommitted { get; }
  public WorldDescriptorSnapshotValue? Descriptor { get; }
  public WorldRulesSnapshotValue? Rules { get; }
  public WorldClockSnapshotValue? Clock { get; }
  public WorldWeatherSnapshotValue? Weather { get; }
  public SessionReadinessSnapshotValue? Readiness { get; }
}
~~~

快照约束：

- CreateCommitted 只接收已经从权威 Component 读取出的值，不读取全局状态、不执行 I/O，也不修改任何 Component。
- IsCommitted=true 时 5 个值必须来自同一提交点；提交点的原子范围仍由 BD-COMP-06 裁决。
- Uncommitted 不可供跨域消费者作为完整世界输入；失败或卸载时应丢弃/替换，不作为持久化恢复源。
- 快照不包含 Entity ID、Network ID、账户 ID、Section 位图、异常对象、文件流或可变集合。
- CreateCommitted 对 CanUpdateEntities 的要求是当前安全门控候选；若整合层最终允许“Ready 但暂不能更新实体”的阶段，必须同步修改不变量和 verifier。

## 7. WorldSession 根的组合草案

~~~text
活动 WorldSession 根
├── WorldDescriptorState       (required candidate)
├── WorldRulesState            (required candidate)
├── WorldClockState            (required candidate)
├── WorldWeatherState          (required candidate)
├── SessionReadinessState      (required candidate)
└── WorldTickSnapshot          (optional immutable candidate)
~~~

组合约束：

| 阶段 | 必须存在 | 快照条件 | 安全约束 |
|---|---|---|---|
| 等待数据 | SessionReadinessState | 不得有已提交快照 | CanUpdateEntities=false |
| 加载/生成中 | readiness 加载阶段；其他 Component 可部分填充 | 只能是 Uncommitted 或不存在 | 生成屏障有效时不得更新完整实体 |
| 已加载可用 | 五个核心 Component | 可有一份当前 IsCommitted=true 快照 | CanUpdateEntities=true |
| 失败/卸载 | SessionReadinessState | 已提交快照必须失效 | CanUpdateEntities=false |
| 客户端世界视图 | 无权威 WorldSession Component | 可持有服务端快照的只读副本 | 不得反向写入服务端权威状态 |

本表不定义 System 顺序、主循环、网络发送顺序或存档顺序。任何必须有先后的效果都要在后续 System/Adapter/Projection 文档中显式声明。

## 8. 当前 NLTX 到草案类型的映射

| 当前材料 | 草案目标 | 处理方式 | 当前状态 |
|---|---|---|---|
| src/WorldSession/WorldSessionComponents.cs:6-27 的 WorldDescriptorState | src/WorldSession/WorldDescriptorState.cs | 保留字段语义；移除 readiness 混入；统一命名空间 | partial；canonical 草案已创建，运行时接线未闭合 |
| src/WorldSession/WorldGeneration/WorldDescriptorState.cs:3-25 | 同一 WorldDescriptorState | 不能与根类型并存为两个 owner；先裁决 canonical path | partial；存在重复 |
| src/WorldSession/WorldSessionComponents.cs:38-56 的 WorldRulesState | src/WorldSession/WorldRulesState.cs | 保留基础规则候选；不自动吸收 difficulty、生态和进度字段 | partial；canonical 草案已创建，运行时接线未闭合 |
| src/WorldSession/WorldGeneration/WorldRulesState.cs:3-18 | 同一 WorldRulesState | GeneratorVersion、OreTiers 等不直接复制；按本草案字段名收敛 | partial；存在重复 |
| src/WorldSession/WorldSessionComponents.cs:61-68 的 WorldTimeWeatherState | WorldClockState + WorldWeatherState | 按时钟/天气事实拆分；事件和表现字段排除 | partial；目标类型已创建，legacy 聚合仍保留 |
| src/WorldSession/WorldGeneration/WorldPreparationState.cs:3-11 | SessionReadinessPhase | Version4 三值与 NLTX 扩展值需整合裁决 | partial；目标类型已创建，旧枚举仍保留 |
| src/WorldSession/WorldGeneration/WorldGenerationLifecycleState.cs:3-18 | SessionReadinessState 的部分字段 | Phase/失败/门控可映射；GenerationRevision 不自动加入 | partial；owner 未闭合 |
| dome/src/Terraria.Dome.Simulation/World/WorldClock.cs:5-180 | WorldClockState 的验证参考 | 只吸收“时钟为独立内聚状态”的结构经验，不复制类型/常量/行为 | partial；不是根项目实现 |
| dome/src/Terraria.Dome.Simulation/World/WorldRuleState.cs:5-237 | WorldRulesState/WorldWeatherState 的计算参考 | 不把 dome 的雨强/风限制当作 Version4 事实 | partial；不是根项目实现 |
| dome/src/Terraria.Dome.Simulation/World/WorldRuntimeSnapshot.cs:5-59 | WorldTickSnapshot 的不可变形状参考 | 不直接复用其字段范围或生成完成语义 | partial；不是目标类型 |
| dome/src/Terraria.Dome.Server/Startup/ServerHostState.cs:6-120 | SessionReadinessState 的服务器边界参考 | 不把 ServerHostState 当作 WorldSession Component | partial；不是目标 owner |

### 8.1 重复类型处理要求

本轮不删除或覆盖已有源码。实际实现时必须先记录：

1. 源路径：根 WorldSessionComponents.cs 与 WorldGeneration/ 下的同名类型；
2. 目标路径：本草案列出的 canonical flat path；
3. 依赖影响：命名空间引用、项目引用、测试引用、序列化/协议边界和文档链接；
4. 回滚方式：保留旧文件直到 focused verifier 和受影响项目验证完成，再进行明确的删除/迁移提交。

## 9. 不从现有聚合类型直接复制的字段

以下字段在当前 WorldSessionComponents.cs 中出现，但不进入本 6 Component 代码草案：

| 字段/类型 | 不复制原因 | 候选归属 |
|---|---|---|
| DifficultyOverride、EffectiveDifficulty | 基础 GameMode 与有效覆写的所有权不同 | SimulationRuleOverrides / integration-review |
| InfectionSpreadAllowed | 生态传播规则，不是基础 WorldSession 描述 | WorldGeneration/Ecology |
| ShadowOrbSmashed、ShadowOrbCount、AltarCount | 世界进度/交互事实，不是基础规则字段 | WorldProgression / WorldInteraction |
| BloodMoon、Eclipse、PumpkinMoon、SnowMoon | 日历/事件运行态跨域读写 | WorldCalendarAndEventOrchestration |
| SlimeRain* | 事件进度和刷怪/天气消费混合 | WorldCalendar / WorldProgression |
| BirthdayPartyState、LanternNightState、SandstormState | 独立事件/环境状态，当前 owner 未闭合 | 相邻事件/生态领域 |
| WorldEventProgressState | Boss、入侵、NPC 解锁和 Lunar 进度为跨域进度集合 | WorldProgression / integration-review |
| WorldSpawnPressureState | 实体统计和刷怪压力缓存，更新频率与 WorldSession 不同 | SpawnLifecycle |
| LoadException | 外部诊断对象，不是模拟共同修改的世界事实 | Persistence/diagnostics |
| WorldFile._temp* | 保存事务 staging，不是运行时长期权威状态 | Persistence adapter |
| cloudAlpha、numClouds、粒子、音乐 | 客户端表现或可重建缓存 | Presentation |

## 10. 代码边界与副作用登记

### 10.1 Component 文件允许的行为

- 保存字段；
- 提供只读派生属性；
- 对不可变快照值执行构造期一致性检查；
- 返回值对象的副本。

### 10.2 Component 文件禁止的行为

- 读取 DateTime.Now、随机 API、环境变量、文件、网络或数据库；
- 启动后台任务、定时器或线程；
- 直接发布消息、事件、日志、指标或 UI 效果；
- 直接写入 Main、WorldGen、WorldFile 或协议 buffer；
- 在 getter 中修改状态或重建缓存；
- 将快照写回权威 Component；
- 用目录顺序或文件枚举顺序表达运行时执行顺序。

### 10.3 后续实现边界的登记模板

后续实现 WorldSession System、加载 Adapter、保存 Projection 或网络 Projection 时，至少记录：

~~~text
读取：哪些 Component、哪个外部端口、是否读取时钟/随机源
写入：哪些 Component、消息、持久化 staging 或客户端视图
责任边界：谁负责提交、失败、取消、资源释放和重试
顺序：相对于 readiness、时钟、天气和快照提交的先后
失败：明确失败 / 结果未知 / 取消 / 重试 / 补偿
重复：是否允许重复提交、如何依据 Revision 去重
~~~

## 11. 未决实现门

| 门 | 必须解决的问题 | 影响文件 |
|---|---|---|
| CODE-GATE-01 | 根 WorldSessionComponents.cs 与 WorldGeneration/ 同名类型的 canonical owner | 全部 Component 文件 |
| CODE-GATE-02 | WorldGameMode 的数值映射与世界头兼容边界 | WorldGameMode.cs、WorldRulesState.cs |
| CODE-GATE-03 | 秘密种子解析算法是否由 Version4 当前代码、兼容 Adapter 或其他边界负责 | WorldSecretSeedFlags.cs、规则 Adapter |
| CODE-GATE-04 | HardMode 快照提交与后台转换事务的分界 | WorldRulesState.cs、相邻 progression 组件 |
| CODE-GATE-05 | SessionReadinessPhase 是否需要区分 Loading/Generating，及失败码 owner | SessionReadinessPhase.cs、SessionReadinessState.cs |
| CODE-GATE-06 | ClockRevision 是否保留、由谁递增、是否与所有五类快照值原子一致 | WorldClockState.cs、WorldTickSnapshot.cs |
| CODE-GATE-07 | Weather counter 是否进入缓存 Component、何时失效、是否持久化 | WorldWeatherState.cs |
| CODE-GATE-08 | WorldSession-only snapshot 与跨域 aggregate snapshot 的选择 | 所有 snapshot value 文件 |
| CODE-GATE-09 | Component 的 public field 是否符合最终 ECS attachment API，或需改成受控 mutation seam | 全部 Component 文件 |

这些门与上一份设计中的 GAP-COMP-01 至 GAP-COMP-11、BD-COMP-01 至 BD-COMP-06 对齐；代码草案不通过命名或构造函数偷偷替整合层做最终裁决。

## 12. 建议的 focused verifier 范围

本轮不创建 verifier，不运行构建或测试。实现阶段应至少覆盖下列行为：

### 12.1 值对象和派生值

- WorldDescriptorState.Bounds 只反映四个边界字段；修改尺寸不会通过派生属性反向修改边界；
- SectionCountX/SectionCountY 不产生 Section 内容或 ID；
- WorldRulesState 的 flags 派生属性只读且不改变 flags；
- WorldClockState.HasValidMoonPhase 对 0..7 和越界值返回正确结果；
- WorldWeatherState.IsRainingForever 只读取 RainTime；
- SessionReadinessState.CanUpdateEntities 在 Phase、屏障和失败码任一不安全时均为 false。

### 12.2 快照不变量

- 负 revision 被拒绝；
- 非 Ready、存在生成屏障或存在失败码时不能创建已提交快照；
- 已提交快照的 5 个值均非空；
- 快照属性没有 setter，修改源 Component 后已有快照值不改变；
- Uncommitted 不被误判为已提交；
- 快照不携带 Entity ID、Network ID、异常对象、文件流或可变集合。

### 12.3 迁移和装配

- 同名类型只保留一个 canonical namespace/type；
- 根项目和 dome 类型不发生隐式交叉引用；
- 世界头/网络 Projection 只能读取 Component 或快照，不能让协议对象成为 Component 字段；
- readiness 未提交时，完整实体更新资格保持关闭；
- 清理后旧 World 的身份、天气、时钟和快照不会被新 WorldSession 根复用。

## 13. 验证状态和交付结论

### 13.1 本轮已完成

- 已按本代码草案创建 6 个 Component、5 个快照值类型和支持值类型源码文件；
- 已按方案 A 保留非目标 legacy 类型，并移除根聚合文件中会与 canonical 类型冲突的定义；
- 已把 WorldTickSnapshot 实现为不可变快照候选，而不是第二套权威状态；
- 已记录现有根项目/dome 类型的映射和重复类型风险；
- 已完成 Terraria.WorldSession.csproj 的串行生产构建：0 个警告、0 个错误；
- 已保留 decision-required、partial 和 build-only 状态，没有把运行时接线写成完成事实。

### 13.2 本轮未完成

- 按用户要求未创建测试项目、未写测试；
- 未解决 canonical owner、Hardmode 事务、readiness 完整转移和跨域快照 owner；
- 未运行 dotnet test；
- 未宣称行为等价、协议兼容或运行时接线完成。

后续若进入代码实现，必须先处理 CODE-GATE-01、CODE-GATE-04、CODE-GATE-05 和 CODE-GATE-06，再按 D:\TRbackup\NLTX\AGENTS.md 的串行构建契约验证 Terraria.WorldSession.csproj。
