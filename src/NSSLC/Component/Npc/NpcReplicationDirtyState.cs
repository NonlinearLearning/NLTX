using System.Collections.Generic;
using Terraria.Npc.Network;

namespace Terraria.Server.Npc;

public sealed class NpcReplicationDirtyState
{
  private readonly NpcNetworkSyncIntentComponent _networkSyncIntent = new();

  private NpcReplicationFlags _flags;

  public NpcReplicationFlags Flags
  {
    get => _networkSyncIntent.IsPending
      ? _flags | NpcReplicationFlags.StateDirty
      : _flags;
    set
    {
      _flags = value & ~NpcReplicationFlags.StateDirty;
      if ((value & NpcReplicationFlags.StateDirty) != 0)
      {
        _networkSyncIntent.Mark();
      }
      else if (_networkSyncIntent.IsPending)
      {
        _networkSyncIntent.Acknowledge(_networkSyncIntent.Revision);
      }
    }
  }

  public NpcNetworkSyncIntentComponent NetworkSyncIntent => _networkSyncIntent;

  public bool AlwaysRelevant { get; set; }

  public int StreamCursor { get; set; }

  public Dictionary<ulong, NpcClientReplicationState> ClientStates { get; } = new();

  public bool IsDirty => _networkSyncIntent.IsPending;

  public bool RequiresSpawnSync => (_flags & NpcReplicationFlags.SpawnNeedsSync) != 0;

  public bool MarkStateChanged()
  {
    return _networkSyncIntent.Mark();
  }

  public bool AcknowledgeStateChanged(uint revision)
  {
    return _networkSyncIntent.Acknowledge(revision);
  }

  public bool RetryStateChanged()
  {
    return _networkSyncIntent.Retry();
  }

  public void ResetForEntityReuse()
  {
    _flags = NpcReplicationFlags.None;
    AlwaysRelevant = false;
    StreamCursor = 0;
    ClientStates.Clear();
    _networkSyncIntent.ResetForEntityReuse();
  }
}
