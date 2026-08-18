using System;
using Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

namespace Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

public sealed class ChestCloseSystem
{
  public bool TryApply(ChestComponent chest, ChestCloseCommand command)
  {
    ArgumentNullException.ThrowIfNull(chest);
    if (command.Sequence < 0 || chest.Opener != command.Player)
    {
      return false;
    }

    chest.Close(command.Player);
    chest.IncrementRevision();
    return true;
  }
}
