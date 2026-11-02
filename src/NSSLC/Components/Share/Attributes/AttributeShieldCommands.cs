using System;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;

namespace Terraria.Attributes;

/// <summary>Attributes-domain entry point for shield consumption requested by other domains.</summary>
/// <remarks>
/// Owning the write here keeps the shield pool's only mutator inside Attributes. Combat states the
/// amount and reads the outcome; it never touches <see cref="AttributeSetComponent"/> to apply a hit.
/// </remarks>
public static class AttributeShieldCommands
{
  /// <summary>Applies one shield consumption request against the target's attribute set.</summary>
  public static AttributeShieldConsumeResult Apply(
      ArchWorld world,
      ArchEntity target,
      in AttributeShieldConsumeRequestComponent request)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!world.IsAlive(target) || !world.Has<AttributeSetComponent>(target))
    {
      return new AttributeShieldConsumeResult(HasAttributes: false, Consumed: 0);
    }

    int consumed = AttributeSystem.ConsumeShield(
        world.Get<AttributeSetComponent>(target),
        request.RequestedDamage);
    return new AttributeShieldConsumeResult(HasAttributes: true, Consumed: consumed);
  }
}
