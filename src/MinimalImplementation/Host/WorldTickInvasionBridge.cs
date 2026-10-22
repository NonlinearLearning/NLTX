using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Events;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>Projects committed World morning facts into the authoritative invasion owner.</summary>
public sealed class WorldTickInvasionBridge
{
  private InvasionState _invasion;
  private readonly long _stableWorldScope;

  public WorldTickInvasionBridge(InvasionState invasion, long stableWorldScope)
  {
    ArgumentNullException.ThrowIfNull(invasion);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stableWorldScope);
    _invasion = invasion;
    _stableWorldScope = stableWorldScope;
  }

  public void Consume(WorldTickResult result)
  {
    ArgumentNullException.ThrowIfNull(result);
    if (!result.Applied)
    {
      return;
    }

    if (result.MorningTransitionCount == 0)
    {
      return;
    }

    if (result.MorningTransitionCount != 1)
    {
      throw new InvalidOperationException(
        $"The applied world tick reported the non-canonical morning transition count " +
        $"{result.MorningTransitionCount}.");
    }

    InvasionCommitResult commit = _invasion.AdvanceMorning(
      _stableWorldScope,
      result.Version);
    switch (commit.Status)
    {
      case WorldEventOutcomeStatus.Applied:
      case WorldEventOutcomeStatus.Duplicate:
        return;
      case WorldEventOutcomeStatus.Rejected:
        throw new InvalidOperationException(
          $"The invasion morning commit was rejected " +
          $"({commit.ErrorCode?.ToString() ?? "unknown"}): " +
          $"{commit.Detail ?? "no detail was supplied"}.");
      default:
        throw new InvalidOperationException(
          $"The invasion morning commit returned unsupported status {commit.Status}.");
    }
  }

  internal void ReplaceState(InvasionState invasion)
  {
    ArgumentNullException.ThrowIfNull(invasion);
    _invasion = invasion;
  }
}
