using System;
using System.Collections.Generic;
using System.Linq;
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
  private readonly IReadOnlyList<EcsServerSystemRegistration> _orderedRegistrations;
  private readonly IReadOnlyList<string> _orderedRegistrationKeys;
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

    List<EcsServerSystemRegistration> registrationList = [];
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
      registrationList.Add(registration);
    }

    IReadOnlyList<ScheduledRegistration> schedule = BuildSchedule(registrationList);
    World = world;
    List<PhaseGroup> phaseGroups = [];
    foreach (EcsServerPhase phase in Enum.GetValues<EcsServerPhase>())
    {
      List<ScheduledRegistration> phaseRegistrations = [];
      foreach (ScheduledRegistration registration in schedule)
      {
        if (registration.Registration.Phase == phase)
        {
          phaseRegistrations.Add(registration);
        }
      }

      phaseGroups.Add(new PhaseGroup(phase, phaseRegistrations));
    }

    _phaseGroups = phaseGroups.AsReadOnly();
    _orderedRegistrations = schedule
        .Select(static registration => registration.Registration)
        .ToArray()
        .AsReadOnly();
    _orderedRegistrationKeys = schedule
        .Select(static registration => registration.EffectiveKey)
        .ToArray()
        .AsReadOnly();
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

  /// <summary>Gets registrations in their validated execution order.</summary>
  public IReadOnlyList<EcsServerSystemRegistration> OrderedRegistrations =>
      _orderedRegistrations;

  /// <summary>
  /// Gets the effective keys in execution order. Legacy registrations use synthetic keys.
  /// </summary>
  public IReadOnlyList<string> OrderedRegistrationKeys => _orderedRegistrationKeys;

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

    using EcsServerCommandBuffer commands = new(World, tick, nameof(EcsServer));
    var context = new EcsServerTickContext(World, tick, deltaTime, commands);
    _updating = true;
    try
    {
      foreach (PhaseGroup phaseGroup in _phaseGroups)
      {
        if (phaseGroup.Phase == EcsServerPhase.Projection && !commands.IsPlayedBack)
        {
          commands.Playback(World);
        }

        phaseGroup.Systems.BeforeUpdate(in context);
        phaseGroup.Systems.Update(in context);
        phaseGroup.Systems.AfterUpdate(in context);

        if (phaseGroup.Phase == EcsServerPhase.Commit && !commands.IsPlayedBack)
        {
          commands.Playback(World);
        }
      }

      if (!commands.IsPlayedBack)
      {
        commands.Playback(World);
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

  private static IReadOnlyList<ScheduledRegistration> BuildSchedule(
      IReadOnlyList<EcsServerSystemRegistration> registrations)
  {
    List<ScheduledRegistration> entries = [];
    Dictionary<string, ScheduledRegistration> byKey = new(StringComparer.Ordinal);
    for (int index = 0; index < registrations.Count; index++)
    {
      EcsServerSystemRegistration registration = registrations[index];
      string effectiveKey = registration.Key ?? $"legacy.{registration.Phase}.{index}";
      ScheduledRegistration entry = new(index, effectiveKey, registration);
      if (!byKey.TryAdd(effectiveKey, entry))
      {
        ScheduledRegistration existing = byKey[effectiveKey];
        throw new EcsServerSchedulingException(
            EcsServerSchedulingFailureKind.DuplicateKey,
            $"Duplicate ECS registration key '{effectiveKey}' at registration indexes " +
            $"{existing.RegistrationIndex} and {index} " +
            $"(phase '{registration.Phase}').");
      }

      entries.Add(entry);
    }

    List<int>[] outgoing = new List<int>[entries.Count];
    HashSet<int>[] outgoingSet = new HashSet<int>[entries.Count];
    List<DependencyEdge> dependencyEdges = [];
    for (int index = 0; index < entries.Count; index++)
    {
      outgoing[index] = [];
      outgoingSet[index] = [];
    }

    foreach (ScheduledRegistration current in entries)
    {
      AddDependencies(
          current,
          current.Registration.After,
          dependencyName: "After",
          dependencyEdges,
          byKey,
          outgoing,
          outgoingSet);
      AddDependencies(
          current,
          current.Registration.Before,
          dependencyName: "Before",
          dependencyEdges,
          byKey,
          outgoing,
          outgoingSet);
    }

    int[] indegree = new int[entries.Count];
    foreach (List<int> targets in outgoing)
    {
      foreach (int target in targets)
      {
        indegree[target]++;
      }
    }

    List<ScheduledRegistration> sorted = [];
    foreach (EcsServerPhase phase in Enum.GetValues<EcsServerPhase>())
    {
      PriorityQueue<int, int> ready = new();
      foreach (ScheduledRegistration entry in entries)
      {
        if (entry.Registration.Phase == phase && indegree[entry.RegistrationIndex] == 0)
        {
          ready.Enqueue(entry.RegistrationIndex, entry.RegistrationIndex);
        }
      }

      int phaseCount = entries.Count(entry => entry.Registration.Phase == phase);
      int phaseSortedCount = 0;
      while (ready.TryDequeue(out int registrationIndex, out _))
      {
        ScheduledRegistration entry = entries[registrationIndex];
        sorted.Add(entry);
        phaseSortedCount++;
        foreach (int target in outgoing[registrationIndex])
        {
          if (entries[target].Registration.Phase != phase)
          {
            continue;
          }

          indegree[target]--;
          if (indegree[target] == 0)
          {
            ready.Enqueue(target, target);
          }
        }
      }

      if (phaseSortedCount != phaseCount)
      {
        throw CreateCycleException(phase, entries, outgoing, dependencyEdges, indegree);
      }
    }

    return sorted.AsReadOnly();
  }

  private static void AddDependencies(
      ScheduledRegistration current,
      IReadOnlyList<string> dependencyKeys,
      string dependencyName,
      List<DependencyEdge> dependencyEdges,
      IReadOnlyDictionary<string, ScheduledRegistration> byKey,
      IReadOnlyList<List<int>> outgoing,
      IReadOnlyList<HashSet<int>> outgoingSet)
  {
    foreach (string dependencyKey in dependencyKeys)
    {
      if (!byKey.TryGetValue(dependencyKey, out ScheduledRegistration? dependency))
      {
        throw new EcsServerSchedulingException(
            EcsServerSchedulingFailureKind.MissingDependency,
            $"Missing ECS {dependencyName} dependency '{dependencyKey}' declared by " +
            $"'{current.EffectiveKey}' in phase '{current.Registration.Phase}'." +
            FormatReason(current.Registration));
      }

      if (dependency.Registration.Key is null)
      {
        throw new EcsServerSchedulingException(
            EcsServerSchedulingFailureKind.MissingDependency,
            $"ECS {dependencyName} dependency '{dependencyKey}' declared by " +
            $"'{current.EffectiveKey}' targets a legacy registration. Dependencies must target " +
            $"explicit keys." + FormatReason(current.Registration));
      }

      int source = dependencyName == "After"
          ? dependency.RegistrationIndex
          : current.RegistrationIndex;
      int target = dependencyName == "After"
          ? current.RegistrationIndex
          : dependency.RegistrationIndex;
      if (dependency.Registration.Phase != current.Registration.Phase)
      {
        throw new EcsServerSchedulingException(
            EcsServerSchedulingFailureKind.CrossPhaseDependency,
            $"Cross-phase ECS {dependencyName} dependency from '{current.EffectiveKey}' " +
            $"(phase '{current.Registration.Phase}') to '{dependency.EffectiveKey}' " +
            $"(phase '{dependency.Registration.Phase}') is invalid. Phase order is a fixed " +
            $"contract; use the phase boundary instead." + FormatReason(current.Registration));
      }

      string edgeReason =
          $"{current.EffectiveKey} declares {dependencyName} '{dependency.EffectiveKey}'" +
          FormatReason(current.Registration);
      if (outgoingSet[source].Add(target))
      {
        outgoing[source].Add(target);
        dependencyEdges.Add(new DependencyEdge(source, target, edgeReason));
      }
    }
  }

  private static EcsServerSchedulingException CreateCycleException(
      EcsServerPhase phase,
      IReadOnlyList<ScheduledRegistration> entries,
      IReadOnlyList<List<int>> outgoing,
      IReadOnlyList<DependencyEdge> dependencyEdges,
      IReadOnlyList<int> indegree)
  {
    HashSet<int> remaining = [];
    foreach (ScheduledRegistration entry in entries)
    {
      if (entry.Registration.Phase == phase && indegree[entry.RegistrationIndex] > 0)
      {
        remaining.Add(entry.RegistrationIndex);
      }
    }

    List<int> cycle = [];
    Dictionary<int, int> colors = [];
    List<int> path = [];
    foreach (int start in remaining.OrderBy(static index => index))
    {
      if (FindCycle(start))
      {
        break;
      }
    }

    string cycleText = cycle.Count == 0
        ? string.Join(", ", remaining.OrderBy(index => index).Select(index => entries[index].EffectiveKey))
        : string.Join(" -> ", cycle.Select(index => entries[index].EffectiveKey));
    string edgeText = string.Join(
        "; ",
        dependencyEdges
            .Where(edge => remaining.Contains(edge.Source) && remaining.Contains(edge.Target))
            .Select(edge => edge.Reason));
    return new EcsServerSchedulingException(
        EcsServerSchedulingFailureKind.Cycle,
        $"Cycle detected in ECS phase '{phase}': {cycleText}. " +
        $"Remaining nodes: {string.Join(", ", remaining.OrderBy(index => index).Select(index => entries[index].EffectiveKey))}. " +
        $"Edges: {edgeText}");

    bool FindCycle(int node)
    {
      colors[node] = 1;
      path.Add(node);
      foreach (int target in outgoing[node])
      {
        if (!remaining.Contains(target))
        {
          continue;
        }

        if (!colors.TryGetValue(target, out int color))
        {
          if (FindCycle(target))
          {
            return true;
          }
        }
        else if (color == 1)
        {
          int cycleStart = path.IndexOf(target);
          cycle = path.Skip(cycleStart).Append(target).ToList();
          return true;
        }
      }

      colors[node] = 2;
      path.RemoveAt(path.Count - 1);
      return false;
    }
  }

  private static string FormatReason(EcsServerSystemRegistration registration)
  {
    return registration.Reason is null ? string.Empty : $" Reason: {registration.Reason}";
  }

  private sealed record ScheduledRegistration(
      int RegistrationIndex,
      string EffectiveKey,
      EcsServerSystemRegistration Registration);

  private sealed record DependencyEdge(int Source, int Target, string Reason);

  private sealed class PhaseGroup
  {
    public PhaseGroup(
        EcsServerPhase phase,
        IReadOnlyList<ScheduledRegistration> registrations)
    {
      Phase = phase;
      Systems = new Group<EcsServerTickContext>(
          $"NSSLC {phase}",
          registrations.Select(static registration => registration.Registration.System).ToArray());
    }

    public EcsServerPhase Phase { get; }

    public Group<EcsServerTickContext> Systems { get; }
  }
}
