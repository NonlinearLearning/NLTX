using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Players;

namespace Terraria.Dome.Server.Protocol;

internal abstract record TerrariaProtocolCommand(byte PlayerSlot);

internal sealed record CreateSessionPlayerCommand(
  byte PlayerSlot,
  int SpawnX,
  int SpawnY,
  PlayerPersistentState Account,
  SessionReplicationState ReplicationState,
  TaskCompletionSource<PlayerInitialProjection> Completion) : TerrariaProtocolCommand(PlayerSlot);

internal readonly record struct PlayerInitialProjection(
  bool IsActive,
  bool IsFacingRight,
  float PositionX,
  float PositionY);

internal sealed record ResolvePlayerAccountCommand(
  byte PlayerSlot,
  PlayerBootstrapState Bootstrap,
  TaskCompletionSource<PlayerPersistentState> Completion) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record EnsureInitialNpcsCommand(
  SimulationVector PlayerSpawn,
  TaskCompletionSource<bool> Completion) : TerrariaProtocolCommand(0);

internal sealed record QueueLiquidSourceCommand(
  LiquidSourceComponent Source,
  TaskCompletionSource<bool> Completion) : TerrariaProtocolCommand(0);

internal sealed record ApplyPlayerControlCommand(
  byte PlayerSlot,
  PlayerControlIntent Controls) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record ApplyTileManipulationCommand(
  byte PlayerSlot,
  TileManipulationIntent Intent) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record PlaceTileEntityCommand(
  byte PlayerSlot,
  TileEntityPlacementIntent Intent) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record OpenChestCommand(
  byte PlayerSlot,
  ChestOpenIntent Intent) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record ToggleDoorCommand(
  byte PlayerSlot,
  DoorToggleIntent Intent) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record UpdateSignCommand(
  byte PlayerSlot,
  SignUpdateIntent Intent) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record OpenSignCommand(
  byte PlayerSlot,
  SignOpenRequestPacket Request,
  SessionReplicationState ReplicationState,
  TaskCompletionSource<SignReplicationSnapshot?> Completion) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record TransferChestItemCommand(
  byte PlayerSlot,
  ChestTransferIntent Intent) : TerrariaProtocolCommand(PlayerSlot);

internal sealed record DestroySessionPlayerCommand(
  byte PlayerSlot,
  SessionReplicationState ReplicationState) : TerrariaProtocolCommand(PlayerSlot);
