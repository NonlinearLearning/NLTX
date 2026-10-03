using System;
using System.Collections.Generic;
using System.IO;

namespace NSSLC.WorldGeneration;

/// <summary>In-memory identities of generated objects with independent contents.</summary>
public sealed class TileEntity {
  private static readonly Dictionary<(int X, int Y), TileEntity> _entities = new();
  public int Id { get; }
  public int X { get; }
  public int Y { get; }
  public int TileType { get; }

  private TileEntity(int id, int x, int y, int type) {
    Id = id;
    X = x;
    Y = y;
    TileType = type;
  }

  public static int Register(int x, int y, int type) {
    if (_entities.TryGetValue((x, y), out TileEntity existing)) {
      return existing.Id;
    }
    var entity = new TileEntity(_entities.Count, x, y, type);
    _entities.Add((x, y), entity);
    return entity.Id;
  }

  public static void Clear() {
    _entities.Clear();
  }

  public static void PerformUpdates() { }

  public static bool TryGetAt<T>(int x, int y, out T entity) where T : class {
    entity = null;
    return _entities.TryGetValue((x, y), out TileEntity value) && (entity = value as T) != null;
  }

  public static void Write(params object[] arguments) {
    throw new NotSupportedException("TileEntity encoding belongs to the world storage adapter.");
  }

  public static TileEntity Read(params object[] arguments) {
    throw new NotSupportedException("TileEntity decoding belongs to the world storage adapter.");
  }
}
