namespace Terraria.Player;

public static class PlayerLifeRegenQuery
{
  private const int RegenThreshold = 120;
  private const int SpecialDamageThreshold = 600;
  private const int SpecialDamageAmount = 5;

  public static PlayerLifeRegenResult Evaluate(
    in PlayerLifeRegenInput input)
  {
    if (input.StatLifeMax2 <= 0)
    {
      return PlayerLifeRegenResult.Rejected(
        PlayerLifeRegenFailureReason.InvalidLifeCap,
        input);
    }

    if (input.StatLife < 0 ||
      !float.IsFinite(input.LifeRegenTime))
    {
      return PlayerLifeRegenResult.Rejected(
        input.StatLife < 0
          ? PlayerLifeRegenFailureReason.InvalidLifeState
          : PlayerLifeRegenFailureReason.InvalidRegenTime,
        input);
    }

    PlayerLifeRegenStatusFlags statuses = input.Statuses;
    bool shinyStoneStandingStill = Has(
      statuses,
      PlayerLifeRegenStatusFlags.ShinyStone | PlayerLifeRegenStatusFlags.StandingStill) &&
      !Has(statuses, PlayerLifeRegenStatusFlags.ItemAnimating);

    long lifeRegen = input.LifeRegen;
    long lifeRegenCount = input.LifeRegenCount;
    float lifeRegenTime = input.LifeRegenTime;

    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.Poisoned),
      4,
      false);
    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.Venom),
      30,
      false);
    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.OnFire),
      Has(statuses, PlayerLifeRegenStatusFlags.VampireSeed) ? 100 : 8,
      Has(statuses, PlayerLifeRegenStatusFlags.DrippingSlime));
    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.OnFire3),
      8,
      Has(statuses, PlayerLifeRegenStatusFlags.DrippingSlime));
    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.OnFrostBurn),
      16,
      Has(statuses, PlayerLifeRegenStatusFlags.DrippingSlime));
    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.OnFrostBurn2),
      16,
      Has(statuses, PlayerLifeRegenStatusFlags.DrippingSlime));
    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.OnFire2),
      24,
      Has(statuses, PlayerLifeRegenStatusFlags.DrippingSlime));
    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.Burned),
      60,
      Has(statuses, PlayerLifeRegenStatusFlags.DrippingSlime));
    ApplyDamageOverTime(
      ref lifeRegen,
      ref lifeRegenTime,
      Has(statuses, PlayerLifeRegenStatusFlags.Suffocating),
      40,
      false);

    if (Has(statuses, PlayerLifeRegenStatusFlags.Electrified))
    {
      SetNonPositive(ref lifeRegen, ref lifeRegenTime);
      lifeRegen -= 8;
      if (HasAny(
        statuses,
        PlayerLifeRegenStatusFlags.ControlLeft |
        PlayerLifeRegenStatusFlags.ControlRight))
      {
        lifeRegen -= 32;
      }
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.Tongued) &&
      Has(statuses, PlayerLifeRegenStatusFlags.ExpertMode))
    {
      ApplyDamageOverTime(ref lifeRegen, ref lifeRegenTime, true, 100, false);
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.Honey) && lifeRegen < 0)
    {
      lifeRegen = Math.Min(0, lifeRegen + 4);
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.NebulaLevelLife) && lifeRegen < 0)
    {
      lifeRegen = 0;
    }

    if (shinyStoneStandingStill && lifeRegen < 0)
    {
      lifeRegen /= 2;
    }

    lifeRegenTime += 1f;
    if (Has(statuses, PlayerLifeRegenStatusFlags.UsedAegisCrystal))
    {
      lifeRegenTime += 0.2f;
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.CrimsonRegen))
    {
      lifeRegenTime += 1f;
    }

    int soulDrain = Math.Max(0, input.SoulDrain);
    if (soulDrain > 0)
    {
      lifeRegenTime += 2f;
    }

    if (shinyStoneStandingStill)
    {
      if (lifeRegenTime > 90f && lifeRegenTime < 1800f)
      {
        lifeRegenTime = 1800f;
      }

      lifeRegenTime += 4f;
      lifeRegen += 4;
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.Honey))
    {
      lifeRegenTime += 2f;
      lifeRegen += 2;
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.Starving))
    {
      lifeRegen = 0;
      if (lifeRegenCount > 0)
      {
        lifeRegenCount = 0;
      }

      lifeRegenTime = 0f;
      long starvationRegen = Math.Max(4, 120L * input.StatLifeMax2 / 3000);
      lifeRegen = -starvationRegen;
    }

    if (soulDrain > 0)
    {
      int soulDrainRegen = (5 + soulDrain) / 2;
      lifeRegenTime += soulDrainRegen;
      lifeRegen += soulDrainRegen;
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.HeartyMeal))
    {
      lifeRegen += 6;
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.Bleed))
    {
      lifeRegenTime = 0f;
    }

    if (!shinyStoneStandingStill && lifeRegenTime >= 3600f)
    {
      lifeRegenTime = 3600f;
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.Resting))
    {
      lifeRegenTime += 3f;
    }

    float regenRate = CalculateRegenRate(
      lifeRegenTime,
      shinyStoneStandingStill,
      statuses);
    long roundedRegenRate = (long)Math.Round(regenRate);
    lifeRegen += roundedRegenRate;

    lifeRegenCount += lifeRegen;
    if (Has(statuses, PlayerLifeRegenStatusFlags.PalladiumRegen))
    {
      lifeRegenCount += 4;
    }

    if (shinyStoneStandingStill &&
      lifeRegen > 0 &&
      input.StatLife < input.StatLifeMax2)
    {
      lifeRegenCount++;
    }

    long healingTicks = 0;
    if (lifeRegenCount >= RegenThreshold)
    {
      healingTicks = lifeRegenCount / RegenThreshold;
      lifeRegenCount -= healingTicks * RegenThreshold;
    }

    long availableLife = Math.Max(0, (long)input.StatLifeMax2 - input.StatLife);
    int healingPoints = SaturateToInt(Math.Min(healingTicks, availableLife));

    bool specialDamage = Has(statuses, PlayerLifeRegenStatusFlags.Burned) ||
      Has(statuses, PlayerLifeRegenStatusFlags.Suffocating) ||
      (Has(statuses, PlayerLifeRegenStatusFlags.Tongued) &&
        Has(statuses, PlayerLifeRegenStatusFlags.ExpertMode));
    long damagePoints;
    long damageCommitCount;
    if (specialDamage)
    {
      (lifeRegenCount, damagePoints, damageCommitCount) = ConsumeDamage(
        lifeRegenCount,
        SpecialDamageThreshold,
        SpecialDamageAmount);
    }
    else if (Has(statuses, PlayerLifeRegenStatusFlags.Starving))
    {
      int starvationDamagePerCommit = Math.Max(2, input.StatLifeMax2 / 50);
      if (Has(statuses, PlayerLifeRegenStatusFlags.StarvingHarshBiome))
      {
        starvationDamagePerCommit = SaturateToInt(
          starvationDamagePerCommit * 2L);
      }

      long starvationThreshold = 120L * Math.Max(2, input.StatLifeMax2 / 50);
      (lifeRegenCount, damagePoints, damageCommitCount) = ConsumeDamage(
        lifeRegenCount,
        starvationThreshold,
        starvationDamagePerCommit);
    }
    else
    {
      (lifeRegenCount, damagePoints, damageCommitCount) = ConsumeDamage(
        lifeRegenCount,
        RegenThreshold,
        1,
        maxDamagePerCommit: 4);
    }

    return new PlayerLifeRegenResult(
      Computed: true,
      LifeRegen: SaturateToInt(lifeRegen),
      LifeRegenCountAfter: SaturateToInt(lifeRegenCount),
      LifeRegenTimeAfter: lifeRegenTime,
      HealingPoints: healingPoints,
      DamagePoints: SaturateToInt(damagePoints),
      DamageCommitCount: SaturateToInt(damageCommitCount),
      UsesSpecialDamageRule: specialDamage,
      RequiresExternalLifeCommit: healingPoints > 0 || damagePoints > 0,
      FailureReason: PlayerLifeRegenFailureReason.None);
  }

  private static float CalculateRegenRate(
    float lifeRegenTime,
    bool shinyStoneStandingStill,
    PlayerLifeRegenStatusFlags statuses)
  {
    float regenRate = 0f;
    int[] thresholds = [300, 600, 900, 1200, 1500, 1800, 2400, 3000];
    foreach (int threshold in thresholds)
    {
      if (lifeRegenTime >= threshold)
      {
        regenRate += 1f;
      }
    }

    if (shinyStoneStandingStill)
    {
      float additionalRate = (lifeRegenTime - 3000f) / 300f;
      if (additionalRate > 0f)
      {
        regenRate += Math.Min(additionalRate, 30f);
      }
    }
    else if (lifeRegenTime >= 3600f)
    {
      regenRate += 1f;
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.Resting))
    {
      regenRate *= 1.3f;
    }

    regenRate *= Has(statuses, PlayerLifeRegenStatusFlags.Moving) &&
      !Has(statuses, PlayerLifeRegenStatusFlags.Grappling)
      ? 0.5f
      : 1.25f;

    if (Has(statuses, PlayerLifeRegenStatusFlags.CrimsonRegen))
    {
      regenRate *= 1.5f;
    }

    if (Has(statuses, PlayerLifeRegenStatusFlags.ShinyStone))
    {
      regenRate *= 1.1f;
    }

    bool expertWithoutWellFed = Has(statuses, PlayerLifeRegenStatusFlags.ExpertMode) &&
      !Has(statuses, PlayerLifeRegenStatusFlags.WellFed);
    if (expertWithoutWellFed || Has(statuses, PlayerLifeRegenStatusFlags.Rabid))
    {
      regenRate *= Has(statuses, PlayerLifeRegenStatusFlags.ShinyStone)
        ? 0.75f
        : 0.5f;
    }

    return regenRate;
  }

  private static void ApplyDamageOverTime(
    ref long lifeRegen,
    ref float lifeRegenTime,
    bool active,
    int amount,
    bool doubled)
  {
    if (!active)
    {
      return;
    }

    SetNonPositive(ref lifeRegen, ref lifeRegenTime);
    lifeRegen -= amount;
    if (doubled)
    {
      lifeRegen -= amount;
    }
  }

  private static void SetNonPositive(
    ref long lifeRegen,
    ref float lifeRegenTime)
  {
    if (lifeRegen > 0)
    {
      lifeRegen = 0;
    }

    lifeRegenTime = 0f;
  }

  private static (long CountAfter, long DamagePoints, long CommitCount) ConsumeDamage(
    long count,
    long threshold,
    int damagePerCommit,
    int maxDamagePerCommit = int.MaxValue)
  {
    if (count > -threshold)
    {
      return (count, 0, 0);
    }

    long damageUnits = (-count) / threshold;
    long damagePoints = damageUnits * damagePerCommit;
    long commitCount = maxDamagePerCommit == int.MaxValue
      ? damageUnits
      : (damageUnits + maxDamagePerCommit - 1) / maxDamagePerCommit;
    long countAfter = count + damageUnits * threshold;
    return (countAfter, damagePoints, commitCount);
  }

  private static bool Has(
    PlayerLifeRegenStatusFlags statuses,
    PlayerLifeRegenStatusFlags flag)
  {
    return (statuses & flag) == flag;
  }

  private static bool HasAny(
    PlayerLifeRegenStatusFlags statuses,
    PlayerLifeRegenStatusFlags flags)
  {
    return (statuses & flags) != PlayerLifeRegenStatusFlags.None;
  }

  private static int SaturateToInt(long value)
  {
    return value > int.MaxValue
      ? int.MaxValue
      : value < int.MinValue
        ? int.MinValue
        : (int)value;
  }
}
