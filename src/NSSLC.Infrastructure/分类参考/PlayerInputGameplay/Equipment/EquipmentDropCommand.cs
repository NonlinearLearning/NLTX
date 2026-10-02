namespace NLTX.PlayerInputGameplay.Equipment;

public readonly record struct EquipmentDropCommand(int SlotKind, int SlotIndex, int ItemType);

public interface IEquipmentDropPort
{
  void Drop(EquipmentDropCommand command);
}
