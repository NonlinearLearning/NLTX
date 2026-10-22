using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>Read-only mount/stool movement result validated before PlayerOwner commits it.</summary>
public sealed class PlayerRuntimeMovementPreparation
{
  private readonly PlayerOwner _players;
  private readonly Player _player;
  private readonly IPlayerRuntimeWorldCapture _world;
  private readonly IPlayerEquipmentMetadataSource _equipmentMetadata;
  private readonly InventoryStore _inventories;
  private readonly CommitVersion _inventoryRevision;
  private readonly CommitVersion _metadataRevision;

  internal PlayerRuntimeMovementPreparation(
    PlayerOwner players,
    SessionBinding binding,
    Player player,
    PlayerMovementInput movement,
    PlayerRuntimeSpatialState runtimeState,
    IPlayerRuntimeWorldCapture world,
    IPlayerEquipmentMetadataSource equipmentMetadata,
    CommitVersion metadataRevision,
    InventoryStore inventories,
    CommitVersion inventoryRevision)
  {
    ArgumentNullException.ThrowIfNull(players);
    _players = players;
    Binding = binding;
    _player = player;
    Movement = movement;
    RuntimeState = runtimeState;
    _world = world;
    _equipmentMetadata = equipmentMetadata;
    _metadataRevision = metadataRevision;
    _inventories = inventories;
    _inventoryRevision = inventoryRevision;
  }

  public SessionBinding Binding { get; }

  public PlayerMovementInput Movement { get; }

  public PlayerRuntimeSpatialState RuntimeState { get; }

  public OwnerCommitResult Validate()
  {
    if (!_players.TryGet(Binding.PlayerHandle, out Player? currentPlayer)
      || !ReferenceEquals(currentPlayer, _player)
      || _player.Epoch != Binding.Epoch || _player.Handle != Binding.PlayerHandle
      || _world.Epoch != Binding.Epoch || _world.PlayerHandle != Binding.PlayerHandle
      || _world.PlayerVersion != _player.Version || !_world.Revision.IsValid
      || _equipmentMetadata.Revision != _metadataRevision
      || !_inventories.TryCapture(Binding.PlayerHandle.Id, out InventorySnapshot? inventory)
      || inventory is null
      || !PlayerRuntimeFactsCaptureService.IsInventorySnapshotValid(
        inventory, Binding.PlayerHandle.Id, InventorySlotLayout.ArmorStart + 10)
      || inventory.Revision != _inventoryRevision)
    {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The Player runtime movement sources changed after preparation.");
    }

    return _world.Validate();
  }
}
