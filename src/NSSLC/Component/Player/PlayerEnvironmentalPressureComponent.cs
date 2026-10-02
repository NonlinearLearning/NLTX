namespace Terraria.Player;

public sealed class PlayerEnvironmentalPressureComponent
{
  public bool WindPushed { get; internal set; }

  public int SunScorchCounter { get; internal set; }

  internal void ResetEffects()
  {
    WindPushed = false;
  }

  internal void ResetForLifecycle()
  {
    WindPushed = false;
    SunScorchCounter = 0;
  }
}
