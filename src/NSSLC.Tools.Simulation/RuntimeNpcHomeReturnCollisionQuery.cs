using Terraria.WorldStorage;
using Terraria.Npc;
using Terraria.NonAuthoritative.Persistence;
using RuntimeMain = NSSLC.WorldGeneration.Main;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimeNpcHomeReturnCollisionQuery : INpcHomeReturnCollisionQuery
{
  private const ushort ActiveTileFlag = 0x20;
  private const ushort InactiveTileFlag = 0x40;

  private readonly LoadedWorldSession _session;

  public RuntimeNpcHomeReturnCollisionQuery(LoadedWorldSession session)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
  }

  public bool IsSolidTile(int tileX, int tileY)
  {
    if (tileX < 0 ||
        tileX >= _session.Storage.TileMap.Width ||
        tileY < 0 ||
        tileY >= _session.Storage.TileMap.Height - 40)
    {
      return true;
    }

    TileCellState tile = _session.Storage.TileMap.GetTile(tileX, tileY);
    if ((tile.TileHeader & ActiveTileFlag) == 0 ||
        (tile.TileHeader & InactiveTileFlag) != 0 ||
        tile.Type >= RuntimeMain.tileSolid.Length ||
        tile.Type >= RuntimeMain.tileSolidTop.Length)
    {
      return false;
    }

    return RuntimeMain.tileSolid[tile.Type] && !RuntimeMain.tileSolidTop[tile.Type];
  }
}
