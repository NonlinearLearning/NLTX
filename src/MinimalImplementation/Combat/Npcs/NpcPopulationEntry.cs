namespace MinimalImplementation.Combat.Npcs;

/// <summary>One active slot, including a zero-life NPC awaiting retirement.</summary>
public sealed class NpcPopulationEntry {
  internal NpcPopulationEntry(NpcView npc, NpcHomeState home) {
    Npc = npc;
    Home = home;
  }

  public NpcView Npc { get; }

  public NpcHomeState Home { get; }

  // NPC.isLikeATownNPC includes the Skeleton Merchant (453) without townNPC.
  public bool IsLikeTownNpc => Npc.Type == 453 || Npc.IsTownNpc;

  // Entity.Center and DoesPositionHaveEnoughNPCs both use single precision.
  public float CenterTileX => (Npc.Position.X + Npc.Width / 2f) / 16f;

  public float CenterTileY => (Npc.Position.Y + Npc.Height / 2f) / 16f;
}
