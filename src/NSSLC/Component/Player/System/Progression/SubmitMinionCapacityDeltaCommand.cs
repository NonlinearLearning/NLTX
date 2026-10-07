using Terraria.Relationships;

namespace Terraria.Player.Progression;

public readonly record struct SubmitMinionCapacityDeltaCommand(
  EntityReference Owner,
  EntityReference ProjectileOwner,
  int ProjectileOwnerSlot,
  int ProjectileIdentity,
  int MinionCountDelta,
  float SlotDelta,
  long SourceRevision,
  Guid IdempotencyToken);
