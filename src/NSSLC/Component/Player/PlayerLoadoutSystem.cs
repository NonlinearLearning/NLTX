namespace Terraria.Player;

public sealed class PlayerLoadoutSystem
{
  private readonly HashSet<Guid> _acceptedCommandIds = [];
  private readonly PlayerEquipmentRelationComponent _equipment;
  private readonly PlayerAppearanceSelectionComponent _appearance;
  private readonly PlayerLoadoutStateComponent _loadouts;

  public PlayerLoadoutSystem(
    PlayerEquipmentRelationComponent equipment,
    PlayerAppearanceSelectionComponent appearance,
    PlayerLoadoutStateComponent loadouts)
  {
    ArgumentNullException.ThrowIfNull(equipment);
    ArgumentNullException.ThrowIfNull(appearance);
    ArgumentNullException.ThrowIfNull(loadouts);

    _equipment = equipment;
    _appearance = appearance;
    _loadouts = loadouts;
  }

  public PlayerLoadoutSwitchResult Switch(
    in PlayerLoadoutSwitchCommand command)
  {
    int currentLoadoutIndex = _loadouts.CurrentLoadoutIndex;
    if (command.CommandId == Guid.Empty)
    {
      return PlayerLoadoutSwitchResult.Rejected(
        currentLoadoutIndex,
        PlayerLoadoutSwitchRejectionReason.EmptyCommand);
    }

    if (_acceptedCommandIds.Contains(command.CommandId))
    {
      return PlayerLoadoutSwitchResult.Rejected(
        currentLoadoutIndex,
        PlayerLoadoutSwitchRejectionReason.DuplicateCommand);
    }

    if (!_loadouts.HasValidLoadoutSelection)
    {
      return PlayerLoadoutSwitchResult.Rejected(
        currentLoadoutIndex,
        PlayerLoadoutSwitchRejectionReason.InvalidCurrentLoadout);
    }

    if (command.PlayerIndex == command.MainPlayerIndex &&
      (command.UsingOrReusingItem || command.CCed || command.Dead))
    {
      return PlayerLoadoutSwitchResult.Rejected(
        currentLoadoutIndex,
        PlayerLoadoutSwitchRejectionReason.SwitchingBlocked);
    }

    if ((uint)command.TargetLoadoutIndex >=
      (uint)_loadouts.Loadouts.Count)
    {
      return PlayerLoadoutSwitchResult.Rejected(
        currentLoadoutIndex,
        PlayerLoadoutSwitchRejectionReason.InvalidTargetIndex);
    }

    if (command.TargetLoadoutIndex == currentLoadoutIndex)
    {
      return PlayerLoadoutSwitchResult.Rejected(
        currentLoadoutIndex,
        PlayerLoadoutSwitchRejectionReason.SameLoadout);
    }

    if (!TryCreatePlan(
      command,
      currentLoadoutIndex,
      out PlayerLoadoutSwitchPlan plan))
    {
      return PlayerLoadoutSwitchResult.Rejected(
        currentLoadoutIndex,
        PlayerLoadoutSwitchRejectionReason.InvalidLoadoutState);
    }

    Apply(in plan);
    _acceptedCommandIds.Add(command.CommandId);
    return new PlayerLoadoutSwitchResult(
      Applied: true,
      PreviousLoadoutIndex: currentLoadoutIndex,
      CurrentLoadoutIndex: command.TargetLoadoutIndex,
      RejectionReason: PlayerLoadoutSwitchRejectionReason.None);
  }

  public void ResetForLifecycle()
  {
    _acceptedCommandIds.Clear();
  }

  private bool TryCreatePlan(
    in PlayerLoadoutSwitchCommand command,
    int currentLoadoutIndex,
    out PlayerLoadoutSwitchPlan plan)
  {
    plan = default;
    EquipmentLoadoutState currentLoadout =
      _loadouts.Loadouts[currentLoadoutIndex];
    EquipmentLoadoutState targetLoadout =
      _loadouts.Loadouts[command.TargetLoadoutIndex];

    if (!TryCopy(
        currentLoadout.Equipment,
        PlayerEquipmentRelationComponent.ArmorSlotCount,
        out ItemEntityRef[] currentArmor) ||
      !TryCopy(
        currentLoadout.Dyes,
        PlayerEquipmentRelationComponent.DyeSlotCount,
        out ItemEntityRef[] currentDyes) ||
      !TryCopy(
        currentLoadout.HiddenAccessories,
        PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount,
        out bool[] currentHidden) ||
      !TryCopy(
        targetLoadout.Equipment,
        PlayerEquipmentRelationComponent.ArmorSlotCount,
        out ItemEntityRef[] targetArmor) ||
      !TryCopy(
        targetLoadout.Dyes,
        PlayerEquipmentRelationComponent.DyeSlotCount,
        out ItemEntityRef[] targetDyes) ||
      !TryCopy(
        targetLoadout.HiddenAccessories,
        PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount,
        out bool[] targetHidden) ||
      !TryCopy(
        _equipment.ArmorSlots,
        PlayerEquipmentRelationComponent.ArmorSlotCount,
        out ItemEntityRef[] playerArmor) ||
      !TryCopy(
        _equipment.DyeSlots,
        PlayerEquipmentRelationComponent.DyeSlotCount,
        out ItemEntityRef[] playerDyes) ||
      !TryCopy(
        _appearance.HiddenVisibleAccessories,
        PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount,
        out bool[] playerHidden))
    {
      return false;
    }

    plan = new PlayerLoadoutSwitchPlan(
      command.CommandId,
      currentLoadoutIndex,
      command.TargetLoadoutIndex,
      new EquipmentLoadoutState(playerArmor, playerDyes, playerHidden),
      new EquipmentLoadoutState(currentArmor, currentDyes, currentHidden),
      targetArmor,
      targetDyes,
      targetHidden);
    return true;
  }

  private void Apply(in PlayerLoadoutSwitchPlan plan)
  {
    _loadouts.ReplaceLoadout(
      plan.CurrentLoadoutIndex,
      plan.CurrentLoadoutAfter);
    _loadouts.ReplaceLoadout(
      plan.TargetLoadoutIndex,
      plan.TargetLoadoutAfter);

    Array.Copy(plan.EquipmentAfter, _equipment.ArmorSlots,
      plan.EquipmentAfter.Length);
    Array.Copy(plan.DyeAfter, _equipment.DyeSlots, plan.DyeAfter.Length);
    Array.Copy(
      plan.HiddenAccessoriesAfter,
      _appearance.HiddenVisibleAccessories,
      plan.HiddenAccessoriesAfter.Length);

    _equipment.Revision++;
    _loadouts.CurrentLoadoutIndex = plan.TargetLoadoutIndex;
  }

  private static bool TryCopy<T>(
    IReadOnlyList<T>? source,
    int expectedLength,
    out T[] copy)
  {
    copy = Array.Empty<T>();
    if (source is null || source.Count != expectedLength)
    {
      return false;
    }

    copy = new T[expectedLength];
    for (int index = 0; index < expectedLength; index++)
    {
      copy[index] = source[index];
    }

    return true;
  }
}
