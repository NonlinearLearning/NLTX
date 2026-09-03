using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Player.Components;

public sealed class PlayerBuffImmunityStateComponent
{
  private readonly HashSet<ushort> _immuneTypes = new();

  public IReadOnlySet<ushort> ImmuneTypes => _immuneTypes;

  public bool IsImmune(ushort buffType)
  {
    return _immuneTypes.Contains(buffType);
  }

  public void Set(ushort buffType, bool immune)
  {
    if (immune)
    {
      _immuneTypes.Add(buffType);
    }
    else
    {
      _immuneTypes.Remove(buffType);
    }
  }

  public void Clear()
  {
    _immuneTypes.Clear();
  }
}
