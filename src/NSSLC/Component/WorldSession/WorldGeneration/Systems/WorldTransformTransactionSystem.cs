using System;
using System.Threading;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class WorldTransformTransactionSystem
{
  public static int Begin(WorldTransformTransactionComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.Begin();
  }

  public static WorldTransformTransactionLease BeginTransaction(
    WorldTransformTransactionComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    var lease = new WorldTransformTransactionLease(component);
    component.Begin();
    return lease;
  }

  public static int Complete(WorldTransformTransactionComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.End();
  }

  public static bool HasActiveTransactions(WorldTransformTransactionComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.IsTransforming;
  }

  public static void WaitUntilIdle(
    WorldTransformTransactionComponent component,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.WaitUntilIdle(cancellationToken);
  }
}
