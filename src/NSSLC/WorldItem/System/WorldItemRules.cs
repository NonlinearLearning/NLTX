using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.Session;
using Terraria.Item;

namespace Terraria.World;

/// <summary>
/// Pure validation and slot arithmetic for world items.
/// </summary>
/// <remarks>
/// <see cref="WorldItemStore" /> owns the table, lock, mutation queue and revision field; every
/// decision about whether an item may exist, change or claim a slot is computed here.
/// </remarks>
public static class WorldItemRules
{
  /// <summary>True when a new item may occupy a slot with the supplied facts.</summary>
  /// <remarks>
  /// 物品类型必须是 <see cref="ItemId" /> 里的已知值：权威世界状态只保存共享
  /// <see cref="ItemStack" /> 能表达的身份，未知类型在此拒绝而不是先存下再试图投影。
  /// </remarks>
  public static bool IsValidItem(
      int itemType,
      int stack,
      Vector2 position,
      Vector2 velocity)
  {
    return itemType > 0 && stack > 0 &&
        ItemIdRules.TryFromNetworkType(itemType, out _) &&
        IsValidPosition(position) && IsValidVelocity(velocity);
  }

  /// <summary>True for finite coordinates inside the bounded world envelope.</summary>
  public static bool IsValidPosition(Vector2 position)
  {
    return float.IsFinite(position.X) && float.IsFinite(position.Y) &&
        MathF.Abs(position.X) <= 1_000_000f && MathF.Abs(position.Y) <= 1_000_000f;
  }

  /// <summary>True for finite velocities inside the bounded item envelope.</summary>
  public static bool IsValidVelocity(Vector2 velocity)
  {
    return float.IsFinite(velocity.X) && float.IsFinite(velocity.Y) &&
        MathF.Abs(velocity.X) <= 256f && MathF.Abs(velocity.Y) <= 256f;
  }

  /// <summary>True when the actor owns or does not contest the slot reservation.</summary>
  public static bool CanMutate(WorldItemState item, SessionEpoch actor)
  {
    ArgumentNullException.ThrowIfNull(item);
    return item.ReservedFor is null || item.ReservedFor == actor;
  }

  /// <summary>Lowest unoccupied slot number, or -1 when the table is full.</summary>
  public static short FindFreeSlot(IReadOnlyDictionary<short, WorldItemState> items)
  {
    ArgumentNullException.ThrowIfNull(items);
    for (short slot = 0; slot < WorldItemStore.SlotCount; slot++)
    {
      if (!items.ContainsKey(slot))
      {
        return slot;
      }
    }

    return -1;
  }

  /// <summary>Next item revision, or a thrown failure when the counter is exhausted.</summary>
  public static long NextRevision(long revision)
  {
    if (revision == long.MaxValue)
    {
      throw new InvalidOperationException("The world item revision reached its maximum value.");
    }

    return revision + 1;
  }
}
