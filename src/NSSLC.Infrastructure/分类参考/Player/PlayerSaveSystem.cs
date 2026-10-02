using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Player;

public sealed class PlayerSaveSystem
{
  private readonly PlayerSaveSessionComponent _session;
  private readonly IPlayerSaveClock _clock;
  private readonly FilePlatformAdapter _platform;
  private TimeSpan _lastClock;

  public PlayerSaveSystem(
    PlayerSaveSessionComponent session,
    IPlayerSaveClock clock,
    FilePlatformAdapter platform)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
    _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    _platform = platform ?? throw new ArgumentNullException(nameof(platform));
    _lastClock = _clock.Now;
  }

  public void Start()
  {
    if (_session.IsTimerActive)
    {
      return;
    }

    _lastClock = _clock.Now;
    _session.SetTimerActive(true);
  }

  public void Update()
  {
    if (!_session.IsTimerActive)
    {
      return;
    }

    TimeSpan now = _clock.Now;
    TimeSpan elapsed = now - _lastClock;
    _lastClock = now;
    if (elapsed > TimeSpan.Zero)
    {
      _session.AddPlayTime(elapsed);
    }
  }

  public void Stop()
  {
    if (!_session.IsTimerActive)
    {
      return;
    }

    Update();
    _session.SetTimerActive(false);
  }

  public PlayerSaveOutcome Save(PlayerSaveCommand command, IPlayerSaveEncoder encoder)
  {
    ArgumentNullException.ThrowIfNull(encoder);
    if (command.Policy.ServerSideCharacter)
    {
      return PlayerSaveOutcome.Skipped;
    }

    PlayerSaveEncodeResult encoded;
    try
    {
      encoded = encoder.Encode(_session.CreateSnapshot(), command);
    }
    catch (Exception exception)
    {
      return PlayerSaveOutcome.Rejected(
        FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, exception.Message));
    }

    if (!encoded.Succeeded)
    {
      return PlayerSaveOutcome.Rejected(encoded.Failure);
    }

    FilePlatformOperationResult write = _platform.WriteAllBytes(
      command.Path,
      encoded.Bytes,
      command.IsCloudSave);
    return write.Succeeded
      ? PlayerSaveOutcome.Success
      : PlayerSaveOutcome.Rejected(write.Failure);
  }
}
