namespace Terraria.Dome.Simulation.Items.Commands;

public readonly record struct ShopPurchaseCommand(
  PlayerHandle Player,
  NpcHandle Npc,
  int OfferId,
  string SessionId,
  long Sequence);
