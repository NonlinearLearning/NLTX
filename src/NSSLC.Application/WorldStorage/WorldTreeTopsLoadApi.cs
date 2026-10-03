using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldGeneration.Terrain.TreeTops;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi(
  "world-generation.tree-tops.load",
  OwnerId,
  WorldFileTreeTopsSection.SectionId,
  MinimumSupportedFormatVersion,
  MaximumSupportedFormatVersion,
  WorldLoadSectionRequirement.Optional)]
public sealed class WorldTreeTopsLoadApi :
  IWorldLoadApi<WorldTreeTopsStateComponent, WorldFileTreeTopsSection, IReadOnlyList<int>>
{
  public const string OwnerId = "world-generation.tree-tops";

  private const int MinimumSupportedFormatVersion = 211;
  private const int MaximumSupportedFormatVersion = 326;

  public WorldLoadPrepareResult<IReadOnlyList<int>> PrepareLoad(
    WorldTreeTopsStateComponent ownerContext,
    WorldLoadSection<WorldFileTreeTopsSection> section)
  {
    ArgumentNullException.ThrowIfNull(ownerContext);
    if (!section.IsPresent)
    {
      return WorldLoadPrepareResult<IReadOnlyList<int>>.Prepared(Array.Empty<int>());
    }

    IReadOnlyList<int> source = section.Value.Variations;
    int count = Math.Min(source.Count, ownerContext.AreaCount);
    int[] variations = new int[count];
    for (int index = 0; index < count; index++)
    {
      variations[index] = source[index];
    }

    return WorldLoadPrepareResult<IReadOnlyList<int>>.Prepared(
      Array.AsReadOnly(variations));
  }

  public WorldLoadCommitResult CommitLoad(
    WorldTreeTopsStateComponent ownerContext,
    in IReadOnlyList<int> preparedData)
  {
    ArgumentNullException.ThrowIfNull(ownerContext);
    ArgumentNullException.ThrowIfNull(preparedData);

    WorldTreeTopsSystem.ApplyStyles(ownerContext, preparedData);
    return WorldLoadCommitResult.Committed();
  }

  public void DiscardPrepared(
    WorldTreeTopsStateComponent ownerContext,
    in IReadOnlyList<int> preparedData)
  {
    ArgumentNullException.ThrowIfNull(ownerContext);
    ArgumentNullException.ThrowIfNull(preparedData);
  }
}
