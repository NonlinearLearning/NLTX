using System;

namespace Terraria.Physics;

/// <summary>Owns current and maximum breath capacity for an entity.</summary>
public sealed class BreathComponent
{
  public BreathComponent(int maximum, int? current = null)
  {
    if (maximum <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }

    Maximum = maximum;
    Current = Math.Clamp(current ?? maximum, 0, maximum);
  }

  public int Current { get; private set; }

  public int Maximum { get; private set; }

  public bool IsDepleted => Current == 0;

  public int Consume(int amount)
  {
    if (amount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    int consumed = Math.Min(Current, amount);
    Current -= consumed;
    return consumed;
  }

  public void Recover(int amount)
  {
    if (amount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    Current = Math.Min(Maximum, Current + amount);
  }

  public void SetMaximum(int maximum, bool refill = false)
  {
    if (maximum <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }

    Maximum = maximum;
    Current = refill ? maximum : Math.Min(Current, maximum);
  }
}
