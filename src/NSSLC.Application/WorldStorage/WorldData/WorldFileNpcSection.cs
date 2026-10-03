using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// NPC records decoded from the pointer-based WorldFile NPC section.
/// </summary>
public sealed class WorldFileNpcSection
{
  public const string SectionId = "world.npcs";

  public WorldFileNpcSection(
    IReadOnlyList<int> shimmeredTownNpcIds,
    IReadOnlyList<WorldFileNpcRecord> townNpcs,
    IReadOnlyList<WorldFileNpcRecord> savedNpcs)
  {
    ArgumentNullException.ThrowIfNull(shimmeredTownNpcIds);
    ArgumentNullException.ThrowIfNull(townNpcs);
    ArgumentNullException.ThrowIfNull(savedNpcs);
    if (shimmeredTownNpcIds.Count > 10000 ||
        townNpcs.Count > 10000 ||
        savedNpcs.Count > 10000)
    {
      throw new ArgumentOutOfRangeException(nameof(townNpcs));
    }

    ShimmeredTownNpcIds = Array.AsReadOnly(new List<int>(shimmeredTownNpcIds).ToArray());
    TownNpcs = CopyRecords(townNpcs, nameof(townNpcs), expectedTownNpc: true);
    SavedNpcs = CopyRecords(savedNpcs, nameof(savedNpcs), expectedTownNpc: false);
  }

  public IReadOnlyList<int> ShimmeredTownNpcIds { get; }

  public IReadOnlyList<WorldFileNpcRecord> TownNpcs { get; }

  public IReadOnlyList<WorldFileNpcRecord> SavedNpcs { get; }

  public static WorldFileNpcSection Empty => new(
    Array.Empty<int>(),
    Array.Empty<WorldFileNpcRecord>(),
    Array.Empty<WorldFileNpcRecord>());

  private static IReadOnlyList<WorldFileNpcRecord> CopyRecords(
    IReadOnlyList<WorldFileNpcRecord> records,
    string parameterName,
    bool expectedTownNpc)
  {
    List<WorldFileNpcRecord> copiedRecords = new(records.Count);
    for (int index = 0; index < records.Count; index++)
    {
      WorldFileNpcRecord record = records[index] ?? throw new ArgumentException(
        "An NPC section cannot contain a null record.", parameterName);
      if (record.IsTownNpc != expectedTownNpc)
      {
        throw new ArgumentException(
          "An NPC record does not match its town or saved list.", parameterName);
      }

      copiedRecords.Add(record);
    }

    return Array.AsReadOnly(copiedRecords.ToArray());
  }
}
