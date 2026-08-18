using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldEventStartCommand(WorldEventKind Kind, long Sequence)
{
  public bool IsValid => Sequence >= 0 && Enum.IsDefined(Kind);
}
