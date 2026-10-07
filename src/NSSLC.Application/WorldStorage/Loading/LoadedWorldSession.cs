using EntityEcs;
using Terraria.Relationships;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Terrain.TreeTops;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>A fresh, unpublished load target. Publish it only after all owner commits succeed.</summary>
public sealed class LoadedWorldSession : IDisposable {
  public const string OwnerContextId = "world.load-session";

  private readonly EntityIdentityRegistry _identityRegistry;
  private readonly EntityRuntime _entityRuntime;
  private readonly WorldStorageRoot _storage;
  private readonly WorldSessionRestoreState _world = new();
  private readonly WorldLoadLifecycleComponent _lifecycle = new();
  private readonly WorldDimensionCompatibilityState _dimensionCompatibility = new();
  private readonly WorldTreeTopsStateComponent _treeTops = new();
  private readonly HashSet<string> _committedSections = new(StringComparer.Ordinal);
  private bool _isDisposed;

  public LoadedWorldSession(
      EntityIdentityRegistry? identityRegistry = null,
      int maximumWorldItemSlots = int.MaxValue) {
    _identityRegistry = identityRegistry ?? new EntityIdentityRegistry();
    _entityRuntime = new EntityRuntime(_identityRegistry);
    _storage = new WorldStorageRoot(_entityRuntime, maximumWorldItemSlots);
  }

  public EntityRuntime EntityRuntime {
    get {
      ThrowIfDisposed();
      return _entityRuntime;
    }
  }
  /// <summary>Immutable session token used to reject requests queued across world replacement.</summary>
  public EntityRuntimeId WorldRuntimeId => _entityRuntime.RuntimeId;
  public EntityIdentityRegistry IdentityRegistry => _identityRegistry;
  public WorldStorageRoot Storage {
    get {
      ThrowIfDisposed();
      return _storage;
    }
  }
  public WorldSessionRestoreState World {
    get {
      ThrowIfDisposed();
      return _world;
    }
  }
  public WorldLoadLifecycleComponent Lifecycle {
    get {
      ThrowIfDisposed();
      return _lifecycle;
    }
  }
  public WorldDimensionCompatibilityState DimensionCompatibility {
    get {
      ThrowIfDisposed();
      return _dimensionCompatibility;
    }
  }
  public WorldTreeTopsStateComponent TreeTops {
    get {
      ThrowIfDisposed();
      return _treeTops;
    }
  }
  public TownHousingRegistryComponent TownHousing => World.TownHousing;
  public WorldFileMetadataSection? Metadata { get; internal set; }
  /// <summary>The immutable document used to hydrate this session, retained as the base for saves.</summary>
  public WorldPersistenceDocument? SourceDocument { get; private set; }
  public DateTime? CreationTime { get; internal set; }
  public DateTime? LastPlayed { get; internal set; }
  public string? ManifestJson { get; internal set; }
  public IReadOnlyList<bool> FrameImportant { get; internal set; } = Array.Empty<bool>();
  public bool HasCreativePowers { get; internal set; }
  public bool IsComplete { get; internal set; }
  public bool IsPublished { get; private set; }
  public bool IsPublicationUncertain { get; private set; }
  public bool IsDisposed => _isDisposed;
  public IReadOnlyCollection<string> CommittedSections =>
      Array.AsReadOnly(_committedSections.Order(StringComparer.Ordinal).ToArray());
  public bool IsFresh =>
      !_isDisposed &&
      _entityRuntime.EntityCount == 0 &&
      _committedSections.Count == 0 &&
      _storage.TileMap.Width == 0 &&
      _storage.TileMap.Height == 0 &&
      _storage.Players.ActiveCount == 0 &&
      _storage.Npcs.ActiveCount == 0 &&
      _storage.Projectiles.ActiveCount == 0 &&
      _storage.WorldItems.ActiveCount == 0 &&
      _storage.ProjectileIdentities.Count == 0 &&
      _storage.WorldContainers.ActiveChestCount == 0 &&
      _storage.WorldSigns.ActiveSignCount == 0 &&
      _storage.TileEntities.Count == 0 &&
      _storage.TileEntityUpdates.Count == 0 &&
      _storage.Sections.Count == 0 &&
      _storage.PressurePlates.Anchors.Count == 0 &&
      HasDefaultTreeTopStyles() &&
      _dimensionCompatibility.LastMaxTilesX == 0 &&
      _dimensionCompatibility.LastMaxTilesY == 0 &&
      SourceDocument is null &&
      Metadata is null &&
      CreationTime is null &&
      LastPlayed is null &&
      ManifestJson is null &&
      FrameImportant.Count == 0 &&
      !HasCreativePowers &&
      !IsComplete &&
      !IsPublished &&
      !IsPublicationUncertain;

  public void Dispose() {
    if (_isDisposed) {
      return;
    }

    _entityRuntime.EnsureCanDispose();
    _storage.Dispose();
    _entityRuntime.Dispose();
    _isDisposed = true;
  }

  private void ThrowIfDisposed() {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
  }

  private bool HasDefaultTreeTopStyles() {
    WorldTreeTopsStateSnapshot snapshot = _treeTops.CreateSnapshot();
    foreach (int style in snapshot.Variations) {
      if (style != 0) {
        return false;
      }
    }

    return true;
  }

  internal void RecordCommit(string sectionId) {
    _committedSections.Add(sectionId);
  }

  internal void SetSourceDocument(WorldPersistenceDocument document) {
    ArgumentNullException.ThrowIfNull(document);
    if (SourceDocument is not null) {
      throw new InvalidOperationException("A loaded session can retain only one source document.");
    }

    SourceDocument = document;
  }

  internal void MarkPublished() {
    if (!IsComplete || IsPublished || !IsPublicationUncertain) {
      throw new InvalidOperationException("Only a complete unpublished world session can be published.");
    }
    IsPublished = true;
    IsPublicationUncertain = false;
  }

  internal void MarkPublicationAttempted() {
    if (!IsComplete || IsPublished || IsPublicationUncertain) {
      throw new InvalidOperationException("The world session cannot begin publication in its current state.");
    }
    IsPublicationUncertain = true;
  }

  internal void MarkUnpublished() {
    IsPublished = false;
    IsPublicationUncertain = false;
  }
}
