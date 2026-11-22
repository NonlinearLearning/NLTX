using System;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Ai;

namespace Terraria.Npc;

/// <summary>Creates and owns the shared AI registry used by server NPC entities.</summary>
public sealed class NpcAiRuntime : IDisposable
{
  private readonly AiSystem<NpcAiIntent> _system;
  private bool _disposed;

  public NpcAiRuntime(ArchWorld world, NpcAiSnapshotProvider? snapshots = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    _system = new AiSystem<NpcAiIntent>();
    _system.Register<GuideAiBehavior>();
    _system.Register<SlimeAiBehavior>();
    Entities = new AiEntitySystem<ArchEntity, NpcAiIntent>(_system);
    Snapshots = snapshots ?? new NpcAiSnapshotProvider(world);
  }

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
}
