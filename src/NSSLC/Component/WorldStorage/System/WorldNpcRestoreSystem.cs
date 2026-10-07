namespace Terraria.WorldStorage;

public static class WorldNpcRestoreSystem {
  /// <summary>Restores into an empty session; a live NPC store cannot be silently replaced.</summary>
  public static void Apply(EntitySlotStore<WorldEntityState, NpcSlot> owner,
      IReadOnlyList<WorldNpcState> prepared) {
    ArgumentNullException.ThrowIfNull(owner);
    if (owner.ActiveCount != 0) {
      throw new InvalidOperationException("NPC restoration requires a fresh world session.");
    }
    foreach (WorldNpcState npc in prepared) {
      if (!owner.TryAllocate(npc, out _, out _)) {
        throw new InvalidOperationException("The NPC store capacity was exhausted.");
      }
    }
  }
}
