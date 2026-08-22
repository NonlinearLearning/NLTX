using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Commands;

public readonly record struct TileChangeCommand(
  long Sequence,
  int X,
  int Y,
  TileChangeKind Kind,
  ushort TileType,
  ushort WallType = 0,
  bool PreserveLiquid = false,
  bool IsInactive = false,
  short? FrameX = null,
  short? FrameY = null,
  bool? IsHalfBrick = null,
  byte? Slope = null);
