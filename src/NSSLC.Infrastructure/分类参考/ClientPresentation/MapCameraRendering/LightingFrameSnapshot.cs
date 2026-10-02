namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class LightingFrameSnapshot
{
  public LightingFrameSnapshot(
    int width,
    int height,
    uint sourceRevision,
    ReadOnlySpan<RgbaColor> colors,
    ReadOnlySpan<byte> masks)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    int cellCount = checked(width * height);
    if (colors.Length != cellCount)
    {
      throw new ArgumentException("The color buffer does not match the frame size.", nameof(colors));
    }

    if (masks.Length != cellCount)
    {
      throw new ArgumentException("The mask buffer does not match the frame size.", nameof(masks));
    }

    Width = width;
    Height = height;
    SourceRevision = sourceRevision;
    Colors = colors.ToArray();
    Masks = masks.ToArray();
  }

  public int Width { get; }

  public int Height { get; }

  public uint SourceRevision { get; }

  public ReadOnlyMemory<RgbaColor> Colors { get; }

  public ReadOnlyMemory<byte> Masks { get; }

  public bool TryRead(int x, int y, out RgbaColor color, out byte mask)
  {
    if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
    {
      color = default;
      mask = default;
      return false;
    }

    int index = y * Width + x;
    color = Colors.Span[index];
    mask = Masks.Span[index];
    return true;
  }
}
