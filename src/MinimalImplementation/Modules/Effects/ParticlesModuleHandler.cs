using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Effects;

/// <summary>Validates client particle requests and returns visible-client broadcast intents.</summary>
public sealed class ParticlesModuleHandler : IModuleHandler, IModuleEpochRetirable
{
  private const byte MakeAWishSetSpawnSoundType = 64;

  private readonly Packet82ModuleIntentProducer _producer;
  private readonly ParticleRateLimiter _rateLimiter = new();
  private CommitVersion _commitVersion = CommitVersion.Initial;

  public ParticlesModuleHandler(Packet82ModuleIntentProducer producer)
  {
    ArgumentNullException.ThrowIfNull(producer);
    _producer = producer;
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.Particles;

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
      "Particles");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Packet82ParticlesData request
      || !Packet82PresentationValidation.IsValidParticleData(request))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 Particles contains an invalid type or non-finite/out-of-range settings.");
    }

    if (!TryGetVisibleTargets(context, out List<SessionEpoch> targets))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Particle visibility facts exceed the Version4 player recipient bound.");
    }

    if (!_rateLimiter.TryConsume(context.Epoch, envelope.ReceivedAt))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.RateLimited,
        "The particle request exceeded the per-session presentation event limit.");
    }

    if (targets.Count == 0)
    {
      return ModuleDispatchResult.Handled();
    }

    Packet82ParticlesData authenticatedRequest = RebindInvokingPlayer(request, context.PlayerSlot);
    CommitVersion nextVersion = _commitVersion.Next();
    OutboundPacketIntent intent = CreateCommittedIntent(
      authenticatedRequest,
      targets,
      nextVersion);
    _commitVersion = nextVersion;
    return ModuleDispatchResult.Handled(nextVersion, new[] { intent });
  }

  /// <summary>
  /// Creates a bounded particle intent for a P05/P06 committed presentation event.
  /// Gameplay effects must be committed by their owning direction before calling this method.
  /// </summary>
  public OutboundPacketIntent CreateCommittedIntent(
    Packet82ParticlesData particleData,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(particleData);
    Packet82PresentationValidation.ValidateTargets(targets, commitVersion);
    if (!Packet82PresentationValidation.IsValidParticleData(particleData))
    {
      throw new ArgumentOutOfRangeException(
        nameof(particleData),
        "Particle data must use a Version4 type and bounded finite settings.");
    }

    return _producer.CreateIntent(
      ModuleId,
      particleData,
      targets,
      commitVersion);
  }

  /// <summary>
  /// Produces the Version4 Make-A-Wish set sound after P04 commits the shimmer result.
  /// The server source uses player sentinel 255; coordinates and recipients come from that commit.
  /// </summary>
  public OutboundPacketIntent CreateCommittedMakeAWishSetIntent(
    float positionX,
    float positionY,
    IReadOnlyCollection<SessionEpoch> targets,
    CommitVersion commitVersion)
  {
    Packet82ParticlesData particleData = new(
      MakeAWishSetSpawnSoundType,
      new Packet82ParticleSettings(
        positionX,
        positionY,
        0f,
        0f,
        0,
        byte.MaxValue));
    return CreateCommittedIntent(particleData, targets, commitVersion);
  }

  /// <summary>Retires the limiter state for a closed authenticated session.</summary>
  public void RetireEpoch(SessionEpoch epoch)
  {
    _rateLimiter.RetireEpoch(epoch);
  }

  private static bool TryGetVisibleTargets(
    ModuleDispatchContext context,
    out List<SessionEpoch> targets)
  {
    const int maximumPlayerSlots = byte.MaxValue;
    targets = new(maximumPlayerSlots - 1);
    HashSet<SessionEpoch> uniqueTargets = new();
    foreach (ModuleRecipient recipient in context.ActiveRecipients)
    {
      if (recipient.Epoch == context.Epoch || !uniqueTargets.Add(recipient.Epoch))
      {
        continue;
      }

      if (targets.Count >= maximumPlayerSlots - 1)
      {
        return false;
      }

      targets.Add(recipient.Epoch);
    }

    return true;
  }

  private static Packet82ParticlesData RebindInvokingPlayer(
    Packet82ParticlesData request,
    byte authenticatedPlayerSlot)
  {
    Packet82ParticleSettings settings = request.Settings;
    return new Packet82ParticlesData(
      request.ParticleType,
      settings with { InvokingPlayer = authenticatedPlayerSlot });
  }
}
