using System;
using Terraria.Share.Entities;
using Terraria.Share.Liquid;

namespace Terraria.Unshared.Projectiles;

/// <summary>Contributes the liquid family: interaction capability plus contact scratch state.</summary>
/// <remarks>
/// 只挂 <see cref="LiquidInteractionCapabilityComponent" /> 是不够的：空间协调系统只在实体已有
/// <see cref="LiquidContactComponent" /> 时回写接触事实，缺这一项，液体策略就是一个没人读的开关。
/// 因此启用液体的内容必须同时拿到两件，这条耦合由本 section 自己保证，不由装配函数记忆。
/// </remarks>
public sealed class ProjectileLiquidSection : IProjectileSection
{
  private readonly bool _enabled;
  private readonly LiquidContactFlags _affectedLiquids;

  public ProjectileLiquidSection(bool enabled, LiquidContactFlags affectedLiquids)
  {
    if (enabled && affectedLiquids == LiquidContactFlags.None)
    {
      throw new ArgumentException(
          "An enabled liquid family must declare at least one affected liquid.",
          nameof(affectedLiquids));
    }

    _enabled = enabled;
    _affectedLiquids = affectedLiquids;
  }

  public bool Enabled => _enabled;

  public LiquidContactFlags AffectedLiquids => _affectedLiquids;

  public void Collect(in ProjectileSectionContext context, ProjectileCompositionParameters parameters)
  {
    parameters.DeclareLiquid(_enabled, _affectedLiquids);
  }

  public void Contribute(
      in ProjectileSectionContext context,
      ProjectileCompositionParameters parameters,
      EntityBuilder builder)
  {
    builder.Add(() => new LiquidInteractionCapabilityComponent(
        Enabled: _enabled,
        AffectedLiquids: _affectedLiquids));

    if (_enabled)
    {
      builder.Add(() => new LiquidContactComponent(LiquidContactFlags.None, 0));
    }
  }
}
