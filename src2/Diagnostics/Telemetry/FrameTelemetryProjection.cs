namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class FrameTelemetryProjection
{
  public FrameTelemetrySnapshot Project(FrameTelemetryRing ring)
  {
    ArgumentNullException.ThrowIfNull(ring);
    return ring.CreateSnapshot();
  }
}
