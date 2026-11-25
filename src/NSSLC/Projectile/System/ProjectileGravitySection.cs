using System;
using Terraria.Share.Entities;
using Terraria.Share.Movement;

namespace Terraria.Unshared.Projectiles;

/// <summary>Contributes the gravity family: capability, configuration and initial state.</summary>
/// <remarks>
/// 关闭重力的内容不挂一个 <c>Enabled = false</c> 的"半开能力"，而是照旧挂满三件套并把开关写进
/// <see cref="GravityCapabilityComponent" />：重力积分是否真的跑由能力开关决定，
/// 而 <see cref="GravityConfigurationComponent" /> 必须存在，否则共享重力系统会在第一个 tick
/// 用默认值覆盖内容值。
/// <para>
/// 移动模式是本族唯一的跨族声明：受重力的弹体必须是 <see cref="MovementMode.Free" />，
/// 否则重力积分被 <see cref="MovementMode.Flight" /> 跳过，内容里的重力等于没写。
/// </para>
/// </remarks>
public sealed class ProjectileGravitySection : IProjectileSection
{
  private readonly bool _enabled;
  private readonly float _strength;
  private readonly float _maximumFallSpeed;

  public ProjectileGravitySection(bool enabled, float strength, float maximumFallSpeed)
  {
    if (!float.IsFinite(strength) || strength < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(strength));
    }

    if (!float.IsFinite(maximumFallSpeed) || maximumFallSpeed < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumFallSpeed));
    }

    _enabled = enabled;
    _strength = strength;
    _maximumFallSpeed = maximumFallSpeed;
  }

  public bool Enabled => _enabled;

  /// <summary>每 tick 的重力加速度。</summary>
  public float Strength => _strength;

  /// <summary>重力积分允许到达的最大下落速度。</summary>
  public float MaximumFallSpeed => _maximumFallSpeed;

  public void Collect(in ProjectileSectionContext context, ProjectileCompositionParameters parameters)
  {
    parameters.DeclareMovementMode(_enabled ? MovementMode.Free : MovementMode.Flight);
  }

  public void Contribute(
      in ProjectileSectionContext context,
      ProjectileCompositionParameters parameters,
      EntityBuilder builder)
  {
    builder
        .Add(() => new GravityCapabilityComponent(
            Enabled: _enabled,
            CanToggleDirection: false))
        .Add(() => new GravityConfigurationComponent(
            baseGravityMagnitude: _strength,
            baseMaxFallSpeed: _maximumFallSpeed))
        .Add(() => new GravityDirectionStateComponent(GravityDirection.Normal))
        .Add(() => new GravityModifierCollectionComponent())
        .Add(() => new GravityStateComponent(
            _strength,
            _maximumFallSpeed,
            GravityDirection.Normal,
            false,
            false));
  }
}
