namespace MinimalImplementation.Server.Inventory.Content;

/// <summary>
/// Immutable Version4 item bounds and defaults required before inventory operations can be
/// validated. The full item behavior remains owned by the inventory domain.
/// </summary>
/// <remarks>
/// Source evidence was checked in both D:/TRbackup/Version4 and the user-supplied complete
/// v1.4.5.6 source tree. ItemID.Count is 6147 (Version4 ItemID.cs:13814; complete source
/// ItemID.cs:13835) and PrefixID.Count is 98 (PrefixID.cs:206 in both trees). Item.cs sets
/// maxStack to CommonMaxStack (9999; Version4 line 48462, complete source line 48763) and
/// changes it only at Version4 Item.cs lines 1907, 2048, 2069, 2090, 3474, 19244, 19249,
/// 20389, 20394, 20680, and 30834 (complete source lines 2114, 2255, 2276, 2297, 3681,
/// 19451, 19456, 20596, 20601, 20887, and 31041). ItemID.Sets.CanGetPrefixes defaults true
/// except for the IDs listed here (Version4 ItemID.cs:167; complete source ItemID.cs:175).
///
/// Version4 SHA-256: Item.cs 932AA78145344BF9D825FB42ECD71A732BDDCE36474442D9CCB7B06A6F385E55;
/// ItemID.cs 87EEA4B1C4284F3747F2850A0A563D9F29FADBD1D2215BB04159792558838BEE;
/// PrefixID.cs 189C629B0F1AA7A4A78C198D29663C2ABAF850EC796751C154810AEBD7768FFE.
/// Complete-source SHA-256: Item.cs F2A22ABEC0D56DB7CA7D30BBB557377A61B2418C226E8A0584EFEC7E0E722096;
/// ItemID.cs 281C39CA8584D53002D15CC574A65104C87A9D00FDE4B5A710F4538065688035;
/// PrefixID.cs 182D98D1E2EEB38A5666E94A82277CB2C96F9355E2C106E74956701A5AA598A5.
/// The complete-source declarations were checked against the Version4 source before transcribing.
/// </remarks>
public static class ItemContentFacts
{
  public const int ItemTypeCount = 6147;
  public const int PrefixCount = 98;
  public const int CommonMaxStack = 9999;

  private static readonly int[] s_itemsWithoutPrefixes =
  [
    267, 562, 563, 564, 565, 566, 567, 568, 569, 570, 571, 572, 573, 574, 576, 1307,
    1596, 1597, 1598, 1599, 1600, 1601, 1602, 1603, 1604, 1605, 1606, 1607, 1608,
    1609, 1610, 1963, 1964, 1965, 2742, 3044, 3235, 3236, 3237, 3370, 3371, 3796,
    3869, 4077, 4078, 4079, 4080, 4081, 4082, 4237, 4356, 4357, 4358, 4421, 4606,
    4979, 4985, 4990, 4991, 4992, 5006, 5014, 5015, 5016, 5017, 5018, 5019, 5020,
    5021, 5022, 5023, 5024, 5025, 5026, 5027, 5028, 5029, 5030, 5031, 5032, 5033,
    5034, 5035, 5036, 5037, 5038, 5039, 5040, 5044, 5112, 5362, 5538, 5578, 5579,
    5580, 5581, 5582, 5637, 5638, 5639, 6144, 6145, 6146
  ];

  public static bool IsKnownItemType(int itemType) =>
    itemType >= 0 && itemType < ItemTypeCount;

  public static bool IsValidPrefixId(int prefixId) =>
    prefixId >= 0 && prefixId < PrefixCount;

  public static bool CanHavePrefix(int itemType) =>
    IsKnownItemType(itemType) && Array.BinarySearch(s_itemsWithoutPrefixes, itemType) < 0;

  public static bool IsCoin(int itemType) => itemType is >= 71 and <= 74;

  internal static int GetAxePower(int itemType) => itemType switch
  {
    10 => 9,
    45 or 799 or 1190 or 1222 => 15,
    204 => 20,
    217 or 1507 or 3098 or >= 3522 and <= 3525 or 4317 or 5095 or 5519 => 30,
    383 or 991 or 2342 => 14,
    384 or 992 => 17,
    387 or 993 => 20,
    579 or 990 => 22,
    1197 or 1223 => 18,
    1204 or 1224 => 21,
    1232 or 1233 => 23,
    1294 or 2176 or 5295 => 25,
    1305 => 35,
    2772 or 2773 => 27,
    3482 => 12,
    3488 => 11,
    3494 or 3512 => 10,
    3500 => 8,
    3506 => 7,
    3518 => 11,
    _ => 0
  };

  // ItemID.Sets.IsAPickup and IgnoresEncumberingStone contain this same set in v1.4.5.6.
  public static bool IsPickup(int itemType) => itemType is
    58 or 184 or 1734 or 1735 or 1867 or 1868 or 3453 or 3454 or 3455 or 4143;

  // All 101 DefaultToMusicBox item cases agree in both Item.cs source trees.
  // Empty music box 576 uses a separate default and does not create tile 139.
  internal static bool IsPlacedMusicBox(int itemType) => itemType is
    >= 562 and <= 574 or >= 1596 and <= 1610 or >= 1963 and <= 1965
    or 2742 or 3044 or >= 3235 and <= 3237 or 3370 or 3371 or 3796 or 3869
    or >= 4077 and <= 4082 or 4237 or >= 4356 and <= 4358 or 4421 or 4606
    or 4979 or 4985 or >= 4990 and <= 4992 or 5006 or >= 5014 and <= 5040
    or 5044 or 5112 or 5362 or 5538 or 5539 or >= 5578 and <= 5582
    or >= 5637 and <= 5639 or >= 6144 and <= 6146;

  public static bool IsValidPrefixForItem(int itemType, int prefixId) =>
    IsKnownItemType(itemType)
    && IsValidPrefixId(prefixId)
    && (prefixId == 0 || CanHavePrefix(itemType));

  public static bool TryGetMaximumStack(int itemType, out int maximumStack)
  {
    if (!IsKnownItemType(itemType))
    {
      maximumStack = 0;
      return false;
    }

    maximumStack = itemType switch
    {
      58 or 184 or 1734 or 1735 or 1867 or 1868 or 1922 or >= 3453 and <= 3455 => 1,
      71 or 72 or 73 => 100,
      _ => CommonMaxStack
    };
    return true;
  }

  /// <summary>Known rarity after SetDefaults without a prefix; false means missing content.</summary>
  public static bool TryGetUnprefixedRarity(int itemType, out int rarity)
  {
    if (IsPlacedMusicBox(itemType))
    {
      rarity = 4;
      return true;
    }

    // Both Item.cs SetDefaults1 bodies agree. ResetStats starts at zero; these cases
    // have no rarity-changing helper, bait or variant branch. Prefix() can change rarity.
    rarity = 0;
    switch (itemType)
    {
      case 1:
      case 2:
      case 3:
      // Current Item.SetDefaults1: mushroom is consumable, has no OnlyNeedOne or
      // passive-stack override, and therefore stacks through CanPassivelyStackInWorld.
      case 5:
      case 8:
      case 9:
      case 40:
      case 41:
      case >= 71 and <= 74:
      case 97:
      case 530:
        return true;
      case 75:
        rarity = 1;
        return true;
      default:
        return false;
    }
  }

  /// <summary>Returns only source-verified passive stacking facts; unknown is distinct from false.</summary>
  public static bool TryGetPassiveStackEligibility(int itemType, out bool canStack)
  {
    canStack = false;
    if (!TryGetMaximumStack(itemType, out int maximumStack) || itemType == 0)
    {
      return false;
    }

    if (maximumStack == 1)
    {
      return true;
    }

    // DefaultToMusicBox is consumable and creates tile 139. Neither passive-stack
    // nor OnlyNeedOne overrides contain these IDs, so OnlyNeedOne returns false.
    if (IsPlacedMusicBox(itemType))
    {
      canStack = true;
      return true;
    }

    // Complete source: Item.cs:49346, ItemID.Sets.CanPassivelyStackInWorldOverride.
    // The initial content below uses SetDefaults1 and OnlyNeedOneInInventory, not maxStack.
    switch (itemType)
    {
      case 1:
      case >= 71 and <= 75:
        return true;
      case 2:
      case 3:
      case 8:
      case 9:
      case 40:
      case 41:
      case 97:
      case 205:
      case 206:
      case 207:
      case 530:
      case 1128:
        canStack = true;
        return true;
      default:
        return false;
    }
  }
}
