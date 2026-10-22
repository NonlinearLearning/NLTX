namespace MinimalImplementation.Server.Modules.Social.Chat;

/// <summary>Routes authenticated chat messages through the registered Version4 command set.</summary>
public sealed class ChatCommandProcessor
{
  private readonly IReadOnlyDictionary<string, IChatCommand> _commands;

  public ChatCommandProcessor(IEnumerable<IChatCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(commands);
    Dictionary<string, IChatCommand> registrations = new(StringComparer.Ordinal);
    foreach (IChatCommand command in commands)
    {
      ArgumentNullException.ThrowIfNull(command);
      ArgumentException.ThrowIfNullOrWhiteSpace(command.CommandId);
      if (!registrations.TryAdd(command.CommandId, command))
      {
        throw new ArgumentException(
          $"Chat command '{command.CommandId}' has more than one owner.",
          nameof(commands));
      }
    }

    _commands = new System.Collections.ObjectModel.ReadOnlyDictionary<string, IChatCommand>(
      registrations);
  }

  public bool IsRegistered(string commandId) => _commands.ContainsKey(commandId);

  public ChatCommandResult ProcessIncomingMessage(
    ChatMessage message,
    byte authenticatedPlayerSlot,
    ModuleDispatchContext context)
  {
    ArgumentNullException.ThrowIfNull(message);
    ArgumentNullException.ThrowIfNull(context);
    if (!ChatInputBudget.TryValidate(message, out string? validationError))
    {
      return ChatCommandResult.Rejected(
        MinimalImplementation.Contracts.ProtocolErrorCode.MalformedFrame,
        validationError!);
    }

    if (!_commands.TryGetValue(message.CommandId, out IChatCommand? command))
    {
      return ChatCommandResult.Rejected(
        MinimalImplementation.Contracts.ProtocolErrorCode.UnknownSubtype,
        $"Chat command '{message.CommandId}' is not registered.");
    }

    return command.ProcessIncomingMessage(message.Text, authenticatedPlayerSlot, context);
  }
}
