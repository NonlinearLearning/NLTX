using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct SignIdentityComponent(int SignId)
{
  public bool IsValid => SignId > 0;

  public SignIdentityComponent Validate()
  {
    if (!IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(SignId));
    }

    return this;
  }
}
