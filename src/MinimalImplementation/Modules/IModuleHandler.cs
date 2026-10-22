using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

public interface IModuleHandler
{
  ushort ModuleId { get; }

  ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload);
}
