namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class DelegateDebugCommand : IDebugCommand
{
  private readonly Func<DebugMessage, bool> _process;

  public DelegateDebugCommand(
    DebugCommandMetadata metadata,
    Func<DebugMessage, bool> process)
  {
    Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    _process = process ?? throw new ArgumentNullException(nameof(process));
  }

  public DebugCommandMetadata Metadata { get; }

  public bool Process(DebugMessage message)
  {
    return _process.Invoke(message);
  }
}
