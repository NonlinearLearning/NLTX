using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Host;

/// <summary>Exact source-slot debit captured from the client's post-drop equipment upload.</summary>
internal sealed class InventoryItemDropPreparation : IWorldItemDropPreparation {
  private readonly InventoryStore _inventories;
  private readonly InventorySlotSnapshot _source;
  private bool _committed;

  public EntityHandle PlayerHandle { get; }
  public SessionEpoch Epoch { get; }
  public CommitVersion PlayerVersion { get; }
  public CommitVersion SourceRevision => InventoryRevision;
  public ulong WorldEpoch { get; }
  public CommitVersion WorldVersion { get; }
  public CommitVersion InventoryRevision { get; }
  public IReadOnlyList<WorldItemChange> WorldChanges => Array.Empty<WorldItemChange>();
  public IReadOnlyList<WorldItemSpawn> Spawns { get; }
  public IReadOnlyList<InventoryMutation> InventoryDebits { get; }
  public IReadOnlyList<WorldItemHandle> ReleaseClaims => Array.Empty<WorldItemHandle>();

  public InventoryItemDropPreparation(InventoryStore inventories, SessionBinding binding,
      PlayerEntity player, WorldHeaderCapture world, InventorySnapshot inventory,
      InventorySlotSnapshot source, WorldItemSpawn spawn) {
    _inventories = inventories;
    _source = source;
    PlayerHandle = binding.PlayerHandle;
    Epoch = binding.Epoch;
    PlayerVersion = player.Version;
    WorldEpoch = world.WorldEpoch;
    WorldVersion = world.Version;
    InventoryRevision = inventory.Revision;
    Spawns = Array.AsReadOnly(new[] { spawn });
    InventoryDebits = Array.AsReadOnly(new[] {
      new InventoryMutation(source.SlotId, source.Generation,
        source.Item.WithStack(source.Item.Stack - spawn.InitialState.Item.Stack))
    });
  }

  public OwnerCommitResult Validate() {
    if (_committed || !_inventories.TryCapture(PlayerHandle.Id, out InventorySnapshot? inventory)
        || inventory is null || inventory.Revision != InventoryRevision ||
        inventory.Slots[_source.SlotId] != _source) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
        "The drop source slot changed after its debit was prepared.");
    }
    InventoryItemState output = Spawns[0].InitialState.Item;
    if (output.Type != _source.Item.Type || output.Prefix != _source.Item.Prefix ||
        output.Stack <= 0 || output.Stack > _source.Item.Stack) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The drop output does not match its source inventory slot.");
    }
    return OwnerCommitResult.Applied(SourceRevision);
  }

  public void Commit() {
    _committed = true;
  }
}
