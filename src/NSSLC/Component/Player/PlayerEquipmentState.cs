namespace Terraria.Player;

public sealed class PlayerEquipmentState
{
  private readonly ItemEntityRef[] _armorAndAccessorySlots = new ItemEntityRef[20];
  private readonly ItemEntityRef[] _dyeSlots = new ItemEntityRef[10];
  private readonly ItemEntityRef[] _miscEquipmentSlots = new ItemEntityRef[5];
  private readonly ItemEntityRef[] _miscDyeSlots = new ItemEntityRef[5];
  private readonly EquipmentLoadoutState[] _loadouts = new EquipmentLoadoutState[3];
  private readonly bool[] _hiddenAccessorySlots = new bool[10];

  public IReadOnlyList<ItemEntityRef> ArmorAndAccessorySlots => _armorAndAccessorySlots;

  public IReadOnlyList<ItemEntityRef> DyeSlots => _dyeSlots;

  public IReadOnlyList<ItemEntityRef> MiscEquipmentSlots => _miscEquipmentSlots;

  public IReadOnlyList<ItemEntityRef> MiscDyeSlots => _miscDyeSlots;

  public IReadOnlyList<EquipmentLoadoutState> Loadouts => _loadouts;

  public int CurrentLoadoutIndex { get; set; }

  public IReadOnlyList<bool> HiddenAccessorySlots => _hiddenAccessorySlots;

  public EquipmentLoadoutView CurrentLoadout
  {
    get
    {
      if (!HasValidLoadoutSelection)
      {
        return new EquipmentLoadoutView(Array.Empty<ItemEntityRef>(), Array.Empty<ItemEntityRef>(), Array.Empty<bool>());
      }

      EquipmentLoadoutState loadout = _loadouts[CurrentLoadoutIndex];
      return new EquipmentLoadoutView(loadout.Equipment, loadout.Dyes, loadout.HiddenAccessories);
    }
  }

  public bool HasValidLoadoutSelection => (uint)CurrentLoadoutIndex < (uint)_loadouts.Length;
}
