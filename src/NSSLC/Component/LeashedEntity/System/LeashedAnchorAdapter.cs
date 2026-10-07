using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Converts anchor lifecycle and item events into commands for an external host owner.
/// </summary>
public static class LeashedAnchorAdapter
{
  public static LeashedAnchorCommand CreateRespawn(
    TileEntityId anchorId,
    TileCoordinate position)
  {
    return new(
      LeashedAnchorCommandKind.Respawn,
      anchorId,
      position,
      RuntimeEntityReference: null,
      ItemType: null);
  }

  public static LeashedAnchorCommand CreateDespawn(
    TileEntityId anchorId,
    TileCoordinate position,
    EntityReference runtimeEntityReference)
  {
    if (runtimeEntityReference.IsEmpty || runtimeEntityReference.Scope == EntityReferenceScope.None)
    {
      throw new ArgumentException(
        "A runtime entity reference is required for despawn.",
        nameof(runtimeEntityReference));
    }

    return new(
      LeashedAnchorCommandKind.Despawn,
      anchorId,
      position,
      runtimeEntityReference,
      ItemType: null);
  }

  public static LeashedAnchorCommand CreateInsertItem(
    TileEntityId anchorId,
    TileCoordinate position,
    int itemType)
  {
    ValidateItemType(itemType);
    return new(
      LeashedAnchorCommandKind.InsertItem,
      anchorId,
      position,
      RuntimeEntityReference: null,
      itemType);
  }

  public static LeashedAnchorCommand CreateDropItem(
    TileEntityId anchorId,
    TileCoordinate position,
    int itemType)
  {
    ValidateItemType(itemType);
    return new(
      LeashedAnchorCommandKind.DropItem,
      anchorId,
      position,
      RuntimeEntityReference: null,
      itemType);
  }

  private static void ValidateItemType(int itemType)
  {
    if (itemType <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemType));
    }
  }
}
