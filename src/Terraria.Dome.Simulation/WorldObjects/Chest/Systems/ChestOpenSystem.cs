using System;
using Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

namespace Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

public sealed class ChestOpenSystem
{
  public bool TryApply(ChestComponent chest, ChestOpenCommand command)
  {
    ArgumentNullException.ThrowIfNull(chest);
    if (command.Sequence < 0 || !IsInRange(chest, command.PlayerPosition))
    {
      return false;
    }

    if (!chest.TryOpen(command.Player))
    {
      return false;
    }

    chest.IncrementRevision();
    return true;
  }

  private static bool IsInRange(ChestComponent chest, SimulationVector position)
  {
    float deltaX = position.X - chest.TileX;
    float deltaY = position.Y - chest.TileY;
    return deltaX * deltaX + deltaY * deltaY <= 6.0f * 6.0f;
  }
}
