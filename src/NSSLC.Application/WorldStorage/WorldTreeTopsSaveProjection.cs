using System;
using Terraria.WorldGeneration.Terrain.TreeTops;

namespace Terraria.NonAuthoritative.Persistence;

public static class WorldTreeTopsSaveProjection
{
  public static WorldPersistenceSection CreateSection(WorldTreeTopsStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    WorldTreeTopsStateSnapshot snapshot = state.CreateSnapshot();
    var section = new WorldFileTreeTopsSection(snapshot.Variations);
    return WorldPersistenceSection.Create(WorldFileTreeTopsSection.SectionId, section);
  }
}
