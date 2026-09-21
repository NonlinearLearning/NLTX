namespace Terraria.Player;

public static class PlayerNpcPressureQuery
{
  public const int UnownedReleaseOwner = byte.MaxValue;
  public const float SlimeRainNpcSlotMultiplier = 0.65f;

  public static PlayerNpcPressureContribution Evaluate(
    in PlayerNpcPressureContributionInput input)
  {
    if (!float.IsFinite(input.NpcSlotCost) || input.NpcSlotCost < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(input.NpcSlotCost));
    }

    if (!input.IsNpcActive ||
      !input.IsPlayerInActiveRange ||
      input.LifeMaximum <= 0 ||
      input.ReleaseOwner != UnownedReleaseOwner ||
      IsExcludedNpcType(input.NpcType))
    {
      return new PlayerNpcPressureContribution(false, 0f);
    }

    float multiplier = input.IsSlimeRainActive && input.IsSlimeRainNpc
      ? SlimeRainNpcSlotMultiplier
      : 1f;
    return new PlayerNpcPressureContribution(
      true,
      input.NpcSlotCost * multiplier);
  }

  public static float SumContributions(
    IReadOnlyList<PlayerNpcPressureContributionInput> inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs);

    float total = 0f;
    foreach (PlayerNpcPressureContributionInput input in inputs)
    {
      total += Evaluate(input).SlotWeight;
    }

    return total;
  }

  private static bool IsExcludedNpcType(int npcType)
  {
    return npcType is 25 or 30 or 33;
  }
}
