using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Wiring.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class LampCommandSystem
{
  public IReadOnlyList<TileChangeCommand> CreateCommands(
    LampComponent lamp,
    MechanismActivationCommand activation,
    long firstSequence)
  {
    ArgumentNullException.ThrowIfNull(lamp);
    if (activation.MechanismId != lamp.MechanismId || firstSequence < 0)
    {
      return [];
    }

    bool isLit = activation.Kind switch
    {
      MechanismActivationKind.Activate or MechanismActivationKind.Open => true,
      MechanismActivationKind.Close => false,
      MechanismActivationKind.Toggle => !lamp.IsLit,
      _ => lamp.IsLit
    };
    lamp.SetLit(isLit);
    WiringTileCoordinate tile = lamp.Tile;
    return [new TileChangeCommand(
      firstSequence,
      tile.X,
      tile.Y,
      TileChangeKind.Place,
      isLit ? lamp.LitTileType : lamp.UnlitTileType)];
  }
}
