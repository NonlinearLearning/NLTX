namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class FrameTelemetrySnapshot
{
  public FrameTelemetrySnapshot(
    IReadOnlyList<FrameTelemetrySlotSnapshot> frames)
  {
    Frames = frames;
  }

  public IReadOnlyList<FrameTelemetrySlotSnapshot> Frames { get; }
}
