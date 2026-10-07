using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;

namespace NSSLC.WorldGeneration;

/// <summary>
/// Holds the persisted bestiary state projected for the active world session.
/// Gameplay tracking and network publication remain host-owned.
/// </summary>
public sealed class WorldBestiaryTracker
{
  private sealed class PublishedState
  {
    public static readonly PublishedState Empty = CreateEmpty();

    public WorldBestiaryTrackerSnapshot Snapshot { get; }
    public HashSet<string> SeenNpcIds { get; }
    public HashSet<string> ChattedNpcIds { get; }

    public PublishedState(
      WorldBestiaryTrackerSnapshot snapshot,
      HashSet<string> seenNpcIds,
      HashSet<string> chattedNpcIds)
    {
      Snapshot = snapshot;
      SeenNpcIds = seenNpcIds;
      ChattedNpcIds = chattedNpcIds;
    }

    private static PublishedState CreateEmpty()
    {
      var emptyKillCounts = new ReadOnlyDictionary<string, int>(
        new Dictionary<string, int>(StringComparer.Ordinal));
      return new PublishedState(
        new WorldBestiaryTrackerSnapshot(
          emptyKillCounts,
          Array.Empty<string>(),
          Array.Empty<string>()),
        new HashSet<string>(StringComparer.Ordinal),
        new HashSet<string>(StringComparer.Ordinal));
    }
  }

  private PublishedState _state = PublishedState.Empty;

  public WorldBestiaryTrackerSnapshot Snapshot => Volatile.Read(ref _state).Snapshot;

  public void Publish(
    IReadOnlyDictionary<string, int> killCounts,
    IReadOnlyList<string> seenNpcIds,
    IReadOnlyList<string> chattedNpcIds)
  {
    ArgumentNullException.ThrowIfNull(killCounts);
    ArgumentNullException.ThrowIfNull(seenNpcIds);
    ArgumentNullException.ThrowIfNull(chattedNpcIds);

    var copiedKillCounts = new Dictionary<string, int>(
      killCounts.Count,
      StringComparer.Ordinal);
    foreach (KeyValuePair<string, int> entry in killCounts)
    {
      if (entry.Key is null || entry.Value < 0 ||
          !copiedKillCounts.TryAdd(entry.Key, entry.Value))
      {
        throw new InvalidDataException("Invalid bestiary kill count state.");
      }
    }

    HashSet<string> seen = CopyIdentifiers(seenNpcIds, out string[] seenIds);
    HashSet<string> chatted = CopyIdentifiers(chattedNpcIds, out string[] chattedIds);
    var snapshot = new WorldBestiaryTrackerSnapshot(
      new ReadOnlyDictionary<string, int>(copiedKillCounts),
      Array.AsReadOnly(seenIds),
      Array.AsReadOnly(chattedIds));
    Volatile.Write(ref _state, new PublishedState(snapshot, seen, chatted));
  }

  public void Reset()
  {
    Volatile.Write(ref _state, PublishedState.Empty);
  }

  public int GetKillCount(string persistentId)
  {
    ArgumentNullException.ThrowIfNull(persistentId);
    return Snapshot.KillCounts.TryGetValue(persistentId, out int count) ? count : 0;
  }

  public bool GetWasNearbyBefore(string persistentId)
  {
    ArgumentNullException.ThrowIfNull(persistentId);
    return Volatile.Read(ref _state).SeenNpcIds.Contains(persistentId);
  }

  public bool GetWasChatWith(string persistentId)
  {
    ArgumentNullException.ThrowIfNull(persistentId);
    return Volatile.Read(ref _state).ChattedNpcIds.Contains(persistentId);
  }

  private static HashSet<string> CopyIdentifiers(
    IReadOnlyList<string> source,
    out string[] orderedIds)
  {
    var identifiers = new HashSet<string>(StringComparer.Ordinal);
    var ordered = new List<string>(source.Count);
    for (int index = 0; index < source.Count; index++)
    {
      string identifier = source[index] ??
        throw new InvalidDataException("A bestiary identifier cannot be null.");
      if (identifiers.Add(identifier))
      {
        ordered.Add(identifier);
      }
    }

    orderedIds = ordered.ToArray();
    return identifiers;
  }
}

public sealed class WorldBestiaryTrackerSnapshot
{
  public IReadOnlyDictionary<string, int> KillCounts { get; }
  public IReadOnlyList<string> SeenNpcIds { get; }
  public IReadOnlyList<string> ChattedNpcIds { get; }

  internal WorldBestiaryTrackerSnapshot(
    IReadOnlyDictionary<string, int> killCounts,
    IReadOnlyList<string> seenNpcIds,
    IReadOnlyList<string> chattedNpcIds)
  {
    KillCounts = killCounts;
    SeenNpcIds = seenNpcIds;
    ChattedNpcIds = chattedNpcIds;
  }
}
