namespace Terraria.Items;

public enum CraftingPhase : byte
{
  Unknown,
  Idle,
  Accepted,
  Consuming,
  Producing,
  Completed,
  Rejected,
  Cancelled,
}
