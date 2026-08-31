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
  bool PreserveTileState = false,
  bool IsInactive = false,
  short? FrameX = null,
  short? FrameY = null,
  bool? IsHalfBrick = null,
  byte? Slope = null,
  int Priority = 0,
  string Source = "unspecified",
  long? ExpectedSectionVersion = null,
  bool? IsActive = null,
  byte? TileColor = null,
  byte? WallColor = null,
  bool? IsInvisibleBlock = null,
  bool? IsInvisibleWall = null,
  bool? IsFullbrightBlock = null,
  bool? IsFullbrightWall = null,
  bool IsCartTrack = false,
  int SourceLine = 0);
