namespace Terraria.NonAuthoritative.Diagnostics;

public interface IDebugCommand
{
  DebugCommandMetadata Metadata { get; }

  bool Process(DebugMessage message);
}
