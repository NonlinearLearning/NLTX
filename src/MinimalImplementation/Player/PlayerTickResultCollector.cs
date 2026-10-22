using System;
using System.Collections.Generic;

namespace MinimalImplementation.Server.Player;

/// <summary>Small handoff buffer; it never sends packets or performs transport I/O.</summary>
public sealed class PlayerTickResultCollector : IPlayerTickResultSink
{
  private readonly Queue<PlayerUpdateResult> _results = new();

  public void Capture(PlayerUpdateResult result)
  {
    ArgumentNullException.ThrowIfNull(result);
    _results.Enqueue(result);
  }

  public bool TryDequeue(out PlayerUpdateResult? result)
  {
    if (_results.Count == 0)
    {
      result = null;
      return false;
    }

    result = _results.Dequeue();
    return true;
  }
}
