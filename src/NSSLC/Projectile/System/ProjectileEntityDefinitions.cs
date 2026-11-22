using Terraria.Attributes;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Physics;
using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>把投射物定义里的“静态组件组合”表达成共享的 <see cref="EntityDefinition"/>。</summary>
/// <remarks>
/// 审计 §30（#6）选择 A：先补 <c>IEntityDefinitionHost</c> 宿主，再把投射物创建接到同一套定义框架上。
/// 这里只收编与实例无关的组件：身份由工厂每次新建，运动学/生命周期/碰撞策略/攻击定义引用来自
/// <see cref="ProjectileDefinition"/>。出生位置、速度、阵营、归属、尺寸、碰撞体等属于实例级输入，
/// 仍由 <c>ProjectileLifecycleSystem.TrySpawn</c> 在生成后写入。
/// </remarks>
internal static class ProjectileEntityDefinitions
{
  /// <summary>内容定义 id 到实体组合定义 id 的唯一映射，注册与查表共用。</summary>
  public static EntityDefinitionId IdFor(ProjectileDefinitionId id)
  {
    return new EntityDefinitionId($"projectile/{id}");
  }

  public static EntityDefinition Build(ProjectileDefinition definition)
  {
    return new EntityDefinitionBuilder(IdFor(definition.Id))
        // 身份每次生成都新建；宿主据此分配并登记运行时句柄。
        .Add(() => new EntityIdentityComponent(EntityUuid.Create()))
        .Add(() => definition.Collision)
        .Add(() => definition.Behavior)
        .Add(() => new ProjectileBehaviorStateComponent(0))
        .Add(() => definition.Kinematics)
        .Add(() => definition.Lifetime)
        .Add(() => new ProjectilePenetrationStateComponent())
        .Add(() => definition.Attributes.Clone())
        .Add(() => new ProjectileHitImmunityStateComponent())
        .Add(() => definition.CollisionPolicy)
        .Add(() => new ProjectileAttackDefinitionReferenceComponent(definition.AttackDefinitionId))
        .Add(() => new ProjectileDispositionStateComponent(ProjectileDisposition.Active))
        // 以下一组是共享 SpatialSimulationCoordinatorSystem 的前置条件：查询要求
        // Location/Velocity/Collider/Grounding，回调里再 TryGet
        // MovementCapability/GravityCapability/GravityState/TileCollisionCapability/
        // LiquidInteractionCapability/MovementIntent。缺任何一项都会被静默跳过，投射物就永远不动。
        .Add(() => GroundingComponent.Airborne)
        // acceleration 必须为 0：MovementIntent.Stop 的 HasDesiredVelocity 为 true、目标速度为零，
        // 非零加速度会让 MoveTowards 把投射物逐帧减速到静止。
        // maximumSpeed 取 0 表示不钳制（SpatialSimulationSystem 只在 >0 时生效）。
        .Add(() => new MovementCapabilityComponent(
            enabled: true,
            maximumSpeed: 0f,
            acceleration: 0f,
            canJump: false,
            jumpSpeed: 0f,
            mode: MovementMode.Flight))
        // Flight 模式跳过重力积分，因此投射物按出生速度匀速直线飞行。
        // 逐类型重力需要 ProjectileDefinition 增加内容字段，届时再从定义取值；
        // 现在定义里没有重力，硬编码一个有值才是凭空造内容。
        .Add(() => new GravityCapabilityComponent(Enabled: false, CanToggleDirection: false))
        .Add(() => new GravityConfigurationComponent())
        .Add(() => new GravityDirectionStateComponent(GravityDirection.Normal))
        .Add(() => new GravityModifierCollectionComponent())
        .Add(() => new GravityStateComponent(0.4f, 10f, GravityDirection.Normal, false, false))
        .Add(() => new TileCollisionCapabilityComponent(Enabled: true))
        .Add(() => new LiquidInteractionCapabilityComponent(
            Enabled: true,
            AffectedLiquids: LiquidContactFlags.Water | LiquidContactFlags.Honey |
                LiquidContactFlags.Lava | LiquidContactFlags.Shimmer))
        .Add(() => MovementIntent.Stop(tick: 0))
        .Build();
  }
}
