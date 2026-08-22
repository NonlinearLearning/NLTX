using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Commands;

namespace Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Systems;

public sealed class FoodPlatterDestructionCommitSystem
{
  private const ushort FoodPlatterTileType = 520;

  public bool TryCommit(
    WorldGrid world,
    Dictionary<int, TileEntityPersistentState> tileEntities,
    FoodPlatterDestructionBatch batch,
    List<ItemStack> droppedItems,
    out FoodPlatterDestructionCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(tileEntities);
    ArgumentNullException.ThrowIfNull(droppedItems);
    if (batch.Sequence < 0 || !world.Contains(batch.TileX, batch.TileY))
    {
      result = FoodPlatterDestructionCommitResult.Failed("Food Platter batch was invalid.");
      return false;
    }

    WorldTile tile = world.GetTile(batch.TileX, batch.TileY);
    if (!tile.IsActive || tile.Type != FoodPlatterTileType)
    {
      result = FoodPlatterDestructionCommitResult.Failed("Food Platter tile was not present.");
      return false;
    }

    TileEntityPersistentState? entity = null;
    if (batch.RemoveTileEntity &&
        (!tileEntities.TryGetValue(batch.EntityId, out entity) ||
        entity.TileX != batch.TileX || entity.TileY != batch.TileY))
    {
      result = FoodPlatterDestructionCommitResult.Failed(
        "Food Platter tile entity was not present.");
      return false;
    }

    bool entityRemoved = false;
    if (entity is not null)
    {
      entityRemoved = tileEntities.Remove(batch.EntityId);
      if (!entityRemoved)
      {
        result = FoodPlatterDestructionCommitResult.Failed(
          "Food Platter tile entity could not be removed.");
        return false;
      }
    }

    TileChangeCommand tileCommand = new(
      batch.Sequence,
      batch.TileX,
      batch.TileY,
      TileChangeKind.Kill,
      TileType: 0);
    if (!new TileChangeCommitSystem().TryCommit(
          world,
          new[] { tileCommand },
          out TileChangeCommitResult tileResult))
    {
      if (entity is not null)
      {
        tileEntities.Add(batch.EntityId, entity);
      }

      result = FoodPlatterDestructionCommitResult.Failed(tileResult.FailureReason ??
        "Food Platter tile could not be removed.");
      return false;
    }

    bool itemDropped = !batch.DroppedItem.IsEmpty;
    if (itemDropped)
    {
      droppedItems.Add(batch.DroppedItem);
    }

    result = new FoodPlatterDestructionCommitResult(true, entityRemoved, itemDropped, null);
    return true;
  }
}
