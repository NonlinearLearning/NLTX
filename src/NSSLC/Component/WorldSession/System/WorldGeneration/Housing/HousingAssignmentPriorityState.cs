using System;

namespace Terraria.WorldGeneration.Housing;

public sealed class HousingAssignmentPriorityState
{
  public HousingAssignmentPriorityState(int? prioritizedTownNpcType = null)
  {
    Replace(prioritizedTownNpcType);
  }

  public int? PrioritizedTownNpcType { get; private set; }

  public bool HasPriority => PrioritizedTownNpcType.HasValue;

  public void Replace(int? prioritizedTownNpcType)
  {
    if (prioritizedTownNpcType is int npcType && npcType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(prioritizedTownNpcType));
    }

    PrioritizedTownNpcType = prioritizedTownNpcType;
  }

  public void Clear()
  {
    PrioritizedTownNpcType = null;
  }
}
