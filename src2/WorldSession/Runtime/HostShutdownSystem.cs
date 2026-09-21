namespace Terraria.WorldSession.Runtime;

public sealed class HostShutdownSystem
{
  private ShutdownRequestCommand? _pending;

  public void Submit(ShutdownRequestCommand command)
  {
    ArgumentNullException.ThrowIfNull(command);
    _pending = command;
  }

  public bool TryConsume(out ShutdownRequestCommand? command)
  {
    command = _pending;
    _pending = null;
    return command is not null;
  }
}
