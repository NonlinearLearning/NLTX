namespace NLTX.PlayerInputGameplay.Equipment;

public sealed class EquipmentLoadoutSwapSystem
{
  public void Swap(EquipmentLoadoutComponent first, EquipmentLoadoutComponent second)
  {
    ArgumentNullException.ThrowIfNull(first);
    ArgumentNullException.ThrowIfNull(second);
    var armor = (int[])first.Armor.Clone();
    var dye = (int[])first.Dye.Clone();
    var hide = (bool[])first.Hide.Clone();
    Array.Copy(second.Armor, first.Armor, first.Armor.Length);
    Array.Copy(armor, second.Armor, second.Armor.Length);
    Array.Copy(second.Dye, first.Dye, first.Dye.Length);
    Array.Copy(dye, second.Dye, second.Dye.Length);
    Array.Copy(second.Hide, first.Hide, first.Hide.Length);
    Array.Copy(hide, second.Hide, second.Hide.Length);
  }
}
