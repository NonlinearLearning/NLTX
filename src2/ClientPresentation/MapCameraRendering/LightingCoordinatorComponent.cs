namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class LightingCoordinatorComponent
{
  private RgbaColor[] _activeColors;
  private RgbaColor[] _workingColors;
  private byte[] _activeMasks;
  private byte[] _workingMasks;

  public LightingCoordinatorComponent(int width, int height)
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
    _activeColors = new RgbaColor[checked(width * height)];
    _workingColors = new RgbaColor[checked(width * height)];
    _activeMasks = new byte[checked(width * height)];
    _workingMasks = new byte[checked(width * height)];
  }

  public int Width { get; }

  public int Height { get; }

  public LightingMode Mode { get; private set; } = LightingMode.Legacy;

  public float GlobalBrightness { get; private set; } = 1f;

  public int OffScreenTileBudget { get; private set; }

  public uint Revision { get; private set; }

  public uint ActiveFrameRevision { get; private set; }

  public bool HasActiveFrame { get; private set; }

  public bool HasPreparedFrame { get; private set; }

  internal void Configure(
    LightingMode mode,
    float globalBrightness,
    int offScreenTileBudget)
  {
    bool modeChanged = Mode != mode;
    Mode = mode;
    GlobalBrightness = globalBrightness;
    OffScreenTileBudget = offScreenTileBudget;
    if (modeChanged)
    {
      Invalidate();
      return;
    }

    Revision++;
  }

  internal void Prepare(LightingFrameSnapshot frame)
  {
    ValidateFrame(frame);
    frame.Colors.Span.CopyTo(_workingColors);
    frame.Masks.Span.CopyTo(_workingMasks);
    PreparedFrameRevision = frame.SourceRevision;
    HasPreparedFrame = true;
  }

  internal void CommitPrepared()
  {
    if (!HasPreparedFrame)
    {
      throw new InvalidOperationException("No prepared lighting frame is available.");
    }

    (_activeColors, _workingColors) = (_workingColors, _activeColors);
    (_activeMasks, _workingMasks) = (_workingMasks, _activeMasks);
    ActiveFrameRevision = PreparedFrameRevision;
    HasActiveFrame = true;
    HasPreparedFrame = false;
    Revision++;
  }

  internal void Invalidate()
  {
    Array.Clear(_activeColors);
    Array.Clear(_workingColors);
    Array.Clear(_activeMasks);
    Array.Clear(_workingMasks);
    ActiveFrameRevision = 0;
    HasActiveFrame = false;
    HasPreparedFrame = false;
    PreparedFrameRevision = 0;
    Revision++;
  }

  internal bool TryReadActive(
    int x,
    int y,
    out RgbaColor color,
    out byte mask)
  {
    if (!HasActiveFrame || (uint)x >= (uint)Width || (uint)y >= (uint)Height)
    {
      color = default;
      mask = default;
      return false;
    }

    int index = y * Width + x;
    color = _activeColors[index];
    mask = _activeMasks[index];
    return true;
  }

  private uint PreparedFrameRevision { get; set; }

  private void ValidateFrame(LightingFrameSnapshot frame)
  {
    ArgumentNullException.ThrowIfNull(frame);
    if (frame.Width != Width || frame.Height != Height)
    {
      throw new ArgumentException("The lighting frame dimensions do not match the coordinator.",
        nameof(frame));
    }
  }
}
