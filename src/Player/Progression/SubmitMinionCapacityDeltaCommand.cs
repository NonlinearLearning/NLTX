using Terraria.Projectile;
using Terraria.Relationships;

namespace Terraria.Player.Progression;

public readonly record struct SubmitMinionCapacityDeltaCommand(
  EntityReference Owner,
  ProjectileIdentityComponent Projectile,
  int MinionCountDelta,
  float SlotDelta,
  long SourceRevision,
  Guid IdempotencyToken);
