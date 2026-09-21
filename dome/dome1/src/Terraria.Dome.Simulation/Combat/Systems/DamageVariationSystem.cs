using System;

namespace Terraria.Dome.Simulation.Combat.Systems;

public sealed class DamageVariationSystem
{
  private const float DamageStepPercent = 0.01f;

  public int Calculate(
    float damage,
    float luck,
    bool isVariationDisabled,
    IDamageVariationRandom random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (isVariationDisabled)
    {
      return (int)damage;
    }

    float result = ApplyStep(damage, random.NextDamageStep());
    if (luck > 0.0f)
    {
      if (random.NextChance() < luck)
      {
        result = MathF.Max(result, ApplyStep(damage, random.NextDamageStep()));
      }
    }
    else if (luck < 0.0f && random.NextChance() < -luck)
    {
      result = MathF.Min(result, ApplyStep(damage, random.NextDamageStep()));
    }

    return (int)Math.Round(result);
  }

  private static float ApplyStep(float damage, int step)
  {
    return damage * (1.0f + step * DamageStepPercent);
  }
}
