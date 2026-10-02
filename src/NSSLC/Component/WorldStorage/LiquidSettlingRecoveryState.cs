namespace Terraria.WorldStorage;

public sealed class LiquidSettlingRecoveryState
{
  public long? EnteredAtTick;
  public bool IsPanicMode;
  public int NextSettlingRow;
  public int PanicCounter;
  public bool IsSettling => IsPanicMode && NextSettlingRow >= 3;
}
