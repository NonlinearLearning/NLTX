using System.Numerics;

namespace Terraria.Ai.Goap.Core;

/// <summary>
/// Project adapter for one living ECS entity. Observations and targets must be stable for one tick;
/// the host validates entity lifetime and scope before commands. StopMoving must be idempotent.
/// </summary>
public interface IAgentEnvironment {
  Vector3 Position { get; }
  IWorldData WorldData { get; }
  ITarget? GetTarget(IGoapAction action);
  void Move(ITarget target);
  void StopMoving();
  void LogWarning(string message);
}
