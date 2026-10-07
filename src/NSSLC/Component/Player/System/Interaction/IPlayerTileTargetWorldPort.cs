namespace Terraria.Player.Interaction;

public interface IPlayerTileTargetWorldPort
{
  void EnsureTileExists(int tileX, int tileY);

  PlayerTileTargetTileFacts ReadTile(int tileX, int tileY);
}
