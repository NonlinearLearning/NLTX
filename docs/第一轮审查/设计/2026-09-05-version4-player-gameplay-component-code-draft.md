# Version4 PlayerGameplay Component 代码草案

## 1. 文档元数据

| 项目 | 值 |
|---|---|
| subsystemId | `PlayerGameplay` |
| 草案状态 | `draft` |
| 设计基线 | [2026-09-05-version4-player-gameplay-component-design.md](2026-09-05-version4-player-gameplay-component-design.md) |
| 研究基线 | [2026-09-05-version4-player-gameplay-public-decomposition.md](../research/2026-09-05-version4-player-gameplay-public-decomposition.md) |
| 候选源目录 | `D:\TRbackup\NLTX\src\Player` |
| 代码验证状态 | `not-run` |
| 当前实现声明 | 仅生成本文档中的代码草案；未创建或修改 `.cs` 文件 |

## 2. 草案范围

本文档把 Component-only Design 转换成面向 C# 实现的最小数据结构草案。每个代码块对应一个候选 `.cs` 文件和一个核心公共类型，代码块只保存组件状态，不实现行为。

本文档不定义或实现以下内容：

- System、Query、Command、Event、Adapter、Projection 或调度顺序；
- 输入包校验、伤害/死亡结算、物品转移、掉落、传送、坐骑移动或钓鱼 catch；
- 存档/网络 DTO、日志、时钟、随机数、UI 或持久化 I/O；
- 当前 `*State` 文件的迁移、删除、重命名或公共 API 改动；
- 编译、测试、行为等价或 Version4 迁移完成声明。

代码块中的 `TODO` 是实现前的类型或 owner 决策点，不是允许通过猜测补齐的实现细节。

## 3. 代码形状决策

### 3.1 组件类型

- `InputIntentComponent` 和 `PlayerLifecycleComponent` 保留当前 NLTX 已有的 `struct` 形状；扩展字段前必须审查现有调用方的值复制语义。
- 其余玩家状态组件使用 `sealed class`，与当前 `Player*State` 文件的可变状态形状一致，避免把包含数组和容器关系的状态意外复制。
- 组件不提供业务方法。只保留必要的固定容量常量、字段和只读的集合暴露形状；状态转换由后续 System 或显式边界负责。
- 一个代码块只声明一个同名核心公共类型，候选文件名与类型名相同。

### 3.2 共享类型和身份边界

- `EntityEcs.Components.EntityIdentityComponent` 是当前共享实体身份组件。下面的 `PlayerIdentityComponent` 草案不重复声明 `EntityUuid` 字段；设计文档中的 `entityId` 行保留为 `integration-review` 决策记录。
- `LocationComponent` 是既有统一位置组件，PlayerGameplay 不新增玩家位置组件。
- `ItemEntityRef`、`LegacyPlayerSlot`、`LegacyProjectileSlot`、`TileCoordinate`、`WorldPosition`、`SimulationTick`、`ContentId<TDefinition>`、`BuffSlot` 和相关玩家值类型来自当前 `src/Player/PlayerValueTypes.cs`，但其跨域 owner 仍需按设计文档裁决。
- `PersistentPlayerId`、`DirectionKind` 和 `PlayerContainerRef` 目前只是设计名；代码草案引用它们，但不在本文档中私自定义。
- `EntityUuid`、账户 UUID、网络/session slot、`PlayerHandle`、`LegacyPlayerSlot` 和持久化玩家身份不可互换。

## 4. 候选文件清单

| Component | 候选文件 | 当前 NLTX 对应状态 | 代码草案状态 |
|---|---|---|---|
| `PlayerIdentityComponent` | `src/Player/PlayerIdentityComponent.cs` | `PlayerIdentityState.cs` 为 `partial` | `proposed` |
| `InputIntentComponent` | `src/Player/InputIntentComponent.cs` | 已存在，字段不完整 | `partial-draft` |
| `PlayerLifecycleComponent` | `src/Player/PlayerLifecycleComponent.cs` | 已存在，字段不完整 | `partial-draft` |
| `PlayerVitalComponent` | `src/Player/PlayerVitalComponent.cs` | `PlayerVitalState.cs` 为 `partial` | `proposed` |
| `PlayerEquipmentComponent` | `src/Player/PlayerEquipmentComponent.cs` | `PlayerEquipmentState.cs` 为 `partial` | `proposed` |
| `PlayerInventoryComponent` | `src/Player/PlayerInventoryComponent.cs` | `PlayerInventoryState.cs` 为 `partial` | `proposed` |
| `PlayerUseComponent` | `src/Player/PlayerUseComponent.cs` | `PlayerItemUseState.cs` 为 `partial` | `proposed` |
| `PlayerAbilityComponent` | `src/Player/PlayerAbilityComponent.cs` | `PlayerSummonCapacityState.cs` 为 `partial` | `proposed` |
| `PlayerBuffComponent` | `src/Player/PlayerBuffComponent.cs` | `PlayerBuffState.cs` 为 `partial` | `proposed` |
| `PlayerRegenerationAndImmunityComponent` | `src/Player/PlayerRegenerationAndImmunityComponent.cs` | 对应 `State` 为 `partial` | `proposed` |
| `PlayerSpawnPointComponent` | `src/Player/PlayerSpawnPointComponent.cs` | 对应 `State` 为 `partial` | `proposed` |
| `PlayerMountComponent` | `src/Player/PlayerMountComponent.cs` | 对应 `State` 为 `partial` | `proposed` |
| `PlayerRestComponent` | `src/Player/PlayerRestComponent.cs` | 对应 `State` 为 `partial` | `proposed` |
| `PlayerFishingCapabilityComponent` | `src/Player/PlayerFishingCapabilityComponent.cs` | 对应 `State` 为 `partial` | `proposed` |

当前 `State` 文件不能与同名目标 Component 直接并存为两个独立的权威状态源。实际实现时应先完成 owner、调用方和迁移策略审查，再决定是改名、合并、让旧类型成为 projection，还是保留兼容包装。

## 5. Component 代码草案

### 5.1 `PlayerIdentityComponent.cs`

候选路径：`src/Player/PlayerIdentityComponent.cs`

说明：运行时 `EntityUuid` 由共享 `EntityIdentityComponent` 管理，因此本草案只保留玩家角色身份、持久化身份候选和兼容 slot。`PersistentPlayerId` 仍缺 Version4/PlayerFileData 的充分证据。

```csharp
namespace Terraria.Player;

public sealed class PlayerIdentityComponent
{
  // Runtime EntityUuid is owned by EntityEcs.Components.EntityIdentityComponent.
  // Do not add a second writable EntityUuid field here without resolving BD-COMP-04.
  public PersistentPlayerId? PersistentPlayerId { get; set; }

  public string CharacterName { get; set; } = string.Empty;

  public int TeamId { get; set; }

  public PlayerDifficulty Difficulty { get; set; }

  public bool IsHost { get; set; }

  // Compatibility projection only; it is not a persistence key or identity root.
  public LegacyPlayerSlot? LegacyPlayerSlot { get; set; }
}
```

### 5.2 `InputIntentComponent.cs`

候选路径：`src/Player/InputIntentComponent.cs`

说明：保留当前 `struct` 形状。`RequestedDirection` 的最终共享类型和 owner 未裁决；它不能写入 `LocationComponent`。

```csharp
namespace Terraria.Player;

public struct InputIntentComponent
{
  public bool MoveLeft;
  public bool MoveRight;
  public bool MoveUp;
  public bool MoveDown;
  public bool Jump;
  public bool UseItem;
  public bool UseTile;
  public bool ControlTorch;
  public bool ControlDash;
  public bool ControlDownHold;

  public bool ReleaseJump;
  public bool ReleaseUp;
  public bool ReleaseUseItem;
  public bool ReleaseUseTile;
  public bool ReleaseLeft;
  public bool ReleaseRight;
  public bool ReleaseDown;
  public bool ReleaseDash;

  public ItemUseMode AlternateUseMode;
  public DirectionKind RequestedDirection;
  public SimulationTick? IssuedAtTick;
  public uint Sequence;
  public InputIntentSource Source;
}
```

### 5.3 `PlayerLifecycleComponent.cs`

候选路径：`src/Player/PlayerLifecycleComponent.cs`

说明：`Phase` 是目标生命周期事实；`IsActive` 和 `IsDead` 只作为兼容/快照字段保留，不能与 `Phase` 形成三套可任意写入的权威状态。

```csharp
namespace Terraria.Player;

public struct PlayerLifecycleComponent
{
  public PlayerLifecyclePhase Phase;

  // Compatibility projections. Phase remains the authoritative lifecycle state.
  public bool IsActive;
  public bool IsDead;

  public int DeadElapsedTicks;
  public int RespawnRemainingTicks;
  public LegacyPlayerSlot? SpectatingTarget;

  public bool WasPvpDeath;
  public int PveDeathCount;
  public int PvpDeathCount;
  public WorldPosition LastDeathPosition;
  public DateTime LastDeathTime;
  public bool ShowLastDeath;
}
```

### 5.4 `PlayerVitalComponent.cs`

候选路径：`src/Player/PlayerVitalComponent.cs`

说明：保存资源和上下限，不实现伤害、治疗、死亡或防御结算。`Defense` 的最终 owner 与 `CombatAndStatus` 仍为未决关系。

```csharp
namespace Terraria.Player;

public sealed class PlayerVitalComponent
{
  public int Life { get; set; } = 100;

  public int BaseLifeMaximum { get; set; } = 100;

  public int EffectiveLifeMaximum { get; set; } = 100;

  public int Mana { get; set; }

  public int BaseManaMaximum { get; set; }

  public int EffectiveManaMaximum { get; set; }

  // Owner remains under BD-COMP-01 review with CombatAndStatus.
  public int Defense { get; set; }
}
```

### 5.5 `PlayerEquipmentComponent.cs`

候选路径：`src/Player/PlayerEquipmentComponent.cs`

说明：数组只保存玩家到 Item 实例的关系，不复制 Item 状态，也不在组件中计算装备效果。

```csharp
namespace Terraria.Player;

public sealed class PlayerEquipmentComponent
{
  public const int ArmorSlotCount = 20;
  public const int DyeSlotCount = 10;
  public const int MiscEquipmentSlotCount = 5;
  public const int MiscDyeSlotCount = 5;
  public const int HiddenAccessorySlotCount = 10;

  public ItemEntityRef[] ArmorSlots { get; } = new ItemEntityRef[ArmorSlotCount];

  public ItemEntityRef[] DyeSlots { get; } = new ItemEntityRef[DyeSlotCount];

  public ItemEntityRef[] MiscEquipmentSlots { get; } =
    new ItemEntityRef[MiscEquipmentSlotCount];

  public ItemEntityRef[] MiscDyeSlots { get; } = new ItemEntityRef[MiscDyeSlotCount];

  public bool[] HiddenAccessorySlots { get; } =
    new bool[HiddenAccessorySlotCount];

  public int CurrentLoadoutIndex { get; set; }
}
```

### 5.6 `PlayerInventoryComponent.cs`

候选路径：`src/Player/PlayerInventoryComponent.cs`

说明：容器关系与 Item 事务分离。`PlayerContainerRef` 是待裁决的容器关系类型，不能用裸数组下标作为持久化容器 ID。

```csharp
namespace Terraria.Player;

public sealed class PlayerInventoryComponent
{
  public const int MainInventorySlotCount = 59;

  public ItemEntityRef[] MainInventory { get; } =
    new ItemEntityRef[MainInventorySlotCount];

  public bool[] InventoryChestStackEligibility { get; } =
    new bool[MainInventorySlotCount];

  public PlayerContainerRef Bank { get; set; }

  public PlayerContainerRef Bank2 { get; set; }

  public PlayerContainerRef Bank3 { get; set; }

  public PlayerContainerRef Bank4 { get; set; }

  public VoidVaultState VoidVaultState { get; set; }

  public ItemEntityRef TrashItem { get; set; }

  public int SelectedSlotIndex { get; set; }

  public int LastHotbarSlotIndex { get; set; }

  public int? BufferedSelectedSlotIndex { get; set; }

  public int? OverriddenSelectedSlotIndex { get; set; }
}
```

### 5.7 `PlayerUseComponent.cs`

候选路径：`src/Player/PlayerUseComponent.cs`

说明：只保存玩家侧使用时序和结果快照。`HeldProjectile` 是兼容 slot，不是投射物实体身份或投射物列表。

```csharp
namespace Terraria.Player;

public sealed class PlayerUseComponent
{
  public int AnimationRemainingTicks { get; set; }

  public int AnimationDurationTicks { get; set; }

  public int UseRemainingTicks { get; set; }

  public int UseDurationTicks { get; set; }

  public int ToolTime { get; set; }

  public int ReuseDelayRemainingTicks { get; set; }

  public bool HasPendingReuse { get; set; }

  public bool IsChanneling { get; set; }

  public bool IsUseDelayed { get; set; }

  // Compatibility projection only; not a Projectile entity identity.
  public LegacyProjectileSlot? HeldProjectile { get; set; }

  public bool LastUseAttemptSucceeded { get; set; }
}
```

### 5.8 `PlayerAbilityComponent.cs`

候选路径：`src/Player/PlayerAbilityComponent.cs`

说明：保存玩家侧召唤/炮台容量及资格派生值，不保存每个召唤实体或投射物实体列表。

```csharp
namespace Terraria.Player;

public sealed class PlayerAbilityComponent
{
  public float MaximumMinionSlots { get; set; } = 1;

  public float UsedMinionSlots { get; set; }

  public int MinionCount { get; set; }

  public int MaximumTurrets { get; set; } = 1;

  public int PreviousMaximumTurrets { get; set; } = 1;

  public MinionFeatureFlags FeatureFlags { get; set; }
}
```

### 5.9 `PlayerBuffComponent.cs`

候选路径：`src/Player/PlayerBuffComponent.cs`

说明：使用 `BuffSlot` 聚合 Buff 类型和剩余时间，避免重新引入可错位的并行数组。Buff 效果规则和伤害免疫不属于本组件。

```csharp
namespace Terraria.Player;

public sealed class PlayerBuffComponent
{
  public const int MaximumSlotCount = 44;

  public BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];

  // Buff immunity is distinct from damage immunity timers.
  public HashSet<ContentId<BuffDefinition>> ImmuneBuffTypes { get; } = [];
}
```

### 5.10 `PlayerRegenerationAndImmunityComponent.cs`

候选路径：`src/Player/PlayerRegenerationAndImmunityComponent.cs`

说明：组件只保存恢复和免疫结果状态，不读取时钟、不生成随机数，也不直接修改生命或法力资源。

```csharp
namespace Terraria.Player;

public sealed class PlayerRegenerationAndImmunityComponent
{
  public int LifeRegenRate { get; set; }

  public int LifeRegenAccumulator { get; set; }

  public float LifeRegenElapsed { get; set; }

  public int ManaRegenRate { get; set; }

  public int ManaRegenAccumulator { get; set; }

  public float ManaRegenDelay { get; set; }

  public int ManaRegenRateBonus { get; set; }

  public float ManaRegenDelayBonus { get; set; }

  public bool HasManaRegenBuff { get; set; }

  public int GeneralImmunityRemainingTicks { get; set; }

  // TODO(PG): bind length to the resolved Version4 ImmunityCooldownID contract.
  public int[] CooldownImmunityRemainingTicks { get; } = [];

  public bool SuppressImmunityBlink { get; set; }
}
```

### 5.11 `PlayerSpawnPointComponent.cs`

候选路径：`src/Player/PlayerSpawnPointComponent.cs`

说明：个人出生点、当前实体位置和返回路线保持分离。坐标有效性和传送提交由外部空间/传送边界处理。

```csharp
namespace Terraria.Player;

public sealed class PlayerSpawnPointComponent
{
  public TileCoordinate? PersonalSpawnTile { get; set; }

  // Compatibility representation retained until the Version4 mapping is closed.
  public int SpawnX { get; set; } = -1;

  public int SpawnY { get; set; } = -1;

  public WorldPosition? ReturnOriginalUsePosition { get; set; }

  public WorldPosition? ReturnHomePosition { get; set; }
}
```

### 5.12 `PlayerMountComponent.cs`

候选路径：`src/Player/PlayerMountComponent.cs`

说明：只保存玩家侧坐骑关系和能力计时，不保存坐骑实体状态，不修改坐骑位置或车辆运动。

```csharp
namespace Terraria.Player;

public sealed class PlayerMountComponent
{
  public ContentId<MountDefinition>? MountType { get; set; }

  public bool IsActive { get; set; }

  public int FlightRemainingTicks { get; set; }

  public float Fatigue { get; set; }

  public float MaximumFatigue { get; set; }

  public int AbilityCharge { get; set; }

  public int AbilityCooldownRemainingTicks { get; set; }

  public int AbilityDurationRemainingTicks { get; set; }

  public bool IsDismountLocked { get; set; }

  public bool IsDismountRequested { get; set; }

  public bool IsMinecart { get; set; }
}
```

### 5.13 `PlayerRestComponent.cs`

候选路径：`src/Player/PlayerRestComponent.cs`

说明：`Mode` 是坐下/睡眠的唯一玩家侧模式；锚点、座椅能力和朝向都不拥有空间占用或位置事实。

```csharp
namespace Terraria.Player;

public sealed class PlayerRestComponent
{
  public PlayerRestMode Mode { get; set; }

  public TileCoordinate? AnchorTile { get; set; }

  // TODO(PG): replace with the resolved shared DirectionKind owner.
  public DirectionKind RequiredFacing { get; set; }

  public int StackIndex { get; set; }

  public int SleepElapsedTicks { get; set; }

  public RestSeatFeatures SeatFeatures { get; set; }
}
```

### 5.14 `PlayerFishingCapabilityComponent.cs`

候选路径：`src/Player/PlayerFishingCapabilityComponent.cs`

说明：保存由装备/Buff 推导出的玩家资格，不保存浮标实体、饵料关系或 catch 结果。

```csharp
namespace Terraria.Player;

public sealed class PlayerFishingCapabilityComponent
{
  public int BaseSkill { get; set; }

  public bool AllowsCrates { get; set; }

  public bool HasSonar { get; set; }

  public bool HasFishingLineProtection { get; set; }

  public bool HasBobberBonus { get; set; }

  public bool HasTackleBoxBonus { get; set; }

  public bool CanFishInLava { get; set; }

  public ContentId<ProjectileDefinition>? BobberOverrideType { get; set; }

  public int EffectiveFishingLevel { get; set; }
}
```

## 6. 尚未闭合的值类型和 owner

以下引用必须在实际 `.cs` 实现前解决；本草案不通过新增临时类型来掩盖证据缺口。

| 类型/边界 | 当前状态 | 实现前必须确认 |
|---|---|---|
| `EntityUuid` | 共享身份语义已写入 `CONTEXT.md`，现有 Player 目录没有最终类型 | 是否由现有 `EntityIdentityComponent` 及 Registry 统一提供；禁止在 `PlayerIdentityComponent` 重复可写 |
| `PersistentPlayerId` | `missing` | PlayerFileData、账户/存档边界和重连规则的真实 owner |
| `DirectionKind` | 设计名，现有 Player 值类型仍有 `Direction`，共享 `DirectionComponent` 仍是旧形状 | 统一枚举、零值和跨 Movement/PlayerRest 的 owner |
| `PlayerContainerRef` | 设计名，现有 Player 状态直接使用容器状态对象 | 容器实体/存档关系、作用域、失效结果和 ItemContainerAndEconomy owner |
| `CooldownImmunityID.Count` | Version4 语义存在，Player 代码草案未导出可复用固定长度 | 数组大小、槽位定义和 CombatAndStatus owner |
| `EntityReference` 与 `ItemEntityRef` | 两种关系值并存 | 是否保留玩家专用 Item 引用，或统一到带作用域的关系类型；不能隐式转换 |
| `PlayerHandle`、账户 UUID、网络/session slot | 仅有局部/外部模型证据 | Registry、WorldSession 和协议 Adapter 的单向投影关系 |

## 7. 字段与现有 `State` 类型的合并注意事项

### 7.1 不允许双写

在任何代码迁移前，不得同时让以下旧状态和目标 Component 成为可写权威源：

- `PlayerIdentityState` 与 `PlayerIdentityComponent`；
- `PlayerVitalState` 与 `PlayerVitalComponent`；
- `PlayerEquipmentState` 与 `PlayerEquipmentComponent`；
- `PlayerInventoryState` 与 `PlayerInventoryComponent`；
- `PlayerItemUseState` 与 `PlayerUseComponent`；
- `PlayerBuffState` 与 `PlayerBuffComponent`；
- 其余 `Player*State` 与同名目标 Component。

迁移前必须选择单一权威，并把旧类型明确标记为兼容包装、读取 projection、迁移输入或删除候选。

### 7.2 不能从草案推导的行为

下面的字段虽然出现在代码结构中，但草案不定义它们的更新方：

- `Life`、`Mana` 和 `Defense` 的伤害/治疗/消耗提交；
- `Slots`、装备槽和银行关系的转移、锁定、消费与掉落；
- `Phase`、`IsDead`、`RespawnRemainingTicks` 的死亡/重生事务；
- `MountType` 的附着/解除和坐骑运动；
- `EffectiveFishingLevel`、`FeatureFlags` 等派生值的重算时机；
- 输入 `Sequence` 的权限、去重、回退和 Tick 归属；
- 坐标、方向、实体引用和容器引用的解析失败处理。

## 8. 推荐的后续实现门禁

这不是实现计划，也不表示下列工作已完成；它是把代码草案转成真实源文件前的门禁清单。

1. 逐项裁决 `BD-COMP-01` 至 `BD-COMP-04`，尤其是生命/死亡、Item 容器和身份 Registry owner。
2. 为 `EntityUuid`、`PersistentPlayerId`、`DirectionKind`、`PlayerContainerRef` 和免疫槽位取得真实来源证据。
3. 盘点所有 `Player*State`、dome 组件和调用方，确定每个字段只有一个权威写入路径。
4. 先补状态转换和关系失效的 focused verifier，再迁移一个 Component 边界；不要以“能编译”替代行为验证。
5. 按 `架构设计/ECS文件组织设计约束.md` 和 `AGENTS.md` 记录源路径、目标路径及依赖影响。
6. 只在代码边界确定后创建 `.cs` 文件，并按仓库要求通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行验证受影响项目。

## 9. 未验证声明

- 本文档中的 C# 代码块是实现草案，不是已创建的源文件。
- `verificationStatus: not-run`；本次没有运行 `dotnet restore`、`build`、`test`、`run`、`publish`、`pack` 或 `msbuild`。
- 本文档不声明代码可编译、测试通过、行为等价、迁移完成或 owner 已最终裁决。
- 本次只新增本 Markdown 文档，不修改 `src/`、`dome/`、Version4、参考源码、测试或原研究报告。

## 10. 最终声明

本文件是 PlayerGameplay Component 的实际代码形状草案。它把 14 个 proposed Component 映射为候选 C# 文件和数据字段，同时保留研究报告中的 `decision-required`、`partial`、`integration-review` 与 `evidence-gap` 边界。

本文件不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、测试计划或迁移计划；所有代码块在 owner 和值类型证据闭合前都不得视为可直接合入的实现。
