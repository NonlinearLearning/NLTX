# DeathPenaltyAndRevenge Component Code Draft

## 1. 草案元数据

```text
subsystemId: DeathPenaltyAndRevenge
taskNumber: 02
sourceDesign: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-death-penalty-and-revenge-component-design.md
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-death-penalty-and-revenge-public-decomposition.md
outputPath: D:\TRbackup\NLTX\docs\design\2026-09-06-version4-death-penalty-and-revenge-component-code-draft.md
draftVariant: compile-oriented skeleton
draftStatus: implemented-from-approved-draft
designStatus: decision-required
implementationStatus: implemented
verificationStatus: build-passed
componentCount: 4
subagentUsed: false
```

本文件是经确认并已落地的 compile-oriented skeleton。它记录四个 Component 的 C# 实现
形状和实际源码位置；实现只覆盖组件状态和自身不变量，没有接入外部行为、网络或存档。

本草案遵循以下边界：

- 只草拟 `RevengeMarkerRegistryComponent`、`RevengeMarkerIdentityComponent`、
  `RevengeMarkerLifecycleComponent` 和 `RevengeTargetSnapshotComponent`；
- `RevengeMarkerId` 是支撑值类型，不计入 Component 数量；
- 不把玩家死亡、金币扣除、NPC 实体身份、网络包或存档字段重复塞入 marker Component；
- 不创建任何运行时行为结构；
- 不将 marker 直接绑定到 NPC entity、玩家 entity、连接或文件句柄；
- 对未决的 owner、持久化、网络接收和幂等语义只保留 TODO，不做猜测性实现。

## 2. 拟议文件布局

ECS 文件按能力域优先组织。本次已创建 `src/DeathPenaltyAndRevenge/` 项目并落地以下文件。

| 拟议路径 | 核心类型 | 状态 | 说明 |
|---|---|---|---|
| `src/DeathPenaltyAndRevenge/RevengeMarkerRegistryComponent.cs` | `RevengeMarkerRegistryComponent` | implemented | World entity 的 marker membership/index |
| `src/DeathPenaltyAndRevenge/RevengeMarkerId.cs` | `RevengeMarkerId` | implemented supporting value | marker 的 typed legacy `int` 身份；不是 Component |
| `src/DeathPenaltyAndRevenge/RevengeMarkerIdentityComponent.cs` | `RevengeMarkerIdentityComponent` | implemented | 单个 marker entity 的身份 |
| `src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs` | `RevengeMarkerLifecycleComponent` | implemented | 单个 marker entity 的过期和重建尝试锁 |
| `src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs` | `RevengeTargetSnapshotComponent` | implemented | 单个 marker entity 的 NPC target snapshot |
| `src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj` | 项目文件 | implemented | 引用 `Items` 和 `Npc` 项目 |

不建议把这些文件放入泛化的 `src/Components/` 目录，也不建议在尚未出现独立访问边界
前创建 `Components/` 子目录。四个 Component 数量小且属于同一能力域，先保持域目录扁平。

## 3. 编译假设与依赖边界

### 3.1 已落地项目引用

实际创建的独立项目使用以下最小项目引用：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Items\Terraria.Items.csproj" />
    <ProjectReference Include="..\Npc\Terraria.Npc.csproj" />
  </ItemGroup>
</Project>
```

项目引用已创建并经过目标项目编译验证。引用方向是
`DeathPenaltyAndRevenge -> Items/Npc`，没有反向修改现有 `Items` 或 `Npc` 项目。

### 3.2 类型来源

| 草案类型 | 暂用来源 | 当前处理 | 风险 |
|---|---|---|---|
| `NpcNetId` | `Terraria.Npc.NpcNetId` | 直接复用当前 NLTX 的 typed value | 它是 target snapshot 的兼容字段，不是 NPC 实例 ID |
| `NpcTypeId` | `Terraria.Npc.NpcTypeId` | 直接复用当前 NLTX 的 typed value | 不能用映射后的 net ID 取代原 type |
| `WorldPosition` | `Terraria.Items.WorldPosition` | 作为暂时的编译引用 | 当前 `Player` 域也有同名类型，最终 owner 为 `integration-review` |
| `RevengeMarkerId` | 本草案支撑值类型 | 新增 proposed typed wrapper | 跨 world、进程、网络和存档稳定性尚未裁决 |
| `LegacyAiStyle` | `int` | 保留 Version4 的兼容标量 | 当前没有充分证据证明需要新的枚举或 NPC 行为值类型 |

`NpcNetId` 的负值 variant 语义必须保留。`NpcInstanceId`、`NpcSlot`、`NetworkId`、
`PersistentEntityId` 和 `PlayerHandle` 均不能作为 `RevengeMarkerId` 的替代品。

### 3.3 代码状态边界

草案中的 `internal` 方法只用于保护组件自身状态不变量，不能被解释为完整的运行时行为
入口。它们不读写文件、网络、时钟、随机源、日志或外部服务。外部 game time 通过纯参数
传入，组件不拥有时钟。

## 4. 支撑值类型草案：RevengeMarkerId

### 4.1 拟议文件

`src/DeathPenaltyAndRevenge/RevengeMarkerId.cs`

### 4.2 C# 草案

```csharp
using System;

namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeMarkerId
{
  public const int UnassignedValue = -1;

  public RevengeMarkerId(int value)
  {
    if (value < UnassignedValue)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public int Value { get; }

  public static RevengeMarkerId Unassigned => new(UnassignedValue);

  public bool IsAssigned => Value >= 0;
}
```

### 4.3 语义约束

- `-1` 只表示尚未分配；观察到的 Version4 assigned ID 从 `0` 开始。
- `Value < -1` 在构造时拒绝。
- `default(RevengeMarkerId)` 的 CLR 默认值是 `0`，因此代码不得把 `default` 当作未分配
  sentinel；新建未分配身份必须显式使用 `RevengeMarkerId.Unassigned`。
- 该类型只包装 Version4 兼容 `int`，不宣称它是跨进程、跨重启或跨存档的稳定主键。
- 是否把它映射到网络 ID、持久化 ID 或 entity UUID，必须由 `crossSubsystemOwner:
  integration-review` 裁决。

## 5. Component 草案一：RevengeMarkerRegistryComponent

### 5.1 拟议文件与职责

`src/DeathPenaltyAndRevenge/RevengeMarkerRegistryComponent.cs`

该 Component 只放在一个 World entity 上，表达当前 world registry 中的 marker membership。
它不复制 marker 字段，也不保存 NPC、玩家、网络连接、锁对象或存档 writer。

```text
componentId: DPR-COMP-01
name: RevengeMarkerRegistryComponent
status: proposed
componentOwner: DeathPenaltyAndRevenge (candidate)
crossSubsystemOwner: integration-review
entityScope: one World entity / one active world registry
lifecycle: world initialization -> empty registry -> membership changes -> world reset cleanup
```

### 5.2 C# 草案

```csharp
using System;
using System.Collections.Generic;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerRegistryComponent
{
  private readonly HashSet<RevengeMarkerId> _markerIds;

  public RevengeMarkerRegistryComponent()
    : this(Array.Empty<RevengeMarkerId>())
  {
  }

  public RevengeMarkerRegistryComponent(IEnumerable<RevengeMarkerId> markerIds)
  {
    ArgumentNullException.ThrowIfNull(markerIds);

    _markerIds = new HashSet<RevengeMarkerId>();
    foreach (RevengeMarkerId markerId in markerIds)
    {
      if (!markerId.IsAssigned)
      {
        throw new ArgumentException(
          "A marker registry can contain only assigned marker IDs.",
          nameof(markerIds));
      }

      _markerIds.Add(markerId);
    }
  }

  public IReadOnlySet<RevengeMarkerId> MarkerIds => _markerIds;

  internal bool TryRegister(RevengeMarkerId markerId)
  {
    if (!markerId.IsAssigned)
    {
      return false;
    }

    return _markerIds.Add(markerId);
  }

  internal bool TryUnregister(RevengeMarkerId markerId)
  {
    return _markerIds.Remove(markerId);
  }

  internal void Clear()
  {
    _markerIds.Clear();
  }
}
```

### 5.3 字段和状态映射

| 草案成员 | Version4 来源 | 状态分类 | 说明 |
|---|---|---|---|
| `_markerIds` / `MarkerIds` | `_markers` | 权威 registry membership | 从对象列表收敛为 ID 集合；不复制 marker 对象 |
| `TryRegister` | `AddMarker` 的局部状态入口 | 受控写入 seam | 只接受 assigned ID；不负责创建 marker entity |
| `TryUnregister` | 成功重建和清理路径的移除 | 受控写入 seam | 不负责销毁 entity 或发布网络消息 |
| `Clear` | `Reset` | 受控清理 seam | 不重置外部 game time，也不恢复持久化数据 |

### 5.4 不变量与风险

- registry 只包含 assigned ID；同一 ID 在当前 registry 中最多出现一次。
- `MarkerIds` 不包含 marker 的 lifecycle 或 target snapshot；这些状态必须位于 marker entity。
- `HashSet<T>` 不是线程安全容器。Version4 的 `_markersLock` 不被复制进 Component；并发
  执行边界和同步策略属于 `BD-COMP-01`，不能在本草案中擅自决定。
- 当前 `IReadOnlySet<T>` 是只读 API 形状，但底层集合仍由组件拥有。若跨边界需要深度只读
  快照，后续必须选择冻结快照或明确租借期限。
- 当前证据没有证明 registry 需要按玩家或 section 建立反向索引；不得在此 Component
  预埋该索引。

## 6. Component 草案二：RevengeMarkerIdentityComponent

### 6.1 拟议文件与职责

`src/DeathPenaltyAndRevenge/RevengeMarkerIdentityComponent.cs`

该 Component 放在单个 marker entity 上，保存当前运行时和兼容消息所需的 marker 身份。
它不把 marker ID 当作 NPC 实例 ID、玩家 slot、持久化主键或网络连接 ID。

```text
componentId: DPR-COMP-02
name: RevengeMarkerIdentityComponent
status: proposed
componentOwner: DeathPenaltyAndRevenge (candidate)
crossSubsystemOwner: integration-review
entityScope: one marker entity
lifecycle: unassigned construction -> assigned registration -> active -> invalid after cleanup
```

### 6.2 C# 草案

```csharp
namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerIdentityComponent
{
  public RevengeMarkerIdentityComponent()
    : this(RevengeMarkerId.Unassigned)
  {
  }

  public RevengeMarkerIdentityComponent(RevengeMarkerId markerId)
  {
    MarkerId = markerId;
  }

  public RevengeMarkerId MarkerId { get; }

  public int LegacyId => MarkerId.Value;

  public bool IsAssigned => MarkerId.IsAssigned;
}
```

### 6.3 字段和状态映射

| 草案成员 | Version4 来源 | 状态分类 | 说明 |
|---|---|---|---|
| `MarkerId` | `_uniqueID` | 权威身份；兼容字段 typed form | `int` wire 表示通过 `LegacyId` 读取；不是新网络实体 ID |
| `LegacyId` | `WriteSelfTo` 写出的 ID | 派生兼容表示 | 不应作为第二份可写身份保存 |
| `IsAssigned` | `uniqueID == -1` 分支 | 派生值 | 仅表达是否已分配，不表达是否仍在 registry 中 |

### 6.4 不变量与风险

- 使用默认构造函数时身份为 `RevengeMarkerId.Unassigned`。
- Component 不提供修改 `MarkerId` 的方法；身份一旦作为 assigned component 创建，就只能在
  entity 组合层用新的完整 Component 替换，避免隐藏的二次写入。
- `MarkerId` 在 active registry 内必须唯一，但本 Component 自身无法验证跨 entity 唯一性；
  该不变量由 registry owner 维护。
- Version4 的静态 counter 只证明观察到的进程范围生成方式，不证明跨 world、重启、存档
  或服务器迁移稳定。
- `LegacyId` 可用于兼容旧协议，但不能直接复用 `NpcNetId`、`NpcInstanceId`、`NpcSlot`、
  `NetworkId` 或 `PersistentEntityId`。

## 7. Component 草案三：RevengeMarkerLifecycleComponent

### 7.1 拟议文件与职责

`src/DeathPenaltyAndRevenge/RevengeMarkerLifecycleComponent.cs`

该 Component 放在单个 marker entity 上，保存 marker 自身的过期边界、强制过期事实和
重建尝试锁。它不拥有 world clock，也不保存 NPC spawn 结果或经济事务结果。

```text
componentId: DPR-COMP-03
name: RevengeMarkerLifecycleComponent
status: proposed
componentOwner: DeathPenaltyAndRevenge (candidate)
crossSubsystemOwner: integration-review
entityScope: one marker entity
lifecycle: initialized -> time/qualification reads -> lock/forced expiry -> cleanup
```

### 7.2 C# 草案

```csharp
namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerLifecycleComponent
{
  public RevengeMarkerLifecycleComponent(int expiresAtGameTime)
  {
    ExpiresAtGameTime = expiresAtGameTime;
  }

  public int ExpiresAtGameTime { get; }

  public bool ForceExpire { get; private set; }

  public bool RespawnAttemptLocked { get; private set; }

  public bool IsExpiredAt(int currentGameTime)
  {
    return ForceExpire || currentGameTime >= ExpiresAtGameTime;
  }

  internal void MarkForceExpired()
  {
    ForceExpire = true;
  }

  internal void SetRespawnAttemptLocked(bool locked)
  {
    RespawnAttemptLocked = locked;
  }
}
```

### 7.3 字段和状态映射

| 草案成员 | Version4 来源 | 状态分类 | 说明 |
|---|---|---|---|
| `ExpiresAtGameTime` | `_expirationTime` | 权威生命周期状态 | 构造时依据外部 game time 和 `CoinValue` 计算；组件不拥有时钟 |
| `ForceExpire` | `_forceExpire` | 权威生命周期状态 | 一旦为 `true`，`IsExpiredAt` 不再受普通时间比较影响 |
| `RespawnAttemptLocked` | `_attemptedRespawn` | 权威生命周期状态；幂等 owner unresolved | 目前只保留 Version4 的 bool，不补造 token 或 revision |
| `IsExpiredAt` | `IsExpired` 的纯化读取 | 纯计算 | 调用方显式传入当前 tick，不读取全局时间 |

### 7.4 不变量与风险

- `ForceExpire == true` 是 marker 失效事实，不是可重试锁；本草案没有清除它的 API。
- `ExpiresAtGameTime` 没有独立的空值 sentinel；它必须在构造时得到明确的 legacy `int`。
- `IsExpiredAt` 不处理 tick 回绕、非法外部时间或持久化恢复；这些行为需要兼容策略裁决。
- `SetRespawnAttemptLocked` 只表达当前 bool，不等同于一次可追踪、可重放或幂等的事务。
- 失败恢复、并发竞争、owner handle、attempt token、revision 和离开 proximity 后的释放规则
  仍属于 `EG-COMP-02` / `BD-COMP-02`，本草案不扩展字段。

## 8. Component 草案四：RevengeTargetSnapshotComponent

### 8.1 拟议文件与职责

`src/DeathPenaltyAndRevenge/RevengeTargetSnapshotComponent.cs`

该 Component 放在单个 marker entity 上，保存 marker 创建时从 NPC 复制的不可变 target
snapshot。它不保存原 NPC entity 引用，也不拥有活 NPC 的生命、AI、slot 或普通生命周期。

```text
componentId: DPR-COMP-04
name: RevengeTargetSnapshotComponent
status: proposed
componentOwner: DeathPenaltyAndRevenge (candidate)
crossSubsystemOwner: integration-review
entityScope: one marker entity
lifecycle: captured from eligible NPC -> immutable while active -> read for recreation -> discarded
```

### 8.2 C# 草案

```csharp
using Terraria.Items;
using Terraria.Npc;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeTargetSnapshotComponent
{
  public RevengeTargetSnapshotComponent(
    WorldPosition location,
    NpcNetId npcNetId,
    NpcTypeId npcTypeId,
    int legacyAiStyle,
    float lifeFraction,
    int coinValue,
    float baseValue,
    bool spawnedFromStatue)
  {
    Location = location;
    NpcNetId = npcNetId;
    NpcTypeId = npcTypeId;
    LegacyAiStyle = legacyAiStyle;
    LifeFraction = lifeFraction;
    CoinValue = coinValue;
    BaseValue = baseValue;
    SpawnedFromStatue = spawnedFromStatue;
  }

  public WorldPosition Location { get; }

  public NpcNetId NpcNetId { get; }

  public NpcTypeId NpcTypeId { get; }

  public int LegacyAiStyle { get; }

  public float LifeFraction { get; }

  public int CoinValue { get; }

  public float BaseValue { get; }

  public bool SpawnedFromStatue { get; }
}
```

### 8.3 字段和状态映射

| 草案成员 | Version4 来源 | 状态分类 | 说明 |
|---|---|---|---|
| `Location` | `_location` | 不可变快照 | marker active 生命周期内不变；不是玩家或 NPC 的当前位置引用 |
| `NpcNetId` | `_npcNetID` | 兼容快照字段 | 供重建使用；不能解释为 `NpcInstanceId` 或新 NPC entity ID |
| `NpcTypeId` | `_npcTypeAgainstDiscouragement` | 快照字段 | 保留 discouragement 使用的原 type |
| `LegacyAiStyle` | `_npcAIStyleAgainstDiscouragement` | 快照字段 | 暂用 `int`，不复制完整 AI 状态 |
| `LifeFraction` | `_npcHPPercent` | 快照字段；值域 unresolved | 当前不主动添加 NaN、范围或至少 `0.5` 的新验证规则 |
| `CoinValue` | `_coinsValue` | 快照/兼容经济值 | 只表达 marker snapshot 中的 legacy 值，不拥有玩家金币事务 |
| `BaseValue` | `_baseValue` | 快照字段 | 重建时作为 NPC 输入读取 |
| `SpawnedFromStatue` | `_spawnedFromStatue` | 快照/兼容字段 | 重建时作为 NPC 输入读取 |

### 8.4 不变量与风险

- 所有属性均为 get-only；创建后不得原地改变 snapshot。
- `NpcNetId` 的负值 variant 语义必须保留；不得用 `NpcTypeId` 或 `NpcInstanceId` 替换它。
- snapshot 不持有 `EntityReference`、`NpcInstanceId`、`NpcSlot` 或 NPC 对象引用；marker 可以在
  原 NPC entity 已经被清理后继续存在。
- `LifeFraction` 的完整值域、NaN、无穷值和异常输入处理仍是 `EG-COMP-03`，本草案不猜测
  验证规则。
- `WorldPosition` 的共享类型归属仍是 `EG-COMP-04` / `BD-COMP-04`；本草案的 `using`
  只是临时编译形状，不是最终跨域 owner 决策。

## 9. Entity 组合草案

当前 NLTX 尚未提供本子系统使用的统一 Entity 容器 API，因此这里只表达组合关系，不伪造
注册方法或 entity handle 类型。

```text
World entity
└── RevengeMarkerRegistryComponent
    └── MarkerIds: set of RevengeMarkerId

Marker entity
├── RevengeMarkerIdentityComponent
│   └── MarkerId
├── RevengeMarkerLifecycleComponent
│   ├── ExpiresAtGameTime
│   ├── ForceExpire
│   └── RespawnAttemptLocked
└── RevengeTargetSnapshotComponent
    ├── Location
    ├── NpcNetId
    ├── NpcTypeId
    ├── LegacyAiStyle
    ├── LifeFraction
    ├── CoinValue
    ├── BaseValue
    └── SpawnedFromStatue
```

组合规则：

- World entity 只组合一个 active registry；不在每个 marker entity 上复制 `MarkerIds`。
- Marker entity 必须以 identity、lifecycle、target snapshot 三者表达完整的 active marker。
- registry 中的 `RevengeMarkerId` 只提供 membership/index；它不能单独构成可重建 marker。
- NPC entity 不附加上述三个 marker 组件；marker 是在 NPC 清理后仍可存在的独立状态。
- Player entity 不附加上述组件；当前证据没有确认玩家死亡金币链会直接创建 marker。

## 10. 组件间状态边界

| 状态 | 唯一候选 owner | 组件间关系 | 不允许的复制 |
|---|---|---|---|
| active marker membership | `RevengeMarkerRegistryComponent` | registry 引用 marker ID | 不把完整 marker snapshot 放进 registry |
| marker ID | `RevengeMarkerIdentityComponent` | registry 通过 ID 索引 marker | 不复制为 NPC slot、网络 ID 或持久化 ID |
| expiry / force expiry / attempt lock | `RevengeMarkerLifecycleComponent` | lifecycle 决定 marker 是否仍可结算 | 不把 world clock 或玩家 proximity 状态放入组件 |
| NPC recreation inputs | `RevengeTargetSnapshotComponent` | 重建边界读取不可变 snapshot | 不持有活 NPC 引用或完整 NPC 行为状态 |
| current game time | 外部 world time owner | 以参数输入生命周期纯读取 | 不在 marker Component 中复制 `_gameTime` |
| player coin result | Player/Economy candidate owner | 当前仅有相邻链证据 | 不用 `CoinValue` 反推玩家金币事务 |

## 11. 受控本地写入 seam

本草案仅保留组件内部需要维护自身不变量的最小写入 seam：

| seam | 目标状态 | 可见性 | 外部效果 | owner 状态 |
|---|---|---|---|---|
| `TryRegister` | registry membership add | `internal` | 无 | World registry owner unresolved |
| `TryUnregister` | registry membership remove | `internal` | 无 | World registry owner unresolved |
| `Clear` | registry cleanup | `internal` | 无 | World lifecycle owner unresolved |
| `MarkForceExpired` | force-expire latch | `internal` | 无 | marker lifecycle candidate |
| `SetRespawnAttemptLocked` | attempt bool | `internal` | 无 | attempt/idempotency owner unresolved |
| `IsExpiredAt` | expiry calculation | `public` pure read | 无 | 外部传入 game time |

这些 seam 不负责资格筛选、NPC 重建、玩家金币结算、网络发布、存档写入或日志。后续如需
实现这些动作，必须另行完成行为边界设计和 owner 裁决，不能把动作塞回四个 Component。

## 12. 兼容、网络和持久化边界

### 12.1 Version4 兼容字段

`RevengeTargetSnapshotComponent` 只保留 Version4 `WriteSelfTo` 可确认的九字段数据形状，
`RevengeMarkerIdentityComponent.LegacyId` 保留旧 `int` 表示。草案没有创建 writer、reader、
消息对象或客户端存储类型。

### 12.2 当前 dome 协议缺口

当前 `dome` 的同步 marker 占位只有 `short Id`，而 Version4 的 126 payload 实际包含九个
字段；因此本草案不把当前 packet 类型当作完整 Component 序列化契约。126/127 的字段布局、
客户端 marker store、重复消息去重、丢包恢复和版本协商均待单独裁决。

### 12.3 持久化缺口

当前 Version4 和 NLTX 证据没有证明 marker registry、marker ID、expiration 或 snapshot
需要跨 world save / process restart 恢复。本草案不添加 `PersistentEntityId`、存档版本、
恢复状态或 tombstone 字段。

## 13. 未决 TODO 清单

| TODO | 未决事实 | 影响 |
|---|---|---|
| `TODO-DPR-CODE-01` | World registry 是单一 World aggregate、按玩家索引还是双向索引 | 影响 `MarkerIds` 所有权和并发边界 |
| `TODO-DPR-CODE-02` | attempt lock 是否需要 token、owner handle、revision、失败恢复 | 影响 lifecycle Component 是否需要扩展字段 |
| `TODO-DPR-CODE-03` | 玩家死亡金币结果是否直接创建 marker | 影响是否增加跨域 transient context；当前不增加 |
| `TODO-DPR-CODE-04` | `WorldPosition` 和 marker/NPC ID 的最终共享类型 owner | 影响项目引用、namespace 和 wire conversion |
| `TODO-DPR-CODE-05` | marker 是否跨存档/重启存在，以及稳定身份如何恢复 | 影响 registry、identity 和 persistence schema |
| `TODO-DPR-CODE-06` | `LifeFraction` 的完整值域和异常输入策略 | 影响 snapshot 构造验证 |
| `TODO-DPR-CODE-07` | Version4 126/127 的客户端 receive/store 语义 | 影响兼容边界，但不应改变权威 Component 所有权 |
| `TODO-DPR-CODE-08` | `NPC.checkDead` 与 marker cache 是否存在直接调用关系 | 影响 NPC/marker 集成入口，不改变 Component 字段 |

上表对应已有的 `EG-COMP-01` 至 `EG-COMP-08` 和 `BD-COMP-01` 至 `BD-COMP-05`。代码草案
没有因为 TODO 尚未解决而添加猜测字段。

## 14. 后续集成顺序（源码已落地，行为接入未执行）

四个 Component 和支撑值类型已经按本文件落地。若后续获得 owner 决策，行为接入建议按以下顺序进行：

1. 先裁决 `RevengeMarkerId`、`WorldPosition`、NPC ID 和持久化 ID 的类型边界。
2. 在不改变现有组件 owner 的前提下，为 registry uniqueness、identity sentinel、force expiry、attempt lock 和 immutable
   snapshot 编写 focused verifier。
3. 再决定外部行为边界，逐项接入 NPC 清理、world tick、重建、网络和存档。
4. 每次接入只验证受影响项目；遵循仓库的 serial dotnet build/test 规则。

本次未执行行为接入和 focused verifier；按照用户要求没有创建测试文件。

## 15. 验证记录

```text
verificationStatus: build-passed
compile: passed
tests: not-written (user requested no tests)
sourceFilesCreated: true
projectFilesCreated: true
runtimeIntegration: not-started
buildProject: src/DeathPenaltyAndRevenge/Terraria.DeathPenaltyAndRevenge.csproj
buildExitCode: 0
buildWarnings: 0
buildErrors: 0
buildArtifact: Build/bin/Terraria.DeathPenaltyAndRevenge/Debug/net10.0/Terraria.DeathPenaltyAndRevenge.dll
```

本次实际编译命令通过仓库串行包装器执行：

```powershell
$dotnetArguments = @(
  'build',
  '.\\src\\DeathPenaltyAndRevenge\\Terraria.DeathPenaltyAndRevenge.csproj',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false')
& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 @dotnetArguments
```

构建结果为 exit code `0`、0 warning、0 error，DLL 已确认位于 `Build/bin/`。本次没有运行
测试，因为用户明确要求不写测试；未运行测试不等于测试通过。

## 16. 最终声明

本文件是 `DeathPenaltyAndRevenge` 的四 Component compile-oriented code draft，包含：

- `DPR-COMP-01 RevengeMarkerRegistryComponent`；
- `DPR-COMP-02 RevengeMarkerIdentityComponent`；
- `DPR-COMP-03 RevengeMarkerLifecycleComponent`；
- `DPR-COMP-04 RevengeTargetSnapshotComponent`。

四个 Component 均为 `status: proposed`。本文件不表示源码迁移完成、不表示行为等价、
不表示网络或存档兼容已完成，也不表示编译或测试已通过。组件 owner、跨子系统 ID、
respawn 幂等、玩家金币关系、持久化恢复和协议接收语义仍需 integration review。
