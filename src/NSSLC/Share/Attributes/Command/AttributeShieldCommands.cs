using System;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;

namespace Terraria.Share.Attributes;

/// <summary>Attributes-domain entry point for shield consumption requested by other domains.</summary>
/// <remarks>
/// Owning the write here keeps the shield pool's only mutator inside the shield boundary. Combat
/// states the amount and reads the outcome; it never touches <see cref="AttributeSetComponent"/>
/// to apply a hit.
/// </remarks>
public static class AttributeShieldCommands
{
  /// <summary>Applies one shield consumption request against the target's shield pool.</summary>
  public static AttributeShieldConsumeResult Apply(
      ArchWorld world,
      ArchEntity target,
      in AttributeShieldConsumeRequestComponent request)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!world.IsAlive(target) || !world.Has<ShieldPoolComponent>(target))
    {
      return new AttributeShieldConsumeResult(HasShieldPool: false, Consumed: 0);
    }

    int consumed = AttributeSystem.ConsumeShield(
        world.Get<ShieldPoolComponent>(target),
        request.RequestedDamage);
    return new AttributeShieldConsumeResult(HasShieldPool: true, Consumed: consumed);
  }
}
