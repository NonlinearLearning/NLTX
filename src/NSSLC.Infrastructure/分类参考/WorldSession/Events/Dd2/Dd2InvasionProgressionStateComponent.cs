namespace Terraria.WorldSession.Events.Dd2;

public sealed class Dd2InvasionProgressionStateComponent
{
  public Dd2InvasionProgressionStateComponent(
    bool downedTier1 = false,
    bool downedTier2 = false,
    bool downedTier3 = false)
  {
    DownedTier1 = downedTier1;
    DownedTier2 = downedTier2;
    DownedTier3 = downedTier3;
  }

  public bool DownedTier1 { get; internal set; }

  public bool DownedTier2 { get; internal set; }

  public bool DownedTier3 { get; internal set; }
}
