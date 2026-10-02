using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete jungle-region structure snapshot for its generation session.
/// </summary>
public static class JungleRegionStructureSystem
{
  public static void Commit(
    JungleRegionStructureComponent component,
    in JungleRegionStructureSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Jungle-region structure facts cannot be committed to another generation.",
        nameof(snapshot));
    }

    component.ReplaceState(
      snapshot.ExtraBastStatueCount,
      snapshot.ExtraBastStatueCountMax,
      snapshot.JungleOriginX,
      snapshot.JungleMinX,
      snapshot.JungleMaxX,
      snapshot.JungleHut,
      snapshot.MudWall);
  }
}
