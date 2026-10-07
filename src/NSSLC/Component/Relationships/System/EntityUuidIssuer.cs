using System;
using System.Collections.Generic;

namespace Terraria.Relationships;

/// <summary>Issues domain UUIDs and prevents reuse across candidate world sessions.</summary>
public sealed class EntityUuidIssuer
{
  private readonly Func<Guid> _createUuid;
  private readonly HashSet<EntityUuid> _issued = new();
  private readonly int _ownerThreadId;

  public EntityUuidIssuer(Func<Guid>? createUuid = null)
  {
    _createUuid = createUuid ?? Guid.NewGuid;
    _ownerThreadId = Environment.CurrentManagedThreadId;
  }

  public EntityUuid Issue()
  {
    VerifyOwnerThread();
    EntityUuid uuid = new(_createUuid());
    if (!_issued.Add(uuid))
    {
      throw new InvalidOperationException("The entity UUID has already been issued by this issuer.");
    }

    return uuid;
  }

  private void VerifyOwnerThread()
  {
    if (_ownerThreadId != Environment.CurrentManagedThreadId)
    {
      throw new InvalidOperationException("Entity UUIDs may only be issued by their owner thread.");
    }
  }
}
