namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1402..P09-1406, Terraria.Player.TryTogglingShield, Terraria.Player.UpdateReleaseUseTile
// crossSubsystemOwner: input eligibility, Combat cooldown arrays, sound/dust effects, and tile world interaction remain integration-review
public sealed class PlayerDefenseInteractionSystem
{
  private const int ShieldParryCooldownOnRelease = 15;
  private const int AttackCooldownOnRelease = 20;
  private const int TileInteractionLockTicks = 3;
  private readonly HashSet<Guid> _acceptedCommandIds = [];

  public PlayerDefenseInteractionResult ToggleShield(
    PlayerDefenseStateComponent defense,
    in PlayerShieldToggleCommand command)
  {
    ArgumentNullException.ThrowIfNull(defense);

    if (command.CommandId == Guid.Empty)
    {
      return Rejected(
        defense,
        PlayerDefenseInteractionRejectionReason.EmptyCommand);
    }

    if (_acceptedCommandIds.Contains(command.CommandId))
    {
      return Rejected(
        defense,
        PlayerDefenseInteractionRejectionReason.DuplicateCommand);
    }

    if (command.ShouldGuard == defense.ShieldRaised)
    {
      _acceptedCommandIds.Add(command.CommandId);
      return new PlayerDefenseInteractionResult(
        Applied: false,
        ShieldRaised: defense.ShieldRaised,
        ShieldParryTimeLeft: defense.ShieldParryTimeLeft,
        ShieldParryCooldown: defense.ShieldParryCooldown,
        AttackCooldownFrames: 0,
        ResetItemTimers: false,
        RejectionReason: PlayerDefenseInteractionRejectionReason.NoChange);
    }

    defense.ShieldRaised = command.ShouldGuard;
    if (defense.ShieldRaised)
    {
      if (defense.ShieldParryCooldown == 0)
      {
        defense.ShieldParryTimeLeft = 1;
      }

      _acceptedCommandIds.Add(command.CommandId);
      return new PlayerDefenseInteractionResult(
        Applied: true,
        ShieldRaised: true,
        ShieldParryTimeLeft: defense.ShieldParryTimeLeft,
        ShieldParryCooldown: defense.ShieldParryCooldown,
        AttackCooldownFrames: 0,
        ResetItemTimers: true,
        RejectionReason: PlayerDefenseInteractionRejectionReason.None);
    }

    defense.ShieldParryCooldown = ShieldParryCooldownOnRelease;
    defense.ShieldParryTimeLeft = 0;
    _acceptedCommandIds.Add(command.CommandId);
    return new PlayerDefenseInteractionResult(
      Applied: true,
      ShieldRaised: false,
      ShieldParryTimeLeft: defense.ShieldParryTimeLeft,
      ShieldParryCooldown: defense.ShieldParryCooldown,
      AttackCooldownFrames: AttackCooldownOnRelease,
      ResetItemTimers: false,
      RejectionReason: PlayerDefenseInteractionRejectionReason.None);
  }

  public PlayerDefenseInteractionResult AdvanceTick(
    PlayerDefenseStateComponent defense)
  {
    ArgumentNullException.ThrowIfNull(defense);

    if (defense.ShieldParryCooldown > 0)
    {
      defense.ShieldParryCooldown--;
    }

    if (defense.ShieldParryTimeLeft > 0 &&
      ++defense.ShieldParryTimeLeft > 20)
    {
      defense.ShieldParryTimeLeft = 0;
    }

    return new PlayerDefenseInteractionResult(
      Applied: false,
      ShieldRaised: defense.ShieldRaised,
      ShieldParryTimeLeft: defense.ShieldParryTimeLeft,
      ShieldParryCooldown: defense.ShieldParryCooldown,
      AttackCooldownFrames: 0,
      ResetItemTimers: false,
      RejectionReason: PlayerDefenseInteractionRejectionReason.None);
  }

  public void ResetForLifecycle(PlayerDefenseStateComponent defense)
  {
    ArgumentNullException.ThrowIfNull(defense);

    defense.HasRaisableShield = false;
    defense.ShieldRaised = false;
    defense.ShieldParryTimeLeft = 0;
    defense.ShieldParryCooldown = 0;
    _acceptedCommandIds.Clear();
  }

  public PlayerTileInteractionUpdateResult LockTileInteractions(
    PlayerInteractionLockStateComponent lockState)
  {
    ArgumentNullException.ThrowIfNull(lockState);

    lockState.ReleaseUseTile = false;
    lockState.LockTileInteractionsTimer = TileInteractionLockTicks;
    return new PlayerTileInteractionUpdateResult(
      ReleaseUseTile: lockState.ReleaseUseTile,
      LockTileInteractionsTimer: lockState.LockTileInteractionsTimer);
  }

  public PlayerTileInteractionUpdateResult UpdateTileInteractions(
    PlayerInteractionLockStateComponent lockState,
    bool tileInteractAttempted,
    bool mouseInterface)
  {
    ArgumentNullException.ThrowIfNull(lockState);

    bool releaseUseTile = !tileInteractAttempted;
    if (lockState.LockTileInteractionsTimer > 0 && !lockState.ReleaseUseTile)
    {
      releaseUseTile = false;
    }

    if (mouseInterface)
    {
      releaseUseTile = false;
    }

    lockState.ReleaseUseTile = releaseUseTile;
    if (lockState.LockTileInteractionsTimer > 0)
    {
      lockState.LockTileInteractionsTimer--;
    }

    return new PlayerTileInteractionUpdateResult(
      ReleaseUseTile: lockState.ReleaseUseTile,
      LockTileInteractionsTimer: lockState.LockTileInteractionsTimer);
  }

  private static PlayerDefenseInteractionResult Rejected(
    PlayerDefenseStateComponent defense,
    PlayerDefenseInteractionRejectionReason reason)
  {
    return new PlayerDefenseInteractionResult(
      Applied: false,
      ShieldRaised: defense.ShieldRaised,
      ShieldParryTimeLeft: defense.ShieldParryTimeLeft,
      ShieldParryCooldown: defense.ShieldParryCooldown,
      AttackCooldownFrames: 0,
      ResetItemTimers: false,
      RejectionReason: reason);
  }
}
