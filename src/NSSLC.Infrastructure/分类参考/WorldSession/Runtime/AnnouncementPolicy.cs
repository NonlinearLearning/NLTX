namespace Terraria.WorldSession.Runtime;

public readonly record struct AnnouncementPolicy
{
  public bool Disabled { get; }

  public int Range { get; }

  public AnnouncementPolicy(bool disabled, int range)
  {
    Disabled = disabled;
    Range = range < -1 ? throw new ArgumentOutOfRangeException(nameof(range)) : range;
  }
}
