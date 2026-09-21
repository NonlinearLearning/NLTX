namespace Terraria.SpatialMotionPhysics;

public sealed class DarknessHazardStateComponent
{
  public int DarknessTimer { get; internal set; } = -1;

  public int DarknessHitTimer { get; internal set; }

  public bool SaidMessage { get; internal set; }

  public bool LastFrameWasTooBright { get; internal set; } = true;
}
