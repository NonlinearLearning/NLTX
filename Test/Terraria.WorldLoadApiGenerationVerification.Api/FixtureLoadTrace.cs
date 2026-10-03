using System.Collections.Generic;

namespace Terraria.WorldLoadApiGenerationVerification.Api;

public sealed class FixtureLoadTrace
{
  private readonly List<string> _events = new();

  public IReadOnlyList<string> Events => _events;

  public void Record(string value)
  {
    _events.Add(value);
  }
}
