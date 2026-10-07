namespace Terraria.WorldStorage;

public static class WorldContainerRestoreSystem {
  public static void Apply(WorldContainerStore owner, IReadOnlyList<WorldChestSnapshot> prepared) {
    ArgumentNullException.ThrowIfNull(owner);
    ArgumentNullException.ThrowIfNull(prepared);
    owner.Replace(prepared);
  }
}
