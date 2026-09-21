namespace Terraria.Player;

// Owns short-lived player interaction facts that are local to the Player entity.
// Creative, chat, mount, grapple and combat objects remain external boundaries.
public sealed class PlayerRuntimeInteractionComponent
{
  public int EmoteRemainingTicks { get; set; }

  public byte SpelunkerRemainingTicks { get; set; }

  // The array is component-owned state; read adapters must return a defensive copy.
  public int[] BuilderToggleStatuses { get; } =
    new int[PlayerBuilderInteractionCatalog.Count];
}
