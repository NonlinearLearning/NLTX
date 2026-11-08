using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Chest records decoded from the WorldFile section after the compressed Tile section.
/// </summary>
public sealed class WorldFileChestSection
{
  public const string SectionId = "world.chests";

  public WorldFileChestSection(IReadOnlyList<WorldFileChestRecord> chests)
  {
    ArgumentNullException.ThrowIfNull(chests);
    if (chests.Count > 8000)
    {
      throw new ArgumentOutOfRangeException(nameof(chests));
    }

    List<WorldFileChestRecord> copiedChests = new(chests.Count);
    for (int index = 0; index < chests.Count; index++)
    {
      copiedChests.Add(chests[index] ?? throw new ArgumentException(
        "A chest section cannot contain a null record.", nameof(chests)));
    }

    Chests = Array.AsReadOnly(copiedChests.ToArray());
  }

  public IReadOnlyList<WorldFileChestRecord> Chests { get; }

  public static WorldFileChestSection Empty => new(Array.Empty<WorldFileChestRecord>());
}
