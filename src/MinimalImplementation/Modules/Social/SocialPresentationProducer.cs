using System.IO;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Modules.Social;

public sealed record InstrumentSoundRequest(
  SessionEpoch SourceEpoch,
  byte AuthenticatedPlayerSlot,
  ModuleSessionStage Stage,
  float Pitch,
  IReadOnlyCollection<SessionEpoch> VisibleTargets);

public sealed record ItemUseSoundRequest(
  SessionEpoch SourceEpoch,
  byte AuthenticatedPlayerSlot,
  ModuleSessionStage Stage,
  IReadOnlyCollection<SessionEpoch> VisibleTargets,
  CommitVersion? OwnerCommitVersion = null);

public enum SocialPresentationOutcomeStatus
{
  Applied,
  NoRecipients,
  Rejected
}

public sealed record SocialPresentationResult(
  SocialPresentationOutcomeStatus Status,
  CommitVersion? Version,
  ProtocolErrorCode? ErrorCode,
  string? Detail,
  OutboundPacketIntent? Intent)
{
  public static SocialPresentationResult Applied(
    CommitVersion version,
    OutboundPacketIntent intent)
  {
    ArgumentNullException.ThrowIfNull(intent);
    return new SocialPresentationResult(
      SocialPresentationOutcomeStatus.Applied,
      version,
      null,
      null,
      intent);
  }

  public static SocialPresentationResult NoRecipients()
  {
    return new SocialPresentationResult(
      SocialPresentationOutcomeStatus.NoRecipients,
      null,
      null,
      "The authenticated source has no other visible active recipients.",
      null);
  }

  public static SocialPresentationResult Rejected(
    ProtocolErrorCode errorCode,
    string detail)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(detail);
    return new SocialPresentationResult(
      SocialPresentationOutcomeStatus.Rejected,
      null,
      errorCode,
      detail,
      null);
  }
}

/// <summary>
/// Produces ordinary server-to-client social presentation packets after P02 visibility filtering.
/// This class does not inspect selected items, invoke audio/UI code, or write a transport.
/// </summary>
public sealed class SocialPresentationProducer
{
  private const int MaximumRecipientCount = byte.MaxValue - 1;

  private CommitVersion _version = CommitVersion.Initial;

  public SocialPresentationResult CreateInstrumentSoundIntent(InstrumentSoundRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (request.Stage != ModuleSessionStage.Active)
    {
      return SocialPresentationResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Instrument sound requires an active authenticated session.");
    }

    if (!float.IsFinite(request.Pitch))
    {
      return SocialPresentationResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Instrument pitch must be a finite Single value.");
    }

    return CreateRelayIntent(
      request.SourceEpoch,
      request.AuthenticatedPlayerSlot,
      request.VisibleTargets,
      messageId: 58,
      writer => InstrumentSoundPacket.Write(
        writer,
        request.AuthenticatedPlayerSlot,
        request.Pitch));
  }

  public SocialPresentationResult CreateItemUseSoundIntent(ItemUseSoundRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (request.Stage != ModuleSessionStage.Active)
    {
      return SocialPresentationResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Item-use sound requires an active authenticated session.");
    }

    if (request.OwnerCommitVersion is not CommitVersion commitVersion
        || !commitVersion.IsValid)
    {
      return SocialPresentationResult.Rejected(
        ProtocolErrorCode.NotReady,
        "Item-use sound requires the committed item event version from its owner.");
    }

    return CreateRelayIntent(
      request.SourceEpoch,
      request.AuthenticatedPlayerSlot,
      request.VisibleTargets,
      messageId: 152,
      writer => ItemUseSoundPacket.Write(writer, request.AuthenticatedPlayerSlot),
      ownerCommitVersion: request.OwnerCommitVersion);
  }

  private SocialPresentationResult CreateRelayIntent(
    SessionEpoch sourceEpoch,
    byte authenticatedPlayerSlot,
    IReadOnlyCollection<SessionEpoch>? visibleTargets,
    byte messageId,
    Action<PacketWireWriter> writePayload,
    CommitVersion? ownerCommitVersion = null)
  {
    if (!sourceEpoch.IsValid)
    {
      return SocialPresentationResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "A social presentation requires a valid authenticated source epoch.");
    }

    if (authenticatedPlayerSlot >= byte.MaxValue)
    {
      return SocialPresentationResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The authenticated player slot is outside the Version4 player range.");
    }

    if (!TryGetOtherTargets(sourceEpoch, visibleTargets, out SessionEpoch[] targets))
    {
      return SocialPresentationResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Social presentation visibility facts must contain valid session epochs.");
    }

    if (targets.Length == 0)
    {
      return SocialPresentationResult.NoRecipients();
    }

    ArgumentNullException.ThrowIfNull(writePayload);
    using MemoryStream stream = new();
    PacketWireWriter writer = new(stream, 1_048_576);
    writePayload(writer);
    CommitVersion nextVersion = ownerCommitVersion ?? _version.Next();
    OutboundPacketIntent intent = new(
      targets,
      sourceEpoch,
      messageId,
      stream.ToArray(),
      nextVersion);
    if (ownerCommitVersion is null)
    {
      _version = nextVersion;
    }

    return SocialPresentationResult.Applied(nextVersion, intent);
  }

  private static bool TryGetOtherTargets(
    SessionEpoch sourceEpoch,
    IReadOnlyCollection<SessionEpoch>? visibleTargets,
    out SessionEpoch[] targets)
  {
    targets = Array.Empty<SessionEpoch>();
    if (visibleTargets is null)
    {
      return false;
    }

    HashSet<SessionEpoch> uniqueTargets = new();
    foreach (SessionEpoch target in visibleTargets)
    {
      if (!target.IsValid)
      {
        return false;
      }

      if (target != sourceEpoch)
      {
        uniqueTargets.Add(target);
      }
    }

    if (uniqueTargets.Count > MaximumRecipientCount)
    {
      return false;
    }

    targets = uniqueTargets.OrderBy(target => target.Value).ToArray();
    return true;
  }
}
