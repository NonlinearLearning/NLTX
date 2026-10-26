using System;

namespace Terraria.Liquid;

[Flags]
public enum LiquidContactFlags : byte
{
  None = 0,
  Water = 1,
  Honey = 2,
  Lava = 4,
  Shimmer = 8
}
