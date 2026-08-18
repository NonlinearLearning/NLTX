using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public readonly record struct NetworkItemSlice(
  int ReplicationId,
  ushort ItemType,
  int Quantity,
  bool IsActive,
  float PositionX,
  float PositionY,
  long Revision)
{
  public static NetworkItemSlice From(ItemReplicationSnapshot snapshot)
  {
    return new NetworkItemSlice(
      snapshot.ReplicationId,
      snapshot.Stack.ItemType,
      snapshot.Stack.Quantity,
      snapshot.IsActive,
      snapshot.Position.X,
      snapshot.Position.Y,
      snapshot.Revision);
  }
}
