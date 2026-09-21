using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public sealed class ChestStateComponent
{
  private string _name;

  public ChestStateComponent(string name = "")
  {
    ArgumentNullException.ThrowIfNull(name);
    if (name.Length > ChestComponent.MaximumNameLength)
    {
      throw new ArgumentOutOfRangeException(nameof(name));
    }

    _name = name;
  }

  public string Name => _name;

  public bool TryRename(string name)
  {
    ArgumentNullException.ThrowIfNull(name);
    if (name.Length > ChestComponent.MaximumNameLength)
    {
      return false;
    }

    _name = name;
    return true;
  }
}
