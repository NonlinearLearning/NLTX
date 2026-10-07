namespace Terraria.Player;

// Applies the ten visibility bits read by the Version4 loadout packet.
// It does not own loadout or equipment relations.
public sealed class PlayerAccessoryVisibilitySystem
{
  private readonly HashSet<Guid> _acceptedCommandIds = [];
  private readonly PlayerAppearanceSelectionComponent _appearance;

  public PlayerAccessoryVisibilitySystem(
    PlayerAppearanceSelectionComponent appearance)
  {
    ArgumentNullException.ThrowIfNull(appearance);
    _appearance = appearance;
  }

  public PlayerAccessoryVisibilityResult Apply(
    in PlayerAccessoryVisibilityApplyCommand command)
  {
    PlayerAccessoryVisibilitySnapshot current = Snapshot();
    if (command.CommandId == Guid.Empty)
    {
      return PlayerAccessoryVisibilityResult.Rejected(
        current,
        PlayerAccessoryVisibilityRejectionReason.EmptyCommand);
    }

    if (_acceptedCommandIds.Contains(command.CommandId))
    {
      return PlayerAccessoryVisibilityResult.Rejected(
        current,
        PlayerAccessoryVisibilityRejectionReason.DuplicateCommand);
    }

    if (_appearance.HiddenVisibleAccessories.Length !=
      PlayerAppearanceSelectionComponent.HiddenVisibleAccessoryCount)
    {
      return PlayerAccessoryVisibilityResult.Rejected(
        current,
        PlayerAccessoryVisibilityRejectionReason.InvalidState);
    }

    for (int index = 0;
      index < _appearance.HiddenVisibleAccessories.Length;
      index++)
    {
      _appearance.HiddenVisibleAccessories[index] =
        (command.VisibilityMask & (1 << index)) != 0;
    }

    _acceptedCommandIds.Add(command.CommandId);
    return new PlayerAccessoryVisibilityResult(
      Applied: true,
      Snapshot: Snapshot(),
      RejectionReason: PlayerAccessoryVisibilityRejectionReason.None);
  }

  public PlayerAccessoryVisibilitySnapshot Snapshot()
  {
    bool[] hiddenAccessories = new bool[
      _appearance.HiddenVisibleAccessories.Length];
    Array.Copy(
      _appearance.HiddenVisibleAccessories,
      hiddenAccessories,
      hiddenAccessories.Length);

    ushort visibilityMask = 0;
    for (int index = 0; index < hiddenAccessories.Length; index++)
    {
      if (hiddenAccessories[index])
      {
        visibilityMask |= (ushort)(1 << index);
      }
    }

    return new PlayerAccessoryVisibilitySnapshot(
      visibilityMask,
      Array.AsReadOnly(hiddenAccessories));
  }

  public void ResetForLifecycle()
  {
    _acceptedCommandIds.Clear();
  }
}
