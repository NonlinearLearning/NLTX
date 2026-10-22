using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;

namespace MinimalImplementation.Server.Modules.Social.Chat;

/// <summary>Handles client-to-server ChatMessage requests for Packet 82 Text module 1.</summary>
public sealed class TextModuleHandler : IModuleHandler
{
  private readonly ChatCommandProcessor _commandProcessor;

  public TextModuleHandler(ChatCommandProcessor commandProcessor)
  {
    ArgumentNullException.ThrowIfNull(commandProcessor);
    _commandProcessor = commandProcessor;
  }

  public ushort ModuleId => 1;

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
      "Text");
    if (validation is not null)
    {
      return validation;
    }

    if (decodedPayload is not Version4ModulePayloads.TextCommandData command
        || command.CommandName is null
        || command.Text is null)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 Text input must contain a command id and text string.");
    }

    ChatCommandResult result = _commandProcessor.ProcessIncomingMessage(
      new ChatMessage(command.CommandName, command.Text),
      context.PlayerSlot,
      context);
    if (!result.IsHandled)
    {
      return ModuleDispatchResult.Rejected(
        result.ErrorCode ?? ProtocolErrorCode.UnknownSubtype,
        result.Detail ?? "The chat command was rejected.");
    }

    return ModuleDispatchResult.Handled(result.CommitVersion, result.OutboundIntents);
  }
}
