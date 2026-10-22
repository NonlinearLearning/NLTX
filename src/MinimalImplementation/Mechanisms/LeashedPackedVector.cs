using System.Numerics;

namespace MinimalImplementation.Server.Mechanisms;

internal static class LeashedPackedVector
{
  public static bool IsFinite(uint value)
  {
    Vector2 vector = Unpack(value);
    return float.IsFinite(vector.X) && float.IsFinite(vector.Y);
  }

  public static Vector2 Unpack(uint value)
  {
    return new Vector2(
      (float)BitConverter.UInt16BitsToHalf((ushort)value),
      (float)BitConverter.UInt16BitsToHalf((ushort)(value >> 16)));
  }

  public static uint Pack(Vector2 value)
  {
    return BitConverter.HalfToUInt16Bits((Half)value.X) |
      ((uint)BitConverter.HalfToUInt16Bits((Half)value.Y) << 16);
  }
}
