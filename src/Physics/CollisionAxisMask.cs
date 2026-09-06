using System;

namespace Terraria.Physics;

[Flags]
public enum CollisionAxisMask : byte
{
  None = 0,
  Horizontal = 1,
  Vertical = 2,
}
