namespace MinimalImplementation.Combat.Npcs;

public sealed partial class Npc {
  // A completed roll from a non-reentrant random port is pinned until Ai[1] commits.
  // Later prepare/commit conflicts reuse it; it is not authoritative NPC AI state.
  internal int? PendingSlimeBodyItem { get; private set; }
  internal bool PreparingSlimeBody { get; set; }

  internal void CacheSlimeBodyItem(int item) {
    PendingSlimeBodyItem = item;
  }

  internal void ClearSlimeBodyItem() {
    PendingSlimeBodyItem = null;
  }
}
