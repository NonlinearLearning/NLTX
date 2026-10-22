using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

/// <summary>Routes decoded module messages to their unique domain owner.</summary>
public sealed class ModuleDispatcher
{
  public const ushort ModuleCount = 15;

  private readonly IReadOnlyDictionary<ushort, IModuleHandler> _handlers;
  private readonly ModuleDispatchReplayCache _replayCache;

  public ModuleDispatcher(
    IEnumerable<IModuleHandler> handlers,
    ModuleDispatchReplayCache? replayCache = null)
  {
    ArgumentNullException.ThrowIfNull(handlers);
    _replayCache = replayCache ?? new ModuleDispatchReplayCache();
    var registrations = new Dictionary<ushort, IModuleHandler>();
    foreach (IModuleHandler handler in handlers)
    {
      ArgumentNullException.ThrowIfNull(handler);
      if (handler.ModuleId >= ModuleCount)
      {
        throw new ArgumentOutOfRangeException(
          nameof(handlers),
          $"Packet 82 module id {handler.ModuleId} is outside the Version4 range 0-14.");
      }

      if (!registrations.TryAdd(handler.ModuleId, handler))
      {
        throw new ArgumentException(
          $"Packet 82 module {handler.ModuleId} has more than one owner.",
          nameof(handlers));
      }
    }

    _handlers = new System.Collections.ObjectModel.ReadOnlyDictionary<ushort, IModuleHandler>(
      registrations
        .OrderBy(static registration => registration.Key)
        .ToDictionary(static registration => registration.Key, static registration => registration.Value));
  }

  public bool IsComplete => _handlers.Count == ModuleCount;

  public IReadOnlyCollection<ushort> RegisteredModuleIds => _handlers.Keys.ToArray();

  public IReadOnlyList<ushort> MissingModuleIds => Enumerable.Range(0, ModuleCount)
    .Select(static value => (ushort)value)
    .Where(moduleId => !_handlers.ContainsKey(moduleId))
    .ToArray();

  public ModuleDispatchResult Dispatch(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);

    if (envelope.MessageId != 82)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.UnknownPacket,
        "The module dispatcher accepts only Packet 82 envelopes.");
    }

    if (envelope.Epoch != context.Epoch || !context.Sender.IsValid)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "The authenticated module sender does not match the envelope epoch.");
    }

    if (!string.Equals(envelope.Stage, context.Stage.ToString(), StringComparison.Ordinal))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "The module sender stage does not match the stage captured at frame receipt.");
    }

    if (context.Stage != ModuleSessionStage.Active)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Packet 82 module dispatch requires an active authenticated session.");
    }

    return _replayCache.Execute(
      envelope,
      () => DispatchNewEnvelope(envelope, context, decodedPayload));
  }

  /// <summary>Releases bounded replay state when the owning authenticated session closes.</summary>
  public bool RetireEpoch(SessionEpoch epoch)
  {
    bool firstRetirement = _replayCache.RetireEpoch(epoch);
    if (!firstRetirement)
    {
      return false;
    }

    foreach (IModuleEpochRetirable retirable in _handlers.Values.OfType<IModuleEpochRetirable>())
    {
      retirable.RetireEpoch(epoch);
    }

    return true;
  }

  /// <summary>
  /// Closes one authenticated epoch at the server commit boundary and retires its replay and
  /// handler-local state. Repeated calls are idempotent and do not invoke owner retirement twice.
  /// </summary>
  public bool CloseEpoch(SessionEpoch epoch)
  {
    return RetireEpoch(epoch);
  }

  /// <summary>Evicts replay results using a receipt timestamp already captured by P01.</summary>
  public int EvictExpiredReplayResults(DateTimeOffset observedReceivedAt)
  {
    return _replayCache.EvictExpiredResults(observedReceivedAt);
  }

  private ModuleDispatchResult DispatchNewEnvelope(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    if (envelope.ModuleId is not ushort moduleId || moduleId >= ModuleCount)
    {
      return ModuleDispatchResult.NotRegistered(envelope.ModuleId ?? ushort.MaxValue);
    }

    if (!_handlers.TryGetValue(moduleId, out IModuleHandler? handler))
    {
      return ModuleDispatchResult.NotRegistered(moduleId);
    }

    return handler.Handle(envelope, context, decodedPayload);
  }
}
