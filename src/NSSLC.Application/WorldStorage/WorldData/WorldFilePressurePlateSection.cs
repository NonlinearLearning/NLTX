using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Weighted pressure plate coordinates decoded from the WorldFile section.
/// </summary>
public sealed class WorldFilePressurePlateSection
{
  public const string SectionId = "world.pressure-plates";

  public WorldFilePressurePlateSection(IReadOnlyList<WorldFilePressurePlateRecord> plates)
  {
    ArgumentNullException.ThrowIfNull(plates);
    if (plates.Count > 100_000)
    {
      throw new ArgumentOutOfRangeException(nameof(plates));
    }

    List<WorldFilePressurePlateRecord> copiedPlates = new(plates.Count);
    for (int index = 0; index < plates.Count; index++)
    {
      copiedPlates.Add(plates[index] ?? throw new ArgumentException(
        "A pressure plate section cannot contain a null record.", nameof(plates)));
    }

    Plates = Array.AsReadOnly(copiedPlates.ToArray());
  }

  public IReadOnlyList<WorldFilePressurePlateRecord> Plates { get; }

  public static WorldFilePressurePlateSection Empty =>
    new(Array.Empty<WorldFilePressurePlateRecord>());
}
