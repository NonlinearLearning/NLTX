namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A saved town NPC room assignment.
/// </summary>
public sealed class WorldFileTownRoomRecord
{
  public WorldFileTownRoomRecord(int npcType, int tileX, int tileY)
  {
    NpcType = npcType;
    TileX = tileX;
    TileY = tileY;
  }

  public int NpcType { get; }

  public int TileX { get; }

  public int TileY { get; }
}
