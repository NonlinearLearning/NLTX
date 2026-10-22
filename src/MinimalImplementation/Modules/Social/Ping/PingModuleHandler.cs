using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Social.Ping;

/// <summary>Routes authenticated client pings to the sender's other visible sessions.</summary>
public sealed class PingModuleHandler : IModuleHandler
{
  private const float MaximumCoordinateMagnitude = 1_000_000f;
  private const int MaximumRecipientCount = byte.MaxValue - 1;

  private readonly Packet82ModuleIntentProducer _producer;
  private CommitVersion _commitVersion = CommitVersion.Initial;

  public PingModuleHandler(Packet82ModuleIntentProducer producer)
  {
    ArgumentNullException.ThrowIfNull(producer);
    _producer = producer;
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.Ping;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);

    ModuleDispatchResult? validation = Packet82ModuleHandlerValidation.ValidateActiveEnvelope(
      envelope,
      context,
      ModuleId,
      "Ping");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Packet82PingData request
      || !IsValidPosition(request.Position))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 Ping coordinates must be finite and within the accepted bounds.");
    }

    if (!TryGetVisibleTargets(context, out List<SessionEpoch> targets))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Ping visibility facts exceed the Version4 player recipient bound.");
    }

    if (targets.Count == 0)
    {
      return ModuleDispatchResult.Handled();
    }

    CommitVersion nextVersion = _commitVersion.Next();
    OutboundPacketIntent intent = _producer.CreateIntent(
      ModuleId,
      request,
      targets,
      nextVersion);
    _commitVersion = nextVersion;
    return ModuleDispatchResult.Handled(nextVersion, new[] { intent });
  }

  private static bool IsValidPosition(PacketVector2 position)
  {
    return float.IsFinite(position.X)
      && float.IsFinite(position.Y)
      && MathF.Abs(position.X) <= MaximumCoordinateMagnitude
      && MathF.Abs(position.Y) <= MaximumCoordinateMagnitude;
  }

  private static bool TryGetVisibleTargets(
    ModuleDispatchContext context,
    out List<SessionEpoch> targets)
  {
    targets = new(MaximumRecipientCount);
    HashSet<SessionEpoch> uniqueTargets = new();
    foreach (ModuleRecipient recipient in context.ActiveRecipients)
    {
      if (recipient.Epoch == context.Epoch || !uniqueTargets.Add(recipient.Epoch))
      {
        continue;
      }

      if (targets.Count == MaximumRecipientCount)
      {
        return false;
      }

      targets.Add(recipient.Epoch);
    }

    return true;
  }
}
