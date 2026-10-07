using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using Terraria.NonAuthoritative.Persistence;
using WorldLoadRecoveryPhase = Terraria.WorldGeneration.Components.WorldLoadRecoveryPhase;
using WorldDescriptorState = Terraria.WorldSession.Components.WorldDescriptorState;
using WorldRulesState = Terraria.WorldSession.Components.WorldRulesState;
using Terraria.WorldSession.Components;

namespace Terraria.NonAuthoritative.Simulation;

/// <summary>Runs one deterministic simulation tick at a time on its creating thread.</summary>
public sealed class WorldSimulationKernel
{
  private readonly LoadedWorldSession _session;
  private readonly IWorldSimulationTickPhase[] _phases;
  private readonly IWorldSimulationRuntimeProjection? _runtimeProjection;
  private readonly Func<LoadedWorldSession, bool>? _isSessionCurrent;
  private readonly ConcurrentQueue<Action<WorldSimulationTickContext>> _commands = new();
  private readonly int _ownerThreadId;
  private readonly int _worldTimeRate;
  private int _stopRequested;

  public WorldSimulationKernel(
    LoadedWorldSession session,
    IEnumerable<IWorldSimulationTickPhase>? phases = null,
    int worldTimeRate = 1,
    IWorldSimulationRuntimeProjection? runtimeProjection = null,
    Func<LoadedWorldSession, bool>? isSessionCurrent = null)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
    ArgumentOutOfRangeException.ThrowIfNegative(worldTimeRate);
    _worldTimeRate = worldTimeRate;
    _runtimeProjection = runtimeProjection;
    _isSessionCurrent = isSessionCurrent;
    _ownerThreadId = Environment.CurrentManagedThreadId;
    IWorldSimulationTickPhase[] registeredPhases =
      (phases ?? Array.Empty<IWorldSimulationTickPhase>()).ToArray();
    if (registeredPhases.Any(static phase => phase is null))
    {
      throw new ArgumentException("Simulation phases cannot contain null entries.", nameof(phases));
    }

    _phases = registeredPhases
      .OrderBy(phase => phase.Phase)
      .ToArray();

    for (int index = 1; index < _phases.Length; index++)
    {
      if (_phases[index - 1].Phase == _phases[index].Phase)
      {
        throw new ArgumentException(
          $"The simulation phase {_phases[index].Phase} was registered more than once.",
          nameof(phases));
      }
    }

    if (_phases.Any(static phase => phase.Phase is
        WorldSimulationPhase.CommandDrain or
        WorldSimulationPhase.WorldClock or
        WorldSimulationPhase.SnapshotCommit or
        WorldSimulationPhase.RuntimeProjection or
        WorldSimulationPhase.TickCommit))
    {
      throw new ArgumentException(
        "Command, clock, snapshot, runtime projection and commit phases are owned by the simulation kernel.",
        nameof(phases));
    }

    EnsureSessionReady();
    CurrentSnapshot = CreateSnapshot(TickNumber);
    Status = WorldSimulationKernelStatus.Ready;
  }

  public WorldSimulationKernelStatus Status { get; private set; }

  public long TickNumber { get; private set; }

  public WorldTickSnapshot? CurrentSnapshot { get; private set; }

  public void EnqueueCommand(Action<WorldSimulationTickContext> command)
  {
    ArgumentNullException.ThrowIfNull(command);
    if (Status is WorldSimulationKernelStatus.Stopped or WorldSimulationKernelStatus.Faulted)
    {
      throw new InvalidOperationException("A stopped simulation kernel cannot accept commands.");
    }

    if (Volatile.Read(ref _stopRequested) != 0)
    {
      throw new InvalidOperationException("A stopping simulation kernel cannot accept commands.");
    }

    _commands.Enqueue(command);
  }

  public void RequestStop()
  {
    Interlocked.Exchange(ref _stopRequested, 1);
  }

  public WorldSimulationStepResult Step(CancellationToken cancellationToken = default)
  {
    EnsureOwnerThread();
    if (Status is WorldSimulationKernelStatus.Stopped or WorldSimulationKernelStatus.Faulted)
    {
      throw new InvalidOperationException($"The simulation kernel is {Status}.");
    }

    if (cancellationToken.IsCancellationRequested || Volatile.Read(ref _stopRequested) != 0)
    {
      Status = WorldSimulationKernelStatus.Stopped;
      return new WorldSimulationStepResult(
        tickCommitted: false,
        TickNumber,
        commandsApplied: 0,
        Array.Empty<WorldSimulationPhase>(),
        Status);
    }

    if (_isSessionCurrent is not null && !_isSessionCurrent(_session))
    {
      Status = WorldSimulationKernelStatus.Stopped;
      return new WorldSimulationStepResult(
        tickCommitted: false,
        TickNumber,
        commandsApplied: 0,
        Array.Empty<WorldSimulationPhase>(),
        Status);
    }

    try
    {
      EnsureSessionReady();
    }
    catch
    {
      Status = WorldSimulationKernelStatus.Faulted;
      throw;
    }
    Status = WorldSimulationKernelStatus.Running;
    long nextTick = checked(TickNumber + 1);
    var executedPhases = new List<WorldSimulationPhase>(_phases.Length + 5)
    {
      WorldSimulationPhase.CommandDrain,
    };
    var context = new WorldSimulationTickContext(
      _session,
      nextTick,
      CurrentSnapshot!);
    int commandsApplied = 0;

    try
    {
      while (_commands.TryDequeue(out Action<WorldSimulationTickContext>? command))
      {
        command(context);
        commandsApplied++;
        if (!IsSessionCurrent())
        {
          return StopUncommittedStep(commandsApplied, executedPhases);
        }
      }

      if (!IsSessionCurrent())
      {
        return StopUncommittedStep(commandsApplied, executedPhases);
      }

      WorldSimulationClockSystem.Advance(
        _session.World.TimeWeather,
        _worldTimeRate,
        _session.World.Progression);
      executedPhases.Add(WorldSimulationPhase.WorldClock);
      if (!IsSessionCurrent())
      {
        return StopUncommittedStep(commandsApplied, executedPhases);
      }

      context = new WorldSimulationTickContext(_session, nextTick, CreateSnapshot(nextTick));
      executedPhases.Add(WorldSimulationPhase.SnapshotCommit);
      if (_runtimeProjection is not null)
      {
        _runtimeProjection.Project(context);
        executedPhases.Add(WorldSimulationPhase.RuntimeProjection);
        if (!IsSessionCurrent())
        {
          return StopUncommittedStep(commandsApplied, executedPhases);
        }
      }
      foreach (IWorldSimulationTickPhase phase in _phases)
      {
        phase.Execute(context);
        executedPhases.Add(phase.Phase);
        if (!IsSessionCurrent())
        {
          return StopUncommittedStep(commandsApplied, executedPhases);
        }
      }

      TickNumber = nextTick;
      CurrentSnapshot = context.WorldSnapshot;
      executedPhases.Add(WorldSimulationPhase.TickCommit);
      Status = Volatile.Read(ref _stopRequested) != 0 || cancellationToken.IsCancellationRequested
        ? WorldSimulationKernelStatus.Stopped
        : WorldSimulationKernelStatus.Running;
      return new WorldSimulationStepResult(
        tickCommitted: true,
        TickNumber,
        commandsApplied,
        new ReadOnlyCollection<WorldSimulationPhase>(executedPhases),
        Status);
    }
    catch
    {
      Status = WorldSimulationKernelStatus.Faulted;
      throw;
    }
  }

  private WorldTickSnapshot CreateSnapshot(long revision)
  {
    WorldDescriptorState descriptor = _session.World.Descriptor;
    WorldRulesState rules = _session.World.Rules;
    WorldTimeWeatherState time = _session.World.TimeWeather;
    var descriptorSnapshot = new WorldDescriptorSnapshotValue(
      descriptor.WorldId,
      descriptor.UniqueId,
      descriptor.Name,
      descriptor.SeedText,
      descriptor.WorldGeneratorVersion,
      descriptor.SizeX,
      descriptor.SizeY,
      descriptor.Bounds,
      descriptor.SurfaceLayer,
      descriptor.RockLayer,
      descriptor.SpawnTileX,
      descriptor.SpawnTileY,
      descriptor.DungeonTileX,
      descriptor.DungeonTileY);
    var rulesSnapshot = new WorldRulesSnapshotValue(
      rules.GameMode,
      rules.HardMode,
      rules.SecretSeeds,
      rules.WorldEvil,
      rules.SavedOreTiers);
    WorldClockSnapshotValue clockSnapshot = WorldClockSnapshotValue.Capture(time);
    var weatherSnapshot = new WorldWeatherSnapshotValue(
      time.Raining,
      time.RainTime,
      time.RainStrength,
      time.WindTarget,
      time.WindCurrent);
    WorldEventProgressState progression = _session.World.Progression;
    var eventSnapshot = new WorldEventSnapshotValue(
      time.BloodMoon,
      time.Eclipse,
      time.PumpkinMoon,
      time.SnowMoon,
      time.SlimeRain,
      time.SlimeRainKillCount,
      (int)progression.Invasion.Type,
      progression.Invasion.Delay,
      progression.Invasion.Size,
      progression.Invasion.WarningTimer,
      progression.Invasion.Progress,
      progression.Dd2.Ongoing,
      progression.Dd2.TimeLeftUntilSpawningBegins,
      progression.Lunar.LunarApocalypseIsUp,
      progression.Lunar.MoonLordCountdown);
    var readiness = new SessionReadinessSnapshotValue(
      SessionReadinessPhase.Ready,
      GenerationBarrierActive: false,
      FailureStatusCode: null);
    return WorldTickSnapshot.CreateCommitted(
      revision,
      descriptorSnapshot,
      rulesSnapshot,
      clockSnapshot,
      weatherSnapshot,
      readiness,
      eventSnapshot);
  }

  private void EnsureSessionReady()
  {
    if (!_session.IsComplete || !_session.IsPublished ||
        _session.IsPublicationUncertain ||
        _session.Lifecycle.RecoveryPhase != WorldLoadRecoveryPhase.Completed ||
        _session.Lifecycle.IsGeneratingOrLoadingWorld ||
        _session.Lifecycle.LoadFailed || _session.Lifecycle.LoadCanceled)
    {
      throw new InvalidOperationException(
        "Simulation requires a complete, published world session with a released load gate.");
    }

    if (_isSessionCurrent is not null && !_isSessionCurrent(_session))
    {
      throw new InvalidOperationException(
        "The active world session changed after this simulation kernel was created.");
    }
  }

  private bool IsSessionCurrent()
  {
    return _isSessionCurrent is null || _isSessionCurrent(_session);
  }

  private WorldSimulationStepResult StopUncommittedStep(
    int commandsApplied,
    List<WorldSimulationPhase> executedPhases)
  {
    Status = WorldSimulationKernelStatus.Stopped;
    return new WorldSimulationStepResult(
      tickCommitted: false,
      TickNumber,
      commandsApplied,
      new ReadOnlyCollection<WorldSimulationPhase>(executedPhases),
      Status);
  }

  private void EnsureOwnerThread()
  {
    if (Environment.CurrentManagedThreadId != _ownerThreadId)
    {
      throw new InvalidOperationException(
        "Simulation steps and state commits must run on the kernel's owner thread.");
    }
  }
}
