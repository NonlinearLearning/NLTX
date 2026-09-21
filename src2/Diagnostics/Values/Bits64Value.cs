namespace Terraria.NonAuthoritative.Diagnostics;

public struct Bits64Value
{
  private ulong _value;

  public bool this[int index]
  {
    readonly get
    {
      ValidateIndex(index);
      return (_value & (1UL << index)) != 0;
    }
    set
    {
      ValidateIndex(index);
      if (value)
      {
        _value |= 1UL << index;
      }
      else
      {
        _value &= ~(1UL << index);
      }
    }
  }

  public readonly bool IsEmpty => _value == 0;

  public static implicit operator ulong(Bits64Value value)
  {
    return value._value;
  }

  public static implicit operator Bits64Value(ulong value)
  {
    return new Bits64Value { _value = value };
  }

  private static void ValidateIndex(int index)
  {
    if (index is < 0 or > 63)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }
  }
}
