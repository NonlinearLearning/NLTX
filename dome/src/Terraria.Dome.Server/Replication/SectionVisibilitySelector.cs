using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Replication;

public sealed class SectionVisibilitySelector
{
  public const int VisibleSectionColumns = 5;
  public const int VisibleSectionRows = 3;

  public IReadOnlyList<WorldSectionCoordinates> Select(WorldGrid world, PlayerSnapshot player)
  {
    ArgumentNullException.ThrowIfNull(world);

    int playerX = Math.Clamp((int)MathF.Floor(player.Position.X), 0, world.Width - 1);
    int playerY = Math.Clamp((int)MathF.Floor(player.Position.Y), 0, world.Height - 1);
    WorldSectionCoordinates center = world.GetSectionCoordinates(playerX, playerY);
    int sectionColumns = world.Width / WorldGrid.SectionWidth;
    int sectionRows = world.Height / WorldGrid.SectionHeight;
    int firstX = Math.Clamp(
      center.X - VisibleSectionColumns / 2,
      0,
      sectionColumns - VisibleSectionColumns);
    int firstY = Math.Clamp(
      center.Y - VisibleSectionRows / 2,
      0,
      sectionRows - VisibleSectionRows);
    List<WorldSectionCoordinates> coordinates = new(VisibleSectionColumns * VisibleSectionRows);
    for (int y = firstY; y < firstY + VisibleSectionRows; y++)
    {
      for (int x = firstX; x < firstX + VisibleSectionColumns; x++)
      {
        coordinates.Add(new WorldSectionCoordinates(x, y));
      }
    }

    return coordinates;
  }
}
