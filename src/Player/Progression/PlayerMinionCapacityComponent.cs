namespace Terraria.Player.Progression;

public sealed class PlayerMinionCapacityComponent
{
  public int MaxMinions { get; internal set; } = 1;

  public int NumMinions { get; internal set; }

  public float SlotsMinions { get; internal set; }
}
