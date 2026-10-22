using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Saved bestiary kill, sight and chat discovery identifiers.
/// </summary>
public sealed class WorldFileBestiarySection
{
  public const string SectionId = "world.bestiary";

  public WorldFileBestiarySection(
    IReadOnlyList<WorldFileBestiaryKillCount> killCounts,
    IReadOnlyList<string> seenNpcIds,
    IReadOnlyList<string> chattedNpcIds)
  {
    KillCounts = CopyKillCounts(killCounts);
    SeenNpcIds = CopyStrings(seenNpcIds, nameof(seenNpcIds));
    ChattedNpcIds = CopyStrings(chattedNpcIds, nameof(chattedNpcIds));
  }

  public IReadOnlyList<WorldFileBestiaryKillCount> KillCounts { get; }

  public IReadOnlyList<string> SeenNpcIds { get; }

  public IReadOnlyList<string> ChattedNpcIds { get; }

  public static WorldFileBestiarySection Empty => new(
    Array.Empty<WorldFileBestiaryKillCount>(),
    Array.Empty<string>(),
    Array.Empty<string>());

  private static IReadOnlyList<WorldFileBestiaryKillCount> CopyKillCounts(
    IReadOnlyList<WorldFileBestiaryKillCount> values)
  {
    ArgumentNullException.ThrowIfNull(values);
    if (values.Count > 100_000)
    {
      throw new ArgumentOutOfRangeException(nameof(values));
    }

    List<WorldFileBestiaryKillCount> copy = new(values.Count);
    for (int index = 0; index < values.Count; index++)
    {
      copy.Add(values[index] ?? throw new ArgumentException(
        "A bestiary section cannot contain a null kill count.", nameof(values)));
    }

    return Array.AsReadOnly(copy.ToArray());
  }

  private static IReadOnlyList<string> CopyStrings(
    IReadOnlyList<string> values,
    string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values);
    if (values.Count > 100_000)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    string[] copy = new string[values.Count];
    for (int index = 0; index < values.Count; index++)
    {
      copy[index] = values[index] ?? throw new ArgumentException(
        "A bestiary section cannot contain a null identifier.", parameterName);
    }

    return Array.AsReadOnly(copy);
  }
}
