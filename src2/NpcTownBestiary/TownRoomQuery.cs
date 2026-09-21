namespace Terraria.NpcTownBestiary;

public static class TownRoomQuery
{
  public static bool HasRoom(TownRoomRegistryComponent registry, int npcType)
  {
    ArgumentNullException.ThrowIfNull(registry);
    return registry.RoomsByNpcType.ContainsKey(npcType);
  }

  public static bool TryGetRoom(
    TownRoomRegistryComponent registry,
    int npcType,
    out TownRoomTilePoint room)
  {
    ArgumentNullException.ThrowIfNull(registry);
    return registry.RoomsByNpcType.TryGetValue(npcType, out room);
  }
}
