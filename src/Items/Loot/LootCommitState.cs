namespace Terraria.Items.Loot;

public enum LootCommitState : byte
{
  Unresolved,
  Resolving,
  Resolved,
  Committed,
  Skipped,
  Failed,
}
