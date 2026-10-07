using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerSpectatingCameraTargetSnapshot(
  Vector2 Bottom,
  float GfxOffY,
  Vector2 NetOffset);
