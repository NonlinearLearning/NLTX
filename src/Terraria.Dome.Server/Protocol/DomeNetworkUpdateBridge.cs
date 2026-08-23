using System;
using Terraria.Dome.Protocol.V1456.Isolation;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;

namespace Terraria.Dome.Server.Protocol;

internal sealed class DomeNetworkUpdateBridge : IProtocolCommandSink
{
  private readonly Func<TerrariaProtocolCommand, bool> _enqueueCommand;

  public DomeNetworkUpdateBridge(Func<TerrariaProtocolCommand, bool> enqueueCommand)
  {
    _enqueueCommand = enqueueCommand ?? throw new ArgumentNullException(nameof(enqueueCommand));
  }

  public ProtocolCommandResult Accept(NetworkInboundEnvelope envelope)
  {
    TerrariaMessageId messageId = envelope.MessageId;
    switch (messageId)
    {
      case TerrariaMessageId.PlayerControls:
        return Enqueue(new ApplyPlayerControlCommand(
          envelope.PlayerSlot,
          TerrariaPacketCodec.DecodePlayerControls(envelope.FrameBytes.Span)));
      case TerrariaMessageId.TileManipulation:
        return Enqueue(new ApplyTileManipulationCommand(
          envelope.PlayerSlot,
          TerrariaPacketCodec.DecodeTileManipulation(envelope.FrameBytes.Span)));
      case TerrariaMessageId.TileEntityPlacement:
        return Enqueue(new PlaceTileEntityCommand(
          envelope.PlayerSlot,
          TerrariaPacketCodec.DecodeTileEntityPlacement(envelope.FrameBytes.Span)));
      case TerrariaMessageId.RequestChestOpen:
        return Enqueue(new OpenChestCommand(
          envelope.PlayerSlot,
          TerrariaPacketCodec.DecodeChestOpen(envelope.FrameBytes.Span)));
      case TerrariaMessageId.ToggleDoorState:
        return Enqueue(new ToggleDoorCommand(
          envelope.PlayerSlot,
          TerrariaPacketCodec.DecodeDoorToggle(envelope.FrameBytes.Span)));
      case TerrariaMessageId.OpenSignResponse:
        return Enqueue(new UpdateSignCommand(
          envelope.PlayerSlot,
          TerrariaPacketCodec.DecodeSignUpdate(envelope.FrameBytes.Span)));
      case TerrariaMessageId.SyncPlayerChest:
        return Enqueue(new TransferChestItemCommand(
          envelope.PlayerSlot,
          TerrariaPacketCodec.DecodeChestTransfer(envelope.FrameBytes.Span)));
      default:
        return ProtocolCommandResult.Unsupported;
    }
  }

  private ProtocolCommandResult Enqueue(TerrariaProtocolCommand command)
  {
    return _enqueueCommand(command)
      ? ProtocolCommandResult.Accepted
      : ProtocolCommandResult.Rejected;
  }
}
