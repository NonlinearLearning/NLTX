namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-783..P09-803, P09-1326..P09-1348
// crossSubsystemOwner: ArmorID bounds, item metadata adapters, and renderer consumers remain integration-review
public sealed class PlayerEquipmentProjectionSystem
{
  public PlayerEquipmentProjectionResult Project(
    PlayerEquipmentRelationComponent equipment,
    PlayerAppearanceSelectionComponent appearance,
    IPlayerEquipmentVisualItemQuery itemQuery,
    PlayerEquipmentColorProjectionComponent colors,
    PlayerVisibleEquipmentSelectionComponent visible,
    in PlayerEquipmentProjectionInput input)
  {
    ArgumentNullException.ThrowIfNull(equipment);
    ArgumentNullException.ThrowIfNull(appearance);
    ArgumentNullException.ThrowIfNull(itemQuery);
    ArgumentNullException.ThrowIfNull(colors);
    ArgumentNullException.ThrowIfNull(visible);

    if (input.UsableArmorSlots.Count !=
      PlayerEquipmentRelationComponent.ArmorSlotCount)
    {
      return Rejected(
        equipment.Revision,
        PlayerEquipmentProjectionRejectionReason.InvalidUsableArmorSlotCount);
    }

    if (input.HiddenVisibleAccessories.Count !=
      PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount)
    {
      return Rejected(
        equipment.Revision,
        PlayerEquipmentProjectionRejectionReason.InvalidHiddenAccessoryCount);
    }

    int missingItemMetadataCount = 0;
    PlayerEquipmentVisualItemSnapshot[] armor = ReadSlots(
      equipment.ArmorSlots,
      itemQuery,
      ref missingItemMetadataCount);
    PlayerEquipmentVisualItemSnapshot[] dyes = ReadSlots(
      equipment.DyeSlots,
      itemQuery,
      ref missingItemMetadataCount);

    ResetColors(colors);
    ResetVisibleSelection(visible);

    colors.CHead = ReadDyeId(dyes, 0);
    colors.CBody = ReadDyeId(dyes, 1);
    colors.CLegs = input.WearsRobe ? colors.CBody : ReadDyeId(dyes, 2);

    ApplyVisibleArmorSelection(armor, visible);
    ApplyArmorDyes(
      armor,
      dyes,
      input,
      colors);

    return new PlayerEquipmentProjectionResult(
      Applied: true,
      EquipmentRevision: equipment.Revision,
      MissingItemMetadataCount: missingItemMetadataCount,
      RejectionReason: PlayerEquipmentProjectionRejectionReason.None);
  }

  private static PlayerEquipmentProjectionResult Rejected(
    long revision,
    PlayerEquipmentProjectionRejectionReason reason)
  {
    return new PlayerEquipmentProjectionResult(
      Applied: false,
      EquipmentRevision: revision,
      MissingItemMetadataCount: 0,
      RejectionReason: reason);
  }

  private static PlayerEquipmentVisualItemSnapshot[] ReadSlots(
    IReadOnlyList<ItemEntityRef> slots,
    IPlayerEquipmentVisualItemQuery itemQuery,
    ref int missingItemMetadataCount)
  {
    PlayerEquipmentVisualItemSnapshot[] snapshots =
      new PlayerEquipmentVisualItemSnapshot[slots.Count];
    for (int index = 0; index < slots.Count; index++)
    {
      ItemEntityRef item = slots[index];
      if (item.IsEmpty)
      {
        snapshots[index] = PlayerEquipmentVisualItemSnapshot.Air;
        continue;
      }

      if (itemQuery.TryGetItem(item, out PlayerEquipmentVisualItemSnapshot snapshot) &&
        snapshot.Entity == item)
      {
        snapshots[index] = snapshot;
        continue;
      }

      missingItemMetadataCount++;
      snapshots[index] = PlayerEquipmentVisualItemSnapshot.Air;
    }

    return snapshots;
  }

  private static int ReadDyeId(
    IReadOnlyList<PlayerEquipmentVisualItemSnapshot> dyes,
    int index)
  {
    return (uint)index < (uint)dyes.Count && !dyes[index].IsAir
      ? dyes[index].DyeId
      : 0;
  }

  private static void ApplyVisibleArmorSelection(
    IReadOnlyList<PlayerEquipmentVisualItemSnapshot> armor,
    PlayerVisibleEquipmentSelectionComponent visible)
  {
    visible.Head = ReadSlotValue(armor, 0, static item => item.HeadSlot);
    visible.Body = ReadSlotValue(armor, 1, static item => item.BodySlot);
    visible.Legs = ReadSlotValue(armor, 2, static item => item.LegSlot);

    int vanityHead = ReadSlotValue(armor, 10, static item => item.HeadSlot);
    int vanityBody = ReadSlotValue(armor, 11, static item => item.BodySlot);
    int vanityLegs = ReadSlotValue(armor, 12, static item => item.LegSlot);
    if (vanityHead >= 0)
    {
      visible.Head = vanityHead;
    }

    if (vanityBody >= 0)
    {
      visible.Body = vanityBody;
    }

    if (vanityLegs >= 0)
    {
      visible.Legs = vanityLegs;
    }
  }

  private static int ReadSlotValue(
    IReadOnlyList<PlayerEquipmentVisualItemSnapshot> slots,
    int index,
    Func<PlayerEquipmentVisualItemSnapshot, int> selector)
  {
    if ((uint)index >= (uint)slots.Count || slots[index].IsAir)
    {
      return -1;
    }

    return selector(slots[index]);
  }

  private static void ApplyArmorDyes(
    IReadOnlyList<PlayerEquipmentVisualItemSnapshot> armor,
    IReadOnlyList<PlayerEquipmentVisualItemSnapshot> dyes,
    in PlayerEquipmentProjectionInput input,
    PlayerEquipmentColorProjectionComponent colors)
  {
    for (int armorIndex = 0; armorIndex < armor.Count; armorIndex++)
    {
      if (!input.UsableArmorSlots[armorIndex] || armor[armorIndex].IsAir)
      {
        continue;
      }

      int dyeIndex = armorIndex % PlayerEquipmentRelationComponent.DyeSlotCount;
      int dyeId = ReadDyeId(dyes, dyeIndex);
      PlayerEquipmentVisualItemSnapshot armorItem = armor[armorIndex];
      bool hidden = armorIndex < PlayerEquipmentRelationComponent.DyeSlotCount &&
        input.HiddenVisibleAccessories[armorIndex];

      if (armorItem.ShieldSlot > 0 &&
        (colors.CShieldFallback == -1 || !hidden))
      {
        colors.CShieldFallback = dyeId;
      }

      bool wingLike = armorItem.IsWingLike || armorItem.WingSlot > 0;
      if (!wingLike && hidden)
      {
        continue;
      }

      SetLayerColors(armorItem, dyeId, colors);
    }
  }

  private static void SetLayerColors(
    PlayerEquipmentVisualItemSnapshot armorItem,
    int dyeId,
    PlayerEquipmentColorProjectionComponent colors)
  {
    if (armorItem.HandOnSlot > 0)
    {
      colors.CHandOn = dyeId;
    }

    if (armorItem.HandOffSlot > 0)
    {
      colors.CHandOff = dyeId;
    }

    if (armorItem.BackSlot > 0)
    {
      if (armorItem.DrawBackInBackpackLayer)
      {
        colors.CBackpack = dyeId;
      }
      else if (armorItem.DrawBackInTailLayer)
      {
        colors.CTail = dyeId;
      }
      else
      {
        colors.CBack = dyeId;
      }
    }

    if (armorItem.FrontSlot > 0)
    {
      colors.CFront = dyeId;
    }

    if (armorItem.ShoeSlot > 0)
    {
      colors.CShoe = dyeId;
    }

    if (armorItem.WaistSlot > 0)
    {
      colors.CWaist = dyeId;
    }

    if (armorItem.ShieldSlot > 0)
    {
      colors.CShield = dyeId;
    }

    if (armorItem.NeckSlot > 0)
    {
      colors.CNeck = dyeId;
    }

    if (armorItem.FaceSlot > 0)
    {
      if (armorItem.DrawFaceInHeadLayer)
      {
        colors.CFaceHead = dyeId;
      }
      else if (armorItem.DrawFaceInMaskLayer)
      {
        colors.CFaceMask = dyeId;
      }
      else if (armorItem.DrawFaceInFlowerLayer)
      {
        colors.CFaceFlower = dyeId;
      }
      else
      {
        colors.CFace = dyeId;
      }
    }

    if (armorItem.BalloonSlot > 0)
    {
      if (armorItem.DrawBalloonInFrontOfBackArmLayer)
      {
        colors.CBalloonFront = dyeId;
      }
      else
      {
        colors.CBalloon = dyeId;
      }
    }
  }

  private static void ResetColors(PlayerEquipmentColorProjectionComponent colors)
  {
    colors.CHead = 0;
    colors.CBody = 0;
    colors.CLegs = 0;
    colors.CHandOn = 0;
    colors.CHandOff = 0;
    colors.CBack = 0;
    colors.CFront = 0;
    colors.CShoe = 0;
    colors.CWaist = 0;
    colors.CShield = 0;
    colors.CNeck = 0;
    colors.CFace = 0;
    colors.CFaceHead = 0;
    colors.CFaceFlower = 0;
    colors.CFaceMask = 0;
    colors.CBalloon = 0;
    colors.CBalloonFront = 0;
    colors.CBackpack = 0;
    colors.CTail = 0;
    colors.CShieldFallback = -1;
  }

  private static void ResetVisibleSelection(
    PlayerVisibleEquipmentSelectionComponent visible)
  {
    visible.Head = -1;
    visible.Body = -1;
    visible.Legs = -1;
    visible.Coat = -1;
    visible.HandOn = -1;
    visible.HandOff = -1;
    visible.Back = -1;
    visible.Front = -1;
    visible.Shoe = -1;
    visible.Waist = -1;
    visible.Shield = -1;
    visible.Neck = -1;
    visible.Face = -1;
    visible.Balloon = -1;
    visible.Backpack = -1;
    visible.Tail = -1;
    visible.FaceHead = -1;
    visible.FaceFlower = -1;
    visible.FaceMask = -1;
    visible.BalloonFront = -1;
    visible.Beard = -1;
  }
}
