using System;
using Terraria.Dome.Simulation.Wiring.Commands;
using Terraria.Dome.Simulation.Wiring.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class WiringInputValidationSystem
{
  public bool TryValidate(
    WorldGrid world,
    WireNetworkComponent network,
    WiringInputCommand command,
    int maximumRadius)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(network);
    return command.Sequence >= 0 && maximumRadius >= 0 && world.Contains(command.X, command.Y) &&
      network.HasWire(command.X, command.Y, command.Color);
  }
}
