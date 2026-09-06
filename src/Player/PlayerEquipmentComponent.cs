namespace Terraria.Player;

public sealed class PlayerEquipmentComponent
{
  public const int ArmorSlotCount = 20;
  public const int DyeSlotCount = 10;
  public const int MiscEquipmentSlotCount = 5;
  public const int MiscDyeSlotCount = 5;
  public const int HiddenAccessorySlotCount = 10;

  public ItemEntityRef[] ArmorSlots { get; } = new ItemEntityRef[ArmorSlotCount];

  public ItemEntityRef[] DyeSlots { get; } = new ItemEntityRef[DyeSlotCount];

  public ItemEntityRef[] MiscEquipmentSlots { get; } =
    new ItemEntityRef[MiscEquipmentSlotCount];

  public ItemEntityRef[] MiscDyeSlots { get; } = new ItemEntityRef[MiscDyeSlotCount];

  public bool[] HiddenAccessorySlots { get; } =
    new bool[HiddenAccessorySlotCount];

  public int CurrentLoadoutIndex { get; set; }
}
