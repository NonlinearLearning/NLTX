namespace Terraria.Items.InventoryContainers;

public static class ItemEquipmentAppearanceQuery
{
  public static ItemEquipmentAppearanceSnapshot CreateSnapshot(
    ItemEquipmentAppearanceComponent appearance)
  {
    ArgumentNullException.ThrowIfNull(appearance);
    return new ItemEquipmentAppearanceSnapshot(
      appearance.WornArmor,
      appearance.HeadSlot,
      appearance.BodySlot,
      appearance.LegSlot,
      appearance.Social,
      appearance.Vanity,
      appearance.NewAndShiny,
      appearance.HasVanityEffects,
      appearance.HandOnSlot,
      appearance.HandOffSlot,
      appearance.BackSlot,
      appearance.FrontSlot,
      appearance.ShoeSlot,
      appearance.WaistSlot,
      appearance.WingSlot,
      appearance.ShieldSlot,
      appearance.NeckSlot,
      appearance.FaceSlot,
      appearance.BalloonSlot,
      appearance.BeardSlot,
      appearance.VoiceSlot);
  }
}
