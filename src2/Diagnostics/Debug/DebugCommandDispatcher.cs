namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class DebugCommandDispatcher
{
  private readonly DebugCommandCatalogAdapter _catalog;
  private readonly Func<DebugMessage, CommandRequirement, bool> _authorize;

  public DebugCommandDispatcher(
    DebugCommandCatalogAdapter catalog,
    Func<DebugMessage, CommandRequirement, bool>? authorize = null)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    _authorize = authorize ?? DefaultAuthorize;
  }

  public bool Dispatch(DebugMessage message)
  {
    ArgumentNullException.ThrowIfNull(message);
    if (!_catalog.TryGet(message.CommandName, out IDebugCommand? command) ||
      command is null)
    {
      return false;
    }

    if (!_authorize.Invoke(message, command.Metadata.Requirements))
    {
      return false;
    }

    return command.Process(message);
  }

  private static bool DefaultAuthorize(
    DebugMessage message,
    CommandRequirement requirements)
  {
    if (requirements.HasFlag(CommandRequirement.Server))
    {
      return message.Author == byte.MaxValue;
    }

    return true;
  }
}
