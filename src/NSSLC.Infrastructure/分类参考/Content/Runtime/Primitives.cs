namespace Terraria.NonAuthoritative.ContentDefinitions;

public readonly record struct ContentSize
{
  public ContentSize(int width, int height)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    Width = width;
    Height = height;
  }

  public int Width { get; }

  public int Height { get; }
}

public readonly record struct ContentIdentity
{
  public ContentIdentity(int localType, int persistentId, int networkId)
  {
    if (localType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(localType));
    }

    if (persistentId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(persistentId));
    }

    if (networkId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(networkId));
    }

    LocalType = localType;
    PersistentId = persistentId;
    NetworkId = networkId;
  }

  public int LocalType { get; }

  public int PersistentId { get; }

  public int NetworkId { get; }
}

public readonly record struct ColorValue(byte Red, byte Green, byte Blue, byte Alpha = 255)
{
  public static ColorValue Lerp(ColorValue from, ColorValue to, float amount)
  {
    if (float.IsNaN(amount) || float.IsInfinity(amount))
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    float clamped = Math.Clamp(amount, 0f, 1f);
    return new ColorValue(
      Interpolate(from.Red, to.Red, clamped),
      Interpolate(from.Green, to.Green, clamped),
      Interpolate(from.Blue, to.Blue, clamped),
      Interpolate(from.Alpha, to.Alpha, clamped));
  }

  private static byte Interpolate(byte from, byte to, float amount)
  {
    return (byte)Math.Clamp(MathF.Round(from + ((to - from) * amount)), 0f, 255f);
  }
}

public readonly record struct PointValue(int X, int Y);

public readonly record struct RectangleValue
{
  public RectangleValue(int x, int y, int width, int height)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    X = x;
    Y = y;
    Width = width;
    Height = height;
  }

  public int X { get; }

  public int Y { get; }

  public int Width { get; }

  public int Height { get; }
}

public enum ContentDefinitionKind
{
  Item,
  Npc,
  Projectile
}
