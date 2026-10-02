using EntityEcs.Components;
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
      RuntimeEntityId: null,
      ItemType: null);
  }

  public static LeashedAnchorCommand CreateDespawn(
    TileEntityId anchorId,
    TileCoordinate position,
    EntityId runtimeEntityId)
  {
    if (!runtimeEntityId.IsAssigned)
    {
      throw new ArgumentException(
        "A runtime entity identity is required for despawn.",
        nameof(runtimeEntityId));
    }

    return new(
      LeashedAnchorCommandKind.Despawn,
      anchorId,
      position,
      runtimeEntityId,
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
      RuntimeEntityId: null,
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
      RuntimeEntityId: null,
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

