namespace Terraria.Player;

public sealed class PlayerSurfaceElementStatusComponent
{
  public bool Dripping { get; internal set; }

  public bool DrippingSlime { get; internal set; }

  public bool DrippingSparkleSlime { get; internal set; }

  internal void ResetEffects()
  {
    Dripping = false;
    DrippingSlime = false;
    DrippingSparkleSlime = false;
  }
}
