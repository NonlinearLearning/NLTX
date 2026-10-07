namespace Terraria.WorldStorage;

public static class WorldSignRestoreSystem {
  public static void Apply(WorldSignStore owner, IReadOnlyList<WorldSignSnapshot> prepared) {
    ArgumentNullException.ThrowIfNull(owner);
    owner.Replace(prepared);
  }
}
