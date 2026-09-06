using System;

namespace Terraria.Server.Npc;

[Flags]
public enum NpcReplicationFlags : byte
{
  None = 0,
  StateDirty = 1 << 0,
  SpawnNeedsSync = 1 << 1,
  ForceFullSync = 1 << 2,
  RemovalNeedsSync = 1 << 3,
}
