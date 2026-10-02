using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class DungeonSpecialRewardGenerationSystem
{
  public static void Commit(
    DungeonSpecialRewardGenerationComponent component,
    in DungeonSpecialRewardGenerationSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Dungeon reward results cannot be committed to another generation.",
        nameof(snapshot));
    }

    component.ReplaceState(
      snapshot.GeneratedShadowKey,
      snapshot.GeneratedRamRune);
  }
}
