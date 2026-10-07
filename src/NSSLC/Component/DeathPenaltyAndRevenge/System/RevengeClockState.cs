namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeClockState
{
  public int CurrentTick { get; private set; }

  public int Advance()
  {
    CurrentTick = unchecked(CurrentTick + 1);
    return CurrentTick;
  }

  public void Reset()
  {
    CurrentTick = 0;
  }
}
