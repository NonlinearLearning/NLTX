namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class DebugCommandCatalogAdapter
{
  private readonly Dictionary<string, IDebugCommand> _commands =
    new(StringComparer.OrdinalIgnoreCase);

  public void Add(IDebugCommand command)
  {
    ArgumentNullException.ThrowIfNull(command);
    _commands[command.Metadata.Name] = command;
  }

  public bool TryGet(string name, out IDebugCommand? command)
  {
    return _commands.TryGetValue(name, out command);
  }
}
