using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldTileEntitiesPrepared {
  public IReadOnlyList<TileEntitySnapshot> Entities { get; }

  /// <summary>
  /// Gets the original section count, including records later removed during anchor filtering.
  /// </summary>
  public int NextId { get; }

  public WorldTileEntitiesPrepared(IReadOnlyList<TileEntitySnapshot> entities, int nextId) {
    ArgumentNullException.ThrowIfNull(entities);
    if (nextId < 0) {
      throw new ArgumentOutOfRangeException(nameof(nextId));
    }

    TileEntitySnapshot[] entityCopy = entities.ToArray();
    if (entityCopy.Any(entity => entity.Id.Value < 0 || entity.Id.Value >= nextId)) {
      throw new ArgumentException("A tile entity ID is outside the next-ID range.",
          nameof(entities));
    }

    Entities = Array.AsReadOnly(entityCopy);
    NextId = nextId;
  }
}
