namespace Terraria.Player;

// Holds the current-tick tile interaction capabilities derived from equipment.
public sealed class PlayerTileInteractionCapabilityComponent
{
  public bool HasTileRangeAccessory { get; set; }

  public bool HasTileSpeedAccessory { get; set; }

  public bool HasWallSpeedAccessory { get; set; }
}
