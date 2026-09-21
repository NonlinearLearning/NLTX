namespace Terraria.Player;

public sealed class PlayerTridentCapabilityComponent
{
  public bool Trident { get; internal set; }

  internal void ResetEffects()
  {
    Trident = false;
  }
}
