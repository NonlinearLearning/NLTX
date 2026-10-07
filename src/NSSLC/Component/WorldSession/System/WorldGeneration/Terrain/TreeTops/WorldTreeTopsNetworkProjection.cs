namespace Terraria.WorldGeneration.Terrain.TreeTops;

public static class WorldTreeTopsNetworkProjection
{
  public static byte[] CreateSyncPayload(WorldTreeTopsStateComponent state)
  {
    WorldTreeTopsStateSnapshot snapshot = state.CreateSnapshot();
    byte[] payload = new byte[snapshot.Variations.Count];
    for (int areaId = 0; areaId < snapshot.Variations.Count; areaId++)
    {
      payload[areaId] = unchecked((byte)snapshot.Variations[areaId]);
    }

    return payload;
  }
}
