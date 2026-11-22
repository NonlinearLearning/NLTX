using System;

namespace Terraria.Combat;

/// <summary>Stores bounded health state; damage and healing rules belong to combat systems.</summary>
public readonly record struct LifeComponent {
  public int Current { get; }
  public int Maximum { get; }
  public int Minimum { get; }

  public LifeComponent(int current, int maximum, int minimum = 0) {
    if (maximum <= 0) {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }
    if (minimum < 0 || minimum > maximum) {
      throw new ArgumentOutOfRangeException(nameof(minimum));
    }
    if (current < minimum || current > maximum) {
      throw new ArgumentOutOfRangeException(nameof(current));
    }
    Current = current;
    Maximum = maximum;
    Minimum = minimum;
  }

  public bool IsAlive => Current > Minimum;

  public bool IsDepleted => Current == Minimum;

  /// <summary>Creates the next bounded health value for the combat commit system.</summary>
  public LifeComponent WithCurrent(int current)
  {
    return new LifeComponent(current, Maximum, Minimum);
  }

  /// <summary>Creates the next bounded life value after a maximum-life synchronization.</summary>
  public LifeComponent WithMaximum(int maximum)
  {
    if (maximum <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }

    int nextMinimum = Math.Min(Minimum, maximum);
    int nextCurrent = Math.Clamp(Current, nextMinimum, maximum);
    return new LifeComponent(nextCurrent, maximum, nextMinimum);
  }
}
