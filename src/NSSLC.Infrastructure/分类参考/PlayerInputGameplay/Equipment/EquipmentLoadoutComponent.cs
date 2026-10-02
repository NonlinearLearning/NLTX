namespace NLTX.PlayerInputGameplay.Equipment;

public sealed class EquipmentLoadoutComponent
{
  public EquipmentLoadoutComponent(int armorSlots = 20, int dyeSlots = 10, int hideSlots = 10)
  {
    if (armorSlots <= 0 || dyeSlots <= 0 || hideSlots <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(armorSlots));
    }

    Armor = new int[armorSlots];
    Dye = new int[dyeSlots];
    Hide = new bool[hideSlots];
  }

  public int[] Armor { get; }

  public int[] Dye { get; }

  public bool[] Hide { get; }

  public void SwapWith(EquipmentLoadoutComponent other)
  {
    ArgumentNullException.ThrowIfNull(other);
    if (Armor.Length != other.Armor.Length || Dye.Length != other.Dye.Length || Hide.Length != other.Hide.Length)
    {
      throw new ArgumentException("Loadouts must use matching slot counts.", nameof(other));
    }

    Array.Copy(Armor, other.Armor, Armor.Length);
    Array.Copy(other.Armor, Armor, Armor.Length);
    Array.Copy(Dye, other.Dye, Dye.Length);
    Array.Copy(other.Dye, Dye, Dye.Length);
    Array.Copy(Hide, other.Hide, Hide.Length);
    Array.Copy(other.Hide, Hide, Hide.Length);
  }
}
