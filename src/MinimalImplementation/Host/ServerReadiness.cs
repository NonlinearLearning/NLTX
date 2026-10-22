using System;
using System.Collections.Generic;

namespace MinimalImplementation.Server.Host;

public sealed class ServerReadiness
{
  public ServerReadiness(IEnumerable<string> blockers)
  {
    ArgumentNullException.ThrowIfNull(blockers);
    Blockers = Array.AsReadOnly(new List<string>(blockers).ToArray());
  }

  public IReadOnlyList<string> Blockers { get; }

  public bool IsReady => Blockers.Count == 0;
}
