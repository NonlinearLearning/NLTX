using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public static class LegacyNpcInactivityRegistry
{
  public const int NpcTypeCount = 697;

  private const int Type139 = 139;
  private const int Type139Companion = 134;
  private const int CompanionRangeStart = 552;
  private const int CompanionRangeEnd = 578;
  private const int CompanionRangeType = 548;
  private const int SlotCountingType = 668;

  private static readonly FrozenSet<int> _staticNeverDespawnNpcTypes =
    new HashSet<int>
    {
      8,
      9,
      11,
      12,
      14,
      15,
      36,
      40,
      41,
      88,
      89,
      90,
      91,
      92,
      96,
      97,
      99,
      100,
      113,
      114,
      115,
      118,
      119,
      128,
      129,
      130,
      131,
      134,
      135,
      136,
      246,
      247,
      248,
      249,
      263,
      267,
      328,
      379,
      380,
      392,
      393,
      394,
      396,
      397,
      398,
      400,
      422,
      437,
      438,
      439,
      440,
      488,
      492,
      493,
      507,
      517,
      548,
      549,
      551,
      564,
      565
    }.ToFrozenSet();

  private static readonly FrozenSet<int> _companionDependentNpcTypes =
    CreateCompanionDependentNpcTypes();

  public static int StaticNeverDespawnTypeCount => _staticNeverDespawnNpcTypes.Count;

  public static IReadOnlySet<int> StaticNeverDespawnNpcTypes => _staticNeverDespawnNpcTypes;

  public static bool DoesNotDespawnToInactivity(
    int npcType,
    IReadOnlySet<int> activeNpcTypes)
  {
    ValidateNpcType(npcType);
    ArgumentNullException.ThrowIfNull(activeNpcTypes);
    ValidateActiveNpcTypes(activeNpcTypes);

    if (_staticNeverDespawnNpcTypes.Contains(npcType))
    {
      return true;
    }

    if (npcType == Type139)
    {
      return activeNpcTypes.Contains(Type139Companion);
    }

    if (_companionDependentNpcTypes.Contains(npcType))
    {
      return activeNpcTypes.Contains(CompanionRangeType);
    }

    return false;
  }

  public static bool DoesNotDespawnToInactivityAndCountsNpcSlots(int npcType)
  {
    ValidateNpcType(npcType);
    return npcType == SlotCountingType;
  }

  private static FrozenSet<int> CreateCompanionDependentNpcTypes()
  {
    HashSet<int> types = new();
    for (int npcType = CompanionRangeStart; npcType <= CompanionRangeEnd; npcType++)
    {
      if (npcType is not 564 and not 565)
      {
        types.Add(npcType);
      }
    }

    return types.ToFrozenSet();
  }

  private static void ValidateNpcType(int npcType)
  {
    if (npcType < 0 || npcType >= NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(npcType));
    }
  }

  private static void ValidateActiveNpcTypes(IReadOnlySet<int> activeNpcTypes)
  {
    foreach (int activeNpcType in activeNpcTypes)
    {
      ValidateNpcType(activeNpcType);
    }
  }
}
