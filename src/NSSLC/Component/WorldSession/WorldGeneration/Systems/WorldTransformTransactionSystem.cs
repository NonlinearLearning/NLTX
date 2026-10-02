using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class WorldTransformTransactionSystem
{
  public static int Begin(WorldTransformTransactionComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.Begin();
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
}
