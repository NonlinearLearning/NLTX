using System;
using System.Collections.Generic;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Mechanisms;

namespace MinimalImplementation.Server.Host;

/// <summary>Projects the root identity registry into P05's combat boundary.</summary>
public sealed class CombatIdentityAuthority : ICombatEntityIdentityAuthority
{
  private readonly EntityIdentityRegistry _identities;

  public CombatIdentityAuthority(EntityIdentityRegistry identities)
  {
    _identities = identities ?? throw new ArgumentNullException(nameof(identities));
  }

  public long CurrentWorldScope => _identities.CurrentWorldScope;

  public CombatEntityIdentity Create()
  {
    EntityHandle handle = _identities.Create();
    return new CombatEntityIdentity(
      handle.Id,
      handle.WorldScope.Value,
      handle.Generation.Value);
  }

  public bool IsActive(CombatEntityIdentity identity)
  {
    return TryResolveActiveHandle(identity, out _);
  }

  internal bool TryResolveActiveHandle(CombatEntityIdentity identity, out EntityHandle handle) {
    handle = default;
    if (!identity.IsValid) {
      return false;
    }

    EntityHandle candidate = ToHandle(identity);
    if (!_identities.IsActive(candidate)) {
      return false;
    }

    handle = candidate;
    return true;
  }

  public bool Retire(CombatEntityIdentity identity)
  {
    return identity.IsValid && _identities.Retire(ToHandle(identity));
  }

  private static EntityHandle ToHandle(CombatEntityIdentity identity)
  {
    return new EntityHandle(
      identity.Id,
      new WorldScopeId(identity.WorldScope),
      new EntityGeneration(identity.Generation));
  }
}

/// <summary>Projects the root identity registry into P06's mechanism boundary.</summary>
public sealed class MechanismIdentityAuthority : IMechanismIdentityAuthority
{
  private readonly EntityIdentityRegistry _identities;

  public MechanismIdentityAuthority(EntityIdentityRegistry identities)
  {
    _identities = identities ?? throw new ArgumentNullException(nameof(identities));
  }

  public MechanismEntityHandle Create()
  {
    EntityHandle handle = _identities.Create();
    return new MechanismEntityHandle(
      handle.Id,
      handle.WorldScope.Value,
      handle.Generation.Value);
  }

  public bool IsActive(MechanismEntityHandle handle)
  {
    return handle.IsValid && _identities.IsActive(ToHandle(handle));
  }

  public bool TryRetireMany(IReadOnlyList<MechanismEntityHandle> handles)
  {
    ArgumentNullException.ThrowIfNull(handles);
    foreach (MechanismEntityHandle handle in handles)
    {
      if (!IsActive(handle))
      {
        return false;
      }
    }

    foreach (MechanismEntityHandle handle in handles)
    {
      if (!_identities.Retire(ToHandle(handle)))
      {
        throw new InvalidOperationException(
          "The root identity registry changed while retiring mechanism identities.");
      }
    }

    return true;
  }

  private static EntityHandle ToHandle(MechanismEntityHandle handle)
  {
    return new EntityHandle(
      handle.EntityUuid,
      new WorldScopeId(handle.WorldScope),
      new EntityGeneration(handle.Generation));
  }
}
