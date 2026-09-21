using System.Numerics;

namespace Terraria.Player;

public sealed class PlayerNetworkCameraStateSystem
{
  private const float OffsetDecayFraction = 0.1f;
  private const float MinimumOffsetLength = 2f;

  public PlayerCameraSnapshot Update(
    PlayerNetworkCameraStateComponent component,
    in PlayerNetworkCameraInputSnapshot input)
  {
    ArgumentNullException.ThrowIfNull(component);

    if (input.ClearCameraTarget)
    {
      component.SetCameraTarget(null);
    }
    else if (input.CameraTarget.HasValue)
    {
      component.SetCameraTarget(input.CameraTarget);
    }

    if (input.FakeNetOffset.HasValue)
    {
      component.SetNetOffset(input.FakeNetOffset.Value);
    }
    else
    {
      component.SetNetOffset(
        CalculateUpdatedOffset(
          component.NetOffset,
          input.Velocity,
          input.CollisionAdjustedVelocity,
          input.IsGhost));
    }

    return component.ToSnapshot(input.Tick);
  }

  public void MarkCameraTargetSynchronized(
    PlayerNetworkCameraStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.SetLastSyncedCameraTarget(component.NetCameraTarget);
  }

  public PlayerCameraSnapshot ResetForSpawn(
    PlayerNetworkCameraStateComponent component,
    SimulationTick tick)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
    return component.ToSnapshot(tick);
  }

  public PlayerCameraSnapshot ResetForTeleport(
    PlayerNetworkCameraStateComponent component,
    SimulationTick tick)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.SetNetOffset(Vector2.Zero);
    return component.ToSnapshot(tick);
  }

  private static Vector2 CalculateUpdatedOffset(
    Vector2 netOffset,
    Vector2 velocity,
    Vector2 collisionAdjustedVelocity,
    bool isGhost)
  {
    float offsetLength = netOffset.Length();
    if (offsetLength < MinimumOffsetLength)
    {
      return Vector2.Zero;
    }

    float minimumMoveDistance = MinimumOffsetLength;
    if (!isGhost && collisionAdjustedVelocity != velocity)
    {
      Vector2 velocityDifference = velocity - collisionAdjustedVelocity;
      float collisionProjection = Vector2.Dot(velocityDifference, netOffset);
      if (collisionProjection >= 1f)
      {
        float collisionMoveDistance =
          velocityDifference.LengthSquared() * offsetLength / collisionProjection * 1.0001f;
        minimumMoveDistance = Math.Max(minimumMoveDistance, collisionMoveDistance);
      }
    }

    float maximumMoveDistance =
      Math.Max(minimumMoveDistance, offsetLength * OffsetDecayFraction);
    if (offsetLength <= maximumMoveDistance)
    {
      return Vector2.Zero;
    }

    return netOffset - netOffset / offsetLength * maximumMoveDistance;
  }
}
