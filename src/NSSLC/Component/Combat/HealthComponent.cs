using System;

namespace Terraria.Combat;

public struct HealthComponent
{
  public HealthComponent(int current, int maximum)
  {
    Current = current;
    Maximum = maximum;
  }

  public int Current;
  public int Maximum;

  public bool IsDepleted => Current <= 0;

  public int Missing => Math.Max(0, Maximum - Current);
}
