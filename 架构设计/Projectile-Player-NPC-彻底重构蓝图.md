# Projectile、Player、NPC 实体彻底重构蓝图

## 目标

本蓝图不是把旧的 `Projectile.cs`、`Player.cs`、`NPC.cs` 拆成许多更小的同构类，
而是重新定义游戏实体模型：

```text
旧类：状态、行为、生成、全局规则、网络字段全部混在一起
新模型：实体身份 + 组件状态 + 系统规则 + 协议投影
```

目标是允许新模型完全改变旧类的内部结构，同时维持现有 Terraria 网络协议的字节兼容。
网络兼容的对象不是旧 C# 类型，而是消息号、字段顺序、字段宽度、压缩标志和实体匹配规则。

## 当前证据

审查文件：

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Entity.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs`
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\NetMessage.cs`

旧类的结构事实：

- `Entity` 混合了槽位索引、位置、速度、尺寸、朝向和液体接触。
- `Player` 混合了输入、移动、装备、库存、Buff、生命/魔力、死亡、社交和视觉状态。
- `NPC` 混合了 NPC 实例、自然生成器、Boss 全局状态、AI、分段关系、掉落和 Buff。
- `Projectile` 混合了投射物实例、伤害、穿透、免疫、Minion/Sentry、生命周期和数百个
  `AI_###` 行为。

这三类不应成为新 ECS 的基类，也不应分别替换为三个巨型 `PlayerComponent`、
`NpcComponent`、`ProjectileComponent`。

## 总体模型

```text
EntityId
  + TransformComponent
  + MotionComponent
  + ColliderComponent
  + 若干能力组件
  + 一个或多个领域标记组件

系统读取组件
  -> 产生 Event 或 Command
  -> 确定性提交
  -> 更新下一 tick 的组件
  -> ProtocolProjection 读取必要字段并编码网络包
```

实体类别不通过继承表达：

```text
PlayerEntity = PlayerMarker + 控制/库存/装备组件
NpcEntity = NpcMarker + AI/目标/战斗组件
ProjectileEntity = ProjectileMarker + 所有者/行为/伤害组件
```

## 目录目标态

```text
src/
  Entity/
    Components/
      EntityIdentityComponent.cs
      TransformComponent.cs
      MotionComponent.cs
      ColliderComponent.cs
      EntityLifetimeComponent.cs
      FluidContactComponent.cs

  Combat/
    Components/
      HealthComponent.cs
      DefenseComponent.cs
      DamageSourceComponent.cs
      DamageModifierComponent.cs
      FactionComponent.cs
      PenetrationComponent.cs
      HitImmunityComponent.cs
    Systems/
      DamageResolutionSystem.cs
      DeathSystem.cs
      HitImmunitySystem.cs
    Events/
      DamageRequestedEvent.cs
      DamageAppliedEvent.cs
      EntityDiedEvent.cs
    Commands/
      ApplyDamageCommand.cs

  Movement/
    Components/
      MovementIntentComponent.cs
      GravityComponent.cs
      MotionLockComponent.cs
    Systems/
      MovementSystem.cs
      GravitySystem.cs
      MovementCollisionSystem.cs

  StatusEffects/
    Components/
      BuffCollectionComponent.cs
      StatusEffectImmunityComponent.cs
    Systems/
      BuffDurationSystem.cs
      BuffEffectSystem.cs
    Events/
      BuffAddedEvent.cs
      BuffRemovedEvent.cs

  Targeting/
    Components/
      TargetComponent.cs
      AggroComponent.cs
    Systems/
      TargetSelectionSystem.cs
      AggroSystem.cs

  Player/
    Components/
      PlayerMarkerComponent.cs
      PlayerInputComponent.cs
      PlayerControlComponent.cs
      PlayerIdentityComponent.cs
      PlayerRespawnComponent.cs
    Systems/
      PlayerInputSystem.cs
      PlayerControlSystem.cs
      PlayerRespawnSystem.cs
    Events/
      PlayerDiedEvent.cs
      PlayerRespawnRequestedEvent.cs

  Npc/
    Components/
      NpcMarkerComponent.cs
      NpcDefinitionComponent.cs
      NpcAiComponent.cs
      NpcTargetComponent.cs
      NpcBossPhaseComponent.cs
      NpcSegmentParentComponent.cs
      NpcSegmentChildComponent.cs
    Systems/
      NpcAiSystem.cs
      NpcTargetSystem.cs
      NpcPhaseSystem.cs
    Definitions/
      NpcDefinition.cs
      NpcAiDefinition.cs
      NpcLootDefinition.cs

  NpcSpawning/
    Components/
      SpawnBudgetComponent.cs
      SpawnRuleStateComponent.cs
    Systems/
      NaturalNpcSpawnSystem.cs
      InvasionSpawnSystem.cs
      TownNpcSpawnSystem.cs
    Definitions/
      NpcSpawnDefinition.cs

  Projectile/
    Components/
      ProjectileMarkerComponent.cs
      ProjectileDefinitionComponent.cs
      ProjectileOwnerComponent.cs
      ProjectileLifetimeComponent.cs
      ProjectileBehaviorComponent.cs
      ProjectilePenetrationComponent.cs
      MinionComponent.cs
      SentryComponent.cs
      WhipComponent.cs
      GrappleComponent.cs
    Systems/
      ProjectileSpawnSystem.cs
      ProjectileBehaviorSystem.cs
      ProjectileLifetimeSystem.cs
      ProjectileCollisionSystem.cs
      ProjectileDamageSystem.cs
    Definitions/
      ProjectileDefinition.cs
      ProjectileBehaviorDefinition.cs

  Inventory/
    Components/
      InventoryComponent.cs
      SelectedItemComponent.cs
      EquipmentLoadoutComponent.cs
    Systems/
      InventorySystem.cs
      ItemSelectionSystem.cs
      EquipmentStatSystem.cs

  Protocol/
    Player/
      PlayerControlPacket.cs
      PlayerControlPacketCodec.cs
      PlayerStateProjection.cs
    Npc/
      NpcSyncPacket.cs
      NpcSyncPacketCodec.cs
      NpcStateProjection.cs
    Projectile/
      ProjectileSyncPacket.cs
      ProjectileSyncPacketCodec.cs
      ProjectileStateProjection.cs
```

`Protocol/` 是游戏状态到网络字节的适配层，不是实体存储层。它只能读取投影需要的组件，
不能反向要求实体恢复旧的公开字段集合。

## 共享能力与实体专属能力

### 所有可移动实体

```text
EntityIdentityComponent
TransformComponent
MotionComponent
ColliderComponent
EntityLifetimeComponent
```

### Player

```text
PlayerMarkerComponent
PlayerInputComponent
PlayerControlComponent
HealthComponent
DefenseComponent
BuffCollectionComponent
InventoryComponent
EquipmentLoadoutComponent
PlayerRespawnComponent
```

### NPC

```text
NpcMarkerComponent
NpcDefinitionComponent
HealthComponent
DefenseComponent
BuffCollectionComponent
NpcAiComponent
TargetComponent
FactionComponent
```

### Projectile

```text
ProjectileMarkerComponent
ProjectileDefinitionComponent
ProjectileOwnerComponent
ProjectileLifetimeComponent
ProjectileBehaviorComponent
MotionComponent
ColliderComponent
DamageSourceComponent
PenetrationComponent
```

### 特殊投射物

```text
MinionEntity: MinionComponent + ProjectileOwnerComponent + TargetComponent
SentryEntity: SentryComponent + ProjectileOwnerComponent + ProjectileLifetimeComponent
WhipEntity: WhipComponent + ProjectileOwnerComponent
GrappleEntity: GrappleComponent + ProjectileOwnerComponent
```

这些都是同一个实体世界中的组合，不需要从 `Projectile` 继承出若干深层类树。

## 协议兼容合同

### 消息 13：玩家控制/运动包

当前 `NetMessage.cs` 的发送顺序为：

```text
byte playerIndex
BitsByte controlFlags1
BitsByte controlFlags2
BitsByte stateFlags1
BitsByte stateFlags2
byte selectedItem
Vector2 position
[Vector2 velocity]                 // controlFlags2[2]
[ushort mountType]                 // controlFlags2[7]
[Vector2 potionReturnOriginal]     // stateFlags1[6]
[Vector2 potionReturnHome]         // stateFlags1[6]
[Vector2 netCameraTarget]          // stateFlags2[5]
```

接收端 `MessageBuffer.GetData` 将其投影为：

```text
控制：up/down/left/right/jump/useItem
朝向：direction
移动：position/velocity
载具：pulley/mount
状态：vortex stealth、重力方向、盾牌、幽灵、悬浮、坐下、宠物、睡眠
交互：自动连发、按住下、操作其他实体、使用 Tile、物品使用结果
相机：netCameraTarget
```

新模型只需要保证这些字段有来源：

```text
PlayerInputComponent
PlayerControlComponent
TransformComponent
MotionComponent
FacingComponent
MountStateComponent
PlayerInteractionStateComponent
CameraTargetComponent
```

不需要让整个 `Player` 具有旧类的全部字段。

### 消息 23：NPC 同步包

当前发送顺序为：

```text
short npcIndex
Vector2 position
Vector2 velocity
ushort target
BitsByte npcFlags
BitsByte spawnFlags
[float ai[0..3]]                    // npcFlags[2..5] 各自控制
short netId
[byte scaledPlayerCount]            // spawnFlags[0]
[float difficulty]                  // spawnFlags[2]
[lifeWidth + encoded life]          // npcFlags[7] 为 false
[byte releaseOwner]                 // 可捕获 NPC
```

新模型需要提供：

```text
EntityIdentityComponent -> npcIndex
TransformComponent -> position
MotionComponent -> velocity
TargetComponent -> target
NpcDefinitionComponent -> netId/type
NpcAiComponent -> typed behavior state projected to ai[0..3]
HealthComponent -> life/lifeMax
NpcSpawnStateComponent -> statue/difficulty/scaled players/shimmer/release owner
FacingComponent -> direction/directionY/spriteDirection
```

`ai[0..3]` 是协议兼容投影，不是新 ECS 的内部模型。新模型内部应使用有类型的行为状态；
协议层为需要旧客户端的 NPC 定义稳定的四槽映射。

### 消息 27：投射物同步包

当前发送顺序为：

```text
short identity
Vector2 position
Vector2 velocity
byte owner
short type
BitsByte flags1
[BitsByte flags2]                    // flags1[2]
[float ai0]                          // flags1[0]
[float ai1]                          // flags1[1]
[ushort bannerId]                   // flags1[3]
[short damage]                      // flags1[4]
[float knockBack]                   // flags1[5]
[short originalDamage]              // flags1[6]
[short projUuid]                    // flags1[7]
[float ai2]                          // flags2[0]
```

接收端还使用：

```text
owner + identity -> 查找现有投射物实体
找不到 -> 选择空槽或最老实体
type 不匹配 -> 重新应用类型定义
owner/type/identity -> 建立客户端投影索引
```

新模型需要提供：

```text
ProjectileIdentityComponent -> identity/projUuid
TransformComponent -> position
MotionComponent -> velocity
ProjectileOwnerComponent -> owner
ProjectileDefinitionComponent -> type
ProjectileBehaviorComponent -> typed state projected to ai[0..2]
DamageSourceComponent -> damage/originalDamage/knockBack
BannerResponseComponent -> bannerId
```

同样，`ai[0..2]` 只在协议适配层存在。新行为系统不得把 `ai[0]` 当成无类型全局变量。

### 消息 29：投射物销毁

旧协议使用：

```text
short identity
byte owner
```

因此投射物的网络身份不能只用 ECS EntityId。必须保留：

```text
ProjectileNetworkIdentity
  Owner
  Identity
  OptionalUuid
```

ECS EntityId 可以重新分配，但 `Owner + Identity` 的协议索引在该投射物生命周期内必须稳定。

## 协议适配器接口

新 ECS 不直接让 `NetMessage` 访问任意组件。建议把协议边界收窄为三个投影：

```csharp
public interface IPlayerStateProjector
{
  PlayerControlPacket ProjectPlayerControl(EntityId entity);
}

public interface INpcStateProjector
{
  NpcSyncPacket ProjectNpc(EntityId entity);
}

public interface IProjectileStateProjector
{
  ProjectileSyncPacket ProjectProjectile(EntityId entity);
}
```

编码器只接收这些协议 DTO：

```text
ECS World
  -> StateProjector
  -> Protocol DTO
  -> Packet Codec
  -> BinaryWriter
```

解码方向为：

```text
BinaryReader
  -> Packet DTO
  -> Validate/Authorize
  -> Command 或受限组件写入
```

网络包不能直接调用 `SetDefaults`、`AI()`、`Update()` 或旧类的任意行为方法。

## 新行为系统

### Player 主链

```text
PlayerControlPacket
  -> PlayerInputSystem
  -> PlayerControlSystem
  -> MovementSystem
  -> ItemUseSystem
  -> SpawnProjectileCommand
  -> ProjectileSpawnSystem
```

### NPC 主链

```text
NpcDefinition
  -> NpcSpawnSystem
  -> NpcAiSystem
  -> TargetSelectionSystem
  -> MovementSystem
  -> CollisionSystem
  -> DamageResolutionSystem
```

### Projectile 主链

```text
ProjectileDefinition
  -> ProjectileSpawnSystem
  -> ProjectileBehaviorSystem
  -> MovementSystem
  -> ProjectileCollisionSystem
  -> DamageRequestedEvent
  -> DamageResolutionSystem
  -> ProjectileLifetimeSystem
```

系统顺序要显式注册，不依赖目录或反射枚举顺序。

## `ai[]` 的彻底替代策略

旧代码的 `aiStyle`、`ai[]`、`localAI[]` 是行为槽位，不是好的领域模型。新模型分三层：

```text
ProjectileDefinition:
  BehaviorId、初始参数、伤害和碰撞规则

ProjectileBehaviorComponent:
  当前状态、计时器、目标、阶段、行为参数

ProjectileBehaviorSystem:
  按 BehaviorId 执行有类型规则
```

迁移时暂时保留一个兼容映射：

```text
LegacyAiProjection
  BehaviorId -> ai[0..2]
  BehaviorState -> ai[0..2]
```

该映射只能在 `Protocol/Projectile` 使用，不能反向成为实体内部的公共字段。

NPC 的 `ai[0..3]` 同理。

## 必须移出三个实体的内容

以下内容不能留在新实体类中：

```text
NPC.Spawner                       -> NpcSpawning Systems
自然生成条件                       -> NpcSpawnDefinition
Boss 全局索引和事件状态             -> WorldEvents/BossState Resource
Projectile 静态免疫表               -> Combat Definition/Resource
Projectile 类型默认值               -> ProjectileDefinition
Player 输入采样                     -> PlayerInputSystem
Player 的装备汇总计算                -> EquipmentStatSystem
Player/NPC Buff 计时                 -> BuffDurationSystem
各类 AI_### 方法                    -> 有类型 Projectile/Npc Behavior Systems
网络字节读写                        -> Protocol DTO/Codec
```

如果这些内容继续存放在三个实体中，重构只会变成旧 God Object 的新命名。

## 迁移闸门

彻底重构不以“旧类还能编译”为完成标准，而以以下证据为准：

### 协议级

- 消息 13 的编码字节与旧实现逐字段一致。
- 消息 23 的稀疏 AI 标志、生命宽度编码和类型字段一致。
- 消息 27 的 `owner + identity` 匹配、稀疏 AI 和可选字段一致。
- 消息 29 能按 `owner + identity` 销毁同一投射物。
- 已知旧客户端可以接收新服务端投影；新客户端可以接收旧服务端包。

### 游戏级

- Player 可以输入、移动、使用物品、受伤、死亡和重生。
- NPC 可以生成、选择目标、移动、受伤、死亡和产生掉落。
- Projectile 可以生成、移动、碰撞、造成伤害、穿透和销毁。
- Minion、Sentry、分段 NPC 至少各有一个组合实体测试。

### 模型级

- 新实体不继承旧 `Player`、`NPC` 或 `Projectile`。
- 协议编码器不读取旧类字段。
- `ai[]` 不出现在新游戏逻辑的公共接口中。
- 生成器、静态定义、行为系统和网络投影均不归实体实例所有。

## 明确的非目标

本蓝图当前不声称已经完成代码重构。当前仓库 `D:\TRbackup\NLTX` 没有可承载该实现的
`.csproj`/`.cs` ECS 项目；`Version4` 的旧工程仍是 `net40/x86` 的单体项目。

因此本阶段交付的是可实现、可验证的目标模型和协议合同，不是把三份百万行级旧类立即
改写成未验证的新代码。
