namespace Terraria.Player;

public sealed class PlayerLuckCapabilityComponent
{
  public bool HasLuckLuckyCoin { get; internal set; }

  public bool HasLuckLuckyHorseshoe { get; internal set; }

  public bool HasLuckLuckyClover { get; internal set; }

  public bool HasLuckWiltedClover { get; internal set; }

  public bool HasLuckRavenFeather { get; internal set; }

  internal void ResetEffects()
  {
    HasLuckLuckyCoin = false;
    HasLuckLuckyHorseshoe = false;
    HasLuckLuckyClover = false;
    HasLuckWiltedClover = false;
    HasLuckRavenFeather = false;
  }
}
