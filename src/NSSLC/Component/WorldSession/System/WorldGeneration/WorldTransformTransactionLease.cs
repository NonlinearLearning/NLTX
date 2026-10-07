using System;
using System.Threading;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Completes one active world transformation at most once when disposed.
/// </summary>
public sealed class WorldTransformTransactionLease : IDisposable
{
  private WorldTransformTransactionComponent? _component;

  internal WorldTransformTransactionLease(WorldTransformTransactionComponent component)
  {
    _component = component ?? throw new ArgumentNullException(nameof(component));
  }

  public void Dispose()
  {
    WorldTransformTransactionComponent? component =
      Interlocked.Exchange(ref _component, null);
    component?.End();
  }
}
