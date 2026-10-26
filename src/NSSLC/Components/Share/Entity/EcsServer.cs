using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;

namespace Terraria.Entity;

/// <summary>
/// NSSLC's ECS composition and update boundary.
///
/// The host owns the server clock and calls <see cref="Update"/>. Arch.System owns system
/// lifecycle dispatch; this type does not replace <see cref="Group{T}"/> with another scheduler.
/// </summary>
public sealed class EcsServer : IDisposable
{
  private readonly IReadOnlyList<PhaseGroup> _phaseGroups;
  private readonly bool _ownsWorld;
  private long _lastTick = -1;
  private bool _initialized;
  private bool _updating;
  private bool _faulted;
  private bool _disposed;

  public EcsServer(
      ArchWorld world,
      IEnumerable<ISystem<EcsServerTickContext>> systems,
      bool ownsWorld = false)
      : this(world, CreateSimulationRegistrations(systems), ownsWorld)
  {
  }

  private EcsServer(
      ArchWorld world,
      IEnumerable<EcsServerSystemRegistration> registrations,
      bool ownsWorld)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(registrations);

    Dictionary<EcsServerPhase, List<ISystem<EcsServerTickContext>>> systemsByPhase = [];
    List<ISystem<EcsServerTickContext>> registeredSystems = [];
    foreach (EcsServerSystemRegistration? registration in registrations)
    {
      if (registration is null)
      {
        throw new ArgumentException(
            "The NSSLC ECS registration collection cannot contain null values.",
            nameof(registrations));
      }

      bool alreadyRegistered = false;
      foreach (ISystem<EcsServerTickContext> registeredSystem in registeredSystems)
      {
        if (ReferenceEquals(registeredSystem, registration.System))
        {
          alreadyRegistered = true;
          break;
        }
      }

      if (alreadyRegistered)
      {
        throw new ArgumentException(
            "The same Arch system instance cannot be registered more than once.",
            nameof(registrations));
      }

      registeredSystems.Add(registration.System);
      if (!systemsByPhase.TryGetValue(
              registration.Phase,
              out List<ISystem<EcsServerTickContext>>? phaseSystems))
      {
        phaseSystems = [];
        systemsByPhase.Add(registration.Phase, phaseSystems);
      }

      phaseSystems.Add(registration.System);
    }

    World = world;
    List<PhaseGroup> phaseGroups = [];
    foreach (EcsServerPhase phase in Enum.GetValues<EcsServerPhase>())
    {
      systemsByPhase.TryGetValue(
          phase,
          out List<ISystem<EcsServerTickContext>>? phaseSystems);
      phaseGroups.Add(new PhaseGroup(phase, phaseSystems?.ToArray() ?? []));
    }

    _phaseGroups = phaseGroups.AsReadOnly();
    _ownsWorld = ownsWorld;
  }

  /// <summary>
  /// Creates an NSSLC server with explicit phase registration.
  /// </summary>
  public static EcsServer Create(
      ArchWorld world,
      IEnumerable<EcsServerSystemRegistration> registrations,
      bool ownsWorld = false)
  {
    return new EcsServer(world, registrations, ownsWorld);
  }

  /// <summary>The Arch world organized by this server boundary.</summary>
  public ArchWorld World { get; }

  /// <summary>Gets the last successfully completed tick, or -1 before the first update.</summary>
  public long LastTick => _lastTick;

  public bool IsInitialized => _initialized;

  public bool IsFaulted => _faulted;

  /// <summary>Initializes every registered Arch system exactly once.</summary>
  public void Initialize()
  {
    EnsureUsable();
    if (_initialized)
    {
      throw new InvalidOperationException("The NSSLC ECS server is already initialized.");
    }

    try
    {
      foreach (PhaseGroup phaseGroup in _phaseGroups)
      {
        phaseGroup.Systems.Initialize();
      }

      _initialized = true;
    }
    catch
    {
      _faulted = true;
      throw;
    }
  }

  /// <summary>
  /// Runs one complete system group update. The host supplies the authoritative tick and time
  /// step; no wall clock or background task is created here.
  /// </summary>
  public void Update(long tick, float deltaTime)
  {
    EnsureUsable();
    if (!_initialized)
    {
      throw new InvalidOperationException("The NSSLC ECS server must be initialized first.");
    }

    if (_updating)
    {
      throw new InvalidOperationException("NSSLC ECS updates cannot be reentered.");
    }

    if (tick < 0 || (_lastTick >= 0 && tick != _lastTick + 1))
    {
      throw new ArgumentOutOfRangeException(nameof(tick),
          "The ECS tick must be nonnegative and advance by exactly one logical step.");
    }

    if (!float.IsFinite(deltaTime) || deltaTime <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(deltaTime),
          "The ECS delta time must be finite and positive.");
    }

    var context = new EcsServerTickContext(World, tick, deltaTime);
    _updating = true;
    try
    {
      foreach (PhaseGroup phaseGroup in _phaseGroups)
      {
        phaseGroup.Systems.BeforeUpdate(in context);
        phaseGroup.Systems.Update(in context);
        phaseGroup.Systems.AfterUpdate(in context);
      }

      _lastTick = tick;
    }
    catch
    {
      _faulted = true;
      throw;
    }
    finally
    {
      _updating = false;
    }
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    try
    {
      foreach (PhaseGroup phaseGroup in _phaseGroups)
      {
        phaseGroup.Systems.Dispose();
      }
    }
    finally
    {
      if (_ownsWorld)
      {
        World.Dispose();
      }
    }
  }

  private void EnsureUsable()
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (_faulted)
    {
      throw new InvalidOperationException(
          "The NSSLC ECS server faulted during an update and must be disposed.");
    }
  }

  private static IReadOnlyList<EcsServerSystemRegistration> CreateSimulationRegistrations(
      IEnumerable<ISystem<EcsServerTickContext>> systems)
  {
    ArgumentNullException.ThrowIfNull(systems);

    List<EcsServerSystemRegistration> registrations = [];
    foreach (ISystem<EcsServerTickContext>? system in systems)
    {
      if (system is null)
      {
        throw new ArgumentException(
            "The NSSLC ECS system collection cannot contain null values.", nameof(systems));
      }

      registrations.Add(new EcsServerSystemRegistration(EcsServerPhase.Simulation, system));
    }

    return registrations.AsReadOnly();
  }

  private sealed class PhaseGroup
  {
    public PhaseGroup(
        EcsServerPhase phase,
        IReadOnlyList<ISystem<EcsServerTickContext>> systems)
    {
      Phase = phase;
      Systems = new Group<EcsServerTickContext>($"NSSLC {phase}", systems.ToArray());
    }

    public EcsServerPhase Phase { get; }

    public Group<EcsServerTickContext> Systems { get; }
  }
}
