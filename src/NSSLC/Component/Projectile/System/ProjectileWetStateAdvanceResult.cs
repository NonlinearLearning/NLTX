using EntityEcs.Components;

namespace Terraria.Projectile;

/// <summary>Describes a wet-state transition and its legacy splash-effect eligibility.</summary>
public readonly record struct ProjectileWetStateAdvanceResult(
  ProjectileWetTransitionKind Transition,
  LiquidKind LiquidKind,
  bool ShouldRequestSplashEffect);
