namespace Terraria.Player;

public sealed class PlayerWaterAndUtilityCapabilityComponent
{
  public bool AccDivingHelm { get; internal set; }

  public bool AccFlipper { get; internal set; }

  public bool DeadCellsPotionStation { get; internal set; }

  internal void ResetEffects()
  {
    AccDivingHelm = false;
    AccFlipper = false;
    DeadCellsPotionStation = false;
  }
}
