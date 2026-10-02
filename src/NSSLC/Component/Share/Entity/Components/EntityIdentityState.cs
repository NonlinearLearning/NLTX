using System;

namespace EntityEcs.Components;

// status: proposed
public sealed class EntityIdentityState
{
  public EntityIdentityState(
    EntityId runtimeEntityId = default,
    int? compatibilitySlot = null,
    NetworkEntityId? networkId = null,
    EntityId? persistentId = null,
    uint generation = 0)
  {
    if (compatibilitySlot is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(compatibilitySlot));
    }

    if (networkId.HasValue && !networkId.Value.IsAssigned)
    {
      throw new ArgumentOutOfRangeException(nameof(networkId));
    }

    if (persistentId.HasValue && !persistentId.Value.IsAssigned)
    {
      throw new ArgumentOutOfRangeException(nameof(persistentId));
    }

    if (!compatibilitySlot.HasValue && generation != 0)
    {
      throw new InvalidOperationException(
        "Generation requires a compatibility slot.");
    }

    RuntimeEntityId = runtimeEntityId;
    CompatibilitySlot = compatibilitySlot;
    NetworkId = networkId;
    PersistentId = persistentId;
    Generation = generation;
  }

  public EntityId RuntimeEntityId { get; }

  public int? CompatibilitySlot { get; }

  public NetworkEntityId? NetworkId { get; }

  public EntityId? PersistentId { get; }

  public uint Generation { get; }

  public bool HasRuntimeIdentity => RuntimeEntityId.IsAssigned;

  public bool HasCompatibilityIdentity => CompatibilitySlot.HasValue;

  public bool HasNetworkIdentity => NetworkId.HasValue;

  public bool HasPersistentIdentity => PersistentId.HasValue;
}
