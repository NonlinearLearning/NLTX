using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Wiring.Components;

public sealed class ActuatorComponent
{
  public ActuatorComponent(
    int mechanismId,
    IReadOnlyList<WiringTileCoordinate> tiles,
    ushort tileType = 1)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    MechanismId = mechanismId;
    TileType = tileType;
    Tiles = [.. tiles];
  }

  public bool IsEnabled { get; private set; }
  public int MechanismId { get; }
  public ushort TileType { get; }
  public IReadOnlyList<WiringTileCoordinate> Tiles { get; }

  public void SetEnabled(bool enabled)
  {
    IsEnabled = enabled;
  }
}
