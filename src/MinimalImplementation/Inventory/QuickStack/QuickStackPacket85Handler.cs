using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>Authenticates and handles Version4 packet 85, then emits its blocked-chest response.</summary>
public sealed class QuickStackPacket85Handler
{
  public const byte MessageId = 85;

  private static readonly SessionStage[] ActiveStages = [SessionStage.Active];

  private readonly IAuthenticatedPlayerPacketGate _sessions;
  private readonly QuickStackTransactionCoordinator _transactions;

  public QuickStackPacket85Handler(
    IAuthenticatedPlayerPacketGate sessions,
    QuickStackTransactionCoordinator transactions)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(transactions);
    _sessions = sessions;
    _transactions = transactions;
  }

  public QuickStackPacketDispatchResult Handle(
    InboundPacketEnvelope envelope,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    if (envelope.MessageId != MessageId)
    {
      return Failed(OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The QuickStack handler received a different message id."));
    }

    if (decodedPayload is not QuickStackChestsRequestPacket request)
    {
      return Failed(OwnerCommitResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 85 must contain a decoded slot list and smart-stack flag."));
    }

    if (envelope.WorldScope is not long scopeValue || scopeValue <= 0)
    {
      return Failed(OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "QuickStack requires an active world scope."));
    }

    OutboundPacketIntent? response = null;
    OwnerCommitResult dispatch = _sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      MessageId,
      ActiveStages,
      binding =>
      {
        var requestPayload = new QuickStackRequestPayload(request.ItemSlots, request.SmartStack);
        QuickStackTransactionResult transaction = _transactions.Execute(
          binding.PlayerHandle.Id,
          binding.Epoch,
          new WorldScopeId(scopeValue),
          requestPayload,
          envelope.ReceivedAt);
        if (!transaction.IsApplied || transaction.Commit.Version is not CommitVersion version)
        {
          return transaction.Commit;
        }

        using MemoryStream stream = new();
        var writer = new PacketWireWriter(stream, ushort.MaxValue - 3);
        QuickStackPayloadCodec.WriteBlockedChests(
          writer,
          new QuickStackBlockedChestPayload(transaction.BlockedChestIds),
          ContainerContentFacts.ChestIndexCount);
        response = new OutboundPacketIntent(
          [binding.Epoch],
          ignoreEpoch: null,
          MessageId,
          stream.ToArray(),
          version);
        return transaction.Commit;
      });

    return new QuickStackPacketDispatchResult(
      dispatch,
      response is null ? Array.Empty<OutboundPacketIntent>() : [response]);
  }

  private static QuickStackPacketDispatchResult Failed(OwnerCommitResult result) =>
    new(result, Array.Empty<OutboundPacketIntent>());
}
