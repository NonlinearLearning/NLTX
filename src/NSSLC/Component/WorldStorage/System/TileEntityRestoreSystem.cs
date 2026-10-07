namespace Terraria.WorldStorage;

public static class TileEntityRestoreSystem {
  public static void Apply(TileEntityStore owner, IReadOnlyList<TileEntitySnapshot> prepared,
      int nextId) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Replace(prepared, nextId);
  }
}
