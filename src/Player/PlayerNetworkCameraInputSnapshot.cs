using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerNetworkCameraInputSnapshot(
  SimulationTick Tick,
  Vector2 Velocity,
  Vector2 CollisionAdjustedVelocity,
  bool IsGhost = false,
  Vector2? FakeNetOffset = null,
  Vector2? CameraTarget = null,
  bool ClearCameraTarget = false);
