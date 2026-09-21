using System;

namespace Terraria.WorldSession.Calendar;

public sealed class WindPhysicsPolicyComponent
{
  public const float DefaultStrength = 0.1f;

  public WindPhysicsPolicyComponent(bool enabled = false, float strength = DefaultStrength)
  {
    Enabled = enabled;
    Strength = strength;
    Validate();
  }

  public bool Enabled { get; internal set; }

  public float Strength { get; internal set; }

  public void Validate()
  {
    if (!float.IsFinite(Strength) || Strength < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(Strength));
    }
  }
}
