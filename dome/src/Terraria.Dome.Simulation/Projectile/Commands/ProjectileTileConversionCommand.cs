using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Projectile.Commands;

public readonly record struct ProjectileTileConversionCommand(
  int ProjectileType,
  int X,
  int Y,
  byte ConversionType,
  long Sequence,
  string Source = "projectile.tile-conversion")
{
  public bool IsValid(WorldGrid world)
  {
    return world.Contains(X, Y) &&
      ProjectileType is 69 or 70 or 621 &&
      ConversionType is 1 or 2 or 4 &&
      Sequence >= 0 &&
      !string.IsNullOrWhiteSpace(Source);
  }
}
