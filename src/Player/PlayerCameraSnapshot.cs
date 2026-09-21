using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerCameraSnapshot(
  SimulationTick Tick,
  Vector2 NetOffset,
  Vector2? NetCameraTarget,
  Vector2? LastSyncedNetCameraTarget);
