using System.Numerics;

namespace Terraria.Player;

public sealed class PlayerNetworkCameraStateComponent
{
  public Vector2 NetOffset { get; private set; }

  public Vector2? NetCameraTarget { get; private set; }

  public Vector2? LastSyncedNetCameraTarget { get; private set; }

  internal void SetNetOffset(Vector2 netOffset)
  {
    NetOffset = netOffset;
  }

  internal void SetCameraTarget(Vector2? cameraTarget)
  {
    NetCameraTarget = cameraTarget;
  }

  internal void SetLastSyncedCameraTarget(Vector2? cameraTarget)
  {
    LastSyncedNetCameraTarget = cameraTarget;
  }

  internal void Reset()
  {
    NetOffset = Vector2.Zero;
    NetCameraTarget = null;
    LastSyncedNetCameraTarget = null;
  }

  public PlayerCameraSnapshot ToSnapshot(SimulationTick tick)
  {
    return new PlayerCameraSnapshot(
      tick,
      NetOffset,
      NetCameraTarget,
      LastSyncedNetCameraTarget);
  }
}
