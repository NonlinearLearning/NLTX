using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct NpcProjectileReplicationEnvelope(
  int ReplicationId,
  int ProjectileType,
  int Owner,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY,
  int Damage,
  int RemainingLifetime,
  bool IsActive,
  long Revision,
  int SectionX,
  int SectionY,
  int Identity,
  ProjectileTombstoneReason TombstoneReason,
  long TombstoneRetainedUntilTick,
  float DefinitionKnockback,
  int DefinitionOriginalDamage)
{
  public static NpcProjectileReplicationEnvelope From(
    NpcProjectileReplicationSnapshot snapshot)
  {
    return new NpcProjectileReplicationEnvelope(
      snapshot.ReplicationId,
      snapshot.ProjectileType,
      snapshot.Owner.Value,
      snapshot.Position.X,
      snapshot.Position.Y,
      snapshot.Velocity.X,
      snapshot.Velocity.Y,
      snapshot.Damage,
      snapshot.RemainingLifetime,
      snapshot.IsActive,
      snapshot.Revision,
      snapshot.Section.X,
      snapshot.Section.Y,
      snapshot.Identity,
      snapshot.TombstoneReason,
      snapshot.TombstoneRetainedUntilTick,
      snapshot.DefinitionKnockback,
      snapshot.DefinitionOriginalDamage);
  }

  public NpcProjectileReplicationSnapshot ToSnapshot()
  {
    return new NpcProjectileReplicationSnapshot(
      ReplicationId,
      ProjectileType,
      new NpcHandle(Owner),
      new SimulationVector(PositionX, PositionY),
      new SimulationVector(VelocityX, VelocityY),
      Damage,
      RemainingLifetime,
      IsActive,
      Revision,
      new WorldSectionCoordinates(SectionX, SectionY),
      Identity,
      TombstoneReason: TombstoneReason,
      TombstoneRetainedUntilTick: TombstoneRetainedUntilTick,
      DefinitionKnockback: DefinitionKnockback,
      DefinitionOriginalDamage: DefinitionOriginalDamage);
  }
}
