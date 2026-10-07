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
  WorldLoadSectionRequirement.Required)]
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
      return WorldLoadPrepareResult<IReadOnlyList<int>>.Rejected(
        WorldLoadApiFailure.Create(
          "MissingTreeTopsSection",
          "The world TreeTops section is required for this format version."));
    }

    IReadOnlyList<int> source = section.Value.Variations;
    if (source.Count > ownerContext.AreaCount)
    {
      return WorldLoadPrepareResult<IReadOnlyList<int>>.Rejected(
        WorldLoadApiFailure.Create(
          "InvalidTreeTopsCount",
          "The world TreeTops section contains more styles than world areas."));
    }

    int[] variations = new int[source.Count];
    for (int index = 0; index < variations.Length; index++)
    {
      int style = source[index];
      if (!WorldTreeTopsSystem.IsValidStyle(index, style))
      {
        return WorldLoadPrepareResult<IReadOnlyList<int>>.Rejected(
          WorldLoadApiFailure.Create(
            "InvalidTreeTopsStyle",
            $"The world TreeTops section contains invalid style {style} for area {index}."));
      }

      variations[index] = style;
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
