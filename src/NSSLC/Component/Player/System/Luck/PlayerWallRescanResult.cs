namespace Terraria.Player.Luck;

public readonly record struct PlayerWallRescanResult(
  bool DidScan,
  bool WasInsideUnbreakableWalls,
  bool IsInsideUnbreakableWalls)
{
  public bool Changed =>
    DidScan && WasInsideUnbreakableWalls != IsInsideUnbreakableWalls;
}
