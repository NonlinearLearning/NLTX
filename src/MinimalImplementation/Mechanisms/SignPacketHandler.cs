using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Applies Version4 sign packets and produces packet 47 routing intents.</summary>
public sealed class SignPacketHandler
{
  private const byte OpenSignRequestMessageId = 46;
  private const byte OpenSignResponseMessageId = 47;
  private const int MaximumResponsePayloadLength = ushort.MaxValue - 3;

  private readonly SignInteractionService _signs;

  public SignPacketHandler(SignInteractionService signs)
  {
    _signs = signs ?? throw new ArgumentNullException(nameof(signs));
  }

  public SignPacketDispatchResult HandleOpenRequest(
    InboundPacketEnvelope envelope,
    byte authenticatedPlayerSlot,
    OpenSignRequestPacket packet,
    WorldSnapshot world,
    CommitVersion commitVersion)
  {
    ValidateEnvelope(envelope, OpenSignRequestMessageId, authenticatedPlayerSlot, world,
      commitVersion);
    ArgumentNullException.ThrowIfNull(packet);
    if (!IsForCurrentWorld(envelope) || packet.X < 0 || packet.Y < 0)
    {
      return SignPacketDispatchResult.NoReply;
    }

    SignOpenDispatchResult open = _signs.HandleOpen(world, packet, authenticatedPlayerSlot);
    if (open.Response is null)
    {
      return new SignPacketDispatchResult(true, Array.Empty<OutboundPacketIntent>(), open, null);
    }

    byte[] payload = EncodeResponse(open.Response);
    var response = new OutboundPacketIntent(
      [envelope.Epoch],
      ignoreEpoch: null,
      OpenSignResponseMessageId,
      payload,
      commitVersion);
    return new SignPacketDispatchResult(true, [response], open, null);
  }

  public SignPacketDispatchResult HandleUpdate(
    InboundPacketEnvelope envelope,
    byte authenticatedPlayerSlot,
    OpenSignResponsePacket packet,
    WorldSnapshot world,
    IReadOnlyCollection<SessionEpoch> activeSessionEpochs,
    CommitVersion commitVersion)
  {
    ValidateEnvelope(envelope, OpenSignResponseMessageId, authenticatedPlayerSlot, world,
      commitVersion);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(activeSessionEpochs);
    if (!IsForCurrentWorld(envelope))
    {
      return SignPacketDispatchResult.NoReply;
    }

    SignUpdateDispatchResult update = _signs.HandleUpdate(
      world,
      packet,
      authenticatedPlayerSlot);
    if (update.Broadcast is null || !update.ExcludeAuthenticatedSender ||
        activeSessionEpochs.Count == 0)
    {
      return new SignPacketDispatchResult(true, Array.Empty<OutboundPacketIntent>(), null, update);
    }

    byte[] payload = EncodeResponse(update.Broadcast);
    var broadcast = new OutboundPacketIntent(
      activeSessionEpochs,
      ignoreEpoch: envelope.Epoch,
      OpenSignResponseMessageId,
      payload,
      commitVersion);
    return new SignPacketDispatchResult(true, [broadcast], null, update);
  }

  private static byte[] EncodeResponse(OpenSignResponsePacket packet)
  {
    using MemoryStream stream = new();
    var writer = new PacketWireWriter(stream, MaximumResponsePayloadLength);
    OpenSignResponsePacket.Write(
      writer,
      packet.SignIndex,
      packet.X,
      packet.Y,
      packet.Text,
      packet.Player,
      packet.Flags);
    return stream.ToArray();
  }

  private void ValidateEnvelope(
    InboundPacketEnvelope envelope,
    byte expectedMessageId,
    byte authenticatedPlayerSlot,
    WorldSnapshot world,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(world);
    if (envelope.MessageId != expectedMessageId)
    {
      throw new ArgumentException("The envelope message id does not match the sign packet.",
        nameof(envelope));
    }
    if (authenticatedPlayerSlot == byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(authenticatedPlayerSlot));
    }
    if (!commitVersion.IsValid || world.Version != commitVersion)
    {
      throw new ArgumentException(
        "A sign packet requires the matching committed world snapshot.", nameof(commitVersion));
    }
  }

  private bool IsForCurrentWorld(InboundPacketEnvelope envelope)
  {
    return envelope.WorldScope == _signs.WorldScope;
  }
}

public sealed record SignPacketDispatchResult(
  bool Handled,
  IReadOnlyList<OutboundPacketIntent> OutboundIntents,
  SignOpenDispatchResult? OpenResult,
  SignUpdateDispatchResult? UpdateResult)
{
  public static SignPacketDispatchResult NoReply { get; } = new(
    true,
    Array.Empty<OutboundPacketIntent>(),
    null,
    null);
}
