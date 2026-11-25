using System;
using System.Numerics;
using Terraria.Share.Ai;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Contributes the flat-field family: identity, behavior, lifetime and bounds.</summary>
/// <remarks>
/// 「平铺族」对应 JSON 顶层那组无嵌套对象的字段（behaviorId / extraUpdates / lifetimeTicks /
/// tileCollide / ignoreOwner / collider / attackId / baseDamage）。它们共享读者与变更原因，
/// 因此是一族，而不是十二个 part。
/// <para>
/// 本类型只持<b>标量与值对象</b>：没有任何运行时组件类型的字段。组件一律在
/// <see cref="Contribute" /> 里构造，消费者要默认碰撞体与尺寸时读
/// <see cref="ColliderWidth" /> / <see cref="PixelWidth" /> 这组标量自己构造——
/// 内容是"默认值"，不是"当前值"，这条区别靠类型系统兜住。
/// </para>
/// <para>
/// 同时承接三种读者，但每个值只有一个来源：实体模板（身份、行为选择、寿命、碰撞策略）、
/// 生成请求参数（碰撞体与取整尺寸）、启动期伤害来源（攻击 id 与伤害参数）。
/// </para>
/// </remarks>
public sealed class ProjectileBasicsSection : IProjectileSection
{
  /// <summary>子步数上限，来自共享空间管线的能力上限。</summary>
  public const int MaximumSubstepCount = SubstepCapabilityComponent.MaximumCount;

  /// <summary>extraUpdates 上限；子步数是它加一，因此比 <see cref="MaximumSubstepCount" /> 小一。</summary>
  public const int MaximumExtraUpdates = MaximumSubstepCount - 1;

  private readonly AiBehaviorId _behaviorId;
  private readonly AiBehaviorProfileId _profileId;
  private readonly int _extraUpdates;
  private readonly int _lifetimeTicks;
  private readonly bool _collideWithTiles;
  private readonly bool _ignoreOwner;
  private readonly bool _collisionEnabled;
  private readonly uint _collisionLayer;
  private readonly uint _collisionMask;
  private readonly float _colliderWidth;
  private readonly float _colliderHeight;
  private readonly CombatAttackId _attackDefinitionId;
  private readonly DamageType _damageType;
  private readonly int _baseDamage;
  private readonly Vector2 _initialVelocity;

  public ProjectileBasicsSection(
      AiBehaviorId behaviorId,
      AiBehaviorProfileId profileId,
      int extraUpdates,
      int lifetimeTicks,
      bool collideWithTiles,
      bool ignoreOwner,
      float colliderWidth,
      float colliderHeight,
      CombatAttackId attackDefinitionId,
      DamageType damageType,
      int baseDamage,
      Vector2 initialVelocity = default,
      bool collisionEnabled = true,
      uint collisionLayer = 1,
      uint collisionMask = uint.MaxValue)
  {
    if (!behaviorId.IsValid)
    {
      throw new ArgumentException("An AI behavior id is required.", nameof(behaviorId));
    }

    if (!profileId.IsValid)
    {
      throw new ArgumentException("An AI behavior profile id is required.", nameof(profileId));
    }

    if (extraUpdates < 0 || extraUpdates > MaximumExtraUpdates)
    {
      throw new ArgumentOutOfRangeException(nameof(extraUpdates));
    }

    if (lifetimeTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lifetimeTicks));
    }

    if (!float.IsFinite(colliderWidth) || colliderWidth <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(colliderWidth));
    }

    if (!float.IsFinite(colliderHeight) || colliderHeight <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(colliderHeight));
    }

    if (!attackDefinitionId.IsValid)
    {
      throw new ArgumentException(
          "A valid combat attack definition id is required.", nameof(attackDefinitionId));
    }

    if (!Enum.IsDefined(damageType))
    {
      throw new ArgumentOutOfRangeException(nameof(damageType));
    }

    if (baseDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(baseDamage));
    }

    if (!float.IsFinite(initialVelocity.X) || !float.IsFinite(initialVelocity.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(initialVelocity));
    }

    _behaviorId = behaviorId;
    _profileId = profileId;
    _extraUpdates = extraUpdates;
    _lifetimeTicks = lifetimeTicks;
    _collideWithTiles = collideWithTiles;
    _ignoreOwner = ignoreOwner;
    _collisionEnabled = collisionEnabled;
    _collisionLayer = collisionLayer;
    _collisionMask = collisionMask;
    _colliderWidth = colliderWidth;
    _colliderHeight = colliderHeight;
    _attackDefinitionId = attackDefinitionId;
    _damageType = damageType;
    _baseDamage = baseDamage;
    _initialVelocity = initialVelocity;

    // 取整尺寸在这里派生一次：调用方（生成请求、物品使用射击）只读标量，不再二次取整。
    PixelWidth = checked((int)MathF.Ceiling(colliderWidth));
    PixelHeight = checked((int)MathF.Ceiling(colliderHeight));
  }

  /// <summary>默认碰撞体宽度，供生成请求构造碰撞体组件。</summary>
  public float ColliderWidth => _colliderWidth;

  /// <summary>默认碰撞体高度，供生成请求构造碰撞体组件。</summary>
  public float ColliderHeight => _colliderHeight;

  /// <summary>默认逻辑宽度的取整值，唯一推导点在本类型构造函数。</summary>
  public int PixelWidth { get; }

  /// <summary>默认逻辑高度的取整值，唯一推导点在本类型构造函数。</summary>
  public int PixelHeight { get; }

  /// <summary>Attack content this projectile's hits are attributed to.</summary>
  public CombatAttackId AttackDefinitionId => _attackDefinitionId;

  /// <summary>Damage family of the attack this projectile produces.</summary>
  public DamageType DamageType => _damageType;

  /// <summary>Base damage of the attack this projectile produces, before mitigation.</summary>
  public int BaseDamage => _baseDamage;

  /// <summary>
  /// 出厂速度：弹体生成时写入 <c>VelocityComponent</c> 的初始运动。
  /// </summary>
  /// <remarks>
  /// 速度是<b>内容属性</b>，不是发射参数：发射意图只回答"在哪、射什么"，出生速度由内容声明。
  /// 生成后 AI 行为从实体速度读到它并接管——直线弹保持它，追踪弹在它之上转向。
  /// 这与参考端"箭有出膛速度、之后再被 AI 改写"是一致的。
  /// </remarks>
  public Vector2 InitialVelocity => _initialVelocity;

  /// <summary>内容声明的额外更新次数；子步数是它加一。</summary>
  public int ExtraUpdates => _extraUpdates;

  /// <summary>出生时的剩余寿命 tick。</summary>
  public int LifetimeTicks => _lifetimeTicks;

  public AiBehaviorId BehaviorId => _behaviorId;

  public AiBehaviorProfileId ProfileId => _profileId;

  public void Collect(in ProjectileSectionContext context, ProjectileCompositionParameters parameters)
  {
    // 子步数来自内容 extraUpdates：高速弹体靠细分推进避免隧穿。
    parameters.DeclareSubstepCount(_extraUpdates + 1);
    parameters.DeclareTileCollision(_collideWithTiles);
  }

  public void Contribute(
      in ProjectileSectionContext context,
      ProjectileCompositionParameters parameters,
      EntityBuilder builder)
  {
    builder
        // 身份每次生成都新建；宿主据此分配并登记运行时句柄。
        .Add(() => new EntityIdentityComponent(EntityUuid.Create()))
        // 碰撞体属于实体定义，不属于生成请求：物理系统只读它，从不由生成方提供。
        // 与 NPC / 玩家的装配位置一致（见 NpcEntityDefinition.Add(ColliderComponent)）。
        .Add(() => new ColliderComponent(new Vector2(_colliderWidth, _colliderHeight)))
        .Add(() => new EntityCollisionCapabilityComponent(
            Enabled: _collisionEnabled,
            // 层号在构造期已校验非负，这里是唯一一次拓宽；能力组件按位存放，负值无意义。
            Layer: (uint)_collisionLayer,
            Mask: _collisionMask))
        // 出厂速度归内容定义，不归生成请求：发射接缝不携带速度（D2/D3）。
        // AI 行为从实体速度读到它并接管，因此实体必须生成即带这一项，否则首帧无从计算。
        .Add(() => new VelocityComponent(_initialVelocity))
        .Add(() => new AiBehaviorSelectionComponent(_behaviorId, _profileId))
        .Add(() => new ProjectileBehaviorStateComponent(0))
        .Add(() => new ProjectileLifetimeStateComponent(_lifetimeTicks))
        // 属性集每次生成新建：内容是只读模板，运行期会往里写每次生成的修正。
        // 不从内容克隆，因为内容侧已经没有可共享的实例。
        .Add(() => new AttributeSetComponent())
        .Add(() => new ProjectileHitImmunityStateComponent())
        .Add(() => new ProjectileCollisionPolicyComponent(
            CollideWithTiles: _collideWithTiles,
            IgnoreOwner: _ignoreOwner))
        .Add(() => new ProjectileAttackDefinitionReferenceComponent(_attackDefinitionId))
        .Add(() => new ProjectileDispositionStateComponent(ProjectileDisposition.Active));
  }
}
