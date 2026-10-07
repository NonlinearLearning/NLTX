using Terraria.Relationships;

namespace Terraria.WorldStorage;

/// <summary>
/// Index projection from the persistent TileEntity ID to its single runtime entity.
/// Type-specific mutable state lives in components attached to <see cref="RuntimeHandle"/>.
/// </summary>
public sealed class TileEntityRecord
{
  internal TileEntityRecord(
    TileEntityId id,
    TileEntityTypeId type,
    TileCoordinate anchor,
    RuntimeEntityHandle runtimeHandle,
    TileEntitySnapshot persistencePayload)
  {
    Id = id;
    Type = type;
    Anchor = anchor;
    RuntimeHandle = runtimeHandle;
    PersistencePayload = persistencePayload;
  }

  public TileEntityId Id { get; }

  public TileEntityTypeId Type { get; }

  public TileCoordinate Anchor { get; }

  public bool RequiresUpdates => Type.Value is 0 or 2;

  /// <summary>
  /// Immutable persistence input retained for fields with no live simulation owner.
  /// Supported runtime fields are always read from their attached capability component.
  /// </summary>
  internal TileEntitySnapshot PersistencePayload { get; }

  internal RuntimeEntityHandle RuntimeHandle { get; }
}
