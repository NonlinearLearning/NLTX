using System;

using Terraria.Projectile;
using Terraria.WorldSession.Components;

namespace Terraria.NonAuthoritative.Simulation;

/// <summary>
/// Runs the ordered projectile pass as the world's Projectile simulation phase.
/// </summary>
public sealed class ProjectileSimulationTickPhase : IWorldSimulationTickPhase
{
  private readonly ProjectileTickCoordinator _coordinator;
  private readonly Func<WorldSimulationTickContext, IProjectileTickAdapter> _adapterFactory;

  public ProjectileSimulationTickPhase(
    ProjectileTickCoordinator coordinator,
    IProjectileTickAdapter adapter)
  {
    ArgumentNullException.ThrowIfNull(coordinator);
    ArgumentNullException.ThrowIfNull(adapter);
    _coordinator = coordinator;
    _adapterFactory = _ => adapter;
  }

  public ProjectileSimulationTickPhase(
    ProjectileTickCoordinator coordinator,
    Func<WorldSimulationTickContext, IProjectileTickAdapter> adapterFactory)
  {
    ArgumentNullException.ThrowIfNull(coordinator);
    ArgumentNullException.ThrowIfNull(adapterFactory);

    _coordinator = coordinator;
    _adapterFactory = adapterFactory;
  }

  public WorldSimulationPhase Phase => WorldSimulationPhase.Projectile;

  public ProjectileTickResult LastTickResult { get; private set; }

  public void Execute(WorldSimulationTickContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    if (!context.WorldSnapshot.IsCommitted ||
        context.WorldSnapshot.Descriptor is not WorldDescriptorSnapshotValue descriptor)
    {
      throw new InvalidOperationException(
        "Projectile simulation requires a committed world descriptor.");
    }

    WorldBounds bounds = descriptor.Bounds;
    var projectileBounds = new ProjectileWorldBounds(
      (float)bounds.Left,
      (float)bounds.Top,
      (float)bounds.Right,
      (float)bounds.Bottom);
    IProjectileTickAdapter adapter = _adapterFactory(context) ??
      throw new InvalidOperationException(
        "The projectile adapter factory returned no adapter for this tick.");
    LastTickResult = _coordinator.Tick(adapter, projectileBounds);
  }
}
