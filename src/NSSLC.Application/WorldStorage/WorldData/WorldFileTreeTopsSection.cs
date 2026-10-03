using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Immutable TreeTops variation values persisted by a WorldFile.
/// </summary>
public sealed class WorldFileTreeTopsSection
{
  public const string SectionId = "world.tree-tops";

  public WorldFileTreeTopsSection(IReadOnlyList<int> variations)
  {
    ArgumentNullException.ThrowIfNull(variations);
    Variations = Array.AsReadOnly(new List<int>(variations).ToArray());
  }

  public IReadOnlyList<int> Variations { get; }

  public static WorldFileTreeTopsSection Empty => new(Array.Empty<int>());
}
