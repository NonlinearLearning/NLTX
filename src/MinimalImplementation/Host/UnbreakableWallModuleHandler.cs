using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules;

namespace MinimalImplementation.Server.Host;

public sealed class UnbreakableWallModuleHandler : IModuleHandler
{
  public const ushort WallScanModuleId = 14;

  public ushort ModuleId => 14;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);
    if (envelope.MessageId != 82 || envelope.ModuleId != ModuleId ||
        envelope.Epoch != context.Epoch || !context.Sender.IsValid)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "The module 14 packet does not match its authenticated session envelope.");
    }

    if (context.Stage != ModuleSessionStage.Active)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Packet 82 module 14 requires an active authenticated session.");
    }

    return ModuleDispatchResult.Handled();
  }
}
