using System;
using Terraria.Ai;

namespace Terraria.Projectile;

/// <summary>
/// Binds projectile behavior implementations to the shared AI registry after profile validation.
/// </summary>
public static class ProjectileAiRegistration
{
  public static AiSystem<ProjectileAiIntent> CreateSystem(
      ProjectileBehaviorCatalog catalog,
      ProjectileBehaviorProfile homingProfile)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(homingProfile);
    catalog.Get(homingProfile.BehaviorId, homingProfile.ProfileId);
    if (homingProfile.BehaviorType != typeof(ProjectileHomingAiBehavior))
    {
      throw new ArgumentException(
          "The supplied profile is not bound to ProjectileHomingAiBehavior.",
          nameof(homingProfile));
    }

    AiSystem<ProjectileAiIntent> system = new();
    system.Register<ProjectileStraightAiBehavior>();
    system.Register(() => new ProjectileHomingAiBehavior(homingProfile));
    return system;
  }

  public static AiBinding<ProjectileAiIntent> Bind(
      AiSystem<ProjectileAiIntent> system,
      ProjectileBehaviorCatalog catalog,
      in AiBehaviorSelectionComponent behavior,
      Func<ProjectileBehaviorProfile, AiBehavior<ProjectileAiIntent>> factory)
  {
    ArgumentNullException.ThrowIfNull(system);
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(factory);
    ProjectileBehaviorProfile profile = catalog.Get(
        behavior.BehaviorId,
        behavior.ProfileId);
    AiComponent aiComponent = AiComponent.For(profile.BehaviorType);
    return system.Bind(aiComponent, () => factory(profile));
  }
}
