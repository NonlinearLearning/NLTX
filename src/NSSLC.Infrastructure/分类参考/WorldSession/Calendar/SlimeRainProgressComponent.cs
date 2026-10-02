namespace Terraria.WorldSession.Calendar;

public sealed class SlimeRainProgressComponent
{
  public SlimeRainProgressComponent(int killCount = 0)
  {
    KillCount = killCount;
  }

  public int KillCount { get; internal set; }
}
