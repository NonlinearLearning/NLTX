using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Session;

namespace Terraria.Dome.Protocol.V1456.Dispatch;

public readonly record struct TerrariaPacketDispatchResult(
  TerrariaPacketDispatchOutcome Outcome,
  PlayerProfilePacket? PlayerProfile,
  PlayerControlIntent? PlayerControls,
  PlayerSpawnPacket? PlayerSpawn,
  TileManipulationIntent? TileManipulation,
  ChestOpenIntent? ChestOpen,
  DoorToggleIntent? DoorToggle,
  SignUpdateIntent? SignUpdate,
  ChestTransferIntent? ChestTransfer,
  PlayerBootstrapState? PlayerBootstrap = null,
  byte[]? ResponseFrame = null,
  LegacyPlayerControlsState? LegacyPlayerControls = null,
  SignOpenRequestPacket? SignOpenRequest = null,
  TileEntityPlacementIntent? TileEntityPlacement = null);
