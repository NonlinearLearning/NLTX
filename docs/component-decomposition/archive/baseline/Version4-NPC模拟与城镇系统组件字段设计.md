# Version4 NPC 模拟与城镇系统组件字段设计

本设计以《[Version4-NPC模拟与城镇系统组件拆分报告](Version4-NPC模拟与城镇系统组件拆分报告.md)》为唯一
边界输入，只定义有状态模块的字段、支持值对象和从本对象字段重复计算的只读属性。它不定义
`System`、`Query`、`Command`、Adapter、Projection、持久化格式、网络包、业务算法或测试。

建议共享领域命名空间为 `Terraria.Npc` 和 `Terraria.Town`；仅服务端复制状态和对话会话可位于
`Terraria.Server.Npc`、`Terraria.Server.Town`。一个核心公开类型对应一个同名 PascalCase 文件。

## 1. 字段规则

- **权威**字段是该概念在运行期唯一可写来源。只能由其所属的专责 System 或 Command 修改。
- **兼容**字段只为从 Version4 导入、槽位查找或旧协议投影服务，绝不作为新的实体、网络或持久化 ID。
- **可重建**字段是索引或缓存，丢失后由权威字段重新建立，不单独保存。
- **派生**属性只能读取同一对象中的字段，不能缓存为第二份真相。
- ECS 实体携带的组件不包含 `active`：组件存在于 `NpcSlotStore` 的已占用槽位即代表活跃。
  `NPC.active` 由兼容 Adapter 在导入/投影时转换，不能在新模型中重新保存。
- `NPC.position`、`velocity`、尺寸、碰撞、生命、伤害、防御、Buff、掉落和表现帧已有其他领域所有者，
  因而不是本设计的 NPC/城镇组件字段。
- `NPC.ai` 的四槽语义因 AI style 而异；只有保留为兼容载荷才允许使用数组。它不能拆成四个
  “AI0/AI1/AI2/AI3 组件”，也不能与 `localAI` 共用一份可网络同步状态。

所有 `EntityId`、`PlayerEntityId`、`ConnectionId` 和 `WorldTick` 是目标运行时已有的强类型 ID/时钟
占位名；本文件不重定义其底层表示。`NpcInstanceId`、`NpcSlot`、`NpcTypeId`、`NpcNetworkId` 和
`TownRoomTilePoint` 的值对象定义在第 2 节。

## 2. 标识和值对象

这些是组件字段的类型，不是可独立附着的组件。它们把实例身份、容量槽位、内容类型、网络映射和
世界 Tile 坐标分开，避免 Version4 的裸 `int` 交叉污染。

```csharp
namespace Terraria.Npc;

public readonly record struct NpcInstanceId(ulong Value)
{
  public bool IsValid => Value != 0;
}

// 仅兼容 Version4 whoAmI / Main.npc[] 容量槽。可在实体清理后复用。
public readonly record struct NpcSlot(int Value)
{
  public bool IsAssigned => Value >= 0;
}

// 内容目录主键；多个活跃实例可以共享同一 TypeId。
public readonly record struct NpcTypeId(int Value)
{
  public bool IsValid => Value > 0;
}

// 兼容 netID，保留负值变体语义；不是连接范围内的网络实体 ID。
public readonly record struct NpcNetId(int Value)
{
  public bool IsVariant => Value < 0;
}

// 仅由复制 Adapter 使用，不能写入玩法逻辑或持久化住房关系。
public readonly record struct NpcNetworkId(uint Value)
{
  public bool IsAssigned => Value != 0;
}

public readonly record struct TownRoomTilePoint(int X, int Y);

public enum NpcTargetKind : byte
{
  None,
  Player,
  Npc,
  PlayerTankPet
}

public enum NpcDespawnReason : byte
{
  None,
  TimeExpired,
  PopulationPressure,
  WorldUnload,
  DefinitionInvalid,
  ParentDestroyed,
  CompatibilityRemoval
}

public enum TownHousingStatus : byte
{
  Homeless,
  Assigned,
  LookingForHome,
  EvictionPending
}

public enum NpcDialogueSessionPhase : byte
{
  Closed,
  Open,
  AwaitingServiceResult,
  Invalidated
}
```

`NpcTypeId` 与 `NpcNetId` 必须同时保留：离线 API 文档说明部分负值 `NPCID` 变体在实例化后共享
`NPC.type`，需要 `netID` 才能区分。`NpcSlot` 和 `NpcNetworkId` 同样不能替代 `NpcInstanceId`。

## 3. NPC 实体组件

以下九个组件可随 NPC ECS 实体组合附着。没有一个组件同时承担刷新、行为、战斗、住房、网络和
表现职责。

### 3.1 `NpcEntityIdentityComponent`

`NPC.active`、`whoAmI` 和槽位创建路径的拆分结果。实体本身是活跃性的唯一表达，槽位只提供
容量与旧协议兼容。

```csharp
namespace Terraria.Npc;

public sealed class NpcEntityIdentityComponent
{
  // 权威：本次实体生命周期内稳定，不能因槽位复用而改变。
  public NpcInstanceId InstanceId;

  // 兼容：由 NpcSlotStore 分配和释放；不可用作持久化、网络或父子关系 ID。
  public NpcSlot LegacySlot;

  // 派生：实体创建成功且尚未开始清理时为 true。
  public bool HasAssignedSlot => LegacySlot.IsAssigned;
}
```

不保存 `bool Active`、`int WhoAmI` 的第二副本、玩家索引或连接 ID。`NpcSlotStore` 是槽位占用的
权威根，`LegacySlot` 仅记录该实体已经由该根分配的结果。

### 3.2 `NpcDefinitionReferenceComponent`

将实例引用与只读 `NpcDefinitionCatalog` 分开。默认生命、防御、碰撞尺寸、AI 类别和居民能力应由
定义读取，在实例初始化时分别交给战斗、物理和城镇领域；本组件不复制那些默认数值。

```csharp
namespace Terraria.Npc;

public sealed class NpcDefinitionReferenceComponent
{
  // 权威：内容目录主键。
  public NpcTypeId TypeId;

  // 权威：保留 Version4 的负值变体/网络类型语义。
  public NpcNetId NetId;

  // 兼容：仅导入期间记录旧 SetDefaults 使用的初始类型；完成导入后应与 TypeId 一致。
  public NpcTypeId InitialTypeId;

  // 派生：避免将 “type == netID” 误当作永久不变量。
  public bool UsesNetIdVariant => NetId.IsVariant;
  public bool IsInitialized => TypeId.IsValid && InitialTypeId.IsValid;
}
```

### 3.3 `NpcBehaviorStateComponent`

`aiStyle`、`aiAction`、`ai[4]` 的服务器权威部分。行为枚举、阶段和命名计时字段只能在取得某一
行为族读写证据后新增；当前报告没有足够证据将所有具体 AI 规则字段化。

```csharp
namespace Terraria.Npc;

public sealed class NpcBehaviorStateComponent
{
  // 权威：来自定义的行为族/AI style 路由标识。
  public int BehaviorKind;

  // 权威：兼容 aiStyle，保留原参考代码调度所需的数值。
  public int LegacyAiStyle;

  // 权威：兼容 aiAction；只有行为系统可以修改。
  public int Action;

  // 兼容权威：原 NPC.ai 的四个网络同步槽，长度固定为 4。
  // 不允许替换数组实例，也不允许由表现层写入。
  public float[] AuthoritativeAiSlots = new float[4];

  // 权威：行为状态最近一次被行为系统推进的 tick。
  public WorldTick LastUpdatedTick;

  // 派生：不存储 “是否在某阶段”；阶段由已确认行为族从槽位/Action 解释。
  public bool HasAuthoritativeSlots => AuthoritativeAiSlots.Length == 4;
}
```

`localAI[4]` 不进入该共享权威组件。需要它的客户端表现可拥有独立、不可持久化且不可复制的
`NpcBehaviorLocalCache`，其四个槽不参与服务端模拟、住房、交易或存档。

### 3.4 `NpcTargetComponent`

`NPC.target` 的整数编码可能指向玩家、NPC 或坦克宠物；因此不能照搬为 `int Target`。选择结果和
目标观察快照分开，后者用于行为计算，不是跨 Tick 的第二份空间真相。

```csharp
namespace Terraria.Npc;

public sealed class NpcTargetComponent
{
  // 权威：当前选择的运行时实体；None 时 TargetEntity 必须为 null。
  public NpcTargetKind Kind;
  public EntityReference? TargetEntity;

  // 权威：本次选择提交的 tick，用于重选节流和失效判断。
  public WorldTick SelectedAtTick;

  // 可重建：选择时的目标空间快照，不持久化且不能替代 Transform/Physics。
  public NpcTargetSnapshot LastObservedTarget;

  // 派生：禁止单独保存 HasTarget。
  public bool HasTarget => Kind != NpcTargetKind.None && TargetEntity.HasValue;
}

public struct NpcTargetSnapshot
{
  public bool IsValid;
  public int Width;
  public int Height;
  public Vector2 Position;
  public Vector2 Velocity;

  // 派生：不保存中心点或 Hitbox 的副本。
  public Vector2 Center => Position + new Vector2(Width, Height) * 0.5f;
}
```

目标关系使用现有 `Terraria.Relationships.EntityReference`，以保存目标所属范围（玩家、NPC 等）；
它不能退化为 `NpcInstanceId` 或网络 ID。`targetRect` 不作为独立权威字段。其几何结果应由空间查询从目标的 `LocationComponent` 和
`ColliderComponent` 获取；`LastObservedTarget` 仅表达本次选择输入的短寿命快照。

### 3.5 `NpcLifetimeComponent`

`timeLeft`、`npcSlots`、`dontCountMe` 和离场鼓励状态的领域边界。战斗死亡由
`CombatAndStatus` 发出事件，本组件只消费已决的离场原因。

```csharp
namespace Terraria.Npc;

public sealed class NpcLifetimeComponent
{
  // 权威：兼容 NPC.timeLeft；小于等于零时由生命周期系统请求清理。
  public int RemainingTicks;

  // 权威：兼容 npcSlots，用于人口压力计算。
  public float PopulationSlotCost = 1f;

  // 权威：兼容 dontCountMe。false 才纳入自然刷怪人口压力。
  public bool CountsAgainstPopulation = true;

  // 权威：可由人口/距离资格系统提交，但不是每帧缓存结果。
  public bool DespawnEncouraged;

  // 权威：一旦开始清理即固定；None 表示尚未进入清理流程。
  public NpcDespawnReason PendingDespawnReason;

  // 派生：不另存 IsExpired 或 IsPendingDespawn。
  public bool IsExpired => RemainingTicks <= 0;
  public bool IsPendingDespawn => PendingDespawnReason != NpcDespawnReason.None;
  public float EffectivePopulationCost => CountsAgainstPopulation ? PopulationSlotCost : 0f;
}
```

### 3.6 `NpcParentRelationComponent`

`realLife` 是多段 Boss/子实体关系，不能当作同一类型 NPC 的槽位、网络 ID 或持久化 ID。仅实际具有
该关系的实例附着本组件。

```csharp
namespace Terraria.Npc;

public sealed class NpcParentRelationComponent
{
  // 权威：父 NPC 的稳定实体身份；不存在父实体时本组件不附着。
  public NpcInstanceId ParentInstanceId;

  // 兼容：仅供旧 realLife 槽位投影查找，必须与 ParentInstanceId 由同一关系提交。
  public NpcSlot ParentLegacySlot;

  // 权威：关系建立时的 tick，供父实体复用/失效检查。
  public WorldTick AttachedAtTick;

  // 派生：不保存 “parent active”。
  public bool HasLegacyParentSlot => ParentLegacySlot.IsAssigned;
}
```

### 3.7 `NpcReplicationDirtyState`

来自 `netUpdate`、`netAlways`、`spawnNeedsSyncing`、`netStream` 和每玩家同步跳过计数。该组件属于
服务器复制调度，不得被 AI、住房、服务或客户端 UI 写入。

```csharp
namespace Terraria.Server.Npc;

[Flags]
public enum NpcReplicationFlags : byte
{
  None = 0,
  StateDirty = 1 << 0,
  SpawnNeedsSync = 1 << 1,
  ForceFullSync = 1 << 2,
  RemovalNeedsSync = 1 << 3
}

public sealed class NpcReplicationDirtyState
{
  // 权威：复制 Adapter 唯一写者。
  public NpcReplicationFlags Flags;
  public bool AlwaysRelevant;
  public int StreamCursor;

  // 可重建：每连接跳过和确认状态；连接断开必须移除对应项。
  public Dictionary<ConnectionId, NpcClientReplicationState> ClientStates = new();

  // 派生：不单独保存 netUpdate / spawnNeedsSyncing bool 副本。
  public bool IsDirty => (Flags & NpcReplicationFlags.StateDirty) != 0;
  public bool RequiresSpawnSync => (Flags & NpcReplicationFlags.SpawnNeedsSync) != 0;
}

public struct NpcClientReplicationState
{
  public byte SkippedSyncCount;
  public uint LastAcknowledgedRevision;
}
```

### 3.8 `TownResidentComponent`

`townNPC`、`friendly` 和 `housingCategory` 的居民能力边界。它不是一般敌怪都必须附着的“NPC 社交组件”。

```csharp
namespace Terraria.Town;

[Flags]
public enum TownResidentCapabilities : byte
{
  None = 0,
  CanUseHousing = 1 << 0,
  CanOpenDialogue = 1 << 1,
  CanOfferCommerce = 1 << 2,
  CanParticipateInHappiness = 1 << 3
}

public sealed class TownResidentComponent
{
  // 权威：兼容 townNPC；组件存在不等价于所有能力均开启。
  public bool IsTownResident = true;

  // 权威：兼容 friendly，供战斗领域通过只读接口查询。
  public bool IsFriendly = true;

  // 权威：兼容 housingCategory；住房共居资格读取此字段。
  public int HousingCategory;

  // 权威：由定义初始化的居民能力组合。
  public TownResidentCapabilities Capabilities;

  // 派生：不保存重复的 “可住房/可聊天” bool。
  public bool CanUseHousing => (Capabilities & TownResidentCapabilities.CanUseHousing) != 0;
  public bool CanOpenDialogue => (Capabilities & TownResidentCapabilities.CanOpenDialogue) != 0;
}
```

### 3.9 `NpcHousingAssignmentComponent`

它是一个居民实例对世界 `TownHousingStore` 关系的镜像，不能独自成为住房持久化真相。分配/逐出命令
必须同时更新本组件和 Store；`oldHomeless`、`oldHomeTileX/Y` 仅属于旧网络差分实现，不迁移。

```csharp
namespace Terraria.Town;

public sealed class NpcHousingAssignmentComponent
{
  // 权威：当前居民的分配阶段。Homeless 与 HomeTile 必须保持一致。
  public TownHousingStatus Status = TownHousingStatus.Homeless;

  // 权威：Assigned 或 EvictionPending 时的目标房间 Tile；否则为 null。
  public TownRoomTilePoint? HomeTile;

  // 权威：兼容 lookForHomeTimeout；只由住房重评估系统递减。
  public int SearchCooldownTicks;

  // 权威：兼容 homelessDespawn 的显式策略，而非从 Status 隐式猜测。
  public bool DespawnWhenHomeless;

  // 权威：分配命令单调递增，用来丢弃过时扫描结果和投影。
  public uint AssignmentRevision;

  // 派生：不另存 Homeless 或 HouseholdStatus。
  public bool IsHomeless => Status == TownHousingStatus.Homeless;
  public bool HasHome => HomeTile.HasValue && Status != TownHousingStatus.Homeless;
  public bool IsEligibleForSearch =>
    Status is TownHousingStatus.Homeless or TownHousingStatus.LookingForHome &&
    SearchCooldownTicks == 0;
}
```

## 4. 世界会话与存储状态根

下列状态不是单个 NPC 的组件，必须附着于世界会话根或作为该根拥有的 Store。它们之所以在本文件中
定义，是因为报告把它们列为 NPC/城镇子系统的唯一权威状态，而不是任何 System 的隐式静态字段。

### 4.1 `NpcSlotStore`

`Main.npc[]` 和 `GetAvailableNPCSlot` 的容量职责。`Occupants` 是槽位占用唯一真相；
`NpcEntityIdentityComponent.LegacySlot` 是由它分配后的镜像引用。

```csharp
namespace Terraria.Npc;

public sealed class NpcSlotStore
{
  // 权威：位置即 LegacySlot；null 表示空闲槽。
  public NpcInstanceId?[] Occupants = Array.Empty<NpcInstanceId?>();

  // 可重建：从 Occupants 重建，用于快速空槽扫描。
  public BitArray OccupiedSlots = new(0);

  // 运行期权威：下次空槽搜索起点，不影响哪个实例存在。
  public int NextSearchStart;

  // 派生：都不持久化为独立计数。
  public int Capacity => Occupants.Length;
  public int OccupiedCount => OccupiedSlots.Cast<bool>().Count(value => value);
  public int AvailableCount => Capacity - OccupiedCount;
  public bool IsFull => AvailableCount == 0;
}
```

### 4.2 `NpcPopulationPressureState`

自然刷新周期的世界级状态。每位玩家的位置、Biome、活跃城镇 NPC 数、Tile、天气、事件和运气是
`NpcSpawnContextSnapshot` 输入，不复制到此会话状态。

```csharp
namespace Terraria.Npc;

public sealed class NpcPopulationPressureState
{
  // 权威：兼容 noSpawnCycle；只决定全局刷新周期是否暂停。
  public bool IsSpawnCycleSuppressed;

  // 权威：兼容 defaultSpawnRate、defaultMaxSpawns、activeTime。
  public int DefaultSpawnRateTicks = 600;
  public int DefaultMaxSpawns = 5;
  public int DefaultActiveTimeTicks = 750;

  // 权威：本次世界刷新周期序号，供确定性随机数和诊断关联。
  public uint SpawnCycle;

  // 权威：最近完成的世界刷新评估 tick。
  public WorldTick LastEvaluatedTick;

  // 派生：不保存当前活跃 NPC 数或当前总 slot 成本；它们从 NpcSlotStore/Lifetime Query 获得。
  public bool CanAttemptNaturalSpawn => !IsSpawnCycleSuppressed && DefaultSpawnRateTicks > 0;
  public int EffectiveDefaultMaxSpawns => Math.Max(DefaultMaxSpawns, 0);
}
```

### 4.3 `TownHousingStore`

兼容 `TownRoomManager._roomLocationPairs` 和 `_hasRoom`。导入期按 `NpcTypeId` 键保持 Version4 存档
语义；长期多实例模型转为实例关系前，不能假设同类型居民可以安全同时拥有不同房间。

```csharp
namespace Terraria.Town;

public sealed class TownHousingStore
{
  // 权威：兼容存档关系，NPC 类型到房间锚点。
  public Dictionary<NpcTypeId, TownRoomTilePoint> RoomByResidentType = new();

  // 可重建：按房间反查的占用类型集合。
  public Dictionary<TownRoomTilePoint, HashSet<NpcTypeId>> ResidentTypesByRoom = new();

  // 可重建：兼容 _hasRoom 的快速索引；由 RoomByResidentType 重建。
  public HashSet<NpcTypeId> ResidentTypesWithRooms = new();

  // 权威：每次分配/逐出改变；持久化投影和扫描结果据此判过期。
  public uint Revision;

  // 派生：不保存房间数量或 “任意有房居民”。
  public int AssignmentCount => RoomByResidentType.Count;
  public bool HasAnyAssignments => AssignmentCount != 0;
}
```

## 5. 内容定义与交互会话状态

定义目录只在内容加载或热重载时写入，游戏 Tick 只读；它们不能成为每个 NPC 实例的可变字段。
对话会话则是连接/玩家范围状态，不能放进 NPC 实体组件。

### 5.1 `NpcDefinitionCatalog` 和 `NpcDefinition`

```csharp
namespace Terraria.Npc;

public sealed class NpcDefinitionCatalog
{
  // 权威：内容定义主表；加载器或热重载 Adapter 是唯一写者。
  public Dictionary<NpcTypeId, NpcDefinition> ByType = new();

  // 权威：用于拒绝定义与实例不一致的请求/导入数据。
  public uint Revision;

  // 派生：不保存 Count 的副本。
  public int Count => ByType.Count;
}

public sealed class NpcDefinition
{
  // 权威：内容身份与负值变体匹配信息。
  public NpcTypeId TypeId;
  public NpcNetId DefaultNetId;

  // 权威：行为初始化资料；具体行为算法不在定义对象中实现。
  public int BehaviorKind;
  public int LegacyAiStyle;
  public float[] InitialAuthoritativeAiSlots = new float[4];

  // 权威：生命周期和人口默认值。
  public int DefaultLifetimeTicks;
  public float PopulationSlotCost = 1f;
  public bool CountsAgainstPopulation = true;

  // 权威：城镇能力的定义值；实例创建时复制到 TownResidentComponent。
  public bool IsTownResident;
  public bool IsFriendly;
  public int HousingCategory;
  public TownResidentCapabilities TownCapabilities;

  // 派生：不存储 “是否是城镇服务 NPC”。
  public bool CanUseHousing =>
    IsTownResident &&
    (TownCapabilities & TownResidentCapabilities.CanUseHousing) != 0;
}
```

战斗的默认生命、防御、伤害和免疫定义继续属于 `CombatAndStatus` 内容目录；空间尺寸、重力与碰撞
默认值继续属于 `Movement/Physics` 内容目录。这样 `NpcDefinition` 只持有本子系统需要的定义引用，
不成长为 `NPC.SetDefaults` 的新巨型替身。

### 5.2 `NpcPersonalityDefinitionCatalog` 和定义值对象

`PersonalityDatabase` 与 `PersonalityProfile.ShopModifiers` 的只读替代。关系定义按 NPC 类型共享，
不能复制为每名居民的“当前幸福度”字段。

```csharp
namespace Terraria.Town;

public sealed class NpcPersonalityDefinitionCatalog
{
  // 权威：NPC 类型到只读个性定义。
  public Dictionary<NpcTypeId, NpcPersonalityDefinition> ByResidentType = new();

  // 权威：内容重载版本。
  public uint Revision;

  public int Count => ByResidentType.Count;
}

public sealed class NpcPersonalityDefinition
{
  public NpcTypeId ResidentType;
  public List<BiomeAffectionDefinition> BiomeAffections = new();
  public List<NpcAffectionDefinition> NeighborAffections = new();
}

public readonly record struct BiomeAffectionDefinition(
  BiomeId BiomeId,
  AffectionLevel Level);

public readonly record struct NpcAffectionDefinition(
  NpcTypeId OtherResidentType,
  AffectionLevel Level);

public enum AffectionLevel : sbyte
{
  Hate = -100,
  Dislike = -1,
  Neutral = 0,
  Like = 1,
  Love = 100
}
```

### 5.3 `NpcDialogueSession`

原 `LocalPlayer.talkNPC` 是本地玩家索引，不能进入服务器权威 NPC 状态。每个玩家最多拥有一个会话，
会话关闭、断线、NPC 清理或距离失效时由连接/交互边界移除。

```csharp
namespace Terraria.Server.Town;

public sealed class NpcDialogueSession
{
  // 权威：会话所属玩家和连接范围的唯一 ID。
  public PlayerEntityId PlayerId;
  public ConnectionId ConnectionId;

  // 权威：会话锁定的 NPC 实例，不能保存 LegacySlot 作为主键。
  public NpcInstanceId NpcInstanceId;

  // 权威：当前阶段和最后接受的客户端意图序号，防止重放。
  public NpcDialogueSessionPhase Phase;
  public ulong LastAcceptedIntentSequence;

  // 权威：会话打开和最后活动时间，均采用权威世界 tick。
  public WorldTick OpenedAtTick;
  public WorldTick LastActivityTick;

  // 可重建：最近一次资格失败原因，供本连接的对话投影读取。
  public NpcInteractionDenialReason LastDenialReason;

  // 派生：不保存 IsOpen。
  public bool IsOpen => Phase is NpcDialogueSessionPhase.Open or NpcDialogueSessionPhase.AwaitingServiceResult;
}

public enum NpcInteractionDenialReason : byte
{
  None,
  NpcUnavailable,
  PlayerOutOfRange,
  ActionUnavailable,
  SessionClosed,
  SequenceRejected,
  RuleUnavailable
}
```

### 5.4 `NpcInteractionRegistry`

`NPCInteractions.All` 的注册边界。它只记录动作类型、适用 NPC 和 UI 元数据；资格计算和副作用
仍分别属于 Query 与 Command，尤其不能把当前为空的 `Condition`/`Interact` 规则伪装成目录字段。

```csharp
namespace Terraria.Town;

public sealed class NpcInteractionRegistry
{
  // 权威：动作 ID 到静态注册定义。
  public Dictionary<NpcInteractionActionId, NpcInteractionDefinition> ByActionId = new();

  // 可重建：按 NPC 类型反查动作，可由 ByActionId 重建。
  public Dictionary<NpcTypeId, List<NpcInteractionActionId>> ActionIdsByResidentType = new();

  // 权威：注册表版本；初始化/热重载时递增。
  public uint Revision;

  public int ActionCount => ByActionId.Count;
}

public readonly record struct NpcInteractionActionId(int Value);

public sealed class NpcInteractionDefinition
{
  public NpcInteractionActionId ActionId;
  public NpcInteractionKind Kind;
  public List<NpcTypeId> AllowedResidentTypes = new();
  public int? ShopIndex;
  public string? TextKey;
  public bool ShowsExclamation;
}

public enum NpcInteractionKind : byte
{
  OpenShop,
  OpenSign,
  DryadPurification,
  AnglerQuest,
  PetAnimal,
  OldManCurse,
  GuideTip,
  CollectTaxes,
  NurseHeal,
  CloseDialogue,
  ReportHappiness,
  RequestHome,
  PartyMusicSwap,
  GuideReverseCrafting,
  TinkererReforge,
  StylistHair,
  DyeTraderRarePlant,
  TavernkeepAdvice
}
```

## 6. 生成和报价支持值对象

这两个对象不附着到实体，也不进入会话根的长期状态。它们定义字段是为了让资格 Query 和服务
Command 获得无副作用输入/输出，而不是把即时计算结果塞回组件。

### 6.1 `NpcSpawnContextSnapshot`

```csharp
namespace Terraria.Npc;

public struct NpcSpawnContextSnapshot
{
  // 输入身份：玩家实体，而非 Main.player 数组索引。
  public PlayerEntityId PlayerId;

  // 生成 Tile 和玩家中心的即时空间快照。
  public TownRoomTilePoint SpawnTile;
  public int SpawnTileType;
  public int SpawnWallType;
  public Vector2 PlayerPosition;

  // 当前玩家/区域资格快照。
  public bool PlayerIsActive;
  public bool PlayerIsDead;
  public bool IsOnScreen;
  public bool IsInvasionArea;
  public bool IsDungeon;
  public bool IsWater;
  public bool IsLava;
  public bool IsHoney;
  public BiomeFlags Biomes;

  // 本次尝试使用的已计算限制；不是 NpcPopulationPressureState 的第二份字段。
  public int EffectiveSpawnRateTicks;
  public int EffectiveMaxSpawns;
  public float NearbyPopulationSlots;

  // 归属本次请求的确定性随机源标识。
  public uint RandomStreamId;
}

[Flags]
public enum BiomeFlags : ulong
{
  None = 0,
  Corrupt = 1UL << 0,
  Crimson = 1UL << 1,
  Hallow = 1UL << 2,
  Jungle = 1UL << 3,
  Snow = 1UL << 4,
  Glowshroom = 1UL << 5,
  Meteor = 1UL << 6,
  Graveyard = 1UL << 7,
  Desert = 1UL << 8,
  Temple = 1UL << 9
}
```

### 6.2 `TownHappinessQuote`

`ShopHelper.GetShoppingSettings` 的派生输出模型。它不含钱包、商店库存、最终商品价格或任何可写
幸福度计数；交易提交必须在使用它之前重新计算。

```csharp
namespace Terraria.Town;

public readonly record struct TownHappinessQuote(
  NpcInstanceId ResidentInstanceId,
  NpcTypeId ResidentTypeId,
  float PriceMultiplier,
  TownRoomTilePoint? HomeTile,
  BiomeId EvaluatedBiome,
  uint HousingRevision,
  uint PersonalityRevision,
  WorldTick EvaluatedAtTick)
{
  // 派生：不保存单独的 IsDiscount 或 IsSurcharge。
  public bool IsDiscount => PriceMultiplier < 1f;
  public bool IsSurcharge => PriceMultiplier > 1f;
}
```

## 7. 明确不定义字段的报告模块

以下报告模块没有持久状态所有权，因此本设计刻意不为它们创建“空组件”或字段。必要上下文应以方法
参数、命令、事件或上述值对象传递：

| 报告模块 | 不定义字段的理由 |
| --- | --- |
| `NpcSpawnQualificationQuery`、`NpcSpawnSelectionQuery`、`NpcTargetSelectionQuery`、`TownHousingQualificationQuery`、`TownHappinessQuery`、`NpcInteractionQualificationQuery` | 它们是纯资格/派生计算；持有可变字段会污染确定性和测试边界。 |
| `NpcSpawnSystem`、`NpcBehaviorSystem`、`NpcLifecycleSystem`、`TownHousingReevaluationSystem` | 行为系统读取/提交上述状态，不能把自身执行进度伪装成每个 NPC 的状态。 |
| `NpcSpawnCommand`、`TownHousingAssignmentCommand`、`NpcServiceCommand`、`QuestSubmissionCommand` | 命令表达一次意图和原子提交，不是跨 Tick 的组件。 |
| `NpcNetworkProjection`、`TownHousingNetworkProjection`、`TownHousingPersistenceProjection`、`DialogueProjection` | 输出权威状态的投影；网络连接缓存仅可位于 `NpcReplicationDirtyState` 或连接会话，不反向成为玩法真相。 |
| `NpcPresentationProjection` | 帧、颜色、纹理、头像、变体和 UI 只读消费定义/权威状态；不得让表现字段写回行为、住房或交易。 |

下表列出拆分报告中出现、但**不属于本 NPC/城镇字段设计所有权**的组件名称。它们被提及是为了
固定跨领域边界，不能因为本报告引用它们就复制字段到 `Npc/` 或 `Town/`。

| 组件名称 | 报告中的来源 | 字段所有权决定 |
| --- | --- | --- |
| `LocationComponent`、`VelocityComponent`、`ColliderComponent` | NPC 的位置、速度、尺寸和碰撞读写；SS14 的移动/转向参考 | 归 `Movement` 与 `Physics` 领域；NPC 只持有实体引用/行为意图，不保存位置或速度副本。 |
| `HealthComponent`、`NpcCombatStatsComponent`、`NpcStatusSlotsComponent` | `life`、`lifeMax`、伤害、防御、Buff 和免疫 | 归 `CombatAndStatus`；本设计的生命周期只消费死亡/清理结果。 |
| `StoreComponent` | SS14 Store 作为 UI、共享协议和服务器购买提交拆分的组织参考 | 是外部参考项目类型，绝不迁移其字段或领域语义；Version4 商店库存/货币归经济子系统。 |
| `SharedNPCComponent`、`NpcComponent`、`NPCSteeringComponent`、`NpcFactionMemberComponent`、`HTNComponent` | SS14 的薄标记、黑板、转向、阵营和 HTN 组织参考 | 是外部参考项目类型，绝不复制字段、命名或行为；本设计仅采用其“按访问模式拆分状态”的方法。 |

## 8. 字段证据与暂缓项

`NpcEntityIdentityComponent`、`NpcDefinitionReferenceComponent`、`NpcBehaviorStateComponent`、
`NpcTargetComponent`、`NpcLifetimeComponent`、`TownResidentComponent`、
`NpcHousingAssignmentComponent`、`NpcSlotStore`、`TownHousingStore` 和
`NpcReplicationDirtyState` 的核心字段分别对应 Version4 `NPC.cs`、`TownRoomManager.cs`、
`WorldGen.cs`、`Main.cs`、`NetMessage.cs` 的真实声明与主调用路径，证据状态为 `confirmed` 或
`partial`，详见拆分报告第 2、3 节。

下列事项保持 `missing`，故没有在组件中臆造字段或规则：

- 具体特殊刷怪类型选择、权重和事件分支；
- 各 `AI_*` 的阶段专属状态字段；
- 对话动作的距离/世界/任务资格、费用、物品扣除、任务奖励和回滚规则；
- 普通 NPC 商店目录、交易账本与货币扣除事务；
- 住房扫描对每一种 Tile/家具/特殊 NPC 的完整判定细节。

这些缺口获得规则源码或独立规格后，应优先新增对应行为族的专用状态组件或命令输入值对象，
而不是向 `NpcBehaviorStateComponent`、`TownResidentComponent` 或 `NpcDialogueSession` 追加无关字段。
