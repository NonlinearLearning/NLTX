using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public readonly record struct NetworkSignSlice(
  int SignId,
  int TileX,
  int TileY,
  string Text,
  long Revision)
{
  public static NetworkSignSlice From(SignSnapshot snapshot)
  {
    return new NetworkSignSlice(
      snapshot.SignId,
      snapshot.TileX,
      snapshot.TileY,
      snapshot.Text,
      snapshot.Revision);
  }
}
