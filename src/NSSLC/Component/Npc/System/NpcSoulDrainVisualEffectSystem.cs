using System;
using System.Numerics;
using Terraria.Npc.Queries;

namespace Terraria.Npc;

public static class NpcSoulDrainVisualEffectSystem
{
  private const int DustChanceDenominator = 3;
  private const int DustWidth = 1;
  private const int DustHeight = 1;
  private const int SoulDrainDustType = 235;
  private const int PositionJitterMinimum = -100;
  private const int PositionJitterMaximum = 100;
  private const float PositionJitterScale = 0.05f;
  private const int DustScaleMinimumPercent = 70;
  private const int DustScaleMaximumPercent = 85;
  private const float DustScaleMultiplier = 0.01f;

  public static NpcSoulDrainVisualEffectResult Apply(
    in NpcSoulDrainVisualEffectInput input,
    INpcSoulDrainVisualEffectPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    NpcSoulDrainEligibilityResult eligibility =
      NpcSoulDrainEligibilityQuery.Evaluate(input.Eligibility);
    if (!eligibility.Succeeded)
    {
      return new NpcSoulDrainVisualEffectResult(
        Succeeded: false,
        EligibilityFailureReason: eligibility.FailureReason,
        EligiblePlayerCount: 0,
        DustCreatedCount: 0);
    }

    int dustCreatedCount = 0;
    for (int index = 0; index < eligibility.EligibleLegacyPlayerSlots.Count; index++)
    {
      int playerSlot = eligibility.EligibleLegacyPlayerSlots[index];
      if (Next(port, 0, DustChanceDenominator) == 0)
      {
        continue;
      }

      Vector2 position = input.Eligibility.NpcCenter;
      position.X += Next(port, PositionJitterMinimum, PositionJitterMaximum) *
        PositionJitterScale;
      position.Y += Next(port, PositionJitterMinimum, PositionJitterMaximum) *
        PositionJitterScale;
      position += input.NpcVelocity;

      int dustIndex = port.NewDust(position, DustWidth, DustHeight, SoulDrainDustType);
      port.SetDustVelocity(dustIndex, Vector2.Zero);
      float scale = Next(port, DustScaleMinimumPercent, DustScaleMaximumPercent) *
        DustScaleMultiplier;
      port.SetDustScale(dustIndex, scale);
      port.SetDustFadeIn(dustIndex, playerSlot + 1);
      dustCreatedCount++;
    }

    return new NpcSoulDrainVisualEffectResult(
      Succeeded: true,
      EligibilityFailureReason: NpcSoulDrainEligibilityFailureReason.None,
      EligiblePlayerCount: eligibility.EligibleLegacyPlayerSlots.Count,
      DustCreatedCount: dustCreatedCount);
  }

  private static int Next(
    INpcSoulDrainVisualEffectPort port,
    int minimumInclusive,
    int maximumExclusive)
  {
    int value = port.Next(minimumInclusive, maximumExclusive);
    if (value < minimumInclusive || value >= maximumExclusive)
    {
      throw new InvalidOperationException(
        "The Soul Drain effect port returned a value outside its requested range.");
    }

    return value;
  }
}
