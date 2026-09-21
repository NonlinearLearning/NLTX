namespace NLTX.PlayerInputGameplay.Player;

public sealed class PlayerSpawnPointComponent
{
  public int TileX { get; private set; }

  public int TileY { get; private set; }

  public bool IsSet { get; private set; }

  public void Set(int tileX, int tileY)
  {
    TileX = tileX;
    TileY = tileY;
    IsSet = true;
  }

  public void Clear()
  {
    TileX = 0;
    TileY = 0;
    IsSet = false;
  }
}
