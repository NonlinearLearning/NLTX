using System;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct NpcProjectileReplicationV3Envelope(
  NpcProjectileReplicationV2Envelope V2,
  bool HasProjectileUuid,
  Guid ProjectileUuid)
{
  public static NpcProjectileReplicationV3Envelope From(
    NpcProjectileReplicationSnapshot snapshot)
  {
    return new NpcProjectileReplicationV3Envelope(
      NpcProjectileReplicationV2Envelope.From(snapshot),
      snapshot.ProjectileUuid.HasValue,
      snapshot.ProjectileUuid.GetValueOrDefault());
  }

  public NpcProjectileReplicationSnapshot ToSnapshot()
  {
    return V2.ToSnapshot() with
    {
      ProjectileUuid = HasProjectileUuid ? ProjectileUuid : null
    };
  }
}
