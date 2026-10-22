using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Bestiary;

public sealed class BestiarySnapshot
{
  public BestiarySnapshot(
    MinimalImplementation.Contracts.CommitVersion version,
    IReadOnlyDictionary<string, int> killCounts,
    IReadOnlyCollection<string> seenNpcIds,
    IReadOnlyCollection<string> chattedNpcIds,
    MinimalImplementation.Contracts.CommitVersion lastP05CommitVersion = default)
  {
    if (!version.IsValid)
    {
      throw new ArgumentException(
        "A Bestiary snapshot requires a valid commit version.",
        nameof(version));
    }

    if (lastP05CommitVersion.Value < 0)
    {
      throw new ArgumentException(
        "A Bestiary snapshot cannot contain a negative P05 source version.",
        nameof(lastP05CommitVersion));
    }

    ArgumentNullException.ThrowIfNull(killCounts);
    ArgumentNullException.ThrowIfNull(seenNpcIds);
    ArgumentNullException.ThrowIfNull(chattedNpcIds);
    if (killCounts.Count > BestiaryState.MaximumEntryCount
      || seenNpcIds.Count > BestiaryState.MaximumEntryCount
      || chattedNpcIds.Count > BestiaryState.MaximumEntryCount)
    {
      throw new ArgumentException("A Bestiary snapshot exceeds the supported entry limit.");
    }

    foreach ((string persistentId, int count) in killCounts)
    {
      if (!BestiaryState.IsValidIdentifier(persistentId)
        || count < 0
        || count > BestiaryState.MaximumKillCount)
      {
        throw new ArgumentException("A Bestiary snapshot contains an invalid kill entry.");
      }
    }

    Version = version;
    LastP05CommitVersion = lastP05CommitVersion;
    KillCounts = new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(
      killCounts
        .OrderBy(entry => entry.Key, StringComparer.Ordinal)
        .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
    SeenNpcIds = CopyUniqueIdentifiers(seenNpcIds);
    ChattedNpcIds = CopyUniqueIdentifiers(chattedNpcIds);
  }

  public MinimalImplementation.Contracts.CommitVersion Version { get; }
  /// <summary>
  /// The last P05 source revision accepted by this snapshot. Value zero means that the source
  /// revision was not available in the persistence representation.
  /// </summary>
  public MinimalImplementation.Contracts.CommitVersion LastP05CommitVersion { get; }
  public IReadOnlyDictionary<string, int> KillCounts { get; }
  public IReadOnlyList<string> SeenNpcIds { get; }
  public IReadOnlyList<string> ChattedNpcIds { get; }

  public WorldFileBestiarySection ToPersistenceSection()
  {
    List<WorldFileBestiaryKillCount> killCounts = new(KillCounts.Count);
    foreach ((string persistentId, int count) in KillCounts)
    {
      killCounts.Add(new WorldFileBestiaryKillCount(persistentId, count));
    }

    return new WorldFileBestiarySection(killCounts, SeenNpcIds, ChattedNpcIds);
  }

  private static IReadOnlyList<string> CopyUniqueIdentifiers(
    IReadOnlyCollection<string> identifiers)
  {
    HashSet<string> uniqueIdentifiers = new(StringComparer.Ordinal);
    foreach (string identifier in identifiers)
    {
      if (!BestiaryState.IsValidIdentifier(identifier) || !uniqueIdentifiers.Add(identifier))
      {
        throw new ArgumentException(
          "A Bestiary snapshot contains an invalid or duplicated identifier.");
      }
    }

    return Array.AsReadOnly(uniqueIdentifiers.Order(StringComparer.Ordinal).ToArray());
  }
}
