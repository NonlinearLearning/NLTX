using System;
using System.Numerics;

namespace Terraria.World;

/// <summary>
/// Pure pickup eligibility for one world item and one player, ported from Version4
/// <c>WorldItem.FindOwner</c> and <c>Player.GetItemGrabRange</c>.
/// </summary>
/// <remarks>
/// Version4 tests the item rectangle inflated by the grab range against the player hitbox and
/// picks the nearest eligible player by Manhattan distance between centres. Both rules stay pure
/// here; the tick system supplies positions and decides which request to raise.
/// </remarks>
public static class WorldItemPickupRules
{
  /// <summary>Version4 <c>Player.defaultItemGrabRange</c> in pixels.</summary>
  public const int DefaultGrabRange = 42;

  /// <summary>
  /// True when the item rectangle inflated by <paramref name="grabRange"/> intersects the player
  /// hitbox. Version4 compares integer rectangles, so the comparison truncates like
  /// <c>Rectangle</c> does.
  /// </summary>
  public static bool IsWithinGrabRange(
      Vector2 itemPosition,
      Vector2 itemSize,
      Vector2 playerPosition,
      Vector2 playerSize,
      int grabRange)
  {
    if (grabRange < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(grabRange));
    }

    if (!IsFinite(itemPosition) || !IsFinite(itemSize) ||
        !IsFinite(playerPosition) || !IsFinite(playerSize))
    {
      return false;
    }

    int itemLeft = (int)itemPosition.X - grabRange;
    int itemTop = (int)itemPosition.Y - grabRange;
    int itemRight = itemLeft + (int)itemSize.X + grabRange * 2;
    int itemBottom = itemTop + (int)itemSize.Y + grabRange * 2;
    int playerLeft = (int)playerPosition.X;
    int playerTop = (int)playerPosition.Y;
    int playerRight = playerLeft + (int)playerSize.X;
    int playerBottom = playerTop + (int)playerSize.Y;
    return playerLeft < itemRight && itemLeft < playerRight &&
        playerTop < itemBottom && itemTop < playerBottom;
  }

  /// <summary>Manhattan distance between the item and player centres, as Version4 orders them.</summary>
  public static int ManhattanCost(
      Vector2 itemPosition,
      Vector2 itemSize,
      Vector2 playerPosition,
      Vector2 playerSize)
  {
    float itemCenterX = itemPosition.X + itemSize.X / 2f;
    float itemCenterY = itemPosition.Y + itemSize.Y / 2f;
    float playerCenterX = playerPosition.X + playerSize.X / 2f;
    float playerCenterY = playerPosition.Y + playerSize.Y / 2f;
    return (int)(MathF.Abs(itemCenterX - playerCenterX) +
        MathF.Abs(itemCenterY - playerCenterY));
  }

  private static bool IsFinite(Vector2 value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
