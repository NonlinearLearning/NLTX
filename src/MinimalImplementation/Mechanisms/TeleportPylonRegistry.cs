using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Maintains the pylon lookup derived from committed type-7 TileEntities.</summary>
public sealed class TeleportPylonRegistry
{
  public sealed class RemovalPreparation
  {
    private readonly TeleportPylonRegistry _owner;
    private readonly long _baseRevision;
    private readonly long _nextRevision;
    private readonly Dictionary<MechanismEntityHandle, TeleportPylonSnapshot> _identities;
    private readonly Dictionary<TileCoordinate, TeleportPylonSnapshot> _anchors;
    private bool _committed;

    public TeleportPylonSnapshot Snapshot { get; }

    internal RemovalPreparation(TeleportPylonRegistry owner, TeleportPylonSnapshot snapshot)
    {
      _owner = owner;
      _baseRevision = owner.Revision;
      _nextRevision = checked(owner.Revision + 1);
      _identities = new(owner._byIdentity);
      _anchors = new(owner._byAnchor);
      _identities.Remove(snapshot.Identity);
      _anchors.Remove(snapshot.Anchor);
      Snapshot = snapshot;
    }

    public bool Validate()
    {
      return !_committed && _owner.Revision == _baseRevision;
    }

    /// <summary>Requires all participants validated in one exclusive window, without writes.</summary>
    public void Commit()
    {
      if (_committed)
      {
        throw new InvalidOperationException("The pylon removal was already committed.");
      }
      _owner._byIdentity = _identities;
      _owner._byAnchor = _anchors;
      _owner.Revision = _nextRevision;
      _committed = true;
    }
  }

  public sealed class RegistrationPreparation
  {
    private readonly TeleportPylonRegistry _owner;
    private readonly TileEntityRuntimeState _entity;
    private readonly long _entityRevision;
    private readonly long _baseRevision;
    private readonly Dictionary<MechanismEntityHandle, TeleportPylonSnapshot> _identities;
    private readonly Dictionary<TileCoordinate, TeleportPylonSnapshot> _anchors;
    private readonly long _nextRevision;
    private readonly long _nextOrdinal;
    private bool _committed;

    public TeleportPylonSnapshot Snapshot { get; }

    internal RegistrationPreparation(
      TeleportPylonRegistry owner,
      TileEntityRuntimeState entity,
      TeleportPylonSnapshot snapshot)
    {
      _owner = owner;
      _entity = entity;
      _entityRevision = entity.Revision;
      _baseRevision = owner.Revision;
      _nextRevision = checked(owner.Revision + 1);
      _nextOrdinal = checked(owner._nextOrdinal + 1);
      _identities = new(owner._byIdentity);
      _anchors = new(owner._byAnchor);
      _identities.Add(entity.Handle, snapshot);
      _anchors.Add(entity.Anchor, snapshot);
      Snapshot = snapshot;
    }

    public bool Validate()
    {
      return !_committed && _owner.Revision == _baseRevision &&
        _entity.Revision == _entityRevision &&
        _entity.Lifecycle == MechanismEntityLifecycle.Active;
    }

    /// <summary>Requires all participants validated in one exclusive window, without writes.</summary>
    public TeleportPylonSnapshot Commit()
    {
      if (_committed)
      {
        throw new InvalidOperationException("The pylon registration was already committed.");
      }
      _owner._byIdentity = _identities;
      _owner._byAnchor = _anchors;
      _owner._nextOrdinal = _nextOrdinal;
      _owner.Revision = _nextRevision;
      _committed = true;
      return Snapshot;
    }
  }

  private readonly MechanismFactsCatalog _facts;
  private readonly long _worldScope;
  private Dictionary<MechanismEntityHandle, TeleportPylonSnapshot> _byIdentity = [];
  private Dictionary<TileCoordinate, TeleportPylonSnapshot> _byAnchor = [];
  private long _nextOrdinal = 1;

  public TeleportPylonRegistry(MechanismFactsCatalog facts, long worldScope)
  {
    _facts = facts ?? throw new ArgumentNullException(nameof(facts));
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    _worldScope = worldScope;
  }

  public long WorldScope => _worldScope;

  public long Revision { get; private set; } = 1;

  public int Count => _byIdentity.Count;

  public IReadOnlyList<TeleportPylonSnapshot> Capture()
  {
    return Array.AsReadOnly(_byIdentity.Values
      .OrderBy(pylon => pylon.Ordinal)
      .ToArray());
  }

  public bool TryGetAt(TileCoordinate anchor, out TeleportPylonSnapshot? pylon)
  {
    return _byAnchor.TryGetValue(anchor, out pylon);
  }

  public bool TryGet(
    MechanismEntityHandle identity,
    out TeleportPylonSnapshot? pylon)
  {
    if (identity.WorldScope != _worldScope)
    {
      pylon = null;
      return false;
    }
    return _byIdentity.TryGetValue(identity, out pylon);
  }

  /// <summary>Indexes one already-committed type-7 entity using its committed anchor tile.</summary>
  public bool TryRegister(
    TileEntityRuntimeState entity,
    MechanismTileSnapshot anchorTile,
    out TeleportPylonSnapshot? registered)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ValidateEntity(entity);
    byte pylonType = ResolvePylonType(entity.Definition, anchorTile);
    if (_byIdentity.TryGetValue(entity.Handle, out TeleportPylonSnapshot? existing))
    {
      if (existing.Anchor != entity.Anchor || existing.Type != pylonType)
      {
        throw new InvalidOperationException(
          "A registered pylon identity cannot change its anchor or type without removal.");
      }
      registered = existing;
      return false;
    }

    RegistrationPreparation preparation = PrepareRegister(entity, anchorTile);
    if (!preparation.Validate())
    {
      throw new InvalidOperationException("The prepared pylon registration is stale.");
    }
    registered = preparation.Commit();
    return true;
  }

  /// <summary>Stages indexes for a committed or prepared type-7 TileEntity without publishing.</summary>
  public RegistrationPreparation PrepareRegister(
    TileEntityRuntimeState entity,
    MechanismTileSnapshot anchorTile)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ValidateEntity(entity);
    byte pylonType = ResolvePylonType(entity.Definition, anchorTile);
    if (_byIdentity.ContainsKey(entity.Handle))
    {
      throw new InvalidOperationException("The pylon identity is already registered.");
    }
    if (_byAnchor.ContainsKey(entity.Anchor))
    {
      throw new InvalidOperationException(
        $"A pylon is already registered at {entity.Anchor.X},{entity.Anchor.Y}.");
    }
    var snapshot = new TeleportPylonSnapshot(
      entity.Handle,
      entity.PersistenceId,
      entity.NetworkId,
      pylonType,
      entity.Anchor,
      _nextOrdinal);
    return new RegistrationPreparation(this, entity, snapshot);
  }

  public bool TryRemove(
    MechanismEntityHandle identity,
    out TeleportPylonSnapshot? removed)
  {
    if (identity.WorldScope != _worldScope ||
        !_byIdentity.TryGetValue(identity, out TeleportPylonSnapshot? existing))
    {
      removed = null;
      return false;
    }

    RemovalPreparation preparation = PrepareRemove(identity);
    if (!preparation.Validate())
    {
      throw new InvalidOperationException("The prepared pylon removal is stale.");
    }
    preparation.Commit();
    removed = existing;
    return true;
  }

  public RemovalPreparation PrepareRemove(MechanismEntityHandle identity)
  {
    if (identity.WorldScope != _worldScope ||
        !_byIdentity.TryGetValue(identity, out TeleportPylonSnapshot? existing))
    {
      throw new KeyNotFoundException("The pylon identity is not registered in this world.");
    }
    if (!_byAnchor.TryGetValue(existing.Anchor, out TeleportPylonSnapshot? byAnchor) ||
        byAnchor != existing)
    {
      throw new InvalidOperationException("The pylon registry indexes are inconsistent.");
    }
    return new RemovalPreparation(this, existing);
  }

  /// <summary>
  /// Rebuilds derived indexes atomically after TileEntity restore. Duplicate anchors and invalid
  /// styles fail the whole rebuild instead of publishing a partial registry.
  /// </summary>
  public void Rebuild(
    IEnumerable<TileEntityRuntimeState> entities,
    IReadOnlyDictionary<TileCoordinate, MechanismTileSnapshot> anchorTiles)
  {
    ArgumentNullException.ThrowIfNull(entities);
    ArgumentNullException.ThrowIfNull(anchorTiles);

    var nextByIdentity = new Dictionary<MechanismEntityHandle, TeleportPylonSnapshot>();
    var nextByAnchor = new Dictionary<TileCoordinate, TeleportPylonSnapshot>();
    long ordinal = 1;
    foreach (TileEntityRuntimeState entity in entities
      .Where(state => state.Definition.Type == 7)
      .OrderBy(state => state.PersistenceId))
    {
      ValidateEntity(entity);
      if (!anchorTiles.TryGetValue(entity.Anchor, out MechanismTileSnapshot tile))
      {
        throw new InvalidOperationException(
          $"Pylon anchor {entity.Anchor.X},{entity.Anchor.Y} has no committed tile snapshot.");
      }
      byte pylonType = ResolvePylonType(entity.Definition, tile);
      var snapshot = new TeleportPylonSnapshot(
        entity.Handle,
        entity.PersistenceId,
        entity.NetworkId,
        pylonType,
        entity.Anchor,
        ordinal);
      if (!nextByIdentity.TryAdd(entity.Handle, snapshot) ||
          !nextByAnchor.TryAdd(entity.Anchor, snapshot))
      {
        throw new InvalidOperationException(
          "Restored pylons have duplicate types, identities or anchors.");
      }
      ordinal = checked(ordinal + 1);
    }

    long nextRevision = checked(Revision + 1);
    _byIdentity = nextByIdentity;
    _byAnchor = nextByAnchor;
    _nextOrdinal = ordinal;
    Revision = nextRevision;
  }

  public void Clear()
  {
    if (_byIdentity.Count == 0)
    {
      return;
    }
    long nextRevision = checked(Revision + 1);
    _byIdentity = [];
    _byAnchor = [];
    _nextOrdinal = 1;
    Revision = nextRevision;
  }

  private void ValidateEntity(TileEntityRuntimeState entity)
  {
    if (entity.Handle.WorldScope != _worldScope || !entity.Handle.IsValid ||
        entity.Lifecycle != MechanismEntityLifecycle.Active || entity.Definition.Type != 7)
    {
      throw new InvalidOperationException(
        "Only an active type-7 TileEntity from this world can be registered as a pylon.");
    }
  }

  private byte ResolvePylonType(
    TileEntityDefinition definition,
    MechanismTileSnapshot anchorTile)
  {
    if (definition.Type != 7 || anchorTile.Type != definition.TileType ||
        !anchorTile.Active || anchorTile.FrameX < 0 ||
        anchorTile.FrameX % definition.FrameXModulo != 0 || anchorTile.FrameY != 0)
    {
      throw new InvalidOperationException("The pylon anchor tile is invalid.");
    }

    int type = anchorTile.FrameX / definition.FrameXModulo;
    if (type < byte.MinValue || type > byte.MaxValue ||
        !_facts.Pylons.ContainsKey((byte)type))
    {
      throw new InvalidOperationException("The pylon anchor uses an unknown Version4 style.");
    }
    return (byte)type;
  }
}

public sealed record TeleportPylonSnapshot(
  MechanismEntityHandle Identity,
  int PersistenceId,
  int NetworkId,
  byte Type,
  TileCoordinate Anchor,
  long Ordinal);

public readonly record struct PylonReachRectangle(
  int Left,
  int Top,
  int RightExclusive,
  int BottomExclusive)
{
  public bool Contains(TileCoordinate tile)
  {
    return tile.X >= Left && tile.X < RightExclusive &&
      tile.Y >= Top && tile.Y < BottomExclusive;
  }
}

public static class TeleportPylonRules
{
  public const int InteractionReach = 60;
  public const int SceneNpcHomeReach = 100;

  public static PylonReachRectangle GetInteractionRectangle(TileCoordinate pylon)
  {
    return new PylonReachRectangle(
      checked(pylon.X - InteractionReach),
      checked(pylon.Y - InteractionReach),
      checked(pylon.X + InteractionReach + 3),
      checked(pylon.Y + InteractionReach + 4));
  }

  public static bool IsPlayerNearPylon(
    PylonPlayerFact player,
    TileCoordinate pylon)
  {
    if (!double.IsFinite(player.PositionX) || !double.IsFinite(player.PositionY) ||
        player.PositionX < 0 || player.PositionY < 0 || player.Width <= 0 || player.Height <= 0)
    {
      return false;
    }

    // Player.InTileEntityInteractionRange: closest point in the inset 3x4 hitbox,
    // checked against the player's complete tile region with a 60-tile override.
    double closestX = Math.Clamp(
      player.PositionX + player.Width / 2d, pylon.X * 16d + 1, (pylon.X + 3) * 16d - 1);
    double closestY = Math.Clamp(
      player.PositionY + player.Height / 2d, pylon.Y * 16d + 1, (pylon.Y + 4) * 16d - 1);
    double tileX = Math.Floor(closestX / 16);
    double tileY = Math.Floor(closestY / 16);
    return tileX >= Math.Truncate(player.PositionX / 16) - InteractionReach &&
      tileX <= Math.Ceiling((player.PositionX + player.Width) / 16) - 1 + InteractionReach &&
      tileY >= Math.Truncate(player.PositionY / 16) - InteractionReach &&
      tileY <= Math.Ceiling((player.PositionY + player.Height) / 16) - 1 + InteractionReach;
  }

  public static bool IsNpcCenterNearHome(
    double centerTileX,
    double centerTileY,
    TileCoordinate home)
  {
    double deltaX = centerTileX - home.X;
    double deltaY = centerTileY - home.Y;
    return double.IsFinite(deltaX) && double.IsFinite(deltaY) &&
      deltaX * deltaX + deltaY * deltaY < SceneNpcHomeReach * SceneNpcHomeReach;
  }

  public static bool IsNpcHomeInScene(
    TileCoordinate home,
    TileCoordinate pylon,
    int sceneWidth = 169,
    int sceneHeight = 124)
  {
    if (sceneWidth <= 0 || sceneHeight <= 0)
    {
      throw new ArgumentOutOfRangeException();
    }

    int left = pylon.X - sceneWidth / 2;
    int top = pylon.Y - sceneHeight / 2;
    return home.X >= left && home.X < left + sceneWidth &&
      home.Y >= top && home.Y < top + sceneHeight;
  }
}
