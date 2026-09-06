using System.Collections.Generic;

namespace Terraria.Server.Npc;

public sealed class NpcReplicationDirtyState
{
  public NpcReplicationFlags Flags { get; set; }

  public bool AlwaysRelevant { get; set; }

  public int StreamCursor { get; set; }

  public Dictionary<ulong, NpcClientReplicationState> ClientStates { get; } = new();

  public bool IsDirty => (Flags & NpcReplicationFlags.StateDirty) != 0;

  public bool RequiresSpawnSync => (Flags & NpcReplicationFlags.SpawnNeedsSync) != 0;
}
