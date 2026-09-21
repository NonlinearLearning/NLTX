using System;

namespace Terraria.Dome.Simulation.Npc.Components;

public struct NpcGivenNameComponent
{
  public const int MaximumLength = 200;

  private string? _givenName;

  public NpcGivenNameComponent(string? givenName)
  {
    if (givenName is not null && givenName.Length > MaximumLength)
    {
      throw new ArgumentOutOfRangeException(nameof(givenName));
    }

    _givenName = givenName ?? string.Empty;
  }

  public string GivenName => _givenName ?? string.Empty;

  public bool HasGivenName => GivenName.Length != 0;

  public void SetGivenName(string? givenName)
  {
    if (givenName is not null && givenName.Length > MaximumLength)
    {
      throw new ArgumentOutOfRangeException(nameof(givenName));
    }

    _givenName = givenName ?? string.Empty;
  }
}
