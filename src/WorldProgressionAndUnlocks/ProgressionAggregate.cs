using System;
using System.Collections.Generic;

namespace Terraria.WorldProgressionAndUnlocks;

public sealed class ProgressionAggregate
{
  public const int MaxKillCount = 999_999_999;

  public ProgressionAggregate()
  {
    KillCountsByPersistentId = new Dictionary<string, int>(StringComparer.Ordinal);
    SightedPersistentIds = new HashSet<string>(StringComparer.Ordinal);
    ChattedPersistentIds = new HashSet<string>(StringComparer.Ordinal);
  }

  public Dictionary<string, int> KillCountsByPersistentId { get; }

  public HashSet<string> SightedPersistentIds { get; }

  public HashSet<string> ChattedPersistentIds { get; }
}
