namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-708, P09-709, P09-710, P09-711
// crossSubsystemOwner: item payload, equipment effects, and loadout commits remain integration-review
public sealed class PlayerEquipmentRelationComponent
{
  public const int ArmorSlotCount = 20;

  public const int DyeSlotCount = 10;

  public const int MiscEquipmentSlotCount = 5;

  public const int MiscDyeSlotCount = 5;

  public ItemEntityRef[] ArmorSlots { get; } = new ItemEntityRef[ArmorSlotCount];

  public ItemEntityRef[] DyeSlots { get; } = new ItemEntityRef[DyeSlotCount];

  public ItemEntityRef[] MiscEquipmentSlots { get; } =
    new ItemEntityRef[MiscEquipmentSlotCount];

  public ItemEntityRef[] MiscDyeSlots { get; } = new ItemEntityRef[MiscDyeSlotCount];
}
