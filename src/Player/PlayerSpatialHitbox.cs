namespace Terraria.Player;

public readonly record struct PlayerSpatialHitbox(
  int X,
  int Y,
  int Width,
  int Height)
{
  public PlayerSpatialHitbox Inflate(int horizontal, int vertical)
  {
    return new PlayerSpatialHitbox(
      X - horizontal,
      Y - vertical,
      Width + horizontal * 2,
      Height + vertical * 2);
  }
}
