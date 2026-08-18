using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Liquid.Components;

public sealed class LiquidDirtySectionComponent
{
  private readonly Dictionary<WorldSectionCoordinates, long> _revisions = new();

  public int Count => _revisions.Count;

  public IReadOnlyDictionary<WorldSectionCoordinates, long> Revisions => _revisions;

  public void Mark(WorldSectionCoordinates section, long revision)
  {
    if (!_revisions.TryGetValue(section, out long current) || revision > current)
    {
      _revisions[section] = revision;
    }
  }

  public void Clear()
  {
    _revisions.Clear();
  }
}
