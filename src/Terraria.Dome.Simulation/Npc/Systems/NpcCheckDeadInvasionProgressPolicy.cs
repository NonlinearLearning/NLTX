using System;
using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckDeadInvasionProgressPolicy
{
  public const int Version1456NpcTypeCount = 697;

  public static NpcCheckDeadInvasionProgressDecision Evaluate(
    NpcCheckDeadInvasionProgressInput input)
  {
    Validate(input);
    int invasionGroup = LegacyNpcInvasionGroupRegistry.GetInvasionGroup(input.NpcType);
    int points = GetPoints(input.NpcType);
    if (invasionGroup <= 0 || invasionGroup != input.InvasionType ||
        input.InvasionSize <= 0 || points <= 0)
    {
      return new(false, invasionGroup, points, input.InvasionSize, input.InvasionSizeStart);
    }

    int remainingSize = Math.Max(0, input.InvasionSize - points);
    return new(
      true,
      invasionGroup,
      points,
      remainingSize,
      input.InvasionSizeStart - remainingSize);
  }

  private static int GetPoints(int npcType)
  {
    return npcType switch
    {
      216 => 5,
      395 or 491 or 471 => 10,
      472 or 387 => 0,
      _ => LegacyNpcInvasionGroupRegistry.GetInvasionGroup(npcType) > 0 ? 1 : 0
    };
  }

  private static void Validate(NpcCheckDeadInvasionProgressInput input)
  {
    if (input.NpcType < 0 || input.NpcType >= Version1456NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(input.NpcType));
    }

    if (input.InvasionType < 0 || input.InvasionSize < 0 || input.InvasionSizeStart < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input));
    }
  }
}
