using System;
using System.Collections.Generic;
using Terraria.Relationships;

namespace EntityEcs;

public sealed class EntityIdentityRegistry
{
  private readonly Func<Guid> _createUuid;
  private readonly int _ownerThreadId;
  private readonly HashSet<EntityUuid> _issued = new();
  private readonly Dictionary<EntityUuid, RuntimeEntityHandle> _liveByUuid = new();
  private readonly Dictionary<RuntimeEntityHandle, EntityUuid> _liveByHandle = new();

  public EntityIdentityRegistry(Func<Guid>? createUuid = null)
  {
    _createUuid = createUuid ?? Guid.NewGuid;
    _ownerThreadId = Environment.CurrentManagedThreadId;
  }

  internal EntityUuid Register(RuntimeEntityHandle handle)
  {
    VerifyOwnerThread();
    if (!handle.IsAssigned)
    {
      throw new ArgumentException("An identity requires a valid runtime handle.", nameof(handle));
    }

    EntityUuid uuid = new(_createUuid());
    if (!_issued.Add(uuid))
    {
      throw new InvalidOperationException("The entity UUID has already been issued by this registry.");
    }

    _liveByUuid.Add(uuid, handle);
    _liveByHandle.Add(handle, uuid);
    return uuid;
  }

  internal bool TryResolve(EntityReference reference, out RuntimeEntityHandle handle)
  {
    VerifyOwnerThread();
    if (reference.IsEmpty ||
        !_liveByUuid.TryGetValue(reference.EntityId, out handle) ||
        handle.RuntimeId != reference.RuntimeId)
    {
      handle = default;
      return false;
    }

    return true;
  }

  internal bool Unregister(RuntimeEntityHandle handle, EntityUuid uuid)
  {
    VerifyOwnerThread();
    if (!_liveByHandle.TryGetValue(handle, out EntityUuid registeredUuid) || registeredUuid != uuid)
    {
      return false;
    }

    _liveByHandle.Remove(handle);
    return _liveByUuid.Remove(uuid);
  }

  internal bool IsOwnedByCurrentThread => _ownerThreadId == Environment.CurrentManagedThreadId;

  private void VerifyOwnerThread()
  {
    if (!IsOwnedByCurrentThread)
    {
      throw new InvalidOperationException("Entity identities may only be accessed by their owner thread.");
    }
  }
}
