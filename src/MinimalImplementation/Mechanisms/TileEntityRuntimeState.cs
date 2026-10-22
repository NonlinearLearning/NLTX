using System.Collections.ObjectModel;
using Terraria.Items;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Mechanisms;

public enum MechanismEntityLifecycle
{
  Active,
  Stopping,
  Retired
}

public sealed class TileEntityRuntimeState
{
  private ReadOnlyCollection<ItemState> _items;

  internal TileEntityRuntimeState(
    MechanismEntityHandle handle,
    int persistenceId,
    int networkId,
    TileEntityDefinition definition,
    TileCoordinate anchor,
    MechanismSectionCoordinate section,
    IReadOnlyList<ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType)
  {
    Handle = handle;
    PersistenceId = persistenceId;
    NetworkId = networkId;
    Definition = definition;
    Anchor = anchor;
    Section = section;
    _items = Array.AsReadOnly(items.ToArray());
    NpcIndex = npcIndex;
    LogicCheck = logicCheck;
    LogicOn = logicOn;
    Pose = pose;
    AnchorItemType = anchorItemType;
    Lifecycle = MechanismEntityLifecycle.Active;
  }

  public MechanismEntityHandle Handle { get; }

  public int PersistenceId { get; }

  public int NetworkId { get; }

  public TileEntityDefinition Definition { get; }

  public TileCoordinate Anchor { get; }

  public MechanismSectionCoordinate Section { get; }

  public IReadOnlyList<ItemState> Items => _items;

  public short NpcIndex { get; private set; }

  public byte LogicCheck { get; private set; }

  public bool LogicOn { get; private set; }

  public byte Pose { get; private set; }

  public short AnchorItemType { get; private set; }

  public MechanismEntityLifecycle Lifecycle { get; private set; }

  public long Revision { get; private set; } = 1;

  internal void ApplyState(
    IReadOnlyList<ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType)
  {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(items);
    long nextRevision = checked(Revision + 1);
    ReadOnlyCollection<ItemState> itemCopy = Array.AsReadOnly(items.ToArray());
    _items = itemCopy;
    NpcIndex = npcIndex;
    LogicCheck = logicCheck;
    LogicOn = logicOn;
    Pose = pose;
    AnchorItemType = anchorItemType;
    Revision = nextRevision;
  }

  internal void BeginRemoval()
  {
    if (Lifecycle == MechanismEntityLifecycle.Retired)
    {
      return;
    }
    Lifecycle = MechanismEntityLifecycle.Stopping;
  }

  internal void Retire()
  {
    if (Lifecycle == MechanismEntityLifecycle.Retired)
    {
      return;
    }
    long nextRevision = checked(Revision + 1);
    Lifecycle = MechanismEntityLifecycle.Retired;
    Revision = nextRevision;
  }

  internal void EnsureCanRetire()
  {
    _ = checked(Revision + 1);
  }

  public TileEntitySnapshot CapturePersistence()
  {
    if (Lifecycle == MechanismEntityLifecycle.Retired)
    {
      throw new InvalidOperationException("Retired TileEntity state cannot be captured.");
    }
    return new TileEntitySnapshot(
      new TileEntityId(PersistenceId),
      new TileEntityTypeId(Definition.Type),
      Anchor,
      _items,
      NpcIndex,
      LogicCheck,
      LogicOn,
      Pose,
      AnchorItemType);
  }

  private void EnsureActive()
  {
    if (Lifecycle != MechanismEntityLifecycle.Active)
    {
      throw new InvalidOperationException("The TileEntity is not active.");
    }
  }
}

public sealed record TileEntityCreateRequest(
  byte Type,
  TileCoordinate Anchor,
  MechanismSectionCoordinate Section,
  MechanismTileSnapshot AnchorTile,
  IReadOnlyList<ItemState> Items,
  short NpcIndex = -1,
  byte LogicCheck = 0,
  bool LogicOn = false,
  byte Pose = 0,
  short AnchorItemType = 0,
  int? NetworkId = null);

public sealed record TileEntityRestoreRequest(
  TileEntitySnapshot Snapshot,
  MechanismSectionCoordinate Section,
  MechanismTileSnapshot AnchorTile,
  int? NetworkId = null);

public sealed record TileEntityRemovalResult(
  MechanismEntityHandle Handle,
  TileEntityRuntimeState State,
  TileEntitySnapshot PersistenceSnapshot);

public sealed record TileEntityRestoreCandidate(
  MechanismEntityHandle Identity,
  TileEntityRestoreRequest Request);

public enum TileEntityLoadCommitStatus
{
  Applied,
  Stale
}

public sealed class PreparedTileEntityLoad
{
  internal PreparedTileEntityLoad(
    TileEntityRegistry owner,
    long registryRevision,
    IReadOnlyList<TileEntityRuntimeState> states,
    long nextPersistenceId,
    long nextNetworkId,
    TileEntityLoadCommitResult appliedResult)
  {
    Owner = owner;
    RegistryRevision = registryRevision;
    States = states;
    NextPersistenceId = nextPersistenceId;
    NextNetworkId = nextNetworkId;
    AppliedResult = appliedResult;
  }

  internal TileEntityRegistry Owner { get; }

  internal long RegistryRevision { get; }

  internal IReadOnlyList<TileEntityRuntimeState> States { get; }

  internal long NextPersistenceId { get; }

  internal long NextNetworkId { get; }

  internal TileEntityLoadCommitResult AppliedResult { get; }

  public int Count => States.Count;

  internal bool IsConsumed { get; private set; }

  internal bool TryConsume()
  {
    if (IsConsumed)
    {
      return false;
    }
    IsConsumed = true;
    return true;
  }
}

public sealed record TileEntityLoadCommitResult(
  TileEntityLoadCommitStatus Status,
  IReadOnlyList<MechanismEntityHandle> Handles,
  IReadOnlyList<TileEntitySnapshot> Snapshots)
{
  public static TileEntityLoadCommitResult Stale { get; } = new(
    TileEntityLoadCommitStatus.Stale,
    Array.Empty<MechanismEntityHandle>(),
    Array.Empty<TileEntitySnapshot>());
}
