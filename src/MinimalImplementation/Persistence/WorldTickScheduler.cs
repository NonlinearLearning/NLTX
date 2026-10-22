using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MinimalImplementation.Persistence;

/// <summary>Schedules ordered domain ticks at 60 Hz on the single-writer commit queue.</summary>
public sealed class WorldTickScheduler : IAsyncDisposable
{
  private static readonly TimeSpan TickPeriod = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

  private readonly IWorldCommitQueue _commitQueue;
  private readonly IServerTickPhaseOwner[] _owners;
  private readonly Func<bool> _hasActiveClients;
  private readonly TimeProvider _timeProvider;
  private readonly int _maximumCatchUpTicks;
  private readonly object _stateGate = new();
  private CancellationTokenSource? _stopSource;
  private Task? _runTask;
  private long _gameTickIndex;
  private long _maintenanceTickIndex;

  public WorldTickScheduler(
    IWorldCommitQueue commitQueue,
    IReadOnlyCollection<IServerTickPhaseOwner> owners,
    Func<bool> hasActiveClients,
    int maximumCatchUpTicks = 4,
    TimeProvider? timeProvider = null)
  {
    ArgumentNullException.ThrowIfNull(commitQueue);
    ArgumentNullException.ThrowIfNull(owners);
    ArgumentNullException.ThrowIfNull(hasActiveClients);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumCatchUpTicks);
    Dictionary<ServerTickPhase, IServerTickPhaseOwner> phaseOwners = [];
    var transactionIdentities = new HashSet<object>(ReferenceEqualityComparer.Instance);
    foreach (IServerTickPhaseOwner owner in owners)
    {
      RegisterOwner(owner, phaseOwners, transactionIdentities, nameof(owners));
    }

    foreach (ServerTickPhase phase in Enum.GetValues<ServerTickPhase>())
    {
      if (!phaseOwners.ContainsKey(phase))
      {
        throw new ArgumentException(
          "Exactly one owner or composite participant is required for every scheduled server " +
          "tick phase.",
          nameof(owners));
      }
    }

    _commitQueue = commitQueue;
    _owners = owners.OrderBy(owner => owner.Phase).ToArray();
    _hasActiveClients = hasActiveClients;
    _maximumCatchUpTicks = maximumCatchUpTicks;
    _timeProvider = timeProvider ?? TimeProvider.System;
  }

  public long GameTickIndex => Interlocked.Read(ref _gameTickIndex);

  public long MaintenanceTickIndex => Interlocked.Read(ref _maintenanceTickIndex);

  public Task StartAsync(CancellationToken cancellationToken = default)
  {
    lock (_stateGate)
    {
      if (_runTask is not null)
      {
        throw new InvalidOperationException("The world tick scheduler can only be started once.");
      }

      _stopSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      _runTask = RunAsync(_stopSource.Token);
      return _runTask;
    }
  }

  public async ValueTask StopAsync()
  {
    Task? runTask;
    lock (_stateGate)
    {
      _stopSource?.Cancel();
      runTask = _runTask;
    }

    if (runTask is not null)
    {
      try
      {
        await runTask.ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
      }
    }
  }

  public async ValueTask DisposeAsync()
  {
    await StopAsync().ConfigureAwait(false);
    lock (_stateGate)
    {
      _stopSource?.Dispose();
      _stopSource = null;
    }
  }

  private async Task RunAsync(CancellationToken cancellationToken)
  {
    long periodTimestamp = Math.Max(1, (long)Math.Round(
      TickPeriod.TotalSeconds * _timeProvider.TimestampFrequency));
    long nextDeadline = _timeProvider.GetTimestamp();

    while (true)
    {
      nextDeadline = checked(nextDeadline + periodTimestamp);
      TimeSpan delay = _timeProvider.GetElapsedTime(_timeProvider.GetTimestamp(), nextDeadline);
      if (delay > TimeSpan.Zero)
      {
        await Task.Delay(delay, _timeProvider, cancellationToken).ConfigureAwait(false);
      }

      long now = _timeProvider.GetTimestamp();
      long lateBy = Math.Max(0, now - nextDeadline);
      long dueTicks = Math.Max(1, 1 + (lateBy / periodTimestamp));
      long skippedTicks = Math.Max(0, dueTicks - _maximumCatchUpTicks);
      if (skippedTicks > 0)
      {
        nextDeadline = checked(nextDeadline + (skippedTicks * periodTimestamp));
      }

      int ticksToRun = (int)Math.Min(dueTicks, _maximumCatchUpTicks);
      for (int index = 0; index < ticksToRun; index++)
      {
        cancellationToken.ThrowIfCancellationRequested();
        await _commitQueue.ExecuteAsync(
          () =>
          {
            Interlocked.Increment(ref _maintenanceTickIndex);
            if (_hasActiveClients.Invoke())
            {
              long gameTickIndex = Interlocked.Increment(ref _gameTickIndex);
              AdvanceOwners(gameTickIndex);
            }
          },
          cancellationToken).ConfigureAwait(false);
      }
    }
  }

  private void AdvanceOwners(long gameTickIndex)
  {
    foreach (IServerTickPhaseOwner owner in _owners)
    {
      owner.Advance(gameTickIndex);
    }
  }

  private static void RegisterOwner(
    IServerTickPhaseOwner owner,
    IDictionary<ServerTickPhase, IServerTickPhaseOwner> phaseOwners,
    ISet<object> transactionIdentities,
    string parameterName)
  {
    if (owner is null)
    {
      throw new ArgumentException("Server tick owners cannot contain null values.", parameterName);
    }

    ValidatePhase(owner.Phase, parameterName);
    object transactionIdentity = owner.TransactionIdentity ?? throw new ArgumentException(
      "Every server tick owner must expose a transaction identity.",
      parameterName);
    if (transactionIdentity.GetType().IsValueType)
    {
      throw new ArgumentException(
        "Server tick transaction identities must be stable reference objects.",
        parameterName);
    }

    if (!transactionIdentities.Add(transactionIdentity))
    {
      throw new ArgumentException(
        "A server tick transaction coordinator may be registered only once.",
        parameterName);
    }

    if (owner is not IServerTickPhaseCompositeOwner composite)
    {
      RegisterPhase(owner.Phase, owner, phaseOwners, parameterName);
      return;
    }

    IReadOnlyList<IServerTickPhaseParticipant> participants = composite.Participants;
    if (participants is null || participants.Count < 2)
    {
      throw new ArgumentException(
        "A composite server tick owner must declare at least two participants.",
        parameterName);
    }

    if (participants[0] is null || participants[0].Phase != owner.Phase)
    {
      throw new ArgumentException(
        "A composite owner phase must be the first participant and its ordering anchor.",
        parameterName);
    }

    var participantPhases = new HashSet<ServerTickPhase>();
    ServerTickPhase previousPhase = owner.Phase;
    for (int index = 0; index < participants.Count; index++)
    {
      IServerTickPhaseParticipant participant = participants[index];
      if (participant is null)
      {
        throw new ArgumentException(
          "Composite server tick participants cannot contain null values.",
          parameterName);
      }

      ValidatePhase(participant.Phase, parameterName);
      object participantIdentity = participant.TransactionIdentity ?? throw new ArgumentException(
        "Every server tick participant must expose a transaction identity.",
        parameterName);
      if (!ReferenceEquals(participantIdentity, transactionIdentity))
      {
        throw new ArgumentException(
          "All participants in one composite owner must share its transaction identity.",
          parameterName);
      }

      if (!participantPhases.Add(participant.Phase))
      {
        throw new ArgumentException(
          "A composite server tick owner cannot claim one phase more than once.",
          parameterName);
      }

      if (index > 0 && participant.Phase <= previousPhase)
      {
        throw new ArgumentException(
          "Composite server tick participants must be listed in phase order.",
          parameterName);
      }

      if (index > 0 && (int)participant.Phase != (int)previousPhase + 1)
      {
        throw new ArgumentException(
          "Composite server tick participants must cover contiguous phases.",
          parameterName);
      }

      RegisterPhase(participant.Phase, owner, phaseOwners, parameterName);
      previousPhase = participant.Phase;
    }
  }

  private static void RegisterPhase(
    ServerTickPhase phase,
    IServerTickPhaseOwner owner,
    IDictionary<ServerTickPhase, IServerTickPhaseOwner> phaseOwners,
    string parameterName)
  {
    if (!phaseOwners.TryAdd(phase, owner))
    {
      throw new ArgumentException(
        $"Server tick phase '{phase}' has more than one owner or participant.",
        parameterName);
    }
  }

  private static void ValidatePhase(ServerTickPhase phase, string parameterName)
  {
    if (!Enum.IsDefined(phase))
    {
      throw new ArgumentException(
        $"Server tick phase value '{phase}' is not defined.",
        parameterName);
    }
  }
}
