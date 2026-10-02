using Terraria.Player.Mount;

namespace Terraria.Player;

public sealed class PlayerMountComponent
{
  public ContentId<MountDefinition>? MountType { get; set; }

  public bool IsActive { get; set; }

  public int FlightRemainingTicks { get; set; }

  public float Fatigue { get; set; }

  public float MaximumFatigue { get; set; }

  public int AbilityCharge { get; set; }

  public int AbilityCooldownRemainingTicks { get; set; }

  public int AbilityDurationRemainingTicks { get; set; }

  public bool IsDismountLocked { get; set; }

  public bool IsDismountRequested { get; set; }

  public bool IsMinecart { get; set; }
}
