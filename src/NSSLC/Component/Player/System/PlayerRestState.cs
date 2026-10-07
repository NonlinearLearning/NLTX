namespace Terraria.Player;

public readonly record struct PlayerRestState(
  PlayerRestActivity Activities,
  int SleepElapsedTicks)
{
  public bool IsPetting => (Activities & PlayerRestActivity.Petting) != 0;

  public bool IsSitting => (Activities & PlayerRestActivity.Sitting) != 0;

  public bool IsSleeping => (Activities & PlayerRestActivity.Sleeping) != 0;

  public bool IsFullyAsleep =>
    IsSleeping && SleepElapsedTicks >= 120;

  public bool IsResting => Activities != PlayerRestActivity.None;
}
