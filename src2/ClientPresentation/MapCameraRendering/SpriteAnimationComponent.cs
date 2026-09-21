namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SpriteAnimationComponent
{
  public SpriteAnimationComponent(
    int frameCount,
    int ticksPerFrame,
    bool pingPong,
    bool notActuallyAnimating,
    int paddingX,
    int paddingY,
    byte columnCount,
    byte rowCount)
  {
    if (frameCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frameCount));
    }

    if (ticksPerFrame <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticksPerFrame));
    }

    if (paddingX < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(paddingX));
    }

    if (paddingY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(paddingY));
    }

    if (columnCount == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(columnCount));
    }

    if (rowCount == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rowCount));
    }

    FrameCount = frameCount;
    TicksPerFrame = ticksPerFrame;
    PingPong = pingPong;
    NotActuallyAnimating = notActuallyAnimating;
    PaddingX = paddingX;
    PaddingY = paddingY;
    ColumnCount = columnCount;
    RowCount = rowCount;
  }

  public int FrameCount { get; }

  public int TicksPerFrame { get; }

  public bool PingPong { get; }

  public bool NotActuallyAnimating { get; }

  public int PaddingX { get; }

  public int PaddingY { get; }

  public byte ColumnCount { get; }

  public byte RowCount { get; }

  public int FrameCounter { get; private set; }

  public int CurrentFrame { get; private set; }

  public byte CurrentColumn => (byte)(CurrentFrame % ColumnCount);

  public byte CurrentRow => (byte)(CurrentFrame / ColumnCount % RowCount);

  public int Direction { get; private set; } = 1;

  public uint Revision { get; private set; }

  internal void Advance(int elapsedTicks)
  {
    if (elapsedTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(elapsedTicks));
    }

    if (NotActuallyAnimating || elapsedTicks == 0)
    {
      return;
    }

    FrameCounter = checked(FrameCounter + elapsedTicks);
    while (FrameCounter >= TicksPerFrame)
    {
      FrameCounter -= TicksPerFrame;
      StepFrame();
    }

    Revision++;
  }

  public SpriteFrameSnapshot Snapshot()
  {
    return new SpriteFrameSnapshot(
      PaddingX,
      PaddingY,
      CurrentColumn,
      CurrentRow,
      ColumnCount,
      RowCount);
  }

  private void StepFrame()
  {
    int lastFrame = FrameCount - 1;
    if (!PingPong)
    {
      CurrentFrame = (CurrentFrame + 1) % FrameCount;
      return;
    }

    int nextFrame = CurrentFrame + Direction;
    if (nextFrame > lastFrame || nextFrame < 0)
    {
      Direction = -Direction;
      nextFrame = CurrentFrame + Direction;
    }

    CurrentFrame = nextFrame;
  }
}
