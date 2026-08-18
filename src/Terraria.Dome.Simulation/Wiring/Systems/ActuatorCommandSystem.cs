using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Wiring.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class ActuatorCommandSystem
{
  public IReadOnlyList<TileChangeCommand> CreateCommands(
    ActuatorComponent actuator,
    MechanismActivationCommand activation,
    long firstSequence)
  {
    ArgumentNullException.ThrowIfNull(actuator);
    if (activation.MechanismId != actuator.MechanismId || firstSequence < 0)
    {
      return [];
    }

    bool enabled = activation.Kind is MechanismActivationKind.Activate or MechanismActivationKind.Open;
    actuator.SetEnabled(enabled);
    List<TileChangeCommand> commands = new(actuator.Tiles.Count);
    for (int index = 0; index < actuator.Tiles.Count; index++)
    {
      WiringTileCoordinate tile = actuator.Tiles[index];
      commands.Add(new TileChangeCommand(
        firstSequence + index,
        tile.X,
        tile.Y,
        enabled ? TileChangeKind.Place : TileChangeKind.Kill,
        actuator.TileType));
    }

    return commands;
  }
}
