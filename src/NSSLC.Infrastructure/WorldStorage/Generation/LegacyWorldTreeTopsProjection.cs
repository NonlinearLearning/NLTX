using System;
using Terraria.NonAuthoritative.Persistence;
using NSSLC.WorldGeneration.GameContent;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Publishes the loaded TreeTops owner into the legacy WorldGen reader.</summary>
public static class LegacyWorldTreeTopsProjection
{
  public static WorldStorageOperationResult Publish(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!session.IsPublished)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "Tree tops can only be projected from a published world session."));
    }

    TreeTopsInfo? runtimeTreeTops = NSSLC.WorldGeneration.WorldGen.TreeTops;
    if (runtimeTreeTops is null ||
        session.TreeTops.AreaCount != TreeTopsInfo.AreaId.Count)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The loaded TreeTops state does not match the legacy world area catalog."));
    }

    int[] styles = new int[session.TreeTops.AreaCount];
    for (int areaId = 0; areaId < styles.Length; areaId++)
    {
      styles[areaId] = session.TreeTops.GetTreeStyle(areaId);
    }

    runtimeTreeTops.ApplyStyles(styles);
    return WorldStorageOperationResult.Success;
  }
}
