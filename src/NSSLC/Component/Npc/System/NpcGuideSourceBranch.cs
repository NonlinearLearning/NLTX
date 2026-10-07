namespace Terraria.Npc;

[Flags]
public enum NpcGuideSourceBranch
{
  None = 0,
  WeatherReturnPressure = 1 << 0,
  ServerReturnAuthority = 1 << 1,
  HomeReturnEligibility = 1 << 2,
  PlayerOccupancyBlocked = 1 << 3,
  HomeDestinationProbe = 1 << 4,
  HomeTeleport = 1 << 5,
  NetworkSynchronization = 1 << 6,
  ForceSitting = 1 << 7,
  HomeReassignment = 1 << 8,
  HomeReturnNoPath = 1 << 9,
}
