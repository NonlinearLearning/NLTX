namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class LightMapCacheComponent
{
  private readonly RgbaColor[] _colors;
  private readonly byte[] _masks;
  private readonly bool[] _written;

  public LightMapCacheComponent(int width, int height, float lightDecay = 1f)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    ValidateDecay(lightDecay);
    Width = width;
    Height = height;
    LightDecay = lightDecay;
    int cellCount = checked(width * height);
    _colors = new RgbaColor[cellCount];
    _masks = new byte[cellCount];
    _written = new bool[cellCount];
  }

  public int Width { get; }

  public int Height { get; }

  public float LightDecay { get; private set; }

  public uint Revision { get; private set; }

  internal void SetDecay(float lightDecay)
  {
    ValidateDecay(lightDecay);
    LightDecay = lightDecay;
    Revision++;
  }

  internal void SetCell(LightMapCellInput input)
  {
    if (!Contains(input.X, input.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(input));
    }

    int index = input.Y * Width + input.X;
    _colors[index] = ApplyDecay(input.Color, LightDecay);
    _masks[index] = (byte)Math.Clamp(
      MathF.Round(input.Mask * LightDecay),
      byte.MinValue,
      byte.MaxValue);
    _written[index] = true;
    Revision++;
  }

  internal void Clear()
  {
    Array.Clear(_colors);
    Array.Clear(_masks);
    Array.Clear(_written);
    Revision++;
  }

  internal bool TryRead(int x, int y, out LightMapSample sample)
  {
    if (!Contains(x, y) || !_written[y * Width + x])
    {
      sample = default;
      return false;
    }

    int index = y * Width + x;
    sample = new LightMapSample(_colors[index], _masks[index], Revision);
    return true;
  }

  internal bool Contains(int x, int y)
  {
    return (uint)x < (uint)Width && (uint)y < (uint)Height;
  }

  public LightingFrameSnapshot ToFrameSnapshot(uint sourceRevision)
  {
    return new LightingFrameSnapshot(Width, Height, sourceRevision, _colors, _masks);
  }

  private static RgbaColor ApplyDecay(RgbaColor color, float decay)
  {
    return new RgbaColor(
      Scale(color.R, decay),
      Scale(color.G, decay),
      Scale(color.B, decay),
      color.A);
  }

  private static byte Scale(byte value, float factor)
  {
    return (byte)Math.Clamp(MathF.Round(value * factor), byte.MinValue, byte.MaxValue);
  }

  private static void ValidateDecay(float lightDecay)
  {
    if (!float.IsFinite(lightDecay) || lightDecay < 0f || lightDecay > 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(lightDecay));
    }
  }
}
