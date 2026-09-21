using System;

namespace Terraria.Dome.Simulation.Physics.Components;

[Flags]
public enum CollisionAxisMask : byte
{
  None = 0,
  Horizontal = 1,
  Vertical = 2
}
