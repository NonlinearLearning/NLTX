namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct IntRangeValue(int Minimum, int Maximum)
{
  public bool Contains(int value)
  {
    return value >= Minimum && value <= Maximum;
  }
}
