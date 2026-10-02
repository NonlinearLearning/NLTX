namespace NLTX.PlayerInputGameplay.Movement;

public sealed class MountMovementRestoreSnapshot
{
  private bool _readyToPaste;
  private PlayerMovementCapabilityComponent? _captured;

  public bool ReadyToPaste => _readyToPaste;

  public void Capture(PlayerMovementCapabilityComponent source)
  {
    ArgumentNullException.ThrowIfNull(source);
    _captured = new PlayerMovementCapabilityComponent();
    _captured.Set(
      source.RocketTime,
      source.WingTime,
      source.RocketDelay,
      source.RocketDelay2,
      source.JumpAgainCloud,
      source.JumpAgainSandstorm,
      source.JumpAgainBlizzard,
      source.JumpAgainFart,
      source.JumpAgainSail,
      source.JumpAgainUnicorn,
      source.MountPreventedFlight,
      source.MountPreventedExtraJumps);
    _readyToPaste = true;
  }

  public void PasteInto(PlayerMovementCapabilityComponent destination)
  {
    ArgumentNullException.ThrowIfNull(destination);
    if (!_readyToPaste || _captured is null)
    {
      throw new InvalidOperationException("No mount movement snapshot is ready.");
    }

    destination.Set(
      _captured.RocketTime,
      _captured.WingTime,
      _captured.RocketDelay,
      _captured.RocketDelay2,
      _captured.JumpAgainCloud,
      _captured.JumpAgainSandstorm,
      _captured.JumpAgainBlizzard,
      _captured.JumpAgainFart,
      _captured.JumpAgainSail,
      _captured.JumpAgainUnicorn,
      _captured.MountPreventedFlight,
      _captured.MountPreventedExtraJumps);
    _readyToPaste = false;
    _captured = null;
  }
}
