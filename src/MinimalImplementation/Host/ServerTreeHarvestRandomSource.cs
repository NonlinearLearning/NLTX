using System;

namespace MinimalImplementation.Server.Host;

internal interface ITreeHarvestRandomSource {
  int Next(int maximumExclusive);
}

/// <summary>Owns the random stream for authoritative tree-drop bonuses.</summary>
internal sealed class ServerTreeHarvestRandomSource : ITreeHarvestRandomSource {
  private readonly Random _random = new();

  public int Next(int maximumExclusive) {
    return _random.Next(maximumExclusive);
  }
}
