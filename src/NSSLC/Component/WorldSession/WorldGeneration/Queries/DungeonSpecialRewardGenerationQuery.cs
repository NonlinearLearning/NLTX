using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class DungeonSpecialRewardGenerationQuery
{
  public static DungeonSpecialRewardGenerationSnapshot Snapshot(
    DungeonSpecialRewardGenerationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
