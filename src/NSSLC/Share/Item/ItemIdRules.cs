using System;

namespace Terraria.Item;

/// <summary>
/// Item identity rules shared by the domain and the Version4 protocol boundary.
/// </summary>
/// <remarks>
/// <see cref="ItemId"/> is the authoritative identity. The protocol still transports the same
/// number as an <see cref="short"/> item type, so the two conversions live here and nowhere else.
/// </remarks>
public static class ItemIdRules {
  /// <summary>
  /// True when the value matches a constant in the reference client's item table.
  /// </summary>
  /// <remarks>
  /// <see cref="ItemId.None"/> is value zero and is a known constant, so an all-default
  /// <see cref="ItemStack"/> is empty because its quantity is zero, not because the id is unknown.
  /// </remarks>
  public static bool IsKnown(ItemId itemId) {
    return Enum.IsDefined(itemId);
  }

  /// <summary>
  /// Projects the identity onto the Int16 item type carried by the Version4 protocol.
  /// </summary>
  public static short ToNetworkType(ItemId itemId) {
    int value = (int)itemId;
    if (value is < short.MinValue or > short.MaxValue) {
      throw new ArgumentOutOfRangeException(
          nameof(itemId), value, "The item id does not fit the protocol item type.");
    }

    return (short)value;
  }

  /// <summary>
  /// Reads a protocol item type back into the domain identity.
  /// </summary>
  public static bool TryFromNetworkType(int itemType, out ItemId itemId) {
    itemId = (ItemId)itemType;
    return IsKnown(itemId);
  }
}
