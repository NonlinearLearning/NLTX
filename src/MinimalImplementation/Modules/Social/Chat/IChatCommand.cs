namespace MinimalImplementation.Server.Modules.Social.Chat;

public interface IChatCommand
{
  string CommandId { get; }

  ChatCommandResult ProcessIncomingMessage(
    string text,
    byte authenticatedPlayerSlot,
    ModuleDispatchContext context);
}
