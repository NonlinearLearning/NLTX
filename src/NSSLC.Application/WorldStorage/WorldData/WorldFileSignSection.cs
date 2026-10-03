using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Sign records decoded from the WorldFile section after chests.
/// </summary>
public sealed class WorldFileSignSection
{
  public const string SectionId = "world.signs";

  public WorldFileSignSection(IReadOnlyList<WorldFileSignRecord> signs)
  {
    ArgumentNullException.ThrowIfNull(signs);
    if (signs.Count > 32000)
    {
      throw new ArgumentOutOfRangeException(nameof(signs));
    }

    List<WorldFileSignRecord> copiedSigns = new(signs.Count);
    for (int index = 0; index < signs.Count; index++)
    {
      copiedSigns.Add(signs[index] ?? throw new ArgumentException(
        "A sign section cannot contain a null record.", nameof(signs)));
    }

    Signs = Array.AsReadOnly(copiedSigns.ToArray());
  }

  public IReadOnlyList<WorldFileSignRecord> Signs { get; }

  public static WorldFileSignSection Empty => new(Array.Empty<WorldFileSignRecord>());
}
