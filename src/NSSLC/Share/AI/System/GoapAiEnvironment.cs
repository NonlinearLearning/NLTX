using System;
using System.Numerics;
using Terraria.Share.Ai.Goap.Core;
using Terraria.Share.Ai.Goap.Runtime;

namespace Terraria.Share.Ai;

/// <summary>
/// Snapshot-backed GOAP environment. Move/Stop and domain action methods must only emit intents.
/// It must not hold an ECS World, mutable component references or authoritative services.
/// </summary>
public abstract class GoapAiEnvironment<TIntent> : IAgentEnvironment where TIntent : unmanaged {
  private AiOutput<TIntent>? _currentOutput;

  public abstract Vector3 Position { get; }
  public WorldData WorldData { get; } = new();
  IWorldData IAgentEnvironment.WorldData => WorldData;

  protected AiOutput<TIntent> _output => _currentOutput ??
      throw new InvalidOperationException("The GOAP environment has no active AI operation.");

  public abstract ITarget? GetTarget(IGoapAction action);
  public abstract void Move(ITarget target);
  public abstract void StopMoving();

  public void LogWarning(string message) {
    _output.Warn(message);
  }

  protected abstract void ReadSnapshot(AiSnapshot snapshot, float deltaTime);

  internal void Begin(AiOutput<TIntent> output) {
    _currentOutput = output;
  }

  internal void Refresh(AiSnapshot snapshot, float deltaTime) {
    // Facts are observations, not accumulated predictions from earlier planning passes.
    WorldData.Clear();
    ReadSnapshot(snapshot, deltaTime);
  }

  internal void End() {
    _currentOutput = null;
  }
}
