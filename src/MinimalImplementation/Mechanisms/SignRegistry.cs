using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Owns sign text and protocol ids for one world scope. Call mutations on the server commit loop.
/// The tile map remains owned by <see cref="WorldOwner"/>.
/// </summary>
public sealed class SignRegistry
{
  public const int MaximumSignCount = 32_000;

  private static readonly HashSet<ushort> SignTileTypes = [55, 85, 425, 573];

  private readonly SignSnapshot?[] _byId = new SignSnapshot?[MaximumSignCount];
  private readonly Dictionary<TileCoordinate, SortedDictionary<int, SignSnapshot>> _byAnchor = [];
  private readonly long _worldScope;

  public SignRegistry(long worldScope)
  {
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }

    _worldScope = worldScope;
  }

  public int Count => _byId.Count(sign => sign is not null);

  public long WorldScope => _worldScope;

  public long Revision { get; private set; } = 1;

  public IReadOnlyList<SignSnapshot> Capture()
  {
    return Array.AsReadOnly(_byId
      .Where(sign => sign is not null)
      .Select(sign => sign!)
      .ToArray());
  }

  /// <summary>
  /// Resolves the clicked tile to its Version4 sign anchor and optionally creates its empty record.
  /// An invalid sign root also retires an existing record at that root, matching Sign.ReadSign.
  /// </summary>
  public SignLookupResult ReadSign(
    WorldSnapshot world,
    TileCoordinate clicked,
    bool createIfMissing = true)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!world.Header.Geometry.Contains(clicked))
    {
      return new SignLookupResult(SignLookupStatus.OutOfWorld, null, Revision);
    }

    WorldTile clickedTile = world.GetTile(clicked);
    int anchorX = clicked.X - ((clickedTile.FrameX / 18) % 2);
    int anchorY = clicked.Y - (clickedTile.FrameY / 18);
    var anchor = new TileCoordinate(anchorX, anchorY);
    if (!world.Header.Geometry.Contains(anchor))
    {
      return new SignLookupResult(SignLookupStatus.InvalidAnchor, null, Revision);
    }

    WorldTile anchorTile = world.GetTile(anchor);
    if (!IsSignTile(anchorTile))
    {
      if (_byAnchor.ContainsKey(anchor))
      {
        long nextRevision = checked(Revision + 1);
        RemoveAtCore(anchor);
        Revision = nextRevision;
      }
      return new SignLookupResult(SignLookupStatus.InvalidAnchor, null, Revision);
    }

    if (_byAnchor.TryGetValue(anchor, out SortedDictionary<int, SignSnapshot>? existingSigns))
    {
      return new SignLookupResult(SignLookupStatus.Found, existingSigns.First().Value, Revision);
    }

    if (!createIfMissing)
    {
      return new SignLookupResult(SignLookupStatus.NotFound, null, Revision);
    }

    int id = FindFirstFreeId();
    if (id < 0)
    {
      return new SignLookupResult(SignLookupStatus.CapacityExhausted, null, Revision);
    }

    long nextRegistryRevision = checked(Revision + 1);
    var created = new SignSnapshot(_worldScope, id, anchor, string.Empty, 1);
    Add(created);
    Revision = nextRegistryRevision;
    return new SignLookupResult(SignLookupStatus.Created, created, Revision);
  }

  /// <summary>Applies a sign edit using Version4's id replacement and valid-tile rules.</summary>
  public SignUpdateResult ApplyText(
    WorldSnapshot world,
    int id,
    TileCoordinate anchor,
    string text)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(text);
    if (id < 0 || id >= MaximumSignCount)
    {
      return new SignUpdateResult(SignUpdateStatus.InvalidId, null, null);
    }
    SignSnapshot? existing = _byId[id];
    bool textChanged = existing?.Text != text;
    bool anchorChanged = existing?.Anchor != anchor;
    if (!TryValidateAnchor(world, anchor))
    {
      if (existing is not null)
      {
        long nextRevision = checked(Revision + 1);
        RemoveFromAnchorIndex(existing);
        _byId[id] = null;
        Revision = nextRevision;
      }
      return new SignUpdateResult(SignUpdateStatus.InvalidAnchor, null, existing);
    }
    if (!textChanged && !anchorChanged)
    {
      return new SignUpdateResult(SignUpdateStatus.Unchanged, existing, existing);
    }

    long nextRegistryRevision = checked(Revision + 1);
    long nextSignRevision = existing is null ? 1 : checked(existing.Revision + 1);
    var updated = new SignSnapshot(
      _worldScope,
      id,
      anchor,
      text,
      nextSignRevision);
    if (existing is not null)
    {
      RemoveFromAnchorIndex(existing);
    }
    Add(updated);
    Revision = nextRegistryRevision;
    return new SignUpdateResult(
      textChanged ? SignUpdateStatus.Applied : SignUpdateStatus.Unchanged,
      updated,
      existing);
  }

  public bool RemoveAt(TileCoordinate anchor)
  {
    if (!_byAnchor.ContainsKey(anchor))
    {
      return false;
    }

    long nextRevision = checked(Revision + 1);
    RemoveAtCore(anchor);
    Revision = nextRevision;
    return true;
  }

  public IReadOnlyList<WorldFileSignRecord> CapturePersistence()
  {
    var records = new List<WorldFileSignRecord>(Count);
    foreach (SignSnapshot? sign in _byId)
    {
      if (sign is not null)
      {
        records.Add(new WorldFileSignRecord(sign.Text, sign.Anchor.X, sign.Anchor.Y));
      }
    }
    return Array.AsReadOnly(records.ToArray());
  }

  public IReadOnlyList<Packet10SignRecord> CaptureSection(WorldSectionBounds bounds)
  {
    Packet10SignRecord[] records = _byId
      .Where(sign => sign is not null && bounds.Contains(sign.Anchor))
      .Select(sign => ToPacketRecord(sign!))
      .ToArray();
    return Array.AsReadOnly(records);
  }

  public PreparedSignLoad PrepareRestore(
    WorldSnapshot world,
    IReadOnlyList<WorldFileSignRecord> records)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(records);
    if (Count != 0)
    {
      throw new InvalidOperationException("Sign restore requires an empty registry.");
    }
    if (records.Count > MaximumSignCount)
    {
      throw new ArgumentOutOfRangeException(nameof(records));
    }

    var states = new SignSnapshot?[MaximumSignCount];
    var anchors = new HashSet<TileCoordinate>();
    for (int id = 0; id < records.Count; id++)
    {
      WorldFileSignRecord record = records[id]
        ?? throw new ArgumentException("A sign record cannot be null.", nameof(records));
      var anchor = new TileCoordinate(record.X, record.Y);
      if (!TryValidateAnchor(world, anchor))
      {
        throw new InvalidDataException(
          $"The persisted sign at {record.X},{record.Y} does not have a valid sign tile anchor.");
      }

      if (!anchors.Add(anchor))
      {
        throw new InvalidDataException(
          $"The persisted sign section contains a duplicate anchor at {record.X},{record.Y}.");
      }

      states[id] = new SignSnapshot(_worldScope, id, anchor, record.Text, 1);
    }

    return new PreparedSignLoad(this, Revision, world.WorldEpoch, world.Version, states);
  }

  public SignLoadResult CommitRestore(PreparedSignLoad prepared, WorldSnapshot currentWorld)
  {
    ArgumentNullException.ThrowIfNull(prepared);
    ArgumentNullException.ThrowIfNull(currentWorld);
    if (!ReferenceEquals(prepared.Owner, this) || prepared.IsConsumed ||
        prepared.RegistryRevision != Revision || prepared.WorldEpoch != currentWorld.WorldEpoch ||
        prepared.WorldVersion != currentWorld.Version ||
        Count != 0)
    {
      return SignLoadResult.Stale;
    }

    if (!prepared.TryConsume())
    {
      return SignLoadResult.Stale;
    }

    long nextRevision = checked(Revision + 1);
    Array.Copy(prepared.States, _byId, MaximumSignCount);
    _byAnchor.Clear();
    foreach (SignSnapshot? sign in _byId)
    {
      if (sign is not null)
      {
        AddToAnchorIndex(sign);
      }
    }
    Revision = nextRevision;
    return new SignLoadResult(SignLoadCommitStatus.Applied, Count);
  }

  public void ClearForWorldUnload()
  {
    if (Count == 0)
    {
      return;
    }

    long nextRevision = checked(Revision + 1);
    Array.Clear(_byId);
    _byAnchor.Clear();
    Revision = nextRevision;
  }

  private static bool IsSignTile(WorldTile tile)
  {
    // Sign.ReadSign checks the tileSign table at the resolved root; it does not test Active here.
    return SignTileTypes.Contains(tile.Type);
  }

  private static bool TryValidateAnchor(WorldSnapshot world, TileCoordinate anchor)
  {
    if (!world.Header.Geometry.Contains(anchor))
    {
      return false;
    }

    WorldTile tile = world.GetTile(anchor);
    if (!tile.Active || !IsSignTile(tile))
    {
      return false;
    }

    return true;
  }

  private static Packet10SignRecord ToPacketRecord(SignSnapshot sign)
  {
    if (sign.Id > short.MaxValue || sign.Anchor.X < 0 || sign.Anchor.X > short.MaxValue ||
        sign.Anchor.Y < 0 || sign.Anchor.Y > short.MaxValue)
    {
      throw new InvalidOperationException("A sign cannot be represented by Version4 section fields.");
    }
    return new Packet10SignRecord(
      (short)sign.Id,
      (short)sign.Anchor.X,
      (short)sign.Anchor.Y,
      sign.Text);
  }

  private void Add(SignSnapshot sign)
  {
    _byId[sign.Id] = sign;
    AddToAnchorIndex(sign);
  }

  private bool RemoveAtCore(TileCoordinate anchor)
  {
    if (!_byAnchor.Remove(anchor, out SortedDictionary<int, SignSnapshot>? signs))
    {
      return false;
    }

    foreach (int id in signs.Keys)
    {
      _byId[id] = null;
    }
    return true;
  }

  private void RemoveFromAnchorIndex(SignSnapshot sign)
  {
    if (!_byAnchor.TryGetValue(sign.Anchor, out SortedDictionary<int, SignSnapshot>? signs) ||
        !signs.Remove(sign.Id))
    {
      throw new InvalidOperationException("The sign anchor index is inconsistent.");
    }
    if (signs.Count == 0)
    {
      _byAnchor.Remove(sign.Anchor);
    }
  }

  private void AddToAnchorIndex(SignSnapshot sign)
  {
    if (!_byAnchor.TryGetValue(sign.Anchor, out SortedDictionary<int, SignSnapshot>? signs))
    {
      signs = [];
      _byAnchor.Add(sign.Anchor, signs);
    }
    if (!signs.TryAdd(sign.Id, sign))
    {
      throw new InvalidOperationException("The sign anchor index contains a duplicate id.");
    }
  }

  private int FindFirstFreeId()
  {
    for (int id = 0; id < _byId.Length; id++)
    {
      if (_byId[id] is null)
      {
        return id;
      }
    }
    return -1;
  }

}

public sealed record SignSnapshot(
  long WorldScope,
  int Id,
  TileCoordinate Anchor,
  string Text,
  long Revision);

public enum SignLookupStatus
{
  Found,
  Created,
  NotFound,
  InvalidAnchor,
  OutOfWorld,
  CapacityExhausted
}

public sealed record SignLookupResult(
  SignLookupStatus Status,
  SignSnapshot? Sign,
  long RegistryRevision);

public enum SignUpdateStatus
{
  Applied,
  Unchanged,
  InvalidId,
  InvalidAnchor
}

public sealed record SignUpdateResult(
  SignUpdateStatus Status,
  SignSnapshot? Sign,
  SignSnapshot? PreviousSign);

public enum SignLoadCommitStatus
{
  Applied,
  Stale
}

public sealed record SignLoadResult(SignLoadCommitStatus Status, int Count)
{
  public static SignLoadResult Stale { get; } = new(SignLoadCommitStatus.Stale, 0);
}

public sealed class PreparedSignLoad
{
  internal PreparedSignLoad(
    SignRegistry owner,
    long registryRevision,
    ulong worldEpoch,
    CommitVersion worldVersion,
    SignSnapshot?[] states)
  {
    Owner = owner;
    RegistryRevision = registryRevision;
    WorldEpoch = worldEpoch;
    WorldVersion = worldVersion;
    States = states;
  }

  internal SignRegistry Owner { get; }

  internal long RegistryRevision { get; }

  internal ulong WorldEpoch { get; }

  internal CommitVersion WorldVersion { get; }

  internal SignSnapshot?[] States { get; }

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
