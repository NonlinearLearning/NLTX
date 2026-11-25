using System;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Projectiles;

/// <summary>Contributes the homing family: steering capability plus the cross-family acceleration.</summary>
/// <remarks>
/// 转向率是全仓唯一真实存在的跨族读数：它同时是 <see cref="ProjectileHomingCapabilityComponent" />
/// 的输入和 <c>MovementCapabilityComponent.acceleration</c> 的输入。前者在本族构造，
/// 后者只能经由参数块声明；两遍折叠因此不是风格选择，而是"装配结果与 section 顺序无关"的前提。
/// <para>
/// 加速度必须来自转向率而不是写死 0：空间运动用 <c>MoveTowards</c> 逼近目标速度，
/// 加速率为 0 时追踪弹每一帧都要求"立刻转向"，实际表现为撞向墙而不是绕行。
/// </para>
/// </remarks>
public sealed class ProjectileHomingSection : IProjectileSection
{
  private readonly float _turnRate;
  private readonly float _range;

  public ProjectileHomingSection(float turnRate, float range)
  {
    if (!float.IsFinite(turnRate) || turnRate < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(turnRate));
    }

    if (!float.IsFinite(range) || range < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(range));
    }

    _turnRate = turnRate;
    _range = range;
  }

  /// <summary>每 tick 允许改变的速度量。</summary>
  public float TurnRate => _turnRate;

  /// <summary>追踪半径；0 表示不限制。</summary>
  public float Range => _range;

  public void Collect(in ProjectileSectionContext context, ProjectileCompositionParameters parameters)
  {
    parameters.DeclareMovementAcceleration(_turnRate);
  }

  public void Contribute(
      in ProjectileSectionContext context,
      ProjectileCompositionParameters parameters,
      EntityBuilder builder)
  {
    builder.Add(() => new ProjectileHomingCapabilityComponent(_turnRate, _range));
  }
}
