namespace Terraria.WorldStorage;

public static class TileMapRestoreSystem {
  public static void Apply(TileMapStore owner, TileMapSnapshot prepared) {
    ArgumentNullException.ThrowIfNull(owner);
    ArgumentNullException.ThrowIfNull(prepared);
    owner.Replace(prepared);
  }
}
