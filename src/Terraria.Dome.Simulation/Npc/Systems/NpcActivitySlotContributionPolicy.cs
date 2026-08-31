using System;
using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcActivitySlotContributionPolicy
{
  public const int UnownedReleaseOwner = byte.MaxValue;

  public const float SlimeRainNpcSlotMultiplier = 0.65f;

  private const int ExcludedNpcTypeOne = 25;

  private const int ExcludedNpcTypeTwo = 30;

  private const int ExcludedNpcTypeThree = 33;

  public static NpcActivitySlotContribution Evaluate(NpcActivitySlotContributionInput input)
  {
    Validate(input);
    if (!input.IsNpcActive || !input.IsPlayerInActiveRange || input.LifeMaximum <= 0 ||
        input.ReleaseOwner != UnownedReleaseOwner || IsExcludedNpcType(input.NpcType))
    {
      return new(false, 0.0f);
    }

    float multiplier = input.IsSlimeRainActive &&
      LegacySlimeRainNpcRegistry.IsSlimeRainNpc(input.NpcType)
        ? SlimeRainNpcSlotMultiplier
        : 1.0f;
    float slotWeight = input.NpcSlotCost * multiplier;
    if (!float.IsFinite(slotWeight))
    {
      throw new ArgumentOutOfRangeException(nameof(input));
    }

    return new(true, slotWeight);
  }

  private static bool IsExcludedNpcType(int npcType)
  {
    return npcType is ExcludedNpcTypeOne or ExcludedNpcTypeTwo or ExcludedNpcTypeThree;
  }

  private static void Validate(NpcActivitySlotContributionInput input)
  {
    if (input.NpcType < 0 || input.NpcType >= LegacySlimeRainNpcRegistry.NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(input.NpcType));
    }

    if (input.ReleaseOwner < 0 || input.ReleaseOwner > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(input.ReleaseOwner));
    }

    if (!float.IsFinite(input.NpcSlotCost) || input.NpcSlotCost < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(input.NpcSlotCost));
    }
  }
}
