namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class Vertical64BitStripsValue
{
  private readonly Bits64Value[] _values;

  public Vertical64BitStripsValue(int length)
  {
    if (length < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(length));
    }

    _values = new Bits64Value[length];
  }

  public int Length => _values.Length;

  public Bits64Value this[int index]
  {
    get => _values[index];
    set => _values[index] = value;
  }
}
