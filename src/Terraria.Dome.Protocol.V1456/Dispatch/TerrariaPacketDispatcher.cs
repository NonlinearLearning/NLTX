using System;
using System.IO;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;

namespace Terraria.Dome.Protocol.V1456.Dispatch;

public sealed class TerrariaPacketDispatcher
{
  public TerrariaPacketDispatchResult Dispatch(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    ArgumentNullException.ThrowIfNull(session);

    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    TerrariaMessageDescriptor descriptor = TerrariaMessageCatalog.Get(frame.MessageId);
    if (descriptor.Direction == TerrariaPacketDirection.ServerToClient)
    {
      throw new InvalidDataException("Terraria server-only packet was received from a client.");
    }

    return frame.MessageId switch
    {
      TerrariaMessageId.SyncPlayer => new TerrariaPacketDispatchResult(
        TerrariaPacketDispatchOutcome.PlayerProfileAccepted,
        session.AcceptPlayerProfile(frameBytes),
        null,
        null,
        null,
        null,
        null,
        null,
        null),
      TerrariaMessageId.SyncEquipment => RoutePlayerEquipment(session, frameBytes),
      TerrariaMessageId.PlayerLifeMana => RoutePlayerLifeMana(session, frameBytes),
      TerrariaMessageId.ItemRotationAndAnimation => RoutePlayerMana(session, frameBytes),
      TerrariaMessageId.PlayerBuffs => RoutePlayerBuffs(session, frameBytes),
      TerrariaMessageId.AddPlayerBuffPvp => RouteAddPlayerBuffPvp(session, frameBytes),
      TerrariaMessageId.PlayerUuid => RouteBootstrap(session, frameBytes),
      TerrariaMessageId.SyncLoadout => RouteBootstrap(session, frameBytes),
      TerrariaMessageId.PlayerControls => RoutePlayerControls(session, frameBytes),
      TerrariaMessageId.SyncPlayerZone => RoutePlayerZone(session, frameBytes),
      TerrariaMessageId.UniqueTownNpcInfoSyncRequest => RouteUniqueTownNpcInfoSyncRequest(
        session,
        frameBytes),
      TerrariaMessageId.SyncTalkNpc => RouteClientTalkNpc(session, frameBytes),
      TerrariaMessageId.SyncProjectile => RouteClientProjectile(session, frameBytes),
      TerrariaMessageId.KillProjectile => RouteClientProjectileTermination(session, frameBytes),
      TerrariaMessageId.ClientSyncedInventory => RouteClientSyncedInventory(session, frameBytes),
      TerrariaMessageId.NetModules => RouteNetModule(session, frameBytes),
      TerrariaMessageId.TileManipulation => new TerrariaPacketDispatchResult(
        TerrariaPacketDispatchOutcome.TileManipulationAccepted,
        null,
        null,
        null,
        session.AcceptTileManipulation(frameBytes),
        null,
        null,
        null,
        null),
      TerrariaMessageId.TileEntityPlacement => new TerrariaPacketDispatchResult(
        TerrariaPacketDispatchOutcome.TileEntityPlacementAccepted,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        TileEntityPlacement: TerrariaPacketCodec.DecodeTileEntityPlacement(frameBytes)),
      TerrariaMessageId.RequestChestOpen => new TerrariaPacketDispatchResult(
        TerrariaPacketDispatchOutcome.ChestOpenAccepted,
        null,
        null,
        null,
        null,
        session.AcceptChestOpen(frameBytes),
        null,
        null,
        null),
      TerrariaMessageId.SyncPlayerChest => new TerrariaPacketDispatchResult(
        TerrariaPacketDispatchOutcome.ChestTransferAccepted,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        session.AcceptChestTransfer(frameBytes)),
      TerrariaMessageId.ToggleDoorState => new TerrariaPacketDispatchResult(
        TerrariaPacketDispatchOutcome.DoorToggleAccepted,
        null,
        null,
        null,
        null,
        null,
        session.AcceptDoorToggle(frameBytes),
        null,
        null),
      TerrariaMessageId.OpenSignRequest => RouteSignOpenRequest(session, frameBytes),
      TerrariaMessageId.OpenSignResponse => new TerrariaPacketDispatchResult(
        TerrariaPacketDispatchOutcome.SignUpdateAccepted,
        null,
        null,
        null,
        null,
        null,
        null,
        session.AcceptSignUpdate(frameBytes),
        null),
      TerrariaMessageId.RequestWorldData => RouteWorldDataRequest(session, frameBytes),
      TerrariaMessageId.SpawnTileData => RouteSpawnTileData(session, frameBytes),
      TerrariaMessageId.RequestSection => RouteRequestSection(session, frameBytes),
      TerrariaMessageId.PlayerSpawn => RoutePlayerSpawn(session, frameBytes),
      TerrariaMessageId.Ping => RoutePing(session, frameBytes),
      _ => throw new InvalidDataException(
        $"Terraria packet {descriptor.Name} is not supported for this session.")
    };
  }

  private static TerrariaPacketDispatchResult RouteNetModule(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = session.AcceptNetModule(frameBytes);
    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.NetModuleAccepted,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      ResponseFrame: packet.ResponseFrame);
  }

  private static TerrariaPacketDispatchResult RouteSignOpenRequest(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    SignOpenRequestPacket request = session.AcceptSignOpenRequest(frameBytes);
    return CreateActiveSynchronizationResult() with { SignOpenRequest = request };
  }

  private static TerrariaPacketDispatchResult RouteClientProjectile(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    session.AcceptClientProjectile(frameBytes);
    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.ClientProjectileSyncIgnored,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null);
  }

  private static TerrariaPacketDispatchResult RouteClientProjectileTermination(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    _ = session.AcceptClientProjectileTermination(frameBytes);
    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.ClientProjectileTerminationIgnored,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null);
  }

  private static TerrariaPacketDispatchResult RouteClientSyncedInventory(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    session.AcceptClientSyncedInventory(frameBytes);
    return CreateActiveSynchronizationResult();
  }

  private static TerrariaPacketDispatchResult RouteClientTalkNpc(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    session.AcceptClientTalkNpc(frameBytes);
    return CreateActiveSynchronizationResult();
  }

  private static TerrariaPacketDispatchResult RouteUniqueTownNpcInfoSyncRequest(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    session.AcceptUniqueTownNpcInfoSyncRequest(frameBytes);
    return CreateActiveSynchronizationResult();
  }

  private static TerrariaPacketDispatchResult RoutePlayerBuffs(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    if (session.State != TerrariaSessionState.Active)
    {
      return RouteBootstrap(session, frameBytes);
    }

    _ = session.AcceptActivePlayerBuffs(frameBytes);
    return CreateActiveSynchronizationResult();
  }

  private static TerrariaPacketDispatchResult RouteAddPlayerBuffPvp(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    AddPlayerBuffPvpPacket request = session.AcceptActivePlayerBuffPvp(frameBytes);
    return CreateActiveSynchronizationResult() with { AddPlayerBuffPvp = request };
  }

  private static TerrariaPacketDispatchResult RoutePlayerEquipment(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    if (session.State != TerrariaSessionState.Active)
    {
      return RouteBootstrap(session, frameBytes);
    }

    _ = session.AcceptActivePlayerEquipment(frameBytes);
    return CreateActiveSynchronizationResult();
  }

  private static TerrariaPacketDispatchResult RoutePlayerLifeMana(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    if (session.State != TerrariaSessionState.Active)
    {
      return RouteBootstrap(session, frameBytes);
    }

    _ = session.AcceptActivePlayerLifeMana(frameBytes);
    return CreateActiveSynchronizationResult();
  }

  private static TerrariaPacketDispatchResult RoutePlayerMana(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    if (session.State != TerrariaSessionState.Active)
    {
      return RouteBootstrap(session, frameBytes);
    }

    _ = session.AcceptActivePlayerMana(frameBytes);
    return CreateActiveSynchronizationResult();
  }

  private static TerrariaPacketDispatchResult RoutePlayerZone(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    _ = session.AcceptPlayerZone(frameBytes);
    return CreateActiveSynchronizationResult();
  }

  private static TerrariaPacketDispatchResult RoutePlayerSpawn(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    bool isInitialSpawn = session.State == TerrariaSessionState.AwaitingPlayerSpawn;
    PlayerSpawnPacket spawn = session.AcceptPlayerSpawn(frameBytes);
    return new TerrariaPacketDispatchResult(
      isInitialSpawn
        ? TerrariaPacketDispatchOutcome.PlayerSpawnAccepted
        : TerrariaPacketDispatchOutcome.PlayerSpawnUpdated,
      null,
      null,
      spawn,
      null,
      null,
      null,
      null,
      null);
  }

  private static TerrariaPacketDispatchResult RoutePlayerControls(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    PlayerControlIntent controls = session.AcceptPlayerControls(frameBytes);
    LegacyPlayerControlsState legacyControls = session.LegacyPlayerControls ??
      throw new InvalidDataException("Terraria PlayerControls did not retain compatibility state.");
    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.PlayerControlsAccepted,
      null,
      controls,
      null,
      null,
      null,
      null,
      null,
      null,
      LegacyPlayerControls: legacyControls);
  }

  private static TerrariaPacketDispatchResult RouteSpawnTileData(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    _ = session.AcceptSpawnTileData(frameBytes);
    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.TileDataRequested,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null);
  }

  private static TerrariaPacketDispatchResult RouteRequestSection(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    _ = session.AcceptRequestSection(frameBytes);
    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.SectionRequested,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null);
  }

  private static TerrariaPacketDispatchResult RouteWorldDataRequest(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    PlayerBootstrapState bootstrap = session.AcceptRequestWorldData(frameBytes);
    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.WorldDataRequested,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      bootstrap);
  }

  private static TerrariaPacketDispatchResult RoutePing(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaPacketCodec.ValidatePing(frameBytes);
    if (session.State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria Ping packet arrived before the session was active.");
    }

    return CreateActiveSynchronizationResult(TerrariaPacketCodec.EncodePing());
  }

  private static TerrariaPacketDispatchResult RouteBootstrap(
    TerrariaSession session,
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    switch (frame.MessageId)
    {
      case TerrariaMessageId.SyncEquipment:
        _ = session.AcceptPlayerEquipment(frameBytes);
        break;
      case TerrariaMessageId.PlayerLifeMana:
        _ = session.AcceptPlayerLifeMana(frameBytes);
        break;
      case TerrariaMessageId.ItemRotationAndAnimation:
        _ = session.AcceptPlayerMana(frameBytes);
        break;
      case TerrariaMessageId.PlayerBuffs:
        _ = session.AcceptPlayerBuffs(frameBytes);
        break;
      case TerrariaMessageId.PlayerUuid:
        _ = session.AcceptPlayerUuid(frameBytes);
        break;
      case TerrariaMessageId.SyncLoadout:
        _ = session.AcceptPlayerLoadout(frameBytes);
        break;
      default:
        throw new InvalidDataException("Terraria packet is not a bootstrap packet.");
    }

    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.PlayerBootstrapAccepted,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null);
  }

  private static TerrariaPacketDispatchResult CreateActiveSynchronizationResult(
    byte[]? responseFrame = null)
  {
    return new TerrariaPacketDispatchResult(
      TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      responseFrame);
  }

}
