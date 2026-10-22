namespace MinimalImplementation.Server.Player;

public readonly record struct PlayerZones(
  byte Zone1,
  byte Zone2,
  byte Zone3,
  byte Zone4,
  byte Zone5,
  byte TownNpcCount)
{
  public bool IsShimmerZone => (Zone5 & 0b0000_0001) != 0;

  public bool ShouldSpawnFaelings(PlayerZones previous)
  {
    return !previous.IsShimmerZone && IsShimmerZone;
  }
}
