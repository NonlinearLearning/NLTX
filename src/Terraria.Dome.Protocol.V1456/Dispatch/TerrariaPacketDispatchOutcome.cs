namespace Terraria.Dome.Protocol.V1456.Dispatch;

public enum TerrariaPacketDispatchOutcome
{
  PlayerProfileAccepted,
  PlayerBootstrapAccepted,
  PlayerControlsAccepted,
  ActiveSynchronizationAccepted,
  ClientProjectileSyncIgnored,
  ClientProjectileTerminationIgnored,
  ClientProjectileTerminationAccepted,
  NetModuleAccepted,
  TileManipulationAccepted,
  TileEntityPlacementAccepted,
  SectionRequested,
  WorldDataRequested,
  TileDataRequested,
  PlayerSpawnAccepted,
  PlayerSpawnUpdated,
  ChestOpenAccepted,
  DoorToggleAccepted,
  SignUpdateAccepted,
  ChestTransferAccepted
}
