namespace Terraria.SpatialMotionPhysics;

public sealed class EntityLiquidContactComponent
{
  public bool Wet { get; internal set; }

  public bool ShimmerWet { get; internal set; }

  public bool HoneyWet { get; internal set; }

  public byte WetCount { get; internal set; }

  public bool LavaWet { get; internal set; }
}
