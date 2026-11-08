using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A saved bestiary kill count keyed by its persistent content identifier.
/// </summary>
public sealed class WorldFileBestiaryKillCount
{
  public WorldFileBestiaryKillCount(string persistentId, int count)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(persistentId);
    if (count < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    PersistentId = persistentId;
    Count = count;
  }

  public string PersistentId { get; }

  public int Count { get; }
}
