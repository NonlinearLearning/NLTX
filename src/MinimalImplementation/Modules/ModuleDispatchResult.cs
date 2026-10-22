using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

public enum ModuleDispatchDisposition
{
  Handled,
  NotRegistered,
  Rejected
}

public sealed class ModuleDispatchResult
{
  private readonly OutboundPacketIntent[] _outboundIntents;
  private readonly bool _replayOutboundEffects;

  private ModuleDispatchResult(
    ModuleDispatchDisposition disposition,
    ProtocolErrorCode? errorCode,
    CommitVersion? commitVersion,
    IReadOnlyCollection<OutboundPacketIntent> outboundIntents,
    string? detail,
    IOutboundDeliverySettlement? deliverySettlement,
    bool replayOutboundEffects = true)
  {
    ArgumentNullException.ThrowIfNull(outboundIntents);
    Disposition = disposition;
    ErrorCode = errorCode;
    CommitVersion = commitVersion;
    _outboundIntents = outboundIntents.ToArray();
    OutboundIntents = Array.AsReadOnly(_outboundIntents);
    Detail = detail;
    if (deliverySettlement is not null)
    {
      if (deliverySettlement.Intents.Count != _outboundIntents.Length)
      {
        throw new ArgumentException(
          "An outbound delivery settlement must cover every returned intent.",
          nameof(deliverySettlement));
      }

      for (int index = 0; index < _outboundIntents.Length; index++)
      {
        if (!ReferenceEquals(deliverySettlement.Intents[index], _outboundIntents[index]))
        {
          throw new ArgumentException(
            "An outbound delivery settlement must preserve intent identity and order.",
            nameof(deliverySettlement));
        }
      }
    }

    DeliverySettlement = deliverySettlement;
    _replayOutboundEffects = replayOutboundEffects;
  }

  public ModuleDispatchDisposition Disposition { get; }

  public ProtocolErrorCode? ErrorCode { get; }

  public CommitVersion? CommitVersion { get; }

  public IReadOnlyList<OutboundPacketIntent> OutboundIntents { get; }

  public string? Detail { get; }

  public IOutboundDeliverySettlement? DeliverySettlement { get; }

  /// <summary>
  /// Disable replay of outbound effects when repeating a response can grant an item again.
  /// A replay then reports the original commit without intents or a delivery settlement.
  /// </summary>
  public static ModuleDispatchResult Handled(
    CommitVersion? commitVersion = null,
    IReadOnlyCollection<OutboundPacketIntent>? outboundIntents = null,
    IOutboundDeliverySettlement? deliverySettlement = null,
    bool replayOutboundEffects = true)
  {
    return new ModuleDispatchResult(
      ModuleDispatchDisposition.Handled,
      null,
      commitVersion,
      outboundIntents ?? Array.Empty<OutboundPacketIntent>(),
      null,
      deliverySettlement,
      replayOutboundEffects);
  }

  public static ModuleDispatchResult NotRegistered(ushort moduleId)
  {
    return new ModuleDispatchResult(
      ModuleDispatchDisposition.NotRegistered,
      null,
      null,
      Array.Empty<OutboundPacketIntent>(),
      $"Module {moduleId} has no registered owner; the protocol boundary must apply its " +
      "unknown-module policy.",
      null);
  }

  public static ModuleDispatchResult Rejected(ProtocolErrorCode errorCode, string detail)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(detail);
    return new ModuleDispatchResult(
      ModuleDispatchDisposition.Rejected,
      errorCode,
      null,
      Array.Empty<OutboundPacketIntent>(),
      detail,
      null);
  }

  internal ModuleDispatchResult ForReplay()
  {
    if (_replayOutboundEffects)
    {
      return this;
    }

    return new ModuleDispatchResult(
      Disposition,
      ErrorCode,
      CommitVersion,
      Array.Empty<OutboundPacketIntent>(),
      Detail,
      null);
  }
}
