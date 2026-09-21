namespace Terraria.NonAuthoritative.Diagnostics;

public struct BitsByteValue
{
  private byte _value;

  public bool this[int index]
  {
    readonly get
    {
      ValidateIndex(index);
      return (_value & (1 << index)) != 0;
    }
    set
    {
      ValidateIndex(index);
      if (value)
      {
        _value |= (byte)(1 << index);
      }
      else
      {
        _value &= (byte)~(1 << index);
      }
    }
  }

  public static implicit operator byte(BitsByteValue value)
  {
    return value._value;
  }

  public static implicit operator BitsByteValue(byte value)
  {
    return new BitsByteValue { _value = value };
  }

  private static void ValidateIndex(int index)
  {
    if (index is < 0 or > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }
  }
}
