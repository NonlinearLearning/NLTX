namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct TileFrameCommand(
  long Sequence,
  int X,
  int Y,
  short FrameX,
  short FrameY,
  bool? IsHalfBrick = null,
  byte? Slope = null);
