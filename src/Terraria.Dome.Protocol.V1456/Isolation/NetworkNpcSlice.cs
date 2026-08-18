using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public readonly record struct NetworkNpcSlice(
  int ReplicationId,
  int NpcType,
  bool IsActive,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY,
  int Health,
  long Revision)
{
  public static NetworkNpcSlice From(NpcReplicationSnapshot snapshot)
  {
    return new NetworkNpcSlice(
      snapshot.ReplicationId,
      snapshot.NpcType,
      snapshot.IsActive,
      snapshot.Position.X,
      snapshot.Position.Y,
      snapshot.Velocity.X,
      snapshot.Velocity.Y,
      snapshot.Health,
      snapshot.Revision);
  }
}
