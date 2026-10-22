using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>World-owned facts captured once for one Player tick input.</summary>
public interface IPlayerTickWorldFactsSource
{
  IPlayerTickWorldFacts? Capture(SessionBinding binding, Player player);
}

public interface IPlayerTickWorldFacts
{
  SessionEpoch Epoch { get; }

  EntityHandle PlayerHandle { get; }

  CommitVersion PlayerVersion { get; }

  CommitVersion WorldRevision { get; }

  bool LanternsUp { get; }

  int DayRate { get; }

  OwnerCommitResult Validate();
}

/// <summary>Read-only activity query supplied by the entity runtime for NPC/projectile handles.</summary>
public interface IPlayerEntityActivitySource
{
  bool IsActive(EntityHandle handle);
}

/// <summary>Captures recipients, world tick facts and binding guards for PlayerUpdateService.</summary>
public sealed class PlayerTickInputCapture
{
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly IPlayerTickWorldFactsSource? _worldFacts;
  private readonly PlayerRuntimeFactsCaptureService? _runtimeFacts;

  public PlayerTickInputCapture(
    SessionAdmissionCoordinator sessions,
    IPlayerTickWorldFactsSource worldFacts)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(worldFacts);
    _sessions = sessions;
    _worldFacts = worldFacts;
  }

  public PlayerTickInputCapture(
    SessionAdmissionCoordinator sessions,
    PlayerRuntimeFactsCaptureService runtimeFacts)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(runtimeFacts);
    _sessions = sessions;
    _runtimeFacts = runtimeFacts;
  }

  public PlayerTickInput? Capture(Player player)
  {
    ArgumentNullException.ThrowIfNull(player);
    if (!_sessions.TryGetStage(player.Epoch, out SessionStage stage)
      || stage != SessionStage.Active
      || !_sessions.TryGetBinding(player.Epoch, out SessionBinding? binding)
      || binding is null || binding.PlayerHandle != player.Handle)
    {
      return null;
    }

    if (_runtimeFacts is not null)
    {
      OwnerCommitResult runtimeResult = _runtimeFacts.TryCaptureTickFacts(
        binding, player, out IPlayerTickWorldFacts? runtime);
      return runtimeResult.Status == OwnerCommitStatus.Applied && runtime is not null
        ? new PlayerTickInput(binding, runtime, _sessions.CaptureActiveRecipients(binding.Epoch))
        : null;
    }

    IPlayerTickWorldFacts? facts = _worldFacts?.Capture(binding, player);
    if (facts is null || facts.Epoch != binding.Epoch || facts.PlayerHandle != player.Handle
      || facts.PlayerVersion != player.Version || !facts.WorldRevision.IsValid
      || facts.DayRate < 0 || facts.Validate().Status != OwnerCommitStatus.Applied)
    {
      return null;
    }

    return new PlayerTickInput(
      binding, facts, _sessions.CaptureActiveRecipients(binding.Epoch));
  }

  public PlayerUpdateContext CreateContext(
    IPlayerEntityActivitySource entityActivity,
    PlayerSpectatingCoordinator? spectating = null)
  {
    ArgumentNullException.ThrowIfNull(entityActivity);
    return new PlayerUpdateContext(Capture, entityActivity.IsActive, spectating);
  }
}

/// <summary>Captures tick results for a transport pump after the commit-loop callback returns.</summary>
public interface IPlayerTickResultSink
{
  void Capture(PlayerUpdateResult result);
}
