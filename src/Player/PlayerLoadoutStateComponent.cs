namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-1411, P09-1412
// crossSubsystemOwner: loadout swap, item payload, network, and persistence remain integration-review
public sealed class PlayerLoadoutStateComponent
{
  public const int LoadoutCount = 3;

  private readonly EquipmentLoadoutState[] _loadouts =
    CreateEmptyLoadouts();

  public IReadOnlyList<EquipmentLoadoutState> Loadouts => _loadouts;

  public int CurrentLoadoutIndex { get; internal set; }

  public bool HasValidLoadoutSelection =>
    (uint)CurrentLoadoutIndex < (uint)_loadouts.Length;

  private static EquipmentLoadoutState[] CreateEmptyLoadouts()
  {
    EquipmentLoadoutState[] loadouts = new EquipmentLoadoutState[LoadoutCount];
    for (int index = 0; index < loadouts.Length; index++)
    {
      loadouts[index] = new EquipmentLoadoutState(
        new ItemEntityRef[PlayerEquipmentRelationComponent.ArmorSlotCount],
        new ItemEntityRef[PlayerEquipmentRelationComponent.DyeSlotCount],
        new bool[PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount]);
    }

    return loadouts;
  }
}
