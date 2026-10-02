namespace Terraria.SpatialMotionPhysics;

public sealed class MainRuntimeBoundaryAdapter
{
  public MainRuntimeBoundaryAdapter(bool autoGen)
  {
    AutoGen = autoGen;
  }

  public bool AutoGen { get; }
}
