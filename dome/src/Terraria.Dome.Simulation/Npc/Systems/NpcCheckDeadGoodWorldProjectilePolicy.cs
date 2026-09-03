using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckDeadGoodWorldProjectilePolicy
{
  public const int Version1456NpcTypeCount = 697;
  public const int GoodWorldNpcType = 631;
  public const int ProjectileType = 99;
  public const int ProjectileDamage = 70;
  public const float ProjectileKnockback = 10.0f;

  public static NpcCheckDeadGoodWorldProjectileDecision Evaluate(
    NpcCheckDeadGoodWorldProjectileInput input)
  {
    Validate(input);
    if (!input.IsGoodWorld || input.NpcType != GoodWorldNpcType)
    {
      return new(false, 0, input.Center, default, 0, 0.0f, input.ProjectileOwner);
    }

    return new(
      true,
      ProjectileType,
      input.Center,
      default,
      ProjectileDamage,
      ProjectileKnockback,
      input.ProjectileOwner);
  }

  private static void Validate(NpcCheckDeadGoodWorldProjectileInput input)
  {
    if (input.NpcType < 0 || input.NpcType >= Version1456NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(input.NpcType));
    }

    if (!float.IsFinite(input.Center.X) || !float.IsFinite(input.Center.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Center));
    }

    if (input.ProjectileOwner < -1 || input.ProjectileOwner >= 255)
    {
      throw new ArgumentOutOfRangeException(nameof(input.ProjectileOwner));
    }
  }
}
