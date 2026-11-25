using System;
using System.Collections.Generic;

namespace Terraria.Share.Ai;

/// <summary>
/// One runtime per binding. Callbacks may update private planning state and emit data only.
/// They must not capture a World, entity component references, I/O or authoritative services.
/// </summary>
public abstract class AiBehavior<TIntent> : IAiBehavior where TIntent : unmanaged {
  private readonly AiRequirements _requirements;
  private bool _claimed;

  public Type IntentType => typeof(TIntent);
  public IReadOnlyList<Type> RequiredComponents => _requirements.Components;

  protected AiBehavior(AiRequirements requirements) {
    ArgumentNullException.ThrowIfNull(requirements);
    _requirements = requirements;
  }

  protected abstract void CalculateCore(AiSnapshot snapshot, float deltaTime,
                                        AiOutput<TIntent> output);
  protected abstract void StopCore(AiOutput<TIntent> output);

  protected virtual void DisposeCore(AiOutput<TIntent> output) {
  }

  internal void Claim() {
    if (_claimed) {
      throw new InvalidOperationException(
          "A behavior factory must create a fresh runtime per bind.");
    }
    _claimed = true;
  }

  internal Type[] FindMissing(AiSnapshot snapshot) => _requirements.FindMissing(snapshot);

  internal void Calculate(AiSnapshot snapshot, float deltaTime, AiOutput<TIntent> output) {
    CalculateCore(snapshot, deltaTime, output);
  }

  internal void Stop(AiOutput<TIntent> output) => StopCore(output);

  internal void Dispose(AiOutput<TIntent> output) => DisposeCore(output);
}
