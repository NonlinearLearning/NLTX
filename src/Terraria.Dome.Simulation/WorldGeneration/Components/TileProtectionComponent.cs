namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileProtectionComponent(
  int SpawnCenterX,
  int SpawnTopY,
  int HalfWidth,
  int Height)
{
  public bool IsProtected(int x, int y)
  {
    return HalfWidth >= 0 && Height >= 0 &&
      x >= SpawnCenterX - HalfWidth && x <= SpawnCenterX + HalfWidth &&
      y >= SpawnTopY && y <= SpawnTopY + Height;
  }
}
