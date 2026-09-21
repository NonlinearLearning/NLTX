namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct FloatRangeValue(float Minimum, float Maximum)
{
  public bool Contains(float value)
  {
    return value >= Minimum && value <= Maximum;
  }
}
