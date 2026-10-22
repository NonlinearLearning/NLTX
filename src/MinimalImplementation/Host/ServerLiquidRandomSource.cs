using System;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>Owns the liquid random stream consumed under the world's commit lock.</summary>
internal sealed class ServerLiquidRandomSource : ILiquidRandomSource {
  private readonly Random _random = new();

  public int Next(int maximumExclusive) {
    return _random.Next(maximumExclusive);
  }
}
