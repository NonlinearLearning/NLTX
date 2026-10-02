namespace Terraria.Npc;

public sealed class NpcShimmerStateComponent
{
  public float Transparency { get; private set; }

  internal void ApplyTransparency(float transparency)
  {
    Transparency = transparency;
  }

  internal void ResetForLifecycle()
  {
    Transparency = 0.0f;
  }
}
