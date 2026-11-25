using System;
using System.Collections.Generic;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;

namespace Terraria.Unshared.Npc;

/// <summary>Creates and owns the shared AI registry used by server NPC entities.</summary>
/// <remarks>
/// Behaviors are registered from a sealed <see cref="NpcAiCatalog"/>: the catalog is the single
/// statement of which NPC behaviors exist, what each reads and what each may emit. Construction
/// proves the read half here; the emit half is proven by <see cref="NpcAiIntentAdapter"/>, which
/// owns the dispatch table it validates against.
/// </remarks>
public sealed class NpcAiRuntime : IDisposable
{
  private readonly AiSystem<NpcAiIntent> _system;
  private bool _disposed;

  public NpcAiRuntime(ArchWorld world, NpcAiSnapshotProvider? snapshots = null)
      : this(NpcAiDeclaration.CreateCatalog(), snapshots ?? CreateDefaultSnapshots(world))
  {
    ArgumentNullException.ThrowIfNull(world);
  }

  public NpcAiRuntime(
      NpcAiCatalog catalog,
      NpcAiSnapshotProvider snapshots)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(snapshots);
    if (!catalog.IsSealed)
    {
      throw new ArgumentException(
          "Seal the NPC AI catalog before creating the runtime.", nameof(catalog));
    }

    EnsureProjectable(catalog, snapshots);
    _system = new AiSystem<NpcAiIntent>();
    foreach (NpcAiBehaviorDeclaration declaration in catalog.Declarations)
    {
      _system.Register(declaration.Behavior);
    }

    Entities = new AiEntitySystem<ArchEntity, NpcAiIntent>(_system);
    Catalog = catalog;
    Snapshots = snapshots;
  }

  /// <summary>The declarations this runtime registers.</summary>
  public NpcAiCatalog Catalog { get; }

  public AiEntitySystem<ArchEntity, NpcAiIntent> Entities { get; }

  public NpcAiSnapshotProvider Snapshots { get; }

  public void AttachGuide(ArchEntity entity)
  {
    Attach(entity, AiComponent.For<GuideAiBehavior>());
  }

  public void AttachSlime(ArchEntity entity)
  {
    Attach(entity, AiComponent.For<SlimeAiBehavior>());
  }

  public void Attach(ArchEntity entity, in AiComponent component)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    Entities.Attach(entity, component);
  }

  public bool TryDetach(
      ArchEntity entity,
      out AiEntityResult<ArchEntity, NpcAiIntent> result)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    return Entities.TryDetach(entity, out result);
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    Entities.Dispose();
  }

  private static NpcAiSnapshotProvider CreateDefaultSnapshots(ArchWorld world)
  {
    return new NpcAiSnapshotProvider(
        world ?? throw new ArgumentNullException(nameof(world)));
  }

  private static void EnsureProjectable(
      NpcAiCatalog catalog,
      NpcAiSnapshotProvider snapshots)
  {
    List<string> unprojectable = [];
    foreach (NpcAiBehaviorDeclaration declaration in catalog.Declarations)
    {
      foreach (Type factType in declaration.Behavior.Reads.Components)
      {
        if (!snapshots.FactTypes.Contains(factType))
        {
          unprojectable.Add(
              $"{declaration.Behavior.BehaviorType.FullName} reads {factType.FullName}");
        }
      }
    }

    if (unprojectable.Count != 0)
    {
      throw new InvalidOperationException(
          "The NPC snapshot provider cannot project declared AI inputs: " +
          string.Join("; ", unprojectable));
    }
  }
}
