using System.Numerics;

namespace NLTX.PlayerInputGameplay.SmartCursor;

public readonly record struct SmartCursorUsageInfo(
  int PlayerSlot,
  int ItemType,
  Vector2 Mouse,
  Vector2 Position,
  Vector2 Center,
  int ScreenTargetX,
  int ScreenTargetY,
  int ReachableStartX,
  int ReachableEndX,
  int ReachableStartY,
  int ReachableEndY,
  int PaintLookup,
  int PaintCoatingLookup);
