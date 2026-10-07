using System;
using System.Collections.Generic;
using Terraria.Relationships;

namespace EntityEcs;

public sealed class EntityIdentityRegistry
{
  private readonly int _ownerThreadId;
  private readonly EntityUuidIssuer _uuidIssuer;
  private readonly Dictionary<EntityUuid, RuntimeEntityHandle> _liveByUuid = new();
  private readonly Dictionary<RuntimeEntityHandle, EntityUuid> _liveByHandle = new();

  public EntityIdentityRegistry(Func<Guid>? createUuid = null)
      : this(new EntityUuidIssuer(createUuid))
  {
  }

  public EntityIdentityRegistry(EntityUuidIssuer uuidIssuer)
  {
    _uuidIssuer = uuidIssuer ?? throw new ArgumentNullException(nameof(uuidIssuer));
    _ownerThreadId = Environment.CurrentManagedThreadId;
  }

  public EntityUuidIssuer UuidIssuer => _uuidIssuer;

  internal EntityUuid Register(RuntimeEntityHandle handle)
  {
    VerifyOwnerThread();
    if (!handle.IsAssigned)
    {
      throw new ArgumentException("An identity requires a valid runtime handle.", nameof(handle));
    }

    EntityUuid uuid = _uuidIssuer.Issue();

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
