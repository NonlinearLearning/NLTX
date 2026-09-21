using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Components;

public sealed class LampComponent
{
  public LampComponent(
    int mechanismId,
    WiringTileCoordinate tile,
    ushort litTileType,
    ushort unlitTileType)
  {
    if (mechanismId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mechanismId));
    }

    MechanismId = mechanismId;
    Tile = tile;
    LitTileType = litTileType;
    UnlitTileType = unlitTileType;
  }

  public bool IsLit { get; private set; }
  public ushort LitTileType { get; }
  public int MechanismId { get; }
  public WiringTileCoordinate Tile { get; }
  public ushort UnlitTileType { get; }

  public void SetLit(bool isLit)
  {
    IsLit = isLit;
  }
}
