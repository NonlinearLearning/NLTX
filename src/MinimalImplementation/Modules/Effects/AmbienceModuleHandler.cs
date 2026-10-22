using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Effects;

/// <summary>Provides the server-only Packet 82 Ambience producer.</summary>
public sealed class AmbienceModuleHandler : IModuleHandler
{
  private const int SkyEntityTypeCount = 19;

  private readonly Packet82ModuleIntentProducer _producer;

  public AmbienceModuleHandler(Packet82ModuleIntentProducer producer)
  {
    ArgumentNullException.ThrowIfNull(producer);
    _producer = producer;
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.Ambience;

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
      "Ambience");
    if (validation is not null)
    {
      return validation;
    }

    return ModuleDispatchResult.Rejected(
      ProtocolErrorCode.Forbidden,
      "Packet 82 Ambience is server-to-client only.");
  }

  /// <summary>
  /// Creates an ambience broadcast from an already-approved server ambience decision.
  /// The caller owns spawn conditions, random seed selection, and commit ordering.
  /// </summary>
  public OutboundPacketIntent CreateCommittedSpawnIntent(
    Packet82AmbienceData ambience,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(ambience);
    Packet82PresentationValidation.ValidateTargets(targets, commitVersion);
    if (ambience.Seed < 0
      || ambience.SkyEntityType >= SkyEntityTypeCount)
    {
      throw new ArgumentOutOfRangeException(
        nameof(ambience),
        "Ambience must use a non-negative seed and a known sky entity type.");
    }

    return _producer.CreateIntent(
      ModuleId,
      ambience,
      targets,
      commitVersion);
  }
}
