namespace Terraria.Player;

public sealed class PlayerContainerRelationSystem
{
  private readonly HashSet<Guid> _acceptedCommandIds = [];
  private readonly PlayerContainerRelationComponent _relations;

  public PlayerContainerRelationSystem(
    PlayerContainerRelationComponent relations)
  {
    ArgumentNullException.ThrowIfNull(relations);
    _relations = relations;
  }

  public PlayerContainerRelationResult Apply(
    in PlayerContainerRelationCommand command)
  {
    PlayerContainerRef currentContainer = ReadContainer(command.Slot);
    VoidVaultState currentVoidVaultState = _relations.VoidVaultState;

    if (command.CommandId == Guid.Empty)
    {
      return PlayerContainerRelationResult.Rejected(
        command.Slot,
        currentContainer,
        currentVoidVaultState,
        PlayerContainerRelationRejectionReason.EmptyCommand);
    }

    if (_acceptedCommandIds.Contains(command.CommandId))
    {
      return PlayerContainerRelationResult.Rejected(
        command.Slot,
        currentContainer,
        currentVoidVaultState,
        PlayerContainerRelationRejectionReason.DuplicateCommand);
    }

    if (!Enum.IsDefined(command.Slot))
    {
      return PlayerContainerRelationResult.Rejected(
        command.Slot,
        currentContainer,
        currentVoidVaultState,
        PlayerContainerRelationRejectionReason.InvalidSlot);
    }

    if (command.Slot == PlayerContainerRelationSlot.VoidVault)
    {
      if (!command.Container.IsEmpty)
      {
        return PlayerContainerRelationResult.Rejected(
          command.Slot,
          currentContainer,
          currentVoidVaultState,
          PlayerContainerRelationRejectionReason.EmptyContainer);
      }

      if (command.IsOpen && !command.IsAvailable)
      {
        return PlayerContainerRelationResult.Rejected(
          command.Slot,
          currentContainer,
          currentVoidVaultState,
          PlayerContainerRelationRejectionReason.InvalidVoidVaultState);
      }

      _relations.VoidVaultState = new VoidVaultState(
        command.IsAvailable,
        command.IsOpen);
      _acceptedCommandIds.Add(command.CommandId);
      return Applied(command.Slot, PlayerContainerRef.None);
    }

    if (!command.IsAvailable && !command.Container.IsEmpty)
    {
      return PlayerContainerRelationResult.Rejected(
        command.Slot,
        currentContainer,
        currentVoidVaultState,
        PlayerContainerRelationRejectionReason.EmptyContainer);
    }

    WriteContainer(command.Slot, command.Container);
    _acceptedCommandIds.Add(command.CommandId);
    return Applied(command.Slot, command.Container);
  }

  public void ResetForLifecycle()
  {
    _acceptedCommandIds.Clear();
  }

  private PlayerContainerRelationResult Applied(
    PlayerContainerRelationSlot slot,
    PlayerContainerRef container)
  {
    return new PlayerContainerRelationResult(
      Applied: true,
      Slot: slot,
      CurrentContainer: container,
      CurrentVoidVaultState: _relations.VoidVaultState,
      RejectionReason: PlayerContainerRelationRejectionReason.None);
  }

  private PlayerContainerRef ReadContainer(
    PlayerContainerRelationSlot slot)
  {
    return slot switch
    {
      PlayerContainerRelationSlot.Bank => _relations.Bank,
      PlayerContainerRelationSlot.Bank2 => _relations.Bank2,
      PlayerContainerRelationSlot.Bank3 => _relations.Bank3,
      PlayerContainerRelationSlot.Bank4 => _relations.Bank4,
      _ => PlayerContainerRef.None,
    };
  }

  private void WriteContainer(
    PlayerContainerRelationSlot slot,
    PlayerContainerRef container)
  {
    switch (slot)
    {
      case PlayerContainerRelationSlot.Bank:
        _relations.Bank = container;
        break;
      case PlayerContainerRelationSlot.Bank2:
        _relations.Bank2 = container;
        break;
      case PlayerContainerRelationSlot.Bank3:
        _relations.Bank3 = container;
        break;
      case PlayerContainerRelationSlot.Bank4:
        _relations.Bank4 = container;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
