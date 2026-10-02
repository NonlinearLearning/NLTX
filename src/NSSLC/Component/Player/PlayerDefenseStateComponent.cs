namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1402, P09-1403, P09-1404, P09-1405
// crossSubsystemOwner: shield command, combat resolution, and effect reset remain integration-review
public sealed class PlayerDefenseStateComponent
{
  public bool HasRaisableShield { get; internal set; }

  public bool ShieldRaised { get; internal set; }

  public int ShieldParryTimeLeft { get; internal set; }

  public int ShieldParryCooldown { get; internal set; }
}
