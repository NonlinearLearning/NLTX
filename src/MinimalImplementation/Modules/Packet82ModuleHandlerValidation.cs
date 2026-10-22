using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

/// <summary>
/// Preserves the authenticated Packet 82 boundary when a module handler is invoked directly.
/// </summary>
internal static class Packet82ModuleHandlerValidation
{
  public static ModuleDispatchResult? ValidateActiveEnvelope(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    ushort moduleId,
    string moduleName)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);
    ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);

    if (envelope.MessageId != 82 || envelope.ModuleId != moduleId)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.UnknownModule,
        $"The {moduleName} handler accepts only Packet 82 module {moduleId}.");
    }

    if (envelope.Epoch != context.Epoch || !context.Sender.IsValid)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        $"The {moduleName} packet does not match the captured authenticated session.");
    }

    if (context.Stage != ModuleSessionStage.Active
        || !string.Equals(
          envelope.Stage,
          ModuleSessionStage.Active.ToString(),
          StringComparison.Ordinal))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.WrongStage,
        $"Packet 82 module {moduleId} requires an active authenticated session.");
    }

    return null;
  }
}
