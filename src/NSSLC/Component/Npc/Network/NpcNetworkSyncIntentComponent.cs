using System;

namespace Terraria.Npc.Network;

public sealed class NpcNetworkSyncIntentComponent
{
  private bool _isPending;
  private uint _revision;

  public bool IsPending => _isPending;

  public uint Revision => _revision;

  public uint LastAcknowledgedRevision { get; private set; }

  public bool Mark()
  {
    if (_isPending)
    {
      return false;
    }

    if (_revision == uint.MaxValue)
    {
      throw new InvalidOperationException(
        "NPC network synchronization revision exhausted its range.");
    }

    _revision++;
    _isPending = true;
    return true;
  }

  public bool Acknowledge(uint revision)
  {
    if (!_isPending || revision != _revision)
    {
      return false;
    }

    _isPending = false;
    LastAcknowledgedRevision = revision;
    return true;
  }

  public bool Retry()
  {
    return _isPending;
  }

  public void ResetForEntityReuse()
  {
    _isPending = false;
    LastAcknowledgedRevision = 0;
  }
}
