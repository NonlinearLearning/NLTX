namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class LegacyLightingCacheComponent
{
  private readonly RgbaColor[] _colors;
  private readonly byte[] _masks;
  private readonly bool[] _written;

  public LegacyLightingCacheComponent(int width, int height)
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
    int cellCount = checked(width * height);
    _colors = new RgbaColor[cellCount];
    _masks = new byte[cellCount];
    _written = new bool[cellCount];
  }

  public int Width { get; }

  public int Height { get; }

  public bool HasTemporaryLights { get; private set; }

  public uint Revision { get; private set; }

  internal void SetTemporaryLights(bool hasTemporaryLights)
  {
    HasTemporaryLights = hasTemporaryLights;
    Revision++;
  }

  internal void SetCell(LightMapCellInput input)
  {
    if (!Contains(input.X, input.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(input));
    }

    int index = input.Y * Width + input.X;
    _colors[index] = input.Color;
    _masks[index] = input.Mask;
    _written[index] = true;
    Revision++;
  }

  internal void Clear()
  {
    Array.Clear(_colors);
    Array.Clear(_masks);
    Array.Clear(_written);
    HasTemporaryLights = false;
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

  private bool Contains(int x, int y)
  {
    return (uint)x < (uint)Width && (uint)y < (uint)Height;
  }
}
