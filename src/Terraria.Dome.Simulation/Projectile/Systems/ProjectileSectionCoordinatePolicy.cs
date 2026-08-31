using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileSectionCoordinatePolicy
{
  public static WorldSectionCoordinates Resolve(
    SimulationVector position,
    int worldWidth,
    int worldHeight)
  {
    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(position));
    }

    if (worldWidth <= 0 || worldHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    int tileX = Math.Clamp((int)MathF.Floor(position.X), 0, worldWidth - 1);
    int tileY = Math.Clamp((int)MathF.Floor(position.Y), 0, worldHeight - 1);
    return new WorldSectionCoordinates(
      tileX / WorldGrid.SectionWidth,
      tileY / WorldGrid.SectionHeight);
  }
}
