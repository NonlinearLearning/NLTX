using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Server.Validation;

public sealed class ContainerInteractionValidator
{
  public bool IsOpenRequestAccepted(
    ChestSnapshot chest,
    PlayerSnapshot player,
    short assertedTileX,
    short assertedTileY,
    IReadOnlySet<WorldSectionCoordinates> visibleSections)
  {
    ArgumentNullException.ThrowIfNull(visibleSections);
    if (assertedTileX != chest.TileX || assertedTileY != chest.TileY ||
        !visibleSections.Contains(chest.Section))
    {
      return false;
    }

    float deltaX = player.Position.X - chest.TileX;
    float deltaY = player.Position.Y - chest.TileY;
    return deltaX * deltaX + deltaY * deltaY <= 6.0f * 6.0f;
  }
}
