using System;
using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckActiveKeepAlivePolicy
{
  private const int Type399 = 399;
  private const int NightKeepAliveTypeStart = 583;
  private const int NightKeepAliveTypeEnd = 585;

  public static NpcCheckActiveKeepAliveDecision Evaluate(
    NpcCheckActiveKeepAliveInput input)
  {
    Validate(input);
    if (!input.IsActive || !input.HasActivePlayer)
    {
      return new(false, false);
    }

    bool isNightKeepAliveType = input.NpcType is >= NightKeepAliveTypeStart and
      <= NightKeepAliveTypeEnd;
    bool shouldRefresh =
      (input.NpcType == Type399 && (input.Ai0 == 1.0f || input.Ai0 == 2.0f)) ||
      (isNightKeepAliveType && !input.IsDayTime && input.Ai2 == 0.0f);
    bool shouldKeepActive = input.IsBoss ||
      LegacyNpcCheckActiveKeepAliveRegistry.IsStaticKeepAliveType(input.NpcType) ||
      input.NpcType == Type399 ||
      (isNightKeepAliveType && !input.IsDayTime && input.Ai2 == 0.0f);

    return new(shouldKeepActive, shouldRefresh);
  }

  private static void Validate(NpcCheckActiveKeepAliveInput input)
  {
    if (input.NpcType < 0 || input.NpcType >= LegacyNpcCheckActiveKeepAliveRegistry.NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(input.NpcType));
    }

    if (!float.IsFinite(input.Ai0))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Ai0));
    }

    if (!float.IsFinite(input.Ai2))
    {
      throw new ArgumentOutOfRangeException(nameof(input.Ai2));
    }
  }
}
