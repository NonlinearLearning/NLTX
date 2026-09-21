# Version4 ProjectileSimulation 实际代码组件草案

## 1. 文档元数据

~~~text
subsystemId: ProjectileSimulation
taskNumber: 13
sourceReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-projectile-simulation-public-decomposition.md
sourceDesign: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-projectile-simulation-component-design.md
outputPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-projectile-simulation-code-component-draft.md
draftScope: component-code-shape
designStatus: candidate
implementationStatus: implemented-in-src
evidenceStatus: partial
nltxStatus: partial
verificationStatus: compile-verified
behaviorVerificationStatus: not-run
testStatus: not-written
~~~

本文件把已有的 Component-only 设计翻译成接近实际 C# 的类型声明草案。status: proposed 表示“建议的代码形状”，不表示对应 .cs 文件已经创建、迁移、编译或行为等价。

本文件最初只描述代码形状；本轮已按其候选结构将生产组件实现到根 src/Projectile 项目。测试项目仍未创建，dome 组件也未被本轮修改：

- Test/Terraria.Projectile.Components.Verification/
- dome/src/Terraria.Dome.Simulation/Components/Projectile/*.cs
- 任何协议、快照或测试文件

## 2. 如何阅读这份草案

### 2.1 已选代码形状

本次采用评审型方案：

| 方案 | 形状 | 取舍 | 本草案选择 |
| --- | --- | --- | --- |
| A | 只有字段的最小 struct | 最接近纯数据，但默认值、不变量和目标文件映射不明显 | 否 |
| B | 每个组件独立文件的 struct/marker，加最小构造入口、字段注释和映射 | 可直接转成源码评审，仍不把运行时行为塞进组件 | 是 |
| C | B 加完整 value object、跨域 owner 和序列化类型 | 信息最完整，但会在证据不足时过早锁定跨子系统契约 | 否 |

方案 B 的构造函数只承担字段初始化，不执行时钟、随机数、网络、持久化、日志、实体创建、目标结算或其他外部效果。跨子系统类型未闭合的地方保留为注释和 unresolved 状态，不能用猜测的类型把缺口伪装成已解决。

### 2.2 目标代码边界

草案原先建议 dome 的 Projectile 领域扁平目录；本轮按用户指定改为根 src 下的 Projectile 领域扁平目录：

~~~text
D:\TRbackup\NLTX\src\Projectile\
  ProjectileIdentityComponent.cs
  ProjectileDefinitionComponent.cs
  ProjectileTrajectoryStateComponent.cs
  ProjectileLifetimeComponent.cs
  ProjectileDamagePayloadComponent.cs
  ProjectilePenetrationStateComponent.cs
  ProjectileHitImmunityStateComponent.cs
  ProjectileHitImmunityPolicyComponent.cs
  ProjectileCollisionPolicyComponent.cs
  ProjectileGeometryStateComponent.cs
  ProjectileNetworkStateComponent.cs
  ProjectileSourceMetadataComponent.cs
  ProjectileTrailCacheComponent.cs
  ProjectileSentryCapabilityComponent.cs
  ProjectileMinionCapabilityComponent.cs
  ProjectileTrapCapabilityComponent.cs
  ProjectileBobberCapabilityComponent.cs
  ProjectileCounterweightCapabilityComponent.cs
~~

实际实现使用命名空间 Terraria.Projectile。原 dome 路径的代码块仍保留为历史目标形状，不表示 dome 已迁移。

~~~text
D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Components\Projectile\
  ProjectileIdentityComponent.cs
  ProjectileDefinitionComponent.cs
  ProjectileTrajectoryStateComponent.cs
  ProjectileLifetimeComponent.cs
  ProjectileDamagePayloadComponent.cs
  ProjectilePenetrationStateComponent.cs
  ProjectileHitImmunityStateComponent.cs
  ProjectileHitImmunityPolicyComponent.cs
  ProjectileCollisionPolicyComponent.cs
  ProjectileGeometryStateComponent.cs
  ProjectileNetworkStateComponent.cs
  ProjectileSourceMetadataComponent.cs
  ProjectileTrailCacheComponent.cs
  ProjectileSentryCapabilityComponent.cs
  ProjectileMinionCapabilityComponent.cs
  ProjectileTrapCapabilityComponent.cs
  ProjectileBobberCapabilityComponent.cs
  ProjectileCounterweightCapabilityComponent.cs
~~~

建议命名空间为现有 dome 组件使用的 Terraria.Dome.Simulation.Components。目录表达 Projectile 领域，命名空间是否调整必须另行作为 API/依赖变更评估；不能由目录移动自动决定。

根 src/Projectile 下已经存在的组件作为当前兼容实现和映射参考，不在本草案中宣布迁移目标。两个代码边界的 owner、引用关系和共享类型仍需 integration-review 裁决。

### 2.3 共同代码约定

- 每个代码块对应一个独立文件和一个核心公开类型；代码块之间的同名类型不能与现有源码同时放入同一项目。
- 组件只持有持续的实体状态、定义快照、缓存或能力标记；不承载跨实体行为。
- 不把 LocationComponent、VelocityComponent、通用 ColliderComponent、WorldGrid、Tile、NPC/Player health、Fishing outcome 或 LeashedEntity registry 复制到 Projectile。
- SlotIndex、owner-scoped Identity、ProjectileUuid、runtime entity、network replication ID 和 future persistent ID 保持不同的命名空间。
- raw Ai0..Ai2 与 LocalAi0..LocalAi2 在全部行为契约闭合前保留；不能因为 dome 已有局部 typed state 就删除兼容槽位。
- 代码草案中的 EntityReference、目标 identity 容量、Fishing context reference 和跨项目 enum 只在已有证据允许的范围内使用；未解决处明确标注。

## 3. 依赖与暂未锁定的共享类型

以下类型不是本文件新建的 Component。它们只作为代码形状的依赖说明：

| 类型 | 当前来源 | 在本草案中的处理 | 状态 |
| --- | --- | --- | --- |
| System.Numerics.Vector2 | .NET | 用于轨迹缓存的既有数学类型 | confirmed |
| Terraria.Relationships.EntityReference | src/Relationships/EntityReference.cs | 作为 owner reference 候选；root/dome 项目引用关系未锁定 | partial |
| Terraria.Projectile.ProjectileEndReason | src/Projectile/ProjectileEndReason.cs | 复用已有终止原因类型候选 | partial |
| ProjectileDamageClass | dome Projectile/Definitions/ProjectileDamageClass.cs | 作为伤害载荷的类型候选 | partial |
| ProjectileHostileDamageScaling | dome Projectile/Definitions/ProjectileHostileDamageScaling.cs | 作为 hostile 伤害规则的类型候选 | partial |
| FishingContextReference | 尚未找到稳定共享类型 | 不在代码中虚构；只留待整合注释 | unresolved |
| ProjectileOwnerKind | 尚未完成 owner domain 设计 | 不新增；owner domain 仍由 EntityReference.Scope/整合边界裁决 | unresolved |
| ReplicationId、persistent ID | 尚未找到闭合契约 | 不放入 Component | missing |

如果最终统一到 dome 专属 value object，应先确认项目引用、生命周期和序列化边界，再替换这里的候选类型；本文件不因代码外观而宣布这些整合已经完成。

## 4. Component 代码草案

### 4.1 ProjectileIdentityComponent

~~~text
componentId: BD-COMP-PROJ-01
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileIdentityComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation
crossSubsystemOwner: integration-review
~~~

~~~csharp
using Terraria.Relationships;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileIdentityComponent
{
  public ProjectileIdentityComponent(
    EntityReference ownerReference,
    int slotIndex = -1,
    int identity = 0,
    int projectileUuid = -1)
  {
    OwnerReference = ownerReference;
    SlotIndex = slotIndex;
    Identity = identity;
    ProjectileUuid = projectileUuid;
  }

  public EntityReference OwnerReference;
  public int SlotIndex;
  public int Identity;
  public int ProjectileUuid;
}
~~~

字段语义：SlotIndex 是可复用的 Version4 槽位 token；Identity 是 owner-scoped identity；ProjectileUuid 只保留 Version4 projUUID 兼容语义。runtime entity、network replication ID 和 persistent ID 不进入此类型。

关键未决项：当前 root 使用 EntityReference，dome 使用 PlayerHandle；NPC、trap、world 和 server owner 的统一 domain 仍为 integration-review。构造函数不验证 owner scope，是为了避免在跨项目契约未确定时偷偷固化错误规则。

现有映射：src/Projectile/ProjectileOwnerComponent.cs 为 partial；dome/.../ProjectileNetworkIdentityComponent.cs 也为 partial，不能直接视为等价替换。

### 4.2 ProjectileDefinitionComponent

~~~text
componentId: BD-COMP-PROJ-02
status: proposed
coverageStatus: partial
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileDefinitionComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 ContentCatalog 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileDefinitionComponent
{
  public ProjectileDefinitionComponent(
    int projectileType,
    int behaviorKey,
    bool friendlyDefault,
    bool hostileDefault,
    int extraUpdates,
    int catalogRevision = 0,
    bool noEnchantments = false)
  {
    ProjectileType = projectileType;
    BehaviorKey = behaviorKey;
    FriendlyDefault = friendlyDefault;
    HostileDefault = hostileDefault;
    ExtraUpdates = extraUpdates;
    CatalogRevision = catalogRevision;
    NoEnchantments = noEnchantments;
  }

  public int ProjectileType;
  public int BehaviorKey;
  public bool FriendlyDefault;
  public bool HostileDefault;
  public int ExtraUpdates;
  public int CatalogRevision;
  public bool NoEnchantments;
}
~~~

这里只保留内容目录解析出的 type、behavior key、阵营默认值、extra update 预算、目录版本和附魔规则。DefaultDamage、碰撞、免疫、网络、能力和 child spawn 不应继续回流到这个组件。FriendlyDefault/HostileDefault 是否需要独立的当前 disposition 状态尚未裁决，因此组件整体保留 partial 状态。

现有映射：根 src/Projectile/ProjectileDefinitionComponent.cs 已有精简定义形状；dome 同名 record 当前过宽，不能直接用本草案替换而不做字段拆分。

### 4.3 ProjectileTrajectoryStateComponent

~~~text
componentId: BD-COMP-PROJ-03
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileTrajectoryStateComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileTrajectoryStateComponent
{
  public ProjectileTrajectoryStateComponent(
    float ai0 = 0.0f,
    float ai1 = 0.0f,
    float ai2 = 0.0f,
    float localAi0 = 0.0f,
    float localAi1 = 0.0f,
    float localAi2 = 0.0f,
    float rotation = 0.0f,
    int spriteDirection = 1,
    float stepSpeed = 1.0f,
    int substepCounter = 0)
  {
    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    LocalAi0 = localAi0;
    LocalAi1 = localAi1;
    LocalAi2 = localAi2;
    Rotation = rotation;
    SpriteDirection = spriteDirection;
    StepSpeed = stepSpeed;
    SubstepCounter = substepCounter;
  }

  public float Ai0;
  public float Ai1;
  public float Ai2;
  public float LocalAi0;
  public float LocalAi1;
  public float LocalAi2;
  public float Rotation;
  public int SpriteDirection;
  public float StepSpeed;
  public int SubstepCounter;
}
~~~

Ai0..Ai2/LocalAi0..LocalAi2 是兼容状态，不是 Definition 的字段，也不是 network snapshot 的第二份 authority。SubstepCounter 是实例临时状态，不能替代 Definition 的 ExtraUpdates。通用位置、速度和 Collider 不在这里复制。

现有映射：根 ProjectileBehaviorComponent 只有部分槽位；dome Components/AI/ProjectileBehaviorComponent.cs 已有局部 typed state，仍不能证明全部 Version4 behavior key 的读写契约已经覆盖。

### 4.4 ProjectileLifetimeComponent

~~~text
componentId: BD-COMP-PROJ-04
status: proposed
coverageStatus: partial
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileLifetimeComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation
crossSubsystemOwner: integration-review
~~~

~~~csharp
using Terraria.Projectile;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileLifetimeComponent
{
  public ProjectileLifetimeComponent(
    int remainingTicks = 3600,
    ProjectileEndReason endReason = ProjectileEndReason.None)
  {
    RemainingTicks = remainingTicks;
    EndReason = endReason;
  }

  public int RemainingTicks;
  public ProjectileEndReason EndReason;
}
~~~

RemainingTicks == 0 是过期/终止状态的候选表达，不等于 child、drop、channel 或 network tombstone 已完成。active 不作为第二个权威字段加入。默认 3600 是 Version4 基线，type 覆盖仍由定义初始化边界负责；现有 dome 的正数构造限制和 root 的 EndReason 尚未统一。

### 4.5 ProjectileDamagePayloadComponent

~~~text
componentId: BD-COMP-PROJ-05
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileDamagePayloadComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 CombatAndStatus 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileDamagePayloadComponent
{
  public ProjectileDamagePayloadComponent(
    int currentDamage = 0,
    int originalDamage = 0,
    float knockback = 0.0f,
    int armorPenetration = 0,
    int bonusCritChance = 0,
    int bonusTagDamage = 0,
    int tagEffectType = 0,
    ProjectileDamageClass damageClass = ProjectileDamageClass.Generic,
    bool isColdDamage = false,
    bool isArrow = false,
    ProjectileHostileDamageScaling hostileDamageScaling =
      ProjectileHostileDamageScaling.Default)
  {
    CurrentDamage = currentDamage;
    OriginalDamage = originalDamage;
    Knockback = knockback;
    ArmorPenetration = armorPenetration;
    BonusCritChance = bonusCritChance;
    BonusTagDamage = bonusTagDamage;
    TagEffectType = tagEffectType;
    DamageClass = damageClass;
    IsColdDamage = isColdDamage;
    IsArrow = isArrow;
    HostileDamageScaling = hostileDamageScaling;
  }

  public int CurrentDamage;
  public int OriginalDamage;
  public float Knockback;
  public int ArmorPenetration;
  public int BonusCritChance;
  public int BonusTagDamage;
  public int TagEffectType;
  public ProjectileDamageClass DamageClass;
  public bool IsColdDamage;
  public bool IsArrow;
  public ProjectileHostileDamageScaling HostileDamageScaling;
}
~~~

伤害 payload 不保存 target ID、target health、最终减血值或跨目标免疫表。DamageClass 是伤害类别的单一来源候选，不再并列保存可能互相冲突的 melee/ranged/magic 布尔值。

现有映射：根 Damage Component 已有部分数值字段；dome Damage Component 还包含 HitCount 近似字段，而命中计数应归入 Penetration，因此不能直接拼接两者。

### 4.6 ProjectilePenetrationStateComponent

~~~text
componentId: BD-COMP-PROJ-06
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectilePenetrationStateComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectilePenetrationStateComponent
{
  public ProjectilePenetrationStateComponent(
    int remainingHits = 1,
    int maximumHits = 1,
    int hitCount = 0,
    bool stopsDealingDamageWhenDepleted = false)
  {
    RemainingHits = remainingHits;
    MaximumHits = maximumHits;
    HitCount = hitCount;
    StopsDealingDamageWhenDepleted = stopsDealingDamageWhenDepleted;
  }

  public int RemainingHits;
  public int MaximumHits;
  public int HitCount;
  public bool StopsDealingDamageWhenDepleted;
}
~~~

-1 是 Version4 无限穿透的候选 sentinel；除 -1 外的负值不应被默默接受。每个接受的命中序列最多增加一次 HitCount。穿透耗尽后的销毁原因仍由 Lifetime 领域表达，不在此组件执行清理。

现有映射：根、dome 都有 Penetration Component，但 dome 目前把命中计数近似放在 Damage Component；实施时必须选定唯一写入 owner。

### 4.7 ProjectileHitImmunityStateComponent

~~~text
componentId: BD-COMP-PROJ-07
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileHitImmunityStateComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 CombatAndStatus 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileHitImmunityStateComponent
{
  public ProjectileHitImmunityStateComponent(
    int npcCapacity,
    int playerCapacity = 255)
  {
    LocalNpcImmunityTicks = new int[npcCapacity];
    PlayerImmunityTicks = new int[playerCapacity];
    RestrikeDelayTicks = 0;
  }

  public int[] LocalNpcImmunityTicks;
  public int[] PlayerImmunityTicks;
  public int RestrikeDelayTicks;
}
~~~

数组是单个 Projectile 的实例状态，目标 identity 的容量和索引语义由整合边界提供。static NPC immunity、owner cooldown、type 共享表和目标自身免疫不复制进来。当前代码骨架故意要求显式容量，避免猜测 NPC 容量常量。

现有 dome 的 hit/restrike 相关组件只能视为局部覆盖；Version4 的 local/static/owner fallback 组合尚未闭合。

### 4.8 ProjectileHitImmunityPolicyComponent

~~~text
componentId: BD-COMP-PROJ-08
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileHitImmunityPolicyComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 CombatAndStatus 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileHitImmunityPolicyComponent
{
  public ProjectileHitImmunityPolicyComponent(
    bool usesLocalNpcImmunity = false,
    bool usesStaticNpcImmunity = false,
    int localNpcCooldownTicks = -2,
    int staticNpcCooldownTicks = -1,
    bool appliesOnSingleHit = false,
    bool usesOwnerMeleeCooldown = false,
    bool copiesOwnerCooldownOnSpawn = false)
  {
    UsesLocalNpcImmunity = usesLocalNpcImmunity;
    UsesStaticNpcImmunity = usesStaticNpcImmunity;
    LocalNpcCooldownTicks = localNpcCooldownTicks;
    StaticNpcCooldownTicks = staticNpcCooldownTicks;
    AppliesOnSingleHit = appliesOnSingleHit;
    UsesOwnerMeleeCooldown = usesOwnerMeleeCooldown;
    CopiesOwnerCooldownOnSpawn = copiesOwnerCooldownOnSpawn;
  }

  public bool UsesLocalNpcImmunity;
  public bool UsesStaticNpcImmunity;
  public int LocalNpcCooldownTicks;
  public int StaticNpcCooldownTicks;
  public bool AppliesOnSingleHit;
  public bool UsesOwnerMeleeCooldown;
  public bool CopiesOwnerCooldownOnSpawn;
}
~~~

策略与实际计数分离。-2 和 -1 sentinel 的解释必须由命中规则确认；本组件不保存 target 集合，也不保存 static immunity 表。CopiesOwnerCooldownOnSpawn 在 owner domain 未闭合前只能作为候选字段。

### 4.9 ProjectileCollisionPolicyComponent

~~~text
componentId: BD-COMP-PROJ-09
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileCollisionPolicyComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 SpatialSimulation 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileCollisionPolicyComponent
{
  public ProjectileCollisionPolicyComponent(
    bool tileCollisionEnabled = true,
    bool ignoreWater = false,
    bool correctSlopeCollision = false,
    bool decidesManualFallThrough = false,
    bool shouldFallThrough = false,
    bool reflectsFromTiles = false,
    int maximumBounces = 0,
    float bounceVelocityMultiplier = 1.0f,
    float minimumBounceSpeed = 0.0f,
    bool ownerHitCheck = false,
    float ownerHitCheckDistance = 1000.0f,
    bool manualDirectionChange = false)
  {
    TileCollisionEnabled = tileCollisionEnabled;
    IgnoreWater = ignoreWater;
    CorrectSlopeCollision = correctSlopeCollision;
    DecidesManualFallThrough = decidesManualFallThrough;
    ShouldFallThrough = shouldFallThrough;
    ReflectsFromTiles = reflectsFromTiles;
    MaximumBounces = maximumBounces;
    BounceVelocityMultiplier = bounceVelocityMultiplier;
    MinimumBounceSpeed = minimumBounceSpeed;
    OwnerHitCheck = ownerHitCheck;
    OwnerHitCheckDistance = ownerHitCheckDistance;
    ManualDirectionChange = manualDirectionChange;
  }

  public bool TileCollisionEnabled;
  public bool IgnoreWater;
  public bool CorrectSlopeCollision;
  public bool DecidesManualFallThrough;
  public bool ShouldFallThrough;
  public bool ReflectsFromTiles;
  public int MaximumBounces;
  public float BounceVelocityMultiplier;
  public float MinimumBounceSpeed;
  public bool OwnerHitCheck;
  public float OwnerHitCheckDistance;
  public bool ManualDirectionChange;
}
~~~

ShouldFallThrough 是实例覆盖，DecidesManualFallThrough 是是否允许该覆盖的策略。该组件不拥有 Tile、liquid、WorldGrid、broadphase 或目标 Collider；ReflectsFromTiles 和 bounce 字段只保存投射物侧规则输入。

现有 dome 的 TileCollision、FallThrough、Bounce、Reflection 等组件已经形成局部边界，但与宽 Definition 的重复字段仍需在实施时裁掉。

### 4.10 ProjectileGeometryStateComponent

~~~text
componentId: BD-COMP-PROJ-10
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileGeometryStateComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 SpatialSimulation 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileGeometryStateComponent
{
  public ProjectileGeometryStateComponent(
    float scale = 1.0f,
    bool reflected = false)
  {
    Scale = scale;
    Reflected = reflected;
  }

  public float Scale;
  public bool Reflected;
}
~~~

Scale 是投射物特有的几何输入，不能成为通用 Collider 的第二份 authority；Reflected 是实例结果，不是 Definition 的镜像。特殊历史点另见 TrailCache。

现有 dome 通过 ProjectileReflectionComponent 和宽 Definition 分散表达这些字段，根组件没有完整对应类型，目标覆盖为 proposed。

### 4.11 ProjectileNetworkStateComponent

~~~text
componentId: BD-COMP-PROJ-11
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileNetworkStateComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 NetworkSessionAndSectionStreaming 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileNetworkStateComponent
{
  public ProjectileNetworkStateComponent(
    int playerCapacity = 255,
    bool networkImportant = false,
    bool primaryUpdatePending = false,
    bool secondaryUpdatePending = false,
    int netSpam = 0,
    bool sendRequested = false)
  {
    NetworkImportant = networkImportant;
    PrimaryUpdatePending = primaryUpdatePending;
    SecondaryUpdatePending = secondaryUpdatePending;
    NetSpam = netSpam;
    SectionSyncSkippedForPlayer = new bool[playerCapacity];
    SendRequested = sendRequested;
  }

  public bool NetworkImportant;
  public bool PrimaryUpdatePending;
  public bool SecondaryUpdatePending;
  public int NetSpam;
  public bool[] SectionSyncSkippedForPlayer;
  public bool SendRequested;
}
~~~

PrimaryUpdatePending/SecondaryUpdatePending 是 dirty/同步状态，不是“已经发出”的证明。SectionSyncSkippedForPlayer 的索引是连接/player slot，不能当作 projectile identity。ReplicationId、packet、serialized bytes、section object 和连接状态不进入此组件。

现有 dome 的 ProjectileNetworkIdentityComponent 与 ProjectileNetworkUpdateComponent 需要先拆出身份和网络状态的边界；不能只按字段合并。

### 4.12 ProjectileSourceMetadataComponent

~~~text
componentId: BD-COMP-PROJ-12
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileSourceMetadataComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 SpawnLifecycleAndLoot 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileSourceMetadataComponent
{
  public ProjectileSourceMetadataComponent(
    int bannerIdToRespondTo = 0,
    string miscText = "",
    bool originatedFromActivableTile = false,
    bool noDropItem = false,
    bool isNpcProjectile = false,
    ushort minionSpawnItemType = 0,
    int minionSpawnItemPrefix = 0)
  {
    BannerIdToRespondTo = bannerIdToRespondTo;
    MiscText = miscText;
    OriginatedFromActivableTile = originatedFromActivableTile;
    NoDropItem = noDropItem;
    IsNpcProjectile = isNpcProjectile;
    MinionSpawnItemType = minionSpawnItemType;
    MinionSpawnItemPrefix = minionSpawnItemPrefix;
  }

  public int BannerIdToRespondTo;
  public string MiscText;
  public bool OriginatedFromActivableTile;
  public bool NoDropItem;
  public bool IsNpcProjectile;
  public ushort MinionSpawnItemType;
  public int MinionSpawnItemPrefix;
}
~~~

MiscText 是来源兼容元数据，不是日志或 UI owner。NoDropItem 只表达禁止掉落标记，不表示掉落事务已经完成。完整 IEntitySource、MinionSpawnInfo、ItemInstanceId、child spawn 和 fishing outcome 不嵌入此组件。

若最终要求强制禁止 null，应在实施前确认项目的 nullable 设置和字段初始化策略；当前构造函数只提供字符串默认字面量，不能代替全局 nullable 契约。

### 4.13 ProjectileTrailCacheComponent

~~~text
componentId: BD-COMP-PROJ-13
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileTrailCacheComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 SpatialSimulation/Presentation 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
using System.Collections.Generic;
using System.Numerics;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileTrailCacheComponent
{
  public ProjectileTrailCacheComponent(int historyLength = 10)
  {
    OldPositions = new Vector2[historyLength];
    OldRotations = new float[historyLength];
    OldSpriteDirections = new int[historyLength];
    WhipPoints = new List<Vector2>();
  }

  public Vector2[] OldPositions;
  public float[] OldRotations;
  public int[] OldSpriteDirections;
  public List<Vector2> WhipPoints;
}
~~~

这些数组和列表是可重建缓存，不是当前位置、速度或方向的唯一 authority。WhipPoints 只在特殊几何需要时使用；其清空、重建和容量规则仍需由对应几何边界确定。组件持有可变列表时，所有权和有效期必须保持在 Projectile 实体生命周期内，不能把内部列表泄漏给任意外部调用方。

现有代码尚未提供与 Version4 三个 trail 数组完全对应的权威 Component，目标为 proposed。

### 4.14 ProjectileSentryCapabilityComponent

~~~text
componentId: BD-COMP-PROJ-14
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileSentryCapabilityComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 PlayerGameplay 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileSentryCapabilityComponent;
~~~

组件的存在即表示 sentry 能力，不再额外存储 IsSentry bool。owner capacity、放置规则、炮塔数量和 persistent placement 不属于该 marker。

现有 dome ProjectileSentryComponent.cs 已有同义 marker，当前只能记为 existing/partial，因为 owner 生命周期和完整行为关系还未验证。

### 4.15 ProjectileMinionCapabilityComponent

~~~text
componentId: BD-COMP-PROJ-15
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileMinionCapabilityComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 PlayerGameplay 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileMinionCapabilityComponent
{
  public ProjectileMinionCapabilityComponent(
    float minionSlots = 0.0f,
    int minionPosition = 0)
  {
    MinionSlots = minionSlots;
    MinionPosition = minionPosition;
  }

  public float MinionSlots;
  public int MinionPosition;
}
~~~

该组件只保存投射物侧的容量和 owner 内位置。owner 的 summon accounting、owner 当前目标 NPC 和 target reference 不复制进来；当前 dome ProjectileMinionComponent 与 SummonedProjectileStateComponent 只覆盖局部能力。

### 4.16 ProjectileTrapCapabilityComponent

~~~text
componentId: BD-COMP-PROJ-16
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileTrapCapabilityComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 WorldInteraction/CombatAndStatus 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileTrapCapabilityComponent;
~~~

组件的存在即表示 trap 能力。Tile 激活来源、world structure、陷阱放置和目标结算保持在相邻领域；它不拥有 Tile entity ID 或 target ID。

现有 dome ProjectileTrapComponent.cs 已有 marker，但 trap owner 仍为 partial。

### 4.17 ProjectileBobberCapabilityComponent

~~~text
componentId: BD-COMP-PROJ-17
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileBobberCapabilityComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 FishingAndCatchSimulation 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public struct ProjectileBobberCapabilityComponent
{
  public ProjectileBobberCapabilityComponent(int bobberType = 0)
  {
    BobberType = bobberType;
  }

  public int BobberType;
}
~~~

该组件的存在表示 bobber carrier；BobberType 只作为兼容 carrier 元数据候选。当前不加入 FishingContextReference，因为稳定 value/reference 类型和 owner 尚未确定。

以下状态明确排除在本 Component 之外：FishingAttempt、phase、水体计数、fishing level、pending item、catch result、掉落和敌对生成。dome 的 FishingBobberStateComponent 因此不能整体并入该类型。

### 4.18 ProjectileCounterweightCapabilityComponent

~~~text
componentId: BD-COMP-PROJ-18
status: proposed
targetFile: dome/src/Terraria.Dome.Simulation/Components/Projectile/ProjectileCounterweightCapabilityComponent.cs
namespace: Terraria.Dome.Simulation.Components
componentOwner: ProjectileSimulation 与 PlayerGameplay 的边界
crossSubsystemOwner: integration-review
~~~

~~~csharp
namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileCounterweightCapabilityComponent;
~~~

组件的存在即表示 counterweight 能力。它不拥有绳索、玩家、挂接实体集合或表现关系；若后续确认独立 attachment lifecycle，应由整合边界建立独立关系模型。

现有 dome ProjectileCounterweightComponent.cs 已有 marker，但与 LeashedEntity/attachment 边界仍未闭合。

## 5. 组件组合约束

以下是实体组合的代码级约束摘要，不是运行时调度或处理流程：

| 组件 | 组合要求 | 禁止的重复 authority |
| --- | --- | --- |
| Identity | 每个活跃 Projectile 必须能区分 owner、slot、identity 和可选 UUID | runtime entity、replication ID、persistent ID |
| Definition | 每个实例绑定一个内容定义快照 | damage、lifetime、immunity count、network dirty |
| TrajectoryState | 需要 Version4 行为兼容状态的 Projectile 使用 | Location、Velocity、通用 Collider |
| Lifetime | 每个具有生命周期的 Projectile 使用 | 独立 active 权威 bool |
| DamagePayload | 只有可造成伤害的 Projectile 使用 | target health、最终减血值、命中次数 |
| PenetrationState | 使用穿透/命中限制时使用 | Damage Component 内的第二个 HitCount |
| HitImmunityState | 使用 Projectile-local 免疫时使用 | static/type/target-owned immunity |
| HitImmunityPolicy | 需要特殊免疫规则时使用 | immunity counter 数组 |
| CollisionPolicy | 需要投射物侧 Tile/liquid/fall-through 规则时使用 | WorldGrid、Tile、broadphase |
| GeometryState | 需要 scale/reflection 实例状态时使用 | 通用 Collider、历史 trail |
| NetworkState | 需要投射物侧 dirty/节流/section skip 状态时使用 | packet、bytes、replication registry |
| SourceMetadata | 需要来源兼容字段或终止标记时使用 | 完整 source object、drop/result transaction |
| TrailCache | 需要历史几何或轨迹缓存时可选使用 | 当前位置、当前速度 |
| Sentry/Minion/Trap/Bobber/Counterweight capability | 按能力存在与否组合 | 在 Definition 中重复能力 bool |

特别注意：能力 marker 的存在是能力事实；SourceMetadata.IsNpcProjectile 是来源分类；Definition.FriendlyDefault/HostileDefault 是定义默认。三者不是同一个布尔字段的不同名字。

## 6. 与当前 NLTX 代码的覆盖映射

本表只说明“草案类型与当前代码的关系”，不构成迁移授权。existing 只表示源码文件中已有局部类型；不表示 Version4 行为等价或已通过验证。

| 草案 Component | 当前根实现 | 当前 dome 实现 | 覆盖判断 |
| --- | --- | --- | --- |
| Identity | src/Projectile/ProjectileOwnerComponent.cs | Components/Projectile/ProjectileNetworkIdentityComponent.cs | partial；owner/slot/UUID/replication 尚未统一 |
| Definition | src/Projectile/ProjectileDefinitionComponent.cs | Components/Projectile/ProjectileDefinitionComponent.cs | partial；dome record 过宽 |
| TrajectoryState | src/Projectile/ProjectileBehaviorComponent.cs、ProjectileDirectionComponent.cs | Components/AI/ProjectileBehaviorComponent.cs、Components/Projectile/ProjectileStepSpeedComponent.cs | partial；raw/typed 覆盖不完整 |
| Lifetime | src/Projectile/ProjectileLifetimeComponent.cs | Components/Projectile/ProjectileLifetimeComponent.cs | partial；EndReason/终止边界不一致 |
| DamagePayload | src/Projectile/ProjectileDamageComponent.cs | Components/Projectile/ProjectileDamageComponent.cs + 宽 Definition | partial；字段和 HitCount owner 不一致 |
| PenetrationState | src/Projectile/ProjectilePenetrationComponent.cs | Components/Projectile/ProjectilePenetrationComponent.cs | partial |
| HitImmunityState | 无完整对应 | dome hit/restrike 相关局部组件 | partial |
| HitImmunityPolicy | 无完整对应 | 宽 Definition 中有部分字段 | partial |
| CollisionPolicy | 无完整对应 | Tile/FallThrough/Bounce/Reflection 组件 | partial；策略和实例状态有重复 |
| GeometryState | 无完整对应 | Reflection 与宽 Definition.Scale | partial |
| NetworkState | 无完整对应 | NetworkIdentity/NetworkUpdate | partial；不应把身份字段混入 |
| SourceMetadata | 无完整对应 | BannerResponse/MiscText/MinionSpawnSource | partial |
| TrailCache | 无完整对应 | 未找到 Version4 三数组等价组件 | proposed-only |
| SentryCapability | 无完整对应 | ProjectileSentryComponent.cs | existing/partial |
| MinionCapability | 无完整对应 | ProjectileMinionComponent.cs、SummonedProjectileStateComponent.cs | existing/partial |
| TrapCapability | 无完整对应 | ProjectileTrapComponent.cs | existing/partial |
| BobberCapability | 无完整对应 | ProjectileBobberComponent.cs + FishingBobberStateComponent.cs | existing/partial；Fishing state 过宽 |
| CounterweightCapability | 无完整对应 | ProjectileCounterweightComponent.cs | existing/partial |

当前 dome 生成路径 dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileSpawnSystem.cs 会创建 Arch entity 并组合若干现有组件，但使用 Guid.NewGuid() 和 PlayerHandle 等当前实现选择，不能被本草案视为 Version4 NewProjectile 的完整等价实现。

## 7. 明确不生成的代码类型

本次只产出 Component 声明草案，以下对象不在新代码清单中：

- 通用 LocationComponent、VelocityComponent、ColliderComponent；Projectile 只组合或引用这些共享能力，不复制其 authority。
- WorldGrid、Tile、liquid、section registry、NPC/Player health、death、status 和 Item inventory/drop transaction。
- FishingAttempt、catch result、pending item、完整 FishingContext 和敌对生成结果。
- LeashedEntity registry、anchor、section list、独立生命周期和网络模块。
- ProjectileReplicationSnapshot、NetworkProjectileSlice 和任何协议 packet/serialized bytes；它们是外部快照/协议表示，不是权威 Component。
- alpha、frame、drawLayer、hide、soundDelay、preview dummy 等表现或工具状态；本草案不建立权威 Presentation Component。
- persistent ID Component、replication ID Component、owner+identity 反查表和 slot allocator；这些对象的生命周期/owner 尚未闭合。

## 8. 代码落地前的阻断项

以下问题会改变字段或依赖形状，因此在它们解决前，所有类型都必须保持 proposed/partial：

| gapId | 阻断项 | 直接影响 |
| --- | --- | --- |
| GAP-CODE-01 | root EntityReference、dome PlayerHandle 与 Version4 owner=255/NPC/trap/world owner 的统一 domain | Identity、Source、Minion、Sentry、Trap、Bobber |
| GAP-CODE-02 | slot、owner-scoped identity、projUUID、runtime entity、replication ID、persistent ID 的映射 | Identity、NetworkState |
| GAP-CODE-03 | raw AI/localAI 的保留期限和全部 behavior key 的 typed 覆盖 | TrajectoryState、Definition |
| GAP-CODE-04 | friendly/hostile 的定义默认与当前可变状态 owner | Definition、可能的新 disposition 边界 |
| GAP-CODE-05 | SetDefaults 的完整 type-specific 覆盖及数组重置契约 | Definition、Lifetime、Penetration、Collision |
| GAP-CODE-06 | static NPC immunity 共享表、owner fallback 和 local state 的最终组合 | ImmunityState、ImmunityPolicy |
| GAP-CODE-07 | MinionSpawnInfo、IEntitySource、ItemInstanceId 的稳定 value object | SourceMetadata、MinionCapability |
| GAP-CODE-08 | whip/cone/lance 等特殊几何的历史缓存格式和失效条件 | GeometryState、TrailCache、CollisionPolicy |
| GAP-CODE-09 | bobber carrier 与 Fishing outcome 的边界；Version4 当前 bobber 方法为空 | BobberCapability、Fishing 边界 |
| GAP-CODE-10 | LeashedEntity anchor/section/reference 的关系 owner | Counterweight、Identity 的跨域 relation |
| GAP-CODE-11 | Projectile persistence contract 缺失 | 不创建 persistent ID 类型 |
| GAP-CODE-12 | dome 项目是否能稳定引用 root 的 EntityReference、ProjectileEndReason 和 enum | 所有跨项目代码块 |

## 9. 草案验收记录

本次执行了根 Projectile 项目的编译检查，没有新增或运行测试：

- [x] 18 个候选 Component 均有独立类型名和目标文件路径。
- [x] 18 个候选 Component 均在元数据中标记 status: proposed，Definition 的整体设计状态保留为 partial。
- [x] 每个类型均给出 C# 风格字段声明；marker Component 使用空的 readonly record struct。
- [x] 每个字段组都注明了当前映射、排除范围或 unresolved 依赖。
- [x] owner、slot、identity、UUID、runtime entity、replication ID 和 persistent ID 没有被合并为一个字段。
- [x] 没有把 Fishing outcome、LeashedEntity registry、WorldGrid 或表现字段加入 Projectile Component。
- [x] 已在 src/Projectile 创建 18 个 Component 代码文件，并补充 2 个 Projectile 伤害规则 enum 文件。
- [x] 没有创建测试项目或测试源码。
- [x] 未修改 dome Projectile 组件、研究报告或 Component-only 设计边界。
- [x] 未启动子代理。
- [x] 根 Terraria.Projectile.csproj 编译：通过；未执行测试和运行时行为等价验证。

实际编译命令：

~~~powershell
$dotnetArgs = @(
  'build',
  '.\src\Projectile\Terraria.Projectile.csproj',
  '--no-restore',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false',
  '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 @dotnetArgs
~~~

编译结果：exit code 0，0 个警告，0 个错误。产物为 `Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`；未执行测试命令。

## 10. 最终声明

本文件是 ProjectileSimulation 的实际代码组件草案及其实现记录，不是行为等价或迁移完成声明。所有代码块仍表达组件边界和候选字段；根 src/Projectile 已有对应生产类型，但 dome 中的同名类型仍然独立存在，不能把两边直接混编。

本文件没有定义或实现任何运行时处理、调用顺序、网络收发、存档流程或测试计划。designStatus: candidate、evidenceStatus: partial 和 nltxStatus: partial 保持不变；组件项目编译已验证，但运行时行为等价仍未验证。

本轮未修改 Version4、完整参考源码、tModLoader 文档、Space Station 14、dome Projectile 源码、公共协议或源研究报告；没有新增测试，未启动子代理。
