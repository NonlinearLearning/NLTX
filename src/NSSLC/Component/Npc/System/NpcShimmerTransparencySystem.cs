using System;

namespace Terraria.Npc;

public static class NpcShimmerTransparencySystem
{
  private const float ShimmerIncreasePerTick = 0.01f;
  private const float HitDecrease = 0.1f;
  private const float ImmuneDecayPerTick = 0.015f;
  private const float NormalDecayPerTick = 0.001f;

  public static NpcShimmerTransparencyResult Advance(
    NpcShimmerStateComponent state,
    in NpcShimmerTransparencyInput input)
  {
    ArgumentNullException.ThrowIfNull(state);

    float previousTransparency = state.Transparency;
    if (!input.CanDisplayBuffs)
    {
      return new NpcShimmerTransparencyResult(
        Applied: false,
        RequiredInputMissing: false,
        PreviousTransparency: previousTransparency,
        CurrentTransparency: previousTransparency);
    }

    float nextTransparency = previousTransparency;
    if (input.Shimmering)
    {
      nextTransparency += ShimmerIncreasePerTick;
      if (nextTransparency > 1.0f)
      {
        nextTransparency = 1.0f;
      }
    }
    else if (previousTransparency > 0.0f)
    {
      if (!input.IsImmuneToShimmeringBuff.HasValue)
      {
        return new NpcShimmerTransparencyResult(
          Applied: false,
          RequiredInputMissing: true,
          PreviousTransparency: previousTransparency,
          CurrentTransparency: previousTransparency);
      }

      if (input.JustHit)
      {
        nextTransparency -= HitDecrease;
      }

      nextTransparency -= input.IsImmuneToShimmeringBuff.Value
        ? ImmuneDecayPerTick
        : NormalDecayPerTick;
      if (nextTransparency < 0.0f)
      {
        nextTransparency = 0.0f;
      }
    }

    state.ApplyTransparency(nextTransparency);
    return new NpcShimmerTransparencyResult(
      Applied: true,
      RequiredInputMissing: false,
      PreviousTransparency: previousTransparency,
      CurrentTransparency: nextTransparency);
  }
}
